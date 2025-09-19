using Krypton.Docking;
using Krypton.Navigator;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Usecases;
using PlanAthena.Utilities;
using PlanAthena.View.Planificator;
using PlanAthena.View.TaskManager.PertDiagram;
using PlanAthena.View.TaskManager.Utilitaires;
using System.Text;

namespace PlanAthena.View.TaskManager
{
    /// <summary>
    /// Vue principale pour la gestion des tâches en mode diagramme (PERT).
    /// Gère le layout des panneaux d'outils et l'interaction avec le diagramme.
    /// </summary>
    public partial class TaskManagerView : UserControl
    {
        #region Champs et Propriétés

        // Services
        private readonly ProjetService _projetService;
        private readonly TaskManagerService _taskManagerService;
        private readonly RessourceService _ressourceService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly ImportWizardOrchestrator _orchestrator;
        private readonly ExportService _exportService;
        private readonly CheminsPrefereService _cheminsService;
        private readonly PertDiagramSettings _pertSettings = new PertDiagramSettings();
        private readonly TacheDetailViewController _tacheDetailController;

        private bool _suppressPlanningWarning = false;

        // État de la vue
        private string _activeLotId;
        public event EventHandler<Type> NavigateToViewRequested;

        private string _lastSelectedBlocId;

        // Éléments d'UI managés par le DockingManager
        private KryptonPage _detailsPage;
        private KryptonDockspace _rightDockspaceControl;
        #endregion

        #region Constructeur et Initialisation

        public TaskManagerView(ProjetService projetService, TaskManagerService taskManagerService, RessourceService ressourceService, DependanceBuilder dependanceBuilder, ImportWizardOrchestrator orchestrator, ExportService exportService, CheminsPrefereService cheminsPrefereService)
        {
            InitializeComponent();
            _projetService = projetService;
            _taskManagerService = taskManagerService;
            _ressourceService = ressourceService;
            _dependanceBuilder = dependanceBuilder;
            _orchestrator = orchestrator;
            _exportService = exportService;
            _cheminsService = cheminsPrefereService;
            _tacheDetailController = new TacheDetailViewController(_taskManagerService);

            this.Load += TaskManagerView_Load;
        }

        private void TaskManagerView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            InitializeServicesForControls();
            InitializeDockingLayout();
            AttachEvents();
            RefreshAll();
        }

        /// <summary>
        /// Injecte les services nécessaires dans les UserControls enfants.
        /// </summary>
        private void InitializeServicesForControls()
        {
            tacheDetailView1.InitializeServices(_projetService, _taskManagerService, _ressourceService, this, _pertSettings, _tacheDetailController);
            pertDiagramControl1.Initialize(_projetService, _ressourceService, _dependanceBuilder, _pertSettings);
        }

        #endregion

        #region Configuration du Docking Layout

        /// <summary>
        /// Configure le layout initial des panneaux dockés autour du diagramme PERT.
        /// </summary>
        private void InitializeDockingLayout()
        {
            kryptonDockingManager.ManageControl("RootControl", kryptonPanelMain);

            pertDiagramControl1.Dock = DockStyle.Fill;
            kryptonPanelMain.Controls.Add(pertDiagramControl1);

            var parentForm = FindForm();
            if (parentForm != null)
            {
                kryptonDockingManager.ManageFloating("Floating", parentForm);
            }

            SetupDockingPanels();
        }

        /// <summary>
        /// Crée les pages et les dockspaces pour les panneaux d'outils.
        /// </summary>
        private void SetupDockingPanels()
        {
            var headerFont = new Font("Segoe UI", 11F, FontStyle.Bold);

            // Page de gauche - Creation Toolbox
            var creationToolboxPage = CreateDockingPage("Création", "toolbox", Properties.Resources.tache, creationToolboxView1, headerFont);
            var leftDockspace = kryptonDockingManager.AddDockspace("RootControl", DockingEdge.Left, new[] { creationToolboxPage });
            leftDockspace.DockspaceControl.Width = 220;

            // La page de droite - Selecteur Lot + Détails (Tâche/Bloc) 
            _detailsPage = CreateDockingPage("Gestion", "details", Properties.Resources.tache, tacheDetailView1, headerFont);

            var rightDockingElement = kryptonDockingManager.AddDockspace("RootControl", DockingEdge.Right, new[] { _detailsPage });
            _rightDockspaceControl = rightDockingElement.DockspaceControl;
            _rightDockspaceControl.Width = 280;

        }

