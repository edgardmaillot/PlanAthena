// Fichier: Services/Business/DTOs/ConfDTOs.cs

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Configuration de planification et calculs métier
    /// Anciennement ConfigurationUI - refactorisé pour une meilleure organisation
    /// Utilisé pour l'export Excel, les calculs KPI et la planification
    /// </summary>
    public class ConfigurationPlanification
    {
        public List<DayOfWeek> JoursOuvres { get; set; } = new List<DayOfWeek>();
        public int HeureDebutJournee { get; set; }
        public int HeuresTravailEffectifParJour { get; set; }
        public string TypeDeSortie { get; set; } = "Analyse et Estimation";
        public string Description { get; set; } = "Test";
        public DateTime? DateDebutSouhaitee { get; set; }
        public DateTime? DateFinSouhaitee { get; set; }
        public int DureeJournaliereStandardHeures { get; set; }
        public decimal PenaliteChangementOuvrierPourcentage { get; set; }
        public int DureeCalculMaxMinutes { get; set; }

        public long CoutIndirectJournalierAbsolu { get; set; }
        //Conserver cette data jusqu'à la mise à jour du Core
        public decimal CoutIndirectJournalierPourcentage { get; set; }
        public int SeuilJoursDecoupageTache { get; set; } = 2;
    }
}