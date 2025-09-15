using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;

namespace PlanAthenaTests.Utilities
{
    /// <summary>
    /// Tests unitaires pour la classe DependanceAffichage et l'enum EtatDependance.
    /// 
    /// COUVERTURE :
    /// - Propriétés et constructeur
    /// - Méthode ToString() avec différents cas
    /// - Enum EtatDependance et ses valeurs
    /// - Cas limites et robustesse
    /// </summary>
    [TestClass]
    public class DependanceAffichageTests
    {
        #region Tests des Propriétés de Base

        [TestMethod]
        [TestCategory("Unit - Propriétés")]
        public void DependanceAffichage_ProprietesDeBase_DoiventEtreCorrectementInitialisees()
        {
            // Arrange
            var tache = new Tache { TacheId = "T001", TacheNom = "Test Tâche" };
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Suggeree,
                EstHeritee = true
            };

            // Act & Assert
            Assert.AreEqual(tache, dependance.TachePredecesseur, "TachePredecesseur doit être correctement assignée");
            Assert.AreEqual(EtatDependance.Suggeree, dependance.Etat, "Etat doit être correctement assigné");
            Assert.IsTrue(dependance.EstHeritee, "EstHeritee doit être correctement assignée");
        }

        #endregion

        #region Tests de la Méthode ToString()

        [TestMethod]
        [TestCategory("Unit - ToString")]
        public void ToString_AvecTacheValide_DoitRetournerFormatCorrect()
        {
            // Arrange
            var tache = new Tache { TacheId = "T001", TacheNom = "Maçonnerie Bureau" };
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Stricte,
                EstHeritee = false
            };

            // Act
            var resultat = dependance.ToString();

