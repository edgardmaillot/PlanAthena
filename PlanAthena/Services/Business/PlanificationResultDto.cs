// Fichier: Services/Business/PlanificationResultDto.cs

using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Services.Processing;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Résultat complet d'une planification incluant les données brutes et consolidées
    /// </summary>
    public class PlanificationResultDto
    {
        /// <summary>
        /// Résultat brut retourné par le solveur (pour les logs et analyses détaillées)
        /// </summary>
        public ProcessChantierResultDto ResultatBrut { get; set; } = new ProcessChantierResultDto();

        /// <summary>
        /// Données consolidées pour l'export Gantt (structure hiérarchique)
        /// </summary>
        public ConsolidatedGanttDto GanttConsolide { get; set; } = new ConsolidatedGanttDto();
    }
}