using PlanAthena.Data;
using System.Diagnostics;

namespace PlanAthena.View.TaskManager
{
    public partial class LotSelectionView : UserControl
    {
        private bool _isLoading = false;
        private ToolTip _tooltip = new ToolTip();

        // Événement pour notifier le parent qu'un nouveau lot est sélectionné
        public event EventHandler<Lot> LotSelectionChanged;

        public LotSelectionView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Remplit la liste déroulante des lots.
        /// </summary>
        public void PopulateLots(List<Lot> lots)
        {
            _isLoading = true;
            cmbLots.DataSource = lots;
            cmbLots.DisplayMember = "Nom";
            cmbLots.ValueMember = "LotId";
            _isLoading = false;
        }

        /// <summary>
        /// Sélectionne un lot spécifique dans la liste déroulante.
        /// </summary>
        public void SetSelectedLot(string lotId)
        {
            if (string.IsNullOrEmpty(lotId))
            {
                cmbLots.SelectedIndex = -1;
                return;
            }

            _isLoading = true;
            cmbLots.SelectedValue = lotId;
            _isLoading = false;

            // Déclencher manuellement la mise à jour de l'affichage du plan
            if (cmbLots.SelectedItem is Lot selectedLot)
            {
                UpdatePlanDisplay(selectedLot);
            }
        }

        private void cmbLots_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            if (cmbLots.SelectedItem is Lot selectedLot)
            {
                UpdatePlanDisplay(selectedLot);
                LotSelectionChanged?.Invoke(this, selectedLot);
            }
        }

        private void UpdatePlanDisplay(Lot lot)
        {
            previewPlan.Image?.Dispose();
            previewPlan.Image = null;
            _tooltip.SetToolTip(previewPlan, "");

            if (lot == null || string.IsNullOrWhiteSpace(lot.CheminFichierPlan))
            {
                _tooltip.SetToolTip(previewPlan, "Aucun plan défini pour ce lot.");
                return;
            }

            string filePath = lot.CheminFichierPlan;
            if (!File.Exists(filePath))
            {
                _tooltip.SetToolTip(previewPlan, $"Fichier introuvable:\n{filePath}");
                return;
            }

            try
            {
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                if (new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" }.Contains(extension))
                {
                    previewPlan.Image = Image.FromFile(filePath);
                }
                _tooltip.SetToolTip(previewPlan, $"Cliquez pour ouvrir: {filePath}");
            }
            catch (Exception ex)
            {
                _tooltip.SetToolTip(previewPlan, $"Erreur chargement image:\n{ex.Message}");
            }
        }

        private void previewPlan_Click(object sender, EventArgs e)
        {
            if (cmbLots.SelectedItem is Lot selectedLot && !string.IsNullOrWhiteSpace(selectedLot.CheminFichierPlan))
            {
                string filePath = selectedLot.CheminFichierPlan;
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Utilise le ShellExecute pour ouvrir le fichier avec l'application par défaut
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Impossible d'ouvrir le fichier:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}