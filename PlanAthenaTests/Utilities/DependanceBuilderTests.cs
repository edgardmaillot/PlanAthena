using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using PlanAthena.Services.DataAccess; // Ajouté pour CsvDataService, ExcelReader, IdGeneratorService

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Suite de tests complète pour DependanceBuilder Version 2.0 "Graph First".
    /// 
    /// ORGANISATION DES TESTS :
    /// - Tests de base et validation
    /// - Tests des 4 états de dépendances
    /// - Tests de suggestions métier (règle de base)
    /// - Tests de remontée de chaîne
    /// - Tests des fins de chaîne (cas particuliers 1 & 2)
    /// - Tests anti-cycles robustes
    /// - Tests d'intégration réalistes
    /// </summary>
    [TestClass]
    public class DependanceBuilderTests
    {
        private ProjetService _projetService;
        private DependanceBuilder _dependanceBuilder;

        [TestInitialize]
        public void Setup()
        {
            // Initialiser les dépendances de ProjetService et des autres services pour les tests.
            // Utiliser des stubs ou des instances simples pour l'isolement du test unitaire.

            // Déclaration anticipée des services qui ont des dépendances mutuelles ou circulaires
            // pour pouvoir les référencer dans les délégués Func<T> (factories)
            ProjetService projetServiceInstance = null;
            TacheService tacheServiceInstance = null;
            BlocService blocServiceInstance = null;

            // Instanciation des services "feuilles" ou de base
            var csvDataService = new CsvDataService();
            var excelReader = new ExcelReader();
            var ouvrierService = new OuvrierService(csvDataService, excelReader);
            var lotService = new LotService();

            // 1. Créer les Factories (Func<T>) qui captureront les instances une fois qu'elles existeront.
            Func<ProjetService> projetServiceFactory = () => projetServiceInstance;
            Func<TacheService> tacheServiceFactory = () => tacheServiceInstance;
            Func<BlocService> blocServiceFactory = () => blocServiceInstance;

            // 2. Instancier les services dans un ordre qui permet de briser les cycles.
            // On peut commencer par BlocService car sa dépendance Func<TacheService> ne s'active qu'à l'appel.
            blocServiceInstance = new BlocService(tacheServiceFactory);

            // Ensuite TacheService, qui a besoin des factories de ProjetService et BlocService.
            tacheServiceInstance = new TacheService(
                csvDataService,
                excelReader,
                projetServiceFactory, // Factory vers ProjetService
                lotService,
                blocServiceFactory // Factory vers BlocService
            );

            // Enfin ProjetService, qui a besoin des factories de TacheService et BlocService.
            projetServiceInstance = new ProjetService(
                ouvrierService,
                tacheServiceFactory, // Factory vers TacheService
                csvDataService,
                lotService,
                blocServiceFactory // Factory vers BlocService
            );

            // Assigner l'instance de ProjetService à la variable de classe de test
            _projetService = projetServiceInstance;

            // Le DependanceBuilder dépend de ProjetService directement
            _dependanceBuilder = new DependanceBuilder(_projetService);

            // Initialiser les métiers pour les tests qui en ont besoin
            // (La logique de setup spécifique aux métiers est déjà dans les tests individuels via _projetService.RemplacerTousLesMetiers)
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
                _dependanceBuilder.ObtenirDependancesPourTache(null, contexteTaches));
        }

        [TestMethod]
        [TestCategory("Unit - Validation")]
        public void ObtenirDependancesPourTache_AvecContexteNull_DoitLeverArgumentNullException()
        {
            var tache = new Tache { TacheId = "T001", BlocId = "B001" };
            Assert.ThrowsException<ArgumentNullException>(() =>
                _dependanceBuilder.ObtenirDependancesPourTache(tache, null));
        }

        [TestMethod]
        [TestCategory("Unit - Robustesse")]
        public void ObtenirDependancesPourTache_AvecContexteVide_DoitRetournerListeVide()
        {
            var tache = new Tache { TacheId = "T001", BlocId = "B001" };
            var contexteVide = new List<Tache>();

            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(tache, contexteVide);

            Assert.AreEqual(0, resultats.Count, "Un contexte vide doit retourner une liste vide");
        }

        #endregion

        #region Tests des 4 États de Dépendances

        [TestMethod]
        [TestCategory("Functional - États")]
        public void ObtenirDependancesPourTache_AvecDependanceStricte_DoitMarquerCommeStricte()
        {
            // Arrange
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", TacheNom = "Tâche 1", BlocId = "B001" },
                new Tache { TacheId = "T002", TacheNom = "Tâche 2", BlocId = "B001", Dependencies = "T001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T002"), taches);

            // Assert
            var dependanceStricte = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(dependanceStricte, "T001 doit apparaître dans les résultats");
            Assert.AreEqual(EtatDependance.Stricte, dependanceStricte.Etat, "T001 doit être Stricte (noir, coché)");
            Assert.IsFalse(dependanceStricte.EstHeritee, "Une dépendance stricte n'est pas héritée");
        }

        [TestMethod]
        [TestCategory("Functional - États")]
        public void ObtenirDependancesPourTache_AvecDependanceExclue_DoitMarquerCommeExclue()
        {
            // Arrange
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M2", BlocId = "B001",
                    Dependencies = "", ExclusionsDependances = "T001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T002"), taches);

            // Assert
            var dependanceExclue = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(dependanceExclue, "T001 doit apparaître pour montrer l'exclusion");
            Assert.AreEqual(EtatDependance.Exclue, dependanceExclue.Etat, "T001 doit être Exclue (rouge, décoché)");
            Assert.IsTrue(dependanceExclue.EstHeritee, "Une exclusion est héritée (était une suggestion rejetée)");
        }

        [TestMethod]
        [TestCategory("Functional - États")]
        public void ObtenirDependancesPourTache_AvecDependanceSuggeree_DoitMarquerCommeSuggeree()
        {
            // Arrange
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M2", BlocId = "B001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T002"), taches);

            // Assert
            var dependanceSuggeree = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(dependanceSuggeree, "T001 doit être suggérée car M2 dépend de M1");
            Assert.AreEqual(EtatDependance.Suggeree, dependanceSuggeree.Etat, "T001 doit être Suggérée (bleu, coché)");
            Assert.IsTrue(dependanceSuggeree.EstHeritee, "Une suggestion est héritée des règles métier");
        }

        [TestMethod]
        [TestCategory("Functional - États")]
        public void ObtenirDependancesPourTache_AvecDependanceNeutre_DoitMarquerCommeNeutre()
        {
            // Arrange
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M_INEXISTANT", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M_INEXISTANT", BlocId = "B001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T002"), taches);

            // Assert
            var dependanceNeutre = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(dependanceNeutre, "T001 doit apparaître");
            Assert.AreEqual(EtatDependance.Neutre, dependanceNeutre.Etat, "T001 doit être Neutre (normal, décoché)");
            Assert.IsFalse(dependanceNeutre.EstHeritee, "Une dépendance neutre n'est pas héritée");
        }

        #endregion

        #region Tests Suggestions Métier - Règle de Base

        [TestMethod]
        [TestCategory("Functional - Suggestions")]
        public void ObtenirDependancesPourTache_RegleDeBase_M2DependDeM1()
        {
            // Arrange - Règle de base : M2 a pour prérequis M1
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Maçonnerie", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Plaquerie", PrerequisMetierIds = "M1" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", TacheNom = "Maçonnerie", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", TacheNom = "Plaquerie", MetierId = "M2", BlocId = "B001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T002"), taches);

            // Assert
            var suggestion = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(suggestion, "T001 (M1) doit être suggérée pour T002 (M2)");
            Assert.AreEqual(EtatDependance.Suggeree, suggestion.Etat, "T001 doit être suggérée");
            Assert.IsTrue(suggestion.EstHeritee, "La suggestion provient des règles métier");
        }

        [TestMethod]
        [TestCategory("Functional - Suggestions")]
        public void ObtenirDependancesPourTache_PourJalon_AucuneSuggestionMetier()
        {
            // Arrange
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "J001", Type = TypeActivite.JalonUtilisateur, BlocId = "B001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "J001"), taches);

            // Assert
            var suggestionT001 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(suggestionT001, "T001 doit apparaître dans la liste");
            Assert.AreEqual(EtatDependance.Neutre, suggestionT001.Etat,
                "T001 doit être Neutre car les jalons n'ont pas de suggestions métier");
        }

        #endregion

        #region Tests Remontée de Chaîne

        [TestMethod]
        [TestCategory("Functional - Remontée")]
        public void ObtenirDependancesPourTache_RemonteeDeChaine_MetierIntermediaireAbsent()
        {
            // Arrange - Chaîne : M3 → M2 → M1, mais M2 absent du bloc
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Maçonnerie", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Huisserie", PrerequisMetierIds = "M1" },
                new Metier { MetierId = "M3", Nom = "Plaquerie", PrerequisMetierIds = "M2" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", TacheNom = "Maçonnerie", MetierId = "M1", BlocId = "B001" },
                // M2 (Huisserie) absent du bloc
                new Tache { TacheId = "T003", TacheNom = "Plaquerie", MetierId = "M3", BlocId = "B001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T003"), taches);

            // Assert
            var suggestion = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(suggestion, "T001 (M1) doit être suggérée via remontée de chaîne");
            Assert.AreEqual(EtatDependance.Suggeree, suggestion.Etat,
                "T001 doit être suggérée car M3→M2→M1 et M2 absent → M3 suggère M1");
        }

        [TestMethod]
        [TestCategory("Functional - Remontée")]
        public void ObtenirDependancesPourTache_RemonteeDeChaine_ChaineLongue()
        {
            // Arrange - Chaîne longue : M4 → M3 → M2 → M1, seuls M1 et M4 présents
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Préparation", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Maçonnerie", PrerequisMetierIds = "M1" },
                new Metier { MetierId = "M3", Nom = "Huisserie", PrerequisMetierIds = "M2" },
                new Metier { MetierId = "M4", Nom = "Plaquerie", PrerequisMetierIds = "M3" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                // M2 et M3 absents du bloc
                new Tache { TacheId = "T004", MetierId = "M4", BlocId = "B001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T004"), taches);

            // Assert
            var suggestion = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            Assert.IsNotNull(suggestion, "T001 (M1) doit être suggérée via remontée longue");
            Assert.AreEqual(EtatDependance.Suggeree, suggestion.Etat,
                "T001 doit être suggérée car M4→M3→M2→M1 avec M2,M3 absents");
        }

        #endregion

        #region Tests Fins de Chaîne - Cas Particuliers 1 & 2

        [TestMethod]
        [TestCategory("Functional - Fins de Chaîne")]
        public void ObtenirDependancesPourTache_CasParticulier1_SeuleFinDeChaineSuggeree()
        {
            // Arrange - Cas particulier 1 : chaîne intra-métier T3→T2→T1, seule T1 (fin) suggérée
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Maçonnerie", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Plaquerie", PrerequisMetierIds = "M1" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                // Chaîne intra-métier M1 : T3→T2→T1
                new Tache { TacheId = "T001", TacheNom = "Maçon fin", MetierId = "M1", BlocId = "B001", Dependencies = "" },
                new Tache { TacheId = "T002", TacheNom = "Maçon milieu", MetierId = "M1", BlocId = "B001", Dependencies = "T001" },
                new Tache { TacheId = "T003", TacheNom = "Maçon début", MetierId = "M1", BlocId = "B001", Dependencies = "T002" },
                // Tâche qui va recevoir les suggestions
                new Tache { TacheId = "T004", TacheNom = "Plaquerie", MetierId = "M2", BlocId = "B001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T004"), taches);

            // Assert
            var suggestionT001 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            var suggestionT002 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T002");
            var suggestionT003 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T003");

            Assert.IsNotNull(suggestionT001, "T001 doit être dans la liste");
            Assert.IsNotNull(suggestionT002, "T002 doit être dans la liste");
            Assert.IsNotNull(suggestionT003, "T003 doit être dans la liste");

            // CAS PARTICULIER 1 : Seule la fin de chaîne (T001) est suggérée
            Assert.AreEqual(EtatDependance.Neutre, suggestionT001.Etat,
                "T001 (début de chaîne) doit être Neutre");
            Assert.AreEqual(EtatDependance.Neutre, suggestionT002.Etat,
                "T002 (milieu de chaîne) doit être Neutre");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionT003.Etat,
                "T003 (vraie fin de chaîne) doit être Suggérée");
        }

        [TestMethod]
        [TestCategory("Functional - Fins de Chaîne")]
        public void ObtenirDependancesPourTache_CasParticulier2_FinDeChaneExclue_SuggererSuivante()
        {
            // Arrange - Cas particulier 2 : T001 exclue → T002 devient suggérée
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Maçonnerie", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Plaquerie", PrerequisMetierIds = "M1" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                // Chaîne intra-métier M1 : T3→T2→T1
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001", Dependencies = "" },
                new Tache { TacheId = "T002", MetierId = "M1", BlocId = "B001", Dependencies = "T001" },
                new Tache { TacheId = "T003", MetierId = "M1", BlocId = "B001", Dependencies = "T002" },
                // Tâche qui exclut T003 et va recevoir les suggestions
                new Tache { TacheId = "T004", MetierId = "M2", BlocId = "B001",
                    Dependencies = "", ExclusionsDependances = "T003" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T004"), taches);

            // Assert
            var suggestionT001 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");
            var suggestionT002 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T002");
            var suggestionT003 = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T003");

            // CAS PARTICULIER 2 : T003 exclue → T002 devient suggérée
            Assert.AreEqual(EtatDependance.Neutre, suggestionT001.Etat,
                "T001 (début de chaîne) doit être Neutre");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionT002.Etat,
                "T002 doit devenir Suggérée car T003 (fin normale) est exclue");
            Assert.AreEqual(EtatDependance.Exclue, suggestionT003.Etat,
                "T003 doit être Exclue (utilisateur l'a rejetée)");
        }

        #endregion

        #region Tests Anti-Cycles Robustes

        [TestMethod]
        [TestCategory("Functional - Anti-Cycles")]
        public void ObtenirDependancesPourTache_AvecCyclePotentiel_DoitExclureCandidatCyclique()
        {
            // Arrange - Scénario où T003 dépend déjà de T002
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", BlocId = "B001", Dependencies = "" },
                new Tache { TacheId = "T002", BlocId = "B001", Dependencies = "T001" },
                new Tache { TacheId = "T003", BlocId = "B001", Dependencies = "T002" }
            };

            // Act - Pour T002, T003 ne doit pas être proposée car T003→T002 créerait un cycle
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T002"), taches);

            // Assert
            var candidatIds = resultats.Select(r => r.TachePredecesseur.TacheId).ToList();
            Assert.IsTrue(candidatIds.Contains("T001"), "T001 doit être proposée (pas de cycle)");
            Assert.IsFalse(candidatIds.Contains("T003"),
                "T003 ne doit PAS être proposée car T003→T002 existe déjà (créerait un cycle)");
        }

        [TestMethod]
        [TestCategory("Functional - Anti-Cycles")]
        public void ObtenirDependancesPourTache_AvecCycleTransitif_DoitExclureCandidatCyclique()
        {
            // Arrange - Cycle transitif : T004→T003→T002
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", BlocId = "B001", Dependencies = "" },
                new Tache { TacheId = "T002", BlocId = "B001", Dependencies = "T001" },
                new Tache { TacheId = "T003", BlocId = "B001", Dependencies = "T002" },
                new Tache { TacheId = "T004", BlocId = "B001", Dependencies = "T003" }
            };

            // Act - Pour T002, ni T003 ni T004 ne doivent être proposées
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T002"), taches);

            // Assert
            var candidatIds = resultats.Select(r => r.TachePredecesseur.TacheId).ToList();
            Assert.IsTrue(candidatIds.Contains("T001"), "T001 doit être proposée");
            Assert.IsFalse(candidatIds.Contains("T003"), "T003 ne doit pas être proposée (cycle direct)");
            Assert.IsFalse(candidatIds.Contains("T004"), "T004 ne doit pas être proposée (cycle transitif)");
        }

        [TestMethod]
        [TestCategory("Functional - Anti-Cycles")]
        public void ObtenirDependancesPourTache_SansCycle_DoitProposerTousCandidats()
        {
            // Arrange - Pas de cycles, toutes les tâches doivent être proposées
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", BlocId = "B001", Dependencies = "" },
                new Tache { TacheId = "T002", BlocId = "B001", Dependencies = "" },
                new Tache { TacheId = "T003", BlocId = "B001", Dependencies = "" },
                new Tache { TacheId = "T004", BlocId = "B001", Dependencies = "" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T004"), taches);

            // Assert
            Assert.AreEqual(3, resultats.Count, "Les 3 autres tâches doivent être proposées");
            var candidatIds = resultats.Select(r => r.TachePredecesseur.TacheId).ToList();
            Assert.IsTrue(candidatIds.Contains("T001"), "T001 doit être proposée");
            Assert.IsTrue(candidatIds.Contains("T002"), "T002 doit être proposée");
            Assert.IsTrue(candidatIds.Contains("T003"), "T003 doit être proposée");
        }

        #endregion

        #region Tests Gestion des Jalons

        [TestMethod]
        [TestCategory("Functional - Jalons")]
        public void ObtenirDependancesPourTache_AvecJalonSuccesseur_DoitPrivilegierJalon()
        {
            // Arrange
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001", Dependencies = "" },
                new Tache { TacheId = "J001", Type = TypeActivite.JalonUtilisateur, BlocId = "B001", Dependencies = "T001" },
                new Tache { TacheId = "T002", MetierId = "M2", BlocId = "B001", Dependencies = "" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T002"), taches);

            // Assert
            var suggestionJalon = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "J001");
            var suggestionTache = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T001");

            Assert.IsNotNull(suggestionJalon, "Le jalon J001 doit être dans la liste");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionJalon.Etat, "Le jalon J001 doit être privilégié et suggéré");

            if (suggestionTache != null)
            {
                Assert.AreEqual(EtatDependance.Neutre, suggestionTache.Etat,
                    "T001 doit être Neutre car le jalon J001 est privilégié");
            }
        }

        [TestMethod]
        [TestCategory("Functional - Jalons")]
        public void ObtenirDependancesPourTache_AvecPlusieursJalons_DoitSuggererJalonsRelevants()
        {
            // Arrange
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "J_Relevant", Type = TypeActivite.JalonDeSynchronisation, BlocId = "B001", Dependencies = "T001,T002" },
                new Tache { TacheId = "J_NonRelevant", Type = TypeActivite.JalonUtilisateur, BlocId = "B001", Dependencies = "T999" },
                new Tache { TacheId = "T003", MetierId = "M2", BlocId = "B001" }
            };

            // Act
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T003"), taches);

            // Assert
            var suggestionJalonRelevant = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "J_Relevant");
            var suggestionJalonNonRelevant = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "J_NonRelevant");

            Assert.IsNotNull(suggestionJalonRelevant, "J_Relevant doit être suggéré car il dépend de tâches M1");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionJalonRelevant.Etat, "J_Relevant doit être suggéré");

            if (suggestionJalonNonRelevant != null)
            {
                Assert.AreEqual(EtatDependance.Neutre, suggestionJalonNonRelevant.Etat,
                    "J_NonRelevant doit être neutre car il ne dépend pas de tâches M1");
            }
        }

        #endregion

        #region Tests d'Intégration Réalistes

        [TestMethod]
        [TestCategory("Integration - Scénario Réel")]
        public void ObtenirDependancesPourTache_ScenarioCompletBatiment_DoitFonctionnerCorrectement()
        {
            // Arrange - Reproduction d'un scénario réaliste de construction
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "PREPT_01", Nom = "Preparation chantier", PrerequisMetierIds = "" },
                new Metier { MetierId = "MACON_01", Nom = "Maconnerie", PrerequisMetierIds = "PREPT_01" },
                new Metier { MetierId = "BOISPoFe", Nom = "Huisseries", PrerequisMetierIds = "MACON_01" },
                new Metier { MetierId = "PLOMB_01", Nom = "Plomberie", PrerequisMetierIds = "MACON_01" },
                new Metier { MetierId = "ELECT_01", Nom = "Electricite Special", PrerequisMetierIds = "MACON_01" },
                new Metier { MetierId = "PLAQUIST", Nom = "Plaquerie", PrerequisMetierIds = "BOISPoFe,ELECT_01,PLOMB_01" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", TacheNom = "Preparation chantier",
                    BlocId = "B001", MetierId = "PREPT_01", Dependencies = "" },
                new Tache { TacheId = "T002", TacheNom = "Maconnerie",
                    BlocId = "B001", MetierId = "MACON_01", Dependencies = "T001" },
                new Tache { TacheId = "J_Sync_MACON", TacheNom = "Synchro Fin Maçonnerie",
                    BlocId = "B001", Type = TypeActivite.JalonDeSynchronisation, Dependencies = "T002" },
                new Tache { TacheId = "T003", TacheNom = "Pose Portes",
                    BlocId = "B001", MetierId = "BOISPoFe", Dependencies = "J_Sync_MACON" },
                new Tache { TacheId = "T004", TacheNom = "Electricité",
                    BlocId = "B001", MetierId = "ELECT_01", Dependencies = "J_Sync_MACON" },
                new Tache { TacheId = "T005", TacheNom = "Plomberie",
                    BlocId = "B001", MetierId = "PLOMB_01", Dependencies = "J_Sync_MACON" }
            };

            // Act - Test pour nouvelle tâche PLAQUIST
            var nouvelleTachePlaquist = new Tache
            {
                TacheId = "T006",
                BlocId = "B001",
                MetierId = "PLAQUIST",
                Dependencies = "",
                ExclusionsDependances = ""
            };

            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(nouvelleTachePlaquist, taches);

            // Assert
            Assert.IsTrue(resultats.Count > 0, "Des dépendances doivent être proposées pour PLAQUIST");

            // Vérifier que les tâches des métiers prérequis sont suggérées
            var suggestionBois = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T003");
            var suggestionElec = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T004");
            var suggestionPlomb = resultats.FirstOrDefault(r => r.TachePredecesseur.TacheId == "T005");

            Assert.IsNotNull(suggestionBois, "T003 (BOISPoFe) doit être suggérée");
            Assert.IsNotNull(suggestionElec, "T004 (ELECT_01) doit être suggérée");
            Assert.IsNotNull(suggestionPlomb, "T005 (PLOMB_01) doit être suggérée");

            Assert.AreEqual(EtatDependance.Suggeree, suggestionBois.Etat, "T003 doit être suggérée");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionElec.Etat, "T004 doit être suggérée");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionPlomb.Etat, "T005 doit être suggérée");

            // Vérifier qu'aucun cycle n'est créé
            var tacheIds = resultats.Select(r => r.TachePredecesseur.TacheId).ToList();
            Assert.IsFalse(tacheIds.Contains("T006"), "La tâche elle-même ne doit pas être dans la liste");
        }

        [TestMethod]
        [TestCategory("Integration - Performance")]
        public void ObtenirDependancesPourTache_AvecGrandNombreTaches_DoitEtrePerformant()
        {
            // Arrange - 50 tâches (cas nominal)
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>();
            for (int i = 1; i <= 50; i++)
            {
                taches.Add(new Tache
                {
                    TacheId = $"T{i:D3}",
                    TacheNom = $"Tâche {i}",
                    BlocId = "B001",
                    MetierId = i <= 25 ? "M1" : "M2",
                    Dependencies = i > 1 ? $"T{i - 1:D3}" : ""
                });
            }

            var tacheCible = taches.Last();

            // Act & Assert - Doit s'exécuter rapidement
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(tacheCible, taches);
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
                "L'opération doit s'exécuter en moins de 500ms pour 50 tâches");
            Assert.IsTrue(resultats.Count > 0, "Des suggestions doivent être générées");
        }

        [TestMethod]
        [TestCategory("Integration - Robustesse")]
        public void ObtenirDependancesPourTache_AvecDonneesInconsistantes_DoitGererGracieusement()
        {
            // Arrange - Données avec inconsistances (métiers inexistants, dépendances cassées)
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" }
            };
            _projetService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", MetierId = "M1", BlocId = "B001" },
                new Tache { TacheId = "T002", MetierId = "M_INEXISTANT", BlocId = "B001" },
                new Tache { TacheId = "T003", MetierId = "M1", BlocId = "B001", Dependencies = "T_INEXISTANTE" },
                new Tache { TacheId = "T004", MetierId = "", BlocId = "B001", Dependencies = "T001" }
            };

            // Act - Ne doit pas lever d'exception
            var resultats = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T004"), taches);

            // Assert - Doit retourner un résultat valide malgré les inconsistances
            Assert.IsNotNull(resultats, "Le résultat ne doit jamais être null");
            Assert.IsTrue(resultats.Count >= 0, "Le résultat doit être une liste valide");
        }

        #endregion

        #region Tests de Cas Limites


        [TestMethod]
        [TestCategory("Unit - Cas Limites")]
        public void ObtenirDependancesPourTache_AvecDependancesMalformees_DoitParserCorrectement()
        {
            // Arrange - Dépendances avec espaces, virgules multiples, etc.
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T001", BlocId = "B001" },
                new Tache { TacheId = "T002", BlocId = "B001" },
                new Tache { TacheId = "T003", BlocId = "B001", Dependencies = " T001 , T002 ,, " },
                new Tache { TacheId = "T004", BlocId = "B001", ExclusionsDependances = "T001,  , T002" }
            };

            // Act
            var resultats1 = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T003"), taches);
            var resultats2 = _dependanceBuilder.ObtenirDependancesPourTache(
                taches.First(t => t.TacheId == "T004"), taches);

            // Assert - Parsing robuste
            var strictes = resultats1.Where(r => r.Etat == EtatDependance.Stricte).Select(r => r.TachePredecesseur.TacheId).ToList();
            Assert.IsTrue(strictes.Contains("T001"), "T001 doit être parsée correctement");
            Assert.IsTrue(strictes.Contains("T002"), "T002 doit être parsée correctement");
            Assert.AreEqual(2, strictes.Count, "Exactement 2 dépendances strictes");

            var excluses = resultats2.Where(r => r.Etat == EtatDependance.Exclue).Select(r => r.TachePredecesseur.TacheId).ToList();
            Assert.IsTrue(excluses.Contains("T001"), "T001 doit être parsée comme exclue");
            Assert.IsTrue(excluses.Contains("T002"), "T002 doit être parsée comme exclue");
        }

        #endregion
    }
}