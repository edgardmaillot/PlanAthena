namespace PlanAthena
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

        private void InitializeComponent()
        {
            panelPrincipal = new Panel();
            toolStripPrincipal = new ToolStrip();
            btnNouvelleTache = new ToolStripButton();
            btnModifierTache = new ToolStripButton();
            btnSupprimerTache = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnZoomAjuster = new ToolStripButton();
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
            toolStripPrincipal.SuspendLayout();
            statusStripPrincipal.SuspendLayout();
            groupBoxActions.SuspendLayout();
            SuspendLayout();
            // 
            // panelPrincipal
            // 
            panelPrincipal.BackColor = SystemColors.Window;
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.Location = new Point(0, 25);
            panelPrincipal.Margin = new Padding(3, 2, 3, 2);
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.Size = new Size(1192, 479);
            panelPrincipal.TabIndex = 0;
            // 
            // toolStripPrincipal
            // 
            toolStripPrincipal.ImageScalingSize = new Size(24, 24);
            toolStripPrincipal.Items.AddRange(new ToolStripItem[] { btnNouvelleTache, btnModifierTache, btnSupprimerTache, toolStripSeparator1, btnZoomAjuster, toolStripSeparator2, lblRechercheToolStrip, txtRecherche, toolStripSeparator3, btnMappingAuto });
            toolStripPrincipal.Location = new Point(0, 0);
            toolStripPrincipal.Name = "toolStripPrincipal";
            toolStripPrincipal.Padding = new Padding(0, 0, 2, 0);
            toolStripPrincipal.Size = new Size(1192, 25);
            toolStripPrincipal.TabIndex = 1;
            toolStripPrincipal.Text = "toolStrip1";
            // 
            // btnNouvelleTache
            // 
            btnNouvelleTache.ImageTransparentColor = Color.Magenta;
            btnNouvelleTache.Name = "btnNouvelleTache";
            btnNouvelleTache.Size = new Size(73, 22);
            btnNouvelleTache.Text = "➕ Nouvelle";
            btnNouvelleTache.ToolTipText = "Créer une nouvelle tâche";
            btnNouvelleTache.Click += btnNouvelleTache_Click;
            // 
            // btnModifierTache
            // 
            btnModifierTache.ImageTransparentColor = Color.Magenta;
            btnModifierTache.Name = "btnModifierTache";
            btnModifierTache.Size = new Size(71, 22);
            btnModifierTache.Text = "✏️ Modifier";
            btnModifierTache.ToolTipText = "Modifier la tâche sélectionnée";
            btnModifierTache.Click += btnModifierTache_Click;
            // 
            // btnSupprimerTache
            // 
            btnSupprimerTache.ImageTransparentColor = Color.Magenta;
            btnSupprimerTache.Name = "btnSupprimerTache";
            btnSupprimerTache.Size = new Size(81, 22);
            btnSupprimerTache.Text = "🗑️ Supprimer";
            btnSupprimerTache.ToolTipText = "Supprimer la tâche sélectionnée";
            btnSupprimerTache.Click += btnSupprimerTache_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
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
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Panel panelPrincipal;
        private System.Windows.Forms.ToolStrip toolStripPrincipal;
        private System.Windows.Forms.ToolStripButton btnNouvelleTache;
        private System.Windows.Forms.ToolStripButton btnModifierTache;
        private System.Windows.Forms.ToolStripButton btnSupprimerTache;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnZoomAjuster;
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
    }
}