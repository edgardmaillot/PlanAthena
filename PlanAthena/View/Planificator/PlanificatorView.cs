// PlanAthena Version 0.4.8 - Version finale corrigée
using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.DTOs.ImportExport;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.UseCases;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Timer = System.Windows.Forms.Timer;

namespace PlanAthena.View.Planificator
{
    public partial class PlanificatorView : UserControl
    {
        private readonly ApplicationService _applicationService;
        private readonly PlanificationOrchestrator _planificationOrchestrator;
        private readonly PlanningService _planningService;
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly TaskStatusService _taskStatusService;
        private readonly PlanningExcelExportService _planningExcelExportService;
        private readonly GanttExportService _ganttExportService;
        private readonly CheminsPrefereService _cheminsPrefereService = new CheminsPrefereService();
        private PlanificationRunResult _lastRunResult = null;
        private int _elapsedSeconds = 0;
        private Timer _solverTimer;
        private int _solverMaxSeconds;
        private int _solverElapsedSeconds;

        public PlanificatorView(
            ApplicationService applicationService,
            PlanificationOrchestrator planificationOrchestrator,
            PlanningService planningService,
            ProjetService projetService,
            RessourceService ressourceService,
            TaskStatusService taskStatusService,
            PlanningExcelExportService planningExcelExportService,
            GanttExportService ganttExportService)
        {
            InitializeComponent();
            _applicationService = applicationService;
            _planificationOrchestrator = planificationOrchestrator;
            _planningService = planningService;
            _projetService = projetService;
            _ressourceService = ressourceService;
            _taskStatusService = taskStatusService;
            _planningExcelExportService = planningExcelExportService;
            _ganttExportService = ganttExportService;
            _solverTimer = new Timer { Interval = 1000 };
            _solverTimer.Tick += SolverTimer_Tick;
            this.Load += PlanificatorView_Load; // Ligne correcte
        }

        // --- MÉTHODE RÉINTRODUITE ---
        private void PlanificatorView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            InitializeFields();
            PopulateFormFromSessionConfig();
            navigatorResultats.Visible = false;
        }

        #region Initialisation et Configuration du Formulaire

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
            cmbCalculMax.Items.AddRange(new object[] { 1, 3, 5, 10, 15, 30, 60 });

            dtpDateDebut.Value = DateTime.Today;
            dtpDateFin.Value = DateTime.Today.AddDays(90);
            chkDateDebut.Checked = true;
            chkDateFin.Checked = true;

            dgvAnalyseOuvriers.AutoGenerateColumns = false; // CORRIGÉ : dgvAnalyseOuvriers
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
            cmbCalculMax.SelectedItem = config.DureeCalculMaxMinutes > 0 ? (object)config.DureeCalculMaxMinutes : 5;
            cmbTypeDeSortie.SelectedItem = config.TypeDeSortie;

