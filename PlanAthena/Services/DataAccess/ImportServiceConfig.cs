using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;

namespace PlanAthena
{
    /// <summary>
    /// Configuration simple pour POC - Pas de sur-ingénierie
    /// </summary>
    public static class ImportServiceConfig
    {
        /// <summary>
        /// Crée un ImportService avec les dépendances minimales
        /// </summary>
        public static ImportService CreerImportService(
            TacheService tacheService,
            ProjetService projetService,
            BlocService blocService)
        {
            var idGenerator = new IdGeneratorService(projetService, blocService, tacheService);

            return new ImportService(
                tacheService,
                projetService,
                blocService,
                idGenerator);
        }

        /// <summary>
        /// Crée un ImportOrchestrationService simple
        /// </summary>
        public static ImportOrchestrationService CreerProjetService(ImportService importService)
        {
            return new ImportOrchestrationService(importService);
        }
    }

    /// <summary>
    /// Représente la configuration de mappage entre les noms de colonnes d'un fichier CSV
    /// et les propriétés de l'objet Tache de PlanAthena.
    /// Inclut également les paramètres par défaut et options d'import.
    /// </summary>
    public class ImportMappingConfiguration
    {
        /// <summary>
        /// Indique si la première ligne du fichier CSV doit être traitée comme un en-tête.
        /// </summary>
        public bool HasHeaderRecord { get; set; } = true; // Nouvelle propriété

        // Propriétés représentant les noms de colonnes CSV mappées aux champs de la Tache PlanAthena.
        // Si une propriété est null ou vide, cela signifie que le champ correspondant n'a pas été mappé.
        public string CsvColumn_IdImporte { get; set; } = string.Empty;
        public string CsvColumn_TacheNom { get; set; } = string.Empty;
        public string CsvColumn_HeuresHommeEstimees { get; set; } = string.Empty;
        public string CsvColumn_MetierId { get; set; } = string.Empty;
        public string CsvColumn_BlocId { get; set; } = string.Empty;
        public string CsvColumn_Dependencies { get; set; } = string.Empty;
        public string CsvColumn_ExclusionsDependances { get; set; } = string.Empty;
        public string CsvColumn_EstJalon { get; set; } = string.Empty;

        // Paramètres additionnels configurés par l'utilisateur lors de l'import
        public bool CreerBlocParDefautSiNonSpecifie { get; set; } = true;
        public string NomBlocParDefaut { get; set; } = "Bloc Général";
        public int HeuresEstimeesParDefaut { get; set; } = 8;
        public int CapaciteMaxOuvriersBlocParDefaut { get; set; } = 6;
    }


    /// <summary>
    /// Service d'orchestration pour l'import uniquement - RENOMMÉ pour éviter les conflits
    /// </summary>
    public class ImportOrchestrationService
    {
        private readonly ImportService _importService;

        public ImportOrchestrationService(ImportService importService)
        {
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        }

        /// <summary>
        /// Orchestre l'import de tâches depuis un fichier CSV en utilisant une configuration de mappage.
        /// </summary>
        /// <param name="filePath">Chemin du fichier CSV.</param>
        /// <param name="lotIdCible">ID du lot de destination.</param>
        /// <param name="mappingConfig">Configuration de mappage des colonnes et paramètres d'import.</param>
        /// <param name="confirmerEcrasement">Indique si l'écrasement a déjà été confirmé.</param>
        /// <returns>Le résultat de l'import.</returns>
        public ImportResult ImporterTachesDepuisCsv(
            string filePath,
            string lotIdCible,
            ImportMappingConfiguration mappingConfig,
            bool confirmerEcrasement = false)
        {
            return _importService.ImporterTachesCSV(filePath, lotIdCible, mappingConfig, confirmerEcrasement);
        }
    }
}