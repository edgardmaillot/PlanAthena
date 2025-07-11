// Fichier : Facade/Dto/Output/PlanningOptimizationResultDto.cs

using System;
using System.Collections.Generic;
using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Core.Facade.Dto.Output
{
    public record PlanningOptimizationResultDto
    {
        public string ChantierId { get; init; }
        public OptimizationStatus Status { get; init; }

        // --- MODIFICATIONS ---
        // Décomposition des coûts (en centimes)
        public long? CoutTotalEstime { get; init; }
        public long? CoutTotalRhEstime { get; init; }
        public long? CoutTotalIndirectEstime { get; init; }

        // Renommé pour plus de clarté
        public long? DureeTotaleEnSlots { get; init; }

        // Ajout de la liste détaillée des affectations
        public IReadOnlyList<AffectationDto> Affectations { get; init; } = Array.Empty<AffectationDto>();
    }

    // Ce sous-DTO est ajouté au même fichier pour la simplicité
    public record AffectationDto
    {
        public required string TacheId { get; init; }
        public required string TacheNom { get; init; }
        public required string OuvrierId { get; init; }
        public required string OuvrierNom { get; init; }
        public required string BlocId { get; init; }
        public required DateTime DateDebut { get; init; }
        public required long DureeHeures { get; init; }
    }
}