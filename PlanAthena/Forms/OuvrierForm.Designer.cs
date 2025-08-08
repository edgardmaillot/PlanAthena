namespace PlanAthena.Forms
{
    partial class OuvrierForm
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
            groupBoxListeOuvriers = new GroupBox();
            lblStatutOuvriers = new Label();
            btnSupprimerOuvrier = new Button();
            btnModifierOuvrier = new Button();
            btnNouveauOuvrier = new Button();
            txtRechercheOuvrier = new TextBox();
            lblRechercheOuvrier = new Label();
            listViewOuvriers = new ListView();
            colOuvrierId = new ColumnHeader();
            colNomPrenom = new ColumnHeader();
            colCoutJournalier = new ColumnHeader();
            colNombreCompetences = new ColumnHeader();
            splitContainer2 = new SplitContainer();
            groupBoxDetailsOuvrier = new GroupBox();
            numCoutJournalier = new NumericUpDown();
            lblCoutJournalier = new Label();
            txtPrenom = new TextBox();
            lblPrenom = new Label();
            txtNom = new TextBox();
            lblNom = new Label();
            txtOuvrierId = new TextBox();
            lblOuvrierId = new Label();
            groupBoxCompetences = new GroupBox();
            lblStatutCompetences = new Label();
            btnSupprimerCompetence = new Button();
            btnModifierCompetence = new Button();
            btnAjouterCompetence = new Button();
            listViewCompetences = new ListView();
            colMetierId = new ColumnHeader();
            colNomMetier = new ColumnHeader();
            groupBoxActions = new GroupBox();
            btnImportExcelSAP = new Button();
            btnExporter = new Button();
            btnImporter = new Button();
            btnFermer = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBoxListeOuvriers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBoxDetailsOuvrier.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCoutJournalier).BeginInit();
            groupBoxCompetences.SuspendLayout();
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
            splitContainer1.Panel1.Controls.Add(groupBoxListeOuvriers);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(984, 511);
            splitContainer1.SplitterDistance = 350;
            splitContainer1.TabIndex = 0;
            // 
            // groupBoxListeOuvriers
            // 
            groupBoxListeOuvriers.Controls.Add(lblStatutOuvriers);
            groupBoxListeOuvriers.Controls.Add(btnSupprimerOuvrier);
            groupBoxListeOuvriers.Controls.Add(btnModifierOuvrier);
            groupBoxListeOuvriers.Controls.Add(btnNouveauOuvrier);
            groupBoxListeOuvriers.Controls.Add(txtRechercheOuvrier);
            groupBoxListeOuvriers.Controls.Add(lblRechercheOuvrier);
            groupBoxListeOuvriers.Controls.Add(listViewOuvriers);
            groupBoxListeOuvriers.Dock = DockStyle.Fill;
            groupBoxListeOuvriers.Location = new Point(0, 0);
            groupBoxListeOuvriers.Name = "groupBoxListeOuvriers";
            groupBoxListeOuvriers.Size = new Size(350, 511);
            groupBoxListeOuvriers.TabIndex = 0;
            groupBoxListeOuvriers.TabStop = false;
            groupBoxListeOuvriers.Text = "Liste des Ouvriers";
            // 
            // lblStatutOuvriers
            // 
            lblStatutOuvriers.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatutOuvriers.AutoSize = true;
            lblStatutOuvriers.Location = new Point(6, 487);
            lblStatutOuvriers.Name = "lblStatutOuvriers";
            lblStatutOuvriers.Size = new Size(66, 15);
            lblStatutOuvriers.TabIndex = 6;
            lblStatutOuvriers.Text = "0 ouvrier(s)";
            // 
            // btnSupprimerOuvrier
            // 
            btnSupprimerOuvrier.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSupprimerOuvrier.Location = new Point(269, 455);
            btnSupprimerOuvrier.Name = "btnSupprimerOuvrier";
            btnSupprimerOuvrier.Size = new Size(75, 23);
            btnSupprimerOuvrier.TabIndex = 5;
            btnSupprimerOuvrier.Text = "🗑️ Supprimer";
            btnSupprimerOuvrier.UseVisualStyleBackColor = true;
            btnSupprimerOuvrier.Click += btnSupprimerOuvrier_Click;
            // 
            // btnModifierOuvrier
            // 
            btnModifierOuvrier.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnModifierOuvrier.Location = new Point(188, 455);
            btnModifierOuvrier.Name = "btnModifierOuvrier";
            btnModifierOuvrier.Size = new Size(75, 23);
            btnModifierOuvrier.TabIndex = 4;
            btnModifierOuvrier.Text = "✏️ Modifier";
            btnModifierOuvrier.UseVisualStyleBackColor = true;
            btnModifierOuvrier.Click += btnModifierOuvrier_Click;
            // 
            // btnNouveauOuvrier
            // 
            btnNouveauOuvrier.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnNouveauOuvrier.Location = new Point(107, 455);
            btnNouveauOuvrier.Name = "btnNouveauOuvrier";
            btnNouveauOuvrier.Size = new Size(75, 23);
            btnNouveauOuvrier.TabIndex = 3;
            btnNouveauOuvrier.Text = "➕ Nouveau";
            btnNouveauOuvrier.UseVisualStyleBackColor = true;
            btnNouveauOuvrier.Click += btnNouveauOuvrier_Click;
            // 
            // txtRechercheOuvrier
            // 
            txtRechercheOuvrier.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRechercheOuvrier.Location = new Point(72, 22);
            txtRechercheOuvrier.Name = "txtRechercheOuvrier";
            txtRechercheOuvrier.Size = new Size(272, 23);
            txtRechercheOuvrier.TabIndex = 2;
            txtRechercheOuvrier.TextChanged += txtRechercheOuvrier_TextChanged;
            // 
            // lblRechercheOuvrier
            // 
            lblRechercheOuvrier.AutoSize = true;
            lblRechercheOuvrier.Location = new Point(6, 25);
            lblRechercheOuvrier.Name = "lblRechercheOuvrier";
            lblRechercheOuvrier.Size = new Size(65, 15);
            lblRechercheOuvrier.TabIndex = 1;
            lblRechercheOuvrier.Text = "Recherche:";
            // 
            // listViewOuvriers
            // 
            listViewOuvriers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewOuvriers.Columns.AddRange(new ColumnHeader[] { colOuvrierId, colNomPrenom, colCoutJournalier, colNombreCompetences });
            listViewOuvriers.FullRowSelect = true;
            listViewOuvriers.GridLines = true;
            listViewOuvriers.Location = new Point(6, 51);
            listViewOuvriers.MultiSelect = false;
            listViewOuvriers.Name = "listViewOuvriers";
            listViewOuvriers.Size = new Size(338, 398);
            listViewOuvriers.TabIndex = 0;
            listViewOuvriers.UseCompatibleStateImageBehavior = false;
            listViewOuvriers.View = View.Details;
            listViewOuvriers.SelectedIndexChanged += listViewOuvriers_SelectedIndexChanged;
            // 
            // colOuvrierId
            // 
            colOuvrierId.Text = "ID";
            // 
            // colNomPrenom
            // 
            colNomPrenom.Text = "Nom Prénom";
            colNomPrenom.Width = 140;
            // 
            // colCoutJournalier
            // 
            colCoutJournalier.Text = "Coût/j";
            // 
            // colNombreCompetences
            // 
            colNombreCompetences.Text = "Comp.";
            colNombreCompetences.Width = 50;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(groupBoxDetailsOuvrier);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupBoxCompetences);
            splitContainer2.Size = new Size(630, 511);
            splitContainer2.SplitterDistance = 150;
            splitContainer2.TabIndex = 0;
            // 
            // groupBoxDetailsOuvrier
            // 
            groupBoxDetailsOuvrier.Controls.Add(numCoutJournalier);
            groupBoxDetailsOuvrier.Controls.Add(lblCoutJournalier);
            groupBoxDetailsOuvrier.Controls.Add(txtPrenom);
            groupBoxDetailsOuvrier.Controls.Add(lblPrenom);
            groupBoxDetailsOuvrier.Controls.Add(txtNom);
            groupBoxDetailsOuvrier.Controls.Add(lblNom);
            groupBoxDetailsOuvrier.Controls.Add(txtOuvrierId);
            groupBoxDetailsOuvrier.Controls.Add(lblOuvrierId);
            groupBoxDetailsOuvrier.Dock = DockStyle.Fill;
            groupBoxDetailsOuvrier.Location = new Point(0, 0);
            groupBoxDetailsOuvrier.Name = "groupBoxDetailsOuvrier";
            groupBoxDetailsOuvrier.Size = new Size(630, 150);
            groupBoxDetailsOuvrier.TabIndex = 0;
            groupBoxDetailsOuvrier.TabStop = false;
            groupBoxDetailsOuvrier.Text = "Détails de l'Ouvrier";
            // 
            // numCoutJournalier
            // 
            numCoutJournalier.Location = new Point(120, 102);
            numCoutJournalier.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numCoutJournalier.Name = "numCoutJournalier";
            numCoutJournalier.Size = new Size(100, 23);
            numCoutJournalier.TabIndex = 7;
            numCoutJournalier.ValueChanged += DetailOuvrier_Changed;
            // 
            // lblCoutJournalier
            // 
            lblCoutJournalier.AutoSize = true;
            lblCoutJournalier.Location = new Point(16, 104);
            lblCoutJournalier.Name = "lblCoutJournalier";
            lblCoutJournalier.Size = new Size(106, 15);
            lblCoutJournalier.TabIndex = 6;
            lblCoutJournalier.Text = "Coût journalier (€):";
            // 
            // txtPrenom
            // 
            txtPrenom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPrenom.Location = new Point(120, 73);
            txtPrenom.Name = "txtPrenom";
            txtPrenom.Size = new Size(497, 23);
            txtPrenom.TabIndex = 5;
            txtPrenom.TextChanged += DetailOuvrier_Changed;
            // 
            // lblPrenom
            // 
            lblPrenom.AutoSize = true;
            lblPrenom.Location = new Point(16, 76);
            lblPrenom.Name = "lblPrenom";
            lblPrenom.Size = new Size(52, 15);
            lblPrenom.TabIndex = 4;
            lblPrenom.Text = "Prénom:";
            // 
            // txtNom
            // 
            txtNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNom.Location = new Point(120, 44);
            txtNom.Name = "txtNom";
            txtNom.Size = new Size(497, 23);
            txtNom.TabIndex = 3;
            txtNom.TextChanged += DetailOuvrier_Changed;
            // 
            // lblNom
            // 
            lblNom.AutoSize = true;
            lblNom.Location = new Point(16, 47);
            lblNom.Name = "lblNom";
            lblNom.Size = new Size(37, 15);
            lblNom.TabIndex = 2;
            lblNom.Text = "Nom:";
            // 
            // txtOuvrierId
            // 
            txtOuvrierId.Location = new Point(120, 15);
            txtOuvrierId.Name = "txtOuvrierId";
            txtOuvrierId.Size = new Size(150, 23);
            txtOuvrierId.TabIndex = 1;
            txtOuvrierId.TextChanged += DetailOuvrier_Changed;
            // 
            // lblOuvrierId
            // 
            lblOuvrierId.AutoSize = true;
            lblOuvrierId.Location = new Point(16, 18);
            lblOuvrierId.Name = "lblOuvrierId";
            lblOuvrierId.Size = new Size(21, 15);
            lblOuvrierId.TabIndex = 0;
            lblOuvrierId.Text = "ID:";
            // 
            // groupBoxCompetences
            // 
            groupBoxCompetences.Controls.Add(lblStatutCompetences);
            groupBoxCompetences.Controls.Add(btnSupprimerCompetence);
            groupBoxCompetences.Controls.Add(btnModifierCompetence);
            groupBoxCompetences.Controls.Add(btnAjouterCompetence);
            groupBoxCompetences.Controls.Add(listViewCompetences);
            groupBoxCompetences.Dock = DockStyle.Fill;
            groupBoxCompetences.Location = new Point(0, 0);
            groupBoxCompetences.Name = "groupBoxCompetences";
            groupBoxCompetences.Size = new Size(630, 357);
            groupBoxCompetences.TabIndex = 0;
            groupBoxCompetences.TabStop = false;
            groupBoxCompetences.Text = "Compétences";
            // 
            // lblStatutCompetences
            // 
            lblStatutCompetences.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatutCompetences.AutoSize = true;
            lblStatutCompetences.Location = new Point(16, 333);
            lblStatutCompetences.Name = "lblStatutCompetences";
            lblStatutCompetences.Size = new Size(95, 15);
            lblStatutCompetences.TabIndex = 4;
            lblStatutCompetences.Text = "0 compétence(s)";
            // 
            // btnSupprimerCompetence
            // 
            btnSupprimerCompetence.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSupprimerCompetence.Location = new Point(542, 301);
            btnSupprimerCompetence.Name = "btnSupprimerCompetence";
            btnSupprimerCompetence.Size = new Size(75, 23);
            btnSupprimerCompetence.TabIndex = 3;
            btnSupprimerCompetence.Text = "🗑️ Supprimer";
            btnSupprimerCompetence.UseVisualStyleBackColor = true;
            btnSupprimerCompetence.Click += btnSupprimerCompetence_Click;
            // 
            // btnModifierCompetence
            // 
            btnModifierCompetence.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnModifierCompetence.Location = new Point(461, 301);
            btnModifierCompetence.Name = "btnModifierCompetence";
            btnModifierCompetence.Size = new Size(75, 23);
            btnModifierCompetence.TabIndex = 2;
            btnModifierCompetence.Text = "✏️ Modifier";
            btnModifierCompetence.UseVisualStyleBackColor = true;
            btnModifierCompetence.Click += btnModifierCompetence_Click;
            // 
            // btnAjouterCompetence
            // 
            btnAjouterCompetence.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAjouterCompetence.Location = new Point(380, 301);
            btnAjouterCompetence.Name = "btnAjouterCompetence";
            btnAjouterCompetence.Size = new Size(75, 23);
            btnAjouterCompetence.TabIndex = 1;
            btnAjouterCompetence.Text = "➕ Ajouter";
            btnAjouterCompetence.UseVisualStyleBackColor = true;
            btnAjouterCompetence.Click += btnAjouterCompetence_Click;
            // 
            // listViewCompetences
            // 
            listViewCompetences.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewCompetences.Columns.AddRange(new ColumnHeader[] { colMetierId, colNomMetier });
            listViewCompetences.FullRowSelect = true;
            listViewCompetences.GridLines = true;
            listViewCompetences.Location = new Point(16, 22);
            listViewCompetences.MultiSelect = false;
            listViewCompetences.Name = "listViewCompetences";
            listViewCompetences.Size = new Size(601, 273);
            listViewCompetences.TabIndex = 0;
            listViewCompetences.UseCompatibleStateImageBehavior = false;
            listViewCompetences.View = View.Details;
            listViewCompetences.SelectedIndexChanged += listViewCompetences_SelectedIndexChanged;
            // 
            // colMetierId
            // 
            colMetierId.Text = "ID Métier";
            colMetierId.Width = 100;
            // 
            // colNomMetier
            // 
            colNomMetier.Text = "Nom du Métier";
            colNomMetier.Width = 200;
            // 
            // groupBoxActions
            // 
            groupBoxActions.Controls.Add(btnImportExcelSAP);
            groupBoxActions.Controls.Add(btnExporter);
            groupBoxActions.Controls.Add(btnImporter);
            groupBoxActions.Controls.Add(btnFermer);
            groupBoxActions.Dock = DockStyle.Bottom;
            groupBoxActions.Location = new Point(0, 511);
            groupBoxActions.Name = "groupBoxActions";
            groupBoxActions.Size = new Size(984, 50);
            groupBoxActions.TabIndex = 1;
            groupBoxActions.TabStop = false;
            groupBoxActions.Text = "Actions";
            // 
            // btnImportExcelSAP
            // 
            btnImportExcelSAP.Location = new Point(59, 19);
            btnImportExcelSAP.Name = "btnImportExcelSAP";
            btnImportExcelSAP.Size = new Size(120, 23);
            btnImportExcelSAP.TabIndex = 3;
            btnImportExcelSAP.Text = "📥 Import Excel SAP";
            btnImportExcelSAP.UseVisualStyleBackColor = true;
            btnImportExcelSAP.Click += btnImportExcelSAP_Click;
            // 
            // btnExporter
            // 
            btnExporter.Location = new Point(291, 19);
            btnExporter.Name = "btnExporter";
            btnExporter.Size = new Size(100, 23);
            btnExporter.TabIndex = 2;
            btnExporter.Text = "📤 Export CSV";
            btnExporter.UseVisualStyleBackColor = true;
            btnExporter.Click += btnExporter_Click;
            // 
            // btnImporter
            // 
            btnImporter.Location = new Point(185, 19);
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
            btnFermer.Location = new Point(897, 19);
            btnFermer.Name = "btnFermer";
            btnFermer.Size = new Size(75, 23);
            btnFermer.TabIndex = 0;
            btnFermer.Text = "Fermer";
            btnFermer.UseVisualStyleBackColor = true;
            btnFermer.Click += btnFermer_Click;
            // 
            // OuvrierForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 561);
            Controls.Add(splitContainer1);
            Controls.Add(groupBoxActions);
            Name = "OuvrierForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestion des Ouvriers";
            Load += OuvrierForm_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBoxListeOuvriers.ResumeLayout(false);
            groupBoxListeOuvriers.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBoxDetailsOuvrier.ResumeLayout(false);
            groupBoxDetailsOuvrier.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCoutJournalier).EndInit();
            groupBoxCompetences.ResumeLayout(false);
            groupBoxCompetences.PerformLayout();
            groupBoxActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private GroupBox groupBoxListeOuvriers;
        private GroupBox groupBoxDetailsOuvrier;
        private GroupBox groupBoxCompetences;
        private GroupBox groupBoxActions;
        private ListView listViewOuvriers;
        private ColumnHeader colOuvrierId;
        private ColumnHeader colNomPrenom;
        private ColumnHeader colCoutJournalier;
        private ColumnHeader colNombreCompetences;
        private TextBox txtRechercheOuvrier;
        private Label lblRechercheOuvrier;
        private Button btnSupprimerOuvrier;
        private Button btnModifierOuvrier;
        private Button btnNouveauOuvrier;
        private Label lblStatutOuvriers;
        private TextBox txtOuvrierId;
        private Label lblOuvrierId;
        private TextBox txtNom;
        private Label lblNom;
        private TextBox txtPrenom;
        private Label lblPrenom;
        private NumericUpDown numCoutJournalier;
        private Label lblCoutJournalier;
        private ListView listViewCompetences;
        private ColumnHeader colMetierId;
        private ColumnHeader colNomMetier;
        private Button btnAjouterCompetence;
        private Button btnModifierCompetence;
        private Button btnSupprimerCompetence;
        private Label lblStatutCompetences;
        private Button btnImporter;
        private Button btnExporter;
        private Button btnImportExcelSAP;
        private Button btnFermer;
    }
}