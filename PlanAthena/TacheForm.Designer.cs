namespace PlanAthena
{
    partial class TacheForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            groupBoxArbre = new GroupBox();
            lblStatutArbre = new Label();
            btnSupprimerElement = new Button();
            btnModifierElement = new Button();
            btnNouvelleElement = new Button();
            txtRechercheArbre = new TextBox();
            lblRechercheArbre = new Label();
            radioTaches = new RadioButton();
            radioBlocs = new RadioButton();
            radioLots = new RadioButton();
            treeViewTaches = new TreeView();
            splitContainer2 = new SplitContainer();
            groupBoxDetails = new GroupBox();
            cmbMetier = new ComboBox();
            lblMetier = new Label();
            txtDependances = new TextBox();
            lblDependances = new Label();
            numHeuresHomme = new NumericUpDown();
            lblHeuresHomme = new Label();
            txtNomElement = new TextBox();
            lblNomElement = new Label();
            txtIdElement = new TextBox();
            lblIdElement = new Label();
            groupBoxMapping = new GroupBox();
            btnMappingAuto = new Button();
            lblStatutMapping = new Label();
            progressMapping = new ProgressBar();
            listViewTachesSansMetier = new ListView();
            colTacheId = new ColumnHeader();
            colTacheNom = new ColumnHeader();
            colBlocId = new ColumnHeader();
            groupBoxActions = new GroupBox();
            btnImportExcelFieldwire = new Button();
            btnExporter = new Button();
            btnImporter = new Button();
            btnFermer = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBoxArbre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBoxDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numHeuresHomme).BeginInit();
            groupBoxMapping.SuspendLayout();
            groupBoxActions.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(groupBoxArbre);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1084, 561);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.TabIndex = 0;
            // 
            // groupBoxArbre
            // 
            groupBoxArbre.Controls.Add(lblStatutArbre);
            groupBoxArbre.Controls.Add(btnSupprimerElement);
            groupBoxArbre.Controls.Add(btnModifierElement);
            groupBoxArbre.Controls.Add(btnNouvelleElement);
            groupBoxArbre.Controls.Add(txtRechercheArbre);
            groupBoxArbre.Controls.Add(lblRechercheArbre);
            groupBoxArbre.Controls.Add(radioTaches);
            groupBoxArbre.Controls.Add(radioBlocs);
            groupBoxArbre.Controls.Add(radioLots);
            groupBoxArbre.Controls.Add(treeViewTaches);
            groupBoxArbre.Dock = DockStyle.Fill;
            groupBoxArbre.Location = new Point(0, 0);
            groupBoxArbre.Name = "groupBoxArbre";
            groupBoxArbre.Size = new Size(400, 561);
            groupBoxArbre.TabIndex = 0;
            groupBoxArbre.TabStop = false;
            groupBoxArbre.Text = "Arbre des Tâches";
            // 
            // lblStatutArbre
            // 
            lblStatutArbre.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatutArbre.AutoSize = true;
            lblStatutArbre.Location = new Point(6, 537);
            lblStatutArbre.Name = "lblStatutArbre";
            lblStatutArbre.Size = new Size(102, 15);
            lblStatutArbre.TabIndex = 9;
            lblStatutArbre.Text = "0 élément(s)";
            // 
            // btnSupprimerElement
            // 
            btnSupprimerElement.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSupprimerElement.Location = new Point(319, 505);
            btnSupprimerElement.Name = "btnSupprimerElement";
            btnSupprimerElement.Size = new Size(75, 23);
            btnSupprimerElement.TabIndex = 8;
            btnSupprimerElement.Text = "🗑️ Supprimer";
            btnSupprimerElement.UseVisualStyleBackColor = true;
            btnSupprimerElement.Click += btnSupprimerElement_Click;
            // 
            // btnModifierElement
            // 
            btnModifierElement.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnModifierElement.Location = new Point(238, 505);
            btnModifierElement.Name = "btnModifierElement";
            btnModifierElement.Size = new Size(75, 23);
            btnModifierElement.TabIndex = 7;
            btnModifierElement.Text = "✏️ Modifier";
            btnModifierElement.UseVisualStyleBackColor = true;
            btnModifierElement.Click += btnModifierElement_Click;
            // 
            // btnNouvelleElement
            // 
            btnNouvelleElement.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnNouvelleElement.Location = new Point(157, 505);
            btnNouvelleElement.Name = "btnNouvelleElement";
            btnNouvelleElement.Size = new Size(75, 23);
            btnNouvelleElement.TabIndex = 6;
            btnNouvelleElement.Text = "➕ Nouveau";
            btnNouvelleElement.UseVisualStyleBackColor = true;
            btnNouvelleElement.Click += btnNouvelleElement_Click;
            // 
            // txtRechercheArbre
            // 
            txtRechercheArbre.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRechercheArbre.Location = new Point(72, 75);
            txtRechercheArbre.Name = "txtRechercheArbre";
            txtRechercheArbre.Size = new Size(322, 23);
            txtRechercheArbre.TabIndex = 5;
            txtRechercheArbre.TextChanged += txtRechercheArbre_TextChanged;
            // 
            // lblRechercheArbre
            // 
            lblRechercheArbre.AutoSize = true;
            lblRechercheArbre.Location = new Point(6, 78);
            lblRechercheArbre.Name = "lblRechercheArbre";
            lblRechercheArbre.Size = new Size(60, 15);
            lblRechercheArbre.TabIndex = 4;
            lblRechercheArbre.Text = "Recherche:";
            // 
            // radioTaches
            // 
            radioTaches.AutoSize = true;
            radioTaches.Checked = true;
            radioTaches.Location = new Point(160, 22);
            radioTaches.Name = "radioTaches";
            radioTaches.Size = new Size(61, 19);
            radioTaches.TabIndex = 3;
            radioTaches.TabStop = true;
            radioTaches.Text = "Tâches";
            radioTaches.UseVisualStyleBackColor = true;
            radioTaches.CheckedChanged += radioAffichage_CheckedChanged;
            // 
            // radioBlocs
            // 
            radioBlocs.AutoSize = true;
            radioBlocs.Location = new Point(86, 22);
            radioBlocs.Name = "radioBlocs";
            radioBlocs.Size = new Size(54, 19);
            radioBlocs.TabIndex = 2;
            radioBlocs.Text = "Blocs";
            radioBlocs.UseVisualStyleBackColor = true;
            radioBlocs.CheckedChanged += radioAffichage_CheckedChanged;
            // 
            // radioLots
            // 
            radioLots.AutoSize = true;
            radioLots.Location = new Point(16, 22);
            radioLots.Name = "radioLots";
            radioLots.Size = new Size(47, 19);
            radioLots.TabIndex = 1;
            radioLots.Text = "Lots";
            radioLots.UseVisualStyleBackColor = true;
            radioLots.CheckedChanged += radioAffichage_CheckedChanged;
            // 
            // treeViewTaches
            // 
            treeViewTaches.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeViewTaches.FullRowSelect = true;
            treeViewTaches.HideSelection = false;
            treeViewTaches.Location = new Point(6, 104);
            treeViewTaches.Name = "treeViewTaches";
            treeViewTaches.ShowLines = true;
            treeViewTaches.ShowPlusMinus = true;
            treeViewTaches.ShowRootLines = true;
            treeViewTaches.Size = new Size(388, 395);
            treeViewTaches.TabIndex = 0;
            treeViewTaches.AfterSelect += treeViewTaches_AfterSelect;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(groupBoxDetails);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupBoxMapping);
            splitContainer2.Size = new Size(680, 561);
            splitContainer2.SplitterDistance = 280;
            splitContainer2.TabIndex = 0;
            // 
            // groupBoxDetails
            // 
            groupBoxDetails.Controls.Add(cmbMetier);
            groupBoxDetails.Controls.Add(lblMetier);
            groupBoxDetails.Controls.Add(txtDependances);
            groupBoxDetails.Controls.Add(lblDependances);
            groupBoxDetails.Controls.Add(numHeuresHomme);
            groupBoxDetails.Controls.Add(lblHeuresHomme);
            groupBoxDetails.Controls.Add(txtNomElement);
            groupBoxDetails.Controls.Add(lblNomElement);
            groupBoxDetails.Controls.Add(txtIdElement);
            groupBoxDetails.Controls.Add(lblIdElement);
            groupBoxDetails.Dock = DockStyle.Fill;
            groupBoxDetails.Location = new Point(0, 0);
            groupBoxDetails.Name = "groupBoxDetails";
            groupBoxDetails.Size = new Size(680, 280);
            groupBoxDetails.TabIndex = 0;
            groupBoxDetails.TabStop = false;
            groupBoxDetails.Text = "Détails";
            // 
            // cmbMetier
            // 
            cmbMetier.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbMetier.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetier.Location = new Point(120, 133);
            cmbMetier.Name = "cmbMetier";
            cmbMetier.Size = new Size(547, 23);
            cmbMetier.TabIndex = 9;
            cmbMetier.SelectedIndexChanged += cmbMetier_SelectedIndexChanged;
            // 
            // lblMetier
            // 
            lblMetier.AutoSize = true;
            lblMetier.Location = new Point(16, 136);
            lblMetier.Name = "lblMetier";
            lblMetier.Size = new Size(46, 15);
            lblMetier.TabIndex = 8;
            lblMetier.Text = "Métier:";
            // 
            // txtDependances
            // 
            txtDependances.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtDependances.Location = new Point(120, 162);
            txtDependances.Multiline = true;
            txtDependances.Name = "txtDependances";
            txtDependances.ScrollBars = ScrollBars.Vertical;
            txtDependances.Size = new Size(547, 105);
            txtDependances.TabIndex = 7;
            txtDependances.TextChanged += txtDependances_TextChanged;
            // 
            // lblDependances
            // 
            lblDependances.AutoSize = true;
            lblDependances.Location = new Point(16, 165);
            lblDependances.Name = "lblDependances";
            lblDependances.Size = new Size(86, 15);
            lblDependances.TabIndex = 6;
            lblDependances.Text = "Dépendances:";
            // 
            // numHeuresHomme
            // 
            numHeuresHomme.Location = new Point(120, 104);
            numHeuresHomme.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numHeuresHomme.Name = "numHeuresHomme";
            numHeuresHomme.Size = new Size(100, 23);
            numHeuresHomme.TabIndex = 5;
            numHeuresHomme.ValueChanged += numHeuresHomme_ValueChanged;
            // 
            // lblHeuresHomme
            // 
            lblHeuresHomme.AutoSize = true;
            lblHeuresHomme.Location = new Point(16, 106);
            lblHeuresHomme.Name = "lblHeuresHomme";
            lblHeuresHomme.Size = new Size(98, 15);
            lblHeuresHomme.TabIndex = 4;
            lblHeuresHomme.Text = "Heures-homme:";
            // 
            // txtNomElement
            // 
            txtNomElement.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNomElement.Location = new Point(120, 44);
            txtNomElement.Multiline = true;
            txtNomElement.Name = "txtNomElement";
            txtNomElement.Size = new Size(547, 54);
            txtNomElement.TabIndex = 3;
            txtNomElement.TextChanged += txtNomElement_TextChanged;
            // 
            // lblNomElement
            // 
            lblNomElement.AutoSize = true;
            lblNomElement.Location = new Point(16, 47);
            lblNomElement.Name = "lblNomElement";
            lblNomElement.Size = new Size(37, 15);
            lblNomElement.TabIndex = 2;
            lblNomElement.Text = "Nom:";
            // 
            // txtIdElement
            // 
            txtIdElement.Location = new Point(120, 15);
            txtIdElement.Name = "txtIdElement";
            txtIdElement.Size = new Size(200, 23);
            txtIdElement.TabIndex = 1;
            txtIdElement.TextChanged += txtIdElement_TextChanged;
            // 
            // lblIdElement
            // 
            lblIdElement.AutoSize = true;
            lblIdElement.Location = new Point(16, 18);
            lblIdElement.Name = "lblIdElement";
            lblIdElement.Size = new Size(21, 15);
            lblIdElement.TabIndex = 0;
            lblIdElement.Text = "ID:";
            // 
            // groupBoxMapping
            // 
            groupBoxMapping.Controls.Add(btnMappingAuto);
            groupBoxMapping.Controls.Add(lblStatutMapping);
            groupBoxMapping.Controls.Add(progressMapping);
            groupBoxMapping.Controls.Add(listViewTachesSansMetier);
            groupBoxMapping.Dock = DockStyle.Fill;
            groupBoxMapping.Location = new Point(0, 0);
            groupBoxMapping.Name = "groupBoxMapping";
            groupBoxMapping.Size = new Size(680, 277);
            groupBoxMapping.TabIndex = 0;
            groupBoxMapping.TabStop = false;
            groupBoxMapping.Text = "Mapping Tâches ↔ Métiers";
            // 
            // btnMappingAuto
            // 
            btnMappingAuto.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnMappingAuto.Location = new Point(560, 220);
            btnMappingAuto.Name = "btnMappingAuto";
            btnMappingAuto.Size = new Size(107, 23);
            btnMappingAuto.TabIndex = 3;
            btnMappingAuto.Text = "🔗 Mapping Auto";
            btnMappingAuto.UseVisualStyleBackColor = true;
            btnMappingAuto.Click += btnMappingAuto_Click;
            // 
            // lblStatutMapping
            // 
            lblStatutMapping.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatutMapping.AutoSize = true;
            lblStatutMapping.Location = new Point(16, 253);
            lblStatutMapping.Name = "lblStatutMapping";
            lblStatutMapping.Size = new Size(185, 15);
            lblStatutMapping.TabIndex = 2;
            lblStatutMapping.Text = "Mapping: 0% (0/0 tâches)";
            // 
            // progressMapping
            // 
            progressMapping.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressMapping.Location = new Point(16, 249);
            progressMapping.Name = "progressMapping";
            progressMapping.Size = new Size(651, 15);
            progressMapping.TabIndex = 1;
            // 
            // listViewTachesSansMetier
            // 
            listViewTachesSansMetier.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewTachesSansMetier.Columns.AddRange(new ColumnHeader[] { colTacheId, colTacheNom, colBlocId });
            listViewTachesSansMetier.FullRowSelect = true;
            listViewTachesSansMetier.GridLines = true;
            listViewTachesSansMetier.Location = new Point(16, 22);
            listViewTachesSansMetier.MultiSelect = false;
            listViewTachesSansMetier.Name = "listViewTachesSansMetier";
            listViewTachesSansMetier.Size = new Size(651, 192);
            listViewTachesSansMetier.TabIndex = 0;
            listViewTachesSansMetier.UseCompatibleStateImageBehavior = false;
            listViewTachesSansMetier.View = View.Details;
            listViewTachesSansMetier.DoubleClick += listViewTachesSansMetier_DoubleClick;
            // 
            // colTacheId
            // 
            colTacheId.Text = "ID Tâche";
            colTacheId.Width = 120;
            // 
            // colTacheNom
            // 
            colTacheNom.Text = "Nom de la Tâche";
            colTacheNom.Width = 300;
            // 
            // colBlocId
            // 
            colBlocId.Text = "Bloc";
            colBlocId.Width = 100;
            // 
            // groupBoxActions
            // 
            groupBoxActions.Controls.Add(btnImportExcelFieldwire);
            groupBoxActions.Controls.Add(btnExporter);
            groupBoxActions.Controls.Add(btnImporter);
            groupBoxActions.Controls.Add(btnFermer);
            groupBoxActions.Dock = DockStyle.Bottom;
            groupBoxActions.Location = new Point(0, 561);
            groupBoxActions.Name = "groupBoxActions";
            groupBoxActions.Size = new Size(1084, 50);
            groupBoxActions.TabIndex = 1;
            groupBoxActions.TabStop = false;
            groupBoxActions.Text = "Actions";
            // 
            // btnImportExcelFieldwire
            // 
            btnImportExcelFieldwire.Location = new Point(59, 19);
            btnImportExcelFieldwire.Name = "btnImportExcelFieldwire";
            btnImportExcelFieldwire.Size = new Size(140, 23);
            btnImportExcelFieldwire.TabIndex = 3;
            btnImportExcelFieldwire.Text = "📥 Import Excel Fieldwire";
            btnImportExcelFieldwire.UseVisualStyleBackColor = true;
            btnImportExcelFieldwire.Click += btnImportExcelFieldwire_Click;
            // 
            // btnExporter
            // 
            btnExporter.Location = new Point(311, 19);
            btnExporter.Name = "btnExporter";
            btnExporter.Size = new Size(100, 23);
            btnExporter.TabIndex = 2;
            btnExporter.Text = "📤 Export CSV";
            btnExporter.UseVisualStyleBackColor = true;
            btnExporter.Click += btnExporter_Click;
            // 
            // btnImporter
            // 
            btnImporter.Location = new Point(205, 19);
            btnImporter.Name = "btnImporter";
            btnImporter.Size = new Size(100, 23);
            btnImporter.TabIndex = 1;
            btnImporter.Text = "📥 Import CSV";
            btnImporter.UseVisualStyleBackColor = true;
            btnImporter.Click += btnImporter_Click;
            // 
            // btnFermer
            // 
            btnFermer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFermer.Location = new Point(997, 19);
            btnFermer.Name = "btnFermer";
            btnFermer.Size = new Size(75, 23);
            btnFermer.TabIndex = 0;
            btnFermer.Text = "Fermer";
            btnFermer.UseVisualStyleBackColor = true;
            btnFermer.Click += btnFermer_Click;
            // 
            // TacheForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 611);
            Controls.Add(splitContainer1);
            Controls.Add(groupBoxActions);
            Name = "TacheForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestion des Tâches";
            Load += TacheForm_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBoxArbre.ResumeLayout(false);
            groupBoxArbre.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBoxDetails.ResumeLayout(false);
            groupBoxDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numHeuresHomme).EndInit();
            groupBoxMapping.ResumeLayout(false);
            groupBoxMapping.PerformLayout();
            groupBoxActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private GroupBox groupBoxArbre;
        private GroupBox groupBoxDetails;
        private GroupBox groupBoxMapping;
        private GroupBox groupBoxActions;
        private TreeView treeViewTaches;
        private RadioButton radioTaches;
        private RadioButton radioBlocs;
        private RadioButton radioLots;
        private TextBox txtRechercheArbre;
        private Label lblRechercheArbre;
        private Button btnSupprimerElement;
        private Button btnModifierElement;
        private Button btnNouvelleElement;
        private Label lblStatutArbre;
        private TextBox txtIdElement;
        private Label lblIdElement;
        private TextBox txtNomElement;
        private Label lblNomElement;
        private NumericUpDown numHeuresHomme;
        private Label lblHeuresHomme;
        private TextBox txtDependances;
        private Label lblDependances;
        private ComboBox cmbMetier;
        private Label lblMetier;
        private ListView listViewTachesSansMetier;
        private ColumnHeader colTacheId;
        private ColumnHeader colTacheNom;
        private ColumnHeader colBlocId;
        private ProgressBar progressMapping;
        private Label lblStatutMapping;
        private Button btnMappingAuto;
        private Button btnImporter;
        private Button btnExporter;
        private Button btnImportExcelFieldwire;
        private Button btnFermer;
    }
}