        /// <summary>
        /// Méthode utilitaire pour créer une KryptonPage configurée.
        /// </summary>
        private KryptonPage CreateDockingPage(string title, string uniqueName, System.Drawing.Image icon, Control content, Font headerFont)
        {
            var page = new KryptonPage()
            {
                Text = title,
                TextTitle = title,
                UniqueName = uniqueName,
                ImageSmall = (Bitmap)icon,
                Dock = DockStyle.Fill
            };
            page.StateCommon.HeaderGroup.HeaderPrimary.Content.ShortText.Font = headerFont;

            if (content != null)
            {
                content.Dock = DockStyle.Fill;
                page.Controls.Add(content);
            }
            return page;
        }

        #endregion

        #region Gestion des événements

        /// <summary>
        /// Abonne les gestionnaires d'événements aux événements des contrôles enfants.
        /// </summary>
        private void AttachEvents()
        {
            tacheDetailView1.LotSelectionChanged += OnLotSelectionChanged;
            tacheDetailView1.SaveRequested += OnTacheSaveRequested;
            tacheDetailView1.TacheDeleteRequested += OnTacheDeleteRequested;

            creationToolboxView1.AddBlocRequested += OnAddBlocRequested;
            creationToolboxView1.AddTacheRequested += OnAddTacheRequested;
            creationToolboxView1.AddJalonRequested += OnAddJalonRequested;

            pertDiagramControl1.TacheClick += OnDiagramTacheClick;
            pertDiagramControl1.BlocClick += OnDiagramBlocClick;

            blocDetailView1.BlocChanged += OnBlocChanged;

            zoomTrackBar.ValueChanged += zoomTrackBar_ValueChanged;
            pertDiagramControl1.ZoomChanged += PertDiagramControl1_ZoomChanged;

            btnPlanificator.Click += (s, e) => NavigateToViewRequested?.Invoke(this, typeof(PlanificatorView));
        }

        private void OnLotSelectionChanged(object sender, Lot lot)
        {
            if (lot != null && lot.LotId != _activeLotId)
            {
                _activeLotId = lot.LotId;
                _lastSelectedBlocId = null;
                RefreshUIForActiveLot();
            }
        }

        private void OnDiagramTacheClick(object sender, TacheSelectedEventArgs e)
        {
            _lastSelectedBlocId = e.Tache.BlocId;
            if (e.InteractionType == TacheInteractionType.SingleClick)
            {
                //vérifier ce qui est affiché dans tacheDetailView1
                ShowTacheDetails(e.Tache);
                tacheDetailView1.LoadTache(e.Tache);
            }
            else if (e.InteractionType == TacheInteractionType.DoubleClick)
            {
                ShowTacheDetailsXL(e.Tache);
            }
        }

        private void OnDiagramBlocClick(object sender, BlocSelectedEventArgs e)
        {
            var bloc = _projetService.ObtenirBlocParId(e.BlocId);
            if (bloc != null)
            {
                _lastSelectedBlocId = e.BlocId;
                ShowBlocDetails(bloc);
            }
        }

