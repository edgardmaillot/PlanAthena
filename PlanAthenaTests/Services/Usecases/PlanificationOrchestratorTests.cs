// Emplacement: /PlanAthena.Tests/Services/UseCases/PlanificationOrchestratorTests.cs
// Assurez-vous d'avoir la référence au package NuGet "Moq"

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.Processing;
using PlanAthena.Services.UseCases;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlanAthena.Tests.Services.UseCases
{
    [TestClass]
    public class PlanificationOrchestratorTests
    {
        // Mocks pour toutes les dépendances
        private Mock<ProjetService> _mockProjetService;
        private Mock<RessourceService> _mockRessourceService;
        private Mock<PreparationSolveurService> _mockPreparationService;
        private Mock<DataTransformer> _mockTransformerService;
        private Mock<PlanAthenaCoreFacade> _mockFacade;
        private Mock<PlanningConsolidationService> _mockConsolidationService;
        private Mock<PlanningService> _mockPlanningService;
        private Mock<TaskStatusService> _mockTaskStatusService;
        private Mock<AnalysisService> _mockAnalysisService;

        private PlanificationOrchestrator _orchestrator;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialisation des mocks
            var mockIdGenerator = new Mock<PlanAthena.Interfaces.IIdGeneratorService>();
            _mockProjetService = new Mock<ProjetService>(mockIdGenerator.Object) { CallBase = true };
            _mockRessourceService = new Mock<RessourceService>(mockIdGenerator.Object) { CallBase = true };
            _mockPreparationService = new Mock<PreparationSolveurService>() { CallBase = true };
            _mockTransformerService = new Mock<DataTransformer>() { CallBase = true };
            _mockConsolidationService = new Mock<PlanningConsolidationService>() { CallBase = true };
            _mockPlanningService = new Mock<PlanningService>() { CallBase = true };
            _mockTaskStatusService = new Mock<TaskStatusService>(_mockProjetService.Object, _mockPlanningService.Object, _mockRessourceService.Object) { CallBase = true };
            _mockAnalysisService = new Mock<AnalysisService>() { CallBase = true };

            var mockProcessChantierUseCase = new Mock<IProcessChantierUseCase>();
            _mockFacade = new Mock<PlanAthenaCoreFacade>(mockProcessChantierUseCase.Object) { CallBase = true };

            // Création de l'instance de l'orchestrateur avec les mocks
            _orchestrator = new PlanificationOrchestrator(
                _mockProjetService.Object,
                _mockRessourceService.Object,
                _mockPreparationService.Object,
                _mockTransformerService.Object,
                _mockFacade.Object,
                _mockConsolidationService.Object,
                _mockPlanningService.Object,
                _mockTaskStatusService.Object,
                _mockAnalysisService.Object
            );
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenOptimisationSucceeds_ShouldCallFullWorkflowAndPassMetiers()
        {
            // --- ARRANGE ---
            var config = new ConfigurationPlanification();
            var sampleMetiers = new List<Metier> { new Metier { MetierId = "TEST" } };
            _mockRessourceService.Setup(r => r.GetAllMetiers()).Returns(sampleMetiers);

            var rawResult = new ProcessChantierResultDto
            {
                OptimisationResultat = new PlanningOptimizationResultDto()
            };
            _mockFacade.Setup(f => f.ProcessChantierAsync(It.IsAny<ChantierSetupInputDto>())).ReturnsAsync(rawResult);

            _mockPreparationService.Setup(p => p.PreparerPourSolveur(It.IsAny<IReadOnlyList<Tache>>(), It.IsAny<ConfigurationPlanification>()))
                .Returns(new PreparationResult { TachesPreparees = new List<Tache>(), ParentIdParSousTacheId = new Dictionary<string, string>() });

            var consolidatedPlanning = new ConsolidatedPlanning();
            _mockConsolidationService.Setup(c => c.Process(It.IsAny<ProcessChantierResultDto>(), It.IsAny<ConfigurationPlanification>()))
                .Returns(consolidatedPlanning);

            var analysisReport = new AnalysisReport();
            _mockAnalysisService.Setup(a => a.GenerateReport(
                It.IsAny<ConsolidatedPlanning>(),
                It.IsAny<IReadOnlyList<Ouvrier>>(),
                It.IsAny<IReadOnlyList<Metier>>(),
                It.IsAny<ConfigurationPlanification>(),
                It.IsAny<AnalysisService.JoursOuvresCalculator>()
            )).Returns(analysisReport);

            // --- ACT ---
            var result = await _orchestrator.ExecuteAsync(config);

            // --- ASSERT ---
            _mockConsolidationService.Verify(c => c.Process(rawResult, config), Times.Once);
            _mockPlanningService.Verify(p => p.UpdatePlanning(consolidatedPlanning, config), Times.Once);
            _mockTaskStatusService.Verify(t => t.ModifierTachePlanifiee(It.IsAny<IEnumerable<string>>()), Times.Once);

            // On s'assure que la liste de métiers provenant du RessourceService est bien celle passée à l'AnalysisService.
            _mockAnalysisService.Verify(a => a.GenerateReport(
                consolidatedPlanning,
                It.IsAny<IReadOnlyList<Ouvrier>>(),
                sampleMetiers, // On vérifie que la liste `sampleMetiers` est bien passée
                config,
                It.IsAny<AnalysisService.JoursOuvresCalculator>()
            ), Times.Once);

            Assert.IsNotNull(result.AnalysisReport);
            Assert.IsNull(result.MetierTensionReport);
            Assert.AreSame(rawResult, result.RawResult);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenAnalyseRapideSucceeds_ShouldCallAnalysisWorkflow()
        {
            // --- ARRANGE ---
            var config = new ConfigurationPlanification();

            var rawResult = new ProcessChantierResultDto
            {
                AnalyseStatiqueResultat = new AnalyseRessourcesResultatDto { OuvriersClesSuggereIds = new List<string> { "O1" } }
            };
            _mockFacade.Setup(f => f.ProcessChantierAsync(It.IsAny<ChantierSetupInputDto>())).ReturnsAsync(rawResult);
            _mockPreparationService.Setup(p => p.PreparerPourSolveur(It.IsAny<IReadOnlyList<Tache>>(), It.IsAny<ConfigurationPlanification>()))
               .Returns(new PreparationResult { TachesPreparees = new List<Tache>(), ParentIdParSousTacheId = new Dictionary<string, string>() });

            var tensionReport = new MetierTensionReport();
            _mockAnalysisService.Setup(a => a.AnalyzeMetierTension(It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<Ouvrier>>()))
                .Returns(tensionReport);

            // --- ACT ---
            var result = await _orchestrator.ExecuteAsync(config);

            // --- ASSERT ---
            _mockAnalysisService.Verify(a => a.AnalyzeMetierTension(rawResult.AnalyseStatiqueResultat.OuvriersClesSuggereIds, It.IsAny<IReadOnlyList<Ouvrier>>()), Times.Once);

            _mockPlanningService.Verify(p => p.UpdatePlanning(It.IsAny<ConsolidatedPlanning>(), It.IsAny<ConfigurationPlanification>()), Times.Never);
            _mockTaskStatusService.Verify(t => t.ModifierTachePlanifiee(It.IsAny<IEnumerable<string>>()), Times.Never);
            _mockAnalysisService.Verify(a => a.GenerateReport(
                It.IsAny<ConsolidatedPlanning>(),
                It.IsAny<IReadOnlyList<Ouvrier>>(),
                It.IsAny<IReadOnlyList<Metier>>(),
                It.IsAny<ConfigurationPlanification>(),
                It.IsAny<AnalysisService.JoursOuvresCalculator>()
            ), Times.Never);

            Assert.IsNotNull(result.MetierTensionReport);
            Assert.IsNull(result.AnalysisReport);
            Assert.AreSame(rawResult, result.RawResult);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenResultIsEmpty_ShouldReturnRawResultOnly()
        {
            // --- ARRANGE ---
            var config = new ConfigurationPlanification();

            var rawResult = new ProcessChantierResultDto
            {
                OptimisationResultat = null,
                AnalyseStatiqueResultat = null
            };
            _mockFacade.Setup(f => f.ProcessChantierAsync(It.IsAny<ChantierSetupInputDto>())).ReturnsAsync(rawResult);
            _mockPreparationService.Setup(p => p.PreparerPourSolveur(It.IsAny<IReadOnlyList<Tache>>(), It.IsAny<ConfigurationPlanification>()))
               .Returns(new PreparationResult { TachesPreparees = new List<Tache>(), ParentIdParSousTacheId = new Dictionary<string, string>() });

            // --- ACT ---
            var result = await _orchestrator.ExecuteAsync(config);

            // --- ASSERT ---
            _mockConsolidationService.Verify(c => c.Process(It.IsAny<ProcessChantierResultDto>(), It.IsAny<ConfigurationPlanification>()), Times.Never);
            _mockAnalysisService.Verify(a => a.AnalyzeMetierTension(It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<Ouvrier>>()), Times.Never);
            _mockAnalysisService.Verify(a => a.GenerateReport(
                It.IsAny<ConsolidatedPlanning>(),
                It.IsAny<IReadOnlyList<Ouvrier>>(),
                It.IsAny<IReadOnlyList<Metier>>(),
                It.IsAny<ConfigurationPlanification>(),
                It.IsAny<AnalysisService.JoursOuvresCalculator>()
            ), Times.Never);

            Assert.IsNull(result.AnalysisReport);
            Assert.IsNull(result.MetierTensionReport);
            Assert.AreSame(rawResult, result.RawResult);
        }
    }
}