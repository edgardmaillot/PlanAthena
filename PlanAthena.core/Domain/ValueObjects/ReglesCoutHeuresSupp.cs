using System;

namespace PlanAthena.Core.Domain.ValueObjects;

/// <summary>
/// POURQUOI : Ce VO encapsule les règles métier liées au calcul des heures supplémentaires.
/// Il extrait les "magic numbers" de la logique de calcul, rendant le système
/// plus facile à configurer et à faire évoluer si les règles changent.
/// </summary>
public record ReglesCoutHeuresSupp
{
    public int DureePalier1 { get; }
    public decimal MultiplicateurPalier1 { get; }
    public decimal MultiplicateurPalier2 { get; }

    public ReglesCoutHeuresSupp(int dureePalier1, decimal multiplicateurPalier1, decimal multiplicateurPalier2)
    {
        if (dureePalier1 <= 0) throw new ArgumentOutOfRangeException(nameof(dureePalier1));
        if (multiplicateurPalier1 < 1) throw new ArgumentOutOfRangeException(nameof(multiplicateurPalier1));
        if (multiplicateurPalier2 < 1) throw new ArgumentOutOfRangeException(nameof(multiplicateurPalier2));

        DureePalier1 = dureePalier1;
        MultiplicateurPalier1 = multiplicateurPalier1;
        MultiplicateurPalier2 = multiplicateurPalier2;
    }

    // Un constructeur par défaut avec les valeurs standard pour le POC
    public ReglesCoutHeuresSupp() : this(2, 1.25m, 1.50m) { }
}