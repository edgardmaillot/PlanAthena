using PlanAthena.View.Utils;

namespace PlanAthena.View.TaskManager.Cockpit

{
    partial class PlanningView
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
            kryptonTableLayoutPanel1 = new DoubleBufferedTableLayoutPanel();
            kryptonTableLayoutPanel2 = new DoubleBufferedTableLayoutPanel();
            kpCentre = new Krypton.Toolkit.KryptonPanel();
            kpTop = new Krypton.Toolkit.KryptonPanel();
            klEnd = new Krypton.Toolkit.KryptonLabel();
            klStart = new Krypton.Toolkit.KryptonLabel();
            resetView = new Krypton.Toolkit.KryptonButton();
            kryptonTrackBar1 = new Krypton.Toolkit.KryptonTrackBar();
            kpBottom = new Krypton.Toolkit.KryptonPanel();
            lblNoPlanning = new Krypton.Toolkit.KryptonLabel();
            kryptonTableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpCentre).BeginInit();
            kpCentre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpTop).BeginInit();
            kpTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpBottom).BeginInit();
            kpBottom.SuspendLayout();
            SuspendLayout();
            // 
            // kryptonTableLayoutPanel1
            // 
            kryptonTableLayoutPanel1.BackColor = Color.Transparent;
            kryptonTableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            kryptonTableLayoutPanel1.ColumnCount = 15;
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142856F));
            kryptonTableLayoutPanel1.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel1.Location = new Point(0, 0);
            kryptonTableLayoutPanel1.Margin = new Padding(0);
            kryptonTableLayoutPanel1.Name = "kryptonTableLayoutPanel1";
            kryptonTableLayoutPanel1.RowCount = 5;
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            kryptonTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel1.Size = new Size(794, 494);
            kryptonTableLayoutPanel1.TabIndex = 0;
            // 
            // kryptonTableLayoutPanel2
            // 
            kryptonTableLayoutPanel2.BackColor = Color.Transparent;
            kryptonTableLayoutPanel2.ColumnCount = 1;
            kryptonTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel2.Controls.Add(kpCentre, 0, 1);
            kryptonTableLayoutPanel2.Controls.Add(kpTop, 0, 0);
            kryptonTableLayoutPanel2.Controls.Add(kpBottom, 0, 2);
            kryptonTableLayoutPanel2.Dock = DockStyle.Fill;
            kryptonTableLayoutPanel2.Location = new Point(0, 0);
            kryptonTableLayoutPanel2.Name = "kryptonTableLayoutPanel2";
            kryptonTableLayoutPanel2.RowCount = 3;
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            kryptonTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            kryptonTableLayoutPanel2.Size = new Size(800, 600);
            kryptonTableLayoutPanel2.TabIndex = 2;
            // 
            // kpCentre
            // 
            kpCentre.Controls.Add(kryptonTableLayoutPanel1);
            kpCentre.Dock = DockStyle.Fill;
            kpCentre.Location = new Point(3, 53);
            kpCentre.Name = "kpCentre";
            kpCentre.Size = new Size(794, 494);
            kpCentre.TabIndex = 0;
            // 
            // kpTop
            // 
            kpTop.Controls.Add(klEnd);
            kpTop.Controls.Add(klStart);
            kpTop.Controls.Add(resetView);
            kpTop.Controls.Add(kryptonTrackBar1);
            kpTop.Dock = DockStyle.Fill;
            kpTop.Location = new Point(3, 3);
            kpTop.Name = "kpTop";
            kpTop.Size = new Size(794, 44);
            kpTop.TabIndex = 1;
            // 
            // klEnd
            // 
            klEnd.Location = new Point(645, 19);
            klEnd.Name = "klEnd";
            klEnd.Size = new Size(27, 20);
            klEnd.TabIndex = 3;
            klEnd.Values.Text = "Fin";
            // 
            // klStart
            // 
            klStart.Location = new Point(317, 16);
            klStart.Name = "klStart";
            klStart.Size = new Size(44, 20);
            klStart.TabIndex = 2;
            klStart.Values.Text = "Début";
            // 
            // resetView
            // 
            resetView.Location = new Point(701, 16);
            resetView.Name = "resetView";
            resetView.Size = new Size(90, 25);
            resetView.TabIndex = 1;
            resetView.Values.DropDownArrowColor = Color.Empty;
            resetView.Values.Text = "Reset";
            // 
            // kryptonTrackBar1
            // 
            kryptonTrackBar1.Location = new Point(367, 14);
            kryptonTrackBar1.Name = "kryptonTrackBar1";
            kryptonTrackBar1.Size = new Size(273, 27);
            kryptonTrackBar1.TabIndex = 0;
            // 
            // kpBottom
            // 
            kpBottom.Controls.Add(lblNoPlanning);
            kpBottom.Dock = DockStyle.Fill;
            kpBottom.Location = new Point(3, 553);
            kpBottom.Name = "kpBottom";
            kpBottom.Size = new Size(794, 44);
            kpBottom.TabIndex = 2;
            // 
            // lblNoPlanning
            // 
            lblNoPlanning.Location = new Point(292, 0);
            lblNoPlanning.Name = "lblNoPlanning";
            lblNoPlanning.Size = new Size(182, 36);
            lblNoPlanning.TabIndex = 0;
            lblNoPlanning.Values.Text = "Aucun planning n'a été généré.\r\nVeuillez lancer une planification";
            // 
            // PlanningView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kryptonTableLayoutPanel2);
            Name = "PlanningView";
            Size = new Size(800, 600);
            kryptonTableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kpCentre).EndInit();
            kpCentre.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kpTop).EndInit();
            kpTop.ResumeLayout(false);
            kpTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kpBottom).EndInit();
            kpBottom.ResumeLayout(false);
            kpBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PlanAthena.View.Utils.DoubleBufferedTableLayoutPanel kryptonTableLayoutPanel1;
        private PlanAthena.View.Utils.DoubleBufferedTableLayoutPanel kryptonTableLayoutPanel2;
        private Krypton.Toolkit.KryptonPanel kpCentre;
        private Krypton.Toolkit.KryptonPanel kpTop;
        private Krypton.Toolkit.KryptonLabel klEnd;
        private Krypton.Toolkit.KryptonLabel klStart;
        private Krypton.Toolkit.KryptonButton resetView;
        private Krypton.Toolkit.KryptonTrackBar kryptonTrackBar1;
        private Krypton.Toolkit.KryptonPanel kpBottom;
        private Krypton.Toolkit.KryptonLabel lblNoPlanning;
    }
}
