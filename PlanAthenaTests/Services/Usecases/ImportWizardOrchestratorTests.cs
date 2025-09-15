using Moq;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.DTOs.ImportExport;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Usecases;
using PlanAthena.Utilities;

namespace PlanAthenaTests.Services.Usecases
{
    [TestClass]
    public class ImportWizardOrchestratorTests
    {
        private Mock<ImportService> _mockImportService;
        private Mock<RessourceService> _mockRessourceService;
        private Mock<ProjetService> _mockProjetService;
        private Mock<ValueMappingService> _mockValueMappingService;
        private ImportWizardOrchestrator _orchestrator;
        private string _tempFilePath;
        private List<Metier> _metiersDeTest;

        [TestInitialize]
        public void Setup()
        {
            var idGeneratorMock = new Mock<IIdGeneratorService>();
            var mockCheminsService = new Mock<CheminsPrefereService>();
            var mockDataAccess = new Mock<ProjetServiceDataAccess>(mockCheminsService.Object);
            var mockPreferencesService = new Mock<UserPreferencesService>(mockCheminsService.Object, mockDataAccess.Object);

            _mockRessourceService = new Mock<RessourceService>(idGeneratorMock.Object);
            var planningService = new PlanningService(_mockRessourceService.Object);
            var taskManagerServiceMock = new Mock<TaskManagerService>(planningService, idGeneratorMock.Object);
            var csvDataServiceMock = new Mock<CsvDataService>();

            _mockProjetService = new Mock<ProjetService>(idGeneratorMock.Object);
            _mockImportService = new Mock<ImportService>(_mockProjetService.Object, _mockRessourceService.Object, taskManagerServiceMock.Object, idGeneratorMock.Object, csvDataServiceMock.Object);
            _mockValueMappingService = new Mock<ValueMappingService>(mockPreferencesService.Object);

            _orchestrator = new ImportWizardOrchestrator(
                _mockImportService.Object,
                _mockRessourceService.Object,
                _mockProjetService.Object,
                _mockValueMappingService.Object
            );

            _tempFilePath = Path.GetTempFileName();
            _metiersDeTest = new List<Metier> { new Metier { MetierId = "M001", Nom = "Maçon" }, new Metier { MetierId = "M002", Nom = "Plombier" } };
            _mockRessourceService.Setup(s => s.GetAllMetiers()).Returns(_metiersDeTest);
            _mockImportService.Setup(s => s.DetectCsvDelimiter(It.IsAny<string>())).Returns(";");
        }

        [TestCleanup]
        public void Cleanup() { if (File.Exists(_tempFilePath)) { File.Delete(_tempFilePath); } }

        private T PrivateMethod<T>(string methodName, params object[] args)
        {
            var method = typeof(ImportWizardOrchestrator).GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)method.Invoke(_orchestrator, args);
        }

