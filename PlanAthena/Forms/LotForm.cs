// Fichier: PlanAthena/Forms/LotForm.cs
// Version: 0.4.4
// Description: Corrigé pour utiliser le ProjetService "stateful" refondu.

using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class LotForm : Form
    {
        private readonly ProjetService _projetService;
        private Lot _lotSelectionne = null;
        private bool _isEditing = false;
        private readonly ToolTip _toolTip = new ToolTip();

        // CORRIGÉ : Le constructeur est simplifié.
        public LotForm(ProjetService projetService)
        {
            InitializeComponent();
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
        }

        private void LotForm_Load(object sender, EventArgs e)
        {
            InitialiserComboBoxPhases();
            SetEditingMode(false);
            RafraichirAffichageComplet();
        }

        private void InitialiserComboBoxPhases()
        {
            var phasesValides = Enum.GetValues(typeof(ChantierPhase))
                                    .Cast<ChantierPhase>()
                                    .Where(p => p != ChantierPhase.None)
                                    .ToList();
            cmbPhases.DataSource = phasesValides;
            cmbPhases.SelectedItem = null;
        }

        private void SetEditingMode(bool isEditing)
        {
            _isEditing = isEditing;
            listViewLots.Enabled = !isEditing;
            groupBoxDetails.Enabled = isEditing;

            txtNom.ReadOnly = !isEditing;
            numPriorite.ReadOnly = !isEditing;
            btnParcourirPlan.Enabled = isEditing;
            cmbPhases.Enabled = isEditing;
            txtLotId.ReadOnly = true;

            btnNouveau.Visible = !isEditing;
            btnSupprimer.Visible = !isEditing;
            btnModifier.Text = isEditing ? "💾 Sauvegarder" : "✏️ Modifier";
            btnModifier.Enabled = isEditing || _lotSelectionne != null;
            btnAnnuler.Visible = isEditing;

            bool lotPeutEtreSupprime = _lotSelectionne != null && !IsLotInUse(_lotSelectionne.LotId);
            btnSupprimer.Enabled = lotPeutEtreSupprime;
            _toolTip.SetToolTip(btnSupprimer, lotPeutEtreSupprime
                ? "Supprimer le lot sélectionné."
                : "Ce lot ne peut pas être supprimé car il est utilisé par au moins une tâche.");
        }

        // CORRIGÉ : L'appel à _tacheService est remplacé par _projetService
        private bool IsLotInUse(string lotId)
        {
            if (string.IsNullOrEmpty(lotId)) return false;
            return _projetService.ObtenirTachesParLot(lotId).Any();
        }

        #region Gestion des données et affichage

        private void RafraichirAffichageComplet()
        {
            var idLotSelectionne = _lotSelectionne?.LotId;
            RafraichirListeLots();

            if (idLotSelectionne != null)
            {
                var itemToReselect = listViewLots.Items.Cast<ListViewItem>()
                    .FirstOrDefault(item => (item.Tag as Lot)?.LotId == idLotSelectionne);
                if (itemToReselect != null)
                {
                    itemToReselect.Selected = true;
                }
            }
            else
            {
                _lotSelectionne = null;
                NettoyerDetails();
            }
            RafraichirStatut();
            SetEditingMode(_isEditing); // Appelé après avoir potentiellement re-sélectionné
        }

        private void RafraichirListeLots()
        {
            listViewLots.Items.Clear();
            var lots = _projetService.ObtenirTousLesLots();
            foreach (var lot in lots)
            {
                var item = new ListViewItem(new[] { lot.LotId, lot.Nom, lot.Priorite.ToString() }) { Tag = lot };
                listViewLots.Items.Add(item);
            }
        }

        private void RafraichirStatut()
        {
            lblStatut.Text = $"{listViewLots.Items.Count} lot(s)";
        }

        private void AfficherDetailsLot(Lot lot)
        {
            _lotSelectionne = lot;
            if (lot == null)
            {
                NettoyerDetails();
                return;
            }
            txtLotId.Text = lot.LotId;
            txtNom.Text = lot.Nom;
            numPriorite.Value = lot.Priorite > 0 ? lot.Priorite : 1;
            txtCheminFichierPlan.Text = lot.CheminFichierPlan;
            groupBoxDetails.Text = $"Détails: {lot.Nom}";

            var optionsValides = cmbPhases.DataSource as List<ChantierPhase>;
            if (optionsValides != null && optionsValides.Contains(lot.Phases))
            {
                cmbPhases.SelectedItem = lot.Phases;
            }
            else
            {
                cmbPhases.SelectedItem = null;
            }
        }

        private void NettoyerDetails()
        {
            txtLotId.Clear();
            txtNom.Clear();
            numPriorite.Value = 1;
            txtCheminFichierPlan.Clear();
            groupBoxDetails.Text = "Détails du Lot";
        }

        #endregion

        #region Événements interface

        private void listViewLots_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isEditing) return;
            if (listViewLots.SelectedItems.Count > 0)
            {
                _lotSelectionne = listViewLots.SelectedItems[0].Tag as Lot;
                AfficherDetailsLot(_lotSelectionne);
            }
            else
            {
                _lotSelectionne = null;
                NettoyerDetails();
            }
            SetEditingMode(false);
        }

        private void btnFermer_Click(object sender, EventArgs e) => this.Close();

        private void btnParcourirPlan_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { /* ... */ };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtCheminFichierPlan.Text = ofd.FileName;
            }
        }

        #endregion

        #region Actions CRUD

        private void btnNouveau_Click(object sender, EventArgs e)
        {
            listViewLots.SelectedItems.Clear();
            // CORRIGÉ : Utilise maintenant la méthode Creer de ProjetService
            _lotSelectionne = _projetService.CreerLot();
            AfficherDetailsLot(_lotSelectionne);
            SetEditingMode(true);
            txtNom.Focus();
            txtNom.SelectAll();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (!_isEditing)
            {
                if (_lotSelectionne == null) return;
                SetEditingMode(true);
                txtNom.Focus();
                txtNom.SelectAll();
                return;
            }

            try
            {
                if (cmbPhases.SelectedItem is not ChantierPhase phaseSelectionnee)
                {
                    MessageBox.Show("Veuillez sélectionner une phase pour le lot.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtNom.Text))
                {
                    MessageBox.Show("Le nom du lot ne peut pas être vide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _lotSelectionne.Nom = txtNom.Text;
                _lotSelectionne.Priorite = (int)numPriorite.Value;
                _lotSelectionne.CheminFichierPlan = txtCheminFichierPlan.Text;
                _lotSelectionne.Phases = phaseSelectionnee;

                _projetService.ModifierLot(_lotSelectionne);

                SetEditingMode(false);
                RafraichirAffichageComplet();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur de sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            SetEditingMode(false);
            if (listViewLots.SelectedItems.Count > 0)
            {
                _lotSelectionne = listViewLots.SelectedItems[0].Tag as Lot;
                AfficherDetailsLot(_lotSelectionne);
            }
            else
            {
                _lotSelectionne = null;
                NettoyerDetails();
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_lotSelectionne == null) return;
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le lot '{_lotSelectionne.Nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _projetService.SupprimerLot(_lotSelectionne.LotId);
                    _lotSelectionne = null;
                    NettoyerDetails();
                    SetEditingMode(false);
                    RafraichirAffichageComplet();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }
}