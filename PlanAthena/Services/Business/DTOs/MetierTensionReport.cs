// Emplacement: /Services/Business/DTOs/MetierTensionReport.cs

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Rapport d'analyse de la tension sur les métiers, basé sur les "ouvriers clés" 
    /// identifiés par l'analyse rapide de la DLL.
    /// </summary>
    public record MetierTensionReport
    {
        /// <summary>
        /// Conclusion textuelle interprétant la répartition des métiers en tension.
        /// Ex: "Un métier en tension forte", "Équilibrage des compétences correct".
        /// </summary>
        public string Conclusion { get; init; } = string.Empty;

        /// <summary>
        /// Liste détaillée de la répartition des métiers parmi les ouvriers clés.
        /// </summary>
        public List<MetierCount> Repartition { get; init; } = new();
    }

    /// <summary>
    /// Représente le décompte d'un métier spécifique.
    /// </summary>
    public record MetierCount
    {
        public string MetierId { get; init; } = string.Empty;
        public int Count { get; init; }
    }
}