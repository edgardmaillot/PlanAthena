// Fichier: PlanAthena/Forms/OuvrierForm.cs
// Version: 0.4.5 (Refactorisation de l'état local)
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PlanAthena.Services.Business.DTOs; // Ajout pour OuvrierInfo DTO

namespace PlanAthena.Forms
{
    public partial class OuvrierForm : Form
    {
        private readonly RessourceService _ressourceService;
        private readonly ProjetService _projetService;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly ImportService _importService;

        // La source de vérité de la sélection est l'ID de l'entité.
        private string _ouvrierIdSelectionne = null;
        private string _competenceMetierIdSelectionnee = null;

        private bool _isLoading = false;

        public OuvrierForm(RessourceService ressourceService, ProjetService projetService, IIdGeneratorService idGeneratorService, ImportService importService)
        {
            InitializeComponent();
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _idGeneratorService = idGeneratorService ?? throw new ArgumentNullException(nameof(idGeneratorService));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        }

        private void OuvrierForm_Load(object sender, EventArgs e)
        {
            MettreAJourUI();
        }

        #region Helpers d'accès aux données

        /// <summary>
        /// Méthode helper qui récupère l'objet Ouvrier actif à la demande depuis le service.
        /// C'est le seul point d'accès à l'objet Ouvrier complet.
        /// </summary>
        private Ouvrier GetSelectedOuvrier()
        {
            if (string.IsNullOrEmpty(_ouvrierIdSelectionne))
            {
                return null;
            }
            return _ressourceService.GetOuvrierById(_ouvrierIdSelectionne);
        }

        /// <summary>
        /// Méthode helper qui récupère la compétence sélectionnée pour l'ouvrier actif.
        /// </summary>
        private CompetenceOuvrier GetSelectedCompetence()
        {
            var ouvrier = GetSelectedOuvrier();
            if (ouvrier == null || string.IsNullOrEmpty(_competenceMetierIdSelectionnee))
            {
                return null;
            }
            return ouvrier.Competences.FirstOrDefault(c => c.MetierId == _competenceMetierIdSelectionnee);
        }

        #endregion

        #region Gestion de l'Affichage

        private void MettreAJourUI()
        {
            RemplirListeOuvriers();

            // Tenter de resélectionner l'ouvrier
            if (_ouvrierIdSelectionne != null)
            {
                var itemToSelect = listViewOuvriers.Items.Cast<ListViewItem>().FirstOrDefault(i => (i.Tag as Ouvrier)?.OuvrierId == _ouvrierIdSelectionne);
                if (itemToSelect != null)
                {
                    itemToSelect.Selected = true;
                    itemToSelect.EnsureVisible();
                }
            }

            if (listViewOuvriers.SelectedItems.Count == 0)
            {
                _ouvrierIdSelectionne = null;
            }

            MettreAJourDetailsOuvrier();
            MettreAJourListeCompetences();
            MettreAJourEtatBoutons();
        }

        private void RemplirListeOuvriers()
        {
            _isLoading = true;
            listViewOuvriers.Items.Clear();

            var recherche = txtRechercheOuvrier.Text.ToLower();
            var tousLesOuvriers = _ressourceService.GetAllOuvriers();

            var ouvriersFiltres = tousLesOuvriers.Where(o =>
                string.IsNullOrWhiteSpace(recherche) ||
                o.OuvrierId.ToLower().Contains(recherche) ||
                o.NomComplet.ToLower().Contains(recherche)
            ).OrderBy(o => o.Nom).ThenBy(o => o.Prenom);

            foreach (var ouvrier in ouvriersFiltres)
            {
                var item = new ListViewItem(new[] {
                    ouvrier.OuvrierId,
                    ouvrier.NomComplet,
                    ouvrier.CoutJournalier + "€",
                    ouvrier.Competences.Count.ToString()
                })
                {
                    Tag = ouvrier // Le Tag contient l'objet Ouvrier complet
                };
                listViewOuvriers.Items.Add(item);
            }

            lblStatutOuvriers.Text = $"{listViewOuvriers.Items.Count}/{tousLesOuvriers.Count} ouvrier(s)";
            _isLoading = false;
        }

        private void MettreAJourDetailsOuvrier()
        {
            _isLoading = true;
            Ouvrier ouvrierSelectionne = GetSelectedOuvrier();

            if (ouvrierSelectionne != null)
            {
                txtOuvrierId.Text = ouvrierSelectionne.OuvrierId;
                txtNom.Text = ouvrierSelectionne.Nom;
                txtPrenom.Text = ouvrierSelectionne.Prenom;
                numCoutJournalier.Value = ouvrierSelectionne.CoutJournalier;
                txtOuvrierId.ReadOnly = true;
            }
            else
            {
                txtOuvrierId.Clear();
                txtNom.Clear();
                txtPrenom.Clear();
                numCoutJournalier.Value = 0;
            }
            _isLoading = false;
        }

