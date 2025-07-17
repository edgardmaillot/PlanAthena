using PlanAthena.Services.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// A renommer en Projet à la fin de la refactorisation
namespace PlanAthena.Data
{
    public class ProjetData
    {
        public InformationsProjet InformationsProjet { get; set; } = new InformationsProjet();
        public List<Metier> Metiers { get; set; } = new List<Metier>();
        public List<Ouvrier> Ouvriers { get; set; } = new List<Ouvrier>();
        public List<Tache> Taches { get; set; } = new List<Tache>();
        public DateTime DateSauvegarde { get; set; }
        public string VersionApplication { get; set; } = "";
    }
}
