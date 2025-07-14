using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Application.UseCases;
using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Core.Infrastructure;
using PlanAthena.Core.Infrastructure.Services.OrTools;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    // Dictionnaire qui mappe le choix de l'utilisateur à une action de test
    private static readonly Dictionary<string, Func<ServiceProvider, Task>> TestActions = new()
    {
        { "1", (sp) => RunValidationTestAsync(sp, "sprint2_valid.json") },
        { "2", (sp) => RunValidationTestAsync(sp, "sprint2_cycle.json") },
        { "3", (sp) => RunValidationTestAsync(sp, "sprint2_invalid_ref.json") },
        { "4", (sp) => RunOptimizationTestAsync(sp, "sprint3_optimisation_complexe.json") }
    };

    static async Task Main(string[] args)
    {
        var serviceProvider = ConfigureServices();

        while (true)
        {
            DisplayMenu();
            string? choice = Console.ReadLine();

            if (string.Equals(choice, "q", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (TestActions.TryGetValue(choice ?? "", out var action))
            {
                await action(serviceProvider);
            }
            else
            {
                Console.WriteLine("Choix invalide. Veuillez réessayer.");
            }

            Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private static void DisplayMenu()
    {
        Console.WriteLine("===================================");
        Console.WriteLine("  Orchestrateur de Tests PlanAthena.core");
        Console.WriteLine("===================================");
        Console.WriteLine("1. Test de Validation (Scénario Valide)");
        Console.WriteLine("2. Test de Validation (Détection de Cycle)");
        Console.WriteLine("3. Test de Validation (Référence Invalide)");
        Console.WriteLine("4. Test d'Optimisation (Scénario Complexe)");
        Console.WriteLine("-----------------------------------");
        Console.WriteLine("Q. Quitter");
        Console.Write("Votre choix : ");
    }

    private static async Task RunValidationTestAsync(ServiceProvider serviceProvider, string fileName)
    {
        Console.WriteLine($"\n--- Lancement du test de validation pour : {fileName} ---");
        var facade = serviceProvider.GetRequiredService<PlanAthenaCoreFacade>();
        var inputDto = LoadChantierInputFromFile(fileName);
        if (inputDto == null) return;

        // Pour un test de validation, on ne fournit pas de config d'optimisation
        var resultat = await facade.ProcessChantierAsync(inputDto);
        DisplayProcessResult(resultat);
    }

    private static async Task RunOptimizationTestAsync(ServiceProvider serviceProvider, string fileName)
    {
        Console.WriteLine($"\n--- Lancement du test d'optimisation pour : {fileName} ---");
        var facade = serviceProvider.GetRequiredService<PlanAthenaCoreFacade>();
        var inputDto = LoadChantierInputFromFile(fileName);
        if (inputDto == null) return;

        var dtoAvecConfig = inputDto with
        {
            OptimizationConfig = new OptimizationConfigDto
            {
                TypeDeSortie = "PlanningDetaille",
                DureeJournaliereStandardHeures = 7,
                PenaliteChangementOuvrierPourcentage = 30.0m
            }
        };

        var resultat = await facade.ProcessChantierAsync(dtoAvecConfig);
        DisplayProcessResult(resultat);
    }

    private static ServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddApplicationServices();
        serviceCollection.AddInfrastructureServices();

        // CORRECTION : Ajouter l'enregistrement manquant
        serviceCollection.AddScoped<IConstructeurProblemeOrTools, ConstructeurProblemeOrTools>();

        serviceCollection.AddScoped<IProcessChantierUseCase, ProcessChantierUseCase>();
        serviceCollection.AddScoped<PlanAthenaCoreFacade>();

        return serviceCollection.BuildServiceProvider();
    }

    // Dans le fichier Program.cs

    private static void DisplayProcessResult(ProcessChantierResultDto? resultat)
    {
        if (resultat == null) { Console.WriteLine("Le résultat est null."); return; }
        Console.WriteLine($"\n--- Résultat pour le Chantier ID: {resultat.ChantierId} ---");
        Console.WriteLine($"État du Traitement: {resultat.Etat}");

        if (resultat.Messages.Any())
        {
            Console.WriteLine("\nMessages:");
            foreach (var msg in resultat.Messages)
            {
                ConsoleColor color = msg.Type switch
                {
                    TypeMessageValidation.Erreur => ConsoleColor.Red,
                    TypeMessageValidation.Avertissement => ConsoleColor.Yellow,
                    TypeMessageValidation.Suggestion => ConsoleColor.Cyan,
                    _ => ConsoleColor.White
                };
                Console.ForegroundColor = color;
                Console.WriteLine($"  [{msg.Type}] ({msg.CodeMessage}) {msg.Message}");
                Console.ResetColor();
            }
        }

        if (resultat.OptimisationResultat != null)
        {
            var optimResult = resultat.OptimisationResultat;
            Console.WriteLine("\n--- Résultat de l'Optimisation ---");
            ConsoleColor color = optimResult.Status switch
            {
                OptimizationStatus.Optimal or OptimizationStatus.Feasible => ConsoleColor.Green,
                _ => ConsoleColor.Red
            };
            Console.ForegroundColor = color;
            Console.WriteLine($"Statut du Solveur: {optimResult.Status}");
            Console.ResetColor();

            // --- CORRECTION ---
            if (optimResult.CoutTotalEstime.HasValue) Console.WriteLine($"Coût Total Estimé: {optimResult.CoutTotalEstime / 100.0m:C}");
            if (optimResult.CoutTotalRhEstime.HasValue) Console.WriteLine($"  - Coût RH: {optimResult.CoutTotalRhEstime / 100.0m:C}");
            if (optimResult.CoutTotalIndirectEstime.HasValue) Console.WriteLine($"  - Coût Indirect: {optimResult.CoutTotalIndirectEstime / 100.0m:C}");
            if (optimResult.DureeTotaleEnSlots.HasValue) Console.WriteLine($"Durée Totale (en slots d'1h): {optimResult.DureeTotaleEnSlots}");
            // --- FIN CORRECTION ---
        }

        if (resultat.AnalysePostOptimisationResultat != null)
        {
            var analysisResult = resultat.AnalysePostOptimisationResultat;
            Console.WriteLine("\n--- Analyse du Planning ---");
            Console.WriteLine($"Taux d'Occupation Moyen Pondéré: {analysisResult.KpisGlobaux.TauxOccupationMoyenPondere}%");
            foreach (var kpi in analysisResult.KpisParOuvrier)
            {
                Console.WriteLine($"  - {kpi.OuvrierNom} ({kpi.OuvrierId}):");
                Console.WriteLine($"    - Jours de Présence: {kpi.JoursDePresence}");
                Console.WriteLine($"    - Heures Travaillées: {kpi.HeuresTravaillees:F1}h");
                Console.WriteLine($"    - Taux d'Occupation: {kpi.TauxOccupation}%");
                Console.WriteLine($"    - Taux de Fragmentation: {kpi.TauxFragmentation}%");
            }
        }
    }
    private static ChantierSetupInputDto? LoadChantierInputFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erreur : Le fichier de test '{filePath}' n'a pas été trouvé.");
                Console.ResetColor();
                return null;
            }
            string jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };
            return JsonSerializer.Deserialize<ChantierSetupInputDto>(jsonString, options);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Erreur lors de la lecture ou de la désérialisation du fichier '{filePath}': {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }
}