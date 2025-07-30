namespace PlanAthena.Forms
{
    partial class BlocForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxDetails = new System.Windows.Forms.GroupBox();
            this.txtEmplacementY = new System.Windows.Forms.TextBox();
            this.txtEmplacementX = new System.Windows.Forms.TextBox();
            this.lblEmplacementY = new System.Windows.Forms.Label();
            this.lblEmplacementX = new System.Windows.Forms.Label();
            this.lblEmplacement = new System.Windows.Forms.Label();
            this.numCapaciteMax = new System.Windows.Forms.NumericUpDown();
            this.lblCapacite = new System.Windows.Forms.Label();
            this.txtNom = new System.Windows.Forms.TextBox();
            this.lblNom = new System.Windows.Forms.Label();
            this.txtBlocId = new System.Windows.Forms.TextBox();
            this.lblBlocId = new System.Windows.Forms.Label();
            this.btnSauvegarder = new System.Windows.Forms.Button();
            this.btnAnnulerEdition = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnFermer = new System.Windows.Forms.Button();
            this.toolTipEmplacement = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCapaciteMax)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxDetails
            // 
            this.groupBoxDetails.Controls.Add(this.txtEmplacementY);
            this.groupBoxDetails.Controls.Add(this.txtEmplacementX);
            this.groupBoxDetails.Controls.Add(this.lblEmplacementY);
            this.groupBoxDetails.Controls.Add(this.lblEmplacementX);
            this.groupBoxDetails.Controls.Add(this.lblEmplacement);
            this.groupBoxDetails.Controls.Add(this.numCapaciteMax);
            this.groupBoxDetails.Controls.Add(this.lblCapacite);
            this.groupBoxDetails.Controls.Add(this.txtNom);
            this.groupBoxDetails.Controls.Add(this.lblNom);
            this.groupBoxDetails.Controls.Add(this.txtBlocId);
            this.groupBoxDetails.Controls.Add(this.lblBlocId);
            this.groupBoxDetails.Location = new System.Drawing.Point(12, 12);
            this.groupBoxDetails.Name = "groupBoxDetails";
            this.groupBoxDetails.Size = new System.Drawing.Size(360, 200);
            this.groupBoxDetails.TabIndex = 0;
            this.groupBoxDetails.TabStop = false;
            this.groupBoxDetails.Text = "Détails du Bloc";
            // 
            // txtEmplacementY
            // 
            this.txtEmplacementY.Enabled = false;
            this.txtEmplacementY.Location = new System.Drawing.Point(280, 140);
            this.txtEmplacementY.Name = "txtEmplacementY";
            this.txtEmplacementY.ReadOnly = true;
            this.txtEmplacementY.Size = new System.Drawing.Size(60, 23);
            this.txtEmplacementY.TabIndex = 10;
            this.toolTipEmplacement.SetToolTip(this.txtEmplacementY, "Fonctionnalité à venir");
            // 
            // txtEmplacementX
            // 
            this.txtEmplacementX.Enabled = false;
            this.txtEmplacementX.Location = new System.Drawing.Point(190, 140);
            this.txtEmplacementX.Name = "txtEmplacementX";
            this.txtEmplacementX.ReadOnly = true;
            this.txtEmplacementX.Size = new System.Drawing.Size(60, 23);
            this.txtEmplacementX.TabIndex = 9;
            this.toolTipEmplacement.SetToolTip(this.txtEmplacementX, "Fonctionnalité à venir");
            // 
            // lblEmplacementY
            // 
            this.lblEmplacementY.AutoSize = true;
            this.lblEmplacementY.Location = new System.Drawing.Point(256, 143);
            this.lblEmplacementY.Name = "lblEmplacementY";
            this.lblEmplacementY.Size = new System.Drawing.Size(17, 15);
            this.lblEmplacementY.TabIndex = 8;
            this.lblEmplacementY.Text = "Y:";
            // 
            // lblEmplacementX
            // 
            this.lblEmplacementX.AutoSize = true;
            this.lblEmplacementX.Location = new System.Drawing.Point(166, 143);
            this.lblEmplacementX.Name = "lblEmplacementX";
            this.lblEmplacementX.Size = new System.Drawing.Size(17, 15);
            this.lblEmplacementX.TabIndex = 7;
            this.lblEmplacementX.Text = "X:";
            // 
            // lblEmplacement
            // 
            this.lblEmplacement.AutoSize = true;
            this.lblEmplacement.Location = new System.Drawing.Point(16, 143);
            this.lblEmplacement.Name = "lblEmplacement";
            this.lblEmplacement.Size = new System.Drawing.Size(132, 15);
            this.lblEmplacement.TabIndex = 6;
            this.lblEmplacement.Text = "Emplacement sur le plan:";
            // 
            // numCapaciteMax
            // 
            this.numCapaciteMax.Location = new System.Drawing.Point(140, 83);
            this.numCapaciteMax.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numCapaciteMax.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCapaciteMax.Name = "numCapaciteMax";
            this.numCapaciteMax.Size = new System.Drawing.Size(120, 23);
            this.numCapaciteMax.TabIndex = 5;
            this.numCapaciteMax.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblCapacite
            // 
            this.lblCapacite.AutoSize = true;
            this.lblCapacite.Location = new System.Drawing.Point(16, 85);
            this.lblCapacite.Name = "lblCapacite";
            this.lblCapacite.Size = new System.Drawing.Size(118, 15);
            this.lblCapacite.TabIndex = 4;
            this.lblCapacite.Text = "Capacité max. (ouvr):";
            // 
            // txtNom
            // 
            this.txtNom.Location = new System.Drawing.Point(59, 54);
            this.txtNom.Name = "txtNom";
            this.txtNom.Size = new System.Drawing.Size(281, 23);
            this.txtNom.TabIndex = 3;
            // 
            // lblNom
            // 
            this.lblNom.AutoSize = true;
            this.lblNom.Location = new System.Drawing.Point(16, 57);
            this.lblNom.Name = "lblNom";
            this.lblNom.Size = new System.Drawing.Size(37, 15);
            this.lblNom.TabIndex = 2;
            this.lblNom.Text = "Nom:";
            // 
            // txtBlocId
            // 
            this.txtBlocId.Location = new System.Drawing.Point(43, 25);
            this.txtBlocId.Name = "txtBlocId";
            this.txtBlocId.ReadOnly = true;
            this.txtBlocId.Size = new System.Drawing.Size(150, 23);
            this.txtBlocId.TabIndex = 1;
            // 
            // lblBlocId
            // 
            this.lblBlocId.AutoSize = true;
            this.lblBlocId.Location = new System.Drawing.Point(16, 28);
            this.lblBlocId.Name = "lblBlocId";
            this.lblBlocId.Size = new System.Drawing.Size(21, 15);
            this.lblBlocId.TabIndex = 0;
            this.lblBlocId.Text = "ID:";
            // 
            // btnSauvegarder
            // 
            this.btnSauvegarder.Location = new System.Drawing.Point(28, 230);
            this.btnSauvegarder.Name = "btnSauvegarder";
            this.btnSauvegarder.Size = new System.Drawing.Size(100, 30);
            this.btnSauvegarder.TabIndex = 1;
            this.btnSauvegarder.Text = "💾 Sauvegarder";
            this.btnSauvegarder.UseVisualStyleBackColor = true;
            this.btnSauvegarder.Click += new System.EventHandler(this.btnSauvegarder_Click);
            // 
            // btnAnnulerEdition
            // 
            this.btnAnnulerEdition.Location = new System.Drawing.Point(146, 230);
            this.btnAnnulerEdition.Name = "btnAnnulerEdition";
            this.btnAnnulerEdition.Size = new System.Drawing.Size(100, 30);
            this.btnAnnulerEdition.TabIndex = 2;
            this.btnAnnulerEdition.Text = "❌ Annuler";
            this.btnAnnulerEdition.UseVisualStyleBackColor = true;
            this.btnAnnulerEdition.Click += new System.EventHandler(this.btnAnnulerEdition_Click);
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.Location = new System.Drawing.Point(264, 230);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(100, 30);
            this.btnSupprimer.TabIndex = 3;
            this.btnSupprimer.Text = "🗑️ Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = true;
            this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click);
            // 
            // btnFermer
            // 
            this.btnFermer.Location = new System.Drawing.Point(297, 275);
            this.btnFermer.Name = "btnFermer";
            this.btnFermer.Size = new System.Drawing.Size(75, 25);
            this.btnFermer.TabIndex = 4;
            this.btnFermer.Text = "Fermer";
            this.btnFermer.UseVisualStyleBackColor = true;
            this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);
            // 
            // BlocForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 312);
            this.Controls.Add(this.btnFermer);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnAnnulerEdition);
            this.Controls.Add(this.btnSauvegarder);
            this.Controls.Add(this.groupBoxDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BlocForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Édition de Bloc";
            this.Load += new System.EventHandler(this.BlocForm_Load);
            this.groupBoxDetails.ResumeLayout(false);
            this.groupBoxDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCapaciteMax)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxDetails;
        private System.Windows.Forms.TextBox txtBlocId;
        private System.Windows.Forms.Label lblBlocId;
        private System.Windows.Forms.TextBox txtNom;
        private System.Windows.Forms.Label lblNom;
        private System.Windows.Forms.NumericUpDown numCapaciteMax;
        private System.Windows.Forms.Label lblCapacite;
        private System.Windows.Forms.Label lblEmplacement;
        private System.Windows.Forms.Label lblEmplacementX;
        private System.Windows.Forms.Label lblEmplacementY;
        private System.Windows.Forms.TextBox txtEmplacementX;
        private System.Windows.Forms.TextBox txtEmplacementY;
        private System.Windows.Forms.Button btnSauvegarder;
        private System.Windows.Forms.Button btnAnnulerEdition;
        private System.Windows.Forms.Button btnSupprimer;
        private System.Windows.Forms.Button btnFermer;
        private System.Windows.Forms.ToolTip toolTipEmplacement;
    }
}