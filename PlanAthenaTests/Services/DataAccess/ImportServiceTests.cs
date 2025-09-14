// Emplacement: PlanAthenaTests/Services/DataAccess/ImportServiceTests.cs

using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.View.Utils;
using PlanAthena.Data;
using System.Text;

namespace PlanAthenaTests.Services.DataAccess
{
    [TestClass]
    public class ImportServiceTests
    {
        private ImportService _importService;
        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private TaskManagerService _taskManagerService;
        private CsvDataService _csvDataService;
        private IIdGeneratorService _idGenerator;
        private string _tempFilePath;

        [TestInitialize]
        public void Setup()
        {
            _idGenerator = new IdGeneratorService();
            _csvDataService = new CsvDataService();

            // Instanciation des services avec leurs nouvelles dépendances
            _projetService = new ProjetService(_idGenerator);
            _ressourceService = new RessourceService(_idGenerator);
            var planningService = new PlanningService(_ressourceService);
            _taskManagerService = new TaskManagerService(planningService, _idGenerator);

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
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);
            var mapping = new ImportMappingConfiguration
            {
                HasHeaderRecord = true,
                CsvColumn_IdImporte = "ID",
                CsvColumn_TacheNom = "Nom",
                CsvColumn_MetierId = "Métier",
                CsvColumn_HeuresHommeEstimees = "Heures",
                CsvColumn_BlocId = "Zone"
            };

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
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);
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
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);
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
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);
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
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);
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

        #region Tests Import Ouvriers - Nouvelle Architecture

        [TestMethod]
        [TestCategory("Import - Ouvriers")]
        public void ImporterOuvriers_AvecListeOuvriersValides_ImporteCorrectement()
        {
            // Arrange - Préparer une liste d'ouvriers directement
            var ouvriersAImporter = new List<Ouvrier>
            {
                new Ouvrier { Nom = "Durand", Prenom = "Paul", CoutJournalier = 300 },
                new Ouvrier { Nom = "Martin", Prenom = "Marie", CoutJournalier = 320 }
            };

            // Ajouter des compétences
            ouvriersAImporter[0].Competences.Add(new CompetenceOuvrier { MetierId = "M001", EstMetierPrincipal = true });
            ouvriersAImporter[0].Competences.Add(new CompetenceOuvrier { MetierId = "M002", EstMetierPrincipal = false });
            ouvriersAImporter[1].Competences.Add(new CompetenceOuvrier { MetierId = "M003", EstMetierPrincipal = true });

            // Act
            var result = _importService.ImporterOuvriers(ouvriersAImporter, remplacerExistants: true);

            // Assert
            Assert.IsTrue(result.EstSucces, $"L'import a échoué: {result.MessageErreur}");
            Assert.AreEqual(2, result.NbOuvriersImportes);

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
        public void ImporterOuvriers_ModeNonRemplacement_AjouteEtMetAJourLesOuvriers()
        {
            // Arrange
            var ouvrierExistant = _ressourceService.CreerOuvrier("Paul", "Durand", 280);
            _ressourceService.AjouterCompetence(ouvrierExistant.OuvrierId, "M001");

            var ouvriersAImporter = new List<Ouvrier>
            {
                new Ouvrier { Nom = "Durand", Prenom = "Paul", CoutJournalier = 290 },
                new Ouvrier { Nom = "Martin", Prenom = "Marie", CoutJournalier = 320 }
            };

            // Ajouter compétence pour Paul
            ouvriersAImporter[0].Competences.Add(new CompetenceOuvrier { MetierId = "M002", EstMetierPrincipal = false });
            // Compétence pour Marie
            ouvriersAImporter[1].Competences.Add(new CompetenceOuvrier { MetierId = "M003", EstMetierPrincipal = true });

            // Act
            var result = _importService.ImporterOuvriers(ouvriersAImporter, remplacerExistants: false);

            // Assert
            Assert.IsTrue(result.EstSucces);
            var allOuvriers = _ressourceService.GetAllOuvriers();
            Assert.AreEqual(2, allOuvriers.Count);

            var paul = allOuvriers.FirstOrDefault(o => o.Nom == "Durand" && o.Prenom == "Paul");
            Assert.IsNotNull(paul);
            Assert.AreEqual(290, paul.CoutJournalier); // Coût mis à jour
            Assert.AreEqual(2, paul.Competences.Count); // M001 existant + M002 nouveau
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M001"));
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M002"));
        }

        [TestMethod]
        [TestCategory("Import - Ouvriers")]
        public void ImporterOuvriers_AvecRemplacement_VideLesOuvriersExistants()
        {
            // Arrange
            var ouvrierExistant = _ressourceService.CreerOuvrier("Jean", "Dupont", 250);
            _ressourceService.AjouterCompetence(ouvrierExistant.OuvrierId, "M003");

            var ouvriersAImporter = new List<Ouvrier>
            {
                new Ouvrier { Nom = "Durand", Prenom = "Paul", CoutJournalier = 300 }
            };
            ouvriersAImporter[0].Competences.Add(new CompetenceOuvrier { MetierId = "M001", EstMetierPrincipal = true });

            // Act
            var result = _importService.ImporterOuvriers(ouvriersAImporter, remplacerExistants: true);

            // Assert
            Assert.IsTrue(result.EstSucces);
            var allOuvriers = _ressourceService.GetAllOuvriers();
            Assert.AreEqual(1, allOuvriers.Count);

            var ouvrier = allOuvriers.First();
            Assert.AreEqual("Paul", ouvrier.Prenom);
            Assert.AreEqual("Durand", ouvrier.Nom);
        }

        [TestMethod]
        [TestCategory("Import - Ouvriers")]
        public void ImporterOuvriers_AvecOuvrierSansCompetence_CreeOuvrierSansMetier()
        {
            // Arrange
            var ouvriersAImporter = new List<Ouvrier>
            {
                new Ouvrier { Nom = "Durand", Prenom = "Paul", CoutJournalier = 300 }, // Sans compétences
                new Ouvrier { Nom = "Martin", Prenom = "Marie", CoutJournalier = 320 }
            };
            ouvriersAImporter[1].Competences.Add(new CompetenceOuvrier { MetierId = "M001", EstMetierPrincipal = true });

            // Act
            var result = _importService.ImporterOuvriers(ouvriersAImporter, remplacerExistants: true);

            // Assert
            Assert.IsTrue(result.EstSucces);
            var allOuvriers = _ressourceService.GetAllOuvriers();
            Assert.AreEqual(2, allOuvriers.Count);

            var paul = allOuvriers.FirstOrDefault(o => o.Prenom == "Paul");
            Assert.IsNotNull(paul);
            Assert.AreEqual(0, paul.Competences.Count);

            var marie = allOuvriers.FirstOrDefault(o => o.Prenom == "Marie");
            Assert.IsNotNull(marie);
            Assert.AreEqual(1, marie.Competences.Count);
        }

        #endregion

        #region Tests Import Format Natif

        [TestMethod]
        [TestCategory("Import - Format Natif")]
        public void ImporterOuvriersFormatNatif_AvecFichierValide_ImporteCorrectement()
        {
            // Arrange - Créer un fichier au format natif PlanAthena
            var csvContent = "OuvrierId;Nom;Prenom;CoutJournalier;MetierId\n" +
                            "Ouv001;Durand;Paul;300;M001\n" +
                            "Ouv001;Durand;Paul;300;M002\n" +
                            "Ouv002;Martin;Marie;320;M003";
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);

            // Act
            var result = _importService.ImporterOuvriersFormatNatif(_tempFilePath, remplacerExistants: true);

            // Assert
            Assert.IsTrue(result.EstSucces, $"L'import a échoué: {result.MessageErreur}");
            Assert.AreEqual(2, result.NbOuvriersImportes);

            var allOuvriers = _ressourceService.GetAllOuvriers();
            Assert.AreEqual(2, allOuvriers.Count);

            var paul = allOuvriers.FirstOrDefault(o => o.Nom == "Durand" && o.Prenom == "Paul");
            Assert.IsNotNull(paul);
            Assert.AreEqual(2, paul.Competences.Count);
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M001"));
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M002"));
        }

        [TestMethod]
        [TestCategory("Import - Format Natif")]
        public void ImporterOuvriersFormatNatif_AvecMetierInexistant_EmetWarning()
        {
            // Arrange
            var csvContent = "OuvrierId;Nom;Prenom;CoutJournalier;MetierId\n" +
                            "Ouv001;Durand;Paul;300;M001\n" +
                            "Ouv001;Durand;Paul;300;M999"; // Métier inexistant
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);

            // Act
            var result = _importService.ImporterOuvriersFormatNatif(_tempFilePath, remplacerExistants: true);

            // Assert
            Assert.IsTrue(result.EstSucces);
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("M999") && w.Contains("n'existe pas")));

            var allOuvriers = _ressourceService.GetAllOuvriers();
            var paul = allOuvriers.FirstOrDefault(o => o.Prenom == "Paul");
            Assert.IsNotNull(paul);
            Assert.AreEqual(1, paul.Competences.Count); // Seulement M001, pas M999
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M001"));
        }

        [TestMethod]
        [TestCategory("Import - Format Natif - Erreurs")]
        public void ImporterOuvriersFormatNatif_FichierInexistant_RetourneResultatEchec()
        {
            // Arrange
            var fichierInexistant = "C:\\chemin\\inexistant\\fichier.csv";

            // Act
            var result = _importService.ImporterOuvriersFormatNatif(fichierInexistant, remplacerExistants: true);

            // Assert
            Assert.IsFalse(result.EstSucces);
            Assert.IsTrue(result.MessageErreur.Contains("Erreur lors de l'import natif"));
        }

        #endregion

        #region Tests Utilitaires

        [TestMethod]
        [TestCategory("Import - Utilitaires")]
        public void DetectCsvDelimiter_AvecPointVirgule_RetournePointVirgule()
        {
            // Arrange
            var csvContent = "Nom;Prenom;Age\nDurand;Paul;30";
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);

            // Act
            var delimiter = _importService.DetectCsvDelimiter(_tempFilePath);

            // Assert
            Assert.AreEqual(";", delimiter);
        }

        [TestMethod]
        [TestCategory("Import - Utilitaires")]
        public void DetectCsvDelimiter_AvecVirgule_RetourneVirgule()
        {
            // Arrange
            var csvContent = "Nom,Prenom,Age\nDurand,Paul,30";
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);

            // Act
            var delimiter = _importService.DetectCsvDelimiter(_tempFilePath);

            // Assert
            Assert.AreEqual(",", delimiter);
        }

        [TestMethod]
        [TestCategory("Import - Utilitaires")]
        public void DetectCsvDelimiter_AvecTabulation_RetourneTabulation()
        {
            // Arrange
            var csvContent = "Nom\tPrenom\tAge\nDurand\tPaul\t30";
            File.WriteAllText(_tempFilePath, csvContent, Encoding.UTF8);

            // Act
            var delimiter = _importService.DetectCsvDelimiter(_tempFilePath);

            // Assert
            Assert.AreEqual("\t", delimiter);
        }

        [TestMethod]
        [TestCategory("Import - Utilitaires")]
        public void DetectCsvDelimiter_FichierVide_RetournePointVirguleParDefaut()
        {
            // Arrange
            File.WriteAllText(_tempFilePath, "", Encoding.UTF8);

            // Act
            var delimiter = _importService.DetectCsvDelimiter(_tempFilePath);

            // Assert
            Assert.AreEqual(";", delimiter); // Valeur par défaut
        }

        #endregion
    }
}