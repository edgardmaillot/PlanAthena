using Krypton.Toolkit;
using System.Drawing;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager
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
            chkListDependances = new KryptonCheckedListBox();
            cmbLots = new KryptonComboBox();
            kryptonLabel2 = new KryptonLabel();
            previewPlan = new PictureBox();
            kryptonPanel1 = new KryptonPanel();
            txtTacheNom = new KryptonTextBox();
            numHeuresHomme = new KryptonNumericUpDown();
            kryptonTableLayoutPanel1 = new KryptonTableLayoutPanel();
            kryptonPanel2 = new KryptonPanel();
            kryptonHeader2 = new KryptonHeader();
            lblBlocNom = new KryptonLabel();
            cmbBlocNom = new KryptonComboBox();
            lblMetier = new KryptonLabel();
            cmbMetier = new KryptonComboBox();
            chkIsJalon = new KryptonCheckBox();
            lblHeures = new KryptonLabel();
            lblTacheNom = new KryptonLabel();
            kryptonPanel3 = new KryptonPanel();
            lblStatut = new KryptonLabel();
            lblStatutValeur = new KryptonLabel();
            lblStatutPicto = new KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)panelActions).BeginInit();
            panelActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbLots).BeginInit();
            ((System.ComponentModel.ISupportInitialize)previewPlan).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel1).BeginInit();
            kryptonPanel1.SuspendLayout();
            kryptonTableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel2).BeginInit();
            kryptonPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbBlocNom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbMetier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel3).BeginInit();
            kryptonPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // panelActions
            // 
            panelActions.Controls.Add(btnSauvegarder);
            panelActions.Controls.Add(btnSupprimer);
            panelActions.Location = new Point(4, 761);
            panelActions.Margin = new Padding(4, 3, 4, 3);
            panelActions.Name = "panelActions";
            panelActions.Size = new Size(280, 45);
            panelActions.TabIndex = 3;
            // 
            // btnSauvegarder
            // 
            btnSauvegarder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSauvegarder.Location = new Point(4, 3);
            btnSauvegarder.Margin = new Padding(4, 3, 4, 3);
            btnSauvegarder.Name = "btnSauvegarder";
            btnSauvegarder.Size = new Size(132, 40);
            btnSauvegarder.TabIndex = 1;
            btnSauvegarder.Values.DropDownArrowColor = Color.Empty;
            btnSauvegarder.Values.Text = "Sauvegarder";
            // 
            // btnSupprimer
            // 
            btnSupprimer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSupprimer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSupprimer.Location = new Point(160, 3);
            btnSupprimer.Margin = new Padding(4, 3, 4, 3);
            btnSupprimer.Name = "btnSupprimer";
            btnSupprimer.Size = new Size(112, 40);
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
            kryptonHeader1.Size = new Size(280, 36);
            kryptonHeader1.TabIndex = 21;
            kryptonHeader1.Values.Description = "";
            kryptonHeader1.Values.Heading = "Détail de la Tâche";
            kryptonHeader1.Values.Image = Properties.Resources.tache;
            kryptonHeader1.Visible = false;
            // 
            // chkListDependances
            // 
            chkListDependances.Dock = DockStyle.Fill;
            chkListDependances.Location = new Point(0, 0);
            chkListDependances.Margin = new Padding(4, 3, 4, 3);
            chkListDependances.Name = "chkListDependances";
            chkListDependances.Size = new Size(282, 338);
            chkListDependances.TabIndex = 20;
            // 
            // cmbLots
            // 
            cmbLots.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLots.DropDownWidth = 198;
            cmbLots.IntegralHeight = false;
            cmbLots.Location = new Point(16, 22);
            cmbLots.Margin = new Padding(4, 3, 4, 3);
            cmbLots.Name = "cmbLots";
            cmbLots.Size = new Size(245, 22);
            cmbLots.TabIndex = 27;
            // 
            // kryptonLabel2
            // 
            kryptonLabel2.Location = new Point(17, 3);
            kryptonLabel2.Name = "kryptonLabel2";
            kryptonLabel2.Size = new Size(56, 20);
            kryptonLabel2.TabIndex = 28;
            kryptonLabel2.Values.Text = "Lot Actif";
            // 
            // previewPlan
            // 
            previewPlan.BackColor = SystemColors.ControlDark;
            previewPlan.BorderStyle = BorderStyle.FixedSingle;
            previewPlan.Cursor = Cursors.Hand;
            previewPlan.Location = new Point(17, 50);
            previewPlan.Margin = new Padding(4, 3, 4, 3);
            previewPlan.Name = "previewPlan";
            previewPlan.Size = new Size(244, 183);
            previewPlan.SizeMode = PictureBoxSizeMode.Zoom;
            previewPlan.TabIndex = 29;
            previewPlan.TabStop = false;
            // 
            // kryptonPanel1
            // 
            kryptonPanel1.Controls.Add(previewPlan);
            kryptonPanel1.Controls.Add(kryptonLabel2);
            kryptonPanel1.Controls.Add(cmbLots);
            kryptonPanel1.Location = new Point(3, 3);
            kryptonPanel1.Name = "kryptonPanel1";
            kryptonPanel1.Size = new Size(280, 237);
            kryptonPanel1.TabIndex = 28;
            // 
            // txtTacheNom
            // 
            txtTacheNom.Location = new Point(75, 7);
            txtTacheNom.Name = "txtTacheNom";
            txtTacheNom.Size = new Size(187, 23);
            txtTacheNom.TabIndex = 30;
            txtTacheNom.Text = "nom";
            // 
            // numHeuresHomme
            // 
            numHeuresHomme.Anchor = AnchorStyles.Top;
            numHeuresHomme.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numHeuresHomme.Location = new Point(149, 33);
            numHeuresHomme.Margin = new Padding(4, 3, 4, 3);
            numHeuresHomme.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numHeuresHomme.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHeuresHomme.Name = "numHeuresHomme";
            numHeuresHomme.Size = new Size(59, 22);
            numHeuresHomme.TabIndex = 33;
            numHeuresHomme.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // kryptonTableLayoutPanel1
            // 
            kryptonTableLayoutPanel1.AutoSize = true;
            kryptonTableLayoutPanel1.ColumnCount = 1;
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            kryptonTableLayoutPanel1.Controls.Add(panelActions, 0, 3);
            kryptonTableLayoutPanel1.Controls.Add(kryptonPanel2, 0, 1);
            kryptonTableLayoutPanel1.Controls.Add(kryptonPanel1, 0, 0);
            kryptonTableLayoutPanel1.Controls.Add(kryptonPanel3, 0, 2);
            kryptonTableLayoutPanel1.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel1.Location = new Point(0, 36);
            kryptonTableLayoutPanel1.Name = "kryptonTableLayoutPanel1";
            kryptonTableLayoutPanel1.RowCount = 4;
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 243F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 171F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 51F));
            kryptonTableLayoutPanel1.Size = new Size(280, 809);
            kryptonTableLayoutPanel1.TabIndex = 29;
            // 
            // kryptonPanel2
            // 
            kryptonPanel2.Controls.Add(lblStatutPicto);
            kryptonPanel2.Controls.Add(lblStatutValeur);
            kryptonPanel2.Controls.Add(lblStatut);
            kryptonPanel2.Controls.Add(kryptonHeader2);
            kryptonPanel2.Controls.Add(lblBlocNom);
            kryptonPanel2.Controls.Add(cmbBlocNom);
            kryptonPanel2.Controls.Add(lblMetier);
            kryptonPanel2.Controls.Add(cmbMetier);
            kryptonPanel2.Controls.Add(chkIsJalon);
            kryptonPanel2.Controls.Add(lblHeures);
            kryptonPanel2.Controls.Add(lblTacheNom);
            kryptonPanel2.Controls.Add(txtTacheNom);
            kryptonPanel2.Controls.Add(numHeuresHomme);
            kryptonPanel2.Location = new Point(3, 246);
            kryptonPanel2.Name = "kryptonPanel2";
            kryptonPanel2.Size = new Size(277, 165);
            kryptonPanel2.TabIndex = 21;
            // 
            // kryptonHeader2
            // 
            kryptonHeader2.Dock = DockStyle.Bottom;
            kryptonHeader2.HeaderStyle = HeaderStyle.Secondary;
            kryptonHeader2.Location = new Point(0, 143);
            kryptonHeader2.Name = "kryptonHeader2";
            kryptonHeader2.Size = new Size(277, 22);
            kryptonHeader2.TabIndex = 43;
            kryptonHeader2.Values.Description = "";
            kryptonHeader2.Values.Heading = "Dépendances";
            kryptonHeader2.Values.Image = null;
            // 
            // lblBlocNom
            // 
            lblBlocNom.Location = new Point(20, 89);
            lblBlocNom.Margin = new Padding(4, 3, 4, 3);
            lblBlocNom.Name = "lblBlocNom";
            lblBlocNom.Size = new Size(40, 20);
            lblBlocNom.TabIndex = 42;
            lblBlocNom.Values.Text = "Bloc :";
            // 
            // cmbBlocNom
            // 
            cmbBlocNom.AllowDrop = true;
            cmbBlocNom.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBlocNom.DropDownWidth = 250;
            cmbBlocNom.IntegralHeight = false;
            cmbBlocNom.Location = new Point(71, 87);
            cmbBlocNom.Margin = new Padding(4, 3, 4, 3);
            cmbBlocNom.Name = "cmbBlocNom";
            cmbBlocNom.Size = new Size(192, 22);
            cmbBlocNom.TabIndex = 41;
            cmbBlocNom.UseMnemonic = false;
            // 
            // lblMetier
            // 
            lblMetier.Location = new Point(16, 61);
            lblMetier.Margin = new Padding(4, 3, 4, 3);
            lblMetier.Name = "lblMetier";
            lblMetier.Size = new Size(52, 20);
            lblMetier.TabIndex = 40;
            lblMetier.Values.Text = "Métier :";
            // 
            // cmbMetier
            // 
            cmbMetier.AllowDrop = true;
            cmbMetier.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetier.DropDownWidth = 250;
            cmbMetier.IntegralHeight = false;
            cmbMetier.Location = new Point(71, 59);
            cmbMetier.Margin = new Padding(4, 3, 4, 3);
            cmbMetier.Name = "cmbMetier";
            cmbMetier.Size = new Size(192, 22);
            cmbMetier.TabIndex = 39;
            cmbMetier.UseMnemonic = false;
            // 
            // chkIsJalon
            // 
            chkIsJalon.Location = new Point(24, 33);
            chkIsJalon.Margin = new Padding(4, 3, 4, 3);
            chkIsJalon.Name = "chkIsJalon";
            chkIsJalon.Size = new Size(86, 20);
            chkIsJalon.TabIndex = 38;
            chkIsJalon.Values.Text = "Est un jalon";
            // 
            // lblHeures
            // 
            lblHeures.Anchor = AnchorStyles.Top;
            lblHeures.Location = new Point(207, 33);
            lblHeures.Margin = new Padding(4, 3, 4, 3);
            lblHeures.Name = "lblHeures";
            lblHeures.Size = new Size(47, 20);
            lblHeures.TabIndex = 37;
            lblHeures.Values.Text = "heures";
            // 
            // lblTacheNom
            // 
            lblTacheNom.Location = new Point(24, 7);
            lblTacheNom.Margin = new Padding(4, 3, 4, 3);
            lblTacheNom.Name = "lblTacheNom";
            lblTacheNom.Size = new Size(44, 20);
            lblTacheNom.TabIndex = 36;
            lblTacheNom.Values.Text = "Nom :";
            // 
            // kryptonPanel3
            // 
            kryptonPanel3.Controls.Add(chkListDependances);
            kryptonPanel3.Dock = DockStyle.Fill;
            kryptonPanel3.Location = new Point(3, 417);
            kryptonPanel3.Name = "kryptonPanel3";
            kryptonPanel3.Size = new Size(282, 338);
            kryptonPanel3.TabIndex = 27;
            // 
            // lblStatut
            // 
            lblStatut.Location = new Point(11, 117);
            lblStatut.Name = "lblStatut";
            lblStatut.Size = new Size(49, 20);
            lblStatut.TabIndex = 44;
            lblStatut.Values.Text = "Statut :";
            // 
            // lblStatutValeur
            // 
            lblStatutValeur.Location = new Point(103, 117);
            lblStatutValeur.Name = "lblStatutValeur";
            lblStatutValeur.Size = new Size(56, 20);
            lblStatutValeur.TabIndex = 45;
            lblStatutValeur.Values.Text = "Planifiée";
            // 
            // lblStatutPicto
            // 
            lblStatutPicto.Location = new Point(66, 117);
            lblStatutPicto.MaximumSize = new Size(32, 22);
            lblStatutPicto.MinimumSize = new Size(32, 22);
            lblStatutPicto.Name = "lblStatutPicto";
            lblStatutPicto.Size = new Size(32, 22);
            lblStatutPicto.StateNormal.ShortText.Font = new Font("Segoe UI Symbol", 14F);
            lblStatutPicto.TabIndex = 46;
            lblStatutPicto.Values.Text = "";
            // 
            // TacheDetailView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonTableLayoutPanel1);
            Controls.Add(kryptonHeader1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "TacheDetailView";
            Size = new Size(280, 845);
            ((System.ComponentModel.ISupportInitialize)panelActions).EndInit();
            panelActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)cmbLots).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewPlan).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
            kryptonPanel1.ResumeLayout(false);
            kryptonPanel1.PerformLayout();
            kryptonTableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonPanel2).EndInit();
            kryptonPanel2.ResumeLayout(false);
            kryptonPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbBlocNom).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbMetier).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel3).EndInit();
            kryptonPanel3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private KryptonPanel panelActions;
        private KryptonButton btnSupprimer;
        private KryptonButton btnSauvegarder;
        private KryptonHeader kryptonHeader1;
        private KryptonCheckedListBox chkListDependances;
        private KryptonComboBox cmbLots;
        private KryptonLabel kryptonLabel2;
        private PictureBox previewPlan;
        private KryptonPanel kryptonPanel1;
        private KryptonTextBox txtTacheNom;
        private KryptonNumericUpDown numHeuresHomme;
        private KryptonTableLayoutPanel kryptonTableLayoutPanel1;
        private KryptonPanel kryptonPanel2;
        private KryptonLabel lblBlocNom;
        private KryptonComboBox cmbBlocNom;
        private KryptonLabel lblMetier;
        private KryptonComboBox cmbMetier;
        private KryptonCheckBox chkIsJalon;
        private KryptonLabel lblHeures;
        private KryptonLabel lblTacheNom;
        private KryptonPanel kryptonPanel3;
        private KryptonHeader kryptonHeader2;
        private KryptonLabel lblStatut;
        private KryptonLabel lblStatutPicto;
        private KryptonLabel lblStatutValeur;
    }
}