using PlanAthena.Services.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanAthena.View.TaskManager.Utilitaires
{
    /// <summary>
    /// Représente un élément affiché dans la liste de dépendances,
    /// encapsulant les données brutes et l'état de la case à cocher.
    /// </summary>
    // Dans le fichier DependanceDisplayItem.cs

    public class DependanceDisplayItem
    {
        public DependanceAffichage OriginalData { get; }

        public DependanceDisplayItem(DependanceAffichage originalData)
        {
            OriginalData = originalData;
        }

        public override string ToString()
        {
            string picto;

            switch (OriginalData.Etat)
            {
                case EtatDependance.Suggeree:
                    picto = "✓ "; // Un check simple pour la suggestion
                    break;

                case EtatDependance.Exclue:
                    picto = "✗ "; // Un 'X' pour l'exclusion
                    break;

                // Pour les choix manuels et les neutres, on n'ajoute pas de picto.
                // On ajoute des espaces pour l'alignement vertical du texte.
                case EtatDependance.Stricte:
                case EtatDependance.Neutre:
                default:
                    picto = "  "; // Deux espaces pour aligner avec les pictogrammes
                    break;
            }

            // On retourne la chaîne formatée
            return $"{picto}{OriginalData.TachePredecesseur.TacheNom}";
        }
    }
}
