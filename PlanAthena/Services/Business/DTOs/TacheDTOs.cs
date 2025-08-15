// Fichier: PlanAthena/Services/Business/DTOs/TacheDTOs.cs
// Version: 0.4.4
// Description: Nouveau fichier pour centraliser les DTOs relatifs aux Tâches, incluant les statistiques.

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Statistiques sur l'ensemble des tâches d'un projet.
    /// </summary>
    public class StatistiquesTaches
    {
        public int NombreTachesTotal { get; set; }
        public int HeuresHommeTotal { get; set; }
        public double HeuresHommeMoyenneParTache { get; set; }
        public int NombreBlocsUniques { get; set; }
        public int NombreLotsUniques { get; set; }
        public int TachesAvecDependances { get; set; }
        public int JalonsSurcharge { get; set; }
    }

    /// <summary>
    /// Statistiques sur le mapping entre les tâches et les métiers.
    /// </summary>
    public class StatistiquesMappingMetiers
    {
        public int TotalTaches { get; set; }
        public int TachesAvecMetier { get; set; }
        public int TachesSansMetier { get; set; }
        public double PourcentageMapping { get; set; }
    }
}