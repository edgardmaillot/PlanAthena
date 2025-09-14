// --- START OF FILE DashboardView.cs ---

using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.ProjectPersistence; // For ProjetSummaryDto and ValidationProjet
using PlanAthena.Services.Usecases;
using PlanAthena.View.TaskManager;
using System.ComponentModel; // Required for ComponentResourceManager
using System.IO; // Required for File.Exists
using PlanAthena.Properties; // For Resources.image_immeuble
using PlanAthena.Services.DTOs.Projet; // For InformationsProjet (your new consolidated class)

namespace PlanAthena.View.Dashboard
{
    public partial class DashboardView : UserControl
    {
        private readonly ProjectPersistenceUseCase _persistenceUseCase;
        private readonly ProjetService _projetService;
        
        // Stocker une image par défaut pour éviter de la recharger
        private static readonly Image DefaultProjectImage = GetDefaultImage();

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

            var palette = KryptonManager.CurrentGlobalPalette;
            kTable4projets.BackColor = palette.GetBackColor1(PaletteBackStyle.PanelClient, PaletteState.Normal);
        }

        private static Image GetDefaultImage()
        {
            return Resources.image_immeuble; // Utilisation directe de Properties.Resources
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
            kTable4projets.SuspendLayout();

            // Vider les anciennes cartes, en conservant le header (contrôle à l'index 0)
            while (kTable4projets.Controls.Count > 1)
            {
                kTable4projets.Controls[1].Dispose();
            }

            var recentProjectSummaries = _persistenceUseCase.ObtenirSummariesProjetsRecents();
            // Pour l'exemple, nous ajoutons les cartes à partir de la deuxième ligne du TableLayoutPanel
            // La première ligne est occupée par `headerProjects`.
            int row = 1;
            foreach (var summary in recentProjectSummaries)
            {
                var projectCard = CreateProjectCard(summary);
                // Ajouter la carte et gérer le layout automatique (Fill)
                kTable4projets.Controls.Add(projectCard, 0, row);
                kTable4projets.RowStyles.Add(new RowStyle(SizeType.Absolute, projectCard.Height + (int)projectCard.Margin.Vertical)); // Ajuster la hauteur de la ligne si besoin
                row++;
                if (row >= 5)
                {
                    break; // Limite à 4 projets affichés
                }
            }
            // Ajoutez des styles de lignes vides si moins de 4 projets pour remplir l'espace
            while (row < kTable4projets.RowCount)
            {
                if (kTable4projets.GetControlFromPosition(0, row) == null) // Ne pas ajouter si un contrôle existe déjà
                {
                    kTable4projets.RowStyles[row] = new RowStyle(SizeType.Percent, 25F);
                }
                row++;
            }


            kTable4projets.ResumeLayout(true);
        }

