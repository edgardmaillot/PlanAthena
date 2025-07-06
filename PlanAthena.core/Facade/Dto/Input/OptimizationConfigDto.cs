namespace PlanAthena.Core.Facade.Dto.Input;

/// <summary>
/// Contient les paramètres qui pilotent le processus d'optimisation.
/// </summary>
public record OptimizationConfigDto
{
    /// <summary>
    /// Spécifie le type de résultat attendu par le client.
    /// Ex: "Estimation", "PlanningDetaille".
    /// </summary>
    public required string TypeDeSortie { get; init; }

    /// <summary>
    /// Durée de travail standard d'un ouvrier par jour (en heures) avant le déclenchement des heures supplémentaires.
    /// </summary>
    public int DureeJournaliereStandardHeures { get; init; } = 7;

    /// <summary>
    /// Pénalité de coût (en pourcentage) appliquée si une tâche change d'ouvrier.
    /// </summary>
    public decimal PenaliteChangementOuvrierPourcentage { get; init; } = 30.0m;

    // Coût indirect en pourcentage de la masse salariale journalière.
    // Optionnel : si null, une valeur par défaut sera appliquée.
    public decimal? CoutIndirectJournalierPourcentage { get; init; }
}