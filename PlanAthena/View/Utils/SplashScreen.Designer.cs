namespace PlanAthena.View.Utils
{
    partial class SplashScreen
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
            kryptonLabel1 = new Krypton.Toolkit.KryptonLabel();
            kryptonPictureBox1 = new Krypton.Toolkit.KryptonPictureBox();
            lblDimensions = new Krypton.Toolkit.KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)kryptonPictureBox1).BeginInit();
            SuspendLayout();
            // 
            // kryptonLabel1
            // 
            kryptonLabel1.Dock = DockStyle.Bottom;
            kryptonLabel1.LabelStyle = Krypton.Toolkit.LabelStyle.NormalControl;
            kryptonLabel1.Location = new Point(0, 847);
            kryptonLabel1.Name = "kryptonLabel1";
            kryptonLabel1.Size = new Size(1656, 37);
            kryptonLabel1.StateNormal.ShortText.Color1 = Color.Black;
            kryptonLabel1.StateNormal.ShortText.Color2 = Color.DimGray;
            kryptonLabel1.StateNormal.ShortText.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            kryptonLabel1.StateNormal.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            kryptonLabel1.StateNormal.ShortText.TextV = Krypton.Toolkit.PaletteRelativeAlign.Center;
            kryptonLabel1.TabIndex = 0;
            kryptonLabel1.Values.Text = "Chargement du cockpit de pilotage...";
            // 
            // kryptonPictureBox1
            // 
            kryptonPictureBox1.Dock = DockStyle.Fill;
            kryptonPictureBox1.Image = Properties.Resources.SplashScreen;
            kryptonPictureBox1.Location = new Point(0, 0);
            kryptonPictureBox1.Name = "kryptonPictureBox1";
            kryptonPictureBox1.Size = new Size(1656, 884);
            kryptonPictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            kryptonPictureBox1.TabIndex = 1;
            kryptonPictureBox1.TabStop = false;
            // 
            // lblDimensions
            // 
            lblDimensions.Dock = DockStyle.Top;
            lblDimensions.LabelStyle = Krypton.Toolkit.LabelStyle.NormalControl;
            lblDimensions.Location = new Point(0, 0);
            lblDimensions.Name = "lblDimensions";
            lblDimensions.Size = new Size(1656, 37);
            lblDimensions.StateNormal.ShortText.Color1 = Color.Black;
            lblDimensions.StateNormal.ShortText.Color2 = Color.DimGray;
            lblDimensions.StateNormal.ShortText.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDimensions.StateNormal.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            lblDimensions.StateNormal.ShortText.TextV = Krypton.Toolkit.PaletteRelativeAlign.Center;
            lblDimensions.TabIndex = 2;
            lblDimensions.Values.Text = "data";
            // 
            // SplashScreen
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblDimensions);
            Controls.Add(kryptonLabel1);
            Controls.Add(kryptonPictureBox1);
            Margin = new Padding(0);
            Name = "SplashScreen";
            Size = new Size(1656, 884);
            ((System.ComponentModel.ISupportInitialize)kryptonPictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private Krypton.Toolkit.KryptonPictureBox kryptonPictureBox1;
        private Krypton.Toolkit.KryptonLabel lblDimensions;
    }
}
