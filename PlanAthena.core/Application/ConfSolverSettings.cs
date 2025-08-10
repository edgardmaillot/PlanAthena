using System.IO;
using System.Text.Json;

namespace PlanAthena.core.Application;

/// <summary>
/// Configuration des paramètres d'infrastructure du solveur.
/// Ces paramètres sont automatiquement chargés depuis le fichier appsettings.json
/// pour permettre l'adaptation à chaque environnement de déploiement sans recompilation.
/// </summary>
public class SolverSettings
{
    /// <summary>
    /// Nom de la section dans la configuration (appsettings.json).
    /// </summary>
    public const string SectionName = "SolverSettings";

    /// <summary>
    /// Nombre de workers de recherche utilisés par le solveur OR-Tools.
    /// Ce paramètre permet d'adapter la solution aux ressources CPU disponibles
    /// sur chaque machine de déploiement.
    /// Valeur par défaut de sécurité : 8.
    /// </summary>
    public int NumSearchWorkers { get; set; } = 8;

    /// <summary>
    /// Charge la configuration depuis le fichier appsettings.json.
    /// Si le fichier ou la section n'existe pas, les valeurs par défaut sont conservées.
    /// </summary>
    /// <returns>Instance configurée de SolverSettings</returns>
    public static SolverSettings LoadFromConfiguration()
    {
        var settings = new SolverSettings();

        try
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            if (File.Exists(appSettingsPath))
            {
                var json = File.ReadAllText(appSettingsPath);
                using var document = JsonDocument.Parse(json);

                if (document.RootElement.TryGetProperty(SectionName, out var solverSection))
                {
                    if (solverSection.TryGetProperty(nameof(NumSearchWorkers), out var numWorkersElement))
                    {
                        if (numWorkersElement.TryGetInt32(out var numWorkers))
                        {
                            settings.NumSearchWorkers = numWorkers;
                        }
                    }
                }
            }
        }
        catch
        {
            // En cas d'erreur, on garde les valeurs par défaut
            // Cela garantit que la DLL fonctionne même sans configuration
        }

        return settings;
    }
}