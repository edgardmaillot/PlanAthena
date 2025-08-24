// Fichier: Services/Business/DTOs/AnalysePlanificationDto.cs
// 🆕 NOUVEAU - DTOs pour le rapport d'analyse post-planification

using System;
using System.Collections.Generic;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// DTO principal contenant le rapport d'analyse complet d'une planification.
    /// C'est l'objet produit par le PlanningResultatService et consommé par l'UI et l'export Excel.
    /// </summary>
    public class AnalysePlanificationDto
    {
        public SyntheseProjetDto SyntheseProjet { get; set; } = new SyntheseProjetDto();
        public List<AnalyseOuvrierDto> AnalyseOuvriers { get; set; } = new List<AnalyseOuvrierDto>();
    }

    /// <summary>
    /// Contient les KPIs globaux et la synthèse du projet planifié.
    /// </summary>
    public class SyntheseProjetDto
    {
        public string NomProjet { get; set; } = "N/A";
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int DureeJoursCalendaires { get; set; }

        // --- KPIs Financiers ---
        public long? CoutTotalEstime { get; set; }
        public long? CoutTotalRhEstime { get; set; }
        public long? CoutTotalIndirectEstime { get; set; }

        // --- KPIs d'Effort ---
        public int TotalJoursHommeTravailles { get; set; }
        public List<SyntheseMetierAnalyseDto> SyntheseParMetier { get; set; } = new List<SyntheseMetierAnalyseDto>();

    }

    /// <summary>
    /// Contient les KPIs détaillés pour un ouvrier.
    /// </summary>
    public class AnalyseOuvrierDto
    {
        public string OuvrierId { get; set; } = "";
        public string NomComplet { get; set; } = "";
        public string MetierPrincipal { get; set; } = "";
        public int CoutJournalier { get; set; }
        public List<string> AutresMetiers { get; set; } = new List<string>();

        // --- KPIs d'Activité ---
        public int JoursTravaillesUniques { get; set; }
        public double HeuresTravaillees { get; set; }

        // --- KPIs d'Efficacité ---
        public double TauxOccupation { get; set; }
        public double TauxFragmentation { get; set; }
    }

    /// <summary>
    /// Contient la synthèse de l'effort par métier.
    /// </summary>
    public class SyntheseMetierAnalyseDto
    {
        public string MetierId { get; set; } = "";
        public string NomMetier { get; set; } = "";
        public int TotalJoursHommeTravailles { get; set; }
    }
}