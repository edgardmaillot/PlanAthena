namespace PlanAthena.Forms
{
    partial class TacheDetailForm
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
            this.txtTacheId = new System.Windows.Forms.TextBox();
            this.txtTacheNom = new System.Windows.Forms.TextBox();
            this.numHeuresHomme = new System.Windows.Forms.NumericUpDown();
            this.cmbMetier = new System.Windows.Forms.ComboBox();
            this.chkListDependances = new System.Windows.Forms.CheckedListBox(); // MODIFIÉ
            this.cmbLotNom = new System.Windows.Forms.ComboBox();
            this.numLotPriorite = new System.Windows.Forms.NumericUpDown();
            this.cmbBlocNom = new System.Windows.Forms.ComboBox();
            this.numBlocCapacite = new System.Windows.Forms.NumericUpDown();
            this.btnSauvegarder = new System.Windows.Forms.Button();
            this.chkEstJalon = new System.Windows.Forms.CheckBox(); // NOUVEAU
            this.grpTache = new System.Windows.Forms.GroupBox();
            this.lblTacheId = new System.Windows.Forms.Label();
            this.lblTacheNom = new System.Windows.Forms.Label();
            this.lblHeures = new System.Windows.Forms.Label();
            this.lblMetier = new System.Windows.Forms.Label();
            this.grpLot = new System.Windows.Forms.GroupBox();
            this.lblLotNom = new System.Windows.Forms.Label();
            this.lblLotPrio = new System.Windows.Forms.Label();
            this.grpBloc = new System.Windows.Forms.GroupBox();
            this.lblBlocNom = new System.Windows.Forms.Label();
            this.lblBlocCap = new System.Windows.Forms.Label();
            this.grpDep = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.numHeuresHomme)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLotPriorite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlocCapacite)).BeginInit();
            this.grpTache.SuspendLayout();
            this.grpLot.SuspendLayout();
            this.grpBloc.SuspendLayout();
            this.grpDep.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTache
            // 
            this.grpTache.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTache.Controls.Add(this.lblTacheId);
            this.grpTache.Controls.Add(this.txtTacheId);
            this.grpTache.Controls.Add(this.lblTacheNom);
            this.grpTache.Controls.Add(this.txtTacheNom);
            this.grpTache.Controls.Add(this.lblHeures);
            this.grpTache.Controls.Add(this.numHeuresHomme);
            this.grpTache.Controls.Add(this.chkEstJalon);
            this.grpTache.Controls.Add(this.lblMetier);
            this.grpTache.Controls.Add(this.cmbMetier);
            this.grpTache.Location = new System.Drawing.Point(12, 12);
            this.grpTache.Name = "grpTache";
            this.grpTache.Size = new System.Drawing.Size(360, 160); // MODIFIÉ
            this.grpTache.TabIndex = 0;
            this.grpTache.TabStop = false;
            this.grpTache.Text = "Informations Tâche";
            // 
            // lblTacheId
            // 
            this.lblTacheId.AutoSize = true;
            this.lblTacheId.Location = new System.Drawing.Point(10, 25);
            this.lblTacheId.Name = "lblTacheId";
            this.lblTacheId.Size = new System.Drawing.Size(60, 15);
            this.lblTacheId.TabIndex = 0;
            this.lblTacheId.Text = "ID Tâche:";
            // 
            // txtTacheId
            // 
            this.txtTacheId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTacheId.Location = new System.Drawing.Point(86, 22);
            this.txtTacheId.Name = "txtTacheId";
            this.txtTacheId.ReadOnly = true; // MODIFIÉ
            this.txtTacheId.Size = new System.Drawing.Size(268, 23);
            this.txtTacheId.TabIndex = 1;
            this.txtTacheId.BackColor = System.Drawing.SystemColors.Control; // AJOUTÉ
            this.txtTacheId.PlaceholderText = "Auto-généré"; // AJOUTÉ
            // 
            // lblTacheNom
            // 
            this.lblTacheNom.AutoSize = true;
            this.lblTacheNom.Location = new System.Drawing.Point(10, 54);
            this.lblTacheNom.Name = "lblTacheNom";
            this.lblTacheNom.Size = new System.Drawing.Size(37, 15);
            this.lblTacheNom.TabIndex = 2;
            this.lblTacheNom.Text = "Nom:";
            // 
            // txtTacheNom
            // 
            this.txtTacheNom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTacheNom.Location = new System.Drawing.Point(86, 51);
            this.txtTacheNom.Name = "txtTacheNom";
            this.txtTacheNom.Size = new System.Drawing.Size(268, 23);
            this.txtTacheNom.TabIndex = 3;
            // 
            // lblHeures
            // 
            this.lblHeures.AutoSize = true;
            this.lblHeures.Location = new System.Drawing.Point(10, 83);
            this.lblHeures.Name = "lblHeures";
            this.lblHeures.Size = new System.Drawing.Size(98, 15);
            this.lblHeures.TabIndex = 4;
            this.lblHeures.Text = "Durée estimée:";
            // 
            // numHeuresHomme
            // 
            this.numHeuresHomme.Location = new System.Drawing.Point(114, 80);
            this.numHeuresHomme.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numHeuresHomme.Name = "numHeuresHomme";
            this.numHeuresHomme.Size = new System.Drawing.Size(72, 23);
            this.numHeuresHomme.TabIndex = 5;
            this.numHeuresHomme.ValueChanged += new System.EventHandler(this.numHeuresHomme_ValueChanged); // AJOUTÉ
            // 
            // chkEstJalon
            // 
            this.chkEstJalon.AutoSize = true;
            this.chkEstJalon.Location = new System.Drawing.Point(10, 109);
            this.chkEstJalon.Name = "chkEstJalon";
            this.chkEstJalon.Size = new System.Drawing.Size(141, 19);
            this.chkEstJalon.TabIndex = 6;
            this.chkEstJalon.Text = "🏁 Jalon d'attente";
            this.chkEstJalon.UseVisualStyleBackColor = true;
            this.chkEstJalon.CheckedChanged += new System.EventHandler(this.chkEstJalon_CheckedChanged); // AJOUTÉ
            // 
            // lblMetier
            // 
            this.lblMetier.AutoSize = true;
            this.lblMetier.Location = new System.Drawing.Point(10, 135);
            this.lblMetier.Name = "lblMetier";
            this.lblMetier.Size = new System.Drawing.Size(46, 15);
            this.lblMetier.TabIndex = 7;
            this.lblMetier.Text = "Métier:";
            // 
            // cmbMetier
            // 
            this.cmbMetier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMetier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMetier.FormattingEnabled = true;
            this.cmbMetier.Location = new System.Drawing.Point(86, 131);
            this.cmbMetier.Name = "cmbMetier";
            this.cmbMetier.Size = new System.Drawing.Size(268, 23);
            this.cmbMetier.TabIndex = 8;
            this.cmbMetier.SelectedIndexChanged += new System.EventHandler(this.cmbMetier_SelectedIndexChanged); // AJOUTÉ
            // 
            // grpLot
            // 
            this.grpLot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLot.Controls.Add(this.lblLotNom);
            this.grpLot.Controls.Add(this.cmbLotNom);
            this.grpLot.Controls.Add(this.lblLotPrio);
            this.grpLot.Controls.Add(this.numLotPriorite);
            this.grpLot.Location = new System.Drawing.Point(12, 180); // MODIFIÉ
            this.grpLot.Name = "grpLot";
            this.grpLot.Size = new System.Drawing.Size(360, 55);
            this.grpLot.TabIndex = 1;
            this.grpLot.TabStop = false;
            this.grpLot.Text = "Informations Lot";
            // 
            // lblLotNom
            // 
            this.lblLotNom.AutoSize = true;
            this.lblLotNom.Location = new System.Drawing.Point(10, 25);
            this.lblLotNom.Name = "lblLotNom";
            this.lblLotNom.Size = new System.Drawing.Size(37, 15);
            this.lblLotNom.TabIndex = 2;
            this.lblLotNom.Text = "Nom:";
            // 
            // cmbLotNom
            // 
            this.cmbLotNom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLotNom.FormattingEnabled = true;
            this.cmbLotNom.Location = new System.Drawing.Point(86, 22);
            this.cmbLotNom.Name = "cmbLotNom";
            this.cmbLotNom.Size = new System.Drawing.Size(168, 23);
            this.cmbLotNom.TabIndex = 3;
            this.cmbLotNom.SelectedIndexChanged += new System.EventHandler(this.cmbLotNom_SelectedIndexChanged);
            // 
            // lblLotPrio
            // 
            this.lblLotPrio.AutoSize = true;
            this.lblLotPrio.Location = new System.Drawing.Point(260, 25);
            this.lblLotPrio.Name = "lblLotPrio";
            this.lblLotPrio.Size = new System.Drawing.Size(50, 15);
            this.lblLotPrio.TabIndex = 4;
            this.lblLotPrio.Text = "Priorité:";
            // 
            // numLotPriorite
            // 
            this.numLotPriorite.Location = new System.Drawing.Point(316, 22);
            this.numLotPriorite.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numLotPriorite.Name = "numLotPriorite";
            this.numLotPriorite.ReadOnly = true;
            this.numLotPriorite.Size = new System.Drawing.Size(38, 23);
            this.numLotPriorite.TabIndex = 5;
            this.numLotPriorite.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // grpBloc
            // 
            this.grpBloc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBloc.Controls.Add(this.lblBlocNom);
            this.grpBloc.Controls.Add(this.cmbBlocNom);
            this.grpBloc.Controls.Add(this.lblBlocCap);
            this.grpBloc.Controls.Add(this.numBlocCapacite);
            this.grpBloc.Location = new System.Drawing.Point(12, 241); // MODIFIÉ
            this.grpBloc.Name = "grpBloc";
            this.grpBloc.Size = new System.Drawing.Size(360, 55);
            this.grpBloc.TabIndex = 2;
            this.grpBloc.TabStop = false;
            this.grpBloc.Text = "Informations Bloc";
            // 
            // lblBlocNom
            // 
            this.lblBlocNom.AutoSize = true;
            this.lblBlocNom.Location = new System.Drawing.Point(10, 25);
            this.lblBlocNom.Name = "lblBlocNom";
            this.lblBlocNom.Size = new System.Drawing.Size(37, 15);
            this.lblBlocNom.TabIndex = 2;
            this.lblBlocNom.Text = "Nom:";
            // 
            // cmbBlocNom
            // 
            this.cmbBlocNom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBlocNom.FormattingEnabled = true;
            this.cmbBlocNom.Location = new System.Drawing.Point(86, 22);
            this.cmbBlocNom.Name = "cmbBlocNom";
            this.cmbBlocNom.Size = new System.Drawing.Size(168, 23);
            this.cmbBlocNom.TabIndex = 3;
            this.cmbBlocNom.SelectedIndexChanged += new System.EventHandler(this.cmbBlocNom_SelectedIndexChanged);
            // 
            // lblBlocCap
            // 
            this.lblBlocCap.AutoSize = true;
            this.lblBlocCap.Location = new System.Drawing.Point(260, 25);
            this.lblBlocCap.Name = "lblBlocCap";
            this.lblBlocCap.Size = new System.Drawing.Size(55, 15);
            this.lblBlocCap.TabIndex = 4;
            this.lblBlocCap.Text = "Capacité:";
            // 
            // numBlocCapacite
            // 
            this.numBlocCapacite.Location = new System.Drawing.Point(316, 22);
            this.numBlocCapacite.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            this.numBlocCapacite.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numBlocCapacite.Name = "numBlocCapacite";
            this.numBlocCapacite.ReadOnly = true;
            this.numBlocCapacite.Size = new System.Drawing.Size(38, 23);
            this.numBlocCapacite.TabIndex = 5;
            this.numBlocCapacite.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // grpDep
            // 
            this.grpDep.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDep.Controls.Add(this.chkListDependances);
            this.grpDep.Location = new System.Drawing.Point(12, 302); // MODIFIÉ
            this.grpDep.Name = "grpDep";
            this.grpDep.Size = new System.Drawing.Size(360, 120); // MODIFIÉ
            this.grpDep.TabIndex = 3;
            this.grpDep.TabStop = false;
            this.grpDep.Text = "Dépendances (bleu=hérité métier, rouge=exclu)";
            // 
            // chkListDependances
            // 
            this.chkListDependances.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkListDependances.CheckOnClick = true;
            this.chkListDependances.FormattingEnabled = true;
            this.chkListDependances.Location = new System.Drawing.Point(10, 22);
            this.chkListDependances.Name = "chkListDependances";
            this.chkListDependances.Size = new System.Drawing.Size(344, 88);
            this.chkListDependances.TabIndex = 0;
            this.chkListDependances.DrawMode = DrawMode.OwnerDrawFixed; // AJOUTÉ
            this.chkListDependances.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.chkListDependances_DrawItem); // AJOUTÉ
            // 
            // btnSauvegarder
            // 
            this.btnSauvegarder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSauvegarder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSauvegarder.Location = new System.Drawing.Point(12, 428); // MODIFIÉ
            this.btnSauvegarder.Name = "btnSauvegarder";
            this.btnSauvegarder.Size = new System.Drawing.Size(360, 30);
            this.btnSauvegarder.TabIndex = 4;
            this.btnSauvegarder.Text = "💾 Sauvegarder les modifications";
            this.btnSauvegarder.UseVisualStyleBackColor = true;
            this.btnSauvegarder.Click += new System.EventHandler(this.btnSauvegarder_Click);
            // 
            // TacheDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 470); // MODIFIÉ
            this.Controls.Add(this.btnSauvegarder);
            this.Controls.Add(this.grpDep);
            this.Controls.Add(this.grpBloc);
            this.Controls.Add(this.grpLot);
            this.Controls.Add(this.grpTache);
            this.MinimumSize = new System.Drawing.Size(400, 500); // AJOUTÉ
            this.Name = "TacheDetailForm";
            this.Text = "Détails de la Tâche";
            ((System.ComponentModel.ISupportInitialize)(this.numHeuresHomme)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLotPriorite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlocCapacite)).EndInit();
            this.grpTache.ResumeLayout(false);
            this.grpTache.PerformLayout();
            this.grpLot.ResumeLayout(false);
            this.grpLot.PerformLayout();
            this.grpBloc.ResumeLayout(false);
            this.grpBloc.PerformLayout();
            this.grpDep.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox txtTacheId;
        private System.Windows.Forms.TextBox txtTacheNom;
        private System.Windows.Forms.NumericUpDown numHeuresHomme;
        private System.Windows.Forms.ComboBox cmbMetier;
        private System.Windows.Forms.CheckedListBox chkListDependances; // MODIFIÉ
        private System.Windows.Forms.ComboBox cmbLotNom;
        private System.Windows.Forms.NumericUpDown numLotPriorite;
        private System.Windows.Forms.ComboBox cmbBlocNom;
        private System.Windows.Forms.NumericUpDown numBlocCapacite;
        private System.Windows.Forms.Button btnSauvegarder;
        private System.Windows.Forms.CheckBox chkEstJalon; // NOUVEAU
        private System.Windows.Forms.GroupBox grpTache;
        private System.Windows.Forms.Label lblTacheId;
        private System.Windows.Forms.Label lblTacheNom;
        private System.Windows.Forms.Label lblHeures;
        private System.Windows.Forms.Label lblMetier;
        private System.Windows.Forms.GroupBox grpLot;
        private System.Windows.Forms.Label lblLotNom;
        private System.Windows.Forms.Label lblLotPrio;
        private System.Windows.Forms.GroupBox grpBloc;
        private System.Windows.Forms.Label lblBlocNom;
        private System.Windows.Forms.Label lblBlocCap;
        private System.Windows.Forms.GroupBox grpDep;
    }
}