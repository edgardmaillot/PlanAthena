// Emplacement: /PlanAthena.Tests/Services/UseCases/PlanificationOrchestratorTests.cs

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

namespace PlanAthena.Tests.Services.UseCases
{
    [TestClass]
    public class PlanificationOrchestratorTests
    {
        private Mock<ProjetService> _mockProjetService;
        private Mock<RessourceService> _mockRessourceService;
        private Mock<TaskManagerService> _mockTaskManagerService;
        private Mock<PreparationSolveurService> _mockPreparationService;
        private Mock<DataTransformer> _mockTransformerService;
        private Mock<PlanAthenaCoreFacade> _mockFacade;
        private Mock<PlanningConsolidationService> _mockConsolidationService;
        private Mock<PlanningService> _mockPlanningService;
        private Mock<AnalysisService> _mockAnalysisService;

        private PlanificationOrchestrator _orchestrator;

        [TestInitialize]
        public void TestInitialize()
        {
            var mockIdGenerator = new Mock<PlanAthena.Interfaces.IIdGeneratorService>();

            // Étape 1: Créer les mocks de base
            _mockProjetService = new Mock<ProjetService>(mockIdGenerator.Object);
            _mockRessourceService = new Mock<RessourceService>(mockIdGenerator.Object);

            // Étape 2: Créer les mocks qui dépendent des précédents
            // PlanningService dépend de RessourceService
            _mockPlanningService = new Mock<PlanningService>(_mockRessourceService.Object);
            // TaskManagerService dépend de PlanningService et IIdGeneratorService
            _mockTaskManagerService = new Mock<TaskManagerService>(_mockPlanningService.Object, mockIdGenerator.Object);

            // Le reste des mocks n'a pas de dépendances complexes entre eux
            _mockPreparationService = new Mock<PreparationSolveurService>();
            _mockTransformerService = new Mock<DataTransformer>();
            _mockConsolidationService = new Mock<PlanningConsolidationService>();
            _mockAnalysisService = new Mock<AnalysisService>();
            var mockProcessChantierUseCase = new Mock<IProcessChantierUseCase>();
            _mockFacade = new Mock<PlanAthenaCoreFacade>(mockProcessChantierUseCase.Object);

            // Activer CallBase = true sur tous les mocks de nos services pour tester la vraie logique
            _mockProjetService.CallBase = true;
            _mockRessourceService.CallBase = true;
            _mockPlanningService.CallBase = true;
            _mockTaskManagerService.CallBase = true;
            _mockPreparationService.CallBase = true;
            _mockTransformerService.CallBase = true;
            _mockConsolidationService.CallBase = true;
            _mockAnalysisService.CallBase = true;
            _mockFacade.CallBase = true;

            // Étape 3: Créer l'objet à tester avec tous les mocks prêts
            _orchestrator = new PlanificationOrchestrator(
                _mockProjetService.Object,
                _mockRessourceService.Object,
                _mockTaskManagerService.Object,
                _mockPreparationService.Object,
                _mockTransformerService.Object,
                _mockFacade.Object,
                _mockConsolidationService.Object,
                _mockPlanningService.Object,
                _mockAnalysisService.Object
            );
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenOptimisationSucceeds_ShouldCallFullWorkflow()
        {
            // --- ARRANGE ---
            var config = new ConfigurationPlanification();
            var sampleMetiers = new List<Metier> { new Metier { MetierId = "TEST" } };
            var sampleOuvriers = new List<Ouvrier> { new Ouvrier { OuvrierId = "O1" } };

            _mockRessourceService.Setup(r => r.GetAllMetiers()).Returns(sampleMetiers);
            _mockRessourceService.Setup(r => r.GetAllOuvriers()).Returns(sampleOuvriers);

            var rawResult = new ProcessChantierResultDto { OptimisationResultat = new PlanningOptimizationResultDto() };
            _mockFacade.Setup(f => f.ProcessChantierAsync(It.IsAny<ChantierSetupInputDto>())).ReturnsAsync(rawResult);

            var preparationResult = new PreparationResult
            {
                TachesPreparees = new List<Tache>(),
                ParentIdParSousTacheId = new Dictionary<string, string>()
            };
            _mockPreparationService.Setup(p => p.PreparerPourSolveur(It.IsAny<IReadOnlyList<Tache>>(), It.IsAny<ConfigurationPlanification>()))
                .Returns(preparationResult);

            var consolidatedPlanning = new ConsolidatedPlanning();
            _mockConsolidationService.Setup(c => c.Process(It.IsAny<ProcessChantierResultDto>(), It.IsAny<ConfigurationPlanification>()))
                .Returns(consolidatedPlanning);

            var analysisReport = new AnalysisReport();
            _mockAnalysisService.Setup(a => a.GenerateReport(
                It.IsAny<ConsolidatedPlanning>(),
                It.IsAny<IReadOnlyList<Ouvrier>>(),
                It.IsAny<IReadOnlyList<Metier>>(),
                It.IsAny<ConfigurationPlanification>(),
                It.IsAny<AnalysisService.JoursOuvresCalculator>()))
                .Returns(analysisReport);

            // Setup pour la nouvelle méthode MettreAJourApresPlanification
            _mockTaskManagerService.Setup(t => t.MettreAJourApresPlanification(
                It.IsAny<PlanningService>(),
                It.IsAny<PreparationResult>()));

            // --- ACT ---
            var result = await _orchestrator.ExecuteAsync(config);

            // --- ASSERT ---
            _mockPlanningService.Verify(p => p.UpdatePlanning(consolidatedPlanning, config), Times.Once);

            // Vérifier que la nouvelle méthode de mise à jour après planification est bien appelée
            _mockTaskManagerService.Verify(t => t.MettreAJourApresPlanification(
                _mockPlanningService.Object,
                preparationResult), Times.Once);

            _mockAnalysisService.Verify(a => a.GenerateReport(
                consolidatedPlanning,
                sampleOuvriers,
                sampleMetiers,
                config,
                It.IsAny<AnalysisService.JoursOuvresCalculator>()), Times.Once);

            Assert.IsNotNull(result.AnalysisReport);
            Assert.AreSame(rawResult, result.RawResult);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenAnalyseRapideSucceeds_ShouldCallAnalysisWorkflow()
        {
            // --- ARRANGE ---
            var config = new ConfigurationPlanification();
            var sampleOuvriers = new List<Ouvrier> { new Ouvrier { OuvrierId = "O1" } };

            _mockRessourceService.Setup(r => r.GetAllOuvriers()).Returns(sampleOuvriers);

            var rawResult = new ProcessChantierResultDto
            {
                AnalyseStatiqueResultat = new AnalyseRessourcesResultatDto
                {
                    OuvriersClesSuggereIds = new List<string> { "O1" }
                }
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
            _mockAnalysisService.Verify(a => a.AnalyzeMetierTension(
                rawResult.AnalyseStatiqueResultat.OuvriersClesSuggereIds,
                sampleOuvriers), Times.Once);

            // Dans le cas d'analyse rapide, MettreAJourApresPlanification ne doit PAS être appelée
            _mockTaskManagerService.Verify(t => t.MettreAJourApresPlanification(
                It.IsAny<PlanningService>(),
                It.IsAny<PreparationResult>()), Times.Never);

            Assert.IsNotNull(result.MetierTensionReport);
            Assert.AreSame(rawResult, result.RawResult);
            Assert.IsNull(result.AnalysisReport);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenResultIsEmpty_ShouldReturnRawResultOnly()
        {
            // --- ARRANGE ---
            var config = new ConfigurationPlanification();
            var rawResult = new ProcessChantierResultDto { OptimisationResultat = null, AnalyseStatiqueResultat = null };
            _mockFacade.Setup(f => f.ProcessChantierAsync(It.IsAny<ChantierSetupInputDto>())).ReturnsAsync(rawResult);
            _mockPreparationService.Setup(p => p.PreparerPourSolveur(It.IsAny<IReadOnlyList<Tache>>(), It.IsAny<ConfigurationPlanification>()))
               .Returns(new PreparationResult { TachesPreparees = new List<Tache>(), ParentIdParSousTacheId = new Dictionary<string, string>() });

            // --- ACT ---
            var result = await _orchestrator.ExecuteAsync(config);

            // --- ASSERT ---
            Assert.IsNull(result.AnalysisReport);
            Assert.IsNull(result.MetierTensionReport);
            Assert.AreSame(rawResult, result.RawResult);

            // Dans ce cas, aucune mise à jour ne doit être effectuée
            _mockTaskManagerService.Verify(t => t.MettreAJourApresPlanification(
                It.IsAny<PlanningService>(),
                It.IsAny<PreparationResult>()), Times.Never);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldAlwaysCallPreparationService()
        {
            // --- ARRANGE ---
            var config = new ConfigurationPlanification();
            var rawResult = new ProcessChantierResultDto();
            _mockFacade.Setup(f => f.ProcessChantierAsync(It.IsAny<ChantierSetupInputDto>())).ReturnsAsync(rawResult);

            var expectedTaches = new List<Tache> { new Tache { TacheId = "T1" } };
            // Si ObtenirToutesLesTaches a des paramètres optionnels, on utilise It.IsAny pour tous
            _mockTaskManagerService.Setup(t => t.ObtenirToutesLesTaches(It.IsAny<string?>(), It.IsAny<string>())).Returns(expectedTaches);

            var preparationResult = new PreparationResult
            {
                TachesPreparees = new List<Tache>(),
                ParentIdParSousTacheId = new Dictionary<string, string>()
            };
            _mockPreparationService.Setup(p => p.PreparerPourSolveur(It.IsAny<IReadOnlyList<Tache>>(), It.IsAny<ConfigurationPlanification>()))
                .Returns(preparationResult);

            // --- ACT ---
            await _orchestrator.ExecuteAsync(config);

            // --- ASSERT ---
            _mockTaskManagerService.Verify(t => t.ObtenirToutesLesTaches(It.IsAny<string?>(), It.IsAny<string>()), Times.Once);
            _mockPreparationService.Verify(p => p.PreparerPourSolveur(It.IsAny<IReadOnlyList<Tache>>(), config), Times.Once);
        }
    }
}