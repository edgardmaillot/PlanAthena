using NodaTime;

namespace PlanAthena.Core.Domain.ValueObjects;

/// <summary>
/// Représente un créneau temporel discret et indivisible dans l'échelle de temps,
/// servant de base à la planification par OR-Tools.
/// Le code doit parler : Ce VO est immuable et garantit la cohérence d'un slot.
/// </summary>
/// <param name="Index">Index unique et séquentiel du slot, utilisé par CP-SAT.</param>
/// <param name="Debut">Date et heure de début du slot.</param>
/// <param name="Fin">Date et heure de fin du slot.</param>
public record SlotTemporel(int Index, LocalDateTime Debut, LocalDateTime Fin)
{
    /// <summary>
    /// Calcule la durée exacte du slot.
    /// POURQUOI : Utile pour les calculs de capacité et la validation, sans avoir à le stocker.
    /// </summary>
    public Duration Duree => Period.Between(Debut, Fin).ToDuration();
    //public Period Duree => Period.Between(Debut, Fin);

    /// <summary>
    /// Vérifie si une date/heure donnée est incluse dans ce slot.
    /// La borne de début est inclusive, la borne de fin est exclusive, ce qui est standard pour les intervalles de temps.
    /// </summary>
    /// <param name="moment">Le moment à vérifier.</param>
    /// <returns>Vrai si le moment est dans le slot, sinon faux.</returns>
    public bool Contient(LocalDateTime moment) => moment >= Debut && moment < Fin;
}