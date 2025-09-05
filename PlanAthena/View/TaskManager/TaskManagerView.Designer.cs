// --- START OF FILE TaskManagerView.Designer.cs ---

using Krypton.Docking;
using Krypton.Toolkit;
using PlanAthena.View.Ressources;
using PlanAthena.View.Structure;
using PlanAthena.View.TaskManager;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskManagerView));
            kryptonPanelMain = new KryptonPanel();
            panelGlobalActions = new KryptonPanel();
            btnToggleView = new KryptonButton();
            btnPlanificator = new KryptonButton();
            btnAdjustView = new KryptonButton();
            btnMappingAuto = new KryptonButton();
            btnPrint = new KryptonButton();
            btnExporter = new KryptonButton();
            btnImporter = new KryptonButton();
            kryptonDockingManager = new KryptonDockingManager();
            creationToolboxView1 = new CreationToolboxView();
            pertDiagramControl1 = new PlanAthena.View.TaskManager.PertDiagram.PertDiagramControl();
            taskManagerListe1 = new TaskManagerList();
            lotSelectionView1 = new LotSelectionView();
            tacheDetailView1 = new TacheDetailView();
            blocDetailView1 = new BlocDetailView();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).BeginInit();
            panelGlobalActions.SuspendLayout();
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
            panelGlobalActions.Controls.Add(btnToggleView);
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
            // btnToggleView
            // 
            btnToggleView.Location = new Point(697, 14);
            btnToggleView.Name = "btnToggleView";
            btnToggleView.Size = new Size(128, 40);
            btnToggleView.TabIndex = 6;
            btnToggleView.Values.DropDownArrowColor = Color.Empty;
            btnToggleView.Values.Text = "Afficher la liste";
            btnToggleView.Click += BtnToggleView_Click;
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
            creationToolboxView1.BackgroundImage = (Image)resources.GetObject("creationToolboxView1.BackgroundImage");
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
            // taskManagerListe1
            // 
            taskManagerListe1.Location = new Point(0, 0);
            taskManagerListe1.Name = "taskManagerListe1";
            taskManagerListe1.Size = new Size(1400, 800);
            taskManagerListe1.TabIndex = 0;
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
        private TaskManagerList taskManagerListe1;
        private KryptonButton btnImporter;
        private KryptonButton btnPlanificator;
        private KryptonButton btnAdjustView;
        private KryptonButton btnMappingAuto;
        private KryptonButton btnPrint;
        private KryptonButton btnExporter;
        private KryptonDockingManager kryptonDockingManager;
        private KryptonButton btnToggleView;
    }
}