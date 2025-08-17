// Fichier: PlanAthenaTests/Utilities/DependanceBuilderTests.cs
// Version: Finale Reconstruite
// Description: Suite de tests complète pour DependanceBuilder, alignée sur la nouvelle architecture.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Utilities
{
    [TestClass]
    public class DependanceBuilderTests
    {
        private IIdGeneratorService _idGenerator;
        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private DependanceBuilder _dependanceBuilder;

        private const ChantierPhase TestPhaseContexte = ChantierPhase.SecondOeuvre;

        [TestInitialize]
        public void Setup()
        {
            _idGenerator = new IdGeneratorService();
            _projetService = new ProjetService(_idGenerator);
            _ressourceService = new RessourceService(_idGenerator);
            _dependanceBuilder = new DependanceBuilder(_projetService, _ressourceService);
        }

        // Helper CORRIGÉ pour charger les données de test dans les nouveaux services
        private void ChargerDonneesDeTest(List<Metier> metiers, List<Tache> taches)
        {
            var lots = new List<Lot> { new Lot { LotId = "L001", Blocs = new List<Bloc> { new Bloc { BlocId = "L001_B001" } } } };

            // Utilise les nouvelles méthodes de chargement des services
            _ressourceService.ChargerRessources(metiers, new List<Ouvrier>());
            var projetData = new ProjetData { Lots = lots, Taches = taches };
            _projetService.ChargerProjet(projetData);
        }

        #region Tests ObtenirDependancesPourTache

        [TestMethod]
        [TestCategory("DependanceBuilder - Logique")]
        public void ObtenirDependances_CycleExistant_NeDoitPasProposerDependanceInverse()
        {
            // ARRANGE: T2 dépend de T1 (T1 -> T2)
            var taches = new List<Tache>
            {
                new Tache { TacheId = "L001_B001_T001", TacheNom = "T1" },
                new Tache { TacheId = "L001_B001_T002", TacheNom = "T2", Dependencies = "L001_B001_T001" }
            };
            ChargerDonneesDeTest(new List<Metier>(), taches);

            var tache1 = _projetService.ObtenirTacheParId("L001_B001_T001");
            var contexte = _projetService.ObtenirToutesLesTaches();

            // ACT: On cherche les dépendances possibles pour T1.
            var resultatsPourT1 = _dependanceBuilder.ObtenirDependancesPourTache(tache1, contexte, TestPhaseContexte);

            // ASSERT: La liste des candidats pour T1 NE DOIT PAS contenir T2, car cela créerait un cycle.
            var dependanceT2 = resultatsPourT1.FirstOrDefault(r => r.TachePredecesseur.TacheId == "L001_B001_T002");
            Assert.IsNull(dependanceT2, "La tâche T2 ne doit pas être un candidat pour T1 car elle en dépend déjà.");
        }

        [TestMethod]
        [TestCategory("DependanceBuilder - Etats")]
        public void ObtenirDependances_SuggestionMetier_DoitMarquerCommeSuggeree()
        {
            // ARRANGE: Plomberie (M002) requiert Maçonnerie (M001)
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M001", Nom = "Maçonnerie" },
                new Metier { MetierId = "M002", Nom = "Plomberie", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M001" } } }
            };
            var taches = new List<Tache>
            {
                new Tache { TacheId = "L001_B001_T001", TacheNom = "Mur", MetierId = "M001" },
                new Tache { TacheId = "L001_B001_T002", TacheNom = "Tuyaux", MetierId = "M002" }
            };
            ChargerDonneesDeTest(metiers, taches);

            var tachePlomberie = _projetService.ObtenirTacheParId("L001_B001_T002");

            // ACT
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(tachePlomberie, taches, TestPhaseContexte);

            // ASSERT
            var suggestion = resultats.SingleOrDefault(r => r.TachePredecesseur.TacheId == "L001_B001_T001");
            Assert.IsNotNull(suggestion);
            Assert.AreEqual(EtatDependance.Suggeree, suggestion.Etat);
            Assert.IsTrue(suggestion.EstHeritee);
        }

        [TestMethod]
        [TestCategory("DependanceBuilder - Etats")]
        public void ObtenirDependances_DependanceStricte_DoitMarquerCommeStricte()
        {
            var taches = new List<Tache>
            {
                new Tache { TacheId = "L001_B001_T001" },
                new Tache { TacheId = "L001_B001_T002", Dependencies = "L001_B001_T001" }
            };
            ChargerDonneesDeTest(new List<Metier>(), taches);
            var tache2 = _projetService.ObtenirTacheParId("L001_B001_T002");

            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(tache2, taches, TestPhaseContexte);

            var dependance = resultats.SingleOrDefault(r => r.TachePredecesseur.TacheId == "L001_B001_T001");
            Assert.IsNotNull(dependance);
            Assert.AreEqual(EtatDependance.Stricte, dependance.Etat);
            Assert.IsFalse(dependance.EstHeritee);
        }

        [TestMethod]
        [TestCategory("DependanceBuilder - Etats")]
        public void ObtenirDependances_DependanceExclue_DoitMarquerCommeExclue()
        {
            var taches = new List<Tache>
            {
                new Tache { TacheId = "L001_B001_T001" },
                new Tache { TacheId = "L001_B001_T002", ExclusionsDependances = "L001_B001_T001" }
            };
            ChargerDonneesDeTest(new List<Metier>(), taches);
            var tache2 = _projetService.ObtenirTacheParId("L001_B001_T002");

            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(tache2, taches, TestPhaseContexte);

            var dependance = resultats.SingleOrDefault(r => r.TachePredecesseur.TacheId == "L001_B001_T001");
            Assert.IsNotNull(dependance);
            Assert.AreEqual(EtatDependance.Exclue, dependance.Etat);
            Assert.IsTrue(dependance.EstHeritee);
        }

        #endregion

        #region Tests Tri et Validation Métiers

        [TestMethod]
        [TestCategory("DependanceBuilder - Métiers")]
        public void ObtenirMetiersTriesParDependance_OrdreComplexe_DoitTrierCorrectement()
        {
            // ARRANGE: C -> B, B -> A. L'ordre attendu est A, B, C.
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M003", Nom = "Finition", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M002" } } },
                new Metier { MetierId = "M001", Nom = "Fondations" },
                new Metier { MetierId = "M002", Nom = "Murs", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M001" } } }
            };
            ChargerDonneesDeTest(metiers, new List<Tache>());

            // ACT
            var metiersTries = _dependanceBuilder.ObtenirMetiersTriesParDependance();

            // ASSERT
            var ids = metiersTries.Select(m => m.MetierId).ToList();
            Assert.AreEqual(3, ids.Count);
            Assert.IsTrue(ids.IndexOf("M001") < ids.IndexOf("M002"), "Fondations (M001) doit venir avant Murs (M002)");
            Assert.IsTrue(ids.IndexOf("M002") < ids.IndexOf("M003"), "Murs (M002) doit venir avant Finition (M003)");
        }

        [TestMethod]
        [TestCategory("DependanceBuilder - Métiers")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValiderMetier_DependanceCirculaireDirecte_LeveInvalidOperationException()
        {
            // ARRANGE: A -> B et B -> A
            var metierA = new Metier { MetierId = "M001", Nom = "A", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M002" } } };
            var metierB = new Metier { MetierId = "M002", Nom = "B" };
            var tousLesMetiers = new List<Metier> { metierA, metierB };

            // ACT
            // Simule la modification de B pour qu'il dépende de A, créant un cycle
            metierB.PrerequisParPhase[TestPhaseContexte] = new List<string> { "M001" };
            _dependanceBuilder.ValiderMetier(metierB, tousLesMetiers);
        }

        #endregion
    }
}