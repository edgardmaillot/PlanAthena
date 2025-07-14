// PlanAthena.Core.Application.Interfaces.IChantierSetupInputMapper.cs
using PlanAthena.Core.Domain; // Pour l'entité Chantier
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output; // Pour MessageValidationDto

namespace PlanAthena.Core.Application.Interfaces
{
    /// <summary>
    /// Responsable du mapping des DTOs d'entrée vers le modèle de domaine (Agrégat Chantier).
    /// Peut également effectuer des validations de format de base et retourner des erreurs de mapping.
    /// </summary>
    public interface IChantierSetupInputMapper
    {
        /// <summary>
        /// Mappe le DTO d'entrée vers l'agrégat racine Chantier du domaine.
        /// </summary>
        /// <param name="inputDto">Le DTO d'entrée contenant les données du chantier.</param>
        /// <returns>
        /// Un tuple contenant l'objet Chantier du domaine (s'il a pu être construit, même partiellement)
        /// et une liste de messages de validation (erreurs ou avertissements) survenus pendant le mapping
        /// ou la construction des objets du domaine due à des données invalides.
        /// Si des erreurs critiques sont rencontrées, l'objet Chantier peut être null.
        /// </returns>
        Task<(Chantier? chantier, List<MessageValidationDto> messages)> MapToDomainAsync(ChantierSetupInputDto inputDto);
    }
}