// Fichier: PlanAthena/Forms/PrerequisMetierForm.cs
// Version: 0.5.0
// Description: Nouveau formulaire dédié à la configuration des prérequis métiers par phase.
// Contient la logique graphique et la gestion des dépendances (ancien "Concept 2").


using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using PlanAthena.View;
using PlanAthena.View.Ressources;
using PlanAthena.View.Ressources.MetierDiagram;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace PlanAthena.Forms
{
    public partial class PrerequisMetierForm : Form
    {
        #region Champs et Services

        private readonly RessourceService _ressourceService;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly ProjetService _projetService;

        private bool _enCoursDeChargement = false;
        private TabControl _tabControlPhases;
        private readonly Dictionary<ChantierPhase, PlanAthena.View.Ressources.MetierDiagram.MetierDiagramControl> _diagrammesParPhase = new();
        private Metier _metierSelectionne = null;
        private ChantierPhase _phaseSelectionnee = ChantierPhase.None;

        #endregion

        #region Constructeur et Initialisation

        public PrerequisMetierForm(
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

        private void PrerequisMetierForm_Load(object sender, EventArgs e)
        {
            InitialiserFormulaire();
        }

        #endregion

        #region Gestion Prérequis par Phase

        private void InitialiserFormulaire()
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
                this.Text = "Configuration Prérequis par Phase";
            }
            finally
            {
                _enCoursDeChargement = false;
            }
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
            groupBoxDetails.Enabled = true;

            var oldChkList = groupBoxDetails.Controls.OfType<CheckedListBox>().FirstOrDefault(c => c.Name == "chkListPrerequisPhase");
            if (oldChkList != null) { groupBoxDetails.Controls.Remove(oldChkList); oldChkList.Dispose(); }

            var chkListPrerequis = new CheckedListBox { Name = "chkListPrerequisPhase", Location = new Point(16, 200), Size = new Size(300, 200), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, CheckOnClick = true };
            chkListPrerequis.ItemCheck += ChkListPrerequisPhase_ItemCheck;
            groupBoxDetails.Controls.Add(chkListPrerequis);

            groupBoxDetails.Text = "Configuration Prérequis";
            var lblExplication = new Label { Text = "Sélectionnez un métier dans le diagramme\npour configurer ses prérequis pour cette phase.", Location = new Point(16, 170), Size = new Size(300, 30), Name = "lblExplicationPrerequis" };
            groupBoxDetails.Controls.Add(lblExplication);
        }

        private void DiagrammePhase_MetierSelected(ChantierPhase phase, MetierSelectedEventArgs e)
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
            finally
            {
                _enCoursDeChargement = false;
            }
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

            chkGrosOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.GrosOeuvre);
            chkSecondOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.SecondOeuvre);
            chkFinition.Checked = metier.Phases.HasFlag(ChantierPhase.Finition);

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

        private void NettoyerDetails()
        {
            _enCoursDeChargement = true;
            try
            {
                _metierSelectionne = null;
                lblMetierId.Text = "N/A";
                txtNom.Clear();
                txtPictogram.Clear();
                panelCouleurApercu.BackColor = SystemColors.Control;
                chkGrosOeuvre.Checked = false;
                chkSecondOeuvre.Checked = false;
                chkFinition.Checked = false;
            }
            finally
            {
                _enCoursDeChargement = false;
            }
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

        #region Actions

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion
    }
}