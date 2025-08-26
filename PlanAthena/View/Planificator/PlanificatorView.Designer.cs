using Krypton.Navigator;
using Krypton.Docking;
using Krypton.Workspace;
using Krypton.Toolkit;



namespace PlanAthena.View.Planificator
{
    partial class PlanificatorView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                // Disposer le timer du solveur
                if (_solverTimer != null)
                {
                    _solverTimer.Stop();
                    _solverTimer.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            kryptonPanelMain = new KryptonPanel();
            SolverProgressBar = new KryptonProgressBar();
            navigatorResultats = new KryptonNavigator();
            tabPageSynthese = new KryptonPage();
            kryptonHeaderGroup4 = new KryptonHeaderGroup();
            lblJoursHommeValue = new KryptonLabel();
            kryptonHeaderGroup3 = new KryptonHeaderGroup();
            lblDureeValue = new KryptonLabel();
            kryptonHeaderGroup2 = new KryptonHeaderGroup();
            lblCoutIndirectValue = new KryptonLabel();
            lblCoutRhValue = new KryptonLabel();
            kryptonLabel4 = new KryptonLabel();
            kryptonLabel3 = new KryptonLabel();
            kryptonHeaderGroup1 = new KryptonHeaderGroup();
            lblCoutTotalValue = new KryptonLabel();
            tabPageRessources = new KryptonPage();
            kryptonSplitContainer1 = new KryptonSplitContainer();
            dgvAnalyseOuvriers = new KryptonDataGridView();
            colOuvrier = new DataGridViewTextBoxColumn();
            colMetierPrincipal = new DataGridViewTextBoxColumn();
            colTauxOccupation = new DataGridViewTextBoxColumn();
            colTauxFragmentation = new DataGridViewTextBoxColumn();
            colHeures = new DataGridViewTextBoxColumn();
            colJours = new DataGridViewTextBoxColumn();
            chartChargeJournaliere = new System.Windows.Forms.DataVisualization.Charting.Chart();
            tabPageJournal = new KryptonPage();
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
            progressBar = new KryptonProgressBar();
            labelSeuilDecoupage = new KryptonLabel();
            numSeuilDecoupage = new KryptonNumericUpDown();
            planningTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)navigatorResultats).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tabPageSynthese).BeginInit();
            tabPageSynthese.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup4.Panel).BeginInit();
            kryptonHeaderGroup4.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup3.Panel).BeginInit();
            kryptonHeaderGroup3.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup2.Panel).BeginInit();
            kryptonHeaderGroup2.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup1.Panel).BeginInit();
            kryptonHeaderGroup1.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tabPageRessources).BeginInit();
            tabPageRessources.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).BeginInit();
            (kryptonSplitContainer1.Panel1).BeginInit();
            kryptonSplitContainer1.Panel1.SuspendLayout();
            (kryptonSplitContainer1.Panel2).BeginInit();
            kryptonSplitContainer1.Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAnalyseOuvriers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chartChargeJournaliere).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tabPageJournal).BeginInit();
            tabPageJournal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupConfig).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupConfig.Panel).BeginInit();
            groupConfig.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbCalculMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbTypeDeSortie).BeginInit();
            SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            kryptonPanelMain.Controls.Add(SolverProgressBar);
            kryptonPanelMain.Controls.Add(navigatorResultats);
            kryptonPanelMain.Controls.Add(groupConfig);
            kryptonPanelMain.Dock = DockStyle.Fill;
            kryptonPanelMain.Location = new Point(0, 0);
            kryptonPanelMain.Margin = new Padding(4, 3, 4, 3);
            kryptonPanelMain.Name = "kryptonPanelMain";
            kryptonPanelMain.Padding = new Padding(12);
            kryptonPanelMain.Size = new Size(1003, 692);
            kryptonPanelMain.TabIndex = 0;
            // 
            // SolverProgressBar
            // 
            SolverProgressBar.Location = new Point(205, 205);
            SolverProgressBar.Name = "SolverProgressBar";
            SolverProgressBar.Size = new Size(773, 21);
            SolverProgressBar.StateCommon.Back.Color1 = Color.Green;
            SolverProgressBar.StateDisabled.Back.ColorStyle = PaletteColorStyle.OneNote;
            SolverProgressBar.StateNormal.Back.ColorStyle = PaletteColorStyle.OneNote;
            SolverProgressBar.TabIndex = 3;
            SolverProgressBar.Text = "";
            SolverProgressBar.Values.Text = "Calcul en cours";
            SolverProgressBar.Visible = false;
            // 
            // navigatorResultats
            // 
            navigatorResultats.Button.ButtonDisplayLogic = ButtonDisplayLogic.Context;
            navigatorResultats.Button.CloseButtonAction = CloseButtonAction.None;
            navigatorResultats.Button.CloseButtonDisplay = ButtonDisplay.Hide;
            navigatorResultats.Button.ContextButtonAction = ContextButtonAction.SelectPage;
            navigatorResultats.Button.ContextButtonDisplay = ButtonDisplay.Logic;
            navigatorResultats.Button.ContextMenuMapImage = MapKryptonPageImage.Small;
            navigatorResultats.Button.ContextMenuMapText = MapKryptonPageText.TextTitle;
            navigatorResultats.Button.NextButtonAction = DirectionButtonAction.ModeAppropriateAction;
            navigatorResultats.Button.NextButtonDisplay = ButtonDisplay.Logic;
            navigatorResultats.Button.PreviousButtonAction = DirectionButtonAction.ModeAppropriateAction;
            navigatorResultats.Button.PreviousButtonDisplay = ButtonDisplay.Logic;
            navigatorResultats.ControlKryptonFormFeatures = false;
            navigatorResultats.Dock = DockStyle.Fill;
            navigatorResultats.Location = new Point(12, 229);
            navigatorResultats.NavigatorMode = NavigatorMode.BarTabGroup;
            navigatorResultats.Owner = null;
            navigatorResultats.PageBackStyle = PaletteBackStyle.PanelClient;
            navigatorResultats.Pages.AddRange(new KryptonPage[] { tabPageSynthese, tabPageRessources, tabPageJournal });
            navigatorResultats.SelectedIndex = 0;
            navigatorResultats.Size = new Size(979, 451);
            navigatorResultats.TabIndex = 2;
            navigatorResultats.Text = "navigatorResultats";
            // 
            // tabPageSynthese
            // 
            tabPageSynthese.AutoHiddenSlideSize = new Size(200, 200);
            tabPageSynthese.Controls.Add(kryptonHeaderGroup4);
            tabPageSynthese.Controls.Add(kryptonHeaderGroup3);
            tabPageSynthese.Controls.Add(kryptonHeaderGroup2);
            tabPageSynthese.Controls.Add(kryptonHeaderGroup1);
            tabPageSynthese.Flags = 65534;
            tabPageSynthese.LastVisibleSet = true;
            tabPageSynthese.MinimumSize = new Size(50, 50);
            tabPageSynthese.Name = "tabPageSynthese";
            tabPageSynthese.Padding = new Padding(10);
            tabPageSynthese.Size = new Size(977, 424);
            tabPageSynthese.Text = "Synthèse";
            tabPageSynthese.ToolTipTitle = "Page ToolTip";
            tabPageSynthese.UniqueName = "1D744655612F4462B15591322051E959";
            // 
            // kryptonHeaderGroup4
            // 
            kryptonHeaderGroup4.Location = new Point(623, 13);
            // 
            // 
            // 
            kryptonHeaderGroup4.Panel.Controls.Add(lblJoursHommeValue);
            kryptonHeaderGroup4.Size = new Size(195, 120);
            kryptonHeaderGroup4.TabIndex = 3;
            kryptonHeaderGroup4.ValuesPrimary.Heading = "Effort Total";
            kryptonHeaderGroup4.ValuesPrimary.Image = null;
            kryptonHeaderGroup4.ValuesSecondary.Heading = "(estimation jours-homme)";
            // 
            // lblJoursHommeValue
            // 
            lblJoursHommeValue.Dock = DockStyle.Fill;
            lblJoursHommeValue.Location = new Point(0, 0);
            lblJoursHommeValue.Name = "lblJoursHommeValue";
            lblJoursHommeValue.Size = new Size(193, 67);
            lblJoursHommeValue.StateCommon.ShortText.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold);
            lblJoursHommeValue.StateCommon.ShortText.TextH = PaletteRelativeAlign.Center;
            lblJoursHommeValue.StateCommon.ShortText.TextV = PaletteRelativeAlign.Center;
            lblJoursHommeValue.TabIndex = 1;
            lblJoursHommeValue.Values.Text = "N/A";
            // 
            // kryptonHeaderGroup3
            // 
            kryptonHeaderGroup3.Location = new Point(422, 13);
            // 
            // 
            // 
            kryptonHeaderGroup3.Panel.Controls.Add(lblDureeValue);
            kryptonHeaderGroup3.Size = new Size(195, 120);
            kryptonHeaderGroup3.TabIndex = 2;
            kryptonHeaderGroup3.ValuesPrimary.Heading = "Durée Chantier";
            kryptonHeaderGroup3.ValuesPrimary.Image = null;
            kryptonHeaderGroup3.ValuesSecondary.Heading = "(jours calendaires)";
            // 
            // lblDureeValue
            // 
            lblDureeValue.Dock = DockStyle.Fill;
            lblDureeValue.Location = new Point(0, 0);
            lblDureeValue.Name = "lblDureeValue";
            lblDureeValue.Size = new Size(193, 67);
            lblDureeValue.StateCommon.ShortText.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold);
            lblDureeValue.StateCommon.ShortText.TextH = PaletteRelativeAlign.Center;
            lblDureeValue.StateCommon.ShortText.TextV = PaletteRelativeAlign.Center;
            lblDureeValue.TabIndex = 1;
            lblDureeValue.Values.Text = "N/A";
            // 
            // kryptonHeaderGroup2
            // 
            kryptonHeaderGroup2.Location = new Point(221, 13);
            // 
            // 
            // 
            kryptonHeaderGroup2.Panel.Controls.Add(lblCoutIndirectValue);
            kryptonHeaderGroup2.Panel.Controls.Add(lblCoutRhValue);
            kryptonHeaderGroup2.Panel.Controls.Add(kryptonLabel4);
            kryptonHeaderGroup2.Panel.Controls.Add(kryptonLabel3);
            kryptonHeaderGroup2.Size = new Size(195, 120);
            kryptonHeaderGroup2.TabIndex = 1;
            kryptonHeaderGroup2.ValuesPrimary.Heading = "Détail Coûts";
            kryptonHeaderGroup2.ValuesPrimary.Image = null;
            kryptonHeaderGroup2.ValuesSecondary.Heading = "(Main d'oeuvre & Indirect)";
            // 
            // lblCoutIndirectValue
            // 
            lblCoutIndirectValue.Location = new Point(70, 35);
            lblCoutIndirectValue.Name = "lblCoutIndirectValue";
            lblCoutIndirectValue.Size = new Size(33, 20);
            lblCoutIndirectValue.StateCommon.ShortText.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblCoutIndirectValue.TabIndex = 3;
            lblCoutIndirectValue.Values.Text = "N/A";
            // 
            // lblCoutRhValue
            // 
            lblCoutRhValue.Location = new Point(70, 9);
            lblCoutRhValue.Name = "lblCoutRhValue";
            lblCoutRhValue.Size = new Size(33, 20);
            lblCoutRhValue.StateCommon.ShortText.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblCoutRhValue.TabIndex = 2;
            lblCoutRhValue.Values.Text = "N/A";
            // 
            // kryptonLabel4
            // 
            kryptonLabel4.Location = new Point(4, 35);
            kryptonLabel4.Name = "kryptonLabel4";
            kryptonLabel4.Size = new Size(58, 20);
            kryptonLabel4.TabIndex = 1;
            kryptonLabel4.Values.Text = "Indirect :";
            // 
            // kryptonLabel3
            // 
            kryptonLabel3.Location = new Point(4, 9);
            kryptonLabel3.Name = "kryptonLabel3";
            kryptonLabel3.Size = new Size(33, 20);
            kryptonLabel3.TabIndex = 0;
            kryptonLabel3.Values.Text = "RH :";
            // 
            // kryptonHeaderGroup1
            // 
            kryptonHeaderGroup1.Location = new Point(20, 13);
            // 
            // 
            // 
            kryptonHeaderGroup1.Panel.Controls.Add(lblCoutTotalValue);
            kryptonHeaderGroup1.Size = new Size(195, 120);
            kryptonHeaderGroup1.TabIndex = 0;
            kryptonHeaderGroup1.ValuesPrimary.Heading = "Coût Total Estimé";
            kryptonHeaderGroup1.ValuesPrimary.Image = null;
            kryptonHeaderGroup1.ValuesSecondary.Heading = "";
            // 
            // lblCoutTotalValue
            // 
            lblCoutTotalValue.Dock = DockStyle.Fill;
            lblCoutTotalValue.Location = new Point(0, 0);
            lblCoutTotalValue.Name = "lblCoutTotalValue";
            lblCoutTotalValue.Size = new Size(193, 85);
            lblCoutTotalValue.StateCommon.ShortText.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblCoutTotalValue.StateCommon.ShortText.TextH = PaletteRelativeAlign.Center;
            lblCoutTotalValue.StateCommon.ShortText.TextV = PaletteRelativeAlign.Center;
            lblCoutTotalValue.TabIndex = 0;
            lblCoutTotalValue.Values.Text = "N/A";
            // 
            // tabPageRessources
            // 
            tabPageRessources.AutoHiddenSlideSize = new Size(200, 200);
            tabPageRessources.Controls.Add(kryptonSplitContainer1);
            tabPageRessources.Flags = 65534;
            tabPageRessources.LastVisibleSet = true;
            tabPageRessources.MinimumSize = new Size(50, 50);
            tabPageRessources.Name = "tabPageRessources";
            tabPageRessources.Size = new Size(977, 424);
            tabPageRessources.Text = "Analyse des Ressources";
            tabPageRessources.ToolTipTitle = "Page ToolTip";
            tabPageRessources.UniqueName = "B8B61A42A5C945653A2260667B7808E5";
            // 
            // kryptonSplitContainer1
            // 
            kryptonSplitContainer1.Dock = DockStyle.Fill;
            kryptonSplitContainer1.Location = new Point(0, 0);
            kryptonSplitContainer1.Orientation = Orientation.Horizontal;
            // 
            // 
            // 
            kryptonSplitContainer1.Panel1.Controls.Add(dgvAnalyseOuvriers);
            // 
            // 
            // 
            kryptonSplitContainer1.Panel2.Controls.Add(chartChargeJournaliere);
            kryptonSplitContainer1.Size = new Size(977, 424);
            kryptonSplitContainer1.SplitterDistance = 200;
            kryptonSplitContainer1.TabIndex = 0;
            // 
            // dgvAnalyseOuvriers
            // 
            dgvAnalyseOuvriers.AllowUserToAddRows = false;
            dgvAnalyseOuvriers.AllowUserToDeleteRows = false;
            dgvAnalyseOuvriers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAnalyseOuvriers.BorderStyle = BorderStyle.None;
            dgvAnalyseOuvriers.ColumnHeadersHeight = 28;
            dgvAnalyseOuvriers.Columns.AddRange(new DataGridViewColumn[] { colOuvrier, colMetierPrincipal, colTauxOccupation, colTauxFragmentation, colHeures, colJours });
            dgvAnalyseOuvriers.Dock = DockStyle.Fill;
            dgvAnalyseOuvriers.Location = new Point(0, 0);
            dgvAnalyseOuvriers.Name = "dgvAnalyseOuvriers";
            dgvAnalyseOuvriers.ReadOnly = true;
            dgvAnalyseOuvriers.RowHeadersVisible = false;
            dgvAnalyseOuvriers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAnalyseOuvriers.Size = new Size(977, 200);
            dgvAnalyseOuvriers.TabIndex = 0;
            // 
            // colOuvrier
            // 
            colOuvrier.DataPropertyName = "NomComplet";
            colOuvrier.FillWeight = 150F;
            colOuvrier.HeaderText = "Ouvrier";
            colOuvrier.Name = "colOuvrier";
            colOuvrier.ReadOnly = true;
            // 
            // colMetierPrincipal
            // 
            colMetierPrincipal.DataPropertyName = "MetierPrincipal";
            colMetierPrincipal.HeaderText = "Métier Principal";
            colMetierPrincipal.Name = "colMetierPrincipal";
            colMetierPrincipal.ReadOnly = true;
            // 
            // colTauxOccupation
            // 
            colTauxOccupation.DataPropertyName = "TauxOccupation";
            colTauxOccupation.HeaderText = "Taux Occupation (%)";
            colTauxOccupation.Name = "colTauxOccupation";
            colTauxOccupation.ReadOnly = true;
            // 
            // colTauxFragmentation
            // 
            colTauxFragmentation.DataPropertyName = "TauxFragmentation";
            colTauxFragmentation.HeaderText = "Taux Fragmentation (%)";
            colTauxFragmentation.Name = "colTauxFragmentation";
            colTauxFragmentation.ReadOnly = true;
            // 
            // colHeures
            // 
            colHeures.DataPropertyName = "HeuresTravaillees";
            colHeures.HeaderText = "Heures Trav.";
            colHeures.Name = "colHeures";
            colHeures.ReadOnly = true;
            // 
            // colJours
            // 
            colJours.DataPropertyName = "JoursTravaillesUniques";
            colJours.HeaderText = "Jours Trav.";
            colJours.Name = "colJours";
            colJours.ReadOnly = true;
            // 
            // chartChargeJournaliere
            // 
            chartArea1.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea1.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea1.Name = "ChartArea1";
            chartChargeJournaliere.ChartAreas.Add(chartArea1);
            chartChargeJournaliere.Dock = DockStyle.Fill;
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            chartChargeJournaliere.Legends.Add(legend1);
            chartChargeJournaliere.Location = new Point(0, 0);
            chartChargeJournaliere.Name = "chartChargeJournaliere";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chartChargeJournaliere.Series.Add(series1);
            chartChargeJournaliere.Size = new Size(977, 219);
            chartChargeJournaliere.TabIndex = 0;
            chartChargeJournaliere.Text = "chart1";
            title1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            title1.Name = "Title1";
            title1.Text = "Charge Journalière (Nombre d'Ouvriers)";
            chartChargeJournaliere.Titles.Add(title1);
            // 
            // tabPageJournal
            // 
            tabPageJournal.AutoHiddenSlideSize = new Size(200, 200);
            tabPageJournal.Controls.Add(rtbLog);
            tabPageJournal.Flags = 65534;
            tabPageJournal.LastVisibleSet = true;
            tabPageJournal.MinimumSize = new Size(50, 50);
            tabPageJournal.Name = "tabPageJournal";
            tabPageJournal.Size = new Size(977, 424);
            tabPageJournal.Text = "Journal (Log)";
            tabPageJournal.ToolTipTitle = "Page ToolTip";
            tabPageJournal.UniqueName = "A7401349E4F54728C91341F207D6D32E";
            // 
            // rtbLog
            // 
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Location = new Point(0, 0);
            rtbLog.Margin = new Padding(4, 3, 4, 3);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(977, 424);
            rtbLog.TabIndex = 2;
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
            groupConfig.Panel.Controls.Add(progressBar);
            groupConfig.Panel.Controls.Add(labelSeuilDecoupage);
            groupConfig.Panel.Controls.Add(numSeuilDecoupage);
            groupConfig.Size = new Size(979, 217);
            groupConfig.TabIndex = 0;
            groupConfig.Values.Heading = "Configuration et Planification";
            // 
            // dtpDateFin
            // 
            dtpDateFin.Location = new Point(719, 43);
            dtpDateFin.Margin = new Padding(4, 3, 4, 3);
            dtpDateFin.Name = "dtpDateFin";
            dtpDateFin.Size = new Size(233, 21);
            dtpDateFin.TabIndex = 22;
            // 
            // chkDateFin
            // 
            chkDateFin.Checked = true;
            chkDateFin.CheckState = CheckState.Checked;
            chkDateFin.Location = new Point(643, 43);
            chkDateFin.Margin = new Padding(4, 3, 4, 3);
            chkDateFin.Name = "chkDateFin";
            chkDateFin.Size = new Size(75, 20);
            chkDateFin.TabIndex = 21;
            chkDateFin.Values.Text = "Date Fin :";
            // 
            // dtpDateDebut
            // 
            dtpDateDebut.Location = new Point(374, 42);
            dtpDateDebut.Margin = new Padding(4, 3, 4, 3);
            dtpDateDebut.Name = "dtpDateDebut";
            dtpDateDebut.Size = new Size(210, 21);
            dtpDateDebut.TabIndex = 20;
            // 
            // chkDateDebut
            // 
            chkDateDebut.Checked = true;
            chkDateDebut.CheckState = CheckState.Checked;
            chkDateDebut.Location = new Point(281, 43);
            chkDateDebut.Margin = new Padding(4, 3, 4, 3);
            chkDateDebut.Name = "chkDateDebut";
            chkDateDebut.Size = new Size(92, 20);
            chkDateDebut.TabIndex = 19;
            chkDateDebut.Values.Text = "Date Début :";
            // 
            // btnExportGantt
            // 
            btnExportGantt.Enabled = false;
            btnExportGantt.Location = new Point(789, 133);
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
            btnExportPlanningExcel.Location = new Point(607, 133);
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
            btnLaunch.Location = new Point(191, 128);
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
            cmbCalculMax.Location = new Point(374, 100);
            cmbCalculMax.Margin = new Padding(4, 3, 4, 3);
            cmbCalculMax.Name = "cmbCalculMax";
            cmbCalculMax.Size = new Size(82, 22);
            cmbCalculMax.TabIndex = 15;
            // 
            // labelCalculMax
            // 
            labelCalculMax.Location = new Point(191, 100);
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
            cmbTypeDeSortie.Location = new Point(607, 100);
            cmbTypeDeSortie.Margin = new Padding(4, 3, 4, 3);
            cmbTypeDeSortie.Name = "cmbTypeDeSortie";
            cmbTypeDeSortie.Size = new Size(175, 22);
            cmbTypeDeSortie.TabIndex = 13;
            // 
            // label8
            // 
            label8.Location = new Point(489, 101);
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
            numCoutIndirect.Location = new Point(720, 70);
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
            label11.Location = new Point(607, 70);
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
            numPenaliteChangement.Location = new Point(375, 70);
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
            label10.Location = new Point(186, 70);
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
            // progressBar
            // 
            progressBar.Location = new Point(12, 195);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(979, 15);
            progressBar.TabIndex = 23;
            progressBar.Text = "Label";
            progressBar.Visible = false;
            // 
            // labelSeuilDecoupage
            // 
            labelSeuilDecoupage.Location = new Point(792, 100);
            labelSeuilDecoupage.Name = "labelSeuilDecoupage";
            labelSeuilDecoupage.Size = new Size(113, 20);
            labelSeuilDecoupage.TabIndex = 25;
            labelSeuilDecoupage.Values.Text = "Découper si > (jrs):";
            // 
            // numSeuilDecoupage
            // 
            numSeuilDecoupage.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numSeuilDecoupage.Location = new Point(910, 100);
            numSeuilDecoupage.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numSeuilDecoupage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSeuilDecoupage.Name = "numSeuilDecoupage";
            numSeuilDecoupage.Size = new Size(50, 22);
            numSeuilDecoupage.TabIndex = 26;
            numSeuilDecoupage.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // planningTimer
            // 
            planningTimer.Interval = 1000;
            planningTimer.Tick += planningTimer_Tick;
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
            ((System.ComponentModel.ISupportInitialize)navigatorResultats).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabPageSynthese).EndInit();
            tabPageSynthese.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup4.Panel).EndInit();
            kryptonHeaderGroup4.Panel.ResumeLayout(false);
            kryptonHeaderGroup4.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup4).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup3.Panel).EndInit();
            kryptonHeaderGroup3.Panel.ResumeLayout(false);
            kryptonHeaderGroup3.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup3).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup2.Panel).EndInit();
            kryptonHeaderGroup2.Panel.ResumeLayout(false);
            kryptonHeaderGroup2.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup2).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup1.Panel).EndInit();
            kryptonHeaderGroup1.Panel.ResumeLayout(false);
            kryptonHeaderGroup1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonHeaderGroup1).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabPageRessources).EndInit();
            tabPageRessources.ResumeLayout(false);
            (kryptonSplitContainer1.Panel1).EndInit();
            kryptonSplitContainer1.Panel1.ResumeLayout(false);
            (kryptonSplitContainer1.Panel2).EndInit();
            kryptonSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvAnalyseOuvriers).EndInit();
            ((System.ComponentModel.ISupportInitialize)chartChargeJournaliere).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabPageJournal).EndInit();
            tabPageJournal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupConfig.Panel).EndInit();
            groupConfig.Panel.ResumeLayout(false);
            groupConfig.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupConfig).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCalculMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbTypeDeSortie).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonGroupBox groupConfig;
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
        private KryptonDateTimePicker dtpDateFin;
        private KryptonCheckBox chkDateFin;
        private KryptonDateTimePicker dtpDateDebut;
        private KryptonCheckBox chkDateDebut;
        private Krypton.Navigator.KryptonNavigator navigatorResultats;
        private Krypton.Navigator.KryptonPage tabPageSynthese;
        private Krypton.Navigator.KryptonPage tabPageRessources;
        private Krypton.Navigator.KryptonPage tabPageJournal;
        private KryptonRichTextBox rtbLog;
        private KryptonHeaderGroup kryptonHeaderGroup1;
        private KryptonLabel lblCoutTotalValue;
        private KryptonHeaderGroup kryptonHeaderGroup2;
        private KryptonLabel lblCoutIndirectValue;
        private KryptonLabel lblCoutRhValue;
        private KryptonLabel kryptonLabel4;
        private KryptonLabel kryptonLabel3;
        private KryptonHeaderGroup kryptonHeaderGroup4;
        private KryptonLabel lblJoursHommeValue;
        private KryptonHeaderGroup kryptonHeaderGroup3;
        private KryptonLabel lblDureeValue;
        private KryptonSplitContainer kryptonSplitContainer1;
        private KryptonDataGridView dgvAnalyseOuvriers;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartChargeJournaliere;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOuvrier;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMetierPrincipal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTauxOccupation;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTauxFragmentation;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHeures;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJours;
        private Krypton.Toolkit.KryptonProgressBar progressBar;
        private System.Windows.Forms.Timer planningTimer;
        private KryptonProgressBar SolverProgressBar;
        private KryptonLabel labelSeuilDecoupage;
        private KryptonNumericUpDown numSeuilDecoupage;
    }
}