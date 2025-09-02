namespace PlanAthena.Services.Business.DTOs
{
    public class JalonPlanifie
    {
        public string TacheId { get; set; }
        public string TacheNom { get; set; }
        public string BlocId { get; set; }
        public DateTime DateDebut { get; set; } // L'heure de début exacte
        public DateTime DateFin { get; set; }   // L'heure de fin exacte
        public double DureeHeures { get; set; } // La durée totale
    }
}
