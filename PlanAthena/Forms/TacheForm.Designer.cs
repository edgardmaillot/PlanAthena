namespace PlanAthena.Forms
{
    partial class TacheForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TacheForm));
            toolStripPrincipal = new ToolStrip();
            btnZoomAjuster = new ToolStripButton();
            toolStripSeparatorDiagramme = new ToolStripSeparator();
            btnPan = new ToolStripButton();
            btnSauvegarderImage = new ToolStripButton();
            btnImprimer = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            lblRechercheToolStrip = new ToolStripLabel();
            txtRecherche = new ToolStripTextBox();
            toolStripSeparator3 = new ToolStripSeparator();
            btnMappingAuto = new ToolStripButton();
            statusStripPrincipal = new StatusStrip();
            lblStatistiques = new ToolStripStatusLabel();
            lblTacheSelectionnee = new ToolStripStatusLabel();
            groupBoxActions = new GroupBox();
            btnImportExcelFieldwire = new Button();
            btnExporter = new Button();
            btnImporter = new Button();
            btnFermer = new Button();
            panelPrincipal = new Panel();
            splitContainerPrincipal = new SplitContainer();
            panelOutilsMetiers = new Panel();
            panelOutilsMetiersDynamiques = new Panel();
            panelOutilsStatiques = new Panel();
            btnAjouterBloc = new Button();
            splitContainerInterne = new SplitContainer();
            panelDiagrammeCentral = new Panel();
            panelGestionDetails = new Panel();
            splitContainerDetails = new SplitContainer();
            panelPlans = new Panel();
            pictureBox1 = new PictureBox();
            panelDetailsTache = new Panel();
            groupBoxGestionLots = new GroupBox();
            btnGererLots = new Button();
            cmbLots = new ComboBox();
            lblLotActif = new Label();
            toolTipBloc = new ToolTip(components);
            toolStripPrincipal.SuspendLayout();
            statusStripPrincipal.SuspendLayout();
            groupBoxActions.SuspendLayout();
            panelPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerPrincipal).BeginInit();
            splitContainerPrincipal.Panel1.SuspendLayout();
            splitContainerPrincipal.Panel2.SuspendLayout();
            splitContainerPrincipal.SuspendLayout();
            panelOutilsMetiers.SuspendLayout();
            panelOutilsStatiques.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerInterne).BeginInit();
            splitContainerInterne.Panel1.SuspendLayout();
            splitContainerInterne.Panel2.SuspendLayout();
            splitContainerInterne.SuspendLayout();
            panelGestionDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerDetails).BeginInit();
            splitContainerDetails.Panel1.SuspendLayout();
            splitContainerDetails.Panel2.SuspendLayout();
            splitContainerDetails.SuspendLayout();
            panelPlans.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupBoxGestionLots.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripPrincipal
            // 
            toolStripPrincipal.ImageScalingSize = new Size(24, 24);
            toolStripPrincipal.Items.AddRange(new ToolStripItem[] { btnZoomAjuster, toolStripSeparatorDiagramme, btnPan, btnSauvegarderImage, btnImprimer, toolStripSeparator2, lblRechercheToolStrip, txtRecherche, toolStripSeparator3, btnMappingAuto });
            toolStripPrincipal.Location = new Point(0, 0);
            toolStripPrincipal.Name = "toolStripPrincipal";
            toolStripPrincipal.Padding = new Padding(0, 0, 2, 0);
            toolStripPrincipal.Size = new Size(1192, 25);
            toolStripPrincipal.TabIndex = 1;
            toolStripPrincipal.Text = "toolStrip1";
            // 
            // btnZoomAjuster
            // 
            btnZoomAjuster.ImageTransparentColor = Color.Magenta;
            btnZoomAjuster.Name = "btnZoomAjuster";
            btnZoomAjuster.Size = new Size(86, 22);
            btnZoomAjuster.Text = "🔍 Ajuster Vue";
            btnZoomAjuster.ToolTipText = "Ajuster le zoom pour voir tout le diagramme";
            btnZoomAjuster.Click += btnZoomAjuster_Click;
            // 
            // toolStripSeparatorDiagramme
            // 
            toolStripSeparatorDiagramme.Name = "toolStripSeparatorDiagramme";
            toolStripSeparatorDiagramme.Size = new Size(6, 25);
            // 
            // btnPan
            // 
            btnPan.CheckOnClick = true;
            btnPan.ImageTransparentColor = Color.Magenta;
            btnPan.Name = "btnPan";
            btnPan.Size = new Size(46, 22);
            btnPan.Text = "🖐 Pan";
            btnPan.ToolTipText = "Activer le mode déplacement (Pan)";
            btnPan.Click += btnPan_Click;
            // 
            // btnSauvegarderImage
            // 
            btnSauvegarderImage.ImageTransparentColor = Color.Magenta;
            btnSauvegarderImage.Name = "btnSauvegarderImage";
            btnSauvegarderImage.Size = new Size(127, 22);
            btnSauvegarderImage.Text = "🖼️ Sauvegarder Image";
            btnSauvegarderImage.ToolTipText = "Sauvegarder le diagramme comme image";
            btnSauvegarderImage.Click += btnSauvegarderImage_Click;
            // 
            // btnImprimer
            // 
            btnImprimer.ImageTransparentColor = Color.Magenta;
            btnImprimer.Name = "btnImprimer";
            btnImprimer.Size = new Size(75, 22);
            btnImprimer.Text = "🖨️ Imprimer";
            btnImprimer.ToolTipText = "Imprimer le diagramme";
            btnImprimer.Click += btnImprimer_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // lblRechercheToolStrip
            // 
            lblRechercheToolStrip.Name = "lblRechercheToolStrip";
            lblRechercheToolStrip.Size = new Size(80, 22);
            lblRechercheToolStrip.Text = "🔍 Recherche:";
            // 
            // txtRecherche
            // 
            txtRecherche.Name = "txtRecherche";
            txtRecherche.Size = new Size(219, 25);
            txtRecherche.ToolTipText = "Rechercher par ID, nom, bloc, lot ou métier";
            txtRecherche.TextChanged += txtRecherche_TextChanged;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // btnMappingAuto
            // 
            btnMappingAuto.ImageTransparentColor = Color.Magenta;
            btnMappingAuto.Name = "btnMappingAuto";
            btnMappingAuto.Size = new Size(103, 22);
            btnMappingAuto.Text = "🔗 Mapping Auto";
            btnMappingAuto.ToolTipText = "Assignation automatique des métiers aux tâches";
            btnMappingAuto.Click += btnMappingAuto_Click;
            // 
            // statusStripPrincipal
            // 
            statusStripPrincipal.ImageScalingSize = new Size(20, 20);
            statusStripPrincipal.Items.AddRange(new ToolStripItem[] { lblStatistiques, lblTacheSelectionnee });
            statusStripPrincipal.Location = new Point(0, 549);
            statusStripPrincipal.Name = "statusStripPrincipal";
            statusStripPrincipal.Size = new Size(1192, 24);
            statusStripPrincipal.TabIndex = 2;
            statusStripPrincipal.Text = "statusStrip1";
            // 
            // lblStatistiques
            // 
            lblStatistiques.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblStatistiques.BorderStyle = Border3DStyle.Etched;
            lblStatistiques.Name = "lblStatistiques";
            lblStatistiques.Size = new Size(163, 19);
            lblStatistiques.Text = "Statistiques de chargement...";
            // 
            // lblTacheSelectionnee
            // 
            lblTacheSelectionnee.Margin = new Padding(10, 3, 0, 2);
            lblTacheSelectionnee.Name = "lblTacheSelectionnee";
            lblTacheSelectionnee.Size = new Size(1004, 19);
            lblTacheSelectionnee.Spring = true;
            lblTacheSelectionnee.Text = "Aucune sélection";
            lblTacheSelectionnee.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBoxActions
            // 
            groupBoxActions.Controls.Add(btnImportExcelFieldwire);
            groupBoxActions.Controls.Add(btnExporter);
            groupBoxActions.Controls.Add(btnImporter);
            groupBoxActions.Controls.Add(btnFermer);
            groupBoxActions.Dock = DockStyle.Bottom;
            groupBoxActions.Location = new Point(0, 504);
            groupBoxActions.Margin = new Padding(3, 2, 3, 2);
            groupBoxActions.Name = "groupBoxActions";
            groupBoxActions.Padding = new Padding(3, 2, 3, 6);
            groupBoxActions.Size = new Size(1192, 45);
            groupBoxActions.TabIndex = 3;
            groupBoxActions.TabStop = false;
            groupBoxActions.Text = "Actions Import/Export";
            // 
            // btnImportExcelFieldwire
            // 
            btnImportExcelFieldwire.Location = new Point(10, 16);
            btnImportExcelFieldwire.Margin = new Padding(3, 2, 3, 2);
            btnImportExcelFieldwire.Name = "btnImportExcelFieldwire";
            btnImportExcelFieldwire.Size = new Size(140, 22);
            btnImportExcelFieldwire.TabIndex = 0;
            btnImportExcelFieldwire.Text = "📥 Import Excel Fieldwire";
            btnImportExcelFieldwire.UseVisualStyleBackColor = true;
            btnImportExcelFieldwire.Click += btnImportExcelFieldwire_Click;
            // 
            // btnExporter
            // 
            btnExporter.Location = new Point(278, 16);
            btnExporter.Margin = new Padding(3, 2, 3, 2);
            btnExporter.Name = "btnExporter";
            btnExporter.Size = new Size(105, 22);
            btnExporter.TabIndex = 2;
            btnExporter.Text = "📤 Export CSV";
            btnExporter.UseVisualStyleBackColor = true;
            btnExporter.Click += btnExporter_Click;
            // 
            // btnImporter
            // 
            btnImporter.Location = new Point(162, 16);
            btnImporter.Margin = new Padding(3, 2, 3, 2);
            btnImporter.Name = "btnImporter";
            btnImporter.Size = new Size(105, 22);
            btnImporter.TabIndex = 1;
            btnImporter.Text = "📥 Import CSV";
            btnImporter.UseVisualStyleBackColor = true;
            btnImporter.Click += btnImporter_Click;
            // 
            // btnFermer
            // 
            btnFermer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFermer.Location = new Point(1113, 16);
            btnFermer.Margin = new Padding(3, 2, 3, 2);
            btnFermer.Name = "btnFermer";
            btnFermer.Size = new Size(70, 22);
            btnFermer.TabIndex = 3;
            btnFermer.Text = "Fermer";
            btnFermer.UseVisualStyleBackColor = true;
            btnFermer.Click += btnFermer_Click;
            // 
            // panelPrincipal
            // 
            panelPrincipal.Controls.Add(splitContainerPrincipal);
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.Location = new Point(0, 25);
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.Size = new Size(1192, 479);
            panelPrincipal.TabIndex = 4;
            // 
            // splitContainerPrincipal
            // 
            splitContainerPrincipal.Dock = DockStyle.Fill;
            splitContainerPrincipal.FixedPanel = FixedPanel.Panel1;
            splitContainerPrincipal.Location = new Point(0, 0);
            splitContainerPrincipal.Name = "splitContainerPrincipal";
            // 
            // splitContainerPrincipal.Panel1
            // 
            splitContainerPrincipal.Panel1.Controls.Add(panelOutilsMetiers);
            // 
            // splitContainerPrincipal.Panel2
            // 
            splitContainerPrincipal.Panel2.Controls.Add(splitContainerInterne);
            splitContainerPrincipal.Size = new Size(1192, 479);
            splitContainerPrincipal.SplitterDistance = 200;
            splitContainerPrincipal.TabIndex = 0;
            // 
            // panelOutilsMetiers
            // 
            panelOutilsMetiers.BorderStyle = BorderStyle.FixedSingle;
            panelOutilsMetiers.Controls.Add(panelOutilsMetiersDynamiques);
            panelOutilsMetiers.Controls.Add(panelOutilsStatiques);
            panelOutilsMetiers.Dock = DockStyle.Fill;
            panelOutilsMetiers.Location = new Point(0, 0);
            panelOutilsMetiers.Name = "panelOutilsMetiers";
            panelOutilsMetiers.Size = new Size(200, 479);
            panelOutilsMetiers.TabIndex = 0;
            // 
            // panelOutilsMetiersDynamiques
            // 
            panelOutilsMetiersDynamiques.AutoScroll = true;
            panelOutilsMetiersDynamiques.Dock = DockStyle.Fill;
            panelOutilsMetiersDynamiques.Location = new Point(0, 88);
            panelOutilsMetiersDynamiques.Name = "panelOutilsMetiersDynamiques";
            panelOutilsMetiersDynamiques.Size = new Size(198, 389);
            panelOutilsMetiersDynamiques.TabIndex = 1;
            // 
            // panelOutilsStatiques
            // 
            panelOutilsStatiques.Controls.Add(btnAjouterBloc);
            panelOutilsStatiques.Dock = DockStyle.Top;
            panelOutilsStatiques.Location = new Point(0, 0);
            panelOutilsStatiques.Name = "panelOutilsStatiques";
            panelOutilsStatiques.Size = new Size(198, 88);
            panelOutilsStatiques.TabIndex = 0;
            // 
            // btnAjouterBloc
            // 
            btnAjouterBloc.ForeColor = SystemColors.ButtonHighlight;
            btnAjouterBloc.Image = (Image)resources.GetObject("btnAjouterBloc.Image");
            btnAjouterBloc.Location = new Point(35, 8);
            btnAjouterBloc.Name = "btnAjouterBloc";
            btnAjouterBloc.Size = new Size(129, 77);
            btnAjouterBloc.TabIndex = 0;
            btnAjouterBloc.Text = "Ajouter un Bloc";
            btnAjouterBloc.TextAlign = ContentAlignment.TopCenter;
            toolTipBloc.SetToolTip(btnAjouterBloc, "Créer un nouveau bloc");
            btnAjouterBloc.UseVisualStyleBackColor = true;
            btnAjouterBloc.Click += btnAjouterBloc_Click;
            // 
            // splitContainerInterne
            // 
            splitContainerInterne.Dock = DockStyle.Fill;
            splitContainerInterne.FixedPanel = FixedPanel.Panel2;
            splitContainerInterne.Location = new Point(0, 0);
            splitContainerInterne.Name = "splitContainerInterne";
            // 
            // splitContainerInterne.Panel1
            // 
            splitContainerInterne.Panel1.Controls.Add(panelDiagrammeCentral);
            // 
            // splitContainerInterne.Panel2
            // 
            splitContainerInterne.Panel2.Controls.Add(panelGestionDetails);
            splitContainerInterne.Size = new Size(988, 479);
            splitContainerInterne.SplitterDistance = 584;
            splitContainerInterne.TabIndex = 0;
            // 
            // panelDiagrammeCentral
            // 
            panelDiagrammeCentral.Dock = DockStyle.Fill;
            panelDiagrammeCentral.Location = new Point(0, 0);
            panelDiagrammeCentral.Name = "panelDiagrammeCentral";
            panelDiagrammeCentral.Size = new Size(584, 479);
            panelDiagrammeCentral.TabIndex = 0;
            // 
            // panelGestionDetails
            // 
            panelGestionDetails.Controls.Add(splitContainerDetails);
            panelGestionDetails.Controls.Add(groupBoxGestionLots);
            panelGestionDetails.Dock = DockStyle.Fill;
            panelGestionDetails.Location = new Point(0, 0);
            panelGestionDetails.Name = "panelGestionDetails";
            panelGestionDetails.Size = new Size(400, 479);
            panelGestionDetails.TabIndex = 0;
            // 
            // splitContainerDetails
            // 
            splitContainerDetails.Dock = DockStyle.Fill;
            splitContainerDetails.Location = new Point(0, 80);
            splitContainerDetails.Name = "splitContainerDetails";
            splitContainerDetails.Orientation = Orientation.Horizontal;
            // 
            // splitContainerDetails.Panel1
            // 
            splitContainerDetails.Panel1.Controls.Add(panelPlans);
            // 
            // splitContainerDetails.Panel2
            // 
            splitContainerDetails.Panel2.Controls.Add(panelDetailsTache);
            splitContainerDetails.Size = new Size(400, 399);
            splitContainerDetails.SplitterDistance = 151;
            splitContainerDetails.TabIndex = 1;
            // 
            // panelPlans
            // 
            panelPlans.BorderStyle = BorderStyle.FixedSingle;
            panelPlans.Controls.Add(pictureBox1);
            panelPlans.Dock = DockStyle.Fill;
            panelPlans.Location = new Point(0, 0);
            panelPlans.Name = "panelPlans";
            panelPlans.Size = new Size(400, 151);
            panelPlans.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = Properties.Resources.office_layout_example_1;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(398, 149);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // panelDetailsTache
            // 
            panelDetailsTache.BorderStyle = BorderStyle.FixedSingle;
            panelDetailsTache.Dock = DockStyle.Fill;
            panelDetailsTache.Location = new Point(0, 0);
            panelDetailsTache.Name = "panelDetailsTache";
            panelDetailsTache.Size = new Size(400, 244);
            panelDetailsTache.TabIndex = 0;
            // 
            // groupBoxGestionLots
            // 
            groupBoxGestionLots.Controls.Add(btnGererLots);
            groupBoxGestionLots.Controls.Add(cmbLots);
            groupBoxGestionLots.Controls.Add(lblLotActif);
            groupBoxGestionLots.Dock = DockStyle.Top;
            groupBoxGestionLots.Location = new Point(0, 0);
            groupBoxGestionLots.Name = "groupBoxGestionLots";
            groupBoxGestionLots.Size = new Size(400, 80);
            groupBoxGestionLots.TabIndex = 0;
            groupBoxGestionLots.TabStop = false;
            groupBoxGestionLots.Text = "Gestion des Lots";
            // 
            // btnGererLots
            // 
            btnGererLots.Location = new Point(280, 47);
            btnGererLots.Name = "btnGererLots";
            btnGererLots.Size = new Size(110, 23);
            btnGererLots.TabIndex = 2;
            btnGererLots.Text = "Gérer les Lots...";
            btnGererLots.UseVisualStyleBackColor = true;
            btnGererLots.Click += btnGererLots_Click;
            // 
            // cmbLots
            // 
            cmbLots.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLots.FormattingEnabled = true;
            cmbLots.Location = new Point(70, 21);
            cmbLots.Name = "cmbLots";
            cmbLots.Size = new Size(320, 23);
            cmbLots.TabIndex = 1;
            cmbLots.SelectedIndexChanged += cmbLots_SelectedIndexChanged;
            // 
            // lblLotActif
            // 
            lblLotActif.AutoSize = true;
            lblLotActif.Location = new Point(6, 24);
            lblLotActif.Name = "lblLotActif";
            lblLotActif.Size = new Size(58, 15);
            lblLotActif.TabIndex = 0;
            lblLotActif.Text = "Lot Actif :";
            // 
            // TacheForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1192, 573);
            Controls.Add(panelPrincipal);
            Controls.Add(toolStripPrincipal);
            Controls.Add(groupBoxActions);
            Controls.Add(statusStripPrincipal);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(877, 460);
            Name = "TacheForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestion des Tâches - Diagramme PERT";
            WindowState = FormWindowState.Maximized;
            Load += TacheForm_Load;
            toolStripPrincipal.ResumeLayout(false);
            toolStripPrincipal.PerformLayout();
            statusStripPrincipal.ResumeLayout(false);
            statusStripPrincipal.PerformLayout();
            groupBoxActions.ResumeLayout(false);
            panelPrincipal.ResumeLayout(false);
            splitContainerPrincipal.Panel1.ResumeLayout(false);
            splitContainerPrincipal.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerPrincipal).EndInit();
            splitContainerPrincipal.ResumeLayout(false);
            panelOutilsMetiers.ResumeLayout(false);
            panelOutilsStatiques.ResumeLayout(false);
            splitContainerInterne.Panel1.ResumeLayout(false);
            splitContainerInterne.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerInterne).EndInit();
            splitContainerInterne.ResumeLayout(false);
            panelGestionDetails.ResumeLayout(false);
            splitContainerDetails.Panel1.ResumeLayout(false);
            splitContainerDetails.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerDetails).EndInit();
            splitContainerDetails.ResumeLayout(false);
            panelPlans.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupBoxGestionLots.ResumeLayout(false);
            groupBoxGestionLots.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripPrincipal;
        private System.Windows.Forms.ToolStripButton btnZoomAjuster;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorDiagramme;
        private System.Windows.Forms.ToolStripButton btnPan;
        private System.Windows.Forms.ToolStripButton btnSauvegarderImage;
        private System.Windows.Forms.ToolStripButton btnImprimer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel lblRechercheToolStrip;
        private System.Windows.Forms.ToolStripTextBox txtRecherche;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnMappingAuto;
        private System.Windows.Forms.StatusStrip statusStripPrincipal;
        private System.Windows.Forms.ToolStripStatusLabel lblStatistiques;
        private System.Windows.Forms.ToolStripStatusLabel lblTacheSelectionnee;
        private System.Windows.Forms.GroupBox groupBoxActions;
        private System.Windows.Forms.Button btnImportExcelFieldwire;
        private System.Windows.Forms.Button btnExporter;
        private System.Windows.Forms.Button btnImporter;
        private System.Windows.Forms.Button btnFermer;
        private System.Windows.Forms.Panel panelPrincipal;
        private System.Windows.Forms.SplitContainer splitContainerPrincipal;
        private System.Windows.Forms.Panel panelOutilsMetiers;
        private System.Windows.Forms.Panel panelOutilsMetiersDynamiques;
        private System.Windows.Forms.Panel panelOutilsStatiques;
        private System.Windows.Forms.Button btnAjouterBloc;
        private System.Windows.Forms.SplitContainer splitContainerInterne;
        private System.Windows.Forms.Panel panelDiagrammeCentral;
        private System.Windows.Forms.Panel panelGestionDetails;
        private System.Windows.Forms.SplitContainer splitContainerDetails;
        private System.Windows.Forms.Panel panelPlans;
        private System.Windows.Forms.Panel panelDetailsTache;
        private System.Windows.Forms.GroupBox groupBoxGestionLots;
        private System.Windows.Forms.Button btnGererLots;
        private System.Windows.Forms.ComboBox cmbLots;
        private System.Windows.Forms.Label lblLotActif;
        private PictureBox pictureBox1;
        private ToolTip toolTipBloc;
    }
}