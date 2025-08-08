// Forms/MetierForm.cs
// 🔄 VERSION CORRIGÉE V0.4.2 - DÉBUGGÉE selon spécifications

using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using PlanAthena.Interfaces;
using PlanAthena.Controls;
using PlanAthena.Controls.Config;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    /// <summary>
    /// 🔄 VERSION CORRIGÉE V0.4.2 - MetierForm avec 2 concepts séparés + prérequis par phase
    /// 
    /// CORRECTIONS APPLIQUÉES :
    /// 1. Protection rigoureuse contre sauvegarde automatique pendant chargement
    /// 2. Exclusion du métier sélectionné de ses propres prérequis
    /// 3. Filtrage correct des métiers par phase
    /// 4. Champs en lecture seule dans Concept 2
    /// 5. Gestion correcte des événements ItemCheck
    /// </summary>
    public partial class MetierForm : Form
    {
        #region Champs et Services

        private readonly ProjetService _projetService;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly DependanceBuilder _dependanceBuilder;

        private Metier _metierEnEdition = null;

        // 🛡️ PROTECTION RENFORCÉE : Sauvegarde automatique
        private bool _enCoursDeChargement = false;

        // 🆕 V0.4.2 : Mode de fonctionnement
        private enum ConceptMode
        {
            Concept1_CrudSimple,      // CRUD métiers sans prérequis
            Concept2_PrecedencesPhase // Configuration prérequis par phase
        }

        private ConceptMode _currentMode = ConceptMode.Concept1_CrudSimple;

        // 🆕 V0.4.2 : Contrôles Concept 2
        private TabControl _tabControlPhases;
        private readonly Dictionary<ChantierPhase, MetierDiagramControl> _diagrammesParPhase = new();

        // 🆕 AJOUT : Métier et phase actuellement sélectionnés dans Concept 2
        private Metier _metierSelectionne = null;
        private ChantierPhase _phaseSelectionnee = ChantierPhase.None;

        #endregion

        #region Constructeur et Initialisation

        public MetierForm(
            ProjetService projetService,
            IIdGeneratorService idGeneratorService,
            DependanceBuilder dependanceBuilder)
        {
            InitializeComponent();

            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _idGeneratorService = idGeneratorService ?? throw new ArgumentNullException(nameof(idGeneratorService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
        }

        private void MetierForm_Load(object sender, EventArgs e)
        {
            InitialiserConcept1();
            SetUIState(MetierFormState.Initial);
        }

        #endregion

        #region 🆕 CONCEPT 1 - CRUD Métiers Simple

        private void InitialiserConcept1()
        {
            _currentMode = ConceptMode.Concept1_CrudSimple;
            panelLeft.Controls.Clear();
            CreerListeMetiers();
            groupBoxDetails.Text = "Détails du Métier";
            groupBoxDetails.Visible = true;

            // Restaurer contrôles éditables
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

                var lblTitre = new Label
                {
                    Text = "📋 Liste des Métiers",
                    Location = new Point(11, yPos),
                    Size = new Size(200, 25),
                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                    ForeColor = Color.DarkBlue
                };
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
                            BackColor = _projetService.GetDisplayColorForMetier(metier.MetierId),
                            FlatStyle = FlatStyle.Popup,
                            TextAlign = ContentAlignment.MiddleLeft,
                            Padding = new Padding(10, 0, 0, 0)
                        };

                        btnMetier.Click += MetierButton_Click;

                        var phases = metier.Phases != ChantierPhase.None ?
                            $"Phases: {metier.Phases}" : "Aucune phase définie";
                        _toolTipMetiers.SetToolTip(btnMetier, phases);

                        panelLeft.Controls.Add(btnMetier);
                        yPos += 40;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur création bouton métier {metier.Nom}: {ex.Message}");
                    }
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
            finally
            {
                _enCoursDeChargement = false;
            }
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
            _metierEnEdition = new Metier
            {
                MetierId = _idGeneratorService.GenererProchainMetierId(_projetService.GetAllMetiers()),
                Nom = "Nouveau Métier",
                CouleurHex = "",
                Pictogram = "",
                Phases = ChantierPhase.None,
                PrerequisParPhase = new Dictionary<ChantierPhase, List<string>>()
            };

            PopulateDetailsPanel(_metierEnEdition);
            SetUIState(MetierFormState.Editing);
            txtNom.Focus();
            txtNom.SelectAll();
        }

        #endregion

        #region 🆕 CONCEPT 2 - Précédences par Phase

        private void InitialiserConcept2()
        {
            if (_projetService.GetAllMetiers().Count < 2)
            {
                MessageBox.Show("Il faut au moins 2 métiers pour configurer les prérequis.",
                    "Configuration impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                _tabControlPhases = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Name = "tabControlPhases"
                };

                CreerOngletPhase(ChantierPhase.GrosOeuvre, "Gros Œuvre");
                CreerOngletPhase(ChantierPhase.SecondOeuvre, "Second Œuvre");
                CreerOngletPhase(ChantierPhase.Finition, "Finition");

                panelLeft.Controls.Add(_tabControlPhases);
                AdapterGroupBoxDetailsPourPrerequisPhase();

                btnValider.Text = "💾 Sauvegarder Prérequis";
                btnAnnuler.Text = "⬅️ Retour CRUD";
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

            var diagramme = new MetierDiagramControl
            {
                Dock = DockStyle.Fill,
                Name = $"diagram{phase}"
            };

            var settings = new MetierDiagramSettings();
            diagramme.Initialize(_projetService, _dependanceBuilder, settings);
            diagramme.MetierSelected += (sender, args) => DiagrammePhase_MetierSelected(phase, args);

            _diagrammesParPhase[phase] = diagramme;
            tabPage.Controls.Add(diagramme);
            _tabControlPhases.TabPages.Add(tabPage);

            ChargerMetiersPhase(phase);
        }

        private void ChargerMetiersPhase(ChantierPhase phase)
        {
            if (!_diagrammesParPhase.TryGetValue(phase, out var diagramme))
                return;

            var metiersPhase = _projetService.GetAllMetiers()
                .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H")
                .Where(m => m.Phases.HasFlag(phase))
                .ToList();

            diagramme.ChargerDonnees(metiersPhase, phase);
            diagramme.ZoomToutAjuster();
        }

        private void AdapterGroupBoxDetailsPourPrerequisPhase()
        {
            // 🔒 CHAMPS EN LECTURE SEULE pour Concept 2
            txtNom.ReadOnly = true;
            txtPictogram.ReadOnly = true;
            panelCouleurApercu.Enabled = false;
            btnChoisirCouleur.Enabled = false;
            grpPhases.Enabled = false;

            // Supprimer ancienne CheckedListBox si elle existe
            var oldChkList = groupBoxDetails.Controls.OfType<CheckedListBox>()
                .FirstOrDefault(c => c.Name == "chkListPrerequisPhase");
            if (oldChkList != null)
            {
                groupBoxDetails.Controls.Remove(oldChkList);
                oldChkList.Dispose();
            }

            // Créer nouvelle CheckedListBox pour prérequis
            var chkListPrerequis = new CheckedListBox
            {
                Name = "chkListPrerequisPhase",
                Location = new Point(16, 200),
                Size = new Size(300, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                CheckOnClick = true
            };

            chkListPrerequis.ItemCheck += ChkListPrerequisPhase_ItemCheck;
            groupBoxDetails.Controls.Add(chkListPrerequis);

            groupBoxDetails.Text = "Configuration Prérequis";

            var lblExplication = new Label
            {
                Text = "Sélectionnez un métier dans le diagramme\npour configurer ses prérequis pour cette phase.",
                Location = new Point(16, 170),
                Size = new Size(300, 30),
                Name = "lblExplicationPrerequis"
            };
            groupBoxDetails.Controls.Add(lblExplication);
        }

        /// <summary>
        /// 🔧 CORRIGÉ : Gestion sélection métier avec protection rigoureuse
        /// </summary>
        private void DiagrammePhase_MetierSelected(ChantierPhase phase, PlanAthena.Controls.MetierSelectedEventArgs e)
        {
            // 🛡️ PROTECTION COMPLÈTE dès l'entrée
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
                // 🛡️ Réactiver SEULEMENT après chargement complet
                _enCoursDeChargement = false;
            }
        }

        /// <summary>
        /// 🔧 CORRIGÉ : Charge prérequis de LA MÊME PHASE uniquement
        /// CORRECTION BUG : Assure que GetPrerequisMetier retourne une liste vide si la phase n'existe pas
        /// </summary>
        private void ChargerPrerequisMetierPhase(ChantierPhase phase, Metier metier)
        {
            var chkList = groupBoxDetails.Controls.OfType<CheckedListBox>()
                .FirstOrDefault(c => c.Name == "chkListPrerequisPhase");

            if (chkList == null) return;

            chkList.Items.Clear();

            // Afficher infos métier sélectionné (lecture seule)
            lblMetierId.Text = metier.MetierId;
            txtNom.Text = metier.Nom;
            txtPictogram.Text = metier.Pictogram;
            panelCouleurApercu.BackColor = _projetService.GetDisplayColorForMetier(metier.MetierId);

            // Phases (lecture seule)
            DetachPhaseCheckboxesEvents();
            chkGrosOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.GrosOeuvre);
            chkSecondOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.SecondOeuvre);
            chkFinition.Checked = metier.Phases.HasFlag(ChantierPhase.Finition);
            AttachPhaseCheckboxesEvents();

            // 🔧 LOGIQUE SIMPLE : Métiers de la MÊME phase, SAUF lui-même
            var metiersDisponibles = _projetService.GetAllMetiers()
                .Where(m => m.MetierId != metier.MetierId) // ✅ PAS lui-même
                .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H") // ✅ PAS les métiers système
                .Where(m => m.Phases.HasFlag(phase)) // ✅ MÊME PHASE
                .OrderBy(m => m.Nom)
                .ToList();

            // 🔧 CORRECTION CRITIQUE : Prérequis SEULEMENT pour cette phase spécifique
            // S'assurer qu'on ne récupère pas les prérequis d'autres phases par erreur
            var prerequisActuels = new List<string>();
            if (metier.PrerequisParPhase != null && metier.PrerequisParPhase.ContainsKey(phase))
            {
                prerequisActuels = metier.PrerequisParPhase[phase].ToList();
            }

            System.Diagnostics.Debug.WriteLine($"🔍 CHARGEMENT - Métier: {metier.Nom}, Phase: {phase}");
            System.Diagnostics.Debug.WriteLine($"  📋 Métiers disponibles: {metiersDisponibles.Count}");
            System.Diagnostics.Debug.WriteLine($"  ✅ Prérequis actuels: [{string.Join(", ", prerequisActuels)}]");
            System.Diagnostics.Debug.WriteLine($"  📊 PrerequisParPhase complet:");
            if (metier.PrerequisParPhase != null)
            {
                foreach (var kvp in metier.PrerequisParPhase)
                {
                    System.Diagnostics.Debug.WriteLine($"    Phase {kvp.Key}: [{string.Join(", ", kvp.Value)}]");
                }
            }

            foreach (var metierDisponible in metiersDisponibles)
            {
                bool isChecked = prerequisActuels.Contains(metierDisponible.MetierId);
                chkList.Items.Add(metierDisponible, isChecked);

                System.Diagnostics.Debug.WriteLine($"    {(isChecked ? "✅" : "⬜")} {metierDisponible.Nom} (Phases: {metierDisponible.Phases})");
            }

            chkList.DisplayMember = "Nom";
            groupBoxDetails.Text = $"Prérequis {metier.Nom} - {phase}";
            chkList.Tag = new { Metier = metier, Phase = phase };
        }

        private void ViderPrerequisPhase()
        {
            // Protection déjà activée dans DiagrammePhase_MetierSelected

            var chkList = groupBoxDetails.Controls.OfType<CheckedListBox>()
                .FirstOrDefault(c => c.Name == "chkListPrerequisPhase");

            if (chkList != null)
            {
                chkList.Items.Clear();
                chkList.Tag = null;
            }

            groupBoxDetails.Text = "Configuration Prérequis";
            NettoyerDetails();
        }

        /// <summary>
        /// 🔧 CORRIGÉ : Sauvegarde automatique avec protection rigoureuse
        /// </summary>
        private void ChkListPrerequisPhase_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // 🛡️ PROTECTION : Ne pas sauvegarder pendant chargement
            if (_enCoursDeChargement)
            {
                System.Diagnostics.Debug.WriteLine($"🛡️ PROTECTION ACTIVÉE - ItemCheck ignoré pendant chargement");
                return;
            }

            if (sender is CheckedListBox chkList && chkList.Tag != null)
            {
                dynamic tagData = chkList.Tag;
                Metier metier = tagData.Metier;
                ChantierPhase phase = tagData.Phase;

                // Sauvegarde différée après ItemCheck
                BeginInvoke(new Action(() => SauvegarderPrerequisAutomatique(chkList, metier, phase)));
            }
        }

        private void SauvegarderPrerequisAutomatique(CheckedListBox chkList, Metier metier, ChantierPhase phase)
        {
            // 🛡️ DOUBLE PROTECTION 
            if (_enCoursDeChargement)
            {
                System.Diagnostics.Debug.WriteLine($"🛡️ PROTECTION DOUBLE - Sauvegarde ignorée pendant chargement");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 SAUVEGARDE AUTO - Métier: {metier.Nom}, Phase: {phase}");

                var prerequisCoches = new List<string>();
                foreach (var item in chkList.CheckedItems)
                {
                    if (item is Metier metierPrerequis)
                    {
                        prerequisCoches.Add(metierPrerequis.MetierId);
                        System.Diagnostics.Debug.WriteLine($"  ✅ Prérequis coché: {metierPrerequis.Nom}");
                    }
                }

                // DEBUG état avant/après
                System.Diagnostics.Debug.WriteLine($"📋 AVANT - PrerequisParPhase du métier {metier.Nom}:");
                if (metier.PrerequisParPhase != null)
                {
                    foreach (var kvp in metier.PrerequisParPhase)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Phase {kvp.Key}: [{string.Join(", ", kvp.Value)}]");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("  PrerequisParPhase était null");
                }

                if (metier.PrerequisParPhase == null)
                    metier.PrerequisParPhase = new Dictionary<ChantierPhase, List<string>>();

                metier.PrerequisParPhase[phase] = prerequisCoches;

                System.Diagnostics.Debug.WriteLine($"📋 APRÈS - PrerequisParPhase du métier {metier.Nom}:");
                foreach (var kvp in metier.PrerequisParPhase)
                {
                    System.Diagnostics.Debug.WriteLine($"  Phase {kvp.Key}: [{string.Join(", ", kvp.Value)}]");
                }

                _projetService.ModifierMetier(
                    metier.MetierId,
                    metier.Nom,
                    metier.PrerequisParPhase,
                    metier.CouleurHex,
                    metier.Pictogram,
                    metier.Phases
                );

                System.Diagnostics.Debug.WriteLine($"💾 Sauvegardé dans ProjetService");

                // Rafraîchir SEULEMENT les diagrammes, PAS le formulaire
                foreach (var (phaseKey, diagramme) in _diagrammesParPhase)
                {
                    ChargerMetiersPhase(phaseKey);
                }

                System.Diagnostics.Debug.WriteLine($"🔄 Diagrammes rafraîchis");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur sauvegarde automatique prérequis: {ex.Message}");
            }
        }

        #endregion

        #region 🔄 Sauvegarde Automatique

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

                if (string.IsNullOrWhiteSpace(_metierEnEdition.Nom) || selectedPhases == ChantierPhase.None)
                {
                    return;
                }

                if (_metierEnEdition.PrerequisParPhase == null)
                    _metierEnEdition.PrerequisParPhase = new Dictionary<ChantierPhase, List<string>>();

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
                        _metierEnEdition.PrerequisParPhase,
                        _metierEnEdition.CouleurHex,
                        _metierEnEdition.Pictogram,
                        _metierEnEdition.Phases
                    );
                }

                CreerListeMetiers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur sauvegarde automatique: {ex.Message}");
            }
        }

        #endregion

        #region 🔄 Gestion États et Interface

        private enum MetierFormState
        {
            Initial,
            Editing,
        }

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
                if (metier == null)
                {
                    NettoyerDetails();
                    return;
                }

                lblMetierId.Text = metier.MetierId;
                txtNom.Text = metier.Nom;
                txtPictogram.Text = metier.Pictogram;
                panelCouleurApercu.BackColor = _projetService.GetDisplayColorForMetier(metier.MetierId);

                DetachPhaseCheckboxesEvents();
                chkGrosOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.GrosOeuvre);
                chkSecondOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.SecondOeuvre);
                chkFinition.Checked = metier.Phases.HasFlag(ChantierPhase.Finition);
                AttachPhaseCheckboxesEvents();
            }
            finally
            {
                _enCoursDeChargement = false;
            }
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
            finally
            {
                _enCoursDeChargement = false;
            }
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
                    MessageBox.Show(
                        "Impossible de passer à la configuration des prérequis :\n\n" +
                        "• Le nom du métier ne peut pas être vide\n" +
                        "• Au moins une phase doit être sélectionnée\n\n" +
                        "Veuillez corriger ces erreurs.",
                        "Métier invalide",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Passage au Concept 2
                InitialiserConcept2();
            }
            else
            {
                // CONCEPT 2 : Finaliser configuration prérequis
                SauvegarderPrerequisConcept2();
            }
        }

        private void SauvegarderPrerequisConcept2()
        {
            try
            {
                MessageBox.Show("Configuration des prérequis par phase terminée.",
                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RetournerConcept1();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la finalisation: {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Nettoyer Concept 2
            panelLeft.Controls.Clear();
            _diagrammesParPhase.Clear();
            _tabControlPhases?.Dispose();
            _tabControlPhases = null;

            // Supprimer contrôles ajoutés pour Concept 2
            RestaurerGroupBoxDetailsOriginal();

            // Restaurer Concept 1
            InitialiserConcept1();

            // Restaurer boutons et titre
            btnValider.Text = "✅ Valider";
            btnAnnuler.Text = "❌ Annuler";
            this.Text = "Gestion des Métiers";
        }

        private void RestaurerGroupBoxDetailsOriginal()
        {
            // Supprimer contrôles ajoutés pour Concept 2
            var chkListToRemove = groupBoxDetails.Controls.OfType<CheckedListBox>()
                .Where(c => c.Name == "chkListPrerequisPhase").ToList();
            foreach (var control in chkListToRemove)
            {
                groupBoxDetails.Controls.Remove(control);
                control.Dispose();
            }

            var lblToRemove = groupBoxDetails.Controls.OfType<Label>()
                .Where(c => c.Name == "lblExplicationPrerequis").ToList();
            foreach (var control in lblToRemove)
            {
                groupBoxDetails.Controls.Remove(control);
                control.Dispose();
            }

            // Restaurer l'état des contrôles originaux
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