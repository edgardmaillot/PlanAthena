namespace PlanAthena.Forms
{
    partial class PrerequisMetierForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            panelLeft = new System.Windows.Forms.Panel();
            groupBoxDetails = new System.Windows.Forms.GroupBox();
            grpPhases = new System.Windows.Forms.GroupBox();
            chkFinition = new System.Windows.Forms.CheckBox();
            chkSecondOeuvre = new System.Windows.Forms.CheckBox();
            chkGrosOeuvre = new System.Windows.Forms.CheckBox();
            txtPictogram = new System.Windows.Forms.TextBox();
            lblPictogram = new System.Windows.Forms.Label();
            btnChoisirCouleur = new System.Windows.Forms.Button();
            panelCouleurApercu = new System.Windows.Forms.Panel();
            lblCouleur = new System.Windows.Forms.Label();
            txtNom = new System.Windows.Forms.TextBox();
            lblNom = new System.Windows.Forms.Label();
            lblMetierId = new System.Windows.Forms.Label();
            groupBoxActions = new System.Windows.Forms.GroupBox();
            btnAnnuler = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            _toolTipMetiers = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBoxDetails.SuspendLayout();
            grpPhases.SuspendLayout();
            groupBoxActions.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panelLeft);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(groupBoxDetails);
            splitContainer1.Size = new System.Drawing.Size(984, 561);
            splitContainer1.SplitterDistance = 580;
            splitContainer1.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.AutoScroll = true;
            panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            panelLeft.Location = new System.Drawing.Point(0, 0);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new System.Drawing.Size(580, 561);
            panelLeft.TabIndex = 0;
            // 
            // groupBoxDetails
            // 
            groupBoxDetails.Controls.Add(grpPhases);
            groupBoxDetails.Controls.Add(txtPictogram);
            groupBoxDetails.Controls.Add(lblPictogram);
            groupBoxDetails.Controls.Add(btnChoisirCouleur);
            groupBoxDetails.Controls.Add(panelCouleurApercu);
            groupBoxDetails.Controls.Add(lblCouleur);
            groupBoxDetails.Controls.Add(txtNom);
            groupBoxDetails.Controls.Add(lblNom);
            groupBoxDetails.Controls.Add(lblMetierId);
            groupBoxDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxDetails.Location = new System.Drawing.Point(0, 0);
            groupBoxDetails.Name = "groupBoxDetails";
            groupBoxDetails.Size = new System.Drawing.Size(400, 561);
            groupBoxDetails.TabIndex = 0;
            groupBoxDetails.TabStop = false;
            groupBoxDetails.Text = "Détails du Métier";
            // 
            // grpPhases
            // 
            grpPhases.Controls.Add(chkFinition);
            grpPhases.Controls.Add(chkSecondOeuvre);
            grpPhases.Controls.Add(chkGrosOeuvre);
            grpPhases.Location = new System.Drawing.Point(16, 142);
            grpPhases.Name = "grpPhases";
            grpPhases.Size = new System.Drawing.Size(300, 50);
            grpPhases.TabIndex = 13;
            grpPhases.TabStop = false;
            grpPhases.Text = "Phases de chantier";
            // 
            // chkFinition
            // 
            chkFinition.AutoSize = true;
            chkFinition.Location = new System.Drawing.Point(220, 22);
            chkFinition.Name = "chkFinition";
            chkFinition.Size = new System.Drawing.Size(66, 19);
            chkFinition.TabIndex = 2;
            chkFinition.Text = "Finition";
            chkFinition.UseVisualStyleBackColor = true;
            // 
            // chkSecondOeuvre
            // 
            chkSecondOeuvre.AutoSize = true;
            chkSecondOeuvre.Location = new System.Drawing.Point(100, 22);
            chkSecondOeuvre.Name = "chkSecondOeuvre";
            chkSecondOeuvre.Size = new System.Drawing.Size(102, 19);
            chkSecondOeuvre.TabIndex = 1;
            chkSecondOeuvre.Text = "Second œuvre";
            chkSecondOeuvre.UseVisualStyleBackColor = true;
            // 
            // chkGrosOeuvre
            // 
            chkGrosOeuvre.AutoSize = true;
            chkGrosOeuvre.Location = new System.Drawing.Point(7, 22);
            chkGrosOeuvre.Name = "chkGrosOeuvre";
            chkGrosOeuvre.Size = new System.Drawing.Size(87, 19);
            chkGrosOeuvre.TabIndex = 0;
            chkGrosOeuvre.Text = "Gros œuvre";
            chkGrosOeuvre.UseVisualStyleBackColor = true;
            // 
            // txtPictogram
            // 
            txtPictogram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            txtPictogram.Location = new System.Drawing.Point(100, 113);
            txtPictogram.Name = "txtPictogram";
            txtPictogram.Size = new System.Drawing.Size(291, 23);
            txtPictogram.TabIndex = 12;
            // 
            // lblPictogram
            // 
            lblPictogram.AutoSize = true;
            lblPictogram.Location = new System.Drawing.Point(16, 116);
            lblPictogram.Name = "lblPictogram";
            lblPictogram.Size = new System.Drawing.Size(68, 15);
            lblPictogram.TabIndex = 11;
            lblPictogram.Text = "Pictogram :";
            // 
            // btnChoisirCouleur
            // 
            btnChoisirCouleur.Location = new System.Drawing.Point(217, 80);
            btnChoisirCouleur.Name = "btnChoisirCouleur";
            btnChoisirCouleur.Size = new System.Drawing.Size(100, 23);
            btnChoisirCouleur.TabIndex = 6;
            btnChoisirCouleur.Text = "🎨 Choisir";
            btnChoisirCouleur.UseVisualStyleBackColor = true;
            // 
            // panelCouleurApercu
            // 
            panelCouleurApercu.BackColor = System.Drawing.Color.LightGray;
            panelCouleurApercu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelCouleurApercu.Location = new System.Drawing.Point(100, 80);
            panelCouleurApercu.Name = "panelCouleurApercu";
            panelCouleurApercu.Size = new System.Drawing.Size(111, 23);
            panelCouleurApercu.TabIndex = 5;
            // 
            // lblCouleur
            // 
            lblCouleur.AutoSize = true;
            lblCouleur.Location = new System.Drawing.Point(16, 83);
            lblCouleur.Name = "lblCouleur";
            lblCouleur.Size = new System.Drawing.Size(52, 15);
            lblCouleur.TabIndex = 4;
            lblCouleur.Text = "Couleur:";
            // 
            // txtNom
            // 
            txtNom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            txtNom.Location = new System.Drawing.Point(100, 54);
            txtNom.Name = "txtNom";
            txtNom.Size = new System.Drawing.Size(291, 23);
            txtNom.TabIndex = 3;
            // 
            // lblNom
            // 
            lblNom.AutoSize = true;
            lblNom.Location = new System.Drawing.Point(16, 57);
            lblNom.Name = "lblNom";
            lblNom.Size = new System.Drawing.Size(37, 15);
            lblNom.TabIndex = 2;
            lblNom.Text = "Nom:";
            // 
            // lblMetierId
            // 
            lblMetierId.AutoSize = true;
            lblMetierId.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblMetierId.Location = new System.Drawing.Point(100, 28);
            lblMetierId.Name = "lblMetierId";
            lblMetierId.Size = new System.Drawing.Size(23, 15);
            lblMetierId.TabIndex = 0;
            lblMetierId.Text = "ID:";
            // 
            // groupBoxActions
            // 
            groupBoxActions.Controls.Add(btnAnnuler);
            groupBoxActions.Controls.Add(btnOK);
            groupBoxActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupBoxActions.Location = new System.Drawing.Point(0, 561);
            groupBoxActions.Name = "groupBoxActions";
            groupBoxActions.Size = new System.Drawing.Size(984, 50);
            groupBoxActions.TabIndex = 1;
            groupBoxActions.TabStop = false;
            groupBoxActions.Text = "Actions";
            // 
            // btnAnnuler
            // 
            btnAnnuler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnAnnuler.Location = new System.Drawing.Point(897, 19);
            btnAnnuler.Name = "btnAnnuler";
            btnAnnuler.Size = new System.Drawing.Size(75, 23);
            btnAnnuler.TabIndex = 0;
            btnAnnuler.Text = "Annuler";
            btnAnnuler.UseVisualStyleBackColor = true;
            btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // btnOK
            // 
            btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnOK.Location = new System.Drawing.Point(816, 19);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // _toolTipMetiers
            // 
            _toolTipMetiers.AutoPopDelay = 5000;
            _toolTipMetiers.InitialDelay = 500;
            _toolTipMetiers.ReshowDelay = 100;
            // 
            // PrerequisMetierForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(984, 611);
            Controls.Add(splitContainer1);
            Controls.Add(groupBoxActions);
            Name = "PrerequisMetierForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Configuration des Prérequis Métiers";
            Load += new System.EventHandler(this.PrerequisMetierForm_Load);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBoxDetails.ResumeLayout(false);
            groupBoxDetails.PerformLayout();
            grpPhases.ResumeLayout(false);
            grpPhases.PerformLayout();
            groupBoxActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBoxDetails;
        private System.Windows.Forms.GroupBox groupBoxActions;
        private System.Windows.Forms.Label lblCouleur;
        private System.Windows.Forms.Panel panelCouleurApercu;
        private System.Windows.Forms.Button btnChoisirCouleur;
        private System.Windows.Forms.TextBox txtNom;
        private System.Windows.Forms.Label lblNom;
        private System.Windows.Forms.Label lblMetierId;
        private System.Windows.Forms.TextBox txtPictogram;
        private System.Windows.Forms.Label lblPictogram;
        private System.Windows.Forms.GroupBox grpPhases;
        private System.Windows.Forms.CheckBox chkGrosOeuvre;
        private System.Windows.Forms.CheckBox chkFinition;
        private System.Windows.Forms.CheckBox chkSecondOeuvre;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnAnnuler;
        private System.Windows.Forms.ToolTip _toolTipMetiers;
    }
}