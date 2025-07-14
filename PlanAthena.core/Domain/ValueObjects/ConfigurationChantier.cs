// PlanAthena.Core.Domain.ValueObjects.ConfigurationChantier.cs
namespace PlanAthena.Core.Domain.ValueObjects
{
    // Ce VO représente la configuration fournie par le CdC, mappée depuis ConfigurationChefChantierDto.
    // Il est simple et contient des listes d'IDs typés.
    public record ConfigurationChantier
    {
        public IReadOnlySet<OuvrierId> OuvriersClefs { get; }
        public IReadOnlySet<MetierId> MetiersClefs { get; }

        // Note Edgard: Si nous avions gardé les Jalons, ils seraient ici sous forme de VOs Domaine.

        public ConfigurationChantier(
            IEnumerable<OuvrierId>? ouvriersClefs = null,
            IEnumerable<MetierId>? metiersClefs = null)
        {
            OuvriersClefs = ouvriersClefs?.ToHashSet() ?? new HashSet<OuvrierId>();
            MetiersClefs = metiersClefs?.ToHashSet() ?? new HashSet<MetierId>();
        }

        // Constructeur pratique pour mapper depuis les listes de string du DTO
        public static ConfigurationChantier CreerDepuisIds(
            IEnumerable<string>? ouvriersClefsIds,
            IEnumerable<string>? metiersClefsIds)
        {
            var ouvrierIds = ouvriersClefsIds?.Select(id => new OuvrierId(id)) ?? Enumerable.Empty<OuvrierId>();
            var metierIds = metiersClefsIds?.Select(id => new MetierId(id)) ?? Enumerable.Empty<MetierId>();
            return new ConfigurationChantier(ouvrierIds, metierIds);
        }

        public bool AUneConfigurationSpecifique => OuvriersClefs.Any() || MetiersClefs.Any();
    }
}