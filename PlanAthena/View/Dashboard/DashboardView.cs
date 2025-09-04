using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.ProjectPersistence;
using PlanAthena.Services.Usecases;
using PlanAthena.View.TaskManager;

namespace PlanAthena.View.Dashboard
{
    public partial class DashboardView : UserControl
    {
        private readonly ProjectPersistenceUseCase _persistenceUseCase;
        private readonly ProjetService _projetService;

        public event EventHandler<Type> NavigateToViewRequested;

        public DashboardView(
            ProjectPersistenceUseCase persistenceUseCase,
            ProjetService projetService)
        {
            InitializeComponent();
            _persistenceUseCase = persistenceUseCase;
            _projetService = projetService;

            textFilePath.ReadOnly = true;
            this.Load += DashboardView_Load;
            this.panelProjectList.Resize += new System.EventHandler(this.panelProjectList_Resize);
        }

        private void DashboardView_Load(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                RefreshRecentProjects();
                UpdateDetailsForm();
            }
        }

        private void RefreshRecentProjects()
        {
            panelProjectList.SuspendLayout();
            while (panelProjectList.Controls.Count > 1)
            {
                panelProjectList.Controls[1].Dispose();
            }
            var recentProjectSummaries = _persistenceUseCase.ObtenirSummariesProjetsRecents();
            foreach (var summary in recentProjectSummaries)
            {
                AddProjectControlToList(summary);
            }
            panelProjectList.ResumeLayout(true);
        }

        private void AddProjectControlToList(ProjetSummaryDto summary)
        {
            var projectButton = new KryptonButton
            {
                Text = $"{summary.NomProjet}\n{summary.Description}",
                Tag = summary.FilePath,
                AutoSize = true,
                MinimumSize = new Size(340, 50),
                Margin = new Padding(3),
                StateCommon = { Content = { ShortText = { TextH = PaletteRelativeAlign.Near, TextV = PaletteRelativeAlign.Center, MultiLine = InheritBool.True } } }
            };

            if (summary.ErreurLecture)
            {
                projectButton.StateCommon.Back.Color1 = Color.LightCoral;
            }

            projectButton.Click += ProjectControl_Click;
            panelProjectList.Controls.Add(projectButton);
        }

        private void panelProjectList_Resize(object sender, EventArgs e)
        {
            foreach (Control control in panelProjectList.Controls)
            {
                if (control is KryptonButton button)
                {
                    button.Width = panelProjectList.ClientSize.Width - button.Margin.Left - button.Margin.Right;
                }
            }
        }

        private void ProjectControl_Click(object sender, EventArgs e)
        {
            if (sender is Control { Tag: string filePath })
            {
                _persistenceUseCase.ChargerProjetDepuisChemin(filePath);
                UpdateDetailsForm();
                RefreshRecentProjects(); // Pour actualiser l'ordre
            }
        }

        private void btnNewProject_Click(object sender, EventArgs e)
        {
            _persistenceUseCase.CreerNouveauProjet();
            UpdateDetailsForm();
            textName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_projetService.ObtenirInformationsProjet() == null) return;

            UpdateProjectFromForm();

            _persistenceUseCase.SauvegarderProjet();
            MessageBox.Show("Projet sauvegardé avec succès !", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);

