using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class LotForm : System.Windows.Forms.Form
    {
        private readonly LotService _lotService;
        private readonly TacheService _tacheService;
        private Lot _lotSelectionne = null;
        private bool _isEditing = false;
        private readonly ToolTip _toolTip = new ToolTip();

        public LotForm(LotService lotService, TacheService tacheService)
        {
            InitializeComponent();
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
        }

        private void LotForm_Load(object sender, EventArgs e)
        {
            SetEditingMode(false);
            RafraichirAffichageComplet();
        }

        /// <summary>
        /// Gère l'état de l'interface (lecture seule ou édition) en activant/désactivant les contrôles.
        /// </summary>
        private void SetEditingMode(bool isEditing)
        {
            _isEditing = isEditing;

            listViewLots.Enabled = !isEditing;
            groupBoxDetails.Enabled = isEditing;

            if (isEditing)
            {
                txtNom.ReadOnly = false;
                numPriorite.ReadOnly = false;
                btnParcourirPlan.Enabled = true;
                txtLotId.ReadOnly = (_lotSelectionne != null && !string.IsNullOrEmpty(_lotSelectionne.LotId));
            }
            else
            {
                txtLotId.ReadOnly = true;
                txtNom.ReadOnly = true;
                numPriorite.ReadOnly = true;
                btnParcourirPlan.Enabled = false;
            }

            btnNouveau.Visible = !isEditing;
            btnSupprimer.Visible = !isEditing;
            btnModifier.Text = isEditing ? "💾 Sauvegarder" : "✏️ Modifier";
            btnModifier.Enabled = isEditing || _lotSelectionne != null;
            btnAnnuler.Visible = isEditing;

            bool lotPeutEtreSupprime = _lotSelectionne != null && !IsLotInUse(_lotSelectionne.LotId);
            btnSupprimer.Enabled = lotPeutEtreSupprime;
            if (lotPeutEtreSupprime)
            {
                _toolTip.SetToolTip(btnSupprimer, "Supprimer le lot sélectionné.");
            }
            else
            {
                _toolTip.SetToolTip(btnSupprimer, "Ce lot ne peut pas être supprimé car il est utilisé par au moins une tâche.");
            }
        }

        private bool IsLotInUse(string lotId)
        {
            if (string.IsNullOrEmpty(lotId)) return false;
            return _tacheService.ObtenirTachesParLot(lotId).Any();
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
            SetEditingMode(_isEditing);
        }

        private void RafraichirListeLots()
        {
            listViewLots.Items.Clear();
            var lots = _lotService.ObtenirTousLesLots();
            foreach (var lot in lots.OrderBy(l => l.Priorite).ThenBy(l => l.Nom))
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
            using var ofd = new OpenFileDialog
            {
                Title = "Sélectionner le fichier de plan",
                Filter = "Fichiers PDF (*.pdf)|*.pdf|Tous les fichiers (*.*)|*.*"
            };
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
            _lotSelectionne = new Lot { LotId = "", Nom = "Nouveau lot", Priorite = 99 };
            AfficherDetailsLot(_lotSelectionne);
            SetEditingMode(true);
            txtLotId.Focus();
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
                bool isNew = string.IsNullOrEmpty(_lotSelectionne.LotId);
                var lotToSave = new Lot
                {
                    LotId = isNew ? txtLotId.Text : _lotSelectionne.LotId,
                    Nom = txtNom.Text,
                    Priorite = (int)numPriorite.Value,
                    CheminFichierPlan = txtCheminFichierPlan.Text
                };

                if (isNew) _lotService.AjouterLot(lotToSave);
                else _lotService.ModifierLot(lotToSave);

                _lotSelectionne = lotToSave;
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
            if (_lotSelectionne != null) AfficherDetailsLot(_lotSelectionne);
            else NettoyerDetails();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_lotSelectionne == null) return;
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le lot '{_lotSelectionne.Nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _lotService.SupprimerLot(_lotSelectionne.LotId);
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