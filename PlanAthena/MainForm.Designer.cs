namespace PlanAthena
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
            dtpDateFin = new DateTimePicker();
            chkDateFin = new CheckBox();
            dtpDateDebut = new DateTimePicker();
            chkDateDebut = new CheckBox();
            txtDescription = new TextBox();
            label4 = new Label();
            groupBox1 = new GroupBox();
            lblTachesStatus = new Label();
            btnImportTaches = new Button();
            txtTachesPath = new TextBox();
            label3 = new Label();
            lblMetiersStatus = new Label();
            btnImportMetiers = new Button();
            txtMetiersPath = new TextBox();
            label2 = new Label();
            lblOuvriersStatus = new Label();
            btnImportOuvriers = new Button();
            txtOuvriersPath = new TextBox();
            label1 = new Label();
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
            groupBox1.SuspendLayout();
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
            splitContainer1.Panel1.Controls.Add(groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(rtbLog);
            splitContainer1.Size = new Size(784, 711);
            splitContainer1.SplitterDistance = 352;
            splitContainer1.TabIndex = 4;
            // 
            // groupBox3
            // 
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
            groupBox3.Location = new Point(0, 218);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(784, 133);
            groupBox3.TabIndex = 6;
            groupBox3.TabStop = false;
            groupBox3.Text = "Calendrier et Configuration d'Optimisation";
            // 
            // btnGenerateAndTest
            // 
            btnGenerateAndTest.Dock = DockStyle.Bottom;
            btnGenerateAndTest.Location = new Point(3, 107);
            btnGenerateAndTest.Name = "btnGenerateAndTest";
            btnGenerateAndTest.Size = new Size(778, 23);
            btnGenerateAndTest.TabIndex = 14;
            btnGenerateAndTest.Text = "Lancer la planification";
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
            groupBox2.Controls.Add(dtpDateFin);
            groupBox2.Controls.Add(chkDateFin);
            groupBox2.Controls.Add(dtpDateDebut);
            groupBox2.Controls.Add(chkDateDebut);
            groupBox2.Controls.Add(txtDescription);
            groupBox2.Controls.Add(label4);
            groupBox2.Dock = DockStyle.Top;
            groupBox2.Location = new Point(0, 127);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(784, 91);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "Informations du Chantier";
            // 
            // dtpDateFin
            // 
            dtpDateFin.Location = new Point(515, 51);
            dtpDateFin.Name = "dtpDateFin";
            dtpDateFin.Size = new Size(200, 23);
            dtpDateFin.TabIndex = 5;
            // 
            // chkDateFin
            // 
            chkDateFin.AutoSize = true;
            chkDateFin.Location = new Point(442, 52);
            chkDateFin.Name = "chkDateFin";
            chkDateFin.Size = new Size(67, 19);
            chkDateFin.TabIndex = 4;
            chkDateFin.Text = "Date fin";
            chkDateFin.UseVisualStyleBackColor = true;
            // 
            // dtpDateDebut
            // 
            dtpDateDebut.Location = new Point(121, 52);
            dtpDateDebut.Name = "dtpDateDebut";
            dtpDateDebut.Size = new Size(200, 23);
            dtpDateDebut.TabIndex = 3;
            // 
            // chkDateDebut
            // 
            chkDateDebut.AutoSize = true;
            chkDateDebut.Location = new Point(30, 56);
            chkDateDebut.Name = "chkDateDebut";
            chkDateDebut.Size = new Size(85, 19);
            chkDateDebut.TabIndex = 2;
            chkDateDebut.Text = "Date Début";
            chkDateDebut.UseVisualStyleBackColor = true;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(108, 22);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(607, 23);
            txtDescription.TabIndex = 1;
            txtDescription.Text = "Test";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(23, 25);
            label4.Name = "label4";
            label4.Size = new Size(67, 15);
            label4.TabIndex = 0;
            label4.Text = "Description";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblTachesStatus);
            groupBox1.Controls.Add(btnImportTaches);
            groupBox1.Controls.Add(txtTachesPath);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(lblMetiersStatus);
            groupBox1.Controls.Add(btnImportMetiers);
            groupBox1.Controls.Add(txtMetiersPath);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(lblOuvriersStatus);
            groupBox1.Controls.Add(btnImportOuvriers);
            groupBox1.Controls.Add(txtOuvriersPath);
            groupBox1.Controls.Add(label1);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(784, 127);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Imports des Fichiers CSV";
            // 
            // lblTachesStatus
            // 
            lblTachesStatus.AutoSize = true;
            lblTachesStatus.Location = new Point(553, 91);
            lblTachesStatus.Name = "lblTachesStatus";
            lblTachesStatus.Size = new Size(78, 15);
            lblTachesStatus.TabIndex = 11;
            lblTachesStatus.Text = "Aucun fichier";
            // 
            // btnImportTaches
            // 
            btnImportTaches.Location = new Point(472, 87);
            btnImportTaches.Name = "btnImportTaches";
            btnImportTaches.Size = new Size(75, 23);
            btnImportTaches.TabIndex = 10;
            btnImportTaches.Text = "Parcourir...";
            btnImportTaches.UseVisualStyleBackColor = true;
            btnImportTaches.Click += btnImportTaches_Click;
            // 
            // txtTachesPath
            // 
            txtTachesPath.Location = new Point(63, 87);
            txtTachesPath.Name = "txtTachesPath";
            txtTachesPath.ReadOnly = true;
            txtTachesPath.Size = new Size(403, 23);
            txtTachesPath.TabIndex = 9;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 94);
            label3.Name = "label3";
            label3.Size = new Size(43, 15);
            label3.TabIndex = 8;
            label3.Text = "Tâches";
            // 
            // lblMetiersStatus
            // 
            lblMetiersStatus.AutoSize = true;
            lblMetiersStatus.Location = new Point(553, 61);
            lblMetiersStatus.Name = "lblMetiersStatus";
            lblMetiersStatus.Size = new Size(78, 15);
            lblMetiersStatus.TabIndex = 7;
            lblMetiersStatus.Text = "Aucun fichier";
            // 
            // btnImportMetiers
            // 
            btnImportMetiers.Location = new Point(472, 58);
            btnImportMetiers.Name = "btnImportMetiers";
            btnImportMetiers.Size = new Size(75, 23);
            btnImportMetiers.TabIndex = 6;
            btnImportMetiers.Text = "Parcourir...";
            btnImportMetiers.UseVisualStyleBackColor = true;
            btnImportMetiers.Click += btnImportMetiers_Click;
            // 
            // txtMetiersPath
            // 
            txtMetiersPath.Location = new Point(63, 58);
            txtMetiersPath.Name = "txtMetiersPath";
            txtMetiersPath.ReadOnly = true;
            txtMetiersPath.Size = new Size(403, 23);
            txtMetiersPath.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 65);
            label2.Name = "label2";
            label2.Size = new Size(46, 15);
            label2.TabIndex = 4;
            label2.Text = "Métiers";
            // 
            // lblOuvriersStatus
            // 
            lblOuvriersStatus.AutoSize = true;
            lblOuvriersStatus.Location = new Point(553, 32);
            lblOuvriersStatus.Name = "lblOuvriersStatus";
            lblOuvriersStatus.Size = new Size(78, 15);
            lblOuvriersStatus.TabIndex = 3;
            lblOuvriersStatus.Text = "Aucun fichier";
            // 
            // btnImportOuvriers
            // 
            btnImportOuvriers.Location = new Point(472, 29);
            btnImportOuvriers.Name = "btnImportOuvriers";
            btnImportOuvriers.Size = new Size(75, 23);
            btnImportOuvriers.TabIndex = 2;
            btnImportOuvriers.Text = "Parcourir...";
            btnImportOuvriers.UseVisualStyleBackColor = true;
            btnImportOuvriers.Click += btnImportOuvriers_Click;
            // 
            // txtOuvriersPath
            // 
            txtOuvriersPath.Location = new Point(63, 29);
            txtOuvriersPath.Name = "txtOuvriersPath";
            txtOuvriersPath.ReadOnly = true;
            txtOuvriersPath.Size = new Size(403, 23);
            txtOuvriersPath.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 32);
            label1.Name = "label1";
            label1.Size = new Size(51, 15);
            label1.TabIndex = 0;
            label1.Text = "Ouvriers";
            // 
            // rtbLog
            // 
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Location = new Point(0, 0);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(784, 355);
            rtbLog.TabIndex = 0;
            rtbLog.Text = "Bienvenue sur le testeur PlanAthena. Chargez les fichiers et lancez le test.";
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
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private SplitContainer splitContainer1;
        private GroupBox groupBox1;
        private TextBox txtOuvriersPath;
        private Label label1;
        private Label lblMetiersStatus;
        private Button btnImportMetiers;
        private TextBox txtMetiersPath;
        private Label label2;
        private Label lblOuvriersStatus;
        private Button btnImportOuvriers;
        private Label lblTachesStatus;
        private Button btnImportTaches;
        private TextBox txtTachesPath;
        private Label label3;
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
        private RichTextBox rtbLog;
    }
}
