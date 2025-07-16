namespace PlanAthena.Data
{
    public class MetierRecord
    {
        public string MetierId { get; set; }
        public string Nom { get; set; }
        public string PrerequisMetierIds { get; set; } // On le splittera plus tard
        public string CouleurHex { get; set; } = ""; // Couleur au format hexadécimal (#RRGGBB)
    }
}