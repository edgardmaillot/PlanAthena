using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application;
using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Core.Infrastructure;
using PlanAthena.CsvModels;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace PlanAthena
{
    public partial class MainForm : Form
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly MetierService _metierService;

        private List<OuvrierCsvRecord> _loadedOuvriers = new();
        private List<TacheCsvRecord> _loadedTaches = new();

        public MainForm()
        {
            InitializeComponent();
            _serviceProvider = ConfigureServices();
            _metierService = new MetierService();
            InitializeFormDefaults();
        }

        private void InitializeFormDefaults()
        {
            chkListJoursOuvres.Items.Add(DayOfWeek.Monday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Tuesday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Wednesday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Thursday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Friday, true);
            chkListJoursOuvres.Items.Add(DayOfWeek.Saturday, false);
            chkListJoursOuvres.Items.Add(DayOfWeek.Sunday, false);
            chkListJoursOuvres.DisplayMember = "ToString";

            cmbTypeDeSortie.Items.Clear();
            cmbTypeDeSortie.Items.AddRange(new string[] {
                "Analyse et Estimation",
                "Optimisation Coût",
                "Optimisation Délai"
            });
            cmbTypeDeSortie.SelectedIndex = 0;

            dtpDateDebut.Value = DateTime.Today;
            dtpDateFin.Value = DateTime.Today.AddMonths(3);
            chkDateDebut.Checked = true;
            chkDateFin.Checked = true;

            Log("Application prête. Chargez vos fichiers CSV.");
        }

        #region Logique pour les clics sur les boutons de l'UI

        private void btnImportOuvriers_Click(object sender, EventArgs e)
        {
            _loadedOuvriers = ImportCsvFile<OuvrierCsvRecord>(txtOuvriersPath, lblOuvriersStatus, "lignes ouvrier");
        }

        private void btnImportMetiers_Click(object sender, EventArgs e)
        {
            var metiersCsv = ImportCsvFile<MetierCsvRecord>(txtMetiersPath, lblMetiersStatus, "métiers");
            _metierService.ChargerMetiers(metiersCsv);
            lblMetiersStatus.Text = $"{_metierService.GetAllMetiers().Count} métiers chargés.";
        }

        private void btnImportTaches_Click(object sender, EventArgs e)
        {
            _loadedTaches = ImportCsvFile<TacheCsvRecord>(txtTachesPath, lblTachesStatus, "tâches");
        }

        private void btnGenerateCsv_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Fichier CSV (*.csv)|*.csv",
                Title = "Enregistrer le fichier de tâches",
                FileName = "taches_generees.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Log($"Génération du fichier CSV de test vers : {sfd.FileName}");
                    CsvGenerator.GenerateTachesCsv(sfd.FileName);
                    Log("Génération terminée avec succès !");
                    MessageBox.Show($"Le fichier '{sfd.FileName}' a été créé.", "Génération terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtTachesPath.Text = sfd.FileName;
                    _loadedTaches = ImportCsvFile<TacheCsvRecord>(txtTachesPath, lblTachesStatus, "tâches");
                }
                catch (Exception ex)
                {
                    Log($"ERREUR lors de la génération du CSV : {ex.Message}");
                    MessageBox.Show($"Une erreur est survenue :\n{ex.Message}", "Erreur de génération", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnGenerateAndTest_Click(object sender, EventArgs e)
        {
            Log("Lancement du test...");

            if (!ValidateInputs())
            {
                Log("VALIDATION ÉCHOUÉE. Veuillez charger tous les fichiers CSV requis.");
                MessageBox.Show("Veuillez charger les 3 fichiers CSV (Ouvriers, Métiers, Tâches) avant de lancer le test.", "Fichiers manquants", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Log("Construction du DTO d'entrée...");
                var inputDto = BuildChantierSetupInputDto();
                Log("DTO `ChantierSetupInputDto` généré avec succès.");

                Log("Appel du constructeur PlanAthena Core");
                var facade = _serviceProvider.GetRequiredService<PlanAthenaCoreFacade>();
                var resultatDto = await facade.ProcessChantierAsync(inputDto);
                DisplayResultInLog(resultatDto);

                Log("TEST TERMINÉ.");
            }
            catch (Exception ex)
            {
                Log($"ERREUR CRITIQUE lors de la création du DTO ou de l'appel DLL : {ex.ToString()}");
                MessageBox.Show($"Une erreur critique est survenue:\n{ex.ToString()}", "Erreur Critique", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Logique d'Import CSV
        private List<T> ImportCsvFile<T>(TextBox pathTextBox, Label statusLabel, string itemTypeName)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                Title = "Sélectionner un fichier CSV"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return new List<T>();
            }

            pathTextBox.Text = ofd.FileName;

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = true,
                    HeaderValidated = null,
                    MissingFieldFound = null,
                };

                using var reader = new StreamReader(ofd.FileName);
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<T>().ToList();
                statusLabel.Text = $"{records.Count} {itemTypeName} chargé(e)s.";
                Log($"Fichier '{Path.GetFileName(ofd.FileName)}' chargé : {records.Count} lignes.");
                return records;
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Erreur d'import";
                Log($"ERREUR lors de la lecture du fichier CSV '{pathTextBox.Text}': {ex.Message}");
                MessageBox.Show($"Erreur lors de la lecture du fichier CSV:\n{ex.Message}", "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<T>();
            }
        }
        #endregion

        #region Logique de construction du DTO principal
        private ChantierSetupInputDto BuildChantierSetupInputDto()
        {
            Log("Pré-traitement : Découpage et gestion des dépendances...");
            var dataProcessor = new ChantierDataProcessor();
            var processedTaches = dataProcessor.ProcessTaches(_loadedTaches, _metierService);
            var allMetiersRecords = _metierService.GetAllMetiers();
            Log($"Pré-traitement terminé. Tâches: {processedTaches.Count}, Métiers: {allMetiersRecords.Count}");

            var taches = processedTaches.Select(t => new TacheDto
            {
                TacheId = t.TacheId,
                Nom = t.TacheNom,
                BlocId = t.BlocId,
                HeuresHommeEstimees = t.HeuresHommeEstimees,
                MetierId = t.MetierId,
                Dependencies = t.Dependencies?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()
            }).ToList();

            var blocs = processedTaches
                .GroupBy(t => t.BlocId)
                .Select(g => g.First())
                .Select(t => new BlocTravailDto
                {
                    BlocId = t.BlocId,
                    Nom = t.BlocNom,
                    CapaciteMaxOuvriers = t.BlocCapaciteMaxOuvriers
                }).ToList();

            var lots = processedTaches
                .GroupBy(t => t.LotId)
                .Select(g => new LotTravauxDto
                {
                    LotId = g.Key,
                    Nom = g.First().LotNom,
                    Priorite = g.First().LotPriorite,
                    BlocIds = g.Select(b => b.BlocId).Distinct().ToList()
                }).ToList();

            var metiers = allMetiersRecords.Select(m => new MetierDto
            {
                MetierId = m.MetierId,
                Nom = m.Nom,
                PrerequisMetierIds = m.PrerequisMetierIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()
            }).ToList();

            var ouvriers = _loadedOuvriers
                .GroupBy(o => o.OuvrierId)
                .Select(g => new OuvrierDto
                {
                    OuvrierId = g.Key,
                    Nom = g.First().Nom,
                    Prenom = g.First().Prenom,
                    CoutJournalier = g.First().CoutJournalier,
                    Competences = g.Select(c => new CompetenceDto
                    {
                        MetierId = c.MetierId,
                        Niveau = (PlanAthena.Core.Facade.Dto.Enums.NiveauExpertise)c.NiveauExpertise,
                        PerformancePct = c.PerformancePct
                    }).ToList()
                }).ToList();

            var calendrier = new CalendrierTravailDefinitionDto
            {
                JoursOuvres = chkListJoursOuvres.CheckedItems.Cast<DayOfWeek>().ToList(),
                HeureDebutJournee = (int)numHeureDebut.Value,
                HeuresTravailEffectifParJour = (int)numHeuresTravail.Value
            };

            OptimizationConfigDto? optimConfig = null;
            string selectionUI = cmbTypeDeSortie.SelectedItem.ToString();
            if (selectionUI != "Analyse et Estimation")
            {
                string typeDeSortiePourDll;
                switch (selectionUI)
                {
                    case "Optimisation Coût":
                        typeDeSortiePourDll = "OPTIMISATION_COUT";
                        break;
                    case "Optimisation Délai":
                        typeDeSortiePourDll = "OPTIMISATION_DELAI";
                        break;
                    default:
                        typeDeSortiePourDll = "OPTIMISATION_COUT";
                        break;
                }
                optimConfig = new OptimizationConfigDto
                {
                    TypeDeSortie = typeDeSortiePourDll,
                    DureeJournaliereStandardHeures = (int)numDureeStandard.Value,
                    PenaliteChangementOuvrierPourcentage = numPenaliteChangement.Value,
                    CoutIndirectJournalierPourcentage = numCoutIndirect.Value
                };
            }

            var dtoFinal = new ChantierSetupInputDto
            {
                ChantierId = $"CHANTIER_TEST_{DateTime.Now:yyyyMMdd_HHmmss}",
                Description = txtDescription.Text,
                DateDebutSouhaitee = chkDateDebut.Checked ? dtpDateDebut.Value.Date : null,
                DateFinSouhaitee = chkDateFin.Checked ? dtpDateFin.Value.Date : null,
                CalendrierTravail = calendrier,
                OptimizationConfig = optimConfig,
                Taches = taches,
                Blocs = blocs,
                Lots = lots,
                Metiers = metiers,
                Ouvriers = ouvriers
            };

            if (dtoFinal.OptimizationConfig == null)
            {
                Log("Aucune config d'optimisation envoyée (cas d'usage #1 : Analyse).");
            }
            else
            {
                Log($"Config d'optimisation envoyée pour TypeDeSortie: '{dtoFinal.OptimizationConfig.TypeDeSortie}' (cas d'usage #2).");
            }

            return dtoFinal;
        }

        private bool ValidateInputs()
        {
            return _loadedOuvriers.Any() && _metierService.GetAllMetiers().Any() && _loadedTaches.Any();
        }
        #endregion

        #region Utilitaires
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

            serviceCollection.AddApplicationServices();
            serviceCollection.AddInfrastructureServices();
            serviceCollection.AddScoped<PlanAthena.Core.Application.Interfaces.IConstructeurProblemeOrTools, PlanAthena.Core.Infrastructure.Services.OrTools.ConstructeurProblemeOrTools>();
            serviceCollection.AddScoped<PlanAthenaCoreFacade>();

            return serviceCollection.BuildServiceProvider();
        }

        private void DisplayResultInLog(ProcessChantierResultDto? resultat)
        {
            if (resultat == null)
            {
                Log("Le résultat retourné par la façade est null.");
                return;
            }

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

                if (analyse.CoutTotalEstime.HasValue)
                {
                    Log($"Coût Total Estimé : {analyse.CoutTotalEstime / 100.0m:C}");
                }
                if (analyse.DureeTotaleEstimeeEnSlots.HasValue)
                {
                    Log($"Durée Totale Estimée : {analyse.DureeTotaleEstimeeEnSlots} heures ({analyse.DureeTotaleEstimeeEnSlots / 7.0:F1} jours de 7h)");
                }

                if (analyse.OuvriersClesSuggereIds.Any())
                {
                    Log($"Ouvriers clés suggérés : {string.Join(", ", analyse.OuvriersClesSuggereIds)}");
                }
            }

            if (resultat.OptimisationResultat?.Affectations?.Any() ?? false)
            {
                Log("\n--- Planning Détaillé (Affectations) ---");

                var planningParJour = resultat.OptimisationResultat.Affectations
                                              .OrderBy(a => a.DateDebut)
                                              .GroupBy(a => a.DateDebut.Date);

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