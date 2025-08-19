using Krypton.Toolkit;

namespace PlanAthena.View
{
    partial class TacheDetailView
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            panelActions = new KryptonPanel();
            btnSupprimer = new KryptonButton();
            chkListDependances = new KryptonCheckedListBox();
            lblBlocNom = new KryptonLabel();
            cmbBlocNom = new KryptonComboBox();
            lblBlocCap = new KryptonLabel();
            numBlocCapacite = new KryptonNumericUpDown();
            cmbMetier = new KryptonComboBox();
            lblMetier = new KryptonLabel();
            kryptonHeader1 = new KryptonHeader();
            kryptonLabel1 = new KryptonLabel();
            numHeuresHomme = new KryptonNumericUpDown();
            lblHeures = new KryptonLabel();
            chkIsJalon = new KryptonCheckBox();
            textTacheNom = new KryptonTextBox();
            lblTacheNom = new KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)panelActions).BeginInit();
            panelActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbBlocNom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbMetier).BeginInit();
            SuspendLayout();
            // 
            // panelActions
            // 
            panelActions.Controls.Add(btnSupprimer);
            panelActions.Dock = DockStyle.Bottom;
            panelActions.Location = new Point(0, 569);
            panelActions.Margin = new Padding(4, 3, 4, 3);
            panelActions.Name = "panelActions";
            panelActions.Size = new Size(376, 58);
            panelActions.TabIndex = 3;
            // 
            // btnSupprimer
            // 
            btnSupprimer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSupprimer.Location = new Point(254, 12);
            btnSupprimer.Margin = new Padding(4, 3, 4, 3);
            btnSupprimer.Name = "btnSupprimer";
            btnSupprimer.Size = new Size(105, 35);
            btnSupprimer.TabIndex = 0;
            btnSupprimer.Values.DropDownArrowColor = Color.Empty;
            btnSupprimer.Values.Text = "Supprimer";
            // 
            // chkListDependances
            // 
            chkListDependances.Dock = DockStyle.Bottom;
            chkListDependances.Location = new Point(0, 260);
            chkListDependances.Name = "chkListDependances";
            chkListDependances.Size = new Size(376, 309);
            chkListDependances.TabIndex = 4;
            // 
            // lblBlocNom
            // 
            lblBlocNom.Location = new Point(32, 167);
            lblBlocNom.Margin = new Padding(4, 3, 4, 3);
            lblBlocNom.Name = "lblBlocNom";
            lblBlocNom.Size = new Size(40, 20);
            lblBlocNom.TabIndex = 5;
            lblBlocNom.Values.Text = "Bloc :";
            // 
            // cmbBlocNom
            // 
            cmbBlocNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbBlocNom.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBlocNom.DropDownWidth = 252;
            cmbBlocNom.IntegralHeight = false;
            cmbBlocNom.Location = new Point(80, 167);
            cmbBlocNom.Margin = new Padding(4, 3, 4, 3);
            cmbBlocNom.Name = "cmbBlocNom";
            cmbBlocNom.Size = new Size(288, 22);
            cmbBlocNom.TabIndex = 6;
            // 
            // lblBlocCap
            // 
            lblBlocCap.Location = new Point(7, 195);
            lblBlocCap.Margin = new Padding(4, 3, 4, 3);
            lblBlocCap.Name = "lblBlocCap";
            lblBlocCap.Size = new Size(63, 20);
            lblBlocCap.TabIndex = 7;
            lblBlocCap.Values.Text = "Capacité :";
            // 
            // numBlocCapacite
            // 
            numBlocCapacite.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numBlocCapacite.Location = new Point(79, 195);
            numBlocCapacite.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numBlocCapacite.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numBlocCapacite.Name = "numBlocCapacite";
            numBlocCapacite.Size = new Size(120, 22);
            numBlocCapacite.TabIndex = 8;
            numBlocCapacite.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // cmbMetier
            // 
            cmbMetier.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbMetier.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetier.DropDownWidth = 286;
            cmbMetier.IntegralHeight = false;
            cmbMetier.Location = new Point(80, 125);
            cmbMetier.Margin = new Padding(4, 3, 4, 3);
            cmbMetier.Name = "cmbMetier";
            cmbMetier.Size = new Size(288, 22);
            cmbMetier.TabIndex = 9;
            // 
            // lblMetier
            // 
            lblMetier.Location = new Point(20, 125);
            lblMetier.Margin = new Padding(4, 3, 4, 3);
            lblMetier.Name = "lblMetier";
            lblMetier.Size = new Size(52, 20);
            lblMetier.TabIndex = 10;
            lblMetier.Values.Text = "Métier :";
            // 
            // kryptonHeader1
            // 
            kryptonHeader1.Dock = DockStyle.Top;
            kryptonHeader1.HeaderStyle = HeaderStyle.Secondary;
            kryptonHeader1.Location = new Point(0, 0);
            kryptonHeader1.Name = "kryptonHeader1";
            kryptonHeader1.Size = new Size(376, 36);
            kryptonHeader1.TabIndex = 16;
            kryptonHeader1.Values.Heading = "Detail de la tâche";
            kryptonHeader1.Values.Image = Properties.Resources.tache;
            // 
            // kryptonLabel1
            // 
            kryptonLabel1.Location = new Point(7, 243);
            kryptonLabel1.Name = "kryptonLabel1";
            kryptonLabel1.Size = new Size(84, 20);
            kryptonLabel1.TabIndex = 17;
            kryptonLabel1.Values.Text = "Dépendances";
            // 
            // numHeuresHomme
            // 
            numHeuresHomme.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numHeuresHomme.Location = new Point(275, 82);
            numHeuresHomme.Margin = new Padding(4, 3, 4, 3);
            numHeuresHomme.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numHeuresHomme.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHeuresHomme.Name = "numHeuresHomme";
            numHeuresHomme.Size = new Size(93, 22);
            numHeuresHomme.TabIndex = 13;
            numHeuresHomme.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // lblHeures
            // 
            lblHeures.Location = new Point(174, 82);
            lblHeures.Margin = new Padding(4, 3, 4, 3);
            lblHeures.Name = "lblHeures";
            lblHeures.Size = new Size(67, 20);
            lblHeures.TabIndex = 12;
            lblHeures.Values.Text = "Durée (h) :";
            // 
            // chkIsJalon
            // 
            chkIsJalon.Location = new Point(22, 82);
            chkIsJalon.Margin = new Padding(4, 3, 4, 3);
            chkIsJalon.Name = "chkIsJalon";
            chkIsJalon.Size = new Size(130, 20);
            chkIsJalon.TabIndex = 11;
            chkIsJalon.Values.Text = "Est un jalon/attente";
            // 
            // textTacheNom
            // 
            textTacheNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textTacheNom.Location = new Point(80, 53);
            textTacheNom.Margin = new Padding(4, 3, 4, 3);
            textTacheNom.Name = "textTacheNom";
            textTacheNom.Size = new Size(288, 23);
            textTacheNom.TabIndex = 14;
            // 
            // lblTacheNom
            // 
            lblTacheNom.Location = new Point(19, 56);
            lblTacheNom.Margin = new Padding(4, 3, 4, 3);
            lblTacheNom.Name = "lblTacheNom";
            lblTacheNom.Size = new Size(44, 20);
            lblTacheNom.TabIndex = 15;
            lblTacheNom.Values.Text = "Nom :";
            // 
            // TacheDetailView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonLabel1);
            Controls.Add(kryptonHeader1);
            Controls.Add(lblTacheNom);
            Controls.Add(textTacheNom);
            Controls.Add(numHeuresHomme);
            Controls.Add(lblHeures);
            Controls.Add(chkIsJalon);
            Controls.Add(lblMetier);
            Controls.Add(cmbMetier);
            Controls.Add(numBlocCapacite);
            Controls.Add(lblBlocCap);
            Controls.Add(cmbBlocNom);
            Controls.Add(lblBlocNom);
            Controls.Add(chkListDependances);
            Controls.Add(panelActions);
            Margin = new Padding(4, 3, 4, 3);
            Name = "TacheDetailView";
            Size = new Size(376, 627);
            
            ((System.ComponentModel.ISupportInitialize)panelActions).EndInit();
            panelActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)cmbBlocNom).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbMetier).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private KryptonPanel panelActions;
        private KryptonButton btnSupprimer;
        private KryptonCheckedListBox chkListDependances;
        private KryptonLabel lblBlocNom;
        private KryptonComboBox cmbBlocNom;
        private KryptonLabel lblBlocCap;
        private KryptonNumericUpDown numBlocCapacite;
        private KryptonComboBox cmbMetier;
        private KryptonLabel lblMetier;
        private KryptonHeader kryptonHeader1;
        private KryptonLabel kryptonLabel1;
        private KryptonNumericUpDown numHeuresHomme;
        private KryptonLabel lblHeures;
        private KryptonCheckBox chkIsJalon;
        private KryptonTextBox textTacheNom;
        private KryptonLabel lblTacheNom;
    }
}