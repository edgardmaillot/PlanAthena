// Fichier: PlanAthenaTests/Services/DataAccess/ImportServiceTests.cs
// Version: Finale consolidée
// Description: Suite de tests unitaires pour ImportService, enrichie des scénarios de l'ancienne version.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlanAthenaTests.Services.DataAccess
{
    [TestClass]
    public class ImportServiceTests
    {
        private ImportService _importService;
        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private CsvDataService _csvDataService; // Le vrai service sera utilisé pour lire le fichier temporaire
        private IIdGeneratorService _idGenerator;
        private string _tempFilePath;

        [TestInitialize]
        public void Setup()
        {
            _idGenerator = new IdGeneratorService();
            _projetService = new ProjetService(_idGenerator);
            _csvDataService = new CsvDataService(); // Utiliser une vraie instance car le service est simple
            _ressourceService = new RessourceService(_idGenerator, _projetService);
            _importService = new ImportService(_projetService, _ressourceService, _idGenerator, _csvDataService);

            // Créer un fichier temporaire pour chaque test
            _tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csv");

            // Pré-charger des métiers pour que les imports de tâches fonctionnent
            _ressourceService.CreerMetier("Maçonnerie"); // M001
            _ressourceService.CreerMetier("Plomberie");  // M002
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
            // Arrange
            _projetService.InitialiserNouveauProjet(); // Crée "Lot Principal" (L001)
            var lotCibleId = "L001";

            // Le contenu CSV avec des points-virgules, tel qu'attendu par le service
            var csvContent = "ID;Nom;Métier;Heures;Zone\nT1;Tâche Maçonnerie;M001;16;RDC\nT2;Tâche Plomberie;M002;8;RDC\nT3;Autre tâche;M001;24;Etage 1";

            // CORRECTION : On écrit le fichier SANS remplacer les points-virgules
            File.WriteAllText(_tempFilePath, csvContent);

            var mapping = new ImportMappingConfiguration
            {
                HasHeaderRecord = true,
                CsvColumn_IdImporte = "ID",
                CsvColumn_TacheNom = "Nom",
                CsvColumn_MetierId = "Métier",
                CsvColumn_HeuresHommeEstimees = "Heures",
                CsvColumn_BlocId = "Zone"
            };

            // Act
            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            // Assert
            Assert.IsTrue(result.EstSucces, $"L'import a échoué: {result.MessageErreur}");
            Assert.AreEqual(3, result.NbTachesImportees, "Le nombre de tâches importées est incorrect.");
            Assert.AreEqual(2, result.NbBlocsTraites, "Doit créer deux blocs : RDC et Etage 1.");
            Assert.AreEqual(3, _projetService.ObtenirToutesLesTaches().Count);
            Assert.AreEqual(2, _projetService.ObtenirTousLesBlocs().Count);

            var tache1 = _projetService.ObtenirToutesLesTaches().First(t => t.IdImporte == "T1");
            Assert.IsNotNull(tache1);
            Assert.AreEqual("RDC", _projetService.ObtenirBlocParId(tache1.BlocId)?.Nom);
        }

        [TestMethod]
        [TestCategory("Import - Tâches")]
        public void ImporterTachesCSV_AvecEcrasement_VideLeLotAvantImport()
        {
            // Arrange
            _projetService.InitialiserNouveauProjet(); // L001
            var lotCibleId = "L001";
            _projetService.CreerBloc(lotCibleId, "Ancien Bloc"); // L001_B001
            _projetService.CreerTache(lotCibleId, "L001_B001", "Ancienne Tâche");

            var csvContent = "Nom;Métier\nNouvelle Tâche;M001";
            File.WriteAllText(_tempFilePath, csvContent);

            var mapping = new ImportMappingConfiguration { CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier" };

            // Act
            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            // Assert
            Assert.IsTrue(result.EstSucces);
            Assert.AreEqual(1, _projetService.ObtenirToutesLesTaches().Count, "Il ne doit rester que la nouvelle tâche.");
            Assert.AreEqual("Nouvelle Tâche", _projetService.ObtenirToutesLesTaches()[0].TacheNom);
            Assert.AreEqual(1, _projetService.ObtenirTousLesBlocs().Count, "Il ne doit rester que le nouveau bloc par défaut.");
            Assert.AreEqual(mapping.NomBlocParDefaut, _projetService.ObtenirTousLesBlocs()[0].Nom);
        }

        [TestMethod]
        [TestCategory("Import - Tâches")]
        public void ImporterTachesCSV_RemappeCorrectementLesDependancesParNom()
        {
            // Arrange
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";

            var csvContent = "Nom;Métier;Dépendances\nFondations;M001;\nMurs;M001;Fondations";
            File.WriteAllText(_tempFilePath, csvContent);

            var mapping = new ImportMappingConfiguration { CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier", CsvColumn_Dependencies = "Dépendances" };

            // Act
            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            // Assert
            Assert.IsTrue(result.EstSucces);
            var tacheFondations = _projetService.ObtenirToutesLesTaches().Single(t => t.TacheNom == "Fondations");
            var tacheMurs = _projetService.ObtenirToutesLesTaches().Single(t => t.TacheNom == "Murs");
            Assert.AreEqual(tacheFondations.TacheId, tacheMurs.Dependencies, "La dépendance doit être l'ID PlanAthena de la tâche 'Fondations'.");
        }

        [TestMethod]
        [TestCategory("Import - Tâches - Erreurs")]
        public void ImporterTachesCSV_AvecMetierInexistant_IgnoreLaTacheEtEmetUnWarning()
        {
            // Arrange
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";

            var csvContent = "Nom;Métier\nTâche Valide;M001\nTâche Invalide;M999"; // M999 n'existe pas
            File.WriteAllText(_tempFilePath, csvContent);

            var mapping = new ImportMappingConfiguration { CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier" };

            // Act
            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            // Assert
            Assert.IsTrue(result.EstSucces, "L'import doit être considéré comme un succès global.");
            Assert.AreEqual(1, result.NbTachesImportees, "Seule la tâche valide doit être importée.");
            Assert.AreEqual(1, _projetService.ObtenirToutesLesTaches().Count);
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("M999") && w.Contains("invalide ou manquant")), "Un avertissement pour le métier manquant doit être présent.");
        }

        [TestMethod]
        [TestCategory("Import - Tâches - Erreurs")]
        public void ImporterTachesCSV_AvecNomDeTacheManquant_IgnoreLaLigneEtEmetUnWarning()
        {
            // Arrange
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";

            var csvContent = "Nom;Métier\nTâche Valide;M001\n;M002"; // Nom de tâche vide sur la deuxième ligne
            File.WriteAllText(_tempFilePath, csvContent);

            var mapping = new ImportMappingConfiguration { CsvColumn_TacheNom = "Nom", CsvColumn_MetierId = "Métier" };

            // Act
            var result = _importService.ImporterTachesCSV(_tempFilePath, lotCibleId, mapping, true);

            // Assert
            Assert.IsTrue(result.EstSucces);
            Assert.AreEqual(1, result.NbTachesImportees);
            Assert.AreEqual(1, _projetService.ObtenirToutesLesTaches().Count);
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("Nom de tâche manquant")), "Un avertissement pour le nom manquant doit être présent.");
        }

        [TestMethod]
        [TestCategory("Import - Tâches - Erreurs")]
        public void ImporterTachesCSV_FichierInexistant_RetourneResultatEchec()
        {
            // Arrange
            _projetService.InitialiserNouveauProjet();
            var lotCibleId = "L001";
            var fichierInexistant = "C:\\chemin\\inexistant\\fichier.csv";

            // Act
            var result = _importService.ImporterTachesCSV(fichierInexistant, lotCibleId, new ImportMappingConfiguration(), true);

            // Assert
            Assert.IsFalse(result.EstSucces);
            Assert.IsTrue(result.MessageErreur.Contains("Erreur lors de l'import"), "Le message d'erreur doit être clair.");
        }

        #endregion
    }
}