using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Tests fonctionnels pour valider la primauté absolue des choix manuels de l'utilisateur.
    /// 
    /// RÈGLE TESTÉE : Règle N°2 - La Primauté du Manuel sur l'Automatique
    /// Le travail du chef de chantier est sacré. Si une dépendance a été ajoutée ou supprimée 
    /// manuellement, l'algorithme doit le respecter absolument.
    /// 
    /// OBJECTIF : S'assurer que l'algorithme de mapping automatique enrichit les dépendances
    /// existantes sans jamais les écraser, et respecte les exclusions explicites.
    /// 
    /// PRINCIPE FONDAMENTAL : L'utilisateur a toujours le dernier mot, même si ses choix
    /// vont à l'encontre des suggestions métier logiques.
    /// </summary>
    [TestClass]
    public class DependanceBuilderPrimauteManuelTests
    {
        private MetierService _metierService;
        private DependanceBuilder _dependanceBuilder;

        [TestInitialize]
        public void Setup()
        {
            _metierService = new MetierService();
            _dependanceBuilder = new DependanceBuilder(_metierService);

            // Configuration métier : Peinture dépend d'Enduit
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "ENDUIT", Nom = "Enduit", PrerequisMetierIds = "" },
                new Metier { MetierId = "PEINTURE", Nom = "Peinture", PrerequisMetierIds = "ENDUIT" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);
        }

        /// <summary>
        /// Scénario 5 : Test de la primauté du manuel
        /// 
        /// CONTEXTE (Given) : Tache_P1 et Tache_P2 sont du métier Peinture. Tache_E1 est du métier Enduit. 
        /// Peinture a pour prérequis Enduit. L'utilisateur a manuellement créé une dépendance de Tache_P1 vers Tache_E1. 
        /// Tache_P2 n'a aucune dépendance.
        /// 
        /// ACTION (When) : L'utilisateur clique sur "Mapping Auto".
        /// 
        /// RÉSULTAT ATTENDU (Then) :
        /// 1. Les dépendances de Tache_P1 ne sont pas modifiées.
        /// 2. Une nouvelle dépendance est créée de Tache_P2 vers Tache_E1.
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 2")]
        public void AppliquerMappingAuto_AvecDependanceManuelleExistante_DoitPreserverChoixUtilisateur()
        {
            // === ARRANGE (Préparation) ===
            var taches = new List<Tache>
            {
                // Tâche Enduit (prérequis métier)
                new Tache
                {
                    TacheId = "Tache_E1",
                    TacheNom = "Enduit E1",
                    MetierId = "ENDUIT",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },
                
                // Tâche Peinture avec dépendance manuelle EXISTANTE
                new Tache
                {
                    TacheId = "Tache_P1",
                    TacheNom = "Peinture P1",
                    MetierId = "PEINTURE",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "Tache_E1",  // DÉPENDANCE MANUELLE EXISTANTE
                    ExclusionsDependances = ""
                },
                
                // Tâche Peinture SANS dépendance manuelle
                new Tache
                {
                    TacheId = "Tache_P2",
                    TacheNom = "Peinture P2",
                    MetierId = "PEINTURE",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",  // AUCUNE DÉPENDANCE MANUELLE
                    ExclusionsDependances = ""
                }
            };

            // Sauvegarde de l'état initial pour vérification
            var dependancesP1Initiales = taches.First(t => t.TacheId == "Tache_P1").Dependencies;

            // === ACT (Action) ===
            // Simulation du clic "Mapping Auto"
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT (Vérifications) ===
            var tacheP1 = taches.First(t => t.TacheId == "Tache_P1");
            var tacheP2 = taches.First(t => t.TacheId == "Tache_P2");

            // VÉRIFICATION 1 : Les dépendances manuelles de Tache_P1 doivent être PRÉSERVÉES
            Assert.AreEqual(dependancesP1Initiales, tacheP1.Dependencies,
                "RÈGLE 2 FONDAMENTALE : Les dépendances manuelles de Tache_P1 ne doivent JAMAIS être modifiées. " +
                "Le travail du chef de chantier est sacré.");

            // VÉRIFICATION 2 : Une dépendance automatique doit être créée pour Tache_P2
            Assert.IsFalse(string.IsNullOrEmpty(tacheP2.Dependencies),
                "Tache_P2 doit avoir une dépendance générée automatiquement car elle n'en avait pas.");

            Assert.IsTrue(tacheP2.Dependencies.Contains("Tache_E1"),
                "Tache_P2 doit dépendre de Tache_E1 selon les règles métier (PEINTURE dépend d'ENDUIT).");

            // VÉRIFICATION 3 : Cohérence générale
            Assert.AreEqual(3, taches.Count, "Le nombre de tâches ne doit pas changer.");
        }

        /// <summary>
        /// Test critique : Respect des exclusions manuelles
        /// 
        /// L'utilisateur peut explicitement REJETER une suggestion métier via ExclusionsDependances.
        /// Cette décision doit être respectée même si elle va à l'encontre de la logique métier.
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 2")]
        public void AppliquerMappingAuto_AvecExclusionManuelle_DoitRespectorRejetUtilisateur()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                // Tâche Enduit
                new Tache
                {
                    TacheId = "Tache_E1",
                    MetierId = "ENDUIT",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = ""
                },
                
                // Tâche Peinture avec EXCLUSION explicite de la dépendance métier logique
                new Tache
                {
                    TacheId = "Tache_P1",
                    MetierId = "PEINTURE",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",
                    ExclusionsDependances = "Tache_E1"  // L'utilisateur REJETTE cette dépendance métier
                }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var tacheP1 = taches.First(t => t.TacheId == "Tache_P1");

            // VÉRIFICATION CRITIQUE : La dépendance métier logique ne doit PAS être créée
            Assert.IsTrue(string.IsNullOrEmpty(tacheP1.Dependencies),
                "RÈGLE 2 CRITIQUE : Tache_P1 ne doit avoir AUCUNE dépendance car l'utilisateur " +
                "a explicitement exclu Tache_E1, même si la logique métier la suggère.");

            // VÉRIFICATION : L'exclusion doit être préservée
            Assert.IsTrue(tacheP1.ExclusionsDependances.Contains("Tache_E1"),
                "L'exclusion manuelle doit être préservée intacte.");
        }

        /// <summary>
        /// Test complexe : Dépendances manuelles + suggestions automatiques
        /// 
        /// L'utilisateur a créé certaines dépendances manuelles, mais l'algorithme peut
        /// en ajouter d'autres qui ne sont pas en conflit.
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 2")]
        public void AppliquerMappingAuto_AvecDependancesMixtesManuellesEtAuto_DoitEnrichirSansEcraser()
        {
            // === ARRANGE ===
            // Ajout d'un 3ème métier pour rendre le test plus intéressant
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "PREP", Nom = "Préparation", PrerequisMetierIds = "" },
                new Metier { MetierId = "ENDUIT", Nom = "Enduit", PrerequisMetierIds = "PREP" },
                new Metier { MetierId = "PEINTURE", Nom = "Peinture", PrerequisMetierIds = "ENDUIT" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                // Tâches des métiers prérequis
                new Tache { TacheId = "T_Prep", MetierId = "PREP", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T_Enduit", MetierId = "ENDUIT", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                
                // Tâche avec dépendance manuelle partielle
                new Tache
                {
                    TacheId = "T_Peinture",
                    MetierId = "PEINTURE",
                    BlocId = "Bloc1",
                    LotId = "Lot1",
                    Dependencies = "T_Enduit",  // L'utilisateur a créé cette dépendance manuellement
                    ExclusionsDependances = ""
                }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var tPeinture = taches.First(t => t.TacheId == "T_Peinture");

            // La dépendance manuelle doit être préservée
            Assert.IsTrue(tPeinture.Dependencies.Contains("T_Enduit"),
                "La dépendance manuelle vers T_Enduit doit être préservée.");

            // L'algorithme peut enrichir avec d'autres dépendances métier logiques si nécessaire
            // Dans ce cas, T_Enduit devrait dépendre de T_Prep automatiquement
            var tEnduit = taches.First(t => t.TacheId == "T_Enduit");
            Assert.IsTrue(tEnduit.Dependencies.Contains("T_Prep"),
                "L'algorithme doit créer automatiquement la dépendance T_Enduit -> T_Prep.");
        }

        /// <summary>
        /// Test de limite : Dépendance manuelle "illogique" doit être respectée
        /// 
        /// L'utilisateur peut créer des dépendances qui ne suivent pas la logique métier.
        /// L'algorithme doit les respecter sans les questionner.
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 2")]
        public void AppliquerMappingAuto_AvecDependanceManuelleIllogique_DoitRespectorChoixUtilisateur()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T_Enduit", MetierId = "ENDUIT", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                
                // Dépendance "illogique" : PEINTURE ne dépend pas d'ENDUIT (contre la logique métier)
                // mais l'utilisateur l'a voulu ainsi
                new Tache
                {
                    TacheId = "T_Peinture",
                    MetierId = "PEINTURE",
                    BlocId = "Bloc1",
                    LotId = "Lot1",
                    Dependencies = "",  // Volontairement pas d'ENDUIT
                    ExclusionsDependances = "T_Enduit"  // Exclusion explicite
                }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var tPeinture = taches.First(t => t.TacheId == "T_Peinture");

            // L'algorithme ne doit PAS forcer la dépendance logique
            Assert.IsFalse(tPeinture.Dependencies.Contains("T_Enduit"),
                "RÈGLE 2 ABSOLUE : L'algorithme ne doit JAMAIS imposer une dépendance " +
                "que l'utilisateur a explicitement rejetée, même si elle est logique métier.");

            Assert.IsTrue(string.IsNullOrEmpty(tPeinture.Dependencies),
                "T_Peinture doit rester sans dépendance selon la volonté explicite de l'utilisateur.");
        }
    }
}