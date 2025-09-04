using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using System.Data;
using System.Text;

namespace PlanAthena.View.Utils
{
    public partial class ImportMapOuvrierP2 : UserControl
    {
        // --- Événements pour l'Orchestrateur ---
        public event EventHandler TerminerClicked;
        public event EventHandler RetourClicked;

        // --- Propriété de résultat ---
        public Dictionary<string, MetierMappingItem> ValueMappings { get; private set; } = new Dictionary<string, MetierMappingItem>();

        // --- Services et état ---
        private readonly RessourceService _ressourceService;
        private readonly string _filePath;
        private readonly int _metierColumnIndex;
        private readonly bool _hasHeader;
        private List<string> _metiersCsvUniques = new List<string>();
        private List<MetierMappingItem> _metiersPlanAthenaMapping = new List<MetierMappingItem>();

        public ImportMapOuvrierP2(string filePath, int metierColumnName, RessourceService ressourceService, bool hasHeader)
        {
            InitializeComponent();
            _filePath = filePath;
            _metierColumnIndex = metierColumnName;
            _ressourceService = ressourceService;
            _hasHeader = hasHeader;

            this.Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            btnTerminer.Enabled = false;

            // 1. Préparer les données
            LoadAndPrepareData();

            // 2. Générer l'interface de mapping dynamique
            PopulateMappingGrid();

            // 3. Mettre à jour les listes de feedback visuel
            UpdateProgressLists();

            // 4. Valider l'état (pour le bouton Terminer)
            ValidateState();
        }

