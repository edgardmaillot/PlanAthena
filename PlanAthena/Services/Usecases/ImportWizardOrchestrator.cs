
// Version 0.5.1
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.DTOs.ImportExport;
using PlanAthena.Utilities;
using PlanAthena.View.Utils;
using System.ComponentModel;
using System.Text;

namespace PlanAthena.Services.Usecases
{
    public class ImportWizardOrchestrator
    {
        private enum WizardStep { Step1_ColumnMapping, Step2_ValueMapping, Step3_Validation, Finished, Canceled }

        private readonly ImportService _importService;
        private readonly RessourceService _ressourceService;
        private readonly ProjetService _projetService;
        private readonly ValueMappingService _valueMappingService;

        public ImportWizardOrchestrator(
            ImportService importService,
            RessourceService ressourceService,
            ProjetService projetService,
            ValueMappingService valueMappingService)
        {
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _valueMappingService = valueMappingService ?? throw new ArgumentNullException(nameof(valueMappingService));
        }

        #region Point d'Entrée - Import Ouvriers
        public ImportResult LancerWizardImportOuvriers(string filePath)
        {
            if (!_ressourceService.GetAllMetiers().Any())
            {
                MessageBox.Show("Aucun métier n'est défini. Veuillez en créer au moins un avant d'importer des ouvriers.", "Pré-requis manquant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return ImportResult.Echec("Aucun métier n'est défini.");
            }

            var (modeConfirmation, remplacerExistants) = DemanderModeImport();
            if (modeConfirmation == DialogResult.Cancel) return ImportResult.Echec("Importation annulée.");

            var currentStep = WizardStep.Step1_ColumnMapping;
            ImportP1Result resultP1 = null;
            ImportP2Result resultP2 = null;

            while (currentStep != WizardStep.Finished && currentStep != WizardStep.Canceled)
            {
                Form currentHost = null;
                try
                {
                    switch (currentStep)
                    {
                        case WizardStep.Step1_ColumnMapping:
                            var configP1 = PreparerConfig_P1_Ouvriers(filePath);
                            var (viewP1, hostP1) = AfficherVue<ImportP1>("Étape 1/3: Mapper les colonnes", configP1);
                            currentHost = hostP1;
                            if (hostP1.DialogResult == DialogResult.OK)
                            {
                                resultP1 = viewP1.Result;
                                MemoriserChoixColonnes(resultP1, configP1.SuggestedMappings);
                                currentStep = WizardStep.Step2_ValueMapping;
                            }
                            else { currentStep = WizardStep.Canceled; }
                            break;

                        case WizardStep.Step2_ValueMapping:
                            var champMetierMappe = resultP1.FieldMappings.FirstOrDefault(f => f.InternalName == "Metier" && !string.IsNullOrEmpty(f.MappedCsvHeader));
                            if (champMetierMappe == null) { currentStep = WizardStep.Step3_Validation; continue; }
                            var configP2 = PreparerConfig_P2_Ouvriers(filePath, resultP1);
                            if (!configP2.SourceValues.Any()) { currentStep = WizardStep.Step3_Validation; continue; }

                            var (viewP2, hostP2) = AfficherVue<ImportP2>("Étape 2/3: Mapper les métiers", configP2);
                            currentHost = hostP2;
                            if (hostP2.DialogResult == DialogResult.OK)
                            {
                                resultP2 = viewP2.Result;
                                MemoriserChoixValeurs(resultP2, configP2.SuggestedMappings);
                                currentStep = WizardStep.Step3_Validation;
                            }
                            else if (hostP2.DialogResult == DialogResult.Abort) { currentStep = WizardStep.Step1_ColumnMapping; }
                            else { currentStep = WizardStep.Canceled; }
                            break;

                        case WizardStep.Step3_Validation:
                            var (objetsValides, rejets) = TransformerDonnees_Ouvriers(filePath, resultP1, resultP2);
                            var bindingList = new BindingList<Ouvrier>(objetsValides);
                            var bindingSource = new BindingSource { DataSource = bindingList };
                            var configP3 = new ImportP3Config { EntityDisplayName = "Ouvriers", EntityImage = Properties.Resources.Import_Ouvriers, DataSource = bindingSource, RejectedRows = rejets };
                            var (viewP3, hostP3) = AfficherVue<ImportP3>("Étape 3/3: Valider les données", configP3);
                            currentHost = hostP3;

                            if (hostP3.DialogResult == DialogResult.OK)
                            {
                                var finalObjects = new List<Ouvrier>(viewP3.Result.FinalData.OfType<Ouvrier>());
                                viewP3.SetWaitState(true);
                                var importResult = _importService.ImporterOuvriers(finalObjects, remplacerExistants);
                                viewP3.SetWaitState(false);

                                if (importResult.EstSucces)
                                {
                                    currentStep = WizardStep.Finished;
                                    hostP3.Close();
                                    return importResult;
                                }
                                viewP3.ShowImportError(importResult.MessageErreur);
                            }
                            else if (hostP3.DialogResult == DialogResult.Abort) { currentStep = WizardStep.Step2_ValueMapping; }
                            else { currentStep = WizardStep.Canceled; }
                            break;
                    }
                }
                finally { currentHost?.Dispose(); }
            }
            return ImportResult.Echec("Importation annulée.");
        }
        #endregion

        #region Point d'Entrée - Import Tâches
        public ImportResult LancerWizardImportTaches(string filePath, Lot lotCible)
        {
            if (lotCible == null) return ImportResult.Echec("Aucun lot cible fourni.");

            var confirmResult = MessageBox.Show($"Voulez-vous remplacer toutes les tâches et blocs existants dans le lot '{lotCible.Nom}' ?", "Mode d'importation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Cancel) return ImportResult.Echec("Importation annulée.");
            bool remplacerExistants = (confirmResult == DialogResult.Yes);

            var currentStep = WizardStep.Step1_ColumnMapping;
            ImportP1Result resultP1 = null;
            ImportP2Result resultP2 = null;

            while (currentStep != WizardStep.Finished && currentStep != WizardStep.Canceled)
            {
                Form currentHost = null;
                try
                {
                    switch (currentStep)
                    {
                        case WizardStep.Step1_ColumnMapping:
                            var configP1 = PreparerConfig_P1_Taches(filePath, lotCible);
                            var (viewP1, hostP1) = AfficherVue<ImportP1>("Étape 1/3: Mapper les colonnes", configP1);
                            currentHost = hostP1;
                            if (hostP1.DialogResult == DialogResult.OK)
                            {
                                resultP1 = viewP1.Result;
                                MemoriserChoixColonnes(resultP1, configP1.SuggestedMappings);
                                currentStep = WizardStep.Step2_ValueMapping;
                            }
                            else { currentStep = WizardStep.Canceled; }
                            break;

                        case WizardStep.Step2_ValueMapping:
                            var champMetierMappe = resultP1.FieldMappings.FirstOrDefault(f => f.InternalName == "MetierId" && !string.IsNullOrEmpty(f.MappedCsvHeader));
                            if (champMetierMappe == null)
                            {
                                MessageBox.Show("Le champ 'Métier Requis' est obligatoire.", "Mapping Incomplet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                currentStep = WizardStep.Step1_ColumnMapping;
                                continue;
                            }
                            var configP2 = PreparerConfig_P2_Taches(filePath, resultP1);
                            if (!configP2.SourceValues.Any()) { currentStep = WizardStep.Step3_Validation; continue; }

                            var (viewP2, hostP2) = AfficherVue<ImportP2>("Étape 2/3: Mapper les métiers", configP2);
                            currentHost = hostP2;
                            if (hostP2.DialogResult == DialogResult.OK)
                            {
                                resultP2 = viewP2.Result;
                                MemoriserChoixValeurs(resultP2, configP2.SuggestedMappings);
                                currentStep = WizardStep.Step3_Validation;
                            }
                            else if (hostP2.DialogResult == DialogResult.Abort) { currentStep = WizardStep.Step1_ColumnMapping; }
                            else { currentStep = WizardStep.Canceled; }
                            break;

                        case WizardStep.Step3_Validation:
                            var (importPlan, rejets) = TransformerDonnees_Taches(filePath, lotCible, resultP1, resultP2);
                            var bindingList = new BindingList<Tache>(importPlan.NouvellesTaches);
                            var bindingSource = new BindingSource { DataSource = bindingList };
                            var configP3 = new ImportP3Config { EntityDisplayName = "Tâches", EntityImage = Properties.Resources.Import_Task, DataSource = bindingSource, RejectedRows = rejets };
                            var (viewP3, hostP3) = AfficherVue<ImportP3>("Étape 3/3: Valider les tâches", configP3);
                            currentHost = hostP3;

                            if (hostP3.DialogResult == DialogResult.OK)
                            {
                                importPlan.NouvellesTaches = new List<Tache>(viewP3.Result.FinalData.OfType<Tache>());
                                viewP3.SetWaitState(true);
                                var importResult = _importService.ImporterTaches(importPlan, lotCible.LotId, remplacerExistants);
                                viewP3.SetWaitState(false);

                                if (importResult.EstSucces)
                                {
                                    currentStep = WizardStep.Finished;
                                    hostP3.Close();
                                    return importResult;
                                }
                                viewP3.ShowImportError(importResult.MessageErreur);
                            }
                            else if (hostP3.DialogResult == DialogResult.Abort) { currentStep = WizardStep.Step2_ValueMapping; }
                            else { currentStep = WizardStep.Canceled; }
                            break;
                    }
                }
                finally { currentHost?.Dispose(); }
            }
            return ImportResult.Echec("Importation annulée.");
        }
        #endregion

        #region Méthodes de Préparation de Configuration

        private ImportP1Config PreparerConfig_P1_Ouvriers(string filePath)
        {
            var fieldsToMap = new List<MappingFieldDefinition>
            {
                new MappingFieldDefinition { InternalName = "Nom", DisplayName = "Nom de famille", IsMandatory = true, AllowDefaultValue = false },
                new MappingFieldDefinition { InternalName = "Prenom", DisplayName = "Prénom", IsMandatory = true, AllowDefaultValue = false },
                new MappingFieldDefinition { InternalName = "CoutJournalier", DisplayName = "Coût journalier", IsMandatory = true, AllowDefaultValue = true },
                new MappingFieldDefinition { InternalName = "Metier", DisplayName = "Compétence (Métier)", IsMandatory = true, AllowDefaultValue = false }
            };

            var (headers, preview) = LirePreviewCsv(filePath);
            var suggestions = SuggérerMappingsColonnes(headers, fieldsToMap);

            return new ImportP1Config
            {
                EntityDisplayName = "Ouvriers",
                EntityImage = Properties.Resources.Import_Ouvriers,
                FieldsToMap = fieldsToMap,
                CsvHeaders = headers,
                DataPreview = preview,
                SuggestedMappings = suggestions
            };
        }

        private ImportP2Config PreparerConfig_P2_Ouvriers(string filePath, ImportP1Result p1Result)
        {
            var colonneMetierCsv = p1Result.FieldMappings.First(f => f.InternalName == "Metier").MappedCsvHeader;
            var sourceValues = LireValeursUniques(filePath, p1Result.HasHeader, colonneMetierCsv);

            var targetValues = _ressourceService.GetAllMetiers()
                .Select(m => new TargetValueItem { Id = m.MetierId, DisplayName = m.Nom })
                .ToList();

            var suggestions = SuggérerMappingsValeurs(sourceValues, targetValues);

            return new ImportP2Config
            {
                EntityDisplayName = "Ouvriers",
                EntityImage = Properties.Resources.Import_Ouvriers,
                ValueCategoryName = "Métiers",
                SourceValues = sourceValues,
                TargetValues = targetValues,
                SuggestedMappings = suggestions
            };
        }

        #endregion

        #region Méthode de Transformation (Ouvriers)

        private (List<Ouvrier> objetsValides, List<RejectedRowInfo> rejets) TransformerDonnees_Ouvriers(
            string filePath, ImportP1Result p1Result, ImportP2Result p2Result)
        {
            // TODO [DEBT]: Déplacer cette logique dans ImportService ou un service de transformation dédié.
            var ouvriersValides = new List<Ouvrier>();
            var rejets = new List<RejectedRowInfo>();
            var ouvriersEnConstruction = new Dictionary<string, Ouvrier>();

            var lignesCsv = File.ReadAllLines(filePath, Encoding.Default).ToList();
            string delimiter = _importService.DetectCsvDelimiter(filePath);

            var dataStartIndex = p1Result.HasHeader ? 1 : 0;

            // Préparer les mappings pour un accès rapide
            var p1Mappings = p1Result.FieldMappings.ToDictionary(m => m.InternalName, m => m);
            var metierValueMappings = p2Result?.AllMappingDecisions
                .Where(d => d.Action == MappingAction.MapToExisting)
                .ToDictionary(d => d.SourceValue, d => d.MappedTargetId, StringComparer.OrdinalIgnoreCase);

            for (int i = dataStartIndex; i < lignesCsv.Count; i++)
            {
                string line = lignesCsv[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] columns = line.Split(delimiter[0]);

                string GetValue(string internalName)
                {
                    if (p1Mappings.TryGetValue(internalName, out var mapping) && !string.IsNullOrEmpty(mapping.MappedCsvHeader))
                    {
                        var headerIndex = p1Result.HasHeader ? LirePreviewCsv(filePath).headers.IndexOf(mapping.MappedCsvHeader) : int.Parse(mapping.MappedCsvHeader.Replace("Colonne ", "")) - 1;
                        if (headerIndex >= 0 && headerIndex < columns.Length)
                        {
                            var value = columns[headerIndex].Trim();
                            if (!string.IsNullOrEmpty(value)) return value;
                        }
                    }
                    return p1Mappings.ContainsKey(internalName) ? p1Mappings[internalName].DefaultValue : null;
                }

                try
                {

                    // 1. Extraire TOUTES les valeurs de la ligne
                    string nom = GetValue("Nom");
                    string prenom = GetValue("Prenom");
                    string coutStr = GetValue("CoutJournalier");
                    string metierCsv = GetValue("Metier");

                    // 2. Valider TOUTES les valeurs
                    if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenom))
                    {
                        throw new Exception("Le nom ou le prénom est manquant.");
                    }
                    if (!int.TryParse(coutStr, out int cout))
                    {
                        throw new Exception("Le coût journalier est invalide.");
                    }
                    if (string.IsNullOrWhiteSpace(metierCsv))
                    {
                        throw new Exception("La compétence (métier) est manquante.");
                    }
                    if (metierValueMappings == null || !metierValueMappings.TryGetValue(metierCsv, out string metierId))
                    {
                        throw new Exception($"La compétence '{metierCsv}' n'a pas été mappée à un métier existant.");
                    }

                    // 3. Si tout est valide, on peut maintenant créer/modifier l'objet Ouvrier
                    string cleOuvrier = $"{nom}|{prenom}".ToUpperInvariant();
                    if (!ouvriersEnConstruction.TryGetValue(cleOuvrier, out var ouvrierCourant))
                    {
                        // On ne crée l'ouvrier que maintenant
                        ouvrierCourant = new Ouvrier { Nom = nom, Prenom = prenom, CoutJournalier = cout };
                        ouvriersEnConstruction[cleOuvrier] = ouvrierCourant;
                    }

                    // 4. Ajouter la compétence (qui est maintenant garantie d'être valide)
                    if (!ouvrierCourant.Competences.Any(c => c.MetierId == metierId))
                    {
                        ouvrierCourant.Competences.Add(new PlanAthena.Services.Business.DTOs.CompetenceOuvrier { MetierId = metierId });
                    }
                }
                catch (Exception ex)
                {
                    rejets.Add(new RejectedRowInfo { OriginalLineNumber = i + 1, RawRowContent = line, Reason = ex.Message });
                }
            }

            // Finalisation : définir le métier principal
            foreach (var ouvrier in ouvriersEnConstruction.Values)
            {
                if (ouvrier.Competences.Any() && !ouvrier.Competences.Any(c => c.EstMetierPrincipal))
                {
                    ouvrier.Competences.First().EstMetierPrincipal = true;
                }
                ouvriersValides.Add(ouvrier);
            }

            return (ouvriersValides, rejets);
        }

        #endregion

        #region Méthodes de Préparation de Configuration (Tâches)

        private ImportP1Config PreparerConfig_P1_Taches(string filePath, Lot lotCible)
        {
            var fieldsToMap = new List<MappingFieldDefinition>
    {
        new MappingFieldDefinition { InternalName = "TacheNom", DisplayName = "Nom de la Tâche", IsMandatory = true, AllowDefaultValue = false },
        new MappingFieldDefinition { InternalName = "HeuresHommeEstimees", DisplayName = "Heures Estimées", IsMandatory = true, AllowDefaultValue = true },
        new MappingFieldDefinition { InternalName = "MetierId", DisplayName = "Métier Requis", IsMandatory = true, AllowDefaultValue = false },
        new MappingFieldDefinition { InternalName = "BlocId", DisplayName = "Nom du Bloc/Zone", IsMandatory = true, AllowDefaultValue = true },
        new MappingFieldDefinition { InternalName = "IdImporte", DisplayName = "ID Externe (optionnel)", IsMandatory = false, AllowDefaultValue = false },
        new MappingFieldDefinition { InternalName = "Dependencies", DisplayName = "Dépendances (optionnel)", IsMandatory = false, AllowDefaultValue = false },
        new MappingFieldDefinition { InternalName = "ExclusionsDependances", DisplayName = "Exclusions (optionnel)", IsMandatory = false, AllowDefaultValue = false },
        new MappingFieldDefinition { InternalName = "EstJalon", DisplayName = "Est un jalon (optionnel)", IsMandatory = false, AllowDefaultValue = false }
    };

            var (headers, preview) = LirePreviewCsv(filePath);
            var suggestions = SuggérerMappingsColonnes(headers, fieldsToMap);

            return new ImportP1Config
            {
                EntityDisplayName = $"Tâches (Lot: {lotCible.Nom})",
                EntityImage = Properties.Resources.Import_Task,
                FieldsToMap = fieldsToMap,
                CsvHeaders = headers,
                DataPreview = preview,
                SuggestedMappings = suggestions
            };
        }

        private ImportP2Config PreparerConfig_P2_Taches(string filePath, ImportP1Result p1Result)
        {
            var colonneMetierCsv = p1Result.FieldMappings.First(f => f.InternalName == "MetierId").MappedCsvHeader;
            var sourceValues = LireValeursUniques(filePath, p1Result.HasHeader, colonneMetierCsv);

            var targetValues = _ressourceService.GetAllMetiers()
                .Select(m => new TargetValueItem { Id = m.MetierId, DisplayName = m.Nom })
                .ToList();

            var suggestions = SuggérerMappingsValeurs(sourceValues, targetValues);

            return new ImportP2Config
            {
                EntityDisplayName = "Tâches",
                EntityImage = Properties.Resources.Import_Task,
                ValueCategoryName = "Métiers",
                SourceValues = sourceValues,
                TargetValues = targetValues,
                SuggestedMappings = suggestions
            };
        }

        #endregion

        #region Méthode de Transformation (Tâches)
        private (TachesImportPlan plan, List<RejectedRowInfo> rejets) TransformerDonnees_Taches(
            string filePath, Lot lotCible, ImportP1Result p1Result, ImportP2Result p2Result)
        {
            var plan = new TachesImportPlan();
            var rejets = new List<RejectedRowInfo>();
            var blocsTemporaires = new Dictionary<string, Bloc>(StringComparer.OrdinalIgnoreCase);

            var lignesCsv = File.ReadAllLines(filePath, Encoding.Default).ToList();
            string delimiter = _importService.DetectCsvDelimiter(filePath);
            var dataStartIndex = p1Result.HasHeader ? 1 : 0;
            var p1Mappings = p1Result.FieldMappings.ToDictionary(m => m.InternalName, m => m);
            var metierValueMappings = p2Result?.AllMappingDecisions
                .Where(d => d.Action == MappingAction.MapToExisting)
                .ToDictionary(d => d.SourceValue, d => d.MappedTargetId, StringComparer.OrdinalIgnoreCase);

            var headers = p1Result.HasHeader ? LirePreviewCsv(filePath).headers : null;

            for (int i = dataStartIndex; i < lignesCsv.Count; i++)
            {
                string line = lignesCsv[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] columns = line.Split(delimiter[0]);

                string GetValue(string internalName)
                {
                    if (p1Mappings.TryGetValue(internalName, out var mapping) && !string.IsNullOrEmpty(mapping.MappedCsvHeader))
                    {
                        int headerIndex = -1;
                        if (p1Result.HasHeader)
                        {
                            // Correction : Utiliser la liste de headers déjà connue, pas relire le fichier.
                            headerIndex = headers?.IndexOf(mapping.MappedCsvHeader) ?? -1;
                        }
                        else
                        {
                            // Correction: Le nom de la colonne est "Colonne X", il faut parser le X.
                            if (mapping.MappedCsvHeader.StartsWith("Colonne "))
                            {
                                int.TryParse(mapping.MappedCsvHeader.Substring(8), out int colNum);
                                headerIndex = colNum - 1;
                            }
                        }

                        if (headerIndex >= 0 && headerIndex < columns.Length)
                        {
                            var value = columns[headerIndex].Trim();
                            if (!string.IsNullOrEmpty(value)) return value;
                        }
                    }
                    return p1Mappings.ContainsKey(internalName) ? p1Mappings[internalName].DefaultValue : null;
                }

                try
                {
                    string nomTache = GetValue("TacheNom");
                    if (string.IsNullOrWhiteSpace(nomTache)) throw new Exception("Nom de tâche manquant.");
                    // Déterminer si c'est un jalon EN AVANCE
                    string estJalonStr = GetValue("EstJalon");
                    bool estJalon = (estJalonStr ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                    var typeActivite = estJalon ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;

                    if (!int.TryParse(GetValue("HeuresHommeEstimees"), out int heures)) throw new Exception($"Heures estimées invalides pour '{nomTache}'.");

                    string metierCsv = GetValue("MetierId");
                    string metierId = null;

                    if (!estJalon)
                    {
                        if (string.IsNullOrWhiteSpace(metierCsv)) throw new Exception($"Métier manquant pour la tâche '{nomTache}'.");
                        if (metierValueMappings == null || !metierValueMappings.TryGetValue(metierCsv, out metierId))
                        {
                            throw new Exception($"Métier '{metierCsv}' non mappé pour la tâche '{nomTache}'.");
                        }
                    }
                    string blocNom = GetValue("BlocId");
                    if (string.IsNullOrWhiteSpace(blocNom)) throw new Exception($"Nom de bloc manquant pour '{nomTache}'.");

                    if (!blocsTemporaires.TryGetValue(blocNom, out Bloc blocTemp))
                    {
                        blocTemp = new Bloc { BlocId = $"TEMP_{blocNom}", Nom = blocNom, CapaciteMaxOuvriers = 6, LotId = lotCible.LotId };
                        plan.NouveauxBlocs.Add(blocTemp);
                        blocsTemporaires[blocNom] = blocTemp;
                    }

                    var nouvelleTache = new Tache
                    {
                        LotId = lotCible.LotId,
                        BlocId = blocTemp.BlocId,
                        TacheNom = nomTache,
                        HeuresHommeEstimees = heures,
                        MetierId = metierId,
                        IdImporte = GetValue("IdImporte"),
                        Dependencies = GetValue("Dependencies"),
                        ExclusionsDependances = GetValue("ExclusionsDependances"),
                        Type = typeActivite
                    };
                    plan.NouvellesTaches.Add(nouvelleTache);
                }
                catch (Exception ex)
                {
                    rejets.Add(new RejectedRowInfo { OriginalLineNumber = i + 1, RawRowContent = line, Reason = ex.Message });
                }
            }
            return (plan, rejets);
        }
        #endregion

        #region Méthodes Utilitaires

        private (DialogResult, bool) DemanderModeImport()
        {
            var confirmResult = MessageBox.Show(
                "Comment voulez-vous importer les données ?\n\n" +
                "- 'Oui' : Efface TOUS les ouvriers actuels avant d'importer.\n" +
                "- 'Non' : Ajoute les nouveaux ouvriers et met à jour ceux existants.\n" +
                "- 'Annuler' : Ne fait rien.",
                "Mode d'importation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            return (confirmResult, confirmResult == DialogResult.Yes);
        }

        private (T, Form) AfficherVue<T>(string title, object config) where T : UserControl, new()
        {
            var host = CreerWizardHostForm(title);
            var view = new T { Dock = DockStyle.Fill };

            var initializeMethod = typeof(T).GetMethod("Initialize");
            if (initializeMethod == null) throw new InvalidOperationException($"La vue {typeof(T).Name} doit avoir une méthode publique 'Initialize'.");
            initializeMethod.Invoke(view, new object[] { config });

            // Abonnement aux événements
            Action<string, DialogResult> subscribe = (eventName, dialogResult) =>
            {
                var eventInfo = view.GetType().GetEvent(eventName);
                if (eventInfo != null)
                {
                    var handler = new EventHandler((s, e) =>
                    {
                        host.DialogResult = dialogResult;
                        if (dialogResult != DialogResult.None) host.Close();
                    });
                    eventInfo.AddEventHandler(view, handler);
                }
            };

            subscribe("SuivantClicked", DialogResult.OK);
            subscribe("TerminerClicked", DialogResult.OK);
            subscribe("ValiderClicked", DialogResult.OK);
            subscribe("AnnulerClicked", DialogResult.Cancel);
            subscribe("RetourClicked", DialogResult.Abort);

            host.Controls.Add(view);
            host.ShowDialog();

            return (view, host);
        }

        private Form CreerWizardHostForm(string title)
        {
            return new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterScreen,
                Size = new System.Drawing.Size(1020, 680),
                MinimumSize = new System.Drawing.Size(980, 500),
                FormBorderStyle = FormBorderStyle.Sizable,
                MaximizeBox = true,
                MinimizeBox = true
            };
        }

        private void MemoriserChoixColonnes(ImportP1Result p1Result, Dictionary<string, string> suggestionsInitiales)
        {
            if (!p1Result.ShouldMemorizeMappings) return;

            foreach (var mapping in p1Result.FieldMappings)
            {
                if (!string.IsNullOrEmpty(mapping.MappedCsvHeader) &&
                    (!suggestionsInitiales.TryGetValue(mapping.InternalName, out var suggestion) || suggestion != mapping.MappedCsvHeader))
                {
                    _valueMappingService.AjouteCorrespondance(mapping.MappedCsvHeader, mapping.InternalName);
                }
            }
        }

        private void MemoriserChoixValeurs(ImportP2Result p2Result, Dictionary<string, string> suggestionsInitiales)
        {
            if (!p2Result.ShouldMemorizeMappings) return;

            foreach (var decision in p2Result.AllMappingDecisions)
            {
                suggestionsInitiales.TryGetValue(decision.SourceValue, out var suggestion);

                // Si la décision est différente de la suggestion (même si la suggestion était nulle)
                if (decision.MappedTargetId != suggestion)
                {
                    // Supprimer l'ancienne correspondance si elle existait
                    if (suggestion != null)
                    {
                        _valueMappingService.SupprimeCorrespondance(decision.SourceValue);
                    }
                    // Ajouter la nouvelle si elle est valide
                    if (decision.Action == MappingAction.MapToExisting)
                    {
                        _valueMappingService.AjouteCorrespondance(decision.SourceValue, decision.MappedTargetId);
                    }
                }
            }
        }

        // TODO [DEBT]: Déplacer les 2 méthodes suivantes dans CsvDataService
        private (List<string> headers, List<string[]> preview) LirePreviewCsv(string filePath)
        {
            var headers = new List<string>();
            var preview = new List<string[]>();
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.Default);
                if (lines.Length == 0) return (headers, preview);

                string delimiter = _importService.DetectCsvDelimiter(filePath);

                headers.AddRange(lines[0].Split(delimiter[0]).Select(h => h.Trim()));

                for (int i = 1; i < lines.Length && i <= 10; i++)
                {
                    preview.Add(lines[i].Split(delimiter[0]));
                }
            }
            catch { /* Gérer l'erreur silencieusement pour la preview */ }
            return (headers, preview);
        }

