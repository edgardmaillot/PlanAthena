using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Tests fonctionnels pour valider la préservation des jalons existants pertinents.
    /// 
    /// RÈGLE TESTÉE : Règle N°5 - Raffinement de la Factorisation (Anti-Jalons Redondants)
    /// L'algorithme ne doit pas créer de nouveaux jalons si des jalons existants font déjà 
    /// efficacement le travail de synchronisation.
    /// 
    /// OBJECTIF : S'assurer que les jalons manuels ou automatiques existants sont préservés
    /// et que l'algorithme ne crée pas de jalons redondants qui compliqueraient le graphe.
    /// 
    /// CONTEXTE : Ce test valide la correction apportée à EstFactorisationNecessaire()
    /// qui résolvait le problème de destruction des jalons existants pertinents.
    /// </summary>
    [TestClass]
    public class DependanceBuilderJalonNonRedondantTests
    {
        private MetierService _metierService;
        private DependanceBuilder _dependanceBuilder;

        [TestInitialize]
        public void Setup()
        {
            _metierService = new MetierService();
            _dependanceBuilder = new DependanceBuilder(_metierService);

            // Configuration minimale pour les tests
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);
        }

        /// <summary>
        /// Scénario 4 : Test de non-création de Jalon redondant
        /// 
        /// CONTEXTE (Given) : Tache_A et Tache_B dépendent d'un jalon existant J_Existant.
        /// 
        /// ACTION (When) : L'utilisateur clique sur "Mapping Auto".
        /// 
        /// RÉSULTAT ATTENDU (Then) : Aucun nouveau jalon n'est créé. 
        /// Les dépendances de Tache_A et Tache_B restent inchangées (J_Existant).
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 5 Raffinée")]
        public void AppliquerMappingAuto_AvecJalonExistantPertinent_DoitPreserverJalonExistant()
        {
            // === ARRANGE (Préparation) ===
            var taches = new List<Tache>
            {
                // Jalon existant (manuel ou automatique précédent)
                new Tache
                {
                    TacheId = "J_Existant",
                    TacheNom = "Jalon Existant",
                    Type = TypeActivite.JalonUtilisateur,
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "Tache_Prerequis"
                },
                
                // Tâche prérequise
                new Tache
                {
                    TacheId = "Tache_Prerequis",
                    TacheNom = "Tâche Prérequise",
                    MetierId = "M1",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = ""
                },
                
                // Deux tâches qui dépendent du jalon existant
                new Tache
                {
                    TacheId = "Tache_A",
                    TacheNom = "Tâche A",
                    MetierId = "M2",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "J_Existant"  // Dépendance existante vers le jalon
                },

                new Tache
                {
                    TacheId = "Tache_B",
                    TacheNom = "Tâche B",
                    MetierId = "M2",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "J_Existant"  // Dépendance existante vers le jalon
                }
            };

            var nombreTachesInitial = taches.Count;
            var jalonExistantId = "J_Existant";

            // === ACT (Action) ===
            // Simulation du clic "Mapping Auto"
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT (Vérifications) ===

            // VÉRIFICATION 1 : Le jalon existant doit être préservé
            var jalonExistant = taches.FirstOrDefault(t => t.TacheId == jalonExistantId);
            Assert.IsNotNull(jalonExistant,
                "Le jalon existant J_Existant doit être préservé.");
            Assert.IsTrue(jalonExistant.EstJalon,
                "J_Existant doit rester un jalon.");

            // VÉRIFICATION 2 : Aucun nouveau jalon de synchronisation ne doit être créé
            var nouveauxJalonsSync = taches.Where(t =>
                t.EstJalon &&
                t.TacheId.StartsWith("J_Sync_") &&
                t.TacheId != jalonExistantId).ToList();

            Assert.AreEqual(0, nouveauxJalonsSync.Count,
                "RÈGLE 5 RAFFINÉE : Aucun nouveau jalon de synchronisation ne doit être créé " +
                "car le jalon existant fait déjà efficacement le travail de synchronisation.");

            // VÉRIFICATION 3 : Le nombre total de tâches ne doit pas avoir augmenté
            Assert.AreEqual(nombreTachesInitial, taches.Count,
                "Aucune nouvelle tâche/jalon ne doit être ajoutée.");

            // VÉRIFICATION 4 : Les dépendances de Tache_A et Tache_B doivent rester inchangées
            var tacheA = taches.First(t => t.TacheId == "Tache_A");
            var tacheB = taches.First(t => t.TacheId == "Tache_B");

            Assert.AreEqual(jalonExistantId, tacheA.Dependencies.Trim(),
                "Tache_A doit continuer à dépendre du jalon existant J_Existant.");
            Assert.AreEqual(jalonExistantId, tacheB.Dependencies.Trim(),
                "Tache_B doit continuer à dépendre du jalon existant J_Existant.");

            // VÉRIFICATION 5 : La dépendance du jalon existant doit être préservée
            Assert.AreEqual("Tache_Prerequis", jalonExistant.Dependencies.Trim(),
                "Les dépendances du jalon existant doivent être préservées.");
        }

        /// <summary>
        /// Test du cas complexe : Jalon existant + tâche individuelle (le cas de votre JSON de test)
        /// 
        /// Ce test reproduit exactement le scénario qui était cassé avant notre correction :
        /// - BA13 Bureau et BA13 sanitaires dépendent de J_Sync_BOISPoFe + Cablage convecteurs PAC
        /// - L'algorithme doit préserver le jalon existant et ne pas le détruire
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 5 Raffinée")]
        public void AppliquerMappingAuto_AvecJalonExistantEtTacheIndividuelle_DoitPreserverStructureExistante()
        {
            // === ARRANGE ===
            // Reproduction du cas réel de votre JSON de test
            var taches = new List<Tache>
            {
                // Jalon existant (équivalent à J_Sync_BOISPoFe_Zone boutique_Prive)
                new Tache
                {
                    TacheId = "J_Sync_Existant",
                    TacheNom = "Synchro Existante",
                    Type = TypeActivite.JalonDeSynchronisation,
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "Tache_Bois1,Tache_Bois2"
                },
                
                // Tâches prérequises du jalon
                new Tache { TacheId = "Tache_Bois1", MetierId = "M1", BlocId = "BlocTest", LotId = "LotTest", Dependencies = "" },
                new Tache { TacheId = "Tache_Bois2", MetierId = "M1", BlocId = "BlocTest", LotId = "LotTest", Dependencies = "" },
                
                // Tâche individuelle (équivalent à Cablage convecteurs PAC)
                new Tache
                {
                    TacheId = "Tache_Elec",
                    TacheNom = "Tâche Électrique",
                    MetierId = "M1",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = ""
                },
                
                // Tâches qui dépendent du jalon + tâche individuelle (équivalent aux BA13)
                new Tache
                {
                    TacheId = "Tache_Placo1",
                    MetierId = "M2",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "J_Sync_Existant,Tache_Elec"  // LE CAS CRITIQUE
                },

                new Tache
                {
                    TacheId = "Tache_Placo2",
                    MetierId = "M2",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "J_Sync_Existant,Tache_Elec"  // LE CAS CRITIQUE
                }
            };

            var nombreTachesInitial = taches.Count;

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===

            // VÉRIFICATION CRITIQUE : Le jalon existant doit être préservé
            var jalonExistant = taches.FirstOrDefault(t => t.TacheId == "J_Sync_Existant");
            Assert.IsNotNull(jalonExistant,
                "Le jalon existant doit être préservé (notre correction en action).");

            // VÉRIFICATION : Aucun nouveau jalon redondant créé
            var nouveauxJalons = taches.Where(t =>
                t.EstJalon &&
                t.TacheId.StartsWith("J_Sync_") &&
                t.TacheId != "J_Sync_Existant").ToList();

            Assert.AreEqual(0, nouveauxJalons.Count,
                "Notre correction EstFactorisationNecessaire() doit empêcher la création d'un jalon redondant.");

            // VÉRIFICATION : Les tâches Placo continuent de dépendre du jalon existant + tâche élec
            var tachePlaco1 = taches.First(t => t.TacheId == "Tache_Placo1");
            var tachePlaco2 = taches.First(t => t.TacheId == "Tache_Placo2");

            Assert.IsTrue(tachePlaco1.Dependencies.Contains("J_Sync_Existant"),
                "Tache_Placo1 doit continuer à dépendre du jalon existant.");
            Assert.IsTrue(tachePlaco1.Dependencies.Contains("Tache_Elec"),
                "Tache_Placo1 doit continuer à dépendre de la tâche électrique.");

            Assert.IsTrue(tachePlaco2.Dependencies.Contains("J_Sync_Existant"),
                "Tache_Placo2 doit continuer à dépendre du jalon existant.");
            Assert.IsTrue(tachePlaco2.Dependencies.Contains("Tache_Elec"),
                "Tache_Placo2 doit continuer à dépendre de la tâche électrique.");

            Assert.AreEqual(nombreTachesInitial, taches.Count,
                "Aucune tâche supplémentaire ne doit être créée.");
        }

        /// <summary>
        /// Test de validation : S'assurer que la factorisation fonctionne toujours 
        /// quand elle est vraiment nécessaire
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 5 Raffinée")]
        public void AppliquerMappingAuto_SansJalonExistantPertinent_DoitCreerJalonQuandNecessaire()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                // Deux tâches prérequises indépendantes (pas de jalon existant)
                new Tache { TacheId = "T_A", MetierId = "M1", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T_B", MetierId = "M1", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                
                // Deux tâches qui vont créer des croisements
                new Tache { TacheId = "T_C", MetierId = "M2", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T_D", MetierId = "M2", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            // Dans ce cas, un jalon DOIT être créé car il n'y en a pas d'existant pertinent
            var nouveauxJalons = taches.Where(t => t.EstJalon && t.TacheId.StartsWith("J_Sync_")).ToList();
            Assert.AreEqual(1, nouveauxJalons.Count,
                "Un jalon doit être créé car il n'y a pas de jalon existant pertinent " +
                "et les croisements multiples justifient la factorisation.");
        }
    }
}