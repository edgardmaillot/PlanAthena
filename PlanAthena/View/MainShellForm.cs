//Fichier : TaskManagerView.cs Version : 0.4.7.18
using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Utilities;
using System;
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
            GanttExportService ganttExportService)
        {
            InitializeComponent();

            kryptonManager.GlobalPaletteMode = PaletteMode.Microsoft365SilverDarkMode;
            InitializeThemeSelector();
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

        #region Gestionnaires d'événements

        private void menuAccueil_Click(object sender, EventArgs e) => NavigateToAccueil();
        private void menuStructureListe_Click(object sender, EventArgs e) => NavigateToStructure();
        private void menuRessourcesMetiers_Click(object sender, EventArgs e) => NavigateToRessourcesMetiers();
        private void menuRessourcesOuvriers_Click(object sender, EventArgs e) => NavigateToRessourcesOuvriers();
        private void menuTachesDiagramme_Click(object sender, EventArgs e) => NavigateToTaskManager();

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

            ApplyTheme(PaletteMode.SparkleBlueDarkMode);
        }

        private void ThemeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBoxThemes.KryptonComboBoxControl.SelectedItem != null)
            {
                var selectedThemeValue = (PaletteMode)toolStripComboBoxThemes.KryptonComboBoxControl.SelectedValue;
                ApplyTheme(selectedThemeValue);
            }
        }

        private void ApplyTheme(PaletteMode theme)
        {
            kryptonManager.GlobalPaletteMode = theme;
        }
    }
}