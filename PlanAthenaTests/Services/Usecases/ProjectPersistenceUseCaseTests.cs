// Emplacement: /PlanAthena.Tests/Services/Usecases/ProjectPersistenceUseCaseTests.cs

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Usecases;
using System.Collections.Generic;
using PlanAthena.Services.Business.DTOs; // Ajouté pour Status
using PlanAthena.Services.DTOs.TaskManager; // Ajouté pour Statut

namespace PlanAthenaTests.Services.Usecases
{
    [TestClass]
    public class ProjectPersistenceUseCaseTests
    {
        private Mock<ProjetService> _mockProjetService;
        private Mock<RessourceService> _mockRessourceService;
        private Mock<PlanningService> _mockPlanningService;
        private Mock<TaskManagerService> _mockTaskManagerService;
        private Mock<ProjetServiceDataAccess> _mockDataAccess;
        private Mock<CheminsPrefereService> _mockCheminsService;
        private Mock<IIdGeneratorService> _mockIdGenerator;

        private ProjectPersistenceUseCase _useCase;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockIdGenerator = new Mock<IIdGeneratorService>();

            // --- ORDRE DE DÉPENDANCE CORRECT ---

            // 1. Mocks sans dépendances complexes
            _mockIdGenerator.Setup(g => g.GenererProchainLotId(It.IsAny<IReadOnlyList<Lot>>())).Returns("L001");
            _mockProjetService = new Mock<ProjetService>(_mockIdGenerator.Object);
            _mockRessourceService = new Mock<RessourceService>(_mockIdGenerator.Object);

            // 2. Mocks qui dépendent des précédents
            // PlanningService dépend de RessourceService
            _mockPlanningService = new Mock<PlanningService>(_mockRessourceService.Object);

            // TaskManagerService dépend de PlanningService et IIdGeneratorService
            _mockTaskManagerService = new Mock<TaskManagerService>(_mockPlanningService.Object, _mockIdGenerator.Object);

            // 3. Le reste des mocks
            _mockCheminsService = new Mock<CheminsPrefereService>();
            _mockDataAccess = new Mock<ProjetServiceDataAccess>(_mockCheminsService.Object);

            // 4. Activer CallBase pour tous les mocks concernés
            _mockProjetService.CallBase = true;
            _mockRessourceService.CallBase = true;
            _mockPlanningService.CallBase = true;
            _mockTaskManagerService.CallBase = true;

            // 5. Instancier l'objet à tester
            _useCase = new ProjectPersistenceUseCase(
                _mockProjetService.Object,
                _mockRessourceService.Object,
                _mockPlanningService.Object,
                _mockTaskManagerService.Object,
                _mockDataAccess.Object,
                _mockCheminsService.Object
            );
        }

        [TestMethod]
        public void SauvegarderProjet_QuandCheminConnu_AppelleDataAccessSauvegarder()
        {
            string knownPath = "C:\\projects\\myproject.json";
            _mockDataAccess.Setup(da => da.IsProjectPathKnown()).Returns(true);
            _mockDataAccess.Setup(da => da.GetCurrentProjectPath()).Returns(knownPath);
            _mockProjetService.Setup(ps => ps.GetProjetDataPourSauvegarde()).Returns(new ProjetData());

            _useCase.SauvegarderProjet();

            _mockDataAccess.Verify(da => da.Sauvegarder(It.IsAny<ProjetData>(), knownPath), Times.Once);
            _mockDataAccess.Verify(da => da.ShowSaveDialog(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void SauvegarderProjet_QuandCheminInconnu_AppelleShowSaveDialogEtSauvegarde()
        {
            string newPath = "C:\\projects\\new_project.json";
            _mockDataAccess.Setup(da => da.IsProjectPathKnown()).Returns(false);
            _mockDataAccess.Setup(da => da.ShowSaveDialog(It.IsAny<string>())).Returns(newPath);
            _mockProjetService.Setup(ps => ps.GetProjetDataPourSauvegarde()).Returns(new ProjetData());

            _useCase.SauvegarderProjet();

            _mockDataAccess.Verify(da => da.ShowSaveDialog(It.IsAny<string>()), Times.Once);
            _mockDataAccess.Verify(da => da.Sauvegarder(It.IsAny<ProjetData>(), newPath), Times.Once);
        }

        [TestMethod]
        public void SauvegarderProjet_QuandCheminInconnuEtDialogueAnnule_NeSauvegardePas()
        {
            _mockDataAccess.Setup(da => da.IsProjectPathKnown()).Returns(false);
            _mockDataAccess.Setup(da => da.ShowSaveDialog(It.IsAny<string>())).Returns((string)null);

            _useCase.SauvegarderProjet();

            _mockDataAccess.Verify(da => da.ShowSaveDialog(It.IsAny<string>()), Times.Once);
            _mockDataAccess.Verify(da => da.Sauvegarder(It.IsAny<ProjetData>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void ChargerProjetDepuisChemin_QuandReussi_AppelleServicesDansLeBonOrdre()
        {
            string filePath = "C:\\projects\\existing.json";
            var projetData = new ProjetData { Lots = new List<Lot>(), Metiers = new List<Metier>(), Ouvriers = new List<Ouvrier>(), Taches = new List<Tache>() };
            _mockDataAccess.Setup(da => da.Charger(filePath)).Returns(projetData);

            _useCase.ChargerProjetDepuisChemin(filePath);

            _mockDataAccess.Verify(da => da.Charger(filePath), Times.Once);
            _mockProjetService.Verify(ps => ps.ViderProjet(), Times.Once);
            _mockRessourceService.Verify(rs => rs.ViderMetiers(), Times.Once);
            _mockRessourceService.Verify(rs => rs.ViderOuvriers(), Times.Once);
            _mockPlanningService.Verify(ps => ps.ClearPlanning(), Times.Once);

            // CORRECTION : On vérifie que la méthode a été appelée au moins une fois,
            // car ChargerTaches l'appelle aussi en interne.
            _mockTaskManagerService.Verify(ts => ts.ViderTaches(), Times.AtLeastOnce);

            _mockProjetService.Verify(ps => ps.ChargerProjet(projetData), Times.Once);
            _mockRessourceService.Verify(rs => rs.ChargerRessources(projetData.Metiers, projetData.Ouvriers), Times.Once);
            _mockTaskManagerService.Verify(ts => ts.ChargerTaches(projetData.Taches), Times.Once);
            _mockTaskManagerService.Verify(ts => ts.SynchroniserStatutsTaches(), Times.Once);
        }

        [TestMethod]
        public void CreerNouveauProjet_QuandAppele_ReinitialiseTousLesServices()
        {
            _useCase.CreerNouveauProjet();

            _mockProjetService.Verify(ps => ps.ViderProjet(), Times.Once);
            _mockRessourceService.Verify(rs => rs.ViderMetiers(), Times.Once);
            _mockRessourceService.Verify(rs => rs.ViderOuvriers(), Times.Once);
            _mockPlanningService.Verify(ps => ps.ClearPlanning(), Times.Once);
            _mockTaskManagerService.Verify(ts => ts.ViderTaches(), Times.Once);

            _mockDataAccess.Verify(da => da.ResetCurrentProjectPath(), Times.Once);
            _mockProjetService.Verify(ps => ps.InitialiserNouveauProjet(), Times.Once);
            _mockRessourceService.Verify(rs => rs.ChargerMetiersParDefaut(), Times.Once);
        }
    }
}