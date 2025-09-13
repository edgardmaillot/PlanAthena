// Version 0.5.1
using Krypton.Toolkit;
using PlanAthena.Services.DTOs.ImportExport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlanAthena.View.Utils
{
    public partial class ImportP3 : UserControl
    {
        public event EventHandler ValiderClicked;
        public event EventHandler RetourClicked;

        public ImportP3Result Result { get; private set; }

        private BindingSource _bindingSource;

        public ImportP3()
        {
            InitializeComponent();
            this.Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            kBtValider.Click += KBtValider_Click;
            kBtRetour.Click += KBtRetour_Click;
        }

        // La méthode n'est plus générique
        public void Initialize(ImportP3Config config)
        {
            _bindingSource = config.DataSource;

            khTitre.Values.Heading = $"Importer des {config.EntityDisplayName}";
            if (config.EntityImage != null)
                kryptonPanel1.StateNormal.Image = config.EntityImage;

            DisplayRejections(config.RejectedRows);

            // Lier la source de données directement au DataGridView
            kDataGrid_P3.DataSource = _bindingSource;

            FormatDataGridView();
        }

        /// <summary>
        /// Affiche un message d'erreur à l'utilisateur, typiquement après un échec de l'ImportService.
        /// </summary>
        public void ShowImportError(string message)
        {
            MessageBox.Show(this, message, "Erreur d'importation", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Met le contrôle en état d'attente (ou le restaure) pendant que l'orchestrateur travaille.
        /// </summary>
        public void SetWaitState(bool isWaiting)
        {
            kBtValider.Enabled = !isWaiting;
            kBtRetour.Enabled = !isWaiting;
            kDataGrid_P3.Enabled = !isWaiting;
            this.Cursor = isWaiting ? Cursors.WaitCursor : Cursors.Default;
        }

        private void DisplayRejections(List<RejectedRowInfo> rejections)
        {
            if (rejections == null || !rejections.Any())
            {
                kRichTxtRejet.Text = "Aucune ligne n'a été rejetée lors de la phase de transformation.";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Données non importées ({rejections.Count} lignes rejetées) :");
            foreach (var rejection in rejections.OrderBy(r => r.OriginalLineNumber))
            {
                sb.AppendLine($"- Ligne {rejection.OriginalLineNumber}: {rejection.Reason}");
            }
            kRichTxtRejet.Text = sb.ToString();
        }

        private void FormatDataGridView()
        {
            kDataGrid_P3.ReadOnly = false;
            kDataGrid_P3.AllowUserToAddRows = true;
            kDataGrid_P3.AllowUserToDeleteRows = true;
            kDataGrid_P3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }


        private void KBtValider_Click(object sender, EventArgs e)
        {
            try
            {
                this.Validate();
                _bindingSource.EndEdit();

                // Le résultat est simplement la liste contenue dans le BindingSource
                this.Result = new ImportP3Result
                {
                    FinalData = (IBindingList)_bindingSource.List
                };

                ValiderClicked?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Erreur de validation des données saisies : {ex.Message}", "Erreur de saisie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KBtRetour_Click(object sender, EventArgs e)
        {
            RetourClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}