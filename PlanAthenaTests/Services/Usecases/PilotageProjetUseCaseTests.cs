// Emplacement: /PlanAthena.Tests/Services/Usecases/PilotageProjetUseCaseTests.cs
using Moq;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PlanAthenaTests.Services.Usecases
{
    [TestClass]
    public class PilotageProjetUseCaseTests
    {
        // Mocks pour les dépendances
        private Mock<TaskManagerService> _mockTaskManagerService;
        private Mock<PlanningService> _mockPlanningService;
        private Mock<ProjetService> _mockProjetService;
        private Mock<RessourceService> _mockRessourceService;

        // L'instance de la classe à tester
        private PilotageProjetUseCase _useCase;

        [TestInitialize]
        public void Setup()
        {
            // Dépendance commune pour tous les constructeurs
            var mockIdGenerator = new Mock<PlanAthena.Interfaces.IIdGeneratorService>();

            // 1. Mocker les dépendances en fournissant les arguments à leur constructeur
            _mockRessourceService = new Mock<RessourceService>(mockIdGenerator.Object);
            _mockPlanningService = new Mock<PlanningService>(_mockRessourceService.Object);
            _mockProjetService = new Mock<ProjetService>(mockIdGenerator.Object);
            _mockTaskManagerService = new Mock<TaskManagerService>(_mockPlanningService.Object, mockIdGenerator.Object);

            // 2. IMPORTANT: Ne pas activer CallBase pour les mocks, cela peut causer des effets de bord
            // _mockTaskManagerService.CallBase = false;
            // _mockPlanningService.CallBase = false;
            // _mockProjetService.CallBase = false;
            // _mockRessourceService.CallBase = false;

            // 3. Créer l'instance du UseCase avec les mocks correctement instanciés
            _useCase = new PilotageProjetUseCase(
                _mockTaskManagerService.Object,
                _mockPlanningService.Object,
                _mockProjetService.Object,
                _mockRessourceService.Object
            );
        }

        #region Tests pour ObtenirIndicateursCockpit

        [TestMethod]
        public void ObtenirIndicateursCockpit_AvecTachesTerminees_CalculeProgressionCorrectement()
        {
            // Arrange
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T1", EstConteneur = false, Statut = Statut.Terminée },
                new Tache { TacheId = "T2", EstConteneur = false, Statut = Statut.Terminée },
                new Tache { TacheId = "T3", EstConteneur = false, Statut = Statut.EnCours },
                new Tache { TacheId = "T4", EstConteneur = false, Statut = Statut.Planifiée },
                new Tache { TacheId = "T5", EstConteneur = true, Statut = Statut.EnCours } // Doit être ignorée
            };

            // Setup des mocks avec les valeurs attendues
            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            // Configurer le mock pour GetRapportEVMComplet
            var evmReport = new EvmReportDto
            {
                BaselineExists = false,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 40000m,
                ActualCost = 45000m
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<List<Tache>>())).Returns(evmReport);

            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirIndicateursCockpit();

            // Assert
            // 2 terminées sur 4 tâches non-conteneur planifiées => 50%
            Assert.AreEqual(50.0, result.ProgressionGlobalePourcentage, 0.001);

            // Vérifier que les mocks ont été appelés
            _mockTaskManagerService.Verify(s => s.ObtenirToutesLesTaches(null, null), Times.Once);
            _mockPlanningService.Verify(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<List<Tache>>()), Times.Once);
        }

        [TestMethod]
        public void ObtenirIndicateursCockpit_AvecBaseline_CalculeIndicesEVMCorrectement()
        {
            // Arrange
            var taches = new List<Tache>
    {
        new Tache { TacheId = "T1", EstConteneur = false, Statut = Statut.Planifiée }
    };
            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            // CORRECTION: Utiliser les nouvelles propriétés du DTO enrichi
            var evmReport = new EvmReportDto
            {
                BaselineExists = true,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 40000m,
                ActualCost = 45000m,
                // AJOUT: Les nouvelles propriétés calculées
                ScheduleVariance = -10000m, // EV - PV = 40k - 50k
                CostVariance = -5000m, // EV - AC = 40k - 45k
                SchedulePerformanceIndex = 0.8, // EV / PV = 40k / 50k
                CostPerformanceIndex = 40000.0 / 45000.0, // EV / AC = 40k / 45k
                EstimateAtCompletion = 112500m, // BAC / CPI = 100k / (40k/45k)
                ScheduleVarianceDays = -2.5 // Exemple de conversion en jours
            };

            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<IReadOnlyList<Tache>>())).Returns(evmReport);
            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "N/A", TauxOccupation = 0 });

            // Act
            var result = _useCase.ObtenirIndicateursCockpit();

            // Assert
            Assert.AreEqual(100000m, result.BudgetAtCompletion);
            Assert.AreEqual(-5000m, result.CostVariance, "CV = 40k - 45k");
            Assert.AreEqual(0.8, result.SchedulePerformanceIndex, 0.001, "SPI = 40k / 50k");
            Assert.AreEqual(40000.0 / 45000.0, result.CostPerformanceIndex, 0.001, "CPI = 40k / 45k");
            Assert.AreEqual(112500m, result.EstimateAtCompletion, 0.01m, "EAC = 100k / (40k/45k)");
            Assert.AreEqual(-2.5, result.ScheduleVarianceDays, 0.1, "SV en jours");
        }

        [TestMethod]
        public void ObtenirIndicateursCockpit_SansDerive_LotARisqueEstNA()
        {
            // Arrange
            var taches = new List<Tache>
            {
                new Tache
                {
                    TacheId = "T1",
                    EstConteneur = false,
                    Statut = Statut.EnCours,
                    DateFinPlanifiee = DateTime.Today.AddDays(1), // Pas encore en retard
                    LotId = "L001"
                }
            };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            var evmReport = new EvmReportDto
            {
                BaselineExists = false,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 40000m,
                ActualCost = 45000m
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<List<Tache>>())).Returns(evmReport);
            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirIndicateursCockpit();

            // Assert
            Assert.AreEqual("N/A", result.LotLePlusARisqueNom);
            Assert.AreEqual(0, result.LotLePlusARisqueDeriveJours);
        }

        #endregion

        #region Tests pour ObtenirMeteoProjet

        [TestMethod]
        public void ObtenirMeteoProjet_SansBaseline_RetourneStatutSunny()
        {
            // Arrange
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T1", EstConteneur = false, Statut = Statut.Terminée }
            };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            var evmReport = new EvmReportDto
            {
                BaselineExists = false,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 40000m,
                ActualCost = 45000m
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<List<Tache>>())).Returns(evmReport);
            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirMeteoProjet();

            // Assert
            Assert.AreEqual(ProjectWeatherStatus.Sunny, result.Statut);
            Assert.AreEqual(0, result.DerivPlanningJours); // Pas de dérive sans baseline
        }

        [TestMethod]
        public void ObtenirMeteoProjet_AvecBaselineEtSPI1_RetourneStatutSunny()
        {
            // Arrange
            var taches = new List<Tache>
    {
        new Tache { TacheId = "T1", EstConteneur = false, Statut = Statut.Terminée, ParentId = null, Type = TypeActivite.Tache }
    };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            // CORRECTION: Créer un rapport EVM avec performance parfaite
            var evmReport = new EvmReportDto
            {
                BaselineExists = true,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 50000m, // SPI = 1.0
                ActualCost = 50000m, // CPI = 1.0
                SchedulePerformanceIndex = 1.0, // Performance parfaite
                CostPerformanceIndex = 1.0, // Performance parfaite
                DeviationBudgetPourcentage = 0.0 // Pas de déviation
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<IReadOnlyList<Tache>>())).Returns(evmReport);

            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.2 }); // 80% de disponibilité

            // Act
            var result = _useCase.ObtenirMeteoProjet();

            // Assert
            Assert.AreEqual(ProjectWeatherStatus.Sunny, result.Statut);
        }
        [TestMethod]
        public void ObtenirMeteoProjet_ConditionsOptimales_RetourneStatutSunny()
        {
            // Arrange
            var taches = new List<Tache>
    {
        new Tache
        {
            TacheId = "T1",
            EstConteneur = false,
            Statut = Statut.Terminée,
            ParentId = null,
            Type = TypeActivite.Tache,
            DateDebutPlanifiee = DateTime.Today.AddDays(1) // Pas encore démarrée, normal
        }
    };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            var evmReport = new EvmReportDto
            {
                BaselineExists = true,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 52000m, // SPI = 1.04 > 0.95
                ActualCost = 48000m, // CPI = 1.08 > 0.95
                SchedulePerformanceIndex = 1.04,
                CostPerformanceIndex = 52000.0 / 48000.0,
                DeviationBudgetPourcentage = 0.0
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<IReadOnlyList<Tache>>())).Returns(evmReport);

            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.2 }); // 80% disponible

            // Act
            var result = _useCase.ObtenirMeteoProjet();

            // Assert
            Assert.AreEqual(ProjectWeatherStatus.Sunny, result.Statut);
        }
        [TestMethod]
        public void ObtenirMeteoProjet_AvecBaselineEtSPI05_RetourneStatutStormy()
        {
            // Arrange
            var taches = new List<Tache>
    {
        // AJOUT: 3 tâches en retard de démarrage pour déclencher Stormy
        new Tache
        {
            TacheId = "T1",
            EstConteneur = false,
            Statut = Statut.Planifiée,
            ParentId = null,
            Type = TypeActivite.Tache,
            DateDebutPlanifiee = DateTime.Today.AddDays(-5) // En retard de démarrage
        },
        new Tache
        {
            TacheId = "T2",
            EstConteneur = false,
            Statut = Statut.Planifiée,
            ParentId = null,
            Type = TypeActivite.Tache,
            DateDebutPlanifiee = DateTime.Today.AddDays(-3) // En retard de démarrage
        },
        new Tache
        {
            TacheId = "T3",
            EstConteneur = false,
            Statut = Statut.Planifiée,
            ParentId = null,
            Type = TypeActivite.Tache,
            DateDebutPlanifiee = DateTime.Today.AddDays(-1) // En retard de démarrage
        }
    };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            // CORRECTION: Créer un rapport EVM avec dérive importante mais pas critique
            var evmReport = new EvmReportDto
            {
                BaselineExists = true,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 25000m, // SPI = 0.5
                ActualCost = 45000m,
                SchedulePerformanceIndex = 0.5, // 50% de dérive
                CostPerformanceIndex = 25000.0 / 45000.0, // ~0.56
                DeviationBudgetPourcentage = 0.15 // 15% de déviation budgétaire
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<IReadOnlyList<Tache>>())).Returns(evmReport);

            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirMeteoProjet();

            // Assert
            // CORRECTION: Avec 3 tâches en retard de démarrage, le statut doit être Stormy
            Assert.AreEqual(ProjectWeatherStatus.Stormy, result.Statut);
        }
        #endregion

        #region Tests pour MettreAJourAvancementTache

        [TestMethod]
        public void MettreAJourAvancementTache_AvecDateDebutSeule_AppelleServicesCorrectement()
        {
            // Arrange
            var tache = new Tache { TacheId = "T1", Statut = Statut.Planifiée };
            _mockTaskManagerService.Setup(s => s.ObtenirTache("T1")).Returns(tache);

            // Setup des méthodes appelées
            _mockTaskManagerService.Setup(s => s.ModifierTache(It.IsAny<Tache>()));
            _mockPlanningService.Setup(s => s.ReconcilierAvecAvancementReel(It.IsAny<Tache>()));

            // Act
            _useCase.MettreAJourAvancementTache("T1", DateTime.Now, null);

            // Assert
            Assert.AreEqual(Statut.EnCours, tache.Statut, "Le statut aurait dû passer à 'EnCours'.");
            _mockTaskManagerService.Verify(s => s.ModifierTache(It.Is<Tache>(t => t.TacheId == "T1")), Times.Once);
            _mockPlanningService.Verify(s => s.ReconcilierAvecAvancementReel(It.Is<Tache>(t => t.TacheId == "T1")), Times.Once);
        }

        [TestMethod]
        public void MettreAJourAvancementTache_AvecDebutEtFin_PasseATerminee()
        {
            // Arrange
            var tache = new Tache { TacheId = "T1", Statut = Statut.EnCours };
            var debut = DateTime.Now.AddDays(-2);
            var fin = DateTime.Now;

            _mockTaskManagerService.Setup(s => s.ObtenirTache("T1")).Returns(tache);
            _mockTaskManagerService.Setup(s => s.ModifierTache(It.IsAny<Tache>()));
            _mockPlanningService.Setup(s => s.ReconcilierAvecAvancementReel(It.IsAny<Tache>()));

            // Act
            _useCase.MettreAJourAvancementTache("T1", debut, fin);

            // Assert
            Assert.AreEqual(Statut.Terminée, tache.Statut);
            Assert.AreEqual(debut, tache.DateDebutReelle);
            Assert.AreEqual(fin, tache.DateFinReelle);
            _mockTaskManagerService.Verify(s => s.ModifierTache(tache), Times.Once);
            _mockPlanningService.Verify(s => s.ReconcilierAvecAvancementReel(tache), Times.Once);
        }

        #endregion

        #region Tests pour ObtenirDonneesPourTaskList

        [TestMethod]
        public void ObtenirDonneesPourTaskList_CasNominal_RetourneDtoBienFormate()
        {
            // Arrange
            var affectations = new List<AffectationOuvrier>
            {
                new AffectationOuvrier { NomOuvrier = "Bob" }
            };

            var taches = new List<Tache>
            {
                new Tache
                {
                    TacheId = "T1",
                    TacheNom = "Tache 1",
                    LotId = "L001",
                    MetierId = "M1",
                    Affectations = affectations,
                    Statut = Statut.EnCours,
                    EstConteneur = false
                }
            };

            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Plomberie" }
            };

            var lot = new Lot { LotId = "L001", Nom = "Lot A" };

            // Setup des mocks
            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);
            _mockRessourceService.Setup(s => s.GetAllMetiers()).Returns(metiers);
            _mockProjetService.Setup(s => s.ObtenirLotParId("L001")).Returns(lot);

            // Act
            var result = _useCase.ObtenirDonneesPourTaskList();

            // Assert
            Assert.IsNotNull(result, "Le résultat ne doit pas être null");
            Assert.AreEqual(1, result.Items.Count, "Il doit y avoir 1 tâche");

            var item = result.Items[0];
            Assert.AreEqual("T1", item.TacheId);
            Assert.AreEqual("Tache 1", item.NomTache);
            Assert.AreEqual("Plomberie", item.NomMetier);
            Assert.AreEqual("Bob", item.NomsOuvriersAffectes);
            Assert.AreEqual("EnCours", item.Statut);
            Assert.AreEqual(false, item.EstConteneur);

            Assert.AreEqual(1, result.LotsDisponibles.Count, "Il doit y avoir 1 lot disponible");
            Assert.AreEqual("Lot A", result.LotsDisponibles[0]);

            Assert.AreEqual(1, result.MetiersDisponibles.Count, "Il doit y avoir 1 métier disponible");
            Assert.AreEqual("Plomberie", result.MetiersDisponibles[0]);

            // Vérifier les appels aux mocks
            _mockTaskManagerService.Verify(s => s.ObtenirToutesLesTaches(null, null), Times.Once);
            _mockRessourceService.Verify(s => s.GetAllMetiers(), Times.Once);
            _mockProjetService.Verify(s => s.ObtenirLotParId("L001"), Times.Once);
        }

        [TestMethod]
        public void ObtenirDonneesPourTaskList_AvecTacheSansAffectation_AfficheTimet()
        {
            // Arrange
            var taches = new List<Tache>
            {
                new Tache
                {
                    TacheId = "T1",
                    TacheNom = "Tache sans ouvrier",
                    LotId = "L001",
                    MetierId = "M1",
                    Affectations = new List<AffectationOuvrier>(), // Vide
                    Statut = Statut.Planifiée,
                    EstConteneur = false
                }
            };

            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Électricité" }
            };

            var lot = new Lot { LotId = "L001", Nom = "Lot B" };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);
            _mockRessourceService.Setup(s => s.GetAllMetiers()).Returns(metiers);
            _mockProjetService.Setup(s => s.ObtenirLotParId("L001")).Returns(lot);

            // Act
            var result = _useCase.ObtenirDonneesPourTaskList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("-", result.Items[0].NomsOuvriersAffectes, "Doit afficher '-' quand pas d'ouvrier affecté");
        }

        [TestMethod]
        public void ObtenirDonneesPourTaskList_AvecMetierInconnu_AfficheTimet()
        {
            // Arrange
            var taches = new List<Tache>
            {
                new Tache
                {
                    TacheId = "T1",
                    TacheNom = "Tache métier inconnu",
                    LotId = "L001",
                    MetierId = "M999", // Métier inexistant
                    Affectations = new List<AffectationOuvrier>(),
                    Statut = Statut.Planifiée,
                    EstConteneur = false
                }
            };

            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Électricité" }
            };

            var lot = new Lot { LotId = "L001", Nom = "Lot B" };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);
            _mockRessourceService.Setup(s => s.GetAllMetiers()).Returns(metiers);
            _mockProjetService.Setup(s => s.ObtenirLotParId("L001")).Returns(lot);

            // Act
            var result = _useCase.ObtenirDonneesPourTaskList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("-", result.Items[0].NomMetier, "Doit afficher '-' pour métier inconnu");
        }

        #endregion
    }
}