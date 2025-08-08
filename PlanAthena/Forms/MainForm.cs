//Fichier: MainForm.cs Version 0.4.1 - Avec export Excel
using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application;
using PlanAthena.Core.Facade;
using PlanAthena.Core.Infrastructure;
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Processing;
using PlanAthena.Utilities;
using System; // Nécessaire pour DayOfWeek, Action

namespace PlanAthena.Forms
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly PlanificationService _planificationService;
        private readonly OuvrierService _ouvrierService;
        private readonly TacheService _tacheService;
        private readonly ProjetService _projetService;
        private readonly GanttExportService _ganttExportService;
        private readonly ConfigurationBuilder _configBuilder;
        private readonly BlocService _blocService;
        private readonly IIdGeneratorService _idGeneratorService;

        // NOUVEAUX SERVICES pour l'export Excel
        private readonly PlanningExcelExportService _planningExcelExportService;
        private readonly CheminsPrefereService _cheminsPrefereService;

        private InformationsProjet _projetActuel;
        private PlanAthena.Core.Facade.Dto.Output.ProcessChantierResultDto _dernierResultatPlanification;
        private ConsolidatedGanttDto _dernierGanttConsolide;

        // NOUVEAU : Résultat complet pour l'export Excel
        private PlanificationResultDto _dernierResultatPlanificationComplet;

        // Dans MainForm.cs

        public MainForm()
        {
            try
            {
                InitializeComponent();

                // 1. Configurer l'injection de dépendances
                _serviceProvider = ConfigureServices();

                // 2. Récupérer les services nécessaires pour le formulaire principal
                // On récupère les services dans un ordre logique, bien que le conteneur gère les dépendances.
                _projetService = _serviceProvider.GetRequiredService<ProjetService>();
                _tacheService = _serviceProvider.GetRequiredService<TacheService>();
                _blocService = _serviceProvider.GetRequiredService<BlocService>();
                _ouvrierService = _serviceProvider.GetRequiredService<OuvrierService>();
                _planificationService = _serviceProvider.GetRequiredService<PlanificationService>();

                // Services utilitaires ou d'export
                _configBuilder = _serviceProvider.GetRequiredService<ConfigurationBuilder>();
                _cheminsPrefereService = _serviceProvider.GetRequiredService<CheminsPrefereService>();
                _ganttExportService = _serviceProvider.GetRequiredService<GanttExportService>();
                _planningExcelExportService = _serviceProvider.GetRequiredService<PlanningExcelExportService>();

                // L'IdGeneratorService n'est plus une dépendance directe de MainForm,
                // mais il est disponible pour les autres services via l'injection.
                _idGeneratorService = _serviceProvider.GetRequiredService<IIdGeneratorService>();
                // 3. Initialiser l'interface utilisateur et les données
                InitializeInterface();
                CreerNouveauProjetParDefaut();
            }
            catch (Exception ex)
            {
                // En cas d'erreur grave à l'initialisation, afficher un message clair et fermer.
                var errorForm = new Form
                {
                    Text = "Erreur d'initialisation critique",
                    Size = new System.Drawing.Size(600, 400),
                    StartPosition = FormStartPosition.CenterScreen
                };

                var textBox = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Text = $"Une erreur irrécupérable est survenue lors du démarrage de l'application :\r\n\r\n" +
                           $"{ex.Message}\r\n\r\n" +
                           $"Détails techniques :\r\n{ex.StackTrace}"
                };

                errorForm.Controls.Add(textBox);
                errorForm.ShowDialog();

                // Si l'initialisation échoue, l'application ne peut pas continuer.
                Application.Exit();
            }
        }

        private ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // --- Core DLL ---
            serviceCollection.AddApplicationServices();
            serviceCollection.AddInfrastructureServices();
            serviceCollection.AddScoped<PlanAthena.Core.Application.Interfaces.IConstructeurProblemeOrTools, PlanAthena.Core.Infrastructure.Services.OrTools.ConstructeurProblemeOrTools>();
            serviceCollection.AddScoped<PlanAthenaCoreFacade>();

            // --- Services de l'application principale ---

            // Utilitaires et services de données (généralement des Singletons)
            serviceCollection.AddSingleton<CsvDataService>();
            serviceCollection.AddSingleton<ExcelReader>();
            serviceCollection.AddSingleton<CheminsPrefereService>();
            serviceCollection.AddSingleton<GanttExportService>();
            serviceCollection.AddSingleton<ConfigurationBuilder>();
            serviceCollection.AddSingleton<PlanningExcelExportService>();

            // Services de traitement
            serviceCollection.AddSingleton<PreparationSolveurService>();
            serviceCollection.AddSingleton<ResultatConsolidationService>();
            serviceCollection.AddSingleton<DependanceBuilder>();
            serviceCollection.AddSingleton<DataTransformer>();

            // --- GESTION DES DÉPENDANCES CIRCULAIRES VIA FACTORIES ---
            // C'est la partie qui manquait.
            // On explique au conteneur comment créer une "factory" (un Func<T>) pour chaque service
            // impliqué dans une dépendance circulaire.
            // Quand TacheService demandera Func<ProjetService>, le conteneur saura lui fournir.
            serviceCollection.AddSingleton<Func<ProjetService>>(provider => () => provider.GetRequiredService<ProjetService>());
            serviceCollection.AddSingleton<Func<TacheService>>(provider => () => provider.GetRequiredService<TacheService>());
            serviceCollection.AddSingleton<Func<BlocService>>(provider => () => provider.GetRequiredService<BlocService>());

            // Services métier principaux (en Singleton car ils maintiennent l'état du projet chargé)
            // Le conteneur peut maintenant les construire, même avec leurs dépendances circulaires,
            // grâce aux factories enregistrées juste au-dessus.
            serviceCollection.AddSingleton<OuvrierService>();
            serviceCollection.AddSingleton<TacheService>();
            serviceCollection.AddSingleton<BlocService>();
            serviceCollection.AddSingleton<ProjetService>();
            serviceCollection.AddSingleton<PlanificationService>();

            // IdGeneratorService n'a pas de dépendances circulaires, son enregistrement est simple.
            serviceCollection.AddSingleton<IIdGeneratorService, IdGeneratorService>();

            return serviceCollection.BuildServiceProvider();
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

            var menuDonnees = new ToolStripMenuItem("Structure du projet");
            menuDonnees.DropDownItems.Add("📋 Métiers", null, OuvrirGestionMetiers_Click);
            menuDonnees.DropDownItems.Add("👷 Ouvriers", null, OuvrirGestionOuvriers_Click);
            menuDonnees.DropDownItems.Add("📦 Lots", null, OuvrirGestionLots_Click);
            menuDonnees.DropDownItems.Add("🧱 Blocs", null, OuvrirGestionBlocs_Click);

            var menuTaches = new ToolStripMenuItem("⚙️ Gérer les Tâches", null, OuvrirGestionTaches_Click);

            menuStrip.Items.AddRange(new ToolStripItem[] { menuProjet, menuDonnees, menuTaches });
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
                // MODIFIÉ : Utilise CheminsPrefereService
                using var ofd = new OpenFileDialog
                {
                    InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierProjets(),
                    Filter = "Fichiers projet (*.json)|*.json",
                    Title = "Charger un projet"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _projetActuel = _projetService.ChargerProjet(ofd.FileName);
                        MettreAJourAffichageProjet();
                        Log($"Projet chargé : {_projetActuel.NomProjet}");

                        // NOUVEAU : Sauvegarder le chemin utilisé
                        _cheminsPrefereService.SauvegarderDernierDossier(TypeOperation.ProjetChargement, ofd.FileName);
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

            // MODIFIÉ : Utilise CheminsPrefereService
            using var sfd = new SaveFileDialog
            {
                InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierProjets(),
                Filter = "Fichiers projet (*.json)|*.json",
                Title = "Sauvegarder le projet",
                FileName = $"{_projetActuel?.NomProjet?.Replace(" ", "_") ?? "projet"}.json"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _projetService.SauvegarderProjet(sfd.FileName, _projetActuel);
                    Log($"Projet sauvegardé : {sfd.FileName}");

                    // NOUVEAU : Sauvegarder le chemin utilisé
                    _cheminsPrefereService.SauvegarderDernierDossier(TypeOperation.ProjetSauvegarde, sfd.FileName);
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
            MessageBox.Show("L'export CSV est en cours de refonte et sera disponible dans une prochaine version.", "Fonctionnalité indisponible", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ImportCsvGroupe_Click(object sender, EventArgs e)
        {
            MessageBox.Show("L'import CSV est en cours de refonte et sera disponible dans une prochaine version.", "Fonctionnalité indisponible", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OuvrirGestionMetiers_Click(object sender, EventArgs e)
        {
            using var form = new MetierForm(
                _serviceProvider.GetRequiredService<ProjetService>(),
                _serviceProvider.GetRequiredService<IIdGeneratorService>(),
                _serviceProvider.GetRequiredService<DependanceBuilder>()
            );
            form.ShowDialog(this);
        }

        private void OuvrirGestionOuvriers_Click(object sender, EventArgs e)
        {
            using var form = new OuvrierForm(_ouvrierService, _projetService, _idGeneratorService);
            form.ShowDialog(this);
        }

        private void OuvrirGestionLots_Click(object sender, EventArgs e)
        {
            using var form = new LotForm(_projetService, _tacheService);
            form.ShowDialog(this);
        }

        private void OuvrirGestionBlocs_Click(object sender, EventArgs e)
        {
            using var form = new BlocForm(_blocService, _tacheService);
            form.ShowDialog(this);
        }

        private void OuvrirGestionTaches_Click(object sender, EventArgs e)
        {
            var dependanceBuilder = _serviceProvider.GetRequiredService<DependanceBuilder>();
            using var form = new TacheForm(_tacheService, _projetService, dependanceBuilder, _blocService);
            form.ShowDialog(this);
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

                var statsTraitement = _planificationService.ObtenirStatistiquesTraitement();
                Log($"Préparation des données : {statsTraitement.Resume}");

                Log("Lancement de la planification avec PlanAthena...");
                _planificationService.ChargerDonnees(
                    _ouvrierService.ObtenirTousLesOuvriers(),
                    _tacheService.ObtenirToutesLesTaches(),
                    _projetService.GetAllMetiers().ToList()
                );

                var resultatComplet = await _planificationService.LancerPlanificationAsync(configuration);

                _dernierResultatPlanification = resultatComplet.ResultatBrut;
                _dernierGanttConsolide = resultatComplet.GanttConsolide;

                // NOUVEAU : Sauvegarder le résultat complet pour l'export Excel
                _dernierResultatPlanificationComplet = resultatComplet;

                AfficherResultatDansLog(_dernierResultatPlanification);

                VerifierDisponibiliteExportGantt();

                // NOUVEAU : Vérifier disponibilité export Excel
                VerifierDisponibiliteExportExcel();

                Log("PLANIFICATION TERMINÉE.");
            }
            catch (Exception ex)
            {
                Log($"ERREUR CRITIQUE lors de la planification : {ex}");
                MessageBox.Show($"Une erreur critique est survenue:\n{ex.Message}", "Erreur Critique", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // NOUVELLE MÉTHODE : Export Planning Excel
        private async void btnExportPlanningExcel_Click(object sender, EventArgs e)
        {
            if (_dernierResultatPlanificationComplet == null)
            {
                MessageBox.Show("Veuillez d'abord lancer une planification avant d'exporter.",
                    "Planification requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Désactiver le bouton pendant l'export
                btnExportPlanningExcel.Enabled = false;
                btnExportPlanningExcel.Text = "Export en cours...";

                // Récupérer les données nécessaires
                var ouvriers = _ouvrierService.ObtenirTousLesOuvriers();
                var metiers = _projetService.GetAllMetiers();
                var nomProjet = _projetActuel?.NomProjet ?? "Projet PlanAthena";

                // NOUVEAU : Récupérer la configuration pour les calculs KPI
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

                Log("Début de l'export Planning Excel...");

                // Lancer l'export avec la configuration
                var cheminFichier = await _planningExcelExportService.ExporterPlanningComplet(
                    _dernierResultatPlanificationComplet,
                    ouvriers,
                    metiers,
                    nomProjet,
                    configuration
                );

                Log($"📋 Export Planning Excel réussi : {cheminFichier}");

                // Afficher le résultat
                var result = MessageBox.Show(
                    $"Planning exporté avec succès !\n\nFichier : {Path.GetFileName(cheminFichier)}\nDossier : {Path.GetDirectoryName(cheminFichier)}\n\nVoulez-vous ouvrir le dossier ?",
                    "Export réussi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );

                if (result == DialogResult.Yes)
                {
                    // Ouvrir le dossier contenant le fichier
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{cheminFichier}\"");
                }
            }
            catch (Exception ex)
            {
                Log($"ERREUR lors de l'export Excel : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'export Excel :\n{ex.Message}",
                    "Erreur d'export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Réactiver le bouton
                btnExportPlanningExcel.Enabled = true;
                btnExportPlanningExcel.Text = "📋 Export Planning Excel";
            }
        }

        #endregion

        #region Export GanttProject

        private void VerifierDisponibiliteExportGantt()
        {
            bool peutExporter = _dernierResultatPlanification?.OptimisationResultat?.Affectations?.Any() == true
                               && _dernierGanttConsolide?.TachesRacines?.Any() == true;

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
                Log($"📊 Export GanttProject disponible ({_dernierGanttConsolide.TachesRacines.Count} tâches consolidées)");
            }
        }

        // NOUVELLE MÉTHODE : Vérifier disponibilité export Excel
        private void VerifierDisponibiliteExportExcel()
        {
            bool peutExporter = _dernierResultatPlanificationComplet?.ResultatBrut?.OptimisationResultat?.Affectations?.Any() == true;

            if (btnExportPlanningExcel.InvokeRequired)
            {
                btnExportPlanningExcel.Invoke(new Action(() => btnExportPlanningExcel.Enabled = peutExporter));
            }
            else
            {
                btnExportPlanningExcel.Enabled = peutExporter;
            }

            if (peutExporter)
            {
                Log($"📋 Export Planning Excel disponible ({_dernierResultatPlanificationComplet.ResultatBrut.OptimisationResultat.Affectations.Count} affectations)");
            }
        }

        private void btnExportGantt_Click(object sender, EventArgs e)
        {
            if (_dernierGanttConsolide?.TachesRacines?.Any() != true)
            {
                MessageBox.Show("Aucun planning consolidé à exporter. Veuillez d'abord lancer une planification.", "Export impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog { Filter = "Fichiers GanttProject (*.gan)|*.gan", Title = "Exporter vers GanttProject", FileName = $"{_projetActuel?.NomProjet?.Replace(" ", "_") ?? "planning"}.gan" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var config = _configBuilder.ConstruireConfigExportGantt(_projetActuel?.NomProjet ?? "Planning", (double)numHeuresTravail.Value, chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>());

                    _ganttExportService.ExporterVersGanttProjectXml(_dernierGanttConsolide, sfd.FileName, config);

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
                txtNomProjet.TextChanged -= SynchroniserProjet;
                txtAuteur.TextChanged -= SynchroniserProjet;
                txtDescription.TextChanged -= SynchroniserProjet;

                txtNomProjet.Text = _projetActuel.NomProjet;
                txtAuteur.Text = _projetActuel.Auteur;
                txtDescription.Text = _projetActuel.Description;
                lblDateCreation.Text = $"Créé le: {_projetActuel.DateCreation:dd/MM/yyyy}";
                this.Text = $"PlanAthena - {_projetActuel.NomProjet}";

                txtNomProjet.TextChanged -= SynchroniserProjet;
                txtAuteur.TextChanged -= SynchroniserProjet;
                txtDescription.TextChanged -= SynchroniserProjet;
            }

            MettreAJourResume();
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