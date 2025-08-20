using Krypton.Toolkit;
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
            tacheDetailView1.InitializeServices(_projetService, _ressourceService, _dependanceBuilder);
            pertDiagramControl1.Initialize(_projetService, _ressourceService, _dependanceBuilder, new PertDiagramSettings());
            AttachEvents();
            RefreshAll();
        }

        private void AttachEvents()
        {
            lotSelectionView1.LotSelectionChanged += OnLotSelectionChanged;
            creationToolboxView1.AddBlocRequested += OnAddBlocRequested;
            creationToolboxView1.AddTacheRequested += OnAddTacheRequested;

            pertDiagramControl1.TacheClick += PertDiagram_TacheClick;
            pertDiagramControl1.BlocClick += PertDiagram_BlocClick;
            pertDiagramControl1.TacheDoubleClick += PertDiagram_TacheDoubleClick;
            pertDiagramControl1.BlocDoubleClick += PertDiagram_BlocDoubleClick;

            tacheDetailView1.SaveRequested += OnTacheSaveRequested;
            tacheDetailView1.TacheDeleteRequested += OnTacheDeleteRequested;

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
                creationToolboxView1.PopulateMetiers(null, null);
                pertDiagramControl1.ChargerDonnees(null);
                tacheDetailView1.Clear();
                return;
            }

            var metiersPourLot = _ressourceService.GetAllMetiers().Where(m => m.Phases.HasFlag(lot.Phases));
            creationToolboxView1.PopulateMetiers(metiersPourLot, _ressourceService.GetDisplayColorForMetier);

            var tachesDuLot = _projetService.ObtenirTachesParLot(_activeLotId);
            pertDiagramControl1.ChargerDonnees(tachesDuLot);

            tacheDetailView1.UpdateDropdowns(_activeLotId);
            tacheDetailView1.Clear();
        }
        #endregion

        #region Gestionnaires d'événements du Diagramme PERT

        private void PertDiagram_TacheClick(object sender, TacheSelectedEventArgs e)
        {
            tacheDetailView1.LoadTache(e.Tache);
        }

        private void PertDiagram_BlocClick(object sender, BlocSelectedEventArgs e)
        {
            // Pour l'instant, un simple clic sur un bloc ne fait qu'effacer le détail de la tâche
            tacheDetailView1.Clear();
        }

        private void PertDiagram_TacheDoubleClick(object sender, TacheSelectedEventArgs e)
        {
            // Action future possible (ex: ouvrir un popup d'édition "RUN")
            // Pour l'instant, on s'assure juste que la tâche est sélectionnée.
            tacheDetailView1.LoadTache(e.Tache);
        }

        private void PertDiagram_BlocDoubleClick(object sender, BlocSelectedEventArgs e)
        {
            var blocToEdit = _projetService.ObtenirBlocParId(e.BlocId);
            if (blocToEdit == null) return;

            // Il faudra utiliser le nouveau formulaire /View/Structure/BlocDetailView pour éditer les blocs
            /*using (var form = new PlanAthena.Forms.BlocDetailForm(blocToEdit)) 
            {
                if (form.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    _projetService.ModifierBloc(blocToEdit);
                    RefreshUIForActiveLot();
                }
            }*/
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
            tacheDetailView1.LoadTache(nouvelleTache, isNew: true);
        }

        private void OnTacheSaveRequested(object sender, Tache tacheASauvegarder)
        {
            if (tacheASauvegarder == null) return;
            _projetService.ModifierTache(tacheASauvegarder);
            RefreshUIForActiveLot();
            tacheDetailView1.LoadTache(tacheASauvegarder);
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