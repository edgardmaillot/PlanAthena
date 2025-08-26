using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Timer = System.Windows.Forms.Timer;


namespace PlanAthena.View.Planificator

{
    public partial class PlanificatorView : UserControl
    {
        private readonly ApplicationService _applicationService;
        private readonly PlanificationService _planificationService;
        private readonly PlanningExcelExportService _planningExcelExportService;
        private readonly GanttExportService _ganttExportService;
        private readonly RessourceService _ressourceService;
        private readonly CheminsPrefereService _cheminsPrefereService = new CheminsPrefereService();

        private PlanificationResultDto _lastResult = null;
        private int _elapsedSeconds = 0;

        //Variables du Timer pour la barre de progression
        private Timer _solverTimer;
        private int _solverMaxSeconds;
        private int _solverElapsedSeconds;

        public PlanificatorView(ApplicationService applicationService, PlanificationService planificationService, PlanningExcelExportService planningExcelExportService, GanttExportService ganttExportService, RessourceService ressourceService)
        {
            InitializeComponent();
            _applicationService = applicationService;
            _planificationService = planificationService;
            _planningExcelExportService = planningExcelExportService;
            _ganttExportService = ganttExportService;
            _ressourceService = ressourceService;

            _solverTimer = new Timer();
            _solverTimer.Interval = 1000; // 1 seconde
            _solverTimer.Tick += SolverTimer_Tick;

            this.Load += PlanificatorView_Load;
        }

        private void PlanificatorView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            InitializeFields();
            PopulateFormFromSessionConfig();
        }

        private void InitializeFields()
        {
            chkListJoursOuvres.Items.Add(DayOfWeek.Monday);
            chkListJoursOuvres.Items.Add(DayOfWeek.Tuesday);
            chkListJoursOuvres.Items.Add(DayOfWeek.Wednesday);
            chkListJoursOuvres.Items.Add(DayOfWeek.Thursday);
            chkListJoursOuvres.Items.Add(DayOfWeek.Friday);
            chkListJoursOuvres.Items.Add(DayOfWeek.Saturday);
            chkListJoursOuvres.Items.Add(DayOfWeek.Sunday);

            cmbTypeDeSortie.Items.AddRange(new string[] { "Analyse et Estimation", "Optimisation Coût", "Optimisation Délai" });
            cmbCalculMax.Items.AddRange(new object[] { 1,3, 5, 15, 30, 60 });

            dtpDateDebut.Value = DateTime.Today;
            dtpDateFin.Value = DateTime.Today.AddDays(90);
            chkDateDebut.Checked = true;
            chkDateFin.Checked = true;
        }

        private void PopulateFormFromSessionConfig()
        {
            var config = _applicationService.ConfigPlanificationActuelle;
            if (config == null) return;

            for (int i = 0; i < chkListJoursOuvres.Items.Count; i++)
            {
                var day = (DayOfWeek)chkListJoursOuvres.Items[i];
                chkListJoursOuvres.SetItemChecked(i, config.JoursOuvres.Contains(day));
            }

            numHeureDebut.Value = config.HeureDebutJournee;
            numDureeOuverture.Value = config.DureeJournaliereStandardHeures;
            numHeuresTravail.Value = config.HeuresTravailEffectifParJour;
            numPenaliteChangement.Value = config.PenaliteChangementOuvrierPourcentage;
            numCoutIndirect.Value = config.CoutIndirectJournalierAbsolu;
            cmbCalculMax.SelectedItem = config.DureeCalculMaxMinutes > 0 ? (object)config.DureeCalculMaxMinutes : 3;
            cmbTypeDeSortie.SelectedItem = config.TypeDeSortie;

            if (config.DateDebutSouhaitee.HasValue)
            {
                dtpDateDebut.Value = config.DateDebutSouhaitee.Value;
                chkDateDebut.Checked = true;
            }
            if (config.DateFinSouhaitee.HasValue)
            {
                dtpDateFin.Value = config.DateFinSouhaitee.Value;
                chkDateFin.Checked = true;
            }
        }

