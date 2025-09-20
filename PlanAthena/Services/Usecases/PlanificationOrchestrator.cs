// Fichier: /Services/UseCases/PlanificationOrchestrator.cs Version 0.8.0 - AMÉLIORÉE

using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.Processing;
using PlanAthena.Utilities;

namespace PlanAthena.Services.UseCases
{
    /// <summary>
    /// Exécute le cas d'utilisation "Lancer une nouvelle planification".
    /// Il agit comme un chef d'orchestre, appelant les services appropriés dans le bon ordre.
    /// Version 0.8.0 : Amélioration de l'interprétation des résultats du solveur avec guidance utilisateur.
    /// </summary>
    public class PlanificationOrchestrator
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly TaskManagerService _taskManagerService;
        private readonly PreparationSolveurService _preparationService;
        private readonly DataTransformer _transformerService;
        private readonly PlanAthenaCoreFacade _facade;
        private readonly PlanningConsolidationService _consolidationService;
        private readonly PlanningService _planningService;
        private readonly AnalysisService _analysisService;

        public PlanificationOrchestrator(
            ProjetService projetService,
            RessourceService ressourceService,
            TaskManagerService taskManagerService,
            PreparationSolveurService preparationService,
            DataTransformer transformerService,
            PlanAthenaCoreFacade facade,
            PlanningConsolidationService consolidationService,
            PlanningService planningService,
            AnalysisService analysisService)
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _taskManagerService = taskManagerService ?? throw new ArgumentNullException(nameof(taskManagerService));
            _preparationService = preparationService ?? throw new ArgumentNullException(nameof(preparationService));
            _transformerService = transformerService ?? throw new ArgumentNullException(nameof(transformerService));
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _consolidationService = consolidationService ?? throw new ArgumentNullException(nameof(consolidationService));
            _planningService = planningService ?? throw new ArgumentNullException(nameof(planningService));
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        }

        /// <summary>
        /// Exécute un cycle de planification complet.
        /// </summary>
        /// <param name="config">La configuration de la planification.</param>
        /// <param name="reinitialiserBaseline">Si vrai, la baseline existante sera écrasée par le résultat de cette planification.</param>
        /// <returns>Un DTO encapsulant tous les résultats du cycle.</returns>
        public virtual async Task<PlanificationRunResult> ExecuteAsync(ConfigurationPlanification config, bool reinitialiserBaseline)
        {
            // --- PHASE 1 : COLLECTE & PRÉPARATION ---
            var projetData = _projetService.GetProjetDataPourSauvegarde();
            var poolOuvriers = _ressourceService.GetAllOuvriers();
            var poolMetiers = _ressourceService.GetAllMetiers();
            var tachesOriginales = _taskManagerService.ObtenirToutesLesTaches();
            var preparationResult = _preparationService.PreparerPourSolveur(tachesOriginales, config);
            var inputDto = _transformerService.TransformToChantierSetupDto(
                 projetData, poolOuvriers, poolMetiers, preparationResult.TachesPreparees, config);

            // --- PHASE 2 : APPEL AU MOTEUR DE CALCUL ---
            var rawResult = await _facade.ProcessChantierAsync(inputDto);

            // --- PHASE 3 & 4 : TRAITEMENT DU RÉSULTAT AMÉLIORÉ ---
            return TraiterResultatSolveur(rawResult, config, poolOuvriers, poolMetiers, preparationResult, reinitialiserBaseline);
        }

        /// <summary>
        /// Traite le résultat du solveur de manière structurée selon le statut retourné
        /// Version améliorée avec guidance utilisateur spécifique selon le type d'échec
        /// </summary>
        private PlanificationRunResult TraiterResultatSolveur(
            ProcessChantierResultDto rawResult,
            ConfigurationPlanification config,
            IReadOnlyList<Ouvrier> poolOuvriers,
            IReadOnlyList<Metier> poolMetiers,
            dynamic preparationResult,
            bool reinitialiserBaseline)
        {
            // Cas spécial : Analyse statique demandée (ANALYSE_RAPIDE)
            if (rawResult.AnalyseStatiqueResultat?.OuvriersClesSuggereIds?.Any() == true)
            {
                var tensionReport = _analysisService.AnalyzeMetierTension(
                    rawResult.AnalyseStatiqueResultat.OuvriersClesSuggereIds, poolOuvriers);
                return new PlanificationRunResult { RawResult = rawResult, MetierTensionReport = tensionReport };
            }

            // Vérifier si nous avons un résultat d'optimisation valide
            if (rawResult.OptimisationResultat == null)
            {
                return new PlanificationRunResult { RawResult = rawResult };
            }

            // Traitement selon le statut du solveur avec guidance améliorée
            return rawResult.OptimisationResultat.Status switch
            {
                OptimizationStatus.Optimal or OptimizationStatus.Feasible => TraiterResultatAvecSolution(
                    rawResult, config, poolOuvriers, poolMetiers, preparationResult, reinitialiserBaseline),

                OptimizationStatus.Infeasible => TraiterResultatInfaisable(rawResult, config, poolOuvriers, poolMetiers),

                OptimizationStatus.Unknown => TraiterResultatTimeout(rawResult, config),

                OptimizationStatus.Aborted => TraiterResultatInterrompu(rawResult),

                OptimizationStatus.ModelInvalid => TraiterResultatErreurModele(rawResult),

                _ => TraiterResultatInconnu(rawResult)
            };
        }

        /// <summary>
        /// Traite les cas de succès (Optimal/Feasible) - logique existante
        /// </summary>
        private PlanificationRunResult TraiterResultatAvecSolution(
            ProcessChantierResultDto rawResult,
            ConfigurationPlanification config,
            IReadOnlyList<Ouvrier> poolOuvriers,
            IReadOnlyList<Metier> poolMetiers,
            dynamic preparationResult,
            bool reinitialiserBaseline)
        {
            // Logique existante pour les succès
            var consolidatedPlanning = _consolidationService.Process(rawResult, config);
            _planningService.UpdatePlanning(consolidatedPlanning, config);
            _taskManagerService.MettreAJourApresPlanification(_planningService, preparationResult);

            // Gestion de la baseline
            if (reinitialiserBaseline || _planningService.GetBaseline() == null)
            {
                _CreerEtStockerNouvelleBaseline(consolidatedPlanning, config, poolOuvriers);
            }

            var joursOuvresCalculator = new AnalysisService.JoursOuvresCalculator((start, end) =>
                _planningService.GetNombreJoursOuvres(start, end));

            var analysisReport = _analysisService.GenerateReport(
                consolidatedPlanning, poolOuvriers, poolMetiers, config, joursOuvresCalculator);

            return new PlanificationRunResult
            {
                RawResult = rawResult,
                AnalysisReport = analysisReport,
                Success = true
            };
        }

        /// <summary>
        /// Traite le cas où le problème est infaisable avec analyse des causes
        /// </summary>
        private PlanificationRunResult TraiterResultatInfaisable(
            ProcessChantierResultDto rawResult,
            ConfigurationPlanification config,
            IReadOnlyList<Ouvrier> poolOuvriers,
            IReadOnlyList<Metier> poolMetiers)
        {
            // Analyser les messages pour identifier les causes spécifiques
            var analyseCauses = AnalyserCausesInfaisabilite(rawResult.Messages, poolOuvriers, poolMetiers);

            return new PlanificationRunResult
            {
                RawResult = rawResult,
                Success = false,
                ErrorType = "INFEASIBLE",
                UserMessage = $"Planning impossible à réaliser avec les contraintes actuelles.\n\n{analyseCauses.RecommandationsPrincipales}\n\nDétails techniques :\n{analyseCauses.DetailsAnalyse}"
            };
        }

        /// <summary>
        /// Traite le cas où le solveur n'a pas eu assez de temps (Unknown)
        /// </summary>
        private PlanificationRunResult TraiterResultatTimeout(ProcessChantierResultDto rawResult, ConfigurationPlanification config)
        {
            var tempsActuel = config.DureeCalculMaxMinutes;
            var tempsRecommande = Math.Min(tempsActuel * 2, 60); // Doubler le temps, max 60 min

            return new PlanificationRunResult
            {
                RawResult = rawResult,
                Success = false,
                ErrorType = "TIMEOUT_EXPLORATION",
                UserMessage = $"Le solveur n'a pas eu assez de temps pour explorer toutes les solutions possibles (actuellement {tempsActuel} min).\n\n" +
                             $"Recommandations :\n" +
                             $"• Augmentez le temps de calcul à {tempsRecommande} minutes minimum\n" +
                             $"• Ou simplifiez temporairement le projet (moins de tâches/contraintes)\n" +
                             $"• Le solveur pourrait trouver une solution avec plus de temps"
            };
        }

        /// <summary>
        /// Traite le cas où le solveur a été interrompu (Aborted)
        /// </summary>
        private PlanificationRunResult TraiterResultatInterrompu(ProcessChantierResultDto rawResult)
        {
            return new PlanificationRunResult
            {
                RawResult = rawResult,
                Success = false,
                ErrorType = "ABORTED",
                UserMessage = "La planification a été interrompue avant la fin du calcul.\n\n" +
                             "Cela peut arriver si :\n" +
                             "• Le processus a été arrêté manuellement\n" +
                             "• Une erreur système est survenue\n" +
                             "• Relancez la planification pour un nouveau calcul"
            };
        }

        /// <summary>
        /// Traite le cas d'erreur de modèle
        /// </summary>
        private PlanificationRunResult TraiterResultatErreurModele(ProcessChantierResultDto rawResult)
        {
            return new PlanificationRunResult
            {
                RawResult = rawResult,
                Success = false,
                ErrorType = "MODEL_ERROR",
                UserMessage = "Erreur technique dans la construction du modèle mathématique.\n\n" +
                             "Cette erreur indique un problème de cohérence dans les données du projet.\n" +
                             "Vérifiez les dépendances entre tâches et contactez le support technique si le problème persiste."
            };
        }

        /// <summary>
        /// Traite le cas de statut inconnu
        /// </summary>
        private PlanificationRunResult TraiterResultatInconnu(ProcessChantierResultDto rawResult)
        {
            return new PlanificationRunResult
            {
                RawResult = rawResult,
                Success = false,
                ErrorType = "UNKNOWN",
                UserMessage = "Statut de résolution indéterminé.\n\n" +
                             "Le solveur a retourné un état non reconnu.\n" +
                             "Contactez le support technique en joignant les logs de cette session."
            };
        }

        /// <summary>
        /// Analyse les messages d'erreur pour identifier les causes spécifiques d'infaisabilité
        /// et fournir des recommandations ciblées à l'utilisateur
        /// </summary>
        private (string RecommandationsPrincipales, string DetailsAnalyse) AnalyserCausesInfaisabilite(
    IReadOnlyList<MessageValidationDto> messages,
    IReadOnlyList<Ouvrier> poolOuvriers,
    IReadOnlyList<Metier> poolMetiers)
        {
            var recommandations = new List<string>();
            var details = new List<string>();

            // 1. PRIORITÉ : Analyser les problèmes de charge/délais
            var messagesCapacite = messages.Where(m =>
                m.CodeMessage == "WARN_CAPACITY_OVERLOAD" ||
                m.CodeMessage.Contains("CHARGE") ||
                m.CodeMessage.Contains("DELAI")).ToList();

            if (messagesCapacite.Any())
            {
                recommandations.Add("📅 DÉLAIS INSUFFISANTS :");

                foreach (var msg in messagesCapacite)
                {
                    // Extraire les heures du message (pattern: "XXXh requises vs ~YYYh disponibles")
                    var match = System.Text.RegularExpressions.Regex.Match(msg.Message, @"(\d+)h requises vs ~(\d+)h disponibles");
                    if (match.Success)
                    {
                        var heuresRequises = int.Parse(match.Groups[1].Value);
                        var heuresDisponibles = int.Parse(match.Groups[2].Value);
                        var ecartHeures = heuresRequises - heuresDisponibles;

                        // Calculer l'extension de planning nécessaire
                        var heuresTravailParJour = 7; // Valeur par défaut, pourrait être récupérée de la config
                        var joursSupplementaires = Math.Ceiling((double)ecartHeures / heuresTravailParJour);

                        recommandations.Add($"  • Charge de travail excessive : {heuresRequises}h vs {heuresDisponibles}h disponibles");
                        recommandations.Add($"  • SOLUTION : Prolonger le planning d'au moins {joursSupplementaires} jours ouvrés");
                        recommandations.Add($"  • OU recruter temporairement des ouvriers supplémentaires");
                    }
                    else
                    {
                        // Fallback si le pattern n'est pas reconnu
                        recommandations.Add($"  • {msg.Message}");
                        recommandations.Add($"  • SOLUTION : Prolonger significativement les délais du projet");
                    }
                }
            }

            // 2. Analyser les messages de ressources insuffisantes (métiers spécifiques)
            var messagesRessources = messages.Where(m => m.CodeMessage == "WARN_INSUFFICIENT_WORKERS").ToList();
            if (messagesRessources.Any())
            {
                recommandations.Add("🔧 RESSOURCES HUMAINES :");
                foreach (var msg in messagesRessources)
                {
                    // Extraire le métier du message (pattern: "Métier 'X' : YYYh")
                    var match = System.Text.RegularExpressions.Regex.Match(msg.Message, @"Métier '([^']+)' : (\d+)h");
                    if (match.Success)
                    {
                        var metier = match.Groups[1].Value;
                        var heures = int.Parse(match.Groups[2].Value);
                        var joursEstimes = Math.Ceiling(heures / 8.0); // Estimation en jours de 8h

                        recommandations.Add($"  • Ajouter au moins 1 ouvrier '{metier}' (charge: {joursEstimes:F0} jours)");
                        recommandations.Add($"  • OU prolonger les délais pour permettre un travail séquentiel");
                    }
                }
            }

            // 3. Analyser les messages de surcapacité des blocs (moins prioritaire)
            var messagesBlocs = messages.Where(m => m.CodeMessage == "WARN_BLOC_OVERCAPACITY").ToList();
            if (messagesBlocs.Any())
            {
                recommandations.Add("📐 ORGANISATION SPATIALE (secondaire) :");
                foreach (var msg in messagesBlocs)
                {
                    // Extraire les informations du bloc
                    var match = System.Text.RegularExpressions.Regex.Match(msg.Message, @"Bloc '([^']+)' : (\d+) ouvriers.*?(\d+)");
                    if (match.Success)
                    {
                        var bloc = match.Groups[1].Value;
                        var disponibles = int.Parse(match.Groups[2].Value);
                        var capacite = int.Parse(match.Groups[3].Value);

                        recommandations.Add($"  • Bloc '{bloc}': {disponibles} ouvriers disponibles pour {capacite} places max");
                        recommandations.Add($"  • Augmenter la capacité du bloc ou étaler le travail dans le temps");
                    }
                }
            }

            // 4. Si aucune cause spécifique détectée, recommandations générales
            if (!recommandations.Any())
            {
                recommandations.Add("📋 DIAGNOSTIC GÉNÉRAL :");
                recommandations.Add("  • Les contraintes temporelles sont trop restrictives");
                recommandations.Add("  • SOLUTION PRINCIPALE : Prolonger les délais du projet");
                recommandations.Add("  • Vérifier la disponibilité des ressources pour tous les métiers");
                recommandations.Add("  • Réduire le nombre de tâches simultanées si possible");
            }

            // Construire les détails techniques
            details.Add($"Messages du solveur ({messages.Count}) :");
            foreach (var msg in messages.Take(5)) // Limiter à 5 messages pour éviter le spam
            {
                details.Add($"  [{msg.Type}] {msg.CodeMessage}: {msg.Message}");
            }

            if (messages.Count > 5)
            {
                details.Add($"  ... et {messages.Count - 5} autres messages (voir logs complets)");
            }

            return (string.Join("\n", recommandations), string.Join("\n", details));
        }

        /// <summary>
        /// Méthode privée pour encapsuler la logique de création de la baseline.
        /// </summary>
        private void _CreerEtStockerNouvelleBaseline(
            ConsolidatedPlanning planning,
            ConfigurationPlanification config,
            IReadOnlyList<Ouvrier> poolOuvriers)
        {
            var joursOuvresCalculator = new AnalysisService.JoursOuvresCalculator((start, end) =>
                _planningService.GetNombreJoursOuvres(start, end));

            // 1. Calculer les données de la baseline
            var bac = _analysisService.CalculerBudgetTotal(planning, poolOuvriers, config, joursOuvresCalculator);
            var courbePV = _analysisService.CalculerCourbePlannedValueCumulative(planning, poolOuvriers, config);
            var budgetTaches = _analysisService.CalculerBudgetParTache(planning, poolOuvriers, config);

            // 2. Créer l'objet Baseline
            var nouvelleBaseline = new PlanningBaseline
            {
                DateCreation = DateTime.Now,
                BudgetAtCompletion = bac,
                DateFinPlanifieeInitiale = planning.DateFinProjet,
                CourbePlannedValueCumulative = courbePV,
                BudgetInitialParTacheId = budgetTaches,
                ConsPlanningInitial = planning,       // Stocke une copie du planning
                ConfigurationInitiale = config        // Stocke une copie de la config
            };

            // 3. Stocker la baseline dans le service d'état
            _planningService.SetBaseline(nouvelleBaseline);
        }
    }
}