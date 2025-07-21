using PlanAthena.Controls;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Représente l'interface principale de gestion des tâches du projet.
    /// Son rôle est de fournir un espace de travail interactif centré sur un "Lot" de travaux.
    /// Il orchestre l'affichage du diagramme PERT, la création de nouvelles activités via une barre d'outils,
    /// et l'affichage des détails d'une activité sélectionnée.
    /// </summary>
    public partial class TacheForm : System.Windows.Forms.Form
    {
        // Services
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly LotService _lotService;
        private readonly BlocService _blocService;

        // Données
        private List<Tache> _tachesBrutes = new List<Tache>();
        private Lot _lotActif = null;

        // Contrôles UI
        private readonly PertDiagramControl _pertControl;
        private readonly TacheDetailForm _tacheDetailForm; // Formulaire embarqué pour l'affichage des détails
        private readonly ToolTip _toolTipMetiers = new ToolTip();

        public TacheForm(TacheService tacheService, MetierService metierService, DependanceBuilder dependanceBuilder, LotService lotService, BlocService blocService)
        {
            InitializeComponent();
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));

            _pertControl = new PertDiagramControl();
            _pertControl.Dock = DockStyle.Fill;
            _pertControl.Initialize(_metierService, _lotService, _blocService, new PertDiagramSettings());
            _pertControl.TacheSelected += PertControl_TacheSelected;
            _pertControl.TacheDoubleClicked += PertControl_TacheDoubleClicked;

            this.panelDiagrammeCentral.Controls.Add(_pertControl);

            // Le formulaire de détail est maintenant doublement utilisé :
            // 1. Embarqué, pour un affichage rapide des détails (lecture seule).
            // 2. En popup, pour la création / modification.
            _tacheDetailForm = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService);
            IntegrerFormulaireDetails();
        }

        private void IntegrerFormulaireDetails()
        {
            _tacheDetailForm.TopLevel = false;
            _tacheDetailForm.FormBorderStyle = FormBorderStyle.None;
            _tacheDetailForm.Dock = DockStyle.Fill;
            panelDetailsTache.Controls.Add(_tacheDetailForm);
            _tacheDetailForm.Show();

            // Si une sauvegarde est faite depuis le formulaire embarqué, on rafraîchit.
            _tacheDetailForm.TacheSauvegardee += (s, e) =>
            {
                ChargerDonneesProjet();
                RafraichirAffichageDiagramme();
            };
        }

        private void TacheForm_Load(object sender, EventArgs e)
        {
            ChargerDonneesProjet();
            PeuplerComboBoxLots();
            CreerBoutonsMetiers();
            RafraichirAffichageDiagramme();
        }

        #region Gestion de la Barre d'Outils Métiers et Création

        /// <summary>
        /// Construit dynamiquement la barre d'outils latérale pour la création d'activités.
        /// Affiche les métiers triés logiquement, ainsi qu'un bouton dédié pour les jalons.
        /// </summary>
        private void CreerBoutonsMetiers()
        {
            panelOutilsMetiersDynamiques.Controls.Clear();

            var metiersTries = _metierService.ObtenirMetiersTriesParDependance()
                .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H"); // Nettoyage des anciens méta-métiers

            int yPos = 10;
            foreach (var metier in metiersTries)
            {
                var btn = new Button
                {
                    Text = metier.Nom,
                    Tag = metier,
                    Location = new System.Drawing.Point(11, yPos),
                    Size = new System.Drawing.Size(160, 30),
                    BackColor = _metierService.GetDisplayColorForMetier(metier.MetierId),
                    FlatStyle = FlatStyle.Popup
                };
                btn.Click += MetierButton_Click;

                var prerequis = _metierService.GetPrerequisForMetier(metier.MetierId);
                if (prerequis.Any())
                {
                    var prerequisNoms = prerequis.Select(id => _metierService.GetMetierById(id)?.Nom ?? id);
                    _toolTipMetiers.SetToolTip(btn, $"Prérequis: {string.Join(", ", prerequisNoms)}");
                }
                else
                {
                    _toolTipMetiers.SetToolTip(btn, "Aucun prérequis");
                }

                panelOutilsMetiersDynamiques.Controls.Add(btn);
                yPos += 35;
            }

            // Ajout d'un séparateur et du bouton de création de jalon à la fin.
            var separator = new Label { BorderStyle = BorderStyle.Fixed3D, Height = 2, Width = 160, Location = new Point(11, yPos + 5) };
            panelOutilsMetiersDynamiques.Controls.Add(separator);
            yPos += 15;

            var btnJalon = new Button
            {
                Text = "◆ Créer Jalon/Attente",
                Location = new System.Drawing.Point(11, yPos),
                Size = new System.Drawing.Size(160, 30),
                BackColor = Color.Gold,
                Font = new Font(this.Font, FontStyle.Bold)
            };
            btnJalon.Click += JalonButton_Click;
            _toolTipMetiers.SetToolTip(btnJalon, "Créer un jalon d'attente manuel (ex: séchage)");
            panelOutilsMetiersDynamiques.Controls.Add(btnJalon);
        }

        /// <summary>
        /// Gère le clic sur un bouton métier pour initier la création d'une nouvelle tâche.
        /// </summary>
        private void MetierButton_Click(object sender, EventArgs e)
        {
            if (_lotActif == null)
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot actif.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (sender is Button { Tag: Metier metier })
            {
                using var form = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService);
                var nouvelleTache = new Tache
                {
                    MetierId = metier.MetierId,
                    LotId = _lotActif.LotId,
                    TacheNom = $"Nouvelle tâche - {metier.Nom}",
                    HeuresHommeEstimees = 8
                };
                form.ChargerTache(nouvelleTache, true);

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ChargerDonneesProjet();
                    RafraichirAffichageDiagramme();
                }
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton de création de jalon.
        /// </summary>
        private void JalonButton_Click(object sender, EventArgs e)
        {
            if (_lotActif == null)
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot actif.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService);
            var nouveauJalon = new Tache
            {
                Type = TypeActivite.JalonUtilisateur,
                HeuresHommeEstimees = 24,
                TacheNom = "Nouveau Jalon d'attente",
                LotId = _lotActif.LotId
            };
            form.ChargerTache(nouveauJalon, true);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                ChargerDonneesProjet();
                RafraichirAffichageDiagramme();
            }
        }

        #endregion

        #region Reste du Code (Logique d'affichage et événements)

        private void ChargerDonneesProjet()
        {
            _tachesBrutes = _tacheService.ObtenirToutesLesTaches();
            _tacheDetailForm.MettreAJourListesDeroulantes();
        }

        private void PeuplerComboBoxLots()
        {
            var lots = _lotService.ObtenirTousLesLots();
            cmbLots.DataSource = lots;
            cmbLots.DisplayMember = "Nom";
            cmbLots.ValueMember = "LotId";

            if (lots.Any())
            {
                cmbLots.SelectedIndex = 0;
                _lotActif = cmbLots.SelectedItem as Lot;
            }
            else
            {
                _lotActif = null;
            }
        }

        private void RafraichirAffichageDiagramme()
        {
            List<Tache> tachesAffichees = new List<Tache>();
            if (_lotActif != null)
            {
                tachesAffichees = _tachesBrutes.Where(t => t.LotId == _lotActif.LotId).ToList();
            }
            _pertControl.ChargerDonnees(tachesAffichees, txtRecherche.Text);
            RafraichirStatistiques();
        }

        private void RafraichirStatistiques()
        {
            var totalTaches = _tachesBrutes.Count;
            int tachesLotActif = _lotActif != null ? _tachesBrutes.Count(t => t.LotId == _lotActif.LotId) : 0;
            lblStatistiques.Text = $"Total Projet: {totalTaches} tâches | Lot Actif '{_lotActif?.Nom ?? "Aucun"}': {tachesLotActif} tâches";
        }

        private void PertControl_TacheSelected(object sender, TacheSelectedEventArgs e)
        {
            // Un simple clic met à jour le panneau de détails embarqué pour une consultation rapide.
            _tacheDetailForm.ChargerTache(e.Tache, false);
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom}";
        }

        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            // Un double-clic ouvre une popup pour une édition focalisée.
            var tacheOriginale = _tachesBrutes.FirstOrDefault(t => t.TacheId == e.Tache.TacheId);
            if (tacheOriginale != null)
            {
                using var form = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService);
                form.ChargerTache(tacheOriginale, false);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ChargerDonneesProjet();
                    RafraichirAffichageDiagramme();
                }
            }
        }

        private void cmbLots_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLots.SelectedItem is Lot selectedLot)
            {
                _lotActif = selectedLot;
                RafraichirAffichageDiagramme();
                // Vide le panneau de détail car la sélection de tâche n'est plus pertinente
                _tacheDetailForm.ChargerTache(null, true);
                lblTacheSelectionnee.Text = "Aucune sélection";
            }
        }

        private void btnGererLots_Click(object sender, EventArgs e)
        {
            // On passe maintenant les deux services requis
            using (var form = new LotForm(_lotService, _tacheService)) { form.ShowDialog(this); }

            var idLotActif = _lotActif?.LotId;
            PeuplerComboBoxLots();
            if (idLotActif != null)
            {
                var itemToReselect = cmbLots.Items.Cast<Lot>().FirstOrDefault(l => l.LotId == idLotActif);
                if (itemToReselect != null) cmbLots.SelectedItem = itemToReselect;
            }
            RafraichirAffichageDiagramme();
        }

        private void btnGererBlocs_Click(object sender, EventArgs e)
        {
            using (var form = new BlocForm(_blocService, _tacheService)) { form.ShowDialog(this); }
            _tacheDetailForm.MettreAJourListesDeroulantes();
            RafraichirAffichageDiagramme();
        }

        private void txtRecherche_TextChanged(object sender, EventArgs e) => RafraichirAffichageDiagramme();
        private void btnZoomAjuster_Click(object sender, EventArgs e) => _pertControl.ZoomToutAjuster();
        private void btnPan_Click(object sender, EventArgs e) => _pertControl.TogglePan(btnPan.Checked);
        private void btnSauvegarderImage_Click(object sender, EventArgs e) => _pertControl.SauvegarderImage();
        private void btnImprimer_Click(object sender, EventArgs e) => _pertControl.ImprimerDiagramme();
        private void btnMappingAuto_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Cette action va (re)construire les dépendances logiques pour TOUT le projet.\n\nVoulez-vous continuer ?",
                "Initialiser les dépendances", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;
            try
            {
                var tachesActuelles = _tacheService.ObtenirToutesLesTaches();
                _dependanceBuilder.ConstruireDependancesLogiques(tachesActuelles);
                _tacheService.ChargerTaches(tachesActuelles);
                ChargerDonneesProjet();
                RafraichirAffichageDiagramme();
                MessageBox.Show("Dépendances initialisées avec succès.", "Opération terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation des dépendances :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportExcelFieldwire_Click(object sender, EventArgs e) => MessageBox.Show("L'import Excel Fieldwire/Dalux n'est pas encore implémenté.", "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnImporter_Click(object sender, EventArgs e) => MessageBox.Show("L'import/export CSV est temporairement désactivé et sera ré-implémenté dans une version future.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnExporter_Click(object sender, EventArgs e) => MessageBox.Show("L'import/export CSV est temporairement désactivé et sera ré-implémenté dans une version future.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnFermer_Click(object sender, EventArgs e) => this.Close();

        #endregion
    }
}