        private void MettreAJourListeCompetences()
        {
            listViewCompetences.Items.Clear();
            lblStatutCompetences.Text = "0 compétence(s)";
            Ouvrier ouvrierSelectionne = GetSelectedOuvrier();

            if (ouvrierSelectionne != null)
            {
                foreach (var competence in ouvrierSelectionne.Competences.OrderBy(c => c.MetierId))
                {
                    var metier = _ressourceService.GetMetierById(competence.MetierId);
                    var nomMetier = metier?.Nom ?? "(Métier inconnu)";
                    var estPrincipal = competence.EstMetierPrincipal ? " (Principal)" : "";
                    var item = new ListViewItem(new[] { competence.MetierId, nomMetier + estPrincipal }) { Tag = competence };
                    if (competence.EstMetierPrincipal) { item.Font = new System.Drawing.Font(listViewCompetences.Font, System.Drawing.FontStyle.Bold); }
                    listViewCompetences.Items.Add(item);
                }
                lblStatutCompetences.Text = $"{ouvrierSelectionne.Competences.Count} compétence(s)";
            }
        }

        private void MettreAJourEtatBoutons()
        {
            bool ouvrierEstSelectionne = !string.IsNullOrEmpty(_ouvrierIdSelectionne);
            bool competenceEstSelectionnee = !string.IsNullOrEmpty(_competenceMetierIdSelectionnee);

            groupBoxDetailsOuvrier.Enabled = ouvrierEstSelectionne;
            groupBoxCompetences.Enabled = ouvrierEstSelectionne;
            btnModifierOuvrier.Enabled = ouvrierEstSelectionne;
            btnSupprimerOuvrier.Enabled = ouvrierEstSelectionne;
            btnAjouterCompetence.Enabled = ouvrierEstSelectionne;
            btnModifierCompetence.Enabled = competenceEstSelectionnee;
            btnSupprimerCompetence.Enabled = competenceEstSelectionnee;
        }

        #endregion

        #region Événements

        private void listViewOuvriers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            if (listViewOuvriers.SelectedItems.Count > 0)
            {
                var ouvrierTag = listViewOuvriers.SelectedItems[0].Tag as Ouvrier;
                _ouvrierIdSelectionne = ouvrierTag?.OuvrierId;
            }
            else
            {
                _ouvrierIdSelectionne = null;
            }

            _competenceMetierIdSelectionnee = null;
            MettreAJourDetailsOuvrier();
            MettreAJourListeCompetences();
            MettreAJourEtatBoutons();
        }

