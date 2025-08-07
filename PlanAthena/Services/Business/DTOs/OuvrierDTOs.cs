using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanAthena.Services.Business.DTOs
{
        /// <summary>
        /// Informations consolidées sur un ouvrier
        /// </summary>
        public class OuvrierInfo
        {
            public string OuvrierId { get; set; } = "";
            public string Nom { get; set; } = "";
            public string Prenom { get; set; } = "";
            public int CoutJournalier { get; set; }
            public int NombreCompetences { get; set; }

            public string NomComplet => $"{Prenom} {Nom}";
        }

        /// <summary>
        /// Statistiques sur les ouvriers
        /// </summary>
        public class StatistiquesOuvriers
        {
            public int NombreOuvriersTotal { get; set; }
            public int NombreCompetencesTotal { get; set; }
            public double CoutJournalierMoyen { get; set; }
            public int CoutJournalierMin { get; set; }
            public int CoutJournalierMax { get; set; }
            public double NombreCompetencesParOuvrierMoyen { get; set; }
        }
}
