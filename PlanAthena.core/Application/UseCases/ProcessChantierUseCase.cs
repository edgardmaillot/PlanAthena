// Fichier : Application/UseCases/ProcessChantierUseCase.cs (Version Refactorisée)

using FluentValidation;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Application.Services;
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
        private readonly ICapacityValidationService _capacityValidationService;
        private readonly ICrossReferenceValidationService _crossReferenceValidationService;

        public ProcessChantierUseCase(
            IValidator<ChantierSetupInputDto> fluentValidator,
            IChantierSetupInputMapper inputMapper,
            IChantierValidationService chantierValidationService,
            IInitialFeasibilityAnalysisService feasibilityService,
            IKeyResourceSuggestionService keyResourceService,
            ICalendrierService calendrierService,
            IConstructeurProblemeOrTools problemeBuilder,
            ISolutionInterpreterService solutionInterpreter,
            SolverSettings solverSettings,
            ICapacityValidationService capacityValidationService,
    ICrossReferenceValidationService crossReferenceValidationService)
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
            _capacityValidationService = capacityValidationService;
            _crossReferenceValidationService = crossReferenceValidationService;
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

            // 1. Validation FluentValidation (structure et format)
            var fluentValidationResult = await _fluentValidator.ValidateAsync(inputDto);
            if (!fluentValidationResult.IsValid)
            {
                Debug.WriteLine("[DEBUG_TRACE] 2a. Échec de la validation FluentValidation.");
                allMessages.AddRange(fluentValidationResult.Errors.Select(e => new MessageValidationDto { Type = TypeMessageValidation.Erreur, CodeMessage = e.ErrorCode ?? "ERR_VALIDATION", Message = e.ErrorMessage, ProprieteConcernee = e.PropertyName }));
                return (null, allMessages);
            }
            Debug.WriteLine("[DEBUG_TRACE] 2a. Validation FluentValidation OK.");

            // 2. Validation des références croisées (avant mapping)
            var crossRefMessages = await _crossReferenceValidationService.ValidateCrossReferencesAsync(inputDto, null);
            allMessages.AddRange(crossRefMessages);
            if (allMessages.Any(m => m.Type == TypeMessageValidation.Erreur))
            {
                Debug.WriteLine("[DEBUG_TRACE] 2b. Erreurs de références croisées détectées.");
                return (null, allMessages);
            }

            // 3. Mapping vers le domaine
            var (chantier, mappingMessages) = await _inputMapper.MapToDomainAsync(inputDto);
            allMessages.AddRange(mappingMessages);
            if (chantier == null)
            {
                Debug.WriteLine("[DEBUG_TRACE] 3. Échec du mapping du DTO vers le domaine.");
                return (null, allMessages);
            }
            Debug.WriteLine("[DEBUG_TRACE] 3. Mapping du DTO vers le domaine OK.");

            // 4. Validations métier avancées
            var cycleMessages = await _chantierValidationService.ValiderChantierCompletAsync(inputDto, chantier);
            allMessages.AddRange(cycleMessages);

            var capacityMessages = await _capacityValidationService.ValidateCapacityConstraintsAsync(inputDto, chantier);
            allMessages.AddRange(capacityMessages);

            // Vérifier s'il y a des erreurs critiques (pas seulement des avertissements)
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

                // === 5. INTERPRÉTATION REFACTORISÉE DES RÉSULTATS ===
                planningResult = InterpretarResultatSolveur(solver, modeleCpSat, probleme, solverStatus, chantier.Id.Value, allMessages);

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

            var finalEtat = DeterminerEtatFinal(allMessages);

            return new ProcessChantierResultDto
            {
                ChantierId = chantier.Id.Value,
                Etat = finalEtat,
                Messages = allMessages,
                OptimisationResultat = planningResult,
                AnalyseStatiqueResultat = analyseStatiqueResult
            };
        }

        /// <summary>
        /// Interprète le résultat du solveur OR-Tools de manière claire et structurée
        /// </summary>
        private PlanningOptimizationResultDto InterpretarResultatSolveur(
            CpSolver solver,
            dynamic modeleCpSat, // Remplacez par le type exact si disponible
            ProblemeOptimisation probleme,
            CpSolverStatus solverStatus,
            string chantierId,
            List<MessageValidationDto> messages)
        {
            var optimizationStatus = ConvertirStatutSolveur(solverStatus);

            return optimizationStatus switch
            {
                OptimizationStatus.Optimal => CreerResultatAvecSolution(solver, modeleCpSat, probleme, chantierId, optimizationStatus, messages, "Résolution optimale trouvée."),

                OptimizationStatus.Feasible => CreerResultatAvecSolution(solver, modeleCpSat, probleme, chantierId, optimizationStatus, messages, "Solution réalisable trouvée (non optimale)."),

                OptimizationStatus.Infeasible => CreerResultatSansolution(chantierId, optimizationStatus, messages,
                    "WARN_SOLVEUR_INFEASIBLE",
                    "Aucune solution trouvée. Le chantier est infaisable avec les contraintes actuelles."),

                OptimizationStatus.Aborted => CreerResultatSansolution(chantierId, optimizationStatus, messages,
                    "WARN_SOLVEUR_TIMEOUT",
                    "Le solveur n'a pas pu trouver de solution dans le délai imparti. Essayez d'augmenter le temps de calcul ou de simplifier les contraintes."),

                OptimizationStatus.ModelInvalid => CreerResultatSansolution(chantierId, optimizationStatus, messages,
                    "ERR_SOLVEUR_MODEL_INVALID",
                    "Le modèle mathématique est invalide. Contactez le support technique."),

                _ => CreerResultatSansolution(chantierId, optimizationStatus, messages,
                    "ERR_SOLVEUR_UNKNOWN",
                    $"Statut de solveur inconnu: {solverStatus}. Contactez le support technique.")
            };
        }

        /// <summary>
        /// Convertit le statut OR-Tools vers notre enum métier
        /// </summary>
        private static OptimizationStatus ConvertirStatutSolveur(CpSolverStatus solverStatus)
        {
            return solverStatus switch
            {
                CpSolverStatus.Optimal => OptimizationStatus.Optimal,
                CpSolverStatus.Feasible => OptimizationStatus.Feasible,
                CpSolverStatus.Infeasible => OptimizationStatus.Infeasible,
                CpSolverStatus.ModelInvalid => OptimizationStatus.ModelInvalid,
                CpSolverStatus.Unknown => OptimizationStatus.Unknown,
                _ => OptimizationStatus.Aborted // Pour tous les autres cas (timeout, etc.)
            };
        }

        /// <summary>
        /// Crée un résultat avec solution (cas Optimal/Feasible)
        /// </summary>
        private PlanningOptimizationResultDto CreerResultatAvecSolution(
            CpSolver solver,
            dynamic modeleCpSat,
            ProblemeOptimisation probleme,
            string chantierId,
            OptimizationStatus status,
            List<MessageValidationDto> messages,
            string messageSucces)
        {
            var affectations = _solutionInterpreter.InterpreterLaSolution(solver, modeleCpSat, probleme);
            long coutTotalValue = solver.Value(modeleCpSat.CoutTotal);
            long coutRhValue = solver.Value(modeleCpSat.CoutRh);
            long coutIndirectValue = solver.Value(modeleCpSat.CoutIndirect);
            long makespan = solver.Value(modeleCpSat.Makespan);

            messages.Add(new MessageValidationDto
            {
                Type = TypeMessageValidation.Suggestion,
                CodeMessage = "INFO_SOLVEUR_SUCCESS",
                Message = messageSucces
            });

            return new PlanningOptimizationResultDto
            {
                ChantierId = chantierId,
                Status = status,
                CoutTotalEstime = coutTotalValue,
                CoutTotalRhEstime = coutRhValue,
                CoutTotalIndirectEstime = coutIndirectValue,
                DureeTotaleEnSlots = makespan,
                Affectations = affectations
            };
        }

        /// <summary>
        /// Crée un résultat sans solution (cas d'échec)
        /// </summary>
        private static PlanningOptimizationResultDto CreerResultatSansolution(
            string chantierId,
            OptimizationStatus status,
            List<MessageValidationDto> messages,
            string codeMessage,
            string messageTexte)
        {
            var typeMessage = status == OptimizationStatus.ModelInvalid ? TypeMessageValidation.Erreur : TypeMessageValidation.Avertissement;

            messages.Add(new MessageValidationDto
            {
                Type = typeMessage,
                CodeMessage = codeMessage,
                Message = messageTexte
            });

            return new PlanningOptimizationResultDto
            {
                ChantierId = chantierId,
                Status = status
            };
        }

        /// <summary>
        /// Détermine l'état final du traitement basé sur les messages
        /// </summary>
        private static EtatTraitementInput DeterminerEtatFinal(IReadOnlyList<MessageValidationDto> messages)
        {
            if (messages.Any(m => m.Type == TypeMessageValidation.Erreur))
                return EtatTraitementInput.EchecValidation;

            if (messages.Any(m => m.Type == TypeMessageValidation.Avertissement))
                return EtatTraitementInput.SuccesAvecAvertissements;

            return EtatTraitementInput.Succes;
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