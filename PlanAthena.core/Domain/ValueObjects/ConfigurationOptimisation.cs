using System;

namespace PlanAthena.Core.Domain.ValueObjects;

/// <summary>
/// Représente les paramètres de configuration d'une optimisation.
/// Ce Value Object garantit la validité et l'immutabilité des règles de l'optimisation.
/// </summary>
public record ConfigurationOptimisation
{
    public int DureeJournaliereStandardHeures { get; }
    public decimal PenaliteChangementOuvrierPourcentage { get; }

    public ConfigurationOptimisation(int dureeJournaliereStandardHeures, decimal penaliteChangementOuvrierPourcentage)
    {
        // POURQUOI : Un cas d'usage de la validation dans le constructeur, comme pour CapaciteOuvriers.
        // Garantit que l'objet ne peut pas exister dans un état invalide.
        if (dureeJournaliereStandardHeures <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dureeJournaliereStandardHeures), "La durée journalière standard doit être strictement positive.");
        }

        if (penaliteChangementOuvrierPourcentage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(penaliteChangementOuvrierPourcentage), "La pénalité ne peut pas être négative.");
        }

        DureeJournaliereStandardHeures = dureeJournaliereStandardHeures;
        PenaliteChangementOuvrierPourcentage = penaliteChangementOuvrierPourcentage;
    }
}