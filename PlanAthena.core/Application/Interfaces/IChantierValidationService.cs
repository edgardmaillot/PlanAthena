// PlanAthena.Core.Application.Interfaces.IChantierValidationService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PlanAthena.Core.Domain; // Pour l'entité Chantier (si validation post-mapping)
using PlanAthena.Core.Facade.Dto.Input;    // Si validation sur DTOs
using PlanAthena.Core.Facade.Dto.Output; // Pour MessageValidationDto

namespace PlanAthena.Core.Application.Interfaces
{
    /// <summary>
    /// Service responsable de la validation complète des données d'un chantier,
    /// incluant les validations structurelles, de références croisées et de cycles.
    /// </summary>
    public interface IChantierValidationService
    {
        /// <summary>
        /// Valide l'ensemble des données d'entrée du chantier.
        /// Cette méthode englobe :
        /// 1. La validation syntaxique et structurelle des DTOs (P2 - souvent déléguée à FluentValidation en amont).
        /// 2. La validation des références croisées (P3 - ex: un TacheDto.BlocId existe bien).
        /// 3. La détection de cycles de dépendances (P4 - ex: cycles entre TacheDto, cycles entre LotTravauxDto).
        /// </summary>
        /// <param name="inputDto">Les données d'entrée complètes du chantier.</param>
        /// <param name="chantierDomaine">Optionnel: L'objet Chantier du domaine déjà mappé, si certaines validations
        ///                              sont plus faciles à effectuer sur le modèle de domaine.</param>
        /// <returns>Une liste de messages de validation (erreurs ou avertissements).</returns>
        Task<List<MessageValidationDto>> ValiderChantierCompletAsync(
            ChantierSetupInputDto inputDto,
            Chantier? chantierDomaine = null);
        // Note Edgard: Laisser chantierDomaine optionnel donne la flexibilité de valider certaines choses
        // sur les DTOs et d'autres sur le modèle domaine si déjà construit.
        // Si chantierDomaine est null, toutes les validations se font sur inputDto.
    }
}