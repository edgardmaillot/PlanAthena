using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.DTOs.ImportExport;
using PlanAthena.Services.Usecases;
using System.ComponentModel;
using System.Data;

namespace PlanAthena.View.Ressources.MetierDiagram
{
    public partial class RessourceOuvrierView : UserControl
    {
        #region Champs privés
        private readonly RessourceService _ressourceService;
        private readonly ProjetService _projetService;
        private readonly ImportWizardOrchestrator _importWizardOrchestrator;
        private readonly ExportService _exportService;

        private bool _isLoading = false;
        private BindingList<OuvrierViewModel> _ouvriersBindingList;
        private BindingSource _ouvriersBindingSource;

        // Énumération pour les modes de tri
        public enum TriMode
        {
            Patronyme,
            Metier
        }
        private TriMode _currentTriMode = TriMode.Patronyme;
        #endregion

        #region Constructeur et initialisation
        public RessourceOuvrierView(RessourceService ressourceService, ProjetService projetService,
            ImportWizardOrchestrator importWizardOrchestrator, ExportService exportService)
        {
            InitializeComponent();
            _ressourceService = ressourceService;
            _projetService = projetService;
            _importWizardOrchestrator = importWizardOrchestrator;
            _exportService = exportService;

            textId.ReadOnly = true;
        }

        private void RessourceOuvrierView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            InitializeBindingList();
            SetupGrids();
            SetupSortingControls();

            cmbAddCompetence.SelectedIndexChanged += CmbAddCompetence_SelectedIndexChanged;
            gridCompetences.CellContentClick += GridCompetences_CellContentClick;

            RefreshAll();
        }

        private void InitializeBindingList()
        {
            _ouvriersBindingList = new BindingList<OuvrierViewModel>();
            _ouvriersBindingSource = new BindingSource(_ouvriersBindingList, null);
            gridOuvriers.DataSource = _ouvriersBindingSource;
        }

        private void SetupSortingControls()
        {
            // Ajouter des boutons radio ou un ComboBox pour choisir le mode de tri
            // Exemple avec des boutons existants (à adapter selon votre interface)

            // Vous pouvez ajouter ces contrôles dans le Designer :
            // - RadioButton radioTriPatronyme
            // - RadioButton radioTriMetier
            // Ou un ComboBox cmbTriMode
        }
        #endregion

        #region ViewModel pour la grille
        public class OuvrierViewModel : INotifyPropertyChanged
        {
            private string _ouvrierId;
            private string _nomComplet;
            private int _coutJournalier;
            private int _competencesCount;
            private string _metierPrincipal;
            private Color _couleurMetier;

            public string OuvrierId
            {
                get => _ouvrierId;
                set { _ouvrierId = value; OnPropertyChanged(); }
            }

            public string NomComplet
            {
                get => _nomComplet;
                set { _nomComplet = value; OnPropertyChanged(); }
            }

            public int CoutJournalier
            {
                get => _coutJournalier;
                set { _coutJournalier = value; OnPropertyChanged(); }
            }

            public int CompetencesCount
            {
                get => _competencesCount;
                set { _competencesCount = value; OnPropertyChanged(); }
            }

            public string MetierPrincipal
            {
                get => _metierPrincipal;
                set { _metierPrincipal = value; OnPropertyChanged(); }
            }

