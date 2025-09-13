// Fichier destiné à disparaitre
// sera remplacé par le nouveau service d'import

using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using System.Data;
using System.Text;

namespace PlanAthena.View.Utils
{
    /// <summary>
    /// Formulaire interactif pour l'import de tâches depuis un fichier CSV,
    /// permettant à l'utilisateur de mapper les colonnes du CSV aux champs de PlanAthena.
    /// </summary>
    public partial class ImportMappingView : Form
    {
        private Lot _lotActif;
        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private List<string> _csvHeaders = new List<string>();
        private List<Dictionary<string, string>> _csvDataPreview = new List<Dictionary<string, string>>();
        private ImportMappingConfiguration _mappingConfiguration = new ImportMappingConfiguration();
        private string _filePath;

        // Drapeau pour éviter la récursivité lors des mises à jour programmatiques des ComboBoxes
        private bool _isUpdatingComboBoxes = false;

        // Définition des champs de PlanAthena à mapper, avec leur libellé, leur propriété dans Tache et leur caractère obligatoire/par défaut.
        // L'ordre ici détermine l'ordre d'affichage dans le TableLayoutPanel.
        private List<TacheMappingFields> _tacheFieldsToMap = new List<TacheMappingFields>
        {
            // Champs obligatoires
            new TacheMappingFields("Nom de la Tâche", nameof(Tache.TacheNom), true),
            new TacheMappingFields("Heures Estimées", nameof(Tache.HeuresHommeEstimees), true), // Peut avoir une valeur par défaut
            new TacheMappingFields("ID du Métier", nameof(Tache.MetierId), true),
            new TacheMappingFields("ID du Bloc", nameof(Tache.BlocId), true), // Peut avoir une valeur par défaut, et sera toujours assigné à un bloc

            // Champs optionnels
            new TacheMappingFields("ID d'origine", nameof(Tache.IdImporte), false),
            new TacheMappingFields("Dépendances (IDs)", nameof(Tache.Dependencies), false),
            new TacheMappingFields("Exclusions Dépendances (IDs)", nameof(Tache.ExclusionsDependances), false),
            new TacheMappingFields("Type d'activité (Est Jalon ?)", nameof(Tache.EstJalon), false)
        };

        // Dictionnaire pour stocker les ComboBoxes par nom de propriété PlanAthena
        private Dictionary<string, ComboBox> _fieldComboBoxes = new Dictionary<string, ComboBox>();

        public ImportMappingConfiguration MappingConfiguration => _mappingConfiguration;

        public ImportMappingView(string filePath, Lot lotActif, ProjetService projetService, RessourceService ressourceService)
        {
            InitializeComponent();
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _lotActif = lotActif ?? throw new ArgumentNullException(nameof(lotActif));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));

            this.Text = $"Importer des tâches dans le lot '{_lotActif.Nom}'";
            lblSelectedLot.Text = $"Lot cible : {_lotActif.Nom} ({_lotActif.LotId})";

