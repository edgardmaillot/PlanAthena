// Emplacement: /PlanAthena.Tests/Services/Business/TaskManagerServiceTests.cs

using Moq;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.TaskManager;
using System.Data;

namespace PlanAthenaTests.Services.Business
{
    [TestClass]
    public class TaskManagerServiceTests
    {
        private Mock<PlanningService> _mockPlanningService;
        private Mock<IIdGeneratorService> _mockIdGenerator;
        private TaskManagerService _taskManagerService;

        [TestInitialize]
        public void TestInitialize()
        {
            // RessourceService est une dépendance de PlanningService, nous devons le mocker
            var mockRessourceService = new Mock<RessourceService>(new Mock<IIdGeneratorService>().Object);
            _mockPlanningService = new Mock<PlanningService>(mockRessourceService.Object);
            _mockIdGenerator = new Mock<IIdGeneratorService>();

            _taskManagerService = new TaskManagerService(_mockPlanningService.Object, _mockIdGenerator.Object);
        }

        [TestMethod]
        public void MettreAJourApresPlanification_ShouldRemoveOldSubTasksOfReplannedTask()
        {
            // ARRANGE
            var parentTask = new Tache { TacheId = "T1", TacheNom = "Parent" };
            var oldSubTask = new Tache { TacheId = "T1_OLD_P1", ParentId = "T1" };
            _taskManagerService.ChargerTaches(new List<Tache> { parentTask, oldSubTask });

            var newSubTask = new Tache { TacheId = "T1_NEW_P1" };
            var preparationResult = new PreparationResult
            {
                TachesPreparees = new List<Tache> { newSubTask },
                ParentIdParSousTacheId = new Dictionary<string, string> { { "T1_NEW_P1", "T1" } }
            };

            // Simuler un planning vide pour éviter les null refs
            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourTachesFeuilles()).Returns(new Dictionary<string, PlanningInfoPourTache>());
            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourToutesLesTaches()).Returns(new Dictionary<string, PlanningInfoPourTache>());

            // ACT
            _taskManagerService.MettreAJourApresPlanification(_mockPlanningService.Object, preparationResult);

