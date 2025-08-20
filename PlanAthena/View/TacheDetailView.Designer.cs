using Krypton.Toolkit;
using System.Drawing;
using System.Windows.Forms;

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
            btnSauvegarder = new KryptonButton();
            btnSupprimer = new KryptonButton();
            kryptonHeader1 = new KryptonHeader();
            kryptonGroup1 = new KryptonGroup();
            lblTacheNom = new KryptonLabel();
            txtTacheNom = new KryptonTextBox();
            chkIsJalon = new KryptonCheckBox();
            lblHeures = new KryptonLabel();
            numHeuresHomme = new KryptonNumericUpDown();
            lblMetier = new KryptonLabel();
            cmbMetier = new KryptonComboBox();
            lblBlocNom = new KryptonLabel();
            cmbBlocNom = new KryptonComboBox();
            lblBlocCap = new KryptonLabel();
            numBlocCapacite = new KryptonNumericUpDown();
            kryptonLabel1 = new KryptonLabel();
            chkListDependances = new KryptonCheckedListBox();
            ((System.ComponentModel.ISupportInitialize)panelActions).BeginInit();
            panelActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonGroup1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonGroup1.Panel).BeginInit();
            kryptonGroup1.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbMetier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbBlocNom).BeginInit();
            SuspendLayout();
            // 
            // panelActions
            // 
            panelActions.Controls.Add(btnSauvegarder);
            panelActions.Controls.Add(btnSupprimer);
            panelActions.Dock = DockStyle.Bottom;
            panelActions.Location = new Point(0, 680);
            panelActions.Margin = new Padding(4, 3, 4, 3);
            panelActions.Name = "panelActions";
            panelActions.Size = new Size(439, 67);
            panelActions.TabIndex = 3;
            // 
            // btnSauvegarder
            // 
            btnSauvegarder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSauvegarder.Location = new Point(22, 14);
            btnSauvegarder.Margin = new Padding(4, 3, 4, 3);
            btnSauvegarder.Name = "btnSauvegarder";
            btnSauvegarder.Size = new Size(259, 40);
            btnSauvegarder.TabIndex = 1;
            btnSauvegarder.Values.DropDownArrowColor = Color.Empty;
            btnSauvegarder.Values.Text = "Sauvegarder";
            // 
            // btnSupprimer
            // 
            btnSupprimer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSupprimer.Location = new Point(296, 14);
            btnSupprimer.Margin = new Padding(4, 3, 4, 3);
            btnSupprimer.Name = "btnSupprimer";
            btnSupprimer.Size = new Size(122, 40);
            btnSupprimer.TabIndex = 0;
            btnSupprimer.Values.DropDownArrowColor = Color.Empty;
            btnSupprimer.Values.Text = "Supprimer";
            // 
            // kryptonHeader1
            // 
            kryptonHeader1.Dock = DockStyle.Top;
            kryptonHeader1.Location = new Point(0, 0);
            kryptonHeader1.Margin = new Padding(4, 3, 4, 3);
            kryptonHeader1.Name = "kryptonHeader1";
            kryptonHeader1.Size = new Size(439, 36);
            kryptonHeader1.TabIndex = 21;
            kryptonHeader1.Values.Description = "";
            kryptonHeader1.Values.Heading = "Détail de la Tâche";
            kryptonHeader1.Values.Image = Properties.Resources.tache;
            // 
            // kryptonGroup1
            // 
            kryptonGroup1.Dock = DockStyle.Top;
            kryptonGroup1.Location = new Point(0, 36);
            kryptonGroup1.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            kryptonGroup1.Panel.Controls.Add(lblTacheNom);
            kryptonGroup1.Panel.Controls.Add(txtTacheNom);
            kryptonGroup1.Panel.Controls.Add(chkIsJalon);
            kryptonGroup1.Panel.Controls.Add(lblHeures);
            kryptonGroup1.Panel.Controls.Add(numHeuresHomme);
            kryptonGroup1.Panel.Controls.Add(lblMetier);
            kryptonGroup1.Panel.Controls.Add(cmbMetier);
            kryptonGroup1.Panel.Controls.Add(lblBlocNom);
            kryptonGroup1.Panel.Controls.Add(cmbBlocNom);
            kryptonGroup1.Panel.Controls.Add(lblBlocCap);
            kryptonGroup1.Panel.Controls.Add(numBlocCapacite);
            kryptonGroup1.Size = new Size(439, 217);
            kryptonGroup1.TabIndex = 22;
            // 
            // lblTacheNom
            // 
            lblTacheNom.Location = new Point(22, 20);
            lblTacheNom.Margin = new Padding(4, 3, 4, 3);
            lblTacheNom.Name = "lblTacheNom";
            lblTacheNom.Size = new Size(44, 20);
            lblTacheNom.TabIndex = 15;
            lblTacheNom.Values.Text = "Nom :";
            // 
            // txtTacheNom
            // 
            txtTacheNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTacheNom.Location = new Point(93, 16);
            txtTacheNom.Margin = new Padding(4, 3, 4, 3);
            txtTacheNom.Name = "txtTacheNom";
            txtTacheNom.Size = new Size(337, 23);
            txtTacheNom.TabIndex = 14;
            // 
            // chkIsJalon
            // 
            chkIsJalon.Location = new Point(26, 50);
            chkIsJalon.Margin = new Padding(4, 3, 4, 3);
            chkIsJalon.Name = "chkIsJalon";
            chkIsJalon.Size = new Size(130, 20);
            chkIsJalon.TabIndex = 11;
            chkIsJalon.Values.Text = "Est un jalon/attente";
            // 
            // lblHeures
            // 
            lblHeures.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblHeures.Location = new Point(246, 50);
            lblHeures.Margin = new Padding(4, 3, 4, 3);
            lblHeures.Name = "lblHeures";
            lblHeures.Size = new Size(67, 20);
            lblHeures.TabIndex = 12;
            lblHeures.Values.Text = "Durée (h) :";
            // 
            // numHeuresHomme
            // 
            numHeuresHomme.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numHeuresHomme.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numHeuresHomme.Location = new Point(322, 50);
            numHeuresHomme.Margin = new Padding(4, 3, 4, 3);
            numHeuresHomme.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numHeuresHomme.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHeuresHomme.Name = "numHeuresHomme";
            numHeuresHomme.Size = new Size(108, 22);
            numHeuresHomme.TabIndex = 13;
            numHeuresHomme.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // lblMetier
            // 
            lblMetier.Location = new Point(23, 99);
            lblMetier.Margin = new Padding(4, 3, 4, 3);
            lblMetier.Name = "lblMetier";
            lblMetier.Size = new Size(52, 20);
            lblMetier.TabIndex = 10;
            lblMetier.Values.Text = "Métier :";
            // 
            // cmbMetier
            // 
            cmbMetier.AllowDrop = true;
            cmbMetier.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbMetier.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetier.DropDownWidth = 250;
            cmbMetier.IntegralHeight = false;
            cmbMetier.Location = new Point(93, 99);
            cmbMetier.Margin = new Padding(4, 3, 4, 3);
            cmbMetier.Name = "cmbMetier";
            cmbMetier.Size = new Size(250, 22);
            cmbMetier.TabIndex = 9;
            cmbMetier.UseMnemonic = false;
            // 
            // lblBlocNom
            // 
            lblBlocNom.Location = new Point(37, 148);
            lblBlocNom.Margin = new Padding(4, 3, 4, 3);
            lblBlocNom.Name = "lblBlocNom";
            lblBlocNom.Size = new Size(40, 20);
            lblBlocNom.TabIndex = 5;
            lblBlocNom.Values.Text = "Bloc :";
            // 
            // cmbBlocNom
            // 
            cmbBlocNom.AllowDrop = true;
            cmbBlocNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbBlocNom.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBlocNom.DropDownWidth = 250;
            cmbBlocNom.IntegralHeight = false;
            cmbBlocNom.Location = new Point(93, 148);
            cmbBlocNom.Margin = new Padding(4, 3, 4, 3);
            cmbBlocNom.Name = "cmbBlocNom";
            cmbBlocNom.Size = new Size(250, 22);
            cmbBlocNom.TabIndex = 6;
            cmbBlocNom.UseMnemonic = false;
            // 
            // lblBlocCap
            // 
            lblBlocCap.Location = new Point(8, 180);
            lblBlocCap.Margin = new Padding(4, 3, 4, 3);
            lblBlocCap.Name = "lblBlocCap";
            lblBlocCap.Size = new Size(63, 20);
            lblBlocCap.TabIndex = 23;
            lblBlocCap.Values.Text = "Capacité :";
            // 
            // numBlocCapacite
            // 
            numBlocCapacite.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numBlocCapacite.Location = new Point(93, 180);
            numBlocCapacite.Margin = new Padding(4, 3, 4, 3);
            numBlocCapacite.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numBlocCapacite.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numBlocCapacite.Name = "numBlocCapacite";
            numBlocCapacite.Size = new Size(140, 22);
            numBlocCapacite.TabIndex = 24;
            numBlocCapacite.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // kryptonLabel1
            // 
            kryptonLabel1.Dock = DockStyle.Top;
            kryptonLabel1.Location = new Point(0, 253);
            kryptonLabel1.Margin = new Padding(4, 3, 4, 3);
            kryptonLabel1.Name = "kryptonLabel1";
            kryptonLabel1.PaletteMode = PaletteMode.Office2007BlueDarkMode;
            kryptonLabel1.Size = new Size(439, 20);
            kryptonLabel1.TabIndex = 26;
            kryptonLabel1.Values.Text = "Dépendances";
            // 
            // chkListDependances
            // 
            chkListDependances.Dock = DockStyle.Fill;
            chkListDependances.Location = new Point(0, 273);
            chkListDependances.Margin = new Padding(4, 3, 4, 3);
            chkListDependances.Name = "chkListDependances";
            chkListDependances.Size = new Size(439, 407);
            chkListDependances.TabIndex = 20;
            // 
            // TacheDetailView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(chkListDependances);
            Controls.Add(kryptonLabel1);
            Controls.Add(kryptonGroup1);
            Controls.Add(kryptonHeader1);
            Controls.Add(panelActions);
            Margin = new Padding(4, 3, 4, 3);
            Name = "TacheDetailView";
            Size = new Size(439, 747);
            ((System.ComponentModel.ISupportInitialize)panelActions).EndInit();
            panelActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonGroup1.Panel).EndInit();
            kryptonGroup1.Panel.ResumeLayout(false);
            kryptonGroup1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonGroup1).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbMetier).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbBlocNom).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private KryptonPanel panelActions;
        private KryptonButton btnSupprimer;
        private KryptonButton btnSauvegarder;
        private KryptonHeader kryptonHeader1;
        private KryptonGroup kryptonGroup1;
        private KryptonLabel lblTacheNom;
        private KryptonTextBox txtTacheNom;
        private KryptonCheckBox chkIsJalon;
        private KryptonLabel lblHeures;
        private KryptonNumericUpDown numHeuresHomme;
        private KryptonLabel lblMetier;
        private KryptonComboBox cmbMetier;
        private KryptonLabel lblBlocNom;
        private KryptonComboBox cmbBlocNom;
        private KryptonLabel lblBlocCap;
        private KryptonNumericUpDown numBlocCapacite;
        private KryptonLabel kryptonLabel1;
        private KryptonCheckedListBox chkListDependances;
    }
}