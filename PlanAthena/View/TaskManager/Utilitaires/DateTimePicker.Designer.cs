namespace PlanAthena.View.TaskManager.Utilitaires
{
    partial class DateTimePicker
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            kCalendrier = new Krypton.Toolkit.KryptonMonthCalendar();
            kCmbHeure = new Krypton.Toolkit.KryptonComboBox();
            kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
            kbAnnule = new Krypton.Toolkit.KryptonButton();
            kbValide = new Krypton.Toolkit.KryptonButton();
            kryptonLabel1 = new Krypton.Toolkit.KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)kCmbHeure).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel1).BeginInit();
            kryptonPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // kCalendrier
            // 
            kCalendrier.Location = new Point(3, 3);
            kCalendrier.Name = "kCalendrier";
            kCalendrier.Size = new Size(314, 182);
            kCalendrier.TabIndex = 0;
            // 
            // kCmbHeure
            // 
            kCmbHeure.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            kCmbHeure.DropDownStyle = ComboBoxStyle.DropDownList;
            kCmbHeure.DropDownWidth = 63;
            kCmbHeure.Location = new Point(150, 191);
            kCmbHeure.Name = "kCmbHeure";
            kCmbHeure.Size = new Size(63, 22);
            kCmbHeure.Sorted = true;
            kCmbHeure.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            kCmbHeure.TabIndex = 1;
            // 
            // kryptonPanel1
            // 
            kryptonPanel1.Controls.Add(kbAnnule);
            kryptonPanel1.Controls.Add(kbValide);
            kryptonPanel1.Controls.Add(kryptonLabel1);
            kryptonPanel1.Controls.Add(kCmbHeure);
            kryptonPanel1.Controls.Add(kCalendrier);
            kryptonPanel1.Dock = DockStyle.Fill;
            kryptonPanel1.Location = new Point(0, 0);
            kryptonPanel1.Name = "kryptonPanel1";
            kryptonPanel1.Size = new Size(319, 218);
            kryptonPanel1.TabIndex = 2;
            // 
            // kbAnnule
            // 
            kbAnnule.Location = new Point(5, 188);
            kbAnnule.Name = "kbAnnule";
            kbAnnule.Size = new Size(90, 25);
            kbAnnule.TabIndex = 4;
            kbAnnule.Values.DropDownArrowColor = Color.Empty;
            kbAnnule.Values.Text = "Annuler";
            //kbAnnule.Click += SelectionCancelled;
            // 
            // kbValide
            // 
            kbValide.Location = new Point(226, 188);
            kbValide.Name = "kbValide";
            kbValide.Size = new Size(90, 25);
            kbValide.TabIndex = 3;
            kbValide.Values.DropDownArrowColor = Color.Empty;
            kbValide.Values.Text = "Valider";
            // 
            // kryptonLabel1
            // 
            kryptonLabel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            kryptonLabel1.Location = new Point(101, 191);
            kryptonLabel1.Name = "kryptonLabel1";
            kryptonLabel1.Size = new Size(43, 20);
            kryptonLabel1.TabIndex = 2;
            kryptonLabel1.Values.Text = "Heure";
            // 
            // DateTimePicker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonPanel1);
            Name = "DateTimePicker";
            Size = new Size(319, 218);
            ((System.ComponentModel.ISupportInitialize)kCmbHeure).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
            kryptonPanel1.ResumeLayout(false);
            kryptonPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Krypton.Toolkit.KryptonMonthCalendar kCalendrier;
        private Krypton.Toolkit.KryptonComboBox kCmbHeure;
        private Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private Krypton.Toolkit.KryptonButton kbValide;
        private Krypton.Toolkit.KryptonButton kbAnnule;
    }
}
