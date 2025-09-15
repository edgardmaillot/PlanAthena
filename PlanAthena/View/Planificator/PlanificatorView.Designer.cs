// --- START OF FILE PlanificatorView.Designer.cs ---

//Version 0.5.3 - Correction complète et finale UI
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
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            kryptonPanelMain = new KryptonPanel();
            SolverProgressBar = new KryptonProgressBar();
            navigatorResultats = new KryptonNavigator();
            tabPageSynthese = new KryptonPage();
            kryptonPanelKpis = new KryptonPanel();
            khgSolverStatus = new KryptonHeaderGroup();
            lblStatutExplication = new KryptonLabel();
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
            khgNotification = new KryptonHeaderGroup();
            rtbNotification = new KryptonRichTextBox();
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
            btnExportGantt = new KryptonButton();
            btnExportPlanningExcel = new KryptonButton();
            groupOptimisation = new KryptonGroupBox();
            lblHelpTypeSortie = new KryptonLabel();
            lblHelpDecoupage = new KryptonLabel();
            labelCalculMax = new KryptonLabel();
            cmbCalculMax = new KryptonComboBox();
            label8 = new KryptonLabel();
            cmbTypeDeSortie = new KryptonComboBox();
            labelSeuilDecoupage = new KryptonLabel();
            numSeuilDecoupage = new KryptonNumericUpDown();
            groupFinancier = new KryptonGroupBox();
            lblHelpCoutIndirect = new KryptonLabel();
            lblHelpPenalite = new KryptonLabel();
            label11 = new KryptonLabel();
            numCoutIndirect = new KryptonNumericUpDown();
            label10 = new KryptonLabel();
            numPenaliteChangement = new KryptonNumericUpDown();
            btnLaunch = new KryptonButton();
            groupCalendrier = new KryptonGroupBox();
            label5 = new KryptonLabel();
            chkListJoursOuvres = new KryptonCheckedListBox();
            kryptonLabel1 = new KryptonLabel();
            dtpDateDebut = new KryptonDateTimePicker();
            kryptonLabel2 = new KryptonLabel();
            dtpDateFin = new KryptonDateTimePicker();
            label6 = new KryptonLabel();
            numHeureDebut = new KryptonNumericUpDown();
            label7 = new KryptonLabel();
            numDureeOuverture = new KryptonNumericUpDown();
            label9 = new KryptonLabel();
            numHeuresTravail = new KryptonNumericUpDown();
            planningTimer = new System.Windows.Forms.Timer(components);
            kryptonToolTip = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)navigatorResultats).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tabPageSynthese).BeginInit();
            tabPageSynthese.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelKpis).BeginInit();
            kryptonPanelKpis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)khgSolverStatus).BeginInit();
            ((System.ComponentModel.ISupportInitialize)khgSolverStatus.Panel).BeginInit();
            khgSolverStatus.Panel.SuspendLayout();
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
            ((System.ComponentModel.ISupportInitialize)khgNotification).BeginInit();
            ((System.ComponentModel.ISupportInitialize)khgNotification.Panel).BeginInit();
            khgNotification.Panel.SuspendLayout();
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
            ((System.ComponentModel.ISupportInitialize)groupOptimisation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupOptimisation.Panel).BeginInit();
            groupOptimisation.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbCalculMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbTypeDeSortie).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupFinancier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupFinancier.Panel).BeginInit();
            groupFinancier.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupCalendrier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupCalendrier.Panel).BeginInit();
            groupCalendrier.Panel.SuspendLayout();
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
            kryptonPanelMain.Size = new Size(1000, 800);
            kryptonPanelMain.TabIndex = 0;
            // 
            // SolverProgressBar
            // 
            SolverProgressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SolverProgressBar.Location = new Point(128, 241);
            SolverProgressBar.Name = "SolverProgressBar";
            SolverProgressBar.Size = new Size(770, 21);
            SolverProgressBar.StateCommon.Back.Color1 = Color.Green;
            SolverProgressBar.StateDisabled.Back.ColorStyle = PaletteColorStyle.OneNote;
            SolverProgressBar.StateNormal.Back.ColorStyle = PaletteColorStyle.OneNote;
            SolverProgressBar.TabIndex = 3;
            SolverProgressBar.Text = "Calcul en cours";
            SolverProgressBar.Values.Text = "Calcul en cours";
            SolverProgressBar.Visible = false;
            // 
            // navigatorResultats
            // 
            navigatorResultats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
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
            navigatorResultats.Location = new Point(12, 279);
            navigatorResultats.NavigatorMode = NavigatorMode.BarTabGroup;
            navigatorResultats.Owner = null;
            navigatorResultats.PageBackStyle = PaletteBackStyle.PanelClient;
            navigatorResultats.Pages.AddRange(new KryptonPage[] { tabPageSynthese, tabPageRessources, tabPageJournal });
            navigatorResultats.SelectedIndex = 0;
            navigatorResultats.Size = new Size(976, 559);
            navigatorResultats.TabIndex = 2;
            navigatorResultats.Text = "navigatorResultats";
            // 
            // tabPageSynthese
            // 
            tabPageSynthese.AutoHiddenSlideSize = new Size(200, 200);
            tabPageSynthese.Controls.Add(kryptonPanelKpis);
            tabPageSynthese.Controls.Add(khgNotification);
            tabPageSynthese.Flags = 65534;
            tabPageSynthese.LastVisibleSet = true;
            tabPageSynthese.MinimumSize = new Size(50, 50);
            tabPageSynthese.Name = "tabPageSynthese";
            tabPageSynthese.Padding = new Padding(10);
            tabPageSynthese.Size = new Size(974, 532);
            tabPageSynthese.Text = "Synthèse";
            tabPageSynthese.ToolTipTitle = "Page ToolTip";
            tabPageSynthese.UniqueName = "1D744655612F4462B15591322051E959";
            // 
            // kryptonPanelKpis
            // 
            kryptonPanelKpis.Controls.Add(khgSolverStatus);
            kryptonPanelKpis.Controls.Add(kryptonHeaderGroup4);
            kryptonPanelKpis.Controls.Add(kryptonHeaderGroup3);
            kryptonPanelKpis.Controls.Add(kryptonHeaderGroup2);
            kryptonPanelKpis.Controls.Add(kryptonHeaderGroup1);
            kryptonPanelKpis.Dock = DockStyle.Fill;
            kryptonPanelKpis.Location = new Point(10, 154);
            kryptonPanelKpis.Name = "kryptonPanelKpis";
            kryptonPanelKpis.Size = new Size(954, 368);
            kryptonPanelKpis.TabIndex = 6;
            // 
            // khgSolverStatus
            // 
            khgSolverStatus.Location = new Point(748, 34);
            // 
            // 
            // 
            khgSolverStatus.Panel.Controls.Add(lblStatutExplication);
            khgSolverStatus.Size = new Size(181, 120);
            khgSolverStatus.TabIndex = 9;
            khgSolverStatus.ValuesPrimary.Heading = "Qualité de la Solution";
            khgSolverStatus.ValuesPrimary.Image = null;
            khgSolverStatus.ValuesSecondary.Heading = "";
            // 
            // lblStatutExplication
            // 
            lblStatutExplication.Dock = DockStyle.Fill;
            lblStatutExplication.Location = new Point(0, 0);
            lblStatutExplication.Name = "lblStatutExplication";
            lblStatutExplication.Size = new Size(179, 85);
            lblStatutExplication.StateCommon.LongText.MultiLine = InheritBool.True;
            lblStatutExplication.StateCommon.LongText.Prefix = PaletteTextHotkeyPrefix.None;
            lblStatutExplication.StateCommon.ShortText.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblStatutExplication.StateCommon.ShortText.MultiLine = InheritBool.True;
            lblStatutExplication.StateCommon.ShortText.TextH = PaletteRelativeAlign.Center;
            lblStatutExplication.StateCommon.ShortText.TextV = PaletteRelativeAlign.Center;
            lblStatutExplication.TabIndex = 1;
            lblStatutExplication.Values.Text = "En attente de planification...";
            // 
            // kryptonHeaderGroup4
            // 
            kryptonHeaderGroup4.Location = new Point(561, 34);
            // 
            // 
            // 
            kryptonHeaderGroup4.Panel.Controls.Add(lblJoursHommeValue);
            kryptonHeaderGroup4.Size = new Size(181, 120);
            kryptonHeaderGroup4.TabIndex = 8;
            kryptonHeaderGroup4.ValuesPrimary.Heading = "Effort Total";
            kryptonHeaderGroup4.ValuesPrimary.Image = null;
            kryptonHeaderGroup4.ValuesSecondary.Heading = "(estimation jours-homme)";
            // 
            // lblJoursHommeValue
            // 
            lblJoursHommeValue.Dock = DockStyle.Fill;
            lblJoursHommeValue.Location = new Point(0, 0);
            lblJoursHommeValue.Name = "lblJoursHommeValue";
            lblJoursHommeValue.Size = new Size(179, 67);
            lblJoursHommeValue.StateCommon.ShortText.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold);
            lblJoursHommeValue.StateCommon.ShortText.TextH = PaletteRelativeAlign.Center;
            lblJoursHommeValue.StateCommon.ShortText.TextV = PaletteRelativeAlign.Center;
            lblJoursHommeValue.TabIndex = 1;
            lblJoursHommeValue.Values.Text = "N/A";
            // 
            // kryptonHeaderGroup3
            // 
            kryptonHeaderGroup3.Location = new Point(374, 34);
            // 
            // 
            // 
            kryptonHeaderGroup3.Panel.Controls.Add(lblDureeValue);
            kryptonHeaderGroup3.Size = new Size(181, 120);
            kryptonHeaderGroup3.TabIndex = 7;
            kryptonHeaderGroup3.ValuesPrimary.Heading = "Durée Chantier";
            kryptonHeaderGroup3.ValuesPrimary.Image = null;
            kryptonHeaderGroup3.ValuesSecondary.Heading = "(jours ouvrés)";
            // 
            // lblDureeValue
            // 
            lblDureeValue.Dock = DockStyle.Fill;
            lblDureeValue.Location = new Point(0, 0);
            lblDureeValue.Name = "lblDureeValue";
            lblDureeValue.Size = new Size(179, 67);
            lblDureeValue.StateCommon.ShortText.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold);
            lblDureeValue.StateCommon.ShortText.TextH = PaletteRelativeAlign.Center;
            lblDureeValue.StateCommon.ShortText.TextV = PaletteRelativeAlign.Center;
            lblDureeValue.TabIndex = 1;
            lblDureeValue.Values.Text = "N/A";
            // 
            // kryptonHeaderGroup2
            // 
            kryptonHeaderGroup2.Location = new Point(187, 34);
            // 
            // 
            // 
            kryptonHeaderGroup2.Panel.Controls.Add(lblCoutIndirectValue);
            kryptonHeaderGroup2.Panel.Controls.Add(lblCoutRhValue);
            kryptonHeaderGroup2.Panel.Controls.Add(kryptonLabel4);
            kryptonHeaderGroup2.Panel.Controls.Add(kryptonLabel3);
            kryptonHeaderGroup2.Size = new Size(181, 120);
            kryptonHeaderGroup2.TabIndex = 6;
            kryptonHeaderGroup2.ValuesPrimary.Heading = "Détail Coûts";
            kryptonHeaderGroup2.ValuesPrimary.Image = null;
            kryptonHeaderGroup2.ValuesSecondary.Heading = "(Main d'oeuvre & Indirect)";
            // 
            // lblCoutIndirectValue
            // 
            lblCoutIndirectValue.Location = new Point(70, 38);
            lblCoutIndirectValue.Name = "lblCoutIndirectValue";
            lblCoutIndirectValue.Size = new Size(33, 20);
            lblCoutIndirectValue.StateCommon.ShortText.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblCoutIndirectValue.TabIndex = 3;
            lblCoutIndirectValue.Values.Text = "N/A";
            // 
            // lblCoutRhValue
            // 
            lblCoutRhValue.Location = new Point(70, 12);
            lblCoutRhValue.Name = "lblCoutRhValue";
            lblCoutRhValue.Size = new Size(33, 20);
            lblCoutRhValue.StateCommon.ShortText.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblCoutRhValue.TabIndex = 2;
            lblCoutRhValue.Values.Text = "N/A";
            // 
            // kryptonLabel4
            // 
            kryptonLabel4.Location = new Point(4, 38);
            kryptonLabel4.Name = "kryptonLabel4";
            kryptonLabel4.Size = new Size(58, 20);
            kryptonLabel4.TabIndex = 1;
            kryptonLabel4.Values.Text = "Indirect :";
            // 
            // kryptonLabel3
            // 
            kryptonLabel3.Location = new Point(4, 12);
            kryptonLabel3.Name = "kryptonLabel3";
            kryptonLabel3.Size = new Size(33, 20);
            kryptonLabel3.TabIndex = 0;
            kryptonLabel3.Values.Text = "RH :";
            // 
            // kryptonHeaderGroup1
            // 
            kryptonHeaderGroup1.Location = new Point(0, 34);
            // 
            // 
            // 
            kryptonHeaderGroup1.Panel.Controls.Add(lblCoutTotalValue);
            kryptonHeaderGroup1.Size = new Size(181, 120);
            kryptonHeaderGroup1.TabIndex = 5;
            kryptonHeaderGroup1.ValuesPrimary.Heading = "Coût Total Estimé";
            kryptonHeaderGroup1.ValuesPrimary.Image = null;
            kryptonHeaderGroup1.ValuesSecondary.Heading = "";
            // 
            // lblCoutTotalValue
            // 
            lblCoutTotalValue.Dock = DockStyle.Fill;
            lblCoutTotalValue.Location = new Point(0, 0);
            lblCoutTotalValue.Name = "lblCoutTotalValue";
            lblCoutTotalValue.Size = new Size(179, 85);
            lblCoutTotalValue.StateCommon.ShortText.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblCoutTotalValue.StateCommon.ShortText.TextH = PaletteRelativeAlign.Center;
            lblCoutTotalValue.StateCommon.ShortText.TextV = PaletteRelativeAlign.Center;
            lblCoutTotalValue.TabIndex = 0;
            lblCoutTotalValue.Values.Text = "N/A";
            // 
            // khgNotification
            // 
            khgNotification.Dock = DockStyle.Top;
            khgNotification.Location = new Point(10, 10);
            // 
            // 
            // 
            khgNotification.Panel.Controls.Add(rtbNotification);
            khgNotification.Size = new Size(954, 144);
            khgNotification.StateCommon.HeaderPrimary.Content.ShortText.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            khgNotification.TabIndex = 5;
            khgNotification.ValuesPrimary.Heading = "Titre de la Notification";
            khgNotification.ValuesPrimary.Image = null;
            khgNotification.ValuesSecondary.Heading = "Sous-titre de la notification";
            khgNotification.Visible = false;
            // 
            // rtbNotification
            // 
            rtbNotification.Dock = DockStyle.Fill;
            rtbNotification.Location = new Point(0, 0);
            rtbNotification.Name = "rtbNotification";
            rtbNotification.ReadOnly = true;
            rtbNotification.Size = new Size(952, 94);
            rtbNotification.StateCommon.Back.Color1 = SystemColors.Control;
            rtbNotification.TabIndex = 0;
            rtbNotification.Text = "";
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
            colMetierPrincipal.DataPropertyName = "MetierPrincipalNom";
            colMetierPrincipal.HeaderText = "Métier Principal";
            colMetierPrincipal.Name = "colMetierPrincipal";
            colMetierPrincipal.ReadOnly = true;
            // 
            // colTauxOccupation
            // 
            colTauxOccupation.DataPropertyName = "TauxOccupation";
            dataGridViewCellStyle3.Format = "P1";
            colTauxOccupation.DefaultCellStyle = dataGridViewCellStyle3;
            colTauxOccupation.HeaderText = "Taux Occupation (%)";
            colTauxOccupation.Name = "colTauxOccupation";
            colTauxOccupation.ReadOnly = true;
            // 
            // colTauxFragmentation
            // 
            colTauxFragmentation.DataPropertyName = "TauxFragmentation";
            dataGridViewCellStyle4.Format = "P1";
            colTauxFragmentation.DefaultCellStyle = dataGridViewCellStyle4;
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
            colJours.DataPropertyName = "JoursTravailles";
            colJours.HeaderText = "Jours Trav.";
            colJours.Name = "colJours";
            colJours.ReadOnly = true;
            // 
            // chartChargeJournaliere
            // 
            chartArea2.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea2.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea2.Name = "ChartArea1";
            chartChargeJournaliere.ChartAreas.Add(chartArea2);
            chartChargeJournaliere.Dock = DockStyle.Fill;
            legend2.Enabled = false;
            legend2.Name = "Legend1";
            chartChargeJournaliere.Legends.Add(legend2);
            chartChargeJournaliere.Location = new Point(0, 0);
            chartChargeJournaliere.Name = "chartChargeJournaliere";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            chartChargeJournaliere.Series.Add(series2);
            chartChargeJournaliere.Size = new Size(977, 219);
            chartChargeJournaliere.TabIndex = 0;
            chartChargeJournaliere.Text = "chart1";
            title2.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            title2.Name = "Title1";
            title2.Text = "Charge Journalière (Nombre d'Ouvriers)";
            chartChargeJournaliere.Titles.Add(title2);
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
            groupConfig.CaptionEdge = VisualOrientation.Left;
            groupConfig.CaptionOrientation = ButtonOrientation.FixedLeft;
            groupConfig.Dock = DockStyle.Top;
            groupConfig.Location = new Point(12, 12);
            groupConfig.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupConfig.Panel.Controls.Add(btnExportGantt);
            groupConfig.Panel.Controls.Add(btnExportPlanningExcel);
            groupConfig.Panel.Controls.Add(groupOptimisation);
            groupConfig.Panel.Controls.Add(groupFinancier);
            groupConfig.Panel.Controls.Add(btnLaunch);
            groupConfig.Panel.Controls.Add(groupCalendrier);
            groupConfig.Size = new Size(976, 261);
            groupConfig.TabIndex = 0;
            groupConfig.Values.Heading = "Configuration et Planification";
            // 
            // btnExportGantt
            // 
            btnExportGantt.Enabled = false;
            btnExportGantt.Location = new Point(794, 173);
            btnExportGantt.Margin = new Padding(4, 3, 4, 3);
            btnExportGantt.Name = "btnExportGantt";
            btnExportGantt.Size = new Size(175, 40);
            btnExportGantt.TabIndex = 5;
            btnExportGantt.Values.DropDownArrowColor = Color.Empty;
            btnExportGantt.Values.Text = "Export GanttProject";
            btnExportGantt.Click += btnExportGantt_Click;
            // 
            // btnExportPlanningExcel
            // 
            btnExportPlanningExcel.Enabled = false;
            btnExportPlanningExcel.Location = new Point(612, 173);
            btnExportPlanningExcel.Margin = new Padding(4, 3, 4, 3);
            btnExportPlanningExcel.Name = "btnExportPlanningExcel";
            btnExportPlanningExcel.Size = new Size(175, 40);
            btnExportPlanningExcel.TabIndex = 4;
            btnExportPlanningExcel.Values.DropDownArrowColor = Color.Empty;
            btnExportPlanningExcel.Values.Text = "Export Planning";
            btnExportPlanningExcel.Click += btnExportPlanningExcel_Click;
            // 
            // groupOptimisation
            // 
            groupOptimisation.Location = new Point(688, 4);
            // 
            // 
            // 
            groupOptimisation.Panel.Controls.Add(lblHelpTypeSortie);
            groupOptimisation.Panel.Controls.Add(lblHelpDecoupage);
            groupOptimisation.Panel.Controls.Add(labelCalculMax);
            groupOptimisation.Panel.Controls.Add(cmbCalculMax);
            groupOptimisation.Panel.Controls.Add(label8);
            groupOptimisation.Panel.Controls.Add(cmbTypeDeSortie);
            groupOptimisation.Panel.Controls.Add(labelSeuilDecoupage);
            groupOptimisation.Panel.Controls.Add(numSeuilDecoupage);
            groupOptimisation.Size = new Size(281, 163);
            groupOptimisation.TabIndex = 2;
            groupOptimisation.Values.Heading = "Paramètres d'Optimisation";
            // 
            // lblHelpTypeSortie
            // 
            lblHelpTypeSortie.Location = new Point(259, 9);
            lblHelpTypeSortie.Name = "lblHelpTypeSortie";
            lblHelpTypeSortie.Size = new Size(24, 21);
            lblHelpTypeSortie.StateCommon.ShortText.Font = new Font("Segoe UI Symbol", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblHelpTypeSortie.TabIndex = 28;
            lblHelpTypeSortie.Values.Text = "❔";
            // 
            // lblHelpDecoupage
            // 
            lblHelpDecoupage.Location = new Point(259, 67);
            lblHelpDecoupage.Name = "lblHelpDecoupage";
            lblHelpDecoupage.Size = new Size(24, 21);
            lblHelpDecoupage.StateCommon.ShortText.Font = new Font("Segoe UI Symbol", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblHelpDecoupage.TabIndex = 27;
            lblHelpDecoupage.Values.Text = "❔";
            // 
            // labelCalculMax
            // 
            labelCalculMax.Location = new Point(13, 38);
            labelCalculMax.Margin = new Padding(4, 3, 4, 3);
            labelCalculMax.Name = "labelCalculMax";
            labelCalculMax.Size = new Size(115, 20);
            labelCalculMax.TabIndex = 14;
            labelCalculMax.Values.Text = "Temps calcul (min):";
            // 
            // cmbCalculMax
            // 
            cmbCalculMax.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCalculMax.DropDownWidth = 70;
            cmbCalculMax.IntegralHeight = false;
            cmbCalculMax.Location = new Point(164, 38);
            cmbCalculMax.Margin = new Padding(4, 3, 4, 3);
            cmbCalculMax.Name = "cmbCalculMax";
            cmbCalculMax.Size = new Size(96, 22);
            cmbCalculMax.TabIndex = 1;
            // 
            // label8
            // 
            label8.Location = new Point(13, 9);
            label8.Margin = new Padding(4, 3, 4, 3);
            label8.Name = "label8";
            label8.Size = new Size(95, 20);
            label8.TabIndex = 12;
            label8.Values.Text = "Type de Sortie :";
            // 
            // cmbTypeDeSortie
            // 
            cmbTypeDeSortie.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTypeDeSortie.DropDownWidth = 150;
            cmbTypeDeSortie.IntegralHeight = false;
            cmbTypeDeSortie.Location = new Point(116, 8);
            cmbTypeDeSortie.Margin = new Padding(4, 3, 4, 3);
            cmbTypeDeSortie.Name = "cmbTypeDeSortie";
            cmbTypeDeSortie.Size = new Size(144, 22);
            cmbTypeDeSortie.TabIndex = 0;
            // 
            // labelSeuilDecoupage
            // 
            labelSeuilDecoupage.Location = new Point(13, 67);
            labelSeuilDecoupage.Name = "labelSeuilDecoupage";
            labelSeuilDecoupage.Size = new Size(127, 20);
            labelSeuilDecoupage.TabIndex = 25;
            labelSeuilDecoupage.Values.Text = "Découper si > (jours):";
            // 
            // numSeuilDecoupage
            // 
            numSeuilDecoupage.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numSeuilDecoupage.Location = new Point(164, 67);
            numSeuilDecoupage.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numSeuilDecoupage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSeuilDecoupage.Name = "numSeuilDecoupage";
            numSeuilDecoupage.Size = new Size(96, 22);
            numSeuilDecoupage.TabIndex = 2;
            numSeuilDecoupage.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // groupFinancier
            // 
            groupFinancier.Location = new Point(415, 4);
            // 
            // 
            // 
            groupFinancier.Panel.Controls.Add(lblHelpCoutIndirect);
            groupFinancier.Panel.Controls.Add(lblHelpPenalite);
            groupFinancier.Panel.Controls.Add(label11);
            groupFinancier.Panel.Controls.Add(numCoutIndirect);
            groupFinancier.Panel.Controls.Add(label10);
            groupFinancier.Panel.Controls.Add(numPenaliteChangement);
            groupFinancier.Size = new Size(267, 163);
            groupFinancier.TabIndex = 1;
            groupFinancier.Values.Heading = "Paramètres Financiers";
            // 
            // lblHelpCoutIndirect
            // 
            lblHelpCoutIndirect.Location = new Point(241, 9);
            lblHelpCoutIndirect.Name = "lblHelpCoutIndirect";
            lblHelpCoutIndirect.Size = new Size(24, 21);
            lblHelpCoutIndirect.StateCommon.ShortText.Font = new Font("Segoe UI Symbol", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblHelpCoutIndirect.TabIndex = 13;
            lblHelpCoutIndirect.Values.Text = "❔";
            // 
            // lblHelpPenalite
            // 
            lblHelpPenalite.Location = new Point(241, 37);
            lblHelpPenalite.Name = "lblHelpPenalite";
            lblHelpPenalite.Size = new Size(24, 21);
            lblHelpPenalite.StateCommon.ShortText.Font = new Font("Segoe UI Symbol", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblHelpPenalite.TabIndex = 12;
            lblHelpPenalite.Values.Text = "❔";
            // 
            // label11
            // 
            label11.Location = new Point(12, 9);
            label11.Margin = new Padding(4, 3, 4, 3);
            label11.Name = "label11";
            label11.Size = new Size(120, 20);
            label11.TabIndex = 10;
            label11.Values.Text = "Coût indirect / jour :";
            // 
            // numCoutIndirect
            // 
            numCoutIndirect.AllowDecimals = true;
            numCoutIndirect.DecimalPlaces = 2;
            numCoutIndirect.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numCoutIndirect.Location = new Point(142, 8);
            numCoutIndirect.Margin = new Padding(4, 3, 4, 3);
            numCoutIndirect.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numCoutIndirect.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numCoutIndirect.Name = "numCoutIndirect";
            numCoutIndirect.Size = new Size(100, 22);
            numCoutIndirect.TabIndex = 0;
            numCoutIndirect.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // label10
            // 
            label10.Location = new Point(12, 38);
            label10.Margin = new Padding(4, 3, 4, 3);
            label10.Name = "label10";
            label10.Size = new Size(128, 20);
            label10.TabIndex = 8;
            label10.Values.Text = "Pénalité changement:";
            // 
            // numPenaliteChangement
            // 
            numPenaliteChangement.AllowDecimals = true;
            numPenaliteChangement.DecimalPlaces = 1;
            numPenaliteChangement.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numPenaliteChangement.Location = new Point(142, 37);
            numPenaliteChangement.Margin = new Padding(4, 3, 4, 3);
            numPenaliteChangement.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numPenaliteChangement.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numPenaliteChangement.Name = "numPenaliteChangement";
            numPenaliteChangement.Size = new Size(100, 22);
            numPenaliteChangement.TabIndex = 1;
            numPenaliteChangement.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // btnLaunch
            // 
            btnLaunch.Location = new Point(222, 173);
            btnLaunch.Margin = new Padding(4, 3, 4, 3);
            btnLaunch.Name = "btnLaunch";
            btnLaunch.Size = new Size(383, 40);
            btnLaunch.TabIndex = 3;
            btnLaunch.Values.DropDownArrowColor = Color.Empty;
            btnLaunch.Values.Text = "LANCER LA PLANIFICATION";
            btnLaunch.Click += btnLaunch_Click;
            // 
            // groupCalendrier
            // 
            groupCalendrier.Location = new Point(4, 4);
            // 
            // 
            // 
            groupCalendrier.Panel.Controls.Add(label5);
            groupCalendrier.Panel.Controls.Add(chkListJoursOuvres);
            groupCalendrier.Panel.Controls.Add(kryptonLabel1);
            groupCalendrier.Panel.Controls.Add(dtpDateDebut);
            groupCalendrier.Panel.Controls.Add(kryptonLabel2);
            groupCalendrier.Panel.Controls.Add(dtpDateFin);
            groupCalendrier.Panel.Controls.Add(label6);
            groupCalendrier.Panel.Controls.Add(numHeureDebut);
            groupCalendrier.Panel.Controls.Add(label7);
            groupCalendrier.Panel.Controls.Add(numDureeOuverture);
            groupCalendrier.Panel.Controls.Add(label9);
            groupCalendrier.Panel.Controls.Add(numHeuresTravail);
            groupCalendrier.Size = new Size(405, 163);
            groupCalendrier.TabIndex = 0;
            groupCalendrier.Values.Heading = "Paramètres du Calendrier";
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
            // chkListJoursOuvres
            // 
            chkListJoursOuvres.CheckOnClick = true;
            chkListJoursOuvres.Location = new Point(15, 30);
            chkListJoursOuvres.Margin = new Padding(4, 3, 4, 3);
            chkListJoursOuvres.Name = "chkListJoursOuvres";
            chkListJoursOuvres.Size = new Size(110, 80);
            chkListJoursOuvres.TabIndex = 0;
            // 
            // kryptonLabel1
            // 
            kryptonLabel1.Location = new Point(132, 9);
            kryptonLabel1.Name = "kryptonLabel1";
            kryptonLabel1.Size = new Size(95, 20);
            kryptonLabel1.TabIndex = 21;
            kryptonLabel1.Values.Text = "Date de début :";
            // 
            // dtpDateDebut
            // 
            dtpDateDebut.Location = new Point(230, 8);
            dtpDateDebut.Margin = new Padding(4, 3, 4, 3);
            dtpDateDebut.Name = "dtpDateDebut";
            dtpDateDebut.Size = new Size(157, 21);
            dtpDateDebut.TabIndex = 1;
            // 
            // kryptonLabel2
            // 
            kryptonLabel2.Location = new Point(132, 33);
            kryptonLabel2.Name = "kryptonLabel2";
            kryptonLabel2.Size = new Size(77, 20);
            kryptonLabel2.TabIndex = 22;
            kryptonLabel2.Values.Text = "Date de fin :";
            // 
            // dtpDateFin
            // 
            dtpDateFin.Location = new Point(230, 32);
            dtpDateFin.Margin = new Padding(4, 3, 4, 3);
            dtpDateFin.Name = "dtpDateFin";
            dtpDateFin.Size = new Size(157, 21);
            dtpDateFin.TabIndex = 2;
            // 
            // label6
            // 
            label6.Location = new Point(132, 59);
            label6.Margin = new Padding(4, 3, 4, 3);
            label6.Name = "label6";
            label6.Size = new Size(102, 20);
            label6.TabIndex = 2;
            label6.Values.Text = "Heure ouverture:";
            // 
            // numHeureDebut
            // 
            numHeureDebut.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numHeureDebut.Location = new Point(305, 58);
            numHeureDebut.Margin = new Padding(4, 3, 4, 3);
            numHeureDebut.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            numHeureDebut.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHeureDebut.Name = "numHeureDebut";
            numHeureDebut.Size = new Size(82, 22);
            numHeureDebut.TabIndex = 3;
            numHeureDebut.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // label7
            // 
            label7.Location = new Point(132, 85);
            label7.Margin = new Padding(4, 3, 4, 3);
            label7.Name = "label7";
            label7.Size = new Size(124, 20);
            label7.TabIndex = 4;
            label7.Values.Text = "Durée ouverture (h) :";
            // 
            // numDureeOuverture
            // 
            numDureeOuverture.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numDureeOuverture.Location = new Point(305, 84);
            numDureeOuverture.Margin = new Padding(4, 3, 4, 3);
            numDureeOuverture.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numDureeOuverture.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numDureeOuverture.Name = "numDureeOuverture";
            numDureeOuverture.Size = new Size(82, 22);
            numDureeOuverture.TabIndex = 4;
            numDureeOuverture.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // label9
            // 
            label9.Location = new Point(132, 111);
            label9.Margin = new Padding(4, 3, 4, 3);
            label9.Name = "label9";
            label9.Size = new Size(121, 20);
            label9.TabIndex = 6;
            label9.Values.Text = "Durée de travail (h) :";
            // 
            // numHeuresTravail
            // 
            numHeuresTravail.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numHeuresTravail.Location = new Point(305, 110);
            numHeuresTravail.Margin = new Padding(4, 3, 4, 3);
            numHeuresTravail.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numHeuresTravail.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHeuresTravail.Name = "numHeuresTravail";
            numHeuresTravail.Size = new Size(82, 22);
            numHeuresTravail.TabIndex = 5;
            numHeuresTravail.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // PlanificatorView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonPanelMain);
            Margin = new Padding(4, 3, 4, 3);
            Name = "PlanificatorView";
            Size = new Size(1000, 800);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).EndInit();
            kryptonPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)navigatorResultats).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabPageSynthese).EndInit();
            tabPageSynthese.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelKpis).EndInit();
            kryptonPanelKpis.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)khgSolverStatus.Panel).EndInit();
            khgSolverStatus.Panel.ResumeLayout(false);
            khgSolverStatus.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)khgSolverStatus).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)khgNotification.Panel).EndInit();
            khgNotification.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)khgNotification).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)groupConfig).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupOptimisation.Panel).EndInit();
            groupOptimisation.Panel.ResumeLayout(false);
            groupOptimisation.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupOptimisation).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCalculMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbTypeDeSortie).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupFinancier.Panel).EndInit();
            groupFinancier.Panel.ResumeLayout(false);
            groupFinancier.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupFinancier).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupCalendrier.Panel).EndInit();
            groupCalendrier.Panel.ResumeLayout(false);
            groupCalendrier.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupCalendrier).EndInit();
            ResumeLayout(false);

        }
        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonGroupBox groupConfig;
        private KryptonButton btnLaunch;
        private Krypton.Navigator.KryptonNavigator navigatorResultats;
        private Krypton.Navigator.KryptonPage tabPageSynthese;
        private Krypton.Navigator.KryptonPage tabPageRessources;
        private Krypton.Navigator.KryptonPage tabPageJournal;
        private KryptonRichTextBox rtbLog;
        private KryptonHeaderGroup khgSolverStatus;
        private KryptonLabel lblStatutExplication;
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
        private System.Windows.Forms.Timer planningTimer;
        private KryptonProgressBar SolverProgressBar;
        private KryptonGroupBox groupCalendrier;
        private KryptonLabel label5;
        private KryptonCheckedListBox chkListJoursOuvres;
        private KryptonLabel kryptonLabel1;
        private KryptonDateTimePicker dtpDateDebut;
        private KryptonLabel kryptonLabel2;
        private KryptonDateTimePicker dtpDateFin;
        private KryptonLabel label6;
        private KryptonNumericUpDown numHeureDebut;
        private KryptonLabel label7;
        private KryptonNumericUpDown numDureeOuverture;
        private KryptonLabel label9;
        private KryptonNumericUpDown numHeuresTravail;
        private KryptonGroupBox groupOptimisation;
        private KryptonLabel labelCalculMax;
        private KryptonComboBox cmbCalculMax;
        private KryptonLabel label8;
        private KryptonComboBox cmbTypeDeSortie;
        private KryptonLabel labelSeuilDecoupage;
        private KryptonNumericUpDown numSeuilDecoupage;
        private KryptonGroupBox groupFinancier;
        private KryptonLabel label11;
        private KryptonNumericUpDown numCoutIndirect;
        private KryptonLabel label10;
        private KryptonNumericUpDown numPenaliteChangement;
        private KryptonHeaderGroup khgNotification;
        private KryptonRichTextBox rtbNotification;
        private KryptonPanel kryptonPanelKpis;
        private KryptonLabel lblHelpCoutIndirect;
        private KryptonLabel lblHelpPenalite;
        private KryptonLabel lblHelpTypeSortie;
        private KryptonLabel lblHelpDecoupage;
        private KryptonButton btnExportGantt;
        private KryptonButton btnExportPlanningExcel;
        private ToolTip kryptonToolTip;
    }
}