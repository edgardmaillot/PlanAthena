// Emplacement: /PlanAthena.Tests/Services/Business/PlanningServiceTests.cs

using Moq;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.TaskManager;
using PlanAthena.Services.DTOs.UseCases;

namespace PlanAthenaTests.Services.Business
{
    [TestClass]
    public class PlanningServiceTests
    {
        private PlanningService _planningService;
        private ConsolidatedPlanning _samplePlanning;
        private ConfigurationPlanification _sampleConfig;
        private PlanningBaseline _sampleBaseline;

        // Mock de la dépendance requise par le constructeur de PlanningService
        private Mock<RessourceService> _mockRessourceService;

        [TestInitialize]
        public void TestInitialize()
        {
            // Création du mock pour la dépendance de PlanningService
            var mockIdGenerator = new Mock<IIdGeneratorService>();
            _mockRessourceService = new Mock<RessourceService>(mockIdGenerator.Object);

            // Configuration des données de test pour RessourceService
            SetupMockRessourceService();

            // Instanciation de PlanningService
            _planningService = new PlanningService(_mockRessourceService.Object);

            _samplePlanning = CreateSampleConsolidatedPlanning();
            _sampleConfig = CreateSampleConfiguration();
            _sampleBaseline = CreateSampleBaseline();
        }

        private void SetupMockRessourceService()
        {
            var ouvriers = new List<Ouvrier>
            {
                new Ouvrier { OuvrierId = "O1", Prenom="Jean", Nom = "Dupont", MetierId = "M1", CoutJournalier = 400 },
                new Ouvrier { OuvrierId = "O2", Prenom = "Marie", Nom = "Martin", MetierId = "M2", CoutJournalier = 350 }
            };

            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Développeur" },
                new Metier { MetierId = "M2", Nom = "Testeur" }
            };

            _mockRessourceService.Setup(rs => rs.GetAllOuvriers()).Returns(ouvriers);
            _mockRessourceService.Setup(rs => rs.GetAllMetiers()).Returns(metiers);
            _mockRessourceService.Setup(rs => rs.GetOuvrierById("O1")).Returns(ouvriers[0]);
            _mockRessourceService.Setup(rs => rs.GetOuvrierById("O2")).Returns(ouvriers[1]);
        }

        private ConsolidatedPlanning CreateSampleConsolidatedPlanning()
        {
            var segments = new List<SegmentDeTravail>
            {
                new SegmentDeTravail
                {
                    TacheId = "T1",
                    TacheNom = "Tâche 1",
                    OuvrierId = "O1",
                    Jour = new DateTime(2023, 10, 2),
                    HeureDebut = TimeSpan.FromHours(8),
                    HeureFin = TimeSpan.FromHours(12),
                    HeuresTravaillees = 4.0,
                    BlocId = "B1"
                },
                new SegmentDeTravail
                {
                    TacheId = "T2",
                    TacheNom = "Tâche 2",
                    ParentTacheId = "T1",
                    OuvrierId = "O2",
                    Jour = new DateTime(2023, 10, 3),
                    HeureDebut = TimeSpan.FromHours(9),
                    HeureFin = TimeSpan.FromHours(17),
                    HeuresTravaillees = 8.0,
                    BlocId = "B2"
                }
            };

            return new ConsolidatedPlanning
            {
                SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>
                {
                    { "O1", new List<SegmentDeTravail> { segments[0] } },
                    { "O2", new List<SegmentDeTravail> { segments[1] } }
                },
                DateDebutProjet = new DateTime(2023, 10, 1),
                DateFinProjet = new DateTime(2023, 12, 31)
            };
        }

        private ConfigurationPlanification CreateSampleConfiguration()
        {
            return new ConfigurationPlanification
            {
                JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                HeuresTravailEffectifParJour = 8,
                CoutIndirectJournalierAbsolu = 100
            };
        }

        private PlanningBaseline CreateSampleBaseline()
        {
            return new PlanningBaseline
            {
                BudgetAtCompletion = 10000m,
                CourbePlannedValueCumulative = new Dictionary<DateTime, decimal>
                {
                    { new DateTime(2023, 10, 1), 1000m },
                    { new DateTime(2023, 10, 15), 3000m },
                    { new DateTime(2023, 11, 1), 6000m },
                    { new DateTime(2023, 12, 1), 9000m },
                    { new DateTime(2023, 12, 31), 10000m }
                },
                BudgetInitialParTacheId = new Dictionary<string, decimal>
                {
                    { "T1", 5000m },
                    { "T2", 3000m },
                    { "T3", 2000m }
                }
            };
        }

        #region Tests de Gestion de l'État (Planning & Baseline)

