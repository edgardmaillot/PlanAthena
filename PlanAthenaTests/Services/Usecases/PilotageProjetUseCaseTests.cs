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
            // Fournir au moins une tâche valide pour passer la clause de garde initiale.
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T1", EstConteneur = false, Statut = Statut.Planifiée }
            };
            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            var evmReport = new EvmReportDto
            {
                BaselineExists = true,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 40000m,
                ActualCost = 45000m
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<IReadOnlyList<Tache>>())).Returns(evmReport);
            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "N/A", TauxOccupation = 0 });

            // Act
            var result = _useCase.ObtenirIndicateursCockpit();

            // Assert
            Assert.AreEqual(100000m, result.BudgetAtCompletion);
            //Assert.AreEqual(-10000m, result.ScheduleVariance, "SV = 40k - 50k");
            Assert.AreEqual(-5000m, result.CostVariance, "CV = 40k - 45k");
            Assert.AreEqual(0.8, result.SchedulePerformanceIndex, 0.001, "SPI = 40k / 50k");
            Assert.AreEqual(40000.0 / 45000.0, result.CostPerformanceIndex, 0.001, "CPI = 40k / 45k");

            // CORRIGÉ : Ajout d'une tolérance (delta) à l'assertion pour gérer l'imprécision
            Assert.AreEqual(112500m, result.EstimateAtCompletion, 0.01m, "EAC = 100k / (40k/45k)");
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
                new Tache { TacheId = "T1", EstConteneur = false, Statut = Statut.Terminée }
            };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            var evmReport = new EvmReportDto
            {
                BaselineExists = true,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 50000m, // SPI = 1
                ActualCost = 45000m
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<List<Tache>>())).Returns(evmReport);

            var baseline = new PlanningBaseline
            {
                DateCreation = DateTime.Today.AddDays(-10),
                DateFinPlanifieeInitiale = DateTime.Today.AddDays(10)
            };
            _mockPlanningService.Setup(s => s.GetBaseline()).Returns(baseline);

            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirMeteoProjet();

            // Assert
            Assert.AreEqual(ProjectWeatherStatus.Sunny, result.Statut);
            Assert.AreEqual(0, result.DerivPlanningJours); // SPI=1 => pas de dérive
        }

        [TestMethod]
        public void ObtenirMeteoProjet_AvecBaselineEtSPI05_RetourneStatutStormy()
        {
            // Arrange
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T1", EstConteneur = false, Statut = Statut.Terminée }
            };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);

            var evmReport = new EvmReportDto
            {
                BaselineExists = true,
                BudgetAtCompletion = 100000m,
                PlannedValue = 50000m,
                EarnedValue = 25000m, // SPI = 0.5
                ActualCost = 45000m
            };
            _mockPlanningService.Setup(s => s.GetRapportEVMComplet(It.IsAny<DateTime>(), It.IsAny<List<Tache>>())).Returns(evmReport);

            var baseline = new PlanningBaseline
            {
                DateCreation = DateTime.Today.AddDays(-10),
                DateFinPlanifieeInitiale = DateTime.Today.AddDays(10) // Durée planifiée: 20 jours
            };
            _mockPlanningService.Setup(s => s.GetBaseline()).Returns(baseline);

            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirMeteoProjet();

            // Assert
            // SPI = 0.5 => durée estimée = 20 / 0.5 = 40 jours => dérive = 40 - 20 = 20 jours => Stormy
            Assert.AreEqual(ProjectWeatherStatus.Stormy, result.Statut);
            Assert.IsTrue(result.DerivPlanningJours > 10); // Dérive de 20 jours
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