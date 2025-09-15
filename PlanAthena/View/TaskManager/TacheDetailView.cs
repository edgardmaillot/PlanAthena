// Fichier: PlanAthena/View/TaskManager/TacheDetailView.cs
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.View.TaskManager.PertDiagram;
using PlanAthena.View.TaskManager.Utilitaires;
using System.IO;

namespace PlanAthena.View.TaskManager
{
    public partial class TacheDetailView : UserControl
    {
        #region Champs et Services

        // Services
        private ProjetService _projetService;
        private TaskManagerService _taskManagerService;
        private RessourceService _ressourceService;
        private TaskManagerView _parentView;
        private PertDiagramSettings _settings;
        private TacheDetailViewController _logicController;

        // État interne
        private Tache _currentTache;
        private bool _isLoading = false;
        private List<Bloc> _availableBlocs;
        private readonly ToolTip _tooltip = new ToolTip();
        private readonly ToolTip _readOnlyTooltip = new ToolTip();

        #endregion

        #region Événements Publics

        public event EventHandler<Tache> SaveRequested;
        public event EventHandler<Tache> TacheDeleteRequested;
        public event EventHandler<Lot> LotSelectionChanged;

        #endregion

        #region Initialisation et Cycle de Vie

        /// <summary>
        /// Constructeur public vide, nécessaire pour le Designer de Windows Forms.
        /// </summary>
        public TacheDetailView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialise le contrôle avec les services et le contrôleur de logique nécessaires.
        /// </summary>
        public void InitializeServices(ProjetService projetService, TaskManagerService taskManagerService, RessourceService ressourceService, TaskManagerView parentView, PertDiagramSettings pertSettings, TacheDetailViewController logicController)
        {
            _projetService = projetService;
            _taskManagerService = taskManagerService;
            _ressourceService = ressourceService;
            _parentView = parentView;
            _settings = pertSettings;
            _logicController = logicController;

            AttachEvents();
            Clear();
        }

        /// <summary>
        /// Abonne les gestionnaires d'événements aux contrôles de l'interface utilisateur.
        /// </summary>
        private void AttachEvents()
        {
            // Sélection du lot
            cmbLots.SelectedIndexChanged += OnLotSelectionChanged;

            // Champs de détail de la tâche
            cmbBlocNom.SelectedIndexChanged += OnDetailChanged;
            cmbMetier.SelectedIndexChanged += OnDetailChanged;
            chkIsJalon.CheckedChanged += OnDetailChanged;
            numHeuresHomme.ValueChanged += OnDetailChanged;
            chkListDependances.ItemCheck += OnDetailChanged;

            // Actions principales
            btnSauvegarder.Click += BtnSauvegarder_Click;
            btnSupprimer.Click += BtnSupprimer_Click;
        }

        #endregion

        #region Logique de Chargement et d'Affichage

        /// <summary>
        /// Charge les informations d'une tâche spécifique dans l'interface.
        /// Applique également les règles métier (ex: lecture seule) via le contrôleur.
        /// </summary>
        public void LoadTache(Tache tache)
        {
            _isLoading = true;
            _currentTache = tache;

            if (_currentTache == null || _settings == null || _logicController == null)
            {
                ClearTacheDetails();
                _isLoading = false;
                return;
            }

            // Délégation des règles métier au contrôleur
            bool isReadOnly = _logicController.IsTacheReadOnly(_currentTache);
            _logicController.ApplyReadOnlyStateToControls(this, isReadOnly);
            if (isReadOnly)
                _readOnlyTooltip.SetToolTip(this, $"Modification impossible : la tâche est '{_currentTache.Statut}'.");
            else
                _readOnlyTooltip.SetToolTip(this, string.Empty);


            // Remplissage des champs de l'UI
            txtTacheNom.Text = _currentTache.TacheNom;
            numHeuresHomme.Value = Math.Max(numHeuresHomme.Minimum, Math.Min(numHeuresHomme.Maximum, _currentTache.HeuresHommeEstimees));
            chkIsJalon.Checked = _currentTache.EstJalon;
            cmbBlocNom.SelectedValue = _availableBlocs.Any(b => b.BlocId == _currentTache.BlocId) ? _currentTache.BlocId : "";
            cmbMetier.SelectedValue = !string.IsNullOrEmpty(_currentTache.MetierId) ? _currentTache.MetierId : "";

            UpdateStatutDisplay();
            LoadDependencies();

            _isLoading = false;
        }

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
            lblStatutPicto.Text = "";
            lblStatutValeur.Text = "";

            // Assure la réactivation des contrôles
            if (_logicController != null)
            {
                _logicController.ApplyReadOnlyStateToControls(this, false);
                _readOnlyTooltip.SetToolTip(this, string.Empty);
            }

            _isLoading = false;
        }

        #endregion

        #region Gestion des Événements UI

        /// <summary>
        /// Gère le clic sur le bouton de sauvegarde.
        /// Valide l'action via le contrôleur avant de lever l'événement SaveRequested.
        /// </summary>
        private void BtnSauvegarder_Click(object sender, EventArgs e)
        {
            if (_currentTache == null || _logicController == null) return;

            // Le contrôleur gère la confirmation si nécessaire
            if (_logicController.ConfirmAndInvalidateIfNeeded(_currentTache))
            {
                ApplyChangesToTache();
                SaveRequested?.Invoke(this, _currentTache);
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton de suppression.
        /// Valide l'action via le contrôleur avant de lever l'événement TacheDeleteRequested.
        /// </summary>
        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (_currentTache == null || _logicController == null) return;

            if (_logicController.ConfirmAndInvalidateIfNeeded(_currentTache))
            {
                TacheDeleteRequested?.Invoke(this, _currentTache);
            }
        }

        /// <summary>
        /// Gère le changement de sélection dans la liste des lots.
        /// </summary>
        private void OnLotSelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            if (cmbLots.SelectedItem is Lot selectedLot)
            {
                UpdatePlanDisplay(selectedLot);
                LotSelectionChanged?.Invoke(this, selectedLot);
            }
        }

