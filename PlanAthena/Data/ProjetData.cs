using PlanAthena.Services.Business;

namespace PlanAthena.Data
{
    public class ProjetData
    {
        public InformationsProjet InformationsProjet { get; set; } = new InformationsProjet();
        public List<Metier> Metiers { get; set; } = new List<Metier>();
        public List<Ouvrier> Ouvriers { get; set; } = new List<Ouvrier>();
        public List<Tache> Taches { get; set; } = new List<Tache>();

        public List<Lot> Lots { get; set; } = new List<Lot>();
        public List<Bloc> Blocs { get; set; } = new List<Bloc>();

        public DateTime DateSauvegarde { get; set; }
        public string VersionApplication { get; set; } = "";
    }
}