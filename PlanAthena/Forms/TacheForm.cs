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
        private readonly DependanceService _dependanceService;

        private List<TacheRecord> _tachesBrutes = new List<TacheRecord>();
        private List<TacheRecord> _tachesOptimisees = new List<TacheRecord>();
        private readonly PertDiagramControl _pertControl;
        private TacheDetailForm _tacheDetailForm;

        public TacheForm(TacheService tacheService, MetierService metierService, DependanceService dependanceService)
        {
            InitializeComponent();
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _dependanceService = dependanceService ?? throw new ArgumentNullException(nameof(dependanceService));

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

        private void RafraichirAffichage()
        {
            try
            {
                // Étape 1: Calculer les dépendances complètes et optimisées
                _tachesOptimisees = _dependanceService.CalculerDependancesMetier(_tachesBrutes);

                // Étape 2: Vérifier les cycles (optionnel - pour debuggage)
                var cycles = _dependanceService.DetecterCyclesDependances(_tachesOptimisees);
                if (cycles.Any())
                {
                    MessageBox.Show($"Attention: Cycles détectés dans les dépendances:\n{string.Join("\n", cycles)}",
                        "Cycles de dépendances", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Étape 3: Charger les tâches optimisées dans le diagramme
                var filtreRecherche = txtRecherche.Text;
                _pertControl.ChargerTaches(_tachesOptimisees, filtreRecherche);

                // Étape 4: Mettre à jour les statistiques
                RafraichirStatistiques();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du rafraîchissement de l'affichage:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // En cas d'erreur, fallback sur les données brutes
                var filtreRecherche = txtRecherche.Text;
                _pertControl.ChargerTaches(_tachesBrutes, filtreRecherche);
                RafraichirStatistiquesSimples();
            }
        }

        private void RafraichirStatistiques()
        {
            var totalTaches = _tachesBrutes.Count;
            var tachesAvecMetier = _tachesBrutes.Count(t => !string.IsNullOrEmpty(t.MetierId));
            var pourcentageMapping = totalTaches > 0 ? (double)tachesAvecMetier / totalTaches * 100 : 0;

            // Statistiques sur les dépendances
            var statsDependances = _dependanceService.ObtenirStatistiques(_tachesBrutes, _tachesOptimisees);

            lblStatistiques.Text = $"Tâches: {totalTaches} | " +
                                  $"Blocs: {_tachesBrutes.Select(t => t.BlocId).Distinct().Count()} | " +
                                  $"Avec métier: {tachesAvecMetier} ({pourcentageMapping:F0}%) | " +
                                  $"Dépendances: {statsDependances.NombreDependancesOptimisees} " +
                                  $"(réduit de {statsDependances.NombreDependancesSupprimees})";
        }

        private void RafraichirStatistiquesSimples()
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
            // Afficher les informations de la tâche sélectionnée
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom}";

            // Optionnel: Afficher les détails des dépendances dans le tooltip ou status
            if (!string.IsNullOrEmpty(e.Tache.Dependencies))
            {
                var dependances = e.Tache.Dependencies.Split(',').Select(d => d.Trim()).ToList();
                lblTacheSelectionnee.Text += $" | Dépend de: {string.Join(", ", dependances)}";
            }
        }

        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            // Rechercher la tâche originale (brute) pour l'édition
            var tacheOriginale = _tachesBrutes.FirstOrDefault(t => t.TacheId == e.Tache.TacheId);
            if (tacheOriginale != null)
            {
                AfficherDetailsTache(tacheOriginale, false);
            }
            else
            {
                MessageBox.Show("Impossible de trouver la tâche originale pour l'édition.",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            if (tacheSelectionnee != null)
            {
                var tacheOriginale = _tachesBrutes.FirstOrDefault(t => t.TacheId == tacheSelectionnee.TacheId);
                if (tacheOriginale != null)
                {
                    AfficherDetailsTache(tacheOriginale, false);
                }
                else
                {
                    MessageBox.Show("Impossible de trouver la tâche originale pour l'édition.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
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

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la tâche '{tacheSelectionnee.TacheId}' ?\n\nNom: {tacheSelectionnee.TacheNom}\nCette action est irréversible.",
                "Confirmation de suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
            MessageBox.Show("La fonction de mapping automatique est en cours de développement.",
                "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void btnAnalyserDependances_Click(object sender, EventArgs e)
        {
            try
            {
                var cycles = _dependanceService.DetecterCyclesDependances(_tachesOptimisees);
                var statsDependances = _dependanceService.ObtenirStatistiques(_tachesBrutes, _tachesOptimisees);

                var message = $"Analyse des dépendances:\n\n" +
                             $"• Dépendances originales: {statsDependances.NombreDependancesOriginales}\n" +
                             $"• Dépendances optimisées: {statsDependances.NombreDependancesOptimisees}\n" +
                             $"• Dépendances supprimées: {statsDependances.NombreDependancesSupprimees}\n" +
                             $"• Réduction: {statsDependances.PourcentageReduction:F1}%\n\n";

                if (cycles.Any())
                {
                    message += $"⚠️ CYCLES DÉTECTÉS:\n{string.Join("\n", cycles)}";
                }
                else
                {
                    message += "✅ Aucun cycle détecté";
                }

                MessageBox.Show(message, "Analyse des dépendances", MessageBoxButtons.OK,
                    cycles.Any() ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'analyse des dépendances:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    var result = MessageBox.Show("Voulez-vous remplacer toutes les tâches existantes ?",
                        "Mode d'import", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
            MessageBox.Show("L'import Excel Fieldwire/Dalux n'est pas encore implémenté.",
                "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!_tachesBrutes.Any())
            {
                MessageBox.Show("Aucune tâche à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var sfd = new SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                Title = "Exporter les tâches",
                FileName = $"taches_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
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