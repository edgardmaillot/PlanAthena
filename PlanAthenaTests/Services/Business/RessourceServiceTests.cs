// Fichier: PlanAthenaTests/Services/Business/RessourceServiceTests.cs
// Description: Suite de tests unitaires pour RessourceService.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Services.Business
{
    [TestClass]
    public class RessourceServiceTests
    {
        private Mock<IIdGeneratorService> _mockIdGenerator;
        private Mock<ProjetService> _mockProjetService;
        private RessourceService _ressourceService;

        [TestInitialize]
        public void Setup()
        {
            _mockIdGenerator = new Mock<IIdGeneratorService>();
            // Le mock de ProjetService est nécessaire pour tester la suppression de métiers/ouvriers
            // On le configure pour qu'il soit "lâche" (Loose), n'exigeant pas un setup pour chaque méthode
            _mockProjetService = new Mock<ProjetService>(Mock.Of<IIdGeneratorService>());

            _ressourceService = new RessourceService(_mockIdGenerator.Object, _mockProjetService.Object);
        }

        #region Tests Métiers

        [TestMethod]
        [TestCategory("CRUD - Metier")]
        public void CreerMetier_Nominal_CreeEtAjouteLeMetier()
        {
            // Arrange
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");

            // Act
            var metier = _ressourceService.CreerMetier("Plomberie", ChantierPhase.SecondOeuvre);

            // Assert
            Assert.IsNotNull(metier);
            Assert.AreEqual("M001", metier.MetierId);
            Assert.AreEqual("Plomberie", metier.Nom);
            Assert.AreEqual(1, _ressourceService.GetAllMetiers().Count);
        }

        [TestMethod]
        [TestCategory("CRUD - Metier")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreerMetier_NomVide_LeveArgumentException()
        {
            // Act
            _ressourceService.CreerMetier(" ");
        }

        [TestMethod]
        [TestCategory("CRUD - Metier")]
        public void SupprimerMetier_NonUtilise_SupprimeCorrectement()
        {
            // Arrange
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");
            var metier = _ressourceService.CreerMetier("Maçonnerie");

            // Configurer le mock pour simuler qu'aucune tâche n'utilise ce métier
            _mockProjetService.Setup(p => p.ObtenirTachesParMetier("M001")).Returns(new List<Tache>());

            // Act
            _ressourceService.SupprimerMetier(metier.MetierId);

            // Assert
            Assert.AreEqual(0, _ressourceService.GetAllMetiers().Count);
        }

        [TestMethod]
        [TestCategory("Validation - Metier")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SupprimerMetier_UtiliseParUneTache_LeveInvalidOperationException()
        {
            // Arrange
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");
            var metier = _ressourceService.CreerMetier("Maçonnerie");

            // Configurer le mock pour simuler qu'une tâche utilise ce métier
            _mockProjetService.Setup(p => p.ObtenirTachesParMetier("M001")).Returns(new List<Tache> { new Tache() });

            // Act
            _ressourceService.SupprimerMetier(metier.MetierId);
        }

        [TestMethod]
        [TestCategory("Validation - Metier")]
        [ExpectedException(typeof(InvalidOperationException), "Dépendance circulaire détectée pour le métier 'Electricien'.")]
        public void ModifierMetier_AjoutDependanceCirculaire_LeveInvalidOperationException()
        {
            // Arrange
            // M001 -> M002
            _mockIdGenerator.SetupSequence(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>()))
                .Returns("M001").Returns("M002");

            var macon = _ressourceService.CreerMetier("Maçon");
            var electricien = _ressourceService.CreerMetier("Electricien");
            electricien.PrerequisParPhase[ChantierPhase.SecondOeuvre] = new List<string> { macon.MetierId };
            _ressourceService.ModifierMetier(electricien); // C'est valide : Elec dépend de Maçon

            // Act : On tente de créer la circularité : M002 -> M001
            macon.PrerequisParPhase[ChantierPhase.SecondOeuvre] = new List<string> { electricien.MetierId };
            _ressourceService.ModifierMetier(macon); // Doit lever une exception

            // Assert est géré par ExpectedException
        }

        #endregion

        #region Tests Ouvriers

        [TestMethod]
        [TestCategory("CRUD - Ouvrier")]
        public void CreerOuvrier_Nominal_CreeEtAjouteOuvrier()
        {
            // Arrange
            _mockIdGenerator.Setup(g => g.GenererProchainOuvrierId(It.IsAny<List<Ouvrier>>())).Returns("O001");

            // Act
            var ouvrier = _ressourceService.CreerOuvrier("Jean", "Dupont", 250);

            // Assert
            Assert.IsNotNull(ouvrier);
            Assert.AreEqual("O001", ouvrier.OuvrierId);
            Assert.AreEqual("Dupont", ouvrier.Nom);
            Assert.AreEqual(1, _ressourceService.GetAllOuvriers().Count);
        }

        [TestMethod]
        [TestCategory("Gestion Compétences")]
        public void AjouterCompetence_Valide_AjouteCorrectement()
        {
            // Arrange
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");
            _mockIdGenerator.Setup(g => g.GenererProchainOuvrierId(It.IsAny<List<Ouvrier>>())).Returns("O001");
            var metier = _ressourceService.CreerMetier("Peinture");
            var ouvrier = _ressourceService.CreerOuvrier();

            // Act
            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metier.MetierId);
            var ouvrierModifie = _ressourceService.GetOuvrierById(ouvrier.OuvrierId);

            // Assert
            Assert.AreEqual(1, ouvrierModifie.Competences.Count);
            Assert.AreEqual("M001", ouvrierModifie.Competences[0].MetierId);
        }

        [TestMethod]
        [TestCategory("Gestion Compétences")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SupprimerCompetence_DerniereCompetence_LeveInvalidOperationException()
        {
            // Arrange
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");
            _mockIdGenerator.Setup(g => g.GenererProchainOuvrierId(It.IsAny<List<Ouvrier>>())).Returns("O001");
            var metier = _ressourceService.CreerMetier("Peinture");
            var ouvrier = _ressourceService.CreerOuvrier();
            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metier.MetierId);

            // Act
            _ressourceService.SupprimerCompetence(ouvrier.OuvrierId, metier.MetierId);
        }

        [TestMethod]
        [TestCategory("Gestion Compétences")]
        public void DefinirMetierPrincipal_Valide_MetLeBonFlag()
        {
            // Arrange
            _mockIdGenerator.SetupSequence(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>()))
                .Returns("M001").Returns("M002");
            _mockIdGenerator.Setup(g => g.GenererProchainOuvrierId(It.IsAny<List<Ouvrier>>())).Returns("O001");

            var metier1 = _ressourceService.CreerMetier("Plomberie");
            var metier2 = _ressourceService.CreerMetier("Chauffage");
            var ouvrier = _ressourceService.CreerOuvrier();

            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metier1.MetierId); // Devient principal par défaut
            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metier2.MetierId);

            // Act
            _ressourceService.DefinirMetierPrincipal(ouvrier.OuvrierId, metier2.MetierId);
            var ouvrierModifie = _ressourceService.GetOuvrierById(ouvrier.OuvrierId);

            // Assert
            var competence1 = ouvrierModifie.Competences.First(c => c.MetierId == "M001");
            var competence2 = ouvrierModifie.Competences.First(c => c.MetierId == "M002");
            Assert.IsFalse(competence1.EstMetierPrincipal);
            Assert.IsTrue(competence2.EstMetierPrincipal);
        }

        #endregion
    }
}