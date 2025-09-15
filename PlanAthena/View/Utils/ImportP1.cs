// Version 0.5.1

using Krypton.Toolkit;
using PlanAthena.Services.DTOs.ImportExport;

namespace PlanAthena.View.Utils
{
    public partial class ImportP1 : UserControl
    {
        // --- Événements pour communiquer avec l'Orchestrateur ---
        public event EventHandler SuivantClicked;
        public event EventHandler AnnulerClicked;

        // --- Propriété pour exposer le résultat ---
        public ImportP1Result Result { get; private set; }

        // --- État interne du contrôle ---
        private ImportP1Config _config;
        private List<string> _currentCsvHeaders = new List<string>();
        private bool _isUpdatingUI = false;

        // --- Dictionnaire pour accéder aux contrôles de mapping générés ---
        private readonly Dictionary<string, MappingControlSet> _mappingControls = new Dictionary<string, MappingControlSet>();

        /// <summary>
        /// Classe interne pour regrouper les contrôles d'une ligne de mapping.
        /// </summary>
        private class MappingControlSet
        {
            public KryptonLabel Label { get; set; }
            public KryptonComboBox ComboBox { get; set; }
            public KryptonTextBox DefaultValueTextBox { get; set; }
            public KryptonPanel StatusIndicator { get; set; }
            public MappingFieldDefinition FieldDefinition { get; set; }
        }

        public ImportP1()
        {
            InitializeComponent();
            this.Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            kBtSuivant.Click += KBtSuivant_Click;
            kBtAnnule.Click += KBtAnnule_Click;
            kcbEntete.CheckedChanged += KcbEntete_CheckedChanged;
        }

        /// <summary>
        /// Point d'entrée principal pour configurer et afficher le contrôle.
        /// Appelé par l'Orchestrateur.
        /// </summary>
        public void Initialize(ImportP1Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // 1. Mise à jour de l'UI "statique"
            khTitre.Values.Heading = $"Importer des {_config.EntityDisplayName}";
            if (_config.EntityImage != null)
            {
                kryptonPanel1.StateNormal.Image = _config.EntityImage;
            }

            // 2. Configuration initiale de la checkbox d'en-tête
            kcbEntete.Checked = _config.InitialHasHeaderState;

            // 3. Charger les données (aperçu, comboboxes, etc.)
            ReloadUIFromData();
        }

        /// <summary>
        /// Recharge toute l'interface utilisateur en fonction des données et de l'état de la checkbox "kcbEntete".
        /// C'est la méthode centrale pour la mise à jour de l'UI.
        /// </summary>
        private void ReloadUIFromData()
        {
            _isUpdatingUI = true;

            // --- Étape A: Déterminer les en-têtes et l'état de la mémorisation ---
            if (kcbEntete.Checked)
            {
                _currentCsvHeaders = new List<string>(_config.CsvHeaders);
                mem1.Enabled = true;
            }
            else
            {
                _currentCsvHeaders.Clear();
                for (int i = 0; i < _config.CsvHeaders.Count; i++)
                {
                    _currentCsvHeaders.Add($"Colonne {i + 1}");
                }
                mem1.Checked = false;
                mem1.Enabled = false;
            }

            // --- Étape B: Remplir l'aperçu DataGridView ---
            PopulateDataGridView();

            // --- Étape C: Générer les contrôles de mapping (uniquement la première fois) ---
            GenerateMappingControls();

            // --- Étape D: Remplir les ComboBox et appliquer les suggestions ---
            PopulateComboBoxesAndApplySuggestions();

            _isUpdatingUI = false;

            // --- Étape E: Validation initiale ---
            ValidateAllMappings();
        }

        private void PopulateDataGridView()
        {
            kDataGrid_P1.Rows.Clear();
            kDataGrid_P1.Columns.Clear();

            foreach (var header in _currentCsvHeaders)
            {
                kDataGrid_P1.Columns.Add(header, header);
            }

            // Si on n'a pas d'en-tête, la première ligne de "preview" est en fait la ligne des headers originaux.
            var dataRows = kcbEntete.Checked
                ? _config.DataPreview
                : new[] { _config.CsvHeaders.ToArray() }.Concat(_config.DataPreview);

            foreach (var row in dataRows)
            {
                if (row.Length > 0)
                {
                    kDataGrid_P1.Rows.Add(row);
                }
            }
        }

