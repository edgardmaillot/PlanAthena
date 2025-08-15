// Fichier: PlanAthena/Forms/MetierForm.cs
// Version: 0.4.4
// Description: Refactorisation chirurgicale pour s'aligner sur la nouvelle architecture.
// Les appels liés aux métiers sont redirigés de ProjetService vers RessourceService.

using PlanAthena.Controls;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class MetierForm : Form
    {
        #region Champs et Services (Corrigés)

        private readonly RessourceService _ressourceService;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly ProjetService _projetService;

        private Metier _metierEnEdition = null;
        private bool _enCoursDeChargement = false;
        private enum ConceptMode { Concept1_CrudSimple, Concept2_PrecedencesPhase }
        private ConceptMode _currentMode = ConceptMode.Concept1_CrudSimple;
        private TabControl _tabControlPhases;
        private readonly Dictionary<ChantierPhase, MetierDiagramControl> _diagrammesParPhase = new();
        private Metier _metierSelectionne = null;
        private ChantierPhase _phaseSelectionnee = ChantierPhase.None;

        #endregion

        #region Constructeur et Initialisation (Corrigés)

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
            InitialiserConcept1();
            SetUIState(MetierFormState.Initial);
        }

        #endregion

        #region CONCEPT 1 - CRUD Métiers Simple (Corrigé)

        private void InitialiserConcept1()
        {
            _currentMode = ConceptMode.Concept1_CrudSimple;
            panelLeft.Controls.Clear();
            CreerListeMetiers();
            groupBoxDetails.Text = "Détails du Métier";
            groupBoxDetails.Visible = true;
            txtNom.ReadOnly = false;
            txtPictogram.ReadOnly = false;
            panelCouleurApercu.Enabled = true;
            btnChoisirCouleur.Enabled = true;
            grpPhases.Enabled = true;
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

        #region CONCEPT 2 - Précédences par Phase (Corrigé)

        private void InitialiserConcept2()
        {
            if (_ressourceService.GetAllMetiers().Count < 2)
            {
                MessageBox.Show("Il faut au moins 2 métiers pour configurer les prérequis.", "Configuration impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _currentMode = ConceptMode.Concept2_PrecedencesPhase;
            CreerInterfaceConcept2();
        }

        private void CreerInterfaceConcept2()
        {
            _enCoursDeChargement = true;
            try
            {
                panelLeft.Controls.Clear();
                _tabControlPhases = new TabControl { Dock = DockStyle.Fill, Name = "tabControlPhases" };
                CreerOngletPhase(ChantierPhase.GrosOeuvre, "Gros Œuvre");
                CreerOngletPhase(ChantierPhase.SecondOeuvre, "Second Œuvre");
                CreerOngletPhase(ChantierPhase.Finition, "Finition");
                panelLeft.Controls.Add(_tabControlPhases);
                AdapterGroupBoxDetailsPourPrerequisPhase();
                btnValider.Text = "💾 Sauvegarder Prérequis";
                btnAnnuler.Text = "⬅️ Retour CRUD";
                this.Text = "Configuration Prérequis par Phase";
            }
            finally { _enCoursDeChargement = false; }
        }

        private void CreerOngletPhase(ChantierPhase phase, string nomPhase)
        {
            var tabPage = new TabPage(nomPhase) { Name = $"tab{phase}" };
            var diagramme = new MetierDiagramControl { Dock = DockStyle.Fill, Name = $"diagram{phase}" };
            var settings = new MetierDiagramSettings();
            diagramme.Initialize(_projetService, _ressourceService, _dependanceBuilder, settings);
            diagramme.MetierSelected += (sender, args) => DiagrammePhase_MetierSelected(phase, args);
            _diagrammesParPhase[phase] = diagramme;
            tabPage.Controls.Add(diagramme);
            _tabControlPhases.TabPages.Add(tabPage);
            ChargerMetiersPhase(phase);
        }

        private void ChargerMetiersPhase(ChantierPhase phase)
        {
            if (!_diagrammesParPhase.TryGetValue(phase, out var diagramme)) return;
            var metiersPhase = _ressourceService.GetAllMetiers()
                .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H" && m.Phases.HasFlag(phase))
                .ToList();
            diagramme.ChargerDonnees(metiersPhase, phase);
            diagramme.ZoomToutAjuster();
        }

        private void AdapterGroupBoxDetailsPourPrerequisPhase()
        {
            txtNom.ReadOnly = true;
            txtPictogram.ReadOnly = true;
            panelCouleurApercu.Enabled = false;
            btnChoisirCouleur.Enabled = false;
            grpPhases.Enabled = false;
            var oldChkList = groupBoxDetails.Controls.OfType<CheckedListBox>().FirstOrDefault(c => c.Name == "chkListPrerequisPhase");
            if (oldChkList != null) { groupBoxDetails.Controls.Remove(oldChkList); oldChkList.Dispose(); }
            var chkListPrerequis = new CheckedListBox { Name = "chkListPrerequisPhase", Location = new Point(16, 200), Size = new Size(300, 200), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, CheckOnClick = true };
            chkListPrerequis.ItemCheck += ChkListPrerequisPhase_ItemCheck;
            groupBoxDetails.Controls.Add(chkListPrerequis);
            groupBoxDetails.Text = "Configuration Prérequis";
            var lblExplication = new Label { Text = "Sélectionnez un métier dans le diagramme\npour configurer ses prérequis pour cette phase.", Location = new Point(16, 170), Size = new Size(300, 30), Name = "lblExplicationPrerequis" };
            groupBoxDetails.Controls.Add(lblExplication);
        }

        private void DiagrammePhase_MetierSelected(ChantierPhase phase, PlanAthena.Controls.MetierSelectedEventArgs e)
        {
            _enCoursDeChargement = true;
            try
            {
                _phaseSelectionnee = phase;
                if (e.SelectedMetier == null)
                {
                    _metierSelectionne = null;
                    ViderPrerequisPhase();
                    return;
                }
                _metierSelectionne = e.SelectedMetier;
                ChargerPrerequisMetierPhase(phase, e.SelectedMetier);
            }
            finally { _enCoursDeChargement = false; }
        }

        private void ChargerPrerequisMetierPhase(ChantierPhase phase, Metier metier)
        {
            var chkList = groupBoxDetails.Controls.OfType<CheckedListBox>().FirstOrDefault(c => c.Name == "chkListPrerequisPhase");
            if (chkList == null) return;
            chkList.Items.Clear();
            lblMetierId.Text = metier.MetierId;
            txtNom.Text = metier.Nom;
            txtPictogram.Text = metier.Pictogram;
            panelCouleurApercu.BackColor = _ressourceService.GetDisplayColorForMetier(metier.MetierId);
            DetachPhaseCheckboxesEvents();
            chkGrosOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.GrosOeuvre);
            chkSecondOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.SecondOeuvre);
            chkFinition.Checked = metier.Phases.HasFlag(ChantierPhase.Finition);
            AttachPhaseCheckboxesEvents();
            var metiersDisponibles = _ressourceService.GetAllMetiers()
                .Where(m => m.MetierId != metier.MetierId && m.MetierId != "JALON" && m.MetierId != "SYNC_0H" && m.Phases.HasFlag(phase))
                .OrderBy(m => m.Nom).ToList();
            var prerequisActuels = _ressourceService.GetPrerequisPourPhase(metier.MetierId, phase);
            foreach (var metierDisponible in metiersDisponibles)
            {
                bool isChecked = prerequisActuels.Contains(metierDisponible.MetierId);
                chkList.Items.Add(metierDisponible, isChecked);
            }
            chkList.DisplayMember = "Nom";
            groupBoxDetails.Text = $"Prérequis {metier.Nom} - {phase}";
            chkList.Tag = new { Metier = metier, Phase = phase };
        }

        private void ViderPrerequisPhase()
        {
            var chkList = groupBoxDetails.Controls.OfType<CheckedListBox>().FirstOrDefault(c => c.Name == "chkListPrerequisPhase");
            if (chkList != null) { chkList.Items.Clear(); chkList.Tag = null; }
            groupBoxDetails.Text = "Configuration Prérequis";
            NettoyerDetails();
        }

        private void ChkListPrerequisPhase_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_enCoursDeChargement) return;
            if (sender is CheckedListBox chkList && chkList.Tag != null)
            {
                dynamic tagData = chkList.Tag;
                Metier metier = tagData.Metier;
                ChantierPhase phase = tagData.Phase;
                BeginInvoke(new Action(() => SauvegarderPrerequisAutomatique(chkList, metier, phase)));
            }
        }

        private void SauvegarderPrerequisAutomatique(CheckedListBox chkList, Metier metier, ChantierPhase phase)
        {
            if (_enCoursDeChargement) return;
            try
            {
                var prerequisCoches = chkList.CheckedItems.Cast<Metier>().Select(m => m.MetierId).ToList();
                metier.PrerequisParPhase[phase] = prerequisCoches;
                _ressourceService.ModifierMetier(metier);
                foreach (var (phaseKey, diagramme) in _diagrammesParPhase)
                {
                    ChargerMetiersPhase(phaseKey);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"❌ Erreur sauvegarde automatique prérequis: {ex.Message}"); }
        }

        #endregion

        #region Sauvegarde Automatique (Corrigé)

        private void OnDetailChanged(object sender, EventArgs e)
        {
            if (_enCoursDeChargement) return;
            if (_metierEnEdition != null && _currentMode == ConceptMode.Concept1_CrudSimple)
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

        private enum MetierFormState { Initial, Editing }

        private void SetUIState(MetierFormState state)
        {
            switch (state)
            {
                case MetierFormState.Initial:
                    groupBoxDetails.Enabled = false;
                    NettoyerDetails();
                    btnValider.Enabled = (_currentMode == ConceptMode.Concept1_CrudSimple);
                    break;
                case MetierFormState.Editing:
                    groupBoxDetails.Enabled = true;
                    btnValider.Enabled = true;
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

        #region Transitions et Actions

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

        private void btnValider_Click(object sender, EventArgs e)
        {
            if (_currentMode == ConceptMode.Concept1_CrudSimple)
            {
                if (_metierEnEdition != null && !EstMetierValide(_metierEnEdition))
                {
                    MessageBox.Show("Impossible de passer à la configuration des prérequis :\n\n• Le nom du métier ne peut pas être vide\n• Au moins une phase doit être sélectionnée\n\nVeuillez corriger ces erreurs.", "Métier invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                InitialiserConcept2();
            }
            else { SauvegarderPrerequisConcept2(); }
        }

        private void SauvegarderPrerequisConcept2()
        {
            try
            {
                MessageBox.Show("Configuration des prérequis par phase terminée.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RetournerConcept1();
            }
            catch (Exception ex) { MessageBox.Show($"Erreur lors de la finalisation: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            if (_currentMode == ConceptMode.Concept2_PrecedencesPhase)
            {
                RetournerConcept1();
                return;
            }
            _metierEnEdition = null;
            NettoyerDetails();
            SetUIState(MetierFormState.Initial);
            this.Close();
        }

        private void RetournerConcept1()
        {
            _currentMode = ConceptMode.Concept1_CrudSimple;
            panelLeft.Controls.Clear();
            _diagrammesParPhase.Clear();
            _tabControlPhases?.Dispose();
            _tabControlPhases = null;
            RestaurerGroupBoxDetailsOriginal();
            InitialiserConcept1();
            btnValider.Text = "✅ Valider";
            btnAnnuler.Text = "❌ Annuler";
            this.Text = "Gestion des Métiers";
        }

        private void RestaurerGroupBoxDetailsOriginal()
        {
            var chkListToRemove = groupBoxDetails.Controls.OfType<CheckedListBox>().Where(c => c.Name == "chkListPrerequisPhase").ToList();
            foreach (var control in chkListToRemove) { groupBoxDetails.Controls.Remove(control); control.Dispose(); }
            var lblToRemove = groupBoxDetails.Controls.OfType<Label>().Where(c => c.Name == "lblExplicationPrerequis").ToList();
            foreach (var control in lblToRemove) { groupBoxDetails.Controls.Remove(control); control.Dispose(); }
            txtNom.ReadOnly = false;
            txtPictogram.ReadOnly = false;
            panelCouleurApercu.Enabled = true;
            btnChoisirCouleur.Enabled = true;
            grpPhases.Enabled = true;
            groupBoxDetails.Text = "Détails du Métier";
        }

        #endregion
    }
}