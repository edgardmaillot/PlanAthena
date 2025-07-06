// Fichier : Application/UseCases/ProcessChantierUseCase.cs

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
using PlanAthena.Core.Infrastructure.Services.OrTools;
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

        public ProcessChantierUseCase(
            IValidator<ChantierSetupInputDto> fluentValidator,
            IChantierSetupInputMapper inputMapper,
            IChantierValidationService chantierValidationService,
            IInitialFeasibilityAnalysisService feasibilityService,
            IKeyResourceSuggestionService keyResourceService,
            ICalendrierService calendrierService,
            IConstructeurProblemeOrTools problemeBuilder)
        {
            _fluentValidator = fluentValidator;
            _inputMapper = inputMapper;
            _chantierValidationService = chantierValidationService;
            _feasibilityService = feasibilityService;
            _keyResourceService = keyResourceService;
            _calendrierService = calendrierService;
            _problemeBuilder = problemeBuilder;
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

            if (inputDto.OptimizationConfig == null)
            {
                return await AnalyserUniquementAsync(chantier, validationMessages);
            }

            return await ExecuterOptimisationAsync(chantier, inputDto.OptimizationConfig, validationMessages);
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
                AnalyseResultat = analyseRessourcesResult
            };
        }

        private async Task<ProcessChantierResultDto> ExecuterOptimisationAsync(Chantier chantier, OptimizationConfigDto config, IReadOnlyList<MessageValidationDto> validationMessages)
        {
            // L'encapsulation dans Task.Run déporte tout le travail CPU-intensif sur un thread du pool,
            // libérant ainsi le thread appelant (ex: UI ou requête web).
            return await Task.Run(() =>
            {
                const decimal COUT_INDIRECT_DEFAUT_PCT = 0.10m;

                decimal masseSalarialeJournaliereTotale = chantier.Ouvriers.Values.Sum(o => o.Cout.Value);

                decimal pourcentageApplique = config.CoutIndirectJournalierPourcentage.HasValue
                    ? config.CoutIndirectJournalierPourcentage.Value / 100.0m
                    : COUT_INDIRECT_DEFAUT_PCT;

                long coutIndirectJournalierEnCentimes = (long)(masseSalarialeJournaliereTotale * pourcentageApplique * 100);

                var configOptimisation = new ConfigurationOptimisation(
                    config.DureeJournaliereStandardHeures,
                    config.PenaliteChangementOuvrierPourcentage,
                    coutIndirectJournalierEnCentimes
                );

                chantier.AppliquerConfigurationOptimisation(configOptimisation);

                var echelleTemps = _calendrierService.CreerEchelleTempsOuvree(chantier.Calendrier, LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateDebut.Value), LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateFin.Value));
                var probleme = new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = chantier.ConfigurationOptimisation! };

                var modeleCpSat = _problemeBuilder.ConstruireModele(probleme);

                var solver = new CpSolver();
                solver.StringParameters =
                    "max_time_in_seconds:60.0" +
                    ",num_search_workers:8" +
                    ",log_search_progress:false" +
                    ",cp_model_presolve:true" +
                    ",cp_model_probing_level:2" +
                    ",relative_gap_limit:0.01";

                var solverStatus = solver.Solve(modeleCpSat.Model);

                var resultatStatus = solverStatus switch
                {
                    CpSolverStatus.Optimal => OptimizationStatus.Optimal,
                    CpSolverStatus.Feasible => OptimizationStatus.Feasible,
                    _ => OptimizationStatus.Infeasible
                };

                var optimResultDto = new PlanningOptimizationResultDto { ChantierId = chantier.Id.Value, Status = resultatStatus };

                if (resultatStatus == OptimizationStatus.Optimal || resultatStatus == OptimizationStatus.Feasible)
                {
                    // Les logs de débogage peuvent être laissés ici, ils ne s'afficheront que
                    // lorsque la tâche de fond sera terminée.
                    Console.WriteLine("\n--- DÉBUT DES INFORMATIONS DE DÉBOGAGE ---");

                    foreach (var tache in chantier.ObtenirToutesLesTaches())
                    {
                        Console.WriteLine($"\n[SONDE 1] Assignations pour la tâche '{tache.Id.Value}':");
                        bool estPlanifiee = false;
                        foreach (var assignable in modeleCpSat.TachesAssignables.Where(kv => kv.Key.Item1 == tache.Id))
                        {
                            var (tacheId, ouvrierId) = assignable.Key;
                            if (solver.BooleanValue(assignable.Value))
                            {
                                estPlanifiee = true;
                                Console.WriteLine($"  - Ouvrier '{ouvrierId.Value}' est assigné.");
                                var intervalle = modeleCpSat.TachesIntervals[tacheId];
                                Console.WriteLine($"    [SONDE 2] Tâche '{tacheId.Value}' assignée à '{ouvrierId.Value}':");
                                Console.WriteLine($"      - Démarre au slot : {solver.Value(intervalle.StartExpr())}");
                                Console.WriteLine($"      - Duree (slots) : {solver.Value(intervalle.SizeExpr())}");
                                Console.WriteLine($"      - Finit au slot   : {solver.Value(intervalle.EndExpr())}");
                            }
                        }
                        if (!estPlanifiee)
                        {
                            Console.WriteLine("  - Tâche non planifiée / aucune assignation trouvée.");
                        }
                    }

                    var coutBrut = (modeleCpSat.CoutTotal != null) ? solver.Value(modeleCpSat.CoutTotal) : -1;
                    var makespanBrut = (modeleCpSat.Makespan != null) ? solver.Value(modeleCpSat.Makespan) : -1;
                    Console.WriteLine($"\n[SONDE 3] Valeurs brutes des objectifs :");
                    Console.WriteLine($"  - CoutTotal Brut   : {coutBrut}");
                    Console.WriteLine($"  - Makespan Brut    : {makespanBrut}");

                    Console.WriteLine("\n--- FIN DES INFORMATIONS DE DÉBOGAGE ---\n");

                    optimResultDto = optimResultDto with
                    {
                        CoutTotalEstime = (coutBrut != -1) ? coutBrut : null,
                        DureeTotaleJoursOuvres = makespanBrut
                    };
                }

                return new ProcessChantierResultDto
                {
                    ChantierId = chantier.Id.Value,
                    Etat = EtatTraitementInput.Succes,
                    Messages = validationMessages,
                    OptimisationResultat = optimResultDto
                };
            });
        }

    }
}