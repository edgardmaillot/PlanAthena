using PlanAthena.CsvModels;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena
{
    public partial class MetierForm : Form
    {
        private readonly MetierService _metierService;
        private readonly CsvDataService _csvDataService;
        private List<MetierCsvRecord> _metiers = new List<MetierCsvRecord>();
        private MetierCsvRecord _metierSelectionne = null;
        private bool _enModification = false;

        public MetierForm(MetierService metierService)
        {
            InitializeComponent();
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _csvDataService = new CsvDataService();
        }

        private void MetierForm_Load(object sender, EventArgs e)
        {
            ChargerMetiers();
            RafraichirAffichage();
            InitialiserInterface();
        }

        private void InitialiserInterface()
        {
            // Désactiver les contrôles de détail au démarrage
            groupBoxDetails.Enabled = false;
            btnModifier.Enabled = false;
            btnSupprimer.Enabled = false;
        }

        #region Gestion des données

        private void ChargerMetiers()
        {
            _metiers = _metierService.GetAllMetiers().ToList();
        }

        private void RafraichirAffichage()
        {
            RafraichirListeMetiers();
            RafraichirStatut();
            RafraichirDetails();
        }

        private void RafraichirListeMetiers()
        {
            listViewMetiers.Items.Clear();

            var metiersAffiches = _metiers.AsEnumerable();

            // Filtrage par recherche
            if (!string.IsNullOrWhiteSpace(txtRecherche.Text))
            {
                var recherche = txtRecherche.Text.ToLower();
                metiersAffiches = metiersAffiches.Where(m =>
                    m.MetierId.ToLower().Contains(recherche) ||
                    m.Nom.ToLower().Contains(recherche));
            }

            foreach (var metier in metiersAffiches.OrderBy(m => m.MetierId))
            {
                var prerequis = ObtenirPrerequisCount(metier.MetierId);
                var item = new ListViewItem(new[] {
                    metier.MetierId,
                    metier.Nom,
                    prerequis.ToString()
                })
                {
                    Tag = metier
                };
                listViewMetiers.Items.Add(item);
            }
        }

        private void RafraichirStatut()
        {
            var total = _metiers.Count;
            var affiches = listViewMetiers.Items.Count;

            if (affiches == total)
                lblStatut.Text = $"{total} métier(s)";
            else
                lblStatut.Text = $"{affiches}/{total} métier(s) affiché(s)";
        }

        private void RafraichirDetails()
        {
            if (_metierSelectionne == null)
            {
                groupBoxDetails.Enabled = false;
                txtMetierId.Clear();
                txtNom.Clear();
                listViewPrerequis.Items.Clear();
                lblUtilisation.Text = "";
                return;
            }

            groupBoxDetails.Enabled = true;
            txtMetierId.Text = _metierSelectionne.MetierId;
            txtNom.Text = _metierSelectionne.Nom;

            RafraichirListePrerequis();
            RafraichirUtilisation();

            // Gestion du mode modification
            txtMetierId.ReadOnly = _enModification;
        }

        private void RafraichirListePrerequis()
        {
            listViewPrerequis.Items.Clear();

            if (_metierSelectionne != null && !string.IsNullOrEmpty(_metierSelectionne.PrerequisMetierIds))
            {
                var prerequisIds = _metierSelectionne.PrerequisMetierIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var prerequisId in prerequisIds)
                {
                    var prerequisMetier = _metiers.FirstOrDefault(m => m.MetierId == prerequisId);
                    var nom = prerequisMetier?.Nom ?? "(Métier non trouvé)";

                    var item = new ListViewItem(new[] { prerequisId, nom })
                    {
                        Tag = prerequisId
                    };
                    listViewPrerequis.Items.Add(item);
                }
            }
        }

        private void RafraichirUtilisation()
        {
            if (_metierSelectionne == null)
            {
                lblUtilisation.Text = "";
                return;
            }

            // Compter l'utilisation du métier (simulation)
            var utilisationTaches = 0; // TODO: Compter les tâches utilisant ce métier
            var utilisationOuvriers = 0; // TODO: Compter les ouvriers ayant ce métier

            if (utilisationTaches > 0 || utilisationOuvriers > 0)
            {
                lblUtilisation.Text = $"⚠️ Utilisé par: {utilisationTaches} tâche(s), {utilisationOuvriers} ouvrier(s)";
                lblUtilisation.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                lblUtilisation.Text = "✅ Métier non utilisé - peut être supprimé";
                lblUtilisation.ForeColor = System.Drawing.Color.Green;
            }
        }

        private int ObtenirPrerequisCount(string metierId)
        {
            var metier = _metiers.FirstOrDefault(m => m.MetierId == metierId);
            if (metier == null || string.IsNullOrEmpty(metier.PrerequisMetierIds))
                return 0;

            return metier.PrerequisMetierIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        #endregion

        #region Événements interface

        private void listViewMetiers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewMetiers.SelectedItems.Count > 0)
            {
                _metierSelectionne = listViewMetiers.SelectedItems[0].Tag as MetierCsvRecord;
                btnModifier.Enabled = true;
                btnSupprimer.Enabled = true;
            }
            else
            {
                _metierSelectionne = null;
                btnModifier.Enabled = false;
                btnSupprimer.Enabled = false;
            }

            RafraichirDetails();
        }

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            RafraichirListeMetiers();
            RafraichirStatut();
        }

        private void txtMetierId_TextChanged(object sender, EventArgs e)
        {
            if (_metierSelectionne != null && !_enModification)
            {
                _metierSelectionne.MetierId = txtMetierId.Text;
            }
        }

        private void txtNom_TextChanged(object sender, EventArgs e)
        {
            if (_metierSelectionne != null)
            {
                _metierSelectionne.Nom = txtNom.Text;
            }
        }

        #endregion

        #region Actions CRUD

        private void btnNouveau_Click(object sender, EventArgs e)
        {
            var nouveauMetier = new MetierCsvRecord
            {
                MetierId = "",
                Nom = "Nouveau métier",
                PrerequisMetierIds = ""
            };

            _metiers.Add(nouveauMetier);
            _metierSelectionne = nouveauMetier;
            _enModification = false;

            RafraichirAffichage();

            // Sélectionner le nouveau métier dans la liste
            var item = listViewMetiers.Items.Cast<ListViewItem>()
                .FirstOrDefault(i => i.Tag == nouveauMetier);
            if (item != null)
            {
                item.Selected = true;
                listViewMetiers.Focus();
            }

            // Focus sur l'ID pour saisie
            txtMetierId.Focus();
            txtMetierId.SelectAll();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (_metierSelectionne == null) return;

            _enModification = true;
            RafraichirDetails();
            txtNom.Focus();
            txtNom.SelectAll();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_metierSelectionne == null) return;

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer le métier '{_metierSelectionne.MetierId}' ?\n\n" +
                "Cette action est irréversible.",
                "Confirmation de suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _metiers.Remove(_metierSelectionne);
                _metierSelectionne = null;
                _enModification = false;

                // Sauvegarder dans le service
                _metierService.ChargerMetiers(_metiers);

                RafraichirAffichage();
                MessageBox.Show("Métier supprimé avec succès.", "Suppression", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Gestion des prérequis

        private void btnAjouterPrerequis_Click(object sender, EventArgs e)
        {
            if (_metierSelectionne == null) return;

            var metiersDisponibles = _metiers
                .Where(m => m.MetierId != _metierSelectionne.MetierId)
                .Where(m => !EstDejaPrerequisDe(m.MetierId, _metierSelectionne.MetierId))
                .ToList();

            if (!metiersDisponibles.Any())
            {
                MessageBox.Show("Aucun métier disponible à ajouter comme prérequis.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SelectionMetierDialog(metiersDisponibles);
            if (dialog.ShowDialog() == DialogResult.OK && dialog.MetierSelectionne != null)
            {
                AjouterPrerequis(dialog.MetierSelectionne.MetierId);
            }
        }

        private void btnSupprimerPrerequis_Click(object sender, EventArgs e)
        {
            if (listViewPrerequis.SelectedItems.Count == 0) return;

            var prerequisId = listViewPrerequis.SelectedItems[0].Tag as string;
            SupprimerPrerequis(prerequisId);
        }

        private void AjouterPrerequis(string prerequisId)
        {
            if (_metierSelectionne == null) return;

            var prerequisActuels = string.IsNullOrEmpty(_metierSelectionne.PrerequisMetierIds)
                ? new List<string>()
                : _metierSelectionne.PrerequisMetierIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            if (!prerequisActuels.Contains(prerequisId))
            {
                prerequisActuels.Add(prerequisId);
                _metierSelectionne.PrerequisMetierIds = string.Join(",", prerequisActuels);

                // Sauvegarder dans le service
                _metierService.ChargerMetiers(_metiers);

                RafraichirListePrerequis();
                RafraichirListeMetiers(); // Pour mettre à jour le nombre de prérequis
            }
        }

        private void SupprimerPrerequis(string prerequisId)
        {
            if (_metierSelectionne == null || string.IsNullOrEmpty(prerequisId)) return;

            var prerequisActuels = _metierSelectionne.PrerequisMetierIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            if (prerequisActuels.Remove(prerequisId))
            {
                _metierSelectionne.PrerequisMetierIds = string.Join(",", prerequisActuels);

                // Sauvegarder dans le service
                _metierService.ChargerMetiers(_metiers);

                RafraichirListePrerequis();
                RafraichirListeMetiers(); // Pour mettre à jour le nombre de prérequis
            }
        }

        private bool EstDejaPrerequisDe(string metierAVerifier, string metierCible)
        {
            var metierCibleRecord = _metiers.FirstOrDefault(m => m.MetierId == metierCible);
            if (metierCibleRecord == null || string.IsNullOrEmpty(metierCibleRecord.PrerequisMetierIds))
                return false;

            var prerequis = metierCibleRecord.PrerequisMetierIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return prerequis.Contains(metierAVerifier);
        }

        #endregion

        #region Import/Export

        private void btnImporter_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                Title = "Importer les métiers depuis un fichier CSV"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Voulez-vous remplacer tous les métiers existants ?\n\n" +
                        "• Oui : Remplace tous les métiers actuels\n" +
                        "• Non : Ajoute aux métiers existants\n" +
                        "• Annuler : Annule l'import",
                        "Mode d'import",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel) return;

                    var metiersImportes = _csvDataService.ImportCsv<MetierCsvRecord>(ofd.FileName);

                    if (result == DialogResult.Yes)
                    {
                        // Remplacer tous
                        _metiers = metiersImportes;
                    }
                    else
                    {
                        // Ajouter en évitant les doublons
                        foreach (var metierImporte in metiersImportes)
                        {
                            var existant = _metiers.FirstOrDefault(m => m.MetierId == metierImporte.MetierId);
                            if (existant != null)
                            {
                                // Mettre à jour l'existant
                                existant.Nom = metierImporte.Nom;
                                existant.PrerequisMetierIds = metierImporte.PrerequisMetierIds;
                            }
                            else
                            {
                                // Ajouter le nouveau
                                _metiers.Add(metierImporte);
                            }
                        }
                    }

                    // Sauvegarder dans le service
                    _metierService.ChargerMetiers(_metiers);

                    _metierSelectionne = null;
                    _enModification = false;
                    RafraichirAffichage();

                    MessageBox.Show(
                        $"Import terminé avec succès !\n\n" +
                        $"• {metiersImportes.Count} métiers importés\n" +
                        $"• {_metiers.Count} métiers total",
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

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!_metiers.Any())
            {
                MessageBox.Show("Aucun métier à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                Title = "Exporter les métiers vers un fichier CSV",
                FileName = $"metiers_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _csvDataService.ExportCsv(_metiers, sfd.FileName);

                    MessageBox.Show(
                        $"Export terminé avec succès !\n\n" +
                        $"• {_metiers.Count} métiers exportés\n" +
                        $"• Fichier : {Path.GetFileName(sfd.FileName)}",
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

        #endregion

        #region Actions du formulaire

        private void btnFermer_Click(object sender, EventArgs e)
        {
            // Sauvegarder les modifications en cours dans le service
            if (_metiers.Any())
            {
                _metierService.ChargerMetiers(_metiers);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Sauvegarder automatiquement les modifications
            if (_metiers.Any())
            {
                _metierService.ChargerMetiers(_metiers);
            }

            base.OnFormClosing(e);
        }

        #endregion
    }

    

}