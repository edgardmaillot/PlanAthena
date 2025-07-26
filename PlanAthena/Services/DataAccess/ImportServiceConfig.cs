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
            LotService lotService,
            BlocService blocService,
            MetierService metierService)
        {
            var idGenerator = new IdGeneratorService(lotService, blocService, tacheService);

            return new ImportService(
                tacheService,
                lotService,
                blocService,
                metierService,
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

        public ImportResult ImporterTachesDepuisCsv(string filePath, string lotIdCible, bool confirmerEcrasement = false)
        {
            return _importService.ImporterTachesCSV(filePath, lotIdCible, confirmerEcrasement);
        }
    }
}