// Emplacement: /PlanAthena.Tests/Services/Business/TaskStatusServiceTests.cs
// Assurez-vous d'avoir la référence au package NuGet "Moq" dans votre projet de test.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Tests.Services.Business
{
    [TestClass]
    public class TaskStatusServiceTests
    {
        private Mock<ProjetService> _mockProjetService;
        private Mock<PlanningService> _mockPlanningService;
        private Mock<RessourceService> _mockRessourceService;
        private TaskStatusService _taskStatusService;

        [TestInitialize]
        public void TestInitialize()
        {
            // Moq nécessite une interface ou une classe avec des méthodes virtuelles pour être mocké.
            // Pour ProjetService, on peut utiliser le mock directement.
            // Pour les autres, il faut s'assurer que les méthodes sont virtuelles ou utiliser des interfaces.
            // Pour cet exemple, nous supposons que nous pouvons mocker les classes directement.
            var mockIdGenerator = new Mock<PlanAthena.Interfaces.IIdGeneratorService>();
            _mockProjetService = new Mock<ProjetService>(mockIdGenerator.Object);

            _mockPlanningService = new Mock<PlanningService>();
            _mockRessourceService = new Mock<RessourceService>(mockIdGenerator.Object);

            _taskStatusService = new TaskStatusService(
                _mockProjetService.Object,
                _mockPlanningService.Object,
                _mockRessourceService.Object
            );

            // Setup des données de base
            var allTasks = new List<Tache>
            {
                new Tache { TacheId = "T01", TacheNom = "Fondations", LotId = "L01"},
                new Tache { TacheId = "T02", TacheNom = "Murs", LotId = "L01"},
                new Tache { TacheId = "T03", TacheNom = "Toiture", LotId = "L02"}
            };
            _mockProjetService.Setup(p => p.ObtenirToutesLesTaches()).Returns(allTasks);
        }

        #region Tests de Persistance et d'État

        [TestMethod]
        public void ChargerStatuts_WithData_ShouldLoadStatusesCorrectly()
        {
            // Arrange
            var statusesToLoad = new Dictionary<string, Status>
            {
                ["T01"] = Status.EnCours,
                ["T02"] = Status.Terminee
            };

            // Act
            _taskStatusService.ChargerStatuts(statusesToLoad);
            var savedStatuses = _taskStatusService.RetourneTousLesStatuts();

            // Assert
            Assert.AreEqual(2, savedStatuses.Count);
            Assert.AreEqual(Status.EnCours, savedStatuses["T01"]);
        }

        [TestMethod]
        public void ModifierStatutTache_SingleTask_ShouldUpdateStatus()
        {
            // Act
            _taskStatusService.ModifierStatutTache(new[] { "T01" }, Status.EnCours);
            var statuses = _taskStatusService.RetourneTousLesStatuts();

            // Assert
            Assert.AreEqual(Status.EnCours, statuses["T01"]);
        }

        [TestMethod]
        public void ModifierTachePlanifiee_ShouldOnlyUpdateIfNotInProgressOrDone()
        {
            // Arrange
            _taskStatusService.ChargerStatuts(new Dictionary<string, Status>
            {
                ["T01"] = Status.NonPlanifiee, // Sera mis à jour
                ["T02"] = Status.EnCours,      // Ne doit pas changer
                ["T03"] = Status.Terminee      // Ne doit pas changer
            });

            // Act
            _taskStatusService.ModifierTachePlanifiee(new[] { "T01", "T02", "T03" });
            var statuses = _taskStatusService.RetourneTousLesStatuts();

            // Assert
            Assert.AreEqual(Status.Planifiee, statuses["T01"]);
            Assert.AreEqual(Status.EnCours, statuses["T02"]);
            Assert.AreEqual(Status.Terminee, statuses["T03"]);
        }

        #endregion

        #region Tests de Consultation Agrégée (RetourneStatutTache)

        [TestMethod]
        public void RetourneStatutTache_ShouldAggregateDataCorrectly()
        {
            // --- ARRANGE ---
            // 1. Statuts
            _taskStatusService.ChargerStatuts(new Dictionary<string, Status> { ["T01"] = Status.Planifiee });

            // 2. Planning (la partie la plus complexe à mocker)
            // *** CORRECTION : Utiliser des dates relatives à aujourd'hui pour rendre le test robuste ***
            var planning = new ConsolidatedPlanning
            {
                SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>
                {
                    ["O1"] = new List<SegmentDeTravail> { new SegmentDeTravail { TacheId = "T01", Jour = DateTime.Today.AddDays(5), OuvrierId = "O1" } },
                    ["O2"] = new List<SegmentDeTravail> { new SegmentDeTravail { TacheId = "T01", Jour = DateTime.Today.AddDays(6), OuvrierId = "O2" } }
                }
            };
            _mockPlanningService.Setup(p => p.GetCurrentPlanning()).Returns(planning);

            // 3. Ressources
            _mockRessourceService.Setup(r => r.GetOuvrierById("O1")).Returns(new Ouvrier { Prenom = "Paul", Nom = "Durand" });
            _mockRessourceService.Setup(r => r.GetOuvrierById("O2")).Returns(new Ouvrier { Prenom = "Alice", Nom = "Martin" });

            // --- ACT ---
            var result = _taskStatusService.RetourneStatutTache();
            var taskInfoT01 = result.FirstOrDefault(t => t.TacheId == "T01");

            // --- ASSERT ---
            Assert.IsNotNull(taskInfoT01);
            Assert.AreEqual("Fondations", taskInfoT01.TacheNom);
            // La date de fin est maintenant dans le futur, donc le statut ne peut pas être "EnRetard"
            Assert.AreEqual(Status.Planifiee, taskInfoT01.Statut);
            Assert.AreEqual(DateTime.Today.AddDays(5), taskInfoT01.DateDebutPlanifiee);
            Assert.AreEqual(DateTime.Today.AddDays(6), taskInfoT01.DateFinPlanifiee);
            Assert.AreEqual(2, taskInfoT01.NomsOuvriersAssignes.Count);
            Assert.IsTrue(taskInfoT01.NomsOuvriersAssignes.Contains("Paul Durand"));
        }

        [TestMethod]
        public void RetourneStatutTache_ShouldCalculateStatusEnRetard()
        {
            // Arrange
            _taskStatusService.ChargerStatuts(new Dictionary<string, Status> { ["T02"] = Status.EnCours });

            var planning = new ConsolidatedPlanning
            {
                SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>
                {
                    ["O1"] = new List<SegmentDeTravail> { new SegmentDeTravail { TacheId = "T02", Jour = DateTime.Today.AddDays(-5) } }
                }
            };
            _mockPlanningService.Setup(p => p.GetCurrentPlanning()).Returns(planning);

            // Act
            var result = _taskStatusService.RetourneStatutTache();
            var taskInfoT02 = result.FirstOrDefault(t => t.TacheId == "T02");

            // Assert
            Assert.IsNotNull(taskInfoT02);
            Assert.AreEqual(Status.EnRetard, taskInfoT02.Statut);
        }

        [TestMethod]
        public void RetourneStatutTache_WithLotFilter_ShouldReturnOnlyTasksFromLot()
        {
            // Arrange
            _mockPlanningService.Setup(p => p.GetCurrentPlanning()).Returns(new ConsolidatedPlanning()); // Planning vide, pas besoin de détails ici

            // Act
            var result = _taskStatusService.RetourneStatutTache(lotIdFilter: "L01");

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => t.TacheId == "T01" || t.TacheId == "T02"));
        }

        [TestMethod]
        public void RetourneStatutTache_WithContainerFilter_ShouldReturnContainerTask()
        {
            // Arrange
            var planningWithParent = new ConsolidatedPlanning
            {
                SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>
                {
                    ["O1"] = new List<SegmentDeTravail> { new SegmentDeTravail { TacheId = "T01_part1", ParentTacheId = "T01", Jour = DateTime.Today } }
                }
            };
            _mockPlanningService.Setup(p => p.GetCurrentPlanning()).Returns(planningWithParent);

            // Act
            var result = _taskStatusService.RetourneStatutTache(taskTypeFilter: TaskTypeFilter.Container);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("T01", result[0].TacheId);
            Assert.IsTrue(result[0].EstTacheConteneur);
        }

        #endregion
    }
}