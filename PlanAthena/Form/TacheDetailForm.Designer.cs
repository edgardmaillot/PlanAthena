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

        private void InitializeComponent()
        {
            this.txtTacheId = new System.Windows.Forms.TextBox();
            this.txtTacheNom = new System.Windows.Forms.TextBox();
            this.numHeuresHomme = new System.Windows.Forms.NumericUpDown();
            this.cmbMetier = new System.Windows.Forms.ComboBox();
            this.txtDependances = new System.Windows.Forms.TextBox();
            this.txtLotId = new System.Windows.Forms.TextBox();
            this.txtLotNom = new System.Windows.Forms.TextBox();
            this.numLotPriorite = new System.Windows.Forms.NumericUpDown();
            this.txtBlocId = new System.Windows.Forms.TextBox();
            this.txtBlocNom = new System.Windows.Forms.TextBox();
            this.numBlocCapacite = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.grpTache = new System.Windows.Forms.GroupBox();
            this.grpLot = new System.Windows.Forms.GroupBox();
            this.grpBloc = new System.Windows.Forms.GroupBox();
            this.grpDep = new System.Windows.Forms.GroupBox();

            // Labels
            this.lblTacheId = new System.Windows.Forms.Label();
            this.lblTacheNom = new System.Windows.Forms.Label();
            this.lblHeures = new System.Windows.Forms.Label();
            this.lblMetier = new System.Windows.Forms.Label();
            this.lblLotId = new System.Windows.Forms.Label();
            this.lblLotNom = new System.Windows.Forms.Label();
            this.lblLotPrio = new System.Windows.Forms.Label();
            this.lblBlocId = new System.Windows.Forms.Label();
            this.lblBlocNom = new System.Windows.Forms.Label();
            this.lblBlocCap = new System.Windows.Forms.Label();

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
            this.grpTache.Controls.Add(this.lblTacheId);
            this.grpTache.Controls.Add(this.txtTacheId);
            this.grpTache.Controls.Add(this.lblTacheNom);
            this.grpTache.Controls.Add(this.txtTacheNom);
            this.grpTache.Controls.Add(this.lblHeures);
            this.grpTache.Controls.Add(this.numHeuresHomme);
            this.grpTache.Controls.Add(this.lblMetier);
            this.grpTache.Controls.Add(this.cmbMetier);
            this.grpTache.Location = new System.Drawing.Point(12, 12);
            this.grpTache.Name = "grpTache";
            this.grpTache.Size = new System.Drawing.Size(360, 130);
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
            this.txtTacheId.Location = new System.Drawing.Point(86, 22);
            this.txtTacheId.Name = "txtTacheId";
            this.txtTacheId.Size = new System.Drawing.Size(250, 23);
            this.txtTacheId.TabIndex = 1;

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
            this.txtTacheNom.Location = new System.Drawing.Point(86, 51);
            this.txtTacheNom.Name = "txtTacheNom";
            this.txtTacheNom.Size = new System.Drawing.Size(250, 23);
            this.txtTacheNom.TabIndex = 3;

            // 
            // lblHeures
            // 
            this.lblHeures.AutoSize = true;
            this.lblHeures.Location = new System.Drawing.Point(10, 83);
            this.lblHeures.Name = "lblHeures";
            this.lblHeures.Size = new System.Drawing.Size(98, 15);
            this.lblHeures.TabIndex = 4;
            this.lblHeures.Text = "Heures-homme:";

            // 
            // numHeuresHomme
            // 
            this.numHeuresHomme.Location = new System.Drawing.Point(86, 80);
            this.numHeuresHomme.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            this.numHeuresHomme.Name = "numHeuresHomme";
            this.numHeuresHomme.Size = new System.Drawing.Size(100, 23);
            this.numHeuresHomme.TabIndex = 5;

            // 
            // lblMetier
            // 
            this.lblMetier.AutoSize = true;
            this.lblMetier.Location = new System.Drawing.Point(200, 83);
            this.lblMetier.Name = "lblMetier";
            this.lblMetier.Size = new System.Drawing.Size(46, 15);
            this.lblMetier.TabIndex = 6;
            this.lblMetier.Text = "Métier:";

            // 
            // cmbMetier
            // 
            this.cmbMetier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMetier.FormattingEnabled = true;
            this.cmbMetier.Location = new System.Drawing.Point(200, 100);
            this.cmbMetier.Name = "cmbMetier";
            this.cmbMetier.Size = new System.Drawing.Size(136, 23);
            this.cmbMetier.TabIndex = 7;

            // 
            // grpLot
            // 
            this.grpLot.Controls.Add(this.lblLotId);
            this.grpLot.Controls.Add(this.txtLotId);
            this.grpLot.Controls.Add(this.lblLotNom);
            this.grpLot.Controls.Add(this.txtLotNom);
            this.grpLot.Controls.Add(this.lblLotPrio);
            this.grpLot.Controls.Add(this.numLotPriorite);
            this.grpLot.Location = new System.Drawing.Point(12, 150);
            this.grpLot.Name = "grpLot";
            this.grpLot.Size = new System.Drawing.Size(360, 80);
            this.grpLot.TabIndex = 1;
            this.grpLot.TabStop = false;
            this.grpLot.Text = "Informations Lot";

            // 
            // lblLotId
            // 
            this.lblLotId.AutoSize = true;
            this.lblLotId.Location = new System.Drawing.Point(10, 25);
            this.lblLotId.Name = "lblLotId";
            this.lblLotId.Size = new System.Drawing.Size(21, 15);
            this.lblLotId.TabIndex = 0;
            this.lblLotId.Text = "ID:";

            // 
            // txtLotId
            // 
            this.txtLotId.Location = new System.Drawing.Point(86, 22);
            this.txtLotId.Name = "txtLotId";
            this.txtLotId.Size = new System.Drawing.Size(100, 23);
            this.txtLotId.TabIndex = 1;

            // 
            // lblLotNom
            // 
            this.lblLotNom.AutoSize = true;
            this.lblLotNom.Location = new System.Drawing.Point(10, 54);
            this.lblLotNom.Name = "lblLotNom";
            this.lblLotNom.Size = new System.Drawing.Size(37, 15);
            this.lblLotNom.TabIndex = 2;
            this.lblLotNom.Text = "Nom:";

            // 
            // txtLotNom
            // 
            this.txtLotNom.Location = new System.Drawing.Point(86, 51);
            this.txtLotNom.Name = "txtLotNom";
            this.txtLotNom.Size = new System.Drawing.Size(168, 23);
            this.txtLotNom.TabIndex = 3;

            // 
            // lblLotPrio
            // 
            this.lblLotPrio.AutoSize = true;
            this.lblLotPrio.Location = new System.Drawing.Point(200, 25);
            this.lblLotPrio.Name = "lblLotPrio";
            this.lblLotPrio.Size = new System.Drawing.Size(50, 15);
            this.lblLotPrio.TabIndex = 4;
            this.lblLotPrio.Text = "Priorité:";

            // 
            // numLotPriorite
            // 
            this.numLotPriorite.Location = new System.Drawing.Point(260, 22);
            this.numLotPriorite.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numLotPriorite.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numLotPriorite.Name = "numLotPriorite";
            this.numLotPriorite.Size = new System.Drawing.Size(60, 23);
            this.numLotPriorite.TabIndex = 5;
            this.numLotPriorite.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // 
            // grpBloc
            // 
            this.grpBloc.Controls.Add(this.lblBlocId);
            this.grpBloc.Controls.Add(this.txtBlocId);
            this.grpBloc.Controls.Add(this.lblBlocNom);
            this.grpBloc.Controls.Add(this.txtBlocNom);
            this.grpBloc.Controls.Add(this.lblBlocCap);
            this.grpBloc.Controls.Add(this.numBlocCapacite);
            this.grpBloc.Location = new System.Drawing.Point(12, 240);
            this.grpBloc.Name = "grpBloc";
            this.grpBloc.Size = new System.Drawing.Size(360, 80);
            this.grpBloc.TabIndex = 2;
            this.grpBloc.TabStop = false;
            this.grpBloc.Text = "Informations Bloc";

            // 
            // lblBlocId
            // 
            this.lblBlocId.AutoSize = true;
            this.lblBlocId.Location = new System.Drawing.Point(10, 25);
            this.lblBlocId.Name = "lblBlocId";
            this.lblBlocId.Size = new System.Drawing.Size(21, 15);
            this.lblBlocId.TabIndex = 0;
            this.lblBlocId.Text = "ID:";

            // 
            // txtBlocId
            // 
            this.txtBlocId.Location = new System.Drawing.Point(86, 22);
            this.txtBlocId.Name = "txtBlocId";
            this.txtBlocId.Size = new System.Drawing.Size(100, 23);
            this.txtBlocId.TabIndex = 1;

            // 
            // lblBlocNom
            // 
            this.lblBlocNom.AutoSize = true;
            this.lblBlocNom.Location = new System.Drawing.Point(10, 54);
            this.lblBlocNom.Name = "lblBlocNom";
            this.lblBlocNom.Size = new System.Drawing.Size(37, 15);
            this.lblBlocNom.TabIndex = 2;
            this.lblBlocNom.Text = "Nom:";

            // 
            // txtBlocNom
            // 
            this.txtBlocNom.Location = new System.Drawing.Point(86, 51);
            this.txtBlocNom.Name = "txtBlocNom";
            this.txtBlocNom.Size = new System.Drawing.Size(168, 23);
            this.txtBlocNom.TabIndex = 3;

            // 
            // lblBlocCap
            // 
            this.lblBlocCap.AutoSize = true;
            this.lblBlocCap.Location = new System.Drawing.Point(200, 25);
            this.lblBlocCap.Name = "lblBlocCap";
            this.lblBlocCap.Size = new System.Drawing.Size(55, 15);
            this.lblBlocCap.TabIndex = 4;
            this.lblBlocCap.Text = "Capacité:";

            // 
            // numBlocCapacite
            // 
            this.numBlocCapacite.Location = new System.Drawing.Point(260, 22);
            this.numBlocCapacite.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            this.numBlocCapacite.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numBlocCapacite.Name = "numBlocCapacite";
            this.numBlocCapacite.Size = new System.Drawing.Size(60, 23);
            this.numBlocCapacite.TabIndex = 5;
            this.numBlocCapacite.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // 
            // grpDep
            // 
            this.grpDep.Controls.Add(this.txtDependances);
            this.grpDep.Location = new System.Drawing.Point(12, 330);
            this.grpDep.Name = "grpDep";
            this.grpDep.Size = new System.Drawing.Size(360, 80);
            this.grpDep.TabIndex = 3;
            this.grpDep.TabStop = false;
            this.grpDep.Text = "Dépendances (IDs séparés par des virgules)";

            // 
            // txtDependances
            // 
            this.txtDependances.Location = new System.Drawing.Point(10, 20);
            this.txtDependances.Multiline = true;
            this.txtDependances.Name = "txtDependances";
            this.txtDependances.PlaceholderText = "T001,T002,T003...";
            this.txtDependances.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDependances.Size = new System.Drawing.Size(340, 50);
            this.txtDependances.TabIndex = 0;

            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(217, 420);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // 
            // btnAnnuler
            // 
            this.btnAnnuler.Location = new System.Drawing.Point(298, 420);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(75, 23);
            this.btnAnnuler.TabIndex = 5;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = true;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);

            // 
            // TacheDetailForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAnnuler;
            this.ClientSize = new System.Drawing.Size(384, 455);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.grpDep);
            this.Controls.Add(this.grpBloc);
            this.Controls.Add(this.grpLot);
            this.Controls.Add(this.grpTache);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TacheDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
            this.grpDep.PerformLayout();
            this.ResumeLayout(false);
        }

        #region Contrôles Windows Form Designer

        private System.Windows.Forms.TextBox txtTacheId;
        private System.Windows.Forms.TextBox txtTacheNom;
        private System.Windows.Forms.NumericUpDown numHeuresHomme;
        private System.Windows.Forms.ComboBox cmbMetier;
        private System.Windows.Forms.TextBox txtDependances;
        private System.Windows.Forms.TextBox txtLotId;
        private System.Windows.Forms.TextBox txtLotNom;
        private System.Windows.Forms.NumericUpDown numLotPriorite;
        private System.Windows.Forms.TextBox txtBlocId;
        private System.Windows.Forms.TextBox txtBlocNom;
        private System.Windows.Forms.NumericUpDown numBlocCapacite;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnAnnuler;
        private System.Windows.Forms.GroupBox grpTache;
        private System.Windows.Forms.GroupBox grpLot;
        private System.Windows.Forms.GroupBox grpBloc;
        private System.Windows.Forms.GroupBox grpDep;
        private System.Windows.Forms.Label lblTacheId;
        private System.Windows.Forms.Label lblTacheNom;
        private System.Windows.Forms.Label lblHeures;
        private System.Windows.Forms.Label lblMetier;
        private System.Windows.Forms.Label lblLotId;
        private System.Windows.Forms.Label lblLotNom;
        private System.Windows.Forms.Label lblLotPrio;
        private System.Windows.Forms.Label lblBlocId;
        private System.Windows.Forms.Label lblBlocNom;
        private System.Windows.Forms.Label lblBlocCap;

        #endregion
    }
}