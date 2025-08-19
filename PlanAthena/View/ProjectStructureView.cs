using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View
{
    public partial class ProjectStructureView : UserControl
    {
        private readonly ApplicationService _applicationService;
        private readonly ProjetService _projetService;

        // Stocke la liste "plate" pour l'affichage et la recherche
        private List<object> _structureItems = new List<object>();
        private object _selectedObject = null;
        private bool _isLoading = false;

        public event EventHandler<Type> NavigateToViewRequested;

        public ProjectStructureView(ApplicationService applicationService, ProjetService projetService)
        {
            InitializeComponent();
            _applicationService = applicationService;
            _projetService = projetService;

            this.Load += ProjectStructureView_Load;
        }

        private void ProjectStructureView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            SetupGrid();
            AttachEvents();
            RefreshAll();
        }

        private void SetupGrid()
        {
            gridStructure.AutoGenerateColumns = false;
            gridStructure.Columns.Clear();
            var nameColumn = new KryptonDataGridViewTextBoxColumn { Name = "StructureName", HeaderText = "Structure", DataPropertyName = "Name", FillWeight = 60 };
            nameColumn.DefaultCellStyle.Font = new Font("Segoe UI Emoji", 9); // Police pour les caractères spéciaux
            gridStructure.Columns.Add(nameColumn);
            gridStructure.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "Phase", HeaderText = "Phase", DataPropertyName = "Phase", FillWeight = 20 });
            gridStructure.Columns.Add(new KryptonDataGridViewTextBoxColumn { Name = "Priority", HeaderText = "Priorité", DataPropertyName = "Priority", FillWeight = 20 });
        }

        private void AttachEvents()
        {
            lotDetailView1.LotChanged += (s, e) => {
                _projetService.ModifierLot((Lot)_selectedObject);
                RefreshAll();
            };
            blocDetailView1.BlocChanged += (s, e) => {
                _projetService.ModifierBloc((Bloc)_selectedObject);
                RefreshAll();
            };
        }

        #region Logique de rafraîchissement

        private void RefreshAll()
        {
            BuildStructureList();
            ApplyFilter();
            UpdateDetailUI();
        }

        private void BuildStructureList()
        {
            _structureItems.Clear();
            var lots = _projetService.ObtenirTousLesLots();
            foreach (var lot in lots)
            {
                _structureItems.Add(lot);
                foreach (var bloc in lot.Blocs.OrderBy(b => b.Nom))
                {
                    _structureItems.Add(bloc);
                }
            }
        }
        public class StructureDisplayItem
        {
            public object Data { get; set; } // Pour stocker le Lot ou le Bloc original
            public string Name { get; set; }
            public string Phase { get; set; }
            public string Priority { get; set; }
        }
        private void ApplyFilter()
        {
            _isLoading = true;

            var filter = textSearch.Text.ToLowerInvariant();
            List<object> structureSource = _structureItems;
            List<object> itemsToDisplay;

            if (string.IsNullOrWhiteSpace(filter))
            {
                itemsToDisplay = structureSource;
            }
            else
            {
                var matchedItems = structureSource
                    .Where(item => (item is Lot lot && lot.Nom.ToLowerInvariant().Contains(filter))
                                || (item is Bloc bloc && bloc.Nom.ToLowerInvariant().Contains(filter)))
                    .ToList();

                var parentLotsOfMatchedBlocs = matchedItems.OfType<Bloc>()
                    .Select(b => _projetService.ObtenirLotParId(b.LotId))
                    .Distinct();

                var finalSet = new HashSet<object>(matchedItems.Union(parentLotsOfMatchedBlocs));

                itemsToDisplay = new List<object>();
                foreach (var lot in structureSource.OfType<Lot>())
                {
                    if (finalSet.Contains(lot))
                    {
                        itemsToDisplay.Add(lot);
                        itemsToDisplay.AddRange(lot.Blocs.Where(b => finalSet.Contains(b)).OrderBy(b => b.Nom));
                    }
                }
            }

            // --- LA CORRECTION EST ICI ---
            // On transforme notre liste d'objets en une liste de notre nouvelle classe d'affichage
            var displayList = new List<StructureDisplayItem>();
            foreach (var item in itemsToDisplay)
            {
                if (item is Lot lot)
                {
                    displayList.Add(new StructureDisplayItem
                    {
                        Data = lot,
                        Name = lot.Nom,
                        Phase = lot.Phases.ToString(),
                        Priority = lot.Priorite.ToString()
                    });
                }
                else if (item is Bloc bloc)
                {
                    displayList.Add(new StructureDisplayItem
                    {
                        Data = bloc,
                        Name = $"  ↳ {bloc.Nom}",
                        Phase = "",
                        Priority = ""
                    });
                }
            }

            gridStructure.DataSource = new BindingSource { DataSource = displayList };

            if (gridStructure.Rows.Count > 0)
            {
                gridStructure.ClearSelection();
            }

            _isLoading = false;

            UpdateDetailUI();
        }

        private void UpdateDetailUI()
        {
            if (gridStructure.SelectedRows.Count > 0)
            {
                dynamic selectedRowData = gridStructure.SelectedRows[0].DataBoundItem;
                _selectedObject = selectedRowData.Data;
            }
            else
            {
                _selectedObject = null;
            }

            if (_selectedObject is Lot selectedLot)
            {
                lotDetailView1.LoadLot(selectedLot);
                lotDetailView1.Visible = true;
                blocDetailView1.Clear();
                blocDetailView1.Visible = false;
            }
            else if (_selectedObject is Bloc selectedBloc)
            {
                blocDetailView1.LoadBloc(selectedBloc);
                blocDetailView1.Visible = true;
                lotDetailView1.Clear();
                lotDetailView1.Visible = false;
            }
            else
            {
                lotDetailView1.Clear();
                lotDetailView1.Visible = false;
                blocDetailView1.Clear();
                blocDetailView1.Visible = false;
            }
            btnDelete.Enabled = (_selectedObject != null);
        }

        #endregion

        #region Événements des contrôles

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void gridStructure_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdateDetailUI();
        }

        private void btnNewLot_Click(object sender, EventArgs e)
        {
            var newLot = _projetService.CreerLot();
            RefreshAll();
            // TODO: Sélectionner le nouveau lot dans la grille
        }

        private void btnNewBloc_Click(object sender, EventArgs e)
        {
            string parentLotId = null;
            if (_selectedObject is Lot lot) parentLotId = lot.LotId;
            if (_selectedObject is Bloc bloc) parentLotId = bloc.LotId;

            if (parentLotId == null)
            {
                MessageBox.Show("Veuillez sélectionner un Lot avant de créer un Bloc.", "Action requise", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var newBloc = _projetService.CreerBloc(parentLotId);
            RefreshAll();
            // TODO: Sélectionner le nouveau bloc dans la grille
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedObject == null) return;

            try
            {
                if (_selectedObject is Lot lot)
                {
                    _projetService.SupprimerLot(lot.LotId);
                }
                else if (_selectedObject is Bloc bloc)
                {
                    _projetService.SupprimerBloc(bloc.BlocId);
                }

                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Impossible de supprimer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnManageTasks_Click(object sender, EventArgs e)
        {
            NavigateToViewRequested?.Invoke(this, typeof(TaskManagerView));
        }

        #endregion
    }
}