// PlanAthena.Core.Facade.Dto.Input.OuvrierDto.cs
namespace PlanAthena.Core.Facade.Dto.Input
{
    public record OuvrierDto
    {
        public required string OuvrierId { get; init; }
        public required string Nom { get; init; }
        public required string Prenom { get; init; }
        public required IReadOnlyList<CompetenceDto> Competences { get; init; }

        /// <summary>
        /// Coût journalier de base pour cet ouvrier, en euros entiers.
        /// </summary>
        public required int CoutJournalier { get; init; }
    }
}