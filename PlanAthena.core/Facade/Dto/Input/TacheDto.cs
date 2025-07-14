// PlanAthena.Core.Facade.Dto.Input.TacheDto.cs
namespace PlanAthena.Core.Facade.Dto.Input
{
    public record TacheDto
    {
        public required string TacheId { get; init; }
        public required string Nom { get; init; }
        public required string BlocId { get; init; }
        public required int HeuresHommeEstimees { get; init; } // Peut être 0.
        public required string MetierId { get; init; }

        // Liste des TacheId des tâches (du même BlocId) dont cette tâche dépend directement.
        public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    }
}