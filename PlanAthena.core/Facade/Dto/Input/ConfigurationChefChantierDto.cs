// PlanAthena.Core.Facade.Dto.Input.ConfigurationChefChantierDto.cs
using System; // Pour Array.Empty
using System.Collections.Generic;

namespace PlanAthena.Core.Facade.Dto.Input
{
    // Ce DTO lui-même est optionnel dans ChantierSetupInputDto
    public record ConfigurationChefChantierDto
    {
        public IReadOnlyList<string> OuvriersClefsIds { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> MetiersClefsIds { get; init; } = Array.Empty<string>();
    }
}