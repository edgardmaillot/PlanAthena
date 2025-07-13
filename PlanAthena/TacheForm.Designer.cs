// Fichier : TacheForm.Designer.cs

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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.toolStripPrincipal = new System.Windows.Forms.ToolStrip();
            this.btnNouvelleTache = new System.Windows.Forms.ToolStripButton();
            this.btnModifierTache = new System.Windows.Forms.ToolStripButton();
            this.btnSupprimerTache = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnZoomAjuster = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorDiagramme = new System.Windows.Forms.ToolStripSeparator();
            this.btnPan = new System.Windows.Forms.ToolStripButton();
            this.btnSauvegarderImage = new System.Windows.Forms.ToolStripButton();
            this.btnImprimer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.lblRechercheToolStrip = new System.Windows.Forms.ToolStripLabel();
            this.txtRecherche = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnMappingAuto = new System.Windows.Forms.ToolStripButton();
            this.statusStripPrincipal = new System.Windows.Forms.StatusStrip();
            this.lblStatistiques = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTacheSelectionnee = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.btnImportExcelFieldwire = new System.Windows.Forms.Button();
            this.btnExporter = new System.Windows.Forms.Button();
            this.btnImporter = new System.Windows.Forms.Button();
            this.btnFermer = new System.Windows.Forms.Button();
            this.splitContainerPrincipal = new System.Windows.Forms.SplitContainer();
            this.splitContainerDetails = new System.Windows.Forms.SplitContainer();
            this.panelPlans = new System.Windows.Forms.Panel();
            this.panelDetailsTache = new System.Windows.Forms.Panel();
            this.toolStripPrincipal.SuspendLayout();
            this.statusStripPrincipal.SuspendLayout();
            this.groupBoxActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrincipal)).BeginInit();
            this.splitContainerPrincipal.Panel2.SuspendLayout();
            this.splitContainerPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDetails)).BeginInit();
            this.splitContainerDetails.Panel1.SuspendLayout();
            this.splitContainerDetails.Panel2.SuspendLayout();
            this.splitContainerDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripPrincipal
            // 
            this.toolStripPrincipal.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripPrincipal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNouvelleTache,
            this.btnModifierTache,
            this.btnSupprimerTache,
            this.toolStripSeparator1,
            this.btnZoomAjuster,
            this.toolStripSeparatorDiagramme,
            this.btnPan,
            this.btnSauvegarderImage,
            this.btnImprimer,
            this.toolStripSeparator2,
            this.lblRechercheToolStrip,
            this.txtRecherche,
            this.toolStripSeparator3,
            this.btnMappingAuto});
            this.toolStripPrincipal.Location = new System.Drawing.Point(0, 0);
            this.toolStripPrincipal.Name = "toolStripPrincipal";
            this.toolStripPrincipal.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripPrincipal.Size = new System.Drawing.Size(1192, 25);
            this.toolStripPrincipal.TabIndex = 1;
            this.toolStripPrincipal.Text = "toolStrip1";
            // 
            // btnNouvelleTache
            // 
            this.btnNouvelleTache.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNouvelleTache.Name = "btnNouvelleTache";
            this.btnNouvelleTache.Size = new System.Drawing.Size(73, 22);
            this.btnNouvelleTache.Text = "➕ Nouvelle";
            this.btnNouvelleTache.ToolTipText = "Créer une nouvelle tâche";
            this.btnNouvelleTache.Click += new System.EventHandler(this.btnNouvelleTache_Click);
            // 
            // btnModifierTache
            // 
            this.btnModifierTache.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnModifierTache.Name = "btnModifierTache";
            this.btnModifierTache.Size = new System.Drawing.Size(71, 22);
            this.btnModifierTache.Text = "✏️ Modifier";
            this.btnModifierTache.ToolTipText = "Modifier la tâche sélectionnée";
            this.btnModifierTache.Click += new System.EventHandler(this.btnModifierTache_Click);
            // 
            // btnSupprimerTache
            // 
            this.btnSupprimerTache.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSupprimerTache.Name = "btnSupprimerTache";
            this.btnSupprimerTache.Size = new System.Drawing.Size(81, 22);
            this.btnSupprimerTache.Text = "🗑️ Supprimer";
            this.btnSupprimerTache.ToolTipText = "Supprimer la tâche sélectionnée";
            this.btnSupprimerTache.Click += new System.EventHandler(this.btnSupprimerTache_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnZoomAjuster
            // 
            this.btnZoomAjuster.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomAjuster.Name = "btnZoomAjuster";
            this.btnZoomAjuster.Size = new System.Drawing.Size(86, 22);
            this.btnZoomAjuster.Text = "🔍 Ajuster Vue";
            this.btnZoomAjuster.ToolTipText = "Ajuster le zoom pour voir tout le diagramme";
            this.btnZoomAjuster.Click += new System.EventHandler(this.btnZoomAjuster_Click);
            // 
            // toolStripSeparatorDiagramme
            // 
            this.toolStripSeparatorDiagramme.Name = "toolStripSeparatorDiagramme";
            this.toolStripSeparatorDiagramme.Size = new System.Drawing.Size(6, 25);
            // 
            // btnPan
            // 
            this.btnPan.CheckOnClick = true;
            this.btnPan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPan.Name = "btnPan";
            this.btnPan.Size = new System.Drawing.Size(49, 22);
            this.btnPan.Text = "🖐 Pan";
            this.btnPan.ToolTipText = "Activer le mode déplacement (Pan)";
            this.btnPan.Click += new System.EventHandler(this.btnPan_Click);
            // 
            // btnSauvegarderImage
            // 
            this.btnSauvegarderImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSauvegarderImage.Name = "btnSauvegarderImage";
            this.btnSauvegarderImage.Size = new System.Drawing.Size(125, 22);
            this.btnSauvegarderImage.Text = "🖼️ Sauvegarder Image";
            this.btnSauvegarderImage.ToolTipText = "Sauvegarder le diagramme comme image";
            this.btnSauvegarderImage.Click += new System.EventHandler(this.btnSauvegarderImage_Click);
            // 
            // btnImprimer
            // 
            this.btnImprimer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImprimer.Name = "btnImprimer";
            this.btnImprimer.Size = new System.Drawing.Size(78, 22);
            this.btnImprimer.Text = "🖨️ Imprimer";
            this.btnImprimer.ToolTipText = "Imprimer le diagramme";
            this.btnImprimer.Click += new System.EventHandler(this.btnImprimer_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // lblRechercheToolStrip
            // 
            this.lblRechercheToolStrip.Name = "lblRechercheToolStrip";
            this.lblRechercheToolStrip.Size = new System.Drawing.Size(80, 22);
            this.lblRechercheToolStrip.Text = "🔍 Recherche:";
            // 
            // txtRecherche
            // 
            this.txtRecherche.Name = "txtRecherche";
            this.txtRecherche.Size = new System.Drawing.Size(219, 25);
            this.txtRecherche.ToolTipText = "Rechercher par ID, nom, bloc, lot ou métier";
            this.txtRecherche.TextChanged += new System.EventHandler(this.txtRecherche_TextChanged);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnMappingAuto
            // 
            this.btnMappingAuto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMappingAuto.Name = "btnMappingAuto";
            this.btnMappingAuto.Size = new System.Drawing.Size(103, 22);
            this.btnMappingAuto.Text = "🔗 Mapping Auto";
            this.btnMappingAuto.ToolTipText = "Assignation automatique des métiers aux tâches";
            this.btnMappingAuto.Click += new System.EventHandler(this.btnMappingAuto_Click);
            // 
            // statusStripPrincipal
            // 
            this.statusStripPrincipal.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStripPrincipal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatistiques,
            this.lblTacheSelectionnee});
            this.statusStripPrincipal.Location = new System.Drawing.Point(0, 549);
            this.statusStripPrincipal.Name = "statusStripPrincipal";
            this.statusStripPrincipal.Size = new System.Drawing.Size(1192, 24);
            this.statusStripPrincipal.TabIndex = 2;
            this.statusStripPrincipal.Text = "statusStrip1";
            // 
            // lblStatistiques
            // 
            this.lblStatistiques.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblStatistiques.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.lblStatistiques.Name = "lblStatistiques";
            this.lblStatistiques.Size = new System.Drawing.Size(163, 19);
            this.lblStatistiques.Text = "Statistiques de chargement...";
            // 
            // lblTacheSelectionnee
            // 
            this.lblTacheSelectionnee.Margin = new System.Windows.Forms.Padding(10, 3, 0, 2);
            this.lblTacheSelectionnee.Name = "lblTacheSelectionnee";
            this.lblTacheSelectionnee.Size = new System.Drawing.Size(1004, 19);
            this.lblTacheSelectionnee.Spring = true;
            this.lblTacheSelectionnee.Text = "Aucune sélection";
            this.lblTacheSelectionnee.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Controls.Add(this.btnImportExcelFieldwire);
            this.groupBoxActions.Controls.Add(this.btnExporter);
            this.groupBoxActions.Controls.Add(this.btnImporter);
            this.groupBoxActions.Controls.Add(this.btnFermer);
            this.groupBoxActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxActions.Location = new System.Drawing.Point(0, 504);
            this.groupBoxActions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Padding = new System.Windows.Forms.Padding(3, 2, 3, 6);
            this.groupBoxActions.Size = new System.Drawing.Size(1192, 45);
            this.groupBoxActions.TabIndex = 3;
            this.groupBoxActions.TabStop = false;
            this.groupBoxActions.Text = "Actions Import/Export";
            // 
            // btnImportExcelFieldwire
            // 
            this.btnImportExcelFieldwire.Location = new System.Drawing.Point(10, 16);
            this.btnImportExcelFieldwire.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnImportExcelFieldwire.Name = "btnImportExcelFieldwire";
            this.btnImportExcelFieldwire.Size = new System.Drawing.Size(140, 22);
            this.btnImportExcelFieldwire.TabIndex = 0;
            this.btnImportExcelFieldwire.Text = "📥 Import Excel Fieldwire";
            this.btnImportExcelFieldwire.UseVisualStyleBackColor = true;
            this.btnImportExcelFieldwire.Click += new System.EventHandler(this.btnImportExcelFieldwire_Click);
            // 
            // btnExporter
            // 
            this.btnExporter.Location = new System.Drawing.Point(278, 16);
            this.btnExporter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnExporter.Name = "btnExporter";
            this.btnExporter.Size = new System.Drawing.Size(105, 22);
            this.btnExporter.TabIndex = 2;
            this.btnExporter.Text = "📤 Export CSV";
            this.btnExporter.UseVisualStyleBackColor = true;
            this.btnExporter.Click += new System.EventHandler(this.btnExporter_Click);
            // 
            // btnImporter
            // 
            this.btnImporter.Location = new System.Drawing.Point(162, 16);
            this.btnImporter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnImporter.Name = "btnImporter";
            this.btnImporter.Size = new System.Drawing.Size(105, 22);
            this.btnImporter.TabIndex = 1;
            this.btnImporter.Text = "📥 Import CSV";
            this.btnImporter.UseVisualStyleBackColor = true;
            this.btnImporter.Click += new System.EventHandler(this.btnImporter_Click);
            // 
            // btnFermer
            // 
            this.btnFermer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFermer.Location = new System.Drawing.Point(1113, 16);
            this.btnFermer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnFermer.Name = "btnFermer";
            this.btnFermer.Size = new System.Drawing.Size(70, 22);
            this.btnFermer.TabIndex = 3;
            this.btnFermer.Text = "Fermer";
            this.btnFermer.UseVisualStyleBackColor = true;
            this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);
            // 
            // splitContainerPrincipal
            // 
            this.splitContainerPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPrincipal.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerPrincipal.Location = new System.Drawing.Point(0, 25);
            this.splitContainerPrincipal.Name = "splitContainerPrincipal";
            // 
            // splitContainerPrincipal.Panel2
            // 
            this.splitContainerPrincipal.Panel2.Controls.Add(this.splitContainerDetails);
            this.splitContainerPrincipal.Size = new System.Drawing.Size(1192, 479);
            this.splitContainerPrincipal.SplitterDistance = 788;
            this.splitContainerPrincipal.TabIndex = 4;
            // 
            // splitContainerDetails
            // 
            this.splitContainerDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerDetails.Location = new System.Drawing.Point(0, 0);
            this.splitContainerDetails.Name = "splitContainerDetails";
            this.splitContainerDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerDetails.Panel1
            // 
            this.splitContainerDetails.Panel1.Controls.Add(this.panelPlans);
            // 
            // splitContainerDetails.Panel2
            // 
            this.splitContainerDetails.Panel2.Controls.Add(this.panelDetailsTache);
            this.splitContainerDetails.Size = new System.Drawing.Size(400, 479);
            this.splitContainerDetails.SplitterDistance = 239;
            this.splitContainerDetails.TabIndex = 0;
            // 
            // panelPlans
            // 
            this.panelPlans.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPlans.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPlans.Location = new System.Drawing.Point(0, 0);
            this.panelPlans.Name = "panelPlans";
            this.panelPlans.Size = new System.Drawing.Size(400, 239);
            this.panelPlans.TabIndex = 0;
            // 
            // panelDetailsTache
            // 
            this.panelDetailsTache.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDetailsTache.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDetailsTache.Location = new System.Drawing.Point(0, 0);
            this.panelDetailsTache.Name = "panelDetailsTache";
            this.panelDetailsTache.Size = new System.Drawing.Size(400, 236);
            this.panelDetailsTache.TabIndex = 0;
            // 
            // TacheForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1192, 573);
            this.Controls.Add(this.splitContainerPrincipal);
            this.Controls.Add(this.toolStripPrincipal);
            this.Controls.Add(this.groupBoxActions);
            this.Controls.Add(this.statusStripPrincipal);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(877, 460);
            this.Name = "TacheForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gestion des Tâches - Diagramme PERT";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.TacheForm_Load);
            this.toolStripPrincipal.ResumeLayout(false);
            this.toolStripPrincipal.PerformLayout();
            this.statusStripPrincipal.ResumeLayout(false);
            this.statusStripPrincipal.PerformLayout();
            this.groupBoxActions.ResumeLayout(false);
            this.splitContainerPrincipal.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrincipal)).EndInit();
            this.splitContainerPrincipal.ResumeLayout(false);
            this.splitContainerDetails.Panel1.ResumeLayout(false);
            this.splitContainerDetails.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDetails)).EndInit();
            this.splitContainerDetails.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // ANCIEN panelPrincipal est supprimé
        // private System.Windows.Forms.Panel panelPrincipal; 

        // NOUVEAUX contrôles
        private System.Windows.Forms.SplitContainer splitContainerPrincipal;
        private System.Windows.Forms.SplitContainer splitContainerDetails;
        private System.Windows.Forms.Panel panelPlans;
        private System.Windows.Forms.Panel panelDetailsTache;

        private System.Windows.Forms.ToolStrip toolStripPrincipal;
        private System.Windows.Forms.ToolStripButton btnNouvelleTache;
        private System.Windows.Forms.ToolStripButton btnModifierTache;
        private System.Windows.Forms.ToolStripButton btnSupprimerTache;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
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
    }
}