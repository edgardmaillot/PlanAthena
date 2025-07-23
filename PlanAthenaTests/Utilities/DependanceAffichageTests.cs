// Le projet PlanAthenaTests doit avoir une référence de projet vers PlanAthena.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data; // Utilise les vraies classes Tache et TypeActivite
using PlanAthena.Utilities; // Utilise les vraies classes DependanceAffichage et EtatDependance

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Tests unitaires pour la classe DependanceAffichage.
    /// L'objectif est de valider la logique de la méthode ToString(), qui est cruciale
    /// pour la représentation textuelle des dépendances dans l'IHM.
    /// </summary>
    [TestClass]
    public class DependanceAffichageTests
    {
        // Méthode utilitaire pour créer une tâche de test rapidement
        private Tache CreerTacheTest(string id, string nom)
        {
            return new Tache { TacheId = id, TacheNom = nom };
        }

        [TestMethod]
        [TestCategory("Unit - DTO")]
        public void ToString_AvecEtatStricte_DoitRetournerNomEtId()
        {
            // Arrange (Préparation)
            var tachePredecesseur = CreerTacheTest("T001", "Tâche Parente");
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tachePredecesseur,
                Etat = EtatDependance.Stricte
            };
            var attendu = "Tâche Parente (T001)";

            // Act (Action)
            string resultat = dependance.ToString();

            // Assert (Vérification)
            Assert.AreEqual(attendu, resultat, "Une dépendance stricte doit s'afficher sans suffixe.");
        }

        [TestMethod]
        [TestCategory("Unit - DTO")]
        public void ToString_AvecEtatNeutre_DoitRetournerNomEtId()
        {
            // Arrange
            var tachePredecesseur = CreerTacheTest("T002", "Tâche Neutre");
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tachePredecesseur,
                Etat = EtatDependance.Neutre
            };
            var attendu = "Tâche Neutre (T002)";

            // Act
            string resultat = dependance.ToString();

            // Assert
            Assert.AreEqual(attendu, resultat, "Une dépendance neutre doit s'afficher sans suffixe.");
        }

        [TestMethod]
        [TestCategory("Unit - DTO")]
        public void ToString_AvecEtatSuggeree_DoitAjouterSuffixeSuggere()
        {
            // Arrange
            var tachePredecesseur = CreerTacheTest("T003", "Tâche Suggérée");
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tachePredecesseur,
                Etat = EtatDependance.Suggeree,
                EstHeritee = true
            };
            var attendu = "Tâche Suggérée (T003) (suggéré)";

            // Act
            string resultat = dependance.ToString();

            // Assert
            Assert.AreEqual(attendu, resultat);
        }

        [TestMethod]
        [TestCategory("Unit - DTO")]
        public void ToString_AvecEtatExclue_DoitAjouterSuffixeExclu()
        {
            // Arrange
            var tachePredecesseur = CreerTacheTest("T004", "Tâche Exclue");
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tachePredecesseur,
                Etat = EtatDependance.Exclue
            };
            var attendu = "Tâche Exclue (T004) (exclu)";

            // Act
            string resultat = dependance.ToString();

            // Assert
            Assert.AreEqual(attendu, resultat);
        }

        [TestMethod]
        [TestCategory("Unit - DTO")]
        public void ToString_AvecEtatStricteMemeSiHeritee_NeDoitPasAfficherSuggere()
        {
            // Arrange
            // Cas où une dépendance manuelle correspond aussi à une suggestion métier.
            // L'état "Stricte" doit avoir la priorité et ne pas afficher "(suggéré)".
            var tachePredecesseur = CreerTacheTest("T005", "Tâche Stricte et Suggérée");
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tachePredecesseur,
                Etat = EtatDependance.Stricte,
                EstHeritee = true // Important pour le test
            };
            var attendu = "Tâche Stricte et Suggérée (T005)";

            // Act
            string resultat = dependance.ToString();

            // Assert
            Assert.AreEqual(attendu, resultat, "Une dépendance Stricte ne doit jamais afficher le suffixe '(suggéré)', même si elle est aussi héritée.");
        }
    }
}