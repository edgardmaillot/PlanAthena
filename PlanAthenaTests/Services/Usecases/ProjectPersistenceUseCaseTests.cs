using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Usecases;
using System.Collections.Generic;
using PlanAthena.Interfaces;
using PlanAthena.Services.Infrastructure;

namespace PlanAthena.Tests.Services.Usecases
{
    [TestClass]
    public class ProjectPersistenceUseCaseTests
    {
        private Mock<ProjetService> _mockProjetService;
        private Mock<RessourceService> _mockRessourceService;
        private Mock<PlanningService> _mockPlanningService;
        private Mock<TaskStatusService> _mockTaskStatusService;
        private Mock<ProjetServiceDataAccess> _mockDataAccess;
        private Mock<CheminsPrefereService> _mockCheminsService;
        private Mock<IIdGeneratorService> _mockIdGenerator;

        private ProjectPersistenceUseCase _useCase;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockIdGenerator = new Mock<IIdGeneratorService>();
            _mockIdGenerator.Setup(g => g.GenererProchainLotId(It.IsAny<IReadOnlyList<Lot>>())).Returns("L001");
            _mockIdGenerator.Setup(g => g.GenererProchainTacheId(It.IsAny<string>(), It.IsAny<IReadOnlyList<Tache>>(), It.IsAny<TypeActivite>())).Returns("T001");
            _mockIdGenerator.Setup(g => g.GenererProchainBlocId(It.IsAny<string>(), It.IsAny<IReadOnlyList<Bloc>>())).Returns("L001_B001");

            _mockProjetService = new Mock<ProjetService>(_mockIdGenerator.Object) { CallBase = true };
            _mockRessourceService = new Mock<RessourceService>(_mockIdGenerator.Object) { CallBase = true };
            _mockPlanningService = new Mock<PlanningService>() { CallBase = true };
            _mockTaskStatusService = new Mock<TaskStatusService>(_mockProjetService.Object, _mockPlanningService.Object, _mockRessourceService.Object) { CallBase = true };
            _mockCheminsService = new Mock<CheminsPrefereService>() { CallBase = true };

            // Le DataAccess reste un pur mock, sans CallBase = true
            _mockDataAccess = new Mock<ProjetServiceDataAccess>(_mockCheminsService.Object);

            _useCase = new ProjectPersistenceUseCase(
                _mockProjetService.Object,
                _mockRessourceService.Object,
                _mockPlanningService.Object,
                _mockTaskStatusService.Object,
                _mockDataAccess.Object,
                _mockCheminsService.Object
            );
        }

        [TestMethod]
        public void SauvegarderProjet_QuandCheminConnu_AppelleDataAccessSauvegarder()
        {
            // ARRANGE
            string knownPath = "C:\\projects\\myproject.json";
            _mockDataAccess.Setup(da => da.IsProjectPathKnown()).Returns(true);
            _mockDataAccess.Setup(da => da.GetCurrentProjectPath()).Returns(knownPath);
            _mockProjetService.Setup(ps => ps.GetProjetDataPourSauvegarde()).Returns(new ProjetData());

            // ACT
            _useCase.SauvegarderProjet();

            // ASSERT
            _mockDataAccess.Verify(da => da.Sauvegarder(It.IsAny<ProjetData>(), knownPath), Times.Once);
            _mockDataAccess.Verify(da => da.ShowSaveDialog(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void SauvegarderProjet_QuandCheminInconnu_AppelleShowSaveDialogEtSauvegarde()
        {
            // ARRANGE
            string newPath = "C:\\projects\\new_project.json";
            _mockDataAccess.Setup(da => da.IsProjectPathKnown()).Returns(false);
            _mockDataAccess.Setup(da => da.ShowSaveDialog(It.IsAny<string>())).Returns(newPath);
            _mockProjetService.Setup(ps => ps.GetProjetDataPourSauvegarde()).Returns(new ProjetData());

            // ACT
            _useCase.SauvegarderProjet();

            // ASSERT
            _mockDataAccess.Verify(da => da.ShowSaveDialog(It.IsAny<string>()), Times.Once);
            _mockDataAccess.Verify(da => da.Sauvegarder(It.IsAny<ProjetData>(), newPath), Times.Once);
        }

        [TestMethod]
        public void SauvegarderProjet_QuandCheminInconnuEtDialogueAnnule_NeSauvegardePas()
        {
            // ARRANGE
            _mockDataAccess.Setup(da => da.IsProjectPathKnown()).Returns(false);
            _mockDataAccess.Setup(da => da.ShowSaveDialog(It.IsAny<string>())).Returns((string)null);

            // ACT
            _useCase.SauvegarderProjet();

            // ASSERT
            _mockDataAccess.Verify(da => da.ShowSaveDialog(It.IsAny<string>()), Times.Once);
            _mockDataAccess.Verify(da => da.Sauvegarder(It.IsAny<ProjetData>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void ChargerProjetDepuisChemin_QuandReussi_AppelleServicesDansLeBonOrdre()
        {
            // ARRANGE
            string filePath = "C:\\projects\\existing.json";
            var projetData = new ProjetData
            {
                Lots = new List<Lot>(),
                Metiers = new List<Metier>(),
                Ouvriers = new List<Ouvrier>(), // Ajout pour être complet
                TaskStatuses = new Dictionary<string, Status>()
            };
            _mockDataAccess.Setup(da => da.Charger(filePath)).Returns(projetData);

            // ACT
            _useCase.ChargerProjetDepuisChemin(filePath);

            // ASSERT
            // Étape 1: Vérifie que la lecture a bien eu lieu
            _mockDataAccess.Verify(da => da.Charger(filePath), Times.Once);

            // Étape 2: Vérifie que la réinitialisation de chaque service a été initiée
            _mockProjetService.Verify(ps => ps.ViderProjet(), Times.AtLeastOnce());
            _mockRessourceService.Verify(rs => rs.ViderMetiers(), Times.AtLeastOnce());
            _mockRessourceService.Verify(rs => rs.ViderOuvriers(), Times.AtLeastOnce());
            _mockPlanningService.Verify(ps => ps.ClearPlanning(), Times.Once);

            // Étape 3: Vérifie que le chargement des données a eu lieu avec les bonnes données
            _mockProjetService.Verify(ps => ps.ChargerProjet(projetData), Times.Once);
            _mockRessourceService.Verify(rs => rs.ChargerRessources(projetData.Metiers, projetData.Ouvriers), Times.Once);
            _mockTaskStatusService.Verify(ts => ts.ChargerStatuts(projetData.TaskStatuses), Times.Once);
        }

        [TestMethod]
        public void CreerNouveauProjet_QuandAppele_ReinitialiseTousLesServices()
        {
            // ACT
            _useCase.CreerNouveauProjet();

            // ASSERT
            // --- CORRECTION ---
            _mockProjetService.Verify(ps => ps.ViderProjet(), Times.AtLeastOnce());

            _mockRessourceService.Verify(rs => rs.ViderMetiers(), Times.AtLeastOnce);
            _mockRessourceService.Verify(rs => rs.ViderOuvriers(), Times.Once);
            _mockPlanningService.Verify(ps => ps.ClearPlanning(), Times.Once);
            _mockDataAccess.Verify(da => da.ResetCurrentProjectPath(), Times.Once);
            _mockProjetService.Verify(ps => ps.InitialiserNouveauProjet(), Times.Once);
            _mockRessourceService.Verify(rs => rs.ChargerMetiersParDefaut(), Times.Once);
            _mockTaskStatusService.Verify(ts => ts.InitialiserStatutsPourNouveauProjet(), Times.Once);
        }
    }
}