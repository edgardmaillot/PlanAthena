// PlanAthena.Core.Facade.Dto.Output.MessageValidationDto.cs
using PlanAthena.Core.Facade.Dto.Enums; // Pour TypeMessageValidation

namespace PlanAthena.Core.Facade.Dto.Output
{
    public record MessageValidationDto
    {
        public required TypeMessageValidation Type { get; init; }
        public required string CodeMessage { get; init; } // Ex: "ERR_REF_001", "WARN_CYCLE_LOT"
        public required string Message { get; init; } // Description lisible
        public string? ElementId { get; init; } // ID de l'élément concerné (TacheId, LotId, etc.)
        public string? ProprieteConcernee { get; init; } // Nom de la propriété du DTO en erreur
    }
}