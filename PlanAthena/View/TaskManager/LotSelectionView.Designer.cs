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
            groupLotActif.Size = new Size(280, 231);
            groupLotActif.TabIndex = 0;
            groupLotActif.Values.Heading = "";
            // 
            // previewPlan
            // 
            previewPlan.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            previewPlan.BackColor = SystemColors.ControlDark;
            previewPlan.BorderStyle = BorderStyle.FixedSingle;
            previewPlan.Cursor = Cursors.Hand;
            previewPlan.Location = new Point(16, 26);
            previewPlan.Margin = new Padding(4, 3, 4, 3);
            previewPlan.Name = "previewPlan";
            previewPlan.Size = new Size(244, 171);
            previewPlan.SizeMode = PictureBoxSizeMode.Zoom;
            previewPlan.TabIndex = 2;
            previewPlan.TabStop = false;
            previewPlan.Click += previewPlan_Click;
            // 
            // cmbLots
            // 
            cmbLots.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLots.DropDownWidth = 198;
            cmbLots.IntegralHeight = false;
            cmbLots.Location = new Point(16, -2);
            cmbLots.Margin = new Padding(4, 3, 4, 3);
            cmbLots.Name = "cmbLots";
            cmbLots.Size = new Size(245, 22);
            cmbLots.TabIndex = 1;
            cmbLots.SelectedIndexChanged += cmbLots_SelectedIndexChanged;
            // 
            // LotSelectionView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupLotActif);
            Margin = new Padding(4, 3, 4, 3);
            Name = "LotSelectionView";
            Size = new Size(280, 231);
            ((System.ComponentModel.ISupportInitialize)groupLotActif.Panel).EndInit();
            groupLotActif.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupLotActif).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewPlan).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbLots).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private KryptonGroupBox groupLotActif;
        private KryptonComboBox cmbLots;
        private PictureBox previewPlan;
    }
}