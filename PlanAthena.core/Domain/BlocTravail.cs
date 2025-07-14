// PlanAthena.Core.Domain.BlocTravail.cs
using PlanAthena.Core.Domain.ValueObjects;

namespace PlanAthena.Core.Domain
{
    public class BlocTravail : Entity<BlocId>
    {
        public string Nom { get; }
        public CapaciteOuvriers CapaciteMaxOuvriers { get; } // VO

        // Les tâches sont détenues et gérées par le BlocTravail.
        // Le dictionnaire permet un accès rapide par TacheId.
        private readonly Dictionary<TacheId, Tache> _taches = new Dictionary<TacheId, Tache>();
        public IReadOnlyDictionary<TacheId, Tache> Taches => _taches;

        // Ce champ sera renseigné par l'agrégat Chantier lors de l'association Lot-Bloc.
        // Il n'est pas directement fourni par le BlocTravailDto.
        // Il est 'internal set' pour que seul l'assemblage du domaine puisse le définir.
        public LotId? LotParentId { get; internal set; }

        public BlocTravail(
            BlocId id,
            string nom,
            CapaciteOuvriers capaciteMaxOuvriers,
            IEnumerable<Tache>? tachesInitiales = null) // Les tâches sont passées déjà construites
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom du bloc ne peut pas être vide.", nameof(nom));

            Nom = nom;
            CapaciteMaxOuvriers = capaciteMaxOuvriers; // Le VO a déjà validé la capacité

            if (tachesInitiales != null)
            {
                foreach (var tache in tachesInitiales)
                {
                    // Valider que la tâche appartient bien à ce bloc (son BlocParentId doit correspondre)
                    if (!tache.BlocParentId.Equals(Id))
                        throw new InvalidOperationException($"La tâche '{tache.Nom}' (ID: {tache.Id}) n'appartient pas au bloc '{Nom}' (ID: {Id}). Elle est assignée au bloc ID: {tache.BlocParentId}.");

                    if (_taches.ContainsKey(tache.Id))
                        throw new InvalidOperationException($"Une tâche avec l'ID '{tache.Id}' existe déjà dans le bloc '{Nom}'.");

                    _taches.Add(tache.Id, tache);
                }
            }
        }

        public DureeHeuresHomme CalculerChargeTotalHeuresHomme()
        {
            int totalHeures = 0;
            foreach (var tache in _taches.Values)
            {
                totalHeures += tache.HeuresHommeEstimees.Value;
            }
            return new DureeHeuresHomme(totalHeures);
        }

        // Aucune méthode de modification d'état après construction pour le POC.
        // Si on devait ajouter/retirer des tâches dynamiquement, il faudrait des méthodes ici
        // qui maintiendraient les invariants.
    }
}