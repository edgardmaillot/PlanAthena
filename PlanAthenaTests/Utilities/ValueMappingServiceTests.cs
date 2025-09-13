using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;

namespace PlanAthenaTests.Utilities
{
    [TestClass]
    public class ValueMappingServiceTests
    {
        private Mock<CheminsPrefereService> _mockCheminsService;
        private Mock<ProjetServiceDataAccess> _mockDataAccess;
        private Mock<UserPreferencesService> _mockPreferencesService;
        private ValueMappingService _valueMappingService;
        private Dictionary<string, string> _testDictionary;

        [TestInitialize]
        public void Setup()
        {
            // Créer les mocks des dépendances de UserPreferencesService
            _mockCheminsService = new Mock<CheminsPrefereService>();
            _mockDataAccess = new Mock<ProjetServiceDataAccess>(_mockCheminsService.Object);

            // Créer le mock de UserPreferencesService avec ses dépendances
            _mockPreferencesService = new Mock<UserPreferencesService>(
                _mockCheminsService.Object,
                _mockDataAccess.Object);

            // Dictionnaire de test
            _testDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "emplacement", "Localisation" },
                { "technicien", "Intervenant" },
                { "Manœuvre", "Opérateur" },
                { "température", "Temp" }
            };

            // Setup des méthodes virtuelles/mockables
            _mockPreferencesService.Setup(x => x.ChargerDictionnaire())
                                  .Returns(_testDictionary);
        }

        #region Tests de Construction

        [TestMethod]
        public void Constructor_AvecPreferencesServiceValide_DoitInitialiserCorrectement()
        {
            // Act
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Assert
            Assert.IsNotNull(_valueMappingService);
            _mockPreferencesService.Verify(x => x.ChargerDictionnaire(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_AvecPreferencesServiceNull_DoitLeverArgumentNullException()
        {
            // Act & Assert
            _valueMappingService = new ValueMappingService(null);
        }

        [TestMethod]
        public void Constructor_QuandChargementDictionnaireReussit_DoitChargerDictionnaire()
        {
            // Arrange
            var dictionnaireAttendu = new Dictionary<string, string> { { "test", "valeur" } };
            _mockPreferencesService.Setup(x => x.ChargerDictionnaire())
                                  .Returns(dictionnaireAttendu);

            // Act
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Assert
            _mockPreferencesService.Verify(x => x.ChargerDictionnaire(), Times.Once);
        }

        [TestMethod]
        public void Constructor_QuandChargementDictionnaireEchoue_DoitGererErreurSansPlanter()
        {
            // Arrange
            _mockPreferencesService.Setup(x => x.ChargerDictionnaire())
                                  .Returns(new Dictionary<string, string>());

            // Act & Assert - Ne doit pas lever d'exception
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);
            Assert.IsNotNull(_valueMappingService);
        }

        #endregion

        #region Tests de TrouveCorrespondance

        [TestMethod]
        public void TrouveCorrespondance_AvecValeurExistante_DoitRetournerTraduction()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultat = _valueMappingService.TrouveCorrespondance("emplacement");

            // Assert
            Assert.AreEqual("Localisation", resultat);
        }

        [TestMethod]
        public void TrouveCorrespondance_AvecValeurInexistante_DoitRetournerChaineVide()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultat = _valueMappingService.TrouveCorrespondance("inexistant");

            // Assert
            Assert.AreEqual(string.Empty, resultat);
        }

        [TestMethod]
        public void TrouveCorrespondance_AvecValeurNull_DoitRetournerChaineVide()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultat = _valueMappingService.TrouveCorrespondance(null);

            // Assert
            Assert.AreEqual(string.Empty, resultat);
        }

        [TestMethod]
        public void TrouveCorrespondance_AvecValeurVide_DoitRetournerChaineVide()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultat = _valueMappingService.TrouveCorrespondance("");

            // Assert
            Assert.AreEqual(string.Empty, resultat);
        }

        [TestMethod]
        public void TrouveCorrespondance_AvecEspaces_DoitRetournerChaineVide()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultat = _valueMappingService.TrouveCorrespondance("   ");

            // Assert
            Assert.AreEqual(string.Empty, resultat);
        }

        [TestMethod]
        public void TrouveCorrespondance_AvecCaracteresSpeciaux_DoitRetournerTraduction()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultat = _valueMappingService.TrouveCorrespondance("Manœuvre");

            // Assert
            Assert.AreEqual("Opérateur", resultat);
        }

        [TestMethod]
        public void TrouveCorrespondance_ComparaisonCaracteresSpeciaux_DoitEtreInsensibleAuxAccents()
        {
            // Arrange
            // Ajoutons une version sans œ pour tester la comparaison
            _testDictionary.Add("Manoeuvre", "OpérateurSansOE");
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultatAvecOE = _valueMappingService.TrouveCorrespondance("Manœuvre");
            var resultatSansOE = _valueMappingService.TrouveCorrespondance("Manoeuvre");

            // Assert
            Assert.AreEqual("Opérateur", resultatAvecOE);
            Assert.AreEqual("OpérateurSansOE", resultatSansOE);
        }

        [TestMethod]
        public void TrouveCorrespondance_CasseDifferente_DoitRetournerTraduction()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultat1 = _valueMappingService.TrouveCorrespondance("EMPLACEMENT");
            var resultat2 = _valueMappingService.TrouveCorrespondance("EmPlAcEmEnT");

            // Assert
            Assert.AreEqual("Localisation", resultat1);
            Assert.AreEqual("Localisation", resultat2);
        }

        #endregion

        #region Tests de AjouteCorrespondance

        [TestMethod]
        public void AjouteCorrespondance_NouvelleCorrespondance_DoitAjouterEtSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("localisation", "Bloc");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Once);

            // Vérifier que la correspondance a été ajoutée
            var resultat = _valueMappingService.TrouveCorrespondance("localisation");
            Assert.AreEqual("Bloc", resultat);
        }

        [TestMethod]
        public void AjouteCorrespondance_CorrespondanceExistante_DoitMettreAJourEtSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("emplacement", "Bloc");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Once);

            // Vérifier que la correspondance a été mise à jour
            var resultat = _valueMappingService.TrouveCorrespondance("emplacement");
            Assert.AreEqual("Bloc", resultat);
        }

        [TestMethod]
        public void AjouteCorrespondance_NouveauMappingAvecCaracteresSpeciaux_DoitAjouterEtSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("Désamianteur", "Manoeuvre ZPSO");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Once);

            // Vérifier que la correspondance a été ajoutée
            var resultat = _valueMappingService.TrouveCorrespondance("Désamianteur");
            Assert.AreEqual("Manoeuvre ZPSO", resultat);
        }

        [TestMethod]
        public void AjouteCorrespondance_AvecValeurNull_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance(null, "traduction");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [TestMethod]
        public void AjouteCorrespondance_AvecTraductionNull_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("valeur", null);

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [TestMethod]
        public void AjouteCorrespondance_AvecValeurVide_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("", "traduction");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [TestMethod]
        public void AjouteCorrespondance_AvecTraductionVide_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("valeur", "");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [TestMethod]
        public void AjouteCorrespondance_AvecEspaces_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("   ", "   ");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        #endregion

        #region Tests de SupprimeCorrespondance

        [TestMethod]
        public void SupprimeCorrespondance_CorrespondanceExistante_DoitSupprimerEtSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.SupprimeCorrespondance("emplacement");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Once);

            // Vérifier que la correspondance a été supprimée
            var resultat = _valueMappingService.TrouveCorrespondance("emplacement");
            Assert.AreEqual(string.Empty, resultat);
        }

        [TestMethod]
        public void SupprimeCorrespondance_CorrespondanceInexistante_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.SupprimeCorrespondance("inexistant");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [TestMethod]
        public void SupprimeCorrespondance_AvecValeurNull_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.SupprimeCorrespondance(null);

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [TestMethod]
        public void SupprimeCorrespondance_AvecValeurVide_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.SupprimeCorrespondance("");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [TestMethod]
        public void SupprimeCorrespondance_AvecEspaces_NePasSauvegarder()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.SupprimeCorrespondance("   ");

            // Assert
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }
        #endregion

        #region Tests d'Intégration et Cas Complexes

        [TestMethod]
        public void Workflow_AjoutModificationSuppression_DoitFonctionnerCorrectement()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act & Assert - Ajouter
            _valueMappingService.AjouteCorrespondance("nouveau", "NouvelleValeur");
            Assert.AreEqual("NouvelleValeur", _valueMappingService.TrouveCorrespondance("nouveau"));

            // Act & Assert - Modifier
            _valueMappingService.AjouteCorrespondance("nouveau", "ValeurModifiée");
            Assert.AreEqual("ValeurModifiée", _valueMappingService.TrouveCorrespondance("nouveau"));

            // Act & Assert - Supprimer
            _valueMappingService.SupprimeCorrespondance("nouveau");
            Assert.AreEqual(string.Empty, _valueMappingService.TrouveCorrespondance("nouveau"));

            // Vérifier que la sauvegarde a été appelée pour chaque opération
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Exactly(3));
        }

        [TestMethod]
        public void MultiplesOperations_AvecCaracteresSpeciaux_DoitPreserverEncodage()
        {
            // Arrange
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("Contrôleur", "Superviseur");
            _valueMappingService.AjouteCorrespondance("Électricien", "Technicien électrique");
            _valueMappingService.AjouteCorrespondance("Plombier-Chauffagiste", "Tech. CVC");

            // Assert
            Assert.AreEqual("Superviseur", _valueMappingService.TrouveCorrespondance("Contrôleur"));
            Assert.AreEqual("Technicien électrique", _valueMappingService.TrouveCorrespondance("Électricien"));
            Assert.AreEqual("Tech. CVC", _valueMappingService.TrouveCorrespondance("Plombier-Chauffagiste"));
        }

        #endregion

        #region Tests de Cas Limites

        [TestMethod]
        public void TrouveCorrespondance_AvecDictionnaireVide_DoitRetournerChaineVide()
        {
            // Arrange
            _mockPreferencesService.Setup(x => x.ChargerDictionnaire())
                                  .Returns(new Dictionary<string, string>());
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            var resultat = _valueMappingService.TrouveCorrespondance("test");

            // Assert
            Assert.AreEqual(string.Empty, resultat);
        }

        [TestMethod]
        public void AjouteCorrespondance_PremièreCorrespondance_DoitAjouterDansDictionnaireVide()
        {
            // Arrange
            _mockPreferencesService.Setup(x => x.ChargerDictionnaire())
                                  .Returns(new Dictionary<string, string>());
            _valueMappingService = new ValueMappingService(_mockPreferencesService.Object);

            // Act
            _valueMappingService.AjouteCorrespondance("premier", "Première valeur");

            // Assert
            var resultat = _valueMappingService.TrouveCorrespondance("premier");
            Assert.AreEqual("Première valeur", resultat);
            _mockPreferencesService.Verify(x => x.SauverDictionnaire(It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        #endregion
    }
}