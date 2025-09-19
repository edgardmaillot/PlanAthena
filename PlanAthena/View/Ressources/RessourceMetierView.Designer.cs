using Krypton.Toolkit;
using PlanAthena.View;
using PlanAthena.View.TaskManager;
using PlanAthena.View.TaskManager.PertDiagram;

namespace PlanAthena.View.Ressources
{
    partial class RessourceMetierView
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
            headerMetiers = new KryptonHeaderGroup();
            panelMetierActions = new KryptonPanel();
            btnDeleteMetier = new KryptonButton();
            btnNewMetier = new KryptonButton();
            gridMetiers = new KryptonDataGridView();
            ID = new DataGridViewTextBoxColumn();
            Nom = new DataGridViewTextBoxColumn();
            Phases = new DataGridViewTextBoxColumn();
            groupDetails = new KryptonGroupBox();
            groupPhases = new KryptonGroupBox();
            chkFinition = new KryptonCheckBox();
            chkSecondOeuvre = new KryptonCheckBox();
            chkGrosOeuvre = new KryptonCheckBox();
            groupPictogram = new KryptonGroupBox();
            flowPictogramGrid = new FlowLayoutPanel();
            comboPictogramCategory = new KryptonComboBox();
            labelCategory = new KryptonLabel();
            btnChoosePictogram = new KryptonButton();
            panelPictogramPreview = new KryptonPanel();
            lblPictogramPreview = new KryptonLabel();
            btnChooseColor = new KryptonButton();
            panelColor = new KryptonPanel();
            labelColor = new KryptonLabel();
            textName = new KryptonTextBox();
            labelName = new KryptonLabel();
            textId = new KryptonTextBox();
            labelId = new KryptonLabel();
            panelGlobalActions = new KryptonPanel();
            btnConfigurePrerequis = new KryptonButton();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).BeginInit();
            (kryptonSplitContainer1.Panel1).BeginInit();
            kryptonSplitContainer1.Panel1.SuspendLayout();
            (kryptonSplitContainer1.Panel2).BeginInit();
            kryptonSplitContainer1.Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)headerMetiers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerMetiers.Panel).BeginInit();
            headerMetiers.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelMetierActions).BeginInit();
            panelMetierActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridMetiers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).BeginInit();
            groupDetails.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupPhases).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupPhases.Panel).BeginInit();
            groupPhases.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupPictogram).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupPictogram.Panel).BeginInit();
            groupPictogram.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)comboPictogramCategory).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelPictogramPreview).BeginInit();
            panelPictogramPreview.SuspendLayout();
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
            kryptonPanelMain.Padding = new Padding(12);
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
            kryptonSplitContainer1.Panel1.Controls.Add(headerMetiers);
            // 
            // 
            // 
            kryptonSplitContainer1.Panel2.Controls.Add(groupDetails);
            kryptonSplitContainer1.Panel2.Padding = new Padding(12, 0, 0, 0);
            kryptonSplitContainer1.Size = new Size(1376, 726);
            kryptonSplitContainer1.SplitterDistance = 524;
            kryptonSplitContainer1.TabIndex = 1;
            // 
            // headerMetiers
            // 
            headerMetiers.Dock = DockStyle.Fill;
            headerMetiers.HeaderVisibleSecondary = false;
            headerMetiers.Location = new Point(0, 0);
            headerMetiers.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            headerMetiers.Panel.Controls.Add(panelMetierActions);
            headerMetiers.Panel.Controls.Add(gridMetiers);
            headerMetiers.Panel.Padding = new Padding(6);
            headerMetiers.Size = new Size(524, 726);
            headerMetiers.TabIndex = 0;
            headerMetiers.ValuesPrimary.Heading = "Liste des Métiers";
            headerMetiers.ValuesPrimary.Image = null;
            // 
            // panelMetierActions
            // 
            panelMetierActions.Controls.Add(btnDeleteMetier);
            panelMetierActions.Controls.Add(btnNewMetier);
            panelMetierActions.Dock = DockStyle.Bottom;
            panelMetierActions.Location = new Point(6, 630);
            panelMetierActions.Margin = new Padding(4, 3, 4, 3);
            panelMetierActions.Name = "panelMetierActions";
            panelMetierActions.Size = new Size(510, 58);
            panelMetierActions.TabIndex = 1;
            // 
            // btnDeleteMetier
            // 
            btnDeleteMetier.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeleteMetier.Location = new Point(391, 12);
            btnDeleteMetier.Margin = new Padding(4, 3, 4, 3);
            btnDeleteMetier.Name = "btnDeleteMetier";
            btnDeleteMetier.Size = new Size(105, 35);
            btnDeleteMetier.TabIndex = 1;
            btnDeleteMetier.Values.DropDownArrowColor = Color.Empty;
            btnDeleteMetier.Values.Text = "Supprimer";
            // 
            // btnNewMetier
            // 
            btnNewMetier.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNewMetier.Location = new Point(279, 12);
            btnNewMetier.Margin = new Padding(4, 3, 4, 3);
            btnNewMetier.Name = "btnNewMetier";
            btnNewMetier.Size = new Size(105, 35);
            btnNewMetier.TabIndex = 0;
            btnNewMetier.Values.DropDownArrowColor = Color.Empty;
            btnNewMetier.Values.Text = "Nouveau";
            // 
            // gridMetiers
            // 
            gridMetiers.AllowUserToAddRows = false;
            gridMetiers.AllowUserToDeleteRows = false;
            gridMetiers.AllowUserToResizeRows = false;
            gridMetiers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridMetiers.BorderStyle = BorderStyle.None;
            gridMetiers.Columns.AddRange(new DataGridViewColumn[] { ID, Nom, Phases });
            gridMetiers.Dock = DockStyle.Fill;
            gridMetiers.Location = new Point(6, 6);
            gridMetiers.Margin = new Padding(4, 3, 4, 3);
            gridMetiers.MultiSelect = false;
            gridMetiers.Name = "gridMetiers";
            gridMetiers.ReadOnly = true;
            gridMetiers.RowHeadersVisible = false;
            gridMetiers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridMetiers.Size = new Size(510, 682);
            gridMetiers.StateCommon.BackStyle = PaletteBackStyle.GridBackgroundList;
            gridMetiers.StateCommon.HeaderColumn.Content.ColorAlign = PaletteRectangleAlign.Control;
            gridMetiers.StateCommon.HeaderColumn.Content.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            gridMetiers.StateCommon.HeaderColumn.Content.Hint = PaletteTextHint.AntiAlias;
            gridMetiers.TabIndex = 0;
            // 
            // ID
            // 
            ID.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            ID.HeaderText = "Identifiant métier";
            ID.MinimumWidth = 80;
            ID.Name = "ID";
            ID.ReadOnly = true;
            ID.Width = 127;
            // 
            // Nom
            // 
            Nom.HeaderText = "Nom";
            Nom.MinimumWidth = 120;
            Nom.Name = "Nom";
            Nom.ReadOnly = true;
            // 
            // Phases
            // 
            Phases.HeaderText = "Phases";
            Phases.MinimumWidth = 150;
            Phases.Name = "Phases";
            Phases.ReadOnly = true;
            // 
            // groupDetails
            // 
            groupDetails.Dock = DockStyle.Fill;
            groupDetails.Location = new Point(12, 0);
            groupDetails.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupDetails.Panel.Controls.Add(groupPhases);
            groupDetails.Panel.Controls.Add(groupPictogram);
            groupDetails.Panel.Controls.Add(btnChooseColor);
            groupDetails.Panel.Controls.Add(panelColor);
            groupDetails.Panel.Controls.Add(labelColor);
            groupDetails.Panel.Controls.Add(textName);
            groupDetails.Panel.Controls.Add(labelName);
            groupDetails.Panel.Controls.Add(textId);
            groupDetails.Panel.Controls.Add(labelId);
            groupDetails.Size = new Size(835, 726);
            groupDetails.TabIndex = 0;
            groupDetails.Values.Heading = "Détails du Métier";
            // 
            // groupPhases
            // 
            groupPhases.Location = new Point(15, 172);
            groupPhases.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupPhases.Panel.Controls.Add(chkFinition);
            groupPhases.Panel.Controls.Add(chkSecondOeuvre);
            groupPhases.Panel.Controls.Add(chkGrosOeuvre);
            groupPhases.Size = new Size(408, 92);
            groupPhases.TabIndex = 9;
            groupPhases.Values.Heading = "Phases de chantier";
            // 
            // chkFinition
            // 
            chkFinition.Location = new Point(280, 17);
            chkFinition.Margin = new Padding(4, 3, 4, 3);
            chkFinition.Name = "chkFinition";
            chkFinition.Size = new Size(64, 20);
            chkFinition.TabIndex = 2;
            chkFinition.Values.Text = "Finition";
            // 
            // chkSecondOeuvre
            // 
            chkSecondOeuvre.Location = new Point(134, 17);
            chkSecondOeuvre.Margin = new Padding(4, 3, 4, 3);
            chkSecondOeuvre.Name = "chkSecondOeuvre";
            chkSecondOeuvre.Size = new Size(102, 20);
            chkSecondOeuvre.TabIndex = 1;
            chkSecondOeuvre.Values.Text = "Second Œuvre";
            // 
            // chkGrosOeuvre
            // 
            chkGrosOeuvre.Location = new Point(18, 17);
            chkGrosOeuvre.Margin = new Padding(4, 3, 4, 3);
            chkGrosOeuvre.Name = "chkGrosOeuvre";
            chkGrosOeuvre.Size = new Size(87, 20);
            chkGrosOeuvre.TabIndex = 0;
            chkGrosOeuvre.Values.Text = "Gros Œuvre";
            // 
            // groupPictogram
            // 
            groupPictogram.Location = new Point(15, 280);
            groupPictogram.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupPictogram.Panel.Controls.Add(flowPictogramGrid);
            groupPictogram.Panel.Controls.Add(comboPictogramCategory);
            groupPictogram.Panel.Controls.Add(labelCategory);
            groupPictogram.Panel.Controls.Add(btnChoosePictogram);
            groupPictogram.Panel.Controls.Add(panelPictogramPreview);
            groupPictogram.Size = new Size(408, 306);
            groupPictogram.TabIndex = 10;
            groupPictogram.Values.Heading = "Pictogramme";
            // 
            // flowPictogramGrid
            // 
            flowPictogramGrid.AutoScroll = true;
            flowPictogramGrid.BorderStyle = BorderStyle.FixedSingle;
            flowPictogramGrid.Location = new Point(15, 135);
            flowPictogramGrid.Name = "flowPictogramGrid";
            flowPictogramGrid.Size = new Size(370, 130);
            flowPictogramGrid.TabIndex = 5;
            // 
            // comboPictogramCategory
            // 
            comboPictogramCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPictogramCategory.Location = new Point(90, 105);
            comboPictogramCategory.Name = "comboPictogramCategory";
            comboPictogramCategory.Size = new Size(200, 22);
            comboPictogramCategory.TabIndex = 4;
            comboPictogramCategory.SelectedIndexChanged += ComboPictogramCategory_SelectedIndexChanged;
            // 
            // labelCategory
            // 
            labelCategory.Location = new Point(15, 105);
            labelCategory.Name = "labelCategory";
            labelCategory.Size = new Size(69, 20);
            labelCategory.TabIndex = 3;
            labelCategory.Values.Text = "Catégorie :";
            // 
            // btnChoosePictogram
            // 
            btnChoosePictogram.Location = new Point(110, 25);
            btnChoosePictogram.Name = "btnChoosePictogram";
            btnChoosePictogram.Size = new Size(100, 30);
            btnChoosePictogram.TabIndex = 2;
            btnChoosePictogram.Values.DropDownArrowColor = Color.Empty;
            btnChoosePictogram.Values.Text = "Effacer";
            btnChoosePictogram.Click += BtnClearPictogram_Click;
            // 
            // panelPictogramPreview
            // 
            panelPictogramPreview.Controls.Add(lblPictogramPreview);
            panelPictogramPreview.Location = new Point(15, 10);
            panelPictogramPreview.Name = "panelPictogramPreview";
            panelPictogramPreview.Size = new Size(80, 80);
            panelPictogramPreview.StateCommon.Color1 = Color.FromArgb(64, 64, 64);
            panelPictogramPreview.StateCommon.ColorStyle = PaletteColorStyle.SolidAllLine;
            panelPictogramPreview.TabIndex = 0;
            // 
            // lblPictogramPreview
            // 
            lblPictogramPreview.Location = new Point(0, 0);
            lblPictogramPreview.Name = "lblPictogramPreview";
            lblPictogramPreview.Size = new Size(76, 76);
            lblPictogramPreview.StateNormal.ShortText.ColorAlign = PaletteRectangleAlign.Form;
            lblPictogramPreview.StateNormal.ShortText.ColorStyle = PaletteColorStyle.SolidAllLine;
            lblPictogramPreview.StateNormal.ShortText.Font = new Font("Segoe UI Symbol", 38F);
            lblPictogramPreview.TabIndex = 1;
            lblPictogramPreview.Values.Text = "🏠";
            // 
            // btnChooseColor
            // 
            btnChooseColor.Location = new Point(307, 80);
            btnChooseColor.Margin = new Padding(4, 3, 4, 3);
            btnChooseColor.Name = "btnChooseColor";
            btnChooseColor.Size = new Size(117, 29);
            btnChooseColor.TabIndex = 6;
            btnChooseColor.Values.DropDownArrowColor = Color.Empty;
            btnChooseColor.Values.Text = "Choisir...";
            // 
            // panelColor
            // 
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
            textName.Size = new Size(667, 23);
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
            textId.Location = new Point(152, 0);
            textId.Margin = new Padding(4, 3, 4, 3);
            textId.Name = "textId";
            textId.ReadOnly = true;
            textId.Size = new Size(272, 23);
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
            panelGlobalActions.Controls.Add(btnConfigurePrerequis);
            panelGlobalActions.Dock = DockStyle.Bottom;
            panelGlobalActions.Location = new Point(12, 738);
            panelGlobalActions.Margin = new Padding(4, 3, 4, 3);
            panelGlobalActions.Name = "panelGlobalActions";
            panelGlobalActions.Size = new Size(1376, 58);
            panelGlobalActions.TabIndex = 0;
            // 
            // btnConfigurePrerequis
            // 
            btnConfigurePrerequis.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfigurePrerequis.Location = new Point(1159, 12);
            btnConfigurePrerequis.Margin = new Padding(4, 3, 4, 3);
            btnConfigurePrerequis.Name = "btnConfigurePrerequis";
            btnConfigurePrerequis.Size = new Size(203, 35);
            btnConfigurePrerequis.TabIndex = 0;
            btnConfigurePrerequis.Values.DropDownArrowColor = Color.Empty;
            btnConfigurePrerequis.Values.Text = "Configurer les Prérequis";
            btnConfigurePrerequis.Click += BtnConfigurePrerequis_Click;
            // 
            // RessourceMetierView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(kryptonPanelMain);
            Margin = new Padding(4, 3, 4, 3);
            Name = "RessourceMetierView";
            Size = new Size(1400, 808);
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).EndInit();
            kryptonPanelMain.ResumeLayout(false);
            (kryptonSplitContainer1.Panel1).EndInit();
            kryptonSplitContainer1.Panel1.ResumeLayout(false);
            (kryptonSplitContainer1.Panel2).EndInit();
            kryptonSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerMetiers.Panel).EndInit();
            headerMetiers.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)headerMetiers).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelMetierActions).EndInit();
            panelMetierActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridMetiers).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).EndInit();
            groupDetails.Panel.ResumeLayout(false);
            groupDetails.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupDetails).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupPhases.Panel).EndInit();
            groupPhases.Panel.ResumeLayout(false);
            groupPhases.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupPhases).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupPictogram.Panel).EndInit();
            groupPictogram.Panel.ResumeLayout(false);
            groupPictogram.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupPictogram).EndInit();
            ((System.ComponentModel.ISupportInitialize)comboPictogramCategory).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelPictogramPreview).EndInit();
            panelPictogramPreview.ResumeLayout(false);
            panelPictogramPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)panelColor).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).EndInit();
            panelGlobalActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonPanel panelGlobalActions;
        private KryptonButton btnConfigurePrerequis;
        private KryptonSplitContainer kryptonSplitContainer1;
        private KryptonHeaderGroup headerMetiers;
        private KryptonDataGridView gridMetiers;
        private KryptonPanel panelMetierActions;
        private KryptonButton btnDeleteMetier;
        private KryptonButton btnNewMetier;
        private KryptonGroupBox groupDetails;
        private KryptonTextBox textId;
        private KryptonLabel labelId;
        private KryptonTextBox textName;
        private KryptonLabel labelName;
        private KryptonLabel labelColor;
        private KryptonPanel panelColor;
        private KryptonButton btnChooseColor;
        private KryptonTextBox textPictogram;
        private KryptonLabel labelPictogram;
        private KryptonGroupBox groupPhases;
        private KryptonCheckBox chkFinition;
        private KryptonCheckBox chkSecondOeuvre;
        private KryptonCheckBox chkGrosOeuvre;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn Nom;
        private DataGridViewTextBoxColumn Phases;

        private KryptonGroupBox groupPictogram;
        private KryptonPanel panelPictogramPreview;
        private KryptonLabel lblPictogramPreview;
        private KryptonButton btnChoosePictogram;
        private KryptonComboBox comboPictogramCategory;
        private KryptonLabel labelCategory;
        private FlowLayoutPanel flowPictogramGrid;
    }
}