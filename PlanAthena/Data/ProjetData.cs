using PlanAthena.Services.Business.DTOs; // Pour Statut
using PlanAthena.Services.DTOs.Projet; // Pour ConsolidatedPlanning et ConfigurationPlanification

namespace PlanAthena.Data
{
    public class ProjetData
    {
        public InformationsProjet InformationsProjet { get; set; }
        public List<Metier> Metiers { get; set; }
        public List<Ouvrier> Ouvriers { get; set; }
        public List<Tache> Taches { get; set; }
        public List<Lot> Lots { get; set; }

        [Obsolete("Les blocs sont maintenant imbriqués dans les lots. Laisser pour compatibilité ascendante.")]
        public List<Bloc> Blocs { get; set; }

        // --- NOUVELLES PROPRIÉTÉS POUR LA PERSISTANCE COMPLÈTE ---

        /// <summary>
        /// L'état du dernier planning généré.
        /// </summary>
        public ConsolidatedPlanning Planning { get; set; }

        /// <summary>
        /// La configuration de planification utilisée pour générer le planning.
        /// </summary>
        public ConfigurationPlanification Configuration { get; set; }

        /// <summary>
        /// L'état d'avancement (statut) de chaque tâche.
        /// La clé est le TacheId.
        /// </summary>
        public Dictionary<string, Statut> TaskStatuses { get; set; }

        public ProjectSummaryData Summary { get; set; }
        // --- MÉTADONNÉES DE SAUVEGARDE ---

        public DateTime DateSauvegarde { get; set; }
        public string VersionApplication { get; set; }
    }
}