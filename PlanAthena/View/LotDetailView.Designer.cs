using Krypton.Toolkit;

namespace PlanAthena.View
{
    partial class LotDetailView
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
            groupDetails = new KryptonGroupBox();
            previewPlan = new PictureBox();
            cmbPhases = new KryptonComboBox();
            labelPhase = new KryptonLabel();
            btnBrowsePlan = new KryptonButton();
            textPlanPath = new KryptonTextBox();
            labelPlanPath = new KryptonLabel();
            numPriority = new KryptonNumericUpDown();
            labelPriority = new KryptonLabel();
            textName = new KryptonTextBox();
            labelName = new KryptonLabel();
            textId = new KryptonTextBox();
            labelId = new KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)groupDetails).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).BeginInit();
            groupDetails.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)previewPlan).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbPhases).BeginInit();
            SuspendLayout();
            // 
            // groupDetails
            // 
            groupDetails.Dock = DockStyle.Fill;
            groupDetails.Location = new Point(0, 0);
            groupDetails.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupDetails.Panel.Controls.Add(previewPlan);
            groupDetails.Panel.Controls.Add(cmbPhases);
            groupDetails.Panel.Controls.Add(labelPhase);
            groupDetails.Panel.Controls.Add(btnBrowsePlan);
            groupDetails.Panel.Controls.Add(textPlanPath);
            groupDetails.Panel.Controls.Add(labelPlanPath);
            groupDetails.Panel.Controls.Add(numPriority);
            groupDetails.Panel.Controls.Add(labelPriority);
            groupDetails.Panel.Controls.Add(textName);
            groupDetails.Panel.Controls.Add(labelName);
            groupDetails.Panel.Controls.Add(textId);
            groupDetails.Panel.Controls.Add(labelId);
            groupDetails.Size = new Size(708, 577);
            groupDetails.TabIndex = 0;
            groupDetails.Values.Heading = "Édition du Lot";
            // 
            // previewPlan
            // 
            previewPlan.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            previewPlan.BackColor = SystemColors.ControlDark;
            previewPlan.BorderStyle = BorderStyle.FixedSingle;
            previewPlan.Location = new Point(18, 202);
            previewPlan.Margin = new Padding(4, 3, 4, 3);
            previewPlan.Name = "previewPlan";
            previewPlan.Size = new Size(669, 345);
            previewPlan.SizeMode = PictureBoxSizeMode.Zoom;
            previewPlan.TabIndex = 11;
            previewPlan.TabStop = false;
            // 
            // cmbPhases
            // 
            cmbPhases.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPhases.DropDownWidth = 121;
            cmbPhases.IntegralHeight = false;
            cmbPhases.Location = new Point(482, 115);
            cmbPhases.Margin = new Padding(4, 3, 4, 3);
            cmbPhases.Name = "cmbPhases";
            cmbPhases.Size = new Size(191, 22);
            cmbPhases.TabIndex = 10;
            // 
            // labelPhase
            // 
            labelPhase.Location = new Point(420, 115);
            labelPhase.Margin = new Padding(4, 3, 4, 3);
            labelPhase.Name = "labelPhase";
            labelPhase.Size = new Size(48, 20);
            labelPhase.TabIndex = 9;
            labelPhase.Values.Text = "Phase :";
            // 
            // btnBrowsePlan
            // 
            btnBrowsePlan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowsePlan.Location = new Point(582, 155);
            btnBrowsePlan.Margin = new Padding(4, 3, 4, 3);
            btnBrowsePlan.Name = "btnBrowsePlan";
            btnBrowsePlan.Size = new Size(105, 29);
            btnBrowsePlan.TabIndex = 8;
            btnBrowsePlan.Values.DropDownArrowColor = Color.Empty;
            btnBrowsePlan.Values.Text = "Parcourir...";
            btnBrowsePlan.Click += btnBrowsePlan_Click;
            // 
            // textPlanPath
            // 
            textPlanPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textPlanPath.Location = new Point(152, 156);
            textPlanPath.Margin = new Padding(4, 3, 4, 3);
            textPlanPath.Name = "textPlanPath";
            textPlanPath.Size = new Size(423, 23);
            textPlanPath.TabIndex = 7;
            // 
            // labelPlanPath
            // 
            labelPlanPath.Location = new Point(18, 156);
            labelPlanPath.Margin = new Padding(4, 3, 4, 3);
            labelPlanPath.Name = "labelPlanPath";
            labelPlanPath.Size = new Size(40, 20);
            labelPlanPath.TabIndex = 6;
            labelPlanPath.Values.Text = "Plan :";
            // 
            // numPriority
            // 
            numPriority.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numPriority.Location = new Point(152, 115);
            numPriority.Margin = new Padding(4, 3, 4, 3);
            numPriority.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numPriority.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPriority.Name = "numPriority";
            numPriority.Size = new Size(140, 22);
            numPriority.TabIndex = 5;
            numPriority.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // labelPriority
            // 
            labelPriority.Location = new Point(18, 115);
            labelPriority.Margin = new Padding(4, 3, 4, 3);
            labelPriority.Name = "labelPriority";
            labelPriority.Size = new Size(56, 20);
            labelPriority.TabIndex = 4;
            labelPriority.Values.Text = "Priorité :";
            // 
            // textName
            // 
            textName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textName.Location = new Point(152, 75);
            textName.Margin = new Padding(4, 3, 4, 3);
            textName.Name = "textName";
            textName.Size = new Size(535, 23);
            textName.TabIndex = 3;
            // 
            // labelName
            // 
            labelName.Location = new Point(18, 75);
            labelName.Margin = new Padding(4, 3, 4, 3);
            labelName.Name = "labelName";
            labelName.Size = new Size(44, 20);
            labelName.TabIndex = 2;
            labelName.Values.Text = "Nom :";
            // 
            // textId
            // 
            textId.Location = new Point(152, 35);
            textId.Margin = new Padding(4, 3, 4, 3);
            textId.Name = "textId";
            textId.ReadOnly = true;
            textId.Size = new Size(233, 23);
            textId.TabIndex = 1;
            // 
            // labelId
            // 
            labelId.Location = new Point(18, 35);
            labelId.Margin = new Padding(4, 3, 4, 3);
            labelId.Name = "labelId";
            labelId.Size = new Size(29, 20);
            labelId.TabIndex = 0;
            labelId.Values.Text = "ID :";
            // 
            // LotDetailView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupDetails);
            Margin = new Padding(4, 3, 4, 3);
            Name = "LotDetailView";
            Size = new Size(708, 577);
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).EndInit();
            groupDetails.Panel.ResumeLayout(false);
            groupDetails.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupDetails).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewPlan).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbPhases).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private KryptonGroupBox groupDetails;
        private KryptonTextBox textId;
        private KryptonLabel labelId;
        private KryptonTextBox textName;
        private KryptonLabel labelName;
        private KryptonNumericUpDown numPriority;
        private KryptonLabel labelPriority;
        private KryptonButton btnBrowsePlan;
        private KryptonTextBox textPlanPath;
        private KryptonLabel labelPlanPath;
        private KryptonComboBox cmbPhases;
        private KryptonLabel labelPhase;
        private System.Windows.Forms.PictureBox previewPlan;
    }
}