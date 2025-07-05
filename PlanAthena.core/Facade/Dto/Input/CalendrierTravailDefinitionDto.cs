// PlanAthena.Core.Facade.Dto.Input.CalendrierTravailDefinitionDto.cs
using System;
using System.Collections.Generic;

namespace PlanAthena.Core.Facade.Dto.Input
{
    public record CalendrierTravailDefinitionDto
    {
        public required IReadOnlyList<DayOfWeek> JoursOuvres { get; init; }
        public required int HeureDebutJournee { get; init; } // Heure entière (0-23)
        public required int HeuresTravailEffectifParJour { get; init; } // Heures entières
        public IReadOnlyList<DateTime> JoursChomes { get; init; } = Array.Empty<DateTime>();
    }
}