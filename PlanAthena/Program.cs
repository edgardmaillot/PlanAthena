using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application;
using PlanAthena.Core.Facade;
using PlanAthena.Core.Infrastructure;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Processing;
using PlanAthena.Services.Usecases;
using PlanAthena.Services.UseCases;
using PlanAthena.Utilities;
using PlanAthena.View;
using System;
using System.Windows.Forms;

namespace PlanAthena
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            var mainShell = serviceProvider.GetRequiredService<MainShellForm>();
            Application.Run(mainShell);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // --- Core DLL (inchangé) ---
            services.AddApplicationServices();
            services.AddInfrastructureServices();
            services.AddScoped<PlanAthena.Core.Application.Interfaces.IConstructeurProblemeOrTools, PlanAthena.Core.Infrastructure.Services.OrTools.ConstructeurProblemeOrTools>();
            services.AddScoped<PlanAthenaCoreFacade>();

            // --- Services de l'application principale ---

            // 1. Sources de Vérité (Stateful, Singletons)
            services.AddSingleton<ProjetService>();
            services.AddSingleton<RessourceService>();
            services.AddSingleton<PlanningService>();
            services.AddSingleton<TaskStatusService>();

            // 2. Use Cases / Orchestrateurs (Stateless/Stateful selon le rôle)
            // ApplicationService est gardé pour la configuration de session
            services.AddSingleton<ApplicationService>();
            services.AddSingleton<ImportService>();
            services.AddSingleton<PlanificationOrchestrator>();
            // --- NOUVEAU USE CASE DE PERSISTANCE ---
            services.AddSingleton<ProjectPersistenceUseCase>();

            // 3. Utilitaires (Stateless, Singletons)
            services.AddSingleton<IIdGeneratorService, IdGeneratorService>();
            services.AddSingleton<PreparationSolveurService>();
            services.AddSingleton<DataTransformer>();
            services.AddSingleton<PlanningConsolidationService>();
            services.AddSingleton<AnalysisService>();

            // 4. Présentateurs / Exports (Stateless, Singletons)
            services.AddSingleton<PlanningExcelExportService>();
            services.AddSingleton<GanttExportService>();

            // 5. Infrastructure & Accès aux Données (Singletons)
            // --- NOUVELLE COUCHE D'ACCÈS AUX DONNÉES ---
            services.AddSingleton<ProjetServiceDataAccess>();
            services.AddSingleton<UserPreferencesService>();
            services.AddSingleton<CsvDataService>();
            services.AddSingleton<ExcelReader>();
            services.AddSingleton<CheminsPrefereService>();

            // --- SERVICE OBSOLÈTE SUPPRIMÉ ---
            // services.AddSingleton<ProjetRepository>(); // SUPPRIMÉ

            // Le DependanceBuilder est probablement encore utilisé par l'UI, on le garde pour l'instant.
            services.AddSingleton<DependanceBuilder>();

            // Enregistrement des formulaires et vues
            services.AddSingleton<MainShellForm>();
        }
    }
}