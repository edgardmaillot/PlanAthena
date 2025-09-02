// /Services/UseCases/PlanificationOrchestrator.cs V0.4.8

using PlanAthena.Core.Facade;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.Processing;
using PlanAthena.Utilities;

namespace PlanAthena.Services.UseCases
{
    /// <summary>
    /// Exécute le cas d'utilisation "Lancer une nouvelle planification".
    /// Il agit comme un chef d'orchestre, appelant les services appropriés dans le bon ordre.
    /// Ce service est stateless.
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

        public async Task<PlanificationRunResult> ExecuteAsync(ConfigurationPlanification config)
        {
            // --- PHASE 1 : COLLECTE & PRÉPARATION ---
            var projetData = _projetService.GetProjetDataPourSauvegarde();
            var poolOuvriers = _ressourceService.GetAllOuvriers();
            var poolMetiers = _ressourceService.GetAllMetiers();

            // MODIFIÉ: La source de vérité pour les tâches est maintenant TaskManagerService
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
                // --- CAS B : OPTIMISATION ---
                var consolidatedPlanning = _consolidationService.Process(rawResult, config);
                _planningService.UpdatePlanning(consolidatedPlanning, config);

                // --- NOUVELLE ÉTAPE DE RÉCONCILIATION ---
                // On informe le TaskManagerService de la nouvelle décomposition des tâches.
                var tachesCalculees = preparationResult.TachesPreparees;
                var mappageParentEnfant = preparationResult.ParentIdParSousTacheId;
                _taskManagerService.MettreAJourDecompositionTaches(tachesCalculees, mappageParentEnfant);

                var joursOuvresCalculator = new AnalysisService.JoursOuvresCalculator((start, end) =>
                    _planningService.GetNombreJoursOuvres(start, end));

                var analysisReport = _analysisService.GenerateReport(
                    consolidatedPlanning,
                    poolOuvriers,
                    poolMetiers,
                    config,
                    joursOuvresCalculator);

                return new PlanificationRunResult { RawResult = rawResult, AnalysisReport = analysisReport };
            }
            else
            {
                return new PlanificationRunResult { RawResult = rawResult };
            }
        }
    }
}