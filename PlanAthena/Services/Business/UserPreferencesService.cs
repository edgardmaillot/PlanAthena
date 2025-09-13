using Krypton.Docking;
using PlanAthena.Services.DataAccess; // CHANGEMENT
using PlanAthena.Services.Infrastructure;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Orchestre la sauvegarde et le chargement des préférences utilisateur.
    /// </summary>
    public class UserPreferencesService
    {
        private readonly CheminsPrefereService _cheminsService;
        private readonly ProjetServiceDataAccess _dataAccess;

        public UserPreferencesService(
            CheminsPrefereService cheminsService,
            ProjetServiceDataAccess dataAccess)
        {
            _cheminsService = cheminsService;
            _dataAccess = dataAccess;
        }

        #region Gestion du Layout

        private string GetLayoutFilePath(string viewName)
        {
            string dir = _cheminsService.ObtenirDossierUIPrefs();
            return Path.Combine(dir, $"{viewName}.xml");
        }

        public void SaveLayout(KryptonDockingManager manager, string viewName)
        {
            if (manager == null) return;
            string path = GetLayoutFilePath(viewName);
            manager.SaveConfigToFile(path);
        }

        public void LoadLayout(KryptonDockingManager manager, string viewName)
        {
            if (manager == null) return;
            string path = GetLayoutFilePath(viewName);
            if (File.Exists(path))
            {
                try
                {
                    manager.LoadConfigFromFile(path);
                }
                catch { /* Ignorer les erreurs */ }
            }
        }

        #endregion

        #region Gestion du Thème

        private string GetThemeFilePath()
        {
            string dir = _cheminsService.ObtenirDossierUIPrefs();
            return Path.Combine(dir, "theme.config");
        }

        public void SaveTheme(string themeName)
        {
            string path = GetThemeFilePath();
            // --- CHANGEMENT: Appel à la nouvelle dépendance ---
            _dataAccess.SauvegarderFichierTexte(themeName, path);
        }

        public string LoadTheme(string defaultTheme = "SparkleBlueDarkMode")
        {
            string path = GetThemeFilePath();
            // --- CHANGEMENT: Appel à la nouvelle dépendance ---
            string theme = _dataAccess.ChargerFichierTexte(path);
            return string.IsNullOrEmpty(theme) ? defaultTheme : theme;
        }

        #endregion


        #region Gestion des Traductions
        /// <summary>
        /// Charge le dictionnaire des correspondances depuis un fichier de configuration utilisateur.
        /// </summary>
        /// <returns>Un dictionnaire des correspondances. Retourne un dictionnaire vide si le fichier n'existe pas ou est invalide.</returns>
        public virtual Dictionary<string, string> ChargerDictionnaire()
        {
            // Logique pour lire un fichier JSON/XML et désérialiser le dictionnaire.
            // Exemple simplifié :
            string filePath = Path.Combine(_cheminsService.ObtenirDossierUIPrefs(), "user_mappings.json");
            if (!File.Exists(filePath))
            {
                // On peut charger un dictionnaire par défaut ici la première fois.
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var dico = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return new Dictionary<string, string>(dico, StringComparer.OrdinalIgnoreCase);
            }
            catch (System.Exception)
            {
                // Gérer les erreurs de lecture/désérialisation
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Sauvegarde le dictionnaire complet des correspondances dans un fichier de configuration utilisateur.
        /// </summary>
        /// <param name="dictionnaire">Le dictionnaire à sauvegarder.</param>
        public virtual void SauverDictionnaire(Dictionary<string, string> dictionnaire)
        {
            // Logique pour sérialiser le dictionnaire en JSON/XML et l'écrire dans un fichier.
            // Exemple simplifié :
            string filePath = Path.Combine(_cheminsService.ObtenirDossierUIPrefs(), "user_mappings.json");
            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(dictionnaire, options);
            File.WriteAllText(filePath, json);
        }
        #endregion
    }
}