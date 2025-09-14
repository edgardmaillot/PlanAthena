// Emplacement: /View/TaskManager/Cockpit/Cockpit.cs
using Krypton.Navigator;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;
using System;
using System.Windows.Forms;
using static PlanAthena.Services.Usecases.PilotageProjetUseCase;

namespace PlanAthena.View.TaskManager.Cockpit
{
    public partial class Cockpit : Form
    {
        // Services et UseCase
        private PilotageProjetUseCase _useCase;
        private ProjetService _projetService;

        // Vues enfants (UserControls)
        private TaskListView _taskListView;
        private PlanningView _planningView;

        // Timer pour le rafraîchissement automatique des KPIs (optionnel, mais bonne pratique)
        private System.Windows.Forms.Timer _kpiRefreshTimer;

        public Cockpit()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Point d'entrée pour initialiser le Cockpit avec toutes ses dépendances.
        /// Doit être appelé après la création du formulaire.
        /// </summary>
        public void Initialize(
            TaskManagerService taskManagerService,
            PlanningService planningService,
            ProjetService projetService,
            RessourceService ressourceService)
        {
            _projetService = projetService;
            // 1. Création de l'orchestrateur (UseCase)
            _useCase = new PilotageProjetUseCase(
                taskManagerService,
                planningService,
                _projetService,
                ressourceService
            );

            // 2. Initialisation des vues enfants
            InitializeViews();

            // 3. Configuration des événements et du timer
            AttachEvents();

            // 4. Premier chargement des données
            RefreshAllData();
        }

        private void InitializeViews()
        {
            // Création et configuration de la vue TaskList
            _taskListView = new TaskListView
            {
                Dock = DockStyle.Fill
            };
            _taskListView.Initialize(_useCase, _projetService);

            // Ajout de la vue à la bonne page du navigateur
            var taskListPage = kryptonNavigator1.Pages.FirstOrDefault(p => p.Name == "tabPageTaskList") as KryptonPage;
            if (taskListPage != null)
            {
                taskListPage.Controls.Add(_taskListView);
            }

            // Création et configuration de la vue Planning
            _planningView = new PlanningView
            {
                Dock = DockStyle.Fill
            };
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
            kryptonButton2.Click += (s, e) => this.Close(); // Bouton "Fermer"

            // Configuration du timer pour rafraîchir les KPIs toutes les 30 secondes
            _kpiRefreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 30000
            };
            _kpiRefreshTimer.Tick += (s, e) => RefreshKpis();
            _kpiRefreshTimer.Start();
        }

        /// <summary>
        /// Rafraîchit toutes les données du Cockpit (KPIs et vues enfants).
        /// </summary>
        private void RefreshAllData()
        {
            if (_useCase == null) return;

            Cursor = Cursors.WaitCursor;

            RefreshKpis();
            RefreshMeteo(); // Appel de la nouvelle méthode
            _taskListView?.RefreshData();
            _planningView?.RefreshData();

            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Rafraîchit uniquement les indicateurs du panneau latéral.
        /// </summary>
        private void RefreshKpis()
        {
            if (_useCase == null) return;

            var kpiData = _useCase.ObtenirIndicateursCockpit();

            // Mise à jour des labels d'indicateurs
            lblProgression.Text = $"{kpiData.ProgressionGlobalePourcentage:F1} %";
            lblSpi.Text = $"{kpiData.PerformanceCalendrierSPI:F2}";
            lblCpi.Text = $"{kpiData.PerformanceCoutCPI:F2}";

            // Mise à jour des labels de risques
            lblLotRisque.Text = $"{kpiData.LotLePlusARisqueNom} ({kpiData.LotLePlusARisqueDeriveJours} j)";
            lblMetierTension.Text = $"{kpiData.MetierLePlusEnTensionNom} ({kpiData.MetierLePlusEnTensionTauxOccupation:P1})";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Nettoyage du timer pour éviter les fuites de mémoire
            _kpiRefreshTimer?.Stop();
            _kpiRefreshTimer?.Dispose();
            base.OnFormClosed(e);
        }
        // Dans Cockpit.cs

        private void RefreshMeteo()
        {
            var meteoData = _useCase.ObtenirMeteoProjet();

            // Mise à jour des labels (à créer dans le designer)
            lblDerivePlanning.Text = $"{meteoData.DerivPlanningJours} jours";
            lblDispoRessources.Text = $"{meteoData.DisponibiliteRessourcesPourcentage:P0}";
            lblDeviationBudget.Text = $"{meteoData.DeviationBudgetPourcentage:P0}";

            // Mise à jour de l'image de fond du kryptonPictureBox1
            switch (meteoData.Statut)
            {
                case ProjectWeatherStatus.Sunny:
                    kryptonPictureBox1.Image = Properties.Resources.sunny; // Nom à adapter
                    kryptonPictureBox1.BackgroundImage = Properties.Resources.sunny; // Nom à adapter
                    break;
                case ProjectWeatherStatus.Cloudy:
                    kryptonPictureBox1.Image = Properties.Resources.cloudy;
                    kryptonPictureBox1.BackgroundImage = Properties.Resources.cloudy;
                    break;
                case ProjectWeatherStatus.Rainy:
                    kryptonPictureBox1.Image = Properties.Resources.rainy;
                    kryptonPictureBox1.BackgroundImage = Properties.Resources.rainy;
                    break;
                case ProjectWeatherStatus.Stormy:
                    kryptonPictureBox1.Image = Properties.Resources.stormy;
                    kryptonPictureBox1.BackgroundImage = Properties.Resources.stormy;
                    break;
            }
            kryptonPictureBox1.BackgroundImageLayout = ImageLayout.Stretch; // Pour que l'image remplisse le panel
        }
    }
}