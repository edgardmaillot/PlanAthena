using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;

namespace PlanAthena.core.Application.InternalDto;
/// <summary>
/// DTO interne regroupant toutes les données nécessaires pour construire un problème d'optimisation.
/// POURQUOI : Cet objet sert de contrat de données clair et stable entre le cas d'usage
/// et les services d'infrastructure chargés de l'optimisation. Il est immuable.
/// </summary>
public record ProblemeOptimisation
{
    public Chantier Chantier { get; init; }
    public EchelleTempsOuvree EchelleTemps { get; init; }

    // La configuration est extraite du chantier pour un accès direct et facile.
    public ConfigurationOptimisation Configuration { get; init; }


}
