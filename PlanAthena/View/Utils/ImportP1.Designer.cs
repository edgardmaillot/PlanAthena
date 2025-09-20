namespace PlanAthena.View.Utils
{
    partial class ImportP1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportP1));
            kryptonTableLayoutPanel1 = new Krypton.Toolkit.KryptonTableLayoutPanel();
            kryptonTableLayoutPanel2 = new Krypton.Toolkit.KryptonTableLayoutPanel();
            khTitre = new Krypton.Toolkit.KryptonHeader();
            kDataGrid_P1 = new Krypton.Toolkit.KryptonDataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            kryptonPanel2 = new Krypton.Toolkit.KryptonPanel();
            kryptonTableLayoutPanel4 = new Krypton.Toolkit.KryptonTableLayoutPanel();
            kryptonPanel3 = new Krypton.Toolkit.KryptonPanel();
            kBtAnnule = new Krypton.Toolkit.KryptonButton();
            kBtSuivant = new Krypton.Toolkit.KryptonButton();
            kryptonTableLayoutPanel3 = new Krypton.Toolkit.KryptonTableLayoutPanel();
            kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
            kryptonPanel4 = new Krypton.Toolkit.KryptonPanel();
            kWrapLab2 = new Krypton.Toolkit.KryptonWrapLabel();
            kcbEntete = new Krypton.Toolkit.KryptonCheckBox();
            kWrapLab1 = new Krypton.Toolkit.KryptonWrapLabel();
            mem1 = new Krypton.Toolkit.KryptonCheckBox();
            khInstructions = new Krypton.Toolkit.KryptonHeaderGroup();
            kryptonTableLayoutPanel1.SuspendLayout();
            kryptonTableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kDataGrid_P1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel2).BeginInit();
            kryptonPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel3).BeginInit();
            kryptonPanel3.SuspendLayout();
            kryptonTableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel4).BeginInit();
            kryptonPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)khInstructions).BeginInit();
            ((System.ComponentModel.ISupportInitialize)khInstructions.Panel).BeginInit();
            SuspendLayout();
            // 
            // kryptonTableLayoutPanel1
            // 
            kryptonTableLayoutPanel1.ColumnCount = 2;
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel1.Controls.Add(kryptonTableLayoutPanel2, 1, 0);
            kryptonTableLayoutPanel1.Controls.Add(kryptonTableLayoutPanel3, 0, 0);
            kryptonTableLayoutPanel1.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel1.Location = new Point(0, 0);
            kryptonTableLayoutPanel1.Name = "kryptonTableLayoutPanel1";
            kryptonTableLayoutPanel1.RowCount = 1;
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.Size = new Size(1020, 600);
            kryptonTableLayoutPanel1.TabIndex = 1;
            // 
            // kryptonTableLayoutPanel2
            // 
            kryptonTableLayoutPanel2.ColumnCount = 1;
            kryptonTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel2.Controls.Add(khTitre, 0, 0);
            kryptonTableLayoutPanel2.Controls.Add(kDataGrid_P1, 0, 1);
            kryptonTableLayoutPanel2.Controls.Add(kryptonPanel2, 0, 2);
            kryptonTableLayoutPanel2.Controls.Add(kryptonPanel3, 0, 3);
            kryptonTableLayoutPanel2.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel2.Location = new Point(243, 3);
            kryptonTableLayoutPanel2.Name = "kryptonTableLayoutPanel2";
            kryptonTableLayoutPanel2.RowCount = 4;
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            kryptonTableLayoutPanel2.Size = new Size(774, 594);
            kryptonTableLayoutPanel2.TabIndex = 1;
            // 
            // khTitre
            // 
            khTitre.Dock = DockStyle.Fill;
            khTitre.Location = new Point(3, 3);
            khTitre.Name = "khTitre";
            khTitre.Size = new Size(768, 36);
            khTitre.TabIndex = 0;
            khTitre.Values.Description = "Etape 1/3 Sélectionner des colonnes";
            khTitre.Values.Heading = "Importer des {Name}";
            khTitre.Values.Image = null;
            // 
            // kDataGrid_P1
            // 
            kDataGrid_P1.AllowUserToAddRows = false;
            kDataGrid_P1.AllowUserToDeleteRows = false;
            kDataGrid_P1.BorderStyle = BorderStyle.None;
            kDataGrid_P1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            kDataGrid_P1.Columns.AddRange(new DataGridViewColumn[] { Column1 });
            kDataGrid_P1.Dock = DockStyle.Fill;
            kDataGrid_P1.Location = new Point(3, 45);
            kDataGrid_P1.Name = "kDataGrid_P1";
            kDataGrid_P1.ReadOnly = true;
            kDataGrid_P1.Size = new Size(768, 401);
            kDataGrid_P1.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
            kDataGrid_P1.StateCommon.HeaderColumn.Content.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            kDataGrid_P1.StateCommon.HeaderColumn.Content.Hint = Krypton.Toolkit.PaletteTextHint.AntiAlias;
            kDataGrid_P1.TabIndex = 1;
            // 
            // Column1
            // 
            Column1.HeaderText = "Column1";
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            // 
            // kryptonPanel2
            // 
            kryptonPanel2.Controls.Add(kryptonTableLayoutPanel4);
            kryptonPanel2.Dock = DockStyle.Fill;
            kryptonPanel2.Location = new Point(3, 452);
            kryptonPanel2.Name = "kryptonPanel2";
            kryptonPanel2.Size = new Size(768, 94);
            kryptonPanel2.TabIndex = 2;
            // 
            // kryptonTableLayoutPanel4
            // 
            kryptonTableLayoutPanel4.ColumnCount = 1;
            kryptonTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel4.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel4.Location = new Point(0, 0);
            kryptonTableLayoutPanel4.Name = "kryptonTableLayoutPanel4";
            kryptonTableLayoutPanel4.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundSheet;
            kryptonTableLayoutPanel4.RowCount = 1;
            kryptonTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            kryptonTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel4.Size = new Size(768, 94);
            kryptonTableLayoutPanel4.TabIndex = 0;
            // 
            // kryptonPanel3
            // 
            kryptonPanel3.Controls.Add(kBtAnnule);
            kryptonPanel3.Controls.Add(kBtSuivant);
            kryptonPanel3.Dock = DockStyle.Fill;
            kryptonPanel3.Location = new Point(3, 552);
            kryptonPanel3.Name = "kryptonPanel3";
            kryptonPanel3.Size = new Size(768, 39);
            kryptonPanel3.TabIndex = 3;
            // 
            // kBtAnnule
            // 
            kBtAnnule.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            kBtAnnule.Location = new Point(579, 5);
            kBtAnnule.Name = "kBtAnnule";
            kBtAnnule.Size = new Size(90, 25);
            kBtAnnule.TabIndex = 1;
            kBtAnnule.Values.DropDownArrowColor = Color.Empty;
            kBtAnnule.Values.Text = "Annuler";
            // 
            // kBtSuivant
            // 
            kBtSuivant.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            kBtSuivant.Location = new Point(675, 5);
            kBtSuivant.Name = "kBtSuivant";
            kBtSuivant.Size = new Size(90, 25);
            kBtSuivant.TabIndex = 0;
            kBtSuivant.Values.DropDownArrowColor = Color.Empty;
            kBtSuivant.Values.Text = "Suivant";
            // 
            // kryptonTableLayoutPanel3
            // 
            kryptonTableLayoutPanel3.ColumnCount = 1;
            kryptonTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            kryptonTableLayoutPanel3.Controls.Add(kryptonPanel1, 0, 0);
            kryptonTableLayoutPanel3.Controls.Add(kryptonPanel4, 0, 1);
            kryptonTableLayoutPanel3.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel3.Location = new Point(3, 3);
            kryptonTableLayoutPanel3.Name = "kryptonTableLayoutPanel3";
            kryptonTableLayoutPanel3.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundSheet;
            kryptonTableLayoutPanel3.RowCount = 2;
            kryptonTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.6700325F));
            kryptonTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 66.32996F));
            kryptonTableLayoutPanel3.Size = new Size(234, 594);
            kryptonTableLayoutPanel3.TabIndex = 2;
            // 
            // kryptonPanel1
            // 
            kryptonPanel1.Location = new Point(3, 3);
            kryptonPanel1.Name = "kryptonPanel1";
            kryptonPanel1.Size = new Size(228, 180);
            kryptonPanel1.StateNormal.Image = Properties.Resources.Import_Task;
            kryptonPanel1.StateNormal.ImageStyle = Krypton.Toolkit.PaletteImageStyle.Stretch;
            kryptonPanel1.TabIndex = 0;
            // 
            // kryptonPanel4
            // 
            kryptonPanel4.Controls.Add(kWrapLab2);
            kryptonPanel4.Controls.Add(kcbEntete);
            kryptonPanel4.Controls.Add(kWrapLab1);
            kryptonPanel4.Controls.Add(mem1);
            kryptonPanel4.Controls.Add(khInstructions);
            kryptonPanel4.Dock = DockStyle.Fill;
            kryptonPanel4.Location = new Point(12, 212);
            kryptonPanel4.Margin = new Padding(12);
            kryptonPanel4.Name = "kryptonPanel4";
            kryptonPanel4.Size = new Size(210, 370);
            kryptonPanel4.TabIndex = 1;
            // 
            // kWrapLab2
            // 
            kWrapLab2.LabelStyle = Krypton.Toolkit.LabelStyle.AlternateControl;
            kWrapLab2.Location = new Point(5, 91);
            kWrapLab2.Name = "kWrapLab2";
            kWrapLab2.Size = new Size(202, 105);
            kWrapLab2.Text = resources.GetString("kWrapLab2.Text");
            // 
            // kcbEntete
            // 
            kcbEntete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            kcbEntete.Checked = true;
            kcbEntete.CheckState = CheckState.Checked;
            kcbEntete.Location = new Point(178, 65);
            kcbEntete.Name = "kcbEntete";
            kcbEntete.Size = new Size(19, 13);
            kcbEntete.TabIndex = 3;
            kcbEntete.Values.Text = "";
            // 
            // kWrapLab1
            // 
            kWrapLab1.LabelStyle = Krypton.Toolkit.LabelStyle.AlternateControl;
            kWrapLab1.Location = new Point(5, 33);
            kWrapLab1.Name = "kWrapLab1";
            kWrapLab1.Size = new Size(192, 45);
            kWrapLab1.Text = "Si la première ligne de votre fichier \r\ncontient des entêtes, \r\nlaissez cette case cochée";
            // 
            // mem1
            // 
            mem1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            mem1.Checked = true;
            mem1.CheckState = CheckState.Checked;
            mem1.Location = new Point(67, 320);
            mem1.Name = "mem1";
            mem1.RightToLeft = RightToLeft.Yes;
            mem1.Size = new Size(140, 20);
            mem1.TabIndex = 2;
            mem1.Values.Text = "Mémoriser mes choix";
            // 
            // khInstructions
            // 
            khInstructions.Dock = DockStyle.Fill;
            khInstructions.Location = new Point(0, 0);
            khInstructions.Size = new Size(210, 370);
            khInstructions.TabIndex = 0;
            khInstructions.ValuesPrimary.Description = "?";
            khInstructions.ValuesPrimary.Heading = "Instructions";
            khInstructions.ValuesPrimary.Image = Properties.Resources.ic_Instructions;
            khInstructions.ValuesSecondary.Heading = " . . .";
            // 
            // ImportP1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonTableLayoutPanel1);
            Name = "ImportP1";
            Size = new Size(1020, 600);
            kryptonTableLayoutPanel1.ResumeLayout(false);
            kryptonTableLayoutPanel2.ResumeLayout(false);
            kryptonTableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kDataGrid_P1).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel2).EndInit();
            kryptonPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonPanel3).EndInit();
            kryptonPanel3.ResumeLayout(false);
            kryptonTableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel4).EndInit();
            kryptonPanel4.ResumeLayout(false);
            kryptonPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)khInstructions.Panel).EndInit();
            ((System.ComponentModel.ISupportInitialize)khInstructions).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel1;
        private Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel2;
        private Krypton.Toolkit.KryptonHeader khTitre;
        private Krypton.Toolkit.KryptonDataGridView kDataGrid_P1;
        private DataGridViewTextBoxColumn Column1;
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel3;
        private Krypton.Toolkit.KryptonPanel kryptonPanel2;
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel4;
        private Krypton.Toolkit.KryptonPanel kryptonPanel3;
        private Krypton.Toolkit.KryptonPanel kryptonPanel4;
        private Krypton.Toolkit.KryptonHeaderGroup khInstructions;
        private Krypton.Toolkit.KryptonCheckBox mem1;
        private Krypton.Toolkit.KryptonButton kBtAnnule;
        private Krypton.Toolkit.KryptonButton kBtSuivant;
        private Krypton.Toolkit.KryptonCheckBox kcbEntete;
        private Krypton.Toolkit.KryptonWrapLabel kWrapLab1;
        private Krypton.Toolkit.KryptonWrapLabel kWrapLab2;
    }
}
