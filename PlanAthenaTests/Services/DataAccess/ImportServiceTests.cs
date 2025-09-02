// Emplacement: PlanAthenaTests/Services/DataAccess/ImportServiceTests.cs

using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;

namespace PlanAthenaTests.Services.DataAccess
{
    [TestClass]
    public class ImportServiceTests
    {
        private ImportService _importService;
        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private TaskManagerService _taskManagerService; // NOUVEAU
        private CsvDataService _csvDataService;
        private IIdGeneratorService _idGenerator;
        private string _tempFilePath;

        [TestInitialize]
        public void Setup()
        {
            _idGenerator = new IdGeneratorService();
            _csvDataService = new CsvDataService();

            // Instanciation des services avec leurs nouvelles dépendances
            var mockPlanningService = new Moq.Mock<PlanningService>(new RessourceService(_idGenerator));
            _taskManagerService = new TaskManagerService(mockPlanningService.Object, _idGenerator);
            _projetService = new ProjetService(_idGenerator);
            _ressourceService = new RessourceService(_idGenerator);

            _importService = new ImportService(_projetService, _ressourceService, _taskManagerService, _idGenerator, _csvDataService);

            _tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csv");

            _ressourceService.CreerMetier("Maçonnerie"); // M001
            _ressourceService.CreerMetier("Plomberie");  // M002
            _ressourceService.CreerMetier("Electricité"); // M003
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        #region Tests Import Tâches

        [TestMethod]
        [TestCategory("Import - Tâches")]
        public void ImporterTachesCSV_ScenarioNominal_ImporteTachesEtCreeBlocs()
        {
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";
            var csvContent = "ID;Nom;Métier;Heures;Zone\nT1;Tâche Maçonnerie;M001;16;RDC\nT2;Tâche Plomberie;M002;8;RDC\nT3;Autre tâche;M001;24;Etage 1";
            File.WriteAllText(_tempFilePath, csvContent);
            var mapping = new ImportMappingConfiguration { HasHeaderRecord = true, CsvColumn_IdImporte = "ID", CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier", CsvColumn_HeuresHommeEstimees = "Heures", CsvColumn_BlocId = "Zone" };

            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            Assert.IsTrue(result.EstSucces, $"L'import a échoué: {result.MessageErreur}");
            Assert.AreEqual(3, result.NbTachesImportees);
            Assert.AreEqual(2, result.NbBlocsTraites);
            Assert.AreEqual(3, _taskManagerService.ObtenirToutesLesTaches().Count);
            Assert.AreEqual(2, _projetService.ObtenirTousLesBlocs().Count);

            var tache1 = _taskManagerService.ObtenirToutesLesTaches().First(t => t.IdImporte == "T1");
            Assert.IsNotNull(tache1);
            Assert.AreEqual("RDC", _projetService.ObtenirBlocParId(tache1.BlocId)?.Nom);
        }

        [TestMethod]
        [TestCategory("Import - Tâches")]
        public void ImporterTachesCSV_AvecEcrasement_VideLeLotAvantImport()
        {
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";
            _projetService.CreerBloc(lotCibleId, "Ancien Bloc");
            _taskManagerService.CreerTache(lotCibleId, "L001_B001", "Ancienne Tâche", 8);
            var csvContent = "Nom;Métier\nNouvelle Tâche;M001";
            File.WriteAllText(_tempFilePath, csvContent);
            var mapping = new ImportMappingConfiguration { CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier" };

            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            Assert.IsTrue(result.EstSucces);
            Assert.AreEqual(1, _taskManagerService.ObtenirToutesLesTaches().Count);
            Assert.AreEqual("Nouvelle Tâche", _taskManagerService.ObtenirToutesLesTaches()[0].TacheNom);
            Assert.AreEqual(1, _projetService.ObtenirTousLesBlocs().Count);
            Assert.AreEqual(mapping.NomBlocParDefaut, _projetService.ObtenirTousLesBlocs()[0].Nom);
        }

        [TestMethod]
        [TestCategory("Import - Tâches")]
        public void ImporterTachesCSV_RemappeCorrectementLesDependancesParNom()
        {
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";
            var csvContent = "Nom;Métier;Dépendances\nFondations;M001;\nMurs;M001;Fondations";
            File.WriteAllText(_tempFilePath, csvContent);
            var mapping = new ImportMappingConfiguration { CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier", CsvColumn_Dependencies = "Dépendances" };

            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            Assert.IsTrue(result.EstSucces);
            var tacheFondations = _taskManagerService.ObtenirToutesLesTaches().Single(t => t.TacheNom == "Fondations");
            var tacheMurs = _taskManagerService.ObtenirToutesLesTaches().Single(t => t.TacheNom == "Murs");
            Assert.AreEqual(tacheFondations.TacheId, tacheMurs.Dependencies);
        }

        [TestMethod]
        [TestCategory("Import - Tâches - Erreurs")]
        public void ImporterTachesCSV_AvecMetierInexistant_IgnoreLaTacheEtEmetUnWarning()
        {
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";
            var csvContent = "Nom;Métier\nTâche Valide;M001\nTâche Invalide;M999";
            File.WriteAllText(_tempFilePath, csvContent);
            var mapping = new ImportMappingConfiguration { CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier" };

            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            Assert.IsTrue(result.EstSucces);
            Assert.AreEqual(1, result.NbTachesImportees);
            Assert.AreEqual(1, _taskManagerService.ObtenirToutesLesTaches().Count);
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("M999") && w.Contains("invalide ou manquant")));
        }

        [TestMethod]
        [TestCategory("Import - Tâches - Erreurs")]
        public void ImporterTachesCSV_AvecNomDeTacheManquant_IgnoreLaLigneEtEmetUnWarning()
        {
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";
            var csvContent = "Nom;Métier\nTâche Valide;M001\n;M002";
            File.WriteAllText(_tempFilePath, csvContent);
            var mapping = new ImportMappingConfiguration { CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier" };

            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            Assert.IsTrue(result.EstSucces);
            Assert.AreEqual(1, result.NbTachesImportees);
            Assert.AreEqual(1, _taskManagerService.ObtenirToutesLesTaches().Count);
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("Nom de tâche manquant")));
        }

        [TestMethod]
        [TestCategory("Import - Tâches - Erreurs")]
        public void ImporterTachesCSV_FichierInexistant_RetourneResultatEchec()
        {
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";
            var fichierInexistant = "C:\\chemin\\inexistant\\fichier.csv";

            var result = _importService.ImporterTachesCSV(fichierInexistant, lotCibleId, new ImportMappingConfiguration(), true);

            Assert.IsFalse(result.EstSucces);
            Assert.IsTrue(result.MessageErreur.Contains("Erreur lors de l'import"));
        }

        #endregion

        #region Tests Import/Export Ouvriers

        [TestMethod]
        [TestCategory("Import - Ouvriers")]
        public void ImporterOuvriersCSV_AvecCompetencesMultiples_ReconstruitCorrectementLesOuvriers()
        {
            var csvContent = "OuvrierId;Nom;Prenom;CoutJournalier;MetierId\nOuv001;Durand;Paul;300;M001\nOuv001;Durand;Paul;300;M002\nOuv002;Martin;Marie;320;M003";
            File.WriteAllText(_tempFilePath, csvContent);

            int count = _importService.ImporterOuvriersCSV(_tempFilePath, true);

            Assert.AreEqual(2, count);
            var allOuvriers = _ressourceService.GetAllOuvriers();
            Assert.AreEqual(2, allOuvriers.Count);
            var paul = allOuvriers.FirstOrDefault(o => o.Nom == "Durand" && o.Prenom == "Paul");
            Assert.IsNotNull(paul);
            Assert.AreEqual(2, paul.Competences.Count);
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M001"));
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M002"));
        }

        [TestMethod]
        [TestCategory("Import - Ouvriers")]
        public void ImporterOuvriersCSV_ModeNonRemplacement_AjouteEtMetAJourLesOuvriers()
        {
            var ouvrierExistant = _ressourceService.CreerOuvrier("Paul", "Durand", 280);
            _ressourceService.AjouterCompetence(ouvrierExistant.OuvrierId, "M001");
            var csvContent = $"OuvrierId;Nom;Prenom;CoutJournalier;MetierId\n{ouvrierExistant.OuvrierId};Durand;Paul;290;M002\nOuv002;Martin;Marie;320;M003";
            File.WriteAllText(_tempFilePath, csvContent);

            _importService.ImporterOuvriersCSV(_tempFilePath, false);

            var allOuvriers = _ressourceService.GetAllOuvriers();
            Assert.AreEqual(2, allOuvriers.Count);
            var paul = _ressourceService.GetOuvrierById(ouvrierExistant.OuvrierId);
            Assert.IsNotNull(paul);
            Assert.AreEqual(290, paul.CoutJournalier);
            Assert.AreEqual(2, paul.Competences.Count);
        }

        [TestMethod]
        [TestCategory("Export - Ouvriers")]
        public void ExporterOuvriersCSV_AvecCompetencesMultiples_CreeUneLigneParCompetence()
        {
            var ouvrier1 = _ressourceService.CreerOuvrier("Paul", "Durand", 300);
            _ressourceService.AjouterCompetence(ouvrier1.OuvrierId, "M001");
            _ressourceService.AjouterCompetence(ouvrier1.OuvrierId, "M002");
            var ouvrier2 = _ressourceService.CreerOuvrier("Marie", "Martin", 320);
            _ressourceService.AjouterCompetence(ouvrier2.OuvrierId, "M003");

            _importService.ExporterOuvriersCSV(_tempFilePath);

            var lignesCsv = File.ReadAllLines(_tempFilePath);
            Assert.AreEqual(4, lignesCsv.Length);
            var countPaul = lignesCsv.Count(l => l.Contains(ouvrier1.OuvrierId));
            Assert.AreEqual(2, countPaul);
            Assert.IsTrue(lignesCsv.Any(l => l.Contains("M001")));
            Assert.IsTrue(lignesCsv.Any(l => l.Contains("M002")));
        }

        #endregion
    }
}