        /// <summary>
        /// Gère les modifications des champs de détail pour mettre à jour l'état interne.
        /// </summary>
        private void OnDetailChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;

            // Mettre à jour les dépendances si le bloc, le métier ou le type change
            if (sender == cmbBlocNom || sender == cmbMetier || sender == chkIsJalon)
            {
                ApplyChangesToTache(); // Applique les changements partiels avant de recharger
                LoadDependencies();
            }
        }

        #endregion

        #region Méthodes Privées d'UI

        /// <summary>
        /// Applique les valeurs des contrôles de l'UI à l'objet _currentTache.
        /// </summary>
        private void ApplyChangesToTache()
        {
            if (_currentTache == null) return;

            _currentTache.TacheNom = txtTacheNom.Text;
            _currentTache.HeuresHommeEstimees = (int)numHeuresHomme.Value;
            _currentTache.BlocId = cmbBlocNom.SelectedValue as string ?? "";
            _currentTache.MetierId = cmbMetier.SelectedValue as string ?? "";
            _currentTache.Type = chkIsJalon.Checked ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;
            if (chkIsJalon.Checked) _currentTache.MetierId = "";

            // Mise à jour des dépendances
            var nouvellesDependances = new List<string>();
            var nouvellesExclusions = new List<string>();

            for (int i = 0; i < chkListDependances.Items.Count; i++)
            {
                if (chkListDependances.Items[i] is DependanceDisplayItem item)
                {
                    bool estCocheeMaintenant = chkListDependances.GetItemChecked(i);
                    var tacheIdPredecesseur = item.OriginalData.TachePredecesseur.TacheId;
                    bool etaitDependanceAvant = item.OriginalData.Etat == EtatDependance.Stricte || item.OriginalData.Etat == EtatDependance.Suggeree;

                    if (estCocheeMaintenant)
                    {
                        nouvellesDependances.Add(tacheIdPredecesseur);
                    }
                    else if (etaitDependanceAvant)
                    {
                        nouvellesExclusions.Add(tacheIdPredecesseur);
                    }
                }
            }

            _currentTache.Dependencies = string.Join(",", nouvellesDependances.Distinct());
            _currentTache.ExclusionsDependances = string.Join(",", nouvellesExclusions.Distinct());
        }

        /// <summary>
        /// Remplit la liste des dépendances pour la tâche actuelle.
        /// </summary>
        private void LoadDependencies()
        {
            _isLoading = true;
            chkListDependances.Items.Clear();

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

        /// <summary>
        /// Met à jour l'affichage de l'icône et du texte du statut.
        /// </summary>
        private void UpdateStatutDisplay()
        {
            if (_currentTache == null || _settings == null) return;

            string statutIcon;
            switch (_currentTache.Statut)
            {
                case Statut.Estimée: statutIcon = _settings.StatutIconEstimee; break;
                case Statut.Planifiée: statutIcon = _settings.StatutIconPlanifiee; break;
                case Statut.EnCours: statutIcon = _settings.StatutIconEnCours; break;
                case Statut.Terminée: statutIcon = _settings.StatutIconTerminee; break;
                case Statut.EnRetard: statutIcon = _settings.StatutIconEnRetard; break;
                default: statutIcon = _settings.StatutIconDefault; break;
            }
            lblStatutPicto.Text = statutIcon;
            lblStatutValeur.Text = _currentTache.Statut.ToString();
        }

        /// <summary>
        /// Met à jour les listes déroulantes (Métiers, Blocs) en fonction du lot actif.
        /// </summary>
        public void UpdateDropdowns(string lotId)
        {
            _isLoading = true;
            var metiersPourListe = new List<Metier> { new Metier { MetierId = "", Nom = "(Aucun)" } };
            metiersPourListe.AddRange(_ressourceService.GetAllMetiers().OrderBy(m => m.Nom));
            cmbMetier.DataSource = metiersPourListe;
            cmbMetier.DisplayMember = "Nom";
            cmbMetier.ValueMember = "MetierId";

            _availableBlocs = string.IsNullOrEmpty(lotId) ? new List<Bloc>() : _projetService.ObtenirBlocsParLot(lotId);
            cmbBlocNom.DataSource = _availableBlocs;
            cmbBlocNom.DisplayMember = "Nom";
            cmbBlocNom.ValueMember = "BlocId";
            _isLoading = false;
        }

        /// <summary>
        /// Affiche l'image du plan associée au lot sélectionné.
        /// </summary>
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

            if (File.Exists(lot.CheminFichierPlan))
            {
                try
                {
                    previewPlan.Image = Image.FromFile(lot.CheminFichierPlan);
                    _tooltip.SetToolTip(previewPlan, $"Cliquez pour ouvrir: {lot.CheminFichierPlan}");
                }
                catch (Exception ex)
                {
                    _tooltip.SetToolTip(previewPlan, $"Erreur chargement image:\n{ex.Message}");
                }
            }
            else
            {
                _tooltip.SetToolTip(previewPlan, $"Fichier introuvable:\n{lot.CheminFichierPlan}");
            }
        }

        #endregion
    }
}