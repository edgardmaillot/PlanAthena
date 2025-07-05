// PlanAthena.Core.Facade.Dto.Input.BlocTravailDto.cs
namespace PlanAthena.Core.Facade.Dto.Input
{
    public record BlocTravailDto
    {
        public required string BlocId { get; init; }
        public required string Nom { get; init; }
        public required int CapaciteMaxOuvriers { get; init; }
    }
}