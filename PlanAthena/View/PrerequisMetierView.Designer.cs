using Krypton.Toolkit;

namespace PlanAthena.View
{
    partial class PrerequisMetierView
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
            navigatorPhases = new Krypton.Navigator.KryptonNavigator();
            groupDetails = new KryptonGroupBox();
            checkedListPrerequis = new KryptonCheckedListBox();
            panelColor = new KryptonPanel();
            labelColor = new KryptonLabel();
            textName = new KryptonTextBox();
            labelName = new KryptonLabel();
            textId = new KryptonTextBox();
            labelId = new KryptonLabel();
            panelGlobalActions = new KryptonPanel();
            btnManageTasks = new KryptonButton();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).BeginInit();
            (kryptonSplitContainer1.Panel1).BeginInit();
            kryptonSplitContainer1.Panel1.SuspendLayout();
            (kryptonSplitContainer1.Panel2).BeginInit();
            kryptonSplitContainer1.Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)navigatorPhases).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).BeginInit();
            groupDetails.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelColor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).BeginInit();
            panelGlobalActions.SuspendLayout();
            SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            kryptonPanelMain.Controls.Add(kryptonSplitContainer1);
            kryptonPanelMain.Controls.Add(panelGlobalActions);
            kryptonPanelMain.Dock = DockStyle.Fill;
            kryptonPanelMain.Location = new Point(0, 0);
            kryptonPanelMain.Margin = new Padding(4, 3, 4, 3);
            kryptonPanelMain.Name = "kryptonPanelMain";
            kryptonPanelMain.Padding = new Padding(12, 12, 12, 12);
            kryptonPanelMain.Size = new Size(1400, 808);
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
            kryptonSplitContainer1.Panel1.Controls.Add(navigatorPhases);
            // 
            // 
            // 
            kryptonSplitContainer1.Panel2.Controls.Add(groupDetails);
            kryptonSplitContainer1.Panel2.Padding = new Padding(12, 0, 0, 0);
            kryptonSplitContainer1.Size = new Size(1376, 726);
            kryptonSplitContainer1.SplitterDistance = 909;
            kryptonSplitContainer1.TabIndex = 1;
            // 
            // navigatorPhases
            // 
            navigatorPhases.ControlKryptonFormFeatures = false;
            navigatorPhases.Dock = DockStyle.Fill;
            navigatorPhases.Location = new Point(0, 0);
            navigatorPhases.Margin = new Padding(4, 3, 4, 3);
            navigatorPhases.NavigatorMode = Krypton.Navigator.NavigatorMode.BarTabGroup;
            navigatorPhases.Owner = null;
            navigatorPhases.PageBackStyle = PaletteBackStyle.PanelClient;
            navigatorPhases.Size = new Size(909, 726);
            navigatorPhases.TabIndex = 0;
            navigatorPhases.Text = "kryptonNavigator1";
            // 
            // groupDetails
            // 
            groupDetails.Dock = DockStyle.Fill;
            groupDetails.Location = new Point(12, 0);
            groupDetails.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupDetails.Panel.Controls.Add(checkedListPrerequis);
            groupDetails.Panel.Controls.Add(panelColor);
            groupDetails.Panel.Controls.Add(labelColor);
            groupDetails.Panel.Controls.Add(textName);
            groupDetails.Panel.Controls.Add(labelName);
            groupDetails.Panel.Controls.Add(textId);
            groupDetails.Panel.Controls.Add(labelId);
            groupDetails.Size = new Size(450, 726);
            groupDetails.TabIndex = 0;
            groupDetails.Values.Heading = "Détails du Prérequis";
            // 
            // checkedListPrerequis
            // 
            checkedListPrerequis.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            checkedListPrerequis.Location = new Point(15, 127);
            checkedListPrerequis.Margin = new Padding(4, 3, 4, 3);
            checkedListPrerequis.Name = "checkedListPrerequis";
            checkedListPrerequis.Size = new Size(420, 567);
            checkedListPrerequis.TabIndex = 6;
            // 
            // panelColor
            // 
            panelColor.Enabled = false;
            panelColor.Location = new Point(152, 81);
            panelColor.Margin = new Padding(4, 3, 4, 3);
            panelColor.Name = "panelColor";
            panelColor.Size = new Size(148, 27);
            panelColor.StateCommon.Color1 = Color.Gainsboro;
            panelColor.TabIndex = 5;
            // 
            // labelColor
            // 
            labelColor.Location = new Point(15, 81);
            labelColor.Margin = new Padding(4, 3, 4, 3);
            labelColor.Name = "labelColor";
            labelColor.Size = new Size(59, 20);
            labelColor.TabIndex = 4;
            labelColor.Values.Text = "Couleur :";
            // 
            // textName
            // 
            textName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textName.Location = new Point(152, 40);
            textName.Margin = new Padding(4, 3, 4, 3);
            textName.Name = "textName";
            textName.ReadOnly = true;
            textName.Size = new Size(282, 23);
            textName.TabIndex = 3;
            // 
            // labelName
            // 
            labelName.Location = new Point(15, 40);
            labelName.Margin = new Padding(4, 3, 4, 3);
            labelName.Name = "labelName";
            labelName.Size = new Size(44, 20);
            labelName.TabIndex = 2;
            labelName.Values.Text = "Nom :";
            // 
            // textId
            // 
            textId.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textId.Location = new Point(152, 0);
            textId.Margin = new Padding(4, 3, 4, 3);
            textId.Name = "textId";
            textId.ReadOnly = true;
            textId.Size = new Size(282, 23);
            textId.TabIndex = 1;
            // 
            // labelId
            // 
            labelId.Location = new Point(15, 0);
            labelId.Margin = new Padding(4, 3, 4, 3);
            labelId.Name = "labelId";
            labelId.Size = new Size(29, 20);
            labelId.TabIndex = 0;
            labelId.Values.Text = "ID :";
            // 
            // panelGlobalActions
            // 
            panelGlobalActions.Controls.Add(btnManageTasks);
            panelGlobalActions.Dock = DockStyle.Bottom;
            panelGlobalActions.Location = new Point(12, 738);
            panelGlobalActions.Margin = new Padding(4, 3, 4, 3);
            panelGlobalActions.Name = "panelGlobalActions";
            panelGlobalActions.Size = new Size(1376, 58);
            panelGlobalActions.TabIndex = 0;
            // 
            // btnManageTasks
            // 
            btnManageTasks.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnManageTasks.Location = new Point(1194, 12);
            btnManageTasks.Margin = new Padding(4, 3, 4, 3);
            btnManageTasks.Name = "btnManageTasks";
            btnManageTasks.Size = new Size(168, 35);
            btnManageTasks.TabIndex = 0;
            btnManageTasks.Values.DropDownArrowColor = Color.Empty;
            btnManageTasks.Values.Text = "Gérer les Tâches";
            // 
            // PrerequisMetierView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(kryptonPanelMain);
            Margin = new Padding(4, 3, 4, 3);
            Name = "PrerequisMetierView";
            Size = new Size(1400, 808);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).EndInit();
            kryptonPanelMain.ResumeLayout(false);
            (kryptonSplitContainer1.Panel1).EndInit();
            kryptonSplitContainer1.Panel1.ResumeLayout(false);
            (kryptonSplitContainer1.Panel2).EndInit();
            kryptonSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)navigatorPhases).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).EndInit();
            groupDetails.Panel.ResumeLayout(false);
            groupDetails.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupDetails).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelColor).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).EndInit();
            panelGlobalActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonPanel panelGlobalActions;
        private KryptonButton btnManageTasks;
        private KryptonSplitContainer kryptonSplitContainer1;
        private Krypton.Navigator.KryptonNavigator navigatorPhases;
        private KryptonGroupBox groupDetails;
        private KryptonLabel labelId;
        private KryptonTextBox textId;
        private KryptonTextBox textName;
        private KryptonLabel labelName;
        private KryptonPanel panelColor;
        private KryptonLabel labelColor;
        private KryptonCheckedListBox checkedListPrerequis;
    }
}