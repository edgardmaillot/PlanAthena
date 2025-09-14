// Fichier destiné à disparaitre
// sera remplacé par le nouveau service d'import

using Krypton.Toolkit;
using System.Data;
using System.Text;

namespace PlanAthena.View.Utils
{
    public partial class ImportMapOuvrierP1 : UserControl
    {
        // --- Événements pour communiquer avec l'Orchestrateur ---
        public event EventHandler SuivantClicked;
        public event EventHandler AnnulerClicked;

        // --- Propriété pour exposer le résultat ---
        public Dictionary<string, int> Mappings { get; private set; } = new Dictionary<string, int>();

        // --- État interne du contrôle ---
        private readonly string _filePath;
        private List<string> _csvHeaders = new List<string>();
        private bool _isUpdatingComboBoxes = false;

        // --- Définition des champs à mapper (notre "source de vérité") ---
        private readonly List<OuvrierMappingField> _fieldsToMap;
        private readonly Dictionary<string, KryptonComboBox> _comboBoxes = new Dictionary<string, KryptonComboBox>();
        public bool HasHeader => kryptonCheckBox1.Checked;
        /// <summary>
        /// Constructeur principal du contrôle.
        /// </summary>
        /// <param name="filePath">Chemin du fichier CSV à importer.</param>
        public ImportMapOuvrierP1(string filePath)
        {
            InitializeComponent();
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            // Initialiser la structure de mapping
            _fieldsToMap = new List<OuvrierMappingField>
            {
                new OuvrierMappingField("Nom", "Nom", true),
                new OuvrierMappingField("Prénom", "Prenom", true),
                new OuvrierMappingField("Coût journalier", "TauxJour", false),
                new OuvrierMappingField("Métier", "Metier", true)
            };

            // Lier les champs logiques aux contrôles ComboBox du designer
            _comboBoxes["Nom"] = CB_Nom;
            _comboBoxes["Prenom"] = CB_Prenon;
            _comboBoxes["TauxJour"] = CB_TauxJour;
            _comboBoxes["Metier"] = CB_Metier;

            // Attacher les événements et charger les données
            this.Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            // Configuration initiale de l'UI
            kryptonCheckBox1.Text = "La première ligne contient des en-têtes";
            btnSuivant.Enabled = false;

            // Abonner tous les ComboBox au même gestionnaire d'événements
            foreach (var cmb in _comboBoxes.Values)
            {
                cmb.SelectedIndexChanged += ComboBox_MappingChanged;
            }
            kryptonCheckBox1.CheckedChanged += chkHasHeader_CheckedChanged;

            // Charger le fichier CSV
            LoadCsvFile();
        }

        /// <summary>
        /// Charge le fichier CSV, détecte les en-têtes et remplit l'aperçu.
        /// </summary>
        private void LoadCsvFile()
        {
            try
            {
                var lines = File.ReadAllLines(_filePath, Encoding.Default);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Le fichier CSV est vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Détection du séparateur
                char separator = lines[0].Contains(';') ? ';' : '\t';

                PopulateUIFromCsv(lines, separator);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du fichier CSV : {ex.Message}", "Erreur de lecture", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSuivant.Enabled = false;
            }
        }

        /// <summary>
        /// Remplit la grille d'aperçu et les ComboBox à partir des lignes du fichier CSV.
        /// </summary>
        private void PopulateUIFromCsv(string[] lines, char separator)
        {
            kryptonDataGridView1.Columns.Clear();
            _csvHeaders.Clear();

            int dataStartIndex = kryptonCheckBox1.Checked ? 1 : 0;

            // Déterminer les en-têtes
            if (kryptonCheckBox1.Checked && lines.Length > 0)
            {
                _csvHeaders.AddRange(lines[0].Split(separator).Select(h => h.Trim()));
            }
            else
            {
                int columnCount = lines.Length > 0 ? lines[0].Split(separator).Length : 0;
                for (int i = 0; i < columnCount; i++) _csvHeaders.Add($"Colonne {i + 1}");
            }

            // Remplir le DataGridView
            foreach (var header in _csvHeaders)
            {
                kryptonDataGridView1.Columns.Add(header, header);
            }

            for (int i = dataStartIndex; i < lines.Length && i < dataStartIndex + 10; i++) // 10 lignes d'aperçu
            {
                var values = lines[i].Split(separator);
                kryptonDataGridView1.Rows.Add(values);
            }

            // Mettre à jour les ComboBox
            UpdateMappingComboBoxes();
        }

