// Emplacement: /View/TaskManager/Cockpit/PlanningView.cs (Version complète)
using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;
using PlanAthena.View.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager.Cockpit
{
    public partial class PlanningView : UserControl
    {
        private PilotageProjetUseCase _useCase;
        private readonly ToolTip _toolTip = new ToolTip();

        // --- Propriétés pour la navigation ---
        private DateTime _currentStartDate;
        private DateTime _projectOverallStartDate;
        private bool _isNavigating = false;

        // --- NOUVEAUX CHAMPS POUR LA MISE EN CACHE DES CONTRÔLES ---
        private bool _isGridInitialized = false;
        private KryptonHeader[] _dayHeaders;
        private KryptonHeader[] _ouvrierHeaders;
        private KryptonPanel[,] _cellPanels; // [row, col] pour les conteneurs de tâches
        //private KryptonPanel[,] _weekendBackgroundPanels; // [row, col] pour le fond gris

        public PlanningView()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void Initialize(PilotageProjetUseCase useCase)
        {
            _useCase = useCase;
            InitializeNavigation();
            AttachEvents();
            RefreshData(); // Premier appel pour initialiser ou afficher "Aucun planning"
        }

        private void InitializeNavigation()
        {
            var (startDate, endDate) = _useCase.ObtenirPlageDeDatesDuPlanning();

            if (!startDate.HasValue || !endDate.HasValue)
            {
                kryptonTrackBar1.Enabled = false;
                klStart.Text = "-";
                klEnd.Text = "-";
                _currentStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                return;
            }

            kryptonTrackBar1.Enabled = true;

            _projectOverallStartDate = startDate.Value.AddDays(-(int)startDate.Value.DayOfWeek);
            var lastDate = endDate.Value;
            TimeSpan totalDuration = lastDate - _projectOverallStartDate;
            int totalWeeks = (int)Math.Ceiling(totalDuration.TotalDays / 7.0);

            _isNavigating = true; // Empêche le déclenchement de ValueChanged pendant la config
            kryptonTrackBar1.Minimum = 0;
            kryptonTrackBar1.Maximum = Math.Max(0, totalWeeks - 2);

            int currentWeekOffset = (int)Math.Floor((DateTime.Today - _projectOverallStartDate).TotalDays / 7.0);
            if (currentWeekOffset >= kryptonTrackBar1.Minimum && currentWeekOffset <= kryptonTrackBar1.Maximum)
            {
                kryptonTrackBar1.Value = currentWeekOffset;
            }
            else
            {
                kryptonTrackBar1.Value = kryptonTrackBar1.Minimum;
            }
            _isNavigating = false;

            UpdateViewForTrackBarValue(kryptonTrackBar1.Value);
        }

        private void AttachEvents()
        {
            resetView.Click += ResetView_Click;
            kryptonTrackBar1.ValueChanged += KryptonTrackBar1_ValueChanged;
        }

        private void KryptonTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (_isNavigating) return;

            _isNavigating = true;
            UpdateViewForTrackBarValue(kryptonTrackBar1.Value);
            RefreshData();
            _isNavigating = false;
        }

        private void UpdateViewForTrackBarValue(int weekOffset)
        {
            _currentStartDate = _projectOverallStartDate.AddDays(weekOffset * 7);
            klStart.Text = _currentStartDate.ToString("dd/MM/yyyy");
            klEnd.Text = _currentStartDate.AddDays(13).ToString("dd/MM/yyyy");
        }

        private void ResetView_Click(object sender, EventArgs e)
        {
            if (!kryptonTrackBar1.Enabled) return;

            int currentWeekOffset = (int)Math.Floor((DateTime.Today - _projectOverallStartDate).TotalDays / 7.0);

            if (currentWeekOffset >= kryptonTrackBar1.Minimum && currentWeekOffset <= kryptonTrackBar1.Maximum)
            {
                kryptonTrackBar1.Value = currentWeekOffset;
            }
            else
            {
                kryptonTrackBar1.Value = kryptonTrackBar1.Minimum;
            }
        }

        // La méthode RefreshData est maintenant un "aiguilleur"
        public void RefreshData()
        {
            if (_useCase == null) return;

            var viewData = _useCase.ObtenirDonneesPourPlanningView(_currentStartDate, 14);
            if (viewData == null) return;

            SuspendLayout();

            bool hasData = viewData.Ouvriers.Any();
            lblNoPlanning.Visible = !hasData;
            kryptonTableLayoutPanel1.Visible = hasData;

            if (!hasData)
            {
                ResumeLayout(false);
                return;
            }

            // Si la grille n'a jamais été construite, ou si le nombre d'ouvriers a changé, on la reconstruit.
            if (!_isGridInitialized || _ouvrierHeaders.Length != viewData.Ouvriers.Count)
            {
                InitializeGridStructure(viewData);
            }

            // Dans tous les cas, on met à jour le contenu (rapide).
            UpdateGridContents(viewData);

            ResumeLayout(true);
        }

        private void ClearPlanningGrid()
        {
            while (kryptonTableLayoutPanel1.Controls.Count > 0)
            {
                var ctrl = kryptonTableLayoutPanel1.Controls[0];
                kryptonTableLayoutPanel1.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
            _isGridInitialized = false;
        }

        // Cette méthode s'exécute UNE SEULE FOIS (ou rarement)
        private void InitializeGridStructure(PlanningViewData viewData)
        {
            ClearPlanningGrid();
            SetupGridDimensions(viewData.Ouvriers.Count);

            int ouvrierCount = viewData.Ouvriers.Count;
            int dayCount = viewData.Jours.Count; // Sera toujours 14
            _dayHeaders = new KryptonHeader[dayCount];
            _ouvrierHeaders = new KryptonHeader[ouvrierCount];
            _cellPanels = new KryptonPanel[ouvrierCount, dayCount];
            

            // Création des en-têtes des jours
            for (int i = 0; i < dayCount; i++)
            {
                int colIndex = i + 1;
                var header = new KryptonHeader
                {
                    Dock = DockStyle.Fill,
                    HeaderStyle = Krypton.Toolkit.HeaderStyle.Secondary,
                    Values = { Image = null, Description = "" },
                    StateCommon = { Content = { ShortText = { Font = new Font(this.Font, FontStyle.Bold), TextH = PaletteRelativeAlign.Center, TextV = PaletteRelativeAlign.Center } } }
                };
                kryptonTableLayoutPanel1.Controls.Add(header, colIndex, 0);
                _dayHeaders[i] = header;
            }

            // Création des en-têtes des ouvriers
            for (int i = 0; i < ouvrierCount; i++)
            {
                int rowIndex = i + 1;
                var ouvrier = viewData.Ouvriers[i];

                // Récupérer les informations du métier pour cet ouvrier
                viewData.Metiers.TryGetValue(ouvrier.MetierId, out Metier metier);
                viewData.MetierColors.TryGetValue(ouvrier.MetierId, out Color metierColor);

                // 1. Créer le panel parent qui aura la couleur de fond
                var parentPanel = new KryptonPanel
                {
                    Dock = DockStyle.Fill,
                    StateCommon = { Color1 = metierColor }
                };

                // 2. Créer le header pour le nom de l'ouvrier
                var nameHeader = new KryptonHeader
                {
                    Dock = DockStyle.None, // Il remplira l'espace restant
                    Location = new Point(40, 2), // Décalé de 40px pour laisser la place à l'icône
                    Width = parentPanel.Width - 40, // Largeur ajustée dynamiquement
                    MinimumSize = new Size(108, 26),
                    MaximumSize = new Size(108, 26), // Hauteur fixe de 30px
                    HeaderStyle = Krypton.Toolkit.HeaderStyle.Secondary,
                    Values = { Image = null, Heading = ouvrier.NomComplet, Description = "" },
                    // Rendre le fond du header transparent pour voir la couleur du parentPanel
                    StateCommon =
            {
                Back = { Color1 = Color.Transparent },
                Content = { ShortText = { Font = new Font(this.Font, FontStyle.Bold), TextV = PaletteRelativeAlign.Center }},
                ButtonPadding = new Padding(5, 2, -1, 2)
            },
                    Margin = new Padding(0, 2, 0, 2) // La marge de 2px demandée
                };

                // 3. Créer le label pour le pictogramme
                var pictogramLabel = new KryptonLabel
                {
                    Dock = DockStyle.None, // Se colle à gauche
                    Location = new Point(0, 0),
                    Width = 40, // Largeur fixe pour l'icône
                    Margin = new Padding(2, 2, 2, 2),
                    MinimumSize = new Size(40, 30),
                    MaximumSize = new Size(40, 30),
                    Text = metier?.Pictogram ?? "❓", // Utilise le pictogramme ou un "?" par défaut
                    StateCommon =
            {
                ShortText =
                {
                    Font = new Font("Segoe UI Symbol", 14F),
                    TextH = PaletteRelativeAlign.Center,
                    TextV = PaletteRelativeAlign.Center,
                    Color1 = Color.Black
                }
            }
                };

                // 4. Assembler les contrôles dans le panel parent
                // L'ordre est important pour le docking !
                parentPanel.Controls.Add(nameHeader);
                parentPanel.Controls.Add(pictogramLabel);

                // 5. Ajouter le panel parent à la grille et le mettre en cache
                kryptonTableLayoutPanel1.Controls.Add(parentPanel, 0, rowIndex);
                _ouvrierHeaders[i] = nameHeader; // On garde une référence au header si on veut changer le nom plus tard
            }

            // Création des panels de fond et des conteneurs de tâches
            for (int r = 0; r < ouvrierCount; r++)
            {
                for (int c = 0; c < dayCount; c++)
                {
                    int rowIndex = r + 1;
                    int colIndex = c + 1;

                    // On ne crée QUE le panel conteneur.
                    var containerPanel = new KryptonPanel
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Padding(0)
                        // La couleur sera définie dynamiquement dans UpdateGridContents
                    };
                    containerPanel.Layout += PositionTaskBlocksInCell;
                    kryptonTableLayoutPanel1.Controls.Add(containerPanel, colIndex, rowIndex);
                    _cellPanels[r, c] = containerPanel;
                }
            }

            _isGridInitialized = true;
        }

        // Cette méthode s'exécute à CHAQUE navigation (très rapide)
        private void UpdateGridContents(PlanningViewData viewData)
        {
            int ouvrierCount = viewData.Ouvriers.Count;
            int dayCount = viewData.Jours.Count;

            // Mise à jour des en-têtes des jours
            for (int c = 0; c < dayCount; c++)
            {
                var jour = viewData.Jours[c];
                _dayHeaders[c].Values.Heading = $"{jour:ddd \r\ndd/MM}";
            }

            // Mise à jour du contenu ET de la couleur des cellules
            for (int r = 0; r < ouvrierCount; r++)
            {
                var ouvrier = viewData.Ouvriers[r];

                for (int c = 0; c < dayCount; c++)
                {
                    var jour = viewData.Jours[c];
                    bool isWeekend = jour.DayOfWeek == DayOfWeek.Saturday || jour.DayOfWeek == DayOfWeek.Sunday;

                    var containerPanel = _cellPanels[r, c];

                    // --- MODIFICATION : On change la couleur du panel lui-même ---
                    if (isWeekend)
                    {
                        containerPanel.StateCommon.Color1 = Color.FromArgb(230, 230, 230);
                    }
                    else
                    {
                        containerPanel.StateCommon.Color1 = Color.Transparent; // Ou Color.White si vous préférez
                    }

                    // Le reste de la logique est inchangé
                    var blocksForDay = viewData.BlocksParOuvrier[ouvrier.OuvrierId][c];
                    containerPanel.Tag = blocksForDay;
                    PositionTaskBlocksInCell(containerPanel, new LayoutEventArgs(containerPanel, "Tag"));
                }
            }
        }

        // Méthode utilitaire pour dessiner les blocs de tâches
        private void PositionTaskBlocksInCell(object sender, LayoutEventArgs e)
        {
            var panel = sender as KryptonPanel;
            if (panel == null || panel.Width <= 1) return;

            panel.Controls.Clear();

            if (panel.Tag == null) return;

            var blocksToDraw = panel.Tag as List<PlanningBlock>;
            if (blocksToDraw == null || !blocksToDraw.Any()) return;

            foreach (var block in blocksToDraw)
            {
                var taskPanel = new KryptonPanel
                {
                    Left = (int)(panel.ClientSize.Width * block.StartOffsetPercent),
                    Width = Math.Max(2, (int)(panel.ClientSize.Width * block.WidthPercent)),
                    Top = 2,
                    Height = panel.ClientSize.Height - 4,
                    StateCommon = { Color1 = GetColorForStatus(block.Statut) }
                };
                _toolTip.SetToolTip(taskPanel, $"{block.TacheId}\n{block.TacheNom}");
                panel.Controls.Add(taskPanel);
            }
        }

        private void SetupGridDimensions(int ouvrierCount)
        {
            const float OUVRIER_ROW_HEIGHT = 35F; // Hauteur fixe pour chaque ligne d'ouvrier en pixels

            var panel = kryptonTableLayoutPanel1;
            panel.SuspendLayout(); // Suspendre le layout pour des performances optimales

            // Le nombre total de lignes sera : 1 (header) + N (ouvriers) + 1 (ressort)
            panel.RowCount = ouvrierCount + 2;
            panel.RowStyles.Clear();

            // 1. Ajouter le style pour la ligne d'en-tête (inchangé)
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

            // 2. Ajouter les styles pour chaque ligne d'ouvrier avec une hauteur FIXE
            if (ouvrierCount > 0)
            {
                for (int i = 0; i < ouvrierCount; i++)
                {
                    panel.RowStyles.Add(new RowStyle(SizeType.Absolute, OUVRIER_ROW_HEIGHT));
                }
            }

            // 3. Ajouter la ligne "ressort" qui prendra tout l'espace restant
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            panel.ResumeLayout(false); // Reprendre le layout
        }

        private Color GetColorForStatus(Statut statut)
        {
            switch (statut)
            {
                case Statut.EnRetard: return Color.FromArgb(255, 128, 128); // Light Red
                case Statut.Terminée: return Color.Gray;
                case Statut.EnCours: return Color.Orange;
                case Statut.Planifiée: return Color.LightGreen;
                default: return Color.LightBlue;
            }
        }


    }
}