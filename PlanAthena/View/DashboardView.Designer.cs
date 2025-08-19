using Krypton.Toolkit;

namespace PlanAthena.View
{
    partial class DashboardView
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
            kryptonSplitContainer1 = new KryptonSplitContainer();
            headerProjects = new KryptonHeaderGroup();
            panelProjectList = new FlowLayoutPanel();
            btnNewProject = new KryptonButton();
            groupDetails = new KryptonGroupBox();
            groupWorkDays = new KryptonGroupBox();
            chkSunday = new KryptonCheckBox();
            chkSaturday = new KryptonCheckBox();
            chkFriday = new KryptonCheckBox();
            chkThursday = new KryptonCheckBox();
            chkWednesday = new KryptonCheckBox();
            chkTuesday = new KryptonCheckBox();
            chkMonday = new KryptonCheckBox();
            numDailyCost = new KryptonNumericUpDown();
            labelDailyCost = new KryptonLabel();
            numWorkDuration = new KryptonNumericUpDown();
            labelWorkDuration = new KryptonLabel();
            labelCalculatedDuration = new KryptonLabel();
            labelDuration = new KryptonLabel();
            numClosingTime = new KryptonNumericUpDown();
            labelClosingTime = new KryptonLabel();
            numOpeningTime = new KryptonNumericUpDown();
            labelOpeningTime = new KryptonLabel();
            labelDescription = new KryptonLabel();
            textDescription = new KryptonTextBox();
            labelName = new KryptonLabel();
            textName = new KryptonTextBox();
            labelFilePath = new KryptonLabel();
            textFilePath = new KryptonTextBox();
            panelActions = new KryptonPanel();
            btnManage = new KryptonButton();
            btnSave = new KryptonButton();
            btnDelete = new KryptonButton();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).BeginInit();
            (kryptonSplitContainer1.Panel1).BeginInit();
            kryptonSplitContainer1.Panel1.SuspendLayout();
            (kryptonSplitContainer1.Panel2).BeginInit();
            kryptonSplitContainer1.Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)headerProjects).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerProjects.Panel).BeginInit();
            headerProjects.Panel.SuspendLayout();
            panelProjectList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupDetails).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).BeginInit();
            groupDetails.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupWorkDays).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupWorkDays.Panel).BeginInit();
            groupWorkDays.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelActions).BeginInit();
            panelActions.SuspendLayout();
            SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            kryptonPanelMain.Controls.Add(kryptonSplitContainer1);
            kryptonPanelMain.Dock = DockStyle.Fill;
            kryptonPanelMain.Location = new Point(0, 0);
            kryptonPanelMain.Margin = new Padding(4, 3, 4, 3);
            kryptonPanelMain.Name = "kryptonPanelMain";
            kryptonPanelMain.Padding = new Padding(12);
            kryptonPanelMain.Size = new Size(1328, 749);
            kryptonPanelMain.TabIndex = 0;
            // 
            // kryptonSplitContainer1
            // 
            kryptonSplitContainer1.Dock = DockStyle.Fill;
            kryptonSplitContainer1.Location = new Point(12, 12);
            kryptonSplitContainer1.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            kryptonSplitContainer1.Panel1.Controls.Add(headerProjects);
            // 
            // 
            // 
            kryptonSplitContainer1.Panel2.Controls.Add(groupDetails);
            kryptonSplitContainer1.Panel2.Controls.Add(panelActions);
            kryptonSplitContainer1.Panel2.Padding = new Padding(6, 0, 0, 0);
            kryptonSplitContainer1.Size = new Size(1304, 725);
            kryptonSplitContainer1.SplitterDistance = 433;
            kryptonSplitContainer1.TabIndex = 0;
            // 
            // headerProjects
            // 
            headerProjects.Dock = DockStyle.Fill;
            headerProjects.HeaderVisibleSecondary = false;
            headerProjects.Location = new Point(0, 0);
            headerProjects.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            headerProjects.Panel.Controls.Add(panelProjectList);
            headerProjects.Size = new Size(433, 725);
            headerProjects.TabIndex = 0;
            headerProjects.ValuesPrimary.Heading = "Projets Récents";
            headerProjects.ValuesPrimary.Image = null;
            // 
            // panelProjectList
            // 
            panelProjectList.AutoScroll = true;
            panelProjectList.BackColor = Color.Transparent;
            panelProjectList.Controls.Add(btnNewProject);
            panelProjectList.Dock = DockStyle.Fill;
            panelProjectList.FlowDirection = FlowDirection.TopDown;
            panelProjectList.Location = new Point(0, 0);
            panelProjectList.Margin = new Padding(4, 3, 4, 3);
            panelProjectList.Name = "panelProjectList";
            panelProjectList.Padding = new Padding(12);
            panelProjectList.Size = new Size(431, 693);
            panelProjectList.TabIndex = 0;
            panelProjectList.WrapContents = false;
            panelProjectList.Resize += panelProjectList_Resize;
            // 
            // btnNewProject
            // 
            btnNewProject.Location = new Point(16, 15);
            btnNewProject.Margin = new Padding(4, 3, 4, 3);
            btnNewProject.Name = "btnNewProject";
            btnNewProject.Size = new Size(390, 46);
            btnNewProject.TabIndex = 0;
            btnNewProject.Values.DropDownArrowColor = Color.Empty;
            btnNewProject.Values.Text = "Nouveau Projet";
            btnNewProject.Click += btnNewProject_Click;
            // 
            // groupDetails
            // 
            groupDetails.Dock = DockStyle.Fill;
            groupDetails.Location = new Point(6, 0);
            groupDetails.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupDetails.Panel.Controls.Add(groupWorkDays);
            groupDetails.Panel.Controls.Add(numDailyCost);
            groupDetails.Panel.Controls.Add(labelDailyCost);
            groupDetails.Panel.Controls.Add(numWorkDuration);
            groupDetails.Panel.Controls.Add(labelWorkDuration);
            groupDetails.Panel.Controls.Add(labelCalculatedDuration);
            groupDetails.Panel.Controls.Add(labelDuration);
            groupDetails.Panel.Controls.Add(numClosingTime);
            groupDetails.Panel.Controls.Add(labelClosingTime);
            groupDetails.Panel.Controls.Add(numOpeningTime);
            groupDetails.Panel.Controls.Add(labelOpeningTime);
            groupDetails.Panel.Controls.Add(labelDescription);
            groupDetails.Panel.Controls.Add(textDescription);
            groupDetails.Panel.Controls.Add(labelName);
            groupDetails.Panel.Controls.Add(textName);
            groupDetails.Panel.Controls.Add(labelFilePath);
            groupDetails.Panel.Controls.Add(textFilePath);
            groupDetails.Size = new Size(860, 667);
            groupDetails.TabIndex = 1;
            groupDetails.Values.Heading = "Édition du Projet";
            // 
            // groupWorkDays
            // 
            groupWorkDays.Location = new Point(20, 331);
            groupWorkDays.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupWorkDays.Panel.Controls.Add(chkSunday);
            groupWorkDays.Panel.Controls.Add(chkSaturday);
            groupWorkDays.Panel.Controls.Add(chkFriday);
            groupWorkDays.Panel.Controls.Add(chkThursday);
            groupWorkDays.Panel.Controls.Add(chkWednesday);
            groupWorkDays.Panel.Controls.Add(chkTuesday);
            groupWorkDays.Panel.Controls.Add(chkMonday);
            groupWorkDays.Size = new Size(816, 99);
            groupWorkDays.TabIndex = 16;
            groupWorkDays.Values.Heading = "Jours Ouvrés";
            // 
            // chkSunday
            // 
            chkSunday.Location = new Point(350, 36);
            chkSunday.Margin = new Padding(4, 3, 4, 3);
            chkSunday.Name = "chkSunday";
            chkSunday.Size = new Size(78, 20);
            chkSunday.TabIndex = 6;
            chkSunday.Values.Text = "Dimanche";
            // 
            // chkSaturday
            // 
            chkSaturday.Location = new Point(186, 36);
            chkSaturday.Margin = new Padding(4, 3, 4, 3);
            chkSaturday.Name = "chkSaturday";
            chkSaturday.Size = new Size(64, 20);
            chkSaturday.TabIndex = 5;
            chkSaturday.Values.Text = "Samedi";
            // 
            // chkFriday
            // 
            chkFriday.Location = new Point(18, 36);
            chkFriday.Margin = new Padding(4, 3, 4, 3);
            chkFriday.Name = "chkFriday";
            chkFriday.Size = new Size(73, 20);
            chkFriday.TabIndex = 4;
            chkFriday.Values.Text = "Vendredi";
            // 
            // chkThursday
            // 
            chkThursday.Location = new Point(536, 6);
            chkThursday.Margin = new Padding(4, 3, 4, 3);
            chkThursday.Name = "chkThursday";
            chkThursday.Size = new Size(52, 20);
            chkThursday.TabIndex = 3;
            chkThursday.Values.Text = "Jeudi";
            // 
            // chkWednesday
            // 
            chkWednesday.Location = new Point(350, 6);
            chkWednesday.Margin = new Padding(4, 3, 4, 3);
            chkWednesday.Name = "chkWednesday";
            chkWednesday.Size = new Size(72, 20);
            chkWednesday.TabIndex = 2;
            chkWednesday.Values.Text = "Mercredi";
            // 
            // chkTuesday
            // 
            chkTuesday.Location = new Point(186, 6);
            chkTuesday.Margin = new Padding(4, 3, 4, 3);
            chkTuesday.Name = "chkTuesday";
            chkTuesday.Size = new Size(55, 20);
            chkTuesday.TabIndex = 1;
            chkTuesday.Values.Text = "Mardi";
            // 
            // chkMonday
            // 
            chkMonday.Location = new Point(18, 6);
            chkMonday.Margin = new Padding(4, 3, 4, 3);
            chkMonday.Name = "chkMonday";
            chkMonday.Size = new Size(54, 20);
            chkMonday.TabIndex = 0;
            chkMonday.Values.Text = "Lundi";
            // 
            // numDailyCost
            // 
            numDailyCost.AllowDecimals = true;
            numDailyCost.DecimalPlaces = 2;
            numDailyCost.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numDailyCost.Location = new Point(635, 277);
            numDailyCost.Margin = new Padding(4, 3, 4, 3);
            numDailyCost.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numDailyCost.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numDailyCost.Name = "numDailyCost";
            numDailyCost.Size = new Size(140, 22);
            numDailyCost.TabIndex = 15;
            numDailyCost.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // labelDailyCost
            // 
            labelDailyCost.Location = new Point(442, 277);
            labelDailyCost.Margin = new Padding(4, 3, 4, 3);
            labelDailyCost.Name = "labelDailyCost";
            labelDailyCost.Size = new Size(146, 20);
            labelDailyCost.TabIndex = 14;
            labelDailyCost.Values.Text = "Coût Journalier Projet (€)";
            // 
            // numWorkDuration
            // 
            numWorkDuration.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numWorkDuration.Location = new Point(140, 277);
            numWorkDuration.Margin = new Padding(4, 3, 4, 3);
            numWorkDuration.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numWorkDuration.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numWorkDuration.Name = "numWorkDuration";
            numWorkDuration.Size = new Size(84, 22);
            numWorkDuration.TabIndex = 13;
            numWorkDuration.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // labelWorkDuration
            // 
            labelWorkDuration.Location = new Point(20, 277);
            labelWorkDuration.Margin = new Padding(4, 3, 4, 3);
            labelWorkDuration.Name = "labelWorkDuration";
            labelWorkDuration.Size = new Size(99, 20);
            labelWorkDuration.TabIndex = 12;
            labelWorkDuration.Values.Text = "Durée de Travail";
            // 
            // labelCalculatedDuration
            // 
            labelCalculatedDuration.Location = new Point(635, 232);
            labelCalculatedDuration.Margin = new Padding(4, 3, 4, 3);
            labelCalculatedDuration.Name = "labelCalculatedDuration";
            labelCalculatedDuration.Size = new Size(25, 20);
            labelCalculatedDuration.StateCommon.ShortText.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelCalculatedDuration.TabIndex = 11;
            labelCalculatedDuration.Values.Text = "8h";
            // 
            // labelDuration
            // 
            labelDuration.Location = new Point(572, 232);
            labelDuration.Margin = new Padding(4, 3, 4, 3);
            labelDuration.Name = "labelDuration";
            labelDuration.Size = new Size(49, 20);
            labelDuration.TabIndex = 10;
            labelDuration.Values.Text = "Durée :";
            // 
            // numClosingTime
            // 
            numClosingTime.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numClosingTime.Location = new Point(350, 232);
            numClosingTime.Margin = new Padding(4, 3, 4, 3);
            numClosingTime.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            numClosingTime.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numClosingTime.Name = "numClosingTime";
            numClosingTime.Size = new Size(84, 22);
            numClosingTime.TabIndex = 9;
            numClosingTime.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numClosingTime.ValueChanged += OnTimeChanged;
            // 
            // labelClosingTime
            // 
            labelClosingTime.Location = new Point(264, 232);
            labelClosingTime.Margin = new Padding(4, 3, 4, 3);
            labelClosingTime.Name = "labelClosingTime";
            labelClosingTime.Size = new Size(66, 20);
            labelClosingTime.TabIndex = 8;
            labelClosingTime.Values.Text = "Fermeture";
            // 
            // numOpeningTime
            // 
            numOpeningTime.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numOpeningTime.Location = new Point(140, 232);
            numOpeningTime.Margin = new Padding(4, 3, 4, 3);
            numOpeningTime.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            numOpeningTime.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numOpeningTime.Name = "numOpeningTime";
            numOpeningTime.Size = new Size(84, 22);
            numOpeningTime.TabIndex = 7;
            numOpeningTime.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numOpeningTime.ValueChanged += OnTimeChanged;
            // 
            // labelOpeningTime
            // 
            labelOpeningTime.Location = new Point(20, 232);
            labelOpeningTime.Margin = new Padding(4, 3, 4, 3);
            labelOpeningTime.Name = "labelOpeningTime";
            labelOpeningTime.Size = new Size(65, 20);
            labelOpeningTime.TabIndex = 6;
            labelOpeningTime.Values.Text = "Ouverture";
            // 
            // labelDescription
            // 
            labelDescription.Location = new Point(20, 113);
            labelDescription.Margin = new Padding(4, 3, 4, 3);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new Size(73, 20);
            labelDescription.TabIndex = 5;
            labelDescription.Values.Text = "Description";
            // 
            // textDescription
            // 
            textDescription.Location = new Point(140, 113);
            textDescription.Margin = new Padding(4, 3, 4, 3);
            textDescription.Multiline = true;
            textDescription.Name = "textDescription";
            textDescription.Size = new Size(703, 92);
            textDescription.TabIndex = 4;
            // 
            // labelName
            // 
            labelName.Location = new Point(20, 69);
            labelName.Margin = new Padding(4, 3, 4, 3);
            labelName.Name = "labelName";
            labelName.Size = new Size(38, 20);
            labelName.TabIndex = 3;
            labelName.Values.Text = "Nom";
            // 
            // textName
            // 
            textName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textName.Location = new Point(140, 69);
            textName.Margin = new Padding(4, 3, 4, 3);
            textName.Name = "textName";
            textName.Size = new Size(703, 23);
            textName.TabIndex = 2;
            // 
            // labelFilePath
            // 
            labelFilePath.Location = new Point(20, 25);
            labelFilePath.Margin = new Padding(4, 3, 4, 3);
            labelFilePath.Name = "labelFilePath";
            labelFilePath.Size = new Size(95, 20);
            labelFilePath.TabIndex = 1;
            labelFilePath.Values.Text = "Chemin d'accès";
            // 
            // textFilePath
            // 
            textFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textFilePath.Location = new Point(140, 25);
            textFilePath.Margin = new Padding(4, 3, 4, 3);
            textFilePath.Name = "textFilePath";
            textFilePath.ReadOnly = true;
            textFilePath.Size = new Size(703, 23);
            textFilePath.TabIndex = 0;
            // 
            // panelActions
            // 
            panelActions.Controls.Add(btnManage);
            panelActions.Controls.Add(btnSave);
            panelActions.Controls.Add(btnDelete);
            panelActions.Dock = DockStyle.Bottom;
            panelActions.Location = new Point(6, 667);
            panelActions.Margin = new Padding(4, 3, 4, 3);
            panelActions.Name = "panelActions";
            panelActions.Size = new Size(860, 58);
            panelActions.TabIndex = 0;
            // 
            // btnManage
            // 
            btnManage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnManage.Location = new Point(735, 12);
            btnManage.Margin = new Padding(4, 3, 4, 3);
            btnManage.Name = "btnManage";
            btnManage.Size = new Size(105, 35);
            btnManage.TabIndex = 2;
            btnManage.Values.DropDownArrowColor = Color.Empty;
            btnManage.Values.Text = "Gérer";
            this.btnManage.Click += new System.EventHandler(this.btnManage_Click);
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Location = new Point(623, 12);
            btnSave.Margin = new Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(105, 35);
            btnSave.TabIndex = 1;
            btnSave.Values.DropDownArrowColor = Color.Empty;
            btnSave.Values.Text = "Sauvegarder";
            btnSave.Click += btnSave_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(511, 12);
            btnDelete.Margin = new Padding(4, 3, 4, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(105, 35);
            btnDelete.TabIndex = 0;
            btnDelete.Values.DropDownArrowColor = Color.Empty;
            btnDelete.Values.Text = "Supprimer";
            // 
            // DashboardView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonPanelMain);
            Margin = new Padding(4, 3, 4, 3);
            Name = "DashboardView";
            Size = new Size(1328, 749);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).EndInit();
            kryptonPanelMain.ResumeLayout(false);
            (kryptonSplitContainer1.Panel1).EndInit();
            kryptonSplitContainer1.Panel1.ResumeLayout(false);
            (kryptonSplitContainer1.Panel2).EndInit();
            kryptonSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerProjects.Panel).EndInit();
            headerProjects.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)headerProjects).EndInit();
            panelProjectList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).EndInit();
            groupDetails.Panel.ResumeLayout(false);
            groupDetails.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupDetails).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupWorkDays.Panel).EndInit();
            groupWorkDays.Panel.ResumeLayout(false);
            groupWorkDays.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupWorkDays).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelActions).EndInit();
            panelActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonSplitContainer kryptonSplitContainer1;
        private KryptonHeaderGroup headerProjects;
        private System.Windows.Forms.FlowLayoutPanel panelProjectList;
        private KryptonButton btnNewProject;
        private KryptonPanel panelActions;
        private KryptonButton btnManage;
        private KryptonButton btnSave;
        private KryptonButton btnDelete;
        private KryptonGroupBox groupDetails;
        private KryptonLabel labelDescription;
        private KryptonTextBox textDescription;
        private KryptonLabel labelName;
        private KryptonTextBox textName;
        private KryptonLabel labelFilePath;
        private KryptonTextBox textFilePath;
        private KryptonLabel labelOpeningTime;
        private KryptonLabel labelCalculatedDuration;
        private KryptonLabel labelDuration;
        private KryptonNumericUpDown numClosingTime;
        private KryptonLabel labelClosingTime;
        private KryptonNumericUpDown numOpeningTime;
        private KryptonGroupBox groupWorkDays;
        private KryptonNumericUpDown numDailyCost;
        private KryptonLabel labelDailyCost;
        private KryptonNumericUpDown numWorkDuration;
        private KryptonLabel labelWorkDuration;
        private KryptonCheckBox chkSaturday;
        private KryptonCheckBox chkFriday;
        private KryptonCheckBox chkThursday;
        private KryptonCheckBox chkWednesday;
        private KryptonCheckBox chkTuesday;
        private KryptonCheckBox chkMonday;
        private KryptonCheckBox chkSunday;
    }
}