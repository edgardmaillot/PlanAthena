// Fichier: PlanAthena/View/TaskManager/TacheDetailViewXL.cs
using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.View.TaskManager.Utilitaires;
using System.Data;
using System.Text;

namespace PlanAthena.View.TaskManager
{
    public partial class TacheDetailViewXL : UserControl
    {
        #region Champs et Services

        // Services
        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private TaskManagerService _taskManagerService;
        private TaskManagerView _parentView; // Optionnel, pour les dépendances
        private TacheDetailViewController _logicController;

        // État interne
        private Tache _currentTache;
        private bool _isLoading;
        private readonly ToolTip _readOnlyTooltip = new ToolTip();

        #endregion

        #region Événements Publics

        public event EventHandler<Tache> SaveRequested;
        public event EventHandler<Tache> TacheDeleteRequested;

        #endregion

        #region Initialisation et Cycle de Vie

        /// <summary>
        /// Constructeur public vide, nécessaire pour le Designer de Windows Forms.
        /// </summary>
        public TacheDetailViewXL()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialise le contrôle avec les services et le contrôleur de logique nécessaires.
        /// </summary>
        public void InitializeServices(ProjetService projetService, RessourceService ressourceService, TaskManagerService taskManagerService, TaskManagerView parentView, TacheDetailViewController logicController)
        {
            _projetService = projetService;
            _ressourceService = ressourceService;
            _taskManagerService = taskManagerService;
            _parentView = parentView;
            _logicController = logicController;

            AttachEvents();
            Clear();
        }

        /// <summary>
        /// Abonne les gestionnaires d'événements aux contrôles de l'interface utilisateur.
        /// </summary>
        private void AttachEvents()
        {
            // Champs de détail de la tâche
            cmbEtat.SelectedIndexChanged += OnDetailChanged;
            ChkOuvriersAffect.ItemCheck += OnDetailChanged;
            chkListDependances.ItemCheck += OnDetailChanged;
            // Note: les autres champs n'ont pas besoin de lever OnDetailChanged car leur valeur
            // est lue directement au moment de la sauvegarde.

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

            if (_currentTache == null || _logicController == null)
            {
                Clear();
                _isLoading = false;
                return;
            }

            // Délégation des règles métier au contrôleur
            bool isReadOnly = _logicController.IsTacheReadOnly(_currentTache);
            _logicController.ApplyReadOnlyStateToControls(this.kryptonPanel1, isReadOnly); // Cible le panel supérieur
            _logicController.ApplyReadOnlyStateToControls(this.kryptonPanel4, isReadOnly); // Cible le panel des boutons
            chkListDependances.Enabled = !isReadOnly; // Cible manuellement car dans un autre panel
            if (isReadOnly)
                _readOnlyTooltip.SetToolTip(this.kryptonPanel1, $"Modification impossible : la tâche est '{_currentTache.Statut}'.");
            else
                _readOnlyTooltip.SetToolTip(this.kryptonPanel1, string.Empty);

            // Remplissage des champs de l'UI
            kryptonHeader1.Values.Heading = $"Édition : tache.TacheNom";
            kryptonHeader1.Values.Description = $"ID: {tache.TacheId}";
            txtTacheNom.Text = tache.TacheNom;
            numHeuresHomme.Value = tache.HeuresHommeEstimees;
            chkIsJalon.Checked = tache.EstJalon;
            heureDebut.Text = tache.DateDebutPlanifiee?.ToString("dd/MM/yy HH:mm") ?? "N/A";
            heureFin.Text = tache.DateFinPlanifiee?.ToString("dd/MM/yy HH:mm") ?? "N/A";

            PopulateComboBoxes(); // Doit être appelé avant de définir les valeurs
            cmbBlocNom.SelectedValue = tache.BlocId ?? "";
            cmbMetier.SelectedValue = tache.MetierId ?? "";
            cmbEtat.SelectedItem = tache.Statut;

            UpdateStatutColor(tache.Statut);
            PopulateOuvriers(tache);
            LoadDependencies(tache);
            PopulateDebugInfo(tache);
            PopulateSousTaches(tache);

            _isLoading = false;
        }

