using PlanAthena.Controls;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Représente l'interface principale de gestion des tâches du projet.
    /// Son rôle est de fournir un espace de travail interactif centré sur un "Lot" de travaux.
    /// 
    /// PRINCIPE ARCHITECTURAL CLÉ :
    /// Ce formulaire n'embarque PAS de cache de données local (ex: _tachesBrutes).
    /// Il s'appuie systématiquement sur les services (TacheService, LotService, etc.) comme
    /// source de vérité unique. À chaque rafraîchissement, les données sont lues
    /// directement depuis le service, garantissant ainsi que l'affichage n'est jamais
    /// désynchronisé et éliminant une source majeure de bugs.
    /// </summary>
    public partial class TacheForm : System.Windows.Forms.Form
    {
        // Services (sources de vérité)
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly LotService _lotService;
        private readonly BlocService _blocService;

        // État de l'UI
        private Lot _lotActif = null;

        // Contrôles UI
        private readonly PertDiagramControl _pertControl;
        private readonly TacheDetailForm _tacheDetailForm;
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
            _pertControl.Initialize(_metierService, _lotService, _blocService, _dependanceBuilder, new PertDiagramSettings());
            _pertControl.TacheSelected += PertControl_TacheSelected;
            _pertControl.TacheDoubleClicked += PertControl_TacheDoubleClicked;

            this.panelDiagrammeCentral.Controls.Add(_pertControl);

            // Le formulaire de détail a maintenant besoin du DependanceBuilder.
            // On le crée une seule fois et on le réutilise pour éviter les fuites de mémoire.
            _tacheDetailForm = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService, _dependanceBuilder);
            IntegrerFormulaireDetails();
        }

        private void IntegrerFormulaireDetails()
        {
            _tacheDetailForm.TopLevel = false;
            _tacheDetailForm.FormBorderStyle = FormBorderStyle.None;
            _tacheDetailForm.Dock = DockStyle.Fill;
            panelDetailsTache.Controls.Add(_tacheDetailForm);
            _tacheDetailForm.Show();

            _tacheDetailForm.TacheSauvegardee += (s, e) => RafraichirDiagrammeEtStatistiques();
        }

        private void TacheForm_Load(object sender, EventArgs e)
        {
            RafraichirVueComplete();
        }

        #region Logique de Rafraîchissement

        private void RafraichirVueComplete()
        {
            var idLotActif = _lotActif?.LotId;
            PeuplerComboBoxLots();
            if (idLotActif != null)
            {
                var itemToReselect = cmbLots.Items.Cast<Lot>().FirstOrDefault(l => l.LotId == idLotActif);
                cmbLots.SelectedItem = itemToReselect;
            }
            if (cmbLots.SelectedItem == null && cmbLots.Items.Count > 0)
            {
                cmbLots.SelectedIndex = 0;
            }
            CreerBoutonsMetiers();
            _tacheDetailForm.MettreAJourListesDeroulantes();
            RafraichirDiagrammeEtStatistiques();
            _tacheDetailForm.ChargerTache(null, true);
        }

        private void RafraichirDiagrammeEtStatistiques()
        {
            var toutesLesTaches = _tacheService.ObtenirToutesLesTaches();
            List<Tache> tachesAffichees = new List<Tache>();
            if (_lotActif != null)
            {
                tachesAffichees = toutesLesTaches.Where(t => t.LotId == _lotActif.LotId).ToList();
            }
            _pertControl.ChargerDonnees(tachesAffichees, txtRecherche.Text);
            RafraichirStatistiques(toutesLesTaches);
        }

        private void PeuplerComboBoxLots()
        {
            var lots = _lotService.ObtenirTousLesLots();
            cmbLots.DataSource = null;
            cmbLots.DataSource = lots;
            cmbLots.DisplayMember = "Nom";
            cmbLots.ValueMember = "LotId";
        }

        private void RafraichirStatistiques(List<Tache> toutesLesTaches)
        {
            var totalTaches = toutesLesTaches.Count;
            int tachesLotActif = _lotActif != null ? toutesLesTaches.Count(t => t.LotId == _lotActif.LotId) : 0;
            lblStatistiques.Text = $"Total Projet: {totalTaches} tâches | Lot Actif '{_lotActif?.Nom ?? "Aucun"}': {tachesLotActif} tâches";
        }

        #endregion

        #region Gestion de la Barre d'Outils Métiers et Création

        private void CreerBoutonsMetiers()
        {
            panelOutilsMetiersDynamiques.Controls.Clear();
            var metiersTries = _metierService.ObtenirMetiersTriesParDependance()
                .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H");
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

        private void MetierButton_Click(object sender, EventArgs e)
        {
            if (_lotActif == null)
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot actif.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (sender is Button { Tag: Metier metier })
            {
                using var form = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService, _dependanceBuilder);
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
                    RafraichirDiagrammeEtStatistiques();
                }
            }
        }

        private void JalonButton_Click(object sender, EventArgs e)
        {
            if (_lotActif == null)
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot actif.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var form = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService, _dependanceBuilder);

            // Correction: La définition d'un jalon se fait uniquement via la propriété 'Type'.
            // L'affectation à 'EstJalon' a été supprimée.
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
                RafraichirDiagrammeEtStatistiques();
            }
        }

        #endregion

        #region Événements et Logique d'Affichage

        private void PertControl_TacheSelected(object sender, TacheSelectedEventArgs e)
        {
            _tacheDetailForm.ChargerTache(e.Tache, false);
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom}";
        }

        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            var tacheOriginale = _tacheService.ObtenirTacheParId(e.Tache.TacheId);
            if (tacheOriginale != null)
            {
                using var form = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService, _dependanceBuilder);
                form.ChargerTache(tacheOriginale, false);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RafraichirDiagrammeEtStatistiques();
                }
            }
        }

        private void cmbLots_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLots.SelectedItem is Lot selectedLot)
            {
                _lotActif = selectedLot;
                RafraichirDiagrammeEtStatistiques();
                _tacheDetailForm.ChargerTache(null, true);
                lblTacheSelectionnee.Text = "Aucune sélection";
            }
        }

        private void btnGererLots_Click(object sender, EventArgs e)
        {
            using (var form = new LotForm(_lotService, _tacheService)) { form.ShowDialog(this); }
            RafraichirVueComplete();
        }

        private void btnGererBlocs_Click(object sender, EventArgs e)
        {
            using (var form = new BlocForm(_blocService, _tacheService)) { form.ShowDialog(this); }
            RafraichirVueComplete();
        }

        /// <summary>
        /// FONCTIONNALITÉ DÉSACTIVÉE - HORS PÉRIMÈTRE
        /// Le mapping automatique sera implémenté dans une itération future.
        /// Pour cette phase, nous nous concentrons uniquement sur le service de suggestions.
        /// </summary>
        private void btnMappingAuto_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Le mapping automatique des dépendances sera implémenté dans une version future.\n\n" +
                           "Pour l'instant, utilisez les suggestions intelligentes dans le formulaire de détail des tâches.",
                           "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtRecherche_TextChanged(object sender, EventArgs e) => RafraichirDiagrammeEtStatistiques();
        private void btnZoomAjuster_Click(object sender, EventArgs e) => _pertControl.ZoomToutAjuster();
        private void btnPan_Click(object sender, EventArgs e) => _pertControl.TogglePan(btnPan.Checked);
        private void btnSauvegarderImage_Click(object sender, EventArgs e) => _pertControl.SauvegarderImage();
        private void btnImprimer_Click(object sender, EventArgs e) => _pertControl.ImprimerDiagramme();
        private void btnImportExcelFieldwire_Click(object sender, EventArgs e) => MessageBox.Show("L'import Excel Fieldwire/Dalux n'est pas encore implémenté.", "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnImporter_Click(object sender, EventArgs e) => MessageBox.Show("L'import/export CSV est temporairement désactivé et sera ré-implémenté dans une version future.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnExporter_Click(object sender, EventArgs e) => MessageBox.Show("L'import/export CSV est temporairement désactivé et sera ré-implémenté dans une version future.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnFermer_Click(object sender, EventArgs e) => this.Close();

        #endregion
    }
}