namespace PlanAthena.View.TaskManager.Cockpit
{
    partial class TaskListView
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            kpCentre = new Krypton.Toolkit.KryptonPanel();
            kryptonDataGridView1 = new Krypton.Toolkit.KryptonDataGridView();
            DG_TacheId = new DataGridViewTextBoxColumn();
            DG_Tache = new DataGridViewTextBoxColumn();
            DG_Metier = new DataGridViewTextBoxColumn();
            DG_Ouvrier = new DataGridViewTextBoxColumn();
            DG_Statut = new DataGridViewTextBoxColumn();
            DG_DDP = new DataGridViewTextBoxColumn();
            DG_DFP = new DataGridViewTextBoxColumn();
            DG_DDR = new DataGridViewTextBoxColumn();
            DG_DFR = new DataGridViewTextBoxColumn();
            kryptonTableLayoutPanel1 = new Krypton.Toolkit.KryptonTableLayoutPanel();
            kpTop = new Krypton.Toolkit.KryptonPanel();
            kryptonLabel4 = new Krypton.Toolkit.KryptonLabel();
            kryptonLabel1 = new Krypton.Toolkit.KryptonLabel();
            cmbFiltreLot = new Krypton.Toolkit.KryptonComboBox();
            kryptonLabel2 = new Krypton.Toolkit.KryptonLabel();
            cmbFiltreMetier = new Krypton.Toolkit.KryptonComboBox();
            kryptonLabel3 = new Krypton.Toolkit.KryptonLabel();
            cmbFiltreStatut = new Krypton.Toolkit.KryptonComboBox();
            btnAControler = new Krypton.Toolkit.KryptonButton();
            btnResetFiltres = new Krypton.Toolkit.KryptonButton();
            kpBottom = new Krypton.Toolkit.KryptonPanel();
            ((System.ComponentModel.ISupportInitialize)kpCentre).BeginInit();
            kpCentre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonDataGridView1).BeginInit();
            kryptonTableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpTop).BeginInit();
            kpTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbFiltreLot).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbFiltreMetier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbFiltreStatut).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kpBottom).BeginInit();
            SuspendLayout();
            // 
            // kpCentre
            // 
            kpCentre.Controls.Add(kryptonDataGridView1);
            kpCentre.Dock = DockStyle.Fill;
            kpCentre.Location = new Point(3, 53);
            kpCentre.Name = "kpCentre";
            kpCentre.Size = new Size(1650, 778);
            kpCentre.TabIndex = 0;
            // 
            // kryptonDataGridView1
            // 
            kryptonDataGridView1.AllowUserToAddRows = false;
            kryptonDataGridView1.AllowUserToDeleteRows = false;
            kryptonDataGridView1.AllowUserToOrderColumns = true;
            kryptonDataGridView1.BorderStyle = BorderStyle.None;
            kryptonDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            kryptonDataGridView1.Columns.AddRange(new DataGridViewColumn[] { DG_TacheId, DG_Tache, DG_Metier, DG_Ouvrier, DG_Statut, DG_DDP, DG_DFP, DG_DDR, DG_DFR });
            kryptonDataGridView1.Dock = DockStyle.Fill;
            kryptonDataGridView1.Location = new Point(0, 0);
            kryptonDataGridView1.MultiSelect = false;
            kryptonDataGridView1.Name = "kryptonDataGridView1";
            kryptonDataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            kryptonDataGridView1.Size = new Size(1650, 778);
            kryptonDataGridView1.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
            kryptonDataGridView1.StateCommon.HeaderColumn.Content.Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            kryptonDataGridView1.StateCommon.HeaderColumn.Content.Hint = Krypton.Toolkit.PaletteTextHint.AntiAlias;
            kryptonDataGridView1.TabIndex = 0;
            // 
            // DG_TacheId
            // 
            DG_TacheId.FillWeight = 50F;
            DG_TacheId.HeaderText = "Identification";
            DG_TacheId.MinimumWidth = 10;
            DG_TacheId.Name = "DG_TacheId";
            DG_TacheId.ReadOnly = true;
            DG_TacheId.Width = 50;
            // 
            // DG_Tache
            // 
            DG_Tache.FillWeight = 80F;
            DG_Tache.HeaderText = "Tache";
            DG_Tache.MinimumWidth = 100;
            DG_Tache.Name = "DG_Tache";
            DG_Tache.ReadOnly = true;
            DG_Tache.Width = 200;
            // 
            // DG_Metier
            // 
            DG_Metier.FillWeight = 80F;
            DG_Metier.HeaderText = "Métier";
            DG_Metier.MinimumWidth = 20;
            DG_Metier.Name = "DG_Metier";
            DG_Metier.ReadOnly = true;
            DG_Metier.Width = 80;
            // 
            // DG_Ouvrier
            // 
            DG_Ouvrier.FillWeight = 80F;
            DG_Ouvrier.HeaderText = "Ouvrier affecté";
            DG_Ouvrier.MinimumWidth = 20;
            DG_Ouvrier.Name = "DG_Ouvrier";
            DG_Ouvrier.ReadOnly = true;
            DG_Ouvrier.Width = 80;
            // 
            // DG_Statut
            // 
            DG_Statut.FillWeight = 80F;
            DG_Statut.HeaderText = "Statut";
            DG_Statut.MinimumWidth = 20;
            DG_Statut.Name = "DG_Statut";
            DG_Statut.ReadOnly = true;
            DG_Statut.Width = 80;
            // 
            // DG_DDP
            // 
            dataGridViewCellStyle3.NullValue = null;
            DG_DDP.DefaultCellStyle = dataGridViewCellStyle3;
            DG_DDP.FillWeight = 90F;
            DG_DDP.HeaderText = "Début Planifié";
            DG_DDP.MinimumWidth = 120;
            DG_DDP.Name = "DG_DDP";
            DG_DDP.ReadOnly = true;
            DG_DDP.Width = 120;
            // 
            // DG_DFP
            // 
            DG_DFP.FillWeight = 90F;
            DG_DFP.HeaderText = "Fin Planifiée";
            DG_DFP.MinimumWidth = 120;
            DG_DFP.Name = "DG_DFP";
            DG_DFP.ReadOnly = true;
            DG_DFP.Width = 120;
            // 
            // DG_DDR
            // 
            DG_DDR.FillWeight = 90F;
            DG_DDR.HeaderText = "Début Réel";
            DG_DDR.MinimumWidth = 120;
            DG_DDR.Name = "DG_DDR";
            DG_DDR.Width = 120;
            // 
            // DG_DFR
            // 
            DG_DFR.FillWeight = 90F;
            DG_DFR.HeaderText = "Fin Réel";
            DG_DFR.MinimumWidth = 120;
            DG_DFR.Name = "DG_DFR";
            DG_DFR.Width = 120;
            // 
            // kryptonTableLayoutPanel1
            // 
            kryptonTableLayoutPanel1.AutoSize = true;
            kryptonTableLayoutPanel1.ColumnCount = 1;
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel1.Controls.Add(kpCentre, 0, 1);
            kryptonTableLayoutPanel1.Controls.Add(kpTop, 0, 0);
            kryptonTableLayoutPanel1.Controls.Add(kpBottom, 0, 2);
            kryptonTableLayoutPanel1.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel1.Location = new Point(0, 0);
            kryptonTableLayoutPanel1.Name = "kryptonTableLayoutPanel1";
            kryptonTableLayoutPanel1.RowCount = 3;
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            kryptonTableLayoutPanel1.Size = new Size(1656, 884);
            kryptonTableLayoutPanel1.TabIndex = 1;
            // 
            // kpTop
            // 
            kpTop.AutoSize = true;
            kpTop.Controls.Add(kryptonLabel4);
            kpTop.Controls.Add(kryptonLabel1);
            kpTop.Controls.Add(cmbFiltreLot);
            kpTop.Controls.Add(kryptonLabel2);
            kpTop.Controls.Add(cmbFiltreMetier);
            kpTop.Controls.Add(kryptonLabel3);
            kpTop.Controls.Add(cmbFiltreStatut);
            kpTop.Controls.Add(btnAControler);
            kpTop.Controls.Add(btnResetFiltres);
            kpTop.Dock = DockStyle.Fill;
            kpTop.Location = new Point(3, 3);
            kpTop.Name = "kpTop";
            kpTop.Size = new Size(1650, 44);
            kpTop.TabIndex = 1;
            // 
            // kryptonLabel4
            // 
            kryptonLabel4.LabelStyle = Krypton.Toolkit.LabelStyle.TitlePanel;
            kryptonLabel4.Location = new Point(0, 3);
            kryptonLabel4.Name = "kryptonLabel4";
            kryptonLabel4.Size = new Size(65, 29);
            kryptonLabel4.TabIndex = 10;
            kryptonLabel4.Values.Text = "Filtres";
            // 
            // kryptonLabel1
            // 
            kryptonLabel1.Location = new Point(106, 13);
            kryptonLabel1.Margin = new Padding(8);
            kryptonLabel1.Name = "kryptonLabel1";
            kryptonLabel1.Size = new Size(28, 20);
            kryptonLabel1.TabIndex = 0;
            kryptonLabel1.Values.Text = "Lot";
            // 
            // cmbFiltreLot
            // 
            cmbFiltreLot.DropDownWidth = 150;
            cmbFiltreLot.Location = new Point(141, 13);
            cmbFiltreLot.Margin = new Padding(8);
            cmbFiltreLot.Name = "cmbFiltreLot";
            cmbFiltreLot.Size = new Size(150, 22);
            cmbFiltreLot.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            cmbFiltreLot.TabIndex = 1;
            cmbFiltreLot.Text = "kryptonComboBox1";
            // 
            // kryptonLabel2
            // 
            kryptonLabel2.Location = new Point(307, 15);
            kryptonLabel2.Margin = new Padding(8);
            kryptonLabel2.Name = "kryptonLabel2";
            kryptonLabel2.Size = new Size(46, 20);
            kryptonLabel2.TabIndex = 3;
            kryptonLabel2.Values.Text = "Métier";
            // 
            // cmbFiltreMetier
            // 
            cmbFiltreMetier.DropDownWidth = 120;
            cmbFiltreMetier.Location = new Point(352, 13);
            cmbFiltreMetier.Margin = new Padding(8);
            cmbFiltreMetier.Name = "cmbFiltreMetier";
            cmbFiltreMetier.Size = new Size(120, 22);
            cmbFiltreMetier.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            cmbFiltreMetier.TabIndex = 4;
            cmbFiltreMetier.Text = "kryptonComboBox1";
            // 
            // kryptonLabel3
            // 
            kryptonLabel3.Location = new Point(483, 13);
            kryptonLabel3.Name = "kryptonLabel3";
            kryptonLabel3.Size = new Size(43, 20);
            kryptonLabel3.TabIndex = 7;
            kryptonLabel3.Values.Text = "Statut ";
            // 
            // cmbFiltreStatut
            // 
            cmbFiltreStatut.DropDownWidth = 120;
            cmbFiltreStatut.Location = new Point(532, 13);
            cmbFiltreStatut.Name = "cmbFiltreStatut";
            cmbFiltreStatut.Size = new Size(120, 22);
            cmbFiltreStatut.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            cmbFiltreStatut.TabIndex = 8;
            cmbFiltreStatut.Text = "kryptonComboBox1";
            // 
            // btnAControler
            // 
            btnAControler.Location = new Point(672, 13);
            btnAControler.Name = "btnAControler";
            btnAControler.Size = new Size(90, 25);
            btnAControler.TabIndex = 9;
            btnAControler.Values.DropDownArrowColor = Color.Empty;
            btnAControler.Values.Text = "A controler";
            // 
            // btnResetFiltres
            // 
            btnResetFiltres.Location = new Point(783, 13);
            btnResetFiltres.Margin = new Padding(8);
            btnResetFiltres.Name = "btnResetFiltres";
            btnResetFiltres.Size = new Size(90, 25);
            btnResetFiltres.TabIndex = 6;
            btnResetFiltres.Values.DropDownArrowColor = Color.Empty;
            btnResetFiltres.Values.Text = "Reset";
            // 
            // kpBottom
            // 
            kpBottom.Dock = DockStyle.Fill;
            kpBottom.Location = new Point(3, 837);
            kpBottom.Name = "kpBottom";
            kpBottom.Size = new Size(1650, 44);
            kpBottom.TabIndex = 2;
            // 
            // TaskListView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonTableLayoutPanel1);
            Name = "TaskListView";
            Size = new Size(1656, 884);
            ((System.ComponentModel.ISupportInitialize)kpCentre).EndInit();
            kpCentre.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonDataGridView1).EndInit();
            kryptonTableLayoutPanel1.ResumeLayout(false);
            kryptonTableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kpTop).EndInit();
            kpTop.ResumeLayout(false);
            kpTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbFiltreLot).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbFiltreMetier).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbFiltreStatut).EndInit();
            ((System.ComponentModel.ISupportInitialize)kpBottom).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // --- SUPPRESSION des champs inutiles ---
        // private TacheDetailViewXL tacheDetailViewXL1;
        // private SplitContainer splitContainer1;

        private Krypton.Toolkit.KryptonPanel kpCentre;
        private Krypton.Toolkit.KryptonDataGridView kryptonDataGridView1;
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel1;
        private Krypton.Toolkit.KryptonPanel kpTop;
        private Krypton.Toolkit.KryptonPanel kpBottom;
        private Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private Krypton.Toolkit.KryptonComboBox cmbFiltreLot;
        private Krypton.Toolkit.KryptonLabel kryptonLabel2;
        private Krypton.Toolkit.KryptonComboBox cmbFiltreMetier;
        private Krypton.Toolkit.KryptonButton btnResetFiltres;
        private Krypton.Toolkit.KryptonLabel kryptonLabel3;
        private Krypton.Toolkit.KryptonComboBox cmbFiltreStatut;
        private Krypton.Toolkit.KryptonButton btnAControler;
        private Krypton.Toolkit.KryptonLabel kryptonLabel4;
        private DataGridViewTextBoxColumn DG_TacheId;
        private DataGridViewTextBoxColumn DG_Tache;
        private DataGridViewTextBoxColumn DG_Metier;
        private DataGridViewTextBoxColumn DG_Ouvrier;
        private DataGridViewTextBoxColumn DG_Statut;
        private DataGridViewTextBoxColumn DG_DDP;
        private DataGridViewTextBoxColumn DG_DFP;
        private DataGridViewTextBoxColumn DG_DDR;
        private DataGridViewTextBoxColumn DG_DFR;
    }
}