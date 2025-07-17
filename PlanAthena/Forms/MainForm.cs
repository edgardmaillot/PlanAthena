using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application;
using PlanAthena.Core.Facade;
using PlanAthena.Core.Infrastructure;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Processing;
using PlanAthena.Utilities;

namespace PlanAthena.Forms
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly PlanificationService _planificationService;
        private readonly OuvrierService _ouvrierService;
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly ProjetService _projetService;
        private readonly GanttExportService _ganttExportService;
        private readonly ConfigurationBuilder _configBuilder;
        private readonly PreparationSolveurService _decoupageTachesService;

        private InformationsProjet _projetActuel;
        private PlanAthena.Core.Facade.Dto.Output.ProcessChantierResultDto _dernierResultatPlanification;

        public MainForm()
        {
            InitializeComponent();
            _serviceProvider = ConfigureServices();

            _planificationService = _serviceProvider.GetRequiredService<PlanificationService>();
            _ouvrierService = _serviceProvider.GetRequiredService<OuvrierService>();
            _tacheService = _serviceProvider.GetRequiredService<TacheService>();
            _metierService = _serviceProvider.GetRequiredService<MetierService>();
            _projetService = _serviceProvider.GetRequiredService<ProjetService>();
            _ganttExportService = _serviceProvider.GetRequiredService<GanttExportService>();
            _configBuilder = _serviceProvider.GetRequiredService<ConfigurationBuilder>();
            _decoupageTachesService = _serviceProvider.GetRequiredService<PreparationSolveurService>();

            InitializeInterface();
            CreerNouveauProjetParDefaut();
        }

        private void InitializeInterface()
        {
            InitializeMenu();
            chkListJoursOuvres.Items.Add(DayOfWeek.Monday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Tuesday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Wednesday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Thursday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Friday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Saturday, false);
            chkListJoursOuvres.Items.Add(DayOfWeek.Sunday, false);
            chkListJoursOuvres.DisplayMember = "ToString";

            cmbTypeDeSortie.Items.Clear();
            cmbTypeDeSortie.Items.AddRange(new string[] { "Analyse et Estimation", "Optimisation Coût", "Optimisation Délai" });
            cmbTypeDeSortie.SelectedIndex = 0;

            dtpDateDebut.Value = DateTime.Today;
            dtpDateFin.Value = DateTime.Today.AddMonths(3);
            chkDateDebut.Checked = true;
            chkDateFin.Checked = true;

            var timerMiseAJour = new System.Windows.Forms.Timer { Interval = 2000 };
            timerMiseAJour.Tick += (s, e) => MettreAJourResume();
            timerMiseAJour.Start();

            Log("Application prête. Créez un nouveau projet ou chargez un projet existant.");
        }

        private void InitializeMenu()
        {
            var menuStrip = new MenuStrip();
            var menuProjet = new ToolStripMenuItem("📁 Projet");
            menuProjet.DropDownItems.Add("🆕 Nouveau projet", null, NouveauProjet_Click);
            menuProjet.DropDownItems.Add("📂 Charger projet", null, ChargerProjet_Click);
            menuProjet.DropDownItems.Add("💾 Sauvegarder projet", null, SauvegarderProjet_Click);
            menuProjet.DropDownItems.Add(new ToolStripSeparator());
            menuProjet.DropDownItems.Add("📤 Export CSV (tout)", null, ExportCsvTout_Click);
            menuProjet.DropDownItems.Add("📥 Import CSV groupé", null, ImportCsvGroupe_Click);
            menuProjet.DropDownItems.Add(new ToolStripSeparator());
            menuProjet.DropDownItems.Add("❌ Quitter", null, Quitter_Click);

            var menuTaches = new ToolStripMenuItem("⚙️ Tâches", null, OuvrirGestionTaches_Click);
            var menuMetiers = new ToolStripMenuItem("📋 Métiers", null, OuvrirGestionMetiers_Click);
            var menuOuvriers = new ToolStripMenuItem("👷 Ouvriers", null, OuvrirGestionOuvriers_Click);

            menuStrip.Items.AddRange(new ToolStripItem[] { menuProjet, menuTaches, menuMetiers, menuOuvriers });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        #region Événements Menu

        private void NouveauProjet_Click(object sender, EventArgs e)
        {
            if (ConfirmerPerteDonnees())
            {
                using var dialog = new NouveauProjetDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _projetActuel = _projetService.CreerNouveauProjet(dialog.NomProjet, dialog.Description);
                    _projetActuel.Auteur = dialog.Auteur;
                    MettreAJourAffichageProjet();
                    Log($"Nouveau projet créé : {_projetActuel.NomProjet}");
                }
            }
        }

        private void ChargerProjet_Click(object sender, EventArgs e)
        {
            if (ConfirmerPerteDonnees())
            {
                using var ofd = new OpenFileDialog { Filter = "Fichiers projet (*.json)|*.json", Title = "Charger un projet" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _projetActuel = _projetService.ChargerProjet(ofd.FileName);
                        MettreAJourAffichageProjet();
                        Log($"Projet chargé : {_projetActuel.NomProjet}");
                    }
                    catch (Exception ex)
                    {
                        Log($"ERREUR lors du chargement : {ex.Message}");
                        MessageBox.Show($"Erreur lors du chargement du projet :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SauvegarderProjet_Click(object sender, EventArgs e)
        {
            SynchroniserProjetDepuisInterface();
            using var sfd = new SaveFileDialog { Filter = "Fichiers projet (*.json)|*.json", Title = "Sauvegarder le projet", FileName = $"{_projetActuel?.NomProjet?.Replace(" ", "_") ?? "projet"}.json" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _projetService.SauvegarderProjet(sfd.FileName, _projetActuel);
                    Log($"Projet sauvegardé : {sfd.FileName}");
                }
                catch (Exception ex)
                {
                    Log($"ERREUR lors de la sauvegarde : {ex.Message}");
                    MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportCsvTout_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog { Description = "Sélectionner le dossier d'export" };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var prefixe = _projetActuel?.NomProjet?.Replace(" ", "_") ?? "export";
                    _projetService.ExporterToutVersCsv(fbd.SelectedPath, prefixe);
                    Log($"Export CSV complet effectué dans : {fbd.SelectedPath}");
                }
                catch (Exception ex)
                {
                    Log($"ERREUR lors de l'export : {ex.Message}");
                    MessageBox.Show($"Erreur lors de l'export :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ImportCsvGroupe_Click(object sender, EventArgs e)
        {
            using var dialog = new ImportCsvGroupeDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var resume = _projetService.ImporterToutDepuisCsv(dialog.CheminMetiers, dialog.CheminOuvriers, dialog.CheminTaches);
                    if (resume.Succes)
                    {
                        Log($"Import groupé réussi : {resume.MetiersImportes} métiers, {resume.OuvriersImportes} ouvriers, {resume.TachesImportees} tâches");
                    }
                    else
                    {
                        Log($"ERREUR lors de l'import groupé : {resume.MessageErreur}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"ERREUR lors de l'import groupé : {ex.Message}");
                }
            }
        }

        private void OuvrirGestionMetiers_Click(object sender, EventArgs e)
        {
            using var form = new MetierForm(_metierService);
            form.ShowDialog();
        }

        private void OuvrirGestionOuvriers_Click(object sender, EventArgs e)
        {
            using var form = new OuvrierForm(_ouvrierService, _metierService);
            form.ShowDialog();
        }

        private void OuvrirGestionTaches_Click(object sender, EventArgs e)
        {
            var dependanceBuilder = _serviceProvider.GetRequiredService<DependanceBuilder>();
            using var form = new TacheForm(_tacheService, _metierService, dependanceBuilder); // Doit passer le DependanceBuilder
            form.ShowDialog();
        }

        private void Quitter_Click(object sender, EventArgs e)
        {
            if (ConfirmerPerteDonnees())
            {
                Application.Exit();
            }
        }

        #endregion

        #region Planification

        private async void btnGenerateAndTest_Click(object sender, EventArgs e)
        {
            Log("Lancement de la planification...");

            // Validation des données avant planification
            if (!_projetService.ValiderDonneesAvantPlanification(out string messageValidation))
            {
                Log($"ERREUR : {messageValidation}");
                MessageBox.Show(messageValidation, "Données manquantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Log("Construction de la configuration...");
                var configuration = _configBuilder.ConstruireDepuisUI(
                    chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>().ToList(),
                    (int)numHeureDebut.Value,
                    (int)numHeuresTravail.Value,
                    cmbTypeDeSortie.SelectedItem.ToString(),
                    _projetActuel?.Description ?? txtDescription.Text,
                    chkDateDebut.Checked ? dtpDateDebut.Value.Date : null,
                    chkDateFin.Checked ? dtpDateFin.Value.Date : null,
                    (int)numDureeStandard.Value,
                    numPenaliteChangement.Value,
                    numCoutIndirect.Value
                );

                // Afficher les statistiques de traitement avant planification
                var statsTraitement = _planificationService.ObtenirStatistiquesTraitement();
                Log($"Préparation des données : {statsTraitement.Resume}");

                Log("Lancement de la planification avec PlanAthena...");
                _planificationService.ChargerDonnees(
                    _ouvrierService.ObtenirTousLesOuvriers(),
                    _tacheService.ObtenirToutesLesTaches(),
                    _metierService.GetAllMetiers().ToList()
                );

                var resultatDto = await _planificationService.LancerPlanificationAsync(configuration);

                _dernierResultatPlanification = resultatDto;
                AfficherResultatDansLog(resultatDto);
                VerifierDisponibiliteExportGantt();
                Log("PLANIFICATION TERMINÉE.");
            }
            catch (Exception ex)
            {
                Log($"ERREUR CRITIQUE lors de la planification : {ex}");
                MessageBox.Show($"Une erreur critique est survenue:\n{ex.Message}", "Erreur Critique", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Export GanttProject

        private void VerifierDisponibiliteExportGantt()
        {
            bool peutExporter = _dernierResultatPlanification?.OptimisationResultat?.Affectations?.Any() == true;
            if (btnExportGantt.InvokeRequired)
            {
                btnExportGantt.Invoke(new Action(() => btnExportGantt.Enabled = peutExporter));
            }
            else
            {
                btnExportGantt.Enabled = peutExporter;
            }
            if (peutExporter)
            {
                Log("📊 Export GanttProject disponible");
            }
        }

        private void btnExportGantt_Click(object sender, EventArgs e)
        {
            if (_dernierResultatPlanification?.OptimisationResultat?.Affectations?.Any() != true)
            {
                MessageBox.Show("Aucun planning à exporter. Veuillez d'abord lancer une planification.", "Export impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog { Filter = "Fichiers GanttProject (*.gan)|*.gan", Title = "Exporter vers GanttProject", FileName = $"{_projetActuel?.NomProjet?.Replace(" ", "_") ?? "planning"}.gan" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var config = _configBuilder.ConstruireConfigExportGantt(_projetActuel?.NomProjet ?? "Planning", (double)numHeuresTravail.Value, chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>());
                    _ganttExportService.ExporterVersGanttProjectXml(_dernierResultatPlanification, sfd.FileName, config);
                    Log($"📊 Export GanttProject réussi : {sfd.FileName}");
                    MessageBox.Show($"Export GanttProject terminé avec succès !\n\nFichier : {sfd.FileName}", "Export réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Log($"ERREUR lors de l'export GanttProject : {ex.Message}");
                    MessageBox.Show($"Erreur lors de l'export GanttProject :\n{ex.Message}", "Erreur d'export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Gestion du projet et interface

        private void CreerNouveauProjetParDefaut()
        {
            _projetActuel = new InformationsProjet { NomProjet = "Nouveau projet", Description = "", DateCreation = DateTime.Now, DateDerniereModification = DateTime.Now, Auteur = Environment.UserName };
            MettreAJourAffichageProjet();
        }

        private void MettreAJourAffichageProjet()
        {
            if (_projetActuel != null)
            {
                txtNomProjet.Text = _projetActuel.NomProjet;
                txtAuteur.Text = _projetActuel.Auteur;
                txtDescription.Text = _projetActuel.Description;
                lblDateCreation.Text = $"Créé le: {_projetActuel.DateCreation:dd/MM/yyyy}";
                this.Text = $"PlanAthena - {_projetActuel.NomProjet}";
            }
            AttacherEvenementsSynchronisation();
            MettreAJourResume();
        }

        private void AttacherEvenementsSynchronisation()
        {
            txtNomProjet.TextChanged -= SynchroniserProjet;
            txtAuteur.TextChanged -= SynchroniserProjet;
            txtDescription.TextChanged -= SynchroniserProjet;
            txtNomProjet.TextChanged += SynchroniserProjet;
            txtAuteur.TextChanged += SynchroniserProjet;
            txtDescription.TextChanged += SynchroniserProjet;
        }

        private void SynchroniserProjet(object sender, EventArgs e)
        {
            if (_projetActuel != null)
            {
                _projetActuel.NomProjet = txtNomProjet.Text;
                _projetActuel.Auteur = txtAuteur.Text;
                _projetActuel.Description = txtDescription.Text;
                _projetActuel.DateDerniereModification = DateTime.Now;
                this.Text = $"PlanAthena - {_projetActuel.NomProjet}";
            }
        }

        private void SynchroniserProjetDepuisInterface()
        {
            if (_projetActuel != null)
            {
                _projetActuel.NomProjet = txtNomProjet.Text;
                _projetActuel.Description = txtDescription.Text;
                _projetActuel.Auteur = txtAuteur.Text;
                _projetActuel.DateDerniereModification = DateTime.Now;
            }
        }

        private void MettreAJourResume()
        {
            try
            {
                var resume = _projetService.ObtenirResumeProjet();
                var jalonsUtilisateur = _tacheService.ObtenirToutesLesTaches().Count(t => t.Type == TypeActivite.JalonUtilisateur);

                lblResume.Text = $"Résumé: {resume.StatistiquesOuvriers.NombreOuvriersTotal} ouvriers, {resume.NombreMetiers} métiers, {resume.StatistiquesTaches.NombreTachesTotal} tâches (+{jalonsUtilisateur} jalons)";
                lblMapping.Text = $"Mapping: {resume.StatistiquesMappingMetiers.PourcentageMapping:F0}% ({resume.StatistiquesMappingMetiers.TachesAvecMetier}/{resume.StatistiquesMappingMetiers.TotalTaches} tâches)";

                // MODIFIÉ: Afficher les statistiques simplifiées de traitement si des données existent
                if (resume.StatistiquesTaches.NombreTachesTotal > 0)
                {
                    try
                    {
                        var statsTraitement = _planificationService.ObtenirStatistiquesTraitement();
                        if (statsTraitement.TachesSolveur > statsTraitement.TachesChef)
                        {
                            var decoupees = statsTraitement.TachesDecoupees;
                            var jalons = statsTraitement.JalonsTechniques;
                            lblMapping.Text += $" | Préparation: {decoupees} découpées, +{jalons} jalons tech.";
                        }
                    }
                    catch
                    {
                        // Ignorer les erreurs de calcul des statistiques de traitement
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur mise à jour résumé: {ex.Message}");
            }
        }

        private bool ConfirmerPerteDonnees()
        {
            var resume = _projetService.ObtenirResumeProjet();
            var totalDonnees = resume.NombreMetiers + resume.StatistiquesOuvriers.NombreOuvriersTotal + resume.StatistiquesTaches.NombreTachesTotal;
            if (totalDonnees > 0)
            {
                var result = MessageBox.Show("Des données non sauvegardées seront perdues. Continuer ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                return result == DialogResult.Yes;
            }
            return true;
        }

        #endregion

        #region Méthodes utilitaires

        private void Log(string message)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action<string>(Log), message);
                return;
            }
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            rtbLog.ScrollToCaret();
        }

        private ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // Services PlanAthena Core (existants)
            serviceCollection.AddApplicationServices();
            serviceCollection.AddInfrastructureServices();
            serviceCollection.AddScoped<PlanAthena.Core.Application.Interfaces.IConstructeurProblemeOrTools, PlanAthena.Core.Infrastructure.Services.OrTools.ConstructeurProblemeOrTools>();
            serviceCollection.AddScoped<PlanAthenaCoreFacade>();

            // Services qui contiennent des données d'état doivent être Singleton
            serviceCollection.AddSingleton<MetierService>();
            serviceCollection.AddSingleton<OuvrierService>();
            serviceCollection.AddSingleton<TacheService>();
            serviceCollection.AddSingleton<ProjetService>();

            // Les services "sans état" ou utilitaires peuvent rester Scoped ou Transient
            serviceCollection.AddScoped<CsvDataService>();
            serviceCollection.AddScoped<ExcelReader>();
            serviceCollection.AddScoped<DataTransformer>();
            serviceCollection.AddScoped<PlanificationService>();
            serviceCollection.AddScoped<GanttExportService>();
            serviceCollection.AddScoped<ConfigurationBuilder>();


            // NOUVEAUX SERVICES
            serviceCollection.AddScoped<PreparationSolveurService>();
            serviceCollection.AddScoped<TopologieDependanceService>();
            serviceCollection.AddScoped<DependanceBuilder>();

            return serviceCollection.BuildServiceProvider();
        }

        private void AfficherResultatDansLog(PlanAthena.Core.Facade.Dto.Output.ProcessChantierResultDto resultat)
        {
            if (resultat == null) { Log("Le résultat retourné par la façade est null."); return; }
            Log($"\n--- Résultat pour le Chantier ID: {resultat.ChantierId} ---");
            Log($"État du Traitement: {resultat.Etat}");
            if (resultat.Messages.Any())
            {
                Log("\nMessages de validation et suggestions :");
                foreach (var msg in resultat.Messages)
                {
                    string details = !string.IsNullOrEmpty(msg.ElementId) ? $" (Élément: {msg.ElementId})" : "";
                    Log($"  [{msg.Type}] ({msg.CodeMessage}) {msg.Message}{details}");
                }
            }
            if (resultat.AnalyseStatiqueResultat != null)
            {
                Log("\n--- Analyse Statique et Estimation Préliminaire ---");
                var analyse = resultat.AnalyseStatiqueResultat;
                if (analyse.CoutTotalEstime.HasValue) Log($"Coût Total Estimé : {analyse.CoutTotalEstime / 100.0m:C}");
                if (analyse.DureeTotaleEstimeeEnSlots.HasValue) Log($"Durée Totale Estimée : {analyse.DureeTotaleEstimeeEnSlots} heures ({analyse.DureeTotaleEstimeeEnSlots / 7.0:F1} jours de 7h)");
                if (analyse.OuvriersClesSuggereIds.Any()) Log($"Ouvriers clés suggérés : {string.Join(", ", analyse.OuvriersClesSuggereIds)}");
            }
            if (resultat.OptimisationResultat?.Affectations?.Any() ?? false)
            {
                Log("\n--- Planning Détaillé (Affectations) ---");
                var planningParJour = resultat.OptimisationResultat.Affectations.OrderBy(a => a.DateDebut).GroupBy(a => a.DateDebut.Date);
                foreach (var jour in planningParJour)
                {
                    Log($"\n  [ Jour: {jour.Key:dddd dd MMMM yyyy} ]");
                    var tachesParOuvrier = jour.OrderBy(a => a.OuvrierNom).GroupBy(a => a.OuvrierNom);
                    foreach (var groupeOuvrier in tachesParOuvrier)
                    {
                        Log($"    > Ouvrier: {groupeOuvrier.Key}");
                        foreach (var affectation in groupeOuvrier)
                        {
                            var dateFinEstimee = affectation.DateDebut.AddHours(affectation.DureeHeures);
                            Log($"      {affectation.DateDebut:HH:mm}-{dateFinEstimee:HH:mm} ({affectation.DureeHeures}h) | Tâche: {affectation.TacheNom} (Bloc: {affectation.BlocId})");
                        }
                    }
                }
            }
            if (resultat.OptimisationResultat != null)
            {
                var optimResult = resultat.OptimisationResultat;
                Log("\n--- Résumé de l'Optimisation ---");
                Log($"Statut du Solveur: {optimResult.Status}");
                if (optimResult.CoutTotalEstime.HasValue) Log($"Coût Total Estimé : {optimResult.CoutTotalEstime / 100.0m:C}");
                if (optimResult.DureeTotaleEnSlots.HasValue) Log($"Durée Totale (en slots de 1h): {optimResult.DureeTotaleEnSlots}");
            }
            if (resultat.AnalysePostOptimisationResultat != null)
            {
                var analysisResult = resultat.AnalysePostOptimisationResultat;
                Log("\n--- Analyse Post-Planning (KPIs) ---");
                Log($"Taux d'Occupation Moyen Pondéré: {analysisResult.KpisGlobaux.TauxOccupationMoyenPondere:F2}%");
                foreach (var kpi in analysisResult.KpisParOuvrier)
                {
                    Log($"  - {kpi.OuvrierNom} ({kpi.OuvrierId}): Taux d'Occupation: {kpi.TauxOccupation:F2}% ({kpi.HeuresTravaillees:F1}h travaillées)");
                }
            }
        }

        #endregion
    }
}