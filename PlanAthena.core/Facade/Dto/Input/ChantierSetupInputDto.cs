// PlanAthena.Core.Facade.Dto.Input.ChantierSetupInputDto.cs
using System;
using System.Collections.Generic;
using PlanAthena.Core.Facade.Dto.Enums; // Pour FlexibiliteDate

namespace PlanAthena.Core.Facade.Dto.Input
{
    public record ChantierSetupInputDto
    {
        public required string ChantierId { get; init; }
        public required string Description { get; init; }
        public DateTime? DateDebutSouhaitee { get; init; }
        public DateTime? DateFinSouhaitee { get; init; }
        public FlexibiliteDate FlexibiliteDebut { get; init; } = FlexibiliteDate.Flexible;
        public FlexibiliteDate FlexibiliteFin { get; init; } = FlexibiliteDate.Flexible;

        public required CalendrierTravailDefinitionDto CalendrierTravail { get; init; }

        public required IReadOnlyList<BlocTravailDto> Blocs { get; init; }
        public required IReadOnlyList<TacheDto> Taches { get; init; }
        public required IReadOnlyList<LotTravauxDto> Lots { get; init; }
        public required IReadOnlyList<OuvrierDto> Ouvriers { get; init; }
        public required IReadOnlyList<MetierDto> Metiers { get; init; }

        // Configuration optionnelle fournie par le CdC.
        public ConfigurationChefChantierDto? ConfigurationCdC { get; init; }

        /// <summary>
        /// Contient les paramètres spécifiques à la demande d'optimisation.
        /// </summary>
        public OptimizationConfigDto? OptimizationConfig { get; init; }
    }
}