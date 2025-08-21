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
            groupDetails = new KryptonGroupBox();
            textLocationY = new KryptonTextBox();
            labelLocationY = new KryptonLabel();
            textLocationX = new KryptonTextBox();
            labelLocationX = new KryptonLabel();
            labelLocation = new KryptonLabel();
            numCapacity = new KryptonNumericUpDown();
            labelCapacity = new KryptonLabel();
            textName = new KryptonTextBox();
            labelName = new KryptonLabel();
            textId = new KryptonTextBox();
            labelId = new KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)groupDetails).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).BeginInit();
            groupDetails.Panel.SuspendLayout();
            SuspendLayout();
            // 
            // groupDetails
            // 
            groupDetails.CaptionOverlap = 0D;
            groupDetails.Dock = DockStyle.Fill;
            groupDetails.Location = new Point(0, 0);
            groupDetails.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupDetails.Panel.Controls.Add(textLocationY);
            groupDetails.Panel.Controls.Add(labelLocationY);
            groupDetails.Panel.Controls.Add(textLocationX);
            groupDetails.Panel.Controls.Add(labelLocationX);
            groupDetails.Panel.Controls.Add(labelLocation);
            groupDetails.Panel.Controls.Add(numCapacity);
            groupDetails.Panel.Controls.Add(labelCapacity);
            groupDetails.Panel.Controls.Add(textName);
            groupDetails.Panel.Controls.Add(labelName);
            groupDetails.Panel.Controls.Add(textId);
            groupDetails.Panel.Controls.Add(labelId);
            groupDetails.Size = new Size(817, 577);
            groupDetails.TabIndex = 1;
            groupDetails.Values.Heading = "Édition du Bloc";
            // 
            // textLocationY
            // 
            textLocationY.Enabled = false;
            textLocationY.Location = new Point(308, 156);
            textLocationY.Margin = new Padding(4, 3, 4, 3);
            textLocationY.Name = "textLocationY";
            textLocationY.Size = new Size(77, 23);
            textLocationY.TabIndex = 10;
            // 
            // labelLocationY
            // 
            labelLocationY.Enabled = false;
            labelLocationY.Location = new Point(280, 156);
            labelLocationY.Margin = new Padding(4, 3, 4, 3);
            labelLocationY.Name = "labelLocationY";
            labelLocationY.Size = new Size(23, 20);
            labelLocationY.TabIndex = 9;
            labelLocationY.Values.Text = "Y :";
            // 
            // textLocationX
            // 
            textLocationX.Enabled = false;
            textLocationX.Location = new Point(196, 156);
            textLocationX.Margin = new Padding(4, 3, 4, 3);
            textLocationX.Name = "textLocationX";
            textLocationX.Size = new Size(77, 23);
            textLocationX.TabIndex = 8;
            // 
            // labelLocationX
            // 
            labelLocationX.Enabled = false;
            labelLocationX.Location = new Point(168, 156);
            labelLocationX.Margin = new Padding(4, 3, 4, 3);
            labelLocationX.Name = "labelLocationX";
            labelLocationX.Size = new Size(24, 20);
            labelLocationX.TabIndex = 7;
            labelLocationX.Values.Text = "X :";
            // 
            // labelLocation
            // 
            labelLocation.Enabled = false;
            labelLocation.Location = new Point(18, 156);
            labelLocation.Margin = new Padding(4, 3, 4, 3);
            labelLocation.Name = "labelLocation";
            labelLocation.Size = new Size(125, 20);
            labelLocation.TabIndex = 6;
            labelLocation.Values.Text = "Emplacement / Plan :";
            // 
            // numCapacity
            // 
            numCapacity.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numCapacity.Location = new Point(152, 115);
            numCapacity.Margin = new Padding(4, 3, 4, 3);
            numCapacity.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numCapacity.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCapacity.Name = "numCapacity";
            numCapacity.Size = new Size(140, 22);
            numCapacity.TabIndex = 5;
            numCapacity.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelCapacity
            // 
            labelCapacity.Location = new Point(18, 115);
            labelCapacity.Margin = new Padding(4, 3, 4, 3);
            labelCapacity.Name = "labelCapacity";
            labelCapacity.Size = new Size(112, 20);
            labelCapacity.TabIndex = 4;
            labelCapacity.Values.Text = "Capacité Ouvriers :";
            // 
            // textName
            // 
            textName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textName.Location = new Point(152, 75);
            textName.Margin = new Padding(4, 3, 4, 3);
            textName.Name = "textName";
            textName.Size = new Size(644, 23);
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
            // BlocDetailView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupDetails);
            Margin = new Padding(4, 3, 4, 3);
            Name = "BlocDetailView";
            Size = new Size(817, 577);
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).EndInit();
            groupDetails.Panel.ResumeLayout(false);
            groupDetails.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupDetails).EndInit();
            ResumeLayout(false);

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