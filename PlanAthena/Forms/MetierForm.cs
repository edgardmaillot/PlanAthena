using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;

namespace PlanAthena.Forms
{
    public partial class MetierForm : System.Windows.Forms.Form
    {
        private readonly MetierService _metierService;
        private readonly CsvDataService _csvDataService;
        private MetierRecord _metierSelectionne = null;

        // Le mode "Modification" est maintenant géré par l'état des contrôles
        private bool IsEditing => txtMetierId.ReadOnly == false;

        public MetierForm(MetierService metierService)
        {
            InitializeComponent();
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _csvDataService = new CsvDataService();
        }

        private void MetierForm_Load(object sender, EventArgs e)
        {
            RafraichirAffichageComplet();
            InitialiserInterface();
        }

        private void InitialiserInterface()
        {
            groupBoxDetails.Enabled = false;
            btnModifier.Enabled = false;
            btnSupprimer.Enabled = false;
        }

        #region Gestion des données et affichage

        private void RafraichirAffichageComplet()
        {
            var idMetierSelectionne = _metierSelectionne?.MetierId;

            RafraichirListeMetiers();

            // Essayer de re-sélectionner l'élément
            if (idMetierSelectionne != null)
            {
                var itemToReselect = listViewMetiers.Items.Cast<ListViewItem>()
                    .FirstOrDefault(item => (item.Tag as MetierRecord)?.MetierId == idMetierSelectionne);
                if (itemToReselect != null)
                {
                    itemToReselect.Selected = true;
                }
            }

            // Si plus rien n'est sélectionné, nettoyer les détails
            if (listViewMetiers.SelectedItems.Count == 0)
            {
                _metierSelectionne = null;
                NettoyerDetails();
            }

            RafraichirStatut();
        }

        private void RafraichirListeMetiers()
        {
            listViewMetiers.Items.Clear();
            var metiersAffiches = _metierService.GetAllMetiers();

            if (!string.IsNullOrWhiteSpace(txtRecherche.Text))
            {
                var recherche = txtRecherche.Text.ToLower();
                metiersAffiches = metiersAffiches.Where(m =>
                    m.MetierId.ToLower().Contains(recherche) ||
                    m.Nom.ToLower().Contains(recherche)).ToList();
            }

            foreach (var metier in metiersAffiches.OrderBy(m => m.MetierId))
            {
                var prerequisCount = _metierService.GetPrerequisForMetier(metier.MetierId).Count;
                var item = new ListViewItem(new[] { metier.MetierId, metier.Nom, prerequisCount.ToString() })
                {
                    Tag = metier
                };
                listViewMetiers.Items.Add(item);
            }
        }

        private void RafraichirStatut()
        {
            var total = _metierService.GetAllMetiers().Count;
            var affiches = listViewMetiers.Items.Count;
            lblStatut.Text = (affiches == total) ? $"{total} métier(s)" : $"{affiches}/{total} métier(s) affiché(s)";
        }

        private void AfficherDetailsMetier(MetierRecord metier)
        {
            _metierSelectionne = metier;
            if (metier == null)
            {
                NettoyerDetails();
                return;
            }

            groupBoxDetails.Enabled = true;
            txtMetierId.Text = metier.MetierId;
            txtNom.Text = metier.Nom;
            txtMetierId.ReadOnly = true;
            txtNom.ReadOnly = true;

            RafraichirListePrerequis();
            RafraichirUtilisation();
        }

        private void NettoyerDetails()
        {
            _metierSelectionne = null;
            groupBoxDetails.Enabled = false;
            txtMetierId.Clear();
            txtNom.Clear();
            listViewPrerequis.Items.Clear();
            lblUtilisation.Text = "";
        }

        private void RafraichirListePrerequis()
        {
            listViewPrerequis.Items.Clear();
            if (_metierSelectionne == null) return;

            var prerequisIds = _metierService.GetPrerequisForMetier(_metierSelectionne.MetierId);
            foreach (var prerequisId in prerequisIds)
            {
                var prerequisMetier = _metierService.GetMetierById(prerequisId);
                var nom = prerequisMetier?.Nom ?? "(Métier non trouvé)";
                var item = new ListViewItem(new[] { prerequisId, nom }) { Tag = prerequisId };
                listViewPrerequis.Items.Add(item);
            }
        }

        private void RafraichirUtilisation()
        {
            // Cette logique devra être complétée quand les autres services seront accessibles
            lblUtilisation.Text = "Info d'utilisation non disponible.";
        }

        #endregion

        #region Événements interface

