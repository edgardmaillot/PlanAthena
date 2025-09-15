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
    public class DependanceDisplayItem
    {
        public DependanceAffichage OriginalData { get; }

        public DependanceDisplayItem(DependanceAffichage originalData)
        {
            OriginalData = originalData;
        }

        public override string ToString()
        {
            // Le texte affiché dans la CheckedListBox.
            string type = OriginalData.EstHeritee ? "(Héritée) " : "(Directe) ";
            return $"{type}{OriginalData.TachePredecesseur.TacheNom}";
        }
    }
}