            RefreshRecentProjects();
            UpdateDetailsForm();
        }

        public void UpdateDetailsForm()
        {
            var info = _projetService.ObtenirInformationsProjet();
            var chemin = _persistenceUseCase.GetCurrentProjectPath();
            var config = _projetService.ConfigPlanificationActuelle;

            if (info == null)
            {
                ClearDetailsForm();
                return;
            }

            groupDetails.Enabled = true;
            panelActions.Enabled = true;

            groupDetails.Values.Heading = $"Édition du Projet : {info.NomProjet}";
            textFilePath.Text = chemin ?? "(Nouveau projet - non sauvegardé)";
            textName.Text = info.NomProjet;
            textDescription.Text = info.Description;

            numOpeningTime.Value = config.HeureDebutJournee;
            numClosingTime.Value = config.HeureDebutJournee + config.DureeJournaliereStandardHeures;
            numWorkDuration.Value = config.HeuresTravailEffectifParJour;
            numDailyCost.Value = config.CoutIndirectJournalierAbsolu;

            chkMonday.Checked = config.JoursOuvres.Contains(DayOfWeek.Monday);
            chkTuesday.Checked = config.JoursOuvres.Contains(DayOfWeek.Tuesday);
            chkWednesday.Checked = config.JoursOuvres.Contains(DayOfWeek.Wednesday);
            chkThursday.Checked = config.JoursOuvres.Contains(DayOfWeek.Thursday);
            chkFriday.Checked = config.JoursOuvres.Contains(DayOfWeek.Friday);
            chkSaturday.Checked = config.JoursOuvres.Contains(DayOfWeek.Saturday);
            chkSunday.Checked = config.JoursOuvres.Contains(DayOfWeek.Sunday);

            UpdateCalculatedDuration();
        }

        private void UpdateProjectFromForm()
        {
            var info = _projetService.ObtenirInformationsProjet();
            var config = _projetService.ConfigPlanificationActuelle;
            if (info == null) return;

            info.NomProjet = textName.Text;
            info.Description = textDescription.Text;

            config.HeureDebutJournee = (int)numOpeningTime.Value;
            config.DureeJournaliereStandardHeures = (int)numClosingTime.Value - (int)numOpeningTime.Value;
            if (config.DureeJournaliereStandardHeures < 0) config.DureeJournaliereStandardHeures = 0;
            config.HeuresTravailEffectifParJour = (int)numWorkDuration.Value;
            config.CoutIndirectJournalierAbsolu = (long)numDailyCost.Value;

            config.JoursOuvres.Clear();
            if (chkMonday.Checked) config.JoursOuvres.Add(DayOfWeek.Monday);
            if (chkTuesday.Checked) config.JoursOuvres.Add(DayOfWeek.Tuesday);
            if (chkWednesday.Checked) config.JoursOuvres.Add(DayOfWeek.Wednesday);
            if (chkThursday.Checked) config.JoursOuvres.Add(DayOfWeek.Thursday);
            if (chkFriday.Checked) config.JoursOuvres.Add(DayOfWeek.Friday);
            if (chkSaturday.Checked) config.JoursOuvres.Add(DayOfWeek.Saturday);
            if (chkSunday.Checked) config.JoursOuvres.Add(DayOfWeek.Sunday);

            _persistenceUseCase.SetProjectAsDirty();
        }

        private void ClearDetailsForm()
        {
            groupDetails.Enabled = false;
            panelActions.Enabled = false;
            textFilePath.Text = "Aucun projet chargé.";
            textName.Clear();
            textDescription.Clear();

            var defaultConfig = new ConfigurationPlanification();
            numOpeningTime.Value = defaultConfig.HeureDebutJournee;
            numClosingTime.Value = defaultConfig.HeureDebutJournee + defaultConfig.DureeJournaliereStandardHeures;
            numWorkDuration.Value = defaultConfig.HeuresTravailEffectifParJour;
            numDailyCost.Value = defaultConfig.CoutIndirectJournalierAbsolu;

            var defaultDays = new HashSet<DayOfWeek>(defaultConfig.JoursOuvres);
            chkMonday.Checked = defaultDays.Contains(DayOfWeek.Monday);
            chkTuesday.Checked = defaultDays.Contains(DayOfWeek.Tuesday);
            chkWednesday.Checked = defaultDays.Contains(DayOfWeek.Wednesday);
            chkThursday.Checked = defaultDays.Contains(DayOfWeek.Thursday);
            chkFriday.Checked = defaultDays.Contains(DayOfWeek.Friday);
            chkSaturday.Checked = defaultDays.Contains(DayOfWeek.Saturday);
            chkSunday.Checked = defaultDays.Contains(DayOfWeek.Sunday);

            groupDetails.Values.Heading = "Édition du Projet";
        }



        private void OnTimeChanged(object sender, EventArgs e)
        {
            UpdateCalculatedDuration();
        }

        private void UpdateCalculatedDuration()
        {
            int duration = (int)numClosingTime.Value - (int)numOpeningTime.Value;
            if (duration < 0) duration = 0;
            labelCalculatedDuration.Text = $"{duration}h";
        }

        private void btnManage_Click(object sender, EventArgs e)
        {
            if (_projetService.ObtenirInformationsProjet() != null && _projetService.ObtenirTousLesLots().Any())
            {
                NavigateToViewRequested?.Invoke(this, typeof(TaskManagerView));
            }
            else
            {
                MessageBox.Show("Veuillez d'abord charger ou créer un projet.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}