            InitializeMappingUI();
            LoadCsvFile();
            CheckValidationStatus(); // Vérifier l'état initial du bouton d'import
            _ressourceService = ressourceService;
        }

        private void InitializeMappingUI()
        {
            // Initialisation des valeurs par défaut dans l'UI
            numHeuresEstimeesDefaut.Value = 2;
            numCapaciteMaxOuvriersDefaut.Value = 3;
            txtNomBlocParDefaut.Text = "Nouveau";

            // Configuration du TableLayoutPanel pour le mappage
            tlpMapping.ColumnCount = _tacheFieldsToMap.Count;
            tlpMapping.RowCount = 2; // Labels + ComboBoxes
            tlpMapping.ColumnStyles.Clear(); // Nettoyer les styles existants

            for (int i = 0; i < _tacheFieldsToMap.Count; i++)
            {
                tlpMapping.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / _tacheFieldsToMap.Count));
            }

            // Ajout des labels et ComboBoxes pour chaque champ PlanAthena
            for (int i = 0; i < _tacheFieldsToMap.Count; i++)
            {
                var field = _tacheFieldsToMap[i];

                // Label
                var lbl = new Label
                {
                    Text = field.DisplayName + (field.IsMandatory ? " *" : ""),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.BottomCenter,
                    Font = new Font(this.Font, FontStyle.Bold),
                    AutoSize = true
                };
                if (field.IsMandatory) lbl.ForeColor = Color.DarkRed; // Indication visuelle pour obligatoire

                tlpMapping.Controls.Add(lbl, i, 0);

                // ComboBox
                var cmb = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList // Empêche la saisie manuelle
                };
                cmb.Tag = field.PropertyName; // Stocke le nom de la propriété PlanAthena
                cmb.SelectedIndexChanged += ComboBox_MappingSelectionChanged; // Abonnement à l'événement

                _fieldComboBoxes[field.PropertyName] = cmb; // Stocke la ComboBox dans le dictionnaire
                tlpMapping.Controls.Add(cmb, i, 1);
            }
        }

        /// <summary>
        /// Charge le fichier CSV, détecte les en-têtes et remplit l'aperçu du DataGridView.
        /// </summary>
        private void LoadCsvFile()
        {
            try
            {
                var lines = File.ReadAllLines(_filePath);
                if (lines.Length == 0)
                {
                    LogMessage("Le fichier CSV est vide.", true);
                    return;
                }

                // Détection du séparateur
                char separator = lines[0].Contains('\t') ? '\t' : ';';
                LogMessage($"Séparateur détecté : '{(separator == '\t' ? "TAB" : ";")}'", false);

                // Détection des en-têtes (heuristique simple pour le POC)
                string[] firstLineValues = lines[0].Split(separator);
                bool potentialHeaderDetected = firstLineValues.Any(v => !double.TryParse(v, out _) && !(v.All(char.IsLower) || v.All(char.IsUpper)));

                // On met à jour la checkbox. Si l'état de la checkbox change, cela déclenchera chkHasHeader_CheckedChanged.
                // Sinon, il faut appeler PopulateHeadersAndPreview directement pour la première initialisation.
                chkHasHeader.Checked = potentialHeaderDetected;

                // Si le gestionnaire chkHasHeader_CheckedChanged n'a pas été déclenché (parce que l'état de la checkbox n'a pas changé),
                // il faut appeler PopulateHeadersAndPreview manuellement pour la première initialisation.
                if (chkHasHeader.Checked == potentialHeaderDetected && dgvCsvPreview.ColumnCount == 0) // Vérifie que le DGV n'est pas encore rempli
                {
                    PopulateHeadersAndPreview(lines, separator);
                }

                LogMessage($"Détection automatique : Première ligne {(potentialHeaderDetected ? "identifiée comme en-têtes." : "identifiée comme données.")}", false);
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors du chargement du fichier CSV : {ex.Message}", true);
                btnConfirmImport.Enabled = false; // Désactiver l'import en cas d'erreur de chargement
            }
        }

        /// <summary>
        /// Remplit les en-têtes et l'aperçu du DataGridView en fonction de la checkbox HasHeader.
        /// </summary>
        private void PopulateHeadersAndPreview(string[] lines, char separator)
        {
            dgvCsvPreview.Columns.Clear();
            _csvHeaders.Clear();
            _csvDataPreview.Clear();

            int dataStartIndex = 0;
            if (chkHasHeader.Checked)
            {
                _csvHeaders = lines[0].Split(separator).Select(h => h.Trim()).ToList();
                dataStartIndex = 1;
            }
            else
            {
                int maxColumns = lines[0].Split(separator).Length;
                for (int i = 0; i < maxColumns; i++)
                {
                    _csvHeaders.Add($"Colonne {i + 1}");
                }
                dataStartIndex = 0; // Commencer à lire les données dès la première ligne
            }

            // Ajouter les colonnes au DataGridView
            foreach (var header in _csvHeaders)
            {
                dgvCsvPreview.Columns.Add(header, header);
            }

            // Remplir l'aperçu des données
            int previewLinesCount = 0;
            for (int i = dataStartIndex; i < lines.Length && previewLinesCount < 6; i++) // Max 6 lignes pour l'aperçu
            {
                var values = lines[i].Split(separator);
                var rowDict = new Dictionary<string, string>();
                var dgvRow = new DataGridViewRow();
                dgvRow.CreateCells(dgvCsvPreview);

                for (int j = 0; j < _csvHeaders.Count; j++)
                {
                    var value = j < values.Length ? values[j].Trim() : "";
                    rowDict[_csvHeaders[j]] = value;
                    dgvRow.Cells[j].Value = value;
                }
                _csvDataPreview.Add(rowDict);
                dgvCsvPreview.Rows.Add(dgvRow);
                previewLinesCount++;
            }

            UpdateMappingComboBoxes(); // Mettre à jour les ComboBoxes de mappage avec les nouveaux en-têtes
        }


        /// <summary>
        /// Met à jour les éléments des ComboBoxes de mappage en fonction des en-têtes CSV disponibles
        /// et gère les pré-sélections intelligentes.
        /// </summary>
        private void UpdateMappingComboBoxes()
        {
            if (_isUpdatingComboBoxes) return; // Éviter la réentrance

            _isUpdatingComboBoxes = true; // Définir le drapeau

            try
            {
                // Sauvegarder les sélections actuelles pour tenter de les restaurer
                var currentSelections = new Dictionary<string, string>();
                foreach (var cmbEntry in _fieldComboBoxes)
                {
                    if (cmbEntry.Value.SelectedItem is string selectedHeader)
                    {
                        currentSelections[cmbEntry.Key] = selectedHeader;
                    }
                }

                // Remplir toutes les ComboBoxes avec les en-têtes CSV disponibles
                // Puis appliquer la pré-sélection intelligente ou restaurer la sélection précédente
                foreach (var field in _tacheFieldsToMap)
                {
                    if (_fieldComboBoxes.TryGetValue(field.PropertyName, out ComboBox cmb))
                    {
                        cmb.Items.Clear();
                        cmb.Items.Add(""); // Option vide pour "non mappé"

                        foreach (var header in _csvHeaders)
                        {
                            cmb.Items.Add(header);
                        }

                        string bestMatch = GetStrictBestMatch(field.PropertyName, _csvHeaders);
                        if (!string.IsNullOrEmpty(bestMatch) && _csvHeaders.Contains(bestMatch))
                        {
                            cmb.SelectedItem = bestMatch;
                        }
                        else if (currentSelections.TryGetValue(field.PropertyName, out string previousSelection) && !string.IsNullOrEmpty(previousSelection) && _csvHeaders.Contains(previousSelection))
                        {
                            // Tenter de restaurer la sélection précédente si l'en-tête existe toujours
                            cmb.SelectedItem = previousSelection;
                        }
                        else
                        {
                            cmb.SelectedItem = ""; // Réinitialiser si pas de match ou si l'en-tête n'est plus là
                        }
                    }
                }

                // Mettre à jour les ComboBoxes pour éviter les doublons (reflet des sélections)
                // Cette méthode est appelée après que toutes les ComboBoxes ont été repeuplées et pré-sélectionnées.
                RefreshAvailableComboBoxItemsExcludingDuplicates();

            }
            finally
            {
                _isUpdatingComboBoxes = false; // Réinitialiser le drapeau, même en cas d'erreur
            }
        }

        /// <summary>
        /// Met à jour les éléments disponibles dans toutes les ComboBoxes pour éviter les sélections en double.
        /// Cette version est appelée APRÈS que les sélections initiales/restaurées ont été faites.
        /// </summary>
        private void RefreshAvailableComboBoxItemsExcludingDuplicates()
        {
            if (_isUpdatingComboBoxes) return; // Éviter la réentrance si un ComboBox a déclenché l'événement pendant son propre rafraîchissement
                                               // Normalement, ceci ne devrait pas être déclenché si _isUpdatingComboBoxes est géré dans UpdateMappingComboBoxes,
                                               // mais c'est une sécurité supplémentaire.

            _isUpdatingComboBoxes = true; // Définir le drapeau

            try
            {
                var selectedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Collecter toutes les sélections actuelles pour les considérer comme "prises"
                foreach (var cmbEntry in _fieldComboBoxes)
                {
                    if (cmbEntry.Value.SelectedItem is string selectedHeader && !string.IsNullOrEmpty(selectedHeader))
                    {
                        selectedHeaders.Add(selectedHeader);
                    }
                }

                // Mettre à jour les listes d'items de chaque ComboBox pour n'inclure que les non-sélectionnés (ou sa propre sélection)
                foreach (var cmbEntry in _fieldComboBoxes)
                {
                    var currentSelected = cmbEntry.Value.SelectedItem as string; // Garder la sélection actuelle

                    // Désabonner avant de modifier Items
                    cmbEntry.Value.SelectedIndexChanged -= ComboBox_MappingSelectionChanged;

                    cmbEntry.Value.Items.Clear();
                    cmbEntry.Value.Items.Add(""); // Option vide

                    foreach (var header in _csvHeaders)
                    {
                        // Ajouter l'en-tête si ce n'est pas déjà sélectionné par une AUTRE ComboBox
                        // OU si c'est la colonne actuellement sélectionnée par cette ComboBox elle-même.
                        if (!selectedHeaders.Contains(header) || header.Equals(currentSelected, StringComparison.OrdinalIgnoreCase))
                        {
                            cmbEntry.Value.Items.Add(header);
                        }
                    }
                    // Restaurer la sélection actuelle (même si elle était vide)
                    cmbEntry.Value.SelectedItem = currentSelected;

                    // Réabonner après modification
                    cmbEntry.Value.SelectedIndexChanged += ComboBox_MappingSelectionChanged;
                }
            }
            finally
            {
                _isUpdatingComboBoxes = false; // Réinitialiser le drapeau
            }
        }


        /// <summary>
        /// Trouve la meilleure correspondance stricte pour un champ PlanAthena parmi les en-têtes CSV.
        /// </summary>
        private string GetStrictBestMatch(string planAthenaPropertyName, List<string> csvHeaders)
        {
            // Définition des synonymes possibles (case-insensitive)
            var synonyms = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(Tache.IdImporte), new List<string> { "ID", "Identifiant", "OriginalID", "ExternalID" } },
                { nameof(Tache.TacheNom), new List<string> { "TaskName", "NomTache", "Intitulé", "Libellé" } },
                { nameof(Tache.HeuresHommeEstimees), new List<string> { "WorkLoad", "Heures", "Duration", "Durée" } },
                { nameof(Tache.MetierId), new List<string> { "Metier", "Trade", "Profession", "Job" } },
                { nameof(Tache.BlocId), new List<string> { "Room", "Location", "Workspace", "Area" } },
                { nameof(Tache.Dependencies), new List<string> { "Dependencies", "Dépendances", "Prerequis" } },
                { nameof(Tache.ExclusionsDependances), new List<string> { "Exclusions", "ExcludeDeps" } },
                { nameof(Tache.EstJalon), new List<string> { "IsMilestone", "Jalon", "Type", "ActivityType" } }
            };

            List<string> potentialNames = new List<string>();
            potentialNames.Add(planAthenaPropertyName); // Nom de la propriété exact
            if (synonyms.TryGetValue(planAthenaPropertyName, out var specificSynonyms))
            {
                potentialNames.AddRange(specificSynonyms);
            }

            foreach (var potentialName in potentialNames)
            {
                string match = csvHeaders.FirstOrDefault(h => h.Equals(potentialName, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match;
                }
            }
            return null; // Aucune correspondance stricte trouvée
        }


        /// <summary>
        /// Gère le changement de sélection dans une ComboBox de mappage.
        /// </summary>
        private void ComboBox_MappingSelectionChanged(object sender, EventArgs e)
        {
            // Vérifier le drapeau pour éviter la récursivité
            if (_isUpdatingComboBoxes) return;

            // Appelle la méthode qui gère l'unicité des items
            RefreshAvailableComboBoxItemsExcludingDuplicates();
            CheckValidationStatus(); // Vérifier l'état de validation après chaque changement
            UpdateMetiersManquantsList(); // Mettre à jour la liste des métiers manquants
        }

        /// <summary>
        /// Vérifie si tous les champs obligatoires sont mappés et active/désactive le bouton d'import.
        /// </summary>
        private void CheckValidationStatus()
        {
            bool allMandatoryMapped = true;
            StringBuilder sbMessages = new StringBuilder();
            sbMessages.AppendLine("Vérification du mappage :");

            foreach (var field in _tacheFieldsToMap)
            {
                if (field.IsMandatory)
                {
                    if (!_fieldComboBoxes.TryGetValue(field.PropertyName, out ComboBox cmb) || string.IsNullOrEmpty(cmb.SelectedItem as string))
                    {
                        allMandatoryMapped = false;
                        sbMessages.AppendLine($" - Le champ obligatoire '{field.DisplayName}' n'est pas mappé.");
                    }
                    else
                    {
                        // Vérifier la cohérence de type pour les champs numériques/booléens obligatoires
                        string mappedColumn = cmb.SelectedItem as string;
                        if (!string.IsNullOrEmpty(mappedColumn) && _csvDataPreview.Any())
                        {
                            string sampleValue = GetValueOrDefault(_csvDataPreview.First(), mappedColumn);

                            if (field.PropertyName == nameof(Tache.HeuresHommeEstimees))
                            {
                                if (!string.IsNullOrEmpty(sampleValue) && !int.TryParse(sampleValue, out _))
                                {
                                    sbMessages.AppendLine($" - Avertissement: La colonne '{mappedColumn}' pour '{field.DisplayName}' semble contenir des données non numériques dans l'aperçu. Des valeurs par défaut seront utilisées pour les données invalides.");
                                }
                            }
                            // Pourrait ajouter des vérifications pour d'autres types si nécessaire (ex: booléen pour EstJalon)
                        }
                    }
                }
            }

            if (allMandatoryMapped)
            {
                btnConfirmImport.Enabled = true;
                sbMessages.AppendLine(" - Tous les champs obligatoires sont mappés.");
                sbMessages.AppendLine("Prêt pour l'import. Cliquez sur 'Confirmer l'import'.");
            }
            else
            {
                btnConfirmImport.Enabled = false;
                sbMessages.AppendLine(" - Certains champs obligatoires sont manquants. Veuillez les mapper pour continuer.");
            }

            txtLog.Text = sbMessages.ToString();
        }

        /// <summary>
        /// Met à jour la liste des métiers manquants en fonction de la colonne métier mappée.
        /// </summary>
        private void UpdateMetiersManquantsList()
        {
            lstMetiersManquants.Items.Clear();

            string metierIdColumn = _fieldComboBoxes[nameof(Tache.MetierId)].SelectedItem as string;

            if (string.IsNullOrEmpty(metierIdColumn) || !_csvDataPreview.Any())
            {
                lstMetiersManquants.Items.Add("Veuillez mapper la colonne 'ID du Métier' pour voir les métiers manquants.");
                return;
            }

            var allExistingMetiers = _ressourceService.GetAllMetiers().Select(m => m.MetierId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingMetiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in _csvDataPreview)
            {
                string metier = GetValueOrDefault(row, metierIdColumn);
                if (!string.IsNullOrWhiteSpace(metier))
                {
                    if (!allExistingMetiers.Contains(metier))
                    {
                        missingMetiers.Add(metier);
                    }
                }
            }

            if (missingMetiers.Any())
            {
                foreach (var missingMetier in missingMetiers.OrderBy(m => m)) // Tri alphabétique pour la lisibilité
                {
                    lstMetiersManquants.Items.Add(missingMetier);
                }
                LogMessage($"Avertissement: {missingMetiers.Count} métier(s) détecté(s) dans l'aperçu CSV n'existe(nt) pas dans PlanAthena. Ils devront être créés manuellement ou l'import pourrait échouer pour ces tâches si non géré.", true);
            }
            else
            {
                LogMessage("Tous les métiers de l'aperçu CSV existent dans PlanAthena.", false);
            }
        }


        /// <summary>
        /// Gère le clic sur le bouton Confirmer l'Import.
        /// </summary>
        private void btnConfirmImport_Click(object sender, EventArgs e)
        {
            // Remplir l'objet _mappingConfiguration avec les sélections de l'utilisateur
            _mappingConfiguration.HasHeaderRecord = chkHasHeader.Checked;
            _mappingConfiguration.CsvColumn_IdImporte = _fieldComboBoxes[nameof(Tache.IdImporte)].SelectedItem as string;
            _mappingConfiguration.CsvColumn_TacheNom = _fieldComboBoxes[nameof(Tache.TacheNom)].SelectedItem as string;
            _mappingConfiguration.CsvColumn_HeuresHommeEstimees = _fieldComboBoxes[nameof(Tache.HeuresHommeEstimees)].SelectedItem as string;
            _mappingConfiguration.CsvColumn_MetierId = _fieldComboBoxes[nameof(Tache.MetierId)].SelectedItem as string;
            _mappingConfiguration.CsvColumn_BlocId = _fieldComboBoxes[nameof(Tache.BlocId)].SelectedItem as string;
            _mappingConfiguration.CsvColumn_Dependencies = _fieldComboBoxes[nameof(Tache.Dependencies)].SelectedItem as string;
            _mappingConfiguration.CsvColumn_ExclusionsDependances = _fieldComboBoxes[nameof(Tache.ExclusionsDependances)].SelectedItem as string;
            _mappingConfiguration.CsvColumn_EstJalon = _fieldComboBoxes[nameof(Tache.EstJalon)].SelectedItem as string;

            // Remplir les paramètres d'import
            // La création du bloc par défaut est implicite et obligatoire.
            _mappingConfiguration.CreerBlocParDefautSiNonSpecifie = true;
            _mappingConfiguration.NomBlocParDefaut = txtNomBlocParDefaut.Text;
            _mappingConfiguration.HeuresEstimeesParDefaut = (int)numHeuresEstimeesDefaut.Value;
            _mappingConfiguration.CapaciteMaxOuvriersBlocParDefaut = (int)numCapaciteMaxOuvriersDefaut.Value;

            // Définir le DialogResult à OK pour que le formulaire appelant puisse récupérer la config.
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Gère le clic sur le bouton Annuler.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Gère le changement d'état de la checkbox "La première ligne contient des en-têtes".
        /// </summary>
        private void chkHasHeader_CheckedChanged(object sender, EventArgs e)
        {
            // Vérifier le drapeau pour éviter la récursivité
            if (_isUpdatingComboBoxes) return;

            // Recharger les en-têtes et l'aperçu basé sur le nouvel état de la checkbox
            // Lire le fichier à nouveau pour s'assurer d'avoir les lignes originales.
            // On s'assure que le séparateur est le même que celui détecté initialement.
            char separator = File.ReadAllLines(_filePath)[0].Contains('\t') ? '\t' : ';';
            PopulateHeadersAndPreview(File.ReadAllLines(_filePath), separator);
            CheckValidationStatus();
        }

        /// <summary>
        /// Enregistre un message dans la zone de log.
        /// </summary>
        /// <param name="message">Le message à logger.</param>
        /// <param name="isWarning">True si le message est un avertissement (couleur orange).</param>
        private void LogMessage(string message, bool isWarning)
        {
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;

            if (isWarning)
            {
                txtLog.SelectionColor = Color.OrangeRed;
            }
            else
            {
                txtLog.SelectionColor = txtLog.ForeColor;
            }

            txtLog.AppendText(message + Environment.NewLine);
            txtLog.SelectionColor = txtLog.ForeColor; // Reset color
            txtLog.ScrollToCaret();
        }

        /// <summary>
        /// Récupère une valeur d'un dictionnaire de ligne CSV de manière sécurisée.
        /// </summary>
        private static string GetValueOrDefault(Dictionary<string, string> dict, string key, string defaultValue = "")
        {
            return dict.TryGetValue(key, out string value) ? value : defaultValue;
        }
    }

    /// <summary>
    /// Classe interne pour la définition des champs de tâche pour le mappage UI.
    /// </summary>
    internal class TacheMappingFields
    {
        public string DisplayName { get; }
        public string PropertyName { get; } // Nom de la propriété dans Tache.cs
        public bool IsMandatory { get; }

        public TacheMappingFields(string displayName, string propertyName, bool isMandatory)
        {
            DisplayName = displayName;
            PropertyName = propertyName;
            IsMandatory = isMandatory;
        }
    }
}