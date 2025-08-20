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
            this.kryptonPanelMain = new Krypton.Toolkit.KryptonPanel();
            this.rtbLog = new Krypton.Toolkit.KryptonRichTextBox();
            this.groupConfig = new Krypton.Toolkit.KryptonGroupBox();
            this.dtpDateFin = new Krypton.Toolkit.KryptonDateTimePicker();
            this.chkDateFin = new Krypton.Toolkit.KryptonCheckBox();
            this.dtpDateDebut = new Krypton.Toolkit.KryptonDateTimePicker();
            this.chkDateDebut = new Krypton.Toolkit.KryptonCheckBox();
            this.btnExportGantt = new Krypton.Toolkit.KryptonButton();
            this.btnExportPlanningExcel = new Krypton.Toolkit.KryptonButton();
            this.btnLaunch = new Krypton.Toolkit.KryptonButton();
            this.cmbCalculMax = new Krypton.Toolkit.KryptonComboBox();
            this.labelCalculMax = new Krypton.Toolkit.KryptonLabel();
            this.cmbTypeDeSortie = new Krypton.Toolkit.KryptonComboBox();
            this.label8 = new Krypton.Toolkit.KryptonLabel();
            this.numCoutIndirect = new Krypton.Toolkit.KryptonNumericUpDown();
            this.label11 = new Krypton.Toolkit.KryptonLabel();
            this.numPenaliteChangement = new Krypton.Toolkit.KryptonNumericUpDown();
            this.label10 = new Krypton.Toolkit.KryptonLabel();
            this.numHeuresTravail = new Krypton.Toolkit.KryptonNumericUpDown();
            this.label9 = new Krypton.Toolkit.KryptonLabel();
            this.numDureeOuverture = new Krypton.Toolkit.KryptonNumericUpDown();
            this.label7 = new Krypton.Toolkit.KryptonLabel();
            this.numHeureDebut = new Krypton.Toolkit.KryptonNumericUpDown();
            this.label6 = new Krypton.Toolkit.KryptonLabel();
            this.chkListJoursOuvres = new Krypton.Toolkit.KryptonCheckedListBox();
            this.label5 = new Krypton.Toolkit.KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanelMain)).BeginInit();
            this.kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupConfig)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupConfig.Panel)).BeginInit();
            this.groupConfig.Panel.SuspendLayout();
            this.groupConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCalculMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbTypeDeSortie)).BeginInit();
            this.SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            this.kryptonPanelMain.Controls.Add(this.rtbLog);
            this.kryptonPanelMain.Controls.Add(this.groupConfig);
            this.kryptonPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonPanelMain.Location = new System.Drawing.Point(0, 0);
            this.kryptonPanelMain.Name = "kryptonPanelMain";
            this.kryptonPanelMain.Padding = new System.Windows.Forms.Padding(10);
            this.kryptonPanelMain.Size = new System.Drawing.Size(860, 600);
            this.kryptonPanelMain.TabIndex = 0;
            // 
            // rtbLog
            // 
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Location = new System.Drawing.Point(10, 198);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(840, 392);
            this.rtbLog.TabIndex = 1;
            this.rtbLog.Text = "";
            // 
            // groupConfig
            // 
            this.groupConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupConfig.Location = new System.Drawing.Point(10, 10);
            this.groupConfig.Name = "groupConfig";
            this.groupConfig.Panel.Controls.Add(this.dtpDateFin);
            this.groupConfig.Panel.Controls.Add(this.chkDateFin);
            this.groupConfig.Panel.Controls.Add(this.dtpDateDebut);
            this.groupConfig.Panel.Controls.Add(this.chkDateDebut);
            this.groupConfig.Panel.Controls.Add(this.btnExportGantt);
            this.groupConfig.Panel.Controls.Add(this.btnExportPlanningExcel);
            this.groupConfig.Panel.Controls.Add(this.btnLaunch);
            this.groupConfig.Panel.Controls.Add(this.cmbCalculMax);
            this.groupConfig.Panel.Controls.Add(this.labelCalculMax);
            this.groupConfig.Panel.Controls.Add(this.cmbTypeDeSortie);
            this.groupConfig.Panel.Controls.Add(this.label8);
            this.groupConfig.Panel.Controls.Add(this.numCoutIndirect);
            this.groupConfig.Panel.Controls.Add(this.label11);
            this.groupConfig.Panel.Controls.Add(this.numPenaliteChangement);
            this.groupConfig.Panel.Controls.Add(this.label10);
            this.groupConfig.Panel.Controls.Add(this.numHeuresTravail);
            this.groupConfig.Panel.Controls.Add(this.label9);
            this.groupConfig.Panel.Controls.Add(this.numDureeOuverture);
            this.groupConfig.Panel.Controls.Add(this.label7);
            this.groupConfig.Panel.Controls.Add(this.numHeureDebut);
            this.groupConfig.Panel.Controls.Add(this.label6);
            this.groupConfig.Panel.Controls.Add(this.chkListJoursOuvres);
            this.groupConfig.Panel.Controls.Add(this.label5);
            this.groupConfig.Size = new System.Drawing.Size(840, 188);
            this.groupConfig.TabIndex = 0;
            this.groupConfig.Values.Heading = "Configuration et Planification";
            // 
            // dtpDateFin
            // 
            this.dtpDateFin.Location = new System.Drawing.Point(520, 97);
            this.dtpDateFin.Name = "dtpDateFin";
            this.dtpDateFin.Size = new System.Drawing.Size(200, 21);
            this.dtpDateFin.TabIndex = 22;
            // 
            // chkDateFin
            // 
            this.chkDateFin.Location = new System.Drawing.Point(419, 97);
            this.chkDateFin.Name = "chkDateFin";
            this.chkDateFin.Size = new System.Drawing.Size(73, 20);
            this.chkDateFin.TabIndex = 21;
            this.chkDateFin.Values.Text = "Date Fin :";
            // 
            // dtpDateDebut
            // 
            this.dtpDateDebut.Location = new System.Drawing.Point(244, 97);
            this.dtpDateDebut.Name = "dtpDateDebut";
            this.dtpDateDebut.Size = new System.Drawing.Size(200, 21);
            this.dtpDateDebut.TabIndex = 20;
            // 
            // chkDateDebut
            // 
            this.chkDateDebut.Location = new System.Drawing.Point(145, 97);
            this.chkDateDebut.Name = "chkDateDebut";
            this.chkDateDebut.Size = new System.Drawing.Size(93, 20);
            this.chkDateDebut.TabIndex = 19;
            this.chkDateDebut.Values.Text = "Date Début :";

            // 
            // btnExportGantt
            // 
            this.btnExportGantt.Enabled = false;
            this.btnExportGantt.Location = new System.Drawing.Point(676, 100);
            this.btnExportGantt.Name = "btnExportGantt";
            this.btnExportGantt.Size = new System.Drawing.Size(150, 25);
            this.btnExportGantt.TabIndex = 18;
            this.btnExportGantt.Values.Text = "Export GanttProject";
            this.btnExportGantt.Click += new System.EventHandler(this.btnExportGantt_Click);
            // 
            // btnExportPlanningExcel
            // 
            this.btnExportPlanningExcel.Enabled = false;
            this.btnExportPlanningExcel.Location = new System.Drawing.Point(520, 100);
            this.btnExportPlanningExcel.Name = "btnExportPlanningExcel";
            this.btnExportPlanningExcel.Size = new System.Drawing.Size(150, 25);
            this.btnExportPlanningExcel.TabIndex = 17;
            this.btnExportPlanningExcel.Values.Text = "Export Planning";
            this.btnExportPlanningExcel.Click += new System.EventHandler(this.btnExportPlanningExcel_Click);
            // 
            // btnLaunch
            // 
            this.btnLaunch.Location = new System.Drawing.Point(164, 95);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Size = new System.Drawing.Size(328, 35);
            this.btnLaunch.TabIndex = 16;
            this.btnLaunch.Values.Text = "LANCER LA PLANIFICATION";
            this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
            // 
            // cmbCalculMax
            // 
            this.cmbCalculMax.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbCalculMax.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCalculMax.DropDownWidth = 70;
            this.cmbCalculMax.IntegralHeight = false;
            this.cmbCalculMax.Location = new System.Drawing.Point(321, 68);
            this.cmbCalculMax.Name = "cmbCalculMax";
            this.cmbCalculMax.Size = new System.Drawing.Size(70, 21);
            this.cmbCalculMax.TabIndex = 15;
            // 
            // labelCalculMax
            // 
            this.labelCalculMax.Location = new System.Drawing.Point(164, 68);
            this.labelCalculMax.Name = "labelCalculMax";
            this.labelCalculMax.Size = new System.Drawing.Size(129, 20);
            this.labelCalculMax.TabIndex = 14;
            this.labelCalculMax.Values.Text = "Calcul du solveur (min)";
            // 
            // cmbTypeDeSortie
            // 
            this.cmbTypeDeSortie.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbTypeDeSortie.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTypeDeSortie.DropDownWidth = 150;
            this.cmbTypeDeSortie.IntegralHeight = false;
            this.cmbTypeDeSortie.Location = new System.Drawing.Point(520, 68);
            this.cmbTypeDeSortie.Name = "cmbTypeDeSortie";
            this.cmbTypeDeSortie.Size = new System.Drawing.Size(150, 21);
            this.cmbTypeDeSortie.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(419, 68);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 20);
            this.label8.TabIndex = 12;
            this.label8.Values.Text = "Type de Sortie :";
            // 
            // numCoutIndirect
            // 
            this.numCoutIndirect.DecimalPlaces = 2;
            this.numCoutIndirect.Location = new System.Drawing.Point(697, 40);
            this.numCoutIndirect.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numCoutIndirect.Name = "numCoutIndirect";
            this.numCoutIndirect.Size = new System.Drawing.Size(120, 22);
            this.numCoutIndirect.TabIndex = 11;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(581, 40);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(110, 20);
            this.label11.TabIndex = 10;
            this.label11.Values.Text = "Coût indirect (€) :";
            // 
            // numPenaliteChangement
            // 
            this.numPenaliteChangement.DecimalPlaces = 1;
            this.numPenaliteChangement.Location = new System.Drawing.Point(384, 40);
            this.numPenaliteChangement.Name = "numPenaliteChangement";
            this.numPenaliteChangement.Size = new System.Drawing.Size(70, 22);
            this.numPenaliteChangement.TabIndex = 9;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(220, 40);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(158, 20);
            this.label10.TabIndex = 8;
            this.label10.Values.Text = "Pénalité changement (%) :";
            // 
            // numHeuresTravail
            // 
            this.numHeuresTravail.Location = new System.Drawing.Point(747, 12);
            this.numHeuresTravail.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numHeuresTravail.Name = "numHeuresTravail";
            this.numHeuresTravail.Size = new System.Drawing.Size(70, 22);
            this.numHeuresTravail.TabIndex = 7;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(623, 12);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(118, 20);
            this.label9.TabIndex = 6;
            this.label9.Values.Text = "Durée de travail (h) :";
            // 
            // numDureeOuverture
            // 
            this.numDureeOuverture.Location = new System.Drawing.Point(533, 12);
            this.numDureeOuverture.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numDureeOuverture.Name = "numDureeOuverture";
            this.numDureeOuverture.Size = new System.Drawing.Size(70, 22);
            this.numDureeOuverture.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(407, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(120, 20);
            this.label7.TabIndex = 4;
            this.label7.Values.Text = "Durée d\'ouverture :";
            // 
            // numHeureDebut
            // 
            this.numHeureDebut.Location = new System.Drawing.Point(321, 12);
            this.numHeureDebut.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.numHeureDebut.Name = "numHeureDebut";
            this.numHeureDebut.Size = new System.Drawing.Size(70, 22);
            this.numHeureDebut.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(164, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(151, 20);
            this.label6.TabIndex = 2;
            this.label6.Values.Text = "Heure ouverture chantier :";
            // 
            // chkListJoursOuvres
            // 
            this.chkListJoursOuvres.Location = new System.Drawing.Point(13, 31);
            this.chkListJoursOuvres.Name = "chkListJoursOuvres";
            this.chkListJoursOuvres.Size = new System.Drawing.Size(120, 96);
            this.chkListJoursOuvres.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(13, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 20);
            this.label5.TabIndex = 0;
            this.label5.Values.Text = "Jours ouvrés";
            // 
            // PlanificatorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.kryptonPanelMain);
            this.Name = "PlanificatorView";
            this.Size = new System.Drawing.Size(860, 600);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanelMain)).EndInit();
            this.kryptonPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupConfig.Panel)).EndInit();
            this.groupConfig.Panel.ResumeLayout(false);
            this.groupConfig.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupConfig)).EndInit();
            this.groupConfig.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbCalculMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbTypeDeSortie)).EndInit();
            this.ResumeLayout(false);

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