        private ConfigurationPlanification GetConfigFromForm()
        {
            var config = _applicationService.ConfigPlanificationActuelle;

            config.JoursOuvres = chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>().ToList();
            config.HeureDebutJournee = (int)numHeureDebut.Value;
            config.DureeJournaliereStandardHeures = (int)numDureeOuverture.Value;
            config.HeuresTravailEffectifParJour = (int)numHeuresTravail.Value;
            config.TypeDeSortie = cmbTypeDeSortie.SelectedItem.ToString();
            config.CoutIndirectJournalierAbsolu = (long)numCoutIndirect.Value;
            config.DureeCalculMaxMinutes = (int)cmbCalculMax.SelectedItem;
            config.SeuilJoursDecoupageTache = (int)numSeuilDecoupage.Value;
            config.DateDebutSouhaitee = chkDateDebut.Checked ? dtpDateDebut.Value.Date : (DateTime?)null;
            config.DateFinSouhaitee = chkDateFin.Checked ? dtpDateFin.Value.Date : (DateTime?)null;

            // Correction du type de sortie
            //{ "Analyse et Estimation", "Optimisation Coût", "Optimisation Délai" });
            switch (config.TypeDeSortie)
            {
                case "Analyse et Estimation":
                    config.TypeDeSortie = "ANALYSE_RAPIDE";
                    break;
                case "Optimisation Coût":
                    config.TypeDeSortie = "OPTIMISATION_COUT";
                    break;
                case "Optimisation Délai":
                default:
                    config.TypeDeSortie = "OPTIMISATION_DELAI";
                    break;
            }
            // Correction pour la description requise
            config.Description = _applicationService.ProjetActif?.InformationsProjet?.Description ?? "Pas de description";
            if (string.IsNullOrWhiteSpace(config.Description))
            {
                config.Description = "Projet sans description";
            }

            return config;
        }

