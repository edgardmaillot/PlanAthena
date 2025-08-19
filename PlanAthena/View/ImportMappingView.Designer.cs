namespace PlanAthena.Forms
{
    partial class ImportMappingView
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
            this.lblSelectedLot = new System.Windows.Forms.Label();
            this.dgvCsvPreview = new System.Windows.Forms.DataGridView();
            this.chkHasHeader = new System.Windows.Forms.CheckBox();
            this.tlpMapping = new System.Windows.Forms.TableLayoutPanel();
            this.grpImportSettings = new System.Windows.Forms.GroupBox();
            this.txtNomBlocParDefaut = new System.Windows.Forms.TextBox();
            this.lblNomBlocParDefaut = new System.Windows.Forms.Label();
            this.numCapaciteMaxOuvriersDefaut = new System.Windows.Forms.NumericUpDown();
            this.lblCapaciteMaxOuvriersDefaut = new System.Windows.Forms.Label();
            this.numHeuresEstimeesDefaut = new System.Windows.Forms.NumericUpDown();
            this.lblHeuresEstimeesDefaut = new System.Windows.Forms.Label();
            this.lstMetiersManquants = new System.Windows.Forms.ListBox();
            this.lblMetiersManquants = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.btnConfirmImport = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCsvPreview)).BeginInit();
            this.grpImportSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCapaciteMaxOuvriersDefaut)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeuresEstimeesDefaut)).BeginInit();
            this.SuspendLayout();
            //
            // lblSelectedLot
            //
            this.lblSelectedLot.AutoSize = true;
            this.lblSelectedLot.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSelectedLot.Location = new System.Drawing.Point(12, 9);
            this.lblSelectedLot.Name = "lblSelectedLot";
            this.lblSelectedLot.Size = new System.Drawing.Size(89, 17);
            this.lblSelectedLot.TabIndex = 0;
            this.lblSelectedLot.Text = "Lot cible : {0}";
            //
            // dgvCsvPreview
            //
            this.dgvCsvPreview.AllowUserToAddRows = false;
            this.dgvCsvPreview.AllowUserToDeleteRows = false;
            this.dgvCsvPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCsvPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCsvPreview.Location = new System.Drawing.Point(12, 60);
            this.dgvCsvPreview.Name = "dgvCsvPreview";
            this.dgvCsvPreview.ReadOnly = true;
            this.dgvCsvPreview.RowHeadersVisible = false;
            this.dgvCsvPreview.RowTemplate.Height = 25;
            this.dgvCsvPreview.Size = new System.Drawing.Size(760, 150);
            this.dgvCsvPreview.TabIndex = 1;
            //
            // chkHasHeader
            //
            this.chkHasHeader.AutoSize = true;
            this.chkHasHeader.Location = new System.Drawing.Point(12, 37);
            this.chkHasHeader.Name = "chkHasHeader";
            this.chkHasHeader.Size = new System.Drawing.Size(206, 19);
            this.chkHasHeader.TabIndex = 2;
            this.chkHasHeader.Text = "La première ligne contient des en-têtes";
            this.chkHasHeader.UseVisualStyleBackColor = true;
            this.chkHasHeader.CheckedChanged += new System.EventHandler(this.chkHasHeader_CheckedChanged);
            //
            // tlpMapping
            //
            this.tlpMapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMapping.ColumnCount = 2;
            this.tlpMapping.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMapping.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMapping.Location = new System.Drawing.Point(12, 220);
            this.tlpMapping.Name = "tlpMapping";
            this.tlpMapping.RowCount = 2;
            this.tlpMapping.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMapping.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMapping.Size = new System.Drawing.Size(760, 100);
            this.tlpMapping.TabIndex = 3;
            //
            // grpImportSettings
            //
            this.grpImportSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpImportSettings.Controls.Add(this.txtNomBlocParDefaut);
            this.grpImportSettings.Controls.Add(this.lblNomBlocParDefaut);
            this.grpImportSettings.Controls.Add(this.numCapaciteMaxOuvriersDefaut);
            this.grpImportSettings.Controls.Add(this.lblCapaciteMaxOuvriersDefaut);
            this.grpImportSettings.Controls.Add(this.numHeuresEstimeesDefaut);
            this.grpImportSettings.Controls.Add(this.lblHeuresEstimeesDefaut);
            this.grpImportSettings.Location = new System.Drawing.Point(12, 330);
            this.grpImportSettings.Name = "grpImportSettings";
            this.grpImportSettings.Size = new System.Drawing.Size(400, 130); // Hauteur ajustée
            this.grpImportSettings.TabIndex = 4;
            this.grpImportSettings.TabStop = false;
            this.grpImportSettings.Text = "Paramètres d\'Import et Gestion des tâches sans bloc d\'affectation"; // Texte mis à jour
            //
            // txtNomBlocParDefaut
            //
            this.txtNomBlocParDefaut.Location = new System.Drawing.Point(220, 85); // Position ajustée
            this.txtNomBlocParDefaut.Name = "txtNomBlocParDefaut";
            this.txtNomBlocParDefaut.Size = new System.Drawing.Size(150, 23);
            this.txtNomBlocParDefaut.TabIndex = 6;
            //
            // lblNomBlocParDefaut
            //
            this.lblNomBlocParDefaut.AutoSize = true;
            this.lblNomBlocParDefaut.Location = new System.Drawing.Point(10, 88); // Position ajustée
            this.lblNomBlocParDefaut.Name = "lblNomBlocParDefaut";
            this.lblNomBlocParDefaut.Size = new System.Drawing.Size(126, 15);
            this.lblNomBlocParDefaut.TabIndex = 5;
            this.lblNomBlocParDefaut.Text = "Nom du bloc par défaut :";
            //
            // numCapaciteMaxOuvriersDefaut
            //
            this.numCapaciteMaxOuvriersDefaut.Location = new System.Drawing.Point(220, 50);
            this.numCapaciteMaxOuvriersDefaut.Name = "numCapaciteMaxOuvriersDefaut";
            this.numCapaciteMaxOuvriersDefaut.Size = new System.Drawing.Size(70, 23);
            this.numCapaciteMaxOuvriersDefaut.TabIndex = 3;
            //
            // lblCapaciteMaxOuvriersDefaut
            //
            this.lblCapaciteMaxOuvriersDefaut.AutoSize = true;
            this.lblCapaciteMaxOuvriersDefaut.Location = new System.Drawing.Point(10, 52);
            this.lblCapaciteMaxOuvriersDefaut.Name = "lblCapaciteMaxOuvriersDefaut";
            this.lblCapaciteMaxOuvriersDefaut.Size = new System.Drawing.Size(193, 15);
            this.lblCapaciteMaxOuvriersDefaut.TabIndex = 2;
            this.lblCapaciteMaxOuvriersDefaut.Text = "Capacité Max Ouvriers Bloc par défaut :";
            //
            // numHeuresEstimeesDefaut
            //
            this.numHeuresEstimeesDefaut.Location = new System.Drawing.Point(220, 22);
            this.numHeuresEstimeesDefaut.Name = "numHeuresEstimeesDefaut";
            this.numHeuresEstimeesDefaut.Size = new System.Drawing.Size(70, 23);
            this.numHeuresEstimeesDefaut.TabIndex = 1;
            //
            // lblHeuresEstimeesDefaut
            //
            this.lblHeuresEstimeesDefaut.AutoSize = true;
            this.lblHeuresEstimeesDefaut.Location = new System.Drawing.Point(10, 24);
            this.lblHeuresEstimeesDefaut.Name = "lblHeuresEstimeesDefaut";
            this.lblHeuresEstimeesDefaut.Size = new System.Drawing.Size(147, 15);
            this.lblHeuresEstimeesDefaut.TabIndex = 0;
            this.lblHeuresEstimeesDefaut.Text = "Heures Estimées par défaut :";
            //
            // lstMetiersManquants
            //
            this.lstMetiersManquants.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lstMetiersManquants.FormattingEnabled = true;
            this.lstMetiersManquants.ItemHeight = 15;
            this.lstMetiersManquants.Location = new System.Drawing.Point(420, 345);
            this.lstMetiersManquants.Name = "lstMetiersManquants";
            this.lstMetiersManquants.Size = new System.Drawing.Size(350, 124); // Hauteur ajustée
            this.lstMetiersManquants.TabIndex = 5;
            //
            // lblMetiersManquants
            //
            this.lblMetiersManquants.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMetiersManquants.AutoSize = true;
            this.lblMetiersManquants.Location = new System.Drawing.Point(420, 330);
            this.lblMetiersManquants.Name = "lblMetiersManquants";
            this.lblMetiersManquants.Size = new System.Drawing.Size(262, 15);
            this.lblMetiersManquants.TabIndex = 6;
            this.lblMetiersManquants.Text = "Métiers détectés dans le CSV non existants :";
            //
            // txtLog
            //
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 470); // Position ajustée
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(760, 120); // Hauteur ajustée
            this.txtLog.TabIndex = 7;
            this.txtLog.Text = "";
            //
            // btnConfirmImport
            //
            this.btnConfirmImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfirmImport.Location = new System.Drawing.Point(672, 600);
            this.btnConfirmImport.Name = "btnConfirmImport";
            this.btnConfirmImport.Size = new System.Drawing.Size(100, 30);
            this.btnConfirmImport.TabIndex = 8;
            this.btnConfirmImport.Text = "Confirmer l\'Import";
            this.btnConfirmImport.UseVisualStyleBackColor = true;
            this.btnConfirmImport.Click += new System.EventHandler(this.btnConfirmImport_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(566, 600);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Annuler";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // ImportTacheForm
            //
            this.ClientSize = new System.Drawing.Size(784, 641); // Taille ajustée
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirmImport);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblMetiersManquants);
            this.Controls.Add(this.lstMetiersManquants);
            this.Controls.Add(this.grpImportSettings);
            this.Controls.Add(this.tlpMapping);
            this.Controls.Add(this.chkHasHeader);
            this.Controls.Add(this.dgvCsvPreview);
            this.Controls.Add(this.lblSelectedLot);
            this.MinimumSize = new System.Drawing.Size(800, 680); // Ajout pour s'assurer que les éléments ne se chevauchent pas trop sur les petits écrans
            this.Name = "ImportTacheForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import de Tâches CSV";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCsvPreview)).EndInit();
            this.grpImportSettings.ResumeLayout(false);
            this.grpImportSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCapaciteMaxOuvriersDefaut)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeuresEstimeesDefaut)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblSelectedLot;
        private System.Windows.Forms.DataGridView dgvCsvPreview;
        private System.Windows.Forms.CheckBox chkHasHeader;
        private System.Windows.Forms.TableLayoutPanel tlpMapping;
        private System.Windows.Forms.GroupBox grpImportSettings;
        private System.Windows.Forms.TextBox txtNomBlocParDefaut;
        private System.Windows.Forms.Label lblNomBlocParDefaut;
        // chkCreerBlocParDefaut a été supprimé
        private System.Windows.Forms.NumericUpDown numCapaciteMaxOuvriersDefaut;
        private System.Windows.Forms.Label lblCapaciteMaxOuvriersDefaut;
        private System.Windows.Forms.NumericUpDown numHeuresEstimeesDefaut;
        private System.Windows.Forms.Label lblHeuresEstimeesDefaut;
        private System.Windows.Forms.ListBox lstMetiersManquants;
        private System.Windows.Forms.Label lblMetiersManquants;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Button btnConfirmImport;
        private System.Windows.Forms.Button btnCancel;

        #endregion
    }
}