            public Color CouleurMetier
            {
                get => _couleurMetier;
                set { _couleurMetier = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Configuration des grilles
        private void SetupGrids()
        {
            // Configuration de la grille des ouvriers avec colonnes colorées
            gridOuvriers.AutoGenerateColumns = false;
            gridOuvriers.Columns.Clear();

            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn
            {
                Name = "OuvrierId",
                HeaderText = "ID",
                DataPropertyName = "OuvrierId",
                Visible = false
            });

            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn
            {
                Name = "NomComplet",
                HeaderText = "Nom Prénom",
                DataPropertyName = "NomComplet",
                FillWeight = 45
            });

            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn
            {
                Name = "MetierPrincipal",
                HeaderText = "Métier Principal",
                DataPropertyName = "MetierPrincipal",
                FillWeight = 35
            });

            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn
            {
                Name = "CoutJournalier",
                HeaderText = "Coût/j",
                DataPropertyName = "CoutJournalier",
                FillWeight = 15
            });

            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn
            {
                Name = "CompetencesCount",
                HeaderText = "Comp.",
                DataPropertyName = "CompetencesCount",
                FillWeight = 5
            });

            // Gestionnaire pour colorer les cellules selon le métier
            gridOuvriers.CellFormatting += GridOuvriers_CellFormatting;

            // Configuration de la grille des compétences (inchangée)
            gridCompetences.AutoGenerateColumns = false;
            gridCompetences.Columns.Clear();
            gridCompetences.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "MetierId", HeaderText = "ID Métier", DataPropertyName = "MetierId", FillWeight = 1, Visible = false });
            gridCompetences.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "MetierNom", HeaderText = "Nom du Métier", DataPropertyName = "MetierNom", FillWeight = 90 });
            gridCompetences.Columns.Add(new KryptonDataGridViewCheckBoxColumn { Name = "EstPrincipal", HeaderText = "Principal", DataPropertyName = "EstMetierPrincipal", FillWeight = 10 });
            var deleteColumn = new KryptonDataGridViewButtonColumn
            {
                Name = "DeleteColumn",
                Text = "Supprimer",
                UseColumnTextForButtonValue = true,
                HeaderText = "",
                FillWeight = 20
            };
            gridCompetences.Columns.Add(deleteColumn);
        }

        private void GridOuvriers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && gridOuvriers.Columns[e.ColumnIndex].Name == "MetierPrincipal")
            {
                var viewModel = _ouvriersBindingList[e.RowIndex];
                e.CellStyle.BackColor = viewModel.CouleurMetier;

                // Ajuster la couleur du texte selon la luminosité du fond
                var brightness = (viewModel.CouleurMetier.R * 0.299 +
                                viewModel.CouleurMetier.G * 0.587 +
                                viewModel.CouleurMetier.B * 0.114) / 255;
                e.CellStyle.ForeColor = brightness > 0.5 ? Color.Black : Color.White;
            }
        }
        #endregion

        #region Logique de rafraîchissement optimisée
        private void RefreshAll()
        {
            string selectedOuvrierId = GetSelectedOuvrierId();
            RebuildOuvriersBindingList();

            if (selectedOuvrierId != null)
            {
                SelectOuvrierInGrid(selectedOuvrierId);
            }

            RefreshUIFromSelection();
        }

        private void RebuildOuvriersBindingList()
        {
            _isLoading = true;

            var recherche = textSearchOuvrier.Text.ToLowerInvariant();
            var ouvriers = _ressourceService.GetAllOuvriers()
                .Where(o => string.IsNullOrWhiteSpace(recherche) ||
                           o.NomComplet.ToLowerInvariant().Contains(recherche));

            // Vider et reconstruire la liste
            _ouvriersBindingList.Clear();

            // Trier selon le mode actuel
            var ouvriersTries = _currentTriMode == TriMode.Patronyme
                ? ouvriers.OrderBy(o => o.Nom).ThenBy(o => o.Prenom)
                : ouvriers.OrderBy(o => GetMetierPrincipalNom(o)).ThenBy(o => o.Nom);

            foreach (var ouvrier in ouvriersTries)
            {
                _ouvriersBindingList.Add(CreateViewModelFromOuvrier(ouvrier));
            }

            _isLoading = false;
        }

        private OuvrierViewModel CreateViewModelFromOuvrier(Ouvrier ouvrier)
        {
            var metierPrincipalId = ouvrier.Competences?.FirstOrDefault(c => c.EstMetierPrincipal)?.MetierId ??
                                   ouvrier.Competences?.FirstOrDefault()?.MetierId ?? "";
            var metierPrincipalNom = GetMetierPrincipalNom(ouvrier);
            var couleurMetier = _ressourceService.GetDisplayColorForMetier(metierPrincipalId);

            return new OuvrierViewModel
            {
                OuvrierId = ouvrier.OuvrierId,
                NomComplet = ouvrier.NomComplet,
                CoutJournalier = ouvrier.CoutJournalier,
                CompetencesCount = ouvrier.Competences.Count,
                MetierPrincipal = metierPrincipalNom,
                CouleurMetier = couleurMetier
            };
        }

        private string GetMetierPrincipalNom(Ouvrier ouvrier)
        {
            var competencePrincipale = ouvrier.Competences?.FirstOrDefault(c => c.EstMetierPrincipal) ??
                                     ouvrier.Competences?.FirstOrDefault();

            if (competencePrincipale != null)
            {
                var metier = _ressourceService.GetMetierById(competencePrincipale.MetierId);
                return metier?.Nom ?? "(métier inconnu)";
            }
            return "(aucun métier)";
        }

        private void RefreshSingleOuvrierInGrid(string ouvrierId)
        {
            var ouvrier = _ressourceService.GetOuvrierById(ouvrierId);
            if (ouvrier == null) return;

            var existingViewModel = _ouvriersBindingList.FirstOrDefault(vm => vm.OuvrierId == ouvrierId);
            if (existingViewModel != null)
            {
                // Mettre à jour le ViewModel existant
                existingViewModel.NomComplet = ouvrier.NomComplet;
                existingViewModel.CoutJournalier = ouvrier.CoutJournalier;
                existingViewModel.CompetencesCount = ouvrier.Competences.Count;
                existingViewModel.MetierPrincipal = GetMetierPrincipalNom(ouvrier);

                var metierPrincipalId = ouvrier.Competences?.FirstOrDefault(c => c.EstMetierPrincipal)?.MetierId ??
                                       ouvrier.Competences?.FirstOrDefault()?.MetierId ?? "";
                existingViewModel.CouleurMetier = _ressourceService.GetDisplayColorForMetier(metierPrincipalId);

                // Si le mode de tri a changé, reconstruire toute la liste
                if (_currentTriMode == TriMode.Metier)
                {
                    RebuildOuvriersBindingList();
                }
            }
        }

        private void RefreshUIFromSelection()
        {
            RefreshDetails();
            RefreshCompetencesGrid();
            RefreshAddCompetenceComboBox();
            UpdateButtonStates();
        }

        private void RefreshDetails()
        {
            _isLoading = true;
            var ouvrier = GetSelectedOuvrier();
            if (ouvrier != null)
            {
                textId.Text = ouvrier.OuvrierId;
                textNom.Text = ouvrier.Nom;
                textPrenom.Text = ouvrier.Prenom;
                numCoutJournalier.Value = ouvrier.CoutJournalier;
            }
            else
            {
                textId.Clear();
                textNom.Clear();
                textPrenom.Clear();
                numCoutJournalier.Value = 0;
            }
            _isLoading = false;
        }

        private void RefreshAddCompetenceComboBox()
        {
            var ouvrier = GetSelectedOuvrier();
            cmbAddCompetence.DataSource = null;

            if (ouvrier == null)
            {
                cmbAddCompetence.Enabled = false;
                cmbAddCompetence.Text = "Sélectionnez un ouvrier";
                return;
            }

            var metiersDisponibles = _ressourceService.GetMetiersDisponiblesPourOuvrier(ouvrier.OuvrierId);

            if (metiersDisponibles.Any())
            {
                var listWithPlaceholder = new List<Metier>
                {
                    new Metier { MetierId = "-1", Nom = "Ajouter une compétence..." }
                };
                listWithPlaceholder.AddRange(metiersDisponibles);

                cmbAddCompetence.DataSource = listWithPlaceholder;
                cmbAddCompetence.DisplayMember = "Nom";
                cmbAddCompetence.ValueMember = "MetierId";
                cmbAddCompetence.SelectedIndex = 0;
                cmbAddCompetence.Enabled = true;
            }
            else
            {
                cmbAddCompetence.Enabled = false;
                cmbAddCompetence.Text = "Toutes les compétences ajoutées";
            }
        }

        private void RefreshCompetencesGrid()
        {
            var ouvrier = GetSelectedOuvrier();
            if (ouvrier != null)
            {
                var competencesPourAffichage = ouvrier.Competences.Select(c => new
                {
                    c.MetierId,
                    MetierNom = _ressourceService.GetMetierById(c.MetierId)?.Nom ?? "(inconnu)",
                    c.EstMetierPrincipal
                }).ToList();
                gridCompetences.DataSource = competencesPourAffichage;
            }
            else
            {
                gridCompetences.DataSource = null;
            }
        }

        private void UpdateButtonStates()
        {
            bool ouvrierSelected = gridOuvriers.SelectedRows.Count > 0;
            groupDetails.Enabled = ouvrierSelected;
            groupCompetences.Enabled = ouvrierSelected;
            btnDeleteOuvrier.Enabled = ouvrierSelected;
        }
        #endregion

        #region Méthodes de tri
        public void ChangerModeTri(TriMode nouveauMode)
        {
            if (_currentTriMode != nouveauMode)
            {
                _currentTriMode = nouveauMode;
                string selectedOuvrierId = GetSelectedOuvrierId();
                RebuildOuvriersBindingList();

                if (selectedOuvrierId != null)
                {
                    SelectOuvrierInGrid(selectedOuvrierId);
                }
            }
        }

        // Méthodes pour les contrôles de tri (à connecter aux événements)
        private void RadioTriPatronyme_CheckedChanged(object sender, EventArgs e)
        {
            if (((KryptonRadioButton)sender).Checked)
            {
                ChangerModeTri(TriMode.Patronyme);
            }
        }

        private void RadioTriMetier_CheckedChanged(object sender, EventArgs e)
        {
            if (((KryptonRadioButton)sender).Checked)
            {
                ChangerModeTri(TriMode.Metier);
            }
        }
        #endregion

        #region Helpers (inchangés)
        private string GetSelectedOuvrierId()
        {
            if (gridOuvriers.SelectedRows.Count > 0)
            {
                return gridOuvriers.SelectedRows[0].Cells["OuvrierId"].Value?.ToString();
            }
            return null;
        }

        private Ouvrier GetSelectedOuvrier()
        {
            var id = GetSelectedOuvrierId();
            return id != null ? _ressourceService.GetOuvrierById(id) : null;
        }

        private string GetSelectedCompetenceId()
        {
            if (gridCompetences.SelectedRows.Count > 0)
            {
                return gridCompetences.SelectedRows[0].Cells["MetierId"].Value?.ToString();
            }
            return null;
        }

        private void SelectOuvrierInGrid(string ouvrierId)
        {
            for (int i = 0; i < _ouvriersBindingList.Count; i++)
            {
                if (_ouvriersBindingList[i].OuvrierId == ouvrierId)
                {
                    gridOuvriers.Rows[i].Selected = true;
                    gridOuvriers.FirstDisplayedScrollingRowIndex = i;
                    return;
                }
            }
        }
        #endregion

        #region Événements des contrôles - Version optimisée
        private void CmbAddCompetence_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading || cmbAddCompetence.SelectedIndex <= 0) return;

            var ouvrier = GetSelectedOuvrier();
            var selectedMetier = cmbAddCompetence.SelectedItem as Metier;

            if (ouvrier != null && selectedMetier != null)
            {
                try
                {
                    _ressourceService.AjouterCompetence(ouvrier.OuvrierId, selectedMetier.MetierId);

                    RefreshCompetencesGrid();
                    RefreshAddCompetenceComboBox();
                    RefreshSingleOuvrierInGrid(ouvrier.OuvrierId); // Mise à jour optimisée
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GridCompetences_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var ouvrier = GetSelectedOuvrier();
            if (ouvrier == null) return;

            string metierId = gridCompetences.Rows[e.RowIndex].Cells["MetierId"].Value.ToString();

            if (gridCompetences.Columns[e.ColumnIndex].Name == "EstPrincipal")
            {
                try
                {
                    _ressourceService.DefinirMetierPrincipal(ouvrier.OuvrierId, metierId);
                    RefreshCompetencesGrid();
                    RefreshSingleOuvrierInGrid(ouvrier.OuvrierId); // Mise à jour optimisée
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (gridCompetences.Columns[e.ColumnIndex].Name == "DeleteColumn")
            {
                try
                {
                    _ressourceService.SupprimerCompetence(ouvrier.OuvrierId, metierId);

                    RefreshCompetencesGrid();
                    RefreshAddCompetenceComboBox();
                    RefreshSingleOuvrierInGrid(ouvrier.OuvrierId); // Mise à jour optimisée
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textSearchOuvrier_TextChanged(object sender, EventArgs e)
        {
            RebuildOuvriersBindingList(); // Plus précis que RefreshOuvriersGrid
        }

        private void gridOuvriers_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            RefreshUIFromSelection();
        }

        private void DetailOuvrier_Changed(object sender, EventArgs e)
        {
            if (_isLoading) return;
            var ouvrier = GetSelectedOuvrier();
            if (ouvrier == null) return;

            ouvrier.Nom = textNom.Text;
            ouvrier.Prenom = textPrenom.Text;
            ouvrier.CoutJournalier = (int)numCoutJournalier.Value;

            _ressourceService.ModifierOuvrier(ouvrier);
            RefreshSingleOuvrierInGrid(ouvrier.OuvrierId); // Mise à jour optimisée
        }

        // Autres événements inchangés...
        private void btnNewOuvrier_Click(object sender, EventArgs e)
        {
            try
            {
                var nouvelOuvrier = _ressourceService.CreerOuvrierParDefaut();
                RefreshAll();
                SelectOuvrierInGrid(nouvelOuvrier.OuvrierId);
                textNom.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteOuvrier_Click(object sender, EventArgs e)
        {
            var ouvrier = GetSelectedOuvrier();
            if (ouvrier == null) return;

            if (MessageBox.Show($"Supprimer l'ouvrier '{ouvrier.NomComplet}' ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _ressourceService.SupprimerOuvrier(ouvrier.OuvrierId);
                    RefreshAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImporterOuvriers_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Sélectionner le fichier CSV des ouvriers à importer";
                ofd.Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*";

                if (ofd.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    try
                    {
                        var resultat = _importWizardOrchestrator.LancerWizardImportOuvriers(ofd.FileName);

                        if (resultat.EstSucces)
                        {
                            MessageBox.Show($"{resultat.NbOuvriersImportes} ouvriers ont été importés avec succès.",
                                            "Importation Réussie",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                            RefreshAll();
                        }
                        else if (!string.IsNullOrEmpty(resultat.MessageErreur))
                        {
                            if (!resultat.MessageErreur.Contains("annulée par l'utilisateur"))
                            {
                                MessageBox.Show($"L'importation a échoué : {resultat.MessageErreur}",
                                                "Erreur d'Importation",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Une erreur inattendue est survenue : {ex.Message}",
                                        "Erreur Critique",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnExporterOuvriers_Click(object sender, EventArgs e)
        {
            var ouvriersAExporter = _ressourceService.GetAllOuvriers();
            if (!ouvriersAExporter.Any())
            {
                MessageBox.Show("Il n'y a aucun ouvrier à exporter.", "Action impossible",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Exporter les ouvriers au format CSV";
                sfd.Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*";
                sfd.FileName = $"Export_Ouvriers_{DateTime.Now:yyyyMMdd}.csv";

                if (sfd.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    try
                    {
                        _exportService.ExporterOuvriersCSV(ouvriersAExporter, sfd.FileName);
                        MessageBox.Show($"Exportation terminée avec succès.\n{ouvriersAExporter.Count} ouvriers ont été exportés dans le fichier :\n{sfd.FileName}",
                                        "Export Réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Une erreur est survenue lors de l'exportation :\n{ex.Message}",
                                        "Erreur d'Exportation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Les modifications sont enregistrées automatiquement.", "Information",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}