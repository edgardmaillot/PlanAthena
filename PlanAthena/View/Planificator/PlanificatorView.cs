// --- START OF FILE PlanificatorView.cs ---

// PlanAthena Version 0.5.3 - Correction complète et finale UI
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.ImportExport;
using PlanAthena.Services.DTOs.Projet;
using PlanAthena.Services.Export;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.UseCases;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using Krypton.Toolkit;
using Timer = System.Windows.Forms.Timer;

namespace PlanAthena.View.Planificator
{
    public partial class PlanificatorView : UserControl
    {
        private readonly PlanificationOrchestrator _planificationOrchestrator;
        private readonly PlanningService _planningService;
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly PlanningExcelExportService _planningExcelExportService;
        private readonly GanttExportService _ganttExportService;
        private readonly CheminsPrefereService _cheminsPrefereService;
        private PlanificationRunResult _lastRunResult = null;
        private int _elapsedSeconds = 0;
        private Timer _solverTimer;
        private int _solverMaxSeconds;

        public PlanificatorView(
            PlanificationOrchestrator planificationOrchestrator,
            PlanningService planningService,
            ProjetService projetService,
            RessourceService ressourceService,
            PlanningExcelExportService planningExcelExportService,
            GanttExportService ganttExportService,
            CheminsPrefereService cheminsPrefereService)
        {
            InitializeComponent();
            _planificationOrchestrator = planificationOrchestrator;
            _planningService = planningService;
            _projetService = projetService;
            _ressourceService = ressourceService;
            _planningExcelExportService = planningExcelExportService;
            _ganttExportService = ganttExportService;
            _cheminsPrefereService = cheminsPrefereService;
            _solverTimer = new Timer { Interval = 1000 };
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

            dgvAnalyseOuvriers.AutoGenerateColumns = false;

            // Associer les ToolTips avec le composant KryptonToolTip
            kryptonToolTip.SetToolTip(lblHelpCoutIndirect, "Coût journalier fixe du chantier (location, charges, etc.).\nImpacte directement l'optimisation des délais.");
            kryptonToolTip.SetToolTip(lblHelpPenalite, "Surcoût en % appliqué lorsqu'un ouvrier est réaffecté à un autre bloc dans la même journée.\nDécourage la fragmentation du travail.");
            kryptonToolTip.SetToolTip(lblHelpTypeSortie, "Choisir l'objectif principal de la planification :\n- Analyse : Estimation rapide sans optimisation.\n- Optimisation Coût : Cherche la solution la moins chère.\n- Optimisation Délai : Cherche la solution la plus rapide.");
            kryptonToolTip.SetToolTip(lblHelpDecoupage, "Si une tâche unitaire dépasse ce nombre de jours, l'IA est autorisée à la diviser pour mieux l'intégrer dans le planning.");
        }

        private void PopulateFormFromSessionConfig()
        {
            var config = _projetService.ConfigPlanificationActuelle;
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
            cmbTypeDeSortie.SelectedItem = config.TypeDeSortie ?? "Optimisation Délai";

            if (config.DateDebutSouhaitee.HasValue) dtpDateDebut.Value = config.DateDebutSouhaitee.Value;
            if (config.DateFinSouhaitee.HasValue) dtpDateFin.Value = config.DateFinSouhaitee.Value;
        }

