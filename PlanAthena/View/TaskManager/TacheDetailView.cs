using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Utilities;

namespace PlanAthena.View.TaskManager
{
    public partial class TacheDetailView : UserControl
    {
        // Services
        private ProjetService _projetService;
        private TaskManagerService _taskManagerService;
        private RessourceService _ressourceService;
        private DependanceBuilder _dependanceBuilder;

        // État interne
        private Tache _currentTache;
        private bool _isLoading = false;
        private List<Bloc> _availableBlocs;

        // Événements publics
        public event EventHandler<Tache> SaveRequested;
        public event EventHandler<Tache> TacheDeleteRequested;

        /// <summary>
        /// Constructeur public vide, nécessaire pour le Designer de Windows Forms.
        /// </summary>
        public TacheDetailView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialise le contrôle avec les services nécessaires après sa création.
        /// </summary>
        public void InitializeServices(ProjetService projetService, TaskManagerService taskManagerService, RessourceService ressourceService, DependanceBuilder dependanceBuilder)
        {
            _projetService = projetService;
            _taskManagerService = taskManagerService;
            _ressourceService = ressourceService;
            _dependanceBuilder = dependanceBuilder;

            // Le dessin personnalisé n'est plus nécessaire avec Krypton
            AttachEvents();
            Clear();
        }

        private void AttachEvents()
        {
            // Événements pour les rafraîchissements LOCAUX
            cmbBlocNom.SelectedIndexChanged += CmbBlocNom_SelectedIndexChanged;
            cmbMetier.SelectedIndexChanged += CmbMetier_SelectedIndexChanged;
            chkIsJalon.CheckedChanged += ChkIsJalon_CheckedChanged;
            numHeuresHomme.ValueChanged += NumHeuresHomme_ValueChanged;

            chkListDependances.ItemCheck += ChkListDependances_ItemCheck;

            // Événements pour les actions explicites
            btnSauvegarder.Click += BtnSauvegarder_Click;
            btnSupprimer.Click += (s, e) => { if (_currentTache != null) TacheDeleteRequested?.Invoke(this, _currentTache); };
        }

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

                _availableBlocs = _projetService.ObtenirBlocsParLot(lotId);
                cmbBlocNom.DataSource = _availableBlocs;
                cmbBlocNom.DisplayMember = "Nom";
                cmbBlocNom.ValueMember = "BlocId";
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void LoadTache(Tache tache, bool isNew = false)
        {
            _isLoading = true;
            _currentTache = tache;

            if (_currentTache == null)
            {
                Clear();
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

        public void Clear()
        {
            _isLoading = true;
            _currentTache = null;
            txtTacheNom.Clear();
            numHeuresHomme.Value = numHeuresHomme.Minimum;
            chkIsJalon.Checked = false;
            cmbMetier.SelectedIndex = -1;
            cmbBlocNom.SelectedIndex = -1;
            if (numBlocCapacite != null) numBlocCapacite.Value = 3; // Vérification de nullité
            chkListDependances.Items.Clear();
            _isLoading = false;
        }

        private void LoadDependencies()
        {
            _isLoading = true;
            chkListDependances.Items.Clear();

            if (_currentTache == null || string.IsNullOrEmpty(_currentTache.LotId) || string.IsNullOrEmpty(_currentTache.BlocId))
            {
                _isLoading = false;
                return;
            }

            try
            {
                var lot = _projetService.ObtenirLotParId(_currentTache.LotId);
                if (lot == null) return;

                var tachesDuMemeBloc = _taskManagerService.ObtenirToutesLesTaches(blocId: _currentTache.BlocId);
                var etatsDependances = _dependanceBuilder.ObtenirDependancesPourTache(_currentTache, tachesDuMemeBloc, lot.Phases)
                                                        .OrderBy(d => d.TachePredecesseur.TacheNom)
                                                        .ToList();

                foreach (var etat in etatsDependances)
                {
                    var displayItem = new DependanceDisplayItem(etat);
                    bool isChecked = etat.Etat == EtatDependance.Stricte || etat.Etat == EtatDependance.Suggeree;
                    int index = chkListDependances.Items.Add(displayItem);
                    chkListDependances.SetItemChecked(index, isChecked);
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void CmbBlocNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;

            if (cmbBlocNom.SelectedItem is Bloc selectedBloc)
            {
                _currentTache.BlocId = selectedBloc.BlocId;
                if (numBlocCapacite != null) numBlocCapacite.Value = selectedBloc.CapaciteMaxOuvriers;
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
                chkListDependances.Items[e.Index] = item;
            }
        }

        private void BtnSauvegarder_Click(object sender, EventArgs e)
        {
            if (_currentTache == null) return;

            _currentTache.TacheNom = txtTacheNom.Text;
            _currentTache.HeuresHommeEstimees = (int)numHeuresHomme.Value;

            var dependancesStricts = new List<string>();
            var exclusions = new List<string>();

            for (int i = 0; i < chkListDependances.Items.Count; i++)
            {
                var item = (DependanceDisplayItem)chkListDependances.Items[i];
                bool estCochee = chkListDependances.GetItemChecked(i);
                var tacheIdPredecesseur = item.OriginalData.TachePredecesseur.TacheId;

                if (!item.OriginalData.EstHeritee && estCochee) dependancesStricts.Add(tacheIdPredecesseur);
                else if (item.OriginalData.EstHeritee && !estCochee) exclusions.Add(tacheIdPredecesseur);
                else if (item.OriginalData.EstHeritee && estCochee) dependancesStricts.Add(tacheIdPredecesseur);
            }

            _currentTache.Dependencies = string.Join(",", dependancesStricts.Distinct());
            _currentTache.ExclusionsDependances = string.Join(",", exclusions.Distinct());

            SaveRequested?.Invoke(this, _currentTache);
        }
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
            string prefix = "☐ ";

            if (OriginalData.EstHeritee)
            {
                prefix = _isChecked ? "☑ " : "☒ ";
            }
            else if (_isChecked)
            {
                prefix = "✅ ";
            }

            return $"{prefix}{OriginalData.TachePredecesseur.TacheNom}";
        }
    }
}