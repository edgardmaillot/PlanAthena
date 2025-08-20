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
            this.kryptonPanelMain = new Krypton.Toolkit.KryptonPanel();
            this.kryptonSplitContainer1 = new Krypton.Toolkit.KryptonSplitContainer();
            this.headerMetiers = new Krypton.Toolkit.KryptonHeaderGroup();
            this.panelMetierActions = new Krypton.Toolkit.KryptonPanel();
            this.btnDeleteMetier = new Krypton.Toolkit.KryptonButton();
            this.btnNewMetier = new Krypton.Toolkit.KryptonButton();
            this.gridMetiers = new Krypton.Toolkit.KryptonDataGridView();
            this.groupDetails = new Krypton.Toolkit.KryptonGroupBox();
            this.groupPhases = new Krypton.Toolkit.KryptonGroupBox();
            this.chkFinition = new Krypton.Toolkit.KryptonCheckBox();
            this.chkSecondOeuvre = new Krypton.Toolkit.KryptonCheckBox();
            this.chkGrosOeuvre = new Krypton.Toolkit.KryptonCheckBox();
            this.textPictogram = new Krypton.Toolkit.KryptonTextBox();
            this.labelPictogram = new Krypton.Toolkit.KryptonLabel();
            this.btnChooseColor = new Krypton.Toolkit.KryptonButton();
            this.panelColor = new Krypton.Toolkit.KryptonPanel();
            this.labelColor = new Krypton.Toolkit.KryptonLabel();
            this.textName = new Krypton.Toolkit.KryptonTextBox();
            this.labelName = new Krypton.Toolkit.KryptonLabel();
            this.textId = new Krypton.Toolkit.KryptonTextBox();
            this.labelId = new Krypton.Toolkit.KryptonLabel();
            this.panelGlobalActions = new Krypton.Toolkit.KryptonPanel();
            this.btnConfigurePrerequis = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanelMain)).BeginInit();
            this.kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel1)).BeginInit();
            this.kryptonSplitContainer1.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel2)).BeginInit();
            this.kryptonSplitContainer1.Panel2.SuspendLayout();
            this.kryptonSplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerMetiers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.headerMetiers.Panel)).BeginInit();
            this.headerMetiers.Panel.SuspendLayout();
            this.headerMetiers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelMetierActions)).BeginInit();
            this.panelMetierActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridMetiers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupDetails)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupDetails.Panel)).BeginInit();
            this.groupDetails.Panel.SuspendLayout();
            this.groupDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupPhases)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupPhases.Panel)).BeginInit();
            this.groupPhases.Panel.SuspendLayout();
            this.groupPhases.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelGlobalActions)).BeginInit();
            this.panelGlobalActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // kryptonPanelMain
            // 
            this.kryptonPanelMain.Controls.Add(this.kryptonSplitContainer1);
            this.kryptonPanelMain.Controls.Add(this.panelGlobalActions);
            this.kryptonPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonPanelMain.Location = new System.Drawing.Point(0, 0);
            this.kryptonPanelMain.Name = "kryptonPanelMain";
            this.kryptonPanelMain.Padding = new System.Windows.Forms.Padding(10);
            this.kryptonPanelMain.Size = new System.Drawing.Size(1200, 700);
            this.kryptonPanelMain.TabIndex = 0;
            // 
            // kryptonSplitContainer1
            // 
            this.kryptonSplitContainer1.Cursor = System.Windows.Forms.Cursors.Default;
            this.kryptonSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonSplitContainer1.Location = new System.Drawing.Point(10, 10);
            this.kryptonSplitContainer1.Name = "kryptonSplitContainer1";
            // 
            // kryptonSplitContainer1.Panel1
            // 
            this.kryptonSplitContainer1.Panel1.Controls.Add(this.headerMetiers);
            // 
            // kryptonSplitContainer1.Panel2
            // 
            this.kryptonSplitContainer1.Panel2.Controls.Add(this.groupDetails);
            this.kryptonSplitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.kryptonSplitContainer1.Size = new System.Drawing.Size(1180, 630);
            this.kryptonSplitContainer1.SplitterDistance = 450;
            this.kryptonSplitContainer1.TabIndex = 1;
            // 
            // headerMetiers
            // 
            this.headerMetiers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerMetiers.HeaderVisibleSecondary = false;
            this.headerMetiers.Location = new System.Drawing.Point(0, 0);
            this.headerMetiers.Name = "headerMetiers";
            this.headerMetiers.Panel.Controls.Add(this.panelMetierActions);
            this.headerMetiers.Panel.Controls.Add(this.gridMetiers);
            this.headerMetiers.Panel.Padding = new System.Windows.Forms.Padding(5);
            this.headerMetiers.Size = new System.Drawing.Size(450, 630);
            this.headerMetiers.TabIndex = 0;
            this.headerMetiers.ValuesPrimary.Heading = "Liste des Métiers";
            this.headerMetiers.ValuesPrimary.Image = null;
            // 
            // panelMetierActions
            // 
            this.panelMetierActions.Controls.Add(this.btnDeleteMetier);
            this.panelMetierActions.Controls.Add(this.btnNewMetier);
            this.panelMetierActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelMetierActions.Location = new System.Drawing.Point(5, 549);
            this.panelMetierActions.Name = "panelMetierActions";
            this.panelMetierActions.Size = new System.Drawing.Size(438, 50);
            this.panelMetierActions.TabIndex = 1;
            // 
            // btnDeleteMetier
            // 
            this.btnDeleteMetier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteMetier.Location = new System.Drawing.Point(336, 10);
            this.btnDeleteMetier.Name = "btnDeleteMetier";
            this.btnDeleteMetier.Size = new System.Drawing.Size(90, 30);
            this.btnDeleteMetier.TabIndex = 1;
            this.btnDeleteMetier.Values.Text = "Supprimer";
            // 
            // btnNewMetier
            // 
            this.btnNewMetier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewMetier.Location = new System.Drawing.Point(240, 10);
            this.btnNewMetier.Name = "btnNewMetier";
            this.btnNewMetier.Size = new System.Drawing.Size(90, 30);
            this.btnNewMetier.TabIndex = 0;
            this.btnNewMetier.Values.Text = "Nouveau";
            // 
            // gridMetiers
            // 
            this.gridMetiers.AllowUserToAddRows = false;
            this.gridMetiers.AllowUserToDeleteRows = false;
            this.gridMetiers.AllowUserToResizeRows = false;
            this.gridMetiers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridMetiers.Location = new System.Drawing.Point(5, 5);
            this.gridMetiers.MultiSelect = false;
            this.gridMetiers.Name = "gridMetiers";
            this.gridMetiers.ReadOnly = true;
            this.gridMetiers.RowHeadersVisible = false;
            this.gridMetiers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridMetiers.Size = new System.Drawing.Size(438, 544);
            this.gridMetiers.TabIndex = 0;
            // 
            // groupDetails
            // 
            this.groupDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupDetails.Location = new System.Drawing.Point(10, 0);
            this.groupDetails.Name = "groupDetails";
            this.groupDetails.Panel.Controls.Add(this.groupPhases);
            this.groupDetails.Panel.Controls.Add(this.textPictogram);
            this.groupDetails.Panel.Controls.Add(this.labelPictogram);
            this.groupDetails.Panel.Controls.Add(this.btnChooseColor);
            this.groupDetails.Panel.Controls.Add(this.panelColor);
            this.groupDetails.Panel.Controls.Add(this.labelColor);
            this.groupDetails.Panel.Controls.Add(this.textName);
            this.groupDetails.Panel.Controls.Add(this.labelName);
            this.groupDetails.Panel.Controls.Add(this.textId);
            this.groupDetails.Panel.Controls.Add(this.labelId);
            this.groupDetails.Size = new System.Drawing.Size(716, 630);
            this.groupDetails.TabIndex = 0;
            this.groupDetails.Values.Heading = "Détails du Métier";
            // 
            // groupPhases
            // 
            this.groupPhases.Location = new System.Drawing.Point(13, 149);
            this.groupPhases.Name = "groupPhases";
            this.groupPhases.Panel.Controls.Add(this.chkFinition);
            this.groupPhases.Panel.Controls.Add(this.chkSecondOeuvre);
            this.groupPhases.Panel.Controls.Add(this.chkGrosOeuvre);
            this.groupPhases.Size = new System.Drawing.Size(350, 80);
            this.groupPhases.TabIndex = 9;
            this.groupPhases.Values.Heading = "Phases de chantier";
            // 
            // chkFinition
            // 
            this.chkFinition.Location = new System.Drawing.Point(240, 15);
            this.chkFinition.Name = "chkFinition";
            this.chkFinition.Size = new System.Drawing.Size(64, 20);
            this.chkFinition.TabIndex = 2;
            this.chkFinition.Values.Text = "Finition";
            // 
            // chkSecondOeuvre
            // 
            this.chkSecondOeuvre.Location = new System.Drawing.Point(115, 15);
            this.chkSecondOeuvre.Name = "chkSecondOeuvre";
            this.chkSecondOeuvre.Size = new System.Drawing.Size(107, 20);
            this.chkSecondOeuvre.TabIndex = 1;
            this.chkSecondOeuvre.Values.Text = "Second Œuvre";
            // 
            // chkGrosOeuvre
            // 
            this.chkGrosOeuvre.Location = new System.Drawing.Point(15, 15);
            this.chkGrosOeuvre.Name = "chkGrosOeuvre";
            this.chkGrosOeuvre.Size = new System.Drawing.Size(88, 20);
            this.chkGrosOeuvre.TabIndex = 0;
            this.chkGrosOeuvre.Values.Text = "Gros Œuvre";
            // 
            // textPictogram
            // 
            this.textPictogram.Location = new System.Drawing.Point(130, 105);
            this.textPictogram.Name = "textPictogram";
            this.textPictogram.Size = new System.Drawing.Size(233, 23);
            this.textPictogram.TabIndex = 8;
            // 
            // labelPictogram
            // 
            this.labelPictogram.Location = new System.Drawing.Point(13, 105);
            this.labelPictogram.Name = "labelPictogram";
            this.labelPictogram.Size = new System.Drawing.Size(73, 20);
            this.labelPictogram.TabIndex = 7;
            this.labelPictogram.Values.Text = "Pictogram :";
            // 
            // btnChooseColor
            // 
            this.btnChooseColor.Location = new System.Drawing.Point(263, 69);
            this.btnChooseColor.Name = "btnChooseColor";
            this.btnChooseColor.Size = new System.Drawing.Size(100, 25);
            this.btnChooseColor.TabIndex = 6;
            this.btnChooseColor.Values.Text = "Choisir...";
            // 
            // panelColor
            // 
            this.panelColor.Location = new System.Drawing.Point(130, 70);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(127, 23);
            this.panelColor.StateCommon.Color1 = System.Drawing.Color.Gainsboro;
            this.panelColor.TabIndex = 5;
            // 
            // labelColor
            // 
            this.labelColor.Location = new System.Drawing.Point(13, 70);
            this.labelColor.Name = "labelColor";
            this.labelColor.Size = new System.Drawing.Size(58, 20);
            this.labelColor.TabIndex = 4;
            this.labelColor.Values.Text = "Couleur :";
            // 
            // textName
            // 
            this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textName.Location = new System.Drawing.Point(130, 35);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(572, 23);
            this.textName.TabIndex = 3;
            // 
            // labelName
            // 
            this.labelName.Location = new System.Drawing.Point(13, 35);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(44, 20);
            this.labelName.TabIndex = 2;
            this.labelName.Values.Text = "Nom :";
            // 
            // textId
            // 
            this.textId.Location = new System.Drawing.Point(130, 0);
            this.textId.Name = "textId";
            this.textId.ReadOnly = true;
            this.textId.Size = new System.Drawing.Size(233, 23);
            this.textId.TabIndex = 1;
            // 
            // labelId
            // 
            this.labelId.Location = new System.Drawing.Point(13, 0);
            this.labelId.Name = "labelId";
            this.labelId.Size = new System.Drawing.Size(28, 20);
            this.labelId.TabIndex = 0;
            this.labelId.Values.Text = "ID :";
            // 
            // panelGlobalActions
            // 
            this.panelGlobalActions.Controls.Add(this.btnConfigurePrerequis);
            this.panelGlobalActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGlobalActions.Location = new System.Drawing.Point(10, 640);
            this.panelGlobalActions.Name = "panelGlobalActions";
            this.panelGlobalActions.Size = new System.Drawing.Size(1180, 50);
            this.panelGlobalActions.TabIndex = 0;
            // 
            // btnConfigurePrerequis
            // 
            this.btnConfigurePrerequis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfigurePrerequis.Location = new System.Drawing.Point(994, 10);
            this.btnConfigurePrerequis.Name = "btnConfigurePrerequis";
            this.btnConfigurePrerequis.Size = new System.Drawing.Size(174, 30);
            this.btnConfigurePrerequis.TabIndex = 0;
            this.btnConfigurePrerequis.Values.Text = "Configurer les Prérequis";
            this.btnConfigurePrerequis.Click += new System.EventHandler(this.BtnConfigurePrerequis_Click);
            // 
            // RessourceMetierView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.kryptonPanelMain);
            this.Name = "RessourceMetierView";
            this.Size = new System.Drawing.Size(1200, 700);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanelMain)).EndInit();
            this.kryptonPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel1)).EndInit();
            this.kryptonSplitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1.Panel2)).EndInit();
            this.kryptonSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonSplitContainer1)).EndInit();
            this.kryptonSplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.headerMetiers.Panel)).EndInit();
            this.headerMetiers.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.headerMetiers)).EndInit();
            this.headerMetiers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelMetierActions)).EndInit();
            this.panelMetierActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridMetiers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupDetails.Panel)).EndInit();
            this.groupDetails.Panel.ResumeLayout(false);
            this.groupDetails.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupDetails)).EndInit();
            this.groupDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupPhases.Panel)).EndInit();
            this.groupPhases.Panel.ResumeLayout(false);
            this.groupPhases.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupPhases)).EndInit();
            this.groupPhases.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelGlobalActions)).EndInit();
            this.panelGlobalActions.ResumeLayout(false);
            this.ResumeLayout(false);
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
    }
}