// Emplacement: /Services/DTOs/ImportExport/ExportDataProjetDto.cs

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using System.Collections.Generic;

namespace PlanAthena.Services.DTOs.ImportExport
{
    /// <summary>
    /// DTO d'entrée unique pour tous les services d'export liés à un projet.
    /// Il agrège toutes les données nécessaires, déjà calculées et consolidées,
    /// prêtes à être mises en forme par les Présentateurs.
    /// </summary>
    public class ExportDataProjetDto
    {
        /// <summary>
        /// Le nom du projet, utilisé pour les titres et noms de fichiers.
        /// </summary>
        public string NomProjet { get; set; }

        /// <summary>
        /// La configuration de planification qui a été utilisée pour générer le planning.
        /// </summary>
        public ConfigurationPlanification Configuration { get; set; }

        /// <summary>
        /// Le planning consolidé, découpé en segments de travail journaliers.
        /// Fournit le détail jour par jour des affectations avec les horaires précis.
        /// </summary>
        public ConsolidatedPlanning Planning { get; set; }

        /// <summary>
        /// Le rapport d'analyse contenant tous les KPIs pré-calculés (coûts, taux, métier principal, etc.).
        /// </summary>
        public AnalysisReport Report { get; set; }

        /// <summary>
        /// La structure complète du projet (Lots, Blocs, Tâches et leurs dépendances).
        /// Utilisé principalement pour l'export Gantt afin de reconstruire la hiérarchie.
        /// </summary>
        public ProjetData ProjetStructure { get; set; }

        /// <summary>
        /// La liste "vivante" et complète de tous les ouvriers du projet,
        /// provenant de RessourceService. C'est la source de vérité
        /// pour les informations comme le coût journalier.
        /// </summary>
        public IReadOnlyList<Ouvrier> PoolOuvriers { get; set; }
    }
}