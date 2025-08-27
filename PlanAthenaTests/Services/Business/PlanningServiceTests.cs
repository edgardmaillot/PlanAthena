// Emplacement: /PlanAthena.Tests/Services/Business/PlanningServiceTests.cs

using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        [TestInitialize]
        public void TestInitialize()
        {
            _planningService = new PlanningService();
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
            // Act
            var planning = _planningService.GetCurrentPlanning();

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

            // Assert (géré par l'attribut ExpectedException)
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdatePlanning_WithNullConfig_ShouldThrowException()
        {
            // Act
            _planningService.UpdatePlanning(_samplePlanning, null);

            // Assert (géré par l'attribut ExpectedException)
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
            var dateDebut = new DateTime(2023, 10, 2);
            var dateFin = new DateTime(2023, 10, 6);

            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);

            // Assert
            Assert.AreEqual(0, result, "Doit retourner 0 si aucun planning/config n'est chargé.");
        }

        [TestMethod]
        public void GetNombreJoursOuvres_WithStandardWeek_ShouldExcludeWeekends()
        {
            // Arrange
            _planningService.UpdatePlanning(_samplePlanning, _sampleConfig);
            // Lundi 2 au Vendredi 6 Octobre 2023 = 5 jours ouvrés
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
            // Vendredi 6 au Lundi 9 Octobre 2023 = Vendredi + Lundi = 2 jours ouvrés
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
            var configTravailSamedi = new ConfigurationPlanification
            {
                JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Saturday }
            };
            _planningService.UpdatePlanning(_samplePlanning, configTravailSamedi);
            // Lundi 2 au Samedi 7 Octobre 2023 = Lundi + Samedi = 2 jours ouvrés
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
            var dateFin = new DateTime(2023, 10, 5); // Fin avant début

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
            var date = new DateTime(2023, 10, 4); // Mercredi

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
            var date = new DateTime(2023, 10, 8); // Dimanche

            // Act
            int result = _planningService.GetNombreJoursOuvres(date, date);

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion
    }
}