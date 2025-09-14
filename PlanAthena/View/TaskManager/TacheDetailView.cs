using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager
{
    public partial class TacheDetailView : UserControl
    {
        // Services
        private ProjetService _projetService;
        private TaskManagerService _taskManagerService;
        private RessourceService _ressourceService;
        private TaskManagerView _parentView;

        // État interne
        private Tache _currentTache;
        private bool _isLoading = false;
        private List<Bloc> _availableBlocs;
        private ToolTip _tooltip = new ToolTip();

        // Événements publics
        public event EventHandler<Tache> SaveRequested;
        public event EventHandler<Tache> TacheDeleteRequested;
        public event EventHandler<Lot> LotSelectionChanged;

        /// <summary>
        /// Constructeur public vide, nécessaire pour le Designer de Windows Forms.
        /// </summary>
        public TacheDetailView()
        {
            InitializeComponent();
            InitializeLotSelection();
        }

        /// <summary>
        /// Initialise le contrôle avec les services nécessaires après sa création.
        /// </summary>
        public void InitializeServices(ProjetService projetService, TaskManagerService taskManagerService, RessourceService ressourceService, TaskManagerView parentView)
        {
            _projetService = projetService;
            _taskManagerService = taskManagerService;
            _ressourceService = ressourceService;
            _parentView = parentView;
            AttachEvents();
            Clear();
        }

        private void AttachEvents()
        {
            cmbBlocNom.SelectedIndexChanged += CmbBlocNom_SelectedIndexChanged;
            cmbMetier.SelectedIndexChanged += CmbMetier_SelectedIndexChanged;
            chkIsJalon.CheckedChanged += ChkIsJalon_CheckedChanged;
            numHeuresHomme.ValueChanged += NumHeuresHomme_ValueChanged;
            chkListDependances.ItemCheck += ChkListDependances_ItemCheck;
            btnSauvegarder.Click += BtnSauvegarder_Click;
            btnSupprimer.Click += (s, e) => { if (_currentTache != null) TacheDeleteRequested?.Invoke(this, _currentTache); };
        }

        private void InitializeLotSelection()
        {
            cmbLots.SelectedIndexChanged += cmbLots_SelectedIndexChanged;
        }

        #region Logique de Sélection du Lot

        /// <summary>
        /// Remplit la liste déroulante des lots.
        /// </summary>
        public void PopulateLots(IEnumerable<Lot> lots)
        {
            _isLoading = true;
            var currentSelection = cmbLots.SelectedValue;
            cmbLots.DataSource = lots.ToList();
            cmbLots.DisplayMember = "Nom";
            cmbLots.ValueMember = "LotId";
            if (currentSelection != null && lots.Any(l => l.LotId == (string)currentSelection))
            {
                cmbLots.SelectedValue = currentSelection;
            }
            else
            {
                cmbLots.SelectedIndex = -1;
            }
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
                previewPlan.Image = Image.FromFile(filePath);
                _tooltip.SetToolTip(previewPlan, $"Cliquez pour ouvrir: {filePath}");
            }
            catch (Exception ex)
            {
                _tooltip.SetToolTip(previewPlan, $"Erreur chargement image:\n{ex.Message}");
            }
        }

        #endregion

        #region Logique de Détail de la Tâche

        /// <summary>
        /// Met à jour les listes déroulantes (Métiers, Blocs) en fonction du lot actif.
        /// </summary>
        public void UpdateDropdowns(string lotId)
        {
            _isLoading = true;
            try
            {
                var metiersPourListe = new List<Metier> { new Metier { MetierId = "", Nom = "(Aucun)" } };
                metiersPourListe.AddRange(_ressourceService.GetAllMetiers().OrderBy(m => m.Nom));
                cmbMetier.DataSource = metiersPourListe;
                cmbMetier.DisplayMember = "Nom";
                cmbMetier.ValueMember = "MetierId";

                _availableBlocs = string.IsNullOrEmpty(lotId) ? new List<Bloc>() : _projetService.ObtenirBlocsParLot(lotId);
                cmbBlocNom.DataSource = _availableBlocs;
                cmbBlocNom.DisplayMember = "Nom";
                cmbBlocNom.ValueMember = "BlocId";
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Charge les informations d'une tâche spécifique dans les champs de l'interface.
        /// </summary>
        public void LoadTache(Tache tache)
        {
            _isLoading = true;
            _currentTache = tache;

            if (_currentTache == null)
            {
                ClearTacheDetails();
                _isLoading = false;
                return;
            }

            txtTacheNom.Text = _currentTache.TacheNom;
            numHeuresHomme.Value = Math.Max(numHeuresHomme.Minimum, Math.Min(numHeuresHomme.Maximum, _currentTache.HeuresHommeEstimees));
            chkIsJalon.Checked = _currentTache.EstJalon;
            cmbBlocNom.SelectedValue = _availableBlocs.Any(b => b.BlocId == _currentTache.BlocId) ? _currentTache.BlocId : "";
            cmbMetier.SelectedValue = !string.IsNullOrEmpty(_currentTache.MetierId) ? _currentTache.MetierId : "";
            cmbMetier.Enabled = !_currentTache.EstJalon;

            _isLoading = false;
            LoadDependencies();
        }

        /// <summary>
        /// Vide complètement le contrôle, y compris la sélection du lot.
        /// </summary>
        public void Clear()
        {
            _isLoading = true;
            cmbLots.SelectedIndex = -1;
            previewPlan.Image = null;
            ClearTacheDetails();
            _isLoading = false;
        }

        /// <summary>
        /// Vide uniquement les champs relatifs à la tâche, en conservant la sélection du lot.
        /// </summary>
        public void ClearTacheDetails()
        {
            _isLoading = true;
            _currentTache = null;
            txtTacheNom.Clear();
            numHeuresHomme.Value = numHeuresHomme.Minimum;
            chkIsJalon.Checked = false;
            cmbMetier.SelectedIndex = -1;
            cmbBlocNom.SelectedIndex = -1;
            chkListDependances.Items.Clear();
            _isLoading = false;
        }

        private void LoadDependencies()
        {
            _isLoading = true;
            chkListDependances.Items.Clear();

            // On délègue le calcul au parent
            if (_parentView != null && _currentTache != null)
            {
                var items = _parentView.GetDependancesForTache(_currentTache);
                foreach (var displayItem in items)
                {
                    bool isChecked = displayItem.OriginalData.Etat == EtatDependance.Stricte ||
                                     displayItem.OriginalData.Etat == EtatDependance.Suggeree;

                    int index = chkListDependances.Items.Add(displayItem);
                    chkListDependances.SetItemChecked(index, isChecked);
                }
            }

            _isLoading = false;
        }

        private void CmbBlocNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;
            if (cmbBlocNom.SelectedItem is Bloc selectedBloc)
            {
                _currentTache.BlocId = selectedBloc.BlocId;
                LoadDependencies();
            }
        }

        private void CmbMetier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null || _currentTache.EstJalon) return;
            _currentTache.MetierId = cmbMetier.SelectedValue as string ?? "";
            LoadDependencies();
        }

        private void ChkIsJalon_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;
            bool estJalon = chkIsJalon.Checked;
            cmbMetier.Enabled = !estJalon;
            if (estJalon)
            {
                _currentTache.Type = TypeActivite.JalonUtilisateur;
                cmbMetier.SelectedValue = "";
            }
            else
            {
                _currentTache.Type = TypeActivite.Tache;
            }
            LoadDependencies();
        }

        private void NumHeuresHomme_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;
            if (chkIsJalon.Checked && (string.IsNullOrEmpty(txtTacheNom.Text) || txtTacheNom.Text.StartsWith("Attente")))
            {
                txtTacheNom.Text = $"Attente {numHeuresHomme.Value} heures";
            }
        }

        private void ChkListDependances_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_isLoading) return;
            if (chkListDependances.Items[e.Index] is DependanceDisplayItem item)
            {
                item.UpdateIsChecked(e.NewValue == CheckState.Checked);
            }
        }

        private void BtnSauvegarder_Click(object sender, EventArgs e)
        {
            if (_currentTache == null) return;

            _currentTache.TacheNom = txtTacheNom.Text;
            _currentTache.HeuresHommeEstimees = (int)numHeuresHomme.Value;

            var dependancesStricts = new List<string>();
            var exclusions = new List<string>();

            var nouvellesDependances = new List<string>();
            var nouvellesExclusions = new List<string>();

            for (int i = 0; i < chkListDependances.Items.Count; i++)
            {
                var item = (DependanceDisplayItem)chkListDependances.Items[i];
                bool estCocheeMaintenant = chkListDependances.GetItemChecked(i);
                var tacheIdPredecesseur = item.OriginalData.TachePredecesseur.TacheId;

                bool etaitDependanceAvant = item.OriginalData.Etat == EtatDependance.Stricte ||
                                            item.OriginalData.Etat == EtatDependance.Suggeree;

                if (estCocheeMaintenant)
                {
                    nouvellesDependances.Add(tacheIdPredecesseur);
                }
                else
                {
                    if (etaitDependanceAvant)
                    {
                        nouvellesExclusions.Add(tacheIdPredecesseur);
                    }
                }
            }

            _currentTache.Dependencies = string.Join(",", nouvellesDependances.Distinct());
            _currentTache.ExclusionsDependances = string.Join(",", nouvellesExclusions.Distinct());

            SaveRequested?.Invoke(this, _currentTache);
        }

        #endregion
    }

    public class DependanceDisplayItem
    {
        public DependanceAffichage OriginalData { get; }
        private bool _isChecked;

        public DependanceDisplayItem(DependanceAffichage originalData)
        {
            OriginalData = originalData;
            _isChecked = (originalData.Etat == EtatDependance.Stricte || originalData.Etat == EtatDependance.Suggeree);
        }

        public void UpdateIsChecked(bool isChecked)
        {
            _isChecked = isChecked;
        }

        public override string ToString()
        {
            // Cette logique est simplifiée car le dessin personnalisé n'est plus utilisé.
            // On peut l'améliorer plus tard si nécessaire.
            string checkState = _isChecked ? "[x] " : "[ ] ";
            string type = OriginalData.EstHeritee ? "(Héritée) " : "(Directe) ";
            return $"{checkState}{type}{OriginalData.TachePredecesseur.TacheNom}";
        }
    }
}