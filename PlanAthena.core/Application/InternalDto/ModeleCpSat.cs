// Fichier : Application/InternalDto/ModeleCpSat.cs

using Google.OrTools.Sat;
using PlanAthena.Core.Domain.ValueObjects;
using System.Collections.Generic;

namespace PlanAthena.core.Application.InternalDto
{
    public record ModeleCpSat
    {
        public CpModel Model { get; init; }
        public IReadOnlyDictionary<TacheId, IntervalVar> TachesIntervals { get; init; }
        public IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> TachesAssignables { get; init; }
        public IntVar? Makespan { get; init; }

        // --- MODIFICATION : Décomposition explicite des coûts ---
        public IntVar? CoutTotal { get; init; }
        public IntVar? CoutRh { get; init; }
        public IntVar? CoutIndirect { get; init; }
    }
}