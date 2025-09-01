using Krypton.Docking;
using PlanAthena.Services.DataAccess; // CHANGEMENT
using PlanAthena.Services.Infrastructure;
using System.IO;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Orchestre la sauvegarde et le chargement des préférences utilisateur.
    /// </summary>
    public class UserPreferencesService
    {
        private readonly CheminsPrefereService _cheminsService;
        // --- CHANGEMENT: Remplacement de la dépendance ---
        private readonly ProjetServiceDataAccess _dataAccess;

        public UserPreferencesService(
            CheminsPrefereService cheminsService,
            ProjetServiceDataAccess dataAccess) // CHANGEMENT
        {
            _cheminsService = cheminsService;
            // --- CHANGEMENT ---
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
    }
}