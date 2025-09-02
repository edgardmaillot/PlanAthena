// Emplacement: /Data/Tache.cs

using PlanAthena.Services.DTOs.TaskManager;

namespace PlanAthena.Data
{
    /// <summary>
    /// Représente une activité dans le système de planification. Peut être une tâche mère (conteneur)
    /// ou une sous-tâche technique générée par le planificateur.
    /// </summary>
    public class Tache
    {
        // --- Propriétés Structurelles (persistées) ---
        public string TacheId { get; set; } = "";
        public string IdImporte { get; set; } = "";
        public string TacheNom { get; set; } = "";
        public int HeuresHommeEstimees { get; set; }
        public string MetierId { get; set; } = "";
        public string Dependencies { get; set; } = "";
        public string ExclusionsDependances { get; set; } = "";
        public TypeActivite Type { get; set; } = TypeActivite.Tache;
        public bool EstJalon => Type != TypeActivite.Tache;
        public string LotId { get; set; } = "";
        public string BlocId { get; set; } = "";

        // --- Propriété de Hiérarchie (gérée par le système, persistée) ---
        public string ParentId { get; set; } // null ou vide pour les tâches mères

        // --- Propriétés enrichies (persistées) ---
        // Ces champs sont calculés par le TaskManagerService et sauvegardés avec le projet.

        public Statut Statut { get; set; } = Statut.Estimée;

        /// <summary>
        /// Flag de performance indiquant si cette tâche contient des sous-tâches.
        /// Calculé par TaskManagerService après une planification.
        /// </summary>
        public bool EstConteneur { get; set; } = false;

        public DateTime? DateDebutPlanifiee { get; set; }
        public DateTime? DateFinPlanifiee { get; set; }
        public List<AffectationOuvrier> Affectations { get; set; } = new List<AffectationOuvrier>();

        public override string ToString()
        {
            return $"{TacheId} - {TacheNom} ({HeuresHommeEstimees}h)";
        }
    }
}