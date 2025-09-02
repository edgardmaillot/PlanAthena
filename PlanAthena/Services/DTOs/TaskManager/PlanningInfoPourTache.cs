// Emplacement: /Services/DTOs/TaskManager/PlanningInfoPourTache.cs

using PlanAthena.Data;

namespace PlanAthena.Services.DTOs.TaskManager
{
    /// <summary>
    /// DTO utilisé par PlanningService pour transmettre au TaskManagerService 
    /// les informations agrégées d'une tâche suite à une planification.
    /// Ces informations concernent une tâche "mère" (conceptuelle).
    /// </summary>
    public record PlanningInfoPourTache
    {
        /// <summary>
        /// Indique si la tâche a été découpée en sous-tâches (et est donc un conteneur).
        /// </summary>
        public bool EstConteneur { get; init; }

        /// <summary>
        /// La date de début la plus précoce parmi toutes les sous-tâches.
        /// </summary>
        public DateTime DateDebut { get; init; }

        /// <summary>
        /// La date de fin la plus tardive parmi toutes les sous-tâches.
        /// </summary>
        public DateTime DateFin { get; init; }

        /// <summary>
        /// La liste agrégée de toutes les affectations d'ouvriers sur les sous-tâches.
        /// </summary>
        public List<AffectationOuvrier> Affectations { get; init; } = new();
    }
}