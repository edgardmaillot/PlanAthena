using Krypton.Toolkit;
using PlanAthena.View;
using PlanAthena.View.TaskManager;
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
            this.kryptonPanelMain = new Krypton.Toolkit.KryptonPanel();
            this.splitContainerPrincipal = new Krypton.Toolkit.KryptonSplitContainer();
            this.creationToolboxView1 = new PlanAthena.View.TaskManager.CreationToolboxView();
            this.splitContainerCentreDroit = new Krypton.Toolkit.KryptonSplitContainer();
            this.pertDiagramControl1 = new PlanAthena.View.TaskManager.PertDiagram.PertDiagramControl();
            this.kryptonSplitContainerRight = new Krypton.Toolkit.KryptonSplitContainer();
            this.lotSelectionView1 = new PlanAthena.View.TaskManager.LotSelectionView();
            this.tacheDetailView1 = new PlanAthena.View.TaskManager.TacheDetailView();
            this.panelGlobalActions = new Krypton.Toolkit.KryptonPanel();
            this.btnPlanificator = new Krypton.Toolkit.KryptonButton();
            this.btnAdjustView = new Krypton.Toolkit.KryptonButton();
            this.btnMappingAuto = new Krypton.Toolkit.KryptonButton();
            this.btnPrint = new Krypton.Toolkit.KryptonButton();
            this.btnExporter = new Krypton.Toolkit.KryptonButton();
            this.btnImporter = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanelMain)).BeginInit();
            this.kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrincipal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrincipal.Panel1)).BeginInit();
            this.splitContainerPrincipal.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrincipal.Panel2)).BeginInit();
            this.splitContainerPrincipal.Panel2.SuspendLayout();
            this.splitContainerPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCentreDroit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCentreDroit.Panel1)).BeginInit();
            this.splitContainerCentreDroit.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCentreDroit.Panel2)).BeginInit();
            this.splitContainerCentreDroit.Panel2.SuspendLayout();
            this.splitContainerCentreDroit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainerRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainerRight.Panel1)).BeginInit();
            this.kryptonSplitContainerRight.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainerRight.Panel2)).BeginInit();
            this.kryptonSplitContainerRight.Panel2.SuspendLayout();
            this.kryptonSplitContainerRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelGlobalActions)).BeginInit();
            this.panelGlobalActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            this.kryptonPanelMain.Controls.Add(this.splitContainerPrincipal);
            this.kryptonPanelMain.Controls.Add(this.panelGlobalActions);
            this.kryptonPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonPanelMain.Location = new System.Drawing.Point(0, 0);
            this.kryptonPanelMain.Name = "kryptonPanelMain";
            this.kryptonPanelMain.Padding = new System.Windows.Forms.Padding(10);
            this.kryptonPanelMain.Size = new System.Drawing.Size(1200, 700);
            this.kryptonPanelMain.TabIndex = 0;
            // 
            // splitContainerPrincipal
            // 
            this.splitContainerPrincipal.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainerPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPrincipal.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerPrincipal.Location = new System.Drawing.Point(10, 10);
            this.splitContainerPrincipal.Name = "splitContainerPrincipal";
            // 
            // splitContainerPrincipal.Panel1
            // 
            this.splitContainerPrincipal.Panel1.Controls.Add(this.creationToolboxView1);
            // 
            // splitContainerPrincipal.Panel2
            // 
            this.splitContainerPrincipal.Panel2.Controls.Add(this.splitContainerCentreDroit);
            this.splitContainerPrincipal.Size = new System.Drawing.Size(1180, 630);
            this.splitContainerPrincipal.SplitterDistance = 200;
            this.splitContainerPrincipal.TabIndex = 1;
            // 
            // creationToolboxView1
            // 
            this.creationToolboxView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.creationToolboxView1.Location = new System.Drawing.Point(0, 0);
            this.creationToolboxView1.Name = "creationToolboxView1";
            this.creationToolboxView1.Size = new System.Drawing.Size(200, 630);
            this.creationToolboxView1.TabIndex = 0;
            // 
            // splitContainerCentreDroit
            // 
            this.splitContainerCentreDroit.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainerCentreDroit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerCentreDroit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerCentreDroit.Location = new System.Drawing.Point(0, 0);
            this.splitContainerCentreDroit.Name = "splitContainerCentreDroit";
            // 
            // splitContainerCentreDroit.Panel1
            // 
            this.splitContainerCentreDroit.Panel1.Controls.Add(this.pertDiagramControl1);
            // 
            // splitContainerCentreDroit.Panel2
            // 
            this.splitContainerCentreDroit.Panel2.Controls.Add(this.kryptonSplitContainerRight);
            this.splitContainerCentreDroit.Size = new System.Drawing.Size(975, 630);
            this.splitContainerCentreDroit.SplitterDistance = 570;
            this.splitContainerCentreDroit.TabIndex = 0;
            // 
            // pertDiagramControl1
            // 
            this.pertDiagramControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pertDiagramControl1.Location = new System.Drawing.Point(0, 0);
            this.pertDiagramControl1.Name = "pertDiagramControl1";
            this.pertDiagramControl1.Size = new System.Drawing.Size(570, 630);
            this.pertDiagramControl1.TabIndex = 0;
            // 
            // kryptonSplitContainerRight
            // 
            this.kryptonSplitContainerRight.Cursor = System.Windows.Forms.Cursors.Default;
            this.kryptonSplitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonSplitContainerRight.Location = new System.Drawing.Point(0, 0);
            this.kryptonSplitContainerRight.Name = "kryptonSplitContainerRight";
            this.kryptonSplitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // kryptonSplitContainerRight.Panel1
            // 
            this.kryptonSplitContainerRight.Panel1.Controls.Add(this.lotSelectionView1);
            // 
            // kryptonSplitContainerRight.Panel2
            // 
            this.kryptonSplitContainerRight.Panel2.Controls.Add(this.tacheDetailView1);
            this.kryptonSplitContainerRight.Size = new System.Drawing.Size(400, 630);
            this.kryptonSplitContainerRight.SplitterDistance = 200;
            this.kryptonSplitContainerRight.TabIndex = 0;
            // 
            // lotSelectionView1
            // 
            this.lotSelectionView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lotSelectionView1.Location = new System.Drawing.Point(0, 0);
            this.lotSelectionView1.Name = "lotSelectionView1";
            this.lotSelectionView1.Size = new System.Drawing.Size(400, 200);
            this.lotSelectionView1.TabIndex = 0;
            // 
            // tacheDetailView1
            // 
            this.tacheDetailView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tacheDetailView1.Location = new System.Drawing.Point(0, 0);
            this.tacheDetailView1.Name = "tacheDetailView1";
            this.tacheDetailView1.Size = new System.Drawing.Size(400, 425);
            this.tacheDetailView1.TabIndex = 0;
            // 
            // panelGlobalActions
            // 
            panelGlobalActions.Controls.Add(btnPlanificator);
            panelGlobalActions.Controls.Add(btnAdjustView);
            panelGlobalActions.Controls.Add(btnMappingAuto);
            panelGlobalActions.Controls.Add(btnPrint);
            panelGlobalActions.Controls.Add(btnExporter);
            panelGlobalActions.Controls.Add(btnImporter);
            panelGlobalActions.Dock = DockStyle.Bottom;
            panelGlobalActions.Location = new Point(12, 738);
            panelGlobalActions.Margin = new Padding(4, 3, 4, 3);
            panelGlobalActions.Name = "panelGlobalActions";
            panelGlobalActions.Size = new Size(1376, 58);
            panelGlobalActions.TabIndex = 0;
            // 
            // btnPlanificator
            // 
            btnPlanificator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPlanificator.Location = new Point(1245, 12);
            btnPlanificator.Margin = new Padding(4, 3, 4, 3);
            btnPlanificator.Name = "btnPlanificator";
            btnPlanificator.Size = new Size(117, 35);
            btnPlanificator.TabIndex = 5;
            btnPlanificator.Values.DropDownArrowColor = Color.Empty;
            btnPlanificator.Values.Text = "Planificateur";
            // 
            // btnAdjustView
            // 
            btnAdjustView.Location = new Point(513, 12);
            btnAdjustView.Margin = new Padding(4, 3, 4, 3);
            btnAdjustView.Name = "btnAdjustView";
            btnAdjustView.Size = new Size(117, 35);
            btnAdjustView.TabIndex = 4;
            btnAdjustView.Values.DropDownArrowColor = Color.Empty;
            btnAdjustView.Values.Text = "Ajuster Vue";
            btnAdjustView.Click += btnAdjustView_Click;
            // 
            // btnMappingAuto
            // 
            btnMappingAuto.Enabled = false;
            btnMappingAuto.Location = new Point(390, 12);
            btnMappingAuto.Margin = new Padding(4, 3, 4, 3);
            btnMappingAuto.Name = "btnMappingAuto";
            btnMappingAuto.Size = new Size(117, 35);
            btnMappingAuto.TabIndex = 3;
            btnMappingAuto.Values.DropDownArrowColor = Color.Empty;
            btnMappingAuto.Values.Text = "Mapping Auto";
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(266, 12);
            btnPrint.Margin = new Padding(4, 3, 4, 3);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(117, 35);
            btnPrint.TabIndex = 2;
            btnPrint.Values.DropDownArrowColor = Color.Empty;
            btnPrint.Values.Text = "Imprimer";
            btnPrint.Click += btnPrint_Click;
            // 
            // btnExporter
            // 
            btnExporter.Location = new Point(131, 12);
            btnExporter.Margin = new Padding(4, 3, 4, 3);
            btnExporter.Name = "btnExporter";
            btnExporter.Size = new Size(117, 35);
            btnExporter.TabIndex = 1;
            btnExporter.Values.DropDownArrowColor = Color.Empty;
            btnExporter.Values.Text = "Exporter";
            btnExporter.Click += btnExporter_Click;
            // 
            // btnImporter
            // 
            btnImporter.Location = new Point(7, 12);
            btnImporter.Margin = new Padding(4, 3, 4, 3);
            btnImporter.Name = "btnImporter";
            btnImporter.Size = new Size(117, 35);
            btnImporter.TabIndex = 0;
            btnImporter.Values.DropDownArrowColor = Color.Empty;
            btnImporter.Values.Text = "Importer";
            btnImporter.Click += btnImporter_Click;
            // 
            // TaskManagerView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.kryptonPanelMain);
            this.Name = "TaskManagerView";
            this.Size = new System.Drawing.Size(1200, 700);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanelMain)).EndInit();
            this.kryptonPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrincipal.Panel1)).EndInit();
            this.splitContainerPrincipal.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrincipal.Panel2)).EndInit();
            this.splitContainerPrincipal.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrincipal)).EndInit();
            this.splitContainerPrincipal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCentreDroit.Panel1)).EndInit();
            this.splitContainerCentreDroit.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCentreDroit.Panel2)).EndInit();
            this.splitContainerCentreDroit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCentreDroit)).EndInit();
            this.splitContainerCentreDroit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainerRight.Panel1)).EndInit();
            this.kryptonSplitContainerRight.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainerRight.Panel2)).EndInit();
            this.kryptonSplitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainerRight)).EndInit();
            this.kryptonSplitContainerRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelGlobalActions)).EndInit();
            this.panelGlobalActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonPanel panelGlobalActions;
        private CreationToolboxView creationToolboxView1;
        private KryptonPanel panelCenterAndRight;
        private View.TaskManager.PertDiagram.PertDiagramControl pertDiagramControl1;
        private KryptonSplitContainer kryptonSplitContainerRight;
        private LotSelectionView lotSelectionView1;
        private TacheDetailView tacheDetailView1;
        private KryptonButton btnImporter;
        private KryptonButton btnPlanificator;
        private KryptonButton btnAdjustView;
        private KryptonButton btnMappingAuto;
        private KryptonButton btnPrint;
        private KryptonButton btnExporter;
        private KryptonSplitContainer splitContainerPrincipal;
        private KryptonSplitContainer splitContainerCentreDroit;
    }
}