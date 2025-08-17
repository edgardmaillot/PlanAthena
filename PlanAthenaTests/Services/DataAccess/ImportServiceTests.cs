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
            _ressourceService = new RessourceService(_idGenerator);
            _importService = new ImportService(_projetService, _ressourceService, _idGenerator, _csvDataService);

            // Créer un fichier temporaire pour chaque test
            _tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csv");

            // Pré-charger des métiers pour que les imports de tâches fonctionnent
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

        #region Tests Import/Export Ouvriers (NOUVEAU)

        [TestMethod]
        [TestCategory("Import - Ouvriers")]
        public void ImporterOuvriersCSV_AvecCompetencesMultiples_ReconstruitCorrectementLesOuvriers()
        {
            // Arrange
            var csvContent = "OuvrierId;Nom;Prenom;CoutJournalier;MetierId\n" +
                             "Ouv001;Durand;Paul;300;M001\n" +
                             "Ouv001;Durand;Paul;300;M002\n" +
                             "Ouv002;Martin;Marie;320;M003";
            File.WriteAllText(_tempFilePath, csvContent);

            // Act
            int count = _importService.ImporterOuvriersCSV(_tempFilePath, true);

            // Assert
            Assert.AreEqual(2, count, "Doit rapporter 2 ouvriers uniques traités.");

            var allOuvriers = _ressourceService.GetAllOuvriers();
            Assert.AreEqual(2, allOuvriers.Count, "Il doit y avoir 2 ouvriers uniques dans le service.");

            // CORRECTION : On cherche l'ouvrier par son nom et prénom, pas par son ID externe.
            var paul = allOuvriers.FirstOrDefault(o => o.Nom == "Durand" && o.Prenom == "Paul");
            Assert.IsNotNull(paul, "L'ouvrier Paul Durand n'a pas été trouvé après l'import."); // L'assert passe maintenant

            // Le reste des assertions peut continuer
            Assert.AreEqual(2, paul.Competences.Count, "Paul Durand doit avoir 2 compétences.");
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M001"));
            Assert.IsTrue(paul.Competences.Any(c => c.MetierId == "M002"));

            var marie = allOuvriers.FirstOrDefault(o => o.Nom == "Martin" && o.Prenom == "Marie");
            Assert.IsNotNull(marie, "L'ouvrier Marie Martin n'a pas été trouvée après l'import.");
            Assert.AreEqual(1, marie.Competences.Count, "Marie Martin doit avoir 1 compétence.");
        }

        [TestMethod]
        [TestCategory("Import - Ouvriers")]
        public void ImporterOuvriersCSV_ModeNonRemplacement_AjouteEtMetAJourLesOuvriers()
        {
            // Arrange
            // 1. Pré-charger un ouvrier existant
            var ouvrierExistant = _ressourceService.CreerOuvrier("Paul", "Durand", 280); // ID: W001
            _ressourceService.AjouterCompetence(ouvrierExistant.OuvrierId, "M001");

            // 2. Préparer un CSV qui met à jour Paul et ajoute un nouvel ouvrier
            var csvContent = $"OuvrierId;Nom;Prenom;CoutJournalier;MetierId\n" +
                             $"{ouvrierExistant.OuvrierId};Durand;Paul;290;M002\n" + // Met à jour le coût de Paul et ajoute M002
                             $"Ouv002;Martin;Marie;320;M003";
            File.WriteAllText(_tempFilePath, csvContent);

            // Act
            _importService.ImporterOuvriersCSV(_tempFilePath, false); // NE PAS remplacer

            // Assert
            var allOuvriers = _ressourceService.GetAllOuvriers();
            Assert.AreEqual(2, allOuvriers.Count, "Il doit y avoir 2 ouvriers au total.");

            var paul = _ressourceService.GetOuvrierById(ouvrierExistant.OuvrierId);
            Assert.IsNotNull(paul);
            Assert.AreEqual(290, paul.CoutJournalier, "Le coût journalier de Paul doit être mis à jour.");
            Assert.AreEqual(2, paul.Competences.Count, "Paul doit maintenant avoir 2 compétences (M001 et M002).");
        }

        [TestMethod]
        [TestCategory("Export - Ouvriers")]
        public void ExporterOuvriersCSV_AvecCompetencesMultiples_CreeUneLigneParCompetence()
        {
            // Arrange
            var ouvrier1 = _ressourceService.CreerOuvrier("Paul", "Durand", 300); // W001
            _ressourceService.AjouterCompetence(ouvrier1.OuvrierId, "M001");
            _ressourceService.AjouterCompetence(ouvrier1.OuvrierId, "M002");
            var ouvrier2 = _ressourceService.CreerOuvrier("Marie", "Martin", 320); // W002
            _ressourceService.AjouterCompetence(ouvrier2.OuvrierId, "M003");

            // Act
            _importService.ExporterOuvriersCSV(_tempFilePath);

            // Assert
            var lignesCsv = File.ReadAllLines(_tempFilePath);
            // 1 ligne d'en-tête + 2 lignes pour Paul + 1 ligne pour Marie = 4 lignes
            Assert.AreEqual(4, lignesCsv.Length, "Le fichier CSV doit contenir 4 lignes au total.");

            // Vérifie que l'ID de Paul apparaît bien 2 fois
            var countPaul = lignesCsv.Count(l => l.Contains(ouvrier1.OuvrierId));
            Assert.AreEqual(2, countPaul, "L'ID de Paul Durand doit apparaître sur 2 lignes.");

            // Vérifie que ses deux métiers sont bien présents
            Assert.IsTrue(lignesCsv.Any(l => l.Contains("M001")));
            Assert.IsTrue(lignesCsv.Any(l => l.Contains("M002")));
        }

        #endregion
    }
}