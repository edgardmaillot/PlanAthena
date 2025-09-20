using Krypton.Toolkit;
using PlanAthena.View.TaskManager;
using PlanAthena.View.Structure;
using Krypton.Docking;
using PlanAthena.View.Ressources;

namespace PlanAthena.View.TaskManager
{
    partial class TaskManagerView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            kryptonPanelMain = new KryptonPanel();
            panelGlobalActions = new KryptonPanel();
            kryptonTableLayoutPanel1 = new KryptonTableLayoutPanel();
            panUp = new KryptonLabel();
            panLeft = new KryptonLabel();
            panRight = new KryptonLabel();
            panDown = new KryptonLabel();
            panCenter = new KryptonLabel();
            kryptonLabel2 = new KryptonLabel();
            kryptonLabel1 = new KryptonLabel();
            zoomTrackBar = new KryptonTrackBar();
            btnPlanificator = new KryptonButton();
            btnAdjustView = new KryptonButton();
            btnMappingAuto = new KryptonButton();
            btnPrint = new KryptonButton();
            btnExporter = new KryptonButton();
            btnImporter = new KryptonButton();
            kryptonDockingManager = new KryptonDockingManager();
            creationToolboxView1 = new CreationToolboxView();
            pertDiagramControl1 = new PlanAthena.View.TaskManager.PertDiagram.PertDiagramControl();
            lotSelectionView1 = new LotSelectionView();
            tacheDetailView1 = new TacheDetailView();
            blocDetailView1 = new BlocDetailView();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).BeginInit();
            panelGlobalActions.SuspendLayout();
            kryptonTableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            kryptonPanelMain.Dock = DockStyle.Fill;
            kryptonPanelMain.Location = new Point(0, 0);
            kryptonPanelMain.Margin = new Padding(4, 3, 4, 3);
            kryptonPanelMain.Name = "kryptonPanelMain";
            kryptonPanelMain.Size = new Size(1400, 741);
            kryptonPanelMain.TabIndex = 0;
            // 
            // panelGlobalActions
            // 
            panelGlobalActions.Controls.Add(kryptonTableLayoutPanel1);
            panelGlobalActions.Controls.Add(kryptonLabel2);
            panelGlobalActions.Controls.Add(kryptonLabel1);
            panelGlobalActions.Controls.Add(zoomTrackBar);
            panelGlobalActions.Controls.Add(btnPlanificator);
            panelGlobalActions.Controls.Add(btnAdjustView);
            panelGlobalActions.Controls.Add(btnMappingAuto);
            panelGlobalActions.Controls.Add(btnPrint);
            panelGlobalActions.Controls.Add(btnExporter);
            panelGlobalActions.Controls.Add(btnImporter);
            panelGlobalActions.Dock = DockStyle.Bottom;
            panelGlobalActions.Location = new Point(0, 741);
            panelGlobalActions.Margin = new Padding(4, 3, 4, 3);
            panelGlobalActions.Name = "panelGlobalActions";
            panelGlobalActions.Size = new Size(1400, 67);
            panelGlobalActions.TabIndex = 1;
            // 
            // kryptonTableLayoutPanel1
            // 
            kryptonTableLayoutPanel1.ColumnCount = 3;
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.Controls.Add(panUp, 1, 0);
            kryptonTableLayoutPanel1.Controls.Add(panLeft, 0, 1);
            kryptonTableLayoutPanel1.Controls.Add(panRight, 2, 1);
            kryptonTableLayoutPanel1.Controls.Add(panDown, 1, 2);
            kryptonTableLayoutPanel1.Controls.Add(panCenter, 1, 1);
            kryptonTableLayoutPanel1.Location = new Point(1017, 3);
            kryptonTableLayoutPanel1.Name = "kryptonTableLayoutPanel1";
            kryptonTableLayoutPanel1.RowCount = 3;
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.Size = new Size(61, 58);
            kryptonTableLayoutPanel1.TabIndex = 9;
            // 
            // panUp
            // 
            panUp.Location = new Point(21, 0);
            panUp.Margin = new Padding(1, 0, 0, 0);
            panUp.Name = "panUp";
            panUp.Size = new Size(19, 20);
            panUp.StateNormal.ShortText.Font = new Font("Segoe UI Emoji", 12F);
            panUp.StateNormal.ShortText.Hint = PaletteTextHint.AntiAlias;
            panUp.TabIndex = 0;
            panUp.Values.Text = "↑";
            // 
            // panLeft
            // 
            panLeft.Location = new Point(0, 20);
            panLeft.Margin = new Padding(0);
            panLeft.Name = "panLeft";
            panLeft.Size = new Size(20, 20);
            panLeft.StateNormal.ShortText.Font = new Font("Segoe UI Emoji", 12F);
            panLeft.StateNormal.ShortText.Hint = PaletteTextHint.AntiAlias;
            panLeft.TabIndex = 1;
            panLeft.Values.Text = "←";
            // 
            // panRight
            // 
            panRight.Location = new Point(40, 20);
            panRight.Margin = new Padding(0);
            panRight.Name = "panRight";
            panRight.Size = new Size(21, 20);
            panRight.StateNormal.ShortText.Font = new Font("Segoe UI Emoji", 12F);
            panRight.StateNormal.ShortText.Hint = PaletteTextHint.AntiAlias;
            panRight.TabIndex = 2;
            panRight.Values.Text = "→";
            // 
            // panDown
            // 
            panDown.Location = new Point(21, 40);
            panDown.Margin = new Padding(1, 0, 0, 0);
            panDown.Name = "panDown";
            panDown.Size = new Size(19, 20);
            panDown.StateNormal.ShortText.Font = new Font("Segoe UI Emoji", 12F);
            panDown.StateNormal.ShortText.Hint = PaletteTextHint.AntiAlias;
            panDown.TabIndex = 3;
            panDown.Values.Text = "↓";
            // 
            // panCenter
            // 
            panCenter.Location = new Point(20, 20);
            panCenter.Margin = new Padding(0);
            panCenter.Name = "panCenter";
            panCenter.Size = new Size(20, 20);
            panCenter.StateNormal.Padding = new Padding(0);
            panCenter.StateNormal.ShortText.Font = new Font("Segoe UI Emoji", 12F);
            panCenter.StateNormal.ShortText.Hint = PaletteTextHint.AntiAlias;
            panCenter.StateNormal.ShortText.TextH = PaletteRelativeAlign.Center;
            panCenter.StateNormal.ShortText.TextV = PaletteRelativeAlign.Center;
            panCenter.TabIndex = 4;
            panCenter.Values.Text = "●";
            // 
            // kryptonLabel2
            // 
            kryptonLabel2.Location = new Point(715, 17);
            kryptonLabel2.Name = "kryptonLabel2";
            kryptonLabel2.Orientation = VisualOrientation.Left;
            kryptonLabel2.Size = new Size(20, 43);
            kryptonLabel2.TabIndex = 8;
            kryptonLabel2.Values.Text = "Zoom";
            // 
            // kryptonLabel1
            // 
            kryptonLabel1.Location = new Point(739, 0);
            kryptonLabel1.Name = "kryptonLabel1";
            kryptonLabel1.Size = new Size(247, 20);
            kryptonLabel1.TabIndex = 7;
            kryptonLabel1.Values.Text = "10%           100%                                  300%";
            // 
            // zoomTrackBar
            // 
            zoomTrackBar.LargeChange = 50;
            zoomTrackBar.Location = new Point(739, 27);
            zoomTrackBar.Maximum = 300;
            zoomTrackBar.Minimum = 10;
            zoomTrackBar.Name = "zoomTrackBar";
            zoomTrackBar.Size = new Size(231, 27);
            zoomTrackBar.SmallChange = 10;
            zoomTrackBar.TabIndex = 6;
            zoomTrackBar.Value = 100;
            // 
            // btnPlanificator
            // 
            btnPlanificator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPlanificator.Location = new Point(1258, 14);
            btnPlanificator.Margin = new Padding(4, 3, 4, 3);
            btnPlanificator.Name = "btnPlanificator";
            btnPlanificator.Size = new Size(128, 40);
            btnPlanificator.TabIndex = 5;
            btnPlanificator.Values.DropDownArrowColor = Color.Empty;
            btnPlanificator.Values.Text = "Planificateur";
            // 
            // btnAdjustView
            // 
            btnAdjustView.Location = new Point(562, 14);
            btnAdjustView.Margin = new Padding(4, 3, 4, 3);
            btnAdjustView.Name = "btnAdjustView";
            btnAdjustView.Size = new Size(128, 40);
            btnAdjustView.TabIndex = 4;
            btnAdjustView.Values.DropDownArrowColor = Color.Empty;
            btnAdjustView.Values.Text = "Ajuster Vue";
            btnAdjustView.Click += btnAdjustView_Click;
            // 
            // btnMappingAuto
            // 
            btnMappingAuto.Enabled = false;
            btnMappingAuto.Location = new Point(427, 14);
            btnMappingAuto.Margin = new Padding(4, 3, 4, 3);
            btnMappingAuto.Name = "btnMappingAuto";
            btnMappingAuto.Size = new Size(128, 40);
            btnMappingAuto.TabIndex = 3;
            btnMappingAuto.Values.DropDownArrowColor = Color.Empty;
            btnMappingAuto.Values.Text = "Mapping Auto";
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(292, 14);
            btnPrint.Margin = new Padding(4, 3, 4, 3);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(128, 40);
            btnPrint.TabIndex = 2;
            btnPrint.Values.DropDownArrowColor = Color.Empty;
            btnPrint.Values.Text = "Imprimer";
            btnPrint.Click += btnPrint_Click;
            // 
            // btnExporter
            // 
            btnExporter.Location = new Point(156, 14);
            btnExporter.Margin = new Padding(4, 3, 4, 3);
            btnExporter.Name = "btnExporter";
            btnExporter.Size = new Size(128, 40);
            btnExporter.TabIndex = 1;
            btnExporter.Values.DropDownArrowColor = Color.Empty;
            btnExporter.Values.Text = "Exporter";
            btnExporter.Click += btnExporter_Click;
            // 
            // btnImporter
            // 
            btnImporter.Location = new Point(21, 14);
            btnImporter.Margin = new Padding(4, 3, 4, 3);
            btnImporter.Name = "btnImporter";
            btnImporter.Size = new Size(128, 40);
            btnImporter.TabIndex = 0;
            btnImporter.Values.DropDownArrowColor = Color.Empty;
            btnImporter.Values.Text = "Importer";
            btnImporter.Click += btnImporter_Click;
            // 
            // creationToolboxView1
            // 
            creationToolboxView1.Location = new Point(0, 0);
            creationToolboxView1.Margin = new Padding(4, 3, 4, 3);
            creationToolboxView1.Name = "creationToolboxView1";
            creationToolboxView1.Size = new Size(200, 630);
            creationToolboxView1.TabIndex = 0;
            // 
            // pertDiagramControl1
            // 
            pertDiagramControl1.Location = new Point(0, 0);
            pertDiagramControl1.Name = "pertDiagramControl1";
            pertDiagramControl1.Size = new Size(570, 630);
            pertDiagramControl1.TabIndex = 0;
            // 
            // lotSelectionView1
            // 
            lotSelectionView1.Location = new Point(0, 0);
            lotSelectionView1.Margin = new Padding(4, 3, 4, 3);
            lotSelectionView1.Name = "lotSelectionView1";
            lotSelectionView1.Size = new Size(400, 200);
            lotSelectionView1.TabIndex = 0;
            // 
            // tacheDetailView1
            // 
            tacheDetailView1.Location = new Point(0, 0);
            tacheDetailView1.Margin = new Padding(4, 3, 4, 3);
            tacheDetailView1.Name = "tacheDetailView1";
            tacheDetailView1.Size = new Size(400, 425);
            tacheDetailView1.TabIndex = 0;
            // 
            // blocDetailView1
            // 
            blocDetailView1.Location = new Point(0, 0);
            blocDetailView1.Margin = new Padding(4, 3, 4, 3);
            blocDetailView1.Name = "blocDetailView1";
            blocDetailView1.Size = new Size(700, 500);
            blocDetailView1.TabIndex = 3;
            // 
            // TaskManagerView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonPanelMain);
            Controls.Add(panelGlobalActions);
            Margin = new Padding(4, 3, 4, 3);
            Name = "TaskManagerView";
            Size = new Size(1400, 808);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).EndInit();
            panelGlobalActions.ResumeLayout(false);
            panelGlobalActions.PerformLayout();
            kryptonTableLayoutPanel1.ResumeLayout(false);
            kryptonTableLayoutPanel1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonPanel panelGlobalActions;
        private CreationToolboxView creationToolboxView1;
        private PertDiagram.PertDiagramControl pertDiagramControl1;
        private LotSelectionView lotSelectionView1;
        private TacheDetailView tacheDetailView1;
        private BlocDetailView blocDetailView1;
        private KryptonButton btnImporter;
        private KryptonButton btnPlanificator;
        private KryptonButton btnAdjustView;
        private KryptonButton btnMappingAuto;
        private KryptonButton btnPrint;
        private KryptonButton btnExporter;
        private KryptonDockingManager kryptonDockingManager;
        private KryptonTrackBar zoomTrackBar;
        private KryptonLabel kryptonLabel1;
        private KryptonLabel kryptonLabel2;
        private KryptonTableLayoutPanel kryptonTableLayoutPanel1;
        private KryptonLabel panUp;
        private KryptonLabel panLeft;
        private KryptonLabel panRight;
        private KryptonLabel panDown;
        private KryptonLabel panCenter;
    }
}