// Fichier: /Data/PlanningBaseline.cs Version 0.6.0

using PlanAthena.Services.Business.DTOs;

namespace PlanAthena.Data
{
    /// <summary>
    /// Contient une photographie immuable ("baseline") du projet à un instant T,
    /// généralement après la première planification réussie.
    /// Toutes les métriques de gestion de la valeur acquise (EVM) sont calculées
    /// par rapport à cette ligne de base.
    /// </summary>
    public class PlanningBaseline
    {
        /// <summary>
        /// La date et l'heure à laquelle cette baseline a été créée.
        /// </summary>
        public DateTime DateCreation { get; set; }

        /// <summary>
        /// Le Budget à l'Achèvement (BAC) : coût total planifié du projet au moment de la baseline.
        /// </summary>
        public decimal BudgetAtCompletion { get; set; }

        /// <summary>
        /// La date de fin du projet telle que planifiée dans cette baseline.
        /// </summary>
        public DateTime DateFinPlanifieeInitiale { get; set; }

        /// <summary>
        /// Dictionnaire représentant la courbe de la Valeur Planifiée (PV) cumulative.
        /// La clé est la date, la valeur est le coût total qui aurait dû être engagé jusqu'à cette date.
        /// </summary>
        public Dictionary<DateTime, decimal> CourbePlannedValueCumulative { get; set; }

        /// <summary>
        /// Dictionnaire contenant le budget initial (RH uniquement) pour chaque tâche mère.
        /// La clé est l'ID de la tâche mère. Utilisé pour calculer la Valeur Acquise (EV).
        /// </summary>
        public Dictionary<string, decimal> BudgetInitialParTacheId { get; set; }

        /// <summary>
        /// Une copie du planning consolidé tel qu'il existait lors de la création de la baseline.
        /// </summary>
        public ConsolidatedPlanning ConsPlanningInitial { get; set; }

        /// <summary>
        /// Une copie de la configuration de planification utilisée pour générer la baseline.
        /// </summary>
        public ConfigurationPlanification ConfigurationInitiale { get; set; }
    }
}