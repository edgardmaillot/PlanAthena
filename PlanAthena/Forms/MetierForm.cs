// Fichier: PlanAthena/Forms/MetierForm.cs
// Version: 0.5.0
// Description: Formulaire dédié à la gestion CRUD (Création, Lecture, Mise à jour) des métiers.
// La logique de configuration des prérequis a été déplacée vers PrerequisMetierForm.

using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class MetierForm : Form
    {
        #region Champs et Services

        private readonly RessourceService _ressourceService;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly ProjetService _projetService;

        private Metier _metierEnEdition = null;
        private bool _enCoursDeChargement = false;

        private enum MetierFormState { Initial, Editing }

        #endregion

        #region Constructeur et Initialisation

        public MetierForm(
            ProjetService projetService,
            RessourceService ressourceService,
            IIdGeneratorService idGeneratorService,
            DependanceBuilder dependanceBuilder)
        {
            InitializeComponent();
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _idGeneratorService = idGeneratorService ?? throw new ArgumentNullException(nameof(idGeneratorService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
        }

        private void MetierForm_Load(object sender, EventArgs e)
        {
            InitialiserFormulaire();
            SetUIState(MetierFormState.Initial);
        }

        #endregion

        #region Gestion CRUD Métiers

        private void InitialiserFormulaire()
        {
            panelLeft.Controls.Clear();
            CreerListeMetiers();
            groupBoxDetails.Text = "Détails du Métier";
            groupBoxDetails.Visible = true;
        }

        private void CreerListeMetiers()
        {
            _enCoursDeChargement = true;
            try
            {
                panelLeft.Controls.Clear();
                int yPos = 10;
                var lblTitre = new Label { Text = "📋 Liste des Métiers", Location = new Point(11, yPos), Size = new Size(200, 25), Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold), ForeColor = Color.DarkBlue };
                panelLeft.Controls.Add(lblTitre);
                yPos += 35;

                var metiersTries = _dependanceBuilder.ObtenirMetiersTriesParDependance()
                    .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H")
                    .ToList();

                foreach (var metier in metiersTries)
                {
                    try
                    {
                        var btnMetier = new Button
                        {
                            Text = metier.Nom,
                            Tag = metier,
                            Location = new Point(11, yPos),
                            Size = new Size(400, 35),
                            BackColor = _ressourceService.GetDisplayColorForMetier(metier.MetierId),
                            FlatStyle = FlatStyle.Popup,
                            TextAlign = ContentAlignment.MiddleLeft,
                            Padding = new Padding(10, 0, 0, 0)
                        };
                        btnMetier.Click += MetierButton_Click;
                        var phases = metier.Phases != ChantierPhase.None ? $"Phases: {metier.Phases}" : "Aucune phase définie";
                        _toolTipMetiers.SetToolTip(btnMetier, phases);
                        panelLeft.Controls.Add(btnMetier);
                        yPos += 40;
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur création bouton métier {metier.Nom}: {ex.Message}"); }
                }

                var btnNouveau = new Button
                {
                    Text = "➕ Créer nouveau métier",
                    Location = new Point(11, yPos),
                    Size = new Size(400, 35),
                    BackColor = Color.LightGreen,
                    FlatStyle = FlatStyle.Popup,
                    Font = new Font(this.Font, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                btnNouveau.Click += BtnNouveauMetier_Click;
                _toolTipMetiers.SetToolTip(btnNouveau, "Créer un nouveau métier");
                panelLeft.Controls.Add(btnNouveau);
            }
            finally { _enCoursDeChargement = false; }
        }

        private void MetierButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Metier metier)
            {
                _metierEnEdition = metier;
                PopulateDetailsPanel(metier);
                SetUIState(MetierFormState.Editing);
            }
        }

        private void BtnNouveauMetier_Click(object sender, EventArgs e)
        {
            _metierEnEdition = _ressourceService.CreerMetier();
            PopulateDetailsPanel(_metierEnEdition);
            SetUIState(MetierFormState.Editing);
            txtNom.Focus();
            txtNom.SelectAll();
        }

        #endregion

        #region Sauvegarde Automatique

        private void OnDetailChanged(object sender, EventArgs e)
        {
            if (_enCoursDeChargement) return;
            if (_metierEnEdition != null)
            {
                SauvegardeAutomatiqueMetier();
            }
        }

        private void SauvegardeAutomatiqueMetier()
        {
            if (_metierEnEdition == null) return;
            try
            {
                _metierEnEdition.Nom = txtNom.Text.Trim();
                _metierEnEdition.Pictogram = txtPictogram.Text.Trim();
                ChantierPhase selectedPhases = ChantierPhase.None;
                if (chkGrosOeuvre.Checked) selectedPhases |= ChantierPhase.GrosOeuvre;
                if (chkSecondOeuvre.Checked) selectedPhases |= ChantierPhase.SecondOeuvre;
                if (chkFinition.Checked) selectedPhases |= ChantierPhase.Finition;
                _metierEnEdition.Phases = selectedPhases;
                if (string.IsNullOrWhiteSpace(_metierEnEdition.Nom) || selectedPhases == ChantierPhase.None) return;
                _ressourceService.ModifierMetier(_metierEnEdition);
                CreerListeMetiers();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur sauvegarde automatique: {ex.Message}"); }
        }

        #endregion

        #region Gestion États et Interface

        private void SetUIState(MetierFormState state)
        {
            switch (state)
            {
                case MetierFormState.Initial:
                    groupBoxDetails.Enabled = false;
                    NettoyerDetails();
                    btnConfigurerPrerequis.Enabled = false;
                    break;
                case MetierFormState.Editing:
                    groupBoxDetails.Enabled = true;
                    btnConfigurerPrerequis.Enabled = true;
                    break;
            }
        }

        private void PopulateDetailsPanel(Metier metier)
        {
            _enCoursDeChargement = true;
            try
            {
                if (metier == null) { NettoyerDetails(); return; }
                lblMetierId.Text = metier.MetierId;
                txtNom.Text = metier.Nom;
                txtPictogram.Text = metier.Pictogram;
                panelCouleurApercu.BackColor = _ressourceService.GetDisplayColorForMetier(metier.MetierId);
                DetachPhaseCheckboxesEvents();
                chkGrosOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.GrosOeuvre);
                chkSecondOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.SecondOeuvre);
                chkFinition.Checked = metier.Phases.HasFlag(ChantierPhase.Finition);
                AttachPhaseCheckboxesEvents();
            }
            finally { _enCoursDeChargement = false; }
        }

        private void NettoyerDetails()
        {
            _enCoursDeChargement = true;
            try
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
            }
            finally { _enCoursDeChargement = false; }
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

        #endregion

        #region Actions

        private bool EstMetierValide(Metier metier)
        {
            if (metier == null) return true;
            var nom = txtNom.Text.Trim();
            ChantierPhase phases = ChantierPhase.None;
            if (chkGrosOeuvre.Checked) phases |= ChantierPhase.GrosOeuvre;
            if (chkSecondOeuvre.Checked) phases |= ChantierPhase.SecondOeuvre;
            if (chkFinition.Checked) phases |= ChantierPhase.Finition;
            return !string.IsNullOrWhiteSpace(nom) && phases != ChantierPhase.None;
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
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _metierEnEdition.CouleurHex = ColorTranslator.ToHtml(colorDialog.Color);
                panelCouleurApercu.BackColor = colorDialog.Color;
                OnDetailChanged(sender, e);
            }
        }

        private void btnConfigurerPrerequis_Click(object sender, EventArgs e)
        {
            if (_metierEnEdition != null && !EstMetierValide(_metierEnEdition))
            {
                MessageBox.Show("Impossible de passer à la configuration des prérequis :\n\n• Le nom du métier ne peut pas être vide\n• Au moins une phase doit être sélectionnée\n\nVeuillez corriger ces erreurs.", "Métier invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_ressourceService.GetAllMetiers().Count < 2)
            {
                MessageBox.Show("Il faut au moins 2 métiers pour configurer les prérequis.", "Configuration impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var prerequisForm = new PrerequisMetierForm(_projetService, _ressourceService, _idGeneratorService, _dependanceBuilder);
            prerequisForm.ShowDialog(this);

            // Rafraîchir la liste car les dépendances peuvent avoir changé l'ordre de tri topologique
            CreerListeMetiers();
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}