using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application;
using PlanAthena.Core.Facade;
using PlanAthena.Core.Infrastructure;
using PlanAthena.Forms;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Processing;
using PlanAthena.Services.UseCases; // Important: using pour le nouvel Orchestrateur
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

            // L'application utilisera le nouveau Shell
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

            // --- ARCHITECTURE v0.4.8 : NOUVELLE ORGANISATION ---
            // Note : Pour une application de bureau WinForms, enregistrer les services stateful
            // en tant que Singleton est une approche standard pour partager l'état.

            // 1. Sources de Vérité (Stateful, Singletons)
            services.AddSingleton<ProjetService>();
            services.AddSingleton<RessourceService>();
            services.AddSingleton<PlanningService>();     
            services.AddSingleton<TaskStatusService>();   

            // 2. Use Cases / Orchestrateurs (Stateless)
            services.AddSingleton<ApplicationService>(); // Rôle à clarifier, mais gardé pour l'instant
            services.AddSingleton<ImportService>();
            services.AddSingleton<PlanificationOrchestrator>();

            // 3. Utilitaires (Stateless, Singletons)
            services.AddSingleton<IIdGeneratorService, IdGeneratorService>();
            services.AddSingleton<PreparationSolveurService>();
            services.AddSingleton<DataTransformer>();
            services.AddSingleton<PlanningConsolidationService>();
            services.AddSingleton<AnalysisService>();

            // 4. Présentateurs / Exports (Stateless, Singletons)
            services.AddSingleton<PlanningExcelExportService>();
            services.AddSingleton<GanttExportService>();

            // 5. Infrastructure & Dépôts (Singletons)
            services.AddSingleton<ProjetRepository>();
            services.AddSingleton<UserPreferencesService>();
            services.AddSingleton<CsvDataService>();
            services.AddSingleton<ExcelReader>();
            services.AddSingleton<CheminsPrefereService>();

            // --- SERVICES OBSOLÈTES ou à revoir ---
            // Le DependanceBuilder est probablement encore utilisé par l'UI, on le garde pour l'instant.
            services.AddSingleton<DependanceBuilder>();

            // Enregistrement des formulaires et vues
            // Le Shell principal est enregistré pour être résolu avec toutes ses dépendances.
            services.AddSingleton<MainShellForm>();
            // L'ancien MainForm est gardé si nécessaire, mais non utilisé dans le flux principal.
            services.AddTransient<MainForm>();
        }
    }
}