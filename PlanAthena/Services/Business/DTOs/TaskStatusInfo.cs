// Emplacement: /Services/Business/DTOs/TaskStatusInfo.cs

using System;
using System.Collections.Generic;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// DTO enrichi qui représente l'état complet et contextuel d'une tâche pour l'affichage.
    /// Il agrège les informations de plusieurs services (Projet, Planning, Ressources).
    /// </summary>
    public record TaskStatusInfo
    {
        public string TacheId { get; init; } = string.Empty;
        public string TacheNom { get; init; } = string.Empty;
        public Status Statut { get; init; }
        public DateTime? DateDebutPlanifiee { get; init; }
        public DateTime? DateFinPlanifiee { get; init; }
        public IReadOnlyList<string> NomsOuvriersAssignes { get; init; } = new List<string>();
        public bool EstTacheConteneur { get; init; }
    }

    /// <summary>
    /// Enumération pour filtrer les types de tâches dans les requêtes.
    /// </summary>
    public enum TaskTypeFilter
    {
        Leaf,      // Tâches "feuilles", directement réalisables
        Container  // Tâches "conteneurs", qui résument des sous-tâches
    }
}