        /// <summary>
        /// Met à jour les ComboBox avec les en-têtes CSV et tente un mapping automatique.
        /// </summary>
        private void UpdateMappingComboBoxes()
        {
            _isUpdatingComboBoxes = true;

            foreach (var cmb in _comboBoxes.Values)
            {
                cmb.Items.Clear();
                cmb.Items.Add("(non mappé)");
                cmb.Items.AddRange(_csvHeaders.ToArray());
                cmb.SelectedIndex = 0;
            }

            // Tentative de mapping automatique
            foreach (var field in _fieldsToMap)
            {
                var bestMatch = _csvHeaders.FirstOrDefault(h => h.Equals(field.DisplayName, StringComparison.OrdinalIgnoreCase));
                if (bestMatch != null)
                {
                    _comboBoxes[field.PropertyName].SelectedItem = bestMatch;
                }
            }

            _isUpdatingComboBoxes = false;

            // Rafraîchir pour gérer les doublons potentiels du mapping auto et valider
            RefreshComboBoxItems();
            ValidateMappings();
        }

        /// <summary>
        /// Gère la sélection dans n'importe quel ComboBox pour éviter les doublons.
        /// </summary>
        private void ComboBox_MappingChanged(object sender, EventArgs e)
        {
            if (_isUpdatingComboBoxes) return;
            RefreshComboBoxItems();
            ValidateMappings();
        }

        /// <summary>
        /// Rafraîchit les listes de tous les ComboBox pour exclure les en-têtes déjà mappés.
        /// </summary>
        private void RefreshComboBoxItems()
        {
            _isUpdatingComboBoxes = true;

            var selectedHeaders = _comboBoxes.Values
                .Where(cmb => cmb.SelectedIndex > 0)
                .Select(cmb => cmb.SelectedItem.ToString())
                .ToHashSet();

            foreach (var kvp in _comboBoxes)
            {
                var currentComboBox = kvp.Value;
                var currentSelection = currentComboBox.SelectedItem?.ToString();

                currentComboBox.Items.Clear();
                currentComboBox.Items.Add("(non mappé)");

                foreach (var header in _csvHeaders)
                {
                    // Ajouter l'en-tête s'il n'est pas utilisé ailleurs, OU si c'est la sélection actuelle de CE ComboBox
                    if (!selectedHeaders.Contains(header) || header == currentSelection)
                    {
                        currentComboBox.Items.Add(header);
                    }
                }
                currentComboBox.SelectedItem = currentSelection ?? "(non mappé)";
            }

            _isUpdatingComboBoxes = false;
        }

        /// <summary>
        /// Vérifie si tous les champs obligatoires sont mappés et active le bouton "Suivant".
        /// </summary>
        private void ValidateMappings()
        {
            bool allMandatoryMapped = true;
            foreach (var field in _fieldsToMap)
            {
                if (field.IsMandatory)
                {
                    if (_comboBoxes.TryGetValue(field.PropertyName, out var cmb) && cmb.SelectedIndex <= 0)
                    {
                        allMandatoryMapped = false;
                        break;
                    }
                }
            }
            btnSuivant.Enabled = allMandatoryMapped;
        }

        private void chkHasHeader_CheckedChanged(object sender, EventArgs e)
        {
            // Recharger tout l'aperçu si l'utilisateur change d'avis sur les en-têtes
            LoadCsvFile();
        }

        private void btnSuivant_Click(object sender, EventArgs e)
        {
            // Construire le dictionnaire de mapping final
            Mappings.Clear();
            foreach (var kvp in _comboBoxes)
            {
                string propertyName = kvp.Key;
                var comboBox = kvp.Value;

                if (comboBox.SelectedIndex > 0) // > 0 pour ignorer "(non mappé)"
                {
                    string selectedHeader = comboBox.SelectedItem.ToString();
                    // --- LA MODIFICATION CLÉ ---
                    // On trouve l'index de l'en-tête sélectionné et on le stocke.
                    int columnIndex = _csvHeaders.IndexOf(selectedHeader);
                    if (columnIndex != -1)
                    {
                        Mappings[propertyName] = columnIndex;
                    }
                }
            }
            SuivantClicked?.Invoke(this, EventArgs.Empty);
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            // Notifier l'orchestrateur de l'annulation
            AnnulerClicked?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Classe interne pour définir les champs de l'ouvrier à mapper.
        /// </summary>
        private class OuvrierMappingField
        {
            public string DisplayName { get; }
            public string PropertyName { get; }
            public bool IsMandatory { get; }

            public OuvrierMappingField(string displayName, string propertyName, bool isMandatory)
            {
                DisplayName = displayName;
                PropertyName = propertyName;
                IsMandatory = isMandatory;
            }
        }
    }
}