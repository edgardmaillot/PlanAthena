using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess; // Ajout pour ProjetServiceDataAccess
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Usecases; // Assurez-vous que le namespace est correct
using PlanAthena.Services.UseCases;
using PlanAthena.Utilities;
using PlanAthena.View.Dashboard;
using PlanAthena.View.Planificator;
using PlanAthena.View.Ressources;
using PlanAthena.View.Ressources.MetierDiagram;
using PlanAthena.View.Structure;
using PlanAthena.View.TaskManager;
using static PlanAthena.View.TaskManager.TaskManagerView;

namespace PlanAthena.View
{
    public partial class MainShellForm : KryptonForm
    {
        // --- NOUVELLE DÉPENDANCE ---
        private readonly ProjectPersistenceUseCase _persistenceUseCase;

        // Stockage des autres services
        private readonly IServiceProvider _serviceProvider;
        private readonly ProjetService _projetService;
        private readonly TaskManagerService _taskManagerService;
        private readonly RessourceService _ressourceService;
        private readonly ImportService _importService;
        private readonly ImportWizardOrchestrator _importWizardOrchestrator;
        private readonly ExportService _exportService;
        private readonly CheminsPrefereService _cheminsPrefereService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly PlanningService _planningService;
        private readonly PlanningExcelExportService _planningExcelExportService;
        private readonly GanttExportService _ganttExportService;
        private readonly UserPreferencesService _userPreferencesService;
        private readonly PlanificationOrchestrator _planificationOrchestrator;

        public MainShellForm(
            IServiceProvider serviceProvider,
            // --- NOUVELLE INJECTION ---
            ProjectPersistenceUseCase persistenceUseCase,
            ProjetService projetService,
            TaskManagerService taskManagerService,
            RessourceService ressourceService,
            ImportService importService,
            ImportWizardOrchestrator ImportWizardOrchestrator,
            ExportService exportService,
            CheminsPrefereService cheminsPrefereService,
            DependanceBuilder dependanceBuilder,
            PlanificationOrchestrator planificationOrchestrator,
            PlanningService planningService,
            PlanningExcelExportService planningExcelExportService,
            GanttExportService ganttExportService,
            UserPreferencesService userPreferencesService)
        {
            InitializeComponent();
            kryptonManager.GlobalPaletteMode = PaletteMode.Microsoft365SilverDarkMode;

            _serviceProvider = serviceProvider;
            // --- NOUVELLE ASSIGNATION ---
            _persistenceUseCase = persistenceUseCase;
            _projetService = projetService;
            _taskManagerService = taskManagerService;
            _ressourceService = ressourceService;
            _importService = importService;
            _importWizardOrchestrator = ImportWizardOrchestrator;
            _exportService = exportService;
            _cheminsPrefereService = cheminsPrefereService;
            _dependanceBuilder = dependanceBuilder;
            _planificationOrchestrator = planificationOrchestrator;
            _planningService = planningService;
            _planningExcelExportService = planningExcelExportService;
            _ganttExportService = ganttExportService;
            _userPreferencesService = userPreferencesService;

            InitializeMenuHoverOpening();
            InitializeThemeSelector();
            NavigateToAccueil();
        }

        #region Logique de Navigation Principale


