using Krypton.Toolkit;

namespace PlanAthena.View.TaskManager
{
    partial class LotSelectionView
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
            groupLotActif = new KryptonGroupBox();
            previewPlan = new PictureBox();
            cmbLots = new KryptonComboBox();
            lblLotActif = new KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)groupLotActif).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupLotActif.Panel).BeginInit();
            groupLotActif.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)previewPlan).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbLots).BeginInit();
            SuspendLayout();
            // 
            // groupLotActif
            // 
            groupLotActif.CaptionOverlap = 0D;
            groupLotActif.Dock = DockStyle.Fill;
            groupLotActif.Location = new Point(0, 0);
            groupLotActif.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupLotActif.Panel.Controls.Add(previewPlan);
            groupLotActif.Panel.Controls.Add(cmbLots);
            groupLotActif.Panel.Controls.Add(lblLotActif);
            groupLotActif.Size = new Size(233, 231);
            groupLotActif.TabIndex = 0;
            groupLotActif.Values.Heading = "Sélection du Lot Actif";
            // 
            // previewPlan
            // 
            previewPlan.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            previewPlan.BackColor = SystemColors.ControlDark;
            previewPlan.BorderStyle = BorderStyle.FixedSingle;
            previewPlan.Cursor = Cursors.Hand;
            previewPlan.Location = new Point(16, 48);
            previewPlan.Margin = new Padding(4, 3, 4, 3);
            previewPlan.Name = "previewPlan";
            previewPlan.Size = new Size(197, 153);
            previewPlan.SizeMode = PictureBoxSizeMode.Zoom;
            previewPlan.TabIndex = 2;
            previewPlan.TabStop = false;
            previewPlan.Click += previewPlan_Click;
            // 
            // cmbLots
            // 
            cmbLots.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLots.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLots.DropDownWidth = 301;
            cmbLots.IntegralHeight = false;
            cmbLots.Location = new Point(96, 13);
            cmbLots.Margin = new Padding(4, 3, 4, 3);
            cmbLots.Name = "cmbLots";
            cmbLots.Size = new Size(118, 22);
            cmbLots.TabIndex = 1;
            cmbLots.SelectedIndexChanged += cmbLots_SelectedIndexChanged;
            // 
            // lblLotActif
            // 
            lblLotActif.Location = new Point(16, 13);
            lblLotActif.Margin = new Padding(4, 3, 4, 3);
            lblLotActif.Name = "lblLotActif";
            lblLotActif.Size = new Size(62, 20);
            lblLotActif.TabIndex = 0;
            lblLotActif.Values.Text = "Lot Actif :";
            // 
            // LotSelectionView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupLotActif);
            Margin = new Padding(4, 3, 4, 3);
            Name = "LotSelectionView";
            Size = new Size(233, 231);
            ((System.ComponentModel.ISupportInitialize)groupLotActif.Panel).EndInit();
            groupLotActif.Panel.ResumeLayout(false);
            groupLotActif.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupLotActif).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewPlan).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbLots).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private KryptonGroupBox groupLotActif;
        private KryptonLabel lblLotActif;
        private KryptonComboBox cmbLots;
        private System.Windows.Forms.PictureBox previewPlan;
    }
}