// Fichier : Application/UseCases/ProcessChantierUseCase.cs (Version Finale Complète)

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
            System.Diagnostics.Debug.WriteLine($"[DEBUG_TRACE] Validation Chantier Usecase");
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

        private async Task<ProcessChantierResultDto> AnalyserUniquementAsync(Chantier chantier, IReadOnlyList<MessageValidationDto> validationMessages)
        {
            var allMessages = new List<MessageValidationDto>(validationMessages);
            long? coutEstime = null;
            long? dureeEstimee = null;

            await Task.Run(() =>
            {
                // === DÉBUT DE LA NOUVELLE LOGIQUE DE TIMEOUT ADAPTATIF ===

                // 1. Calculer le timeout basé sur le nombre de tâches
                int nombreDeTaches = chantier.ObtenirToutesLesTaches().Count();
                double timeoutCalcule = 5.0 + (nombreDeTaches * 0.35);

                // 2. Appliquer les limites minimales et maximales
                double timeoutFinalSecondes = Math.Max(10.0, Math.Min(timeoutCalcule, 180.0));

                // On ajoute un message pour le débogage et la traçabilité
                allMessages.Add(new MessageValidationDto { Type = TypeMessageValidation.Suggestion, CodeMessage = "INFO_TIMEOUT_CALC", Message = $"Timeout adaptatif pour l'estimation calculé à {timeoutFinalSecondes:F1} secondes pour {nombreDeTaches} tâches." });

                // === FIN DE LA NOUVELLE LOGIQUE DE TIMEOUT ADAPTATIF ===


                // 1. Préparation d'une configuration minimale pour le solveur.
                // 7 heures de travail par jour, 0% de pénalité pour changement d'ouvrier, 0 centimes de coût indirect journalier.
                var configOptimisation = new ConfigurationOptimisation(7, 0m, 0);
                chantier.AppliquerConfigurationOptimisation(configOptimisation);

                var echelleTemps = _calendrierService.CreerEchelleTempsOuvree(chantier.Calendrier, LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateDebut.Value), LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateFin.Value));
                var probleme = new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = chantier.ConfigurationOptimisation! };

                // 2. Construction du modèle.
                var modeleCpSat = _problemeBuilder.ConstruireModele(probleme, "DELAI");

                // 3. Configuration du solveur avec le timeout calculé.
                var solver = new CpSolver
                {
                    // On utilise une chaîne de caractères formatée pour inclure notre timeout.
                    // Le "F1" formate le double avec une seule décimale (ex: "12.5").
                    // On utilise CultureInfo.InvariantCulture pour s'assurer que le séparateur décimal est un point (.), ce que le solveur attend.
                    StringParameters = $"max_time_in_seconds:{timeoutFinalSecondes.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},log_search_progress:false"
                };

                var solverStatus = solver.Solve(modeleCpSat.Model);

                // 4. Si une solution est trouvée, on extrait les estimations.
                if (solverStatus == CpSolverStatus.Optimal || solverStatus == CpSolverStatus.Feasible)
                {
                    coutEstime = solver.Value(modeleCpSat.CoutTotal);
                    dureeEstimee = solver.Value(modeleCpSat.Makespan);
                    allMessages.Add(new MessageValidationDto { Type = TypeMessageValidation.Suggestion, CodeMessage = "INFO_ESTIMATION_OK", Message = $"Estimation rapide réussie (statut solveur: {solverStatus})." });
                }
                else
                {
                    allMessages.Add(new MessageValidationDto { Type = TypeMessageValidation.Avertissement, CodeMessage = "WARN_ESTIMATION_FAIL", Message = "Impossible de produire une estimation de coût/durée dans le temps imparti. Le chantier est peut-être infaisable ou très contraint." });
                }
            });

            // On exécute les analyses statiques originales en complément.
            var feasibilityMessages = await _feasibilityService.AnalyserFaisabiliteAsync(chantier);
            allMessages.AddRange(feasibilityMessages);

            var suggestedKeyResourceIds = await _keyResourceService.SuggererOuvriersClesAsync(chantier);

            // On assemble le DTO de résultat avec les nouvelles estimations.
            var analyseStatiqueResult = new AnalyseRessourcesResultatDto
            {
                OuvriersClesSuggereIds = suggestedKeyResourceIds.Select(id => id.Value).ToList(),
                CoutTotalEstime = coutEstime,
                DureeTotaleEstimeeEnSlots = dureeEstimee
            };

            var finalEtat = allMessages.Any(m => m.Type == TypeMessageValidation.Erreur) ? EtatTraitementInput.EchecValidation :
                            allMessages.Any(m => m.Type == TypeMessageValidation.Avertissement) ? EtatTraitementInput.SuccesAvecAvertissements :
                            EtatTraitementInput.Succes;

            return new ProcessChantierResultDto
            {
                ChantierId = chantier.Id.Value,
                Etat = finalEtat,
                Messages = allMessages,
                AnalyseStatiqueResultat = analyseStatiqueResult
            };
        }
        private async Task<ProcessChantierResultDto> ExecuterOptimisationEtAnalyseAsync(Chantier chantier, OptimizationConfigDto config, IReadOnlyList<MessageValidationDto> validationMessages)
        {
            PlanningOptimizationResultDto? planningResult = null;

            await Task.Run(async () =>
            {
                // 1. Préparation commune avec NOUVEAU CALCUL DE COÛT INDIRECT
                long coutIndirectJournalierEnCentimes = config.CoutIndirectJournalier * 100;
                var configOptimisation = new ConfigurationOptimisation(config.DureeJournaliereStandardHeures, config.PenaliteChangementOuvrierPourcentage, coutIndirectJournalierEnCentimes);
                chantier.AppliquerConfigurationOptimisation(configOptimisation);

                var echelleTemps = _calendrierService.CreerEchelleTempsOuvree(chantier.Calendrier, LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateDebut.Value), LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateFin.Value));
                var probleme = new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = chantier.ConfigurationOptimisation! };

                // 2. Récupération du nombre de workers depuis la configuration
                int numWorkers = _solverSettings.NumSearchWorkers;

                // 3. Aiguillage de la stratégie avec PARAMÉTRAGE DYNAMIQUE
                string solverParams;
                string objectif = "COUT"; // Par défaut

                switch (config.TypeDeSortie)
                {
                    case "ANALYSE_RAPIDE":
                        solverParams = $"max_time_in_seconds:{config.DureeCalculMaxSecondes}.0,log_search_progress:false,cp_model_presolve:true";
                        objectif = "COUT";
                        break;

                    case "OPTIMISATION_DELAI":
                        solverParams = $"max_time_in_seconds:{config.DureeCalculMaxSecondes}.0,num_search_workers:{numWorkers},log_search_progress:false,relative_gap_limit:0.01";
                        objectif = "DELAI";
                        break;

                    case "OPTIMISATION_COUT":
                    default:
                        solverParams = $"max_time_in_seconds:{config.DureeCalculMaxSecondes}.0,num_search_workers:{numWorkers},log_search_progress:false,relative_gap_limit:0.01";
                        objectif = "COUT";
                        break;
                }

                // 4. Construction et Résolution
                var modeleCpSat = _problemeBuilder.ConstruireModele(probleme, objectif); // On passe l'objectif au builder
                var solver = new CpSolver { StringParameters = solverParams };
                var solverStatus = solver.Solve(modeleCpSat.Model);
                var resultatStatus = solverStatus switch { CpSolverStatus.Optimal => OptimizationStatus.Optimal, CpSolverStatus.Feasible => OptimizationStatus.Feasible, _ => OptimizationStatus.Infeasible };

                // 5. Interprétation et Analyse
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
            };
        }
    }
}