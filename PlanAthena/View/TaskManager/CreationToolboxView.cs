using Krypton.Toolkit;
using PlanAthena.Data;

namespace PlanAthena.View.TaskManager
{
    public partial class CreationToolboxView : UserControl
    {
        // Événements pour notifier le parent
        public event EventHandler AddBlocRequested;
        public event EventHandler<Metier> AddTacheRequested;
        public event EventHandler AddJalonRequested;

        public CreationToolboxView()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Remplit la boîte à outils avec les boutons des métiers pertinents.
        /// </summary>
        /// <param name="metiers">La liste des métiers à afficher.</param>
        /// <param name="colorProvider">Le service pour obtenir la couleur de chaque métier.</param>
        /// <param name="metiersAvecCompetences">NOUVEAU : L'ensemble des IDs des métiers possédant au moins une compétence.</param>
        /// <summary>
        /// Remplit la boîte à outils avec les boutons des métiers pertinents en utilisant un TableLayoutPanel.
        /// </summary>
        public void PopulateMetiers(IEnumerable<Metier> metiers, Func<string, Color> colorProvider, HashSet<string> metiersAvecCompetences)
        {
            var tbl = panelDynamicButtons as TableLayoutPanel;
            if (tbl == null) return;

            tbl.SuspendLayout();

            tbl.Controls.Clear();
            tbl.RowStyles.Clear();
            tbl.RowCount = 0;

            var competencesActives = metiersAvecCompetences ?? new HashSet<string>();

            if (metiers != null)
            {
                foreach (var metier in metiers)
                {
                    // --- ÉTAPE 1: Créer une nouvelle ligne avec une hauteur fixe ---
                    int rowIndex = tbl.RowCount;
                    tbl.RowCount++;
                    tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

                    // --- ÉTAPE 2: Créer l'indicateur de couleur pour la colonne 0 ---
                    var colorIndicator = new KryptonPanel
                    {
                        Dock = DockStyle.Fill,
                        StateCommon = { Color1 = colorProvider(metier.MetierId) },
                        Margin = new Padding(3, 3, 0, 3) // Marge pour alignement
                    };

                    // --- ÉTAPE 3: (MODIFIÉ) Créer le PANEL de bordure pour la colonne 1 ---
                    var borderPanel = new KryptonPanel
                    {
                        Dock = DockStyle.Fill,
                        StateCommon = { Color1 = colorProvider(metier.MetierId) }, // La couleur de la bordure
                        Padding = new Padding(2), // L'épaisseur de la bordure
                        Margin = new Padding(0, 3, 3, 3) // Marge extérieure
                    };
                    var iconLabel = new KryptonLabel
                    {
                        Text = metier.Pictogram,
                        Dock = DockStyle.Left,
                        Width = 30,
                        StateNormal = { ShortText = { Color1 = Color.Black, Font = new Font("Segoe UI Symbol", 16F, FontStyle.Regular, GraphicsUnit.Point) } },

                    };
                    // --- ÉTAPE 4: Créer le bouton qui ira À L'INTÉRIEUR du panel de bordure ---
                    var metierButton = new KryptonButton
                    {
                        Text = metier.Nom,
                        Tag = metier,
                        Dock = DockStyle.Fill,
                        Enabled = competencesActives.Contains(metier.MetierId),
                        Margin = new Padding(0) // Aucune marge, il remplit son parent
                    };
                    metierButton.Click += MetierButton_Click;

                    // --- ÉTAPE 5: Assembler la hiérarchie ("poupées russes") ---
                    // 1. Mettre le bouton dans le panel de bordure
                    borderPanel.Controls.Add(metierButton);

                    // 2. Mettre les deux éléments principaux dans le tableau
                    tbl.Controls.Add(colorIndicator, 0, rowIndex);
                    colorIndicator.Controls.Add(iconLabel);
                    tbl.Controls.Add(borderPanel, 1, rowIndex);
                }
            }

            // Ajouter la ligne "ressort" pour occuper l'espace restant
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tbl.ResumeLayout();
        }

        private void MetierButton_Click(object sender, EventArgs e)
        {
            if (sender is KryptonButton { Tag: Metier metier })
            {
                // Lever l'événement en passant le métier concerné
                AddTacheRequested?.Invoke(this, metier);
            }
        }
        private void btnCreerJalon_Click(object sender, EventArgs e)
        {
            AddJalonRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnAddBloc_Click(object sender, EventArgs e)
        {
            // Lever l'événement simple
            AddBlocRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}