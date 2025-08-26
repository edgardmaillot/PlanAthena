//Fichier : TaskManagerView.cs Version : 0.4.7.18
using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Utilities;
using PlanAthena.View;
using PlanAthena.View.Dashboard;
using PlanAthena.View.Planificator;
using PlanAthena.View.Ressources;
using PlanAthena.View.Ressources.MetierDiagram;
using PlanAthena.View.Structure;
using PlanAthena.View.TaskManager;
using System;
using System.IO;
using System.Windows.Forms;
using static Krypton.Toolkit.KryptonManager;

namespace PlanAthena.View
{
    public partial class MainShellForm : KryptonForm
    {
        // Stockage des services essentiels
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationService _applicationService;
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly ImportService _importService;
        private readonly CheminsPrefereService _cheminsPrefereService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly PlanificationService _planificationService;
        private readonly PlanningExcelExportService _planningExcelExportService;
        private readonly GanttExportService _ganttExportService;
        private readonly UserPreferencesService _userPreferencesService;

        public MainShellForm(
            IServiceProvider serviceProvider,
            ApplicationService applicationService,
            ProjetService projetService,
            RessourceService ressourceService,
            ImportService importService,
            CheminsPrefereService cheminsPrefereService,
            DependanceBuilder dependanceBuilder,
            PlanificationService planificationService,
            PlanningExcelExportService planningExcelExportService,
            GanttExportService ganttExportService,
            UserPreferencesService userPreferencesService)
        {

            InitializeComponent();
            kryptonManager.GlobalPaletteMode = PaletteMode.Microsoft365SilverDarkMode;

            _serviceProvider = serviceProvider;
            _applicationService = applicationService;
            _projetService = projetService;
            _ressourceService = ressourceService;
            _importService = importService;
            _cheminsPrefereService = cheminsPrefereService;
            _dependanceBuilder = dependanceBuilder;
            _planificationService = planificationService;
            _planningExcelExportService = planningExcelExportService;
            _ganttExportService = ganttExportService;
            _userPreferencesService = userPreferencesService;

            InitializeMenuHoverOpening();
            InitializeThemeSelector();
            // Afficher la vue d'accueil au démarrage
            NavigateToAccueil();

        }

        #region Logique de Navigation Principale

        private void ShowView(UserControl viewToShow)
        {
            if (viewToShow == null) return;

            // On vide le panel précédent avant d'ajouter la nouvelle vue.
            if (panelContent.Controls.Count > 0)
            {
                var oldView = panelContent.Controls[0];
                panelContent.Controls.Remove(oldView);
                oldView.Dispose(); // Libérer les ressources de l'ancienne vue
            }

            panelContent.Controls.Clear();
            viewToShow.Dock = DockStyle.Fill;
            panelContent.Controls.Add(viewToShow);

            // Tentative de chargement automatique du layout APRÈS que la vue soit ajoutée
            var manager = FindActiveDockingManager();
            if (manager != null)
            {
                string viewIdentifier = viewToShow.GetType().Name;
                _userPreferencesService.LoadLayout(manager, viewIdentifier);
            }
        }
        private void InitializeMenuHoverOpening()
        {
            // On parcourt tous les items du MenuStrip
            foreach (ToolStripItem item in menuStrip.Items)
            {
                // On vérifie si l'item est bien un menu déroulant
                if (item is ToolStripMenuItem menuItem)
                {
                    // On s'abonne à l'événement MouseEnter
                    menuItem.MouseEnter += MenuItem_MouseEnter_OpenOnHover;
                }
            }
        }

        private void MenuItem_MouseEnter_OpenOnHover(object sender, EventArgs e)
        {
            ToolStripMenuItem hoveredItem = sender as ToolStripMenuItem;

            // Si l'item survolé a un sous-menu et qu'il n'est pas déjà ouvert
            if (hoveredItem != null && hoveredItem.HasDropDown && !hoveredItem.IsOnDropDown)
            {
                // On ferme tous les autres sous-menus qui pourraient être ouverts
                foreach (ToolStripItem item in menuStrip.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem != hoveredItem && menuItem.IsOnDropDown)
                    {
                        menuItem.HideDropDown();
                    }
                }

                // On ouvre le sous-menu de l'item survolé
                hoveredItem.ShowDropDown();
            }
        }
        #endregion

