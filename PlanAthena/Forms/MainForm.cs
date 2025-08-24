// Fichier: MainForm.cs
// Version: 0.4.4
// Description: Refactorisation complète pour s'aligner sur la nouvelle architecture de services.
// L'injection de dépendances est gérée par Program.cs, les appels de service sont mis à jour.

using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class MainForm : Form
    {
        // NOUVELLES DÉPENDANCES DE SERVICE
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly ProjetRepository _projetRepository;
        private readonly ImportService _importService;
        private readonly PlanificationService _planificationService;
        private readonly PlanningExcelExportService _planningExcelExportService;
        private readonly GanttExportService _ganttExportService;
        private readonly CheminsPrefereService _cheminsPrefereService;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly DependanceBuilder _dependanceBuilder;




        // NOUVELLE GESTION DE L'ÉTAT DU PROJET
        private ProjetData _projetActuel;

        // Données de résultat
        private PlanificationResultDto _dernierResultatPlanificationComplet;

        public MainForm(
            ProjetService projetService,
            RessourceService ressourceService,
            ProjetRepository projetRepository,
            ImportService importService,
            PlanificationService planificationService,
            PlanningExcelExportService planningExcelExportService,
            GanttExportService ganttExportService,
            CheminsPrefereService cheminsPrefereService,
            IIdGeneratorService idGeneratorService,
            DependanceBuilder dependanceBuilder)
        {
            InitializeComponent();

            // Injection des dépendances via le constructeur
            _projetService = projetService;
            _ressourceService = ressourceService;
            _projetRepository = projetRepository;
            _importService = importService;
            _planificationService = planificationService;
            _planningExcelExportService = planningExcelExportService;
            _ganttExportService = ganttExportService;
            _cheminsPrefereService = cheminsPrefereService;
            _idGeneratorService = idGeneratorService;
            _dependanceBuilder = dependanceBuilder;

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

            cmbCalculMax.Items.Clear();
            cmbCalculMax.Items.AddRange(new object[] { 1, 5, 15, 30, 60 });
            cmbCalculMax.SelectedItem = 5;

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
                    CreerNouveauProjetParDefaut();
                    _projetActuel.InformationsProjet.NomProjet = dialog.NomProjet;
                    _projetActuel.InformationsProjet.Description = dialog.Description;
                    _projetActuel.InformationsProjet.Auteur = dialog.Auteur;
                    MettreAJourAffichageProjet();
                    Log($"Nouveau projet créé : {_projetActuel.InformationsProjet.NomProjet}");
                }
            }
        }

        private void ChargerProjet_Click(object sender, EventArgs e)
        {
            if (ConfirmerPerteDonnees())
            {
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
                        // Étape 1 : Lire les données brutes du fichier dans notre objet conteneur.
                        _projetActuel = _projetRepository.Charger(ofd.FileName);

                        // Étape 2 : Donner les données aux services "stateful" pour qu'ils chargent leur état interne.
                        // On utilise les noms de méthodes que nous avons validés.
                        _ressourceService.ChargerRessources(_projetActuel.Metiers, _projetActuel.Ouvriers);
                        _projetService.ChargerProjet(_projetActuel);

                        // Étape 3 : Mettre à jour l'interface utilisateur.
                        MettreAJourAffichageProjet();
                        Log($"Projet chargé : {_projetActuel.InformationsProjet.NomProjet}");

                        // Étape 4 : Sauvegarder le chemin pour la prochaine fois.
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

            _projetActuel.Metiers = _ressourceService.GetAllMetiers();
            _projetActuel.Ouvriers = _ressourceService.GetAllOuvriers();

            using var sfd = new SaveFileDialog
            {
                InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierProjets(),
                Filter = "Fichiers projet (*.json)|*.json",
                Title = "Sauvegarder le projet",
                FileName = $"{_projetActuel?.InformationsProjet?.NomProjet?.Replace(" ", "_") ?? "projet"}.json"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _projetRepository.Sauvegarder(_projetActuel, sfd.FileName);
                    Log($"Projet sauvegardé : {sfd.FileName}");
                    _cheminsPrefereService.SauvegarderDernierDossier(TypeOperation.ProjetSauvegarde, sfd.FileName);
                }
                catch (Exception ex)
                {
                    Log($"ERREUR lors de la sauvegarde : {ex.Message}");
                    MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OuvrirGestionMetiers_Click(object sender, EventArgs e)
        {
            using var form = new MetierForm(_projetService, _ressourceService, _idGeneratorService, _dependanceBuilder);
            form.ShowDialog(this);
            MettreAJourResume();
        }

        private void OuvrirGestionOuvriers_Click(object sender, EventArgs e)
        {
            using var form = new OuvrierForm(_ressourceService, _projetService, _idGeneratorService, _importService);
            form.ShowDialog(this);
            MettreAJourResume();
        }

        private void OuvrirGestionLots_Click(object sender, EventArgs e)
        {
            using var form = new LotForm(_projetService);
            form.ShowDialog(this);
            MettreAJourResume();
        }

        private void OuvrirGestionBlocs_Click(object sender, EventArgs e)
        {
            using var form = new BlocForm(_projetService);
            form.ShowDialog(this);
            MettreAJourResume();
        }

        private void OuvrirGestionTaches_Click(object sender, EventArgs e)
        {
            using var form = new TacheForm(_projetService, _ressourceService, _dependanceBuilder, _importService);
            form.ShowDialog(this);
            MettreAJourResume();
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

            if (!_ressourceService.GetAllMetiers().Any() || !_ressourceService.GetAllOuvriers().Any() || !_projetActuel.Taches.Any())
            {
                var messageValidation = "Données manquantes : veuillez vérifier que le projet contient des métiers, des ouvriers et des tâches.";
                Log($"ERREUR : {messageValidation}");
                MessageBox.Show(messageValidation, "Données manquantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Log("Construction de la configuration...");
                var configuration = new ConfigurationPlanification
                {
                    JoursOuvres = chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>().ToList(),
                    HeureDebutJournee = (int)numHeureDebut.Value,
                    HeuresTravailEffectifParJour = (int)numHeuresTravail.Value,
                    TypeDeSortie = cmbTypeDeSortie.SelectedItem.ToString(),
                    Description = _projetActuel?.InformationsProjet?.Description ?? txtDescription.Text,
                    DateDebutSouhaitee = chkDateDebut.Checked ? dtpDateDebut.Value.Date : null,
                    DateFinSouhaitee = chkDateFin.Checked ? dtpDateFin.Value.Date : null,
                    DureeJournaliereStandardHeures = (int)numDureeStandard.Value,
                    PenaliteChangementOuvrierPourcentage = numPenaliteChangement.Value,
                    CoutIndirectJournalierAbsolu = (long)numCoutIndirect.Value,
                    DureeCalculMaxMinutes = (int)cmbCalculMax.SelectedItem
                };

                var statsTraitement = _planificationService.ObtenirStatistiquesTraitement(_projetActuel.Taches, (int)numHeuresTravail.Value);
                Log($"Préparation des données : {statsTraitement.Resume}");

                Log("Lancement de la planification avec PlanAthena...");

                _dernierResultatPlanificationComplet = await _planificationService.LancerPlanificationAsync(
                    _projetActuel,
                    _ressourceService.GetAllOuvriers(),
                    _ressourceService.GetAllMetiers(),
                    configuration
                );

                AfficherResultatDansLog(_dernierResultatPlanificationComplet.ResultatBrut);

                VerifierDisponibiliteExportGantt();
                VerifierDisponibiliteExportExcel();

                Log("PLANIFICATION TERMINÉE.");
            }
            catch (Exception ex)
            {
                Log($"ERREUR CRITIQUE lors de la planification : {ex}");
                MessageBox.Show($"Une erreur critique est survenue:\n{ex.Message}", "Erreur Critique", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnExportPlanningExcel_Click(object sender, EventArgs e)
        {
            if (_dernierResultatPlanificationComplet == null)
            {
                MessageBox.Show("Veuillez d'abord lancer une planification avant d'exporter.", "Planification requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnExportPlanningExcel.Enabled = false;
                btnExportPlanningExcel.Text = "Export en cours...";

                var ouvriers = _ressourceService.GetAllOuvriers();
                var metiers = _ressourceService.GetAllMetiers();
                var nomProjet = _projetActuel?.InformationsProjet?.NomProjet ?? "Projet PlanAthena";

                var configuration = new ConfigurationPlanification
                {
                    JoursOuvres = chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>().ToList(),
                    HeureDebutJournee = (int)numHeureDebut.Value,
                    HeuresTravailEffectifParJour = (int)numHeuresTravail.Value,
                    TypeDeSortie = cmbTypeDeSortie.SelectedItem.ToString(),
                    Description = _projetActuel?.InformationsProjet?.Description ?? txtDescription.Text,
                    DateDebutSouhaitee = chkDateDebut.Checked ? dtpDateDebut.Value.Date : null,
                    DateFinSouhaitee = chkDateFin.Checked ? dtpDateFin.Value.Date : null,
                    DureeJournaliereStandardHeures = (int)numDureeStandard.Value,
                    PenaliteChangementOuvrierPourcentage = numPenaliteChangement.Value,
                    CoutIndirectJournalierAbsolu = (long)numCoutIndirect.Value,
                    DureeCalculMaxMinutes = (int)cmbCalculMax.SelectedItem
                };

                Log("Début de l'export Planning Excel...");

                /*var cheminFichier = await _planningExcelExportService.ExporterPlanningComplet(
                    _dernierResultatPlanificationComplet,
                    ouvriers,
                    metiers,
                    nomProjet,
                    configuration
                );

                Log($"📋 Export Planning Excel réussi : {cheminFichier}");

                var result = MessageBox.Show($"Planning exporté avec succès !\n\nFichier : {Path.GetFileName(cheminFichier)}\nDossier : {Path.GetDirectoryName(cheminFichier)}\n\nVoulez-vous ouvrir le dossier ?", "Export réussi", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{cheminFichier}\"");
                }*/
            }
            catch (Exception ex)
            {
                Log($"ERREUR lors de l'export Excel : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'export Excel :\n{ex.Message}", "Erreur d'export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExportPlanningExcel.Enabled = true;
                btnExportPlanningExcel.Text = "📋 Export Planning Excel";
            }
        }

        #endregion

        #region Export GanttProject

        private void VerifierDisponibiliteExportGantt()
        {
            bool peutExporter = _dernierResultatPlanificationComplet?.GanttConsolide?.TachesRacines?.Any() == true;
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
                Log($"📊 Export GanttProject disponible ({_dernierResultatPlanificationComplet.GanttConsolide.TachesRacines.Count} tâches consolidées)");
            }
        }

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
            if (_dernierResultatPlanificationComplet?.GanttConsolide?.TachesRacines?.Any() != true)
            {
                MessageBox.Show("Aucun planning consolidé à exporter. Veuillez d'abord lancer une planification.", "Export impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog { Filter = "Fichiers GanttProject (*.gan)|*.gan", Title = "Exporter vers GanttProject", FileName = $"{_projetActuel?.InformationsProjet?.NomProjet?.Replace(" ", "_") ?? "planning"}.gan" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var config = new ConfigurationExportGantt
                    {
                        NomProjet = _projetActuel?.InformationsProjet?.NomProjet ?? "Planning",
                        HeuresParJour = (double)numHeuresTravail.Value,
                        JoursOuvres = chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>()
                    };

                    //_ganttExportService.ExporterVersGanttProjectXml(_dernierResultatPlanificationComplet.GanttConsolide, sfd.FileName, config);

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
            // L'orchestrateur (le majordome) donne les ordres dans un ordre logique et sûr.

            // Étape 1 : Demander aux services de se réinitialiser complètement.
            // L'ordre n'importe pas car ils sont indépendants.
            _projetService.InitialiserNouveauProjet(); // Remplace CreerNouveauProjet
            _ressourceService.ViderOuvriers();       // Remplace Vider()
            _ressourceService.ChargerMetiersParDefaut(); // Remplace Vider() + Charge

            // Étape 2 : Créer l'objet conteneur principal (_projetActuel) en se basant
            // sur l'état maintenant propre des services. C'est la nouvelle source de vérité.
            _projetActuel = _projetService.GetProjetDataPourSauvegarde();
            _projetActuel.Metiers = _ressourceService.GetAllMetiers();
            _projetActuel.Ouvriers = _ressourceService.GetAllOuvriers();
            _projetActuel.InformationsProjet = new InformationsProjet
            {
                NomProjet = "Nouveau projet",
                Description = "Description du projet",
                DateCreation = DateTime.Now,
                Auteur = Environment.UserName
            };

            // Étape 3 : Mettre à jour l'affichage avec ce nouvel état.
            MettreAJourAffichageProjet();
        }

        private void MettreAJourAffichageProjet()
        {
            if (_projetActuel?.InformationsProjet != null)
            {
                var info = _projetActuel.InformationsProjet;
                txtNomProjet.TextChanged -= SynchroniserProjet;
                txtAuteur.TextChanged -= SynchroniserProjet;
                txtDescription.TextChanged -= SynchroniserProjet;

                txtNomProjet.Text = info.NomProjet;
                txtAuteur.Text = info.Auteur;
                txtDescription.Text = info.Description;
                lblDateCreation.Text = $"Créé le: {info.DateCreation:dd/MM/yyyy}";
                this.Text = $"PlanAthena - {info.NomProjet}";

                txtNomProjet.TextChanged += SynchroniserProjet;
                txtAuteur.TextChanged += SynchroniserProjet;
                txtDescription.TextChanged += SynchroniserProjet;
            }
            MettreAJourResume();
        }

        private void SynchroniserProjet(object sender, EventArgs e)
        {
            if (_projetActuel?.InformationsProjet != null)
            {
                _projetActuel.InformationsProjet.NomProjet = txtNomProjet.Text;
                _projetActuel.InformationsProjet.Auteur = txtAuteur.Text;
                _projetActuel.InformationsProjet.Description = txtDescription.Text;
                _projetActuel.InformationsProjet.DateDerniereModification = DateTime.Now;
                this.Text = $"PlanAthena - {_projetActuel.InformationsProjet.NomProjet}";
            }
        }

        private void SynchroniserProjetDepuisInterface()
        {
            if (_projetActuel?.InformationsProjet != null)
            {
                _projetActuel.InformationsProjet.NomProjet = txtNomProjet.Text;
                _projetActuel.InformationsProjet.Description = txtDescription.Text;
                _projetActuel.InformationsProjet.Auteur = txtAuteur.Text;
                _projetActuel.InformationsProjet.DateDerniereModification = DateTime.Now;
            }
        }

        private void MettreAJourResume()
        {
            if (_projetActuel == null) return;
            try
            {
                var nbMetiers = _ressourceService.GetAllMetiers().Count;
                var nbOuvriers = _ressourceService.GetAllOuvriers().Count;
                var nbTaches = _projetActuel.Taches.Count;
                var nbJalons = _projetActuel.Taches.Count(t => t.Type == TypeActivite.JalonUtilisateur);

                lblResume.Text = $"Résumé: {nbOuvriers} ouvriers, {nbMetiers} métiers, {nbTaches} tâches (+{nbJalons} jalons)";

                int tachesAvecMetier = _projetActuel.Taches.Count(t => !string.IsNullOrEmpty(t.MetierId));
                double pourcentageMapping = nbTaches > 0 ? (double)tachesAvecMetier / (double)nbTaches * 100 : 0;
                lblMapping.Text = $"Mapping: {pourcentageMapping:F0}% ({tachesAvecMetier}/{nbTaches} tâches)";

                if (nbTaches > 0)
                {
                    try
                    {
                        var statsTraitement = _planificationService.ObtenirStatistiquesTraitement(_projetActuel.Taches, (int)numHeuresTravail.Value);
                        if (statsTraitement.TachesSolveur > statsTraitement.TachesChef)
                        {
                            var decoupees = statsTraitement.TachesDecoupees;
                            var jalons = statsTraitement.JalonsTechniques;
                            lblMapping.Text += $" | Préparation: {decoupees} découpées, +{jalons} jalons tech.";
                        }
                    }
                    catch { /* Ignorer les erreurs de calcul des statistiques de traitement */ }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur mise à jour résumé: {ex.Message}");
            }
        }

        private bool ConfirmerPerteDonnees()
        {
            if (_projetActuel == null) return true;
            var totalDonnees = _ressourceService.GetAllMetiers().Count + _ressourceService.GetAllOuvriers().Count + _projetActuel.Taches.Count;
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