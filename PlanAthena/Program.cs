// Fichier: PlanAthena/Program.cs
// Version: 0.4.4
// Description: Centralisation de la configuration de l'injection de dépendances.
// Instancie la nouvelle architecture de services et les injecte dans MainForm.

using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application;
using PlanAthena.Core.Infrastructure;
using PlanAthena.Core.Facade;
using PlanAthena.Forms;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Processing;
using PlanAthena.Utilities;
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

            // Création du conteneur de services
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Récupération et lancement du formulaire principal
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
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

            // --- NOUVELLE ARCHITECTURE DES SERVICES MÉTIER ---

            // Ordre d'enregistrement important pour les dépendances directes
            // 1. ProjetService (dépend de IIdGeneratorService)
            services.AddSingleton<ProjetService>();

            // 2. RessourceService (dépend de ProjetService et IIdGeneratorService)
            services.AddSingleton<RessourceService>();

            // 3. ImportService (dépend de ProjetService, RessourceService, CsvDataService)
            services.AddSingleton<ImportService>();

            // 4. PlanificationService (dépend des services de traitement)
            services.AddSingleton<PlanificationService>();

            // --- Enregistrement des formulaires ---
            // Le formulaire principal est enregistré pour pouvoir lui injecter les services.
            services.AddSingleton<MainForm>();
        }
    }
}