using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.Infrastructure;
using PlanAthena.View.TaskManager;

namespace PlanAthena.View.Dashboard

{
    public partial class DashboardView : UserControl
    {
        private readonly ApplicationService _applicationService;
        private readonly CheminsPrefereService _cheminsPrefereService;
        public event EventHandler<Type> NavigateToViewRequested;

        public DashboardView(ApplicationService applicationService, CheminsPrefereService cheminsPrefereService)
        {
            InitializeComponent();
            _applicationService = applicationService;
            _cheminsPrefereService = cheminsPrefereService;

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
            var recentFiles = _cheminsPrefereService.ObtenirFichiersRecents(TypeOperation.ProjetChargement);
            foreach (var filePath in recentFiles)
            {
                var summary = _applicationService.GetProjetSummary(filePath);
                AddProjectControlToList(summary);
            }
            panelProjectList.ResumeLayout();
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
            projectButton.Click += ProjectControl_Click;
            panelProjectList.Controls.Add(projectButton);
        }
        private void panelProjectList_Resize(object sender, EventArgs e)
        {
            // Cette méthode est appelée chaque fois que la taille du panel de gauche change.
            // On ajuste la largeur de chaque bouton pour qu'il remplisse l'espace.
            foreach (Control control in panelProjectList.Controls)
            {
                if (control is KryptonButton)
                {
                    // On soustrait les marges pour un ajustement parfait
                    control.Width = panelProjectList.ClientSize.Width - control.Margin.Left - control.Margin.Right;
                }
            }
        }
        private void ProjectControl_Click(object sender, EventArgs e)
        {
            var control = sender as Control;
            if (control?.Tag is string filePath)
            {
                try
                {
                    _applicationService.ChargerProjetDepuisFichier(filePath);
                    UpdateDetailsForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement du projet:\n{ex.Message}", "Erreur de Chargement", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnNewProject_Click(object sender, EventArgs e)
        {
            _applicationService.CreerNouveauProjet();
            UpdateDetailsForm();
            textName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_applicationService.ProjetActif == null) return;

            UpdateProjectFromForm();

            try
            {
                _applicationService.SauvegarderProjetActuel();
                MessageBox.Show("Projet sauvegardé avec succès !", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RefreshRecentProjects();
                UpdateDetailsForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde du projet:\n{ex.Message}", "Erreur de Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDetailsForm()
        {
            var projet = _applicationService.ProjetActif;
            var chemin = _applicationService.CheminFichierProjetActif;
            var config = _applicationService.ConfigPlanificationActuelle;

            if (projet == null)
            {
                ClearDetailsForm();
                return;
            }

            groupDetails.Enabled = true;
            panelActions.Enabled = true;

            var info = projet.InformationsProjet;
            groupDetails.Values.Heading = $"Édition du Projet : {info.NomProjet}";
            textFilePath.Text = chemin ?? "(Nouveau projet - non sauvegardé)";
            textName.Text = info.NomProjet;
            textDescription.Text = info.Description;

            // -- Champs de configuration --
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
            if (_applicationService.ProjetActif == null) return;

            var info = _applicationService.ProjetActif.InformationsProjet;
            var config = _applicationService.ConfigPlanificationActuelle;

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
            if (_applicationService.ProjetActif != null)
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