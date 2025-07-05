// PlanAthena.Core.Facade.Dto.Input.LotTravauxDto.cs
using System;
using System.Collections.Generic;

namespace PlanAthena.Core.Facade.Dto.Input
{
    public record LotTravauxDto
    {
        public required string LotId { get; init; }
        public required string Nom { get; init; }

        // IDs des blocs regroupés par ce Lot.
        // Un Lot doit contenir au moins un BlocId.
        public required IReadOnlyList<string> BlocIds { get; init; }

        /// <summary>
        /// Priorité relative de ce Lot (ex: 1 = plus haute, 99 = plus basse).
        /// Le CdC doit fournir une priorité pour chaque lot.
        /// </summary>
        public required int Priorite { get; init; }

        /// <summary>
        /// Date de début souhaitée au plus tôt pour ce lot (indicative pour l'analyse préliminaire).
        /// </summary>
        public DateTime? DateDebutAuPlusTotSouhaitee { get; init; }

        /// <summary>
        /// Date de fin souhaitée au plus tard pour ce lot (indicative pour l'analyse préliminaire).
        /// </summary>
        public DateTime? DateFinAuPlusTardSouhaitee { get; init; }
    }
}