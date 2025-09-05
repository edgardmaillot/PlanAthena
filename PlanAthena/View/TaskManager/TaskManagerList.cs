// --- START OF FILE TaskManagerList.cs (CORRIGÉ) ---

using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager
{
    public partial class TaskManagerList : UserControl
    {
        private ProjetService _projetService;
        private TaskManagerService _taskManagerService;
        private RessourceService _ressourceService;

        private List<Tache> _allTasks;
        private Tache _selectedTache;
        private bool _isLoading = false;

        // Cet événement est maintenant la SEULE sortie de ce contrôle.
        public event EventHandler<Tache> TacheSelectionChanged;

        public TaskManagerList()
        {
            InitializeComponent();
        }

        public void InitializeServices(ProjetService projetService, TaskManagerService taskManagerService, RessourceService ressourceService)
        {
            _projetService = projetService;
            _taskManagerService = taskManagerService;
            _ressourceService = ressourceService;
            this.Load += TaskManagerList_Load;
        }

        private void TaskManagerList_Load(object sender, EventArgs e)
        {
            if (DesignMode || _projetService == null) return;
            AttachEvents();
            RefreshAll();
        }

        private void AttachEvents()
        {
            kryptonDataGridView1.SelectionChanged += DataGridView_SelectionChanged;
            // --- SUPPRESSION : Plus d'événements de la vue de détail à écouter ---
        }

        public void RefreshAll()
        {
            _isLoading = true;
            string selectedTaskId = _selectedTache?.TacheId;
            _allTasks = _taskManagerService.ObtenirToutesLesTaches();
            PopulateGrid();
            SelectTaskInGrid(selectedTaskId);
            _isLoading = false;

            // --- SUPPRESSION : Ce contrôle ne met plus à jour la vue de détail ---
            // UpdateDetailView(); 
        }

        private void PopulateGrid()
        {
            kryptonDataGridView1.Rows.Clear();
            if (_allTasks == null) return;

            var lots = _projetService.ObtenirTousLesLots().ToDictionary(l => l.LotId, l => l);
            var blocs = lots.Values.SelectMany(l => l.Blocs).ToDictionary(b => b.BlocId, b => b);
            var metiers = _ressourceService.GetAllMetiers().ToDictionary(m => m.MetierId, m => m.Nom);

            foreach (var tache in _allTasks.OrderBy(t => t.LotId).ThenBy(t => t.BlocId).ThenBy(t => t.TacheNom))
            {
                var rowIndex = kryptonDataGridView1.Rows.Add();
                var row = kryptonDataGridView1.Rows[rowIndex];
                row.Cells["DG_Lot"].Value = lots.TryGetValue(tache.LotId, out var lot) ? lot.Nom : "N/A";
                row.Cells["DG_Bloc"].Value = blocs.TryGetValue(tache.BlocId, out var bloc) ? bloc.Nom : "N/A";
                row.Cells["DG_Tache"].Value = tache.TacheNom;
                row.Cells["DG_Metier"].Value = metiers.TryGetValue(tache.MetierId, out var metierNom) ? metierNom : "-";
                row.Cells["DG_Statut"].Value = tache.Statut.ToString();
                row.Tag = tache;
            }
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            _selectedTache = (kryptonDataGridView1.SelectedRows.Count > 0)
                ? kryptonDataGridView1.SelectedRows[0].Tag as Tache
                : null;

            // On notifie le parent (l'orchestrateur) qu'une sélection a eu lieu.
            TacheSelectionChanged?.Invoke(this, _selectedTache);
        }


        private void SelectTaskInGrid(string taskId)
        {
            if (string.IsNullOrEmpty(taskId)) return;

            foreach (DataGridViewRow row in kryptonDataGridView1.Rows)
            {
                if ((row.Tag as Tache)?.TacheId == taskId)
                {
                    row.Selected = true;
                    // Assurer que la ligne sélectionnée est visible
                    if (row.Displayed)
                        kryptonDataGridView1.FirstDisplayedScrollingRowIndex = row.Index > 5 ? row.Index - 5 : 0;
                    return;
                }
            }
        }
    }
}