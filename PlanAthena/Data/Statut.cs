// Emplacement: /Services/DTOs/TaskManager/Statut.cs
// NOTE: Le dossier TaskManager doit être créé dans /Services/DTOs/

namespace PlanAthena.Data
{
    /// <summary>
    /// Représente l'état d'avancement d'une tâche.
    /// </summary>
    public enum Statut
    {
        /// <summary>
        /// Tâche créée mais non encore traitée par le planificateur.
        /// </summary>
        Estimée,

        /// <summary>
        /// Tâche placée dans le planning, future.
        /// </summary>
        Planifiée,

        /// <summary>
        /// Tâche dont la date de début est atteinte.
        /// </summary>
        EnCours,

        /// <summary>
        /// Tâche déclarée comme terminée manuellement par l'utilisateur. C'est un état final.
        /// </summary>
        Terminée,

        /// <summary>
        /// Tâche non terminée dont la date de fin est dépassée.
        /// </summary>
        EnRetard
    }
}