            if (config.DateDebutSouhaitee.HasValue) dtpDateDebut.Value = config.DateDebutSouhaitee.Value;
            if (config.DateFinSouhaitee.HasValue) dtpDateFin.Value = config.DateFinSouhaitee.Value;
        }

        private ConfigurationPlanification GetConfigFromForm()
        {
            var config = _applicationService.ConfigPlanificationActuelle;

            config.JoursOuvres = chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>().ToList();
            config.HeureDebutJournee = (int)numHeureDebut.Value;
            config.DureeJournaliereStandardHeures = (int)numDureeOuverture.Value;
            config.HeuresTravailEffectifParJour = (int)numHeuresTravail.Value;
            config.TypeDeSortie = cmbTypeDeSortie.SelectedItem.ToString();
            config.PenaliteChangementOuvrierPourcentage = numPenaliteChangement.Value;
            config.CoutIndirectJournalierAbsolu = (long)numCoutIndirect.Value;
            config.DureeCalculMaxMinutes = (int)cmbCalculMax.SelectedItem;
            config.SeuilJoursDecoupageTache = (int)numSeuilDecoupage.Value;

            config.DateDebutSouhaitee = chkDateDebut.Checked ? dtpDateDebut.Value.Date : DateTime.Today;
            config.DateFinSouhaitee = chkDateFin.Checked ? dtpDateFin.Value.Date : DateTime.Today.AddYears(5);

            config.Description = _applicationService.ProjetActif?.InformationsProjet?.NomProjet ?? "Nouveau Projet";

            switch (cmbTypeDeSortie.SelectedItem.ToString())
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

            return config;
        }
        #endregion

        #region Logique Principale et Barre de Progression
        private async void btnLaunch_Click(object sender, EventArgs e)
        {
            if (_applicationService.ProjetActif == null)
            {
                MessageBox.Show("Veuillez charger un projet avant de lancer la planification.", "Projet requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLaunch.Enabled = false;
            navigatorResultats.Visible = false;
            rtbLog.Clear();
            _elapsedSeconds = 0;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = true;
            planningTimer.Start();
            StartSolverProgress();

            Log("Lancement de la planification...");

            try
            {
                var configuration = GetConfigFromForm();
                _lastRunResult = await _planificationOrchestrator.ExecuteAsync(configuration);
                AfficherResultats(_lastRunResult);
                UpdateExportButtonsState();
            }
            catch (Exception ex)
            {
                Log($"ERREUR CRITIQUE : {ex.Message}");
                MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                planningTimer.Stop();
                StopSolverProgress();
                progressBar.Visible = false;
                btnLaunch.Enabled = true;
                Log("PLANIFICATION TERMINÉE.");
            }
        }
        #endregion

        #region Affichage des Résultats

        private void AfficherResultats(PlanificationRunResult runResult)
        {
            if (runResult == null) return;

            navigatorResultats.Visible = true;
            var culture = CultureInfo.GetCultureInfo("fr-FR");

            // Traitement du statut du solveur en premier, car il est présent dans plusieurs cas.
            string solverStatus = runResult.RawResult?.OptimisationResultat?.Status.ToString();
            UpdateSolverStatusDisplay(solverStatus);

            if (runResult.AnalysisReport != null)
            {
                // Cas d'un run d'optimisation (coût ou délai)
                var synthese = runResult.AnalysisReport.SyntheseProjet;
                lblCoutTotalValue.Text = synthese.CoutTotalProjet.ToString("C", culture);
                lblCoutRhValue.Text = synthese.CoutTotalRh.ToString("C", culture);
                lblCoutIndirectValue.Text = synthese.CoutTotalIndirect.ToString("C", culture);
                lblJoursHommeValue.Text = synthese.EffortTotalJoursHomme.ToString("0.0");
                lblDureeValue.Text = synthese.DureeJoursOuvres.ToString();

                AfficherAnalyseRessources(runResult.AnalysisReport);
            }
            else if (runResult.RawResult?.AnalyseStatiqueResultat != null)
            {
                // Cas d'une analyse rapide (estimation)
                var estimation = runResult.RawResult.AnalyseStatiqueResultat;

                decimal coutRhEstime = (estimation.CoutTotalEstime ?? 0) / 100.0m;
                long coutIndirectJournalier = (long)numCoutIndirect.Value;
                decimal heuresTravailParJour = numHeuresTravail.Value > 0 ? numHeuresTravail.Value : 8.0m;

                decimal dureeEstimeeEnJours = Math.Ceiling((decimal)(estimation.DureeTotaleEstimeeEnSlots ?? 0) / heuresTravailParJour);
                decimal coutIndirectEstime = dureeEstimeeEnJours * coutIndirectJournalier;

                decimal coutTotalEstimeBrut = coutRhEstime + coutIndirectEstime;
                decimal coutTotalFinal = Math.Ceiling(coutTotalEstimeBrut / 1000) * 1000;

                lblCoutTotalValue.Text = coutTotalFinal.ToString("C", culture);
                lblCoutIndirectValue.Text = coutIndirectEstime.ToString("C", culture);
                lblDureeValue.Text = $"{estimation.DureeTotaleEstimeeEnSlots ?? 0} heures";
                lblCoutRhValue.Text = "N/A";
                lblJoursHommeValue.Text = "N/A";

                NettoyerAnalyseRessources();
            }
            else
            {
                // Cas d'erreur ou de résultat inattendu
                NettoyerAffichageKpis();
            }

            AfficherResultatDansLog(runResult);
            navigatorResultats.SelectedPage = tabPageSynthese;
        }

        private void UpdateSolverStatusDisplay(string status)
        {
            khgStatutSolveur.Visible = !string.IsNullOrEmpty(status);
            if (string.IsNullOrEmpty(status)) return;

            // Palette de couleurs pour les headers
            var defaultBackColor = SystemColors.ControlDark; // Couleur par défaut
            var optimalColor = Color.FromArgb(0, 150, 0); // Vert foncé
            var feasibleColor = Color.FromArgb(204, 132, 0); // Orange foncé
            var infeasibleColor = Color.FromArgb(192, 0, 0); // Rouge foncé

            switch (status.ToUpperInvariant())
            {
                case "OPTIMAL":
                    khgStatutSolveur.ValuesPrimary.Heading = "OPTIMAL";
                    khgStatutSolveur.StateCommon.HeaderPrimary.Back.Color1 = optimalColor;
                    lblStatutExplication.Text = "La meilleure solution\npossible a été trouvée.";
                    break;

                case "FEASIBLE":
                    khgStatutSolveur.ValuesPrimary.Heading = "FAISABLE";
                    khgStatutSolveur.StateCommon.HeaderPrimary.Back.Color1 = feasibleColor;
                    lblStatutExplication.Text = "Une solution a été trouvée,\nmais elle n'est peut-être pas la meilleure.\nLe temps de calcul était peut-être insuffisant.";
                    break;

                case "INFEASIBLE":
                    khgStatutSolveur.ValuesPrimary.Heading = "IMPOSSIBLE";
                    khgStatutSolveur.StateCommon.HeaderPrimary.Back.Color1 = infeasibleColor;
                    lblStatutExplication.Text = "Aucune solution possible.\nAccordez plus de délai à l'IA.";
                    break;

                default:
                    khgStatutSolveur.ValuesPrimary.Heading = status.ToUpperInvariant();
                    khgStatutSolveur.StateCommon.HeaderPrimary.Back.Color1 = defaultBackColor;
                    lblStatutExplication.Text = "Statut du solveur non reconnu.";
                    break;
            }
        }

        private void AfficherAnalyseRessources(AnalysisReport report)
        {
            dgvAnalyseOuvriers.DataSource = null; // CORRIGÉ
            dgvAnalyseOuvriers.DataSource = report.AnalysesOuvriers; // CORRIGÉ
            foreach (DataGridViewRow row in dgvAnalyseOuvriers.Rows) // CORRIGÉ
            {
                if (row.Cells["colTauxFragmentation"].Value != null)
                {
                    var fragValue = Convert.ToDouble(row.Cells["colTauxFragmentation"].Value) * 100;
                    if (fragValue > 75) row.Cells["colTauxFragmentation"].Style.BackColor = Color.LightCoral;
                    else if (fragValue > 50) row.Cells["colTauxFragmentation"].Style.BackColor = Color.LightSalmon;
                }
            }

            chartChargeJournaliere.Series.Clear();
            var series = new Series("Charge") { ChartType = SeriesChartType.Column, XValueType = ChartValueType.Date };
            foreach (var chargeJour in report.ChargeJournaliere.OrderBy(kvp => kvp.Key))
            {
                series.Points.AddXY(chargeJour.Key, chargeJour.Value);
            }
            chartChargeJournaliere.Series.Add(series);
            chartChargeJournaliere.ChartAreas[0].AxisX.LabelStyle.Format = "dd/MM";
            chartChargeJournaliere.ChartAreas[0].RecalculateAxesScale();
        }

        private void NettoyerAffichageKpis()
        {
            lblCoutTotalValue.Text = "N/A";
            lblCoutRhValue.Text = "N/A";
            lblCoutIndirectValue.Text = "N/A";
            lblJoursHommeValue.Text = "N/A";
            lblDureeValue.Text = "N/A";
            NettoyerAnalyseRessources();
        }

        private void NettoyerAnalyseRessources()
        {
            dgvAnalyseOuvriers.DataSource = null; // CORRIGÉ
            chartChargeJournaliere.Series.Clear();
        }

        #endregion

        #region Exports et États
        private void UpdateExportButtonsState()
        {
            // L'export est possible uniquement si un run d'optimisation a été effectué
            // et a produit un rapport d'analyse complet.
            bool canExport = _lastRunResult?.AnalysisReport != null;
            btnExportPlanningExcel.Enabled = canExport;
            btnExportGantt.Enabled = canExport;
        }

        private async void btnExportPlanningExcel_Click(object sender, EventArgs e)
        {
            if (_lastRunResult?.AnalysisReport == null || _planningService.GetCurrentPlanning() == null)
            {
                MessageBox.Show("Aucun planning optimisé n'est disponible pour l'export.", "Export impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var saveFileDialog = new SaveFileDialog
            {
                Filter = "Fichiers Excel (*.xlsx)|*.xlsx",
                Title = "Exporter le planning au format Excel",
                InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierExport(),
                FileName = $"Planning_{_applicationService.ProjetActif.InformationsProjet.NomProjet}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        btnExportPlanningExcel.Enabled = false;

                        var projetStructure = _projetService.GetProjetDataPourSauvegarde();
                        var currentPlanning = _planningService.GetCurrentPlanning();
                        var currentReport = _lastRunResult.AnalysisReport;
                        var currentConfig = _applicationService.ConfigPlanificationActuelle;
                        // NOUVEAU : Récupération de la liste complète des ouvriers depuis la source de vérité.
                        var poolOuvriers = _ressourceService.GetAllOuvriers();

                        var exportData = new ExportDataProjetDto
                        {
                            NomProjet = _applicationService.ProjetActif.InformationsProjet.NomProjet,
                            Configuration = currentConfig,
                            Planning = currentPlanning,
                            Report = currentReport,
                            ProjetStructure = projetStructure,
                            PoolOuvriers = poolOuvriers // Remplissage de la nouvelle propriété.
                        };

                        await Task.Run(() => _planningExcelExportService.ExporterPlanningComplet(exportData, saveFileDialog.FileName));

                        MessageBox.Show($"Le planning a été exporté avec succès vers :\n{saveFileDialog.FileName}", "Export réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Une erreur est survenue lors de l'export Excel :\n{ex.Message}", "Erreur d'export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log($"ERREUR EXPORT EXCEL: {ex}");
                    }
                    finally
                    {
                        btnExportPlanningExcel.Enabled = true;
                    }
                }
            }
        }

        private async void btnExportGantt_Click(object sender, EventArgs e)
        {
            if (_lastRunResult?.AnalysisReport == null || _planningService.GetCurrentPlanning() == null)
            {
                MessageBox.Show("Aucun planning optimisé n'est disponible pour l'export.", "Export impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var saveFileDialog = new SaveFileDialog
            {
                Filter = "Fichiers GanttProject (*.gan)|*.gan",
                Title = "Exporter vers GanttProject",
                InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierExport(),
                FileName = $"Gantt_{_applicationService.ProjetActif.InformationsProjet.NomProjet}_{DateTime.Now:yyyyMMdd_HHmm}.gan"
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        btnExportGantt.Enabled = false;

                        // La collecte des données est identique à celle de l'export Excel.
                        var projetStructure = _projetService.GetProjetDataPourSauvegarde();
                        var currentPlanning = _planningService.GetCurrentPlanning();
                        var currentReport = _lastRunResult.AnalysisReport;
                        var currentConfig = _applicationService.ConfigPlanificationActuelle;

                        var exportData = new ExportDataProjetDto
                        {
                            NomProjet = _applicationService.ProjetActif.InformationsProjet.NomProjet,
                            Configuration = currentConfig,
                            Planning = currentPlanning,
                            Report = currentReport,
                            ProjetStructure = projetStructure
                        };

                        // L'appel au service d'export est asynchrone pour ne pas geler l'UI.
                        await Task.Run(() => _ganttExportService.ExporterVersGanttProjectXml(exportData, saveFileDialog.FileName));

                        MessageBox.Show($"Le projet a été exporté avec succès vers :\n{saveFileDialog.FileName}", "Export Gantt réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Une erreur est survenue lors de l'export Gantt :\n{ex.Message}", "Erreur d'export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log($"ERREUR EXPORT GANTT: {ex}");
                    }
                    finally
                    {
                        btnExportGantt.Enabled = true;
                    }
                }
            }
        }
        #endregion

        #region Log Helpers & Barre de Progression

        private void Log(string message)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action<string>(Log), message);
                return;
            }
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            rtbLog.ScrollToCaret();
        }

        private void AfficherResultatDansLog(PlanificationRunResult runResult)
        {
            if (runResult?.RawResult == null)
            {
                Log("Le résultat de la planification est invalide.");
                return;
            }

            var sb = new StringBuilder();
            var rawResult = runResult.RawResult;

            sb.AppendLine($"--- Résultat pour le Chantier ID: {rawResult.ChantierId} ---");
            sb.AppendLine($"État du Traitement: {rawResult.Etat}");

            if (rawResult.OptimisationResultat != null)
            {
                sb.AppendLine($"Statut du Solveur: {rawResult.OptimisationResultat.Status}");
            }

            if (rawResult.Messages.Any())
            {
                sb.AppendLine("\nMessages de validation et suggestions :");
                foreach (var msg in rawResult.Messages)
                {
                    string details = !string.IsNullOrEmpty(msg.ElementId) ? $" (Élément: {msg.ElementId})" : "";
                    sb.AppendLine($"  [{msg.Type}] ({msg.CodeMessage}) {msg.Message}{details}");
                }
            }

            if (runResult.MetierTensionReport != null)
            {
                sb.AppendLine("\n--- Analyse Rapide des Tensions ---");
                sb.AppendLine($"Conclusion : {runResult.MetierTensionReport.Conclusion}");
                if (runResult.MetierTensionReport.Repartition.Any())
                {
                    sb.AppendLine("Répartition des métiers clés suggérés :");
                    foreach (var repartition in runResult.MetierTensionReport.Repartition)
                    {
                        var nomMetier = _ressourceService.GetMetierById(repartition.MetierId)?.Nom ?? repartition.MetierId;
                        sb.AppendLine($"  - {nomMetier} : {repartition.Count} ouvrier(s)");
                    }
                }
            }

            if (runResult.AnalysisReport != null)
            {
                sb.AppendLine("\n--- Planning Détaillé ---");
                var planningParJour = _taskStatusService.RetournePlanningDetailleParJour();
                if (planningParJour.Any())
                {
                    foreach (var jour in planningParJour)
                    {
                        sb.AppendLine($"\n  [ Jour: {jour.Jour:dddd dd MMMM yyyy} ]");
                        foreach (var ouvrier in jour.Ouvriers)
                        {
                            sb.AppendLine($"    > Ouvrier: {ouvrier.NomOuvrier}");
                            foreach (var affectation in ouvrier.Affectations)
                            {
                                sb.AppendLine($"      ({affectation.DureeHeures}h) | Tâche: {affectation.TacheNom} (Bloc: {affectation.BlocId})");
                            }
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Aucune affectation générée.");
                }
            }

            Log(sb.ToString());
        }

        private void planningTimer_Tick(object sender, EventArgs e)
        {
            _elapsedSeconds++;
            var delay = TimeSpan.FromSeconds(_elapsedSeconds);
            SolverProgressBar.Text = $"Planification en cours... {delay:g}";
        }

        private void SolverTimer_Tick(object sender, EventArgs e)
        {
            _elapsedSeconds++;
            if (_solverMaxSeconds <= 0) return;

            double progressPercentage = ((double)_solverElapsedSeconds / _solverMaxSeconds) * 100;
            if (progressPercentage > 100) progressPercentage = 100;
            SolverProgressBar.Value = (int)progressPercentage;

            int remainingSeconds = Math.Max(0, _solverMaxSeconds - _solverElapsedSeconds);
            int remainingMinutes = remainingSeconds / 60;
            int remainingSecondsDisplay = remainingSeconds % 60;
            SolverProgressBar.Values.Text = $"Calcul en cours ({remainingMinutes}:{remainingSecondsDisplay:D2} restant)";
        }

        private void StartSolverProgress()
        {
            _solverMaxSeconds = (int)cmbCalculMax.SelectedItem * 60;
            _solverElapsedSeconds = 0;
            SolverProgressBar.Minimum = 0;
            SolverProgressBar.Maximum = 100;
            SolverProgressBar.Value = 0;
            SolverProgressBar.Values.Text = "Calcul en cours";
            SolverProgressBar.Visible = true;
            _solverTimer.Start();
        }

        private void StopSolverProgress()
        {
            _solverTimer.Stop();
            SolverProgressBar.Value = 100;
            SolverProgressBar.Values.Text = "Terminé";
            Task.Delay(2000).ContinueWith(_ =>
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.Invoke(new Action(() => SolverProgressBar.Visible = false));
                }
            });
        }
        #endregion
    }
}