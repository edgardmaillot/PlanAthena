using PlanAthena.Services.Business;
using System.Text.Json.Serialization;

namespace PlanAthena.Data
{
    public class ProjetData
    {
        public InformationsProjet InformationsProjet { get; set; } = new InformationsProjet();
        public List<Metier> Metiers { get; set; } = new List<Metier>();
        public List<Ouvrier> Ouvriers { get; set; } = new List<Ouvrier>();
        public List<Tache> Taches { get; set; } = new List<Tache>();

        public List<Lot> Lots { get; set; } = new List<Lot>();
        public List<Bloc> Blocs { get; set; } = new List<Bloc>();

        public DateTime DateSauvegarde { get; set; }
        public string VersionApplication { get; set; } = "";
    }

    // Définition de la classe Metier déplacée ici
    public class Metier
    {
        public string MetierId { get; set; }
        public string Nom { get; set; }
        public string PrerequisMetierIds { get; set; }
        public string CouleurHex { get; set; } = ""; // Couleur au format hexadécimal (#RRGGBB)
        public string Pictogram { get; set; } = "";

        [JsonConverter(typeof(JsonStringEnumConverter))] // Ajout de cet attribut
        public ChantierPhase Phases { get; set; } = ChantierPhase.None;
    }
}