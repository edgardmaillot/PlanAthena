using Google.OrTools.Sat;
using PlanAthena.Core.Domain.ValueObjects;
using System.Collections.Generic;

namespace PlanAthena.core.Application.InternalDto
{
    /// <summary>
    /// Encapsule le modèle CP-SAT construit et les variables clés pour une utilisation ultérieure.
    /// POURQUOI : Isole la complexité du modèle OR-Tools et fournit un accès structuré
    /// aux variables qui seront nécessaires pour interpréter la solution.
    /// </summary>
    public record ModeleCpSat
    {
        public CpModel Model { get; init; }

        // POURQUOI : Ces dictionnaires sont la "clé de traduction" entre le monde OR-Tools et notre domaine.
        // Après la résolution, nous interrogerons ces variables pour construire le planning final.

        /// <summary>
        /// Dictionnaire associant chaque Tâche à sa variable d'intervalle.
        /// Représente le "QUAND".
        /// </summary>
        public IReadOnlyDictionary<TacheId, IntervalVar> TachesIntervals { get; init; }

        /// <summary>
        /// Dictionnaire associant chaque couple (Tâche, Ouvrier) à une variable booléenne.
        /// Représente le "QUI".
        /// </summary>
        public IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> TachesAssignables { get; init; }

        /// <summary>
        /// Durée totale estimée du planning, calculée comme le makespan du modèle.
        /// </summary>
        public IntVar? Makespan { get; init; }
        public IntVar? CoutTotal { get; init; }
    }
}
