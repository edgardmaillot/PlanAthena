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

        // --- NOUVELLES PROPRIÉTÉS POUR LA PRÉCÉDENCE DES LOTS ---
        // Ces dictionnaires stockent les variables du solveur représentant le début et la fin
        // de chaque lot et de chaque groupe de priorité, permettant leur accès post-résolution.
        public IReadOnlyDictionary<LotId, IntVar>? LotStarts { get; init; }         // Variables de début pour chaque lot
        public IReadOnlyDictionary<LotId, IntVar>? LotEnds { get; init; }           // Variables de fin pour chaque lot
        public IReadOnlyDictionary<int, IntVar>? PriorityGroupStarts { get; init; } // Variables de début pour chaque groupe de priorité
        public IReadOnlyDictionary<int, IntVar>? PriorityGroupEnds { get; init; }   // Variables de fin pour chaque groupe de priorité
    }
}