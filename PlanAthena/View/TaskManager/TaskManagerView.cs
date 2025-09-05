// --- START OF FILE TaskManagerView.cs ---

using Krypton.Docking;
using Krypton.Navigator;
using Krypton.Workspace;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using PlanAthena.View.Planificator;
using PlanAthena.View.TaskManager.PertDiagram;
using PlanAthena.View.Utils;
using System.Text;

namespace PlanAthena.View.TaskManager
{
    public partial class TaskManagerView : UserControl
    {
        #region Champs et Propriétés

        // Services
        private readonly ProjetService _projetService;
        private readonly TaskManagerService _taskManagerService;
        private readonly RessourceService _ressourceService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly ImportService _importService;
        private readonly ExportService _exportService;

        // État de la vue
        private string _activeLotId;
        private readonly TaskManagerDefaultView _defaultView;
        public enum TaskManagerDefaultView { Diagram, List }
        public event EventHandler<Type> NavigateToViewRequested;

        // Éléments d'UI managés
        private KryptonPage _detailsPage;
        private KryptonPage _taskListingsPage;


        #endregion

        #region Constructeur et Initialisation

        public TaskManagerView(ProjetService projetService, TaskManagerService taskManagerService, RessourceService ressourceService, DependanceBuilder dependanceBuilder, ImportService importService, ExportService exportService, TaskManagerDefaultView defaultView = TaskManagerDefaultView.Diagram)
        {
            InitializeComponent();
            _projetService = projetService;
            _taskManagerService = taskManagerService;
            _ressourceService = ressourceService;
            _dependanceBuilder = dependanceBuilder;
            _importService = importService;
            _exportService = exportService;
            _defaultView = defaultView;

            this.Load += TaskManagerView_Load;
        }

        private void TaskManagerView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            InitializeServicesForControls();
            InitializeDockingLayout();
            AttachEvents();
            RefreshAll();

            if (_defaultView == TaskManagerDefaultView.List)
            {
                ShowListView();
            }
            UpdateToggleViewButtonState();
        }

        private void InitializeServicesForControls()
        {
            tacheDetailView1.InitializeServices(_projetService, _taskManagerService, _ressourceService, _dependanceBuilder);
            pertDiagramControl1.Initialize(_projetService, _ressourceService, _dependanceBuilder, new PertDiagram.PertDiagramSettings());
            taskManagerListe1.InitializeServices(_projetService, _taskManagerService, _ressourceService);
        }

        #endregion

        #region Configuration du Docking Layout

        private void InitializeDockingLayout()
        {
            // Le manager prend le contrôle du panel principal pour y organiser les fenêtres.
            kryptonDockingManager.ManageControl("RootControl", kryptonPanelMain);

            // Le diagramme PERT est l'élément central de l'espace de travail.
            pertDiagramControl1.Dock = DockStyle.Fill;
            kryptonPanelMain.Controls.Add(pertDiagramControl1);

            var parentForm = FindForm();
            if (parentForm != null)
            {
                kryptonDockingManager.ManageFloating("Floating", parentForm);
            }

            SetupDockingPanels();
        }

        private void SetupDockingPanels()
        {
            var headerFont = new Font("Segoe UI", 11F, FontStyle.Bold);

            // --- CRÉATION DES PAGES ---
            var creationToolboxPage = CreateDockingPage("Création", "toolbox", Properties.Resources.tache, creationToolboxView1, headerFont);
            _taskListingsPage = CreateDockingPage("Liste des Tâches", "listeTaches", null, taskManagerListe1, headerFont);
            var lotSelectionPage = CreateDockingPage("Lot Actif", "lotselect", Properties.Resources.tache, lotSelectionView1, headerFont);
            _detailsPage = CreateDockingPage("Gestion", "details", Properties.Resources.tache, null, headerFont);

            // --- ORGANISATION DU LAYOUT ---

            // 1. Panneau de DROITE (structure verticale avec splitter)
            var rightDockspace = kryptonDockingManager.AddDockspace("RootControl", DockingEdge.Right, new[] { lotSelectionPage, _detailsPage });
            if (rightDockspace.DockspaceControl.Root is KryptonWorkspaceSequence sequenceRight)
            {
                sequenceRight.Orientation = Orientation.Vertical;
                sequenceRight.StarSize = "33*,67*";
            }

            // 2. Panneaux de GAUCHE (deux dockspaces séparés sur le même bord)

            // On crée le PREMIER dockspace à gauche pour la Toolbox
            var leftToolboxDockspace = kryptonDockingManager.AddDockspace("RootControl", DockingEdge.Left, new[] { creationToolboxPage });
            leftToolboxDockspace.DockspaceControl.Width = 220; // Largeur fixe pour la toolbox

            // On crée le DEUXIÈME dockspace à gauche pour la Liste des Tâches
            var leftListDockspace = kryptonDockingManager.AddDockspace("RootControl", DockingEdge.Left, new[] { _taskListingsPage });
            leftListDockspace.DockspaceControl.Width = 400; // Largeur fixe pour la liste

            // --- ÉTAT INITIAL ---
            kryptonDockingManager.HidePage(_detailsPage);

            // Si la vue par défaut n'est pas la liste, on cache la page.
            // L'utilisateur pourra la réafficher avec le bouton.
            if (_defaultView != TaskManagerDefaultView.List)
            {
                kryptonDockingManager.HidePage(_taskListingsPage);
            }
            // Si la vue par défaut EST la liste, on s'assure qu'elle est bien visible.
            else
            {
                kryptonDockingManager.ShowPage(_taskListingsPage);
            }
        }

