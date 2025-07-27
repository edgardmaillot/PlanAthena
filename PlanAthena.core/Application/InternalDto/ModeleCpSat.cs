// Fichier : Application/InternalDto/ModeleCpSat.cs

using Google.OrTools.Sat;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;

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

        // *** AJOUT: Métadonnées pour corriger l'affichage des jalons ***
        // Ces 3 propriétés permettent au SolutionInterpreterService de distinguer
        // les tâches normales (qui utilisent SizeExpr du solveur) des jalons 
        // (qui doivent utiliser leur durée originale pour l'export Gantt)
        public IReadOnlyDictionary<TacheId, double>? DureesOriginalesHeures { get; init; }
        public IReadOnlyDictionary<TacheId, TypeActivite>? TypesActivites { get; init; }
        public IReadOnlyDictionary<TacheId, string>? NomsActivites { get; init; }
    }
}