using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View
{
    public partial class RessourceMetierView : UserControl
    {
        private readonly RessourceService _ressourceService;
        private readonly ProjetService _projetService;

        // Événement pour demander la navigation vers une autre vue
        public event EventHandler<Type> NavigateToViewRequested;

        private bool _isLoading = false;

        public RessourceMetierView(RessourceService ressourceService, ProjetService projetService)
        {
            InitializeComponent();
            _ressourceService = ressourceService;
            _projetService = projetService;

            this.Load += RessourceMetierView_Load;
        }

        private void RessourceMetierView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            SetupGrid();
            AttachEvents();
            RefreshAll();
        }

        private void SetupGrid()
        {
            gridMetiers.AutoGenerateColumns = false;
            gridMetiers.Columns.Clear();
            gridMetiers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "MetierId", HeaderText = "ID", DataPropertyName = "MetierId", FillWeight = 20, ReadOnly = true });
            gridMetiers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "Nom", HeaderText = "Nom", DataPropertyName = "Nom", FillWeight = 60, ReadOnly = true });
            gridMetiers.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "Phases", HeaderText = "Phases", DataPropertyName = "Phases", FillWeight = 20, ReadOnly = true });
        }

        private void AttachEvents()
        {
            gridMetiers.SelectionChanged += gridMetiers_SelectionChanged;
            btnNewMetier.Click += btnNewMetier_Click;
            btnDeleteMetier.Click += btnDeleteMetier_Click;
            btnChooseColor.Click += btnChooseColor_Click;

            textName.TextChanged += Detail_Changed;
            textPictogram.TextChanged += Detail_Changed;
            chkGrosOeuvre.CheckedChanged += Detail_Changed;
            chkSecondOeuvre.CheckedChanged += Detail_Changed;
            chkFinition.CheckedChanged += Detail_Changed;

            // Attacher l'événement pour le nouveau CTA
            btnConfigurePrerequis.Click += BtnConfigurePrerequis_Click;
        }

        #region Logique de rafraîchissement

        private void RefreshAll()
        {
            var selectedId = GetSelectedMetierId();
            RefreshGrid();
            SelectMetierInGrid(selectedId);
            RefreshUIFromSelection();
        }

        private void RefreshUIFromSelection()
        {
            RefreshDetails();
            UpdateButtonStates();
        }

        private void RefreshGrid()
        {
            _isLoading = true;
            // Utiliser une BindingSource améliore les performances et la gestion de la sélection
            var metiers = _ressourceService.GetAllMetiers();
            var bindingSource = new BindingSource { DataSource = metiers };
            gridMetiers.DataSource = bindingSource;
            _isLoading = false;
        }

        private void RefreshDetails()
        {
            _isLoading = true;
            var metier = GetSelectedMetier();
            if (metier != null)
            {
                textId.Text = metier.MetierId;
                textName.Text = metier.Nom;
                textPictogram.Text = metier.Pictogram;
                panelColor.StateCommon.Color1 = _ressourceService.GetDisplayColorForMetier(metier.MetierId);

                chkGrosOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.GrosOeuvre);
                chkSecondOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.SecondOeuvre);
                chkFinition.Checked = metier.Phases.HasFlag(ChantierPhase.Finition);
            }
            else
            {
                textId.Clear();
                textName.Clear();
                textPictogram.Clear();
                panelColor.StateCommon.Color1 = SystemColors.Control;
                chkGrosOeuvre.Checked = false;
                chkSecondOeuvre.Checked = false;
                chkFinition.Checked = false;
            }
            _isLoading = false;
        }

        private void UpdateButtonStates()
        {
            bool metierSelected = GetSelectedMetier() != null;
            groupDetails.Enabled = metierSelected;
            btnDeleteMetier.Enabled = metierSelected;
        }

        #endregion

        #region Helpers

        private string GetSelectedMetierId()
        {
            if (gridMetiers.SelectedRows.Count > 0 && gridMetiers.SelectedRows[0].DataBoundItem is Metier metier)
            {
                return metier.MetierId;
            }
            return null;
        }

        private Metier GetSelectedMetier()
        {
            var id = GetSelectedMetierId();
            return id != null ? _ressourceService.GetMetierById(id) : null;
        }

        private void SelectMetierInGrid(string metierId)
        {
            if (metierId == null) return;
            _isLoading = true;
            foreach (DataGridViewRow row in gridMetiers.Rows)
            {
                if (row.DataBoundItem is Metier metier && metier.MetierId == metierId)
                {
                    row.Selected = true;
                    gridMetiers.FirstDisplayedScrollingRowIndex = row.Index;
                    _isLoading = false;
                    return;
                }
            }
            _isLoading = false;
        }

        #endregion

        #region Événements des contrôles

        private void gridMetiers_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            RefreshUIFromSelection();
        }

        private void Detail_Changed(object sender, EventArgs e)
        {
            if (_isLoading) return;
            var metier = GetSelectedMetier();
            if (metier == null) return;

            metier.Nom = textName.Text;
            metier.Pictogram = textPictogram.Text;

            ChantierPhase phases = ChantierPhase.None;
            if (chkGrosOeuvre.Checked) phases |= ChantierPhase.GrosOeuvre;
            if (chkSecondOeuvre.Checked) phases |= ChantierPhase.SecondOeuvre;
            if (chkFinition.Checked) phases |= ChantierPhase.Finition;
            metier.Phases = phases;

            _ressourceService.ModifierMetier(metier);

            // Rafraîchir la grille pour refléter les changements
            gridMetiers.Refresh();
        }

        private void btnNewMetier_Click(object sender, EventArgs e)
        {
            var nouveauMetier = _ressourceService.CreerMetier();
            RefreshAll();
            SelectMetierInGrid(nouveauMetier.MetierId);
            textName.Focus();
        }

        private void btnDeleteMetier_Click(object sender, EventArgs e)
        {
            var metier = GetSelectedMetier();
            if (metier == null) return;

            try
            {
                _ressourceService.SupprimerMetier(metier.MetierId, _projetService);
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Impossible de supprimer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnChooseColor_Click(object sender, EventArgs e)
        {
            var metier = GetSelectedMetier();
            if (metier == null) return;

            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = _ressourceService.GetDisplayColorForMetier(metier.MetierId);
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    metier.CouleurHex = ColorTranslator.ToHtml(colorDialog.Color);
                    _ressourceService.ModifierMetier(metier);
                    panelColor.StateCommon.Color1 = colorDialog.Color;
                }
            }
        }

        private void BtnConfigurePrerequis_Click(object sender, EventArgs e)
        {
            // On lève un événement pour demander au Shell de naviguer
            NavigateToViewRequested?.Invoke(this, typeof(PrerequisMetierView));
        }

        #endregion
    }
}