        private KryptonPage CreateDockingPage(string title, string uniqueName, System.Drawing.Bitmap icon, Control content, Font headerFont)
        {
            var page = new KryptonPage()
            {
                Text = title,
                TextTitle = title,
                UniqueName = uniqueName,
                ImageSmall = icon,
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

        private void AttachEvents()
        {
            // Vues principales
            lotSelectionView1.LotSelectionChanged += OnLotSelectionChanged;
            creationToolboxView1.AddBlocRequested += OnAddBlocRequested;
            creationToolboxView1.AddTacheRequested += OnAddTacheRequested;
            creationToolboxView1.AddJalonRequested += OnAddJalonRequested;

            // Diagramme et Liste
            pertDiagramControl1.TacheClick += OnDiagramTacheClick;
            pertDiagramControl1.BlocClick += OnDiagramBlocClick;
            taskManagerListe1.TacheSelectionChanged += OnListTacheSelectionChanged;

            // Vues de détails
            tacheDetailView1.SaveRequested += OnTacheSaveRequested;
            tacheDetailView1.TacheDeleteRequested += OnTacheDeleteRequested;
            blocDetailView1.BlocChanged += OnBlocChanged;

            // Barre d'outils
            btnToggleView.Click += BtnToggleView_Click;
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
                ShowTacheDetails(e.Tache);
            else if (e.InteractionType == TacheInteractionType.DoubleClick)
                ShowTacheDetailsXL(e.Tache);
        }

        private void OnDiagramBlocClick(object sender, BlocSelectedEventArgs e)
        {
            var bloc = _projetService.ObtenirBlocParId(e.BlocId);
            if (bloc != null)
            {
                ShowBlocDetails(bloc);
            }
        }

        private void OnListTacheSelectionChanged(object sender, Tache tache)
        {
            // La sélection dans la liste ouvre directement la vue de détail complète.
            ShowTacheDetailsXL(tache);
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

        public void RefreshAll()
        {
            var allLots = _projetService.ObtenirTousLesLots();
            lotSelectionView1.PopulateLots(allLots);

            if (!string.IsNullOrEmpty(_activeLotId) && allLots.Any(l => l.LotId == _activeLotId))
            {
                lotSelectionView1.SetSelectedLot(_activeLotId);
            }
            else if (allLots.Any())
            {
                _activeLotId = allLots.First().LotId;
                lotSelectionView1.SetSelectedLot(_activeLotId);
            }
            else
            {
                _activeLotId = null;
            }
            RefreshUIForActiveLot();
        }

        private void RefreshUIForActiveLot()
        {
            var lot = _projetService.ObtenirLotParId(_activeLotId);
            if (lot == null)
            {
                creationToolboxView1.PopulateMetiers(null, null, null);
                pertDiagramControl1.ChargerDonnees(null);
                taskManagerListe1.RefreshAll();
                ClearDetailsPanel();
                return;
            }

            var metiersPourLot = _ressourceService.GetAllMetiers().Where(m => m.Phases.HasFlag(lot.Phases));
            var metiersActifs = _ressourceService.GetMetierIdsAvecCompetences();
            creationToolboxView1.PopulateMetiers(metiersPourLot, _ressourceService.GetDisplayColorForMetier, metiersActifs);

            var tachesDuLot = _taskManagerService.ObtenirToutesLesTaches(lotId: _activeLotId);
            pertDiagramControl1.ChargerDonnees(tachesDuLot);

            taskManagerListe1.RefreshAll();
            tacheDetailView1.UpdateDropdowns(_activeLotId);
            ClearDetailsPanel();
        }

        private void ShowTacheDetails(Tache tache)
        {
            _detailsPage.SuspendLayout();
            _detailsPage.Controls.Clear();
            tacheDetailView1.Dock = DockStyle.Fill;
            _detailsPage.Controls.Add(tacheDetailView1);
            tacheDetailView1.LoadTache(tache);
            kryptonDockingManager.ShowPage(_detailsPage);
            _detailsPage.ResumeLayout();
        }

        private void ShowBlocDetails(Bloc bloc)
        {
            _detailsPage.SuspendLayout();
            _detailsPage.Controls.Clear();
            blocDetailView1.Dock = DockStyle.Fill;
            _detailsPage.Controls.Add(blocDetailView1);
            blocDetailView1.LoadBloc(bloc);
            kryptonDockingManager.ShowPage(_detailsPage);
            _detailsPage.ResumeLayout();
        }

        private void ShowTacheDetailsXL(Tache tache)
        {
            if (tache == null) return;

            string pageUniqueName = $"DetailXL_{tache.TacheId}";
            var existingPage = kryptonDockingManager.PageForUniqueName(pageUniqueName);

            if (existingPage != null)
            {
                // La page existe déjà, on la met au premier plan
                kryptonDockingManager.ShowPage(pageUniqueName);
                var cell = kryptonDockingManager.DockingCellForPage(pageUniqueName);
                if (cell?.Parent is KryptonFloatspace floatspace)
                {
                    // On s'assure que la fenêtre flottante est bien active
                    floatspace.FindForm()?.Activate();
                }

                // On met à jour les données de la vue existante dans la page
                var existingDetailView = existingPage.Controls.OfType<TacheDetailViewXL>().FirstOrDefault();
                existingDetailView?.LoadTache(tache);
            }
            else
            {
                // La page n'existe pas, on la crée de A à Z
                var newDetailPage = new KryptonPage()
                {
                    Text = tache.TacheNom,
                    TextTitle = tache.TacheNom,
                    UniqueName = pageUniqueName,
                    Dock = DockStyle.Fill
                };

                // 1. On crée une NOUVELLE instance de la vue de détail
                var newDetailView = new TacheDetailViewXL();
                newDetailView.InitializeServices(_projetService, _ressourceService, _taskManagerService);
                newDetailView.Dock = DockStyle.Fill;

                // 2. On attache les événements à CETTE nouvelle instance
                newDetailView.TacheSaved += OnTacheXLSaved;
                newDetailView.TacheDeleteRequested += OnTacheXLDeleteRequested; // <--- Nouvel événement

                // La page est fermée par l'utilisateur ? On détache les événements pour éviter les fuites mémoire.
                newDetailPage.Disposed += (s, e) => {
                    newDetailView.TacheSaved -= OnTacheXLSaved;
                    newDetailView.TacheDeleteRequested -= OnTacheXLDeleteRequested;
                };

                // 3. On ajoute la vue à la page, PUIS on charge les données
                newDetailPage.Controls.Add(newDetailView);
                newDetailView.LoadTache(tache);

                // 4. On crée la fenêtre flottante
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
        private void OnTacheXLDeleteRequested(object sender, Tache tache)
        {
            // On peut réutiliser la même logique que pour la petite vue
            OnTacheDeleteRequested(sender, tache);

            // Après suppression, il faut fermer la fenêtre de détail devenue invalide
            string pageUniqueName = $"DetailXL_{tache.TacheId}";
            var pageToClose = kryptonDockingManager.PageForUniqueName(pageUniqueName);
            if (pageToClose != null)
            {
                kryptonDockingManager.RemovePage(pageToClose, true); // true pour disposer la page
            }
        }
        private void ClearDetailsPanel()
        {
            _detailsPage.Controls.Clear();
            tacheDetailView1.Clear();
            blocDetailView1.Clear();
            kryptonDockingManager.HidePage(_detailsPage);
        }

        public void ShowListView()
        {
            var page = kryptonDockingManager.PageForUniqueName("listeTaches");
            if (page != null)
            {
                kryptonDockingManager.ShowPage(page);
                page.Select();
            }
        }

        private void UpdateToggleViewButtonState()
        {
            var page = kryptonDockingManager.PageForUniqueName(_taskListingsPage.UniqueName);
            if (page != null && kryptonDockingManager.IsPageShowing(page.UniqueName))
            {
                btnToggleView.Text = "Masquer la Liste";
            }
            else
            {
                btnToggleView.Text = "Afficher la Liste";
            }
        }

        #endregion

        #region Actions de la barre d'outils (Import/Export, etc.)

        private void btnAdjustView_Click(object sender, EventArgs e)
        {
            pertDiagramControl1.ZoomToutAjuster();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            pertDiagramControl1.ImprimerDiagramme();
        }

        private void btnImporter_Click(object sender, EventArgs e)
        {
            if (!IsLotActive("Veuillez sélectionner un lot avant d'importer des tâches.")) return;

            using var ofd = new OpenFileDialog { Title = "Sélectionner le fichier CSV", Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*" };
            if (ofd.ShowDialog(FindForm()) == DialogResult.OK)
            {
                var lotActif = _projetService.ObtenirLotParId(_activeLotId);
                using var importView = new ImportMappingView(ofd.FileName, lotActif, _projetService, _ressourceService);
                if (importView.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    ExecuteImport(ofd.FileName, importView.MappingConfiguration);
                }
            }
        }

        private void ExecuteImport(string filePath, Services.Business.DTOs.ImportMappingConfiguration mappingConfig)
        {
            try
            {
                bool confirmOverwrite = false;
                if (_taskManagerService.ObtenirToutesLesTaches(lotId: _activeLotId).Any())
                {
                    var result = MessageBox.Show($"Le lot '{_activeLotId}' contient déjà des tâches. Voulez-vous les écraser?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes) { confirmOverwrite = true; } else { return; }
                }

                var importResult = _importService.ImporterTachesCSV(filePath, _activeLotId, mappingConfig, confirmOverwrite);
                if (importResult.EstSucces)
                {
                    var sb = new StringBuilder("Import terminé avec succès !\n");
                    sb.AppendLine($"- {importResult.NbTachesImportees} tâches importées.");
                    sb.AppendLine($"- {importResult.NbBlocsTraites} nouveaux blocs créés.");
                    if (importResult.Warnings.Any())
                    {
                        using var warningsView = new ImportWarningsView(importResult.Warnings);
                        warningsView.ShowDialog(FindForm());
                    }
                    MessageBox.Show(sb.ToString(), "Import Réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show($"L'import a échoué : {importResult.MessageErreur}", "Erreur d'Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur inattendue est survenue : {ex.Message}", "Erreur Critique", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void BtnToggleView_Click(object sender, EventArgs e)
        {
            string pageUniqueName = "listeTaches"; // Le nom unique de notre page

            // On vérifie si la page existe déjà quelque part
            var page = kryptonDockingManager.PageForUniqueName(pageUniqueName);

            // Si la page n'existe PAS, on la crée
            if (page == null)
            {
                var headerFont = new Font("Segoe UI", 11F, FontStyle.Bold);

                // On utilise notre méthode helper pour créer la page
                // Note: Assurez-vous que taskManagerListe1 est bien initialisé (il l'est dans InitializeServicesForControls)
                page = CreateDockingPage("Liste des Tâches", pageUniqueName, null, taskManagerListe1, headerFont);

                // Important : Il faut ajouter la page au manager pour qu'il la connaisse,
                // même si on ne la docke pas tout de suite. On l'ajoute à la collection "flottante".
                var floatingElement = kryptonDockingManager.OfType<KryptonDockingFloating>().FirstOrDefault();
                if (floatingElement != null)
                {
                    kryptonDockingManager.AddFloatingWindow(floatingElement.Name, new[] { page }, new Point(100, 100), new Size(600, 400));
                }
                else
                {
                    MessageBox.Show("Erreur : Le conteneur flottant n'a pas été trouvé.", "Erreur Docking", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Maintenant qu'on est sûr que la page existe et est connue du manager,
            // on la montre dans une fenêtre flottante.
            kryptonDockingManager.ShowPage(page.UniqueName); // Le 'true' force la création d'une fenêtre flottante

            // Mettez à jour le texte du bouton si vous le souhaitez
            btnToggleView.Text = "Masquer la Liste";
        }

        #endregion
    }
}