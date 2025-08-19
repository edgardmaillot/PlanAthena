// USING UNIQUEMENT LE NAMESPACE DE BASE GARANTI
using Krypton.Toolkit;

namespace PlanAthena.View
{
    partial class MainShellForm
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
            menuStrip = new MenuStrip();
            menuAccueil = new ToolStripMenuItem();
            menuFichier = new ToolStripMenuItem();
            menuNouveauProjet = new ToolStripMenuItem();
            menuChargerProjet = new ToolStripMenuItem();
            menuSauvegarderProjet = new ToolStripMenuItem();
            menuStructure = new ToolStripMenuItem();
            menuStructureListe = new ToolStripMenuItem();
            menuStructureDiagramme = new ToolStripMenuItem();
            menuRessources = new ToolStripMenuItem();
            menuRessourcesMetiers = new ToolStripMenuItem();
            menuRessourcesOuvriers = new ToolStripMenuItem();
            menuTaches = new ToolStripMenuItem();
            menuTachesDiagramme = new ToolStripMenuItem();
            toolStripComboBoxThemes = new KryptonToolStripComboBox();
            kryptonManager = new KryptonManager(components);
            panelContent = new KryptonPanel();
            menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)toolStripComboBoxThemes.KryptonComboBoxControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelContent).BeginInit();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Font = new Font("Segoe UI", 9F);
            menuStrip.Items.AddRange(new ToolStripItem[] { menuAccueil, menuFichier, menuStructure, menuRessources, menuTaches, toolStripComboBoxThemes });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new Padding(7, 2, 0, 2);
            menuStrip.Size = new Size(1487, 29);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip1";
            // 
            // menuAccueil
            // 
            menuAccueil.Name = "menuAccueil";
            menuAccueil.Size = new Size(58, 25);
            menuAccueil.Text = "Accueil";
            menuAccueil.Click += menuAccueil_Click;
            // 
            // menuFichier
            // 
            menuFichier.DropDownItems.AddRange(new ToolStripItem[] { menuNouveauProjet, menuChargerProjet, menuSauvegarderProjet });
            menuFichier.Name = "menuFichier";
            menuFichier.Size = new Size(54, 25);
            menuFichier.Text = "Fichier";
            // 
            // menuNouveauProjet
            // 
            menuNouveauProjet.Name = "menuNouveauProjet";
            menuNouveauProjet.Size = new Size(173, 22);
            menuNouveauProjet.Text = "Nouveau Projet";
            // 
            // menuChargerProjet
            // 
            menuChargerProjet.Name = "menuChargerProjet";
            menuChargerProjet.Size = new Size(173, 22);
            menuChargerProjet.Text = "Charger Projet";
            // 
            // menuSauvegarderProjet
            // 
            menuSauvegarderProjet.Name = "menuSauvegarderProjet";
            menuSauvegarderProjet.Size = new Size(173, 22);
            menuSauvegarderProjet.Text = "Sauvegarder Projet";
            // 
            // menuStructure
            // 
            menuStructure.DropDownItems.AddRange(new ToolStripItem[] { menuStructureListe, menuStructureDiagramme });
            menuStructure.Name = "menuStructure";
            menuStructure.Size = new Size(67, 25);
            menuStructure.Text = "Structure";
            // 
            // menuStructureListe
            // 
            menuStructureListe.Name = "menuStructureListe";
            menuStructureListe.Size = new Size(136, 22);
            menuStructureListe.Text = "Liste";
            menuStructureListe.Click += menuStructureListe_Click;
            // 
            // menuStructureDiagramme
            // 
            menuStructureDiagramme.Name = "menuStructureDiagramme";
            menuStructureDiagramme.Size = new Size(136, 22);
            menuStructureDiagramme.Text = "Diagramme";
            // 
            // menuRessources
            // 
            menuRessources.DropDownItems.AddRange(new ToolStripItem[] { menuRessourcesMetiers, menuRessourcesOuvriers });
            menuRessources.Name = "menuRessources";
            menuRessources.Size = new Size(77, 25);
            menuRessources.Text = "Ressources";
            // 
            // menuRessourcesMetiers
            // 
            menuRessourcesMetiers.Name = "menuRessourcesMetiers";
            menuRessourcesMetiers.Size = new Size(118, 22);
            menuRessourcesMetiers.Text = "Métiers";
            menuRessourcesMetiers.Click += menuRessourcesMetiers_Click;
            // 
            // menuRessourcesOuvriers
            // 
            menuRessourcesOuvriers.Name = "menuRessourcesOuvriers";
            menuRessourcesOuvriers.Size = new Size(118, 22);
            menuRessourcesOuvriers.Text = "Ouvriers";
            menuRessourcesOuvriers.Click += menuRessourcesOuvriers_Click;
            // 
            // menuTaches
            // 
            menuTaches.DropDownItems.AddRange(new ToolStripItem[] { menuTachesDiagramme });
            menuTaches.Name = "menuTaches";
            menuTaches.Size = new Size(55, 25);
            menuTaches.Text = "Tâches";
            // 
            // menuTachesDiagramme
            // 
            menuTachesDiagramme.Name = "menuTachesDiagramme";
            menuTachesDiagramme.Size = new Size(136, 22);
            menuTachesDiagramme.Text = "Diagramme";
            menuTachesDiagramme.Click += menuTachesDiagramme_Click;
            // 
            // toolStripComboBoxThemes
            // 
            toolStripComboBoxThemes.AutoSize = false;
            // 
            // toolStripComboBoxThemes
            // 
            toolStripComboBoxThemes.KryptonComboBoxControl.AccessibleName = "toolStripComboBoxThemes";
            toolStripComboBoxThemes.KryptonComboBoxControl.DropDownWidth = 121;
            toolStripComboBoxThemes.KryptonComboBoxControl.Location = new Point(618, 3);
            toolStripComboBoxThemes.KryptonComboBoxControl.Name = "kryptonToolStripComboBox1";
            toolStripComboBoxThemes.KryptonComboBoxControl.StateCommon.ComboBox.Content.TextH = PaletteRelativeAlign.Near;
            toolStripComboBoxThemes.KryptonComboBoxControl.TabIndex = 1;
            toolStripComboBoxThemes.KryptonComboBoxControl.Text = "Thème";
            toolStripComboBoxThemes.Name = "toolStripComboBoxThemes";
            this.toolStripComboBoxThemes.Size = new Size(150, 25); 

            // AJOUTE CETTE LIGNE
            this.toolStripComboBoxThemes.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripComboBoxThemes.Text = "Thème";
            // 
            // panelContent
            // 
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 29);
            panelContent.Margin = new Padding(4, 3, 4, 3);
            panelContent.Name = "panelContent";
            panelContent.PaletteMode = PaletteMode.Office2010SilverDarkMode;
            panelContent.Size = new Size(1487, 813);
            panelContent.TabIndex = 1;
            // 
            // MainShellForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1487, 842);
            Controls.Add(panelContent);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainShellForm";
            Text = "PlanAthena v0.5";
            WindowState = FormWindowState.Maximized;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)toolStripComboBoxThemes.KryptonComboBoxControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelContent).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuAccueil;
        private System.Windows.Forms.ToolStripMenuItem menuFichier;
        private System.Windows.Forms.ToolStripMenuItem menuStructure;
        private System.Windows.Forms.ToolStripMenuItem menuRessources;
        private System.Windows.Forms.ToolStripMenuItem menuTaches;
        private Krypton.Toolkit.KryptonPanel panelContent;
        private Krypton.Toolkit.KryptonManager kryptonManager;
        private System.Windows.Forms.ToolStripMenuItem menuNouveauProjet;
        private System.Windows.Forms.ToolStripMenuItem menuChargerProjet;
        private System.Windows.Forms.ToolStripMenuItem menuSauvegarderProjet;
        private System.Windows.Forms.ToolStripMenuItem menuStructureListe;
        private System.Windows.Forms.ToolStripMenuItem menuStructureDiagramme;
        private System.Windows.Forms.ToolStripMenuItem menuRessourcesMetiers;
        private System.Windows.Forms.ToolStripMenuItem menuRessourcesOuvriers;
        private System.Windows.Forms.ToolStripMenuItem menuTachesDiagramme;
        private KryptonToolStripComboBox toolStripComboBoxThemes;
    }
}