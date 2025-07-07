using PlanAthena.core.Application.InternalDto;

namespace PlanAthena.Core.Application.Interfaces;

/// <summary>
/// Définit le service responsable de la traduction du domaine métier
/// en un modèle mathématique solvable par CP-SAT.
/// </summary>
public interface IConstructeurProblemeOrTools
{
    /// <summary>
    /// Construit le modèle CP-SAT à partir des données du problème.
    /// </summary>
    /// <param name="probleme">Le problème d'optimisation à traduire.</param>
    /// <returns>Le modèle CP-SAT encapsulé, prêt à être résolu.</returns>
    ModeleCpSat ConstruireModele(ProblemeOptimisation probleme, string objectif);
}