using PlanAthena.Data;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// DTO pour une tâche importée depuis un CSV avec informations de lot et bloc
    /// </summary>
    public class TacheImportee : Tache
    {
        public string LotNom { get; set; } = "";
        public int LotPriorite { get; set; }
        public string BlocNom { get; set; } = "";
        public int BlocCapaciteMaxOuvriers { get; set; }
    }

    /// <summary>
    /// Résultat d'une opération d'import
    /// </summary>
    public class ImportResult
    {
        public bool EstSucces { get; set; }
        public string MessageErreur { get; set; } = "";
        public int NbTachesImportees { get; set; }
        public int NbLotsTraites { get; set; }
        public int NbBlocsTraites { get; set; }
        public List<string> Warnings { get; set; } = new();
        public TimeSpan DureeImport { get; set; }
        public bool ConfirmationRequise { get; set; }
        public string MessageConfirmation { get; set; } = "";

        public static ImportResult Succes(int nbTaches, int nbLots, int nbBlocs, List<string> warnings, TimeSpan duree) =>
            new()
            {
                EstSucces = true,
                NbTachesImportees = nbTaches,
                NbLotsTraites = nbLots,
                NbBlocsTraites = nbBlocs,
                Warnings = warnings,
                DureeImport = duree
            };

        public static ImportResult Echec(string erreur) =>
            new() { EstSucces = false, MessageErreur = erreur };

        public static ImportResult DemandeConfirmation(string message) =>
            new()
            {
                EstSucces = false,
                ConfirmationRequise = true,
                MessageConfirmation = message
            };
    }

    /// <summary>
    /// Résultat de validation de structure CSV
    /// </summary>
    public class ValidationResult
    {
        public bool EstValide => !ErreursBloquantes.Any();
        public List<string> ErreursBloquantes { get; set; } = new();
        public List<string> Avertissements { get; set; } = new();

        public ValidationResult() { }

        public ValidationResult(List<string> erreurs)
        {
            ErreursBloquantes = erreurs ?? new List<string>();
        }
    }

    /// <summary>
    /// Configuration pour les règles de génération d'ID
    /// </summary>
    public class ConfigurationIds
    {
        public string FormatLot { get; set; } = "L{0:D3}";
        public string FormatBloc { get; set; } = "{0}_B{1:D3}";
        public string FormatTache { get; set; } = "{0}_T{1:D3}";
        public string FormatJalon { get; set; } = "{0}_J{1:D3}";
    }
}