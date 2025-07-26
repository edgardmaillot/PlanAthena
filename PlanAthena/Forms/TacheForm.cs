using PlanAthena.Controls;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
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

        // AJOUT : ImportOrchestrationService pour l'import
        private ImportOrchestrationService _importOrchestrationService;

        // État de l'UI
        private Lot _lotActif = null;

        // Contrôles UI
        private readonly PertDiagramControl _pertControl;
        private readonly TacheDetailForm _tacheDetailForm;
        private readonly ToolTip _toolTipMetiers = new ToolTip();

        // CONSTRUCTEUR CORRIGÉ - ProjetService sera injecté plus tard
        public TacheForm(TacheService tacheService, MetierService metierService, DependanceBuilder dependanceBuilder, LotService lotService, BlocService blocService)
        {
            InitializeComponent();

            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));

            try
            {
                // Créer le service d'import ici
                var importService = ImportServiceConfig.CreerImportService(_tacheService, _lotService, _blocService, _metierService);
                _importOrchestrationService = ImportServiceConfig.CreerProjetService(importService);

                _pertControl = new PertDiagramControl();
                _pertControl.Dock = DockStyle.Fill;
                _pertControl.Initialize(_metierService, _lotService, _blocService, _dependanceBuilder, new PertDiagramSettings());

                // Événements existants
                _pertControl.TacheSelected += PertControl_TacheSelected;
                _pertControl.TacheDoubleClicked += PertControl_TacheDoubleClicked;

                // NOUVEAU : Abonnement à l'événement de changement de zoom
                _pertControl.ZoomChanged += PertControl_ZoomChanged;

                this.panelDiagrammeCentral.Controls.Add(_pertControl);

                _tacheDetailForm = new TacheDetailForm(_tacheService, _metierService, _lotService, _blocService, _dependanceBuilder);
                IntegrerFormulaireDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation du formulaire :\n{ex.Message}",
                              "Erreur d'initialisation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-lancer l'exception car l'initialisation a échoué
            }
        }

        // NOUVEAU : Gestionnaire d'événement pour les changements de zoom
        private void PertControl_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try
            {
                // Mettre à jour les statistiques quand le zoom change
                RafraichirStatistiques(_tacheService.ObtenirToutesLesTaches());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des statistiques de zoom: {ex.Message}");
            }
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
            try
            {
                RafraichirVueComplete();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du formulaire :\n{ex.Message}",
                              "Erreur de chargement", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Logique de Rafraîchissement

        private void RafraichirVueComplete()
        {
            try
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
                _tacheDetailForm?.MettreAJourListesDeroulantes();
                RafraichirDiagrammeEtStatistiques();
                _tacheDetailForm?.ChargerTache(null, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du rafraîchissement de la vue :\n{ex.Message}",
                              "Erreur de rafraîchissement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RafraichirDiagrammeEtStatistiques()
        {
            try
            {
                var toutesLesTaches = _tacheService.ObtenirToutesLesTaches();
                List<Tache> tachesAffichees = new List<Tache>();

                if (_lotActif != null)
                {
                    tachesAffichees = toutesLesTaches.Where(t => t.LotId == _lotActif.LotId).ToList();
                }

                _pertControl?.ChargerDonnees(tachesAffichees, txtRecherche.Text);
                RafraichirStatistiques(toutesLesTaches);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du rafraîchissement du diagramme: {ex.Message}");
                RafraichirStatistiques(new List<Tache>()); // Afficher au moins les statistiques vides
            }
        }

        private void PeuplerComboBoxLots()
        {
            try
            {
                var lots = _lotService.ObtenirTousLesLots();
                cmbLots.DataSource = null;
                cmbLots.DataSource = lots;
                cmbLots.DisplayMember = "Nom";
                cmbLots.ValueMember = "LotId";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du peuplement des lots: {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des lots :\n{ex.Message}",
                              "Erreur de chargement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RafraichirStatistiques(List<Tache> toutesLesTaches)
        {
            try
            {
                var totalTaches = toutesLesTaches?.Count ?? 0;
                int tachesLotActif = 0;

                if (_lotActif != null && toutesLesTaches != null)
                {
                    tachesLotActif = toutesLesTaches.Count(t => t.LotId == _lotActif.LotId);
                }

                // NOUVEAU : Inclure le zoom dans les statistiques
                var zoomPourcentage = Math.Round((_pertControl?.ZoomFacteur ?? 1.0) * 100, 1);

                lblStatistiques.Text = $"Total: {totalTaches} tâches | " +
                                     $"Lot '{_lotActif?.Nom ?? "Aucun"}': {tachesLotActif} tâches | " +
                                     $"Zoom: {zoomPourcentage}%";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du rafraîchissement des statistiques: {ex.Message}");
                lblStatistiques.Text = "Erreur lors du calcul des statistiques";
            }
        }

        #endregion

        #region Gestion de la Barre d'Outils Métiers et Création

        private void CreerBoutonsMetiers()
        {
            try
            {
                panelOutilsMetiersDynamiques.Controls.Clear();

                var metiersTries = _metierService.ObtenirMetiersTriesParDependance()
                    .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H");

                int yPos = 10;

                foreach (var metier in metiersTries)
                {
                    try
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
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur lors de la création du bouton pour {metier.Nom}: {ex.Message}");
                        continue; // Continuer avec les autres métiers
                    }
                }

                // Séparateur et bouton jalon
                var separator = new Label
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Height = 2,
                    Width = 160,
                    Location = new Point(11, yPos + 5)
                };
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la création des boutons métiers: {ex.Message}");
                MessageBox.Show($"Erreur lors de la création des outils métiers :\n{ex.Message}",
                              "Erreur d'interface", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            try
            {
                if (cmbLots.SelectedItem is Lot selectedLot)
                {
                    _lotActif = selectedLot;
                    RafraichirDiagrammeEtStatistiques();
                    _tacheDetailForm?.ChargerTache(null, true);
                    lblTacheSelectionnee.Text = "Aucune sélection";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du changement de lot: {ex.Message}");
                MessageBox.Show($"Erreur lors du changement de lot :\n{ex.Message}",
                              "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnGererLots_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new LotForm(_lotService, _tacheService))
                {
                    form.ShowDialog(this);
                }
                RafraichirVueComplete();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture de la gestion des lots :\n{ex.Message}",
                              "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGererBlocs_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new BlocForm(_blocService, _tacheService))
                {
                    form.ShowDialog(this);
                }
                RafraichirVueComplete();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture de la gestion des blocs :\n{ex.Message}",
                              "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            try
            {
                RafraichirDiagrammeEtStatistiques();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la recherche: {ex.Message}");
                // Ne pas afficher de MessageBox car cet événement se déclenche souvent
            }
        }

        private void btnZoomAjuster_Click(object sender, EventArgs e)
        {
            try
            {
                _pertControl?.ZoomToutAjuster();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajustement du zoom :\n{ex.Message}",
                              "Erreur de zoom", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPan_Click(object sender, EventArgs e)
        {
            try
            {
                _pertControl?.TogglePan(btnPan.Checked);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'activation du pan: {ex.Message}");
                btnPan.Checked = false; // Reset en cas d'erreur
            }
        }

        private void btnSauvegarderImage_Click(object sender, EventArgs e)
        {
            try
            {
                _pertControl?.SauvegarderImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}",
                              "Erreur de sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImprimer_Click(object sender, EventArgs e)
        {
            try
            {
                _pertControl?.ImprimerDiagramme();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'impression :\n{ex.Message}",
                              "Erreur d'impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportExcelFieldwire_Click(object sender, EventArgs e) => MessageBox.Show("L'import Excel Fieldwire/Dalux n'est pas encore implémenté.", "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void btnImporter_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Vérifier qu'un lot est sélectionné
                if (_lotActif == null)
                {
                    MessageBox.Show("Veuillez d'abord sélectionner un lot de destination avant d'importer des tâches.",
                                  "Lot requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Sélection du fichier CSV
                using var openFileDialog = new OpenFileDialog
                {
                    Title = "Sélectionner le fichier CSV à importer",
                    Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                    DefaultExt = "csv",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                // 3. Première tentative d'import (peut demander confirmation)
                ExecuterImportCSV(openFileDialog.FileName, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'import: {ex.Message}",
                              "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Execute l'import CSV avec gestion de la confirmation d'écrasement
        /// </summary>
        private void ExecuterImportCSV(string filePath, bool confirmerEcrasement)
        {
            try
            {
                // Appeler le service d'orchestration
                var resultat = _importOrchestrationService.ImporterTachesDepuisCsv(filePath, _lotActif.LotId, confirmerEcrasement);

                if (resultat.ConfirmationRequise)
                {
                    // Demander confirmation à l'utilisateur
                    var dialogResult = MessageBox.Show(
                        resultat.MessageConfirmation,
                        "Confirmation d'import",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);

                    if (dialogResult == DialogResult.OK)
                    {
                        // Relancer l'import avec confirmation
                        ExecuterImportCSV(filePath, true);
                    }
                    return;
                }

                if (resultat.EstSucces)
                {
                    // Afficher le rapport de succès
                    var rapport = $"🎉 IMPORT RÉUSSI\n==================\n";
                    rapport += $"• {resultat.NbTachesImportees} tâche(s) importée(s)\n";
                    rapport += $"• {resultat.NbLotsTraites} lot(s) traité(s)\n";
                    rapport += $"• {resultat.NbBlocsTraites} bloc(s) traité(s)\n";
                    rapport += $"• Durée: {resultat.DureeImport.TotalSeconds:F1}s\n";

                    if (resultat.Warnings.Any())
                    {
                        rapport += "\n⚠️ AVERTISSEMENTS:\n";
                        foreach (var warning in resultat.Warnings)
                        {
                            rapport += $"• {warning}\n";
                        }
                    }

                    MessageBox.Show(rapport, "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Rafraîchir l'interface
                    RafraichirVueComplete();
                }
                else
                {
                    // Afficher l'erreur
                    MessageBox.Show($"Échec de l'import:\n\n{resultat.MessageErreur}",
                                  "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur inattendue lors de l'import:\n\n{ex.Message}",
                              "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExporter_Click(object sender, EventArgs e) => MessageBox.Show("L'import/export CSV est temporairement désactivé et sera ré-implémenté dans une version future.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnFermer_Click(object sender, EventArgs e) => this.Close();

        #endregion
    }
}