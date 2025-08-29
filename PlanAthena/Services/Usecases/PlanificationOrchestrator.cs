// Emplacement: /Services/UseCases/PlanificationOrchestrator.cs

using PlanAthena.Core.Facade;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs; // AJOUTÉ : Pour accéder aux DTOs officiels
using PlanAthena.Services.Processing;
using PlanAthena.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly PreparationSolveurService _preparationService;
        private readonly DataTransformer _transformerService;
        private readonly PlanAthenaCoreFacade _facade;
        private readonly PlanningConsolidationService _consolidationService;
        private readonly PlanningService _planningService;
        private readonly TaskStatusService _taskStatusService;
        private readonly AnalysisService _analysisService;

        public PlanificationOrchestrator(
            ProjetService projetService,
            RessourceService ressourceService,
            PreparationSolveurService preparationService,
            DataTransformer transformerService,
            PlanAthenaCoreFacade facade,
            PlanningConsolidationService consolidationService,
            PlanningService planningService,
            TaskStatusService taskStatusService,
            AnalysisService analysisService)
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _preparationService = preparationService ?? throw new ArgumentNullException(nameof(preparationService));
            _transformerService = transformerService ?? throw new ArgumentNullException(nameof(transformerService));
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _consolidationService = consolidationService ?? throw new ArgumentNullException(nameof(consolidationService));
            _planningService = planningService ?? throw new ArgumentNullException(nameof(planningService));
            _taskStatusService = taskStatusService ?? throw new ArgumentNullException(nameof(taskStatusService));
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        }

        /// <summary>
        /// Point d'entrée principal pour exécuter le processus de planification de bout en bout.
        /// </summary>
        /// <param name="config">La configuration de planification fournie par l'utilisateur.</param>
        /// <returns>Un DTO contenant tous les résultats du run.</returns>
        public async Task<PlanificationRunResult> ExecuteAsync(ConfigurationPlanification config)
        {
            // --- PHASE 1 : COLLECTE & PRÉPARATION ---
            var projetData = _projetService.GetProjetDataPourSauvegarde();
            var poolOuvriers = _ressourceService.GetAllOuvriers();
            var poolMetiers = _ressourceService.GetAllMetiers();
            var tachesOriginales = _projetService.ObtenirToutesLesTaches();

            var preparationResult = _preparationService.PreparerPourSolveur(tachesOriginales, config);

            var inputDto = _transformerService.TransformToChantierSetupDto(
                 projetData, poolOuvriers, poolMetiers, preparationResult.TachesPreparees, config);

            // --- PHASE 2 : APPEL AU MOTEUR DE CALCUL ---
            var rawResult = await _facade.ProcessChantierAsync(inputDto);

            // --- PHASE 3 & 4 : TRAITEMENT DU RÉSULTAT ---
            if (rawResult.AnalyseStatiqueResultat?.OuvriersClesSuggereIds?.Any() == true)
            {
                // --- CAS A : ANALYSE RAPIDE ---
                var tensionReport = _analysisService.AnalyzeMetierTension(
                    rawResult.AnalyseStatiqueResultat.OuvriersClesSuggereIds, poolOuvriers);

                // CORRECTION 1: On s'assure d'utiliser le type `MetierTensionReport` du namespace DTOs.
                return new PlanificationRunResult { RawResult = rawResult, MetierTensionReport = tensionReport };
            }
            else if (rawResult.OptimisationResultat != null)
            {
                // --- CAS B : OPTIMISATION ---
                var consolidatedPlanning = _consolidationService.Process(rawResult, config);

                _planningService.UpdatePlanning(consolidatedPlanning, config);

                var plannedTaskIds = consolidatedPlanning.SegmentsParOuvrierId
                    .SelectMany(kvp => kvp.Value)
                    .Select(s => s.ParentTacheId ?? s.TacheId)
                    .Distinct()
                    .ToList();
                _taskStatusService.ModifierTachePlanifiee(plannedTaskIds);

                // CORRECTION 2: L'expression lambda est explicitement castée vers le type de délégué attendu par AnalysisService.
                // Cela résout l'ambiguïté de type pour le compilateur.
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
                // --- CAS C : ERREUR ou RÉSULTAT INATTENDU ---
                return new PlanificationRunResult { RawResult = rawResult };
            }
        }
    }

    // SUPPRIMÉ : La définition locale de `PlanificationRunResult` a été retirée pour utiliser
    // une version officielle qui sera définie dans son propre fichier (par exemple, DTOs/PlanificationRunResult.cs)
    // Idem pour MetierTensionReport et MetierCount, qui utilisent maintenant les records du namespace DTOs.

}