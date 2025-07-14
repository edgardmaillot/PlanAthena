// Fichier : Forms/TacheForm.cs

using PlanAthena.Controls;
using PlanAthena.Data;
using PlanAthena.Forms;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Processing;


namespace PlanAthena.Forms
{
    public partial class TacheForm : System.Windows.Forms.Form
    {
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly DependanceService _dependanceService; // NOUVEAU
        //private readonly CsvDataService _csvDataService;

        private List<TacheRecord> _tachesBrutes = new List<TacheRecord>();
        private readonly PertDiagramControl _pertControl;
        private TacheDetailForm _tacheDetailForm;

        // MODIFIÉ: Le constructeur accepte maintenant DependanceService
        public TacheForm(TacheService tacheService, MetierService metierService, DependanceService dependanceService)
        {
            InitializeComponent();
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _dependanceService = dependanceService ?? throw new ArgumentNullException(nameof(dependanceService)); // NOUVEAU
            //_csvDataService = new CsvDataService();

            _pertControl = new PertDiagramControl();
            _pertControl.Dock = DockStyle.Fill;
            _pertControl.TacheSelected += PertControl_TacheSelected;
            _pertControl.TacheDoubleClicked += PertControl_TacheDoubleClicked;

            splitContainerPrincipal.Panel1.Controls.Add(_pertControl);
            IntegrerFormulaireDetails();
        }

        private void IntegrerFormulaireDetails()
        {
            _tacheDetailForm = new TacheDetailForm(_tacheService, _metierService);
            _tacheDetailForm.TopLevel = false;
            _tacheDetailForm.FormBorderStyle = FormBorderStyle.None;
            _tacheDetailForm.Dock = DockStyle.Fill;
            panelDetailsTache.Controls.Add(_tacheDetailForm);
            _tacheDetailForm.Show();
            _tacheDetailForm.TacheSauvegardee += (s, e) =>
            {
                ChargerDonnees();
                RafraichirAffichage();
            };
        }

        private void TacheForm_Load(object sender, EventArgs e)
        {
            ChargerDonnees();
            RafraichirAffichage();
        }

        private void ChargerDonnees()
        {
            _tachesBrutes = _tacheService.ObtenirToutesLesTaches();
        }

        // MODIFIÉ: La méthode utilise maintenant le DependanceService
        private void RafraichirAffichage()
        {
            // Étape 1: Calculer les dépendances complètes (incluant les tâches fictives)
            var tachesPourAffichage = _dependanceService.CalculerDependancesMetier(_tachesBrutes);

            // Étape 2: Charger ces tâches enrichies dans le diagramme
            var filtreRecherche = txtRecherche.Text;
            _pertControl.ChargerTaches(tachesPourAffichage, filtreRecherche);

            // Étape 3: Mettre à jour les statistiques en se basant sur les données brutes
            RafraichirStatistiques();
        }

        private void RafraichirStatistiques()
        {
            var totalTaches = _tachesBrutes.Count;
            var tachesAvecMetier = _tachesBrutes.Count(t => !string.IsNullOrEmpty(t.MetierId));
            var pourcentageMapping = totalTaches > 0 ? (double)tachesAvecMetier / totalTaches * 100 : 0;

            lblStatistiques.Text = $"Tâches: {totalTaches} | " +
                                  $"Blocs: {_tachesBrutes.Select(t => t.BlocId).Distinct().Count()} | " +
                                  $"Avec métier: {tachesAvecMetier} ({pourcentageMapping:F0}%)";
        }

        private void PertControl_TacheSelected(object sender, TacheSelectedEventArgs e)
        {
            // Ne pas afficher les détails pour les tâches de synchronisation
            if (e.Tache.TacheId.StartsWith("Sync_"))
            {
                lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheNom}";
                return;
            }
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom}";
        }

        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            // Ne pas permettre l'édition des tâches de synchronisation
            if (e.Tache.TacheId.StartsWith("Sync_"))
            {
                MessageBox.Show("Les tâches de synchronisation ne peuvent pas être modifiées.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // On recherche la tâche originale (non découpée) pour l'édition
            var tacheOriginaleId = e.Tache.TacheId.Split(new[] { "_split_" }, StringSplitOptions.None)[0];
            var tacheOriginale = _tachesBrutes.FirstOrDefault(t => t.TacheId == tacheOriginaleId);

            AfficherDetailsTache(tacheOriginale ?? e.Tache, false);
        }

        private void AfficherDetailsTache(TacheRecord tache, bool modeCreation)
        {
            _tacheDetailForm.ChargerTache(tache, modeCreation);
        }

        #region Événements Interface

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            RafraichirAffichage();
        }

        private void btnNouvelleTache_Click(object sender, EventArgs e)
        {
            AfficherDetailsTache(null, true);
        }

        private void btnModifierTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee != null && !tacheSelectionnee.TacheId.StartsWith("Sync_"))
            {
                var tacheOriginaleId = tacheSelectionnee.TacheId.Split(new[] { "_split_" }, StringSplitOptions.None)[0];
                var tacheOriginale = _tachesBrutes.FirstOrDefault(t => t.TacheId == tacheOriginaleId);
                AfficherDetailsTache(tacheOriginale ?? tacheSelectionnee, false);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une tâche modifiable.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSupprimerTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee == null || tacheSelectionnee.TacheId.StartsWith("Sync_"))
            {
                MessageBox.Show("Veuillez sélectionner une tâche à supprimer (les tâches de synchronisation ne peuvent pas être supprimées).", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la tâche '{tacheSelectionnee.TacheId}' ?\n\nNom: {tacheSelectionnee.TacheNom}\nCette action est irréversible.", "Confirmation de suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _tacheService.SupprimerTache(tacheSelectionnee.TacheId);
                    ChargerDonnees();
                    RafraichirAffichage();
                    lblTacheSelectionnee.Text = "Aucune sélection";
                    _tacheDetailForm.ChargerTache(null, true);
                    MessageBox.Show("Tâche supprimée avec succès.", "Suppression", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnZoomAjuster_Click(object sender, EventArgs e)
        {
            _pertControl.ZoomToutAjuster();
        }

        private void btnMappingAuto_Click(object sender, EventArgs e)
        {
            MessageBox.Show("La fonction de mapping automatique est en cours de développement.", "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnPan_Click(object sender, EventArgs e)
        {
            _pertControl.TogglePan(btnPan.Checked);
        }

        private void btnSauvegarderImage_Click(object sender, EventArgs e)
        {
            _pertControl.SauvegarderImage();
        }

        private void btnImprimer_Click(object sender, EventArgs e)
        {
            _pertControl.ImprimerDiagramme();
        }

        #endregion

        #region Import/Export

        private void btnImporter_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv", Title = "Importer les tâches" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var result = MessageBox.Show("Voulez-vous remplacer toutes les tâches existantes ?", "Mode d'import", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    var nombreImporte = _tacheService.ImporterDepuisCsv(ofd.FileName, result == DialogResult.Yes);
                    ChargerDonnees();
                    RafraichirAffichage();
                    MessageBox.Show($"{nombreImporte} tâches importées.", "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'import : {ex.Message}", "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportExcelFieldwire_Click(object sender, EventArgs e)
        {
            MessageBox.Show("L'import Excel Fieldwire/Dalux n'est pas encore implémenté.", "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!_tachesBrutes.Any())
            {
                MessageBox.Show("Aucune tâche à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var sfd = new SaveFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv", Title = "Exporter les tâches", FileName = $"taches_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _tacheService.ExporterVersCsv(sfd.FileName);
                    MessageBox.Show("Export réussi !", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'export : {ex.Message}", "Erreur d'export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}