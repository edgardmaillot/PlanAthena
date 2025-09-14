// Emplacement: /Services/Business/DTOs/AnalysisReport.cs

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Contient le rapport d'analyse complet d'un planning.
    /// Utilise des records pour garantir l'immutabilité des données une fois calculées.
    /// </summary>
    public record AnalysisReport
    {
        public SyntheseProjetReport SyntheseProjet { get; init; } = new();
        public List<AnalyseOuvrierReport> AnalysesOuvriers { get; init; } = new();
        public Dictionary<DateTime, int> ChargeJournaliere { get; init; } = new();
    }

    /// <summary>
    /// Synthèse des indicateurs clés (KPIs) au niveau du projet.
    /// </summary>
    public record SyntheseProjetReport
    {
        /// <summary>
        /// Coût total de la main-d'œuvre (RH).
        /// Calcul : Somme de (Jours travaillés uniques par ouvrier * CoutJournalier de l'ouvrier).
        /// </summary>
        public decimal CoutTotalRh { get; init; }

        /// <summary>
        /// Coût total des frais indirects.
        /// Calcul : (Durée totale en jours ouvrés du projet * CoutIndirectJournalierAbsolu).
        /// </summary>
        public decimal CoutTotalIndirect { get; init; }

        /// <summary>
        /// Coût total du projet (RH + Indirect).
        /// </summary>
        public decimal CoutTotalProjet { get; init; }

        /// <summary>
        /// Durée totale du projet en jours ouvrés, de la première à la dernière affectation.
        /// </summary>
        public int DureeJoursOuvres { get; init; }

        /// <summary>
        /// Effort total en jours-homme.
        /// Calcul : Somme des jours de présence uniques pour chaque ouvrier.
        /// </summary>
        public double EffortTotalJoursHomme { get; init; }
    }

    /// <summary>
    /// Analyse détaillée des KPIs pour un ouvrier spécifique.
    /// </summary>
    public record AnalyseOuvrierReport
    {
        public string OuvrierId { get; init; } = string.Empty;
        public string NomComplet { get; init; } = string.Empty;

        /// <summary>
        /// Nombre total d'heures travaillées par l'ouvrier.
        /// </summary>
        public double HeuresTravaillees { get; init; }

        /// <summary>
        /// Nombre de jours uniques où l'ouvrier a travaillé.
        /// </summary>
        public int JoursTravailles { get; init; }

        /// <summary>
        /// Taux d'occupation de l'ouvrier sur ses jours de présence.
        /// Calcul : (Total Heures Travaillées) / (Nombre Jours Travaillés * HeuresTravailEffectifParJour).
        /// </eummary>
        public double TauxOccupation { get; init; }

        /// <summary>
        /// Indicateur de la dispersion du travail de l'ouvrier dans le temps.
        /// Un taux élevé signifie beaucoup de jours "off" entre des jours travaillés.
        /// Calcul : 1 - (Nombre Jours Travaillés / Nombre Jours Ouvrés entre sa première et sa dernière affectation).
        /// </summary>
        public double TauxFragmentation { get; init; }
        /// <summary>
        /// L'identifiant du métier principal de l'ouvrier.
        /// </summary>
        public string MetierPrincipalId { get; init; } = string.Empty;

        /// <summary>
        /// Le nom du métier principal de l'ouvrier.
        /// </summary>
        public string MetierPrincipalNom { get; init; } = string.Empty;
    }
}