// Fichier : TacheForm.cs

using PlanAthena.Controls;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class TacheForm : System.Windows.Forms.Form
    {
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly DependanceBuilder _dependanceBuilder;

        private List<Tache> _tachesBrutes = new List<Tache>();
        private List<Metier> _metiers = new List<Metier>();
        private readonly PertDiagramControl _pertControl;
        private TacheDetailForm _tacheDetailForm;

        public TacheForm(TacheService tacheService, MetierService metierService, DependanceBuilder dependanceBuilder)
        {
            InitializeComponent();
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));

            _pertControl = new PertDiagramControl();
            _pertControl.Dock = DockStyle.Fill;
            _pertControl.Initialize(_metierService, new PertDiagramSettings());
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
                // Après une sauvegarde, on recharge tout pour assurer la cohérence.
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
            _tacheDetailForm.MettreAJourListesDeroulantes();
        }

        private void RafraichirAffichage()
        {
            // L'affichage est direct. On passe la liste de tâches brute au contrôle.
            // Toute transformation (création de dépendances/jalons) a déjà eu lieu
            // en amont via le DependanceBuilder sur action utilisateur.
            var filtreRecherche = txtRecherche.Text;
            _pertControl.ChargerDonnees(_tachesBrutes, filtreRecherche);

            RafraichirStatistiques();
        }

        private void RafraichirStatistiques()
        {
            var totalTaches = _tachesBrutes.Count;
            var tachesAvecMetier = _tachesBrutes.Count(t => !string.IsNullOrEmpty(t.MetierId));
            var pourcentageMapping = totalTaches > 0 ? (double)tachesAvecMetier / totalTaches * 100 : 0;
            var jalons = _tachesBrutes.Count(t => t.EstJalon);

            lblStatistiques.Text = $"Total: {totalTaches} activités ({jalons} jalons) | " +
                                  $"Blocs: {_tachesBrutes.Select(t => t.BlocId).Distinct().Count()} | " +
                                  $"Avec métier: {tachesAvecMetier} ({pourcentageMapping:F0}%)";
        }

        private void PertControl_TacheSelected(object sender, TacheSelectedEventArgs e)
        {
            var typeAffichage = e.Tache.EstJalon ? "Jalon" : "Tâche";
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom} [{typeAffichage}]";

            if (!string.IsNullOrEmpty(e.Tache.Dependencies))
            {
                lblTacheSelectionnee.Text += $" | Dépend de: {e.Tache.Dependencies}";
            }
        }

        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            var tacheOriginale = _tachesBrutes.FirstOrDefault(t => t.TacheId == e.Tache.TacheId);
            if (tacheOriginale != null)
            {
                AfficherDetailsTache(tacheOriginale, false);
            }
            else
            {
                MessageBox.Show($"L'activité '{e.Tache.TacheId}' est une tâche technique ou un artefact visuel qui ne peut pas être modifié directement.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AfficherDetailsTache(Tache tache, bool modeCreation)
        {
            _tacheDetailForm.ChargerTache(tache, modeCreation);
        }

        #region Événements Interface (CRUD, etc.)

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            RafraichirAffichage();
        }

        private void btnNouvelleTache_Click(object sender, EventArgs e)
        {
            AfficherDetailsTache(null, true);
        }

        private void btnNouveauJalon_Click(object sender, EventArgs e)
        {
            var nouveauJalon = new Tache
            {
                Type = TypeActivite.JalonUtilisateur,
                HeuresHommeEstimees = 0,
                TacheNom = "Nouveau Jalon"
            };

            var lots = _tacheService.ObtenirTousLesLots();
            if (lots.Any())
            {
                var premierLot = lots.First();
                nouveauJalon.LotId = premierLot.LotId;
                nouveauJalon.LotNom = premierLot.LotNom;
                nouveauJalon.LotPriorite = premierLot.Priorite;
            }
            var blocs = _tacheService.ObtenirTousLesBlocs();
            if (blocs.Any())
            {
                var premierBloc = blocs.First();
                nouveauJalon.BlocId = premierBloc.BlocId;
                nouveauJalon.BlocNom = premierBloc.BlocNom;
                nouveauJalon.BlocCapaciteMaxOuvriers = premierBloc.CapaciteMaxOuvriers;
            }

            AfficherDetailsTache(nouveauJalon, true);
        }

        private void btnModifierTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee != null)
            {
                var tacheOriginale = _tachesBrutes.FirstOrDefault(t => t.TacheId == tacheSelectionnee.TacheId);
                AfficherDetailsTache(tacheOriginale ?? tacheSelectionnee, false);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une activité à modifier.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSupprimerTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee == null)
            {
                MessageBox.Show("Veuillez sélectionner une activité à supprimer.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var typeElement = tacheSelectionnee.EstJalon ? "jalon" : "tâche";
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer {typeElement} '{tacheSelectionnee.TacheId}' ?\n\nCette action est irréversible.",
                "Confirmation de suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _tacheService.SupprimerTache(tacheSelectionnee.TacheId);
                    ChargerDonnees();
                    RafraichirAffichage();
                    lblTacheSelectionnee.Text = "Aucune sélection";
                    AfficherDetailsTache(null, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Actions de la barre d'outils (Zoom, Pan, etc.)
        private void btnZoomAjuster_Click(object sender, EventArgs e) => _pertControl.ZoomToutAjuster();
        private void btnPan_Click(object sender, EventArgs e) => _pertControl.TogglePan(btnPan.Checked);
        private void btnSauvegarderImage_Click(object sender, EventArgs e) => _pertControl.SauvegarderImage();
        private void btnImprimer_Click(object sender, EventArgs e) => _pertControl.ImprimerDiagramme();
        #endregion

        #region Actions de logique métier et Import/Export

        private void btnMappingAuto_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Cette action va (re)construire les dépendances logiques et créer les jalons de synchronisation nécessaires.\n\nElle est recommandée après un import brut ou des modifications majeures.\nVoulez-vous continuer ?",
                "Initialiser les dépendances", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // === LE FLUX DE DONNÉES CORRECT ===
                    // 1. On récupère TOUJOURS la dernière version des données depuis la source de vérité.
                    var tachesActuelles = _tacheService.ObtenirToutesLesTaches();

                    // 2. On applique le traitement sur cette liste fraîche.
                    _dependanceBuilder.ConstruireDependancesLogiques(tachesActuelles, _metierService);

                    // 3. On met à jour la source de vérité avec le résultat.
                    _tacheService.ChargerTaches(tachesActuelles);

                    // 4. On rafraîchit l'UI, qui va elle-même relire la source de vérité.
                    ChargerDonnees();
                    RafraichirAffichage();

                    MessageBox.Show("Dépendances initialisées et graphe simplifié avec succès.", "Opération terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'initialisation des dépendances :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportExcelFieldwire_Click(object sender, EventArgs e)
        {
            MessageBox.Show("L'import Excel Fieldwire/Dalux n'est pas encore implémenté.",
                "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnImporter_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv", Title = "Importer les tâches" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var result = MessageBox.Show("Voulez-vous remplacer toutes les tâches existantes ?", "Mode d'import", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel) return;

                var nombreImporte = _tacheService.ImporterDepuisCsv(ofd.FileName, result == DialogResult.Yes);

                // === LE FLUX DE DONNÉES CORRECT ===
                // 1. On récupère la liste fraîchement importée depuis la source de vérité.
                var toutesLesTaches = _tacheService.ObtenirToutesLesTaches();

                // 2. On applique le traitement.
                _dependanceBuilder.ConstruireDependancesLogiques(toutesLesTaches, _metierService);

                // 3. On met à jour la source de vérité.
                _tacheService.ChargerTaches(toutesLesTaches);

                MessageBox.Show($"{nombreImporte} tâches importées et dépendances initialisées. L'interface va être rafraîchie.", "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 4. On rafraîchit l'UI.
                ChargerDonnees();
                AfficherDetailsTache(null, true);
                RafraichirAffichage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'import : {ex.Message}", "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void btnFermer_Click(object sender, EventArgs e) => this.Close();

        #endregion
    }
}