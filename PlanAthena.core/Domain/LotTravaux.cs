// PlanAthena.Core.Domain.LotTravaux.cs
using System;
using System.Collections.Generic;
using System.Linq;
using PlanAthena.Core.Domain.ValueObjects;
// using NodaTime; // Si on utilisait LocalDate pour les dates souhaitées

namespace PlanAthena.Core.Domain
{
    public class LotTravaux : Entity<LotId>
    {
        public string Nom { get; }
        public int Priorite { get; } // Requis par le DTO

        // Pour les dates souhaitées, nous utilisons DateTime? pour la simplicité dans le VO
        // mais en interne, si NodaTime était utilisé plus largement, ce seraient des LocalDate?.
        public DateTime? DateDebutAuPlusTotSouhaitee { get; }
        public DateTime? DateFinAuPlusTardSouhaitee { get; }

        // IDs des Blocs appartenant à ce Lot.
        // Ce set est construit à partir de LotTravauxDto.BlocIds.
        private readonly HashSet<BlocId> _blocIds = new HashSet<BlocId>();
        public IReadOnlySet<BlocId> BlocIds => _blocIds;

        public LotTravaux(
            LotId id,
            string nom,
            int priorite,
            IEnumerable<BlocId> blocIds, // Les BlocId sont passés déjà construits (en tant que VOs)
            DateTime? dateDebutAuPlusTotSouhaitee = null,
            DateTime? dateFinAuPlusTardSouhaitee = null)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom du lot ne peut pas être vide.", nameof(nom));

            if (blocIds == null || !blocIds.Any())
                throw new ArgumentException("Un lot doit contenir au moins un BlocId.", nameof(blocIds));

            // Priorité pourrait avoir une plage de validation si nécessaire, ex: > 0
            if (priorite <= 0)
                throw new ArgumentOutOfRangeException(nameof(priorite), "La priorité doit être un entier positif.");

            if (dateDebutAuPlusTotSouhaitee.HasValue && dateFinAuPlusTardSouhaitee.HasValue &&
                dateDebutAuPlusTotSouhaitee.Value > dateFinAuPlusTardSouhaitee.Value)
            {
                throw new ArgumentException("La date de début souhaitée ne peut pas être postérieure à la date de fin souhaitée pour le lot.");
            }

            Nom = nom;
            Priorite = priorite;
            DateDebutAuPlusTotSouhaitee = dateDebutAuPlusTotSouhaitee;
            DateFinAuPlusTardSouhaitee = dateFinAuPlusTardSouhaitee;

            // Initialiser le HashSet des BlocIds
            _blocIds = new HashSet<BlocId>(blocIds);
        }

        public bool ContientBloc(BlocId blocId) => _blocIds.Contains(blocId);

        // Pas de dépendances de lot directes sur cette entité pour le MVP.
        // Pas de ContrainteLot complexe pour le MVP.
        // Aucune méthode de modification d'état après construction pour le POC.
    }
}