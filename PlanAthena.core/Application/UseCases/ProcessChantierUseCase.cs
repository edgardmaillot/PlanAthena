// Fichier : Application/UseCases/ProcessChantierUseCase.cs (Version Refactorisée)

using FluentValidation;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using System.Diagnostics;

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
        private readonly SolverSettings _solverSettings;

        public ProcessChantierUseCase(
            IValidator<ChantierSetupInputDto> fluentValidator,
            IChantierSetupInputMapper inputMapper,
            IChantierValidationService chantierValidationService,
            IInitialFeasibilityAnalysisService feasibilityService,
            IKeyResourceSuggestionService keyResourceService,
            ICalendrierService calendrierService,
            IConstructeurProblemeOrTools problemeBuilder,
            ISolutionInterpreterService solutionInterpreter,
            SolverSettings solverSettings)
        {
            _fluentValidator = fluentValidator;
            _inputMapper = inputMapper;
            _chantierValidationService = chantierValidationService;
            _feasibilityService = feasibilityService;
            _keyResourceService = keyResourceService;
            _calendrierService = calendrierService;
            _problemeBuilder = problemeBuilder;
            _solutionInterpreter = solutionInterpreter;
            _solverSettings = solverSettings;
        }

        public async Task<ProcessChantierResultDto> ExecuteAsync(ChantierSetupInputDto inputDto)
        {
            Debug.WriteLine($"[DEBUG_TRACE] 1. Entrée dans ProcessChantierUseCase.ExecuteAsync avec {inputDto.Taches.Count} tâches.");

            var (chantier, validationMessages) = await ValiderEtCreerChantierAsync(inputDto);
            if (chantier == null)
            {
                Debug.WriteLine("[DEBUG_TRACE] 5. Échec de la validation, retour anticipé.");
                return new ProcessChantierResultDto
                {
                    ChantierId = inputDto.ChantierId,
                    Etat = EtatTraitementInput.EchecValidation,
                    Messages = validationMessages
                };
            }

            Debug.WriteLine("[DEBUG_TRACE] 5. Validation et création du chantier réussies. Passage à l'analyse/optimisation.");

            // === SWITCH UNIQUE POUR DÉFINIR TOUTE LA STRATÉGIE ===
            ConfigurationOptimisation configOptimisation;
            string solverParams;
            string objectif;
            bool includeAnalyseStatique;

            switch (inputDto.OptimizationConfig?.TypeDeSortie)
            {
                case null:
                case "ANALYSE_RAPIDE":
                    // Paramètres par défaut pour l'analyse rapide
                    configOptimisation = new ConfigurationOptimisation(7, 0m, 0);
                    int nombreTaches = chantier.ObtenirToutesLesTaches().Count();
                    double timeout = Math.Max(10.0, Math.Min(5.0 + (nombreTaches * 0.35), 180.0));
                    solverParams = $"max_time_in_seconds:{timeout.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},log_search_progress:false,relative_gap_limit:0.1";
                    objectif = "DELAI";
                    includeAnalyseStatique = true;
                    Debug.WriteLine($"[DEBUG_TRACE] Configuration ANALYSE_RAPIDE avec timeout adaptatif de {timeout:F1}s pour {nombreTaches} tâches.");
                    break;

                case "OPTIMISATION_DELAI":
                    configOptimisation = CreerConfigurationFromDto(inputDto.OptimizationConfig);
                    solverParams = $"max_time_in_seconds:{inputDto.OptimizationConfig.DureeCalculMaxSecondes}.0,num_search_workers:{_solverSettings.NumSearchWorkers},log_search_progress:false,relative_gap_limit:0.02";
                    objectif = "DELAI";
                    includeAnalyseStatique = false;
                    Debug.WriteLine($"[DEBUG_TRACE] Configuration OPTIMISATION_DELAI avec timeout de {inputDto.OptimizationConfig.DureeCalculMaxSecondes}s.");
                    break;

                case "OPTIMISATION_COUT":
                default:
                    configOptimisation = CreerConfigurationFromDto(inputDto.OptimizationConfig);
                    solverParams = $"max_time_in_seconds:{inputDto.OptimizationConfig.DureeCalculMaxSecondes}.0,num_search_workers:{_solverSettings.NumSearchWorkers},log_search_progress:false,relative_gap_limit:0.01";
                    objectif = "COUT";
                    includeAnalyseStatique = false;
                    Debug.WriteLine($"[DEBUG_TRACE] Configuration OPTIMISATION_COUT avec timeout de {inputDto.OptimizationConfig.DureeCalculMaxSecondes}s.");
                    break;
            }

            // === EXÉCUTION UNIQUE ===
            return await ExecuteSolveurAsync(chantier, configOptimisation, solverParams, objectif, includeAnalyseStatique, validationMessages);
        }

        private async Task<(Chantier? chantier, IReadOnlyList<MessageValidationDto> messages)> ValiderEtCreerChantierAsync(ChantierSetupInputDto inputDto)
        {
            var allMessages = new List<MessageValidationDto>();
            Debug.WriteLine($"[DEBUG_TRACE] Validation Chantier Usecase");

            var fluentValidationResult = await _fluentValidator.ValidateAsync(inputDto);
            if (!fluentValidationResult.IsValid)
            {
                Debug.WriteLine("[DEBUG_TRACE] 2a. Échec de la validation FluentValidation.");
                allMessages.AddRange(fluentValidationResult.Errors.Select(e => new MessageValidationDto { Type = TypeMessageValidation.Erreur, CodeMessage = e.ErrorCode ?? "ERR_VALIDATION", Message = e.ErrorMessage, ProprieteConcernee = e.PropertyName }));
                return (null, allMessages);
            }
            Debug.WriteLine("[DEBUG_TRACE] 2a. Validation FluentValidation OK.");

            var (chantier, mappingMessages) = await _inputMapper.MapToDomainAsync(inputDto);
            allMessages.AddRange(mappingMessages);
            if (chantier == null)
            {
                Debug.WriteLine("[DEBUG_TRACE] 3. Échec du mapping du DTO vers le domaine.");
                return (null, allMessages);
            }
            Debug.WriteLine("[DEBUG_TRACE] 3. Mapping du DTO vers le domaine OK.");

            var cycleMessages = await _chantierValidationService.ValiderChantierCompletAsync(inputDto, chantier);
            allMessages.AddRange(cycleMessages);

            if (allMessages.Any(m => m.Type == TypeMessageValidation.Erreur))
            {
                Debug.WriteLine("[DEBUG_TRACE] 4a. Des erreurs ont été détectées durant les validations.");
                return (null, allMessages);
            }

            return (chantier, allMessages);
        }

        private async Task<ProcessChantierResultDto> ExecuteSolveurAsync(
            Chantier chantier,
            ConfigurationOptimisation configOptimisation,
            string solverParams,
            string objectif,
            bool includeAnalyseStatique,
            IReadOnlyList<MessageValidationDto> validationMessages)
        {
            var allMessages = new List<MessageValidationDto>(validationMessages);
            PlanningOptimizationResultDto? planningResult = null;

            await Task.Run(async () =>
            {
                Debug.WriteLine($"[DEBUG_TRACE] Début exécution solveur avec objectif '{objectif}' et paramètres '{solverParams}'.");

                // 1. Configuration du chantier
                chantier.AppliquerConfigurationOptimisation(configOptimisation);

                // 2. Création de l'échelle de temps
                var echelleTemps = _calendrierService.CreerEchelleTempsOuvree(
                    chantier.Calendrier,
                    LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateDebut.Value),
                    LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateFin.Value));

                var probleme = new ProblemeOptimisation
                {
                    Chantier = chantier,
                    EchelleTemps = echelleTemps,
                    Configuration = chantier.ConfigurationOptimisation!
                };

                // 3. Construction du modèle avec l'objectif défini
                var modeleCpSat = _problemeBuilder.ConstruireModele(probleme, objectif);

                // 4. Configuration et résolution
                var solver = new CpSolver { StringParameters = solverParams };
                var solverStatus = solver.Solve(modeleCpSat.Model);

                Debug.WriteLine($"[DEBUG_TRACE] Solveur terminé avec statut : {solverStatus}");

                // 5. Interprétation des résultats
                var resultatStatus = solverStatus switch
                {
                    CpSolverStatus.Optimal => OptimizationStatus.Optimal,
                    CpSolverStatus.Feasible => OptimizationStatus.Feasible,
                    _ => OptimizationStatus.Infeasible
                };

                if (resultatStatus == OptimizationStatus.Optimal || resultatStatus == OptimizationStatus.Feasible)
                {
                    var affectations = _solutionInterpreter.InterpreterLaSolution(solver, modeleCpSat, probleme);
                    long coutTotalValue = solver.Value(modeleCpSat.CoutTotal);
                    long coutRhValue = solver.Value(modeleCpSat.CoutRh);
                    long coutIndirectValue = solver.Value(modeleCpSat.CoutIndirect);

                    planningResult = new PlanningOptimizationResultDto
                    {
                        ChantierId = chantier.Id.Value,
                        Status = resultatStatus,
                        CoutTotalEstime = coutTotalValue,
                        CoutTotalRhEstime = coutRhValue,
                        CoutTotalIndirectEstime = coutIndirectValue,
                        DureeTotaleEnSlots = solver.Value(modeleCpSat.Makespan),
                        Affectations = affectations
                    };

                    allMessages.Add(new MessageValidationDto
                    {
                        Type = TypeMessageValidation.Suggestion,
                        CodeMessage = "INFO_SOLVEUR_OK",
                        Message = $"Résolution réussie (statut solveur: {solverStatus})."
                    });
                }
                else
                {
                    planningResult = new PlanningOptimizationResultDto
                    {
                        ChantierId = chantier.Id.Value,
                        Status = resultatStatus
                    };

                    allMessages.Add(new MessageValidationDto
                    {
                        Type = TypeMessageValidation.Avertissement,
                        CodeMessage = "WARN_SOLVEUR_FAIL",
                        Message = "Impossible de trouver une solution dans le temps imparti. Le chantier est peut-être infaisable ou très contraint."
                    });
                }

                // 6. Analyses statiques complémentaires (uniquement pour l'analyse rapide)
                if (includeAnalyseStatique)
                {
                    Debug.WriteLine("[DEBUG_TRACE] Exécution des analyses statiques complémentaires.");
                    var feasibilityMessages = await _feasibilityService.AnalyserFaisabiliteAsync(chantier);
                    allMessages.AddRange(feasibilityMessages);
                }
            });

            // 7. Construction du résultat final
            AnalyseRessourcesResultatDto? analyseStatiqueResult = null;
            if (includeAnalyseStatique)
            {
                var suggestedKeyResourceIds = await _keyResourceService.SuggererOuvriersClesAsync(chantier);
                analyseStatiqueResult = new AnalyseRessourcesResultatDto
                {
                    OuvriersClesSuggereIds = suggestedKeyResourceIds.Select(id => id.Value).ToList(),
                    CoutTotalEstime = planningResult?.CoutTotalEstime,
                    DureeTotaleEstimeeEnSlots = planningResult?.DureeTotaleEnSlots
                };
            }

            var finalEtat = allMessages.Any(m => m.Type == TypeMessageValidation.Erreur) ? EtatTraitementInput.EchecValidation :
                            allMessages.Any(m => m.Type == TypeMessageValidation.Avertissement) ? EtatTraitementInput.SuccesAvecAvertissements :
                            EtatTraitementInput.Succes;

            return new ProcessChantierResultDto
            {
                ChantierId = chantier.Id.Value,
                Etat = finalEtat,
                Messages = allMessages,
                OptimisationResultat = planningResult,
                AnalyseStatiqueResultat = analyseStatiqueResult
            };
        }

        private ConfigurationOptimisation CreerConfigurationFromDto(OptimizationConfigDto config)
        {
            long coutIndirectJournalierEnCentimes = config.CoutIndirectJournalier * 100;
            return new ConfigurationOptimisation(
                config.DureeJournaliereStandardHeures,
                config.PenaliteChangementOuvrierPourcentage,
                coutIndirectJournalierEnCentimes);
        }
    }
}