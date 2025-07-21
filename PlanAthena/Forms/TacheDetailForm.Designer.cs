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
            txtTacheId = new TextBox();
            txtTacheNom = new TextBox();
            numHeuresHomme = new NumericUpDown();
            cmbMetier = new ComboBox();
            chkListDependances = new CheckedListBox();
            cmbBlocNom = new ComboBox();
            numBlocCapacite = new NumericUpDown();
            btnSauvegarder = new Button();
            chkEstJalon = new CheckBox();
            grpTache = new GroupBox();
            lblTacheNom = new Label();
            lblHeures = new Label();
            lblMetier = new Label();
            grpBloc = new GroupBox();
            lblBlocNom = new Label();
            lblBlocCap = new Label();
            grpDep = new GroupBox();
            flowLayoutPanelActions = new FlowLayoutPanel();
            btnAnnuler = new Button();
            btnSupprimer = new Button();
            ((System.ComponentModel.ISupportInitialize)numHeuresHomme).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numBlocCapacite).BeginInit();
            grpTache.SuspendLayout();
            grpBloc.SuspendLayout();
            grpDep.SuspendLayout();
            flowLayoutPanelActions.SuspendLayout();
            SuspendLayout();
            // 
            // txtTacheId
            // 
            txtTacheId.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTacheId.BackColor = SystemColors.Control;
            txtTacheId.Location = new Point(192, 50);
            txtTacheId.Name = "txtTacheId";
            txtTacheId.PlaceholderText = "Auto-généré";
            txtTacheId.ReadOnly = true;
            txtTacheId.Size = new Size(149, 23);
            txtTacheId.TabIndex = 1;
            // 
            // txtTacheNom
            // 
            txtTacheNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTacheNom.Location = new Point(86, 22);
            txtTacheNom.Name = "txtTacheNom";
            txtTacheNom.Size = new Size(268, 23);
            txtTacheNom.TabIndex = 3;
            // 
            // numHeuresHomme
            // 
            numHeuresHomme.Location = new Point(114, 51);
            numHeuresHomme.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numHeuresHomme.Name = "numHeuresHomme";
            numHeuresHomme.Size = new Size(72, 23);
            numHeuresHomme.TabIndex = 5;
            numHeuresHomme.ValueChanged += numHeuresHomme_ValueChanged;
            // 
            // cmbMetier
            // 
            cmbMetier.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbMetier.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetier.FormattingEnabled = true;
            cmbMetier.Location = new Point(86, 102);
            cmbMetier.Name = "cmbMetier";
            cmbMetier.Size = new Size(268, 23);
            cmbMetier.TabIndex = 8;
            cmbMetier.SelectedIndexChanged += cmbMetier_SelectedIndexChanged;
            // 
            // chkListDependances
            // 
            chkListDependances.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chkListDependances.CheckOnClick = true;
            chkListDependances.FormattingEnabled = true;
            chkListDependances.Location = new Point(10, 22);
            chkListDependances.Name = "chkListDependances";
            chkListDependances.Size = new Size(344, 148);
            chkListDependances.TabIndex = 0;
            chkListDependances.DrawItem += chkListDependances_DrawItem;
            // 
            // cmbBlocNom
            // 
            cmbBlocNom.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBlocNom.FormattingEnabled = true;
            cmbBlocNom.Location = new Point(86, 22);
            cmbBlocNom.Name = "cmbBlocNom";
            cmbBlocNom.Size = new Size(168, 23);
            cmbBlocNom.TabIndex = 3;
            cmbBlocNom.SelectedIndexChanged += cmbBlocNom_SelectedIndexChanged;
            // 
            // numBlocCapacite
            // 
            numBlocCapacite.Location = new Point(316, 22);
            numBlocCapacite.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numBlocCapacite.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numBlocCapacite.Name = "numBlocCapacite";
            numBlocCapacite.ReadOnly = true;
            numBlocCapacite.Size = new Size(38, 23);
            numBlocCapacite.TabIndex = 5;
            numBlocCapacite.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnSauvegarder
            // 
            btnSauvegarder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSauvegarder.Location = new Point(5, 3);
            btnSauvegarder.Name = "btnSauvegarder";
            btnSauvegarder.Size = new Size(180, 30);
            btnSauvegarder.TabIndex = 0;
            btnSauvegarder.Text = "💾 Sauvegarder";
            btnSauvegarder.UseVisualStyleBackColor = true;
            btnSauvegarder.Click += btnSauvegarder_Click;
            // 
            // chkEstJalon
            // 
            chkEstJalon.AutoSize = true;
            chkEstJalon.Location = new Point(10, 80);
            chkEstJalon.Name = "chkEstJalon";
            chkEstJalon.Size = new Size(118, 19);
            chkEstJalon.TabIndex = 6;
            chkEstJalon.Text = "🏁 Jalon d'attente";
            chkEstJalon.UseVisualStyleBackColor = true;
            chkEstJalon.CheckedChanged += chkEstJalon_CheckedChanged;
            // 
            // grpTache
            // 
            grpTache.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpTache.Controls.Add(txtTacheId);
            grpTache.Controls.Add(lblTacheNom);
            grpTache.Controls.Add(txtTacheNom);
            grpTache.Controls.Add(lblHeures);
            grpTache.Controls.Add(numHeuresHomme);
            grpTache.Controls.Add(chkEstJalon);
            grpTache.Controls.Add(lblMetier);
            grpTache.Controls.Add(cmbMetier);
            grpTache.Location = new Point(12, 12);
            grpTache.Name = "grpTache";
            grpTache.Size = new Size(360, 132);
            grpTache.TabIndex = 0;
            grpTache.TabStop = false;
            grpTache.Text = "Informations Tâche";
            // 
            // lblTacheNom
            // 
            lblTacheNom.AutoSize = true;
            lblTacheNom.Location = new Point(10, 25);
            lblTacheNom.Name = "lblTacheNom";
            lblTacheNom.Size = new Size(37, 15);
            lblTacheNom.TabIndex = 2;
            lblTacheNom.Text = "Nom:";
            // 
            // lblHeures
            // 
            lblHeures.AutoSize = true;
            lblHeures.Location = new Point(10, 54);
            lblHeures.Name = "lblHeures";
            lblHeures.Size = new Size(85, 15);
            lblHeures.TabIndex = 4;
            lblHeures.Text = "Durée estimée:";
            // 
            // lblMetier
            // 
            lblMetier.AutoSize = true;
            lblMetier.Location = new Point(10, 106);
            lblMetier.Name = "lblMetier";
            lblMetier.Size = new Size(44, 15);
            lblMetier.TabIndex = 7;
            lblMetier.Text = "Métier:";
            // 
            // grpBloc
            // 
            grpBloc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpBloc.Controls.Add(lblBlocNom);
            grpBloc.Controls.Add(cmbBlocNom);
            grpBloc.Controls.Add(lblBlocCap);
            grpBloc.Controls.Add(numBlocCapacite);
            grpBloc.Location = new Point(12, 150);
            grpBloc.Name = "grpBloc";
            grpBloc.Size = new Size(360, 55);
            grpBloc.TabIndex = 2;
            grpBloc.TabStop = false;
            grpBloc.Text = "Informations Bloc";
            // 
            // lblBlocNom
            // 
            lblBlocNom.AutoSize = true;
            lblBlocNom.Location = new Point(10, 25);
            lblBlocNom.Name = "lblBlocNom";
            lblBlocNom.Size = new Size(37, 15);
            lblBlocNom.TabIndex = 2;
            lblBlocNom.Text = "Nom:";
            // 
            // lblBlocCap
            // 
            lblBlocCap.AutoSize = true;
            lblBlocCap.Location = new Point(260, 25);
            lblBlocCap.Name = "lblBlocCap";
            lblBlocCap.Size = new Size(56, 15);
            lblBlocCap.TabIndex = 4;
            lblBlocCap.Text = "Capacité:";
            // 
            // grpDep
            // 
            grpDep.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpDep.Controls.Add(chkListDependances);
            grpDep.Location = new Point(12, 211);
            grpDep.Name = "grpDep";
            grpDep.Size = new Size(360, 177);
            grpDep.TabIndex = 3;
            grpDep.TabStop = false;
            grpDep.Text = "Dépendances (bleu=hérité métier, rouge=exclu)";
            // 
            // flowLayoutPanelActions
            // 
            flowLayoutPanelActions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanelActions.Controls.Add(btnAnnuler);
            flowLayoutPanelActions.Controls.Add(btnSupprimer);
            flowLayoutPanelActions.Controls.Add(btnSauvegarder);
            flowLayoutPanelActions.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanelActions.Location = new Point(12, 394);
            flowLayoutPanelActions.Name = "flowLayoutPanelActions";
            flowLayoutPanelActions.Size = new Size(360, 35);
            flowLayoutPanelActions.TabIndex = 4;
            // 
            // btnAnnuler
            // 
            btnAnnuler.Location = new Point(282, 3);
            btnAnnuler.Name = "btnAnnuler";
            btnAnnuler.Size = new Size(75, 30);
            btnAnnuler.TabIndex = 2;
            btnAnnuler.Text = "Annuler";
            btnAnnuler.UseVisualStyleBackColor = true;
            btnAnnuler.Click += btnAnnuler_Click;
            // 
            // btnSupprimer
            // 
            btnSupprimer.BackColor = Color.MistyRose;
            btnSupprimer.Location = new Point(191, 3);
            btnSupprimer.Name = "btnSupprimer";
            btnSupprimer.Size = new Size(85, 30);
            btnSupprimer.TabIndex = 1;
            btnSupprimer.Text = "Supprimer";
            btnSupprimer.UseVisualStyleBackColor = false;
            btnSupprimer.Click += btnSupprimer_Click;
            // 
            // TacheDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 441);
            Controls.Add(flowLayoutPanelActions);
            Controls.Add(grpDep);
            Controls.Add(grpBloc);
            Controls.Add(grpTache);
            MinimumSize = new Size(400, 480);
            Name = "TacheDetailForm";
            Text = "Détails de l'Activité";
            ((System.ComponentModel.ISupportInitialize)numHeuresHomme).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBlocCapacite).EndInit();
            grpTache.ResumeLayout(false);
            grpTache.PerformLayout();
            grpBloc.ResumeLayout(false);
            grpBloc.PerformLayout();
            grpDep.ResumeLayout(false);
            flowLayoutPanelActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox txtTacheId;
        private System.Windows.Forms.TextBox txtTacheNom;
        private System.Windows.Forms.NumericUpDown numHeuresHomme;
        private System.Windows.Forms.ComboBox cmbMetier;
        private System.Windows.Forms.CheckedListBox chkListDependances;
        private System.Windows.Forms.ComboBox cmbBlocNom;
        private System.Windows.Forms.NumericUpDown numBlocCapacite;
        private System.Windows.Forms.Button btnSauvegarder;
        private System.Windows.Forms.CheckBox chkEstJalon;
        private System.Windows.Forms.GroupBox grpTache;
        private System.Windows.Forms.Label lblTacheNom;
        private System.Windows.Forms.Label lblHeures;
        private System.Windows.Forms.Label lblMetier;
        private System.Windows.Forms.GroupBox grpBloc;
        private System.Windows.Forms.Label lblBlocNom;
        private System.Windows.Forms.Label lblBlocCap;
        private System.Windows.Forms.GroupBox grpDep;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelActions;
        private System.Windows.Forms.Button btnAnnuler;
        private System.Windows.Forms.Button btnSupprimer;
    }
}