        private void ShowView(UserControl viewToShow)
        {
            if (viewToShow == null) return;
            if (panelContent.Controls.Count > 0)
            {
                var oldView = panelContent.Controls[0];
                panelContent.Controls.Remove(oldView);
                oldView.Dispose();
            }
            panelContent.Controls.Clear();
            viewToShow.Dock = DockStyle.Fill;
            panelContent.Controls.Add(viewToShow);
            var manager = FindActiveDockingManager();
            if (manager != null)
            {
                string viewIdentifier = viewToShow.GetType().Name;
                _userPreferencesService.LoadLayout(manager, viewIdentifier);
            }
        }
        private void InitializeMenuHoverOpening()
        {
            foreach (ToolStripItem item in menuStrip.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.MouseEnter += MenuItem_MouseEnter_OpenOnHover;
                }
            }
        }
        private void MenuItem_MouseEnter_OpenOnHover(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem hoveredItem && hoveredItem.HasDropDown && !hoveredItem.IsOnDropDown)
            {
                foreach (ToolStripItem item in menuStrip.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem != hoveredItem && menuItem.IsOnDropDown)
                    {
                        menuItem.HideDropDown();
                    }
                }
                hoveredItem.ShowDropDown();
            }
        }
        #endregion

        #region Méthodes de Navigation (appelées par les menus et les CTA)

        private void NavigateToAccueil()
        {
            // --- CORRECTION: Le constructeur de DashboardView est mis à jour ---
            var view = new DashboardView(_persistenceUseCase, _projetService);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToStructure()
        {
            // Note: ProjectStructureView devra aussi être mis à jour s'il utilise ApplicationService
            var view = new ProjectStructureView(_projetService, _taskManagerService);
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
            var view = new RessourceOuvrierView(_ressourceService, _projetService, _importWizardOrchestrator, _exportService);
            ShowView(view);
        }

        private void NavigateToPrerequisMetiers()
        {
            // Note: PrerequisMetierView devra aussi être mis à jour
            var view = new PrerequisMetierView(_ressourceService, _projetService, _dependanceBuilder);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToTaskManager()
        {
            // L'appel normal affiche la vue diagramme par défaut
            var view = new TaskManagerView(_projetService, _taskManagerService, _ressourceService, _dependanceBuilder, _importService, _exportService, TaskManagerDefaultView.Diagram);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToTaskListView()
        {
            // L'appel depuis le menu "Liste" force l'affichage de la liste
            var view = new TaskManagerView(_projetService, _taskManagerService, _ressourceService, _dependanceBuilder, _importService, _exportService, TaskManagerDefaultView.List);
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }
        private void NavigateToPlanificator()
        {
            // --- CORRECTION: Le constructeur de PlanificatorView est mis à jour ---
            var view = new PlanificatorView(_planificationOrchestrator, _planningService, _projetService, _ressourceService, _planningExcelExportService, _ganttExportService, _cheminsPrefereService);
            ShowView(view);
        }
        #endregion

        #region Gestionnaires d'événements du Menu

        private void menuAccueil_Click(object sender, EventArgs e) => NavigateToAccueil();
        private void menuStructureListe_Click(object sender, EventArgs e) => NavigateToStructure();
        private void menuRessourcesMetiers_Click(object sender, EventArgs e) => NavigateToRessourcesMetiers();
        private void menuRessourcesOuvriers_Click(object sender, EventArgs e) => NavigateToRessourcesOuvriers();
        private void menuTachesDiagramme_Click(object sender, EventArgs e) => NavigateToTaskManager();
        private void menuTachesListe_Click(object sender, EventArgs e) => NavigateToTaskListView();

        private void menuNouveauProjet_Click(object sender, EventArgs e)
        {
            // --- CORRECTION: Appel au UseCase ---
            _persistenceUseCase.CreerNouveauProjet();
            NavigateToAccueil();
        }

        private void menuChargerProjet_Click(object sender, EventArgs e)
        {
            // --- CORRECTION: Le UseCase gère maintenant la boîte de dialogue ---
            _persistenceUseCase.ChargerProjet();
            NavigateToAccueil(); // On rafraîchit la vue pour afficher le projet chargé
        }

        private void menuSauvegarderProjet_Click(object sender, EventArgs e)
        {
            // --- CORRECTION: Appel au UseCase ---
            _persistenceUseCase.SauvegarderProjet();

            // On rafraîchit la vue actuelle si c'est le dashboard
            if (panelContent.Controls.Count > 0 && panelContent.Controls[0] is DashboardView currentDashboard)
            {
                currentDashboard.UpdateDetailsForm();
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
        private Krypton.Docking.KryptonDockingManager FindActiveDockingManager()
        {
            if (panelContent.Controls.Count > 0 && panelContent.Controls[0] is UserControl currentView)
            {
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
            if (_userPreferencesService != null)
            {
                string savedThemeName = _userPreferencesService.LoadTheme();
                if (Enum.TryParse<PaletteMode>(savedThemeName, out var savedTheme))
                {
                    ApplyTheme(savedTheme, savePreference: false);
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