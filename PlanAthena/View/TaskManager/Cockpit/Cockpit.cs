// Fichier: /View/TaskManager/Cockpit/Cockpit.cs Version 0.6.0.3

using Krypton.Navigator;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;

namespace PlanAthena.View.TaskManager.Cockpit
{
    public partial class Cockpit : Form
    {
        private PilotageProjetUseCase _useCase;
        private ProjetService _projetService;
        private TaskListView _taskListView;
        private PlanningView _planningView;
        private System.Windows.Forms.Timer _kpiRefreshTimer;

        public Cockpit()
        {
            InitializeComponent();
        }

        public void Initialize(
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

            InitializeViews();
            AttachEvents();
            RefreshAllData();
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
        }

        private void AttachEvents()
        {
            this.Load += (s, e) => RefreshAllData();
            kryptonButton2.Click += (s, e) => this.Close();

            _kpiRefreshTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _kpiRefreshTimer.Tick += (s, e) => RefreshKpisAndMeteo();
            _kpiRefreshTimer.Start();
        }

        private void RefreshAllData()
        {
            if (_useCase == null) return;
            Cursor = Cursors.WaitCursor;
            RefreshKpisAndMeteo();
            _taskListView?.RefreshData();
            _planningView?.RefreshData();
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

            // CORRIGÉ : Utilisation de la propriété correcte du DTO et formatage en pourcentage
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _kpiRefreshTimer?.Stop();
            _kpiRefreshTimer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}