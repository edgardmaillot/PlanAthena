using PlanAthena.Data;
using PlanAthena.Services.Business;

namespace PlanAthena.Forms
{
    public partial class BlocForm : System.Windows.Forms.Form
    {
        private readonly BlocService _blocService;
        private readonly TacheService _tacheService;
        private Bloc _blocSelectionne = null;
        private readonly string _blocIdToEditOnInit;
        private readonly Lot _lotActifPourCreation;
        private readonly ToolTip _toolTip = new ToolTip();

        /// <summary>
        /// Constructeur pour l'édition d'un bloc existant ou la création d'un nouveau bloc.
        /// </summary>
        /// <param name="blocService">Service de gestion des blocs</param>
        /// <param name="tacheService">Service de gestion des tâches</param>
        /// <param name="blocIdToEdit">ID du bloc à éditer (null pour création)</param>
        /// <param name="lotActifPourCreation">Lot actif pour la création d'un nouveau bloc (null pour édition)</param>
        public BlocForm(BlocService blocService, TacheService tacheService, string blocIdToEdit = null, Lot lotActifPourCreation = null)
        {
            InitializeComponent();
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _blocIdToEditOnInit = blocIdToEdit;
            _lotActifPourCreation = lotActifPourCreation;
        }

        private void BlocForm_Load(object sender, EventArgs e)
        {
            SetEditingMode(true);

            if (!string.IsNullOrEmpty(_blocIdToEditOnInit))
            {
                var blocExistant = _blocService.ObtenirBlocParId(_blocIdToEditOnInit);
                if (blocExistant != null)
                {
                    _blocSelectionne = blocExistant;
                    AfficherDetailsBloc(_blocSelectionne);
                }
            }
            else if (_lotActifPourCreation != null)
            {
                var nouveauBlocId = _blocService.GenerateNewBlocId(_lotActifPourCreation.LotId);
                _blocSelectionne = new Bloc
                {
                    BlocId = nouveauBlocId,
                    Nom = "Nouveau bloc",
                    CapaciteMaxOuvriers = 1
                };
                AfficherDetailsBloc(_blocSelectionne);
                txtNom.Focus();
                txtNom.SelectAll();
            }

            ConfigurerChampsEmplacement();
        }

        /// <summary>
        /// Configure les champs d'emplacement avec leur état désactivé et tooltip.
        /// </summary>
        private void ConfigurerChampsEmplacement()
        {
            txtEmplacementX.ReadOnly = true;
            txtEmplacementX.Enabled = false;
            txtEmplacementY.ReadOnly = true;
            txtEmplacementY.Enabled = false;

            _toolTip.SetToolTip(txtEmplacementX, "Fonctionnalité à venir");
            _toolTip.SetToolTip(txtEmplacementY, "Fonctionnalité à venir");
        }

        /// <summary>
        /// Gère l'état de l'interface en mode édition.
        /// Active/désactive les champs de saisie et boutons selon le contexte.
        /// </summary>
        /// <param name="isEditing">Mode édition activé</param>
        private void SetEditingMode(bool isEditing)
        {
            txtNom.ReadOnly = !isEditing;
            numCapaciteMax.ReadOnly = !isEditing;
            txtBlocId.ReadOnly = true;

            txtEmplacementX.ReadOnly = true;
            txtEmplacementX.Enabled = false;
            txtEmplacementY.ReadOnly = true;
            txtEmplacementY.Enabled = false;

            btnSauvegarder.Enabled = isEditing;
            btnAnnulerEdition.Enabled = isEditing;

            bool peutSupprimer = _blocSelectionne != null && !string.IsNullOrEmpty(_blocSelectionne.BlocId) && !IsBlocUtilise(_blocSelectionne.BlocId);
            btnSupprimer.Enabled = peutSupprimer;

            if (peutSupprimer)
            {
                _toolTip.SetToolTip(btnSupprimer, "Supprimer ce bloc");
            }
            else if (_blocSelectionne != null && IsBlocUtilise(_blocSelectionne.BlocId))
            {
                _toolTip.SetToolTip(btnSupprimer, "Impossible de supprimer : le bloc est utilisé par des tâches");
            }
            else
            {
                _toolTip.SetToolTip(btnSupprimer, "Aucun bloc sélectionné");
            }
        }

        /// <summary>
        /// Vérifie si un bloc est utilisé par des tâches.
        /// </summary>
        /// <param name="blocId">ID du bloc à vérifier</param>
        /// <returns>True si le bloc est utilisé</returns>
        private bool IsBlocUtilise(string blocId)
        {
            if (string.IsNullOrEmpty(blocId)) return false;
            return _tacheService.ObtenirTachesParBloc(blocId).Any();
        }

        /// <summary>
        /// Affiche les détails d'un bloc dans les champs du formulaire.
        /// </summary>
        /// <param name="bloc">Bloc à afficher</param>
        private void AfficherDetailsBloc(Bloc bloc)
        {
            if (bloc == null)
            {
                NettoyerDetails();
                return;
            }

            txtBlocId.Text = bloc.BlocId;
            txtNom.Text = bloc.Nom;
            numCapaciteMax.Value = bloc.CapaciteMaxOuvriers;
            groupBoxDetails.Text = $"Détails: {bloc.Nom}";

            txtEmplacementX.Text = "";
            txtEmplacementY.Text = "";
        }

        /// <summary>
        /// Nettoie tous les champs de détails.
        /// </summary>
        private void NettoyerDetails()
        {
            txtBlocId.Clear();
            txtNom.Clear();
            numCapaciteMax.Value = 1;
            txtEmplacementX.Clear();
            txtEmplacementY.Clear();
            groupBoxDetails.Text = "Détails du Bloc";
        }

        /// <summary>
        /// Gestionnaire pour le bouton Sauvegarder.
        /// Sauvegarde le bloc avec les données saisies.
        /// </summary>
        private void btnSauvegarder_Click(object sender, EventArgs e)
        {
            try
            {
                if (_blocSelectionne == null) return;

                _blocSelectionne.Nom = txtNom.Text;
                _blocSelectionne.CapaciteMaxOuvriers = (int)numCapaciteMax.Value;

                _blocService.SaveBloc(_blocSelectionne);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur de sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestionnaire pour le bouton Annuler l'édition.
        /// Ferme le formulaire sans sauvegarder.
        /// </summary>
        private void btnAnnulerEdition_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Gestionnaire pour le bouton Supprimer.
        /// Supprime le bloc après confirmation.
        /// </summary>
        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_blocSelectionne == null) return;

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer le bloc '{_blocSelectionne.Nom}' ?",
                "Confirmation de suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _blocService.SupprimerBloc(_blocSelectionne.BlocId);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Gestionnaire pour le bouton Fermer.
        /// Ferme le formulaire (inchangé).
        /// </summary>
        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}