            // ASSERT
            var allTasks = _taskManagerService.ObtenirToutesLesTachesPourSauvegarde();
            Assert.IsNull(allTasks.FirstOrDefault(t => t.TacheId == "T1_OLD_P1"), "L'ancienne sous-tâche aurait dû être supprimée.");
            Assert.IsNotNull(allTasks.FirstOrDefault(t => t.TacheId == "T1_NEW_P1"), "La nouvelle sous-tâche aurait dû être ajoutée.");
        }

        [TestMethod]
        public void MettreAJourApresPlanification_ShouldPreserveSubTasksOfFinishedTask()
        {
            // ARRANGE
            var finishedParent = new Tache { TacheId = "T2", Statut = Statut.Terminée };
            var finishedSubTask = new Tache { TacheId = "T2_P1", ParentId = "T2" };
            _taskManagerService.ChargerTaches(new List<Tache> { finishedParent, finishedSubTask });

            // Le résultat de la préparation est vide car T2 n'a pas été replanifiée
            var preparationResult = new PreparationResult();

            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourTachesFeuilles()).Returns(new Dictionary<string, PlanningInfoPourTache>());
            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourToutesLesTaches()).Returns(new Dictionary<string, PlanningInfoPourTache>());

            // ACT
            _taskManagerService.MettreAJourApresPlanification(_mockPlanningService.Object, preparationResult);

            // ASSERT
            var allTasks = _taskManagerService.ObtenirToutesLesTachesPourSauvegarde();
            Assert.IsNotNull(allTasks.FirstOrDefault(t => t.TacheId == "T2_P1"), "La sous-tâche de la tâche terminée doit être conservée.");
        }

        [TestMethod]
        public void MettreAJourApresPlanification_ShouldAddNewSubTasksWithEmptyDependencies()
        {
            // ARRANGE
            var parentTask = new Tache { TacheId = "T1" };
            _taskManagerService.ChargerTaches(new List<Tache> { parentTask });

            var newSubTask = new Tache { TacheId = "T1_P1", Dependencies = "NE_DOIT_PAS_RESTER" };
            var preparationResult = new PreparationResult
            {
                TachesPreparees = new List<Tache> { newSubTask },
                ParentIdParSousTacheId = new Dictionary<string, string> { { "T1_P1", "T1" } }
            };

            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourTachesFeuilles()).Returns(new Dictionary<string, PlanningInfoPourTache>());
            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourToutesLesTaches()).Returns(new Dictionary<string, PlanningInfoPourTache>());

            // ACT
            _taskManagerService.MettreAJourApresPlanification(_mockPlanningService.Object, preparationResult);

            // ASSERT
            var addedSubTask = _taskManagerService.ObtenirTache("T1_P1");
            Assert.IsNotNull(addedSubTask);
            Assert.AreEqual("T1", addedSubTask.ParentId);
            Assert.AreEqual(string.Empty, addedSubTask.Dependencies, "Les dépendances de la sous-tâche auraient dû être vidées.");
        }

        [TestMethod]
        public void MettreAJourApresPlanification_ShouldUpdateLeafAndParentWithPlanningData()
        {
            // ARRANGE
            var parentTask = new Tache { TacheId = "T1" };
            _taskManagerService.ChargerTaches(new List<Tache> { parentTask });

            var newSubTask = new Tache { TacheId = "T1_P1" };
            var preparationResult = new PreparationResult
            {
                TachesPreparees = new List<Tache> { newSubTask },
                ParentIdParSousTacheId = new Dictionary<string, string> { { "T1_P1", "T1" } }
            };

            // CORRECTION : Utiliser des dates dans le futur pour que SynchroniserStatutsTaches
            // ne passe pas les tâches en "EnRetard".
            var dateDebut = DateTime.Now.AddDays(1);
            var dateFin = DateTime.Now.AddDays(2);

            var leafPlanningInfo = new Dictionary<string, PlanningInfoPourTache>
            {
                ["T1_P1"] = new PlanningInfoPourTache { DateDebut = dateDebut, DateFin = dateFin, Affectations = new List<AffectationOuvrier> { new AffectationOuvrier { OuvrierId = "O1" } } }
            };
            var parentPlanningInfo = new Dictionary<string, PlanningInfoPourTache>
            {
                ["T1"] = new PlanningInfoPourTache { DateDebut = dateDebut, DateFin = dateFin, EstConteneur = true, Affectations = new List<AffectationOuvrier> { new AffectationOuvrier { OuvrierId = "O1" } } }
            };

            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourTachesFeuilles()).Returns(leafPlanningInfo);
            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourToutesLesTaches()).Returns(parentPlanningInfo);

            // ACT
            _taskManagerService.MettreAJourApresPlanification(_mockPlanningService.Object, preparationResult);

            // ASSERT
            var updatedParent = _taskManagerService.ObtenirTache("T1");
            var updatedLeaf = _taskManagerService.ObtenirTache("T1_P1");

            Assert.IsTrue(updatedParent.EstConteneur);
            Assert.AreEqual(dateDebut, updatedParent.DateDebutPlanifiee);
            Assert.AreEqual(1, updatedParent.Affectations.Count);
            // L'assertion est maintenant correcte car la tâche est bien dans le futur.
            Assert.AreEqual(Statut.Planifiée, updatedParent.Statut);

            Assert.AreEqual(dateDebut, updatedLeaf.DateDebutPlanifiee);
            Assert.AreEqual(1, updatedLeaf.Affectations.Count);
            Assert.AreEqual(Statut.Planifiée, updatedLeaf.Statut);
        }


        [TestMethod]
        public void MettreAJourApresPlanification_ShouldRemoveOldTechnicalMilestones()
        {
            // ARRANGE
            var oldMilestone = new Tache { TacheId = "JT_OLD", Type = TypeActivite.JalonTechnique };
            _taskManagerService.ChargerTaches(new List<Tache> { oldMilestone });

            var preparationResult = new PreparationResult(); // Pas de nouvelle planification

            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourTachesFeuilles()).Returns(new Dictionary<string, PlanningInfoPourTache>());
            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourToutesLesTaches()).Returns(new Dictionary<string, PlanningInfoPourTache>());

            // ACT
            _taskManagerService.MettreAJourApresPlanification(_mockPlanningService.Object, preparationResult);

            // ASSERT
            Assert.IsNull(_taskManagerService.ObtenirTache("JT_OLD"), "L'ancien jalon technique aurait dû être supprimé.");
        }


        [TestMethod]
        public void ChargerTaches_ShouldReplaceExistingTasks()
        {
            // ARRANGE
            var initialTask = new Tache { TacheId = "T1", TacheNom = "Initial" };
            _taskManagerService.ChargerTaches(new List<Tache> { initialTask });

            var newTasks = new List<Tache>
    {
        new Tache { TacheId = "T2", TacheNom = "New Task 1" },
        new Tache { TacheId = "T3", TacheNom = "New Task 2" }
    };

            // ACT
            _taskManagerService.ChargerTaches(newTasks);

            // ASSERT
            var allTasks = _taskManagerService.ObtenirToutesLesTachesPourSauvegarde();
            Assert.AreEqual(2, allTasks.Count);
            Assert.IsNull(_taskManagerService.ObtenirTache("T1"));
            Assert.IsNotNull(_taskManagerService.ObtenirTache("T2"));
            Assert.IsNotNull(_taskManagerService.ObtenirTache("T3"));
        }

        [TestMethod]
        public void ChargerTaches_WithNullList_ShouldNotCrash()
        {
            // ARRANGE & ACT
            _taskManagerService.ChargerTaches(null);

            // ASSERT
            var allTasks = _taskManagerService.ObtenirToutesLesTachesPourSauvegarde();
            Assert.AreEqual(0, allTasks.Count);
        }

        [TestMethod]
        public void ViderTaches_ShouldRemoveAllTasks()
        {
            // ARRANGE
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "T1", TacheNom = "Task 1" },
        new Tache { TacheId = "T2", TacheNom = "Task 2" }
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            _taskManagerService.ViderTaches();

            // ASSERT
            var allTasks = _taskManagerService.ObtenirToutesLesTachesPourSauvegarde();
            Assert.AreEqual(0, allTasks.Count);
        }

        [TestMethod]
        public void CreerTache_ShouldCreateTaskWithCorrectProperties()
        {
            // ARRANGE
            var lotId = "LOT1";
            var blocId = "BLOC1";
            var nom = "Nouvelle tâche";
            var heures = 8;
            var metierId = "MET1";

            _mockIdGenerator.Setup(x => x.GenererProchainTacheId(blocId, It.IsAny<List<Tache>>(), TypeActivite.Tache))
                           .Returns("T1");

            // ACT
            var nouvelleTache = _taskManagerService.CreerTache(lotId, blocId, nom, heures, metierId);

            // ASSERT
            Assert.IsNotNull(nouvelleTache);
            Assert.AreEqual("T1", nouvelleTache.TacheId);
            Assert.AreEqual(lotId, nouvelleTache.LotId);
            Assert.AreEqual(blocId, nouvelleTache.BlocId);
            Assert.AreEqual(nom, nouvelleTache.TacheNom);
            Assert.AreEqual(heures, nouvelleTache.HeuresHommeEstimees);
            Assert.AreEqual(metierId, nouvelleTache.MetierId);
            Assert.AreEqual(Statut.Estimée, nouvelleTache.Statut);

            // Vérifier que la tâche est ajoutée au service
            Assert.IsNotNull(_taskManagerService.ObtenirTache("T1"));
        }

        [TestMethod]
        public void CreerTache_WithoutMetierId_ShouldUseEmptyString()
        {
            // ARRANGE
            _mockIdGenerator.Setup(x => x.GenererProchainTacheId(It.IsAny<string>(), It.IsAny<List<Tache>>(), TypeActivite.Tache))
                           .Returns("T1");

            // ACT
            var nouvelleTache = _taskManagerService.CreerTache("LOT1", "BLOC1", "Test", 5);

            // ASSERT
            Assert.AreEqual(string.Empty, nouvelleTache.MetierId);
        }

        [TestMethod]
        public void CreerTacheJalon_ShouldCreateMilestoneWithCorrectType()
        {
            // ARRANGE
            _mockIdGenerator.Setup(x => x.GenererProchainTacheId(It.IsAny<string>(), It.IsAny<List<Tache>>(), TypeActivite.Tache))
                           .Returns("J1");

            // ACT
            var jalon = _taskManagerService.CreerTacheJalon("LOT1", "BLOC1", "Mon Jalon", 2);

            // ASSERT
            Assert.IsNotNull(jalon);
            Assert.AreEqual("J1", jalon.TacheId);
            Assert.AreEqual("Mon Jalon", jalon.TacheNom);
            Assert.AreEqual(2, jalon.HeuresHommeEstimees);
            Assert.AreEqual(TypeActivite.JalonUtilisateur, jalon.Type);
            Assert.AreEqual(Statut.Estimée, jalon.Statut);
        }

        [TestMethod]
        public void CreerTacheJalon_WithDefaultParameters_ShouldUseDefaults()
        {
            // ARRANGE
            _mockIdGenerator.Setup(x => x.GenererProchainTacheId(It.IsAny<string>(), It.IsAny<List<Tache>>(), TypeActivite.Tache))
                           .Returns("J1");

            // ACT
            var jalon = _taskManagerService.CreerTacheJalon("LOT1", "BLOC1");

            // ASSERT
            Assert.AreEqual("Jalon", jalon.TacheNom);
            Assert.AreEqual(0, jalon.HeuresHommeEstimees);
        }

        [TestMethod]
        public void ModifierTache_ShouldUpdateExistingTask()
        {
            // ARRANGE
            var originalTask = new Tache { TacheId = "T1", TacheNom = "Original", HeuresHommeEstimees = 5 };
            _taskManagerService.ChargerTaches(new List<Tache> { originalTask });

            var modifiedTask = new Tache { TacheId = "T1", TacheNom = "Modified", HeuresHommeEstimees = 10 };

            // ACT
            _taskManagerService.ModifierTache(modifiedTask);

            // ASSERT
            var updatedTask = _taskManagerService.ObtenirTache("T1");
            Assert.AreEqual("Modified", updatedTask.TacheNom);
            Assert.AreEqual(10, updatedTask.HeuresHommeEstimees);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ModifierTache_WithNullTask_ShouldThrowException()
        {
            // ACT
            _taskManagerService.ModifierTache(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ModifierTache_WithNonExistentTask_ShouldThrowException()
        {
            // ARRANGE
            var nonExistentTask = new Tache { TacheId = "INEXISTANT", TacheNom = "Test" };

            // ACT
            _taskManagerService.ModifierTache(nonExistentTask);
        }

        [TestMethod]
        public void SupprimerTache_ShouldRemoveTaskAndChildren()
        {
            // ARRANGE
            var parentTask = new Tache { TacheId = "P1", TacheNom = "Parent" };
            var childTask = new Tache { TacheId = "C1", TacheNom = "Child", ParentId = "P1" };
            var grandChildTask = new Tache { TacheId = "GC1", TacheNom = "GrandChild", ParentId = "C1" };

            _taskManagerService.ChargerTaches(new List<Tache> { parentTask, childTask, grandChildTask });

            // ACT
            _taskManagerService.SupprimerTache("P1");

            // ASSERT
            Assert.IsNull(_taskManagerService.ObtenirTache("P1"));
            Assert.IsNull(_taskManagerService.ObtenirTache("C1"));
            Assert.IsNull(_taskManagerService.ObtenirTache("GC1"));
            _mockPlanningService.Verify(p => p.InvaliderTache("P1"), Times.Once);
            _mockPlanningService.Verify(p => p.InvaliderTache("C1"), Times.Once);
            _mockPlanningService.Verify(p => p.InvaliderTache("GC1"), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidConstraintException))]
        public void SupprimerTache_WithDependencies_ShouldThrowException()
        {
            // ARRANGE
            var task1 = new Tache { TacheId = "T1", TacheNom = "Task 1" };
            var task2 = new Tache { TacheId = "T2", TacheNom = "Task 2", Dependencies = "T1" };

            _taskManagerService.ChargerTaches(new List<Tache> { task1, task2 });

            // ACT
            _taskManagerService.SupprimerTache("T1");
        }

        [TestMethod]
        public void SupprimerTache_WithNonExistentId_ShouldNotCrash()
        {
            // ACT & ASSERT (ne doit pas lever d'exception)
            _taskManagerService.SupprimerTache("INEXISTANT");
        }

        [TestMethod]
        public void ObtenirToutesLesTaches_WithLotFilter_ShouldReturnFilteredTasks()
        {
            // ARRANGE
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "T1", LotId = "LOT1", BlocId = "BLOC1" },
        new Tache { TacheId = "T2", LotId = "LOT2", BlocId = "BLOC1" },
        new Tache { TacheId = "T3", LotId = "LOT1", BlocId = "BLOC2" }
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            var result = _taskManagerService.ObtenirToutesLesTaches(lotId: "LOT1");

            // ASSERT
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => t.LotId == "LOT1"));
        }

        [TestMethod]
        public void ObtenirToutesLesTaches_WithBlocFilter_ShouldReturnFilteredTasks()
        {
            // ARRANGE
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "T1", LotId = "LOT1", BlocId = "BLOC1" },
        new Tache { TacheId = "T2", LotId = "LOT2", BlocId = "BLOC1" },
        new Tache { TacheId = "T3", LotId = "LOT1", BlocId = "BLOC2" }
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            var result = _taskManagerService.ObtenirToutesLesTaches(blocId: "BLOC1");

            // ASSERT
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => t.BlocId == "BLOC1"));
        }

        [TestMethod]
        public void ObtenirToutesLesTaches_WithBothFilters_ShouldReturnIntersection()
        {
            // ARRANGE
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "T1", LotId = "LOT1", BlocId = "BLOC1" },
        new Tache { TacheId = "T2", LotId = "LOT2", BlocId = "BLOC1" },
        new Tache { TacheId = "T3", LotId = "LOT1", BlocId = "BLOC2" }
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            var result = _taskManagerService.ObtenirToutesLesTaches("LOT1", "BLOC1");

            // ASSERT
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("T1", result[0].TacheId);
        }

        [TestMethod]
        public void ObtenirTachesParMetier_ShouldReturnTasksWithMatchingMetierId()
        {
            // ARRANGE
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "T1", MetierId = "MET1" },
        new Tache { TacheId = "T2", MetierId = "MET2" },
        new Tache { TacheId = "T3", MetierId = "MET1" },
        new Tache { TacheId = "T4", MetierId = "" }
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            var result = _taskManagerService.ObtenirTachesParMetier("MET1");

            // ASSERT
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => t.MetierId == "MET1"));
        }

        [TestMethod]
        public void ObtenirTachesParMetier_WithEmptyMetierId_ShouldReturnEmptyList()
        {
            // ARRANGE
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "T1", MetierId = "MET1" }
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            var result = _taskManagerService.ObtenirTachesParMetier("");

            // ASSERT
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ObtenirTachesEnfants_ShouldReturnDirectChildren()
        {
            // ARRANGE
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "P1", TacheNom = "Parent" },
        new Tache { TacheId = "C1", TacheNom = "Child1", ParentId = "P1" },
        new Tache { TacheId = "C2", TacheNom = "Child2", ParentId = "P1" },
        new Tache { TacheId = "GC1", TacheNom = "GrandChild", ParentId = "C1" },
        new Tache { TacheId = "C3", TacheNom = "Child3", ParentId = "OTHER" }
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            var children = _taskManagerService.ObtenirTachesEnfants("P1");

            // ASSERT
            Assert.AreEqual(2, children.Count);
            Assert.IsTrue(children.Any(t => t.TacheId == "C1"));
            Assert.IsTrue(children.Any(t => t.TacheId == "C2"));
            Assert.IsFalse(children.Any(t => t.TacheId == "GC1")); // Grand-enfant non inclus
        }

        [TestMethod]
        public void SynchroniserStatutsTaches_ShouldUpdateStatusBasedOnDates()
        {
            // ARRANGE
            var now = DateTime.Now;
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "T1", Statut = Statut.Planifiée }, // Pas de dates -> Estimée
        new Tache { TacheId = "T2", Statut = Statut.Planifiée, DateDebutPlanifiee = now.AddDays(1), DateFinPlanifiee = now.AddDays(2) }, // Future -> Planifiée
        new Tache { TacheId = "T3", Statut = Statut.Planifiée, DateDebutPlanifiee = now.AddDays(-1), DateFinPlanifiee = now.AddDays(1) }, // En cours
        new Tache { TacheId = "T4", Statut = Statut.Planifiée, DateDebutPlanifiee = now.AddDays(-2), DateFinPlanifiee = now.AddDays(-1) }, // En retard
        new Tache { TacheId = "T5", Statut = Statut.Terminée, DateDebutPlanifiee = now.AddDays(-2), DateFinPlanifiee = now.AddDays(-1) } // Terminée (ne doit pas changer)
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            _taskManagerService.SynchroniserStatutsTaches();

            // ASSERT
            Assert.AreEqual(Statut.Estimée, _taskManagerService.ObtenirTache("T1").Statut);
            Assert.AreEqual(Statut.Planifiée, _taskManagerService.ObtenirTache("T2").Statut);
            Assert.AreEqual(Statut.EnCours, _taskManagerService.ObtenirTache("T3").Statut);
            Assert.AreEqual(Statut.EnRetard, _taskManagerService.ObtenirTache("T4").Statut);
            Assert.AreEqual(Statut.Terminée, _taskManagerService.ObtenirTache("T5").Statut); // Inchangé
        }

        [TestMethod]
        public void MarquerTachesTerminees_ShouldMarkTasksAsFinished()
        {
            // ARRANGE
            var tasks = new List<Tache>
    {
        new Tache { TacheId = "T1", Statut = Statut.EnCours },
        new Tache { TacheId = "T2", Statut = Statut.Planifiée }
    };
            _taskManagerService.ChargerTaches(tasks);

            // ACT
            _taskManagerService.MarquerTachesTerminees(new List<string> { "T1", "T2" });

            // ASSERT
            Assert.AreEqual(Statut.Terminée, _taskManagerService.ObtenirTache("T1").Statut);
            Assert.AreEqual(Statut.Terminée, _taskManagerService.ObtenirTache("T2").Statut);
        }

        [TestMethod]
        public void MarquerTachesTerminees_WithContainer_ShouldNotMarkIfChildrenNotFinished()
        {
            // ARRANGE
            var parent = new Tache { TacheId = "P1", EstConteneur = true, Statut = Statut.EnCours };
            var child1 = new Tache { TacheId = "C1", ParentId = "P1", Statut = Statut.Terminée };
            var child2 = new Tache { TacheId = "C2", ParentId = "P1", Statut = Statut.EnCours };

            _taskManagerService.ChargerTaches(new List<Tache> { parent, child1, child2 });

            // ACT
            _taskManagerService.MarquerTachesTerminees(new List<string> { "P1" });

            // ASSERT
            Assert.AreEqual(Statut.EnCours, _taskManagerService.ObtenirTache("P1").Statut); // Ne doit pas changer
        }

        [TestMethod]
        public void MarquerTachesTerminees_WithContainer_ShouldMarkWhenAllChildrenFinished()
        {
            // ARRANGE
            var parent = new Tache { TacheId = "P1", EstConteneur = true, Statut = Statut.EnCours };
            var child1 = new Tache { TacheId = "C1", ParentId = "P1", Statut = Statut.Terminée };
            var child2 = new Tache { TacheId = "C2", ParentId = "P1", Statut = Statut.Terminée };

            _taskManagerService.ChargerTaches(new List<Tache> { parent, child1, child2 });

            // ACT
            _taskManagerService.MarquerTachesTerminees(new List<string> { "P1" });

            // ASSERT
            Assert.AreEqual(Statut.Terminée, _taskManagerService.ObtenirTache("P1").Statut);
        }

        [TestMethod]
        public void MarquerTachesTerminees_ShouldUpdateParentStatus()
        {
            // ARRANGE
            var parent = new Tache { TacheId = "P1", EstConteneur = true, Statut = Statut.EnCours };
            var child1 = new Tache { TacheId = "C1", ParentId = "P1", Statut = Statut.Terminée };
            var child2 = new Tache { TacheId = "C2", ParentId = "P1", Statut = Statut.EnCours };

            _taskManagerService.ChargerTaches(new List<Tache> { parent, child1, child2 });

            // ACT
            _taskManagerService.MarquerTachesTerminees(new List<string> { "C2" });

            // ASSERT
            Assert.AreEqual(Statut.Terminée, _taskManagerService.ObtenirTache("C2").Statut);
            Assert.AreEqual(Statut.Terminée, _taskManagerService.ObtenirTache("P1").Statut); // Parent auto-terminé
        }

        [TestMethod]
        public void MettreAJourTachesFeuillesAvecPlanning_ShouldUpdateLeafTasks()
        {
            // ARRANGE
            var parentTask = new Tache { TacheId = "P1", EstConteneur = true };
            var leafTask = new Tache { TacheId = "L1", ParentId = "P1", Statut = Statut.Estimée };
            var simpleTask = new Tache { TacheId = "S1", EstConteneur = false, Statut = Statut.Estimée };

            _taskManagerService.ChargerTaches(new List<Tache> { parentTask, leafTask, simpleTask });

            var dateDebut = DateTime.Now.AddDays(1);
            var dateFin = DateTime.Now.AddDays(2);
            var affectations = new List<AffectationOuvrier> { new AffectationOuvrier { OuvrierId = "O1" } };

            var infosFeuilles = new Dictionary<string, PlanningInfoPourTache>
            {
                ["L1"] = new PlanningInfoPourTache { DateDebut = dateDebut, DateFin = dateFin, Affectations = affectations },
                ["S1"] = new PlanningInfoPourTache { DateDebut = dateDebut, DateFin = dateFin, Affectations = affectations }
            };

            var preparationResult = new PreparationResult();

            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourTachesFeuilles()).Returns(infosFeuilles);
            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourToutesLesTaches()).Returns(new Dictionary<string, PlanningInfoPourTache>());

            // ACT
            _taskManagerService.MettreAJourApresPlanification(_mockPlanningService.Object, preparationResult);

            // ASSERT
            var updatedLeaf = _taskManagerService.ObtenirTache("L1");
            var updatedSimple = _taskManagerService.ObtenirTache("S1");

            Assert.AreEqual(dateDebut, updatedLeaf.DateDebutPlanifiee);
            Assert.AreEqual(dateFin, updatedLeaf.DateFinPlanifiee);
            Assert.AreEqual(1, updatedLeaf.Affectations.Count);
            Assert.AreEqual(Statut.Planifiée, updatedLeaf.Statut);

            Assert.AreEqual(dateDebut, updatedSimple.DateDebutPlanifiee);
            Assert.AreEqual(Statut.Planifiée, updatedSimple.Statut);
        }

        [TestMethod]
        public void MettreAJourTachesMeresAvecPlanning_ShouldResetUnplannedSimpleTasks()
        {
            // ARRANGE
            var simpleTask = new Tache
            {
                TacheId = "S1",
                EstConteneur = false,
                Statut = Statut.Planifiée,
                DateDebutPlanifiee = DateTime.Now,
                DateFinPlanifiee = DateTime.Now.AddDays(1)
            };
            simpleTask.Affectations.Add(new AffectationOuvrier { OuvrierId = "O1" });

            _taskManagerService.ChargerTaches(new List<Tache> { simpleTask });

            var preparationResult = new PreparationResult();
            var infosPlanning = new Dictionary<string, PlanningInfoPourTache>(); // Vide - la tâche n'est plus planifiée

            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourTachesFeuilles()).Returns(new Dictionary<string, PlanningInfoPourTache>());
            _mockPlanningService.Setup(p => p.ObtenirInfosPlanificationPourToutesLesTaches()).Returns(infosPlanning);

            // ACT
            _taskManagerService.MettreAJourApresPlanification(_mockPlanningService.Object, preparationResult);

            // ASSERT
            var updatedTask = _taskManagerService.ObtenirTache("S1");
            Assert.AreEqual(Statut.Estimée, updatedTask.Statut);
            Assert.IsNull(updatedTask.DateDebutPlanifiee);
            Assert.IsNull(updatedTask.DateFinPlanifiee);
            Assert.AreEqual(0, updatedTask.Affectations.Count);
        }
    }
}