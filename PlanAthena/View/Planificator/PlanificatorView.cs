using Krypton.Toolkit;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
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
        private readonly PlanificationService _planificationService;
        private readonly PlanningResultatService _planningResultatService;
        private readonly PlanningExcelExportService _planningExcelExportService;
        private readonly GanttExportService _ganttExportService;
        private readonly RessourceService _ressourceService;
        private readonly CheminsPrefereService _cheminsPrefereService = new CheminsPrefereService();

        private PlanificationResultDto _lastResult = null;
        private AnalysePlanificationDto _lastAnalyse = null;
        private int _elapsedSeconds = 0;

        //Variables du Timer pour la barre de progression
        private Timer _solverTimer;
        private int _solverMaxSeconds;
        private int _solverElapsedSeconds;

        public PlanificatorView(
            ApplicationService applicationService,
            PlanificationService planificationService,
            PlanningResultatService planningResultatService,
            PlanningExcelExportService planningExcelExportService,
            GanttExportService ganttExportService,
            RessourceService ressourceService)
        {
            InitializeComponent();
            _applicationService = applicationService;
            _planificationService = planificationService;
            _planningResultatService = planningResultatService;
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

            dgvAnalyseOuvriers.AutoGenerateColumns = false;
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
            //chkDateDebut.Checked = config.DateDebutSouhaitee.HasValue;
            //chkDateFin.Checked = config.DateFinSouhaitee.HasValue;
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
            SolverProgressBar.Visible = true;
            btnLaunch.Enabled = false;
            navigatorResultats.Visible = false;
            rtbLog.Clear();
            _elapsedSeconds = 0;

            // Démarrer la barre de progression du planning (existante)
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = true;
            planningTimer.Start();

            // Démarrer la barre de progression du solveur (nouvelle)
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

                _lastAnalyse = _planningResultatService.GenererRapportAnalyse(
                    _lastResult,
                    _ressourceService.GetAllOuvriers(),
                    _ressourceService.GetAllMetiers(),
                    configuration
                );

                AfficherResultats(_lastAnalyse, _lastResult.ResultatBrut);
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
            //lblProgressStatus.Text = $"Planification en cours... {_elapsedSeconds}s";
        }

        #endregion

        #region Affichage des Résultats

        private void AfficherResultats(AnalysePlanificationDto rapport, PlanAthena.Core.Facade.Dto.Output.ProcessChantierResultDto resultatBrut)
        {
            if (rapport == null || resultatBrut == null) return;

            navigatorResultats.Visible = true;

            AfficherSynthese(rapport, resultatBrut);
            AfficherAnalyseRessources(rapport.AnalyseOuvriers, resultatBrut.OptimisationResultat?.Affectations);
            AfficherResultatDansLog(resultatBrut);

            navigatorResultats.SelectedPage = tabPageSynthese;
        }

        private void AfficherSynthese(AnalysePlanificationDto rapport, PlanAthena.Core.Facade.Dto.Output.ProcessChantierResultDto resultatBrut)
        {
            var synthese = rapport.SyntheseProjet;

            if (resultatBrut.OptimisationResultat != null && resultatBrut.OptimisationResultat.CoutTotalEstime.HasValue)
            {
                var optim = resultatBrut.OptimisationResultat;
                lblCoutTotalValue.Text = (optim.CoutTotalEstime / 100.0m)?.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) ?? "N/A";
                lblCoutRhValue.Text = (optim.CoutTotalRhEstime / 100.0m)?.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) ?? "N/A";
                lblCoutIndirectValue.Text = (optim.CoutTotalIndirectEstime / 100.0m)?.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) ?? "N/A";
                lblJoursHommeValue.Text = synthese.TotalJoursHommeTravailles.ToString();
            }
            else if (resultatBrut.AnalyseStatiqueResultat != null && resultatBrut.AnalyseStatiqueResultat.CoutTotalEstime.HasValue)
            {
                var estimation = resultatBrut.AnalyseStatiqueResultat;
                lblCoutTotalValue.Text = (estimation.CoutTotalEstime / 100.0m)?.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) ?? "N/A";
                lblCoutRhValue.Text = "N/A";
                lblCoutIndirectValue.Text = "N/A";
                lblJoursHommeValue.Text = "N/A"; // ✅ CORRECTION : Pas d'effort calculable en mode estimation
            }
            else
            {
                lblCoutTotalValue.Text = "N/A";
                lblCoutRhValue.Text = "N/A";
                lblCoutIndirectValue.Text = "N/A";
                lblJoursHommeValue.Text = "N/A";
            }

            lblDureeValue.Text = $"{synthese.DureeJoursCalendaires} jours";
        }

        private void AfficherAnalyseRessources(List<AnalyseOuvrierDto> analyseOuvriers, IReadOnlyList<PlanAthena.Core.Facade.Dto.Output.AffectationDto> affectations)
        {
            dgvAnalyseOuvriers.DataSource = null;
            if (analyseOuvriers != null && analyseOuvriers.Any())
            {
                dgvAnalyseOuvriers.DataSource = analyseOuvriers;
                foreach (DataGridViewRow row in dgvAnalyseOuvriers.Rows)
                {
                    if (row.Cells["colTauxFragmentation"].Value != null)
                    {
                        var fragValue = Convert.ToDouble(row.Cells["colTauxFragmentation"].Value);
                        if (fragValue > 75) row.Cells["colTauxFragmentation"].Style.BackColor = Color.LightCoral;
                        else if (fragValue > 50) row.Cells["colTauxFragmentation"].Style.BackColor = Color.LightSalmon;
                    }
                }
            }

            chartChargeJournaliere.Series.Clear();
            var series = new Series("Charge") { ChartType = SeriesChartType.Column, XValueType = ChartValueType.Date };
            if (affectations != null && affectations.Any())
            {
                var chargeParJour = affectations
                    .GroupBy(a => a.DateDebut.Date)
                    .Select(g => new { Jour = g.Key, NbOuvriers = g.Select(a => a.OuvrierId).Distinct().Count() })
                    .OrderBy(x => x.Jour);
                foreach (var item in chargeParJour)
                {
                    series.Points.AddXY(item.Jour, item.NbOuvriers);
                }
            }
            chartChargeJournaliere.Series.Add(series);
            chartChargeJournaliere.ChartAreas[0].AxisX.LabelStyle.Format = "dd/MM";
            chartChargeJournaliere.ChartAreas[0].RecalculateAxesScale();
        }

        #endregion

        #region Exports et États

        private void UpdateExportButtonsState()
        {
            bool hasResults = _lastResult != null &&
                              ((_lastResult.ResultatBrut?.OptimisationResultat?.Affectations?.Any() ?? false) ||
                               _lastResult.ResultatBrut?.AnalyseStatiqueResultat != null);

            btnExportPlanningExcel.Enabled = hasResults;
            btnExportGantt.Enabled = hasResults && (_lastResult.ResultatBrut?.OptimisationResultat?.Affectations?.Any() ?? false);
        }

        private async void btnExportPlanningExcel_Click(object sender, EventArgs e)
        {
            if (_lastAnalyse == null || _lastResult == null || _applicationService.ProjetActif == null)
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
                    await _planningExcelExportService.ExporterPlanningComplet(
                        _lastAnalyse,
                        _lastResult, // L'objet complet PlanificationResultDto
                        GetConfigFromForm(),
                        sfd.FileName
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
            if (_lastResult == null || _applicationService.ProjetActif == null)
            {
                MessageBox.Show("Veuillez d'abord lancer une planification pour pouvoir exporter les résultats.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Fichiers GanttProject (*.gan)|*.gan",
                Title = "Exporter le planning vers GanttProject",
                FileName = $"Gantt_{_applicationService.ProjetActif.InformationsProjet.NomProjet}.gan"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var config = new ConfigurationExportGantt { NomProjet = _applicationService.ProjetActif.InformationsProjet.NomProjet };
                    var tachesOriginales = _applicationService.ProjetActif.Taches;
                    _ganttExportService.ExporterVersGanttProjectXml(
                        _lastResult,
                        _lastResult.ParentIdParSousTacheId,
                        tachesOriginales,
                        sfd.FileName,
                        config);
                    MessageBox.Show($"Le planning a été exporté avec succès vers :\n{sfd.FileName}", "Exportation Réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue lors de l'exportation vers GanttProject :\n{ex.Message}", "Erreur d'Exportation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Log Helpers
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

        // Dans le fichier PlanificatorView.cs

        private void AfficherResultatDansLog(PlanAthena.Core.Facade.Dto.Output.ProcessChantierResultDto resultat)
        {
            if (resultat == null)
            {
                Log("Le résultat retourné par la façade est null.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"--- Résultat pour le Chantier ID: {resultat.ChantierId} ---");
            sb.AppendLine($"État du Traitement: {resultat.Etat}");

            if (resultat.Messages.Any())
            {
                sb.AppendLine("\nMessages de validation et suggestions :");
                foreach (var msg in resultat.Messages)
                {
                    string details = !string.IsNullOrEmpty(msg.ElementId) ? $" (Élément: {msg.ElementId})" : "";
                    sb.AppendLine($"  [{msg.Type}] ({msg.CodeMessage}) {msg.Message}{details}");
                }
            }

            if (resultat.AnalyseStatiqueResultat != null)
            {
                sb.AppendLine("\n--- Analyse Statique et Estimation Préliminaire ---");
                var analyse = resultat.AnalyseStatiqueResultat;
                if (analyse.CoutTotalEstime.HasValue) sb.AppendLine($"Coût Total Estimé : {(analyse.CoutTotalEstime / 100.0m)?.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"))}");
                if (analyse.DureeTotaleEstimeeEnSlots.HasValue) sb.AppendLine($"Durée Totale Estimée : {analyse.DureeTotaleEstimeeEnSlots} heures");

                if (analyse.OuvriersClesSuggereIds.Any())
                {
                    var tousLesOuvriers = _ressourceService.GetAllOuvriers();
                    var tousLesMetiers = _ressourceService.GetAllMetiers();

                    var ouvriersMap = tousLesOuvriers.ToDictionary(o => o.OuvrierId);
                    var metiersMap = tousLesMetiers.ToDictionary(m => m.MetierId, m => m.Nom);

                    var nomsOuvriersSuggérés = analyse.OuvriersClesSuggereIds
                        .Select(id =>
                        {
                            if (ouvriersMap.TryGetValue(id, out var ouvrier))
                            {
                                var competencePrincipale = ouvrier.Competences.FirstOrDefault(c => c.EstMetierPrincipal);
                                if (competencePrincipale != null && metiersMap.TryGetValue(competencePrincipale.MetierId, out var nomMetier))
                                {
                                    return $"{ouvrier.Prenom} {ouvrier.Nom} ({nomMetier})";
                                }
                                return $"{ouvrier.Prenom} {ouvrier.Nom}";
                            }
                            return id;
                        })
                        .ToList();

                    sb.AppendLine($"Ouvriers clés suggérés : {string.Join(", ", nomsOuvriersSuggérés)}");
                }
            }

            // --- DÉBUT DE LA LOGIQUE D'AFFICHAGE DU PLANNING DÉTAILLÉ ---
            if (resultat.OptimisationResultat?.Affectations != null)
            {
                sb.AppendLine("\n--- Planning Détaillé (Affectations) ---");

                var affectations = resultat.OptimisationResultat.Affectations;
                var feuillesDeTemps = resultat.OptimisationResultat.FeuillesDeTemps ?? new List<PlanAthena.Core.Facade.Dto.Output.FeuilleDeTempsOuvrierDto>();
                var configuration = GetConfigFromForm(); // Récupérer la configuration pour les heures

                // 1. Pré-traiter TOUTES les affectations pour un accès rapide par heure
                var tacheParHeureUtc = new Dictionary<DateTime, PlanAthena.Core.Facade.Dto.Output.AffectationDto>();
                foreach (var affectation in affectations)
                {
                    for (int i = 0; i < affectation.DureeHeures; i++)
                    {
                        tacheParHeureUtc[affectation.DateDebut.AddHours(i)] = affectation;
                    }
                }

                // 2. Unifier et trier tous les jours concernés
                var joursAvecFeuilleDeTemps = feuillesDeTemps.SelectMany(f => f.PlanningJournalier.Keys).Select(d => d.Date);
                var tousLesJours = joursAvecFeuilleDeTemps.Distinct().OrderBy(d => d);

                foreach (var jour in tousLesJours)
                {
                    var jourUtc = new DateTime(jour.Year, jour.Month, jour.Day, 0, 0, 0, DateTimeKind.Utc);

                    // 3. Trouver les ouvriers ayant travaillé ce jour-là via les feuilles de temps
                    var ouvriersDuJour = feuillesDeTemps
                        .Where(f => f.PlanningJournalier.ContainsKey(jourUtc))
                        .OrderBy(f => f.OuvrierNom)
                        .ToList();

                    if (!ouvriersDuJour.Any()) continue; // Passer les jours sans travail planifié

                    sb.AppendLine($"\n  [ Jour: {jour:dddd dd MMMM yyyy} ]");

                    foreach (var feuilleOuvrier in ouvriersDuJour)
                    {
                        sb.AppendLine($"    > Ouvrier: {feuilleOuvrier.OuvrierNom}");

                        var masque = feuilleOuvrier.PlanningJournalier[jourUtc];

                        // 4. Construire la chaîne binaire correctement, comme une timeline
                        var binaireSb = new StringBuilder(24);
                        for (int h = 0; h < 24; h++)
                        {
                            // On vérifie le bit correspondant à l'heure (décalé par l'heure de début du chantier)
                            int bitIndex = h - configuration.HeureDebutJournee;
                            if (bitIndex >= 0 && (masque & (1L << bitIndex)) != 0)
                            {
                                binaireSb.Append('1');
                            }
                            else
                            {
                                binaireSb.Append('0');
                            }
                        }
                        sb.AppendLine($"          Planning Binaire (24h): {binaireSb}");

                        // 5. Reconstituer les créneaux de travail du jour à partir du masque et du dictionnaire
                        var creneauxDuJour = new List<(PlanAthena.Core.Facade.Dto.Output.AffectationDto affectation, int duree)>();
                        for (int h = 0; h < 24; h++)
                        {
                            int bitIndex = h - configuration.HeureDebutJournee;
                            if (bitIndex >= 0 && (masque & (1L << bitIndex)) != 0)
                            {
                                var heureCouranteUtc = jourUtc.AddHours(h);
                                if (tacheParHeureUtc.TryGetValue(heureCouranteUtc, out var affectation))
                                {
                                    if (creneauxDuJour.Any() && creneauxDuJour.Last().affectation.TacheId == affectation.TacheId)
                                    {
                                        var last = creneauxDuJour.Last();
                                        last.duree++;
                                        creneauxDuJour[creneauxDuJour.Count - 1] = last;
                                    }
                                    else
                                    {
                                        creneauxDuJour.Add((affectation, 1));
                                    }
                                }
                            }
                        }

                        if (!creneauxDuJour.Any() && masque > 0)
                        {
                            sb.AppendLine($"          (Travail planifié sans affectation détaillée)");
                        }
                        else
                        {
                            foreach (var (affectation, duree) in creneauxDuJour)
                            {
                                // Pour afficher l'heure de début, on doit retrouver la première heure de ce créneau
                                var heureDebutCreneau = affectation.DateDebut.Date == jour
                                    ? affectation.DateDebut
                                    : jour.AddHours(configuration.HeureDebutJournee); // Approximation si la tâche a commencé avant

                                // Recalcul plus précis de l'heure de début
                                for (int h = 0; h < 24; h++)
                                {
                                    var heureCouranteUtc = jourUtc.AddHours(h);
                                    if (tacheParHeureUtc.TryGetValue(heureCouranteUtc, out var aff) && aff.TacheId == affectation.TacheId)
                                    {
                                        heureDebutCreneau = heureCouranteUtc;
                                        break;
                                    }
                                }

                                var heureFinCreneau = heureDebutCreneau.AddHours(duree);
                                sb.AppendLine($"      {heureDebutCreneau:HH:mm}-{heureFinCreneau:HH:mm} ({duree}h) | Tâche: {affectation.TacheNom} (Bloc: {affectation.BlocId})");
                            }
                        }
                    }
                }
            }
            // --- FIN DE LA LOGIQUE D'AFFICHAGE DU PLANNING DÉTAILLÉ ---

            if (resultat.OptimisationResultat != null)
            {
                var optimResult = resultat.OptimisationResultat;
                sb.AppendLine("\n--- Résumé de l'Optimisation ---");
                sb.AppendLine($"Statut du Solveur: {optimResult.Status}");
                if (optimResult.CoutTotalEstime.HasValue) sb.AppendLine($"Coût Total Estimé : {(optimResult.CoutTotalEstime / 100.0m)?.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"))}");
                if (optimResult.DureeTotaleEnSlots.HasValue) sb.AppendLine($"Durée Totale (en slots de 1h): {optimResult.DureeTotaleEnSlots}");
            }

            // Le bloc 'AnalysePostOptimisationResultat' a été supprimé de la DLL, donc on le commente/supprime ici aussi.
            /*
            if (resultat.AnalysePostOptimisationResultat != null)
            {
                // ...
            }
            */

            Log(sb.ToString());
        }
        #endregion

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

            SolverProgressBar.Values.Text = $"Calcul en cours ({remainingMinutes}:{remainingSecondsDisplay:D2} restant)";

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
    }
}