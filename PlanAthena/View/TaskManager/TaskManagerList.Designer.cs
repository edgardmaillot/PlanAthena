namespace PlanAthena.View.TaskManager
{
    partial class TaskManagerList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskManagerList));
            kpGauche = new Krypton.Toolkit.KryptonPanel();
            kryptonDataGridView1 = new Krypton.Toolkit.KryptonDataGridView();
            kryptonHeader1 = new Krypton.Toolkit.KryptonHeader();
            DG_Lot = new DataGridViewTextBoxColumn();
            DG_Bloc = new DataGridViewTextBoxColumn();
            DG_Tache = new DataGridViewTextBoxColumn();
            DG_Metier = new DataGridViewTextBoxColumn();
            DG_Statut = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)kpGauche).BeginInit();
            kpGauche.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonDataGridView1).BeginInit();
            SuspendLayout();
            // 
            // kpGauche
            // 
            kpGauche.Controls.Add(kryptonDataGridView1);
            kpGauche.Controls.Add(kryptonHeader1);
            kpGauche.Dock = DockStyle.Fill;
            kpGauche.Location = new Point(0, 0);
            kpGauche.Name = "kpGauche";
            kpGauche.Size = new Size(400, 800);
            kpGauche.TabIndex = 0;
            // 
            // kryptonDataGridView1
            // 
            kryptonDataGridView1.AllowUserToAddRows = false;
            kryptonDataGridView1.AllowUserToDeleteRows = false;
            kryptonDataGridView1.AllowUserToOrderColumns = true;
            kryptonDataGridView1.BorderStyle = BorderStyle.None;
            kryptonDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            kryptonDataGridView1.Columns.AddRange(new DataGridViewColumn[] { DG_Lot, DG_Bloc, DG_Tache, DG_Metier, DG_Statut });
            kryptonDataGridView1.Dock = DockStyle.Fill;
            kryptonDataGridView1.Location = new Point(0, 31);
            kryptonDataGridView1.MultiSelect = false;
            kryptonDataGridView1.Name = "kryptonDataGridView1";
            kryptonDataGridView1.ReadOnly = true;
            kryptonDataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            kryptonDataGridView1.Size = new Size(400, 769);
            kryptonDataGridView1.TabIndex = 0;
            // 
            // kryptonHeader1
            // 
            kryptonHeader1.Dock = DockStyle.Top;
            kryptonHeader1.Location = new Point(0, 0);
            kryptonHeader1.Name = "kryptonHeader1";
            kryptonHeader1.Size = new Size(400, 31);
            kryptonHeader1.TabIndex = 1;
            kryptonHeader1.Values.Description = "";
            kryptonHeader1.Values.Heading = "Gestionnaire de Tâches";
            kryptonHeader1.Values.Image = (Image)resources.GetObject("kryptonHeader1.Values.Image");
            // 
            // DG_Lot
            // 
            DG_Lot.HeaderText = "Lots";
            DG_Lot.MinimumWidth = 20;
            DG_Lot.Name = "DG_Lot";
            DG_Lot.ReadOnly = true;
            DG_Lot.Width = 50;
            // 
            // DG_Bloc
            // 
            DG_Bloc.HeaderText = "Blocs";
            DG_Bloc.MinimumWidth = 20;
            DG_Bloc.Name = "DG_Bloc";
            DG_Bloc.ReadOnly = true;
            DG_Bloc.Width = 50;
            // 
            // DG_Tache
            // 
            DG_Tache.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DG_Tache.HeaderText = "Tache";
            DG_Tache.MinimumWidth = 100;
            DG_Tache.Name = "DG_Tache";
            DG_Tache.ReadOnly = true;
            // 
            // DG_Metier
            // 
            DG_Metier.HeaderText = "Métier";
            DG_Metier.MinimumWidth = 20;
            DG_Metier.Name = "DG_Metier";
            DG_Metier.ReadOnly = true;
            DG_Metier.Width = 80;
            // 
            // DG_Statut
            // 
            DG_Statut.HeaderText = "Statut";
            DG_Statut.MinimumWidth = 20;
            DG_Statut.Name = "DG_Statut";
            DG_Statut.ReadOnly = true;
            DG_Statut.Width = 80;
            // 
            // TaskManagerList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(kpGauche);
            Name = "TaskManagerList";
            Size = new Size(400, 800);
            ((System.ComponentModel.ISupportInitialize)kpGauche).EndInit();
            kpGauche.ResumeLayout(false);
            kpGauche.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonDataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        // --- SUPPRESSION des champs inutiles ---
        // private TacheDetailViewXL tacheDetailViewXL1;
        // private SplitContainer splitContainer1;

        private Krypton.Toolkit.KryptonPanel kpGauche;
        private Krypton.Toolkit.KryptonDataGridView kryptonDataGridView1;
        private Krypton.Toolkit.KryptonHeader kryptonHeader1;
        private DataGridViewTextBoxColumn DG_Lot;
        private DataGridViewTextBoxColumn DG_Bloc;
        private DataGridViewTextBoxColumn DG_Tache;
        private DataGridViewTextBoxColumn DG_Metier;
        private DataGridViewTextBoxColumn DG_Statut;
    }
}