        private List<string> LireValeursUniques(string filePath, bool hasHeader, string columnName)
        {
            var uniqueValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.Default);
                if (lines.Length < (hasHeader ? 2 : 1)) return new List<string>();

                string delimiter = _importService.DetectCsvDelimiter(filePath);

                var headers = lines[0].Split(delimiter[0]).Select(h => h.Trim()).ToList();
                int columnIndex = headers.IndexOf(columnName);
                if (columnIndex == -1) return new List<string>();

                int startIndex = hasHeader ? 1 : 0;
                for (int i = startIndex; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(delimiter[0]);
                    if (columnIndex < columns.Length)
                    {
                        var value = columns[columnIndex].Trim();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            uniqueValues.Add(value);
                        }
                    }
                }
            }
            catch { /* Ignorer les erreurs de lecture */ }
            return uniqueValues.OrderBy(v => v).ToList();
        }

        // --- Méthodes de suggestion de mapping ---
        private Dictionary<string, string> SuggérerMappingsColonnes(List<string> csvHeaders, List<MappingFieldDefinition> fieldsToMap)
        {
            var suggestions = new Dictionary<string, string>();
            foreach (var field in fieldsToMap)
            {
                // Tenter une correspondance directe
                var directMatch = csvHeaders.FirstOrDefault(h => h.Equals(field.DisplayName, StringComparison.OrdinalIgnoreCase));
                if (directMatch != null)
                {
                    suggestions[field.InternalName] = directMatch;
                    continue;
                }

                // Tenter une correspondance via le service de mapping
                foreach (var header in csvHeaders)
                {
                    if (_valueMappingService.TrouveCorrespondance(header) == field.InternalName)
                    {
                        suggestions[field.InternalName] = header;
                        break;
                    }
                }
            }
            return suggestions;
        }

        private Dictionary<string, string> SuggérerMappingsValeurs(List<string> sourceValues, List<TargetValueItem> targetValues)
        {
            var suggestions = new Dictionary<string, string>();
            var targetMap = targetValues.ToDictionary(t => t.DisplayName, t => t.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var source in sourceValues)
            {
                // Tenter une correspondance directe par nom
                if (targetMap.TryGetValue(source, out string targetId))
                {
                    suggestions[source] = targetId;
                    continue;
                }

                // Tenter une correspondance via le service de mapping
                var mappedId = _valueMappingService.TrouveCorrespondance(source);
                if (!string.IsNullOrEmpty(mappedId) && targetValues.Any(t => t.Id == mappedId))
                {
                    suggestions[source] = mappedId;
                }
            }
            return suggestions;
        }

        #endregion
    }
}