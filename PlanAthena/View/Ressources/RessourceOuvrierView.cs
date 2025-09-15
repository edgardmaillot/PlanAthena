using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.DTOs.ImportExport;
using PlanAthena.Services.Usecases;
using System.Data;

namespace PlanAthena.View.Ressources.MetierDiagram
{
    public partial class RessourceOuvrierView : UserControl
    {
        private readonly RessourceService _ressourceService;
        private readonly ProjetService _projetService;
        private readonly ImportWizardOrchestrator _importWizardOrchestrator;
        private readonly ExportService _exportService;

        private bool _isLoading = false;

        public RessourceOuvrierView(RessourceService ressourceService, ProjetService projetService, ImportWizardOrchestrator importWizardOrchestrator, ExportService exportService)
        {
            InitializeComponent();
            _ressourceService = ressourceService;
            _projetService = projetService;
            _importWizardOrchestrator = importWizardOrchestrator;
            _exportService = exportService;
            // Appliquer la règle "ID non éditable"
            textId.ReadOnly = true;

        }

        private void RessourceOuvrierView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            SetupGrids();

            // Attacher les nouveaux événements
            cmbAddCompetence.SelectedIndexChanged += CmbAddCompetence_SelectedIndexChanged;
            gridCompetences.CellContentClick += GridCompetences_CellContentClick;

            RefreshAll();
        }

