using PlanAthena.Data;
using PlanAthena.Utilities;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;

namespace PlanAthena.Forms
{
    public partial class LotForm : System.Windows.Forms.Form
    {
        private readonly ProjetService _projetService;
        private readonly TacheService _tacheService;

        private Lot _lotSelectionne = null;
        private bool _isEditing = false;
        private readonly ToolTip _toolTip = new ToolTip();

        public LotForm(ProjetService projetService, TacheService tacheService)
        {
            InitializeComponent();
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
        }

        private void LotForm_Load(object sender, EventArgs e)
        {
            InitialiserComboBoxPhases();
            SetEditingMode(false);
            RafraichirAffichageComplet();
        }
        private void InitialiserComboBoxPhases()
        {
            // On récupère toutes les valeurs de l'énumération ChantierPhase,
            // on les convertit en une liste, et on exclut la valeur "None".
            var phasesValides = Enum.GetValues(typeof(ChantierPhase))
                                    .Cast<ChantierPhase>()
                                    .Where(p => p != ChantierPhase.None)
                                    .ToList();

            cmbPhases.DataSource = phasesValides;
            cmbPhases.SelectedItem = null; // Aucune sélection au départ
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
                cmbPhases.Enabled = true;
                txtLotId.ReadOnly = (_lotSelectionne != null && !string.IsNullOrEmpty(_lotSelectionne.LotId));
            }
            else
            {
                txtLotId.ReadOnly = true;
                txtNom.ReadOnly = true;
                numPriorite.ReadOnly = true;
                cmbPhases.Enabled = false;
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
            var lots = _projetService.ObtenirTousLesLots();
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

            // --- CORRECTION DU BUG D'AFFICHAGE DE LA PHASE ---

            // 1. On récupère la liste des options valides du ComboBox.
            var optionsValides = cmbPhases.DataSource as List<ChantierPhase>;

            // 2. On vérifie si la phase du lot est une des options uniques disponibles.
            //    Si lot.Phases est une combinaison de flags (ex: GrosOeuvre | SecondOeuvre),
            //    cette condition sera fausse.
            if (optionsValides != null && optionsValides.Contains(lot.Phases))
            {
                // La phase du lot est une valeur simple, on peut la sélectionner sans risque.
                cmbPhases.SelectedItem = lot.Phases;
            }
            else
            {
                // La phase du lot est soit `None`, soit une combinaison de plusieurs phases.
                // Dans ce cas, on ne peut pas l'afficher dans le ComboBox, donc on ne sélectionne rien.
                cmbPhases.SelectedItem = null;
                // Optionnel : vous pourriez afficher un message à l'utilisateur ici,
                // par exemple dans un Label : "Phases multiples assignées".
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
            using var ofd = new OpenFileDialog
            {
                Title = "Sélectionner le fichier de plan",
                Filter = "Fichiers de plan|*.pdf;*.jpg;*.jpeg;*.png;*.gif;*.bmp|Fichiers PDF (*.pdf)|*.pdf|Fichiers Image (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Tous les fichiers (*.*)|*.*"
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

            // 🔧 LOGIQUE CORRIGÉE : Le formulaire DEMANDE un nouveau lot au service.
            // Il ne sait pas comment il est créé.
            _lotSelectionne = _projetService.CreerNouveauLotBrouillon();

            AfficherDetailsLot(_lotSelectionne);
            SetEditingMode(true);

            // 🔧 L'ID est déjà généré, donc on met le focus sur le Nom pour l'édition.
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
                ChantierPhase phaseSelectionnee = ChantierPhase.None;
                if (cmbPhases.SelectedItem is ChantierPhase phase)
                {
                    phaseSelectionnee = phase;
                }

                if (phaseSelectionnee == ChantierPhase.None)
                {
                    MessageBox.Show("Veuillez sélectionner une phase pour le lot.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNom.Text))
                {
                    MessageBox.Show("Le nom du lot ne peut pas être vide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 🔧 LOGIQUE CORRIGÉE : On vérifie si le lot existe déjà dans le service pour savoir si c'est un ajout ou une modification.
                bool isNew = !_projetService.ObtenirTousLesLots().Any(l => l.LotId == _lotSelectionne.LotId);

                if (isNew)
                {
                    // On met à jour l'objet brouillon avec les valeurs de l'UI et on l'ajoute.
                    _lotSelectionne.Nom = txtNom.Text;
                    _lotSelectionne.Priorite = (int)numPriorite.Value;
                    _lotSelectionne.CheminFichierPlan = txtCheminFichierPlan.Text;
                    _lotSelectionne.Phases = phaseSelectionnee;
                    _projetService.AjouterLot(_lotSelectionne);
                }
                else
                {
                    // C'est une modification, on appelle la méthode de modification.
                    _projetService.ModifierLot(
                        _lotSelectionne.LotId,
                        txtNom.Text,
                        (int)numPriorite.Value,
                        txtCheminFichierPlan.Text,
                        phaseSelectionnee
                    );
                }

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