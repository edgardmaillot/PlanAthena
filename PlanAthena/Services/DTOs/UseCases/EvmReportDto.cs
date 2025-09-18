// Fichier: /Services/DTOs/UseCases/EvmReportDto.cs Version 0.7.0

namespace PlanAthena.Services.DTOs.UseCases
{
    /// <summary>
    /// DTO contenant le rapport complet des indicateurs EVM (Earned Value Management)
    /// calculés à une date de référence donnée.
    /// Version 0.7.0 : Ajout de tous les indicateurs dérivés pour éviter les recalculs
    /// </summary>
    public record EvmReportDto
    {
        /// <summary>
        /// Indique si une baseline existe, permettant aux consommateurs de savoir si les données sont valides.
        /// </summary>
        public bool BaselineExists { get; init; }

        #region Indicateurs de base EVM

        /// <summary>
        /// Budget à l'Achèvement (BAC) - Le coût total initialement planifié.
        /// </summary>
        public decimal BudgetAtCompletion { get; init; }

        /// <summary>
        /// Valeur Planifiée (PV) - Le budget qui aurait dû être dépensé à la date de référence.
        /// </summary>
        public decimal PlannedValue { get; init; }

        /// <summary>
        /// Valeur Acquise (EV) - La valeur du travail réellement accompli à la date de référence.
        /// </summary>
        public decimal EarnedValue { get; init; }

        /// <summary>
        /// Coût Réel (AC) - Le coût réel du travail accompli à la date de référence.
        /// </summary>
        public decimal ActualCost { get; init; }

        #endregion

        #region Indicateurs de variance

        /// <summary>
        /// Variance de Planning (SV = EV - PV) - Écart entre le travail accompli et planifié
        /// </summary>
        public decimal ScheduleVariance { get; init; }

        /// <summary>
        /// Variance de Planning en jours - Conversion de SV en équivalent temps
        /// </summary>
        public double ScheduleVarianceDays { get; init; }

        /// <summary>
        /// Variance de Coût (CV = EV - AC) - Écart entre valeur acquise et coût réel
        /// </summary>
        public decimal CostVariance { get; init; }

        #endregion

        #region Indices de performance

        /// <summary>
        /// Indice de Performance de Planning (SPI = EV / PV)
        /// </summary>
        public double SchedulePerformanceIndex { get; init; }

        /// <summary>
        /// Indice de Performance de Coût (CPI = EV / AC)
        /// </summary>
        public double CostPerformanceIndex { get; init; }

        #endregion

        #region Estimations à l'achèvement

        /// <summary>
        /// Estimation à l'Achèvement (EAC) - Coût total estimé du projet
        /// </summary>
        public decimal EstimateAtCompletion { get; init; }

        /// <summary>
        /// Plan to Complete (PTC = BAC - PV) - Budget restant selon le plan
        /// </summary>
        public decimal PlanToComplete { get; init; }

        /// <summary>
        /// Estimate to Complete (ETC) - Estimation du coût pour terminer
        /// </summary>
        public decimal EstimateToComplete { get; init; }

        /// <summary>
        /// Variance à l'Achèvement (VAC = BAC - EAC)
        /// </summary>
        public decimal VarianceAtCompletion { get; init; }

        #endregion

        #region Informations contextuelles

        /// <summary>
        /// Date de référence pour laquelle les calculs ont été effectués
        /// </summary>
        public DateTime DateReference { get; init; }

        /// <summary>
        /// Pourcentage d'avancement planifié à la date de référence
        /// </summary>
        public double AvancementPlanifiePourcentage { get; init; }

        /// <summary>
        /// Pourcentage d'avancement réel à la date de référence
        /// </summary>
        public double AvancementReelPourcentage { get; init; }

        /// <summary>
        /// Déviation budgétaire en pourcentage par rapport au BAC
        /// </summary>
        public double DeviationBudgetPourcentage { get; init; }

        #endregion
    }

    /// <summary>
    /// DTO contenant les données pré-formatées pour l'affichage du graphique EVM.
    /// </summary>
    public record EvmGraphData
    {
        public bool BaselineExists { get; init; }
        public List<DateTime> Dates { get; init; } = new();
        public List<double> PlannedValues { get; init; } = new();
        public List<double> EarnedValues { get; init; } = new();
        public List<double> ActualCosts { get; init; } = new();
        public decimal BudgetAtCompletion { get; init; }
    }

    /// <summary>
    /// DTO contenant les données pré-formatées pour le graphique ETC vs PTC.
    /// </summary>
    public record EtcVsPtcGraphData
    {
        public bool BaselineExists { get; init; }
        public List<DateTime> Dates { get; init; } = new();
        public List<double> PlanToCompleteValues { get; init; } = new();
        public List<double> EstimateToCompleteValues { get; init; } = new();
    }
}