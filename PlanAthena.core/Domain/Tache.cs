// PlanAthena.Core.Domain.Tache.cs
using PlanAthena.Core.Domain.Shared; // Assurez-vous que ce using est là
using PlanAthena.Core.Domain.ValueObjects;

namespace PlanAthena.Core.Domain
{
    // Implémentation de l'interface
    public class Tache : Entity<TacheId>, IHaveDependencies<TacheId>
    {
        public string Nom { get; }
        public BlocId BlocParentId { get; }
        public DureeHeuresHomme HeuresHommeEstimees { get; }
        public MetierId MetierRequisId { get; }

        // La propriété a été renommée pour correspondre à la discussion
        public IReadOnlySet<TacheId> Dependencies { get; }

        // La propriété IReadOnlySet est compatible avec IReadOnlyCollection<TId> de l'interface.
        IReadOnlyCollection<TacheId> IHaveDependencies<TacheId>.Dependencies => this.Dependencies;

        public Tache(
            TacheId id,
            string nom,
            BlocId blocParentId,
            DureeHeuresHomme heuresHommeEstimees,
            MetierId metierRequisId,
            IEnumerable<TacheId>? dependencies = null)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom de la tâche ne peut pas être vide.", nameof(nom));

            Nom = nom;
            BlocParentId = blocParentId;
            HeuresHommeEstimees = heuresHommeEstimees;
            MetierRequisId = metierRequisId;

            var tempDependencies = new HashSet<TacheId>();
            if (dependencies != null)
            {
                foreach (var dependencyId in dependencies)
                {
                    if (dependencyId.Equals(Id))
                        throw new InvalidOperationException("Une tâche ne peut pas dépendre d'elle-même.");
                    tempDependencies.Add(dependencyId);
                }
            }
            Dependencies = tempDependencies;
        }
    }
}