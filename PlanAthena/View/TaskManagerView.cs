using Krypton.Toolkit;
using PlanAthena.Controls;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View
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

            this.tacheDetailView1.Dispose();
            this.tacheDetailView1 = new TacheDetailView(_projetService, _ressourceService, _dependanceBuilder);
            this.tacheDetailView1.Dock = DockStyle.Fill;
            this.kryptonSplitContainerRight.Panel2.Controls.Add(this.tacheDetailView1);

            this.Load += TaskManagerView_Load;
        }

        private void TaskManagerView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            InitializeControls();
            AttachEvents();
            RefreshAll();
        }

        private void InitializeControls()
        {
            pertDiagramControl1.Initialize(_projetService, _ressourceService, _dependanceBuilder, new PertDiagramSettings());
        }

        private void AttachEvents()
        {
            lotSelectionView1.LotSelectionChanged += OnLotSelectionChanged;
            creationToolboxView1.AddBlocRequested += OnAddBlocRequested;
            creationToolboxView1.AddTacheRequested += OnAddTacheRequested;
            pertDiagramControl1.TacheSelected += OnTacheSelectedInPert;
            // pertDiagramControl1.BlocDoubleClicked += OnBlocDoubleClickedInPert; // Gardé pour le futur
            tacheDetailView1.TacheChanged += OnTacheDetailChanged;
            tacheDetailView1.TacheDeleteRequested += OnTacheDeleteRequested;
            btnPlanificator.Click += (s, e) => NavigateToViewRequested?.Invoke(this, typeof(PlanificatorView));
        }

        #region Logique de rafraîchissement

        private void RefreshAll()
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

        #region Gestionnaires d'événements (Orchestration)

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
            // pertDiagramControl1.ForceSelection(nouvelleTache.TacheId); // A réactiver plus tard
        }

        private void OnTacheSelectedInPert(object sender, TacheSelectedEventArgs e)
        {
            tacheDetailView1.LoadTache(e.Tache);
        }

        private void OnTacheDetailChanged(object sender, Tache tacheModifiee)
        {
            if (tacheModifiee == null) return;
            _projetService.ModifierTache(tacheModifiee);
            RefreshUIForActiveLot();
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

            using var ofd = new OpenFileDialog
            {
                Title = "Sélectionner le fichier CSV à importer",
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*"
            };

            if (ofd.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                var lotActif = _projetService.ObtenirLotParId(_activeLotId);
                // Note: On utilise le namespace complet pour éviter toute ambiguïté avec PlanAthena.View
                using (var importView = new PlanAthena.Forms.ImportMappingView(ofd.FileName, lotActif, _projetService, _ressourceService))
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
                    var confirmResult = MessageBox.Show($"Le lot '{_activeLotId}' contient déjà des tâches. Voulez-vous les écraser et les remplacer par le contenu de ce fichier ?\n\nCette action est irréversible.", "Confirmation d'écrasement", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirmResult == DialogResult.Yes)
                    {
                        confirmerEcrasement = true;
                    }
                    else
                    {
                        return; // L'utilisateur a annulé
                    }
                }

                var resultat = _importService.ImporterTachesCSV(filePath, _activeLotId, mappingConfig, confirmerEcrasement);

                if (resultat.EstSucces)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Import terminé avec succès !");
                    sb.AppendLine($"- {resultat.NbTachesImportees} tâches importées.");
                    sb.AppendLine($"- {resultat.NbBlocsTraites} nouveaux blocs créés.");

                    if (resultat.Warnings.Any())
                    {
                        sb.AppendLine("\nDes avertissements ont été générés.");
                        // Note: On utilise le namespace complet pour éviter toute ambiguïté
                        using (var warningsView = new PlanAthena.Forms.ImportWarningsView(resultat.Warnings))
                        {
                            warningsView.ShowDialog(this.FindForm());
                        }
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
                MessageBox.Show($"Une erreur inattendue est survenue durant l'import : {ex.Message}", "Erreur Critique", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            MessageBox.Show("La fonction d'export des tâches sera implémentée ultérieurement.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Ajouter la logique d'export en appelant une méthode sur ImportService
        }
        #endregion
    }
}