        private void GenerateMappingControls()
        {
            if (_mappingControls.Any() || _config == null) return;

            kryptonTableLayoutPanel4.SuspendLayout();
            kryptonTableLayoutPanel4.Controls.Clear();

            // --- CORRECTION FINALE : Respecter le layout original ---
            // 3 Lignes fixes, et une COLONNE par champ à mapper
            kryptonTableLayoutPanel4.RowCount = 3;
            kryptonTableLayoutPanel4.RowStyles.Clear();
            kryptonTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F)); // Ligne des labels
            kryptonTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F)); // Ligne des ComboBoxes
            kryptonTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F)); // Ligne des TextBoxes

            kryptonTableLayoutPanel4.ColumnCount = _config.FieldsToMap.Count;
            kryptonTableLayoutPanel4.ColumnStyles.Clear();
            // -------------------------------------------------------------

            for (int i = 0; i < _config.FieldsToMap.Count; i++)
            {
                var field = _config.FieldsToMap[i];
                // Ajouter une colonne pour ce champ
                kryptonTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / _config.FieldsToMap.Count));

                // --- Ligne 0 : Indicateur + Label ---
                //var panelContainer = new KryptonPanel { Dock = DockStyle.Fill };
                var statusIndicator = new KryptonPanel { Size = new Size(18, 18), Location = new Point(0, 5), StateNormal = { Color1 = Color.Red }, Anchor = AnchorStyles.Right };
                var label = new KryptonLabel { Text = field.DisplayName + (field.IsMandatory ? " *" : ""), Dock = DockStyle.Fill, AutoSize = true, StateCommon = { ShortText = { Color1 = Color.Black, TextH = PaletteRelativeAlign.Center, TextV = PaletteRelativeAlign.Center } } };

                var tlpHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(3) };
                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25F));
                tlpHeader.Controls.Add(label, 0, 0);
                tlpHeader.Controls.Add(statusIndicator, 1, 0);

                kryptonTableLayoutPanel4.Controls.Add(tlpHeader, i, 0);

                // --- Ligne 1 : ComboBox ---
                var comboBox = new KryptonComboBox { Dock = DockStyle.Fill, DropDownWidth = 200, Margin = new Padding(3) };
                kryptonTableLayoutPanel4.Controls.Add(comboBox, i, 1);

                // --- Ligne 2 : TextBox pour valeur par défaut ---
                var textBox = new KryptonTextBox { Dock = DockStyle.Fill, Enabled = field.AllowDefaultValue, Margin = new Padding(3) };
                if (!field.AllowDefaultValue)
                {
                    textBox.Visible = false; // Cacher la textbox si non autorisée
                }
                else
                {
                    textBox.Text = "Valeur par défaut"; // Placeholder simple
                }
                kryptonTableLayoutPanel4.Controls.Add(textBox, i, 2);

                // --- Attachement des événements et stockage ---
                comboBox.SelectedIndexChanged += OnMappingChanged;
                textBox.TextChanged += OnMappingChanged;

                _mappingControls[field.InternalName] = new MappingControlSet
                {
                    Label = label,
                    ComboBox = comboBox,
                    DefaultValueTextBox = textBox,
                    StatusIndicator = statusIndicator,
                    FieldDefinition = field
                };
            }
            kryptonTableLayoutPanel4.ResumeLayout();
        }

        private void PopulateComboBoxesAndApplySuggestions()
        {
            foreach (var controlSet in _mappingControls.Values)
            {
                controlSet.ComboBox.Items.Clear();
                controlSet.ComboBox.Items.Add("(non mappé)");
                controlSet.ComboBox.Items.AddRange(_currentCsvHeaders.ToArray());
                controlSet.ComboBox.SelectedIndex = 0;
            }

            if (kcbEntete.Checked && _config.SuggestedMappings != null)
            {
                foreach (var suggestion in _config.SuggestedMappings)
                {
                    if (_mappingControls.TryGetValue(suggestion.Key, out var controlSet) &&
                        _currentCsvHeaders.Contains(suggestion.Value))
                    {
                        controlSet.ComboBox.SelectedItem = suggestion.Value;
                    }
                }
            }
            RefreshAvailableComboBoxItems();
        }

        private void RefreshAvailableComboBoxItems()
        {
            _isUpdatingUI = true;

            var selectedHeaders = _mappingControls.Values
                .Where(cs => cs.ComboBox.SelectedIndex > 0)
                .Select(cs => cs.ComboBox.SelectedItem.ToString())
                .ToHashSet();

            foreach (var controlSet in _mappingControls.Values)
            {
                var currentSelection = controlSet.ComboBox.SelectedItem?.ToString();
                controlSet.ComboBox.Items.Clear();
                controlSet.ComboBox.Items.Add("(non mappé)");

                foreach (var header in _currentCsvHeaders)
                {
                    if (!selectedHeaders.Contains(header) || header == currentSelection)
                    {
                        controlSet.ComboBox.Items.Add(header);
                    }
                }
                controlSet.ComboBox.SelectedItem = currentSelection ?? "(non mappé)";
            }
            _isUpdatingUI = false;
        }

        private void OnMappingChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUI) return;
            RefreshAvailableComboBoxItems();
            ValidateAllMappings();
        }

        private void ValidateAllMappings()
        {
            bool allMandatoryFieldsAreValid = true;
            foreach (var cs in _mappingControls.Values)
            {
                bool isMapped = cs.ComboBox.SelectedIndex > 0;
                bool hasDefaultValue = !string.IsNullOrWhiteSpace(cs.DefaultValueTextBox.Text) && cs.DefaultValueTextBox.Text != "Valeur par défaut";
                bool isValid = isMapped || hasDefaultValue;

                cs.StatusIndicator.StateNormal.Color1 = isValid ? Color.FromArgb(0, 192, 0) : Color.Red;
                if (cs.FieldDefinition.IsMandatory && !isValid)
                {
                    allMandatoryFieldsAreValid = false;
                }
            }
            kBtSuivant.Enabled = allMandatoryFieldsAreValid;
        }

        private void KcbEntete_CheckedChanged(object sender, EventArgs e)
        {
            ReloadUIFromData();
        }

        private void KBtSuivant_Click(object sender, EventArgs e)
        {
            var result = new ImportP1Result
            {
                HasHeader = kcbEntete.Checked,
                ShouldMemorizeMappings = mem1.Checked,
            };

            foreach (var kvp in _mappingControls)
            {
                var mapping = new FieldMappingResult
                {
                    InternalName = kvp.Key,
                    MappedCsvHeader = kvp.Value.ComboBox.SelectedIndex > 0 ? kvp.Value.ComboBox.SelectedItem.ToString() : null,
                    DefaultValue = kvp.Value.DefaultValueTextBox.Text != "Valeur par défaut" ? kvp.Value.DefaultValueTextBox.Text : null
                };
                result.FieldMappings.Add(mapping);
            }
            this.Result = result;
            SuivantClicked?.Invoke(this, EventArgs.Empty);
        }

        private void KBtAnnule_Click(object sender, EventArgs e)
        {
            AnnulerClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}