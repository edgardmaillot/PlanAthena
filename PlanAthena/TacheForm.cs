// Fichier : TacheForm.cs

using PlanAthena.Controls;
using PlanAthena.CsvModels;
using PlanAthena.Forms;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena
{
    public partial class TacheForm : System.Windows.Forms.Form
    {
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly CsvDataService _csvDataService;
        private readonly ExcelReader _excelReader;

        private List<TacheCsvRecord> _taches = new List<TacheCsvRecord>();
        private readonly PertDiagramControl _pertControl;
        private TacheDetailForm _tacheDetailForm; // NOUVEAU: Instance du formulaire de détails

        public TacheForm(TacheService tacheService, MetierService metierService)
        {
            InitializeComponent();
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _csvDataService = new CsvDataService();
            _excelReader = new ExcelReader();

            _pertControl = new PertDiagramControl();
            _pertControl.Dock = DockStyle.Fill;
            _pertControl.TacheSelected += PertControl_TacheSelected;
            _pertControl.TacheDoubleClicked += PertControl_TacheDoubleClicked;

            // MODIFIÉ: Placer le contrôle PERT dans le panneau de gauche du SplitContainer
            splitContainerPrincipal.Panel1.Controls.Add(_pertControl);

            // NOUVEAU: Intégrer le formulaire de détails dans le panneau de droite
            IntegrerFormulaireDetails();
        }

        // NOUVEAU: Méthode pour intégrer le formulaire de détails
        private void IntegrerFormulaireDetails()
        {
            _tacheDetailForm = new TacheDetailForm(_tacheService, _metierService);
            _tacheDetailForm.TopLevel = false;
            _tacheDetailForm.FormBorderStyle = FormBorderStyle.None;
            _tacheDetailForm.Dock = DockStyle.Fill;

            // Ajout au panneau inférieur droit
            panelDetailsTache.Controls.Add(_tacheDetailForm);
            _tacheDetailForm.Show();

            // S'abonner à l'événement de sauvegarde pour rafraîchir le diagramme
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
            _taches = _tacheService.ObtenirToutesLesTaches();
        }

        private void RafraichirAffichage()
        {
            var filtreRecherche = txtRecherche.Text;
            _pertControl.ChargerTaches(_taches, filtreRecherche);
            RafraichirStatistiques();
        }

        private void RafraichirStatistiques()
        {
            var totalTaches = _taches.Count;
            var tachesAvecMetier = _taches.Count(t => !string.IsNullOrEmpty(t.MetierId));
            var pourcentageMapping = totalTaches > 0 ? (double)tachesAvecMetier / totalTaches * 100 : 0;

            lblStatistiques.Text = $"Tâches: {totalTaches} | " +
                                  $"Blocs: {_taches.Select(t => t.BlocId).Distinct().Count()} | " +
                                  $"Avec métier: {tachesAvecMetier} ({pourcentageMapping:F0}%)";
        }

        private void PertControl_TacheSelected(object sender, TacheSelectedEventArgs e)
        {
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom}";
        }

        // MODIFIÉ: Le double-clic met à jour le panneau de détails
        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            AfficherDetailsTache(e.Tache, false);
        }

        // MODIFIÉ: La méthode met à jour le formulaire intégré au lieu d'en ouvrir un nouveau
        private void AfficherDetailsTache(TacheCsvRecord tache, bool modeCreation)
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
            // Affiche un formulaire de détails vide pour la création
            AfficherDetailsTache(null, true);
        }

        private void btnModifierTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee != null)
            {
                // Affiche les détails de la tâche sélectionnée pour modification
                AfficherDetailsTache(tacheSelectionnee, false);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une tâche à modifier.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSupprimerTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee == null)
            {
                MessageBox.Show("Veuillez sélectionner une tâche à supprimer.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    _tacheDetailForm.ChargerTache(null, true); // Vide le formulaire de détails

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
            using var ofd = new OpenFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*", Title = "Importer les tâches depuis un fichier CSV" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var result = MessageBox.Show("Voulez-vous remplacer toutes les tâches existantes ?\n\n• Oui : Remplace toutes les tâches actuelles\n• Non : Ajoute aux tâches existantes\n• Annuler : Annule l'import", "Mode d'import", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Cancel) return;

                    var nombreImporte = _tacheService.ImporterDepuisCsv(ofd.FileName, result == DialogResult.Yes);
                    ChargerDonnees();
                    RafraichirAffichage();

                    MessageBox.Show($"Import terminé avec succès !\n\n• {nombreImporte} tâches importées\n• {_taches.Count} tâches total\n• {_taches.Count(t => !string.IsNullOrEmpty(t.MetierId))} tâches avec métier", "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'import :\n{ex.Message}", "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportExcelFieldwire_Click(object sender, EventArgs e)
        {
            MessageBox.Show("L'import Excel Fieldwire/Dalux n'est pas encore implémenté.", "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!_taches.Any())
            {
                MessageBox.Show("Aucune tâche à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv", Title = "Exporter les tâches vers un fichier CSV", FileName = $"taches_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _tacheService.ExporterVersCsv(sfd.FileName);
                    var statistiques = _tacheService.ObtenirStatistiques();
                    var mappingStats = _tacheService.ObtenirStatistiquesMappingMetiers();
                    MessageBox.Show($"Export terminé avec succès !\n\n• {statistiques.NombreTachesTotal} tâches exportées\n• {statistiques.NombreBlocsUniques} blocs\n• {statistiques.NombreLotsUniques} lots\n• Mapping: {mappingStats.PourcentageMapping:F0}%\n• Fichier : {Path.GetFileName(sfd.FileName)}", "Export réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'export :\n{ex.Message}", "Erreur d'export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}