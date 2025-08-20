using Krypton.Toolkit;

namespace PlanAthena.View.Ressources.MetierDiagram
{
    partial class RessourceOuvrierView
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
            headerOuvriers = new KryptonHeaderGroup();
            panelOuvrierActions = new KryptonPanel();
            btnDeleteOuvrier = new KryptonButton();
            btnNewOuvrier = new KryptonButton();
            gridOuvriers = new KryptonDataGridView();
            textSearchOuvrier = new KryptonTextBox();
            panelRightContainer = new KryptonPanel();
            groupDetails = new KryptonGroupBox();
            numCoutJournalier = new KryptonNumericUpDown();
            labelCout = new KryptonLabel();
            textPrenom = new KryptonTextBox();
            labelPrenom = new KryptonLabel();
            textNom = new KryptonTextBox();
            labelNom = new KryptonLabel();
            textId = new KryptonTextBox();
            labelId = new KryptonLabel();
            groupCompetences = new KryptonGroupBox();
            panelCompetenceActions = new KryptonPanel();
            btnDeleteCompetence = new KryptonButton();
            btnModifyCompetence = new KryptonButton();
            btnAddCompetence = new KryptonButton();
            gridCompetences = new KryptonDataGridView();
            panelGlobalActions = new KryptonPanel();
            btnEnregistrer = new KryptonButton();
            btnSupprimer = new KryptonButton();
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).BeginInit();
            kryptonPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).BeginInit();
            (kryptonSplitContainer1.Panel1).BeginInit();
            kryptonSplitContainer1.Panel1.SuspendLayout();
            (kryptonSplitContainer1.Panel2).BeginInit();
            kryptonSplitContainer1.Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)headerOuvriers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerOuvriers.Panel).BeginInit();
            headerOuvriers.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelOuvrierActions).BeginInit();
            panelOuvrierActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridOuvriers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).BeginInit();
            groupDetails.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupCompetences).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupCompetences.Panel).BeginInit();
            groupCompetences.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelCompetenceActions).BeginInit();
            panelCompetenceActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridCompetences).BeginInit();
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
            kryptonSplitContainer1.Panel1.Controls.Add(headerOuvriers);
            // 
            // 
            // 
            this.kryptonSplitContainer1.Panel2.Controls.Add(this.panelRightContainer); 
            this.kryptonSplitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.kryptonSplitContainer1.Size = new System.Drawing.Size(1180, 630);
            this.kryptonSplitContainer1.SplitterDistance = 450;
            this.kryptonSplitContainer1.TabIndex = 1;
            // 
            // headerOuvriers
            // 
            headerOuvriers.Dock = DockStyle.Fill;
            headerOuvriers.HeaderVisibleSecondary = false;
            headerOuvriers.Location = new Point(0, 0);
            headerOuvriers.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            headerOuvriers.Panel.Controls.Add(panelOuvrierActions);
            headerOuvriers.Panel.Controls.Add(gridOuvriers);
            headerOuvriers.Panel.Controls.Add(textSearchOuvrier);
            headerOuvriers.Panel.Padding = new Padding(6, 6, 6, 6);
            headerOuvriers.Size = new Size(524, 726);
            headerOuvriers.StateCommon.Back.Color1 = Color.White;
            headerOuvriers.StateCommon.Back.Color2 = Color.White;
            headerOuvriers.TabIndex = 0;
            headerOuvriers.ValuesPrimary.Heading = "Liste des Ouvriers";
            headerOuvriers.ValuesPrimary.Image = null;
            // 
            // panelOuvrierActions
            // 
            panelOuvrierActions.Controls.Add(btnDeleteOuvrier);
            panelOuvrierActions.Controls.Add(btnNewOuvrier);
            panelOuvrierActions.Dock = DockStyle.Bottom;
            panelOuvrierActions.Location = new Point(6, 630);
            panelOuvrierActions.Margin = new Padding(4, 3, 4, 3);
            panelOuvrierActions.Name = "panelOuvrierActions";
            panelOuvrierActions.Size = new Size(510, 58);
            panelOuvrierActions.TabIndex = 2;
            // 
            // btnDeleteOuvrier
            // 
            btnDeleteOuvrier.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeleteOuvrier.Location = new Point(391, 12);
            btnDeleteOuvrier.Margin = new Padding(4, 3, 4, 3);
            btnDeleteOuvrier.Name = "btnDeleteOuvrier";
            btnDeleteOuvrier.Size = new Size(105, 35);
            btnDeleteOuvrier.TabIndex = 1;
            btnDeleteOuvrier.Values.DropDownArrowColor = Color.Empty;
            btnDeleteOuvrier.Values.Text = "Supprimer";
            btnDeleteOuvrier.Click += btnDeleteOuvrier_Click;
            // 
            // btnNewOuvrier
            // 
            btnNewOuvrier.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNewOuvrier.Location = new Point(279, 12);
            btnNewOuvrier.Margin = new Padding(4, 3, 4, 3);
            btnNewOuvrier.Name = "btnNewOuvrier";
            btnNewOuvrier.Size = new Size(105, 35);
            btnNewOuvrier.TabIndex = 0;
            btnNewOuvrier.Values.DropDownArrowColor = Color.Empty;
            btnNewOuvrier.Values.Text = "Nouveau";
            btnNewOuvrier.Click += btnNewOuvrier_Click;
            // 
            // gridOuvriers
            // 
            gridOuvriers.AllowUserToAddRows = false;
            gridOuvriers.AllowUserToDeleteRows = false;
            gridOuvriers.AllowUserToResizeRows = false;
            gridOuvriers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridOuvriers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridOuvriers.BorderStyle = BorderStyle.None;
            gridOuvriers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridOuvriers.Location = new Point(6, 39);
            gridOuvriers.Margin = new Padding(4, 3, 4, 3);
            gridOuvriers.MultiSelect = false;
            gridOuvriers.Name = "gridOuvriers";
            gridOuvriers.ReadOnly = true;
            gridOuvriers.RowHeadersVisible = false;
            gridOuvriers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridOuvriers.Size = new Size(510, 591);
            gridOuvriers.TabIndex = 1;
            gridOuvriers.SelectionChanged += gridOuvriers_SelectionChanged;
            // 
            // textSearchOuvrier
            // 
            textSearchOuvrier.Dock = DockStyle.Top;
            textSearchOuvrier.Location = new Point(6, 6);
            textSearchOuvrier.Margin = new Padding(4, 3, 4, 3);
            textSearchOuvrier.Name = "textSearchOuvrier";
            textSearchOuvrier.Size = new Size(510, 23);
            textSearchOuvrier.TabIndex = 0;
            textSearchOuvrier.TextChanged += textSearchOuvrier_TextChanged;
            // 
            // panelRightContainer
            // 
            this.panelRightContainer.Controls.Add(this.groupCompetences);
            this.panelRightContainer.Controls.Add(this.groupDetails);
            this.panelRightContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRightContainer.Location = new System.Drawing.Point(10, 0);
            this.panelRightContainer.Name = "panelRightContainer";
            this.panelRightContainer.Size = new System.Drawing.Size(716, 630);
            this.panelRightContainer.TabIndex = 0;
            // 
            // groupDetails
            // 
            groupDetails.Dock = DockStyle.Top;
            groupDetails.Location = new Point(12, 0);
            groupDetails.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupDetails.Panel.Controls.Add(numCoutJournalier);
            groupDetails.Panel.Controls.Add(labelCout);
            groupDetails.Panel.Controls.Add(textPrenom);
            groupDetails.Panel.Controls.Add(labelPrenom);
            groupDetails.Panel.Controls.Add(textNom);
            groupDetails.Panel.Controls.Add(labelNom);
            groupDetails.Panel.Controls.Add(textId);
            groupDetails.Panel.Controls.Add(labelId);
            groupDetails.Panel.Padding = new Padding(12, 12, 12, 12);
            groupDetails.Size = new Size(835, 186);
            groupDetails.TabIndex = 0;
            groupDetails.Values.Heading = "Détails de l'Ouvrier";
            // 
            // numCoutJournalier
            // 
            numCoutJournalier.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            numCoutJournalier.Location = new Point(152, 121);
            numCoutJournalier.Margin = new Padding(4, 3, 4, 3);
            numCoutJournalier.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numCoutJournalier.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numCoutJournalier.Name = "numCoutJournalier";
            numCoutJournalier.Size = new Size(140, 22);
            numCoutJournalier.TabIndex = 7;
            numCoutJournalier.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numCoutJournalier.ValueChanged += DetailOuvrier_Changed;
            // 
            // labelCout
            // 
            labelCout.Location = new Point(15, 121);
            labelCout.Margin = new Padding(4, 3, 4, 3);
            labelCout.Name = "labelCout";
            labelCout.Size = new Size(110, 20);
            labelCout.TabIndex = 6;
            labelCout.Values.Text = "Coût Journalier (€)";
            // 
            // textPrenom
            // 
            textPrenom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textPrenom.Location = new Point(152, 81);
            textPrenom.Margin = new Padding(4, 3, 4, 3);
            textPrenom.Name = "textPrenom";
            textPrenom.Size = new Size(668, 23);
            textPrenom.TabIndex = 5;
            textPrenom.TextChanged += DetailOuvrier_Changed;
            // 
            // labelPrenom
            // 
            labelPrenom.Location = new Point(15, 81);
            labelPrenom.Margin = new Padding(4, 3, 4, 3);
            labelPrenom.Name = "labelPrenom";
            labelPrenom.Size = new Size(53, 20);
            labelPrenom.TabIndex = 4;
            labelPrenom.Values.Text = "Prénom";
            // 
            // textNom
            // 
            textNom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textNom.Location = new Point(152, 40);
            textNom.Margin = new Padding(4, 3, 4, 3);
            textNom.Name = "textNom";
            textNom.Size = new Size(668, 23);
            textNom.TabIndex = 3;
            textNom.TextChanged += DetailOuvrier_Changed;
            // 
            // labelNom
            // 
            labelNom.Location = new Point(15, 40);
            labelNom.Margin = new Padding(4, 3, 4, 3);
            labelNom.Name = "labelNom";
            labelNom.Size = new Size(38, 20);
            labelNom.TabIndex = 2;
            labelNom.Values.Text = "Nom";
            // 
            // textId
            // 
            textId.Location = new Point(152, 0);
            textId.Margin = new Padding(4, 3, 4, 3);
            textId.Name = "textId";
            textId.ReadOnly = true;
            textId.Size = new Size(210, 23);
            textId.TabIndex = 1;
            // 
            // labelId
            // 
            labelId.Location = new Point(15, 0);
            labelId.Margin = new Padding(4, 3, 4, 3);
            labelId.Name = "labelId";
            labelId.Size = new Size(22, 20);
            labelId.TabIndex = 0;
            labelId.Values.Text = "ID";
            // 
            // groupCompetences
            // 
            groupCompetences.Dock = DockStyle.Fill;
            groupCompetences.Location = new Point(12, 170);
            groupCompetences.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            groupCompetences.Panel.Controls.Add(panelCompetenceActions);
            groupCompetences.Panel.Controls.Add(gridCompetences);
            groupCompetences.Panel.Padding = new Padding(6, 6, 6, 6);
            groupCompetences.Size = new Size(835, 726);
            groupCompetences.TabIndex = 1;
            groupCompetences.Values.Heading = "Compétences";
            // 
            // panelCompetenceActions
            // 
            panelCompetenceActions.Controls.Add(btnDeleteCompetence);
            panelCompetenceActions.Controls.Add(btnModifyCompetence);
            panelCompetenceActions.Controls.Add(btnAddCompetence);
            panelCompetenceActions.Dock = DockStyle.Bottom;
            panelCompetenceActions.Location = new Point(6, 638);
            panelCompetenceActions.Margin = new Padding(4, 3, 4, 3);
            panelCompetenceActions.Name = "panelCompetenceActions";
            panelCompetenceActions.Size = new Size(819, 58);
            panelCompetenceActions.TabIndex = 1;
            // 
            // btnDeleteCompetence
            // 
            btnDeleteCompetence.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeleteCompetence.Location = new Point(700, 12);
            btnDeleteCompetence.Margin = new Padding(4, 3, 4, 3);
            btnDeleteCompetence.Name = "btnDeleteCompetence";
            btnDeleteCompetence.Size = new Size(105, 35);
            btnDeleteCompetence.TabIndex = 2;
            btnDeleteCompetence.Values.DropDownArrowColor = Color.Empty;
            btnDeleteCompetence.Values.Text = "Supprimer";
            btnDeleteCompetence.Click += btnDeleteCompetence_Click;
            // 
            // btnModifyCompetence
            // 
            btnModifyCompetence.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnModifyCompetence.Location = new Point(588, 12);
            btnModifyCompetence.Margin = new Padding(4, 3, 4, 3);
            btnModifyCompetence.Name = "btnModifyCompetence";
            btnModifyCompetence.Size = new Size(105, 35);
            btnModifyCompetence.TabIndex = 1;
            btnModifyCompetence.Values.DropDownArrowColor = Color.Empty;
            btnModifyCompetence.Values.Text = "Modifier";
            btnModifyCompetence.Click += btnModifyCompetence_Click;
            // 
            // btnAddCompetence
            // 
            btnAddCompetence.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddCompetence.Location = new Point(476, 12);
            btnAddCompetence.Margin = new Padding(4, 3, 4, 3);
            btnAddCompetence.Name = "btnAddCompetence";
            btnAddCompetence.Size = new Size(105, 35);
            btnAddCompetence.TabIndex = 0;
            btnAddCompetence.Values.DropDownArrowColor = Color.Empty;
            btnAddCompetence.Values.Text = "Ajouter";
            btnAddCompetence.Click += btnAddCompetence_Click;
            // 
            // gridCompetences
            // 
            gridCompetences.AllowUserToAddRows = false;
            gridCompetences.AllowUserToDeleteRows = false;
            gridCompetences.AllowUserToResizeRows = false;
            gridCompetences.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridCompetences.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridCompetences.BorderStyle = BorderStyle.None;
            gridCompetences.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridCompetences.Location = new Point(6, 0);
            gridCompetences.Margin = new Padding(4, 3, 4, 3);
            gridCompetences.MultiSelect = false;
            gridCompetences.Name = "gridCompetences";
            gridCompetences.ReadOnly = true;
            gridCompetences.RowHeadersVisible = false;
            gridCompetences.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridCompetences.Size = new Size(821, 612);
            gridCompetences.TabIndex = 0;
            // 
            // panelGlobalActions
            // 
            panelGlobalActions.Controls.Add(btnEnregistrer);
            panelGlobalActions.Controls.Add(btnSupprimer);
            panelGlobalActions.Dock = DockStyle.Bottom;
            panelGlobalActions.Location = new Point(12, 738);
            panelGlobalActions.Margin = new Padding(4, 3, 4, 3);
            panelGlobalActions.Name = "panelGlobalActions";
            panelGlobalActions.Size = new Size(1376, 58);
            panelGlobalActions.TabIndex = 0;
            // 
            // btnEnregistrer
            // 
            btnEnregistrer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEnregistrer.Location = new Point(1257, 12);
            btnEnregistrer.Margin = new Padding(4, 3, 4, 3);
            btnEnregistrer.Name = "btnEnregistrer";
            btnEnregistrer.Size = new Size(105, 35);
            btnEnregistrer.TabIndex = 1;
            btnEnregistrer.Values.DropDownArrowColor = Color.Empty;
            btnEnregistrer.Values.Text = "Enregistrer";
            btnEnregistrer.Click += btnEnregistrer_Click;
            // 
            // btnSupprimer
            // 
            btnSupprimer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSupprimer.Location = new Point(1145, 12);
            btnSupprimer.Margin = new Padding(4, 3, 4, 3);
            btnSupprimer.Name = "btnSupprimer";
            btnSupprimer.Size = new Size(105, 35);
            btnSupprimer.TabIndex = 0;
            btnSupprimer.Values.DropDownArrowColor = Color.Empty;
            btnSupprimer.Values.Text = "Supprimer";
            // 
            // RessourceOuvrierView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(kryptonPanelMain);
            Margin = new Padding(4, 3, 4, 3);
            Name = "RessourceOuvrierView";
            Size = new Size(1400, 808);
            Load += RessourceOuvrierView_Load;
            ((System.ComponentModel.ISupportInitialize)kryptonPanelMain).EndInit();
            kryptonPanelMain.ResumeLayout(false);
            (kryptonSplitContainer1.Panel1).EndInit();
            kryptonSplitContainer1.Panel1.ResumeLayout(false);
            (kryptonSplitContainer1.Panel2).EndInit();
            kryptonSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonSplitContainer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerOuvriers.Panel).EndInit();
            headerOuvriers.Panel.ResumeLayout(false);
            headerOuvriers.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)headerOuvriers).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelOuvrierActions).EndInit();
            panelOuvrierActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridOuvriers).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupDetails.Panel).EndInit();
            groupDetails.Panel.ResumeLayout(false);
            groupDetails.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)groupDetails).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupCompetences.Panel).EndInit();
            groupCompetences.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupCompetences).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelCompetenceActions).EndInit();
            panelCompetenceActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridCompetences).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelGlobalActions).EndInit();
            panelGlobalActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private KryptonPanel kryptonPanelMain;
        private KryptonPanel panelGlobalActions;
        private KryptonButton btnEnregistrer;
        private KryptonButton btnSupprimer;
        private KryptonSplitContainer kryptonSplitContainer1;
        private KryptonHeaderGroup headerOuvriers;
        private KryptonTextBox textSearchOuvrier;
        private KryptonDataGridView gridOuvriers;
        private KryptonPanel panelOuvrierActions;
        private KryptonButton btnDeleteOuvrier;
        private KryptonButton btnNewOuvrier;
        private KryptonGroupBox groupDetails;
        private KryptonGroupBox groupCompetences;
        private KryptonLabel labelId;
        private KryptonTextBox textId;
        private KryptonTextBox textNom;
        private KryptonLabel labelNom;
        private KryptonTextBox textPrenom;
        private KryptonLabel labelPrenom;
        private KryptonNumericUpDown numCoutJournalier;
        private KryptonLabel labelCout;
        private KryptonDataGridView gridCompetences;
        private KryptonPanel panelCompetenceActions;
        private KryptonButton btnDeleteCompetence;
        private KryptonButton btnModifyCompetence;
        private KryptonButton btnAddCompetence;
        private KryptonPanel panelRightContainer;
    }
}