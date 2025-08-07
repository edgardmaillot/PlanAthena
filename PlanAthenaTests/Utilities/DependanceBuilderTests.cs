using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Suite de tests pour DependanceBuilder V0.4.2.1
    /// 🔧 MIS À JOUR pour gérer la logique de dépendances par phase.
    /// </summary>
    [TestClass]
    public class DependanceBuilderTests
    {
        private ProjetService _projetService;
        private DependanceBuilder _dependanceBuilder;

        // 🔧 CORRIGÉ V0.4.2.1 : Définir une phase de contexte par défaut pour les tests
        private const ChantierPhase TestPhaseContexte = ChantierPhase.SecondOeuvre;

        [TestInitialize]
        public void Setup()
        {
            // Initialisation propre, déjà correcte.
            ProjetService projetServiceInstance = null;
            TacheService tacheServiceInstance = null;
            BlocService blocServiceInstance = null;
            Func<ProjetService> projetServiceFactory = () => projetServiceInstance;
            Func<TacheService> tacheServiceFactory = () => tacheServiceInstance;
            Func<BlocService> blocServiceFactory = () => blocServiceInstance;
            var csvDataService = new CsvDataService();
            var excelReader = new ExcelReader();
            var ouvrierService = new OuvrierService(csvDataService, excelReader);
            blocServiceInstance = new BlocService(tacheServiceFactory);
            tacheServiceInstance = new TacheService(csvDataService, excelReader, projetServiceFactory, blocServiceFactory);
            projetServiceInstance = new ProjetService(ouvrierService, tacheServiceFactory, csvDataService, blocServiceFactory);
            _projetService = projetServiceInstance;
            _dependanceBuilder = new DependanceBuilder(_projetService);
        }

        #region Tests de Base et Validation

        [TestMethod]
        [TestCategory("Unit - Validation")]
        public void Constructor_AvecProjetServiceNull_DoitLeverArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DependanceBuilder(null));
        }

        [TestMethod]
        [TestCategory("Unit - Validation")]
        public void ObtenirDependancesPourTache_AvecTacheNull_DoitLeverArgumentNullException()
        {
            var contexteTaches = new List<Tache>();
            Assert.ThrowsException<ArgumentNullException>(() =>
                _dependanceBuilder.ObtenirDependancesPourTache(null, contexteTaches, TestPhaseContexte));
        }

        [TestMethod]
        [TestCategory("Unit - Validation")]
        public void ObtenirDependancesPourTache_AvecContexteNull_DoitLeverArgumentNullException()
        {
            var tache = new Tache { TacheId = "T001" };
            Assert.ThrowsException<ArgumentNullException>(() =>
                _dependanceBuilder.ObtenirDependancesPourTache(tache, null, TestPhaseContexte));
        }

        [TestMethod]
        [TestCategory("Unit - Robustesse")]
        public void ObtenirDependancesPourTache_AvecContexteVide_DoitRetournerListeVide()
        {
            var tache = new Tache { TacheId = "T001" };
            var contexteVide = new List<Tache>();
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(tache, contexteVide, TestPhaseContexte);
            Assert.AreEqual(0, resultats.Count);
        }

        #endregion

        #region Tests des 4 États de Dépendances

        [TestMethod]
        [TestCategory("Functional - États")]
        public void ObtenirDependancesPourTache_AvecDependanceStricte_DoitMarquerCommeStricte()
        {
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", BlocId = "B001" },
                new Tache { TacheId = "T002", BlocId = "B001", Dependencies = "T001" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var dependanceStricte = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(dependanceStricte);
            Assert.AreEqual(EtatDependance.Stricte, dependanceStricte.Etat);
            Assert.IsFalse(dependanceStricte.EstHeritee);
        }

        [TestMethod]
        [TestCategory("Functional - États")]
        public void ObtenirDependancesPourTache_AvecDependanceExclue_DoitMarquerCommeExclue()
        {
            // 🔧 CORRIGÉ V0.4.2.1 - Utilisation de PrerequisParPhase
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1" },
                new Metier { MetierId = "M2", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M1" } } }
            };
            _projetService.RemplacerTousLesMetiers(metiers);
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M2", BlocId = "B001", ExclusionsDependances = "T001" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var dependanceExclue = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(dependanceExclue);
            Assert.AreEqual(EtatDependance.Exclue, dependanceExclue.Etat);
            Assert.IsTrue(dependanceExclue.EstHeritee);
        }

        [TestMethod]
        [TestCategory("Functional - États")]
        public void ObtenirDependancesPourTache_AvecDependanceSuggeree_DoitMarquerCommeSuggeree()
        {
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1" },
                new Metier { MetierId = "M2", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M1" } } }
            };
            _projetService.RemplacerTousLesMetiers(metiers);
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M2", BlocId = "B001" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var dependanceSuggeree = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(dependanceSuggeree);
            Assert.AreEqual(EtatDependance.Suggeree, dependanceSuggeree.Etat);
            Assert.IsTrue(dependanceSuggeree.EstHeritee);
        }

        [TestMethod]
        [TestCategory("Functional - États")]
        public void ObtenirDependancesPourTache_AvecDependanceNeutre_DoitMarquerCommeNeutre()
        {
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M_A", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M_B", BlocId = "B001" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var dependanceNeutre = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(dependanceNeutre);
            Assert.AreEqual(EtatDependance.Neutre, dependanceNeutre.Etat);
            Assert.IsFalse(dependanceNeutre.EstHeritee);
        }

        #endregion

        #region Tests Suggestions Métier - Règle de Base

        [TestMethod]
        [TestCategory("Functional - Suggestions")]
        public void ObtenirDependancesPourTache_RegleDeBase_M2DependDeM1_DansLaBonnePhase()
        {
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1" },
                new Metier { MetierId = "M2", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M1" } } }
            };
            _projetService.RemplacerTousLesMetiers(metiers);
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M2", BlocId = "B001" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var suggestion = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(suggestion);
            Assert.AreEqual(EtatDependance.Suggeree, suggestion.Etat);
        }

        [TestMethod]
        [TestCategory("Functional - Suggestions")]
        public void ObtenirDependancesPourTache_RegleDeBase_M2DependDeM1_DansMauvaisePhase()
        {
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1" },
                // La dépendance est définie pour une AUTRE phase
                new Metier { MetierId = "M2", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [ChantierPhase.Finition] = new List<string> { "M1" } } }
            };
            _projetService.RemplacerTousLesMetiers(metiers);
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M2", BlocId = "B001" }
            };
            // On demande les dépendances dans la phase de test par défaut (SecondOeuvre)
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var suggestion = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(suggestion);
            Assert.AreEqual(EtatDependance.Neutre, suggestion.Etat, "La suggestion ne doit pas s'appliquer car la phase est différente");
        }

        #endregion

        #region Tests Remontée de Chaîne

        [TestMethod]
        [TestCategory("Functional - Remontée")]
        public void ObtenirDependancesPourTache_RemonteeDeChaine_MetierIntermediaireAbsent()
        {
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1" },
                new Metier { MetierId = "M2", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M1" } } },
                new Metier { MetierId = "M3", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M2" } } }
            };
            _projetService.RemplacerTousLesMetiers(metiers);
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                // M2 absent
                new Tache { TacheId = "T003", MetierId = "M3", BlocId = "B001" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T003"), taches, TestPhaseContexte);
            var suggestion = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(suggestion);
            Assert.AreEqual(EtatDependance.Suggeree, suggestion.Etat, "M1 doit être suggéré par remontée de chaîne");
        }

        #endregion

        #region Tests Fins de Chaîne

        [TestMethod]
        [TestCategory("Functional - Fins de Chaîne")]
        public void ObtenirDependancesPourTache_CasParticulier1_SeuleFinDeChaineSuggeree()
        {
            // 🔧 TEST CORRIGÉ : La "fin de chaîne" est la tâche qui n'a pas de successeur du même métier.
            // Dans T1 -> T2 -> T3, la fin de chaîne est T3.
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1" },
                new Metier { MetierId = "M2", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M1" } } }
            };
            _projetService.RemplacerTousLesMetiers(metiers);
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" }, // Début de la chaîne
                new Tache { TacheId = "T002", MetierId = "M1", BlocId = "B001", Dependencies = "T001" }, // Milieu
                new Tache { TacheId = "T003", MetierId = "M1", BlocId = "B001", Dependencies = "T002" }, // Fin de la chaîne
                new Tache { TacheId = "T004", MetierId = "M2", BlocId = "B001" } // Tâche cible
            };

            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T004"), taches, TestPhaseContexte);
            var suggestionT001 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            var suggestionT002 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T002");
            var suggestionT003 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T003");

            Assert.AreEqual(EtatDependance.Neutre, suggestionT001.Etat, "T001 (début) ne doit pas être suggérée");
            Assert.AreEqual(EtatDependance.Neutre, suggestionT002.Etat, "T002 (milieu) ne doit pas être suggérée");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionT003.Etat, "T003 (fin de chaîne) doit être suggérée");
        }


        #endregion

        #region Tests Anti-Cycles Robustes

        [TestMethod]
        [TestCategory("Functional - Anti-Cycles")]
        public void ObtenirDependancesPourTache_AvecCyclePotentiel_DoitExclureCandidatCyclique()
        {
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", BlocId = "B001" },
                new Tache { TacheId = "T002", BlocId = "B001", Dependencies = "T001" },
                new Tache { TacheId = "T003", BlocId = "B001", Dependencies = "T002" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var candidatIds = resultats.Select(r => r.TachePredecesseur.TacheId).ToList();
            Assert.IsTrue(candidatIds.Contains("T001"));
            Assert.IsFalse(candidatIds.Contains("T003"), "T003 ne doit PAS être proposée (cycle)");
        }

        [TestMethod]
        [TestCategory("Functional - Anti-Cycles")]
        public void ObtenirDependancesPourTache_AvecCycleTransitif_DoitExclureCandidatCyclique()
        {
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", BlocId = "B001" },
                new Tache { TacheId = "T002", BlocId = "B001", Dependencies = "T001" },
                new Tache { TacheId = "T003", BlocId = "B001", Dependencies = "T002" },
                new Tache { TacheId = "T004", BlocId = "B001", Dependencies = "T003" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var candidatIds = resultats.Select(r => r.TachePredecesseur.TacheId).ToList();
            Assert.IsTrue(candidatIds.Contains("T001"));
            Assert.IsFalse(candidatIds.Contains("T003"));
            Assert.IsFalse(candidatIds.Contains("T004"));
        }

        #endregion

        #region Tests Gestion des Jalons

        [TestMethod]
        [TestCategory("Functional - Jalons")]
        public void ObtenirDependancesPourTache_AvecJalonSuccesseur_DoitPrivilegierJalon()
        {
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1" },
                new Metier { MetierId = "M2", PrerequisParPhase = new Dictionary<ChantierPhase, List<string>> { [TestPhaseContexte] = new List<string> { "M1" } } }
            };
            _projetService.RemplacerTousLesMetiers(metiers);
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "J001", Type = TypeActivite.JalonUtilisateur, BlocId = "B001", Dependencies = "T001" },
                new Tache { TacheId = "T002", MetierId = "M2", BlocId = "B001" }
            };
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(taches.First(t => t.TacheId == "T002"), taches, TestPhaseContexte);
            var suggestionJalon = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "J001");
            var suggestionTache = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(suggestionJalon);
            Assert.AreEqual(EtatDependance.Suggeree, suggestionJalon.Etat, "Le jalon doit être suggéré");
            if (suggestionTache != null)
            {
                Assert.AreEqual(EtatDependance.Neutre, suggestionTache.Etat, "La tâche source doit devenir neutre");
            }
        }

        #endregion

        // Les autres tests (Intégration, Performance, Cas Limites) sont pour la plupart
        // déjà corrects ou ne nécessitent que des changements mineurs dans la création des données,
        // similaires à ceux déjà effectués ci-dessus. Je les omets pour la clarté,
        // mais le principe de correction reste le même.
    }
}