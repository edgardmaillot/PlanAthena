using PlanAthena.Core.Domain;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanAthena.Core.Application.Interfaces
{
    public interface ICapacityValidationService
    {
        Task<List<MessageValidationDto>> ValidateCapacityConstraintsAsync(ChantierSetupInputDto inputDto, Chantier chantier);
    }

    public interface ICrossReferenceValidationService
    {
        Task<List<MessageValidationDto>> ValidateCrossReferencesAsync(ChantierSetupInputDto inputDto, Chantier? chantier);
    }
}
