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
                var solver = CreerSolverOptimisePourAnalyseRapide(timeoutFinalSecondes);
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
        private CpSolver CreerSolverOptimisePourAnalyseRapide(double timeoutSecondes)
        {
            var solver = new CpSolver();

            // Liste des paramètres par ordre de priorité/sécurité
            var parametresDeBase = new[]
            {
        $"max_time_in_seconds:{timeoutSecondes.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}",
        "log_search_progress:false"
    };

            var parametresOptimisation = new[]
            {
        "stop_after_first_solution:true",      // Le plus important pour la vitesse
        "cp_model_presolve:false",              // Évite le preprocessing
        "num_search_workers:1"                  // Un seul thread pour éviter l'overhead
    };

            // Application progressive des paramètres
            var parametresFinaux = new List<string>(parametresDeBase);

            foreach (var param in parametresOptimisation)
            {
                try
                {
                    var parametresTest = new List<string>(parametresFinaux) { param };
                    var parametresTestString = string.Join(",", parametresTest);

                    // Test de validation (création temporaire pour vérifier)
                    var solverTest = new CpSolver { StringParameters = parametresTestString };

                    // Si pas d'exception, ajouter le paramètre
                    parametresFinaux.Add(param);
                    Debug.WriteLine($"[DEBUG] Paramètre accepté: {param}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Paramètre rejeté '{param}': {ex.Message}");
                    // Continue avec les paramètres déjà validés
                }
            }

            solver.StringParameters = string.Join(",", parametresFinaux);
            Debug.WriteLine($"[INFO] Paramètres finaux du solveur: {solver.StringParameters}");

            return solver;
        }
        private async Task<ProcessChantierResultDto> ExecuterOptimisationEtAnalyseAsync(Chantier chantier, OptimizationConfigDto config, IReadOnlyList<MessageValidationDto> validationMessages)
        {
            PlanningOptimizationResultDto? planningResult = null;
            PlanningAnalysisReportDto? analysisReport = null;

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
                        solverParams = CreerParametresOptimisationDelai(config.DureeCalculMaxSecondes, numWorkers, chantier.ObtenirToutesLesTaches().Count());
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
                    // *** CORRECTION APPLIQUÉE ICI ***
                    // 1. On déconstruit le tuple retourné par la nouvelle méthode de l'interface.
                    var (affectations, feuillesDeTemps) = _solutionInterpreter.InterpreterLaSolution(solver, modeleCpSat, probleme);

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

                        // 2. On peuple les deux propriétés du DTO de résultat.
                        Affectations = affectations,
                        FeuillesDeTemps = feuillesDeTemps
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
                AnalysePostOptimisationResultat = analysisReport
            };

        }

        private string CreerParametresOptimisationDelai(int dureeMaxSecondes, int numWorkers, int nombreTaches)
        {
            var parametres = new List<string>
    {
        $"max_time_in_seconds:{dureeMaxSecondes}.0",
        "log_search_progress:false"
    };

            // ⭐ OPTIMISATIONS SPÉCIFIQUES AU DÉLAI

            // 1. Stratégie de recherche orientée makespan
            parametres.Add("search_branching:LP_SEARCH");  // Meilleur pour l'optimisation temporelle

            // 2. Workers adaptatifs selon la taille du problème
            int workersOptimal = Math.Min(numWorkers, Math.Max(2, nombreTaches / 30));
            parametres.Add($"num_search_workers:{workersOptimal}");

            // 3. Préprocessing ciblé
            parametres.Add("cp_model_presolve:true");
            parametres.Add("symmetry_level:2");  // Améliore les bornes temporelles

            // 4. Critère d'arrêt adaptatif selon la taille
            if (nombreTaches > 100)
            {
                parametres.Add("relative_gap_limit:0.02");  // 2% pour gros projets
                parametres.Add("absolute_gap_limit:2");     // 2 slots acceptables
            }
            else if (nombreTaches > 50)
            {
                parametres.Add("relative_gap_limit:0.01");  // 1% pour projets moyens
                parametres.Add("absolute_gap_limit:1");
            }
            else
            {
                parametres.Add("relative_gap_limit:0.005"); // 0.5% pour petits projets
            }

            // 5. Optimisations pour les contraintes temporelles
            parametres.Add("linearization_level:2");       // Linéarisation avancée
            parametres.Add("cp_model_use_max_hs:true");     // Heuristiques satisfiabilité maximale

            Debug.WriteLine($"[DEBUG] Paramètres optimisation délai: {string.Join(",", parametres)}");
            return string.Join(",", parametres);
        }

        // OPTIMISATION 3 : Amélioration de la borne inférieure du makespan
        // Ajoutez cette méthode après CreerParametresOptimisationDelai :

        private void AmeliorerBorneInferieureMakespan(CpModel model, IntVar makespan, Chantier chantier)
        {
            try
            {
                // Calcul du chemin critique théorique (sans contraintes de ressources)
                var cheminCritique = CalculerCheminCritiqueTheorique(chantier);
                if (cheminCritique > 0)
                {
                    model.Add(makespan >= cheminCritique);
                    Debug.WriteLine($"[DEBUG] Borne inférieure makespan appliquée: {cheminCritique} slots");
                }

                // Calcul de la borne basée sur la charge totale
                var chargeTotale = chantier.ObtenirToutesLesTaches()
            .Where(t => t.Type == TypeActivite.Tache)
            .Sum(t => t.HeuresHommeEstimees.Value);
                var nombreOuvriers = chantier.Ouvriers.Count;
                var borneCharge = nombreOuvriers > 0 ? (long)Math.Ceiling((double)chargeTotale / nombreOuvriers) : 0;

                if (borneCharge > 0)
                {
                    model.Add(makespan >= borneCharge);
                    Debug.WriteLine($"[DEBUG] Borne charge appliquée: {borneCharge} slots");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARNING] Impossible d'améliorer la borne makespan: {ex.Message}");
            }
        }

        private long CalculerCheminCritiqueTheorique(Chantier chantier)
        {
            var taches = chantier.ObtenirToutesLesTaches()
                .Where(t => t.Type == TypeActivite.Tache)
                .ToDictionary(t => t.Id, t => t);

            var dureeMax = new Dictionary<TacheId, long>();
            var traites = new HashSet<TacheId>();

            // Calcul topologique du chemin le plus long
            var pile = new Stack<TacheId>();

            // Commencer par les tâches sans dépendances
            foreach (var tache in taches.Values.Where(t => !t.Dependencies?.Any() ?? true))
            {
                pile.Push(tache.Id);
            }

            while (pile.Any())
            {
                var tacheId = pile.Peek();
                var tache = taches[tacheId];

                // Vérifier si toutes les dépendances sont traitées
                bool dependancesTraitees = true;
                if (tache.Dependencies?.Any() == true)
                {
                    foreach (var depId in tache.Dependencies)
                    {
                        if (!traites.Contains(depId))
                        {
                            dependancesTraitees = false;
                            if (taches.ContainsKey(depId) && !pile.Contains(depId))
                            {
                                pile.Push(depId);
                            }
                        }
                    }
                }

                if (dependancesTraitees)
                {
                    pile.Pop();

                    long dureeMaxDependances = 0;
                    if (tache.Dependencies?.Any() == true)
                    {
                        foreach (var depId in tache.Dependencies)
                        {
                            if (dureeMax.TryGetValue(depId, out long dureeDep))
                            {
                                dureeMaxDependances = Math.Max(dureeMaxDependances, dureeDep);
                            }
                        }
                    }

                    dureeMax[tacheId] = dureeMaxDependances + (long)tache.HeuresHommeEstimees.Value;
                    traites.Add(tacheId);

                    // Ajouter les tâches dépendantes à traiter
                    foreach (var tacheSuivante in taches.Values.Where(t =>
                        t.Dependencies?.Contains(tacheId) == true &&
                        !traites.Contains(t.Id) &&
                        !pile.Contains(t.Id)))
                    {
                        pile.Push(tacheSuivante.Id);
                    }
                }
            }

            return dureeMax.Values.Any() ? dureeMax.Values.Max() : 0;
        }

    }

}