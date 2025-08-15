// Fichier: PlanAthena/Data/Lot.cs
// Version: 0.4.4
// Description: Ajout de la collection de Blocs pour créer la hiérarchie de données Lot -> Blocs.

using System.Collections.Generic;

namespace PlanAthena.Data
{
    /// <summary>
    /// Représente un lot de travaux, un regroupement logique de tâches (ex: CVC RDC).
    /// Il contient maintenant la liste des blocs qui lui sont associés.
    /// </summary>
    public class Lot
    {
        public string LotId { get; set; } = "";
        public string Nom { get; set; } = "";
        public int Priorite { get; set; }
        public string CheminFichierPlan { get; set; } = "";
        public ChantierPhase Phases { get; set; } = ChantierPhase.None;

        /// <summary>
        /// Liste des Blocs appartenant à ce Lot.
        /// La persistance des Blocs se fait désormais via cette hiérarchie.
        /// </summary>
        public List<Bloc> Blocs { get; set; } = new List<Bloc>();
    }
}