using PlanAthena.Core.Facade.Dto.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanAthena.CsvModels
{
    public class OuvrierCsvRecord
    {
        public string OuvrierId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public int CoutJournalier { get; set; }
        public string MetierId { get; set; }
        public NiveauExpertise NiveauExpertise { get; set; }
        public int? PerformancePct { get; set; }
    }

    // CsvModels/MetierCsvRecord.cs
    public class MetierCsvRecord
    {
        public string MetierId { get; set; }
        public string Nom { get; set; }
        public string PrerequisMetierIds { get; set; } // On le splittera plus tard
    }

    // CsvModels/TacheCsvRecord.cs
    public class TacheCsvRecord
    {
        public string TacheId { get; set; }
        public string TacheNom { get; set; }
        public int HeuresHommeEstimees { get; set; }
        public string MetierId { get; set; }
        public string Dependencies { get; set; } // On le splittera plus tard
        public string LotId { get; set; }
        public string LotNom { get; set; }
        public int LotPriorite { get; set; }
        public string BlocId { get; set; }
        public string BlocNom { get; set; }
        public int BlocCapaciteMaxOuvriers { get; set; }
    }
}
