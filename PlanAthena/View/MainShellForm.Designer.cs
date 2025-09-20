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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainShellForm));
            menuStrip = new MenuStrip();
            menuAccueil = new ToolStripMenuItem();
            menuFichier = new ToolStripMenuItem();
            menuNouveauProjet = new ToolStripMenuItem();
            menuChargerProjet = new ToolStripMenuItem();
            menuSauvegarderProjet = new ToolStripMenuItem();
            menuStructure = new ToolStripMenuItem();
            menuStructureListe = new ToolStripMenuItem();
            menuRessources = new ToolStripMenuItem();
            menuRessourcesMetiers = new ToolStripMenuItem();
            menuRessourcesOuvriers = new ToolStripMenuItem();
            menuTaches = new ToolStripMenuItem();
            menuTachesDiagramme = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuTachesListe = new ToolStripMenuItem();
            menuPreferences = new ToolStripMenuItem();
            menuSaveLayout = new ToolStripMenuItem();
            menuLoadLayout = new ToolStripMenuItem();
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
            menuStrip.Items.AddRange(new ToolStripItem[] { menuAccueil, menuFichier, menuStructure, menuRessources, menuTaches, menuPreferences, toolStripComboBoxThemes });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new Padding(7, 2, 0, 2);
            menuStrip.Size = new Size(1503, 32);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip1";
            // 
            // menuAccueil
            // 
            menuAccueil.ForeColor = SystemColors.ActiveCaptionText;
            menuAccueil.Image = Properties.Resources.Ic_Immeuble_b;
            menuAccueil.ImageScaling = ToolStripItemImageScaling.None;
            menuAccueil.Name = "menuAccueil";
            menuAccueil.Size = new Size(82, 28);
            menuAccueil.Text = "Accueil";
            menuAccueil.Click += menuAccueil_Click;
            // 
            // menuFichier
            // 
            menuFichier.DropDownItems.AddRange(new ToolStripItem[] { menuNouveauProjet, menuChargerProjet, menuSauvegarderProjet });
            menuFichier.Name = "menuFichier";
            menuFichier.Size = new Size(69, 28);
            menuFichier.Text = "💾 Fichier";
            // 
            // menuNouveauProjet
            // 
            menuNouveauProjet.Name = "menuNouveauProjet";
            menuNouveauProjet.Size = new Size(173, 22);
            menuNouveauProjet.Text = "Nouveau Projet";
            menuNouveauProjet.Click += menuNouveauProjet_Click;
            // 
            // menuChargerProjet
            // 
            menuChargerProjet.Name = "menuChargerProjet";
            menuChargerProjet.Size = new Size(173, 22);
            menuChargerProjet.Text = "Charger Projet";
            menuChargerProjet.Click += menuChargerProjet_Click;
            // 
            // menuSauvegarderProjet
            // 
            menuSauvegarderProjet.Name = "menuSauvegarderProjet";
            menuSauvegarderProjet.Size = new Size(173, 22);
            menuSauvegarderProjet.Text = "Sauvegarder Projet";
            menuSauvegarderProjet.Click += menuSauvegarderProjet_Click;
            // 
            // menuStructure
            // 
            menuStructure.DropDownItems.AddRange(new ToolStripItem[] { menuStructureListe });
            menuStructure.Name = "menuStructure";
            menuStructure.Size = new Size(82, 28);
            menuStructure.Text = "📐 Structure";
            // 
            // menuStructureListe
            // 
            menuStructureListe.Name = "menuStructureListe";
            menuStructureListe.Size = new Size(98, 22);
            menuStructureListe.Text = "Liste";
            menuStructureListe.Click += menuStructureListe_Click;
            // 
            // menuRessources
            // 
            menuRessources.DropDownItems.AddRange(new ToolStripItem[] { menuRessourcesMetiers, menuRessourcesOuvriers });
            menuRessources.Name = "menuRessources";
            menuRessources.Size = new Size(92, 28);
            menuRessources.Text = "👥 Ressources";
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
            menuTaches.DropDownItems.AddRange(new ToolStripItem[] { menuTachesDiagramme, toolStripSeparator1, menuTachesListe });
            menuTaches.Name = "menuTaches";
            menuTaches.Size = new Size(70, 28);
            menuTaches.Text = "📝 Tâches";
            // 
            // menuTachesDiagramme
            // 
            menuTachesDiagramme.Name = "menuTachesDiagramme";
            menuTachesDiagramme.Size = new Size(136, 22);
            menuTachesDiagramme.Text = "Diagramme";
            menuTachesDiagramme.Click += menuTachesDiagramme_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(133, 6);
            // 
            // menuTachesListe
            // 
            menuTachesListe.Name = "menuTachesListe";
            menuTachesListe.Size = new Size(136, 22);
            menuTachesListe.Text = "Cockpit";
            menuTachesListe.Click += menuTachesListe_Click;
            // 
            // menuPreferences
            // 
            menuPreferences.DropDownItems.AddRange(new ToolStripItem[] { menuSaveLayout, menuLoadLayout });
            menuPreferences.Name = "menuPreferences";
            menuPreferences.Size = new Size(95, 28);
            menuPreferences.Text = "⚙️ Préférences";
            // 
            // menuSaveLayout
            // 
            menuSaveLayout.Name = "menuSaveLayout";
            menuSaveLayout.Size = new Size(212, 22);
            menuSaveLayout.Text = "Sauvegarder la disposition";
            menuSaveLayout.Click += menuSaveLayout_Click;
            // 
            // menuLoadLayout
            // 
            menuLoadLayout.Name = "menuLoadLayout";
            menuLoadLayout.Size = new Size(212, 22);
            menuLoadLayout.Text = "Charger la disposition";
            menuLoadLayout.Click += menuLoadLayout_Click;
            // 
            // toolStripComboBoxThemes
            // 
            toolStripComboBoxThemes.Alignment = ToolStripItemAlignment.Right;
            toolStripComboBoxThemes.AutoSize = false;
            // 
            // toolStripComboBoxThemes
            // 
            toolStripComboBoxThemes.KryptonComboBoxControl.AccessibleName = "toolStripComboBoxThemes";
            toolStripComboBoxThemes.KryptonComboBoxControl.DropDownWidth = 121;
            toolStripComboBoxThemes.KryptonComboBoxControl.Location = new Point(1353, 5);
            toolStripComboBoxThemes.KryptonComboBoxControl.Name = "kryptonToolStripComboBox1";
            toolStripComboBoxThemes.KryptonComboBoxControl.Size = new Size(150, 22);
            toolStripComboBoxThemes.KryptonComboBoxControl.StateCommon.ComboBox.Content.TextH = PaletteRelativeAlign.Near;
            toolStripComboBoxThemes.KryptonComboBoxControl.TabIndex = 1;
            toolStripComboBoxThemes.KryptonComboBoxControl.Text = "Thème";
            toolStripComboBoxThemes.Name = "toolStripComboBoxThemes";
            toolStripComboBoxThemes.Size = new Size(150, 22);
            toolStripComboBoxThemes.Text = "Thème";
            // 
            // panelContent
            // 
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 32);
            panelContent.Margin = new Padding(4, 3, 4, 3);
            panelContent.Name = "panelContent";
            panelContent.PaletteMode = PaletteMode.Office2010SilverDarkMode;
            panelContent.Size = new Size(1503, 762);
            panelContent.TabIndex = 1;
            // 
            // MainShellForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1503, 794);
            Controls.Add(panelContent);
            Controls.Add(menuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainShellForm";
            Text = "PlanAthena v0.7.0";
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
        private System.Windows.Forms.ToolStripMenuItem menuRessourcesMetiers;
        private System.Windows.Forms.ToolStripMenuItem menuRessourcesOuvriers;
        private System.Windows.Forms.ToolStripMenuItem menuTachesDiagramme;
        private System.Windows.Forms.ToolStripMenuItem menuTachesListe;
        private System.Windows.Forms.ToolStripMenuItem menuPreferences;
        private System.Windows.Forms.ToolStripMenuItem menuSaveLayout;
        private System.Windows.Forms.ToolStripMenuItem menuLoadLayout;
        private KryptonToolStripComboBox toolStripComboBoxThemes;
        private ToolStripSeparator toolStripSeparator1;
    }
}