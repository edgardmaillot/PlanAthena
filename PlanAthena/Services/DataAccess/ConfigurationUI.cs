namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Classe pour encapsuler la configuration de l'interface utilisateur
    /// </summary>
    /// Classe à refactoriser car elle fait doublon avec ProjetDTOs
    /// Egalement utilisée pour l'export exel des données ouvriers

    public class ConfigurationUI
    {
        public List<DayOfWeek> JoursOuvres { get; set; } = new List<DayOfWeek>();
        public int HeureDebutJournee { get; set; }
        public int HeuresTravailEffectifParJour { get; set; }
        public string TypeDeSortie { get; set; } = "Analyse et Estimation";
        public string Description { get; set; } = "";
        public DateTime? DateDebutSouhaitee { get; set; }
        public DateTime? DateFinSouhaitee { get; set; }
        public int DureeJournaliereStandardHeures { get; set; }
        public decimal PenaliteChangementOuvrierPourcentage { get; set; }
        public decimal CoutIndirectJournalierPourcentage { get; set; }
    }
}
