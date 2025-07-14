using NodaTime;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Input;

namespace PlanAthena.Core.Application.Interfaces;

/// <summary>
/// Service responsable de la création et de la gestion de la logique du calendrier ouvré.
/// </summary>
public interface ICalendrierService
{
    /// <summary>
    /// Crée un objet CalendrierOuvreChantier à partir de la définition fournie dans le DTO.
    /// (Utilisé par l'épique EF01)
    /// </summary>
    CalendrierOuvreChantier CreerCalendrierOuvreChantier(
        CalendrierTravailDefinitionDto definitionDto,
        DateTime? dateDebutSouhaiteeChantier,
        DateTime? dateFinSouhaiteeChantier);

    /// <summary>
    /// Crée une échelle de temps discrète à partir d'un calendrier de domaine et d'un horizon.
    /// (Nouvelle méthode pour l'épique EF02)
    /// </summary>
    /// <param name="calendrier">Le calendrier du chantier, définissant les jours et heures de travail.</param>
    /// <param name="dateDebut">La date de début de l'horizon de planification.</param>
    /// <param name="dateFin">La date de fin de l'horizon de planification.</param>
    /// <returns>Une échelle de temps discrète prête pour l'optimisation.</returns>
    EchelleTempsOuvree CreerEchelleTempsOuvree(
        CalendrierOuvreChantier calendrier,
        LocalDate dateDebut,
        LocalDate dateFin);
}