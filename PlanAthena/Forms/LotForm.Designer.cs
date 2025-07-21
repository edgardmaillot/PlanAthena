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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBoxListe = new System.Windows.Forms.GroupBox();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.lblStatut = new System.Windows.Forms.Label();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnModifier = new System.Windows.Forms.Button();
            this.btnNouveau = new System.Windows.Forms.Button();
            this.listViewLots = new System.Windows.Forms.ListView();
            this.colLotId = new System.Windows.Forms.ColumnHeader();
            this.colNom = new System.Windows.Forms.ColumnHeader();
            this.colPriorite = new System.Windows.Forms.ColumnHeader();
            this.groupBoxDetails = new System.Windows.Forms.GroupBox();
            this.btnParcourirPlan = new System.Windows.Forms.Button();
            this.txtCheminFichierPlan = new System.Windows.Forms.TextBox();
            this.lblCheminPlan = new System.Windows.Forms.Label();
            this.numPriorite = new System.Windows.Forms.NumericUpDown();
            this.lblPriorite = new System.Windows.Forms.Label();
            this.txtNom = new System.Windows.Forms.TextBox();
            this.lblNom = new System.Windows.Forms.Label();
            this.txtLotId = new System.Windows.Forms.TextBox();
            this.lblLotId = new System.Windows.Forms.Label();
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.btnFermer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBoxListe.SuspendLayout();
            this.groupBoxDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPriorite)).BeginInit();
            this.groupBoxActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Size = new System.Drawing.Size(784, 411);
            this.splitContainer1.SplitterDistance = 380;
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxListe);
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxDetails);
            // 
            // groupBoxListe
            // 
            this.groupBoxListe.Controls.Add(this.btnAnnuler);
            this.groupBoxListe.Controls.Add(this.lblStatut);
            this.groupBoxListe.Controls.Add(this.btnSupprimer);
            this.groupBoxListe.Controls.Add(this.btnModifier);
            this.groupBoxListe.Controls.Add(this.btnNouveau);
            this.groupBoxListe.Controls.Add(this.listViewLots);
            this.groupBoxListe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxListe.Name = "groupBoxListe";
            this.groupBoxListe.Size = new System.Drawing.Size(380, 411);
            this.groupBoxListe.TabStop = false;
            this.groupBoxListe.Text = "Liste des Lots";
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnnuler.Location = new System.Drawing.Point(218, 382);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(75, 23);
            this.btnAnnuler.TabIndex = 7;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = true;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // lblStatut
            // 
            this.lblStatut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatut.AutoSize = true;
            this.lblStatut.Location = new System.Drawing.Point(6, 387);
            this.lblStatut.Name = "lblStatut";
            this.lblStatut.Size = new System.Drawing.Size(52, 15);
            this.lblStatut.TabIndex = 6;
            this.lblStatut.Text = "0 lot(s)";
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSupprimer.Location = new System.Drawing.Point(299, 353);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(75, 23);
            this.btnSupprimer.TabIndex = 5;
            this.btnSupprimer.Text = "🗑️ Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = true;
            this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click);
            // 
            // btnModifier
            // 
            this.btnModifier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModifier.Location = new System.Drawing.Point(218, 353);
            this.btnModifier.Name = "btnModifier";
            this.btnModifier.Size = new System.Drawing.Size(75, 23);
            this.btnModifier.TabIndex = 4;
            this.btnModifier.Text = "✏️ Modifier";
            this.btnModifier.UseVisualStyleBackColor = true;
            this.btnModifier.Click += new System.EventHandler(this.btnModifier_Click);
            // 
            // btnNouveau
            // 
            this.btnNouveau.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNouveau.Location = new System.Drawing.Point(137, 353);
            this.btnNouveau.Name = "btnNouveau";
            this.btnNouveau.Size = new System.Drawing.Size(75, 23);
            this.btnNouveau.TabIndex = 3;
            this.btnNouveau.Text = "➕ Nouveau";
            this.btnNouveau.UseVisualStyleBackColor = true;
            this.btnNouveau.Click += new System.EventHandler(this.btnNouveau_Click);
            // 
            // listViewLots
            // 
            this.listViewLots.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewLots.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colLotId,
            this.colNom,
            this.colPriorite});
            this.listViewLots.FullRowSelect = true;
            this.listViewLots.GridLines = true;
            this.listViewLots.Location = new System.Drawing.Point(6, 22);
            this.listViewLots.MultiSelect = false;
            this.listViewLots.Name = "listViewLots";
            this.listViewLots.Size = new System.Drawing.Size(368, 325);
            this.listViewLots.TabIndex = 0;
            this.listViewLots.UseCompatibleStateImageBehavior = false;
            this.listViewLots.View = System.Windows.Forms.View.Details;
            this.listViewLots.SelectedIndexChanged += new System.EventHandler(this.listViewLots_SelectedIndexChanged);
            // 
            // colLotId
            // 
            this.colLotId.Text = "ID";
            this.colLotId.Width = 80;
            // 
            // colNom
            // 
            this.colNom.Text = "Nom";
            this.colNom.Width = 180;
            // 
            // colPriorite
            // 
            this.colPriorite.Text = "Priorité";
            this.colPriorite.Width = 70;
            // 
            // groupBoxDetails
            // 
            this.groupBoxDetails.Controls.Add(this.btnParcourirPlan);
            this.groupBoxDetails.Controls.Add(this.txtCheminFichierPlan);
            this.groupBoxDetails.Controls.Add(this.lblCheminPlan);
            this.groupBoxDetails.Controls.Add(this.numPriorite);
            this.groupBoxDetails.Controls.Add(this.lblPriorite);
            this.groupBoxDetails.Controls.Add(this.txtNom);
            this.groupBoxDetails.Controls.Add(this.lblNom);
            this.groupBoxDetails.Controls.Add(this.txtLotId);
            this.groupBoxDetails.Controls.Add(this.lblLotId);
            this.groupBoxDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDetails.Location = new System.Drawing.Point(0, 0);
            this.groupBoxDetails.Name = "groupBoxDetails";
            this.groupBoxDetails.Size = new System.Drawing.Size(400, 411);
            this.groupBoxDetails.TabStop = false;
            this.groupBoxDetails.Text = "Détails du Lot";
            // 
            // btnParcourirPlan
            // 
            this.btnParcourirPlan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnParcourirPlan.Location = new System.Drawing.Point(313, 112);
            this.btnParcourirPlan.Name = "btnParcourirPlan";
            this.btnParcourirPlan.Size = new System.Drawing.Size(75, 23);
            this.btnParcourirPlan.TabIndex = 8;
            this.btnParcourirPlan.Text = "Parcourir...";
            this.btnParcourirPlan.UseVisualStyleBackColor = true;
            this.btnParcourirPlan.Click += new System.EventHandler(this.btnParcourirPlan_Click);
            // 
            // txtCheminFichierPlan
            // 
            this.txtCheminFichierPlan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCheminFichierPlan.Location = new System.Drawing.Point(70, 112);
            this.txtCheminFichierPlan.Name = "txtCheminFichierPlan";
            this.txtCheminFichierPlan.ReadOnly = true;
            this.txtCheminFichierPlan.Size = new System.Drawing.Size(237, 23);
            this.txtCheminFichierPlan.TabIndex = 7;
            // 
            // lblCheminPlan
            // 
            this.lblCheminPlan.AutoSize = true;
            this.lblCheminPlan.Location = new System.Drawing.Point(16, 115);
            this.lblCheminPlan.Name = "lblCheminPlan";
            this.lblCheminPlan.Size = new System.Drawing.Size(32, 15);
            this.lblCheminPlan.TabIndex = 6;
            this.lblCheminPlan.Text = "Plan:";
            // 
            // numPriorite
            // 
            this.numPriorite.Location = new System.Drawing.Point(70, 83);
            this.numPriorite.Name = "numPriorite";
            this.numPriorite.Size = new System.Drawing.Size(120, 23);
            this.numPriorite.TabIndex = 5;
            // 
            // lblPriorite
            // 
            this.lblPriorite.AutoSize = true;
            this.lblPriorite.Location = new System.Drawing.Point(16, 85);
            this.lblPriorite.Name = "lblPriorite";
            this.lblPriorite.Size = new System.Drawing.Size(48, 15);
            this.lblPriorite.TabIndex = 4;
            this.lblPriorite.Text = "Priorité:";
            // 
            // txtNom
            // 
            this.txtNom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNom.Location = new System.Drawing.Point(70, 54);
            this.txtNom.Name = "txtNom";
            this.txtNom.Size = new System.Drawing.Size(318, 23);
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
            // txtLotId
            // 
            this.txtLotId.Location = new System.Drawing.Point(70, 25);
            this.txtLotId.Name = "txtLotId";
            this.txtLotId.Size = new System.Drawing.Size(190, 23);
            this.txtLotId.TabIndex = 1;
            // 
            // lblLotId
            // 
            this.lblLotId.AutoSize = true;
            this.lblLotId.Location = new System.Drawing.Point(16, 28);
            this.lblLotId.Name = "lblLotId";
            this.lblLotId.Size = new System.Drawing.Size(21, 15);
            this.lblLotId.TabIndex = 0;
            this.lblLotId.Text = "ID:";
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Controls.Add(this.btnFermer);
            this.groupBoxActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxActions.Location = new System.Drawing.Point(0, 411);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Size = new System.Drawing.Size(784, 50);
            this.groupBoxActions.TabStop = false;
            // 
            // btnFermer
            // 
            this.btnFermer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFermer.Location = new System.Drawing.Point(697, 19);
            this.btnFermer.Name = "btnFermer";
            this.btnFermer.Size = new System.Drawing.Size(75, 23);
            this.btnFermer.TabIndex = 0;
            this.btnFermer.Text = "Fermer";
            this.btnFermer.UseVisualStyleBackColor = true;
            this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);
            // 
            // LotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBoxActions);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "LotForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gestion des Lots";
            this.Load += new System.EventHandler(this.LotForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBoxListe.ResumeLayout(false);
            this.groupBoxListe.PerformLayout();
            this.groupBoxDetails.ResumeLayout(false);
            this.groupBoxDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPriorite)).EndInit();
            this.groupBoxActions.ResumeLayout(false);
            this.ResumeLayout(false);
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
    }
}