// Fichier: PlanAthena/Data/ProjetSummaryDto.cs
namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) contenant les informations minimales
    /// d'un projet pour un affichage rapide (ex: dans le Dashboard).
    /// </summary>
    public class ProjetSummaryDto
    {
        public string FilePath { get; set; }
        public string NomProjet { get; set; }
        public string Description { get; set; }
    }
}