        private void OnTacheSaveRequested(object sender, Tache tacheToSave)
        {
            if (tacheToSave == null) return;

            // Règle 1: on ne fait rien pour les statuts verrouillés (la UI devrait déjà bloquer)
            if (tacheToSave.Statut == Statut.EnCours || tacheToSave.Statut == Statut.Terminée)
            {
                MessageBox.Show("Cette tâche ne peut être modifiée.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // Règle 3: Demander confirmation pour les tâches planifiées
            if (tacheToSave.Statut == Statut.Planifiée || tacheToSave.Statut == Statut.EnRetard)
            {
                if (!DemanderConfirmationModificationPlanifiee())
                {
                    return; // L'utilisateur a annulé
                }
                // L'utilisateur a confirmé, on invalide le planning
                _taskManagerService.InvaliderPlanification();
            }

            // Si on arrive ici, la modification est autorisée
            _taskManagerService.ModifierTache(tacheToSave);
            //pertDiagramControl1.MettreAJourTache(tacheToSave);
            RefreshAll(true);
            ShowTacheDetails(tacheToSave);
        }

        private void OnTacheXLSaved(object sender, Tache tacheToSave)
        {
            if (tacheToSave == null) return;

            // Sauvegarder la tâche modifiée
            _taskManagerService.ModifierTache(tacheToSave);

            // NOUVEAU: Si les dates de planification ont été modifiées, 
            // déclencher une mise à jour du PlanningService
            // (Le TaskManagerService.InvaliderPlanification() a déjà été appelé dans TacheDetailViewXL)

            // Optionnel: Recalculer immédiatement le planning si nécessaire
            // _planningService.RecalculerPlanning(); // À décommenter si cette méthode existe

            // On rafraîchit tout car la vue XL peut modifier plus de choses
            // et qu'elle est indépendante du flux principal.
            RefreshAll(true); // 'true' pour préserver la position du diagramme
        }

        private void OnBlocChanged(object sender, Bloc blocToSave)
        {
            if (blocToSave == null) return;
            _projetService.ModifierBloc(blocToSave);
            RefreshUIForActiveLot();
            ShowBlocDetails(blocToSave);
        }

        private void OnTacheDeleteRequested(object sender, Tache tache)
        {
            if (tache == null) return;
            if (MessageBox.Show($"Supprimer la tâche '{tache.TacheNom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // 1. Sauvegarder l'état de la vue AVANT la modification
                    var viewState = pertDiagramControl1.GetViewState();

                    _taskManagerService.SupprimerTache(tache.TacheId);

                    // 2. Lancer le rafraîchissement en passant l'état sauvegardé
                    RefreshUIForActiveLot(viewState);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Logique de création (depuis la Toolbox)

        private void OnAddBlocRequested(object sender, EventArgs e)
        {
            if (!IsLotActive("Veuillez sélectionner un lot pour y ajouter un bloc.")) return;
            var viewState = pertDiagramControl1.GetViewState();
            var newBloc = _projetService.CreerBloc(_activeLotId);
            _taskManagerService.CreerTacheJalon(_activeLotId, newBloc.BlocId, "Début", 0);
            RefreshUIForActiveLot(viewState);
        }

        private void OnAddTacheRequested(object sender, Metier metier)
        {
            if (!IsLotActiveWithBlocks("Veuillez créer un bloc dans ce lot avant d'ajouter une tâche.")) return;

            var lot = _projetService.ObtenirLotParId(_activeLotId);
            string targetBlocId;

            // --- LOGIQUE AMÉLIORÉE ---
            // 1. On vérifie si un bloc a été sélectionné récemment ET s'il existe toujours dans le lot actuel.
            if (!string.IsNullOrEmpty(_lastSelectedBlocId) && lot.Blocs.Any(b => b.BlocId == _lastSelectedBlocId))
            {
                // On utilise le dernier bloc sélectionné, c'est le cas idéal.
                targetBlocId = _lastSelectedBlocId;
            }
            else
            {
                // 2. Cas de repli : aucun bloc sélectionné ou l'ID n'est plus valide.
                // On prend le premier bloc du lot, comme avant.
                targetBlocId = lot.Blocs.First().BlocId;
            }
            // --- FIN DE LA LOGIQUE AMÉLIORÉE ---

            // On utilise la variable targetBlocId pour la création
            var newTask = _taskManagerService.CreerTache(_activeLotId, targetBlocId, $"Nouvelle tâche - {metier.Nom}", 8);
            newTask.MetierId = metier.MetierId;
            _taskManagerService.ModifierTache(newTask);

            var viewState = pertDiagramControl1.GetViewState();
            RefreshUIForActiveLot(viewState);
            ShowTacheDetails(newTask);
        }

        private void OnAddJalonRequested(object sender, EventArgs e)
        {
            if (!IsLotActiveWithBlocks("Veuillez créer un bloc dans ce lot avant d'ajouter un jalon.")) return;

            var lot = _projetService.ObtenirLotParId(_activeLotId);
            string targetBlocId;

            // --- LOGIQUE RÉPLIQUÉE ---
            // 1. On vérifie si un bloc a été sélectionné récemment ET s'il existe toujours dans le lot actuel.
            if (!string.IsNullOrEmpty(_lastSelectedBlocId) && lot.Blocs.Any(b => b.BlocId == _lastSelectedBlocId))
            {
                // On utilise le dernier bloc sélectionné.
                targetBlocId = _lastSelectedBlocId;
            }
            else
            {
                // 2. Cas de repli : aucun bloc sélectionné ou l'ID n'est plus valide.
                // On prend le premier bloc du lot.
                targetBlocId = lot.Blocs.First().BlocId;
            }
            // --- FIN DE LA LOGIQUE RÉPLIQUÉE ---

            // 1. Sauvegarder l'état de la vue AVANT la modification
            var viewState = pertDiagramControl1.GetViewState();

            // On utilise la variable targetBlocId pour la création du jalon
            var newMilestone = _taskManagerService.CreerTacheJalon(_activeLotId, targetBlocId, "Jalon", 0); // Un jalon a souvent une durée de 0

            // 2. Lancer le rafraîchissement en passant l'état sauvegardé
            RefreshUIForActiveLot(viewState);
            ShowTacheDetails(newMilestone);
        }

        private bool IsLotActive(string message)
        {
            if (string.IsNullOrEmpty(_activeLotId))
            {
                MessageBox.Show(message, "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool IsLotActiveWithBlocks(string message)
        {
            if (!IsLotActive("Veuillez d'abord sélectionner un lot.")) return false;
            var lot = _projetService.ObtenirLotParId(_activeLotId);
            if (lot == null || !lot.Blocs.Any())
            {
                MessageBox.Show(message, "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        #endregion

        #region Logique d'UI (Affichage, Rafraîchissement)

        /// <summary>
        /// Rafraîchit l'ensemble des données de la vue, y compris la liste des lots.
        /// Peut préserver la position et le zoom actuels du diagramme.
        /// </summary>
        /// <param name="preserveViewState">Si true, la position et le zoom du diagramme PERT sont conservés.</param>
        public void RefreshAll(bool preserveViewState = false)
        {
            // 1. Sauvegarde de l'état de la vue du diagramme si demandé.
            // C'est utile pour les rafraîchissements qui ne doivent pas perturber l'utilisateur.
            var viewState = preserveViewState ? pertDiagramControl1.GetViewState() : null;

            // 2. Récupération des données à jour (la liste de tous les lots).
            var allLots = _projetService.ObtenirTousLesLots();

            // 3. Mise à jour du contrôle enfant (TacheDetailView) avec la nouvelle liste de lots.
            // C'est le contrôle enfant qui gère l'affichage de sa ComboBox.
            tacheDetailView1.PopulateLots(allLots);

            // 4. Détermination du lot actif pour la vue parente (TaskManagerView).
            // Cette logique assure que le contexte est maintenu après le rechargement.
            if (!string.IsNullOrEmpty(_activeLotId) && allLots.Any(l => l.LotId == _activeLotId))
            {
                // Le lot actif existe toujours, on le resélectionne dans la vue enfant
                // pour synchroniser l'état de la ComboBox.
                tacheDetailView1.SetSelectedLot(_activeLotId);
            }
            else if (allLots.Any())
            {
                // L'ancien lot actif n'existe plus (ou aucun n'était sélectionné), 
                // on sélectionne le premier de la nouvelle liste.
                _activeLotId = allLots.First().LotId;
                tacheDetailView1.SetSelectedLot(_activeLotId);
            }
            else
            {
                // Il n'y a plus aucun lot. On vide l'ID actif.
                _activeLotId = null;
                tacheDetailView1.SetSelectedLot(null); // On s'assure que la vue enfant est aussi vidée.
            }

            // 5. Rafraîchissement du diagramme et des autres éléments de l'UI
            // avec le lot actif et l'état de la vue sauvegardé.
            RefreshUIForActiveLot(viewState);
        }

        /// <summary>
        /// Met à jour l'interface utilisateur pour le lot actuellement sélectionné.
        /// </summary>
        private void RefreshUIForActiveLot(PertDiagramControl.PertViewState stateToRestore = null)
        {
            var lot = _projetService.ObtenirLotParId(_activeLotId);
            if (lot == null)
            {
                creationToolboxView1.PopulateMetiers(null, null, null);
                pertDiagramControl1.ChargerDonnees(null, "", stateToRestore);
                // --- MODIFICATION : On vide le panneau unifié ---
                tacheDetailView1.Clear();
                return;
            }

            var metiersPourLot = _ressourceService.GetAllMetiers().Where(m => m.Phases.HasFlag(lot.Phases));
            var metiersActifs = _ressourceService.GetMetierIdsAvecCompetences();
            creationToolboxView1.PopulateMetiers(metiersPourLot, _ressourceService.GetDisplayColorForMetier, metiersActifs);

            var tachesDuLot = _taskManagerService.ObtenirToutesLesTaches(lotId: _activeLotId);
            pertDiagramControl1.ChargerDonnees(tachesDuLot, "", stateToRestore);

            tacheDetailView1.UpdateDropdowns(_activeLotId);

            // --- MODIFICATION : On vide uniquement la partie "tâche" du panneau ---
            tacheDetailView1.ClearTacheDetails();
        }
        private bool DemanderConfirmationModificationPlanifiee()
        {
            if (_suppressPlanningWarning) return true;

            var message = "Attention : Vous modifiez une tâche déjà planifiée.\n\n" +
                          "Cela va désynchroniser votre planning jusqu'au prochain calcul.\n\n" +
                          "Voulez-vous continuer ?";

            // Pour un vrai "Ne plus me demander", il faudrait une fenêtre personnalisée.
            // Simulons avec deux MessageBox pour rester simple.
            var result = MessageBox.Show(this, message, "Confirmation de modification",
                                         MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                var result2 = MessageBox.Show(this, "Voulez-vous désactiver cette alerte pour le reste de la session ?",
                                              "Désactiver l'alerte", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result2 == DialogResult.Yes)
                {
                    _suppressPlanningWarning = true;
                }
                return true;
            }

            return false;
        }
        /// <summary>
        /// Affiche le panneau de détails simple pour une tâche.
        /// </summary>
        private void ShowTacheDetails(Tache tache)
        {
            _detailsPage.SuspendLayout();
            _detailsPage.Controls.Clear();
            //reset la largeur du panneau de détails pour les tâches
            _rightDockspaceControl.Width = 280;
            tacheDetailView1.Dock = DockStyle.Fill;
            _detailsPage.Controls.Add(tacheDetailView1);
            tacheDetailView1.LoadTache(tache);
            kryptonDockingManager.ShowPage(_detailsPage);
            _detailsPage.ResumeLayout();
        }

        /// <summary>
        /// Affiche le panneau de détails simple pour un bloc.
        /// </summary>
        private void ShowBlocDetails(Bloc bloc)
        {
            _detailsPage.SuspendLayout();
            _detailsPage.Controls.Clear();
            //fixer la largeur du panneau de détails pour les blocs
            _rightDockspaceControl.Width = 400;
            blocDetailView1.Dock = DockStyle.Fill;
            _detailsPage.Controls.Add(blocDetailView1);
            blocDetailView1.LoadBloc(bloc);
            kryptonDockingManager.ShowPage(_detailsPage);
            _detailsPage.ResumeLayout();
        }

        /// <summary>
        /// Affiche la vue de détail complète (XL) pour une tâche dans une fenêtre flottante.
        /// </summary>
        private void ShowTacheDetailsXL(Tache tache)
        {
            if (tache == null) return;

            string pageUniqueName = $"DetailXL_{tache.TacheId}";
            var existingPage = kryptonDockingManager.PageForUniqueName(pageUniqueName);

            if (existingPage != null)
            {
                kryptonDockingManager.ShowPage(pageUniqueName);
                var cell = kryptonDockingManager.DockingCellForPage(pageUniqueName);
                if (cell?.Parent is KryptonFloatspace floatspace)
                {
                    floatspace.FindForm()?.Activate();
                }
                var existingDetailView = existingPage.Controls.OfType<TacheDetailViewXL>().FirstOrDefault();
                existingDetailView?.LoadTache(tache);
            }
            else
            {
                var newDetailPage = new KryptonPage()
                {
                    Text = tache.TacheNom,
                    TextTitle = $"Edition de la tâche : {tache.TacheNom} - ({tache.TacheId})",
                    UniqueName = pageUniqueName,
                    Dock = DockStyle.Fill
                };

                var newDetailView = new TacheDetailViewXL();
                newDetailView.InitializeServices(_projetService, _ressourceService, _taskManagerService, this, _tacheDetailController);
                newDetailView.Dock = DockStyle.Fill;

                newDetailView.SaveRequested += OnTacheXLSaved;
                newDetailView.TacheDeleteRequested += OnTacheXLDeleteRequested;

                newDetailPage.Disposed += (s, e) =>
                {
                    // On se désabonne du bon événement
                    newDetailView.SaveRequested -= OnTacheXLSaved;
                    newDetailView.TacheDeleteRequested -= OnTacheXLDeleteRequested;
                };

                newDetailPage.Controls.Add(newDetailView);
                newDetailView.LoadTache(tache);
                newDetailPage.VisibleChanged += (s, args) =>
                {
                    if (!newDetailPage.Visible && _taskManagerService.ObtenirTache(tache.TacheId) == null)
                    {
                        // La tâche a été supprimée, fermer la fenêtre
                        kryptonDockingManager.RemovePage(newDetailPage, true);
                    }
                };
                var floatingElement = kryptonDockingManager.OfType<KryptonDockingFloating>().FirstOrDefault();
                if (floatingElement == null)
                {
                    MessageBox.Show("Erreur : Le conteneur flottant n'a pas été trouvé.", "Erreur Docking", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                kryptonDockingManager.AddFloatingWindow(
                    floatingElement.Name,
                    new[] { newDetailPage },
                    new Point(224, 80),
                    new Size(800, 628)
                );
            }
        }

        /// <summary>
        /// Calcule et retourne la liste des dépendances affichables pour une tâche donnée.
        /// Cette méthode centralise la logique pour les vues de détail.
        /// </summary>
        /// <param name="tache">La tâche pour laquelle calculer les dépendances.</param>
        /// <returns>Une liste d'objets DependanceDisplayItem prêts à être affichés.</returns>
        public List<DependanceDisplayItem> GetDependancesForTache(Tache tache)
        {
            var dependancesPourAffichage = new List<DependanceDisplayItem>();

            if (tache == null || _projetService == null || _taskManagerService == null || _dependanceBuilder == null ||
                string.IsNullOrEmpty(tache.LotId) || string.IsNullOrEmpty(tache.BlocId))
            {
                return dependancesPourAffichage; // Retourne une liste vide si les prérequis ne sont pas remplis
            }

            var lot = _projetService.ObtenirLotParId(tache.LotId);
            if (lot == null) return dependancesPourAffichage;

            var tachesDuMemeBloc = _taskManagerService.ObtenirToutesLesTaches(blocId: tache.BlocId);
            var etatsDependances = _dependanceBuilder.ObtenirDependancesPourTache(tache, tachesDuMemeBloc, lot.Phases)
                                                    .OrderBy(d => d.TachePredecesseur.TacheNom)
                                                    .ToList();

            foreach (var etat in etatsDependances)
            {
                dependancesPourAffichage.Add(new DependanceDisplayItem(etat));
            }

            return dependancesPourAffichage;
        }

        private void OnTacheXLDeleteRequested(object sender, Tache tache)
        {
            if (tache == null) return;

            if (MessageBox.Show($"Supprimer la tâche '{tache.TacheNom}' ?", "Confirmation",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // 1. Sauvegarder l'état de la vue AVANT la modification
                    var viewState = pertDiagramControl1.GetViewState();

                    _taskManagerService.SupprimerTache(tache.TacheId);

                    // 2. Lancer le rafraîchissement en passant l'état sauvegardé
                    RefreshUIForActiveLot(viewState);

                    // 3. Fermer la fenêtre XL après suppression
                    string pageUniqueName = $"DetailXL_{tache.TacheId}";
                    var pageToClose = kryptonDockingManager.PageForUniqueName(pageUniqueName);
                    if (pageToClose != null)
                    {
                        kryptonDockingManager.RemovePage(pageToClose, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        #endregion

        #region Actions de la barre d'outils
        private void PertDiagramControl1_ZoomChanged(object sender, PertDiagram.ZoomChangedEventArgs e)
        {
            // Convertir le facteur de zoom (ex: 1.5) en pourcentage entier (ex: 150)
            int newTrackBarValue = (int)Math.Round(e.ZoomFactor * 100.0);

            // S'assurer que la valeur reste dans les limites du TrackBar
            newTrackBarValue = Math.Max(zoomTrackBar.Minimum, Math.Min(zoomTrackBar.Maximum, newTrackBarValue));

            // Mettre à jour la valeur du TrackBar.
            // Important : il faut éviter de déclencher une boucle infinie d'événements.
            // On met à jour la valeur du trackbar SEULEMENT si elle est différente.
            if (zoomTrackBar.Value != newTrackBarValue)
            {
                zoomTrackBar.Value = newTrackBarValue;
            }
        }
        private void btnAdjustView_Click(object sender, EventArgs e)
        {
            pertDiagramControl1.ZoomToutAjuster();
            zoomTrackBar.Value = 100;
        }
        private void zoomTrackBar_ValueChanged(object sender, EventArgs e)
        {
            // Appelle la méthode publique du contrôle enfant avec la valeur actuelle du trackbar.
            pertDiagramControl1.ZoomModifier(zoomTrackBar.Value);
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            pertDiagramControl1.ImprimerDiagramme();
        }

        private void btnImporter_Click(object sender, EventArgs e)
        {
            if (!IsLotActive("Veuillez sélectionner un lot avant d'importer des tâches.")) return;

            using (var ofd = new OpenFileDialog
            {
                Title = "Sélectionner le fichier CSV de tâches",
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                InitialDirectory = _cheminsService.ObtenirDernierDossierImport() // Bonus UX
            })
            {
                if (ofd.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    _cheminsService.SauvegarderDernierDossier(TypeOperation.ImportCsv, ofd.FileName);
                    var lotActif = _projetService.ObtenirLotParId(_activeLotId);

                    // --- NOUVEL APPEL À L'ORCHESTRATEUR ---
                    ImportResult result = _orchestrator.LancerWizardImportTaches(ofd.FileName, lotActif);

                    // --- GESTION DU RÉSULTAT ---
                    if (result.EstSucces)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Importation des tâches terminée avec succès !");
                        sb.AppendLine();
                        sb.AppendLine($"  - Tâches importées : {result.NbTachesImportees}");
                        sb.AppendLine($"  - Blocs traités : {result.NbBlocsTraites}");
                        sb.AppendLine($"  - Durée : {result.DureeImport:g}");

                        if (result.Warnings.Any())
                        {
                            sb.AppendLine();
                            sb.AppendLine("Avertissements :");
                            result.Warnings.Take(10).ToList().ForEach(w => sb.AppendLine($"- {w}"));
                            if (result.Warnings.Count > 10) sb.AppendLine("...");
                        }

                        MessageBox.Show(sb.ToString(), "Importation réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        RefreshUIForActiveLot();
                    }
                    else if (!string.IsNullOrEmpty(result.MessageErreur))
                    {
                        // Ne pas afficher de popup d'erreur si l'utilisateur a juste annulé.
                        if (!result.MessageErreur.ToLower().Contains("annulé"))
                        {
                            MessageBox.Show(result.MessageErreur, "Échec de l'importation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!IsLotActive("Veuillez sélectionner un lot avant d'exporter.")) return;

            var tasksToExport = _taskManagerService.ObtenirToutesLesTaches(lotId: _activeLotId);
            if (!tasksToExport.Any())
            {
                MessageBox.Show("Le lot sélectionné ne contient aucune tâche à exporter.", "Exportation impossible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Exporter les tâches au format CSV",
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                FileName = $"Export_Taches_{_projetService.ObtenirLotParId(_activeLotId)?.Nom.Replace(" ", "_") ?? "Lot"}_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog(FindForm()) == DialogResult.OK)
            {
                try
                {
                    _exportService.ExporterTachesCSV(tasksToExport, sfd.FileName);
                    MessageBox.Show($"Exportation terminée avec succès.\n{tasksToExport.Count} tâches ont été exportées.", "Export Réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue lors de l'exportation :\n{ex.Message}", "Erreur d'Exportation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }
}