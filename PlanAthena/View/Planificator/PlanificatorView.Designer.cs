using Krypton.Toolkit;

namespace PlanAthena.View.Planificator
{
    partial class PlanificatorView
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
            kryptonPanelMain = new KryptonPanel();
            rtbLog = new KryptonRichTextBox();
            groupConfig = new KryptonGroupBox();
            dtpDateFin = new KryptonDateTimePicker();
            chkDateFin = new KryptonCheckBox();
            dtpDateDebut = new KryptonDateTimePicker();
            chkDateDebut = new KryptonCheckBox();
            btnExportGantt = new KryptonButton();
            btnExportPlanningExcel = new KryptonButton();
            btnLaunch = new KryptonButton();
            cmbCalculMax = new KryptonComboBox();
            labelCalculMax = new KryptonLabel();
            cmbTypeDeSortie = new KryptonComboBox();
            label8 = new KryptonLabel();
            numCoutIndirect = new KryptonNumericUpDown();
            label11 = new KryptonLabel();
            numPenaliteChangement = new KryptonNumericUpDown();
            label10 = new KryptonLabel();
            numHeuresTravail = new KryptonNumericUpDown();
            label9 = new KryptonLabel();
            numDureeOuverture = new KryptonNumericUpDown();
            label7 = new KryptonLabel();
            numHeureDebut = new KryptonNumericUpDown();
            label6 = new KryptonLabel();
            chkListJoursOuvres = new KryptonCheckedListBox();
            label5 = new KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupConfig).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupConfig.Panel).BeginInit();
            groupConfig.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbCalculMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbTypeDeSortie).BeginInit();
            SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            kryptonPanelMain.Controls.Add(rtbLog);
            kryptonPanelMain.Controls.Add(groupConfig);
            kryptonPanelMain.Dock = DockStyle.Fill;
            kryptonPanelMain.Location = new Point(0, 0);
            kryptonPanelMain.Margin = new Padding(4, 3, 4, 3);
            kryptonPanelMain.Name = "kryptonPanelMain";
            kryptonPanelMain.Padding = new Padding(12, 12, 12, 12);
            kryptonPanelMain.Size = new Size(1003, 692);
            kryptonPanelMain.TabIndex = 0;
            // 
            // rtbLog
            // 
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Location = new Point(12, 229);
            rtbLog.Margin = new Padding(4, 3, 4, 3);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(979, 451);
            rtbLog.TabIndex = 1;
            rtbLog.Text = "";
            // 
            // groupConfig
            // 
            groupConfig.Dock = DockStyle.Top;
            groupConfig.Location = new Point(12, 12);
            groupConfig.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupConfig.Panel.Controls.Add(dtpDateFin);
            groupConfig.Panel.Controls.Add(chkDateFin);
            groupConfig.Panel.Controls.Add(dtpDateDebut);
            groupConfig.Panel.Controls.Add(chkDateDebut);
            groupConfig.Panel.Controls.Add(btnExportGantt);
            groupConfig.Panel.Controls.Add(btnExportPlanningExcel);
            groupConfig.Panel.Controls.Add(btnLaunch);
            groupConfig.Panel.Controls.Add(cmbCalculMax);
            groupConfig.Panel.Controls.Add(labelCalculMax);
            groupConfig.Panel.Controls.Add(cmbTypeDeSortie);
            groupConfig.Panel.Controls.Add(label8);
            groupConfig.Panel.Controls.Add(numCoutIndirect);
            groupConfig.Panel.Controls.Add(label11);
            groupConfig.Panel.Controls.Add(numPenaliteChangement);
            groupConfig.Panel.Controls.Add(label10);
            groupConfig.Panel.Controls.Add(numHeuresTravail);
            groupConfig.Panel.Controls.Add(label9);
            groupConfig.Panel.Controls.Add(numDureeOuverture);
            groupConfig.Panel.Controls.Add(label7);
            groupConfig.Panel.Controls.Add(numHeureDebut);
            groupConfig.Panel.Controls.Add(label6);
            groupConfig.Panel.Controls.Add(chkListJoursOuvres);
            groupConfig.Panel.Controls.Add(label5);
            groupConfig.Size = new Size(979, 217);
            groupConfig.TabIndex = 0;
            groupConfig.Values.Heading = "Configuration et Planification";
            // 
            // dtpDateFin
            // 
            dtpDateFin.Location = new Point(607, 112);
            dtpDateFin.Margin = new Padding(4, 3, 4, 3);
            dtpDateFin.Name = "dtpDateFin";
            dtpDateFin.Size = new Size(233, 21);
            dtpDateFin.TabIndex = 22;
            // 
            // chkDateFin
            // 
            chkDateFin.Location = new Point(531, 112);
            chkDateFin.Margin = new Padding(4, 3, 4, 3);
            chkDateFin.Name = "chkDateFin";
            chkDateFin.Size = new Size(75, 20);
            chkDateFin.TabIndex = 21;
            chkDateFin.Values.Text = "Date Fin :";
            // 
            // dtpDateDebut
            // 
            dtpDateDebut.Location = new Point(262, 111);
            dtpDateDebut.Margin = new Padding(4, 3, 4, 3);
            dtpDateDebut.Name = "dtpDateDebut";
            dtpDateDebut.Size = new Size(233, 21);
            dtpDateDebut.TabIndex = 20;
            // 
            // chkDateDebut
            // 
            chkDateDebut.Location = new Point(169, 112);
            chkDateDebut.Margin = new Padding(4, 3, 4, 3);
            chkDateDebut.Name = "chkDateDebut";
            chkDateDebut.Size = new Size(92, 20);
            chkDateDebut.TabIndex = 19;
            chkDateDebut.Values.Text = "Date Début :";
            // 
            // btnExportGantt
            // 
            btnExportGantt.Enabled = false;
            btnExportGantt.Location = new Point(789, 153);
            btnExportGantt.Margin = new Padding(4, 3, 4, 3);
            btnExportGantt.Name = "btnExportGantt";
            btnExportGantt.Size = new Size(175, 29);
            btnExportGantt.TabIndex = 18;
            btnExportGantt.Values.DropDownArrowColor = Color.Empty;
            btnExportGantt.Values.Text = "Export GanttProject";
            btnExportGantt.Click += btnExportGantt_Click;
            // 
            // btnExportPlanningExcel
            // 
            btnExportPlanningExcel.Enabled = false;
            btnExportPlanningExcel.Location = new Point(607, 153);
            btnExportPlanningExcel.Margin = new Padding(4, 3, 4, 3);
            btnExportPlanningExcel.Name = "btnExportPlanningExcel";
            btnExportPlanningExcel.Size = new Size(175, 29);
            btnExportPlanningExcel.TabIndex = 17;
            btnExportPlanningExcel.Values.DropDownArrowColor = Color.Empty;
            btnExportPlanningExcel.Values.Text = "Export Planning";
            btnExportPlanningExcel.Click += btnExportPlanningExcel_Click;
            // 
            // btnLaunch
            // 
            btnLaunch.Location = new Point(191, 148);
            btnLaunch.Margin = new Padding(4, 3, 4, 3);
            btnLaunch.Name = "btnLaunch";
            btnLaunch.Size = new Size(383, 40);
            btnLaunch.TabIndex = 16;
            btnLaunch.Values.DropDownArrowColor = Color.Empty;
            btnLaunch.Values.Text = "LANCER LA PLANIFICATION";
            btnLaunch.Click += btnLaunch_Click;
            // 
            // cmbCalculMax
            // 
            cmbCalculMax.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCalculMax.DropDownWidth = 70;
            cmbCalculMax.IntegralHeight = false;
            cmbCalculMax.Location = new Point(374, 78);
            cmbCalculMax.Margin = new Padding(4, 3, 4, 3);
            cmbCalculMax.Name = "cmbCalculMax";
            cmbCalculMax.Size = new Size(82, 22);
            cmbCalculMax.TabIndex = 15;
            // 
            // labelCalculMax
            // 
            labelCalculMax.Location = new Point(191, 78);
            labelCalculMax.Margin = new Padding(4, 3, 4, 3);
            labelCalculMax.Name = "labelCalculMax";
            labelCalculMax.Size = new Size(135, 20);
            labelCalculMax.TabIndex = 14;
            labelCalculMax.Values.Text = "Calcul du solveur (min)";
            // 
            // cmbTypeDeSortie
            // 
            cmbTypeDeSortie.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTypeDeSortie.DropDownWidth = 150;
            cmbTypeDeSortie.IntegralHeight = false;
            cmbTypeDeSortie.Location = new Point(607, 78);
            cmbTypeDeSortie.Margin = new Padding(4, 3, 4, 3);
            cmbTypeDeSortie.Name = "cmbTypeDeSortie";
            cmbTypeDeSortie.Size = new Size(175, 22);
            cmbTypeDeSortie.TabIndex = 13;
            // 
            // label8
            // 
            label8.Location = new Point(489, 78);
            label8.Margin = new Padding(4, 3, 4, 3);
            label8.Name = "label8";
            label8.Size = new Size(95, 20);
            label8.TabIndex = 12;
            label8.Values.Text = "Type de Sortie :";
            // 
            // numCoutIndirect
            // 
            numCoutIndirect.AllowDecimals = true;
            numCoutIndirect.DecimalPlaces = 2;
            numCoutIndirect.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numCoutIndirect.Location = new Point(813, 46);
            numCoutIndirect.Margin = new Padding(4, 3, 4, 3);
            numCoutIndirect.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numCoutIndirect.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numCoutIndirect.Name = "numCoutIndirect";
            numCoutIndirect.Size = new Size(140, 22);
            numCoutIndirect.TabIndex = 11;
            numCoutIndirect.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // label11
            // 
            label11.Location = new Point(678, 46);
            label11.Margin = new Padding(4, 3, 4, 3);
            label11.Name = "label11";
            label11.Size = new Size(104, 20);
            label11.TabIndex = 10;
            label11.Values.Text = "Coût indirect (€) :";
            // 
            // numPenaliteChangement
            // 
            numPenaliteChangement.AllowDecimals = true;
            numPenaliteChangement.DecimalPlaces = 1;
            numPenaliteChangement.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numPenaliteChangement.Location = new Point(448, 46);
            numPenaliteChangement.Margin = new Padding(4, 3, 4, 3);
            numPenaliteChangement.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numPenaliteChangement.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numPenaliteChangement.Name = "numPenaliteChangement";
            numPenaliteChangement.Size = new Size(82, 22);
            numPenaliteChangement.TabIndex = 9;
            numPenaliteChangement.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // label10
            // 
            label10.Location = new Point(257, 46);
            label10.Margin = new Padding(4, 3, 4, 3);
            label10.Name = "label10";
            label10.Size = new Size(152, 20);
            label10.TabIndex = 8;
            label10.Values.Text = "Pénalité changement (%) :";
            // 
            // numHeuresTravail
            // 
            numHeuresTravail.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numHeuresTravail.Location = new Point(872, 14);
            numHeuresTravail.Margin = new Padding(4, 3, 4, 3);
            numHeuresTravail.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numHeuresTravail.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHeuresTravail.Name = "numHeuresTravail";
            numHeuresTravail.Size = new Size(82, 22);
            numHeuresTravail.TabIndex = 7;
            numHeuresTravail.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // label9
            // 
            label9.Location = new Point(727, 14);
            label9.Margin = new Padding(4, 3, 4, 3);
            label9.Name = "label9";
            label9.Size = new Size(121, 20);
            label9.TabIndex = 6;
            label9.Values.Text = "Durée de travail (h) :";
            // 
            // numDureeOuverture
            // 
            numDureeOuverture.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numDureeOuverture.Location = new Point(622, 14);
            numDureeOuverture.Margin = new Padding(4, 3, 4, 3);
            numDureeOuverture.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numDureeOuverture.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numDureeOuverture.Name = "numDureeOuverture";
            numDureeOuverture.Size = new Size(82, 22);
            numDureeOuverture.TabIndex = 5;
            numDureeOuverture.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // label7
            // 
            label7.Location = new Point(475, 14);
            label7.Margin = new Padding(4, 3, 4, 3);
            label7.Name = "label7";
            label7.Size = new Size(116, 20);
            label7.TabIndex = 4;
            label7.Values.Text = "Durée d'ouverture :";
            // 
            // numHeureDebut
            // 
            numHeureDebut.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numHeureDebut.Location = new Point(374, 14);
            numHeureDebut.Margin = new Padding(4, 3, 4, 3);
            numHeureDebut.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            numHeureDebut.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHeureDebut.Name = "numHeureDebut";
            numHeureDebut.Size = new Size(82, 22);
            numHeureDebut.TabIndex = 3;
            numHeureDebut.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // label6
            // 
            label6.Location = new Point(191, 14);
            label6.Margin = new Padding(4, 3, 4, 3);
            label6.Name = "label6";
            label6.Size = new Size(153, 20);
            label6.TabIndex = 2;
            label6.Values.Text = "Heure ouverture chantier :";
            // 
            // chkListJoursOuvres
            // 
            chkListJoursOuvres.Location = new Point(15, 36);
            chkListJoursOuvres.Margin = new Padding(4, 3, 4, 3);
            chkListJoursOuvres.Name = "chkListJoursOuvres";
            chkListJoursOuvres.Size = new Size(140, 111);
            chkListJoursOuvres.TabIndex = 1;
            // 
            // label5
            // 
            label5.Location = new Point(15, 9);
            label5.Margin = new Padding(4, 3, 4, 3);
            label5.Name = "label5";
            label5.Size = new Size(78, 20);
            label5.TabIndex = 0;
            label5.Values.Text = "Jours ouvrés";
            // 
            // PlanificatorView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonPanelMain);
            Margin = new Padding(4, 3, 4, 3);
            Name = "PlanificatorView";
            Size = new Size(1003, 692);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).EndInit();
            kryptonPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupConfig.Panel).EndInit();
            groupConfig.Panel.ResumeLayout(false);
            groupConfig.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupConfig).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCalculMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbTypeDeSortie).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private KryptonDateTimePicker dtpDateFin;
        private KryptonCheckBox chkDateFin;
        private KryptonDateTimePicker dtpDateDebut;
        private KryptonCheckBox chkDateDebut;
        private KryptonPanel kryptonPanelMain;
        private KryptonGroupBox groupConfig;
        private KryptonRichTextBox rtbLog;
        private KryptonLabel label5;
        private KryptonCheckedListBox chkListJoursOuvres;
        private KryptonNumericUpDown numHeureDebut;
        private KryptonLabel label6;
        private KryptonNumericUpDown numDureeOuverture;
        private KryptonLabel label7;
        private KryptonNumericUpDown numHeuresTravail;
        private KryptonLabel label9;
        private KryptonNumericUpDown numPenaliteChangement;
        private KryptonLabel label10;
        private KryptonNumericUpDown numCoutIndirect;
        private KryptonLabel label11;
        private KryptonComboBox cmbTypeDeSortie;
        private KryptonLabel label8;
        private KryptonComboBox cmbCalculMax;
        private KryptonLabel labelCalculMax;
        private KryptonButton btnLaunch;
        private KryptonButton btnExportGantt;
        private KryptonButton btnExportPlanningExcel;
    }
}