        /// <summary>
        /// Charge les métiers depuis le service et extrait les valeurs uniques du fichier CSV.
        /// </summary>
        private void LoadAndPrepareData()
        {
            var metiersExistants = _ressourceService.GetAllMetiers();
            _metiersPlanAthenaMapping.Add(MetierMappingItem.Ignorer);
            _metiersPlanAthenaMapping.AddRange(metiersExistants.Select(m => new MetierMappingItem(m)));

            try
            {
                // Lecture directe du fichier sans ChoETL
                string[] lines = File.ReadAllLines(_filePath, Encoding.UTF8);
                if (lines.Length <= (_hasHeader ? 1 : 0)) return;

                string delimiter = DetectCsvDelimiter(_filePath);
                char delimiterChar = delimiter[0];

                var metiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int startLine = _hasHeader ? 1 : 0;
                for (int i = startLine; i < lines.Length; i++)
                {
                    string[] columns = lines[i].Split(delimiterChar);

                    // --- LA LOGIQUE EST MAINTENANT BASÉE SUR L'INDEX ---
                    if (_metierColumnIndex < columns.Length)
                    {
                        string metierValue = columns[_metierColumnIndex]?.Trim();
                        if (!string.IsNullOrWhiteSpace(metierValue))
                        {
                            metiers.Add(metierValue);
                        }
                    }
                }
                _metiersCsvUniques = metiers.OrderBy(m => m).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la lecture des métiers du fichier CSV.\n\nDétails : {ex.Message}", "Erreur de lecture", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Méthode utilitaire pour détecter le délimiteur CSV le plus probable.
        /// </summary>
        private string DetectCsvDelimiter(string filePath)
        {
            var delimiters = new[] { ';', ',', '\t' };
            string firstLine = File.ReadLines(filePath).FirstOrDefault();
            if (string.IsNullOrEmpty(firstLine)) return ";"; // Par défaut

            return delimiters.OrderByDescending(d => firstLine.Count(c => c == d))
                             .First()
                             .ToString();
        }

        /// <summary>
        /// Construit dynamiquement la grille de mapping dans le TableLayoutPanel.
        /// </summary>
        private void PopulateMappingGrid()
        {
            kryptonTableLayoutPanel1.Controls.Clear();
            kryptonTableLayoutPanel1.RowStyles.Clear();
            kryptonTableLayoutPanel1.RowCount = _metiersCsvUniques.Count;

            for (int i = 0; i < _metiersCsvUniques.Count; i++)
            {
                var metierCsv = _metiersCsvUniques[i];
                kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var label = new KryptonLabel
                {
                    Text = metierCsv,
                    Dock = DockStyle.Fill
                };
                kryptonTableLayoutPanel1.Controls.Add(label, 0, i);

                var comboBox = new KryptonComboBox
                {
                    DisplayMember = "DisplayName", // Toujours utile si vous inspectez les items
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Tag = metierCsv
                };

                // --- CORRECTION : Peupler Items manuellement au lieu de DataSource ---
                comboBox.Items.Clear();
                foreach (var mappingItem in _metiersPlanAthenaMapping)
                {
                    comboBox.Items.Add(mappingItem);
                }
                // -----------------------------------------------------------------

                comboBox.SelectedIndexChanged += OnMappingSelectionChanged;

                // Tentative de mapping automatique
                var autoMatch = _metiersPlanAthenaMapping.FirstOrDefault(m => m.Metier?.Nom.Equals(metierCsv, StringComparison.OrdinalIgnoreCase) ?? false);
                if (autoMatch != null)
                {
                    comboBox.SelectedItem = autoMatch;
                }
                else
                {
                    // Maintenant que les Items sont peuplés, ceci fonctionnera.
                    if (comboBox.Items.Count > 0)
                    {
                        comboBox.SelectedIndex = 0; // Sélectionner "< Ignorer >" par défaut
                    }
                }

                kryptonTableLayoutPanel1.Controls.Add(comboBox, 1, i);
            }
        }

        /// <summary>
        /// Appelé à chaque changement dans une ComboBox pour valider l'état et mettre à jour l'UI.
        /// </summary>
        private void OnMappingSelectionChanged(object sender, EventArgs e)
        {
            ValidateState();
            UpdateProgressLists();
        }
        private void UpdateProgressLists()
        {
            // --- Logique pour la liste de gauche : Métiers PlanAthena restants ---
            var metiersPlanAthenaUtilises = kryptonTableLayoutPanel1.Controls.OfType<KryptonComboBox>()
                .Select(cmb => (MetierMappingItem)cmb.SelectedItem)
                .Where(item => item.Action == MappingAction.MapToExisting)
                .Select(item => item.Metier.MetierId)
                .ToHashSet();

            var metiersPlanAthenaRestants = _ressourceService.GetAllMetiers()
                .Where(m => !metiersPlanAthenaUtilises.Contains(m.MetierId))
                .Select(m => $"{m.Nom} ({m.MetierId})")
                .OrderBy(s => s);

            ListMetierPlanAthena.Items.Clear();
            ListMetierPlanAthena.Items.AddRange(metiersPlanAthenaRestants.ToArray());

            // --- Logique pour la liste de droite : Métiers CSV restants à mapper ---
            var metiersCsvMappes = kryptonTableLayoutPanel1.Controls.OfType<KryptonComboBox>()
                .Where(cmb => ((MetierMappingItem)cmb.SelectedItem).Action != MappingAction.Ignorer)
                .Select(cmb => cmb.Tag.ToString());

            var metiersCsvRestants = _metiersCsvUniques
                .Except(metiersCsvMappes, StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s);

            ListMetierImport.Items.Clear();
            ListMetierImport.Items.AddRange(metiersCsvRestants.ToArray());
        }
        /// <summary>
        /// Met à jour les deux ListBox de droite pour montrer les mappings effectués.
        /// </summary>
        private void UpdateFeedbackLists()
        {
            ListMetierPlanAthena.Items.Clear();
            ListMetierImport.Items.Clear();

            var allComboBoxes = kryptonTableLayoutPanel1.Controls.OfType<KryptonComboBox>().ToList();

            var mappedItems = allComboBoxes
                .Select(cmb => new { CsvMetier = cmb.Tag.ToString(), MappedItem = (MetierMappingItem)cmb.SelectedItem })
                .Where(item => item.MappedItem.Action != MappingAction.Ignorer)
                .OrderBy(item => item.MappedItem.DisplayName);

            foreach (var item in mappedItems)
            {
                ListMetierPlanAthena.Items.Add(item.MappedItem.DisplayName);
                ListMetierImport.Items.Add(item.CsvMetier);
            }
        }

        /// <summary>
        /// Valide si l'état actuel permet de terminer. Pour l'instant, c'est toujours vrai.
        /// On pourrait ajouter une logique pour forcer un choix sur chaque ligne.
        /// </summary>
        private void ValidateState()
        {
            // Pour ce POC, on considère que l'utilisateur peut terminer à tout moment.
            // Les métiers non mappés seront simplement ignorés.
            btnTerminer.Enabled = true;
        }

        private void btnTerminer_Click(object sender, EventArgs e)
        {
            ValueMappings.Clear();
            var allComboBoxes = kryptonTableLayoutPanel1.Controls.OfType<KryptonComboBox>();

            foreach (var cmb in allComboBoxes)
            {
                string metierCsv = cmb.Tag.ToString();
                MetierMappingItem selectedMapping = (MetierMappingItem)cmb.SelectedItem;
                ValueMappings[metierCsv] = selectedMapping;
            }

            TerminerClicked?.Invoke(this, EventArgs.Empty);
        }

        private void btnRetour_Click(object sender, EventArgs e)
        {
            RetourClicked?.Invoke(this, EventArgs.Empty);
        }

        // --- Classe interne/DTO pour peupler les ComboBox ---

        /// <summary>
        /// Représente un item dans la ComboBox de mapping de métier.
        /// Peut représenter un métier existant ou une action spéciale (Ignorer).
        /// </summary>
        public class MetierMappingItem
        {
            public Metier Metier { get; }
            public MappingAction Action { get; }
            public string DisplayName { get; }

            // Constructeur pour un métier existant
            public MetierMappingItem(Metier metier)
            {
                Metier = metier;
                Action = MappingAction.MapToExisting;
                DisplayName = $"{metier.Nom} ({metier.MetierId})";
            }

            // Constructeur pour une action spéciale
            private MetierMappingItem(MappingAction action, string displayName)
            {
                Action = action;
                DisplayName = displayName;
            }

            public static MetierMappingItem Ignorer => new MetierMappingItem(MappingAction.Ignorer, "< Ignorer >");

            public override string ToString() => DisplayName;
        }

        public enum MappingAction
        {
            MapToExisting,
            Ignorer
        }
    }
}