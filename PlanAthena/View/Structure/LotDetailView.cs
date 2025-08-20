using Krypton.Toolkit;
using PlanAthena.Data;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View.Structure
{
    public partial class LotDetailView : UserControl
    {
        private Lot _currentLot;
        private bool _isLoading;

        // Événement pour notifier le parent qu'une modification a eu lieu
        public event EventHandler LotChanged;

        public LotDetailView()
        {
            InitializeComponent();
            this.Load += LotDetailView_Load;
        }

        private void LotDetailView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            InitializeComboBoxPhases();
            AttachEvents();
            Clear();
        }

        private void InitializeComboBoxPhases()
        {
            var phasesValides = Enum.GetValues(typeof(ChantierPhase))
                                    .Cast<ChantierPhase>()
                                    .Where(p => p != ChantierPhase.None)
                                    .ToList();
            cmbPhases.DataSource = phasesValides;
        }

        private void AttachEvents()
        {
            textName.TextChanged += OnDetailChanged;
            numPriority.ValueChanged += OnDetailChanged;
            cmbPhases.SelectedIndexChanged += OnDetailChanged;
            textPlanPath.TextChanged += OnDetailChanged;
        }

        /// <summary>
        /// Charge les informations d'un lot dans le formulaire.
        /// </summary>
        public void LoadLot(Lot lot)
        {
            _isLoading = true;
            _currentLot = lot;

            if (lot != null)
            {
                textId.Text = lot.LotId;
                textName.Text = lot.Nom;
                numPriority.Value = lot.Priorite;
                cmbPhases.SelectedItem = lot.Phases;
                textPlanPath.Text = lot.CheminFichierPlan;
                LoadPlanImage(lot.CheminFichierPlan);
                this.Enabled = true;
            }
            else
            {
                Clear();
            }
            _isLoading = false;
        }

        /// <summary>
        /// Vide et désactive le formulaire.
        /// </summary>
        public void Clear()
        {
            _isLoading = true;
            _currentLot = null;
            textId.Clear();
            textName.Clear();
            numPriority.Value = 50;
            cmbPhases.SelectedIndex = -1;
            textPlanPath.Clear();
            previewPlan.Image = null;
            this.Enabled = false;
            _isLoading = false;
        }

        private void OnDetailChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentLot == null) return;

            // Mettre à jour l'objet Lot en mémoire
            _currentLot.Nom = textName.Text;
            _currentLot.Priorite = (int)numPriority.Value;
            if (cmbPhases.SelectedItem is ChantierPhase phase)
            {
                _currentLot.Phases = phase;
            }
            _currentLot.CheminFichierPlan = textPlanPath.Text;

            // Lever l'événement pour notifier le parent (sauvegarde automatique)
            LotChanged?.Invoke(this, EventArgs.Empty);
        }

        private void btnBrowsePlan_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Tous les fichiers (*.*)|*.*";
                ofd.Title = "Sélectionner un plan";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textPlanPath.Text = ofd.FileName;
                    LoadPlanImage(ofd.FileName);
                }
            }
        }

        private void LoadPlanImage(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                {
                    previewPlan.Image = System.Drawing.Image.FromFile(imagePath);
                }
                else
                {
                    previewPlan.Image = null;
                }
            }
            catch
            {
                previewPlan.Image = null; // En cas d'erreur de chargement
            }
        }
    }
}