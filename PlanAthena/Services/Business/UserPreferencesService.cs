using Krypton.Docking;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Infrastructure;
using System.IO;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Orchestre la sauvegarde et le chargement des préférences utilisateur,
    /// telles que la disposition de l'interface et le thème.
    /// </summary>
    public class UserPreferencesService
    {
        private readonly CheminsPrefereService _cheminsService;
        private readonly ProjetRepository _projetRepository;

        /// <summary>
        /// Initialise une nouvelle instance du service de préférences utilisateur.
        /// </summary>
        /// <param name="cheminsService">Le service qui fournit les chemins de dossiers.</param>
        /// <param name="projetRepository">Le service qui gère la persistance des fichiers.</param>
        public UserPreferencesService(CheminsPrefereService cheminsService, ProjetRepository projetRepository)
        {
            _cheminsService = cheminsService;
            _projetRepository = projetRepository;
        }

        #region Gestion du Layout

        /// <summary>
        /// Obtient le chemin complet du fichier de layout pour une vue donnée.
        /// </summary>
        private string GetLayoutFilePath(string viewName)
        {
            string dir = _cheminsService.ObtenirDossierUIPrefs();
            return Path.Combine(dir, $"{viewName}.xml");
        }

        /// <summary>
        /// Sauvegarde la disposition actuelle d'un KryptonDockingManager dans un fichier XML.
        /// </summary>
        /// <param name="manager">Le manager dont la disposition doit être sauvegardée.</param>
        /// <param name="viewName">Le nom unique de la vue (généralement le nom de la classe).</param>
        public void SaveLayout(KryptonDockingManager manager, string viewName)
        {
            if (manager == null) return;
            string path = GetLayoutFilePath(viewName);
            manager.SaveConfigToFile(path);
        }

        /// <summary>
        /// Charge une disposition depuis un fichier XML et l'applique à un KryptonDockingManager.
        /// </summary>
        /// <param name="manager">Le manager qui doit recevoir la nouvelle disposition.</param>
        /// <param name="viewName">Le nom unique de la vue.</param>
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
                catch
                {
                    // Ignorer les erreurs de chargement, la disposition par défaut sera utilisée
                }
            }
        }

        #endregion

        #region Gestion du Thème

        /// <summary>
        /// Obtient le chemin complet du fichier de configuration du thème.
        /// </summary>
        private string GetThemeFilePath()
        {
            string dir = _cheminsService.ObtenirDossierUIPrefs();
            return Path.Combine(dir, "theme.config");
        }

        /// <summary>
        /// Sauvegarde le nom du thème sélectionné par l'utilisateur.
        /// </summary>
        /// <param name="themeName">Le nom du thème (ex: "SparkleBlueDarkMode").</param>
        public void SaveTheme(string themeName)
        {
            string path = GetThemeFilePath();
            _projetRepository.SauvegarderFichierTexte(themeName, path);
        }

        /// <summary>
        /// Charge le nom du thème sauvegardé par l'utilisateur.
        /// </summary>
        /// <param name="defaultTheme">Le thème à retourner si aucun n'est sauvegardé.</param>
        /// <returns>Le nom du thème sauvegardé ou le thème par défaut.</returns>
        public string LoadTheme(string defaultTheme = "SparkleBlueDarkMode")
        {
            string path = GetThemeFilePath();
            string theme = _projetRepository.ChargerFichierTexte(path);
            return string.IsNullOrEmpty(theme) ? defaultTheme : theme;
        }

        #endregion
    }
}