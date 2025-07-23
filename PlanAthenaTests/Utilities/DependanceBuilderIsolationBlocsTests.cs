using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Tests fonctionnels pour valider l'isolation stricte des blocs de travail.
    /// 
    /// RÈGLE TESTÉE : Règle N°1 - L'Isolation Stricte des Blocs
    /// Les Blocs sont des univers clos et étanches. Une tâche ne peut jamais dépendre 
    /// d'une tâche appartenant à un autre bloc.
    /// 
    /// OBJECTIF : S'assurer qu'aucune dépendance ne traverse les frontières entre blocs,
    /// même si les règles métier le suggèrent.
    /// </summary>
    [TestClass]
    public class DependanceBuilderIsolationBlocsTests
    {
        private MetierService _metierService;
        private DependanceBuilder _dependanceBuilder;

        [TestInitialize]
        public void Setup()
        {
            _metierService = new MetierService();
            _dependanceBuilder = new DependanceBuilder(_metierService);

            // Configuration des métiers pour le test : M2 dépend de M1
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "M1", Nom = "Métier 1", PrerequisMetierIds = "" },
                new Metier { MetierId = "M2", Nom = "Métier 2", PrerequisMetierIds = "M1" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);
        }

        /// <summary>
        /// Scénario 1 : Test de l'Isolation des Blocs
        /// 
        /// CONTEXTE (Given) : Le projet contient deux blocs, Bloc A et Bloc B. 
        /// Bloc A contient Tache_A1 (Métier M1). Bloc B contient Tache_B2 (Métier M2). 
        /// Le Métier M2 a pour prérequis M1.
        /// 
        /// ACTION (When) : L'utilisateur clique sur "Mapping Auto".
        /// 
        /// RÉSULTAT ATTENDU (Then) : La tâche Tache_B2 ne doit avoir aucune dépendance sur Tache_A1. 
        /// Le graphe ne doit contenir aucune arête traversant la frontière entre Bloc A et Bloc B.
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 1")]
        public void AppliquerMappingAuto_AvecTachesDeBlitsDifferents_DoitRespectorIsolationDesBlocs()
        {
            // === ARRANGE (Préparation) ===
            var taches = new List<Tache>
            {
                // Bloc A : Tâche du métier M1
                new Tache
                {
                    TacheId = "Tache_A1",
                    TacheNom = "Tâche A1",
                    MetierId = "M1",
                    BlocId = "BlocA",
                    LotId = "Lot1",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },
                
                // Bloc B : Tâche du métier M2 (qui a M1 comme prérequis)
                new Tache
                {
                    TacheId = "Tache_B2",
                    TacheNom = "Tâche B2",
                    MetierId = "M2",
                    BlocId = "BlocB",
                    LotId = "Lot1",
                    Dependencies = "",
                    ExclusionsDependances = ""
                }
            };

            // === ACT (Action) ===
            // Simulation du clic "Mapping Auto"
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT (Vérifications) ===
            var tacheB2 = taches.First(t => t.TacheId == "Tache_B2");

            // VÉRIFICATION PRINCIPALE : Aucune dépendance inter-blocs
            Assert.IsTrue(string.IsNullOrEmpty(tacheB2.Dependencies),
                "La tâche Tache_B2 ne doit avoir aucune dépendance car elle est dans un bloc différent de son prérequis métier.");

            // VÉRIFICATION COMPLÉMENTAIRE : Aucune tâche ne référence une tâche d'un autre bloc
            foreach (var tache in taches)
            {
                if (!string.IsNullOrEmpty(tache.Dependencies))
                {
                    var dependancesIds = tache.Dependencies.Split(',').Select(d => d.Trim());
                    foreach (var depId in dependancesIds)
                    {
                        var tacheDependance = taches.FirstOrDefault(t => t.TacheId == depId);
                        if (tacheDependance != null)
                        {
                            Assert.AreEqual(tache.BlocId, tacheDependance.BlocId,
                                $"VIOLATION RÈGLE 1 : La tâche '{tache.TacheId}' (Bloc: {tache.BlocId}) " +
                                $"dépend de '{depId}' (Bloc: {tacheDependance.BlocId}). " +
                                $"Les dépendances inter-blocs sont interdites.");
                        }
                    }
                }
            }

            // VÉRIFICATION DE COHÉRENCE : Les deux tâches existent toujours
            Assert.AreEqual(2, taches.Count, "Les deux tâches originales doivent être préservées.");
            Assert.IsTrue(taches.Any(t => t.TacheId == "Tache_A1"), "Tache_A1 doit exister.");
            Assert.IsTrue(taches.Any(t => t.TacheId == "Tache_B2"), "Tache_B2 doit exister.");
        }

        /// <summary>
        /// Test complémentaire : Validation que l'isolation fonctionne même avec des jalons
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 1")]
        public void AppliquerMappingAuto_AvecJalonsDeBlitsDifferents_DoitRespectorIsolationDesBlocs()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                // Bloc A : Jalon
                new Tache
                {
                    TacheId = "Jalon_A1",
                    TacheNom = "Jalon A1",
                    Type = TypeActivite.JalonUtilisateur,
                    BlocId = "BlocA",
                    LotId = "Lot1",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },
                
                // Bloc B : Tâche qui pourrait théoriquement dépendre du jalon
                new Tache
                {
                    TacheId = "Tache_B1",
                    TacheNom = "Tâche B1",
                    MetierId = "M1",
                    BlocId = "BlocB",
                    LotId = "Lot1",
                    Dependencies = "",
                    ExclusionsDependances = ""
                }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var tacheB1 = taches.First(t => t.TacheId == "Tache_B1");
            Assert.IsTrue(string.IsNullOrEmpty(tacheB1.Dependencies),
                "Même les jalons ne peuvent pas créer de dépendances inter-blocs.");
        }

        /// <summary>
        /// Test de validation positive : S'assurer que l'isolation n'empêche pas 
        /// les dépendances légitimes au sein d'un même bloc
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 1")]
        public void AppliquerMappingAuto_AvecTachesDuMemeBloc_DoitCreerDependancesIntraBloc()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                // Même bloc : Tâche M1
                new Tache
                {
                    TacheId = "Tache_A1",
                    TacheNom = "Tâche A1",
                    MetierId = "M1",
                    BlocId = "BlocA",
                    LotId = "Lot1",
                    Dependencies = "",
                    ExclusionsDependances = ""
                },
                
                // Même bloc : Tâche M2 (doit dépendre de M1)
                new Tache
                {
                    TacheId = "Tache_A2",
                    TacheNom = "Tâche A2",
                    MetierId = "M2",
                    BlocId = "BlocA",  // MÊME BLOC
                    LotId = "Lot1",
                    Dependencies = "",
                    ExclusionsDependances = ""
                }
            };

            // === ACT ===
            _dependanceBuilder.AppliquerEtSimplifierDependances(taches);

            // === ASSERT ===
            var tacheA2 = taches.First(t => t.TacheId == "Tache_A2");

            // VÉRIFICATION : La dépendance intra-bloc doit être créée
            Assert.IsFalse(string.IsNullOrEmpty(tacheA2.Dependencies),
                "Les dépendances au sein du même bloc doivent être créées.");

            Assert.IsTrue(tacheA2.Dependencies.Contains("Tache_A1"),
                "Tache_A2 doit dépendre de Tache_A1 car elles sont dans le même bloc et M2 dépend de M1.");
        }
    }
}