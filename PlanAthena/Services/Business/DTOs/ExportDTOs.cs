// Fichier: Services/Export/DTOs/PlanningExportDTOs.cs

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// DTO principal contenant toutes les données consolidées pour l'export Excel
    /// </summary>
    public class PlanningExportDto
    {
        public string NomProjet { get; set; } = "";
        public string TypeSortie { get; set; } = ""; // NOUVEAU : Type d'optimisation
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public decimal CoutTotal { get; set; }
        public int DureeJours { get; set; }

        public List<SyntheseMetierDto> Metiers { get; set; } = new List<SyntheseMetierDto>();
        public List<PlanningOuvrierDto> Ouvriers { get; set; } = new List<PlanningOuvrierDto>();
    }

    /// <summary>
    /// Statistiques par métier pour l'onglet synthèse
    /// </summary>
    public class SyntheseMetierDto
    {
        public string NomMetier { get; set; } = "";
        public double TauxOccupation { get; set; }
        public double HeuresTravaillees { get; set; }
        public List<string> NomsOuvriers { get; set; } = new List<string>();
        public string CouleurHex { get; set; } = "#CCCCCC";

        /// <summary>
        /// Chaîne formatée des ouvriers pour affichage Excel
        /// </summary>
        public string OuvriersFormates => string.Join(" + ", NomsOuvriers);
    }

    /// <summary>
    /// Planning détaillé d'un ouvrier pour son onglet individuel
    /// </summary>
    public class PlanningOuvrierDto
    {
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public string NomComplet => $"{Prenom} {Nom}".Trim();
        public string Metier { get; set; } = "";
        public double TauxOccupation { get; set; }
        public double TauxFragmentation { get; set; } // NOUVEAU KPI
        public double HeuresTravaillees { get; set; }

        public List<CreneauTravailDto> Creneaux { get; set; } = new List<CreneauTravailDto>();
    }

    /// <summary>
    /// Créneau de travail d'un ouvrier pour une journée donnée
    /// </summary>
    public class CreneauTravailDto
    {
        public DateTime Date { get; set; }
        public string TacheNom { get; set; } = "";
        public string BlocId { get; set; } = "";
        public int DureeHeures { get; set; }
        public bool EstJourVide { get; set; }

        /// <summary>
        /// Nom du jour de la semaine (Lundi, Mardi, etc.)
        /// </summary>
        public string NomJour => Date.ToString("dddd", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));

        /// <summary>
        /// Durée formatée pour affichage Excel
        /// </summary>
        public string DureeFormatee => EstJourVide ? "---" : $"{DureeHeures}h";

        /// <summary>
        /// Nom de la tâche formaté pour affichage Excel
        /// </summary>
        public string TacheFormatee => EstJourVide ? "---------------" : TacheNom;
    }

    /// <summary>
    /// Configuration pour personnaliser l'export Excel
    /// </summary>
    public class ConfigurationExport
    {
        public bool InclureJoursVides { get; set; } = true;
        public bool AfficherBlocIds { get; set; } = true;
        public bool FormatageAvance { get; set; } = true;
        public string NomFichier { get; set; } = "";

        /// <summary>
        /// Génère le nom de fichier par défaut
        /// </summary>
        public static string GenererNomFichierDefaut(string nomProjet)
        {
            var nomNettoye = NettoyerNomFichier(nomProjet);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            return $"Planning_{nomNettoye}_{timestamp}.xlsx";
        }

        /// <summary>
        /// Nettoie un nom de fichier des caractères interdits
        /// </summary>
        private static string NettoyerNomFichier(string nom)
        {
            if (string.IsNullOrEmpty(nom))
                return "Chantier";

            var caracteresInterdits = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            var nomNettoye = nom;

            foreach (var c in caracteresInterdits)
            {
                nomNettoye = nomNettoye.Replace(c, '_');
            }

            return nomNettoye.Length > 50 ? nomNettoye.Substring(0, 50) : nomNettoye;
        }
    }
}