        private Control CreateProjectCard(ProjetSummaryDto summary)
        {
            var cardLayout = new KryptonTableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                Dock = DockStyle.Fill,
                Margin = new Padding(15),
                Size = new Size(428, 144),
                PaletteMode = PaletteMode.Global
            };
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));

            Image projectImage = DefaultProjectImage;
            if (!string.IsNullOrEmpty(summary.ImagePath) && File.Exists(summary.ImagePath))
            {
                try
                {
                    // Charger l'image en mémoire pour éviter le verrouillage du fichier
                    using (var stream = new FileStream(summary.ImagePath, FileMode.Open, FileAccess.Read))
                    {
                        projectImage = Image.FromStream(stream);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur chargement image {summary.ImagePath}: {ex.Message}");
                    // Utiliser l'image par défaut en cas d'erreur
                }
            }

            var picBox = new KryptonPictureBox
            {
                Image = projectImage,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(114, 114)
            };
            picBox.BackColor = KryptonManager.CurrentGlobalPalette.GetBackColor1(PaletteBackStyle.PanelClient, PaletteState.Normal);
            cardLayout.Controls.Add(picBox, 0, 1);

            var radioFav = new KryptonRadioButton { Text = "Favori", Checked = summary.IsFavorite, Enabled = false, PaletteMode = PaletteMode.Global }; // La logique de favori n'est pas implémentée
            cardLayout.Controls.Add(radioFav, 0, 0);

            var header = new KryptonHeader
            {
                HeaderStyle = HeaderStyle.Secondary,
                Dock = DockStyle.Fill,
                Values = { Heading = summary.NomProjet, Description="", Image=null }
            };
            cardLayout.Controls.Add(header, 1, 0);

            var detailsPanel = CreateDetailsPanelForCard(summary);
            cardLayout.Controls.Add(detailsPanel, 1, 1);

            if (summary.ErreurLecture)
            {
                cardLayout.StateCommon.Color1 = Color.LightCoral;
            }

            return cardLayout;
        }

        private KryptonPanel CreateDetailsPanelForCard(ProjetSummaryDto summary)
        {
            var panel = new KryptonPanel { Dock = DockStyle.Fill, PaletteMode = PaletteMode.Global };

            var description = new KryptonWrapLabel
            {
                Text = summary.Description ?? "Aucune description.",
                Location = new Point(3, 0),
                MaximumSize = new Size(200, 90),
                MinimumSize = new Size(200, 90),
                LabelStyle = LabelStyle.AlternateControl,
                PaletteMode = PaletteMode.Global
            };

            var totalLabel = new KryptonLabel { Text = "Total", StateNormal = { ShortText = { Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold) } }, Location = new Point(203, 0) };
            var totalCount = new KryptonLabel { Text = $"{summary.NombreTotalTaches} Tâches", Location = new Point(209, 18) };

            var termineLabel = new KryptonLabel { Text = "Terminées", StateNormal = { ShortText = { Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold) } }, Location = new Point(205, 37) };
            var termineCount = new KryptonLabel { Text = $"{summary.NombreTachesTerminees} Tâches", Location = new Point(211, 55) };

            var retardLabel = new KryptonLabel { Text = "En retard", StateNormal = { ShortText = { Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold) } }, Location = new Point(203, 73) };
            var retardCount = new KryptonLabel { Text = $"{summary.NombreTachesEnRetard} Tâches", Location = new Point(209, 91) };

            if (summary.NombreTachesEnRetard > 0)
                retardCount.StateNormal.ShortText.Color1 = Color.Red;

            var loadButton = new KryptonButton
            {
                Text = "Charger",
                Location = new Point(0, 89),
                Size = new Size(203, 25),
                Tag = summary.FilePath
            };
            loadButton.Click += ProjectCard_LoadClick;

            panel.Controls.AddRange(new Control[] { description, totalLabel, totalCount, termineLabel, termineCount, retardLabel, retardCount, loadButton });
            return panel;
        }

        private void ProjectCard_LoadClick(object sender, EventArgs e)
        {
            if (sender is Control { Tag: string filePath })
            {
                _persistenceUseCase.ChargerProjetDepuisChemin(filePath);
                UpdateDetailsForm();
                RefreshRecentProjects(); // Pour actualiser l'ordre et les infos
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
            // `var config = _projetService.ConfigPlanificationActuelle;` est supprimé

            if (info == null)
            {
                ClearDetailsForm();
                return;
            }

            groupDetails.Enabled = true;
            panelActions.Enabled = true; // Active le panel d'actions

            groupDetails.Values.Heading = $"Édition du Projet : {info.NomProjet}";
            textFilePath.Text = chemin ?? "(Nouveau projet - non sauvegardé)";
            textName.Text = info.NomProjet;
            textDescription.Text = info.Description;
            textIllustrationPath.Text = info.ImagePath;

            // Utilisation des propriétés de InformationsProjet
            numOpeningTime.Value = info.HeureOuverture;
            numClosingTime.Value = info.HeureFermeture;
            numWorkDuration.Value = info.HeuresTravailEffectifParJour;
            numDailyCost.Value = info.CoutIndirectJournalierAbsolu;

            chkMonday.Checked = info.JoursOuvres.Contains(DayOfWeek.Monday);
            chkTuesday.Checked = info.JoursOuvres.Contains(DayOfWeek.Tuesday);
            chkWednesday.Checked = info.JoursOuvres.Contains(DayOfWeek.Wednesday);
            chkThursday.Checked = info.JoursOuvres.Contains(DayOfWeek.Thursday);
            chkFriday.Checked = info.JoursOuvres.Contains(DayOfWeek.Friday);
            chkSaturday.Checked = info.JoursOuvres.Contains(DayOfWeek.Saturday);
            chkSunday.Checked = info.JoursOuvres.Contains(DayOfWeek.Sunday);

            UpdateCalculatedDuration();
            btnNewProject.Enabled = true; // Assure que le bouton Nouveau Projet est toujours activé
            btnDelete.Enabled = true;
            btnSave.Enabled = true; // Toujours activé si un projet est chargé
            btnManage.Enabled = _projetService.ObtenirTousLesLots().Any(); // Activer si au moins un lot existe

        }

        private void UpdateProjectFromForm()
        {
            var info = _projetService.ObtenirInformationsProjet();
            if (info == null) return;

            info.NomProjet = textName.Text;
            info.Description = textDescription.Text;
            info.ImagePath = textIllustrationPath.Text;

            info.HeureOuverture = (int)numOpeningTime.Value;
            info.HeureFermeture = (int)numClosingTime.Value;
            info.HeuresTravailEffectifParJour = (int)numWorkDuration.Value;
            info.CoutIndirectJournalierAbsolu = numDailyCost.Value;

            info.JoursOuvres.Clear();
            if (chkMonday.Checked) info.JoursOuvres.Add(DayOfWeek.Monday);
            if (chkTuesday.Checked) info.JoursOuvres.Add(DayOfWeek.Tuesday);
            if (chkWednesday.Checked) info.JoursOuvres.Add(DayOfWeek.Wednesday);
            if (chkThursday.Checked) info.JoursOuvres.Add(DayOfWeek.Thursday);
            if (chkFriday.Checked) info.JoursOuvres.Add(DayOfWeek.Friday);
            if (chkSaturday.Checked) info.JoursOuvres.Add(DayOfWeek.Saturday);
            if (chkSunday.Checked) info.JoursOuvres.Add(DayOfWeek.Sunday);

            _persistenceUseCase.SetProjectAsDirty();
        }

        private void ClearDetailsForm()
        {
            groupDetails.Enabled = false;
            btnSave.Enabled = false;
            btnDelete.Enabled = false;
            btnManage.Enabled = false;
            btnNewProject.Enabled = true; // MAIS garde Nouveau Projet activé

            textFilePath.Text = "Aucun projet chargé.";
            textName.Clear();
            textDescription.Clear();
            textIllustrationPath.Clear();

            // Réinitialisation avec des valeurs par défaut de InformationsProjet
            var defaultInfo = new InformationsProjet();
            numOpeningTime.Value = defaultInfo.HeureOuverture;
            numClosingTime.Value = defaultInfo.HeureFermeture;
            numWorkDuration.Value = defaultInfo.HeuresTravailEffectifParJour;
            numDailyCost.Value = defaultInfo.CoutIndirectJournalierAbsolu;

            var defaultDays = new HashSet<DayOfWeek>(defaultInfo.JoursOuvres);
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

        private void btnBrowseIllustration_Click(object sender, EventArgs e)
        {
            openFileDialogIllustration.InitialDirectory = Path.GetDirectoryName(textIllustrationPath.Text);
            if (openFileDialogIllustration.ShowDialog() == DialogResult.OK)
            {
                textIllustrationPath.Text = openFileDialogIllustration.FileName;
            }
        }
    }
}