using Krypton.Toolkit;
using static Krypton.Toolkit.KryptonManager;
using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PlanAthena.View
{
    public partial class MainShellForm : KryptonForm
    {
        private readonly Dictionary<Type, UserControl> _viewCache = new Dictionary<Type, UserControl>();

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
            if (!panelContent.Controls.Contains(viewToShow))
            {
                panelContent.Controls.Clear();
                viewToShow.Dock = DockStyle.Fill;
                panelContent.Controls.Add(viewToShow);
            }
        }

        private T GetOrCreateView<T>(Func<T> viewFactory) where T : UserControl
        {
            if (_viewCache.TryGetValue(typeof(T), out var view))
            {
                // Avant de retourner une vue du cache, on s'assure de détacher les anciens gestionnaires d'événements
                // pour éviter les abonnements multiples.
                if (view is DashboardView dv) dv.NavigateToViewRequested -= OnNavigateToViewRequested;
                if (view is ProjectStructureView psv) psv.NavigateToViewRequested -= OnNavigateToViewRequested;
                if (view is RessourceMetierView rmv) rmv.NavigateToViewRequested -= OnNavigateToViewRequested;
                if (view is PrerequisMetierView pmv) pmv.NavigateToViewRequested -= OnNavigateToViewRequested;

                return (T)view;
            }
            var newView = viewFactory();
            _viewCache[typeof(T)] = newView;
            return newView;
        }

        #endregion

        #region Méthodes de Navigation (appelées par les menus et les CTA)

        private void NavigateToAccueil()
        {
            var view = GetOrCreateView(() => new DashboardView(_applicationService, _cheminsPrefereService));
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToStructure()
        {
            var view = GetOrCreateView(() => new ProjectStructureView(_applicationService, _projetService));
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToRessourcesMetiers()
        {
            var view = GetOrCreateView(() => new RessourceMetierView(_ressourceService, _projetService));
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToRessourcesOuvriers()
        {
            var view = GetOrCreateView(() => new RessourceOuvrierView(_ressourceService, _projetService, _importService));
            // Cette vue n'a pas besoin de lever d'événement de navigation pour l'instant.
            ShowView(view);
        }

        private void NavigateToPrerequisMetiers()
        {
            var view = GetOrCreateView(() => new PrerequisMetierView(_applicationService, _ressourceService, _projetService, _dependanceBuilder));
            view.NavigateToViewRequested += OnNavigateToViewRequested;
            ShowView(view);
        }

        private void NavigateToTaskManager()
        {
            var view = GetOrCreateView(() => new TaskManagerView(_applicationService, _projetService, _ressourceService, _dependanceBuilder, _importService));
            view.NavigateToViewRequested += OnNavigateToViewRequested; // S'abonner à l'événement
            ShowView(view);
        }
        private void NavigateToPlanificator()
        {
            var view = GetOrCreateView(() => new PlanificatorView(_applicationService, _planificationService, _planningExcelExportService, _ganttExportService, _ressourceService));
            // Cette vue n'a pas besoin de lever d'événement pour l'instant
            ShowView(view);
        }
        #endregion

        #region Gestionnaires d'événements

        // Événements du MenuStrip
        private void menuAccueil_Click(object sender, EventArgs e) => NavigateToAccueil();
        private void menuStructureListe_Click(object sender, EventArgs e) => NavigateToStructure();
        private void menuRessourcesMetiers_Click(object sender, EventArgs e) => NavigateToRessourcesMetiers();
        private void menuRessourcesOuvriers_Click(object sender, EventArgs e) => NavigateToRessourcesOuvriers();
        private void menuTachesDiagramme_Click(object sender, EventArgs e) => NavigateToTaskManager();

        // Gestionnaire central pour les CTA des UserControls
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
            // ... ajouter d'autres destinations ici
        }

        #endregion
        private void InitializeThemeSelector()
        {
            // On crée une petite classe pour stocker le nom du thème et sa valeur Enum
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

            // CORRECTION : On utilise .KryptonComboBox pour accéder au vrai contrôle
            toolStripComboBoxThemes.KryptonComboBoxControl.DataSource = themes;
            toolStripComboBoxThemes.KryptonComboBoxControl.DisplayMember = "Name";
            toolStripComboBoxThemes.KryptonComboBoxControl.ValueMember = "Value";

            // CORRECTION : On s'abonne à l'événement du vrai contrôle
            toolStripComboBoxThemes.KryptonComboBoxControl.SelectedIndexChanged += ThemeSelector_SelectedIndexChanged;

            // CORRECTION : On utilise le nom complet pour éviter les ambiguïtés
            ApplyTheme(PaletteMode.SparkleBlueDarkMode);
        }

        private void ThemeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            // CORRECTION : On accède au vrai contrôle
            if (toolStripComboBoxThemes.KryptonComboBoxControl.SelectedItem != null)
            {
                var selectedThemeValue = (PaletteMode)toolStripComboBoxThemes.KryptonComboBoxControl.SelectedValue;
                ApplyTheme(selectedThemeValue);
            }
        }

        private void ApplyTheme(PaletteMode theme)
        {
            // CORRECTION : La propriété est sur l'instance kryptonManager, pas globale
            kryptonManager.GlobalPaletteMode = theme;

            // Pour la sauvegarde (quand tu l'implémenteras)
            // Properties.Settings.Default.UserTheme = theme.ToString();
            // Properties.Settings.Default.Save();
        }
    }
}