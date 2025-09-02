using Krypton.Navigator;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using PlanAthena.View.TaskManager;

namespace PlanAthena.View.Ressources.MetierDiagram

{
    public partial class PrerequisMetierView : UserControl
    {
        private readonly ApplicationService _applicationService;
        private readonly RessourceService _ressourceService;
        private readonly ProjetService _projetService;
        private readonly DependanceBuilder _dependanceBuilder;

        public event EventHandler<Type> NavigateToViewRequested;

        private bool _isLoading = false;
        private readonly Dictionary<ChantierPhase, MetierDiagramControl> _diagramsByPhase = new Dictionary<ChantierPhase, MetierDiagramControl>();

        public PrerequisMetierView(ApplicationService applicationService, RessourceService ressourceService, ProjetService projetService, DependanceBuilder dependanceBuilder)
        {
            InitializeComponent();
            _applicationService = applicationService;
            _ressourceService = ressourceService;
            _projetService = projetService;
            _dependanceBuilder = dependanceBuilder;

            this.Load += PrerequisMetierView_Load;
        }

        private void PrerequisMetierView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            SetupPhaseTabs();
            AttachEvents();
            ClearDetails();
        }

        private void SetupPhaseTabs()
        {
            _isLoading = true;
            navigatorPhases.Pages.Clear();
            _diagramsByPhase.Clear();

            var phases = new[] { ChantierPhase.GrosOeuvre, ChantierPhase.SecondOeuvre, ChantierPhase.Finition };
            foreach (var phase in phases)
            {
                var page = new KryptonPage { Text = phase.ToString(), Name = phase.ToString() };
                var diagram = new MetierDiagramControl { Dock = DockStyle.Fill };

                diagram.Initialize(_projetService, _ressourceService, _dependanceBuilder, new MetierDiagramSettings());
                diagram.MetierSelected += (s, args) => OnMetierSelectedInDiagram(phase, args.SelectedMetier);

                page.Controls.Add(diagram);
                navigatorPhases.Pages.Add(page);
                _diagramsByPhase[phase] = diagram;

                RefreshDiagram(phase);
            }
            _isLoading = false;
        }

        private void AttachEvents()
        {
            checkedListPrerequis.ItemCheck += CheckedListPrerequis_ItemCheck;
            btnManageTasks.Click += (s, e) => NavigateToViewRequested?.Invoke(this, typeof(TaskManagerView));
        }

        private void RefreshDiagram(ChantierPhase phase)
        {
            if (!_diagramsByPhase.TryGetValue(phase, out var diagram)) return;

            var metiersForPhase = _ressourceService.GetAllMetiers()
                .Where(m => m.Phases.HasFlag(phase))
                .ToList();

            diagram.ChargerDonnees(metiersForPhase, phase);
        }

        private void OnMetierSelectedInDiagram(ChantierPhase phase, Metier selectedMetier)
        {
            _isLoading = true;

            if (selectedMetier == null)
            {
                ClearDetails();
                _isLoading = false;
                return;
            }

            // Afficher les détails du métier sélectionné
            textId.Text = selectedMetier.MetierId;
            textName.Text = selectedMetier.Nom;
            panelColor.StateCommon.Color1 = _ressourceService.GetDisplayColorForMetier(selectedMetier.MetierId);

            // Lister les prérequis possibles
            checkedListPrerequis.Items.Clear();
            var availablePrereqs = _ressourceService.GetAllMetiers()
                .Where(m => m.MetierId != selectedMetier.MetierId && m.Phases.HasFlag(phase))
                .OrderBy(m => m.Nom)
                .ToList();

            var currentPrereqs = _ressourceService.GetPrerequisPourPhase(selectedMetier.MetierId, phase);

            foreach (var metier in availablePrereqs)
            {
                // Étape 1 : Ajouter l'item
                int index = checkedListPrerequis.Items.Add(metier);

                // Étape 2 : Définir son état coché
                bool isChecked = currentPrereqs.Contains(metier.MetierId);
                checkedListPrerequis.SetItemChecked(index, isChecked);
            }


            checkedListPrerequis.Tag = new { Metier = selectedMetier, Phase = phase }; // Stocker le contexte
            groupDetails.Text = $"Prérequis pour '{selectedMetier.Nom}' ({phase})";
            groupDetails.Enabled = true;

            _isLoading = false;
        }

        private void ClearDetails()
        {
            textId.Clear();
            textName.Clear();
            panelColor.StateCommon.Color1 = SystemColors.Control;
            checkedListPrerequis.Items.Clear();
            groupDetails.Text = "Détails du Prérequis";
            groupDetails.Enabled = false;
        }

        private void CheckedListPrerequis_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_isLoading) return;

            // Utiliser BeginInvoke pour laisser le temps à l'état coché de se mettre à jour
            BeginInvoke(new Action(() =>
            {
                if (checkedListPrerequis.Tag is { } tag)
                {
                    var context = (dynamic)tag;
                    Metier metier = context.Metier;
                    ChantierPhase phase = context.Phase;

                    var selectedPrereqIds = checkedListPrerequis.CheckedItems.Cast<Metier>().Select(m => m.MetierId).ToList();
                    metier.PrerequisParPhase[phase] = selectedPrereqIds;

                    try
                    {
                        _ressourceService.ModifierMetier(metier);
                        // Rafraîchir le diagramme pour voir la nouvelle dépendance
                        RefreshDiagram(phase);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Erreur de dépendance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // Recharger pour annuler le changement visuel
                        OnMetierSelectedInDiagram(phase, metier);
                    }
                }
            }));
        }
    }
}