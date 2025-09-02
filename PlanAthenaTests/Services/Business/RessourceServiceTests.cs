// Emplacement: PlanAthenaTests/Services/Business/RessourceServiceTests.cs

using Moq;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;

namespace PlanAthenaTests.Services.Business
{
    [TestClass]
    public class RessourceServiceTests
    {
        private Mock<IIdGeneratorService> _mockIdGenerator;
        private RessourceService _ressourceService;
        private bool _metierEstConsidereCommeUtilise;

        [TestInitialize]
        public void Setup()
        {
            _mockIdGenerator = new Mock<IIdGeneratorService>();

            _metierEstConsidereCommeUtilise = false; // Par défaut, les métiers ne sont pas utilisés
            MetierEstUtilisePredicate validerMetierMock = (metierId) => _metierEstConsidereCommeUtilise;

            _ressourceService = new RessourceService(_mockIdGenerator.Object);

            // On vide les métiers chargés par défaut pour garantir un état propre à chaque test.
            _ressourceService.ViderMetiers();
        }

        #region Tests Métiers

        [TestMethod]
        [TestCategory("CRUD - Metier")]
        public void CreerMetier_Nominal_CreeEtAjouteLeMetier()
        {
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");
            var metier = _ressourceService.CreerMetier("Plomberie", ChantierPhase.SecondOeuvre);
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
            _ressourceService.CreerMetier(" ");
        }

        [TestMethod]
        [TestCategory("CRUD - Metier")]
        public void SupprimerMetier_NonUtilise_SupprimeCorrectement()
        {
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");
            var metier = _ressourceService.CreerMetier("Maçonnerie");

            _metierEstConsidereCommeUtilise = false;

            _ressourceService.SupprimerMetier(metier.MetierId);

            Assert.AreEqual(0, _ressourceService.GetAllMetiers().Count);
        }


        [TestMethod]
        [TestCategory("Validation - Metier")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModifierMetier_AjoutDependanceCirculaire_LeveInvalidOperationException()
        {
            _mockIdGenerator.SetupSequence(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>()))
                .Returns("M001").Returns("M002");

            var macon = _ressourceService.CreerMetier("Maçon");
            var electricien = _ressourceService.CreerMetier("Electricien");
            electricien.PrerequisParPhase[ChantierPhase.SecondOeuvre] = new List<string> { macon.MetierId };
            _ressourceService.ModifierMetier(electricien);

            macon.PrerequisParPhase[ChantierPhase.SecondOeuvre] = new List<string> { electricien.MetierId };
            _ressourceService.ModifierMetier(macon);
        }

        #endregion

        #region Tests Ouvriers

        [TestMethod]
        [TestCategory("CRUD - Ouvrier")]
        public void CreerOuvrier_Nominal_CreeEtAjouteOuvrier()
        {
            _mockIdGenerator.Setup(g => g.GenererProchainOuvrierId(It.IsAny<List<Ouvrier>>())).Returns("O001");
            var ouvrier = _ressourceService.CreerOuvrier("Jean", "Dupont", 250);
            Assert.IsNotNull(ouvrier);
            Assert.AreEqual("O001", ouvrier.OuvrierId);
            Assert.AreEqual("Dupont", ouvrier.Nom);
            Assert.AreEqual(1, _ressourceService.GetAllOuvriers().Count);
        }

        [TestMethod]
        [TestCategory("Gestion Compétences")]
        public void AjouterCompetence_Valide_AjouteCorrectement()
        {
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");
            _mockIdGenerator.Setup(g => g.GenererProchainOuvrierId(It.IsAny<List<Ouvrier>>())).Returns("O001");
            var metier = _ressourceService.CreerMetier("Peinture");
            var ouvrier = _ressourceService.CreerOuvrier();

            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metier.MetierId);
            var ouvrierModifie = _ressourceService.GetOuvrierById(ouvrier.OuvrierId);

            Assert.AreEqual(1, ouvrierModifie.Competences.Count);
            Assert.AreEqual("M001", ouvrierModifie.Competences[0].MetierId);
        }

        [TestMethod]
        [TestCategory("Gestion Compétences")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SupprimerCompetence_DerniereCompetence_LeveInvalidOperationException()
        {
            _mockIdGenerator.Setup(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>())).Returns("M001");
            _mockIdGenerator.Setup(g => g.GenererProchainOuvrierId(It.IsAny<List<Ouvrier>>())).Returns("O001");
            var metier = _ressourceService.CreerMetier("Peinture");
            var ouvrier = _ressourceService.CreerOuvrier();
            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metier.MetierId);

            _ressourceService.SupprimerCompetence(ouvrier.OuvrierId, metier.MetierId);
        }

        [TestMethod]
        [TestCategory("Gestion Compétences")]
        public void DefinirMetierPrincipal_Valide_MetLeBonFlag()
        {
            _mockIdGenerator.SetupSequence(g => g.GenererProchainMetierId(It.IsAny<List<Metier>>()))
                .Returns("M001").Returns("M002");
            _mockIdGenerator.Setup(g => g.GenererProchainOuvrierId(It.IsAny<List<Ouvrier>>())).Returns("O001");
            var metier1 = _ressourceService.CreerMetier("Plomberie");
            var metier2 = _ressourceService.CreerMetier("Chauffage");
            var ouvrier = _ressourceService.CreerOuvrier();
            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metier1.MetierId);
            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metier2.MetierId);

            _ressourceService.DefinirMetierPrincipal(ouvrier.OuvrierId, metier2.MetierId);
            var ouvrierModifie = _ressourceService.GetOuvrierById(ouvrier.OuvrierId);

            var competence1 = ouvrierModifie.Competences.First(c => c.MetierId == "M001");
            var competence2 = ouvrierModifie.Competences.First(c => c.MetierId == "M002");
            Assert.IsFalse(competence1.EstMetierPrincipal);
            Assert.IsTrue(competence2.EstMetierPrincipal);
        }

        #endregion
    }
}