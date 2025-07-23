using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Tests fonctionnels pour valider la factorisation par jalons de synchronisation.
    /// 
    /// RÈGLE TESTÉE : Règle N°5 - La Factorisation par Jalons de Synchronisation (Anti-Croisements Multiples)
    /// Pour simplifier le graphe, les dépendances croisées multiples doivent être remplacées par un jalon.
    /// 
    /// OBJECTIF : S'assurer que l'algorithme détecte les motifs "many-to-many" et les simplifie
    /// en créant des jalons de synchronisation automatiques, tout en préservant les jalons existants pertinents.
    /// </summary>
    [TestClass]
    public class DependanceBuilderFactorisationJalonTests
    {
        private MetierService _metierService;
        private DependanceBuilder _dependanceBuilder;

        [TestInitialize]
        public void Setup()
        {
            _metierService = new MetierService();
            _dependanceBuilder = new DependanceBuilder(_metierService);

            // Configuration des métiers : M3 dépend de M1
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M3", Nom = "Métier 3", PrerequisMetierIds = "M1" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);
        }

        /// <summary>
        /// Scénario 3 : Test de la Factorisation par Jalon (Anti-Croisements)
        /// 
        /// CONTEXTE (Given) : Dans un même bloc, Tache_C1 et Tache_C2 (Métier M3) n'ont pas de dépendances. 
        /// Tache_A et Tache_B (Métier M1) n'ont pas de dépendances. Le métier M3 a pour prérequis M1.
        /// 
        /// ACTION (When) : L'utilisateur clique sur "Mapping Auto".
        /// 
        /// RÉSULTAT ATTENDU (Then) :
        /// 1. Un nouveau jalon (J_Sync_...) est créé.
        /// 2. Ce jalon dépend de Tache_A et Tache_B.
        /// 3. Tache_C1 et Tache_C2 dépendent uniquement de ce nouveau jalon. 
        ///    Elles n'ont plus de dépendances directes sur Tache_A ou Tache_B.
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 5")]
        public void AppliquerMappingAuto_AvecCroisementsMultiples_DoitCreerJalonSynchronisation()
        {
            // === ARRANGE (Préparation) ===
            var taches = new List<Tache>
            {
                // Tâches du métier M1 (prérequis)
                new Tache
                {
                    TacheId = "Tache_A",
                    TacheNom = "Tâche A",
                    MetierId = "M1",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },

                new Tache
                {
                    TacheId = "Tache_B",
                    TacheNom = "Tâche B",
                    MetierId = "M1",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },
                
                // Tâches du métier M3 (qui vont créer des croisements)
                new Tache
                {
                    TacheId = "Tache_C1",
                    TacheNom = "Tâche C1",
                    MetierId = "M3",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },

                new Tache
                {
                    TacheId = "Tache_C2",
                    TacheNom = "Tâche C2",
                    MetierId = "M3",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = ""
                }
            };

            var nombreTachesInitial = taches.Count;

            // === ACT (Action) ===
            // Simulation du clic "Mapping Auto"
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT (Vérifications) ===

            // VÉRIFICATION 1 : Un nouveau jalon doit avoir été créé
            var nouveauxJalons = taches.Where(t => t.EstJalon && t.TacheId.StartsWith("J_Sync_")).ToList();
            Assert.AreEqual(1, nouveauxJalons.Count,
                "Un jalon de synchronisation doit être créé pour factoriser les croisements multiples.");

            var jalonSync = nouveauxJalons.First();
            Assert.AreEqual(TypeActivite.JalonDeSynchronisation, jalonSync.Type,
                "Le jalon créé doit être de type JalonDeSynchronisation.");

            // VÉRIFICATION 2 : Le jalon doit dépendre de Tache_A et Tache_B
            Assert.IsFalse(string.IsNullOrEmpty(jalonSync.Dependencies),
                "Le jalon de synchronisation doit avoir des dépendances.");

            var dependancesJalon = jalonSync.Dependencies.Split(',').Select(d => d.Trim()).ToHashSet();
            Assert.IsTrue(dependancesJalon.Contains("Tache_A"),
                "Le jalon doit dépendre de Tache_A.");
            Assert.IsTrue(dependancesJalon.Contains("Tache_B"),
                "Le jalon doit dépendre de Tache_B.");
            Assert.AreEqual(2, dependancesJalon.Count,
                "Le jalon doit dépendre exactement de Tache_A et Tache_B.");

            // VÉRIFICATION 3 : Tache_C1 et Tache_C2 doivent dépendre uniquement du jalon
            var tacheC1 = taches.First(t => t.TacheId == "Tache_C1");
            var tacheC2 = taches.First(t => t.TacheId == "Tache_C2");

            Assert.AreEqual(jalonSync.TacheId, tacheC1.Dependencies.Trim(),
                "Tache_C1 doit dépendre uniquement du jalon de synchronisation.");
            Assert.AreEqual(jalonSync.TacheId, tacheC2.Dependencies.Trim(),
                "Tache_C2 doit dépendre uniquement du jalon de synchronisation.");

            // VÉRIFICATION CRITIQUE : Pas de dépendances directes vers Tache_A ou Tache_B
            Assert.IsFalse(tacheC1.Dependencies.Contains("Tache_A"),
                "Tache_C1 ne doit PAS avoir de dépendance directe vers Tache_A (factorisation réussie).");
            Assert.IsFalse(tacheC1.Dependencies.Contains("Tache_B"),
                "Tache_C1 ne doit PAS avoir de dépendance directe vers Tache_B (factorisation réussie).");
            Assert.IsFalse(tacheC2.Dependencies.Contains("Tache_A"),
                "Tache_C2 ne doit PAS avoir de dépendance directe vers Tache_A (factorisation réussie).");
            Assert.IsFalse(tacheC2.Dependencies.Contains("Tache_B"),
                "Tache_C2 ne doit PAS avoir de dépendance directe vers Tache_B (factorisation réussie).");

            // VÉRIFICATION 4 : Cohérence du jalon créé
            Assert.AreEqual("BlocTest", jalonSync.BlocId,
                "Le jalon doit appartenir au même bloc que les tâches.");
            Assert.AreEqual("LotTest", jalonSync.LotId,
                "Le jalon doit appartenir au même lot que les tâches.");
            Assert.AreEqual(nombreTachesInitial + 1, taches.Count,
                "Exactement un jalon doit avoir été ajouté.");
        }

        /// <summary>
        /// Test complémentaire : Vérification avec 3 tâches prérequises et 3 tâches dépendantes
        /// pour un cas de factorisation plus complexe
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 5")]
        public void AppliquerMappingAuto_AvecCroisementsComplexes_DoitCreerJalonOptimal()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                // 3 tâches du métier M1 (prérequis)
                new Tache { TacheId = "T_A1", MetierId = "M1", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T_A2", MetierId = "M1", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T_A3", MetierId = "M1", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                
                // 3 tâches du métier M3 (dépendantes)
                new Tache { TacheId = "T_C1", MetierId = "M3", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T_C2", MetierId = "M3", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T_C3", MetierId = "M3", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var jalonSync = taches.Where(t => t.EstJalon && t.TacheId.StartsWith("J_Sync_")).FirstOrDefault();
            Assert.IsNotNull(jalonSync, "Un jalon de synchronisation doit être créé.");

            // Le jalon doit dépendre des 3 tâches du métier M1
            var dependancesJalon = jalonSync.Dependencies.Split(',').Select(d => d.Trim()).ToHashSet();
            Assert.IsTrue(dependancesJalon.Contains("T_A1"), "Le jalon doit dépendre de T_A1.");
            Assert.IsTrue(dependancesJalon.Contains("T_A2"), "Le jalon doit dépendre de T_A2.");
            Assert.IsTrue(dependancesJalon.Contains("T_A3"), "Le jalon doit dépendre de T_A3.");

            // Les 3 tâches du métier M3 doivent dépendre uniquement du jalon
            var tachesM3 = taches.Where(t => t.MetierId == "M3").ToList();
            foreach (var tache in tachesM3)
            {
                Assert.AreEqual(jalonSync.TacheId, tache.Dependencies.Trim(),
                    $"La tâche {tache.TacheId} doit dépendre uniquement du jalon de synchronisation.");
            }
        }

        /// <summary>
        /// Test important : Cas où il n'y a pas besoin de factorisation 
        /// (une seule tâche dépendante ou une seule tâche prérequise)
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 5")]
        public void AppliquerMappingAuto_SansCroisementsMultiples_DoitEviterFactorisation()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                // Une seule tâche M1
                new Tache { TacheId = "Tache_A", MetierId = "M1", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                
                // Deux tâches M3 (pas de croisements multiples car un seul prérequis)
                new Tache { TacheId = "Tache_C1", MetierId = "M3", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "Tache_C2", MetierId = "M3", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var jalonsSync = taches.Where(t => t.EstJalon && t.TacheId.StartsWith("J_Sync_")).ToList();
            Assert.AreEqual(0, jalonsSync.Count,
                "Aucun jalon de synchronisation ne doit être créé car il n'y a pas de croisements multiples.");

            // Les tâches C1 et C2 doivent dépendre directement de A (pas de factorisation nécessaire)
            var tacheC1 = taches.First(t => t.TacheId == "Tache_C1");
            var tacheC2 = taches.First(t => t.TacheId == "Tache_C2");

            Assert.AreEqual("Tache_A", tacheC1.Dependencies.Trim(),
                "Tache_C1 doit dépendre directement de Tache_A (pas de factorisation).");
            Assert.AreEqual("Tache_A", tacheC2.Dependencies.Trim(),
                "Tache_C2 doit dépendre directement de Tache_A (pas de factorisation).");
        }
    }
}