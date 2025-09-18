// Fichier: /Services/DTOs/UseCases/EvmReportDto.cs Version 0.6.0

namespace PlanAthena.Services.DTOs.UseCases
{
    /// <summary>
    /// DTO contenant le rapport complet des indicateurs EVM (Earned Value Management)
    /// calculés à une date de référence donnée.
    /// </summary>
    public record EvmReportDto
    {
        /// <summary>
        /// Indique si une baseline existe, permettant aux consommateurs de savoir si les données sont valides.
        /// </summary>
        public bool BaselineExists { get; init; }

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