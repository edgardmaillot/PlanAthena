// Fichier: /View/TaskManager/Cockpit/Cockpit.cs Version 0.6.5

using Krypton.Navigator;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;
using PlanAthena.View.Utils;

namespace PlanAthena.View.TaskManager.Cockpit
{
    public partial class CockpitView : UserControl
    {
        private PilotageProjetUseCase _useCase;
        private ProjetService _projetService;
        private SplashScreen _splashScreen;
        private TaskListView _taskListView;
        private PlanningView _planningView;
        private EVMgraphView _graphEVM;
        private System.Windows.Forms.Timer _kpiRefreshTimer;

        public CockpitView()
        {
            InitializeComponent();
        }

        public async void Initialize(
            TaskManagerService taskManagerService,
            PlanningService planningService,
            ProjetService projetService,
            RessourceService ressourceService)
        {
            _projetService = projetService;
            _useCase = new PilotageProjetUseCase(
                taskManagerService,
                planningService,
                _projetService,
                ressourceService
            );

            this.SuspendLayout();
            try
            {
                // STAThreadAttributed requirement for certain UI components
                //await Task.Run(() => InitializeViews());
                InitializeViews();
                AttachEvents();
                // Ne plus appeler RefreshAllData ici
                await Task.Run(() => RefreshKpisAndMeteo()); // On rafraîchit seulement les KPI et la météo
                                                             // L'onglet actif sera rafraîchi lors du premier affichage via l'événement Load
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        private void InitializeViews()
        {
            _taskListView = new TaskListView { Dock = DockStyle.Fill };
            _taskListView.Initialize(_useCase, _projetService);
            var taskListPage = kryptonNavigator1.Pages.FirstOrDefault(p => p.Name == "tabPageTaskList") as KryptonPage;
            if (taskListPage != null)
            {
                taskListPage.Controls.Add(_taskListView);
            }

            _planningView = new PlanningView { Dock = DockStyle.Fill };
            _planningView.Initialize(_useCase);
            var planningPage = kryptonNavigator1.Pages.FirstOrDefault(p => p.Name == "tabPagePlanning") as KryptonPage;
            if (planningPage != null)
            {
                planningPage.Controls.Add(_planningView);
            }
            _graphEVM = new EVMgraphView { Dock = DockStyle.Fill };
            _graphEVM.Initialize(_useCase);
            var evmGraphPage = kryptonNavigator1.Pages.FirstOrDefault(p => p.Name == "tabPageEVM") as KryptonPage;
            if (evmGraphPage != null)
            {
                evmGraphPage.Controls.Add(_graphEVM);
            }
            //on ajoute une page SplashScreen pour l'instant
            _splashScreen = new SplashScreen { Dock = DockStyle.Fill };
            var splashPage = kryptonNavigator1.Pages.FirstOrDefault(p => p.Name == "tabPageSplash") as KryptonPage;
            if (splashPage != null)
            {
                splashPage.Controls.Add(_splashScreen);
            }
            kryptonNavigator1.SelectedPageChanged += Navigator_SelectedPageChanged;
        }

        private void AttachEvents()
        {
            this.Load += (s, e) =>
            {
                RefreshKpisAndMeteo();
                RefreshActiveTab();
            };

            //TMP
            //tabPageSplash.Resize += TabPageSplash_FirstResize;

            _kpiRefreshTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _kpiRefreshTimer.Tick += (s, e) => RefreshKpisAndMeteo(); // Only refresh KPI and meteo
            _kpiRefreshTimer.Start();
        }
        



        private void Navigator_SelectedPageChanged(object sender, EventArgs e)
        {
            // Rafraîchir l'onglet qui vient d'être sélectionné
            RefreshActiveTab();
        }

        private void RefreshActiveTab()
        {
            if (kryptonNavigator1.SelectedPage == null) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                switch (kryptonNavigator1.SelectedPage.Name)
                {
                    case "tabPageTaskList":
                        _taskListView?.RefreshData();
                        break;
                    case "tabPagePlanning":
                        _planningView?.RefreshData();
                        break;
                    case "tabPageEVM":
                        _graphEVM?.RefreshData();
                        break;
                    case "tabPageSplash":
                        // Rien à rafraîchir
                        break;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void RefreshAllData()
        {
            if (_useCase == null) return;
            Cursor = Cursors.WaitCursor;
            RefreshKpisAndMeteo(); // Toujours rafraîchir les KPI et la météo
            RefreshActiveTab(); // Rafraîchir l'onglet actif
            Cursor = Cursors.Default;
        }

        // Méthode unifiée pour les rafraîchissements
        private void RefreshKpisAndMeteo()
        {
            if (_useCase == null) return;
            RefreshKpis();
            RefreshMeteo();
        }

        private void RefreshKpis()
        {
            if (_useCase == null) return;
            var kpiData = _useCase.ObtenirIndicateursCockpit();
            var culture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");

            lblProgression.Text = $"{kpiData.ProgressionGlobalePourcentage:F1} %";
            lblLotRisque.Text = $"{kpiData.LotLePlusARisqueNom} ({kpiData.LotLePlusARisqueDeriveJours} j)";
            lblMetierTension.Text = $"{kpiData.MetierLePlusEnTensionNom} ({kpiData.MetierLePlusEnTensionTauxOccupation:P1})";

            lblBacValue.Text = kpiData.BudgetAtCompletion.ToString("C0", culture);
            lblEacValue.Text = kpiData.EstimateAtCompletion.ToString("C0", culture);
            lblCvValue.Text = kpiData.CostVariance.ToString("C0", culture);
            lblSpi.Text = $"{kpiData.SchedulePerformanceIndex:F2}";
            lblCpi.Text = $"{kpiData.CostPerformanceIndex:F2}";

            lblSvValue.Text = $"{kpiData.ScheduleVarianceDays:+0.0;-0.0;0.0} j";
            if (kpiData.ScheduleVarianceDays < -0.1)
                lblSvValue.StateCommon.ShortText.Color1 = Color.Red;
            else if (kpiData.ScheduleVarianceDays > 0.1)
                lblSvValue.StateCommon.ShortText.Color1 = Color.LimeGreen;
            else
                lblSvValue.StateCommon.ShortText.Color1 = Color.White;
        }

        private void RefreshMeteo()
        {
            if (_useCase == null) return;
            var meteoData = _useCase.ObtenirMeteoProjet();

            lblDerivePlanning.Text = $"{meteoData.DerivPlanningJours} jours";
            lblDispoRessources.Text = $"{meteoData.DisponibiliteRessourcesPourcentage:P0}";
            lblDeviationBudget.Text = meteoData.DeviationBudgetPourcentage.ToString("P0");

            switch (meteoData.Statut)
            {
                case ProjectWeatherStatus.Sunny:
                    kryptonPictureBox1.BackgroundImage = Properties.Resources.sunny;
                    break;
                case ProjectWeatherStatus.Cloudy:
                    kryptonPictureBox1.BackgroundImage = Properties.Resources.cloudy;
                    break;
                case ProjectWeatherStatus.Rainy:
                    kryptonPictureBox1.BackgroundImage = Properties.Resources.rainy;
                    break;
                case ProjectWeatherStatus.Stormy:
                    kryptonPictureBox1.BackgroundImage = Properties.Resources.stormy;
                    break;
            }
            kryptonPictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
        }
    }
}