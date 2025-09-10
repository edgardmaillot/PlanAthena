// Emplacement: /PlanAthena.Tests/Services/Usecases/PilotageProjetUseCaseTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;
using System;
using System.Collections.Generic;
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
            _mockPlanningService.Setup(s => s.ObtenirNombreTachesQuiDevraientEtreTerminees(It.IsAny<DateTime>(), It.IsAny<List<Tache>>())).Returns(2);
            _mockPlanningService.Setup(s => s.CalculerPerformanceCoutCPI(It.IsAny<List<Tache>>())).Returns(1.0);
            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirIndicateursCockpit();

            // Assert
            // 2 terminées sur 4 tâches non-conteneur planifiées => 50%
            Assert.AreEqual(50.0, result.ProgressionGlobalePourcentage, 0.001);

            // Vérifier que les mocks ont été appelés
            _mockTaskManagerService.Verify(s => s.ObtenirToutesLesTaches(null, null), Times.Once);
            _mockPlanningService.Verify(s => s.ObtenirNombreTachesQuiDevraientEtreTerminees(It.IsAny<DateTime>(), It.IsAny<List<Tache>>()), Times.Once);
        }

        [TestMethod]
        public void ObtenirIndicateursCockpit_AvecTachesEnRetard_CalculeSpiCorrectement()
        {
            // Arrange
            var aujourdhui = DateTime.Today;
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T1", EstConteneur = false, Statut = Statut.Terminée },
                new Tache { TacheId = "T2", EstConteneur = false, Statut = Statut.Terminée },
                new Tache { TacheId = "T3", EstConteneur = false, Statut = Statut.EnCours },
            };

            _mockTaskManagerService.Setup(s => s.ObtenirToutesLesTaches(null, null)).Returns(taches);
            // 3 tâches auraient dû être terminées mais seulement 2 le sont
            _mockPlanningService.Setup(s => s.ObtenirNombreTachesQuiDevraientEtreTerminees(aujourdhui, taches)).Returns(3);
            _mockPlanningService.Setup(s => s.CalculerPerformanceCoutCPI(It.IsAny<List<Tache>>())).Returns(1.0);
            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirIndicateursCockpit();

            // Assert
            // SPI = (Tâches terminées) / (Tâches qui auraient dû être terminées) = 2 / 3
            Assert.AreEqual(2.0 / 3.0, result.PerformanceCalendrierSPI, 0.001);
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
            _mockPlanningService.Setup(s => s.ObtenirNombreTachesQuiDevraientEtreTerminees(It.IsAny<DateTime>(), It.IsAny<List<Tache>>())).Returns(1);
            _mockPlanningService.Setup(s => s.CalculerPerformanceCoutCPI(It.IsAny<List<Tache>>())).Returns(1.0);
            _mockPlanningService.Setup(s => s.CalculerTensionMetierPourPeriodeFuture(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MetierTensionData { NomMetier = "TestMetier", TauxOccupation = 0.8 });

            // Act
            var result = _useCase.ObtenirIndicateursCockpit();

            // Assert
            Assert.AreEqual("N/A", result.LotLePlusARisqueNom);
            Assert.AreEqual(0, result.LotLePlusARisqueDeriveJours);
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