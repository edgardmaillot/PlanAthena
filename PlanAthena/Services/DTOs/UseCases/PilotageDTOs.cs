// Emplacement: /Services/DTOs/UseCases/PilotageDTOs.cs Version 0.6.0 : Intègre les indicateurs EVM.
using PlanAthena.Data;

namespace PlanAthena.Services.DTOs.UseCases
{
    /// <summary>
    /// DTO contenant les indicateurs clés de performance (KPIs) pour le tableau de bord.
    /// </summary>
    public record CockpitKpiData
    {
        // Indicateurs existants
        public double ProgressionGlobalePourcentage { get; init; }
        public string LotLePlusARisqueNom { get; init; }
        public double LotLePlusARisqueDeriveJours { get; init; }
        public string MetierLePlusEnTensionNom { get; init; }
        public double MetierLePlusEnTensionTauxOccupation { get; init; }

        // Nouveaux indicateurs EVM
        public decimal BudgetAtCompletion { get; init; } // BAC
        public decimal EstimateAtCompletion { get; init; } // EAC
        public double ScheduleVarianceDays { get; init; }  // SV
        public decimal CostVariance { get; init; } // CV
        public double SchedulePerformanceIndex { get; init; } // SPI
        public double CostPerformanceIndex { get; init; } // CPI
    }

    /// <summary>
    /// DTO pour représenter une seule ligne formatée dans la DataGridView de TaskListView.
    /// </summary>
    public record TaskListItem
    {
        public string TacheId { get; init; }
        public string ParentId { get; init; }
        public string NomTache { get; init; }
        public string NomMetier { get; init; }
        public string NomsOuvriersAffectes { get; init; }
        public string Statut { get; init; }
        public bool EstConteneur { get; init; }
        public DateTime? DateDebutPlanifiee { get; init; }
        public DateTime? DateFinPlanifiee { get; init; }
        public DateTime? DateDebutReelle { get; init; }
        public DateTime? DateFinReelle { get; init; }
        public Tache TacheOriginale { get; init; }
    }

    /// <summary>
    /// DTO contenant toutes les données nécessaires à l'affichage du UserControl TaskListView.
    /// </summary>
    public record TaskListData
    {
        public List<TaskListItem> Items { get; init; } = new();
        public List<string> LotsDisponibles { get; init; } = new();
        public List<string> MetiersDisponibles { get; init; } = new();
    }

    /// <summary>
    /// DTO pour retourner le rapport de tension d'un métier.
    /// Utilisé par PlanningService et consommé par PilotageProjetUseCase.
    /// </summary>
    public record MetierTensionData
    {
        public string NomMetier { get; init; }
        public double TauxOccupation { get; init; }
    }
    public enum ProjectWeatherStatus
    {
        Sunny,      // Tout va bien
        Cloudy,     // Orages en vue
        Rainy,      // Problèmes à gérer
        Stormy      // Situation critique
    }

    public record ProjectWeatherData
    {
        public double DerivPlanningJours { get; init; }
        public double DisponibiliteRessourcesPourcentage { get; init; }
        public double DeviationBudgetPourcentage { get; init; } // Toujours 0 en V1
        public ProjectWeatherStatus Statut { get; init; }
    }
    /// <summary>
    /// Représente un bloc de tâche à afficher dans la grille du planning.
    /// </summary>
    public record PlanningBlock
    {
        public string TacheId { get; init; }
        public string TacheNom { get; init; }
        public Statut Statut { get; init; }
        public double StartOffsetPercent { get; init; }
        public double WidthPercent { get; init; }
    }

    /// <summary>
    /// DTO contenant toutes les données préparées pour l'affichage de la PlanningView.
    /// </summary>
    public record PlanningViewData
    {
        // La clé du dictionnaire est l'ID de l'ouvrier.
        // La valeur est une liste de PlanningBlock pour cet ouvrier, indexée par jour (0 = premier jour de la période).
        public Dictionary<string, List<PlanningBlock>[]> BlocksParOuvrier { get; init; } = new();

        public List<Ouvrier> Ouvriers { get; init; } = new();
        public Dictionary<string, Metier> Metiers { get; set; } = new Dictionary<string, Metier>();
        public Dictionary<string, Color> MetierColors { get; set; } = new Dictionary<string, Color>();
        public List<DateTime> Jours { get; init; } = new();
    }
}