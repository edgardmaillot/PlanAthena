// Fichier: /Services/UseCases/PlanificationOrchestrator.cs Version 0.6.0

using PlanAthena.Core.Facade;
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
    /// Version 0.6.0 : Ajout de la logique de création de la PlanningBaseline.
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

            // --- PHASE 3 & 4 : TRAITEMENT DU RÉSULTAT ---
            if (rawResult.AnalyseStatiqueResultat?.OuvriersClesSuggereIds?.Any() == true)
            {
                var tensionReport = _analysisService.AnalyzeMetierTension(
                    rawResult.AnalyseStatiqueResultat.OuvriersClesSuggereIds, poolOuvriers);
                return new PlanificationRunResult { RawResult = rawResult, MetierTensionReport = tensionReport };
            }
            else if (rawResult.OptimisationResultat != null)
            {
                var consolidatedPlanning = _consolidationService.Process(rawResult, config);
                _planningService.UpdatePlanning(consolidatedPlanning, config);
                _taskManagerService.MettreAJourApresPlanification(_planningService, preparationResult);

                // *** NOUVELLE LOGIQUE DE GESTION DE LA BASELINE ***
                if (reinitialiserBaseline || _planningService.GetBaseline() == null)
                {
                    _CreerEtStockerNouvelleBaseline(consolidatedPlanning, config, poolOuvriers);
                }

                var joursOuvresCalculator = new AnalysisService.JoursOuvresCalculator((start, end) =>
                    _planningService.GetNombreJoursOuvres(start, end));

                var analysisReport = _analysisService.GenerateReport(
                    consolidatedPlanning, poolOuvriers, poolMetiers, config, joursOuvresCalculator);

                return new PlanificationRunResult { RawResult = rawResult, AnalysisReport = analysisReport };
            }
            else
            {
                return new PlanificationRunResult { RawResult = rawResult };
            }
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