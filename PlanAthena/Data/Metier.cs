namespace PlanAthena.Data
{
    public class Metier
    {
        public string MetierId { get; set; }
        public string Nom { get; set; }
        public string PrerequisMetierIds { get; set; }
        public string CouleurHex { get; set; } = ""; // Couleur au format hexadécimal (#RRGGBB)
    }
}