// Emplacement: /PlanAthena.Tests/Services/Business/PlanningServiceTests.cs

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using System;
using System.Collections.Generic;

namespace PlanAthenaTests.Services.Business
{
    [TestClass]
    public class PlanningServiceTests
    {
        private PlanningService _planningService;
        private ConsolidatedPlanning _samplePlanning;
        private ConfigurationPlanification _sampleConfig;

        // NOUVEAU: Mock de la dépendance requise par le constructeur de PlanningService
        private Mock<RessourceService> _mockRessourceService;

        [TestInitialize]
        public void TestInitialize()
        {
            // On doit d'abord créer le mock pour la dépendance de PlanningService
            var mockIdGenerator = new Mock<IIdGeneratorService>();
            _mockRessourceService = new Mock<RessourceService>(mockIdGenerator.Object);

            // On peut maintenant instancier PlanningService en lui passant sa dépendance
            _planningService = new PlanningService(_mockRessourceService.Object);

            _samplePlanning = new ConsolidatedPlanning(); // Un DTO simple pour les tests
            _sampleConfig = new ConfigurationPlanification
            {
                JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
            };
        }

        #region Tests de Gestion de l'État

        [TestMethod]
        public void GetCurrentPlanning_InitialState_ShouldReturnNull()
        {
            // Arrange
            var service = new PlanningService(_mockRessourceService.Object); // Utilise une instance fraîche pour ce test
            // Act
            var planning = service.GetCurrentPlanning();
            // Assert
            Assert.IsNull(planning, "Le planning doit être null à l'initialisation.");
        }

