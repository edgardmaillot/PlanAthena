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

        public TacheForm(TacheService tacheService, MetierService metierService)
        {
            InitializeComponent();
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _csvDataService = new CsvDataService();
            _excelReader = new ExcelReader();

            // Initialiser le contrôle PERT
            _pertControl = new PertDiagramControl();
            _pertControl.Dock = DockStyle.Fill;
            _pertControl.TacheSelected += PertControl_TacheSelected;
            _pertControl.TacheDoubleClicked += PertControl_TacheDoubleClicked;

            // Remplacer le contenu du panel principal par le diagramme PERT
            panelPrincipal.Controls.Clear();
            panelPrincipal.Controls.Add(_pertControl);
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

        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            OuvrirDetailsActual(e.Tache, false);
        }

        private void OuvrirDetailsActual(TacheCsvRecord tache, bool modeCreation)
        {
            using var form = new TacheDetailForm(_tacheService, _metierService, tache, modeCreation);
            if (form.ShowDialog() == DialogResult.OK)
            {
                if (form.Modifiee)
                {
                    ChargerDonnees();
                    RafraichirAffichage();
                }
            }
        }

        #region Événements Interface

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            RafraichirAffichage();
        }

        private void btnNouvelleTache_Click(object sender, EventArgs e)
        {
            OuvrirDetailsActual(null, true);
        }

        private void btnModifierTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee != null)
            {
                OuvrirDetailsActual(tacheSelectionnee, false);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une tâche à modifier.",
                    "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSupprimerTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee == null)
            {
                MessageBox.Show("Veuillez sélectionner une tâche à supprimer.",
                    "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer la tâche '{tacheSelectionnee.TacheId}' ?\n\n" +
                $"Nom: {tacheSelectionnee.TacheNom}\n" +
                "Cette action est irréversible.",
                "Confirmation de suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _tacheService.SupprimerTache(tacheSelectionnee.TacheId);
                    ChargerDonnees();
                    RafraichirAffichage();
                    lblTacheSelectionnee.Text = "Aucune sélection";

                    MessageBox.Show("Tâche supprimée avec succès.", "Suppression",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnZoomAjuster_Click(object sender, EventArgs e)
        {
            _pertControl.ZoomToutAjuster();
        }

        private void btnMappingAuto_Click(object sender, EventArgs e)
        {
            var tachesSansMetier = _taches.Where(t => string.IsNullOrEmpty(t.MetierId)).ToList();

            if (!tachesSansMetier.Any())
            {
                MessageBox.Show("Toutes les tâches ont déjà un métier assigné.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Le mapping automatique va essayer d'assigner des métiers aux {tachesSansMetier.Count} tâches sans métier.\n\n" +
                "Cette fonction n'est pas encore implémentée complètement.\n" +
                "Voulez-vous continuer ?",
                "Mapping automatique",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show(
                    "Mapping automatique en cours de développement.\n" +
                    "Utilisez le mapping manuel (double-clic sur les tâches) pour le moment.",
                    "Fonctionnalité en développement",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Import/Export

        private void btnImporter_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                Title = "Importer les tâches depuis un fichier CSV"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Voulez-vous remplacer toutes les tâches existantes ?\n\n" +
                        "• Oui : Remplace toutes les tâches actuelles\n" +
                        "• Non : Ajoute aux tâches existantes\n" +
                        "• Annuler : Annule l'import",
                        "Mode d'import",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel) return;

                    var nombreImporte = _tacheService.ImporterDepuisCsv(ofd.FileName, result == DialogResult.Yes);
                    ChargerDonnees();
                    RafraichirAffichage();

                    MessageBox.Show(
                        $"Import terminé avec succès !\n\n" +
                        $"• {nombreImporte} tâches importées\n" +
                        $"• {_taches.Count} tâches total\n" +
                        $"• {_taches.Count(t => !string.IsNullOrEmpty(t.MetierId))} tâches avec métier",
                        "Import réussi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'import :\n{ex.Message}",
                        "Erreur d'import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportExcelFieldwire_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Tous les fichiers (*.*)|*.*",
                Title = "Importer les tâches depuis un fichier Excel Fieldwire/Dalux"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var nombreImporte = _tacheService.ImporterDepuisExcelFieldwire(ofd.FileName);

                    if (nombreImporte > 0)
                    {
                        ChargerDonnees();
                        RafraichirAffichage();

                        MessageBox.Show(
                            $"Import Excel Fieldwire terminé avec succès !\n\n" +
                            $"• {nombreImporte} tâches importées",
                            "Import réussi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "L'import Excel Fieldwire/Dalux n'est pas encore implémenté.\n" +
                            "Utilisez l'import CSV pour le moment.",
                            "Fonctionnalité en développement",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'import Excel Fieldwire :\n{ex.Message}",
                        "Erreur d'import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!_taches.Any())
            {
                MessageBox.Show("Aucune tâche à exporter.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                Title = "Exporter les tâches vers un fichier CSV",
                FileName = $"taches_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _tacheService.ExporterVersCsv(sfd.FileName);

                    var statistiques = _tacheService.ObtenirStatistiques();
                    var mappingStats = _tacheService.ObtenirStatistiquesMappingMetiers();

                    MessageBox.Show(
                        $"Export terminé avec succès !\n\n" +
                        $"• {statistiques.NombreTachesTotal} tâches exportées\n" +
                        $"• {statistiques.NombreBlocsUniques} blocs\n" +
                        $"• {statistiques.NombreLotsUniques} lots\n" +
                        $"• Mapping: {mappingStats.PourcentageMapping:F0}%\n" +
                        $"• Fichier : {Path.GetFileName(sfd.FileName)}",
                        "Export réussi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'export :\n{ex.Message}",
                        "Erreur d'export",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }
}