        private async void btnLaunch_Click(object sender, EventArgs e)
        {
            if (_applicationService.ProjetActif == null)
            {
                MessageBox.Show("Veuillez charger un projet avant de lancer la planification.", "Projet requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLaunch.Enabled = false;
            SolverProgressBar.Visible = true;
            _elapsedSeconds = 0;
            
            rtbLog.Clear();
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = true;
            planningTimer.Start();

            StartSolverProgress();
            Log("Lancement de la planification...");
            try
            {
                var configuration = GetConfigFromForm();

                _lastResult = await _planificationService.LancerPlanificationAsync(
                    _applicationService.ProjetActif,
                    _ressourceService.GetAllOuvriers(),
                    _ressourceService.GetAllMetiers(),
                    configuration
                );

                AfficherResultatDansLog(_lastResult.ResultatBrut);
                UpdateExportButtonsState();
            }
            catch (Exception ex)
            {
                Log($"ERREUR CRITIQUE : {ex.Message}");
                MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Arrêter les deux barres de progression
                planningTimer.Stop();
                StopSolverProgress();
                progressBar.Visible = false;
                btnLaunch.Enabled = true;
                Log("PLANIFICATION TERMINÉE.");
            }
        }
        private void planningTimer_Tick(object sender, EventArgs e)
        {
            _elapsedSeconds++;
            var Delay = TimeSpan.FromSeconds(_elapsedSeconds);
            //SolverProgressBar.Values.Text = $"Planification en cours... {_elapsedSeconds}s";
            SolverProgressBar.Text = $"Planification en cours... {Delay}s";
            SolverProgressBar.Values.Text = $"Planification en cours... {Delay}s";
            //lblProgressStatus.Text = $"Planification en cours... {_elapsedSeconds}s";
        }
        private void SolverTimer_Tick(object sender, EventArgs e)
        {
            _solverElapsedSeconds++;

            // Calculer le pourcentage de progression
            double progressPercentage = ((double)_solverElapsedSeconds / _solverMaxSeconds) * 100;

            // Limiter à 100%
            if (progressPercentage > 100)
                progressPercentage = 100;

            // Mettre à jour la barre de progression
            SolverProgressBar.Value = (int)progressPercentage;

            // Mettre à jour le texte avec le temps restant
            int remainingSeconds = Math.Max(0, _solverMaxSeconds - _solverElapsedSeconds);
            int remainingMinutes = remainingSeconds / 60;
            int remainingSecondsDisplay = remainingSeconds % 60;

            //SolverProgressBar.Values.Text = $"Calcul en cours ({remainingMinutes}:{remainingSecondsDisplay:D2} restant)";

            // Si le temps maximum est atteint
            if (_solverElapsedSeconds >= _solverMaxSeconds)
            {
                SolverProgressBar.Value = 100;
                SolverProgressBar.Values.Text = "Finalisation...";
            }
        }

        // Méthode pour démarrer la barre de progression du solveur
        private void StartSolverProgress()
        {
            // Récupérer la durée maximale depuis le combo box (en minutes)
            _solverMaxSeconds = (int)cmbCalculMax.SelectedItem * 60;
            _solverElapsedSeconds = 0;

            // Configurer la barre de progression
            SolverProgressBar.Minimum = 0;
            SolverProgressBar.Maximum = 100;
            SolverProgressBar.Value = 0;
            SolverProgressBar.Values.Text = "Calcul en cours";
            SolverProgressBar.Visible = true;

            // Démarrer le timer
            _solverTimer.Start();
        }

        // Méthode pour arrêter la barre de progression du solveur
        private void StopSolverProgress()
        {
            _solverTimer.Stop();
            SolverProgressBar.Value = 100;
            SolverProgressBar.Values.Text = "Terminé";

            // Optionnel : masquer la barre après un délai
            Task.Delay(2000).ContinueWith(_ =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => SolverProgressBar.Visible = false));
                }
                else
                {
                    SolverProgressBar.Visible = false;
                }
            });
        }
        private void UpdateExportButtonsState()
        {
            btnExportPlanningExcel.Enabled = _lastResult?.ResultatBrut?.OptimisationResultat?.Affectations?.Any() == true;
            btnExportGantt.Enabled = _lastResult?.GanttConsolide?.TachesRacines?.Any() == true;
        }

        private async void btnExportPlanningExcel_Click(object sender, EventArgs e)
        {
            if (_lastResult == null || _applicationService.ProjetActif == null)
            {
                MessageBox.Show("Veuillez d'abord lancer une planification pour pouvoir exporter les résultats.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Log("Début de l'export Excel...");
            try
            {
                string initialDirectory = _cheminsPrefereService.ObtenirDernierDossierProjets();
                if (!string.IsNullOrEmpty(_applicationService.CheminFichierProjetActif))
                {
                    initialDirectory = Path.GetDirectoryName(_applicationService.CheminFichierProjetActif);
                }
                var config = GetConfigFromForm();
                var nomProjet = _applicationService.ProjetActif.InformationsProjet.NomProjet;
                using var sfd = new SaveFileDialog
                {
                    Filter = "Fichiers Excel (*.xlsx)|*.xlsx",
                    Title = "Exporter le planning vers Excel",
                    InitialDirectory = initialDirectory,
                    FileName = ConfigurationExport.GenererNomFichierDefaut(_applicationService.ProjetActif.InformationsProjet.NomProjet)
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // On passe l'objet _lastResult complet, qui contient toutes les données brutes nécessaires.
                    var cheminFichier = await _planningExcelExportService.ExporterPlanningComplet(
                    _lastResult,
                    _ressourceService.GetAllOuvriers(),
                    _ressourceService.GetAllMetiers(),
                    nomProjet,
                    config
                );
                    Log($"Export Excel réussi : {sfd.FileName}");
                    MessageBox.Show($"Export terminé : {sfd.FileName}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log($"ERREUR EXPORT EXCEL : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'export : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportGantt_Click(object sender, EventArgs e)
        {
            if (_lastResult == null) return;

            using var sfd = new SaveFileDialog { Filter = "Fichiers GanttProject (*.gan)|*.gan", Title = "Exporter vers GanttProject", FileName = "planning.gan" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var config = new ConfigurationExportGantt
                    {
                        NomProjet = _applicationService.ProjetActif.InformationsProjet.NomProjet,
                        HeuresParJour = (double)numHeuresTravail.Value,
                        JoursOuvres = chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>()
                    };
                    _ganttExportService.ExporterVersGanttProjectXml(_lastResult.GanttConsolide, sfd.FileName, config);
                    Log($"Export Gantt réussi : {sfd.FileName}");
                    MessageBox.Show($"Export terminé : {sfd.FileName}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Log($"ERREUR EXPORT GANTT : {ex.Message}");
                    MessageBox.Show($"Erreur lors de l'export : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Log Helpers
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
                if (analyse.DureeTotaleEstimeeEnSlots.HasValue) Log($"Durée Totale Estimée : {analyse.DureeTotaleEstimeeEnSlots} heures ({analyse.DureeTotaleEstimeeEnSlots / (double)numHeuresTravail.Value:F1} jours de {numHeuresTravail.Value}h)");
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
                            //Log($"      {affectation.DateDebut:HH:mm}-{dateFinEstimee:HH:mm} ({affectation.DureeHeures}h) | Tâche: {affectation.TacheNom} (Bloc: {affectation.BlocId})");
                            Log($"      {affectation.DateDebut:g}-{affectation.DateFin:g} ({affectation.DureeHeures}h) | Tâche: {affectation.TacheNom} (Bloc: {affectation.BlocId})");
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
            
        }
        #endregion
    }
}