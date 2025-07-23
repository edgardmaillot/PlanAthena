using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Tests fonctionnels pour valider la simplification transitive des dépendances.
    /// 
    /// RÈGLE TESTÉE : Règle N°4 - La Simplification par Réduction Transitive (Anti-Redondance)
    /// Si T3 dépend de T2 et T2 dépend de T1, un lien direct T3 -> T1 est redondant et doit être supprimé.
    /// 
    /// OBJECTIF : S'assurer que l'algorithme QuikGraph TransitiveReduction élimine correctement
    /// les dépendances redondantes pour produire un graphe minimal et lisible.
    /// </summary>
    [TestClass]
    public class DependanceBuilderSimplificationTransitiveTests
    {
        private MetierService _metierService;
        private DependanceBuilder _dependanceBuilder;

        [TestInitialize]
        public void Setup()
        {
            _metierService = new MetierService();
            _dependanceBuilder = new DependanceBuilder(_metierService);

            // Configuration des métiers en chaîne : M3 -> M2 -> M1
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" },
                new Metier { MetierId = "M3", Nom = "Métier 3", PrerequisMetierIds = "M2" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);
        }

        /// <summary>
        /// Scénario 2 : Test de la Simplification Transitive (Anti-Spaghetti)
        /// 
        /// CONTEXTE (Given) : Dans un même bloc, on a Tache_A1 (Métier M1), Tache_B2 (Métier M2), 
        /// et Tache_C3 (Métier M3). Les prérequis sont M3 -> M2 et M2 -> M1. 
        /// Aucune dépendance manuelle n'existe.
        /// 
        /// ACTION (When) : L'utilisateur clique sur "Mapping Auto".
        /// 
        /// RÉSULTAT ATTENDU (Then) : Tache_C3 doit dépendre de Tache_B2. Tache_B2 doit dépendre de Tache_A1. 
        /// Il ne doit y avoir aucun lien direct entre Tache_C3 et Tache_A1.
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 4")]
        public void AppliquerMappingAuto_AvecChaineDependancesMetier_DoitSupprimerLiensRedondants()
        {
            // === ARRANGE (Préparation) ===
            var taches = new List<Tache>
            {
                // Tâche du métier M1 (base de la chaîne)
                new Tache
                {
                    TacheId = "Tache_A1",
                    TacheNom = "Tâche A1",
                    MetierId = "M1",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },
                
                // Tâche du métier M2 (milieu de la chaîne)
                new Tache
                {
                    TacheId = "Tache_B2",
                    TacheNom = "Tâche B2",
                    MetierId = "M2",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },
                
                // Tâche du métier M3 (fin de la chaîne)
                new Tache
                {
                    TacheId = "Tache_C3",
                    TacheNom = "Tâche C3",
                    MetierId = "M3",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = ""
                }
            };

            // === ACT (Action) ===
            // Simulation du clic "Mapping Auto"
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT (Vérifications) ===
            var tacheA1 = taches.First(t => t.TacheId == "Tache_A1");
            var tacheB2 = taches.First(t => t.TacheId == "Tache_B2");
            var tacheC3 = taches.First(t => t.TacheId == "Tache_C3");

            // VÉRIFICATION 1 : Tache_A1 (M1) ne doit avoir aucune dépendance
            Assert.IsTrue(string.IsNullOrEmpty(tacheA1.Dependencies),
                "Tache_A1 est à la base de la chaîne métier et ne doit avoir aucune dépendance.");

            // VÉRIFICATION 2 : Tache_B2 (M2) doit dépendre uniquement de Tache_A1 (M1)
            Assert.IsFalse(string.IsNullOrEmpty(tacheB2.Dependencies),
                "Tache_B2 doit avoir des dépendances car M2 dépend de M1.");

            Assert.AreEqual("Tache_A1", tacheB2.Dependencies.Trim(),
                "Tache_B2 doit dépendre uniquement de Tache_A1.");

            // VÉRIFICATION 3 : Tache_C3 (M3) doit dépendre uniquement de Tache_B2 (M2)
            Assert.IsFalse(string.IsNullOrEmpty(tacheC3.Dependencies),
                "Tache_C3 doit avoir des dépendances car M3 dépend de M2.");

            Assert.AreEqual("Tache_B2", tacheC3.Dependencies.Trim(),
                "Tache_C3 doit dépendre uniquement de Tache_B2.");

            // VÉRIFICATION CRITIQUE : Pas de lien direct C3 -> A1 (c'est l'anti-redondance)
            Assert.IsFalse(tacheC3.Dependencies.Contains("Tache_A1"),
                "RÈGLE 4 VIOLÉE : Tache_C3 ne doit PAS avoir de lien direct vers Tache_A1. " +
                "Ce lien est redondant car C3 -> B2 -> A1 existe déjà.");
        }

        /// <summary>
        /// Test complémentaire : Vérification avec une chaîne plus longue (4 niveaux)
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 4")]
        public void AppliquerMappingAuto_AvecChaineLongue_DoitCreerGrapheMinimal()
        {
            // === ARRANGE ===
            // Ajout d'un 4ème métier pour une chaîne plus longue
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" },
                new Metier { MetierId = "M3", Nom = "Métier 3", PrerequisMetierIds = "M2" },
                new Metier { MetierId = "M4", Nom = "Métier 4", PrerequisMetierIds = "M3" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                new Tache { TacheId = "T1", MetierId = "M1", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T2", MetierId = "M2", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T3", MetierId = "M3", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T4", MetierId = "M4", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var t1 = taches.First(t => t.TacheId == "T1");
            var t2 = taches.First(t => t.TacheId == "T2");
            var t3 = taches.First(t => t.TacheId == "T3");
            var t4 = taches.First(t => t.TacheId == "T4");

            // Vérifications de la chaîne minimale : T4 -> T3 -> T2 -> T1
            Assert.IsTrue(string.IsNullOrEmpty(t1.Dependencies), "T1 ne doit avoir aucune dépendance.");
            Assert.AreEqual("T1", t2.Dependencies.Trim(), "T2 doit dépendre uniquement de T1.");
            Assert.AreEqual("T2", t3.Dependencies.Trim(), "T3 doit dépendre uniquement de T2.");
            Assert.AreEqual("T3", t4.Dependencies.Trim(), "T4 doit dépendre uniquement de T3.");

            // Vérifications anti-redondance critiques
            Assert.IsFalse(t3.Dependencies.Contains("T1"), "T3 ne doit PAS dépendre directement de T1.");
            Assert.IsFalse(t4.Dependencies.Contains("T1"), "T4 ne doit PAS dépendre directement de T1.");
            Assert.IsFalse(t4.Dependencies.Contains("T2"), "T4 ne doit PAS dépendre directement de T2.");
        }

        /// <summary>
        /// Test avec dépendances manuelles : S'assurer que la réduction transitive respecte 
        /// les choix manuels de l'utilisateur (Règle 2 + Règle 4)
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 4")]
        public void AppliquerMappingAuto_AvecDependancesManuelles_DoitPreserverChoixUtilisateur()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                new Tache
                {
                    TacheId = "Tache_A1",
                    MetierId = "M1",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = ""  // Pas de dépendance
                },

                new Tache
                {
                    TacheId = "Tache_B2",
                    MetierId = "M2",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "Tache_A1"  // Dépendance manuelle conforme aux métiers
                },

                new Tache
                {
                    TacheId = "Tache_C3",
                    MetierId = "M3",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "Tache_B2,Tache_A1"  // Dépendances manuelles : une redondante + une normale
                }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var tacheC3 = taches.First(t => t.TacheId == "Tache_C3");

            // La réduction transitive doit supprimer la dépendance redondante Tache_C3 -> Tache_A1
            // car Tache_C3 -> Tache_B2 -> Tache_A1 existe déjà
            Assert.AreEqual("Tache_B2", tacheC3.Dependencies.Trim(),
                "Après réduction transitive, Tache_C3 doit dépendre uniquement de Tache_B2. " +
                "La dépendance directe vers Tache_A1 doit être supprimée car elle est redondante.");
        }

        /// <summary>
        /// Test de cas limite : Métier intermédiaire absent avec dépendances manuelles intra-métier
        /// 
        /// CONTEXTE : Un métier M3 a pour prérequis M2, qui a pour prérequis M1.
        /// Nous avons deux tâches (T1 et T2) appartenant au métier M3, aucune tâche appartenant 
        /// au métier M2 et une tâche appartenant au métier M1 (T5).
        /// Les seules dépendances directes sont : T1 dépend de T2. T5 n'a aucune dépendance.
        /// 
        /// RÉSULTAT ATTENDU : T2 doit dépendre de T5 (remontée de M3->M2->M1), mais T1 ne doit 
        /// pas dépendre directement de T5 car elle dépend déjà de T2 (réduction transitive).
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 4")]
        public void AppliquerMappingAuto_AvecMetierIntermediaireAbsent_DoitCreerChaineOptimale()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                // Tâche du métier M1 (base de la chaîne métier)
                new Tache
                {
                    TacheId = "T5",
                    TacheNom = "Tâche T5",
                    MetierId = "M1",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",  // Aucune dépendance
                    ExclusionsDependances = ""
                },
                
                // Première tâche du métier M3 avec dépendance manuelle intra-métier
                new Tache
                {
                    TacheId = "T1",
                    TacheNom = "Tâche T1",
                    MetierId = "M3",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "T2",  // DÉPENDANCE MANUELLE INTRA-MÉTIER
                    ExclusionsDependances = ""
                },
                
                // Deuxième tâche du métier M3
                new Tache
                {
                    TacheId = "T2",
                    TacheNom = "Tâche T2",
                    MetierId = "M3",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",  // Aucune dépendance initialement
                    ExclusionsDependances = ""
                }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var t1 = taches.First(t => t.TacheId == "T1");
            var t2 = taches.First(t => t.TacheId == "T2");
            var t5 = taches.First(t => t.TacheId == "T5");

            // VÉRIFICATION 1 : T5 ne doit avoir aucune dépendance (base de la chaîne)
            Assert.IsTrue(string.IsNullOrEmpty(t5.Dependencies),
                "T5 (métier M1) est à la base de la chaîne métier et ne doit avoir aucune dépendance.");

            // VÉRIFICATION 2 : T2 doit dépendre de T5 (remontée métier M3->M2->M1)
            Assert.IsTrue(t2.Dependencies.Contains("T5"),
                "T2 doit dépendre de T5 car le métier M3 a pour prérequis transitif M1, " +
                "et M2 n'est pas présent dans le bloc (remontée de la chaîne métier).");

            // VÉRIFICATION 3 : T1 doit dépendre uniquement de T2 (réduction transitive)
            Assert.AreEqual("T2", t1.Dependencies.Trim(),
                "T1 doit dépendre uniquement de T2. La dépendance vers T5 ne doit pas être créée " +
                "car elle serait redondante : T1 -> T2 -> T5 couvre déjà le prérequis métier.");

            // VÉRIFICATION CRITIQUE : T1 ne doit PAS dépendre directement de T5
            Assert.IsFalse(t1.Dependencies.Contains("T5"),
                "RÉDUCTION TRANSITIVE : T1 ne doit PAS dépendre directement de T5 " +
                "car ce lien est redondant avec T1 -> T2 -> T5.");
        }
    }
}