        private void SetupGrids()
        {
            // Configuration de la grille des ouvriers
            gridOuvriers.AutoGenerateColumns = false;
            gridOuvriers.Columns.Clear();
            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "OuvrierId", HeaderText = "ID", DataPropertyName = "OuvrierId", Visible = false });
            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "NomComplet", HeaderText = "Nom Prénom", DataPropertyName = "NomComplet", FillWeight = 60 });
            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "CoutJournalier", HeaderText = "Coût/j", DataPropertyName = "CoutJournalier", FillWeight = 20 });
            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "CompetencesCount", HeaderText = "Comp.", DataPropertyName = "CompetencesCount", FillWeight = 20 });

            // Configuration de la grille des compétences
            gridCompetences.AutoGenerateColumns = false;
            gridCompetences.Columns.Clear();
            gridCompetences.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "MetierId", HeaderText = "ID Métier", DataPropertyName = "MetierId", FillWeight = 1, Visible = false });
            gridCompetences.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "MetierNom", HeaderText = "Nom du Métier", DataPropertyName = "MetierNom", FillWeight = 90 });
            gridCompetences.Columns.Add(new KryptonDataGridViewCheckBoxColumn { Name = "EstPrincipal", HeaderText = "Principal", DataPropertyName = "EstMetierPrincipal", FillWeight = 10 });
            var deleteColumn = new KryptonDataGridViewButtonColumn
            {
                Name = "DeleteColumn",
                Text = "Supprimer",
                UseColumnTextForButtonValue = true, // Affiche "Supprimer" sur chaque bouton
                HeaderText = "", // Pas de titre pour la colonne
                FillWeight = 20
            };
            gridCompetences.Columns.Add(deleteColumn);

        }

        #region Logique de rafraîchissement

        private void RefreshAll()
        {
            string selectedOuvrierId = GetSelectedOuvrierId();

            RefreshOuvriersGrid();

            // Tenter de resélectionner l'ouvrier
            if (selectedOuvrierId != null)
            {
                SelectOuvrierInGrid(selectedOuvrierId);
            }

            RefreshUIFromSelection();
        }

        private void RefreshUIFromSelection()
        {
            RefreshDetails();
            RefreshCompetencesGrid();
            RefreshAddCompetenceComboBox();
            UpdateButtonStates();
        }

        private void RefreshOuvriersGrid()
        {
            _isLoading = true;
            var recherche = textSearchOuvrier.Text.ToLowerInvariant();

            // On récupère les ouvriers comme avant
            var ouvriers = _ressourceService.GetAllOuvriers()
                .Where(o => string.IsNullOrWhiteSpace(recherche) || o.NomComplet.ToLowerInvariant().Contains(recherche));

            // --- MODIFICATION ICI ---
            // On transforme la liste d'Ouvrier en une liste d'objets faits pour l'affichage
            var dataSource = ouvriers.Select(o => new
            {
                // On garde les propriétés que la grille utilise déjà
                o.OuvrierId,
                o.NomComplet,
                o.CoutJournalier,
                // On crée une propriété simple et directe pour le comptage
                CompetencesCount = o.Competences.Count
            }).ToList();

            gridOuvriers.DataSource = dataSource;
            _isLoading = false;
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
        // Nouvelle méthode pour rafraîchir le ComboBox
        private void RefreshAddCompetenceComboBox()
        {
            var ouvrier = GetSelectedOuvrier();
            cmbAddCompetence.DataSource = null; // Vider d'abord

            if (ouvrier == null)
            {
                cmbAddCompetence.Enabled = false;
                cmbAddCompetence.Text = "Sélectionnez un ouvrier";
                return;
            }

            var metiersDisponibles = _ressourceService.GetMetiersDisponiblesPourOuvrier(ouvrier.OuvrierId);

            if (metiersDisponibles.Any())
            {
                // Astuce pour le placeholder : créer une liste et ajouter un item factice au début
                var listWithPlaceholder = new List<Metier>
        {
            new Metier { MetierId = "-1", Nom = "Ajouter une compétence..." }
        };
                listWithPlaceholder.AddRange(metiersDisponibles);

                cmbAddCompetence.DataSource = listWithPlaceholder;
                cmbAddCompetence.DisplayMember = "Nom";
                cmbAddCompetence.ValueMember = "MetierId";
                cmbAddCompetence.SelectedIndex = 0; // Sélectionner le placeholder
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
                // On crée une liste anonyme pour afficher le nom du métier
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

        #region Helpers

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
            foreach (DataGridViewRow row in gridOuvriers.Rows)
            {
                if (row.Cells["OuvrierId"].Value?.ToString() == ouvrierId)
                {
                    row.Selected = true;
                    gridOuvriers.FirstDisplayedScrollingRowIndex = row.Index;
                    return;
                }
            }
        }

        #endregion

        #region Événements des contrôles
        // Nouvel événement pour le ComboBox
        private void CmbAddCompetence_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading || cmbAddCompetence.SelectedIndex <= 0) return; // Ignore si loading ou placeholder

            var ouvrier = GetSelectedOuvrier();
            var selectedMetier = cmbAddCompetence.SelectedItem as Metier;

            if (ouvrier != null && selectedMetier != null)
            {
                try
                {
                    _ressourceService.AjouterCompetence(ouvrier.OuvrierId, selectedMetier.MetierId);

                    // Rafraîchir ce qui est nécessaire
                    RefreshCompetencesGrid();
                    RefreshAddCompetenceComboBox();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        // Nouvel événement unique pour gérer les clics DANS la grille
        private void GridCompetences_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignorer les clics sur l'en-tête
            if (e.RowIndex < 0) return;

            var ouvrier = GetSelectedOuvrier();
            if (ouvrier == null) return;

            // Récupérer l'ID du métier de la ligne cliquée
            string metierId = gridCompetences.Rows[e.RowIndex].Cells["MetierId"].Value.ToString();

            // -- GESTION DE LA MODIFICATION (CASE À COCHER) --
            if (gridCompetences.Columns[e.ColumnIndex].Name == "EstPrincipal")
            {
                try
                {
                    _ressourceService.DefinirMetierPrincipal(ouvrier.OuvrierId, metierId);
                    RefreshCompetencesGrid(); // Juste rafraîchir la grille pour voir le changement
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // -- GESTION DE LA SUPPRESSION (BOUTON) --
            if (gridCompetences.Columns[e.ColumnIndex].Name == "DeleteColumn")
            {
                try
                {
                    _ressourceService.SupprimerCompetence(ouvrier.OuvrierId, metierId);

                    // Rafraîchir la grille ET le ComboBox (car une compétence redevient disponible)
                    RefreshCompetencesGrid();
                    RefreshAddCompetenceComboBox();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void textSearchOuvrier_TextChanged(object sender, EventArgs e)
        {
            RefreshOuvriersGrid();
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

            // Rafraîchir la ligne dans la grille sans tout recharger
            if (gridOuvriers.SelectedRows.Count > 0)
            {
                var row = gridOuvriers.SelectedRows[0];
                row.Cells["NomComplet"].Value = ouvrier.NomComplet;
                row.Cells["CoutJournalier"].Value = ouvrier.CoutJournalier;
            }
        }

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

            // Règle: pas de confirmation, sauf pour les entités majeures.
            // On considère un ouvrier comme une entité majeure.
            if (MessageBox.Show($"Supprimer l'ouvrier '{ouvrier.NomComplet}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
        // Dans votre fichier de vue (ex: RessourcesView.cs)

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
                        // La vue ne fait qu'un seul appel : elle lance le wizard.
                        // Toute la complexité (MessageBox, enchaînement des étapes) est dans l'orchestrateur.
                        var resultat = _importWizardOrchestrator.LancerWizardImportOuvriers(ofd.FileName);

                        // La vue affiche le résultat final, quel qu'il soit.
                        if (resultat.EstSucces)
                        {
                            MessageBox.Show($"{resultat.NbOuvriersImportes} ouvriers ont été importés avec succès.", // Adapter le message au DTO de résultat
                                            "Importation Réussie",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                            RefreshAll(); // Ou le nom de votre méthode de rafraîchissement
                        }
                        else if (!string.IsNullOrEmpty(resultat.MessageErreur))
                        {
                            // Ne pas afficher de message si l'erreur est "Annulé par l'utilisateur"
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
            // 1. Récupérer les données depuis le service métier
            var ouvriersAExporter = _ressourceService.GetAllOuvriers();
            if (!ouvriersAExporter.Any())
            {
                MessageBox.Show("Il n'y a aucun ouvrier à exporter.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // 2. Créer la liste des DTOs pour l'export
            var dtosPourExport = new List<OuvrierExportDto>();
            foreach (var ouvrier in ouvriersAExporter)
            {
                if (ouvrier.Competences.Any())
                {
                    foreach (var competence in ouvrier.Competences)
                    {
                        var metier = _ressourceService.GetMetierById(competence.MetierId);
                        dtosPourExport.Add(new OuvrierExportDto
                        {
                            OuvrierId = ouvrier.OuvrierId,
                            Nom = ouvrier.Nom,
                            Prenom = ouvrier.Prenom,
                            CoutJournalier = ouvrier.CoutJournalier,
                            MetierNom = metier?.Nom ?? competence.MetierId // Traduction
                        });
                    }
                }
                else
                {
                    // Ouvrier sans compétence
                    dtosPourExport.Add(new OuvrierExportDto
                    {
                        OuvrierId = ouvrier.OuvrierId,
                        Nom = ouvrier.Nom,
                        Prenom = ouvrier.Prenom,
                        CoutJournalier = ouvrier.CoutJournalier,
                        MetierNom = ""
                    });
                }
            }
            // 3. Ouvrir la boîte de dialogue pour choisir l'emplacement du fichier
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Exporter les ouvriers au format CSV";
                sfd.Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*";
                sfd.FileName = $"Export_Ouvriers_{DateTime.Now:yyyyMMdd}.csv";

                if (sfd.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    try
                    {
                        // 3. Appeler le service d'export avec les données et le chemin
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
            // La sauvegarde est automatique. Ce bouton sert à "Valider et revenir".
            // Pour l'instant, on ne le fait pas naviguer.
            MessageBox.Show("Les modifications sont enregistrées automatiquement.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}