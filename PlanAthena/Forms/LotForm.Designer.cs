namespace PlanAthena.Forms
{
    partial class LotForm
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
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            groupBoxListe = new GroupBox();
            btnAnnuler = new Button();
            lblStatut = new Label();
            btnSupprimer = new Button();
            btnModifier = new Button();
            btnNouveau = new Button();
            listViewLots = new ListView();
            colLotId = new ColumnHeader();
            colNom = new ColumnHeader();
            colPriorite = new ColumnHeader();
            groupBoxDetails = new GroupBox();
            label1 = new Label();
            cmbPhases = new ComboBox();
            btnParcourirPlan = new Button();
            txtCheminFichierPlan = new TextBox();
            lblCheminPlan = new Label();
            numPriorite = new NumericUpDown();
            lblPriorite = new Label();
            txtNom = new TextBox();
            lblNom = new Label();
            txtLotId = new TextBox();
            lblLotId = new Label();
            groupBoxActions = new GroupBox();
            btnFermer = new Button();
            lotBindingSource = new BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBoxListe.SuspendLayout();
            groupBoxDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPriorite).BeginInit();
            groupBoxActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lotBindingSource).BeginInit();
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
            splitContainer1.Size = new Size(784, 411);
            splitContainer1.SplitterDistance = 379;
            splitContainer1.TabIndex = 0;
            // 
            // groupBoxListe
            // 
            groupBoxListe.Controls.Add(btnAnnuler);
            groupBoxListe.Controls.Add(lblStatut);
            groupBoxListe.Controls.Add(btnSupprimer);
            groupBoxListe.Controls.Add(btnModifier);
            groupBoxListe.Controls.Add(btnNouveau);
            groupBoxListe.Controls.Add(listViewLots);
            groupBoxListe.Dock = DockStyle.Fill;
            groupBoxListe.Location = new Point(0, 0);
            groupBoxListe.Name = "groupBoxListe";
            groupBoxListe.Size = new Size(379, 411);
            groupBoxListe.TabIndex = 0;
            groupBoxListe.TabStop = false;
            groupBoxListe.Text = "Liste des Lots";
            // 
            // btnAnnuler
            // 
            btnAnnuler.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAnnuler.Location = new Point(217, 382);
            btnAnnuler.Name = "btnAnnuler";
            btnAnnuler.Size = new Size(75, 23);
            btnAnnuler.TabIndex = 7;
            btnAnnuler.Text = "Annuler";
            btnAnnuler.UseVisualStyleBackColor = true;
            btnAnnuler.Click += btnAnnuler_Click;
            // 
            // lblStatut
            // 
            lblStatut.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatut.AutoSize = true;
            lblStatut.Location = new Point(6, 387);
            lblStatut.Name = "lblStatut";
            lblStatut.Size = new Size(43, 15);
            lblStatut.TabIndex = 6;
            lblStatut.Text = "0 lot(s)";
            // 
            // btnSupprimer
            // 
            btnSupprimer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSupprimer.Location = new Point(298, 353);
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
            btnModifier.Location = new Point(217, 353);
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
            btnNouveau.Location = new Point(136, 353);
            btnNouveau.Name = "btnNouveau";
            btnNouveau.Size = new Size(75, 23);
            btnNouveau.TabIndex = 3;
            btnNouveau.Text = "➕ Nouveau";
            btnNouveau.UseVisualStyleBackColor = true;
            btnNouveau.Click += btnNouveau_Click;
            // 
            // listViewLots
            // 
            listViewLots.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewLots.Columns.AddRange(new ColumnHeader[] { colLotId, colNom, colPriorite });
            listViewLots.FullRowSelect = true;
            listViewLots.GridLines = true;
            listViewLots.Location = new Point(6, 22);
            listViewLots.MultiSelect = false;
            listViewLots.Name = "listViewLots";
            listViewLots.Size = new Size(367, 325);
            listViewLots.TabIndex = 0;
            listViewLots.UseCompatibleStateImageBehavior = false;
            listViewLots.View = View.Details;
            listViewLots.SelectedIndexChanged += listViewLots_SelectedIndexChanged;
            // 
            // colLotId
            // 
            colLotId.Text = "ID";
            colLotId.Width = 80;
            // 
            // colNom
            // 
            colNom.Text = "Nom";
            colNom.Width = 180;
            // 
            // colPriorite
            // 
            colPriorite.Text = "Priorité";
            colPriorite.Width = 70;
            // 
            // groupBoxDetails
            // 
            groupBoxDetails.Controls.Add(label1);
            groupBoxDetails.Controls.Add(cmbPhases);
            groupBoxDetails.Controls.Add(btnParcourirPlan);
            groupBoxDetails.Controls.Add(txtCheminFichierPlan);
            groupBoxDetails.Controls.Add(lblCheminPlan);
            groupBoxDetails.Controls.Add(numPriorite);
            groupBoxDetails.Controls.Add(lblPriorite);
            groupBoxDetails.Controls.Add(txtNom);
            groupBoxDetails.Controls.Add(lblNom);
            groupBoxDetails.Controls.Add(txtLotId);
            groupBoxDetails.Controls.Add(lblLotId);
            groupBoxDetails.Dock = DockStyle.Fill;
            groupBoxDetails.Location = new Point(0, 0);
            groupBoxDetails.Name = "groupBoxDetails";
            groupBoxDetails.Size = new Size(401, 411);
            groupBoxDetails.TabIndex = 0;
            groupBoxDetails.TabStop = false;
            groupBoxDetails.Text = "Détails du Lot";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(162, 85);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 10;
            label1.Text = "Phase active";
            // 
            // cmbPhases
            // 
            cmbPhases.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPhases.FormattingEnabled = true;
            cmbPhases.Location = new Point(240, 82);
            cmbPhases.Name = "cmbPhases";
            cmbPhases.Size = new Size(149, 23);
            cmbPhases.TabIndex = 9;
            // 
            // btnParcourirPlan
            // 
            btnParcourirPlan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnParcourirPlan.Location = new Point(314, 112);
            btnParcourirPlan.Name = "btnParcourirPlan";
            btnParcourirPlan.Size = new Size(75, 23);
            btnParcourirPlan.TabIndex = 8;
            btnParcourirPlan.Text = "Parcourir...";
            btnParcourirPlan.UseVisualStyleBackColor = true;
            btnParcourirPlan.Click += btnParcourirPlan_Click;
            // 
            // txtCheminFichierPlan
            // 
            txtCheminFichierPlan.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCheminFichierPlan.Location = new Point(70, 112);
            txtCheminFichierPlan.Name = "txtCheminFichierPlan";
            txtCheminFichierPlan.ReadOnly = true;
            txtCheminFichierPlan.Size = new Size(238, 23);
            txtCheminFichierPlan.TabIndex = 7;
            // 
            // lblCheminPlan
            // 
            lblCheminPlan.AutoSize = true;
            lblCheminPlan.Location = new Point(16, 115);
            lblCheminPlan.Name = "lblCheminPlan";
            lblCheminPlan.Size = new Size(33, 15);
            lblCheminPlan.TabIndex = 6;
            lblCheminPlan.Text = "Plan:";
            // 
            // numPriorite
            // 
            numPriorite.Location = new Point(70, 83);
            numPriorite.Name = "numPriorite";
            numPriorite.Size = new Size(73, 23);
            numPriorite.TabIndex = 5;
            // 
            // lblPriorite
            // 
            lblPriorite.AutoSize = true;
            lblPriorite.Location = new Point(16, 85);
            lblPriorite.Name = "lblPriorite";
            lblPriorite.Size = new Size(48, 15);
            lblPriorite.TabIndex = 4;
            lblPriorite.Text = "Priorité:";
            // 
            // txtNom
            // 
            txtNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNom.Location = new Point(70, 54);
            txtNom.Name = "txtNom";
            txtNom.Size = new Size(319, 23);
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
            // txtLotId
            // 
            txtLotId.Location = new Point(70, 25);
            txtLotId.Name = "txtLotId";
            txtLotId.Size = new Size(190, 23);
            txtLotId.TabIndex = 1;
            // 
            // lblLotId
            // 
            lblLotId.AutoSize = true;
            lblLotId.Location = new Point(16, 28);
            lblLotId.Name = "lblLotId";
            lblLotId.Size = new Size(21, 15);
            lblLotId.TabIndex = 0;
            lblLotId.Text = "ID:";
            // 
            // groupBoxActions
            // 
            groupBoxActions.Controls.Add(btnFermer);
            groupBoxActions.Dock = DockStyle.Bottom;
            groupBoxActions.Location = new Point(0, 411);
            groupBoxActions.Name = "groupBoxActions";
            groupBoxActions.Size = new Size(784, 50);
            groupBoxActions.TabIndex = 1;
            groupBoxActions.TabStop = false;
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
            // lotBindingSource
            // 
            lotBindingSource.DataSource = typeof(Data.Lot);
            // 
            // LotForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 461);
            Controls.Add(splitContainer1);
            Controls.Add(groupBoxActions);
            MinimumSize = new Size(600, 400);
            Name = "LotForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestion des Lots";
            Load += LotForm_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBoxListe.ResumeLayout(false);
            groupBoxListe.PerformLayout();
            groupBoxDetails.ResumeLayout(false);
            groupBoxDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPriorite).EndInit();
            groupBoxActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lotBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBoxListe;
        private System.Windows.Forms.ListView listViewLots;
        private System.Windows.Forms.ColumnHeader colLotId;
        private System.Windows.Forms.ColumnHeader colNom;
        private System.Windows.Forms.Button btnNouveau;
        private System.Windows.Forms.Button btnModifier;
        private System.Windows.Forms.Button btnSupprimer;
        private System.Windows.Forms.Label lblStatut;
        private System.Windows.Forms.GroupBox groupBoxDetails;
        private System.Windows.Forms.TextBox txtLotId;
        private System.Windows.Forms.Label lblLotId;
        private System.Windows.Forms.TextBox txtNom;
        private System.Windows.Forms.Label lblNom;
        private System.Windows.Forms.NumericUpDown numPriorite;
        private System.Windows.Forms.Label lblPriorite;
        private System.Windows.Forms.GroupBox groupBoxActions;
        private System.Windows.Forms.Button btnFermer;
        private System.Windows.Forms.ColumnHeader colPriorite;
        private System.Windows.Forms.Button btnParcourirPlan;
        private System.Windows.Forms.TextBox txtCheminFichierPlan;
        private System.Windows.Forms.Label lblCheminPlan;
        private System.Windows.Forms.Button btnAnnuler;
        private ComboBox cmbPhases;
        private BindingSource lotBindingSource;
        private Label label1;
    }
}