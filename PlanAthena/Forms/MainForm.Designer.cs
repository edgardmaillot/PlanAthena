namespace PlanAthena.Forms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            groupBox3 = new GroupBox();
            btnExportGantt = new Button();
            btnExportPlanningExcel = new Button();
            btnGenerateAndTest = new Button();
            numCoutIndirect = new NumericUpDown();
            label11 = new Label();
            numPenaliteChangement = new NumericUpDown();
            label10 = new Label();
            numDureeStandard = new NumericUpDown();
            label9 = new Label();
            cmbTypeDeSortie = new ComboBox();
            label8 = new Label();
            numHeuresTravail = new NumericUpDown();
            label7 = new Label();
            numHeureDebut = new NumericUpDown();
            label6 = new Label();
            chkListJoursOuvres = new CheckedListBox();
            label5 = new Label();
            groupBox2 = new GroupBox();
            lblMapping = new Label();
            lblResume = new Label();
            dtpDateFin = new DateTimePicker();
            chkDateFin = new CheckBox();
            dtpDateDebut = new DateTimePicker();
            chkDateDebut = new CheckBox();
            txtDescription = new TextBox();
            label4 = new Label();
            lblDateCreation = new Label();
            txtAuteur = new TextBox();
            labelAuteur = new Label();
            txtNomProjet = new TextBox();
            labelNomProjet = new Label();
            rtbLog = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCoutIndirect).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPenaliteChangement).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDureeStandard).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeuresTravail).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeureDebut).BeginInit();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.MinimumSize = new Size(0, 711);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(groupBox3);
            splitContainer1.Panel1.Controls.Add(groupBox2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(rtbLog);
            splitContainer1.Size = new Size(784, 711);
            splitContainer1.SplitterDistance = 303;
            splitContainer1.TabIndex = 4;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnExportGantt);
            groupBox3.Controls.Add(btnExportPlanningExcel);
            groupBox3.Controls.Add(btnGenerateAndTest);
            groupBox3.Controls.Add(numCoutIndirect);
            groupBox3.Controls.Add(label11);
            groupBox3.Controls.Add(numPenaliteChangement);
            groupBox3.Controls.Add(label10);
            groupBox3.Controls.Add(numDureeStandard);
            groupBox3.Controls.Add(label9);
            groupBox3.Controls.Add(cmbTypeDeSortie);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(numHeuresTravail);
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(numHeureDebut);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(chkListJoursOuvres);
            groupBox3.Controls.Add(label5);
            groupBox3.Dock = DockStyle.Top;
            groupBox3.Location = new Point(0, 158);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(784, 133);
            groupBox3.TabIndex = 6;
            groupBox3.TabStop = false;
            groupBox3.Text = "Configuration et Planification";
            // 
            // btnExportGantt
            // 
            btnExportGantt.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportGantt.Enabled = false;
            btnExportGantt.Location = new Point(481, 107);
            btnExportGantt.Name = "btnExportGantt";
            btnExportGantt.Size = new Size(151, 23);
            btnExportGantt.TabIndex = 15;
            btnExportGantt.Text = "📊 Export GanttProject";
            btnExportGantt.UseVisualStyleBackColor = true;
            btnExportGantt.Click += btnExportGantt_Click;
            // 
            // btnExportPlanningExcel
            // 
            btnExportPlanningExcel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportPlanningExcel.Enabled = false;
            btnExportPlanningExcel.Location = new Point(638, 107);
            btnExportPlanningExcel.Name = "btnExportPlanningExcel";
            btnExportPlanningExcel.Size = new Size(134, 23);
            btnExportPlanningExcel.TabIndex = 15;
            btnExportPlanningExcel.Text = "📊 Export Planning";
            btnExportPlanningExcel.UseVisualStyleBackColor = true;
            btnExportPlanningExcel.Click += btnExportPlanningExcel_Click;
            // 
            // btnGenerateAndTest
            // 
            btnGenerateAndTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnGenerateAndTest.Location = new Point(3, 107);
            btnGenerateAndTest.Name = "btnGenerateAndTest";
            btnGenerateAndTest.Size = new Size(467, 23);
            btnGenerateAndTest.TabIndex = 14;
            btnGenerateAndTest.Text = "🚀 LANCER LA PLANIFICATION";
            btnGenerateAndTest.UseVisualStyleBackColor = true;
            btnGenerateAndTest.Click += btnGenerateAndTest_Click;
            // 
            // numCoutIndirect
            // 
            numCoutIndirect.DecimalPlaces = 1;
            numCoutIndirect.Location = new Point(566, 48);
            numCoutIndirect.Name = "numCoutIndirect";
            numCoutIndirect.Size = new Size(53, 23);
            numCoutIndirect.TabIndex = 13;
            numCoutIndirect.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(460, 50);
            label11.Name = "label11";
            label11.Size = new Size(97, 15);
            label11.TabIndex = 12;
            label11.Text = "Coût indirect (%)";
            // 
            // numPenaliteChangement
            // 
            numPenaliteChangement.DecimalPlaces = 1;
            numPenaliteChangement.Location = new Point(385, 47);
            numPenaliteChangement.Name = "numPenaliteChangement";
            numPenaliteChangement.Size = new Size(51, 23);
            numPenaliteChangement.TabIndex = 11;
            numPenaliteChangement.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(225, 47);
            label10.Name = "label10";
            label10.Size = new Size(140, 15);
            label10.TabIndex = 10;
            label10.Text = "Pénalité changement (%)";
            // 
            // numDureeStandard
            // 
            numDureeStandard.Location = new Point(384, 76);
            numDureeStandard.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            numDureeStandard.Name = "numDureeStandard";
            numDureeStandard.Size = new Size(51, 23);
            numDureeStandard.TabIndex = 9;
            numDureeStandard.Value = new decimal(new int[] { 7, 0, 0, 0 });
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(245, 78);
            label9.Name = "label9";
            label9.Size = new Size(113, 15);
            label9.TabIndex = 8;
            label9.Text = "Durée de travail (7h)";
            // 
            // cmbTypeDeSortie
            // 
            cmbTypeDeSortie.FormattingEnabled = true;
            cmbTypeDeSortie.Location = new Point(547, 75);
            cmbTypeDeSortie.Name = "cmbTypeDeSortie";
            cmbTypeDeSortie.Size = new Size(121, 23);
            cmbTypeDeSortie.TabIndex = 7;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(460, 78);
            label8.Name = "label8";
            label8.Size = new Size(81, 15);
            label8.TabIndex = 6;
            label8.Text = "Type de Sortie";
            // 
            // numHeuresTravail
            // 
            numHeuresTravail.Location = new Point(566, 19);
            numHeuresTravail.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numHeuresTravail.Name = "numHeuresTravail";
            numHeuresTravail.Size = new Size(53, 23);
            numHeuresTravail.TabIndex = 5;
            numHeuresTravail.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(458, 21);
            label7.Name = "label7";
            label7.Size = new Size(102, 15);
            label7.TabIndex = 4;
            label7.Text = "Durée d'ouverture";
            // 
            // numHeureDebut
            // 
            numHeureDebut.Location = new Point(384, 17);
            numHeureDebut.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            numHeureDebut.Name = "numHeureDebut";
            numHeureDebut.Size = new Size(51, 23);
            numHeureDebut.TabIndex = 3;
            numHeureDebut.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(239, 19);
            label6.Name = "label6";
            label6.Size = new Size(139, 15);
            label6.TabIndex = 2;
            label6.Text = "Heure ouverture chantier";
            // 
            // chkListJoursOuvres
            // 
            chkListJoursOuvres.FormattingEnabled = true;
            chkListJoursOuvres.Location = new Point(84, 22);
            chkListJoursOuvres.Name = "chkListJoursOuvres";
            chkListJoursOuvres.Size = new Size(120, 76);
            chkListJoursOuvres.TabIndex = 1;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 22);
            label5.Name = "label5";
            label5.Size = new Size(72, 15);
            label5.TabIndex = 0;
            label5.Text = "Jours ouvrés";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lblMapping);
            groupBox2.Controls.Add(lblResume);
            groupBox2.Controls.Add(dtpDateFin);
            groupBox2.Controls.Add(chkDateFin);
            groupBox2.Controls.Add(dtpDateDebut);
            groupBox2.Controls.Add(chkDateDebut);
            groupBox2.Controls.Add(txtDescription);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(lblDateCreation);
            groupBox2.Controls.Add(txtAuteur);
            groupBox2.Controls.Add(labelAuteur);
            groupBox2.Controls.Add(txtNomProjet);
            groupBox2.Controls.Add(labelNomProjet);
            groupBox2.Dock = DockStyle.Top;
            groupBox2.Location = new Point(0, 0);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(784, 158);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "Informations du Projet";
            // 
            // lblMapping
            // 
            lblMapping.AutoSize = true;
            lblMapping.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblMapping.ForeColor = Color.Blue;
            lblMapping.Location = new Point(400, 130);
            lblMapping.Name = "lblMapping";
            lblMapping.Size = new Size(182, 15);
            lblMapping.TabIndex = 12;
            lblMapping.Text = "Mapping: 89% (139/156 tâches)";
            // 
            // lblResume
            // 
            lblResume.AutoSize = true;
            lblResume.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblResume.ForeColor = Color.DarkGreen;
            lblResume.Location = new Point(23, 130);
            lblResume.Name = "lblResume";
            lblResume.Size = new Size(253, 15);
            lblResume.TabIndex = 11;
            lblResume.Text = "Résumé: 25 ouvriers, 12 métiers, 156 tâches";
            // 
            // dtpDateFin
            // 
            dtpDateFin.Location = new Point(515, 98);
            dtpDateFin.Name = "dtpDateFin";
            dtpDateFin.Size = new Size(200, 23);
            dtpDateFin.TabIndex = 10;
            // 
            // chkDateFin
            // 
            chkDateFin.AutoSize = true;
            chkDateFin.Location = new Point(442, 99);
            chkDateFin.Name = "chkDateFin";
            chkDateFin.Size = new Size(67, 19);
            chkDateFin.TabIndex = 9;
            chkDateFin.Text = "Date fin";
            chkDateFin.UseVisualStyleBackColor = true;
            // 
            // dtpDateDebut
            // 
            dtpDateDebut.Location = new Point(121, 99);
            dtpDateDebut.Name = "dtpDateDebut";
            dtpDateDebut.Size = new Size(200, 23);
            dtpDateDebut.TabIndex = 8;
            // 
            // chkDateDebut
            // 
            chkDateDebut.AutoSize = true;
            chkDateDebut.Location = new Point(30, 103);
            chkDateDebut.Name = "chkDateDebut";
            chkDateDebut.Size = new Size(85, 19);
            chkDateDebut.TabIndex = 7;
            chkDateDebut.Text = "Date Début";
            chkDateDebut.UseVisualStyleBackColor = true;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(108, 69);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(607, 23);
            txtDescription.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(23, 72);
            label4.Name = "label4";
            label4.Size = new Size(67, 15);
            label4.TabIndex = 5;
            label4.Text = "Description";
            // 
            // lblDateCreation
            // 
            lblDateCreation.AutoSize = true;
            lblDateCreation.Location = new Point(500, 47);
            lblDateCreation.Name = "lblDateCreation";
            lblDateCreation.Size = new Size(89, 15);
            lblDateCreation.TabIndex = 4;
            lblDateCreation.Text = "Créé le: __/__/__";
            // 
            // txtAuteur
            // 
            txtAuteur.Location = new Point(270, 44);
            txtAuteur.Name = "txtAuteur";
            txtAuteur.Size = new Size(200, 23);
            txtAuteur.TabIndex = 3;
            // 
            // labelAuteur
            // 
            labelAuteur.AutoSize = true;
            labelAuteur.Location = new Point(220, 47);
            labelAuteur.Name = "labelAuteur";
            labelAuteur.Size = new Size(46, 15);
            labelAuteur.TabIndex = 2;
            labelAuteur.Text = "Auteur:";
            // 
            // txtNomProjet
            // 
            txtNomProjet.Location = new Point(70, 44);
            txtNomProjet.Name = "txtNomProjet";
            txtNomProjet.Size = new Size(140, 23);
            txtNomProjet.TabIndex = 1;
            // 
            // labelNomProjet
            // 
            labelNomProjet.AutoSize = true;
            labelNomProjet.Location = new Point(23, 47);
            labelNomProjet.Name = "labelNomProjet";
            labelNomProjet.Size = new Size(41, 15);
            labelNomProjet.TabIndex = 0;
            labelNomProjet.Text = "Projet:";
            // 
            // rtbLog
            // 
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Location = new Point(0, 0);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(784, 404);
            rtbLog.TabIndex = 0;
            rtbLog.Text = "Bienvenue sur PlanAthena. Créez un nouveau projet ou chargez un projet existant.";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 711);
            Controls.Add(splitContainer1);
            Name = "MainForm";
            Text = "PlanAthena";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCoutIndirect).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPenaliteChangement).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDureeStandard).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeuresTravail).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeureDebut).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private SplitContainer splitContainer1;
        private GroupBox groupBox2;
        private TextBox txtDescription;
        private Label label4;
        private GroupBox groupBox3;
        private CheckedListBox chkListJoursOuvres;
        private Label label5;
        private DateTimePicker dtpDateFin;
        private CheckBox chkDateFin;
        private DateTimePicker dtpDateDebut;
        private CheckBox chkDateDebut;
        private Label label7;
        private NumericUpDown numHeureDebut;
        private Label label6;
        private Label label9;
        private ComboBox cmbTypeDeSortie;
        private Label label8;
        private NumericUpDown numHeuresTravail;
        private NumericUpDown numPenaliteChangement;
        private Label label10;
        private NumericUpDown numDureeStandard;
        private NumericUpDown numCoutIndirect;
        private Label label11;
        private Button btnGenerateAndTest;
        private Button btnExportGantt;
        private Button btnExportPlanningExcel;
        private RichTextBox rtbLog;

        // Nouveaux contrôles pour les informations projet
        private TextBox txtNomProjet;
        private Label labelNomProjet;
        private TextBox txtAuteur;
        private Label labelAuteur;
        private Label lblDateCreation;
        private Label lblResume;
        private Label lblMapping;
    }
}