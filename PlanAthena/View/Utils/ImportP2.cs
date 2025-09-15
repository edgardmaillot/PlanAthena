// Version 0.5.1
using Krypton.Toolkit;
using PlanAthena.Services.DTOs.ImportExport;

namespace PlanAthena.View.Utils
{
    public partial class ImportP2 : UserControl
    {
        // --- Événements pour communiquer avec l'Orchestrateur ---
        public event EventHandler SuivantClicked;
        public event EventHandler RetourClicked;

        // --- Propriété pour exposer le résultat ---
        public ImportP2Result Result { get; private set; }

        // --- État interne du contrôle ---
        private ImportP2Config _config;
        private bool _isUpdatingUI = false;

        // Dictionnaire pour lier une SourceValue (string) à son jeu de contrôles
        private readonly Dictionary<string, ValueMappingControlSet> _mappingControls = new Dictionary<string, ValueMappingControlSet>();

        // Classe interne pour regrouper les contrôles d'une ligne
        private class ValueMappingControlSet
        {
            public KryptonPanel StatusIndicator { get; set; }
            public KryptonLabel SourceLabel { get; set; }
            public KryptonComboBox TargetComboBox { get; set; }
        }

        // Classe interne pour peupler les ComboBox
        private class ComboBoxItem
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public override string ToString() => DisplayName;
        }

        public ImportP2()
        {
            InitializeComponent();
            this.Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            kBtSuivant.Click += KBtSuivant_Click;
            kBtRetour.Click += KBtRetour_Click;
        }

        public void Initialize(ImportP2Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // 1. Mise à jour de l'UI "statique"
            khTitre.Values.Heading = $"Importer des {_config.EntityDisplayName}";
            khTitre.Values.Description = $"Étape 2/3 - Faire correspondre les {_config.ValueCategoryName}";
            if (_config.EntityImage != null)
            {
                kryptonPanel1.StateNormal.Image = _config.EntityImage;
            }

            // 2. Générer les contrôles de mapping dynamiques
            GenerateMappingControls();

            // 3. Remplir les ComboBox et appliquer les suggestions
            PopulateComboBoxesAndApplySuggestions();

            // 4. Mettre à jour l'état initial de l'UI
            UpdateUIState();
        }

        private void GenerateMappingControls()
        {
            kTableData.SuspendLayout();
            kTableData.Controls.Clear();
            kTableData.RowStyles.Clear();
            _mappingControls.Clear();

            if (_config.SourceValues == null || !_config.SourceValues.Any())
            {
                kTableData.ResumeLayout();
                return;
            }

            kTableData.RowCount = _config.SourceValues.Count + 1; // +1 pour la ligne "ressort"

            for (int i = 0; i < _config.SourceValues.Count; i++)
            {
                var sourceValue = _config.SourceValues[i];
                kTableData.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

                var statusIndicator = new KryptonPanel { Size = new Size(18, 18), Margin = new Padding(3) };
                var sourceLabel = new KryptonLabel { Text = sourceValue, Dock = DockStyle.Fill, AutoSize = true, StateCommon = { ShortText = { Color1 = Color.Black, TextH = PaletteRelativeAlign.Far, TextV = PaletteRelativeAlign.Center } } };
                var targetComboBox = new KryptonComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Tag = sourceValue };

                var leftCellPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };
                leftCellPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                leftCellPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25F));
                leftCellPanel.Controls.Add(sourceLabel, 0, 0);
                leftCellPanel.Controls.Add(statusIndicator, 1, 0);

                kTableData.Controls.Add(leftCellPanel, 0, i);
                kTableData.Controls.Add(targetComboBox, 1, i);

                targetComboBox.SelectedIndexChanged += OnMappingChanged;

                _mappingControls[sourceValue] = new ValueMappingControlSet
                {
                    StatusIndicator = statusIndicator,
                    SourceLabel = sourceLabel,
                    TargetComboBox = targetComboBox
                };
            }

            kTableData.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            kTableData.ResumeLayout();
        }

        private void PopulateComboBoxesAndApplySuggestions()
        {
            var items = new List<ComboBoxItem> { new ComboBoxItem { Id = null, DisplayName = "< Ignorer >" } };
            if (_config.TargetValues != null)
            {
                items.AddRange(_config.TargetValues
                    .OrderBy(tv => tv.DisplayName)
                    .Select(tv => new ComboBoxItem { Id = tv.Id, DisplayName = tv.DisplayName }));
            }

            foreach (var controlSet in _mappingControls.Values)
            {
                controlSet.TargetComboBox.DataSource = new List<ComboBoxItem>(items);
                controlSet.TargetComboBox.DisplayMember = "DisplayName";

                var sourceValue = controlSet.TargetComboBox.Tag.ToString();
                if (_config.SuggestedMappings != null && _config.SuggestedMappings.TryGetValue(sourceValue, out string targetId))
                {
                    var itemToSelect = items.FirstOrDefault(item => item.Id == targetId);
                    if (itemToSelect != null)
                    {
                        controlSet.TargetComboBox.SelectedItem = itemToSelect;
                    }
                }
            }
        }

        private void OnMappingChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUI) return;
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            _isUpdatingUI = true;

            var mappedSourceValues = new HashSet<string>();
            var usedTargetIds = new HashSet<string>();

            foreach (var controlSet in _mappingControls.Values)
            {
                var selectedItem = (ComboBoxItem)controlSet.TargetComboBox.SelectedItem;
                bool isMapped = selectedItem != null && selectedItem.Id != null;

                controlSet.StatusIndicator.StateNormal.Color1 = isMapped ? Color.FromArgb(0, 192, 0) : Color.Red;

                if (isMapped)
                {
                    mappedSourceValues.Add(controlSet.TargetComboBox.Tag.ToString());
                    usedTargetIds.Add(selectedItem.Id);
                }
            }

            var unmappedSources = _config.SourceValues.Except(mappedSourceValues).OrderBy(s => s);
            kRichTxt_csv.Text = string.Join(Environment.NewLine, unmappedSources);

            var unusedTargets = _config.TargetValues.Where(tv => !usedTargetIds.Contains(tv.Id))
                                       .Select(tv => tv.DisplayName)
                                       .OrderBy(s => s);
            kRichTxt_PA.Text = string.Join(Environment.NewLine, unusedTargets);

            kBtSuivant.Enabled = true;
            _isUpdatingUI = false;
        }

        private void KBtSuivant_Click(object sender, EventArgs e)
        {
            var result = new ImportP2Result
            {
                ShouldMemorizeMappings = mem1.Checked,
                AllMappingDecisions = new List<ValueMappingResult>()
            };

            foreach (var kvp in _mappingControls)
            {
                var sourceValue = kvp.Key;
                var selectedItem = (ComboBoxItem)kvp.Value.TargetComboBox.SelectedItem;

                result.AllMappingDecisions.Add(new ValueMappingResult
                {
                    SourceValue = sourceValue,
                    Action = (selectedItem?.Id != null) ? MappingAction.MapToExisting : MappingAction.Ignore,
                    MappedTargetId = selectedItem?.Id
                });
            }

            this.Result = result;
            SuivantClicked?.Invoke(this, EventArgs.Empty);
        }

        private void KBtRetour_Click(object sender, EventArgs e)
        {
            RetourClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}