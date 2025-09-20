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
            kryptonPanelMain = new KryptonPanel();
            kryptonSplitContainer1 = new KryptonSplitContainer();
            headerStructure = new KryptonHeaderGroup();
            panelStructureActions = new KryptonPanel();
            btnNewBloc = new KryptonButton();
            btnNewLot = new KryptonButton();
            gridStructure = new KryptonDataGridView();
            textSearch = new KryptonTextBox();
            panelDetailsContainer = new KryptonPanel();
            blocDetailView1 = new BlocDetailView();
            lotDetailView1 = new LotDetailView();
            panelGlobalActions = new KryptonPanel();
            btnManageTasks = new KryptonButton();
            btnDelete = new KryptonButton();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).BeginInit();
            (kryptonSplitContainer1.Panel1).BeginInit();
            kryptonSplitContainer1.Panel1.SuspendLayout();
            (kryptonSplitContainer1.Panel2).BeginInit();
            kryptonSplitContainer1.Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)headerStructure).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerStructure.Panel).BeginInit();
            headerStructure.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelStructureActions).BeginInit();
            panelStructureActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridStructure).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelDetailsContainer).BeginInit();
            panelDetailsContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).BeginInit();
            panelGlobalActions.SuspendLayout();
            SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            kryptonPanelMain.Controls.Add(kryptonSplitContainer1);
            kryptonPanelMain.Controls.Add(panelGlobalActions);
            kryptonPanelMain.Dock = DockStyle.Fill;
            kryptonPanelMain.Location = new Point(0, 0);
            kryptonPanelMain.Margin = new Padding(4, 3, 4, 3);
            kryptonPanelMain.Name = "kryptonPanelMain";
            kryptonPanelMain.Padding = new Padding(12);
            kryptonPanelMain.Size = new Size(1400, 808);
            kryptonPanelMain.TabIndex = 0;
            // 
            // kryptonSplitContainer1
            // 
            kryptonSplitContainer1.Dock = DockStyle.Fill;
            kryptonSplitContainer1.Location = new Point(12, 12);
            kryptonSplitContainer1.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            kryptonSplitContainer1.Panel1.Controls.Add(headerStructure);
            // 
            // 
            // 
            kryptonSplitContainer1.Panel2.Controls.Add(panelDetailsContainer);
            kryptonSplitContainer1.Panel2.Padding = new Padding(12, 0, 0, 0);
            kryptonSplitContainer1.Size = new Size(1376, 726);
            kryptonSplitContainer1.SplitterDistance = 583;
            kryptonSplitContainer1.TabIndex = 1;
            // 
            // headerStructure
            // 
            headerStructure.Dock = DockStyle.Fill;
            headerStructure.HeaderVisibleSecondary = false;
            headerStructure.Location = new Point(0, 0);
            headerStructure.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            headerStructure.Panel.Controls.Add(panelStructureActions);
            headerStructure.Panel.Controls.Add(gridStructure);
            headerStructure.Panel.Controls.Add(textSearch);
            headerStructure.Panel.Padding = new Padding(6);
            headerStructure.Size = new Size(583, 726);
            headerStructure.TabIndex = 0;
            headerStructure.ValuesPrimary.Heading = "Structure du Projet";
            headerStructure.ValuesPrimary.Image = null;
            // 
            // panelStructureActions
            // 
            panelStructureActions.Controls.Add(btnNewBloc);
            panelStructureActions.Controls.Add(btnNewLot);
            panelStructureActions.Dock = DockStyle.Bottom;
            panelStructureActions.Location = new Point(6, 630);
            panelStructureActions.Margin = new Padding(4, 3, 4, 3);
            panelStructureActions.Name = "panelStructureActions";
            panelStructureActions.Size = new Size(569, 58);
            panelStructureActions.TabIndex = 2;
            // 
            // btnNewBloc
            // 
            btnNewBloc.Location = new Point(130, 12);
            btnNewBloc.Margin = new Padding(4, 3, 4, 3);
            btnNewBloc.Name = "btnNewBloc";
            btnNewBloc.Size = new Size(117, 35);
            btnNewBloc.TabIndex = 1;
            btnNewBloc.Values.DropDownArrowColor = Color.Empty;
            btnNewBloc.Values.Text = "Nouveau Bloc";
            btnNewBloc.Click += btnNewBloc_Click;
            // 
            // btnNewLot
            // 
            btnNewLot.Location = new Point(6, 12);
            btnNewLot.Margin = new Padding(4, 3, 4, 3);
            btnNewLot.Name = "btnNewLot";
            btnNewLot.Size = new Size(117, 35);
            btnNewLot.TabIndex = 0;
            btnNewLot.Values.DropDownArrowColor = Color.Empty;
            btnNewLot.Values.Text = "Nouveau Lot";
            btnNewLot.Click += btnNewLot_Click;
            // 
            // gridStructure
            // 
            gridStructure.AllowUserToAddRows = false;
            gridStructure.AllowUserToDeleteRows = false;
            gridStructure.AllowUserToResizeRows = false;
            gridStructure.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridStructure.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridStructure.BorderStyle = BorderStyle.None;
            gridStructure.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridStructure.Location = new Point(6, 39);
            gridStructure.Margin = new Padding(4, 3, 4, 3);
            gridStructure.MultiSelect = false;
            gridStructure.Name = "gridStructure";
            gridStructure.ReadOnly = true;
            gridStructure.RowHeadersVisible = false;
            gridStructure.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridStructure.Size = new Size(569, 591);
            gridStructure.StateCommon.BackStyle = PaletteBackStyle.GridBackgroundList;
            gridStructure.StateCommon.HeaderColumn.Content.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            gridStructure.StateCommon.HeaderColumn.Content.Hint = PaletteTextHint.AntiAlias;
            gridStructure.TabIndex = 1;
            gridStructure.SelectionChanged += gridStructure_SelectionChanged;
            // 
            // textSearch
            // 
            textSearch.Dock = DockStyle.Top;
            textSearch.Location = new Point(6, 6);
            textSearch.Margin = new Padding(4, 3, 4, 3);
            textSearch.Name = "textSearch";
            textSearch.Size = new Size(569, 23);
            textSearch.TabIndex = 0;
            textSearch.TextChanged += textSearch_TextChanged;
            // 
            // panelDetailsContainer
            // 
            panelDetailsContainer.Controls.Add(blocDetailView1);
            panelDetailsContainer.Controls.Add(lotDetailView1);
            panelDetailsContainer.Dock = DockStyle.Fill;
            panelDetailsContainer.Location = new Point(12, 0);
            panelDetailsContainer.Margin = new Padding(4, 3, 4, 3);
            panelDetailsContainer.Name = "panelDetailsContainer";
            panelDetailsContainer.Size = new Size(776, 726);
            panelDetailsContainer.TabIndex = 0;
            // 
            // blocDetailView1
            // 
            blocDetailView1.Dock = DockStyle.Fill;
            blocDetailView1.Location = new Point(0, 0);
            blocDetailView1.Margin = new Padding(5, 3, 5, 3);
            blocDetailView1.Name = "blocDetailView1";
            blocDetailView1.Size = new Size(776, 726);
            blocDetailView1.TabIndex = 1;
            // 
            // lotDetailView1
            // 
            lotDetailView1.Dock = DockStyle.Fill;
            lotDetailView1.Location = new Point(0, 0);
            lotDetailView1.Margin = new Padding(5, 3, 5, 3);
            lotDetailView1.Name = "lotDetailView1";
            lotDetailView1.Size = new Size(776, 726);
            lotDetailView1.TabIndex = 0;
            // 
            // panelGlobalActions
            // 
            panelGlobalActions.Controls.Add(btnManageTasks);
            panelGlobalActions.Controls.Add(btnDelete);
            panelGlobalActions.Dock = DockStyle.Bottom;
            panelGlobalActions.Location = new Point(12, 738);
            panelGlobalActions.Margin = new Padding(4, 3, 4, 3);
            panelGlobalActions.Name = "panelGlobalActions";
            panelGlobalActions.Size = new Size(1376, 58);
            panelGlobalActions.TabIndex = 0;
            // 
            // btnManageTasks
            // 
            btnManageTasks.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnManageTasks.Location = new Point(1194, 12);
            btnManageTasks.Margin = new Padding(4, 3, 4, 3);
            btnManageTasks.Name = "btnManageTasks";
            btnManageTasks.Size = new Size(168, 35);
            btnManageTasks.TabIndex = 1;
            btnManageTasks.Values.DropDownArrowColor = Color.Empty;
            btnManageTasks.Values.Text = "Gérer les Tâches";
            btnManageTasks.Click += btnManageTasks_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(1082, 12);
            btnDelete.Margin = new Padding(4, 3, 4, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(105, 35);
            btnDelete.TabIndex = 0;
            btnDelete.Values.DropDownArrowColor = Color.Empty;
            btnDelete.Values.Text = "Supprimer";
            btnDelete.Click += btnDelete_Click;
            // 
            // ProjectStructureView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonPanelMain);
            Margin = new Padding(4, 3, 4, 3);
            Name = "ProjectStructureView";
            Size = new Size(1400, 808);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).EndInit();
            kryptonPanelMain.ResumeLayout(false);
            (kryptonSplitContainer1.Panel1).EndInit();
            kryptonSplitContainer1.Panel1.ResumeLayout(false);
            (kryptonSplitContainer1.Panel2).EndInit();
            kryptonSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerStructure.Panel).EndInit();
            headerStructure.Panel.ResumeLayout(false);
            headerStructure.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)headerStructure).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelStructureActions).EndInit();
            panelStructureActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridStructure).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelDetailsContainer).EndInit();
            panelDetailsContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).EndInit();
            panelGlobalActions.ResumeLayout(false);
            ResumeLayout(false);
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