using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class BlocForm : System.Windows.Forms.Form
    {
        private readonly BlocService _blocService;
        private readonly TacheService _tacheService; // Ajout pour la validation de suppression
        private Bloc _blocSelectionne = null;
        private bool _isEditing = false;
        private readonly ToolTip _toolTip = new ToolTip();

        public BlocForm(BlocService blocService, TacheService tacheService)
        {
            InitializeComponent();
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService)); // Ajout
        }

        private void BlocForm_Load(object sender, EventArgs e)
        {
            SetEditingMode(false);
            RafraichirAffichageComplet();
        }

        /// <summary>
        /// Gère l'état de l'interface (lecture seule ou édition) en activant/désactivant les contrôles.
        /// C'est la méthode centrale qui pilote l'ergonomie du formulaire.
        /// </summary>
        private void SetEditingMode(bool isEditing)
        {
            _isEditing = isEditing;

            listViewBlocs.Enabled = !isEditing;
            groupBoxDetails.Enabled = isEditing;

            if (isEditing)
            {
                txtNom.ReadOnly = false;
                numCapaciteMax.ReadOnly = false;
                txtBlocId.ReadOnly = (_blocSelectionne != null && !string.IsNullOrEmpty(_blocSelectionne.BlocId));
            }
            else
            {
                txtBlocId.ReadOnly = true;
                txtNom.ReadOnly = true;
                numCapaciteMax.ReadOnly = true;
            }

            // Gestion des boutons d'action principaux
            btnNouveau.Visible = !isEditing;
            btnSupprimer.Visible = !isEditing;
            btnModifier.Text = isEditing ? "💾 Sauvegarder" : "✏️ Modifier";
            btnModifier.Enabled = isEditing || _blocSelectionne != null;
            btnAnnuler.Visible = isEditing;

            // Logique de suppression sécurisée
            bool blocPeutEtreSupprime = _blocSelectionne != null && !IsBlocInUse(_blocSelectionne.BlocId);
            btnSupprimer.Enabled = blocPeutEtreSupprime;
            if (blocPeutEtreSupprime)
            {
                _toolTip.SetToolTip(btnSupprimer, "Supprimer le bloc sélectionné.");
            }
            else
            {
                _toolTip.SetToolTip(btnSupprimer, "Ce bloc ne peut pas être supprimé car il est utilisé par au moins une tâche.");
            }
        }

        private bool IsBlocInUse(string blocId)
        {
            if (string.IsNullOrEmpty(blocId)) return false;
            return _tacheService.ObtenirTachesParBloc(blocId).Any();
        }

        #region Gestion des données et affichage

        private void RafraichirAffichageComplet()
        {
            var idBlocSelectionne = _blocSelectionne?.BlocId;
            RafraichirListeBlocs();

            if (idBlocSelectionne != null)
            {
                var itemToReselect = listViewBlocs.Items.Cast<ListViewItem>()
                    .FirstOrDefault(item => (item.Tag as Bloc)?.BlocId == idBlocSelectionne);
                if (itemToReselect != null)
                {
                    itemToReselect.Selected = true;
                }
            }

            if (listViewBlocs.SelectedItems.Count == 0)
            {
                _blocSelectionne = null;
                NettoyerDetails();
            }
            RafraichirStatut();
            // S'assurer que l'état des boutons est correct après un rafraîchissement
            SetEditingMode(_isEditing);
        }

        private void RafraichirListeBlocs()
        {
            listViewBlocs.Items.Clear();
            var blocs = _blocService.ObtenirTousLesBlocs();
            foreach (var bloc in blocs.OrderBy(b => b.Nom))
            {
                var item = new ListViewItem(new[] { bloc.BlocId, bloc.Nom, bloc.CapaciteMaxOuvriers.ToString() }) { Tag = bloc };
                listViewBlocs.Items.Add(item);
            }
        }

        private void RafraichirStatut()
        {
            lblStatut.Text = $"{listViewBlocs.Items.Count} bloc(s)";
        }

        private void AfficherDetailsBloc(Bloc bloc)
        {
            _blocSelectionne = bloc;
            if (bloc == null)
            {
                NettoyerDetails();
                return;
            }
            txtBlocId.Text = bloc.BlocId;
            txtNom.Text = bloc.Nom;
            numCapaciteMax.Value = bloc.CapaciteMaxOuvriers;
            groupBoxDetails.Text = $"Détails: {bloc.Nom}";
        }

        private void NettoyerDetails()
        {
            txtBlocId.Clear();
            txtNom.Clear();
            numCapaciteMax.Value = 0;
            groupBoxDetails.Text = "Détails du Bloc";
        }

        #endregion

        #region Événements interface

        private void listViewBlocs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isEditing) return;

            if (listViewBlocs.SelectedItems.Count > 0)
            {
                _blocSelectionne = listViewBlocs.SelectedItems[0].Tag as Bloc;
                AfficherDetailsBloc(_blocSelectionne);
            }
            else
            {
                _blocSelectionne = null;
                NettoyerDetails();
            }
            SetEditingMode(false);
        }

        private void btnFermer_Click(object sender, EventArgs e) => this.Close();

        #endregion

        #region Actions CRUD

        private void btnNouveau_Click(object sender, EventArgs e)
        {
            listViewBlocs.SelectedItems.Clear();
            _blocSelectionne = new Bloc { BlocId = "", Nom = "Nouveau bloc" };
            AfficherDetailsBloc(_blocSelectionne);
            SetEditingMode(true);
            txtBlocId.Focus();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (!_isEditing)
            {
                if (_blocSelectionne == null) return;
                SetEditingMode(true);
                txtNom.Focus();
                txtNom.SelectAll();
                return;
            }

            try
            {
                bool isNew = string.IsNullOrEmpty(_blocSelectionne.BlocId);
                var blocToSave = new Bloc
                {
                    BlocId = isNew ? txtBlocId.Text : _blocSelectionne.BlocId,
                    Nom = txtNom.Text,
                    CapaciteMaxOuvriers = (int)numCapaciteMax.Value
                };

                if (isNew) _blocService.AjouterBloc(blocToSave);
                else _blocService.ModifierBloc(blocToSave);

                _blocSelectionne = blocToSave;
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
            // Re-sélectionner l'item qui était actif avant l'annulation
            if (_blocSelectionne != null)
            {
                AfficherDetailsBloc(_blocSelectionne);
            }
            else
            {
                NettoyerDetails();
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_blocSelectionne == null) return;
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le bloc '{_blocSelectionne.Nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _blocService.SupprimerBloc(_blocSelectionne.BlocId);
                    _blocSelectionne = null;
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