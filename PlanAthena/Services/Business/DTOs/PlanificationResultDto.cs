// Fichier: Services/Business/PlanificationResultDto.cs

using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Services.Processing;

namespace PlanAthena.Services.Business.DTOs
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

        // NOUVELLE PROPRIÉTÉ pour transporter la table de mappage
        public Dictionary<string, string> ParentIdParSousTacheId { get; set; } = new Dictionary<string, string>();

        // La propriété GanttConsolide n'est plus nécessaire ici. 
        public ConsolidatedGanttDto GanttConsolide { get; set; } = new ConsolidatedGanttDto();
    }
}