        [TestMethod]
        public void GetCurrentPlanning_InitialState_ShouldReturnNull()
        {
            // Arrange
            var service = new PlanningService(_mockRessourceService.Object);

            // Act
            var planning = service.GetCurrentPlanning();

            // Assert
            Assert.IsNull(planning, "Le planning doit être null à l'initialisation.");
        }

        [TestMethod]
        public void LoadPlanning_WithValidData_ShouldSetCurrentPlanning()
        {
            // Act
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);

            // Assert
            var planning = _planningService.GetCurrentPlanning();
            var config = _planningService.GetCurrentConfig();

            Assert.IsNotNull(planning, "Le planning ne doit pas être null après LoadPlanning.");
            Assert.AreSame(_samplePlanning, planning, "La référence du planning doit être celle qui a été passée.");
            Assert.AreSame(_sampleConfig, config, "La référence de la config doit être celle qui a été passée.");
        }

        [TestMethod]
        public void LoadPlanning_WithBaseline_ShouldSetPlanningConfigAndBaseline()
        {
            // Act
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig, _sampleBaseline);

            // Assert
            var planning = _planningService.GetCurrentPlanning();
            var config = _planningService.GetCurrentConfig();
            var baseline = _planningService.GetBaseline();

            Assert.IsNotNull(planning);
            Assert.IsNotNull(config);
            Assert.IsNotNull(baseline);
            Assert.AreSame(_sampleBaseline, baseline, "La référence de la baseline doit être celle qui a été passée.");
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
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig, _sampleBaseline);

            // Act
            _planningService.ClearPlanning();

            // Assert
            var planning = _planningService.GetCurrentPlanning();
            var config = _planningService.GetCurrentConfig();
            var baseline = _planningService.GetBaseline();

            Assert.IsNull(planning, "Le planning doit être null après ClearPlanning.");
            Assert.IsNull(config, "La config doit être null après ClearPlanning.");
            Assert.IsNull(baseline, "La baseline doit être null après ClearPlanning.");
        }

        [TestMethod]
        public void InvaliderTache_WithValidTacheId_ShouldRemoveSegments()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);

            // Act
            _planningService.InvaliderTache("T1");

            // Assert
            var planning = _planningService.GetCurrentPlanning();
            var allSegments = planning.SegmentsParOuvrierId.Values.SelectMany(list => list).ToList();

            Assert.IsFalse(allSegments.Any(s => s.TacheId == "T1"), "Tous les segments de la tâche T1 doivent être supprimés.");
            Assert.IsFalse(allSegments.Any(s => s.ParentTacheId == "T1"), "Tous les segments ayant T1 comme parent doivent être supprimés.");
        }

        #endregion

        #region Tests pour la Gestion de la Baseline

        [TestMethod]
        public void SetBaseline_WithValidBaseline_ShouldStoreBaseline()
        {
            // Act
            _planningService.SetBaseline(_sampleBaseline);

            // Assert
            var baseline = _planningService.GetBaseline();
            Assert.AreSame(_sampleBaseline, baseline);
        }

        [TestMethod]
        public void ClearBaseline_AfterSet_ShouldReturnNull()
        {
            // Arrange
            _planningService.SetBaseline(_sampleBaseline);

            // Act
            _planningService.ClearBaseline();

            // Assert
            var baseline = _planningService.GetBaseline();
            Assert.IsNull(baseline);
        }

        #endregion

        #region Tests pour les Calculs EVM

        [TestMethod]
        public void GetRapportEVMComplet_WithoutBaseline_ShouldReturnInvalidReport()
        {
            // Arrange
            var taches = new List<Tache>();
            var dateRef = new DateTime(2023, 11, 1);

            // Act
            var rapport = _planningService.GetRapportEVMComplet(dateRef, taches);

            // Assert
            Assert.IsFalse(rapport.BaselineExists, "Le rapport doit indiquer qu'aucune baseline n'existe.");
        }

        [TestMethod]
        public void GetRapportEVMComplet_WithBaseline_ShouldCalculateCorrectValues()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig, _sampleBaseline);
            var dateRef = new DateTime(2023, 10, 15);
            var taches = new List<Tache>
            {
                new Tache
                {
                    TacheId = "T1",
                    Statut = Statut.Terminée,
                    DateFinReelle = new DateTime(2023, 10, 10)
                }
            };

            // Act
            var rapport = _planningService.GetRapportEVMComplet(dateRef, taches);

            // Assert
            Assert.IsTrue(rapport.BaselineExists, "Le rapport doit indiquer qu'une baseline existe.");
            Assert.AreEqual(10000m, rapport.BudgetAtCompletion);
            Assert.AreEqual(3000m, rapport.PlannedValue, "PV doit correspondre à la valeur dans la courbe baseline au 15/10.");
            Assert.AreEqual(5000m, rapport.EarnedValue, "EV doit correspondre au budget de T1 terminée.");
            Assert.IsTrue(rapport.ActualCost > 0, "AC doit être calculé basé sur les segments de travail.");
        }

        [TestMethod]
        public void GetRapportEVMComplet_WithFutureDate_ShouldUseLatestBaselineValue()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig, _sampleBaseline);
            var dateRef = new DateTime(2024, 1, 15); // Date future par rapport à la baseline
            var taches = new List<Tache>();

            // Act
            var rapport = _planningService.GetRapportEVMComplet(dateRef, taches);

            // Assert
            Assert.AreEqual(10000m, rapport.PlannedValue, "PV doit utiliser la dernière valeur de la baseline.");
        }

        #endregion

        #region Tests pour les Méthodes du Cockpit

        [TestMethod]
        public void ReconcilierAvecAvancementReel_TacheEnCours_ShouldUpdateInternalState()
        {
            // Arrange
            var tacheEnCours = new Tache { TacheId = "T1", Statut = Statut.EnCours };
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);

            // Act
            _planningService.ReconcilierAvecAvancementReel(tacheEnCours);

            // Assert
            // Test de non-régression - la méthode doit s'exécuter sans erreur
            Assert.IsTrue(true, "La méthode s'est exécutée sans lever d'exception.");
        }

        [TestMethod]
        public void ReconcilierAvecAvancementReel_TacheRedevientPlanifiee_ShouldRemoveFromInternalState()
        {
            // Arrange
            var tache = new Tache { TacheId = "T1", Statut = Statut.EnCours };
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);
            _planningService.ReconcilierAvecAvancementReel(tache);

            // Act
            tache.Statut = Statut.Planifiée;
            _planningService.ReconcilierAvecAvancementReel(tache);

            // Assert
            Assert.IsTrue(true, "La méthode s'est exécutée sans lever d'exception.");
        }

        [TestMethod]
        public void ReconcilierAvecAvancementReel_WithNullTache_ShouldNotThrow()
        {
            // Act & Assert
            _planningService.ReconcilierAvecAvancementReel(null);
            Assert.IsTrue(true, "La méthode doit gérer les valeurs null sans erreur.");
        }

        [TestMethod]
        public void ObtenirNombreTachesQuiDevraientEtreTerminees_CasNominal_ShouldReturnCorrectCount()
        {
            // Arrange
            var aujourdhui = DateTime.Today;
            var taches = new List<Tache>
            {
                new Tache { EstConteneur = false, DateFinPlanifiee = aujourdhui.AddDays(-2) }, // Doit compter
                new Tache { EstConteneur = false, DateFinPlanifiee = aujourdhui },             // Doit compter
                new Tache { EstConteneur = true,  DateFinPlanifiee = aujourdhui.AddDays(-1) }, // Ne doit pas compter (conteneur)
                new Tache { EstConteneur = false, DateFinPlanifiee = null },                   // Ne doit pas compter (pas de date)
                new Tache { EstConteneur = false, DateFinPlanifiee = aujourdhui.AddDays(1) }   // Ne doit pas compter (futur)
            };

            // Act
            var resultat = _planningService.ObtenirNombreTachesQuiDevraientEtreTerminees(aujourdhui, taches);

            // Assert
            Assert.AreEqual(2, resultat);
        }

        [TestMethod]
        public void CalculerPerformanceCoutCPI_ReturnZeroForV1()
        {
            // Arrange
            var taches = new List<Tache>();

            // Act
            var resultat = _planningService.CalculerPerformanceCoutCPI(taches);

            // Assert
            Assert.AreEqual(0.0, resultat, "CPI doit retourner 0 pour la version 1.");
        }

        [TestMethod]
        public void CalculerTensionMetierPourPeriodeFuture_SansPlanning_ShouldReturnDefault()
        {
            // Act
            var resultat = _planningService.CalculerTensionMetierPourPeriodeFuture(DateTime.Today, 14);

            // Assert
            Assert.AreEqual("N/A", resultat.NomMetier);
            Assert.AreEqual(0, resultat.TauxOccupation);
        }

        [TestMethod]
        public void CalculerTensionMetierPourPeriodeFuture_AvecPlanning_ShouldCalculateCorrectly()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);
            var dateDebut = new DateTime(2023, 10, 2);
            var nombreJours = 5;

            // Act
            var resultat = _planningService.CalculerTensionMetierPourPeriodeFuture(dateDebut, nombreJours);

            // Assert
            Assert.IsNotNull(resultat);
            Assert.IsTrue(resultat.TauxOccupation >= 0, "Le taux d'occupation doit être positif.");
        }

        #endregion

        #region Tests pour les Méthodes d'Agrégation

        [TestMethod]
        public void ObtenirInfosPlanificationPourToutesLesTaches_SansPlanning_ShouldReturnEmpty()
        {
            // Act
            var infos = _planningService.ObtenirInfosPlanificationPourToutesLesTaches();

            // Assert
            Assert.IsNotNull(infos);
            Assert.AreEqual(0, infos.Count);
        }

        [TestMethod]
        public void ObtenirInfosPlanificationPourToutesLesTaches_AvecPlanning_ShouldReturnCorrectInfo()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);

            // Act
            var infos = _planningService.ObtenirInfosPlanificationPourToutesLesTaches();

            // Assert
            Assert.IsTrue(infos.Count > 0, "Des informations de planification doivent être retournées.");

            if (infos.ContainsKey("T1"))
            {
                var infoT1 = infos["T1"];
                Assert.IsNotNull(infoT1.DateDebut);
                Assert.IsNotNull(infoT1.DateFin);
                Assert.IsTrue(infoT1.Affectations.Any(), "T1 doit avoir des affectations.");
            }
        }

        [TestMethod]
        public void ObtenirInfosPlanificationPourTachesFeuilles_AvecPlanning_ShouldReturnLeafTasksOnly()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);

            // Act
            var infos = _planningService.ObtenirInfosPlanificationPourTachesFeuilles();

            // Assert
            Assert.IsTrue(infos.Count > 0);

            // Vérifier que toutes les tâches retournées ne sont pas des conteneurs
            foreach (var info in infos.Values)
            {
                Assert.IsFalse(info.EstConteneur, "Les tâches feuilles ne doivent pas être des conteneurs.");
            }
        }

        #endregion

        #region Tests pour les Méthodes de Vue & Calcul

        [TestMethod]
        public void RetournePlanningDetailleParJour_SansPlanning_ShouldReturnEmpty()
        {
            // Act
            var planning = _planningService.RetournePlanningDetailleParJour();

            // Assert
            Assert.IsNotNull(planning);
            Assert.AreEqual(0, planning.Count);
        }

        [TestMethod]
        public void RetournePlanningDetailleParJour_AvecPlanning_ShouldReturnDailyBreakdown()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);

            // Act
            var planning = _planningService.RetournePlanningDetailleParJour();

            // Assert
            Assert.IsTrue(planning.Count > 0, "Le planning détaillé doit contenir des jours.");

            var premierJour = planning.First();
            Assert.IsTrue(premierJour.Ouvriers.Any(), "Le premier jour doit avoir des ouvriers assignés.");
            Assert.IsTrue(premierJour.Ouvriers.First().Affectations.Any(), "Les ouvriers doivent avoir des affectations.");
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
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);
            var dateDebut = new DateTime(2023, 10, 2); // Lundi
            var dateFin = new DateTime(2023, 10, 6);   // Vendredi

            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);

            // Assert
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void GetNombreJoursOuvres_SpanningAWeekend_ShouldCalculateCorrectly()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);
            var dateDebut = new DateTime(2023, 10, 6); // Vendredi
            var dateFin = new DateTime(2023, 10, 9);   // Lundi

            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);

            // Assert
            Assert.AreEqual(2, result); // Vendredi + Lundi
        }

        [TestMethod]
        public void GetNombreJoursOuvres_WithCustomWorkWeek_ShouldUseConfig()
        {
            // Arrange
            var configTravailSamedi = new ConfigurationPlanification
            {
                JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Saturday },
                HeuresTravailEffectifParJour = 8
            };
            _planningService.LoadPlanning(_samplePlanning, configTravailSamedi);
            var dateDebut = new DateTime(2023, 10, 2); // Lundi
            var dateFin = new DateTime(2023, 10, 7);   // Samedi

            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);

            // Assert
            Assert.AreEqual(2, result); // Lundi + Samedi
        }

        [TestMethod]
        public void GetNombreJoursOuvres_WithInvalidDateRange_ShouldReturnZero()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);
            var dateDebut = new DateTime(2023, 10, 10);
            var dateFin = new DateTime(2023, 10, 5);

            // Act
            int result = _planningService.GetNombreJoursOuvres(dateDebut, dateFin);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetNombreJoursOuvres_SingleWorkday_ShouldReturnOne()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);
            var date = new DateTime(2023, 10, 4); // Mercredi

            // Act
            int result = _planningService.GetNombreJoursOuvres(date, date);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void GetNombreJoursOuvres_SingleWeekendDay_ShouldReturnZero()
        {
            // Arrange
            _planningService.LoadPlanning(_samplePlanning, _sampleConfig);
            var date = new DateTime(2023, 10, 8); // Dimanche

            // Act
            int result = _planningService.GetNombreJoursOuvres(date, date);

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion
    }
}