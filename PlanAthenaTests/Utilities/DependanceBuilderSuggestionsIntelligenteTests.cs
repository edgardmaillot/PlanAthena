using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Tests fonctionnels pour valider les suggestions intelligentes de dépendances dans l'UI.
    /// 
    /// RÈGLE TESTÉE : Règle N°3 - La Suggestion de Dépendances est Ciblée (Anti-Cascade)
    /// Pour une tâche T, on ne doit pas remonter toute la chaîne des prérequis métier. 
    /// La recherche s'arrête dès qu'un métier prérequis est déjà présent dans le bloc.
    /// 
    /// OBJECTIF : S'assurer que l'interface utilisateur propose des suggestions pertinentes
    /// sans submerger l'utilisateur avec des dépendances déjà satisfaites par le graphe existant.
    /// 
    /// USAGE : Ce test valide la méthode ObtenirEtatDependancesPourTache() utilisée par TacheDetailForm.
    /// </summary>
    [TestClass]
    public class DependanceBuilderSuggestionsIntelligenteTests
    {
        private MetierService _metierService;
        private DependanceBuilder _dependanceBuilder;

        [TestInitialize]
        public void Setup()
        {
            _metierService = new MetierService();
            _dependanceBuilder = new DependanceBuilder(_metierService);

            // Configuration d'une chaîne métier : Plaquiste -> Huisserie -> Maçonnerie
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "MACON", Nom = "Maçonnerie", PrerequisMetierIds = "" },
                new Metier { MetierId = "HUISSERIE", Nom = "Huisserie", PrerequisMetierIds = "MACON" },
                new Metier { MetierId = "PLAQUISTE", Nom = "Plaquiste", PrerequisMetierIds = "HUISSERIE" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);
        }

        /// <summary>
        /// Scénario 6 : Test des Suggestions Intelligentes
        /// 
        /// CONTEXTE (Given) : Une tâche Plaquiste dépend d'un jalon J1. Le jalon J1 dépend d'une tâche Huisserie.
        /// 
        /// ACTION (When) : L'utilisateur ouvre le détail de la tâche Plaquiste.
        /// 
        /// RÉSULTAT ATTENDU (Then) : La liste des dépendances suggérées ne doit pas proposer la tâche Huisserie, 
        /// car ce prérequis métier est déjà satisfait par la chaîne de dépendances via le jalon J1.
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 3")]
        public void ObtenirEtatDependancesPourTache_AvecDependanceSatisfaiteParJalon_DoitEviterSuggestionRedondante()
        {
            // === ARRANGE (Préparation) ===
            var taches = new List<Tache>
            {
                // Tâche Maçonnerie (base de la chaîne)
                new Tache
                {
                    TacheId = "T_Macon",
                    TacheNom = "Maçonnerie",
                    MetierId = "MACON",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = ""
                },
                
                // Tâche Huisserie (prérequis de Plaquiste)
                new Tache
                {
                    TacheId = "T_Huisserie",
                    TacheNom = "Huisserie",
                    MetierId = "HUISSERIE",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "T_Macon"
                },
                
                // Jalon intermédiaire qui dépend de Huisserie
                new Tache
                {
                    TacheId = "J1",
                    TacheNom = "Jalon J1",
                    Type = TypeActivite.JalonUtilisateur,
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "T_Huisserie"
                },
                
                // Tâche Plaquiste qui dépend du jalon (le contexte du test)
                new Tache
                {
                    TacheId = "T_Plaquiste",
                    TacheNom = "Plaquiste",
                    MetierId = "PLAQUISTE",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "J1",  // DÉPEND DU JALON
                    ExclusionsDependances = ""
                }
            };

            // === ACT (Action) ===
            // Simulation de l'ouverture du détail de la tâche Plaquiste
            var etatsDependances = _dependanceBuilder.ObtenirEtatDependancesPourTache(
                taches.First(t => t.TacheId == "T_Plaquiste"),
                taches);

            // === ASSERT (Vérifications) ===

            // VÉRIFICATION 1 : La dépendance existante vers J1 doit être présente et stricte
            var dependanceJ1 = etatsDependances.FirstOrDefault(d => d.TachePredecesseur.TacheId == "J1");
            Assert.IsNotNull(dependanceJ1,
                "La dépendance existante vers J1 doit apparaître dans les états.");
            Assert.AreEqual(EtatDependance.Stricte, dependanceJ1.Etat,
                "La dépendance vers J1 doit être marquée comme Stricte (manuelle existante).");

            // VÉRIFICATION CRITIQUE : T_Huisserie ne doit PAS être suggérée
            var suggestionHuisserie = etatsDependances.FirstOrDefault(d => d.TachePredecesseur.TacheId == "T_Huisserie");
            Assert.IsNull(suggestionHuisserie,
                "RÈGLE 3 CRITIQUE : T_Huisserie ne doit PAS être suggérée car le prérequis métier HUISSERIE " +
                "est déjà satisfait par la chaîne T_Plaquiste -> J1 -> T_Huisserie.");

            // VÉRIFICATION COMPLÉMENTAIRE : T_Macon ne doit pas non plus être suggérée
            var suggestionMacon = etatsDependances.FirstOrDefault(d => d.TachePredecesseur.TacheId == "T_Macon");
            Assert.IsNull(suggestionMacon,
                "T_Macon ne doit pas être suggérée car elle est encore plus en amont dans la chaîne.");

            // VÉRIFICATION DE COHÉRENCE : On doit avoir exactement 1 état (J1)
            Assert.AreEqual(1, etatsDependances.Count,
                "Seule la dépendance existante vers J1 doit apparaître, aucune suggestion supplémentaire.");
        }

        /// <summary>
        /// Test complémentaire : Suggestions normales quand aucune dépendance n'existe
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 3")]
        public void ObtenirEtatDependancesPourTache_SansDependanceExistante_DoitProposerSuggestionsDirectes()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                // Tâche Huisserie (prérequis direct)
                new Tache
                {
                    TacheId = "T_Huisserie",
                    MetierId = "HUISSERIE",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = ""
                },
                
                // Tâche Plaquiste SANS dépendance existante
                new Tache
                {
                    TacheId = "T_Plaquiste",
                    MetierId = "PLAQUISTE",
                    BlocId = "BlocTest",
                    LotId = "LotTest",
                    Dependencies = "",  // AUCUNE DÉPENDANCE
                    ExclusionsDependances = ""
                }
            };

            // === ACT ===
            var etatsDependances = _dependanceBuilder.ObtenirEtatDependancesPourTache(
                taches.First(t => t.TacheId == "T_Plaquiste"),
                taches);

            // === ASSERT ===
            // Dans ce cas, T_Huisserie DOIT être suggérée car aucune dépendance n'existe
            var suggestionHuisserie = etatsDependances.FirstOrDefault(d => d.TachePredecesseur.TacheId == "T_Huisserie");
            Assert.IsNotNull(suggestionHuisserie,
                "T_Huisserie doit être suggérée car aucune dépendance n'existe pour couvrir le prérequis métier.");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionHuisserie.Etat,
                "T_Huisserie doit être dans l'état Suggérée.");
            Assert.IsTrue(suggestionHuisserie.EstHeritee,
                "La suggestion doit être marquée comme héritée des règles métier.");
        }

        /// <summary>
        /// Test complexe : Chaîne avec plusieurs niveaux de satisfaction
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 3")]
        public void ObtenirEtatDependancesPourTache_AvecChaineSatisfactionComplexe_DoitOptimiserSuggestions()
        {
            // === ARRANGE ===
            // Ajout d'un métier supplémentaire pour une chaîne plus longue
            var metiers = new List<Metier>
            {
                new Metier { MetierId = "PREP", Nom = "Préparation", PrerequisMetierIds = "" },
                new Metier { MetierId = "MACON", Nom = "Maçonnerie", PrerequisMetierIds = "PREP" },
                new Metier { MetierId = "HUISSERIE", Nom = "Huisserie", PrerequisMetierIds = "MACON" },
                new Metier { MetierId = "PLAQUISTE", Nom = "Plaquiste", PrerequisMetierIds = "HUISSERIE" }
            };
            _metierService.RemplacerTousLesMetiers(metiers);

            var taches = new List<Tache>
            {
                // Chaîne complète : PREP -> MACON -> HUISSERIE
                new Tache { TacheId = "T_Prep", MetierId = "PREP", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                new Tache { TacheId = "T_Macon", MetierId = "MACON", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "T_Prep" },
                new Tache { TacheId = "T_Huisserie", MetierId = "HUISSERIE", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "T_Macon" },
                
                // Tâche Plaquiste avec dépendance partielle (seulement vers MACON)
                new Tache
                {
                    TacheId = "T_Plaquiste",
                    MetierId = "PLAQUISTE",
                    BlocId = "Bloc1",
                    LotId = "Lot1",
                    Dependencies = "T_Macon",  // DÉPENDANCE PARTIELLE : saute HUISSERIE
                    ExclusionsDependances = ""
                }
            };

            // === ACT ===
            var etatsDependances = _dependanceBuilder.ObtenirEtatDependancesPourTache(
                taches.First(t => t.TacheId == "T_Plaquiste"),
                taches);

            // === ASSERT ===
            // T_Macon doit être présente comme dépendance stricte
            var dependanceMacon = etatsDependances.FirstOrDefault(d => d.TachePredecesseur.TacheId == "T_Macon");
            Assert.IsNotNull(dependanceMacon, "T_Macon doit être présente comme dépendance existante.");
            Assert.AreEqual(EtatDependance.Stricte, dependanceMacon.Etat, "T_Macon doit être Stricte.");

            // T_Huisserie doit être suggérée car PLAQUISTE dépend directement de HUISSERIE
            var suggestionHuisserie = etatsDependances.FirstOrDefault(d => d.TachePredecesseur.TacheId == "T_Huisserie");
            Assert.IsNotNull(suggestionHuisserie,
                "T_Huisserie doit être suggérée car PLAQUISTE a HUISSERIE comme prérequis direct non satisfait.");
            Assert.AreEqual(EtatDependance.Suggeree, suggestionHuisserie.Etat, "T_Huisserie doit être Suggérée.");

            // T_Prep ne doit PAS être suggérée car MACON est déjà satisfait par la dépendance existante
            var suggestionPrep = etatsDependances.FirstOrDefault(d => d.TachePredecesseur.TacheId == "T_Prep");
            Assert.IsNull(suggestionPrep,
                "T_Prep ne doit PAS être suggérée car le prérequis MACON est déjà satisfait par la dépendance existante vers T_Macon.");
        }

        /// <summary>
        /// Test des exclusions dans les suggestions
        /// </summary>
        [TestMethod]
        [TestCategory("Functional - Règle 3")]
        public void ObtenirEtatDependancesPourTache_AvecExclusions_DoitMarquerCommeExclue()
        {
            // === ARRANGE ===
            var taches = new List<Tache>
            {
                new Tache { TacheId = "T_Huisserie", MetierId = "HUISSERIE", BlocId = "Bloc1", LotId = "Lot1", Dependencies = "" },
                
                // Tâche avec exclusion explicite
                new Tache
                {
                    TacheId = "T_Plaquiste",
                    MetierId = "PLAQUISTE",
                    BlocId = "Bloc1",
                    LotId = "Lot1",
                    Dependencies = "",
                    ExclusionsDependances = "T_Huisserie"  // EXCLUSION EXPLICITE
                }
            };

            // === ACT ===
            var etatsDependances = _dependanceBuilder.ObtenirEtatDependancesPourTache(
                taches.First(t => t.TacheId == "T_Plaquiste"),
                taches);

            // === ASSERT ===
            var exclusionHuisserie = etatsDependances.FirstOrDefault(d => d.TachePredecesseur.TacheId == "T_Huisserie");
            Assert.IsNotNull(exclusionHuisserie, "T_Huisserie doit apparaître dans les états pour montrer l'exclusion.");
            Assert.AreEqual(EtatDependance.Exclue, exclusionHuisserie.Etat,
                "T_Huisserie doit être marquée comme Exclue pour informer l'utilisateur.");
        }
    }
}