            // Assert
            Assert.AreEqual("✅ Maçonnerie Bureau", resultat,
                "ToString() doit retourner le format 'Icône TacheNom' pour état Stricte");
        }

        [TestMethod]
        [TestCategory("Unit - ToString")]
        public void ToString_AvecTacheIdNull_DoitGererGracieusement()
        {
            // Arrange
            var tache = new Tache { TacheId = null, TacheNom = "Tâche sans ID" };
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Neutre
            };

            // Act
            var resultat = dependance.ToString();

            // Assert
            Assert.AreEqual("⚪ Tâche sans ID", resultat,
                "ToString() doit afficher l'icône neutre + nom même sans TacheId");
        }

        [TestMethod]
        [TestCategory("Unit - ToString")]
        public void ToString_AvecTacheNomNull_DoitGererGracieusement()
        {
            // Arrange
            var tache = new Tache { TacheId = "T001", TacheNom = null };
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Neutre
            };

            // Act
            var resultat = dependance.ToString();

            // Assert
            Assert.AreEqual("⚪ ", resultat,
                "ToString() doit afficher l'icône même avec TacheNom null");
        }

        [TestMethod]
        [TestCategory("Unit - ToString")]
        public void ToString_AvecChainesVides_DoitGererCorrectement()
        {
            // Arrange
            var tache = new Tache { TacheId = "", TacheNom = "" };
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Neutre
            };

            // Act
            var resultat = dependance.ToString();

            // Assert
            Assert.AreEqual("⚪ ", resultat,
                "ToString() doit afficher l'icône neutre même avec nom vide");
        }

        [TestMethod]
        [TestCategory("Unit - ToString")]
        public void ToString_AvecCaracteresSpeciaux_DoitPreserverContenu()
        {
            // Arrange
            var tache = new Tache
            {
                TacheId = "T-001_Spé",
                TacheNom = "Tâche avec accénts & caractères spéciaux (test)"
            };
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Suggeree
            };

            // Act
            var resultat = dependance.ToString();

            // Assert
            Assert.AreEqual("💡 Tâche avec accénts & caractères spéciaux (test)", resultat,
                "ToString() doit préserver tous les caractères spéciaux avec icône suggestion");
        }
        [TestMethod]
        [TestCategory("Unit - ToString")]
        public void ToString_PourTousLesEtats_DoitAfficherBonneIcone()
        {
            // Arrange
            var tache = new Tache { TacheId = "T001", TacheNom = "Test" };

            var dependances = new[]
            {
        new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Neutre },
        new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Suggeree },
        new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Stricte },
        new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Exclue }
    };

            // Act & Assert
            Assert.AreEqual("⚪ Test", dependances[0].ToString(), "Neutre doit avoir l'icône ⚪");
            Assert.AreEqual("💡 Test", dependances[1].ToString(), "Suggérée doit avoir l'icône 💡");
            Assert.AreEqual("✅ Test", dependances[2].ToString(), "Stricte doit avoir l'icône ✅");
            Assert.AreEqual("❌ Test", dependances[3].ToString(), "Exclue doit avoir l'icône ❌");
        }

        #endregion

        #region Tests de l'Enum EtatDependance

        [TestMethod]
        [TestCategory("Unit - Enum")]
        public void EtatDependance_ValeursPredefinies_DoiventExister()
        {
            // Act & Assert - Vérification que toutes les valeurs existent
            Assert.IsTrue(Enum.IsDefined(typeof(EtatDependance), EtatDependance.Neutre),
                "EtatDependance.Neutre doit être défini");
            Assert.IsTrue(Enum.IsDefined(typeof(EtatDependance), EtatDependance.Suggeree),
                "EtatDependance.Suggeree doit être défini");
            Assert.IsTrue(Enum.IsDefined(typeof(EtatDependance), EtatDependance.Stricte),
                "EtatDependance.Stricte doit être défini");
            Assert.IsTrue(Enum.IsDefined(typeof(EtatDependance), EtatDependance.Exclue),
                "EtatDependance.Exclue doit être défini");
        }

        [TestMethod]
        [TestCategory("Unit - Enum")]
        public void EtatDependance_ValeursNumeriques_DoiventEtreStables()
        {
            // Act & Assert - Vérification que les valeurs numériques sont stables
            // Important pour la sérialisation et la compatibilité
            Assert.AreEqual(0, (int)EtatDependance.Neutre, "Neutre doit avoir la valeur 0");
            Assert.AreEqual(1, (int)EtatDependance.Suggeree, "Suggeree doit avoir la valeur 1");
            Assert.AreEqual(2, (int)EtatDependance.Stricte, "Stricte doit avoir la valeur 2");
            Assert.AreEqual(3, (int)EtatDependance.Exclue, "Exclue doit avoir la valeur 3");
        }

        [TestMethod]
        [TestCategory("Unit - Enum")]
        public void EtatDependance_ConversionString_DoitFonctionner()
        {
            // Act & Assert - Test de conversion en chaîne
            Assert.AreEqual("Neutre", EtatDependance.Neutre.ToString());
            Assert.AreEqual("Suggeree", EtatDependance.Suggeree.ToString());
            Assert.AreEqual("Stricte", EtatDependance.Stricte.ToString());
            Assert.AreEqual("Exclue", EtatDependance.Exclue.ToString());
        }

        [TestMethod]
        [TestCategory("Unit - Enum")]
        public void EtatDependance_ParseDepuisString_DoitFonctionner()
        {
            // Act & Assert - Test de parsing depuis chaîne
            Assert.AreEqual(EtatDependance.Neutre, Enum.Parse<EtatDependance>("Neutre"));
            Assert.AreEqual(EtatDependance.Suggeree, Enum.Parse<EtatDependance>("Suggeree"));
            Assert.AreEqual(EtatDependance.Stricte, Enum.Parse<EtatDependance>("Stricte"));
            Assert.AreEqual(EtatDependance.Exclue, Enum.Parse<EtatDependance>("Exclue"));
        }

        #endregion

        #region Tests de Scénarios d'Usage UI

        [TestMethod]
        [TestCategory("Integration - UI")]
        public void DependanceAffichage_PourAffichageUI_DoitFournirInformationsCompletes()
        {
            // Arrange - Simulation d'un objet utilisé dans TacheDetailForm
            var tache = new Tache
            {
                TacheId = "T001",
                TacheNom = "Maçonnerie",
                MetierId = "MACON_01",
                BlocId = "B001"
            };

            var dependanceStricte = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Stricte,
                EstHeritee = false
            };

            var dependanceSuggeree = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Suggeree,
                EstHeritee = true
            };

            // Act & Assert - Vérifications pour l'affichage UI

            // Pour dépendance stricte (définie manuellement)
            Assert.AreEqual("✅ Maçonnerie", dependanceStricte.ToString(),
                "Affichage avec icône ✅ pour CheckedListBox");
            Assert.AreEqual(EtatDependance.Stricte, dependanceStricte.Etat,
                "État pour déterminer le style (noir, gras)");
            Assert.IsFalse(dependanceStricte.EstHeritee,
                "Pas héritée = définie manuellement");

            // Pour dépendance suggérée (par règles métier)
            Assert.AreEqual("💡 Maçonnerie", dependanceSuggeree.ToString(),
                "Affichage avec icône 💡 pour CheckedListBox");
            Assert.AreEqual(EtatDependance.Suggeree, dependanceSuggeree.Etat,
                "État pour déterminer le style (bleu, italique)");
            Assert.IsTrue(dependanceSuggeree.EstHeritee,
                "Héritée = suggérée par règles métier");
        }
        [TestMethod]
        [TestCategory("Integration - UI")]
        public void DependanceAffichage_PourLogiqueSauvegarde_DoitDistinguerTypesCorrectement()
        {
            // Arrange - Simulation de la logique SauvegarderModifications() dans TacheDetailForm
            var tache = new Tache { TacheId = "T001", TacheNom = "Test" };

            var dependances = new[]
            {
                new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Stricte, EstHeritee = false },
                new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Suggeree, EstHeritee = true },
                new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Exclue, EstHeritee = true },
                new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Neutre, EstHeritee = false }
            };

            // Assert - Vérification logique de sauvegarde selon TacheDetailForm
            foreach (var dep in dependances)
            {
                switch (dep.Etat)
                {
                    case EtatDependance.Stricte:
                        // Si cochée ET pas héritée → Dependencies
                        Assert.IsFalse(dep.EstHeritee, "Stricte ne doit pas être héritée");
                        // Logique: if (!item.EstHeritee && estCochee) → Dependencies
                        break;

                    case EtatDependance.Suggeree:
                        // Si cochée ET héritée → Dependencies (matérialisation)
                        Assert.IsTrue(dep.EstHeritee, "Suggérée doit être héritée");
                        // Logique: if (item.EstHeritee && estCochee) → Dependencies
                        break;

                    case EtatDependance.Exclue:
                        // Si décochée ET héritée → ExclusionsDependances
                        Assert.IsTrue(dep.EstHeritee, "Exclue doit être héritée");
                        // Logique: if (item.EstHeritee && !estCochee) → ExclusionsDependances
                        break;

                    case EtatDependance.Neutre:
                        // Si cochée ET pas héritée → Dependencies
                        Assert.IsFalse(dep.EstHeritee, "Neutre ne doit pas être héritée");
                        // Logique: if (!item.EstHeritee && estCochee) → Dependencies
                        break;
                }
            }
        }

        #endregion

        #region Tests de Comparaison et Égalité

        [TestMethod]
        [TestCategory("Unit - Égalité")]
        public void DependanceAffichage_AvecMemeTache_DoitAvoirMemeNom()
        {
            // Arrange
            var tache1 = new Tache { TacheId = "T001", TacheNom = "Tâche Test" };
            var tache2 = new Tache { TacheId = "T001", TacheNom = "Tâche Test" };

            var dep1 = new DependanceAffichage { TachePredecesseur = tache1, Etat = EtatDependance.Stricte };
            var dep2 = new DependanceAffichage { TachePredecesseur = tache2, Etat = EtatDependance.Stricte };

            // Act
            var string1 = dep1.ToString();
            var string2 = dep2.ToString();

            // Assert
            Assert.AreEqual(string1, string2,
                "Deux DependanceAffichage avec des tâches identiques ET même état doivent avoir le même ToString()");

            // Test avec états différents
            var dep3 = new DependanceAffichage { TachePredecesseur = tache1, Etat = EtatDependance.Suggeree };
            Assert.AreNotEqual(dep1.ToString(), dep3.ToString(),
                "Même tâche mais états différents doivent avoir des ToString() différents");
        }

        #endregion

        #region Tests de Performance et Robustesse

        [TestMethod]
        [TestCategory("Unit - Performance")]
        public void ToString_AppelsMultiples_DoitEtrePerformant()
        {
            // Arrange
            var tache = new Tache { TacheId = "T001", TacheNom = "Tâche de performance" };
            var dependance = new DependanceAffichage { TachePredecesseur = tache, Etat = EtatDependance.Neutre };

            // Act & Assert - 1000 appels doivent être rapides
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                var result = dependance.ToString();
                Assert.IsNotNull(result, "ToString() ne doit jamais retourner null");
            }

            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
                "1000 appels à ToString() doivent s'exécuter en moins de 100ms");
        }

        [TestMethod]
        [TestCategory("Unit - Robustesse")]
        public void DependanceAffichage_AvecTacheLongue_DoitGererCorrectement()
        {
            // Arrange - Tâche avec nom très long
            var nomTresLong = new string('A', 1000); // 1000 caractères
            var tache = new Tache { TacheId = "T_LONG", TacheNom = nomTresLong };
            var dependance = new DependanceAffichage
            {
                TachePredecesseur = tache,
                Etat = EtatDependance.Neutre
            };

            // Act
            var resultat = dependance.ToString();

            // Assert
            Assert.IsTrue(resultat.StartsWith("⚪ "), "Doit commencer par l'icône neutre");
            Assert.IsTrue(resultat.Contains(nomTresLong), "Le nom long doit être préservé intégralement");
            Assert.IsTrue(resultat.Length > 1000, "La chaîne résultante doit contenir tout le contenu");

            // Note: L'ID n'est plus affiché dans le nouveau format, donc on ne le teste plus
        }

        #endregion
    }
}