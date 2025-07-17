using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Data
{
    public class Ouvrier
    {
        public string OuvrierId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public int CoutJournalier { get; set; }
        public string MetierId { get; set; }
        public NiveauExpertise NiveauExpertise { get; set; }
        public int? PerformancePct { get; set; }
    }



}
