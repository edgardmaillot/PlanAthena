using Krypton.Docking;
using Krypton.Navigator;
using Krypton.Toolkit;
using Krypton.Workspace;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using PlanAthena.View.Planificator;
using PlanAthena.View.Structure;
using PlanAthena.View.TaskManager.PertDiagram;
using PlanAthena.View.Utils;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager
{
    public partial class TaskManagerView : UserControl
    {
        private readonly ApplicationService _applicationService;
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly ImportService _importService;
        private string _activeLotId;
        public event EventHandler<Type> NavigateToViewRequested;

        private KryptonPage _detailsPage;

        public TaskManagerView(ApplicationService applicationService, ProjetService projetService, RessourceService ressourceService, DependanceBuilder dependanceBuilder, ImportService importService)
        {
            InitializeComponent();
            _applicationService = applicationService;
            _projetService = projetService;
            _ressourceService = ressourceService;
            _dependanceBuilder = dependanceBuilder;
            _importService = importService;
            this.Load += TaskManagerView_Load;
        }

        private void TaskManagerView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            InitializeDockingLayout();

            tacheDetailView1.InitializeServices(_projetService, _ressourceService, _dependanceBuilder);
            pertDiagramControl1.Initialize(_projetService, _ressourceService, _dependanceBuilder, new PertDiagramSettings());

            AttachEvents();
            RefreshAll();
        }

        // Dans TaskManagerView.cs

        private void InitializeDockingLayout()
        {
            // Police commune pour les en-têtes de nos panneaux
            var headerFont = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);

            // 1. Le manager prend en charge le panel principal.
            kryptonDockingManager.ManageControl("RootControl", this.kryptonPanelMain);

            // 2. Le diagramme est placé directement dans le panel.
            pertDiagramControl1.Dock = DockStyle.Fill;
            this.kryptonPanelMain.Controls.Add(this.pertDiagramControl1);

            // 3. Créer les pages pour les panneaux latéraux.

            // Page "Outils de Création"
            var toolboxPage = new KryptonPage()
            {
                Text = "Création",
                TextTitle = "Outils de Création",
                UniqueName = "toolbox",
                ImageSmall = Properties.Resources.tache, // <-- AJOUT DE L'ICÔNE
                Dock = DockStyle.Fill
            };
            this.creationToolboxView1.Dock = DockStyle.Fill;
            toolboxPage.Controls.Add(this.creationToolboxView1);
            toolboxPage.StateCommon.HeaderGroup.HeaderPrimary.Content.ShortText.Font = headerFont;
            toolboxPage.ImageSmall = Properties.Resources.tache; // <-- AJOUT DE L'ICÔNE


            // Page "Sélection du Lot"
            var lotSelectionPage = new KryptonPage()
            {
                Text = "Lot",
                TextTitle = "Lot Actif",
                UniqueName = "lotselect",
                ImageSmall = Properties.Resources.tache, // <-- AJOUT DE L'ICÔNE
                Dock = DockStyle.Fill
            };
            lotSelectionPage.Controls.Add(this.lotSelectionView1);
            lotSelectionPage.StateCommon.HeaderGroup.HeaderPrimary.Content.ShortText.Font = headerFont;


            // Page "Détails" (Tâche ou Bloc)
            _detailsPage = new KryptonPage()
            {
                Text = "Gestion",
                TextTitle = "Gestion",
                UniqueName = "details",
                ImageSmall = Properties.Resources.tache, // <-- AJOUT DE L'ICÔNE
                Dock = DockStyle.Fill
            };
            _detailsPage.StateCommon.HeaderGroup.HeaderPrimary.Content.ShortText.Font = headerFont;


            // 4. Ajouter les dockspaces au contrôle managé "RootControl".

            var leftDockspace = kryptonDockingManager.AddDockspace("RootControl", DockingEdge.Left, new KryptonPage[] { toolboxPage });
            leftDockspace.DockspaceControl.Size = new System.Drawing.Size(220, 500);

            var rightDockspace = kryptonDockingManager.AddDockspace("RootControl", DockingEdge.Right, new KryptonPage[] { lotSelectionPage, _detailsPage });

            // 5. CONFIGURATION PRÉCISE BASÉE SUR LE XML
            if (rightDockspace.DockspaceControl.Root is Krypton.Workspace.KryptonWorkspaceSequence sequence)
            {
                sequence.Orientation = System.Windows.Forms.Orientation.Vertical;
                sequence.StarSize = "33*,67*";
            }

            // 6. On cache le panneau de détails au démarrage
            kryptonDockingManager.HidePage(_detailsPage);
        }

        private void AttachEvents()
        {
            lotSelectionView1.LotSelectionChanged += OnLotSelectionChanged;
            creationToolboxView1.AddBlocRequested += OnAddBlocRequested;
            creationToolboxView1.AddTacheRequested += OnAddTacheRequested;

            pertDiagramControl1.TacheClick += PertDiagram_TacheClick;
            pertDiagramControl1.BlocClick += PertDiagram_BlocClick;

            tacheDetailView1.SaveRequested += OnTacheSaveRequested;
            tacheDetailView1.TacheDeleteRequested += OnTacheDeleteRequested;
            blocDetailView1.BlocChanged += OnBlocChanged;

            btnPlanificator.Click += (s, e) => NavigateToViewRequested?.Invoke(this, typeof(PlanificatorView));
        }

        #region Logique de rafraîchissement
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
                // On passe null pour vider la toolbox
                creationToolboxView1.PopulateMetiers(null, null, null);
                pertDiagramControl1.ChargerDonnees(null);
                ClearDetailsPanel();
                return;
            }

            // 1. Obtenir les métiers pertinents pour le lot (votre code existant)
            var metiersPourLot = _ressourceService.GetAllMetiers().Where(m => m.Phases.HasFlag(lot.Phases));

            // 2. NOUVEAU : Obtenir l'ensemble des métiers avec des compétences
            var metiersActifs = _ressourceService.GetMetierIdsAvecCompetences();

            // 3. MODIFIÉ : Passer cet ensemble à la toolbox
            creationToolboxView1.PopulateMetiers(metiersPourLot, _ressourceService.GetDisplayColorForMetier, metiersActifs);

            var tachesDuLot = _projetService.ObtenirTachesParLot(_activeLotId);
            pertDiagramControl1.ChargerDonnees(tachesDuLot);

            tacheDetailView1.UpdateDropdowns(_activeLotId);
            ClearDetailsPanel();
        }
        #endregion

        #region Gestionnaires d'événements du Diagramme PERT

        private void PertDiagram_TacheClick(object sender, TacheSelectedEventArgs e)
        {
            ShowTacheDetails(e.Tache);
        }

        private void PertDiagram_BlocClick(object sender, BlocSelectedEventArgs e)
        {
            var bloc = _projetService.ObtenirBlocParId(e.BlocId);
            if (bloc != null)
            {
                ShowBlocDetails(bloc);
            }
        }

        #endregion

        #region Gestion des panneaux de détails dynamiques

        private void ShowTacheDetails(Tache tache)
        {
            _detailsPage.SuspendLayout();
            _detailsPage.Controls.Clear();
            tacheDetailView1.Dock = DockStyle.Fill;
            _detailsPage.Controls.Add(tacheDetailView1);
            tacheDetailView1.LoadTache(tache);
            _detailsPage.ResumeLayout();
        }

        private void ShowBlocDetails(Bloc bloc)
        {
            _detailsPage.SuspendLayout();
            _detailsPage.Controls.Clear();
            blocDetailView1.Dock = DockStyle.Fill;
            _detailsPage.Controls.Add(blocDetailView1);
            blocDetailView1.LoadBloc(bloc);
            _detailsPage.ResumeLayout();
        }

        private void ClearDetailsPanel()
        {
            _detailsPage.Controls.Clear();
            tacheDetailView1.Clear();
            blocDetailView1.Clear();
        }

        #endregion

        #region Autres Gestionnaires d'événements
        private void OnLotSelectionChanged(object sender, Lot lot)
        {
            if (lot != null && lot.LotId != _activeLotId)
            {
                _activeLotId = lot.LotId;
                RefreshUIForActiveLot();
            }
        }

        private void OnAddBlocRequested(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeLotId))
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _projetService.CreerBloc(_activeLotId);
            RefreshUIForActiveLot();
        }

        private void OnAddTacheRequested(object sender, Metier metier)
        {
            if (string.IsNullOrEmpty(_activeLotId))
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var lot = _projetService.ObtenirLotParId(_activeLotId);
            if (lot == null || !lot.Blocs.Any())
            {
                MessageBox.Show("Veuillez d'abord créer un bloc dans ce lot.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var premierBlocId = lot.Blocs.First().BlocId;
            var nouvelleTache = _projetService.CreerTache(_activeLotId, premierBlocId, $"Nouvelle tâche - {metier.Nom}", 8);
            nouvelleTache.MetierId = metier.MetierId;
            _projetService.ModifierTache(nouvelleTache);
            RefreshUIForActiveLot();
            ShowTacheDetails(nouvelleTache);
        }

        private void OnTacheSaveRequested(object sender, Tache tacheASauvegarder)
        {
            if (tacheASauvegarder == null) return;
            _projetService.ModifierTache(tacheASauvegarder);
            RefreshUIForActiveLot();
            ShowTacheDetails(tacheASauvegarder);
        }

        private void OnBlocChanged(object sender, Bloc blocASauvegarder)
        {
            if (blocASauvegarder == null) return;
            _projetService.ModifierBloc(blocASauvegarder);
            RefreshUIForActiveLot();
            ShowBlocDetails(blocASauvegarder);
        }

        private void OnTacheDeleteRequested(object sender, Tache tache)
        {
            if (tache == null) return;
            if (MessageBox.Show($"Supprimer la tâche '{tache.TacheNom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _projetService.SupprimerTache(tache.TacheId);
                    RefreshUIForActiveLot();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de suppression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Actions de la barre d'outils
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
            if (string.IsNullOrEmpty(_activeLotId))
            {
                MessageBox.Show("Veuillez sélectionner un lot avant d'importer des tâches.", "Lot requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var ofd = new OpenFileDialog { Title = "Sélectionner le fichier CSV", Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*" };
            if (ofd.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                var lotActif = _projetService.ObtenirLotParId(_activeLotId);
                using (var importView = new ImportMappingView(ofd.FileName, lotActif, _projetService, _ressourceService))
                {
                    if (importView.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        ExecuterImport(ofd.FileName, importView.MappingConfiguration);
                    }
                }
            }
        }

        private void ExecuterImport(string filePath, Services.Business.DTOs.ImportMappingConfiguration mappingConfig)
        {
            try
            {
                bool confirmerEcrasement = false;
                if (_projetService.ObtenirTachesParLot(_activeLotId).Any())
                {
                    var confirmResult = MessageBox.Show($"Le lot '{_activeLotId}' contient déjà des tâches. Voulez-vous les écraser?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirmResult == DialogResult.Yes) { confirmerEcrasement = true; } else { return; }
                }
                var resultat = _importService.ImporterTachesCSV(filePath, _activeLotId, mappingConfig, confirmerEcrasement);
                if (resultat.EstSucces)
                {
                    var sb = new StringBuilder("Import terminé avec succès !\n");
                    sb.AppendLine($"- {resultat.NbTachesImportees} tâches importées.");
                    sb.AppendLine($"- {resultat.NbBlocsTraites} nouveaux blocs créés.");
                    if (resultat.Warnings.Any())
                    {
                        using (var warningsView = new ImportWarningsView(resultat.Warnings)) { warningsView.ShowDialog(this.FindForm()); }
                    }
                    MessageBox.Show(sb.ToString(), "Import Réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show($"L'import a échoué : {resultat.MessageErreur}", "Erreur d'Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur inattendue est survenue : {ex.Message}", "Erreur Critique", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fonction d'export non implémentée.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}