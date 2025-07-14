// PlanAthena.Core.Facade.Dto.Input.MetierDto.cs
namespace PlanAthena.Core.Facade.Dto.Input
{
    public record MetierDto
    {
        public required string MetierId { get; init; }
        public required string Nom { get; init; }

        // Liste des MetierId d'autres métiers qui sont des prérequis techniques généraux pour celui-ci.
        public IReadOnlyList<string> PrerequisMetierIds { get; init; } = Array.Empty<string>();
    }
}