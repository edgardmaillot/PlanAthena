using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
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