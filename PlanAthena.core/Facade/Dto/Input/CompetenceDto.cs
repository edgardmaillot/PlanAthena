// PlanAthena.Core.Facade.Dto.Input.CompetenceDto.cs
using PlanAthena.Core.Facade.Dto.Enums; // Pour NiveauExpertise

namespace PlanAthena.Core.Facade.Dto.Input
{
    public record CompetenceDto
    {
        public required string MetierId { get; init; }
        public NiveauExpertise Niveau { get; init; }

        /// <summary>
        /// Performance relative (pourcentage : 80, 100, 120). Si null, interprété comme 100%.
        /// </summary>
        public int? PerformancePct { get; init; }
    }
}