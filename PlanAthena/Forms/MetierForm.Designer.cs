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
            splitContainer1 = new SplitContainer();
            groupBoxListe = new GroupBox();
            lblStatut = new Label();
            btnSupprimer = new Button();
            btnModifier = new Button();
            btnNouveau = new Button();
            txtRecherche = new TextBox();
            lblRecherche = new Label();
            listViewMetiers = new ListView();
            colMetierId = new ColumnHeader();
            colNom = new ColumnHeader();
            colNombrePrerequisText = new ColumnHeader();
            groupBoxDetails = new GroupBox();
            lblUtilisation = new Label();
            btnSupprimerPrerequis = new Button();
            btnAjouterPrerequis = new Button();
            listViewPrerequis = new ListView();
            colPrerequisId = new ColumnHeader();
            colPrerequisNom = new ColumnHeader();
            lblPrerequis = new Label();
            txtNom = new TextBox();
            lblNom = new Label();
            txtMetierId = new TextBox();
            lblMetierId = new Label();
            groupBoxActions = new GroupBox();
            btnExporter = new Button();
            btnImporter = new Button();
            btnFermer = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBoxListe.SuspendLayout();
            groupBoxDetails.SuspendLayout();
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
            splitContainer1.Panel1.Controls.Add(groupBoxListe);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(groupBoxDetails);
            splitContainer1.Size = new Size(784, 461);
            splitContainer1.SplitterDistance = 350;
            splitContainer1.TabIndex = 0;
            // 
            // groupBoxListe
            // 
            groupBoxListe.Controls.Add(lblStatut);
            groupBoxListe.Controls.Add(btnSupprimer);
            groupBoxListe.Controls.Add(btnModifier);
            groupBoxListe.Controls.Add(btnNouveau);
            groupBoxListe.Controls.Add(txtRecherche);
            groupBoxListe.Controls.Add(lblRecherche);
            groupBoxListe.Controls.Add(listViewMetiers);
            groupBoxListe.Dock = DockStyle.Fill;
            groupBoxListe.Location = new Point(0, 0);
            groupBoxListe.Name = "groupBoxListe";
            groupBoxListe.Size = new Size(350, 461);
            groupBoxListe.TabIndex = 0;
            groupBoxListe.TabStop = false;
            groupBoxListe.Text = "Liste des Métiers";
            // 
            // lblStatut
            // 
            lblStatut.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatut.AutoSize = true;
            lblStatut.Location = new Point(6, 437);
            lblStatut.Name = "lblStatut";
            lblStatut.Size = new Size(76, 15);
            lblStatut.TabIndex = 6;
            lblStatut.Text = "0 métier(s)";
            // 
            // btnSupprimer
            // 
            btnSupprimer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSupprimer.Location = new Point(269, 405);
            btnSupprimer.Name = "btnSupprimer";
            btnSupprimer.Size = new Size(75, 23);
            btnSupprimer.TabIndex = 5;
            btnSupprimer.Text = "🗑️ Supprimer";
            btnSupprimer.UseVisualStyleBackColor = true;
            btnSupprimer.Click += btnSupprimer_Click;
            // 
            // btnModifier
            // 
            btnModifier.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnModifier.Location = new Point(188, 405);
            btnModifier.Name = "btnModifier";
            btnModifier.Size = new Size(75, 23);
            btnModifier.TabIndex = 4;
            btnModifier.Text = "✏️ Modifier";
            btnModifier.UseVisualStyleBackColor = true;
            btnModifier.Click += btnModifier_Click;
            // 
            // btnNouveau
            // 
            btnNouveau.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnNouveau.Location = new Point(107, 405);
            btnNouveau.Name = "btnNouveau";
            btnNouveau.Size = new Size(75, 23);
            btnNouveau.TabIndex = 3;
            btnNouveau.Text = "➕ Nouveau";
            btnNouveau.UseVisualStyleBackColor = true;
            btnNouveau.Click += btnNouveau_Click;
            // 
            // txtRecherche
            // 
            txtRecherche.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRecherche.Location = new Point(72, 22);
            txtRecherche.Name = "txtRecherche";
            txtRecherche.Size = new Size(272, 23);
            txtRecherche.TabIndex = 2;
            txtRecherche.TextChanged += txtRecherche_TextChanged;
            // 
            // lblRecherche
            // 
            lblRecherche.AutoSize = true;
            lblRecherche.Location = new Point(6, 25);
            lblRecherche.Name = "lblRecherche";
            lblRecherche.Size = new Size(60, 15);
            lblRecherche.TabIndex = 1;
            lblRecherche.Text = "Recherche:";
            // 
            // listViewMetiers
            // 
            listViewMetiers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewMetiers.Columns.AddRange(new ColumnHeader[] { colMetierId, colNom, colNombrePrerequisText });
            listViewMetiers.FullRowSelect = true;
            listViewMetiers.GridLines = true;
            listViewMetiers.Location = new Point(6, 51);
            listViewMetiers.MultiSelect = false;
            listViewMetiers.Name = "listViewMetiers";
            listViewMetiers.Size = new Size(338, 348);
            listViewMetiers.TabIndex = 0;
            listViewMetiers.UseCompatibleStateImageBehavior = false;
            listViewMetiers.View = View.Details;
            listViewMetiers.SelectedIndexChanged += listViewMetiers_SelectedIndexChanged;
            // 
            // colMetierId
            // 
            colMetierId.Text = "ID";
            colMetierId.Width = 80;
            // 
            // colNom
            // 
            colNom.Text = "Nom";
            colNom.Width = 180;
            // 
            // colNombrePrerequisText
            // 
            colNombrePrerequisText.Text = "Prérequis";
            colNombrePrerequisText.Width = 70;
            // 
            // groupBoxDetails
            // 
            groupBoxDetails.Controls.Add(lblUtilisation);
            groupBoxDetails.Controls.Add(btnSupprimerPrerequis);
            groupBoxDetails.Controls.Add(btnAjouterPrerequis);
            groupBoxDetails.Controls.Add(listViewPrerequis);
            groupBoxDetails.Controls.Add(lblPrerequis);
            groupBoxDetails.Controls.Add(txtNom);
            groupBoxDetails.Controls.Add(lblNom);
            groupBoxDetails.Controls.Add(txtMetierId);
            groupBoxDetails.Controls.Add(lblMetierId);
            groupBoxDetails.Dock = DockStyle.Fill;
            groupBoxDetails.Location = new Point(0, 0);
            groupBoxDetails.Name = "groupBoxDetails";
            groupBoxDetails.Size = new Size(430, 461);
            groupBoxDetails.TabIndex = 0;
            groupBoxDetails.TabStop = false;
            groupBoxDetails.Text = "Détails du Métier";
            // 
            // lblUtilisation
            // 
            lblUtilisation.AutoSize = true;
            lblUtilisation.ForeColor = Color.Blue;
            lblUtilisation.Location = new Point(16, 415);
            lblUtilisation.Name = "lblUtilisation";
            lblUtilisation.Size = new Size(188, 15);
            lblUtilisation.TabIndex = 8;
            lblUtilisation.Text = "⚠️ Utilisé par: 5 tâches, 12 ouvriers";
            // 
            // btnSupprimerPrerequis
            // 
            btnSupprimerPrerequis.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSupprimerPrerequis.Location = new Point(342, 350);
            btnSupprimerPrerequis.Name = "btnSupprimerPrerequis";
            btnSupprimerPrerequis.Size = new Size(75, 23);
            btnSupprimerPrerequis.TabIndex = 7;
            btnSupprimerPrerequis.Text = "🗑️ Enlever";
            btnSupprimerPrerequis.UseVisualStyleBackColor = true;
            btnSupprimerPrerequis.Click += btnSupprimerPrerequis_Click;
            // 
            // btnAjouterPrerequis
            // 
            btnAjouterPrerequis.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAjouterPrerequis.Location = new Point(342, 321);
            btnAjouterPrerequis.Name = "btnAjouterPrerequis";
            btnAjouterPrerequis.Size = new Size(75, 23);
            btnAjouterPrerequis.TabIndex = 6;
            btnAjouterPrerequis.Text = "➕ Ajouter";
            btnAjouterPrerequis.UseVisualStyleBackColor = true;
            btnAjouterPrerequis.Click += btnAjouterPrerequis_Click;
            // 
            // listViewPrerequis
            // 
            listViewPrerequis.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listViewPrerequis.Columns.AddRange(new ColumnHeader[] { colPrerequisId, colPrerequisNom });
            listViewPrerequis.FullRowSelect = true;
            listViewPrerequis.GridLines = true;
            listViewPrerequis.Location = new Point(16, 104);
            listViewPrerequis.Name = "listViewPrerequis";
            listViewPrerequis.Size = new Size(320, 200);
            listViewPrerequis.TabIndex = 5;
            listViewPrerequis.UseCompatibleStateImageBehavior = false;
            listViewPrerequis.View = View.Details;
            // 
            // colPrerequisId
            // 
            colPrerequisId.Text = "ID Prérequis";
            colPrerequisId.Width = 100;
            // 
            // colPrerequisNom
            // 
            colPrerequisNom.Text = "Nom";
            colPrerequisNom.Width = 200;
            // 
            // lblPrerequis
            // 
            lblPrerequis.AutoSize = true;
            lblPrerequis.Location = new Point(16, 86);
            lblPrerequis.Name = "lblPrerequis";
            lblPrerequis.Size = new Size(60, 15);
            lblPrerequis.TabIndex = 4;
            lblPrerequis.Text = "Prérequis:";
            // 
            // txtNom
            // 
            txtNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNom.Location = new Point(70, 54);
            txtNom.Name = "txtNom";
            txtNom.Size = new Size(347, 23);
            txtNom.TabIndex = 3;
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
            // txtMetierId
            // 
            txtMetierId.Location = new Point(70, 25);
            txtMetierId.Name = "txtMetierId";
            txtMetierId.Size = new Size(150, 23);
            txtMetierId.TabIndex = 1;
            // 
            // lblMetierId
            // 
            lblMetierId.AutoSize = true;
            lblMetierId.Location = new Point(16, 28);
            lblMetierId.Name = "lblMetierId";
            lblMetierId.Size = new Size(21, 15);
            lblMetierId.TabIndex = 0;
            lblMetierId.Text = "ID:";
            // 
            // groupBoxActions
            // 
            groupBoxActions.Controls.Add(btnExporter);
            groupBoxActions.Controls.Add(btnImporter);
            groupBoxActions.Controls.Add(btnFermer);
            groupBoxActions.Dock = DockStyle.Bottom;
            groupBoxActions.Location = new Point(0, 461);
            groupBoxActions.Name = "groupBoxActions";
            groupBoxActions.Size = new Size(784, 50);
            groupBoxActions.TabIndex = 1;
            groupBoxActions.TabStop = false;
            groupBoxActions.Text = "Actions";
            // 
            // btnExporter
            // 
            btnExporter.Location = new Point(165, 19);
            btnExporter.Name = "btnExporter";
            btnExporter.Size = new Size(100, 23);
            btnExporter.TabIndex = 2;
            btnExporter.Text = "📤 Export CSV";
            btnExporter.UseVisualStyleBackColor = true;
            btnExporter.Click += btnExporter_Click;
            // 
            // btnImporter
            // 
            btnImporter.Location = new Point(59, 19);
            btnImporter.Name = "btnImporter";
            btnImporter.Size = new Size(100, 23);
            btnImporter.TabIndex = 1;
            btnImporter.Text = "📥 Import CSV";
            btnImporter.UseVisualStyleBackColor = true;
            btnImporter.Click += btnImporter_Click;
            // 
            // btnFermer
            // 
            btnFermer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFermer.Location = new Point(697, 19);
            btnFermer.Name = "btnFermer";
            btnFermer.Size = new Size(75, 23);
            btnFermer.TabIndex = 0;
            btnFermer.Text = "Fermer";
            btnFermer.UseVisualStyleBackColor = true;
            btnFermer.Click += btnFermer_Click;
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
            groupBoxListe.ResumeLayout(false);
            groupBoxListe.PerformLayout();
            groupBoxDetails.ResumeLayout(false);
            groupBoxDetails.PerformLayout();
            groupBoxActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private GroupBox groupBoxListe;
        private GroupBox groupBoxDetails;
        private GroupBox groupBoxActions;
        private ListView listViewMetiers;
        private ColumnHeader colMetierId;
        private ColumnHeader colNom;
        private ColumnHeader colNombrePrerequisText;
        private TextBox txtRecherche;
        private Label lblRecherche;
        private Button btnSupprimer;
        private Button btnModifier;
        private Button btnNouveau;
        private Label lblStatut;
        private TextBox txtMetierId;
        private Label lblMetierId;
        private TextBox txtNom;
        private Label lblNom;
        private ListView listViewPrerequis;
        private ColumnHeader colPrerequisId;
        private ColumnHeader colPrerequisNom;
        private Label lblPrerequis;
        private Button btnAjouterPrerequis;
        private Button btnSupprimerPrerequis;
        private Label lblUtilisation;
        private Button btnImporter;
        private Button btnExporter;
        private Button btnFermer;
    }
}