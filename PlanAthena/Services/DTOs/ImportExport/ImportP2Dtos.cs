// Version 0.5.1
namespace PlanAthena.Services.DTOs.ImportExport
{
    // --- DTO d'Entrée ---

    /// <summary>
    /// DTO fourni par l'Orchestrateur pour configurer l'écran ImportP2.
    /// </summary>
    public class ImportP2Config
    {
        public string EntityDisplayName { get; set; }
        public Image EntityImage { get; set; }
        public string ValueCategoryName { get; set; }
        public List<string> SourceValues { get; set; } = new List<string>();
        public List<TargetValueItem> TargetValues { get; set; } = new List<TargetValueItem>();
        public Dictionary<string, string> SuggestedMappings { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Représente une valeur cible générique (ex: un Métier, un Calendrier).
    /// </summary>
    public class TargetValueItem
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }

    // --- DTO de Sortie ---

    /// <summary>
    /// DTO exposé par ImportP2 contenant les décisions de mapping de valeurs de l'utilisateur.
    /// </summary>
    public class ImportP2Result
    {
        public bool ShouldMemorizeMappings { get; set; }
        public List<ValueMappingResult> AllMappingDecisions { get; set; } = new List<ValueMappingResult>();
    }

    /// <summary>
    /// Décrit la décision de mapping pour une seule valeur source.
    /// </summary>
    public class ValueMappingResult
    {
        public string SourceValue { get; set; }
        public MappingAction Action { get; set; }
        public string MappedTargetId { get; set; }
    }

    public enum MappingAction
    {
        MapToExisting,
        Ignore
    }
}