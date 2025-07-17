using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Classe pour encapsuler la configuration de l'interface utilisateur
    /// </summary>
    public class ConfigurationUI
    {
        public List<DayOfWeek> JoursOuvres { get; set; } = new List<DayOfWeek>();
        public int HeureDebutJournee { get; set; }
        public int HeuresTravailEffectifParJour { get; set; }
        public string TypeDeSortie { get; set; } = "Analyse et Estimation";
        public string Description { get; set; } = "";
        public DateTime? DateDebutSouhaitee { get; set; }
        public DateTime? DateFinSouhaitee { get; set; }
        public int DureeJournaliereStandardHeures { get; set; }
        public decimal PenaliteChangementOuvrierPourcentage { get; set; }
        public decimal CoutIndirectJournalierPourcentage { get; set; }
    }
}
