using PlanAthena.Data;

namespace PlanAthena.Services.DTOs.ImportExport
{
    /// <summary>
    /// Contient le plan d'importation pour les Tâches,
    /// avec les nouveaux blocs et les nouvelles tâches à créer.
    /// </summary>
    public class TachesImportPlan
    {
        public List<Bloc> NouveauxBlocs { get; set; } = new List<Bloc>();
        public List<Tache> NouvellesTaches { get; set; } = new List<Tache>();
    }
}