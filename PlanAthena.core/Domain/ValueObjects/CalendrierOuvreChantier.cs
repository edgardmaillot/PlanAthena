// PlanAthena.Core.Domain.ValueObjects.CalendrierOuvreChantier.cs
using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace PlanAthena.Core.Domain.ValueObjects
{
    public record CalendrierOuvreChantier
    {
        public IReadOnlySet<IsoDayOfWeek> JoursOuvres { get; }
        public LocalTime HeureDebutTravail { get; }

        // On revient à Duration, qui est le type correct pour une durée fixe.
        public Duration DureeTravailEffectiveParJour { get; }

        // On supprime la propriété HeureFinTravail qui causait les problèmes.
        // Elle sera calculée à la volée si nécessaire.

        public IReadOnlySet<LocalDate> JoursChomes { get; }

        public CalendrierOuvreChantier(
            IReadOnlySet<IsoDayOfWeek> joursOuvres,
            LocalTime heureDebutTravail,
            Duration dureeTravailEffectiveParJour,
            IReadOnlySet<LocalDate> joursChomes)
        {
            if (joursOuvres == null || joursOuvres.Count == 0)
                throw new ArgumentException("Au moins un jour ouvré doit être défini.", nameof(joursOuvres));

            // CORRECTION FINALE: La validation correcte avec Duration.
            if (dureeTravailEffectiveParJour <= Duration.Zero)
                throw new ArgumentOutOfRangeException(nameof(dureeTravailEffectiveParJour), "La durée de travail doit être positive.");

            JoursOuvres = joursOuvres;
            HeureDebutTravail = heureDebutTravail;
            DureeTravailEffectiveParJour = dureeTravailEffectiveParJour;
            JoursChomes = joursChomes ?? new HashSet<LocalDate>();
        }

        public bool EstJourOuvre(LocalDate date)
        {
            if (JoursChomes.Contains(date)) return false;
            return JoursOuvres.Contains(date.DayOfWeek);
        }
    }
}