        /// <summary>
        /// Vide complètement le contrôle.
        /// </summary>
        public void Clear()
        {
            _isLoading = true;
            _currentTache = null;

            kryptonHeader1.Values.Heading = "Détail de la tâche";
            kryptonHeader1.Values.Description = "Aucune tâche sélectionnée";
            txtTacheNom.Clear();
            numHeuresHomme.Value = 0;
            chkIsJalon.Checked = false;
            cmbBlocNom.DataSource = null;
            cmbMetier.DataSource = null;
            cmbEtat.DataSource = null;
            heureDebut.Clear();
            heureFin.Clear();
            ChkOuvriersAffect.Items.Clear();
            chkListDependances.Items.Clear();
            kryptonRichTextBox1.Clear();
            DataGridSousTaches.Rows.Clear();
            DataGridSousTaches.Visible = false;

            UpdateStatutColor(Statut.Estimée);

            // Assure la réactivation des contrôles
            if (_logicController != null)
            {
                _logicController.ApplyReadOnlyStateToControls(this, false);
                _readOnlyTooltip.SetToolTip(this.kryptonPanel1, string.Empty);
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
        /// Gère les modifications des champs de détail pour mettre à jour l'état interne.
        /// </summary>
        private void OnDetailChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;
            // Pour la vue XL, les changements sont appliqués uniquement à la sauvegarde.
            // On met juste à jour la couleur du statut si besoin.
            if (sender == cmbEtat && cmbEtat.SelectedItem is Statut newStatut)
            {
                UpdateStatutColor(newStatut);
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
            _currentTache.Type = chkIsJalon.Checked ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;
            _currentTache.BlocId = cmbBlocNom.SelectedValue as string ?? "";
            _currentTache.MetierId = cmbMetier.SelectedValue as string ?? "";
            if (chkIsJalon.Checked) _currentTache.MetierId = "";

            if (cmbEtat.SelectedItem is Statut statut)
            {
                _currentTache.Statut = statut;
            }

            // Mettre à jour les affectations
            _currentTache.Affectations.Clear();
            foreach (var item in ChkOuvriersAffect.CheckedItems)
            {
                if (item is KryptonListItem listItem && listItem.Tag is Ouvrier ouvrier)
                {
                    _currentTache.Affectations.Add(new AffectationOuvrier
                    {
                        OuvrierId = ouvrier.OuvrierId,
                        NomOuvrier = ouvrier.Nom
                    });
                }
            }

            // Mettre à jour les dépendances
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
        /// Met à jour la couleur du panneau en fonction du statut de la tâche.
        /// </summary>
        private void UpdateStatutColor(Statut statut)
        {
            switch (statut)
            {
                case Statut.Estimée: StatutColor.StateCommon.Color1 = Color.LightGray; break;
                case Statut.Planifiée: StatutColor.StateCommon.Color1 = Color.Green; break;
                case Statut.EnCours: StatutColor.StateCommon.Color1 = Color.Orange; break;
                case Statut.EnRetard: StatutColor.StateCommon.Color1 = Color.Red; break;
                case Statut.Terminée: StatutColor.StateCommon.Color1 = Color.Black; break;
                default: StatutColor.StateCommon.Color1 = Color.Transparent; break;
            }
        }

        /// <summary>
        /// Remplit les listes déroulantes (Blocs, Métiers, Statuts).
        /// </summary>
        private void PopulateComboBoxes()
        {
            if (_projetService == null || _ressourceService == null) return;

            cmbBlocNom.DataSource = _projetService.ObtenirTousLesLots().SelectMany(l => l.Blocs).OrderBy(b => b.Nom).ToList();
            cmbBlocNom.DisplayMember = "Nom";
            cmbBlocNom.ValueMember = "BlocId";

            cmbMetier.DataSource = _ressourceService.GetAllMetiers().OrderBy(m => m.Nom).ToList();
            cmbMetier.DisplayMember = "Nom";
            cmbMetier.ValueMember = "MetierId";

            cmbEtat.DataSource = Enum.GetValues(typeof(Statut));
        }

        /// <summary>
        /// Remplit la liste des ouvriers et coche ceux affectés à la tâche.
        /// </summary>
        private void PopulateOuvriers(Tache tache)
        {
            ChkOuvriersAffect.Items.Clear();
            if (_ressourceService == null) return;

            var allOuvriers = _ressourceService.GetAllOuvriers();
            var ouvriersAffectesIds = new HashSet<string>(tache.Affectations.Select(a => a.OuvrierId));

            foreach (var ouvrier in allOuvriers.OrderBy(o => o.Nom))
            {
                var listItem = new KryptonListItem { ShortText = ouvrier.Nom, Tag = ouvrier };
                int index = ChkOuvriersAffect.Items.Add(listItem);
                if (ouvriersAffectesIds.Contains(ouvrier.OuvrierId))
                {
                    ChkOuvriersAffect.SetItemChecked(index, true);
                }
            }
        }

        /// <summary>
        /// Remplit la liste des dépendances possibles pour la tâche.
        /// </summary>
        private void LoadDependencies(Tache tache)
        {
            chkListDependances.Items.Clear();
            if (_parentView != null && tache != null)
            {
                var items = _parentView.GetDependancesForTache(tache);
                foreach (var displayItem in items)
                {
                    bool isChecked = displayItem.OriginalData.Etat == EtatDependance.Stricte ||
                                     displayItem.OriginalData.Etat == EtatDependance.Suggeree;

                    int index = chkListDependances.Items.Add(displayItem);
                    chkListDependances.SetItemChecked(index, isChecked);
                }
            }
        }

        /// <summary>
        /// Affiche les propriétés brutes de la tâche pour le débogage.
        /// </summary>
        private void PopulateDebugInfo(Tache tache)
        {
            var sb = new StringBuilder("--- Propriétés Brutes de la Tâche ---\n");
            foreach (var prop in tache.GetType().GetProperties())
            {
                try
                {
                    object value = prop.GetValue(tache, null);
                    string valueStr = (value is List<AffectationOuvrier> aff)
                        ? (aff.Any() ? string.Join("; ", aff.Select(a => $"({a.OuvrierId}: {a.NomOuvrier})")) : "(Vide)")
                        : (value?.ToString() ?? "null");
                    if (string.IsNullOrEmpty(valueStr)) valueStr = "\"\"";
                    sb.AppendLine($"{prop.Name} | {prop.PropertyType.Name} | {valueStr}");
                }
                catch { /* Ignorer */ }
            }
            kryptonRichTextBox1.Text = sb.ToString();
        }

        /// <summary>
        /// Si la tâche est un conteneur, affiche ses sous-tâches dans la grille.
        /// </summary>
        private void PopulateSousTaches(Tache tache)
        {
            DataGridSousTaches.Rows.Clear();
            DataGridSousTaches.Visible = tache.EstConteneur;

            if (!tache.EstConteneur || _taskManagerService == null) return;

            var sousTaches = _taskManagerService.ObtenirTachesEnfants(tache.TacheId);
            if (!sousTaches.Any())
            {
                DataGridSousTaches.Rows.Add(null, "(Aucune sous-tâche trouvée)", "N/A");
                return;
            }

            var allOuvriers = _ressourceService.GetAllOuvriers().ToDictionary(o => o.OuvrierId);
            foreach (var sousTache in sousTaches)
            {
                var nomsOuvriers = sousTache.Affectations
                    .Select(a => allOuvriers.TryGetValue(a.OuvrierId, out var o) ? o.Nom : a.OuvrierId);
                string ouvrierDisplay = string.Join(", ", nomsOuvriers);

                DataGridSousTaches.Rows.Add(sousTache.TacheId, sousTache.TacheNom, string.IsNullOrEmpty(ouvrierDisplay) ? "N/A" : ouvrierDisplay);
            }
        }

        #endregion
    }
}