        private void listViewMetiers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewMetiers.SelectedItems.Count > 0)
            {
                var metier = listViewMetiers.SelectedItems[0].Tag as MetierRecord;
                AfficherDetailsMetier(metier);
                btnModifier.Enabled = true;
                btnSupprimer.Enabled = true;
            }
            else
            {
                NettoyerDetails();
                btnModifier.Enabled = false;
                btnSupprimer.Enabled = false;
            }
        }

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            RafraichirListeMetiers();
            RafraichirStatut();
        }

        #endregion

        #region Actions CRUD

        private void btnNouveau_Click(object sender, EventArgs e)
        {
            // On passe en "mode édition" pour un nouvel objet
            _metierSelectionne = new MetierRecord { MetierId = "NOUVEAU_METIER", Nom = "Nouveau métier" };

            AfficherDetailsMetier(_metierSelectionne);

            txtMetierId.ReadOnly = false;
            txtNom.ReadOnly = false;

            groupBoxDetails.Text = "Nouveau Métier";
            txtMetierId.Focus();
            txtMetierId.SelectAll();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (_metierSelectionne == null) return;

            if (txtMetierId.ReadOnly) // Si on n'est pas déjà en mode édition
            {
                // Activer le mode édition
                txtNom.ReadOnly = false;
                groupBoxDetails.Text = $"Modification de {_metierSelectionne.MetierId}";
                txtNom.Focus();
                txtNom.SelectAll();
            }
            else // On est déjà en mode édition, on sauvegarde
            {
                try
                {
                    if (IsEditing) // C'est un nouveau métier
                    {
                        _metierService.AjouterMetier(new MetierRecord { MetierId = txtMetierId.Text, Nom = txtNom.Text, PrerequisMetierIds = "" });
                    }
                    else // C'est une modification
                    {
                        _metierService.ModifierMetier(_metierSelectionne.MetierId, txtNom.Text, _metierSelectionne.PrerequisMetierIds);
                    }

                    txtMetierId.ReadOnly = true;
                    txtNom.ReadOnly = true;
                    groupBoxDetails.Text = "Détails du Métier";

                    RafraichirAffichageComplet();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_metierSelectionne == null) return;

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le métier '{_metierSelectionne.Nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _metierService.SupprimerMetier(_metierSelectionne.MetierId);
                    _metierSelectionne = null;
                    RafraichirAffichageComplet();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Gestion des prérequis

        private void btnAjouterPrerequis_Click(object sender, EventArgs e)
        {
            if (_metierSelectionne == null || txtMetierId.ReadOnly == false)
            {
                MessageBox.Show("Veuillez d'abord sauvegarder le métier avant d'ajouter des prérequis.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var prerequisActuels = _metierService.GetPrerequisForMetier(_metierSelectionne.MetierId);
            var metiersDisponibles = _metierService.GetAllMetiers()
                .Where(m => m.MetierId != _metierSelectionne.MetierId && !prerequisActuels.Contains(m.MetierId))
                .ToList();

            if (!metiersDisponibles.Any())
            {
                MessageBox.Show("Aucun autre métier disponible à ajouter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SelectionMetierDialog(metiersDisponibles);
            if (dialog.ShowDialog() == DialogResult.OK && dialog.MetierSelectionne != null)
            {
                var nouveauxPrerequis = prerequisActuels.ToList();
                nouveauxPrerequis.Add(dialog.MetierSelectionne.MetierId);
                _metierService.ModifierMetier(_metierSelectionne.MetierId, _metierSelectionne.Nom, string.Join(",", nouveauxPrerequis));
                RafraichirListePrerequis();
                RafraichirListeMetiers();
            }
        }

        private void btnSupprimerPrerequis_Click(object sender, EventArgs e)
        {
            if (_metierSelectionne == null || listViewPrerequis.SelectedItems.Count == 0) return;

            var prerequisIdASupprimer = listViewPrerequis.SelectedItems[0].Tag as string;
            var prerequisActuels = _metierService.GetPrerequisForMetier(_metierSelectionne.MetierId).ToList();
            if (prerequisActuels.Remove(prerequisIdASupprimer))
            {
                _metierService.ModifierMetier(_metierSelectionne.MetierId, _metierSelectionne.Nom, string.Join(",", prerequisActuels));
                RafraichirListePrerequis();
                RafraichirListeMetiers();
            }
        }

        #endregion

        #region Import/Export

        private void btnImporter_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv", Title = "Importer les métiers" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var metiersImportes = _csvDataService.ImportCsv<MetierRecord>(ofd.FileName);
                    _metierService.RemplacerTousLesMetiers(metiersImportes);
                    RafraichirAffichageComplet();
                    MessageBox.Show($"{metiersImportes.Count} métiers ont été importés et ont remplacé les données existantes.", "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'import : {ex.Message}", "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            var metiersAExporter = _metierService.GetAllMetiers();
            if (!metiersAExporter.Any())
            {
                MessageBox.Show("Aucun métier à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv", Title = "Exporter les métiers", FileName = $"metiers_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _csvDataService.ExportCsv(metiersAExporter, sfd.FileName);
                    MessageBox.Show($"{metiersAExporter.Count} métiers exportés avec succès.", "Export réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'export : {ex.Message}", "Erreur d'export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Supprimé : txtMetierId_TextChanged et txtNom_TextChanged car la sauvegarde est maintenant explicite
    }
}