        private ConfigurationPlanification GetConfigFromForm()
        {
            var config = _projetService.ConfigPlanificationActuelle;

            config.JoursOuvres = chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>().ToList();
            config.HeureDebutJournee = (int)numHeureDebut.Value;
            config.DureeJournaliereStandardHeures = (int)numDureeOuverture.Value;
            config.HeuresTravailEffectifParJour = (int)numHeuresTravail.Value;
            config.PenaliteChangementOuvrierPourcentage = numPenaliteChangement.Value;
            config.CoutIndirectJournalierAbsolu = (long)numCoutIndirect.Value;
            config.DureeCalculMaxMinutes = (int)cmbCalculMax.SelectedItem;
            config.SeuilJoursDecoupageTache = (int)numSeuilDecoupage.Value;

            config.DateDebutSouhaitee = dtpDateDebut.Value.Date;
            config.DateFinSouhaitee = dtpDateFin.Value.Date;

            InformationsProjet infosProjet = _projetService.ObtenirInformationsProjet();
            config.Description = infosProjet?.NomProjet ?? "Nouveau Projet";

            switch (cmbTypeDeSortie.SelectedItem?.ToString())
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
            if (_projetService.ObtenirInformationsProjet() == null)
            {
                MessageBox.Show("Veuillez charger un projet avant de lancer la planification.", "Projet requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLaunch.Enabled = false;
            ResetResultDisplay();
            _elapsedSeconds = 0;
            StartSolverProgress();

            Log("Lancement de la planification...");

            try
            {
                var configuration = GetConfigFromForm();
                // --- CORRECTIF ---
                bool reinitialiser = chkReinitialiserBaseline.Checked;
                _lastRunResult = await _planificationOrchestrator.ExecuteAsync(configuration, reinitialiser);

                AfficherResultats(_lastRunResult);
            }
            catch (Exception ex)
            {
                Log($"ERREUR CRITIQUE : {ex.Message}");
                MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisplayErrorState(new List<MessageValidationDto>
                {
                    new() { Type = TypeMessageValidation.Erreur, CodeMessage = "FATAL", Message = $"Erreur interne de l'application: {ex.Message}" }
                });
            }
            finally
            {
                StopSolverProgress();
                btnLaunch.Enabled = true;
                UpdateExportButtonsState();
                Log("PLANIFICATION TERMINÉE.");
            }
        }
        #endregion

        #region Affichage des Résultats

        private void ResetResultDisplay()
        {
            navigatorResultats.Visible = false;
            rtbLog.Clear();
            khgNotification.Visible = false;
            rtbNotification.Clear();
            kryptonPanelKpis.Visible = true;
            NettoyerAffichageKpis();
        }


        private void AfficherResultats(PlanificationRunResult runResult)
        {
            if (runResult == null)
            {
                DisplayErrorState(new List<MessageValidationDto>
        {
            new() { Type = TypeMessageValidation.Erreur, CodeMessage = "NULL_RESULT", Message = "Le service de planification n'a retourné aucun résultat." }
        });
                return;
            }

            navigatorResultats.Visible = true;

            // Déterminer le mode de traitement selon le type de résultat
            bool isAnalysisMode = cmbTypeDeSortie.SelectedItem?.ToString() == "Analyse et Estimation";

            // L'analyse rapide a toujours priorité si elle contient des données
            if (isAnalysisMode && runResult.MetierTensionReport != null)
            {
                DisplayAnalysisState(runResult);
            }
            // Sinon, traiter selon le statut du solveur
            else if (runResult.RawResult?.OptimisationResultat != null)
            {
                var status = runResult.RawResult.OptimisationResultat.Status;

                switch (status)
                {
                    case OptimizationStatus.Optimal:
                    case OptimizationStatus.Feasible:
                        DisplayOptimizationState(runResult);
                        break;

                    case OptimizationStatus.Infeasible:
                    case OptimizationStatus.Unknown:
                    case OptimizationStatus.Aborted:
                    case OptimizationStatus.ModelInvalid:
                        // Tous les échecs passent par DisplayErrorState
                        DisplayErrorState(runResult.RawResult.Messages);
                        break;
                }
            }
            // Si pas d'optimisation et pas d'analyse, c'est une erreur
            else
            {
                var messages = runResult.RawResult?.Messages ?? new List<MessageValidationDto>();
                DisplayErrorState(messages);
            }

            AfficherResultatDansLog(runResult);
            navigatorResultats.SelectedPage = tabPageSynthese;
        }

        private void DisplayErrorState(IReadOnlyList<MessageValidationDto> messages)
        {
            // Déterminer le type d'erreur principal pour adapter l'affichage
            var errorType = _lastRunResult?.ErrorType ?? "UNKNOWN";

            SetupNotificationBanner(
                GetErrorTitle(errorType),
                GetErrorSubtitle(errorType),
                GetErrorColor(errorType),
                null
            );

            rtbNotification.Clear();
            var boldFont = new Font(rtbNotification.Font, FontStyle.Bold);

            // Afficher le message utilisateur personnalisé si disponible
            if (!string.IsNullOrEmpty(_lastRunResult?.UserMessage))
            {
                rtbNotification.SelectionFont = boldFont;
                rtbNotification.AppendText("DIAGNOSTIC ET RECOMMANDATIONS :\n\n");
                rtbNotification.SelectionFont = rtbNotification.Font;
                rtbNotification.AppendText(_lastRunResult.UserMessage);
                rtbNotification.AppendText("\n\n");
            }

            // Afficher les messages techniques en détail
            if (messages.Any())
            {
                rtbNotification.SelectionFont = boldFont;
                rtbNotification.AppendText("DÉTAILS TECHNIQUES :\n");
                rtbNotification.SelectionFont = rtbNotification.Font;

                foreach (var msg in messages.Where(m => m.Type == TypeMessageValidation.Erreur))
                {
                    rtbNotification.SelectionFont = boldFont;
                    rtbNotification.AppendText("• ");
                    rtbNotification.SelectionFont = rtbNotification.Font;
                    string elementInfo = !string.IsNullOrEmpty(msg.ElementId) ? $" (Élément: {msg.ElementId})" : "";
                    rtbNotification.AppendText($"{msg.Message}{elementInfo}\n");
                }

                // Afficher aussi les avertissements importants
                var avertissementsImportants = messages.Where(m => m.Type == TypeMessageValidation.Avertissement).ToList();
                if (avertissementsImportants.Any())
                {
                    rtbNotification.SelectionFont = boldFont;
                    rtbNotification.AppendText("\nAVERTISSEMENTS :\n");
                    rtbNotification.SelectionFont = rtbNotification.Font;

                    foreach (var warning in avertissementsImportants)
                    {
                        rtbNotification.AppendText($"⚠ {warning.Message}\n");
                    }
                }
            }

            kryptonPanelKpis.Visible = false; // Masquer les KPIs car invalides
        }

        private void DisplayAnalysisState(PlanificationRunResult runResult)
        {
            // Vérifier s'il y a des erreurs critiques dans l'analyse rapide
            var messagesErreur = runResult.RawResult?.Messages?.Where(m =>
                m.Type == TypeMessageValidation.Erreur ||
                m.CodeMessage.Contains("INFEASIBLE") ||
                m.Message.Contains("infaisable")).ToList() ?? new List<MessageValidationDto>();

            if (messagesErreur.Any())
            {
                // L'analyse rapide a échoué, traiter comme une erreur mais avec un titre spécifique
                SetupNotificationBanner(
                    "ANALYSE RAPIDE - PLANNING IMPOSSIBLE",
                    "L'estimation préliminaire indique que le projet est infaisable",
                    Color.FromArgb(255, 140, 0), // Orange pour différencier d'une erreur technique
                    null
                );

                rtbNotification.Clear();
                var boldFont = new Font(rtbNotification.Font, FontStyle.Bold);

                // Analyser les causes spécifiquement pour l'analyse rapide
                var analyseCauses = AnalyserCausesInfaisabiliteAnalyseRapide(runResult.RawResult.Messages);

                rtbNotification.SelectionFont = boldFont;
                rtbNotification.AppendText("DIAGNOSTIC PRÉLIMINAIRE :\n\n");
                rtbNotification.SelectionFont = rtbNotification.Font;
                rtbNotification.AppendText(analyseCauses);
                rtbNotification.AppendText("\n\n");

                rtbNotification.SelectionFont = boldFont;
                rtbNotification.AppendText("RECOMMANDATION :\n");
                rtbNotification.SelectionFont = rtbNotification.Font;
                rtbNotification.AppendText("Ajustez les paramètres ci-dessus, puis relancez une analyse rapide pour confirmer la faisabilité avant d'optimiser.");

                kryptonPanelKpis.Visible = false;
                return;
            }

            // Analyse rapide réussie - affichage normal
            SetupNotificationBanner(
                "Synthèse de l'Analyse Rapide",
                "Recommandations basées sur les données du projet",
                Color.FromArgb(0, 102, 204), // Bleu informatif
                null
            );

            rtbNotification.Clear();
            var boldFontSuccess = new Font(rtbNotification.Font, FontStyle.Bold);

            // 1. Conclusion sur la tension des métiers
            rtbNotification.SelectionFont = boldFontSuccess;
            rtbNotification.AppendText("Conclusion : ");
            rtbNotification.SelectionFont = rtbNotification.Font;
            rtbNotification.AppendText($"{runResult.MetierTensionReport.Conclusion}{Environment.NewLine}{Environment.NewLine}");

            // 2. Répartition suggérée des métiers clés
            if (runResult.MetierTensionReport.Repartition.Any())
            {
                rtbNotification.SelectionFont = boldFontSuccess;
                rtbNotification.AppendText($"Répartition suggérée des métiers clés :{Environment.NewLine}");
                rtbNotification.SelectionFont = rtbNotification.Font;
                foreach (var repartition in runResult.MetierTensionReport.Repartition)
                {
                    var nomMetier = _ressourceService.GetMetierById(repartition.MetierId)?.Nom ?? repartition.MetierId;
                    rtbNotification.AppendText($"  - {nomMetier} : {repartition.Count} ouvrier(s){Environment.NewLine}");
                }
                rtbNotification.AppendText(Environment.NewLine);
            }

            // 3. Calcul et affichage de la date de fin recommandée
            var dateFinRecommandee = CalculerDateFinRecommandee(runResult);
            if (dateFinRecommandee.HasValue)
            {
                rtbNotification.SelectionFont = boldFontSuccess;
                rtbNotification.AppendText("Planification temporelle recommandée :");
                rtbNotification.SelectionFont = rtbNotification.Font;
                rtbNotification.AppendText(Environment.NewLine);

                var dureeEstimeeHeures = runResult.RawResult.AnalyseStatiqueResultat?.DureeTotaleEstimeeEnSlots ?? 0;
                var heuresTravailParJour = (int)numHeuresTravail.Value;
                var joursEstimes = Math.Ceiling((double)dureeEstimeeHeures / heuresTravailParJour);

                rtbNotification.AppendText($"  - Durée estimée : {dureeEstimeeHeures} heures ({joursEstimes:F0} jours de travail){Environment.NewLine}");
                rtbNotification.AppendText($"  - Date de début : {dtpDateDebut.Value:dddd dd MMMM yyyy}{Environment.NewLine}");
                rtbNotification.AppendText($"  - Date de fin recommandée : {dateFinRecommandee.Value:dddd dd MMMM yyyy}{Environment.NewLine}");

                // Vérifier si la date recommandée dépasse la date de fin actuelle
                if (dateFinRecommandee.Value > dtpDateFin.Value)
                {
                    var ecartJours = (dateFinRecommandee.Value - dtpDateFin.Value).Days;
                    rtbNotification.SelectionFont = boldFontSuccess;
                    rtbNotification.AppendText($"  ⚠ Attention : ");
                    rtbNotification.SelectionFont = rtbNotification.Font;
                    rtbNotification.AppendText($"La date de fin actuelle ({dtpDateFin.Value:dd/MM/yyyy}) est trop courte de {ecartJours} jours.{Environment.NewLine}");
                    rtbNotification.AppendText($"     Modifiez la date de fin à {dateFinRecommandee.Value:dd/MM/yyyy} pour garantir la faisabilité.{Environment.NewLine}");
                }
                else
                {
                    rtbNotification.SelectionFont = boldFontSuccess;
                    rtbNotification.AppendText($"  ✓ ");
                    rtbNotification.SelectionFont = rtbNotification.Font;
                    rtbNotification.AppendText($"La date de fin actuelle laisse une marge suffisante.{Environment.NewLine}");
                }
            }

            kryptonPanelKpis.Visible = true;
            UpdateKpisForAnalysis(runResult);
        }

        private string AnalyserCausesInfaisabiliteAnalyseRapide(IReadOnlyList<MessageValidationDto> messages)
        {
            var problemes = new List<string>();

            // Analyser spécifiquement pour l'analyse rapide
            var capaciteMessage = messages.FirstOrDefault(m => m.CodeMessage == "WARN_CAPACITY_OVERLOAD");
            if (capaciteMessage != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(capaciteMessage.Message, @"(\d+)h requises vs ~(\d+)h disponibles");
                if (match.Success)
                {
                    var heuresRequises = int.Parse(match.Groups[1].Value);
                    var heuresDisponibles = int.Parse(match.Groups[2].Value);
                    var ratioSurcharge = (double)heuresRequises / heuresDisponibles;

                    problemes.Add($"📊 CHARGE DE TRAVAIL : {heuresRequises}h requises vs {heuresDisponibles}h disponibles");
                    problemes.Add($"   Surcharge de {ratioSurcharge:P0} - Le planning est {(ratioSurcharge > 1.5 ? "très" : "")} contraint");

                    var extensionJours = Math.Ceiling((double)(heuresRequises - heuresDisponibles) / 7); // 7h par jour
                    problemes.Add($"   💡 SOLUTION : Prolonger le planning d'au moins {extensionJours} jours ouvrés");
                }
            }

            var insuffisantMessages = messages.Where(m => m.CodeMessage == "WARN_INSUFFICIENT_WORKERS").ToList();
            if (insuffisantMessages.Any())
            {
                problemes.Add($"\n👷 GOULOTS D'ÉTRANGLEMENT : {insuffisantMessages.Count} métier(s) en tension");
                foreach (var msg in insuffisantMessages.Take(3))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(msg.Message, @"Métier '([^']+)'");
                    if (match.Success)
                    {
                        problemes.Add($"   • {match.Groups[1].Value} : Ressource critique");
                    }
                }
                problemes.Add("   💡 SOLUTION : Recruter des ouvriers supplémentaires ou étaler le travail");
            }

            return problemes.Any() ? string.Join("\n", problemes) :
                   "Le projet semble infaisable avec les contraintes actuelles. Vérifiez les délais et les ressources.";
        }

        private void DisplayOptimizationState(PlanificationRunResult runResult)
        {
            khgNotification.Visible = false; // Pas de bannière si tout va bien
            kryptonPanelKpis.Visible = true;

            var culture = CultureInfo.GetCultureInfo("fr-FR");
            string solverStatus = runResult.RawResult?.OptimisationResultat?.Status.ToString();
            UpdateSolverStatusDisplay(solverStatus);

            if (runResult.AnalysisReport != null)
            {
                var synthese = runResult.AnalysisReport.SyntheseProjet;
                lblCoutTotalValue.Text = synthese.CoutTotalProjet.ToString("C", culture);
                lblCoutRhValue.Text = synthese.CoutTotalRh.ToString("C", culture);
                lblCoutIndirectValue.Text = synthese.CoutTotalIndirect.ToString("C", culture);
                lblJoursHommeValue.Text = synthese.EffortTotalJoursHomme.ToString("0.0");
                lblDureeValue.Text = synthese.DureeJoursOuvres.ToString();
                AfficherAnalyseRessources(runResult.AnalysisReport);
            }
            else
            {
                NettoyerAffichageKpis();
            }
        }

        private void SetupNotificationBanner(string title, string subtitle, Color backColor, Image image)
        {
            khgNotification.ValuesPrimary.Heading = title;
            khgNotification.ValuesSecondary.Heading = subtitle;
            khgNotification.StateCommon.HeaderPrimary.Back.Color1 = backColor;
            khgNotification.StateCommon.HeaderPrimary.Back.Color2 = backColor;
            khgNotification.StateCommon.HeaderSecondary.Back.Color1 = ControlPaint.Light(backColor);
            khgNotification.StateCommon.HeaderSecondary.Back.Color2 = ControlPaint.Light(backColor);
            khgNotification.ValuesPrimary.Image = image;
            khgNotification.Visible = true;
        }

        private void UpdateKpisForAnalysis(PlanificationRunResult runResult)
        {
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var estimation = runResult.RawResult.AnalyseStatiqueResultat;
            if (estimation != null)
            {
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
                lblCoutRhValue.Text = coutRhEstime.ToString("C", culture);
            }
            lblJoursHommeValue.Text = "N/A";
            UpdateSolverStatusDisplay("ANALYSE");
            NettoyerAnalyseRessources();
        }

        private void UpdateSolverStatusDisplay(string status)
        {
            khgSolverStatus.Visible = !string.IsNullOrEmpty(status);
            if (string.IsNullOrEmpty(status)) return;

            var defaultBackColor = SystemColors.ControlDark;
            var optimalColor = Color.FromArgb(0, 150, 0);
            var feasibleColor = Color.FromArgb(204, 132, 0);
            var infeasibleColor = Color.FromArgb(192, 0, 0);
            var timeoutColor = Color.FromArgb(255, 140, 0);
            var abortedColor = Color.FromArgb(128, 128, 128);

            switch (status.ToUpperInvariant())
            {
                case "OPTIMAL":
                    khgSolverStatus.ValuesPrimary.Heading = "OPTIMAL";
                    khgSolverStatus.StateCommon.HeaderPrimary.Back.Color1 = optimalColor;
                    lblStatutExplication.Text = "La meilleure solution\npossible a été trouvée.";
                    break;

                case "FEASIBLE":
                    khgSolverStatus.ValuesPrimary.Heading = "FAISABLE";
                    khgSolverStatus.StateCommon.HeaderPrimary.Back.Color1 = feasibleColor;
                    lblStatutExplication.Text = "Une solution a été trouvée,\nmais elle n'est peut-être pas la meilleure.\nLe temps de calcul était peut-être insuffisant.";
                    break;

                case "INFEASIBLE":
                    khgSolverStatus.ValuesPrimary.Heading = "IMPOSSIBLE";
                    khgSolverStatus.StateCommon.HeaderPrimary.Back.Color1 = infeasibleColor;
                    lblStatutExplication.Text = "Aucune solution possible\navec les contraintes actuelles.\nVoir les recommandations ci-dessus.";
                    break;

                case "UNKNOWN":
                    khgSolverStatus.ValuesPrimary.Heading = "TEMPS INSUFFISANT";
                    khgSolverStatus.StateCommon.HeaderPrimary.Back.Color1 = timeoutColor;
                    lblStatutExplication.Text = "Le solveur n'a pas eu\nassez de temps pour explorer\ntoutes les possibilités.\nAugmentez le temps de calcul.";
                    break;

                case "ABORTED":
                    khgSolverStatus.ValuesPrimary.Heading = "INTERROMPU";
                    khgSolverStatus.StateCommon.HeaderPrimary.Back.Color1 = abortedColor;
                    lblStatutExplication.Text = "La planification a été\ninterrompue avant la fin.\nRelancez le calcul.";
                    break;

                case "ANALYSE":
                    khgSolverStatus.ValuesPrimary.Heading = "ANALYSE";
                    khgSolverStatus.StateCommon.HeaderPrimary.Back.Color1 = Color.FromArgb(0, 102, 204);
                    lblStatutExplication.Text = "Mode analyse rapide.\nAucune optimisation effectuée.";
                    break;

                default:
                    khgSolverStatus.ValuesPrimary.Heading = status.ToUpperInvariant();
                    khgSolverStatus.StateCommon.HeaderPrimary.Back.Color1 = defaultBackColor;
                    lblStatutExplication.Text = "Statut du solveur non reconnu.";
                    break;
            }
        }

        private void AfficherAnalyseRessources(AnalysisReport report)
        {
            if (report == null)
            {
                NettoyerAnalyseRessources();
                return;
            }
            dgvAnalyseOuvriers.DataSource = null;
            dgvAnalyseOuvriers.DataSource = report.AnalysesOuvriers;
            foreach (DataGridViewRow row in dgvAnalyseOuvriers.Rows)
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
            UpdateSolverStatusDisplay(null);
            NettoyerAnalyseRessources();
        }

        private void NettoyerAnalyseRessources()
        {
            dgvAnalyseOuvriers.DataSource = null;
            chartChargeJournaliere.Series.Clear();
        }
        #endregion

        #region Exports et États
        private void UpdateExportButtonsState()
        {
            bool hasReport = _lastRunResult?.AnalysisReport != null;
            var status = _lastRunResult?.RawResult?.OptimisationResultat?.Status;

            // L'export n'est possible QUE si un rapport existe ET que le statut est Optimal ou Faisable.
            bool isSuccessfulStatus = (status == OptimizationStatus.Optimal || status == OptimizationStatus.Feasible);
            bool canExport = hasReport && isSuccessfulStatus;

            btnExportPlanningExcel.Enabled = canExport;
            btnExportGantt.Enabled = canExport;

            // Mettre à jour les tooltips pour expliquer pourquoi l'export n'est pas disponible
            if (!canExport)
            {
                string reason = !hasReport ? "Aucun planning valide généré" :
                               !isSuccessfulStatus ? $"Statut du solveur : {status}" : "Export non disponible";
                kryptonToolTip.SetToolTip(btnExportPlanningExcel, $"Export indisponible : {reason}");
                kryptonToolTip.SetToolTip(btnExportGantt, $"Export indisponible : {reason}");
            }
            else
            {
                kryptonToolTip.SetToolTip(btnExportPlanningExcel, "Exporter le planning au format Excel");
                kryptonToolTip.SetToolTip(btnExportGantt, "Exporter le planning vers GanttProject");
            }
        }

        private async void btnExportPlanningExcel_Click(object sender, EventArgs e)
        {
            if (_lastRunResult?.AnalysisReport == null || _planningService.GetCurrentPlanning() == null)
            {
                MessageBox.Show("Aucun planning optimisé n'est disponible pour l'export.", "Export impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            InformationsProjet infosProjet = _projetService.ObtenirInformationsProjet();
            string nomProjet = infosProjet?.NomProjet ?? "Projet";

            using (var saveFileDialog = new SaveFileDialog
            {
                Filter = "Fichiers Excel (*.xlsx)|*.xlsx",
                Title = "Exporter le planning au format Excel",
                InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierExport(),
                FileName = $"Planning_{nomProjet}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        btnExportPlanningExcel.Enabled = false;
                        var exportData = new ExportDataProjetDto
                        {
                            NomProjet = nomProjet,
                            Configuration = _projetService.ConfigPlanificationActuelle,
                            Planning = _planningService.GetCurrentPlanning(),
                            Report = _lastRunResult.AnalysisReport,
                            ProjetStructure = _projetService.GetProjetDataPourSauvegarde(),
                            PoolOuvriers = _ressourceService.GetAllOuvriers()
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

            InformationsProjet infosProjet = _projetService.ObtenirInformationsProjet();
            string nomProjet = infosProjet?.NomProjet ?? "Projet";

            using (var saveFileDialog = new SaveFileDialog
            {
                Filter = "Fichiers GanttProject (*.gan)|*.gan",
                Title = "Exporter vers GanttProject",
                InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierExport(),
                FileName = $"Gantt_{nomProjet}_{DateTime.Now:yyyyMMdd_HHmm}.gan"
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        btnExportGantt.Enabled = false;
                        var exportData = new ExportDataProjetDto
                        {
                            NomProjet = nomProjet,
                            Configuration = _projetService.ConfigPlanificationActuelle,
                            Planning = _planningService.GetCurrentPlanning(),
                            Report = _lastRunResult.AnalysisReport,
                            ProjetStructure = _projetService.GetProjetDataPourSauvegarde()
                        };
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
                var planningParJour = _planningService.RetournePlanningDetailleParJour();
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
        private string GetErrorTitle(string errorType)
        {
            return errorType switch
            {
                "INFEASIBLE" => "PLANNING IMPOSSIBLE",
                "TIMEOUT_EXPLORATION" => "TEMPS DE CALCUL INSUFFISANT",
                "ABORTED" => "PLANIFICATION INTERROMPUE",
                "MODEL_ERROR" => "ERREUR DE MODÈLE",
                _ => "ERREUR CRITIQUE"
            };
        }
        private string GetErrorSubtitle(string errorType)
        {
            return errorType switch
            {
                "INFEASIBLE" => "Les contraintes ne permettent aucune solution",
                "TIMEOUT_EXPLORATION" => "Le solveur n'a pas eu assez de temps",
                "ABORTED" => "Le calcul a été arrêté prématurément",
                "MODEL_ERROR" => "Problème technique dans les données",
                _ => "Une erreur est survenue pendant la planification"
            };
        }

        private Color GetErrorColor(string errorType)
        {
            return errorType switch
            {
                "INFEASIBLE" => Color.FromArgb(192, 0, 0),           // Rouge vif - problème utilisateur
                "TIMEOUT_EXPLORATION" => Color.FromArgb(255, 140, 0), // Orange - action simple requise
                "ABORTED" => Color.FromArgb(128, 128, 128),          // Gris - interruption
                "MODEL_ERROR" => Color.FromArgb(128, 0, 128),        // Violet - problème technique
                _ => Color.FromArgb(192, 0, 0)                       // Rouge par défaut
            };
        }

        /// <summary>
        /// Calcule la date de fin recommandée basée sur la durée estimée du chantier
        /// avec une marge de sécurité et alignement sur la fin de semaine
        /// </summary>
        private DateTime? CalculerDateFinRecommandee(PlanificationRunResult runResult)
        {
            try
            {
                // Récupérer les données nécessaires
                var dureeEstimeeHeures = runResult.RawResult?.AnalyseStatiqueResultat?.DureeTotaleEstimeeEnSlots;
                if (!dureeEstimeeHeures.HasValue || dureeEstimeeHeures.Value <= 0)
                    return null;

                var dateDebut = dtpDateDebut.Value.Date;
                var heuresTravailParJour = (int)numHeuresTravail.Value;

                if (heuresTravailParJour <= 0)
                    return null;

                // Obtenir les jours ouvrés de la configuration
                var joursOuvres = new List<DayOfWeek>();
                for (int i = 0; i < chkListJoursOuvres.Items.Count; i++)
                {
                    if (chkListJoursOuvres.GetItemChecked(i))
                    {
                        joursOuvres.Add((DayOfWeek)chkListJoursOuvres.Items[i]);
                    }
                }

                if (!joursOuvres.Any())
                {
                    // Fallback : jours ouvrés standard (lundi à vendredi)
                    joursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
                }

                // 1. Calculer le nombre de jours de travail nécessaires (avec marge de 5%)
                var joursNecessaires = Math.Ceiling((double)dureeEstimeeHeures.Value / heuresTravailParJour);
                var joursAvecMarge = Math.Ceiling(joursNecessaires * 1.05); // Marge de 5%

                // 2. Calculer la date de fin en comptant uniquement les jours ouvrés
                var dateActuelle = dateDebut;
                var joursComptes = 0;

                while (joursComptes < joursAvecMarge)
                {
                    if (joursOuvres.Contains(dateActuelle.DayOfWeek))
                    {
                        joursComptes++;
                    }

                    if (joursComptes < joursAvecMarge) // Éviter d'ajouter un jour de trop
                    {
                        dateActuelle = dateActuelle.AddDays(1);
                    }
                }

                // 3. Arrondir à la fin de la semaine suivante si on n'est pas déjà en fin de semaine
                var dateFin = dateActuelle;

                // Trouver le dernier jour ouvré de la semaine
                var dernierJourOuvre = joursOuvres.OrderByDescending(j => (int)j).First();

                // Si on n'est pas sur le dernier jour ouvré de la semaine, avancer jusqu'au vendredi suivant
                if (dateFin.DayOfWeek != dernierJourOuvre)
                {
                    while (dateFin.DayOfWeek != dernierJourOuvre)
                    {
                        dateFin = dateFin.AddDays(1);
                    }
                }

                return dateFin;
            }
            catch (Exception ex)
            {
                // En cas d'erreur, ne pas planter l'interface
                Log($"Erreur lors du calcul de la date de fin recommandée : {ex.Message}");
                return null;
            }
        }
        private void SolverTimer_Tick(object sender, EventArgs e)
        {
            _elapsedSeconds++;
            var delay = TimeSpan.FromSeconds(_elapsedSeconds);
            if (_solverMaxSeconds > 0)
            {
                double progressPercentage = ((double)_elapsedSeconds / _solverMaxSeconds) * 100;
                if (progressPercentage > 100) progressPercentage = 100;
                SolverProgressBar.Value = (int)progressPercentage;
                int remainingSeconds = Math.Max(0, _solverMaxSeconds - _elapsedSeconds);
                int remainingMinutes = remainingSeconds / 60;
                int remainingSecondsDisplay = remainingSeconds % 60;
                SolverProgressBar.Values.Text = $"Calcul en cours ({remainingMinutes}:{remainingSecondsDisplay:D2} restant)";
            }
            else
            {
                SolverProgressBar.Values.Text = $"Planification en cours... {delay:g}";
            }
        }
        private void StartSolverProgress()
        {
            _solverMaxSeconds = (int)cmbCalculMax.SelectedItem * 60;
            _elapsedSeconds = 0;
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