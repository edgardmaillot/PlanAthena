// Fichier: Services/Processing/GanttDto.cs

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// DTO racine pour l'export Gantt consolidé
    /// </summary>
    public class ConsolidatedGanttDto
    {
        /// <summary>
        /// Nom du projet
        /// </summary>
        public string NomProjet { get; set; } = "";

        /// <summary>
        /// Liste des tâches racines (tâches mères et tâches non découpées)
        /// </summary>
        public List<GanttTaskItem> TachesRacines { get; set; } = new List<GanttTaskItem>();

        /// <summary>
        /// Date de génération du Gantt
        /// </summary>
        public DateTime DateGeneration { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Représente une tâche dans l'arborescence Gantt avec ses enfants
    /// </summary>
    public class GanttTaskItem
    {
        /// <summary>
        /// Identifiant unique de la tâche
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// Nom de la tâche
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Date de début (heure précise)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Date de fin (heure précise)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Nom de la ressource assignée (pour les tâches feuilles)
        /// Pour les tâches mères, contient la liste des ressources des enfants
        /// </summary>
        public string AssignedResourceName { get; set; } = "";

        /// <summary>
        /// Durée totale en heures
        /// </summary>
        public double DurationHours { get; set; }

        /// <summary>
        /// Identifiant du bloc
        /// </summary>
        public string BlocId { get; set; } = "";

        /// <summary>
        /// Identifiant du lot
        /// </summary>
        public string LotId { get; set; } = "";

        /// <summary>
        /// Indique si c'est une tâche mère (résumé) ou une tâche feuille
        /// </summary>
        public bool EstTacheMere => Children.Any();

        /// <summary>
        /// Liste des sous-tâches (pour les tâches mères)
        /// </summary>
        public List<GanttTaskItem> Children { get; set; } = new List<GanttTaskItem>();

        /// <summary>
        /// Dépendances de la tâche (IDs des tâches dont elle dépend)
        /// </summary>
        public List<string> Dependencies { get; set; } = new List<string>();
    }
}