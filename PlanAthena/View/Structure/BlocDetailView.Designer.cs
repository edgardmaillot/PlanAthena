using Krypton.Toolkit;

namespace PlanAthena.View.Structure
{
    partial class BlocDetailView
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
            this.groupDetails = new Krypton.Toolkit.KryptonGroupBox();
            this.textLocationY = new Krypton.Toolkit.KryptonTextBox();
            this.labelLocationY = new Krypton.Toolkit.KryptonLabel();
            this.textLocationX = new Krypton.Toolkit.KryptonTextBox();
            this.labelLocationX = new Krypton.Toolkit.KryptonLabel();
            this.labelLocation = new Krypton.Toolkit.KryptonLabel();
            this.numCapacity = new Krypton.Toolkit.KryptonNumericUpDown();
            this.labelCapacity = new Krypton.Toolkit.KryptonLabel();
            this.textName = new Krypton.Toolkit.KryptonTextBox();
            this.labelName = new Krypton.Toolkit.KryptonLabel();
            this.textId = new Krypton.Toolkit.KryptonTextBox();
            this.labelId = new Krypton.Toolkit.KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)(this.groupDetails)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupDetails.Panel)).BeginInit();
            this.groupDetails.Panel.SuspendLayout();
            this.groupDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupDetails
            // 
            this.groupDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupDetails.Location = new System.Drawing.Point(0, 0);
            this.groupDetails.Name = "groupDetails";
            // 
            // groupDetails.Panel
            // 
            this.groupDetails.Panel.Controls.Add(this.textLocationY);
            this.groupDetails.Panel.Controls.Add(this.labelLocationY);
            this.groupDetails.Panel.Controls.Add(this.textLocationX);
            this.groupDetails.Panel.Controls.Add(this.labelLocationX);
            this.groupDetails.Panel.Controls.Add(this.labelLocation);
            this.groupDetails.Panel.Controls.Add(this.numCapacity);
            this.groupDetails.Panel.Controls.Add(this.labelCapacity);
            this.groupDetails.Panel.Controls.Add(this.textName);
            this.groupDetails.Panel.Controls.Add(this.labelName);
            this.groupDetails.Panel.Controls.Add(this.textId);
            this.groupDetails.Panel.Controls.Add(this.labelId);
            this.groupDetails.Size = new System.Drawing.Size(700, 500);
            this.groupDetails.TabIndex = 1;
            this.groupDetails.Values.Heading = "Édition du Bloc";
            // 
            // textLocationY
            // 
            this.textLocationY.Enabled = false;
            this.textLocationY.Location = new System.Drawing.Point(264, 135);
            this.textLocationY.Name = "textLocationY";
            this.textLocationY.Size = new System.Drawing.Size(66, 23);
            this.textLocationY.TabIndex = 10;
            // 
            // labelLocationY
            // 
            this.labelLocationY.Enabled = false;
            this.labelLocationY.Location = new System.Drawing.Point(240, 135);
            this.labelLocationY.Name = "labelLocationY";
            this.labelLocationY.Size = new System.Drawing.Size(22, 20);
            this.labelLocationY.TabIndex = 9;
            this.labelLocationY.Values.Text = "Y :";
            // 
            // textLocationX
            // 
            this.textLocationX.Enabled = false;
            this.textLocationX.Location = new System.Drawing.Point(168, 135);
            this.textLocationX.Name = "textLocationX";
            this.textLocationX.Size = new System.Drawing.Size(66, 23);
            this.textLocationX.TabIndex = 8;
            // 
            // labelLocationX
            // 
            this.labelLocationX.Enabled = false;
            this.labelLocationX.Location = new System.Drawing.Point(144, 135);
            this.labelLocationX.Name = "labelLocationX";
            this.labelLocationX.Size = new System.Drawing.Size(22, 20);
            this.labelLocationX.TabIndex = 7;
            this.labelLocationX.Values.Text = "X :";
            // 
            // labelLocation
            // 
            this.labelLocation.Enabled = false;
            this.labelLocation.Location = new System.Drawing.Point(15, 135);
            this.labelLocation.Name = "labelLocation";
            this.labelLocation.Size = new System.Drawing.Size(127, 20);
            this.labelLocation.TabIndex = 6;
            this.labelLocation.Values.Text = "Emplacement / Plan :";
            // 
            // numCapacity
            // 
            this.numCapacity.Location = new System.Drawing.Point(130, 100);
            this.numCapacity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCapacity.Name = "numCapacity";
            this.numCapacity.Size = new System.Drawing.Size(120, 22);
            this.numCapacity.TabIndex = 5;
            this.numCapacity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // labelCapacity
            // 
            this.labelCapacity.Location = new System.Drawing.Point(15, 100);
            this.labelCapacity.Name = "labelCapacity";
            this.labelCapacity.Size = new System.Drawing.Size(111, 20);
            this.labelCapacity.TabIndex = 4;
            this.labelCapacity.Values.Text = "Capacité Ouvriers :";
            // 
            // textName
            // 
            this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textName.Location = new System.Drawing.Point(130, 65);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(551, 23);
            this.textName.TabIndex = 3;
            // 
            // labelName
            // 
            this.labelName.Location = new System.Drawing.Point(15, 65);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(44, 20);
            this.labelName.TabIndex = 2;
            this.labelName.Values.Text = "Nom :";
            // 
            // textId
            // 
            this.textId.Location = new System.Drawing.Point(130, 30);
            this.textId.Name = "textId";
            this.textId.ReadOnly = true;
            this.textId.Size = new System.Drawing.Size(200, 23);
            this.textId.TabIndex = 1;
            // 
            // labelId
            // 
            this.labelId.Location = new System.Drawing.Point(15, 30);
            this.labelId.Name = "labelId";
            this.labelId.Size = new System.Drawing.Size(28, 20);
            this.labelId.TabIndex = 0;
            this.labelId.Values.Text = "ID :";
            // 
            // BlocDetailView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupDetails);
            this.Name = "BlocDetailView";
            this.Size = new System.Drawing.Size(700, 500);
            ((System.ComponentModel.ISupportInitialize)(this.groupDetails.Panel)).EndInit();
            this.groupDetails.Panel.ResumeLayout(false);
            this.groupDetails.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupDetails)).EndInit();
            this.groupDetails.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private KryptonGroupBox groupDetails;
        private KryptonTextBox textId;
        private KryptonLabel labelId;
        private KryptonTextBox textName;
        private KryptonLabel labelName;
        private KryptonNumericUpDown numCapacity;
        private KryptonLabel labelCapacity;
        private KryptonTextBox textLocationY;
        private KryptonLabel labelLocationY;
        private KryptonTextBox textLocationX;
        private KryptonLabel labelLocationX;
        private KryptonLabel labelLocation;
    }
}