        #region Méthodes de Navigation (appelées par les menus et les CTA)

        private void NavigateToAccueil()
        {
            var view = new DashboardView(_applicationService, _cheminsPrefereService);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToStructure()
        {
            var view = new ProjectStructureView(_applicationService, _projetService);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToRessourcesMetiers()
        {
            var view = new RessourceMetierView(_ressourceService, _projetService);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToRessourcesOuvriers()
        {
            var view = new RessourceOuvrierView(_ressourceService, _projetService, _importService);
            ShowView(view);
        }

        private void NavigateToPrerequisMetiers()
        {
            var view = new PrerequisMetierView(_applicationService, _ressourceService, _projetService, _dependanceBuilder);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToTaskManager()
        {
            var view = new TaskManagerView(_applicationService, _projetService, _ressourceService, _dependanceBuilder, _importService);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }
        private void NavigateToPlanificator()
        {
            var view = new PlanificatorView(_applicationService, _planificationService, _planningExcelExportService, _ganttExportService, _ressourceService);
            ShowView(view);
        }
        #endregion

        #region Gestionnaires d'événements du Menu

        private void menuAccueil_Click(object sender, EventArgs e) => NavigateToAccueil();
        private void menuStructureListe_Click(object sender, EventArgs e) => NavigateToStructure();
        private void menuRessourcesMetiers_Click(object sender, EventArgs e) => NavigateToRessourcesMetiers();
        private void menuRessourcesOuvriers_Click(object sender, EventArgs e) => NavigateToRessourcesOuvriers();
        private void menuTachesDiagramme_Click(object sender, EventArgs e) => NavigateToTaskManager();

        private void menuNouveauProjet_Click(object sender, EventArgs e)
        {
            _applicationService.CreerNouveauProjet();
            // Après création, on redirige vers l'accueil qui est le meilleur endroit pour éditer les détails
            NavigateToAccueil();
            //MessageBox.Show("Nouveau projet initialisé.", "Nouveau Projet", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void menuChargerProjet_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierProjets(),
                Filter = "Fichiers projet (*.json)|*.json",
                Title = "Charger un projet"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _applicationService.ChargerProjetDepuisFichier(ofd.FileName);
                    // On navigue vers l'accueil pour voir le résumé du projet chargé
                    NavigateToAccueil();
                    //MessageBox.Show($"Projet '{Path.GetFileNameWithoutExtension(ofd.FileName)}' chargé.", "Chargement réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement du projet :\n{ex.Message}", "Erreur de Chargement", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuSauvegarderProjet_Click(object sender, EventArgs e)
        {
            if (_applicationService.ProjetActif == null)
            {
                MessageBox.Show("Aucun projet actif à sauvegarder. Veuillez d'abord créer ou charger un projet.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _applicationService.SauvegarderProjetActuel();
                MessageBox.Show("Projet sauvegardé avec succès !", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Si la vue actuelle est le dashboard, on la rafraîchit pour mettre à jour le chemin du fichier
                if (panelContent.Controls.Count > 0 && panelContent.Controls[0] is DashboardView currentDashboard)
                {
                    currentDashboard.UpdateDetailsForm(); // Méthode à rendre publique dans DashboardView si nécessaire
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde du projet :\n{ex.Message}", "Erreur de Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnNavigateToViewRequested(object sender, Type viewType)
        {
            if (viewType == typeof(PrerequisMetierView))
            {
                NavigateToPrerequisMetiers();
            }
            else if (viewType == typeof(TaskManagerView))
            {
                NavigateToTaskManager();
            }
            else if (viewType == typeof(ProjectStructureView))
            {
                NavigateToStructure();
            }
            else if (viewType == typeof(PlanificatorView))
            {
                NavigateToPlanificator();
            }
        }

        #endregion

        #region Gestionnaires Layout & Thèmes

        private void menuSaveLayout_Click(object sender, EventArgs e)
        {
            var manager = FindActiveDockingManager();
            if (manager != null && panelContent.Controls.Count > 0)
            {
                string viewIdentifier = panelContent.Controls[0].GetType().Name;
                _userPreferencesService.SaveLayout(manager, viewIdentifier);
                MessageBox.Show("Disposition de l'écran sauvegardée.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Aucune disposition à sauvegarder pour cet écran.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void menuLoadLayout_Click(object sender, EventArgs e)
        {
            var manager = FindActiveDockingManager();
            if (manager != null && panelContent.Controls.Count > 0)
            {
                string viewIdentifier = panelContent.Controls[0].GetType().Name;
                _userPreferencesService.LoadLayout(manager, viewIdentifier);
            }
            else
            {
                MessageBox.Show("Cet écran ne supporte pas le chargement de disposition.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Trouve le KryptonDockingManager dans la vue actuellement affichée.
        /// </summary>
        /// <returns>L'instance du manager, ou null si non trouvé.</returns>
        private Krypton.Docking.KryptonDockingManager FindActiveDockingManager()
        {
            if (panelContent.Controls.Count > 0 && panelContent.Controls[0] is UserControl currentView)
            {
                // Utilise la réflexion pour trouver un champ privé nommé "kryptonDockingManager"
                var field = currentView.GetType().GetField("kryptonDockingManager",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    return field.GetValue(currentView) as Krypton.Docking.KryptonDockingManager;
                }
            }
            return null;
        }


        private void InitializeThemeSelector()
        {
            var themes = new[]
            {
                new { Name = "Sparkle - Dark", Value = PaletteMode.SparkleBlueDarkMode},
                new { Name = "Sparkle - Orange", Value = PaletteMode.SparkleOrange},
                new { Name = "Sparkle - Violet", Value = PaletteMode.SparklePurple},
                new { Name = "Office 365 - Silver", Value = PaletteMode.Microsoft365Silver},
                new { Name = "Office 365 - Dark", Value = PaletteMode.Microsoft365SilverDarkMode},
                new { Name = "Office 365 - Light", Value = PaletteMode.Microsoft365BlueLightMode},
                new { Name = "Système Professionnel", Value = PaletteMode.ProfessionalSystem },
                new { Name = "Système Standard", Value = PaletteMode.ProfessionalSystem },
            };

            toolStripComboBoxThemes.KryptonComboBoxControl.DataSource = themes;
            toolStripComboBoxThemes.KryptonComboBoxControl.DisplayMember = "Name";
            toolStripComboBoxThemes.KryptonComboBoxControl.ValueMember = "Value";

            toolStripComboBoxThemes.KryptonComboBoxControl.SelectedIndexChanged += ThemeSelector_SelectedIndexChanged;

            // On ne charge le thème que si le service est disponible.
            if (_userPreferencesService != null)
            {
                string savedThemeName = _userPreferencesService.LoadTheme();
                if (Enum.TryParse<PaletteMode>(savedThemeName, out var savedTheme))
                {
                    ApplyTheme(savedTheme, savePreference: false); // On ajoute un flag pour éviter de réécrire la valeur qu'on vient de lire

                    var themeToSelect = themes.FirstOrDefault(t => t.Value == savedTheme);
                    if (themeToSelect != null)
                    {
                        toolStripComboBoxThemes.KryptonComboBoxControl.SelectedItem = themeToSelect;
                    }
                }
                else
                {
                    ApplyTheme(PaletteMode.SparkleBlueDarkMode, savePreference: false);
                }
            }
            else
            {
                // Fallback si le service n'a pas été injecté pour une raison quelconque
                ApplyTheme(PaletteMode.SparkleBlueDarkMode, savePreference: false);
            }
        }

        private void ThemeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBoxThemes.KryptonComboBoxControl.SelectedItem != null)
            {
                var selectedThemeValue = (PaletteMode)toolStripComboBoxThemes.KryptonComboBoxControl.SelectedValue;
                ApplyTheme(selectedThemeValue);
            }
        }

        private void ApplyTheme(PaletteMode theme, bool savePreference = true)
        {
            kryptonManager.GlobalPaletteMode = theme;
            if (savePreference && _userPreferencesService != null)
            {
                _userPreferencesService.SaveTheme(theme.ToString());
            }
        }
        #endregion
    }
}