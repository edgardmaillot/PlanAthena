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
        private readonly DecoupageTachesService _decoupageTachesService;

        private List<TacheRecord> _tachesBrutes = new List<TacheRecord>();
        private List<MetierRecord> _metiers = new List<MetierRecord>();
        private readonly PertDiagramControl _pertControl;
        private TacheDetailForm _tacheDetailForm;

        public TacheForm(TacheService tacheService, MetierService metierService, DecoupageTachesService decoupageTachesService)
        {
            InitializeComponent();
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _decoupageTachesService = decoupageTachesService ?? throw new ArgumentNullException(nameof(decoupageTachesService));

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
                System.Diagnostics.Debug.WriteLine("[TacheForm] Tâche sauvegardée - Rafraîchissement");
                ChargerDonnees();
                RafraichirAffichage();
            };
        }

        private void TacheForm_Load(object sender, EventArgs e)
        {
            ChargerDonnees();
            RafraichirAffichage();
        }

        /// <summary>
        /// Charge les données depuis la source de vérité (TacheService)
        /// </summary>
        private void ChargerDonnees()
        {
            var ancienCount = _tachesBrutes.Count;
            _tachesBrutes = _tacheService.ObtenirToutesLesTaches();
            _metiers = _metierService.GetAllMetiers().ToList();

            System.Diagnostics.Debug.WriteLine($"[TacheForm] ChargerDonnees: {ancienCount} -> {_tachesBrutes.Count} tâches");
        }

        /// <summary>
        /// AFFICHAGE SIMPLIFIÉ: Montre directement les décisions du chef
        /// </summary>
        private void RafraichirAffichage()
        {
            try
            {
                // Traiter pour IHM avec gestion des surcharges
                var tachesPourIHM = _decoupageTachesService.TraiterPourIHM(_tachesBrutes);

                var filtreRecherche = txtRecherche.Text;
                _pertControl.ChargerDonnees(tachesPourIHM, _metiers, filtreRecherche, _metierService);

                // Mettre à jour les statistiques
                RafraichirStatistiques();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du rafraîchissement de l'affichage:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Fallback: Afficher les données brutes
                var filtreRecherche = txtRecherche.Text;
                _pertControl.ChargerDonnees(_tachesBrutes, _metiers, filtreRecherche, _metierService);
                RafraichirStatistiquesSimples();
            }
        }

        /// <summary>
        /// Statistiques combinées chef + solveur
        /// </summary>
        private void RafraichirStatistiques()
        {
            var totalTachesBrutes = _tachesBrutes.Count;
            var tachesAvecMetier = _tachesBrutes.Count(t => !string.IsNullOrEmpty(t.MetierId));
            var pourcentageMapping = totalTachesBrutes > 0 ? (double)tachesAvecMetier / totalTachesBrutes * 100 : 0;
            var jalonsUtilisateur = _tachesBrutes.Count(t => _metierService.EstJalon(t));

            try
            {
                // Obtenir les tâches pour IHM (avec jalons de sync)
                var tachesPourIHM = _decoupageTachesService.TraiterPourIHM(_tachesBrutes);
                var jalonsSync = tachesPourIHM.Count(t => _metierService.EstJalon(t)) - jalonsUtilisateur;

                // Obtenir les stats de préparation solveur
                var tachesSolveur = _decoupageTachesService.PreparerPourSolveur(_tachesBrutes);
                var statsDecoupage = _decoupageTachesService.ObtenirStatistiques(_tachesBrutes, tachesSolveur);

                lblStatistiques.Text = $"Chef: {totalTachesBrutes} tâches ({jalonsUtilisateur} jalons) | " +
                                      $"IHM: {tachesPourIHM.Count} (+{jalonsSync} jalons sync) | " +
                                      $"Solveur: {tachesSolveur.Count} " +
                                      $"({statsDecoupage.TachesLonguesDecoupees} découpées, " +
                                      $"{statsDecoupage.JalonsTechniquesCreees} jalons tech.) | " +
                                      $"Blocs: {_tachesBrutes.Select(t => t.BlocId).Distinct().Count()} | " +
                                      $"Avec métier: {tachesAvecMetier} ({pourcentageMapping:F0}%)";
            }
            catch (Exception)
            {
                RafraichirStatistiquesSimples();
            }
        }

        /// <summary>
        /// Statistiques de base en cas d'erreur
        /// </summary>
        private void RafraichirStatistiquesSimples()
        {
            var totalTaches = _tachesBrutes.Count;
            var tachesAvecMetier = _tachesBrutes.Count(t => !string.IsNullOrEmpty(t.MetierId));
            var pourcentageMapping = totalTaches > 0 ? (double)tachesAvecMetier / totalTaches * 100 : 0;
            var jalonsUtilisateur = _tachesBrutes.Count(t => _metierService.EstJalon(t));

            lblStatistiques.Text = $"Tâches: {totalTaches} | " +
                                  $"Blocs: {_tachesBrutes.Select(t => t.BlocId).Distinct().Count()} | " +
                                  $"Avec métier: {tachesAvecMetier} ({pourcentageMapping:F0}%) | " +
                                  $"Jalons: {jalonsUtilisateur}";
        }

        private void PertControl_TacheSelected(object sender, TacheSelectedEventArgs e)
        {
            // Afficher les informations de la tâche sélectionnée
            var typeAffichage = _metierService.EstJalon(e.Tache) ? "Jalon" : "Tâche";
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom} [{typeAffichage}]";

            // Afficher les dépendances
            if (!string.IsNullOrEmpty(e.Tache.Dependencies))
            {
                var dependances = e.Tache.Dependencies.Split(',').Select(d => d.Trim()).ToList();
                lblTacheSelectionnee.Text += $" | Dépend de: {string.Join(", ", dependances)}";
            }
        }

        /// <summary>
        /// ÉDITION CORRIGÉE: Gestion spéciale des jalons J_Sync_
        /// </summary>
        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[TacheForm] Double-clic sur tâche: {e.Tache.TacheId}");

            // GESTION SPÉCIALE: Jalons J_Sync_
            if (_tacheService.EstJalonSync(e.Tache.TacheId))
            {
                System.Diagnostics.Debug.WriteLine($"[TacheForm] Édition jalon J_Sync_: {e.Tache.TacheId}");

                // Chercher d'abord dans les données brutes si une surcharge existe
                var surchargeExistante = _tachesBrutes.FirstOrDefault(t => t.TacheId == e.Tache.TacheId);

                if (surchargeExistante != null)
                {
                    // Éditer la surcharge existante
                    AfficherDetailsTache(surchargeExistante, false);
                }
                else
                {
                    // Créer une nouvelle surcharge basée sur le jalon technique
                    AfficherDetailsTacheJalonSync(e.Tache, true);
                }
                return;
            }

            // GESTION NORMALE: Toutes les autres tâches
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

        private void AfficherDetailsTacheJalonSync(TacheRecord jalonSync, bool modeCreation)
        {
            var message = $"Vous allez créer une surcharge personnalisée du jalon :\n\n" +
                         $"ID: {jalonSync.TacheId}\n" +
                         $"Fonction: Synchroniser la fin du métier dans ce bloc\n\n" +
                         $"Vous pourrez modifier :\n" +
                         $"• Le nom du jalon\n" +
                         $"• La durée d'attente (heures estimées)\n\n" +
                         $"Les dépendances seront recalculées automatiquement.\n\n" +
                         $"Continuer ?";

            var result = MessageBox.Show(message, "Personnaliser Jalon de Synchronisation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _tacheDetailForm.ModeJalonSync = true;
                _tacheDetailForm.ChargerTache(jalonSync, modeCreation);
            }
        }

        private void AfficherDetailsTache(TacheRecord tache, bool modeCreation)
        {
            _tacheDetailForm.ModeJalonSync = false;
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

        private void btnNouveauJalon_Click(object sender, EventArgs e)
        {
            // Créer un nouveau jalon pré-configuré
            var nouveauJalon = new TacheRecord
            {
                HeuresHommeEstimees = 0,
                MetierId = _metierService.GetJalonMetierId(),
                TacheNom = "Attente 0 heures"
            };

            // Sélectionner le premier lot/bloc par défaut
            var lots = _tacheService.ObtenirTousLesLots();
            var blocs = _tacheService.ObtenirTousLesBlocs();
            if (lots.Any())
            {
                nouveauJalon.LotId = lots.First().LotId;
                nouveauJalon.LotNom = lots.First().LotNom;
                nouveauJalon.LotPriorite = lots.First().Priorite;
            }
            if (blocs.Any())
            {
                nouveauJalon.BlocId = blocs.First().BlocId;
                nouveauJalon.BlocNom = blocs.First().BlocNom;
                nouveauJalon.BlocCapaciteMaxOuvriers = blocs.First().CapaciteMaxOuvriers;
            }

            AfficherDetailsTache(nouveauJalon, true);
        }

        private void btnModifierTache_Click(object sender, EventArgs e)
        {
            var tacheSelectionnee = _pertControl.TacheSelectionnee;
            if (tacheSelectionnee != null)
            {
                AfficherDetailsTache(tacheSelectionnee, false);
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

            var typeElement = _metierService.EstJalon(tacheSelectionnee) ? "jalon" : "tâche";
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer {typeElement} '{tacheSelectionnee.TacheId}' ?\n\nNom: {tacheSelectionnee.TacheNom}\nCette action est irréversible.",
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
                    MessageBox.Show($"{char.ToUpper(typeElement[0])}{typeElement.Substring(1)} supprimé{(typeElement == "tâche" ? "e" : "")} avec succès.",
                        "Suppression", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            var result = MessageBox.Show("Voulez-vous appliquer automatiquement les suggestions de dépendances métier à toutes les tâches ?\n\n" +
                                       "Cela ajoutera les dépendances suggérées sans écraser les dépendances existantes.",
                "Mapping automatique", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int compteur = 0;
                    foreach (var tache in _tachesBrutes.Where(t => !_metierService.EstJalon(t)))
                    {
                        var suggestions = _tacheService.SuggererDependancesMetier(tache);
                        if (suggestions.Any())
                        {
                            _tacheService.AppliquerSuggestionsMetier(tache.TacheId);
                            compteur++;
                        }
                    }

                    ChargerDonnees();
                    RafraichirAffichage();
                    MessageBox.Show($"Suggestions appliquées à {compteur} tâches.",
                        "Mapping terminé", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du mapping automatique :\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
                var statsChef = _tacheService.ObtenirStatistiques();
                var tachesSolveur = _decoupageTachesService.PreparerPourSolveur(_tachesBrutes);
                var statsDecoupage = _decoupageTachesService.ObtenirStatistiques(_tachesBrutes, tachesSolveur);

                var message = $"Analyse du flux Chef → Solveur:\n\n" +
                             $"📋 DONNÉES DU CHEF:\n" +
                             $"• Tâches totales: {statsChef.NombreTachesTotal}\n" +
                             $"• Jalons utilisateur: {statsChef.JalonsSurcharge}\n" +
                             $"• Heures totales: {statsChef.HeuresHommeTotal}h\n" +
                             $"• Avec dépendances: {statsChef.TachesAvecDependances}\n" +
                             $"• Blocs: {statsChef.NombreBlocsUniques}\n" +
                             $"• Lots: {statsChef.NombreLotsUniques}\n\n" +
                             $"⚙️ PRÉPARATION SOLVEUR:\n" +
                             $"• Tâches finales: {tachesSolveur.Count}\n" +
                             $"• Tâches découpées: {statsDecoupage.TachesLonguesDecoupees}\n" +
                             $"• Sous-tâches créées: {statsDecoupage.SousTachesCreees}\n" +
                             $"• Jalons techniques: {statsDecoupage.JalonsTechniquesCreees}\n" +
                             $"• Taux découpage: {statsDecoupage.TauxDecoupage:F1}%\n\n" +
                             $"✅ Les décisions du chef sont respectées intégralement";

                MessageBox.Show(message, "Analyse Chef → Solveur", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'analyse:\n{ex.Message}",
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