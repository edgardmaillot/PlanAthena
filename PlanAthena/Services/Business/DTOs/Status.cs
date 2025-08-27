// Emplacement: /Services/Business/DTOs/Status.cs

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Représente l'état d'avancement d'une tâche.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// La tâche existe dans le projet mais n'a pas été affectée lors de la dernière planification.
        /// </summary>
        NonPlanifiee,

        /// <summary>
        /// La tâche a été affectée à une ou plusieurs ressources dans le planning actuel.
        /// </summary>
        Planifiee,

        /// <summary>
        /// La tâche a été déclarée comme étant en cours de réalisation.
        /// </summary>
        EnCours,

        /// <summary>
        /// La tâche a été déclarée comme terminée.
        /// </summary>
        Terminee,

        /// <summary>
        /// État calculé dynamiquement : la tâche n'est pas terminée et sa date de fin planifiée est dans le passé.
        /// </summary>
        EnRetard
    }
}