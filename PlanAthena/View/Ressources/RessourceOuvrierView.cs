using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View.Ressources.MetierDiagram
{
    public partial class RessourceOuvrierView : UserControl
    {
        private readonly RessourceService _ressourceService;
        private readonly ProjetService _projetService;
        private readonly ImportService _importService;

        private bool _isLoading = false;

        public RessourceOuvrierView(RessourceService ressourceService, ProjetService projetService, ImportService importService)
        {
            InitializeComponent();
            _ressourceService = ressourceService;
            _projetService = projetService;
            _importService = importService;

            // Appliquer la règle "ID non éditable"
            textId.ReadOnly = true;
        }

        private void RessourceOuvrierView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            SetupGrids();
            RefreshAll();
        }

        private void SetupGrids()
        {
            // Configuration de la grille des ouvriers
            gridOuvriers.AutoGenerateColumns = false;
            gridOuvriers.Columns.Clear();
            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "OuvrierId", HeaderText = "ID", DataPropertyName = "OuvrierId", FillWeight = 20 });
            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "NomComplet", HeaderText = "Nom Prénom", DataPropertyName = "NomComplet", FillWeight = 50 });
            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "CoutJournalier", HeaderText = "Coût/j", DataPropertyName = "CoutJournalier", FillWeight = 15 });
            gridOuvriers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "CompetencesCount", HeaderText = "Comp.", DataPropertyName = "Competences.Count", FillWeight = 15 });

            // Configuration de la grille des compétences
            gridCompetences.AutoGenerateColumns = false;
            gridCompetences.Columns.Clear();
            gridCompetences.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "MetierId", HeaderText = "ID Métier", DataPropertyName = "MetierId", FillWeight = 30 });
            gridCompetences.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "MetierNom", HeaderText = "Nom du Métier", DataPropertyName = "MetierNom", FillWeight = 50 });
            gridCompetences.Columns.Add(new KryptonDataGridViewCheckBoxColumn { Name = "EstPrincipal", HeaderText = "Principal", DataPropertyName = "EstMetierPrincipal", FillWeight = 20 });
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
            UpdateButtonStates();
        }

        private void RefreshOuvriersGrid()
        {
            _isLoading = true;
            var recherche = textSearchOuvrier.Text.ToLowerInvariant();
            var ouvriers = _ressourceService.GetAllOuvriers()
                .Where(o => string.IsNullOrWhiteSpace(recherche) || o.NomComplet.ToLowerInvariant().Contains(recherche))
                .ToList();

            gridOuvriers.DataSource = ouvriers;
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
            bool competenceSelected = gridCompetences.SelectedRows.Count > 0;

            groupDetails.Enabled = ouvrierSelected;
            groupCompetences.Enabled = ouvrierSelected;
            btnDeleteOuvrier.Enabled = ouvrierSelected;

            btnAddCompetence.Enabled = ouvrierSelected;
            btnModifyCompetence.Enabled = competenceSelected;
            btnDeleteCompetence.Enabled = competenceSelected;
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

        private void btnAddCompetence_Click(object sender, EventArgs e)
        {
            var ouvrier = GetSelectedOuvrier();
            if (ouvrier == null) return;

            // La logique pour trouver les métiers et afficher une dialog existe déjà. On la réutilise.
            var metiersDisponibles = _ressourceService.GetMetiersDisponiblesPourOuvrier(ouvrier.OuvrierId);
            if (!metiersDisponibles.Any())
            {
                MessageBox.Show("Cet ouvrier possède déjà toutes les compétences disponibles.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // NOTE: Suppose l'existence d'une dialog 'CompetenceDialog'
            // using var dialog = new CompetenceDialog(metiersDisponibles, null);
            // if (dialog.ShowDialog() == DialogResult.OK)
            // {
            //     _ressourceService.AjouterCompetence(ouvrier.OuvrierId, dialog.MetierSelectionne.MetierId);
            //     RefreshAll();
            // }

            // Pour l'instant, sans la dialog, on simule l'ajout de la première compétence dispo
            _ressourceService.AjouterCompetence(ouvrier.OuvrierId, metiersDisponibles.First().MetierId);
            RefreshAll();
        }

        private void btnModifyCompetence_Click(object sender, EventArgs e)
        {
            var ouvrierId = GetSelectedOuvrierId();
            var competenceId = GetSelectedCompetenceId();
            if (ouvrierId == null || competenceId == null) return;

            try
            {
                _ressourceService.DefinirMetierPrincipal(ouvrierId, competenceId);
                RefreshCompetencesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteCompetence_Click(object sender, EventArgs e)
        {
            var ouvrierId = GetSelectedOuvrierId();
            var competenceId = GetSelectedCompetenceId();
            if (ouvrierId == null || competenceId == null) return;

            try
            {
                _ressourceService.SupprimerCompetence(ouvrierId, competenceId);
                RefreshCompetencesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
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