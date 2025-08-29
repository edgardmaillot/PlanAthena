// Emplacement: /Services/Business/DTOs/LogPlanningDtos.cs

using System;
using System.Collections.Generic;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Représente le planning détaillé d'une journée spécifique, optimisé pour l'affichage dans les logs.
    /// </summary>
    public record LogPlanningJournalier
    {
        public DateTime Jour { get; init; }
        public IReadOnlyList<LogOuvrierDuJour> Ouvriers { get; init; } = new List<LogOuvrierDuJour>();
    }

    /// <summary>
    /// Représente les activités d'un ouvrier pour une journée donnée.
    /// </summary>
    public record LogOuvrierDuJour
    {
        public string NomOuvrier { get; init; } = string.Empty;
        public IReadOnlyList<LogAffectationDuJour> Affectations { get; init; } = new List<LogAffectationDuJour>();
    }

    /// <summary>
    /// Représente une affectation (un segment de travail) pour un ouvrier sur une journée.
    /// </summary>
    public record LogAffectationDuJour
    {
        public string TacheNom { get; init; } = string.Empty;
        public string BlocId { get; init; } = string.Empty;
        public double DureeHeures { get; init; }
    }
}