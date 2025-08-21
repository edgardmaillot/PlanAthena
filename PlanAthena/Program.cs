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

            bool useNewShell = true;

            if (useNewShell)
            {
                // On récupère TOUS les services nécessaires pour le Shell depuis le conteneur
                var applicationService = serviceProvider.GetRequiredService<ApplicationService>();
                var projetService = serviceProvider.GetRequiredService<ProjetService>();
                var ressourceService = serviceProvider.GetRequiredService<RessourceService>();
                var importService = serviceProvider.GetRequiredService<ImportService>();
                var cheminsPrefereService = serviceProvider.GetRequiredService<CheminsPrefereService>();
                var dependanceBuilder = serviceProvider.GetRequiredService<DependanceBuilder>();
                var planificationService = serviceProvider.GetRequiredService<PlanificationService>();
                var planningExcelExportService = serviceProvider.GetRequiredService<PlanningExcelExportService>();
                var ganttExportService = serviceProvider.GetRequiredService<GanttExportService>();
                var userPreferencesService = serviceProvider.GetRequiredService<UserPreferencesService>();

                // L'appel au constructeur est maintenant complet
                Application.Run(new MainShellForm(
                    serviceProvider,
                    applicationService,
                    projetService,
                    ressourceService,
                    importService,
                    cheminsPrefereService,
                    dependanceBuilder,
                    planificationService,
                    planningExcelExportService,
                    ganttExportService,
                    userPreferencesService));
            }
            else
            {
                var mainForm = serviceProvider.GetRequiredService<MainForm>();
                Application.Run(mainForm);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // --- Core DLL ---
            services.AddApplicationServices();
            services.AddInfrastructureServices();
            services.AddScoped<PlanAthena.Core.Application.Interfaces.IConstructeurProblemeOrTools, PlanAthena.Core.Infrastructure.Services.OrTools.ConstructeurProblemeOrTools>();
            services.AddScoped<PlanAthenaCoreFacade>();

            // --- Services de l'application principale ---

            // Couche DataAccess & Utilitaires (Singletons)
            services.AddSingleton<ProjetRepository>();
            services.AddSingleton<UserPreferencesService>();
            services.AddSingleton<CsvDataService>();
            services.AddSingleton<ExcelReader>();
            services.AddSingleton<CheminsPrefereService>();
            services.AddSingleton<IIdGeneratorService, IdGeneratorService>();
            services.AddSingleton<ConfigurationBuilder>();
            services.AddSingleton<DependanceBuilder>();

            // Couche Processing (Singletons car sans état)
            services.AddSingleton<DataTransformer>();
            services.AddSingleton<PreparationSolveurService>();
            services.AddSingleton<ResultatConsolidationService>();

            // Couche Export (Singletons)
            services.AddSingleton<PlanningExcelExportService>();
            services.AddSingleton<GanttExportService>();

            // Services "Domaine" purs
            services.AddSingleton<ProjetService>();
            services.AddSingleton<RessourceService>();

            // Services "Workflow" / Orchestrateurs
            services.AddSingleton<ImportService>();
            services.AddSingleton<ApplicationService>(); // Notre nouveau chef d'orchestre
            services.AddSingleton<PlanificationService>();

            // Enregistrement des formulaires (pour l'ancienne UI)
            services.AddSingleton<MainForm>();
        }
    }
}