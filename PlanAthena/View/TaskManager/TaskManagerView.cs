using Krypton.Docking;
using Krypton.Navigator;
using Krypton.Workspace;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Infrastructure;
using PlanAthena.Services.Usecases;
using PlanAthena.Utilities;
using PlanAthena.View.Planificator;
using PlanAthena.View.TaskManager.PertDiagram;
using PlanAthena.View.Utils;
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

        // État de la vue
        private string _activeLotId;
        public event EventHandler<Type> NavigateToViewRequested;

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
            tacheDetailView1.InitializeServices(_projetService, _taskManagerService, _ressourceService, this);
            pertDiagramControl1.Initialize(_projetService, _ressourceService, _dependanceBuilder, new PertDiagram.PertDiagramSettings());
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
                RefreshUIForActiveLot();
            }
        }

        private void OnDiagramTacheClick(object sender, TacheSelectedEventArgs e)
        {
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
                ShowBlocDetails(bloc);
            }
        }

        private void OnTacheSaveRequested(object sender, Tache tacheToSave)
        {
            if (tacheToSave == null) return;
            _taskManagerService.ModifierTache(tacheToSave);
            RefreshUIForActiveLot();
            ShowTacheDetails(tacheToSave);
        }

        private void OnTacheXLSaved(object sender, EventArgs e)
        {
            RefreshAll();
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
                    _taskManagerService.SupprimerTache(tache.TacheId);
                    RefreshUIForActiveLot();
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
            _projetService.CreerBloc(_activeLotId);
            RefreshUIForActiveLot();
        }

        private void OnAddTacheRequested(object sender, Metier metier)
        {
            if (!IsLotActiveWithBlocks("Veuillez créer un bloc dans ce lot avant d'ajouter une tâche.")) return;

            var lot = _projetService.ObtenirLotParId(_activeLotId);
            var firstBlocId = lot.Blocs.First().BlocId;
            var newTask = _taskManagerService.CreerTache(_activeLotId, firstBlocId, $"Nouvelle tâche - {metier.Nom}", 8);
            newTask.MetierId = metier.MetierId;
            _taskManagerService.ModifierTache(newTask);

            RefreshUIForActiveLot();
            ShowTacheDetails(newTask);
        }

        private void OnAddJalonRequested(object sender, EventArgs e)
        {
            if (!IsLotActiveWithBlocks("Veuillez créer un bloc dans ce lot avant d'ajouter un jalon.")) return;

            var lot = _projetService.ObtenirLotParId(_activeLotId);
            var firstBlocId = lot.Blocs.First().BlocId;
            var newMilestone = _taskManagerService.CreerTacheJalon(_activeLotId, firstBlocId, "Jalon", 8);

            RefreshUIForActiveLot();
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
        /// </summary>
        public void RefreshAll()
        {
            var allLots = _projetService.ObtenirTousLesLots();
            tacheDetailView1.PopulateLots(allLots);

            if (!string.IsNullOrEmpty(_activeLotId) && allLots.Any(l => l.LotId == _activeLotId))
            {
                tacheDetailView1.SetSelectedLot(_activeLotId);
            }
            else if (allLots.Any())
            {
                _activeLotId = allLots.First().LotId;
                tacheDetailView1.SetSelectedLot(_activeLotId);
            }
            else
            {
                _activeLotId = null;
            }
            RefreshUIForActiveLot();
        }

        /// <summary>
        /// Met à jour l'interface utilisateur pour le lot actuellement sélectionné.
        /// </summary>
        private void RefreshUIForActiveLot()
        {
            var lot = _projetService.ObtenirLotParId(_activeLotId);
            if (lot == null)
            {
                creationToolboxView1.PopulateMetiers(null, null, null);
                pertDiagramControl1.ChargerDonnees(null);
                // --- MODIFICATION : On vide le panneau unifié ---
                tacheDetailView1.Clear();
                return;
            }

            // ... (le reste de la méthode est inchangé) ...
            var metiersPourLot = _ressourceService.GetAllMetiers().Where(m => m.Phases.HasFlag(lot.Phases));
            var metiersActifs = _ressourceService.GetMetierIdsAvecCompetences();
            creationToolboxView1.PopulateMetiers(metiersPourLot, _ressourceService.GetDisplayColorForMetier, metiersActifs);

            var tachesDuLot = _taskManagerService.ObtenirToutesLesTaches(lotId: _activeLotId);
            pertDiagramControl1.ChargerDonnees(tachesDuLot);

            tacheDetailView1.UpdateDropdowns(_activeLotId);

            // --- MODIFICATION : On vide uniquement la partie "tâche" du panneau ---
            tacheDetailView1.ClearTacheDetails();
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
                    TextTitle = tache.TacheNom,
                    UniqueName = pageUniqueName,
                    Dock = DockStyle.Fill
                };

                var newDetailView = new TacheDetailViewXL();
                newDetailView.InitializeServices(_projetService, _ressourceService, _taskManagerService, this);
                newDetailView.Dock = DockStyle.Fill;

                newDetailView.TacheSaved += OnTacheXLSaved;
                newDetailView.TacheDeleteRequested += OnTacheXLDeleteRequested;

                newDetailPage.Disposed += (s, e) => {
                    newDetailView.TacheSaved -= OnTacheXLSaved;
                    newDetailView.TacheDeleteRequested -= OnTacheXLDeleteRequested;
                };

                newDetailPage.Controls.Add(newDetailView);
                newDetailView.LoadTache(tache);

                var floatingElement = kryptonDockingManager.OfType<KryptonDockingFloating>().FirstOrDefault();
                if (floatingElement == null)
                {
                    MessageBox.Show("Erreur : Le conteneur flottant n'a pas été trouvé.", "Erreur Docking", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                kryptonDockingManager.AddFloatingWindow(
                    floatingElement.Name,
                    new[] { newDetailPage },
                    new Point(200, 200),
                    new Size(800, 650)
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
            OnTacheDeleteRequested(sender, tache);

            string pageUniqueName = $"DetailXL_{tache.TacheId}";
            var pageToClose = kryptonDockingManager.PageForUniqueName(pageUniqueName);
            if (pageToClose != null)
            {
                kryptonDockingManager.RemovePage(pageToClose, true);
            }
        }

        private void ClearDetailsPanel()
        {
            _detailsPage.Controls.Clear();
            tacheDetailView1.Clear();
            blocDetailView1.Clear();
            kryptonDockingManager.HidePage(_detailsPage);
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