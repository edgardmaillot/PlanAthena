// Emplacement: /Program.cs

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
using System.Linq; // Ajouté pour .Any()
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
            // --- Core DLL ---
            services.AddApplicationServices();
            services.AddInfrastructureServices();
            services.AddScoped<PlanAthena.Core.Application.Interfaces.IConstructeurProblemeOrTools, PlanAthena.Core.Infrastructure.Services.OrTools.ConstructeurProblemeOrTools>();
            services.AddScoped<PlanAthenaCoreFacade>();

            // --- Services de l'application principale ---

            // 1. Utilitaires & Infrastructure
            services.AddSingleton<IIdGeneratorService, IdGeneratorService>();
            services.AddSingleton<UserPreferencesService>();
            services.AddSingleton<CsvDataService>();
            services.AddSingleton<ExcelReader>();
            services.AddSingleton<CheminsPrefereService>();
            services.AddSingleton<ProjetServiceDataAccess>();
            services.AddSingleton<DependanceBuilder>();
            services.AddSingleton<ValueMappingService>();

            // 2. Sources de Vérité (Stateful, Singletons) - Enregistrement simple
            services.AddSingleton<ProjetService>();
            services.AddSingleton<RessourceService>();
            services.AddSingleton<PlanningService>();
            services.AddSingleton<TaskManagerService>();

            // 3. Use Cases / Orchestrateurs
            services.AddSingleton<ImportService>();
            services.AddSingleton<ExportService>();
            services.AddSingleton<PlanificationOrchestrator>();
            services.AddSingleton<ProjectPersistenceUseCase>();
            services.AddSingleton<ImportWizardOrchestrator>();

            // 4. Services de Processing & Export
            services.AddSingleton<PreparationSolveurService>();
            services.AddSingleton<DataTransformer>();
            services.AddSingleton<PlanningConsolidationService>();
            services.AddSingleton<AnalysisService>();
            services.AddSingleton<PlanningExcelExportService>();
            services.AddSingleton<GanttExportService>();

            // 5. Formulaires et Vues
            services.AddSingleton<MainShellForm>();
        }
    }
}