        private void listViewCompetences_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCompetences.SelectedItems.Count > 0)
            {
                var competenceTag = listViewCompetences.SelectedItems[0].Tag as CompetenceOuvrier;
                _competenceMetierIdSelectionnee = competenceTag?.MetierId;
            }
            else
            {
                _competenceMetierIdSelectionnee = null;
            }
            MettreAJourEtatBoutons();
        }

        private void txtRechercheOuvrier_TextChanged(object sender, EventArgs e)
        {
            RemplirListeOuvriers();
        }

        private void DetailOuvrier_Changed(object sender, EventArgs e)
        {
            if (_isLoading) return;
            Ouvrier ouvrierAModifier = GetSelectedOuvrier();
            if (ouvrierAModifier == null) return;

            ouvrierAModifier.Nom = txtNom.Text.Trim();
            ouvrierAModifier.Prenom = txtPrenom.Text.Trim();
            ouvrierAModifier.CoutJournalier = (int)numCoutJournalier.Value;

            _ressourceService.ModifierOuvrier(ouvrierAModifier);

            // Rafraîchit l'item dans la liste sans recharger toute la liste
            var itemToUpdate = listViewOuvriers.Items.Cast<ListViewItem>().FirstOrDefault(i => (i.Tag as Ouvrier)?.OuvrierId == _ouvrierIdSelectionne);
            if (itemToUpdate != null)
            {
                itemToUpdate.SubItems[1].Text = ouvrierAModifier.NomComplet;
                itemToUpdate.SubItems[2].Text = ouvrierAModifier.CoutJournalier + "€";
            }
        }

        #endregion

        #region Actions CRUD

        private void btnNouveauOuvrier_Click(object sender, EventArgs e)
        {
            try
            {
                var nouvelOuvrier = _ressourceService.CreerOuvrierParDefaut();
                _ouvrierIdSelectionne = nouvelOuvrier.OuvrierId; // Sélectionne le nouvel ouvrier
                MettreAJourUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAjouterCompetence_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_ouvrierIdSelectionne)) return;

            // Logique de récupération des métiers disponibles déplacée dans le service
            var metiersDisponibles = _ressourceService.GetMetiersDisponiblesPourOuvrier(_ouvrierIdSelectionne);
            if (!metiersDisponibles.Any())
            {
                MessageBox.Show("L'ouvrier maîtrise déjà tous les métiers.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new CompetenceDialog(metiersDisponibles, null);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _ressourceService.AjouterCompetence(_ouvrierIdSelectionne, dialog.MetierSelectionne.MetierId);
                    MettreAJourUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSupprimerOuvrier_Click(object sender, EventArgs e)
        {
            Ouvrier ouvrierASupprimer = GetSelectedOuvrier();
            if (ouvrierASupprimer == null) return;

            var result = MessageBox.Show($"Supprimer l'ouvrier '{ouvrierASupprimer.NomComplet}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _ressourceService.SupprimerOuvrier(ouvrierASupprimer.OuvrierId);
                    _ouvrierIdSelectionne = null; // Désélectionne
                    MettreAJourUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnModifierCompetence_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_ouvrierIdSelectionne) || string.IsNullOrEmpty(_competenceMetierIdSelectionnee)) return;

            var metier = _ressourceService.GetMetierById(_competenceMetierIdSelectionnee);
            var result = MessageBox.Show(
                $"Définir '{metier?.Nom}' comme métier principal ?",
                "Définir comme principal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _ressourceService.DefinirMetierPrincipal(_ouvrierIdSelectionne, _competenceMetierIdSelectionnee);
                MettreAJourListeCompetences(); // Rafraîchissement ciblé
            }
        }

        private void btnSupprimerCompetence_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_ouvrierIdSelectionne) || string.IsNullOrEmpty(_competenceMetierIdSelectionnee)) return;

            var metier = _ressourceService.GetMetierById(_competenceMetierIdSelectionnee);
            var result = MessageBox.Show($"Supprimer la compétence '{metier?.Nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _ressourceService.SupprimerCompetence(_ouvrierIdSelectionne, _competenceMetierIdSelectionnee);
                    _competenceMetierIdSelectionnee = null; // Désélectionne
                    MettreAJourUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnModifierOuvrier_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_ouvrierIdSelectionne))
            {
                txtNom.Focus();
            }
        }

        #endregion

        #region Import/Export/Fermer
        private void btnImporter_Click(object sender, EventArgs e)
        {
            // La logique d'import/export reste la même car elle appelle un service externe
            // qui se charge d'orchestrer. Le rafraîchissement est juste plus simple.
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                Title = "Importer les ouvriers depuis un fichier CSV"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Voulez-vous remplacer tous les ouvriers existants ?",
                        "Mode d'import",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel) return;

                    var nombreImporte = _importService.ImporterOuvriersCSV(ofd.FileName, result == DialogResult.Yes);

                    _ouvrierIdSelectionne = null; // Désélectionne tout
                    MettreAJourUI();

                    MessageBox.Show(
                        $"Import terminé avec succès ! {nombreImporte} ligne(s) importée(s).",
                        "Import réussi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'import :\n{ex.Message}",
                        "Erreur d'import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportExcelSAP_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "L'import depuis Excel SAP sera implémenté dans une future version.",
                "Fonctionnalité en développement",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!_ressourceService.GetAllOuvriers().Any())
            {
                MessageBox.Show("Aucun ouvrier à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                Title = "Exporter les ouvriers vers un fichier CSV",
                FileName = $"ouvriers_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _importService.ExporterOuvriersCSV(sfd.FileName);
                    MessageBox.Show(
                        $"Export terminé avec succès !",
                        "Export réussi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'export :\n{ex.Message}",
                        "Erreur d'export",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Ces méthodes ne sont plus nécessaires car remplacées par MettreAJourUI
        private void RafraichirAffichage() { MettreAJourUI(); }
        private void RafraichirListeOuvriers() { RemplirListeOuvriers(); }
        private void RafraichirStatutOuvriers() { /* Géré par RemplirListeOuvriers */ }
        private void RafraichirDetailsOuvrier() { MettreAJourDetailsOuvrier(); }
        private void RafraichirCompetences() { MettreAJourListeCompetences(); }

        #endregion
    }
}