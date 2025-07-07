// Fichier : Application/UseCases/ProcessChantierUseCase.cs (Version Finale Complète)

using FluentValidation;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanAthena.Core.Application.UseCases
{
    public class ProcessChantierUseCase : IProcessChantierUseCase
    {
        private readonly IValidator<ChantierSetupInputDto> _fluentValidator;
        private readonly IChantierSetupInputMapper _inputMapper;
        private readonly IChantierValidationService _chantierValidationService;
        private readonly IInitialFeasibilityAnalysisService _feasibilityService;
        private readonly IKeyResourceSuggestionService _keyResourceService;
        private readonly ICalendrierService _calendrierService;
        private readonly IConstructeurProblemeOrTools _problemeBuilder;
        private readonly ISolutionInterpreterService _solutionInterpreter;
        private readonly IPlanningAnalysisService _planningAnalyzer;

        public ProcessChantierUseCase(
            IValidator<ChantierSetupInputDto> fluentValidator,
            IChantierSetupInputMapper inputMapper,
            IChantierValidationService chantierValidationService,
            IInitialFeasibilityAnalysisService feasibilityService,
            IKeyResourceSuggestionService keyResourceService,
            ICalendrierService calendrierService,
            IConstructeurProblemeOrTools problemeBuilder,
            ISolutionInterpreterService solutionInterpreter,
            IPlanningAnalysisService planningAnalyzer)
        {
            _fluentValidator = fluentValidator;
            _inputMapper = inputMapper;
            _chantierValidationService = chantierValidationService;
            _feasibilityService = feasibilityService;
            _keyResourceService = keyResourceService;
            _calendrierService = calendrierService;
            _problemeBuilder = problemeBuilder;
            _solutionInterpreter = solutionInterpreter;
            _planningAnalyzer = planningAnalyzer;
        }

        public async Task<ProcessChantierResultDto> ExecuteAsync(ChantierSetupInputDto inputDto)
        {
            var (chantier, validationMessages) = await ValiderEtCreerChantierAsync(inputDto);

            if (chantier == null)
            {
                return new ProcessChantierResultDto
                {
                    ChantierId = inputDto.ChantierId,
                    Etat = EtatTraitementInput.EchecValidation,
                    Messages = validationMessages
                };
            }

            // Aiguillage basé sur la présence de la configuration d'optimisation
            if (inputDto.OptimizationConfig == null)
            {
                return await AnalyserUniquementAsync(chantier, validationMessages);
            }

            return await ExecuterOptimisationEtAnalyseAsync(chantier, inputDto.OptimizationConfig, validationMessages);
        }

        private async Task<(Chantier? chantier, IReadOnlyList<MessageValidationDto> messages)> ValiderEtCreerChantierAsync(ChantierSetupInputDto inputDto)
        {
            var allMessages = new List<MessageValidationDto>();

            var fluentValidationResult = await _fluentValidator.ValidateAsync(inputDto);
            if (!fluentValidationResult.IsValid)
            {
                allMessages.AddRange(fluentValidationResult.Errors.Select(e => new MessageValidationDto { Type = TypeMessageValidation.Erreur, CodeMessage = e.ErrorCode ?? "ERR_VALIDATION", Message = e.ErrorMessage, ProprieteConcernee = e.PropertyName }));
                return (null, allMessages);
            }

            var (chantier, mappingMessages) = await _inputMapper.MapToDomainAsync(inputDto);
            allMessages.AddRange(mappingMessages);
            if (chantier == null) return (null, allMessages);

            var cycleMessages = await _chantierValidationService.ValiderChantierCompletAsync(inputDto, chantier);
            allMessages.AddRange(cycleMessages);

            if (allMessages.Any(m => m.Type == TypeMessageValidation.Erreur)) return (null, allMessages);

            return (chantier, allMessages);
        }

        private async Task<ProcessChantierResultDto> AnalyserUniquementAsync(Chantier chantier, IReadOnlyList<MessageValidationDto> validationMessages)
        {
            var allMessages = new List<MessageValidationDto>(validationMessages);

            var feasibilityMessages = await _feasibilityService.AnalyserFaisabiliteAsync(chantier);
            allMessages.AddRange(feasibilityMessages);

            var suggestedKeyResourceIds = await _keyResourceService.SuggererOuvriersClesAsync(chantier);
            var analyseRessourcesResult = new AnalyseRessourcesResultatDto { OuvriersClesSuggereIds = suggestedKeyResourceIds.Select(id => id.Value).ToList() };
            if (suggestedKeyResourceIds.Any())
            {
                allMessages.Add(new MessageValidationDto { Type = TypeMessageValidation.Suggestion, CodeMessage = "SUGGEST_KEY_RESOURCES", Message = $"Suggère les ouvriers suivants comme clés : {string.Join(", ", suggestedKeyResourceIds.Select(id => id.Value))}" });
            }

            var finalEtat = allMessages.Any(m => m.Type == TypeMessageValidation.Avertissement) ? EtatTraitementInput.SuccesAvecAvertissements : EtatTraitementInput.Succes;

            return new ProcessChantierResultDto
            {
                ChantierId = chantier.Id.Value,
                Etat = finalEtat,
                Messages = allMessages,
                AnalyseStatiqueResultat = analyseRessourcesResult
            };
        }

        private async Task<ProcessChantierResultDto> ExecuterOptimisationEtAnalyseAsync(Chantier chantier, OptimizationConfigDto config, IReadOnlyList<MessageValidationDto> validationMessages)
        {
            PlanningOptimizationResultDto? planningResult = null;
            PlanningAnalysisReportDto? analysisReport = null;

            await Task.Run(async () =>
            {
                // 1. Préparation commune
                const decimal COUT_INDIRECT_DEFAUT_PCT = 0.10m;
                decimal masseSalarialeJournaliereTotale = chantier.Ouvriers.Values.Sum(o => o.Cout.Value);
                decimal pourcentageApplique = config.CoutIndirectJournalierPourcentage.HasValue ? config.CoutIndirectJournalierPourcentage.Value / 100.0m : COUT_INDIRECT_DEFAUT_PCT;
                long coutIndirectJournalierEnCentimes = (long)(masseSalarialeJournaliereTotale * pourcentageApplique * 100);
                var configOptimisation = new ConfigurationOptimisation(config.DureeJournaliereStandardHeures, config.PenaliteChangementOuvrierPourcentage, coutIndirectJournalierEnCentimes);
                chantier.AppliquerConfigurationOptimisation(configOptimisation);

                var echelleTemps = _calendrierService.CreerEchelleTempsOuvree(chantier.Calendrier, LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateDebut.Value), LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateFin.Value));
                var probleme = new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = chantier.ConfigurationOptimisation! };

                // 2. Aiguillage de la stratégie
                string solverParams;
                string objectif = "COUT"; // Par défaut

                switch (config.TypeDeSortie)
                {
                    case "ANALYSE_RAPIDE":
                        solverParams = "max_time_in_seconds:5.0,log_search_progress:false,cp_model_presolve:true";
                        objectif = "COUT";
                        break;

                    case "OPTIMISATION_DELAI":
                        solverParams = "max_time_in_seconds:60.0,num_search_workers:8,log_search_progress:false,relative_gap_limit:0.01";
                        objectif = "DELAI";
                        break;

                    case "OPTIMISATION_COUT":
                    default:
                        solverParams = "max_time_in_seconds:60.0,num_search_workers:8,log_search_progress:false,relative_gap_limit:0.01";
                        objectif = "COUT";
                        break;
                }

                // 3. Construction et Résolution
                var modeleCpSat = _problemeBuilder.ConstruireModele(probleme, objectif); // On passe l'objectif au builder
                var solver = new CpSolver { StringParameters = solverParams };
                var solverStatus = solver.Solve(modeleCpSat.Model);
                var resultatStatus = solverStatus switch { CpSolverStatus.Optimal => OptimizationStatus.Optimal, CpSolverStatus.Feasible => OptimizationStatus.Feasible, _ => OptimizationStatus.Infeasible };

                // 4. Interprétation et Analyse
                if (resultatStatus == OptimizationStatus.Optimal || resultatStatus == OptimizationStatus.Feasible)
                {
                    var affectations = _solutionInterpreter.InterpreterLaSolution(solver, modeleCpSat, probleme);
                    long coutTotal = solver.Value(modeleCpSat.CoutTotal);
                    long coutIndirect = affectations.Any() ? chantier.ConfigurationOptimisation.CoutIndirectJournalierEnCentimes * affectations.Select(a => a.DateDebut.Date).Distinct().Count() : 0;
                    long coutRh = coutTotal - coutIndirect;

                    planningResult = new PlanningOptimizationResultDto
                    {
                        ChantierId = chantier.Id.Value,
                        Status = resultatStatus,
                        CoutTotalEstime = coutTotal,
                        CoutTotalRhEstime = coutRh,
                        CoutTotalIndirectEstime = coutIndirect,
                        DureeTotaleEnSlots = solver.Value(modeleCpSat.Makespan),
                        Affectations = affectations
                    };

                    // L'analyse KPI est effectuée pour tous les cas où une solution est trouvée.
                    analysisReport = await _planningAnalyzer.AnalyserLePlanningAsync(affectations, chantier);
                }
                else
                {
                    planningResult = new PlanningOptimizationResultDto { ChantierId = chantier.Id.Value, Status = resultatStatus };
                }
            });

            return new ProcessChantierResultDto
            {
                ChantierId = chantier.Id.Value,
                Etat = EtatTraitementInput.Succes,
                Messages = validationMessages,
                OptimisationResultat = planningResult,
                AnalysePostOptimisationResultat = analysisReport
            };
        }
    }
}