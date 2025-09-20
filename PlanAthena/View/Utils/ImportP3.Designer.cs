namespace PlanAthena.View.Utils
{
    partial class ImportP3
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportP3));
            kryptonTableLayoutPanel1 = new Krypton.Toolkit.KryptonTableLayoutPanel();
            kryptonTableLayoutPanel2 = new Krypton.Toolkit.KryptonTableLayoutPanel();
            khTitre = new Krypton.Toolkit.KryptonHeader();
            kDataGrid_P3 = new Krypton.Toolkit.KryptonDataGridView();
            kryptonPanel3 = new Krypton.Toolkit.KryptonPanel();
            kBtRetour = new Krypton.Toolkit.KryptonButton();
            kBtValider = new Krypton.Toolkit.KryptonButton();
            kRichTxtRejet = new Krypton.Toolkit.KryptonRichTextBox();
            kryptonTableLayoutPanel3 = new Krypton.Toolkit.KryptonTableLayoutPanel();
            kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
            kryptonPanel4 = new Krypton.Toolkit.KryptonPanel();
            khInstructions = new Krypton.Toolkit.KryptonHeaderGroup();
            kryptonTableLayoutPanel1.SuspendLayout();
            kryptonTableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kDataGrid_P3).BeginInit();
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
            kryptonTableLayoutPanel2.Controls.Add(kDataGrid_P3, 0, 1);
            kryptonTableLayoutPanel2.Controls.Add(kryptonPanel3, 0, 3);
            kryptonTableLayoutPanel2.Controls.Add(kRichTxtRejet, 0, 2);
            kryptonTableLayoutPanel2.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel2.Location = new Point(243, 3);
            kryptonTableLayoutPanel2.Name = "kryptonTableLayoutPanel2";
            kryptonTableLayoutPanel2.RowCount = 4;
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
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
            khTitre.Values.Description = "Etape 3/3 Valider les données";
            khTitre.Values.Heading = "Importer des {Name}";
            khTitre.Values.Image = null;
            // 
            // kDataGrid_P3
            // 
            kDataGrid_P3.AllowUserToOrderColumns = true;
            kDataGrid_P3.BorderStyle = BorderStyle.None;
            kDataGrid_P3.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            kDataGrid_P3.Dock = DockStyle.Fill;
            kDataGrid_P3.Location = new Point(3, 45);
            kDataGrid_P3.Name = "kDataGrid_P3";
            kDataGrid_P3.Size = new Size(768, 381);
            kDataGrid_P3.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
            kDataGrid_P3.StateCommon.HeaderColumn.Content.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            kDataGrid_P3.StateCommon.HeaderColumn.Content.Hint = Krypton.Toolkit.PaletteTextHint.AntiAlias;
            kDataGrid_P3.TabIndex = 1;
            // 
            // kryptonPanel3
            // 
            kryptonPanel3.Controls.Add(kBtRetour);
            kryptonPanel3.Controls.Add(kBtValider);
            kryptonPanel3.Dock = DockStyle.Fill;
            kryptonPanel3.Location = new Point(3, 552);
            kryptonPanel3.Name = "kryptonPanel3";
            kryptonPanel3.Size = new Size(768, 39);
            kryptonPanel3.TabIndex = 3;
            // 
            // kBtRetour
            // 
            kBtRetour.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            kBtRetour.Location = new Point(579, 5);
            kBtRetour.Name = "kBtRetour";
            kBtRetour.Size = new Size(90, 25);
            kBtRetour.TabIndex = 1;
            kBtRetour.Values.DropDownArrowColor = Color.Empty;
            kBtRetour.Values.Text = "Retour";
            // 
            // kBtValider
            // 
            kBtValider.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            kBtValider.Location = new Point(675, 5);
            kBtValider.Name = "kBtValider";
            kBtValider.Size = new Size(90, 25);
            kBtValider.TabIndex = 0;
            kBtValider.Values.DropDownArrowColor = Color.Empty;
            kBtValider.Values.Text = "Valider";
            // 
            // kRichTxtRejet
            // 
            kRichTxtRejet.Dock = DockStyle.Fill;
            kRichTxtRejet.Location = new Point(3, 432);
            kRichTxtRejet.Name = "kRichTxtRejet";
            kRichTxtRejet.Size = new Size(768, 114);
            kRichTxtRejet.TabIndex = 4;
            kRichTxtRejet.Text = "Données non importées :";
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
            kryptonTableLayoutPanel3.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.GridHeaderRowList;
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
            kryptonPanel1.StateNormal.Image = (Image)resources.GetObject("kryptonPanel1.StateNormal.Image");
            kryptonPanel1.StateNormal.ImageStyle = Krypton.Toolkit.PaletteImageStyle.Stretch;
            kryptonPanel1.TabIndex = 0;
            // 
            // kryptonPanel4
            // 
            kryptonPanel4.Controls.Add(khInstructions);
            kryptonPanel4.Dock = DockStyle.Fill;
            kryptonPanel4.Location = new Point(12, 212);
            kryptonPanel4.Margin = new Padding(12);
            kryptonPanel4.Name = "kryptonPanel4";
            kryptonPanel4.Size = new Size(210, 370);
            kryptonPanel4.TabIndex = 1;
            // 
            // khInstructions
            // 
            khInstructions.HeaderStylePrimary = Krypton.Toolkit.HeaderStyle.DockActive;
            khInstructions.HeaderStyleSecondary = Krypton.Toolkit.HeaderStyle.DockActive;
            khInstructions.Location = new Point(0, 0);
            khInstructions.Size = new Size(210, 367);
            khInstructions.TabIndex = 0;
            khInstructions.ValuesPrimary.Description = "?";
            khInstructions.ValuesPrimary.Heading = "Instructions";
            khInstructions.ValuesPrimary.Image = Properties.Resources.ic_Instructions;
            khInstructions.ValuesSecondary.Heading = " . . .";
            // 
            // ImportP3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonTableLayoutPanel1);
            Name = "ImportP3";
            Size = new Size(1020, 600);
            kryptonTableLayoutPanel1.ResumeLayout(false);
            kryptonTableLayoutPanel2.ResumeLayout(false);
            kryptonTableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kDataGrid_P3).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel3).EndInit();
            kryptonPanel3.ResumeLayout(false);
            kryptonTableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel4).EndInit();
            kryptonPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)khInstructions.Panel).EndInit();
            ((System.ComponentModel.ISupportInitialize)khInstructions).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel1;
        private Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel2;
        private Krypton.Toolkit.KryptonHeader khTitre;
        private Krypton.Toolkit.KryptonDataGridView kDataGrid_P3;
        private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel3;
        private Krypton.Toolkit.KryptonPanel kryptonPanel4;
        private Krypton.Toolkit.KryptonHeaderGroup khInstructions;
        private Krypton.Toolkit.KryptonPanel kryptonPanel3;
        private Krypton.Toolkit.KryptonButton kBtRetour;
        private Krypton.Toolkit.KryptonButton kBtValider;
        private Krypton.Toolkit.KryptonRichTextBox kRichTxtRejet;

    }
}
