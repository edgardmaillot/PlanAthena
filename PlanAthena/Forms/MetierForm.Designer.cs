// Forms/MetierForm.Designer.cs
// 🔄 VERSION V0.4.2 - Interface adaptée pour 2 concepts

namespace PlanAthena.Forms
{
    partial class MetierForm
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
            splitContainer1 = new SplitContainer();
            panelLeft = new Panel();
            groupBoxDetails = new GroupBox();
            grpPhases = new GroupBox();
            chkFinition = new CheckBox();
            chkSecondOeuvre = new CheckBox();
            chkGrosOeuvre = new CheckBox();
            txtPictogram = new TextBox();
            lblPictogram = new Label();
            btnChoisirCouleur = new Button();
            panelCouleurApercu = new Panel();
            lblCouleur = new Label();
            txtNom = new TextBox();
            lblNom = new Label();
            lblMetierId = new Label();
            groupBoxActions = new GroupBox();
            btnAnnuler = new Button();
            btnValider = new Button();
            _toolTipMetiers = new ToolTip(components);
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
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panelLeft);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(groupBoxDetails);
            splitContainer1.Size = new Size(784, 461);
            splitContainer1.SplitterDistance = 377;
            splitContainer1.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.AutoScroll = true;
            panelLeft.Dock = DockStyle.Fill;
            panelLeft.Location = new Point(0, 0);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(377, 461);
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
            groupBoxDetails.Dock = DockStyle.Right;
            groupBoxDetails.Location = new Point(-1, 0);
            groupBoxDetails.Name = "groupBoxDetails";
            groupBoxDetails.Size = new Size(404, 461);
            groupBoxDetails.TabIndex = 0;
            groupBoxDetails.TabStop = false;
            groupBoxDetails.Text = "Détails du Métier";
            // 
            // grpPhases
            // 
            grpPhases.Controls.Add(chkFinition);
            grpPhases.Controls.Add(chkSecondOeuvre);
            grpPhases.Controls.Add(chkGrosOeuvre);
            grpPhases.Location = new Point(16, 142);
            grpPhases.Name = "grpPhases";
            grpPhases.Size = new Size(300, 50);
            grpPhases.TabIndex = 13;
            grpPhases.TabStop = false;
            grpPhases.Text = "Phases de chantier";
            // 
            // chkFinition
            // 
            chkFinition.AutoSize = true;
            chkFinition.Location = new Point(220, 22);
            chkFinition.Name = "chkFinition";
            chkFinition.Size = new Size(66, 19);
            chkFinition.TabIndex = 2;
            chkFinition.Text = "Finition";
            chkFinition.UseVisualStyleBackColor = true;
            chkFinition.CheckedChanged += OnDetailChanged;
            // 
            // chkSecondOeuvre
            // 
            chkSecondOeuvre.AutoSize = true;
            chkSecondOeuvre.Location = new Point(100, 22);
            chkSecondOeuvre.Name = "chkSecondOeuvre";
            chkSecondOeuvre.Size = new Size(102, 19);
            chkSecondOeuvre.TabIndex = 1;
            chkSecondOeuvre.Text = "Second œuvre";
            chkSecondOeuvre.UseVisualStyleBackColor = true;
            chkSecondOeuvre.CheckedChanged += OnDetailChanged;
            // 
            // chkGrosOeuvre
            // 
            chkGrosOeuvre.AutoSize = true;
            chkGrosOeuvre.Location = new Point(7, 22);
            chkGrosOeuvre.Name = "chkGrosOeuvre";
            chkGrosOeuvre.Size = new Size(87, 19);
            chkGrosOeuvre.TabIndex = 0;
            chkGrosOeuvre.Text = "Gros œuvre";
            chkGrosOeuvre.UseVisualStyleBackColor = true;
            chkGrosOeuvre.CheckedChanged += OnDetailChanged;
            // 
            // txtPictogram
            // 
            txtPictogram.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPictogram.Location = new Point(100, 113);
            txtPictogram.Name = "txtPictogram";
            txtPictogram.Size = new Size(291, 23);
            txtPictogram.TabIndex = 12;
            txtPictogram.TextChanged += OnDetailChanged;
            // 
            // lblPictogram
            // 
            lblPictogram.AutoSize = true;
            lblPictogram.Location = new Point(16, 116);
            lblPictogram.Name = "lblPictogram";
            lblPictogram.Size = new Size(68, 15);
            lblPictogram.TabIndex = 11;
            lblPictogram.Text = "Pictogram :";
            // 
            // btnChoisirCouleur
            // 
            btnChoisirCouleur.Location = new Point(217, 80);
            btnChoisirCouleur.Name = "btnChoisirCouleur";
            btnChoisirCouleur.Size = new Size(100, 23);
            btnChoisirCouleur.TabIndex = 6;
            btnChoisirCouleur.Text = "🎨 Choisir";
            btnChoisirCouleur.UseVisualStyleBackColor = true;
            btnChoisirCouleur.Click += btnChoisirCouleur_Click;
            // 
            // panelCouleurApercu
            // 
            panelCouleurApercu.BackColor = Color.LightGray;
            panelCouleurApercu.BorderStyle = BorderStyle.FixedSingle;
            panelCouleurApercu.Location = new Point(100, 80);
            panelCouleurApercu.Name = "panelCouleurApercu";
            panelCouleurApercu.Size = new Size(111, 23);
            panelCouleurApercu.TabIndex = 5;
            panelCouleurApercu.Click += btnChoisirCouleur_Click;
            // 
            // lblCouleur
            // 
            lblCouleur.AutoSize = true;
            lblCouleur.Location = new Point(16, 83);
            lblCouleur.Name = "lblCouleur";
            lblCouleur.Size = new Size(52, 15);
            lblCouleur.TabIndex = 4;
            lblCouleur.Text = "Couleur:";
            // 
            // txtNom
            // 
            txtNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNom.Location = new Point(100, 54);
            txtNom.Name = "txtNom";
            txtNom.Size = new Size(291, 23);
            txtNom.TabIndex = 3;
            txtNom.TextChanged += OnDetailChanged;
            // 
            // lblNom
            // 
            lblNom.AutoSize = true;
            lblNom.Location = new Point(16, 57);
            lblNom.Name = "lblNom";
            lblNom.Size = new Size(37, 15);
            lblNom.TabIndex = 2;
            lblNom.Text = "Nom:";
            // 
            // lblMetierId
            // 
            lblMetierId.AutoSize = true;
            lblMetierId.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblMetierId.Location = new Point(100, 28);
            lblMetierId.Name = "lblMetierId";
            lblMetierId.Size = new Size(23, 15);
            lblMetierId.TabIndex = 0;
            lblMetierId.Text = "ID:";
            // 
            // groupBoxActions
            // 
            groupBoxActions.Controls.Add(btnAnnuler);
            groupBoxActions.Controls.Add(btnValider);
            groupBoxActions.Dock = DockStyle.Bottom;
            groupBoxActions.Location = new Point(0, 461);
            groupBoxActions.Name = "groupBoxActions";
            groupBoxActions.Size = new Size(784, 50);
            groupBoxActions.TabIndex = 1;
            groupBoxActions.TabStop = false;
            groupBoxActions.Text = "Actions";
            // 
            // btnAnnuler
            // 
            btnAnnuler.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAnnuler.Location = new Point(697, 19);
            btnAnnuler.Name = "btnAnnuler";
            btnAnnuler.Size = new Size(75, 23);
            btnAnnuler.TabIndex = 0;
            btnAnnuler.Text = "❌ Annuler";
            btnAnnuler.UseVisualStyleBackColor = true;
            btnAnnuler.Click += btnAnnuler_Click;
            // 
            // btnValider
            // 
            btnValider.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnValider.Location = new Point(616, 19);
            btnValider.Name = "btnValider";
            btnValider.Size = new Size(75, 23);
            btnValider.TabIndex = 1;
            btnValider.Text = "✅ Valider";
            btnValider.UseVisualStyleBackColor = true;
            btnValider.Click += btnValider_Click;
            // 
            // _toolTipMetiers
            // 
            _toolTipMetiers.AutoPopDelay = 5000;
            _toolTipMetiers.InitialDelay = 500;
            _toolTipMetiers.ReshowDelay = 100;
            // 
            // MetierForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 511);
            Controls.Add(splitContainer1);
            Controls.Add(groupBoxActions);
            Name = "MetierForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestion des Métiers";
            Load += MetierForm_Load;
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

        private SplitContainer splitContainer1;
        private GroupBox groupBoxDetails;
        private GroupBox groupBoxActions;
        private Label lblCouleur;
        private Panel panelCouleurApercu;
        private Button btnChoisirCouleur;
        private TextBox txtNom;
        private Label lblNom;
        private Label lblMetierId;
        private TextBox txtPictogram;
        private Label lblPictogram;
        private GroupBox grpPhases;
        private CheckBox chkGrosOeuvre;
        private CheckBox chkFinition;
        private CheckBox chkSecondOeuvre;
        private Panel panelLeft; // 🆕 V0.4.2 : Panel dynamique pour les 2 concepts
        private Button btnValider; // 🔄 V0.4.2 : Remplace btnSauvegarder
        private Button btnAnnuler;
        private ToolTip _toolTipMetiers; // 🆕 V0.4.2 : Pour tooltips boutons métiers

        // 🗑️ SUPPRIMÉ V0.4.2 : 
        // private CheckedListBox chkListPrerequis;
        // private Label lblPrerequis;
        // private Button btnNouveau;
        // private Button btnSupprimer;
    }
}