using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using PlanAthena.Controls; // Contient MetierDiagramControl
using PlanAthena.Controls.Config; // Contient MetierDiagramSettings, MetierNodeBuilder
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class MetierForm : System.Windows.Forms.Form
    {
        private readonly ProjetService _projetService;
        private readonly IdGeneratorService _idGeneratorService;
        private readonly DependanceBuilder _dependanceBuilder;
        // Les services LotService et BlocService ne sont plus nécessaires directement ici
        // car MetierDiagramControl ne les utilise pas pour les métiers.
        // Ils pourraient être nécessaires si MetierForm gérait des aspects liés aux tâches/blocs,
        // mais pour la gestion pure des métiers, ils ne le sont pas.

        private Metier _metierEnEdition = null;
        private bool _hasUnsavedChanges = false;

        private readonly MetierDiagramControl _metierDiagramControl; // Changement ici: nouveau type de contrôle

        private enum MetierFormState
        {
            Initial,
            Editing,
        }

        // Constructeur mis à jour pour accepter les nouvelles dépendances
        // Les dépendances LotService et BlocService ne sont plus nécessaires dans MetierForm
        // si elles ne sont utilisées que par PertDiagramControl (maintenant remplacé).
        public MetierForm(
            ProjetService projetService,
            IdGeneratorService idGeneratorService,
            DependanceBuilder dependanceBuilder) // Changement ici: suppression de LotService et BlocService
        {
            InitializeComponent();

            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _idGeneratorService = idGeneratorService ?? throw new ArgumentNullException(nameof(idGeneratorService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
            // _lotService et _blocService sont supprimés du constructeur

            _metierDiagramControl = new MetierDiagramControl(); // Changement ici: nouvelle instance
            _metierDiagramControl.Dock = DockStyle.Fill;
            // Changement ici: initialize le nouveau contrôle avec ses propres settings
            _metierDiagramControl.Initialize(_projetService, _dependanceBuilder, new MetierDiagramSettings());
            // Changement ici: abonnement à l'événement spécifique aux métiers
            _metierDiagramControl.MetierSelected += MetierDiagramControl_MetierSelected;
            panelLeft.Controls.Add(_metierDiagramControl); // Ajouter le contrôle au panel gauche

            // Abonnement aux événements de modification pour _hasUnsavedChanges
            txtNom.TextChanged += OnDetailChanged;
            txtPictogram.TextChanged += OnDetailChanged;
            panelCouleurApercu.Click += OnDetailChanged;
            // chkListPrerequis.ItemCheck += chkListPrerequis_ItemCheck; // Géré par PopulatePrerequisCheckList pour synchronisation
        }

        private void MetierForm_Load(object sender, EventArgs e)
        {
            ChargerDiagrammeMetiers();
            SetUIState(MetierFormState.Initial);
        }

        private void SetUIState(MetierFormState state)
        {
            switch (state)
            {
                case MetierFormState.Initial:
                    groupBoxDetails.Enabled = false;
                    NettoyerDetails();
                    btnNouveau.Enabled = true;
                    btnSupprimer.Enabled = false;
                    btnSauvegarder.Visible = false;
                    btnAnnuler.Visible = false;
                    _metierDiagramControl.Enabled = true; // Permettre l'interaction avec le diagramme
                    break;
                case MetierFormState.Editing:
                    groupBoxDetails.Enabled = true;
                    btnNouveau.Enabled = false;
                    // btnSupprimer.Enabled basé sur si le métier est déjà existant (non-nouveau)
                    btnSupprimer.Enabled = (_metierEnEdition != null && _projetService.GetMetierById(_metierEnEdition.MetierId) != null);
                    btnSauvegarder.Visible = true;
                    btnAnnuler.Visible = true;
                    _metierDiagramControl.Enabled = true; // Permettre de cliquer sur d'autres nœuds en mode édition
                    break;
            }
        }

        private void ChargerDiagrammeMetiers()
        {
            var metiers = _projetService.GetAllMetiers();
            // Changement ici: Passer directement la liste des objets Metier au nouveau contrôle
            _metierDiagramControl.ChargerDonnees(metiers.ToList());
            _metierDiagramControl.ZoomToutAjuster();
        }

        // Changement ici: Nouveau nom du gestionnaire d'événements et type d'argument
        private void MetierDiagramControl_MetierSelected(object sender, MetierSelectedEventArgs e)
        {
            // Vérifier si le métier sélectionné correspond déjà au métier en édition
            if (e.SelectedMetier == null || e.SelectedMetier.MetierId == _metierEnEdition?.MetierId)
            {
                // Si la même tâche est cliquée à nouveau, ne rien faire si on est déjà en mode édition.
                // Juste rafraîchir la sélection visuelle si nécessaire.
                if (_metierEnEdition != null)
                {
                    _metierDiagramControl.ForceSelection(e.SelectedMetier?.MetierId); // Re-sélectionner visuellement l'élément
                    return;
                }
                // Si aucun métier n'est en édition et que la sélection est null, on ne fait rien
                if (e.SelectedMetier == null) return;
            }

            // Demander confirmation si des modifications non sauvegardées existent sur le métier PRÉCÉDENT
            

            // Récupérer le métier réel (qui est déjà dans e.SelectedMetier pour MetierDiagramControl)
            _metierEnEdition = e.SelectedMetier;

            // Si le métier est null (e.g. clic sur le fond du diagramme), désactiver le panneau de détails
            if (_metierEnEdition == null)
            {
                NettoyerDetails();
                SetUIState(MetierFormState.Initial);
                return;
            }

            _hasUnsavedChanges = false;
            PopulateDetailsPanel(_metierEnEdition);
            SetUIState(MetierFormState.Editing);
            // S'assurer que le nœud est mis en évidence même si c'est un nouveau métier qui n'était pas encore dans le graphe
            _metierDiagramControl.ForceSelection(_metierEnEdition.MetierId);
        }

        private void PopulateDetailsPanel(Metier metier)
        {
            if (metier == null)
            {
                NettoyerDetails();
                return;
            }

            lblMetierId.Text = metier.MetierId;
            txtNom.Text = metier.Nom;
            txtPictogram.Text = metier.Pictogram;
            panelCouleurApercu.BackColor = _projetService.GetDisplayColorForMetier(metier.MetierId);

            // Phases
            DetachPhaseCheckboxesEvents();
            chkGrosOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.GrosOeuvre);
            chkSecondOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.SecondOeuvre);
            chkFinition.Checked = metier.Phases.HasFlag(ChantierPhase.Finition);
            AttachPhaseCheckboxesEvents();

            // Prérequis
            PopulatePrerequisCheckList();
        }

        private void NettoyerDetails()
        {
            _metierEnEdition = null;
            lblMetierId.Text = "N/A";
            txtNom.Clear();
            txtPictogram.Clear();
            panelCouleurApercu.BackColor = SystemColors.Control;

            DetachPhaseCheckboxesEvents();
            chkGrosOeuvre.Checked = false;
            chkSecondOeuvre.Checked = false;
            chkFinition.Checked = false;
            AttachPhaseCheckboxesEvents();

            chkListPrerequis.Items.Clear();
        }

        private void AttachPhaseCheckboxesEvents()
        {
            chkGrosOeuvre.CheckedChanged += OnDetailChanged;
            chkSecondOeuvre.CheckedChanged += OnDetailChanged;
            chkFinition.CheckedChanged += OnDetailChanged;
        }

        private void DetachPhaseCheckboxesEvents()
        {
            chkGrosOeuvre.CheckedChanged -= OnDetailChanged;
            chkSecondOeuvre.CheckedChanged -= OnDetailChanged;
            chkFinition.CheckedChanged -= OnDetailChanged;
        }

        private void PopulatePrerequisCheckList()
        {
            chkListPrerequis.Items.Clear();
            if (_metierEnEdition == null) return;

            var allMetiers = _projetService.GetAllMetiers().ToList();
            var currentPrereqs = _projetService.GetPrerequisForMetier(_metierEnEdition.MetierId).ToHashSet();

            foreach (var metier in allMetiers.OrderBy(m => m.Nom))
            {
                // Un métier ne peut pas être son propre prérequis.
                // Vérifier si le métier prérequis ne crée pas de circularité pour le métier en cours d'édition.
                bool canBePrereq = (metier.MetierId != _metierEnEdition.MetierId);

                if (canBePrereq)
                {
                    // Pour éviter de proposer un prérequis qui formerait un cycle avec le métier EN COURS D'EDITION
                    // On doit simuler l'ajout de ce prérequis pour le métier en édition et valider le graphe.
                    // Cette validation est coûteuse, elle sera faite à la sauvegarde.
                    // Ici, on se contente de ne pas proposer le métier lui-même.

                    bool isChecked = currentPrereqs.Contains(metier.MetierId);
                    chkListPrerequis.Items.Add(metier, isChecked);
                }
            }
            chkListPrerequis.DisplayMember = "Nom";
        }

        private void OnDetailChanged(object sender, EventArgs e)
        {
            _hasUnsavedChanges = true;
        }

        private void chkListPrerequis_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _hasUnsavedChanges = true;

            var metierPredecesseur = (Metier)chkListPrerequis.Items[e.Index];
            var currentPrereqsList = _projetService.GetPrerequisForMetier(_metierEnEdition.MetierId).ToList();

            if (e.NewValue == CheckState.Checked)
            {
                if (!currentPrereqsList.Contains(metierPredecesseur.MetierId))
                {
                    currentPrereqsList.Add(metierPredecesseur.MetierId);
                }
            }
            else
            {
                currentPrereqsList.Remove(metierPredecesseur.MetierId);
            }
            _metierEnEdition.PrerequisMetierIds = string.Join(",", currentPrereqsList.Distinct().OrderBy(id => id));
        }

        private void btnChoisirCouleur_Click(object sender, EventArgs e)
        {
            if (_metierEnEdition == null) return;

            using var colorDialog = new ColorDialog();
            if (!string.IsNullOrEmpty(_metierEnEdition.CouleurHex))
            {
                try { colorDialog.Color = ColorTranslator.FromHtml(_metierEnEdition.CouleurHex); }
                catch { colorDialog.Color = Color.LightBlue; }
            }
            else { colorDialog.Color = Color.LightBlue; }

            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _metierEnEdition.CouleurHex = ColorTranslator.ToHtml(colorDialog.Color);
                panelCouleurApercu.BackColor = colorDialog.Color;
                OnDetailChanged(sender, e);
            }
        }

        private void btnNouveau_Click(object sender, EventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show("Des modifications non sauvegardées seront perdues. Continuer ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;
            }

            _metierEnEdition = new Metier
            {
                MetierId = _idGeneratorService.GenererProchainMetierId(_projetService.GetAllMetiers()),
                Nom = "Nouveau Métier",
                CouleurHex = "",
                Pictogram = "",
                Phases = ChantierPhase.None,
                PrerequisMetierIds = ""
            };
            _hasUnsavedChanges = true;

            PopulateDetailsPanel(_metierEnEdition);
            SetUIState(MetierFormState.Editing);
            txtNom.Focus();
            txtNom.SelectAll();
        }

        private void btnSauvegarder_Click(object sender, EventArgs e)
        {
            if (_metierEnEdition == null) return;

            _metierEnEdition.Nom = txtNom.Text;
            _metierEnEdition.Pictogram = txtPictogram.Text;

            ChantierPhase selectedPhases = ChantierPhase.None;
            if (chkGrosOeuvre.Checked) selectedPhases |= ChantierPhase.GrosOeuvre;
            if (chkSecondOeuvre.Checked) selectedPhases |= ChantierPhase.SecondOeuvre;
            if (chkFinition.Checked) selectedPhases |= ChantierPhase.Finition;
            _metierEnEdition.Phases = selectedPhases;

            var prereqsFromList = new List<string>();
            foreach (Metier checkedMetier in chkListPrerequis.CheckedItems)
            {
                prereqsFromList.Add(checkedMetier.MetierId);
            }
            _metierEnEdition.PrerequisMetierIds = string.Join(",", prereqsFromList.Distinct().OrderBy(id => id));

            if (string.IsNullOrWhiteSpace(_metierEnEdition.Nom))
            {
                MessageBox.Show("Le nom du métier ne peut pas être vide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_metierEnEdition.MetierId))
            {
                MessageBox.Show("L'ID du métier ne peut pas être vide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var allMetiersForValidation = _projetService.GetAllMetiers().ToList();
                allMetiersForValidation.RemoveAll(m => m.MetierId == _metierEnEdition.MetierId);
                allMetiersForValidation.Add(_metierEnEdition);

                _dependanceBuilder.ValiderMetier(_metierEnEdition, allMetiersForValidation);

                bool isNewMetier = (_projetService.GetMetierById(_metierEnEdition.MetierId) == null);

                if (isNewMetier)
                {
                    _projetService.AjouterMetier(_metierEnEdition);
                }
                else
                {
                    _projetService.ModifierMetier(
                        _metierEnEdition.MetierId,
                        _metierEnEdition.Nom,
                        _metierEnEdition.PrerequisMetierIds,
                        _metierEnEdition.CouleurHex,
                        _metierEnEdition.Pictogram,
                        _metierEnEdition.Phases);
                }

                _hasUnsavedChanges = false;
                ChargerDiagrammeMetiers();
                // IMPORTANT: Recharger l'objet métier après sauvegarde pour s'assurer qu'il est à jour
                // et re-sélectionner visuellement le nœud dans le diagramme.
                _metierEnEdition = _projetService.GetMetierById(_metierEnEdition.MetierId);
                PopulateDetailsPanel(_metierEnEdition);
                _metierDiagramControl.ForceSelection(_metierEnEdition.MetierId); // Force la sélection visuelle

                MessageBox.Show("Métier sauvegardé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetUIState(MetierFormState.Editing);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Erreur de validation: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show("Des modifications non sauvegardées seront perdues. Continuer ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;
            }

            _metierEnEdition = null;
            _hasUnsavedChanges = false;
            NettoyerDetails();
            SetUIState(MetierFormState.Initial);
            this.Close();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_metierEnEdition == null)
            {
                MessageBox.Show("Aucun métier sélectionné à supprimer.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifier si le métier existe réellement dans le service (pas juste un "nouveau" temporaire)
            if (_projetService.GetMetierById(_metierEnEdition.MetierId) == null)
            {
                MessageBox.Show("Ce métier n'existe pas encore dans la base et ne peut pas être supprimé. Annulez si vous souhaitez abandonner la création.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le métier '{_metierEnEdition.Nom}' ?\n\n" +
                                         "Cette action est irréversible et peut impacter les ouvriers et tâches y faisant référence.",
                                         "Confirmation de suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _projetService.SupprimerMetier(_metierEnEdition.MetierId);
                    _metierEnEdition = null;
                    _hasUnsavedChanges = false;
                    ChargerDiagrammeMetiers();
                    NettoyerDetails();
                    SetUIState(MetierFormState.Initial);
                    MessageBox.Show("Métier supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

    }
}