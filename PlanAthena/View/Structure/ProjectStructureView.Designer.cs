using Krypton.Toolkit;

namespace PlanAthena.View.Structure
{
    partial class ProjectStructureView
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
            this.kryptonSplitContainer1 = new Krypton.Toolkit.KryptonSplitContainer();
            this.headerStructure = new Krypton.Toolkit.KryptonHeaderGroup();
            this.panelStructureActions = new Krypton.Toolkit.KryptonPanel();
            this.btnNewBloc = new Krypton.Toolkit.KryptonButton();
            this.btnNewLot = new Krypton.Toolkit.KryptonButton();
            this.gridStructure = new Krypton.Toolkit.KryptonDataGridView();
            this.textSearch = new Krypton.Toolkit.KryptonTextBox();
            this.panelDetailsContainer = new Krypton.Toolkit.KryptonPanel();
            this.blocDetailView1 = new PlanAthena.View.Structure.BlocDetailView();
            this.lotDetailView1 = new PlanAthena.View.Structure.LotDetailView();
            this.panelGlobalActions = new Krypton.Toolkit.KryptonPanel();
            this.btnManageTasks = new Krypton.Toolkit.KryptonButton();
            this.btnDelete = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanelMain)).BeginInit();
            this.kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel1)).BeginInit();
            this.kryptonSplitContainer1.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel2)).BeginInit();
            this.kryptonSplitContainer1.Panel2.SuspendLayout();
            this.kryptonSplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerStructure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.headerStructure.Panel)).BeginInit();
            this.headerStructure.Panel.SuspendLayout();
            this.headerStructure.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelStructureActions)).BeginInit();
            this.panelStructureActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridStructure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelDetailsContainer)).BeginInit();
            this.panelDetailsContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelGlobalActions)).BeginInit();
            this.panelGlobalActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            this.kryptonPanelMain.Controls.Add(this.kryptonSplitContainer1);
            this.kryptonPanelMain.Controls.Add(this.panelGlobalActions);
            this.kryptonPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonPanelMain.Location = new System.Drawing.Point(0, 0);
            this.kryptonPanelMain.Name = "kryptonPanelMain";
            this.kryptonPanelMain.Padding = new System.Windows.Forms.Padding(10);
            this.kryptonPanelMain.Size = new System.Drawing.Size(1200, 700);
            this.kryptonPanelMain.TabIndex = 0;
            // 
            // kryptonSplitContainer1
            // 
            this.kryptonSplitContainer1.Cursor = System.Windows.Forms.Cursors.Default;
            this.kryptonSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonSplitContainer1.Location = new System.Drawing.Point(10, 10);
            this.kryptonSplitContainer1.Name = "kryptonSplitContainer1";
            // 
            // kryptonSplitContainer1.Panel1
            // 
            this.kryptonSplitContainer1.Panel1.Controls.Add(this.headerStructure);
            // 
            // kryptonSplitContainer1.Panel2
            // 
            this.kryptonSplitContainer1.Panel2.Controls.Add(this.panelDetailsContainer);
            this.kryptonSplitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.kryptonSplitContainer1.Size = new System.Drawing.Size(1180, 630);
            this.kryptonSplitContainer1.SplitterDistance = 500;
            this.kryptonSplitContainer1.TabIndex = 1;
            // 
            // headerStructure
            // 
            this.headerStructure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerStructure.HeaderVisibleSecondary = false;
            this.headerStructure.Location = new System.Drawing.Point(0, 0);
            this.headerStructure.Name = "headerStructure";
            // 
            // headerStructure.Panel
            // 
            this.headerStructure.Panel.Controls.Add(this.panelStructureActions);
            this.headerStructure.Panel.Controls.Add(this.gridStructure);
            this.headerStructure.Panel.Controls.Add(this.textSearch);
            this.headerStructure.Panel.Padding = new System.Windows.Forms.Padding(5);
            this.headerStructure.Size = new System.Drawing.Size(500, 630);
            this.headerStructure.TabIndex = 0;
            this.headerStructure.ValuesPrimary.Heading = "Structure du Projet";
            this.headerStructure.ValuesPrimary.Image = null;
            // 
            // panelStructureActions
            // 
            this.panelStructureActions.Controls.Add(this.btnNewBloc);
            this.panelStructureActions.Controls.Add(this.btnNewLot);
            this.panelStructureActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStructureActions.Location = new System.Drawing.Point(5, 549);
            this.panelStructureActions.Name = "panelStructureActions";
            this.panelStructureActions.Size = new System.Drawing.Size(488, 50);
            this.panelStructureActions.TabIndex = 2;
            // 
            // btnNewBloc
            // 
            this.btnNewBloc.Location = new System.Drawing.Point(111, 10);
            this.btnNewBloc.Name = "btnNewBloc";
            this.btnNewBloc.Size = new System.Drawing.Size(100, 30);
            this.btnNewBloc.TabIndex = 1;
            this.btnNewBloc.Values.Text = "Nouveau Bloc";
            this.btnNewBloc.Click += new System.EventHandler(this.btnNewBloc_Click);
            // 
            // btnNewLot
            // 
            this.btnNewLot.Location = new System.Drawing.Point(5, 10);
            this.btnNewLot.Name = "btnNewLot";
            this.btnNewLot.Size = new System.Drawing.Size(100, 30);
            this.btnNewLot.TabIndex = 0;
            this.btnNewLot.Values.Text = "Nouveau Lot";
            this.btnNewLot.Click += new System.EventHandler(this.btnNewLot_Click);
            // 
            // gridStructure
            // 
            this.gridStructure.AllowUserToAddRows = false;
            this.gridStructure.AllowUserToDeleteRows = false;
            this.gridStructure.AllowUserToResizeRows = false;
            this.gridStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridStructure.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridStructure.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridStructure.Location = new System.Drawing.Point(5, 34);
            this.gridStructure.MultiSelect = false;
            this.gridStructure.Name = "gridStructure";
            this.gridStructure.ReadOnly = true;
            this.gridStructure.RowHeadersVisible = false;
            this.gridStructure.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridStructure.Size = new System.Drawing.Size(488, 509);
            this.gridStructure.TabIndex = 1;
            this.gridStructure.SelectionChanged += new System.EventHandler(this.gridStructure_SelectionChanged);
            // 
            // textSearch
            // 
            this.textSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.textSearch.Location = new System.Drawing.Point(5, 5);
            this.textSearch.Name = "textSearch";
            this.textSearch.Size = new System.Drawing.Size(488, 23);
            this.textSearch.TabIndex = 0;
            this.textSearch.TextChanged += new System.EventHandler(this.textSearch_TextChanged);
            // 
            // panelDetailsContainer
            // 
            this.panelDetailsContainer.Controls.Add(this.blocDetailView1);
            this.panelDetailsContainer.Controls.Add(this.lotDetailView1);
            this.panelDetailsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDetailsContainer.Location = new System.Drawing.Point(10, 0);
            this.panelDetailsContainer.Name = "panelDetailsContainer";
            this.panelDetailsContainer.Size = new System.Drawing.Size(666, 630);
            this.panelDetailsContainer.TabIndex = 0;
            // 
            // blocDetailView1
            // 
            this.blocDetailView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blocDetailView1.Location = new System.Drawing.Point(0, 0);
            this.blocDetailView1.Name = "blocDetailView1";
            this.blocDetailView1.Size = new System.Drawing.Size(666, 630);
            this.blocDetailView1.TabIndex = 1;
            // 
            // lotDetailView1
            // 
            this.lotDetailView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lotDetailView1.Location = new System.Drawing.Point(0, 0);
            this.lotDetailView1.Name = "lotDetailView1";
            this.lotDetailView1.Size = new System.Drawing.Size(666, 630);
            this.lotDetailView1.TabIndex = 0;
            // 
            // panelGlobalActions
            // 
            this.panelGlobalActions.Controls.Add(this.btnManageTasks);
            this.panelGlobalActions.Controls.Add(this.btnDelete);
            this.panelGlobalActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGlobalActions.Location = new System.Drawing.Point(10, 640);
            this.panelGlobalActions.Name = "panelGlobalActions";
            this.panelGlobalActions.Size = new System.Drawing.Size(1180, 50);
            this.panelGlobalActions.TabIndex = 0;
            // 
            // btnManageTasks
            // 
            this.btnManageTasks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnManageTasks.Location = new System.Drawing.Point(1024, 10);
            this.btnManageTasks.Name = "btnManageTasks";
            this.btnManageTasks.Size = new System.Drawing.Size(144, 30);
            this.btnManageTasks.TabIndex = 1;
            this.btnManageTasks.Values.Text = "Gérer les Tâches";
            this.btnManageTasks.Click += new System.EventHandler(this.btnManageTasks_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(928, 10);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(90, 30);
            this.btnDelete.TabIndex = 0;
            this.btnDelete.Values.Text = "Supprimer";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // ProjectStructureView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.kryptonPanelMain);
            this.Name = "ProjectStructureView";
            this.Size = new System.Drawing.Size(1200, 700);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanelMain)).EndInit();
            this.kryptonPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel1)).EndInit();
            this.kryptonSplitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel2)).EndInit();
            this.kryptonSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1)).EndInit();
            this.kryptonSplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.headerStructure.Panel)).EndInit();
            this.headerStructure.Panel.ResumeLayout(false);
            this.headerStructure.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerStructure)).EndInit();
            this.headerStructure.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelStructureActions)).EndInit();
            this.panelStructureActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridStructure)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelDetailsContainer)).EndInit();
            this.panelDetailsContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelGlobalActions)).EndInit();
            this.panelGlobalActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonPanel panelGlobalActions;
        private KryptonButton btnManageTasks;
        private KryptonButton btnDelete;
        private KryptonSplitContainer kryptonSplitContainer1;
        private KryptonHeaderGroup headerStructure;
        private KryptonPanel panelStructureActions;
        private KryptonButton btnNewBloc;
        private KryptonButton btnNewLot;
        private KryptonDataGridView gridStructure;
        private KryptonTextBox textSearch;
        private KryptonPanel panelDetailsContainer;
        private BlocDetailView blocDetailView1;
        private LotDetailView lotDetailView1;
    }
}