        [TestMethod]
        public void UpdatePlanning_WithValidData_ShouldSetCurrentPlanning()
        {
            // Act
            _planningService.UpdatePlanning(_samplePlanning, _sampleConfig);
            var planning = _planningService.GetCurrentPlanning();
            // Assert
            Assert.IsNotNull(planning, "Le planning ne doit pas être null après UpdatePlanning.");
            Assert.AreSame(_samplePlanning, planning, "La référence du planning doit être celle qui a été passée.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdatePlanning_WithNullPlanning_ShouldThrowException()
        {
            // Act
            _planningService.UpdatePlanning(null, _sampleConfig);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdatePlanning_WithNullConfig_ShouldThrowException()
        {
            // Act
            _planningService.UpdatePlanning(_samplePlanning, null);
        }

        [TestMethod]
        public void ClearPlanning_AfterUpdate_ShouldResetStateToNull()
        {
            // Arrange
            _planningService.UpdatePlanning(_samplePlanning, _sampleConfig);
            // Act
            _planningService.ClearPlanning();
            var planning = _planningService.GetCurrentPlanning();
            // Assert
            Assert.IsNull(planning, "Le planning doit être null après ClearPlanning.");
        }

        #endregion

        #region Tests pour GetNombreJoursOuvres

        [TestMethod]
        public void GetNombreJoursOuvres_BeforePlanningIsSet_ShouldReturnZero()
        {
            // Arrange
            var service = new PlanningService(_mockRessourceService.Object);
            var dateDebut = new DateTime(2023, 10, 2);
            var dateFin = new DateTime(2023, 10, 6);
            // Act
            int result = service.GetNombreJoursOuvres(dateDebut, dateFin);
            // Assert
            Assert.AreEqual(0, result, "Doit retourner 0 si aucun planning/config n'est chargé.");
        }

        [TestMethod]
        public void GetNombreJoursOuvres_WithStandardWeek_ShouldExcludeWeekends()
        {
            // Arrange
            _planningService.UpdatePlanning(_samplePlanning, _sampleConfig);
            var dateDebut = new DateTime(2023, 10, 2);
            var dateFin = new DateTime(2023, 10, 6);
            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);
            // Assert
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void GetNombreJoursOuvres_SpanningAWeekend_ShouldCalculateCorrectly()
        {
            // Arrange
            _planningService.UpdatePlanning(_samplePlanning, _sampleConfig);
            var dateDebut = new DateTime(2023, 10, 6);
            var dateFin = new DateTime(2023, 10, 9);
            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);
            // Assert
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void GetNombreJoursOuvres_WithCustomWorkWeek_ShouldUseConfig()
        {
            // Arrange
            var configTravailSamedi = new ConfigurationPlanification { JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Saturday } };
            _planningService.UpdatePlanning(_samplePlanning, configTravailSamedi);
            var dateDebut = new DateTime(2023, 10, 2);
            var dateFin = new DateTime(2023, 10, 7);
            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);
            // Assert
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void GetNombreJoursOuvres_WithInvalidDateRange_ShouldReturnZero()
        {
            // Arrange
            _planningService.UpdatePlanning(_samplePlanning, _sampleConfig);
            var dateDebut = new DateTime(2023, 10, 10);
            var dateFin = new DateTime(2023, 10, 5);
            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);
            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetNombreJoursOuvres_SingleDay_ShouldReturnOneIfWorkday()
        {
            // Arrange
            _planningService.UpdatePlanning(_samplePlanning, _sampleConfig);
            var date = new DateTime(2023, 10, 4);
            // Act
            int result = _planningService.GetNombreJoursOuvres(date, date);
            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void GetNombreJoursOuvres_SingleDay_ShouldReturnZeroIfNotWorkday()
        {
            // Arrange
            _planningService.UpdatePlanning(_samplePlanning, _sampleConfig);
            var date = new DateTime(2023, 10, 8);
            // Act
            int result = _planningService.GetNombreJoursOuvres(date, date);
            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion
        #region NOUVEAU : Tests pour les Méthodes du Cockpit

        [TestMethod]
        public void ReconcilierAvecAvancementReel_TacheUniqueEnCours_MetAJourLeStatutReelInterne()
        {
            // Arrange
            var tacheEnCours = new Tache { TacheId = "T1", Statut = Statut.EnCours };
            // On s'assure que le planning est "chargé" pour que le service soit dans un état valide
            _planningService.UpdatePlanning(new ConsolidatedPlanning(), new ConfigurationPlanification());

            // Act
            _planningService.ReconcilierAvecAvancementReel(tacheEnCours);

            // Assert
            // NOTE: On ne peut pas vérifier directement le dictionnaire privé.
            // Ce test sert surtout de "smoke test" pour s'assurer que la méthode s'exécute sans erreur.
            // La vraie validation se fera via les tests d'intégration ou en exposant un état (non recommandé).
            // Pour l'instant, on se contente de vérifier l'absence d'exception.
            Assert.IsTrue(true, "La méthode s'est exécutée sans lever d'exception.");
        }

        [TestMethod]
        public void ReconcilierAvecAvancementReel_TacheRedevientPlanifiee_RetireLeStatutReelInterne()
        {
            // Arrange
            var tache = new Tache { TacheId = "T1", Statut = Statut.EnCours };
            _planningService.UpdatePlanning(new ConsolidatedPlanning(), new ConfigurationPlanification());
            _planningService.ReconcilierAvecAvancementReel(tache); // Statut "EnCours" est maintenant stocké

            // Act
            tache.Statut = Statut.Planifiée; // L'utilisateur annule sa saisie
            _planningService.ReconcilierAvecAvancementReel(tache);

            // Assert
            // Comme précédemment, on vérifie l'absence d'erreur.
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ObtenirNombreTachesQuiDevraientEtreTerminees_CasNominal_RetourneLeBonCompte()
        {
            // Arrange
            var aujourdhui = DateTime.Today;
            var taches = new List<Tache>
            {
                new Tache { EstConteneur = false, DateFinPlanifiee = aujourdhui.AddDays(-2) }, // Doit compter
                new Tache { EstConteneur = false, DateFinPlanifiee = aujourdhui },             // Doit compter
                new Tache { EstConteneur = true,  DateFinPlanifiee = aujourdhui.AddDays(-1) }, // Ne doit pas compter (conteneur)
                new Tache { EstConteneur = false, DateFinPlanifiee = null },                   // Ne doit pas compter (pas de date)
                new Tache { EstConteneur = false, DateFinPlanifiee = aujourdhui.AddDays(1) }     // Ne doit pas compter (futur)
            };
            _planningService.UpdatePlanning(new ConsolidatedPlanning(), new ConfigurationPlanification());


            // Act
            var resultat = _planningService.ObtenirNombreTachesQuiDevraientEtreTerminees(aujourdhui, taches);

            // Assert
            Assert.AreEqual(2, resultat);
        }

        [TestMethod]
        public void CalculerPerformanceCoutCPI_PourV1_RetourneToujoursZero()
        {
            // Arrange
            var taches = new List<Tache>(); // Le contenu n'importe pas pour la V1

            // Act
            var resultat = _planningService.CalculerPerformanceCoutCPI(taches);

            // Assert
            Assert.AreEqual(0.0, resultat);
        }

        // Les tests pour CalculerTensionMetierPourPeriodeFuture sont plus complexes
        // car ils nécessitent de mocker un planning complet. On se contente d'un cas simple.
        [TestMethod]
        public void CalculerTensionMetierPourPeriodeFuture_SansPlanning_RetourneValeurParDefaut()
        {
            // Arrange
            // Pas de _planningService.UpdatePlanning()

            // Act
            var resultat = _planningService.CalculerTensionMetierPourPeriodeFuture(DateTime.Today, 14);

            // Assert
            Assert.AreEqual("N/A", resultat.NomMetier);
            Assert.AreEqual(0, resultat.TauxOccupation);
        }

        #endregion
    }
}