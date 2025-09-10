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
        private DateTime _currentStartDate;

        public PlanningView()
        {
            InitializeComponent();
            // Activer le double buffering pour réduire le scintillement
            this.DoubleBuffered = true;
        }

        public void Initialize(PilotageProjetUseCase useCase)
        {
            _useCase = useCase;
            // Initialiser la date de début au dimanche de la semaine en cours
            _currentStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            AttachEvents();
        }

        private void AttachEvents()
        {
            resetView.Click += (s, e) => RefreshData();
            // La logique du trackbar sera pour une version future
        }


        public void RefreshData()
        {
            if (_useCase == null) return;

            var viewData = _useCase.ObtenirDonneesPourPlanningView(_currentStartDate, 14);
            if (viewData == null) return;

            SuspendLayout();
            ClearPlanningGrid(); // Cette méthode va maintenant tout nettoyer.

            bool hasData = viewData.Ouvriers.Any();
            lblNoPlanning.Visible = !hasData;
            kryptonTableLayoutPanel1.Visible = hasData;

            if (!hasData)
            {
                ResumeLayout();
                return;
            }

            SetupGridDimensions(viewData.Ouvriers.Count);
            PopulateHeaders(viewData.Jours, viewData.Ouvriers);
            PopulateTaskBlocks(viewData); // Nous allons réécrire cette méthode.
            ResumeLayout();
        }

        private void ClearPlanningGrid()
        {
            // Nettoie TOUS les contrôles du TLP, car nous allons tout reconstruire.
            while (kryptonTableLayoutPanel1.Controls.Count > 0)
            {
                var ctrl = kryptonTableLayoutPanel1.Controls[0];
                kryptonTableLayoutPanel1.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
        }

        private void SetupGridDimensions(int ouvrierCount)
        {
            var panel = kryptonTableLayoutPanel1;
            panel.RowCount = ouvrierCount + 1;
            panel.RowStyles.Clear();
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // Header row

            if (ouvrierCount > 0)
            {
                float percent = 100F / ouvrierCount;
                for (int i = 0; i < ouvrierCount; i++)
                {
                    panel.RowStyles.Add(new RowStyle(SizeType.Percent, percent));
                }
            }
        }

        private void PopulateHeaders(List<DateTime> jours, List<Ouvrier> ouvriers)
        {
            // En-têtes des jours
            for (int i = 0; i < jours.Count; i++)
            {
                int colIndex = i + 1;
                var jour = jours[i];

                // On utilise UN SEUL contrôle KryptonHeader qui gère tout
                var header = new KryptonHeader
                {
                    Dock = DockStyle.Fill,
                    HeaderStyle = Krypton.Toolkit.HeaderStyle.Secondary,
                    Values = { Image = null, Heading = $"{jour:ddd \r\ndd/MM}", Description = "" },
                    StateCommon =
            {
                Content = { ShortText = {
                    Font = new Font(this.Font, FontStyle.Bold),
                    TextH = PaletteRelativeAlign.Center,
                    TextV = PaletteRelativeAlign.Center
                }}
            }
                };
                kryptonTableLayoutPanel1.Controls.Add(header, colIndex, 0);

                // La logique pour griser les week-ends reste la même et fonctionnera
                if (jour.DayOfWeek == DayOfWeek.Saturday || jour.DayOfWeek == DayOfWeek.Sunday)
                {
                    for (int rowIndex = 1; rowIndex < kryptonTableLayoutPanel1.RowCount; rowIndex++)
                    {
                        var bgPanel = new KryptonPanel
                        {
                            Dock = DockStyle.Fill,
                            StateCommon = { Color1 = Color.FromArgb(230, 230, 230) }
                        };
                        kryptonTableLayoutPanel1.Controls.Add(bgPanel, colIndex, rowIndex);
                    }
                }
            }

            // En-têtes des ouvriers
            for (int i = 0; i < ouvriers.Count; i++)
            {
                int rowIndex = i + 1;

                var header = new KryptonHeader
                {
                    Dock = DockStyle.Fill,
                    HeaderStyle = Krypton.Toolkit.HeaderStyle.Secondary,
                    Values = { Image = null, Heading = ouvriers[i].NomComplet, Description = "" },
                    StateCommon =
            {
                Content = { ShortText = {
                    Font = new Font(this.Font, FontStyle.Bold),
                    TextV = PaletteRelativeAlign.Center
                }},
                ButtonPadding = new Padding(5, -1, -1, -1),
            }
                };
                kryptonTableLayoutPanel1.Controls.Add(header, 0, rowIndex);
            }
        }

        private void PopulateTaskBlocks(PlanningViewData viewData)
        {
            // Créer un dictionnaire pour trouver rapidement l'index d'un ouvrier
            var mapOuvrierToRowIndex = viewData.Ouvriers
                .Select((ouvrier, index) => new { ouvrier.OuvrierId, RowIndex = index + 1 })
                .ToDictionary(x => x.OuvrierId, x => x.RowIndex);

            for (int dayIndex = 0; dayIndex < viewData.Jours.Count; dayIndex++)
            {
                int colIndex = dayIndex + 1; // Les jours commencent à la colonne 1

                foreach (var ouvrier in viewData.Ouvriers)
                {
                    if (!viewData.BlocksParOuvrier.ContainsKey(ouvrier.OuvrierId)) continue;

                    var blocksForDay = viewData.BlocksParOuvrier[ouvrier.OuvrierId][dayIndex];
                    if (!blocksForDay.Any()) continue;

                    int rowIndex = mapOuvrierToRowIndex[ouvrier.OuvrierId];

                    // Créer un panel conteneur qui servira de "canvas" pour cette cellule
                    var containerPanel = new KryptonPanel
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Padding(0),
                        StateCommon = { Color1 = Color.Transparent } // Important pour voir le fond grisé du week-end
                    };
                    kryptonTableLayoutPanel1.Controls.Add(containerPanel, colIndex, rowIndex);

                    // Attacher un gestionnaire d'événement pour positionner les tâches APRÈS que le conteneur soit dimensionné
                    containerPanel.SizeChanged += (sender, e) =>
                    {
                        var panel = sender as KryptonPanel;
                        if (panel == null || panel.Width == 0) return;

                        // Vider les anciennes tâches avant de redessiner
                        panel.Controls.Clear();

                        foreach (var block in blocksForDay)
                        {
                            var taskPanel = new KryptonPanel
                            {
                                Left = (int)(panel.ClientSize.Width * block.StartOffsetPercent),
                                Width = Math.Max(2, (int)(panel.ClientSize.Width * block.WidthPercent)),
                                Top = 2,
                                Height = panel.ClientSize.Height - 4,
                                StateCommon =
                        {
                            Color1 = GetColorForStatus(block.Statut),
                            //Border = { DrawBorders = PaletteDrawBorders.All, Color1 = Color.Black, Width = 1 }
                        }
                            };
                            _toolTip.SetToolTip(taskPanel, $"{block.TacheId}\n{block.TacheNom}");
                            panel.Controls.Add(taskPanel);
                        }
                    };
                }
            }
        }

        private void DrawBlocksOnPanel(object sender, PaintEventArgs e, List<PlanningBlock> blocks)
        {
            var panel = sender as Panel;
            if (panel == null) return;

            foreach (var block in blocks)
            {
                int x = (int)(panel.Width * block.StartOffsetPercent);
                int width = (int)(panel.Width * block.WidthPercent);
                Rectangle rect = new Rectangle(x, 2, Math.Max(2, width), panel.Height - 4);

                using (var brush = new SolidBrush(GetColorForStatus(block.Statut)))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                using (var pen = new Pen(Color.Black))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
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