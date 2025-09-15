namespace PlanAthena.Services.DTOs.ProjectPersistence
{
    /// <summary>
    /// Représente un résumé concis d'un projet pour affichage dans l'IHM,
    /// typiquement sur un écran d'accueil listant les projets récents.
    /// </summary>
    public class ProjetSummaryDto
    {
        public string FilePath { get; set; }
        public string NomProjet { get; set; }
        public string Description { get; set; }
        public int NombreTotalTaches { get; set; }
        public int NombreTachesTerminees { get; set; }
        public int NombreTachesEnRetard { get; set; }
        public bool ErreurLecture { get; set; }
        public string ImagePath { get; set; }
        public bool IsFavorite { get; set; }
    }


}