        #region Tests du Constructeur
        [TestMethod]
        [TestCategory("Orchestrator - Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullImportService_ShouldThrowException() => new ImportWizardOrchestrator(null, _mockRessourceService.Object, _mockProjetService.Object, _mockValueMappingService.Object);

        [TestMethod]
        [TestCategory("Orchestrator - Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullRessourceService_ShouldThrowException() => new ImportWizardOrchestrator(_mockImportService.Object, null, _mockProjetService.Object, _mockValueMappingService.Object);

        [TestMethod]
        [TestCategory("Orchestrator - Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullProjetService_ShouldThrowException() => new ImportWizardOrchestrator(_mockImportService.Object, _mockRessourceService.Object, null, _mockValueMappingService.Object);

        [TestMethod]
        [TestCategory("Orchestrator - Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullValueMappingService_ShouldThrowException() => new ImportWizardOrchestrator(_mockImportService.Object, _mockRessourceService.Object, _mockProjetService.Object, null);
        #endregion

        #region Tests Unitaires - Transformation Ouvriers
        [TestMethod]
        [TestCategory("Orchestrator - Transform - Ouvriers")]
        public void TransformerDonnees_Ouvriers_CasSimple_CreeUnOuvrierAvecUneCompetence()
        {
            var csvContent = "Nom;Prenom;Cout;Metier\nDurand;Paul;300;Maçon";
            File.WriteAllText(_tempFilePath, csvContent);
            var p1Result = new ImportP1Result { HasHeader = true, FieldMappings = new List<FieldMappingResult> { new FieldMappingResult { InternalName = "Nom", MappedCsvHeader = "Nom" }, new FieldMappingResult { InternalName = "Prenom", MappedCsvHeader = "Prenom" }, new FieldMappingResult { InternalName = "CoutJournalier", MappedCsvHeader = "Cout" }, new FieldMappingResult { InternalName = "Metier", MappedCsvHeader = "Metier" } } };
            var p2Result = new ImportP2Result { AllMappingDecisions = new List<ValueMappingResult> { new ValueMappingResult { SourceValue = "Maçon", Action = MappingAction.MapToExisting, MappedTargetId = "M001" } } };

            var (ouvriers, rejets) = PrivateMethod<(List<Ouvrier>, List<RejectedRowInfo>)>("TransformerDonnees_Ouvriers", _tempFilePath, p1Result, p2Result);

            Assert.AreEqual(0, rejets.Count);
            Assert.AreEqual(1, ouvriers.Count);
            var ouvrier = ouvriers.First();
            Assert.AreEqual("Durand", ouvrier.Nom);
            Assert.AreEqual(300, ouvrier.CoutJournalier);
            Assert.AreEqual(1, ouvrier.Competences.Count);
            Assert.AreEqual("M001", ouvrier.Competences.First().MetierId);
        }

        [TestMethod]
        [TestCategory("Orchestrator - Transform - Ouvriers")]
        public void TransformerDonnees_Ouvriers_MultiCompetences_AgregeCorrectement()
        {
            var csvContent = "Nom;Prenom;Cout;Metier\nDurand;Paul;300;Maçon\nDurand;Paul;300;Plombier";
            File.WriteAllText(_tempFilePath, csvContent);
            var p1Result = new ImportP1Result { HasHeader = true, FieldMappings = new List<FieldMappingResult> { new FieldMappingResult { InternalName = "Nom", MappedCsvHeader = "Nom" }, new FieldMappingResult { InternalName = "Prenom", MappedCsvHeader = "Prenom" }, new FieldMappingResult { InternalName = "CoutJournalier", MappedCsvHeader = "Cout" }, new FieldMappingResult { InternalName = "Metier", MappedCsvHeader = "Metier" } } };
            var p2Result = new ImportP2Result { AllMappingDecisions = new List<ValueMappingResult> { new ValueMappingResult { SourceValue = "Maçon", Action = MappingAction.MapToExisting, MappedTargetId = "M001" }, new ValueMappingResult { SourceValue = "Plombier", Action = MappingAction.MapToExisting, MappedTargetId = "M002" } } };

            var (ouvriers, rejets) = PrivateMethod<(List<Ouvrier>, List<RejectedRowInfo>)>("TransformerDonnees_Ouvriers", _tempFilePath, p1Result, p2Result);

            Assert.AreEqual(0, rejets.Count);
            Assert.AreEqual(1, ouvriers.Count);
            Assert.AreEqual(2, ouvriers.First().Competences.Count);
        }

        [TestMethod]
        [TestCategory("Orchestrator - Transform - Ouvriers")]
        public void TransformerDonnees_Ouvriers_ValeurParDefaut_Cout_EstUtilisee()
        {
            var csvContent = "Nom;Prenom;Cout;Metier\nDurand;Paul;;Maçon";
            File.WriteAllText(_tempFilePath, csvContent);
            var p1Result = new ImportP1Result { HasHeader = true, FieldMappings = new List<FieldMappingResult> { new FieldMappingResult { InternalName = "Nom", MappedCsvHeader = "Nom" }, new FieldMappingResult { InternalName = "Prenom", MappedCsvHeader = "Prenom" }, new FieldMappingResult { InternalName = "CoutJournalier", MappedCsvHeader = "Cout", DefaultValue = "250" }, new FieldMappingResult { InternalName = "Metier", MappedCsvHeader = "Metier" } } };
            var p2Result = new ImportP2Result { AllMappingDecisions = new List<ValueMappingResult> { new ValueMappingResult { SourceValue = "Maçon", Action = MappingAction.MapToExisting, MappedTargetId = "M001" } } };

            var (ouvriers, rejets) = PrivateMethod<(List<Ouvrier>, List<RejectedRowInfo>)>("TransformerDonnees_Ouvriers", _tempFilePath, p1Result, p2Result);

            Assert.AreEqual(0, rejets.Count);
            Assert.AreEqual(1, ouvriers.Count);
            Assert.AreEqual(250, ouvriers.First().CoutJournalier);
        }

        [TestMethod]
        [TestCategory("Orchestrator - Transform - Ouvriers")]
        public void TransformerDonnees_Ouvriers_MetierNonMappe_CreeRejet()
        {
            var csvContent = "Nom;Prenom;Cout;Metier\nDurand;Paul;300;Electricien";
            File.WriteAllText(_tempFilePath, csvContent);
            var p1Result = new ImportP1Result { HasHeader = true, FieldMappings = new List<FieldMappingResult> { new FieldMappingResult { InternalName = "Nom", MappedCsvHeader = "Nom" }, new FieldMappingResult { InternalName = "Prenom", MappedCsvHeader = "Prenom" }, new FieldMappingResult { InternalName = "CoutJournalier", MappedCsvHeader = "Cout" }, new FieldMappingResult { InternalName = "Metier", MappedCsvHeader = "Metier" } } };
            var p2Result = new ImportP2Result { AllMappingDecisions = new List<ValueMappingResult> { new ValueMappingResult { SourceValue = "Electricien", Action = MappingAction.Ignore } } };

            var (ouvriers, rejets) = PrivateMethod<(List<Ouvrier>, List<RejectedRowInfo>)>("TransformerDonnees_Ouvriers", _tempFilePath, p1Result, p2Result);

            Assert.AreEqual(1, rejets.Count, "Un rejet était attendu.");
            Assert.AreEqual(0, ouvriers.Count, "Aucun ouvrier ne devait être créé.");
            Assert.IsTrue(rejets.First().Reason.Contains("n'a pas été mappée"), $"La raison du rejet était : {rejets.First().Reason}");
        }
        #endregion

        #region Tests Unitaires - Transformation Tâches
        [TestMethod]
        [TestCategory("Orchestrator - Transform - Tâches")]
        public void TransformerDonnees_Taches_CasSimple_CreeUnPlanValide()
        {
            var csvContent = "Titre;Duree;Competence;Zone\nTache A;8;Maçon;Bloc 1";
            File.WriteAllText(_tempFilePath, csvContent);
            var lotCible = new Lot { LotId = "L01" };
            var p1Result = new ImportP1Result { HasHeader = true, FieldMappings = new List<FieldMappingResult> { new FieldMappingResult { InternalName = "TacheNom", MappedCsvHeader = "Titre" }, new FieldMappingResult { InternalName = "HeuresHommeEstimees", MappedCsvHeader = "Duree" }, new FieldMappingResult { InternalName = "MetierId", MappedCsvHeader = "Competence" }, new FieldMappingResult { InternalName = "BlocId", MappedCsvHeader = "Zone" } } };
            var p2Result = new ImportP2Result { AllMappingDecisions = new List<ValueMappingResult> { new ValueMappingResult { SourceValue = "Maçon", Action = MappingAction.MapToExisting, MappedTargetId = "M001" } } };

            var (plan, rejets) = PrivateMethod<(TachesImportPlan, List<RejectedRowInfo>)>("TransformerDonnees_Taches", _tempFilePath, lotCible, p1Result, p2Result);

            Assert.AreEqual(0, rejets.Count);
            Assert.AreEqual(1, plan.NouveauxBlocs.Count);
            Assert.AreEqual(1, plan.NouvellesTaches.Count);
            Assert.AreEqual("Bloc 1", plan.NouveauxBlocs.First().Nom);
            Assert.AreEqual("Tache A", plan.NouvellesTaches.First().TacheNom);
        }

        [TestMethod]
        [TestCategory("Orchestrator - Transform - Tâches")]
        public void TransformerDonnees_Taches_BlocParDefaut_SiChampVide_EstUtilisee()
        {
            var csvContent = "Titre;Duree;Competence;Zone\nTache A;8;Maçon;\nTache B;12;Plombier;Bloc 2";
            File.WriteAllText(_tempFilePath, csvContent);
            var lotCible = new Lot { LotId = "L01" };
            var p1Result = new ImportP1Result { HasHeader = true, FieldMappings = new List<FieldMappingResult> { new FieldMappingResult { InternalName = "TacheNom", MappedCsvHeader = "Titre" }, new FieldMappingResult { InternalName = "HeuresHommeEstimees", MappedCsvHeader = "Duree" }, new FieldMappingResult { InternalName = "MetierId", MappedCsvHeader = "Competence" }, new FieldMappingResult { InternalName = "BlocId", MappedCsvHeader = "Zone", DefaultValue = "Bloc par Defaut" } } };
            var p2Result = new ImportP2Result { AllMappingDecisions = new List<ValueMappingResult> { new ValueMappingResult { SourceValue = "Maçon", Action = MappingAction.MapToExisting, MappedTargetId = "M001" }, new ValueMappingResult { SourceValue = "Plombier", Action = MappingAction.MapToExisting, MappedTargetId = "M002" } } };

            var (plan, rejets) = PrivateMethod<(TachesImportPlan, List<RejectedRowInfo>)>("TransformerDonnees_Taches", _tempFilePath, lotCible, p1Result, p2Result);

            Assert.AreEqual(0, rejets.Count);
            Assert.AreEqual(2, plan.NouveauxBlocs.Count);
            Assert.AreEqual(2, plan.NouvellesTaches.Count);
            var tacheA = plan.NouvellesTaches.First(t => t.TacheNom == "Tache A");
            var blocDefaut = plan.NouveauxBlocs.First(b => b.Nom == "Bloc par Defaut");
            Assert.AreEqual(blocDefaut.BlocId, tacheA.BlocId);
        }
        #endregion

        #region Tests Unitaires - Logique de Mémorisation
        [TestMethod]
        [TestCategory("Orchestrator - Mémorisation")]
        public void MemoriserChoixColonnes_NouveauMapping_AppelleValueMappingService()
        {
            var p1Result = new ImportP1Result { ShouldMemorizeMappings = true, FieldMappings = new List<FieldMappingResult> { new FieldMappingResult { MappedCsvHeader = "lastname", InternalName = "Nom" } } };
            var suggestionsInitiales = new Dictionary<string, string>();

            PrivateMethod<object>("MemoriserChoixColonnes", p1Result, suggestionsInitiales);

            _mockValueMappingService.Verify(s => s.AjouteCorrespondance("lastname", "Nom"), Times.Once);
        }

        [TestMethod]
        [TestCategory("Orchestrator - Mémorisation")]
        public void MemoriserChoixValeurs_ChangementMapping_AppelleSupprimeEtAjoute()
        {
            var p2Result = new ImportP2Result { ShouldMemorizeMappings = true, AllMappingDecisions = new List<ValueMappingResult> { new ValueMappingResult { SourceValue = "Plumber", Action = MappingAction.MapToExisting, MappedTargetId = "M002" } } };
            var suggestionsInitiales = new Dictionary<string, string> { { "Plumber", "M001" } };

            PrivateMethod<object>("MemoriserChoixValeurs", p2Result, suggestionsInitiales);

            _mockValueMappingService.Verify(s => s.SupprimeCorrespondance("Plumber"), Times.Once);
            _mockValueMappingService.Verify(s => s.AjouteCorrespondance("Plumber", "M002"), Times.Once);
        }
        #endregion
    }
}