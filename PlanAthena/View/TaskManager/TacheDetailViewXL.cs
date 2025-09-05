using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager
{
    public partial class TacheDetailViewXL : UserControl
    {
        private Tache _currentTache;
        private bool _isLoading;

        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private TaskManagerService _taskManagerService;

        public event EventHandler TacheSaved;
        public event EventHandler<Tache> TacheDeleteRequested;

        public TacheDetailViewXL()
        {
            InitializeComponent();

            // --- CORRECTION : Déplacer l'initialisation ici ---
            // Le constructeur est le bon endroit pour attacher les événements 
            // et mettre le contrôle dans son état initial (vide).
            // Cela ne s'exécutera qu'une seule fois à la création de l'objet.
            AttachEvents();
            Clear();
        }

        public void InitializeServices(ProjetService projetService, RessourceService ressourceService, TaskManagerService taskManagerService)
        {
            _projetService = projetService;
            _ressourceService = ressourceService;
            _taskManagerService = taskManagerService;

            // --- CORRECTION : On retire la gestion de l'événement Load ---
            // this.Load += TacheDetailViewXL_Load; // <--- SUPPRIMER CETTE LIGNE
        }

        // --- CORRECTION : La méthode TacheDetailViewXL_Load n'est plus nécessaire ---
        /*
        private void TacheDetailViewXL_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            // Ce code a été déplacé dans le constructeur.
            // AttachEvents();
            // Clear();
        }
        */

        private void AttachEvents()
        {
            btnSauvegarder.Click += BtnSauvegarder_Click;
            btnSupprimer.Click += BtnSupprimer_Click;
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (_currentTache != null)
            {
                TacheDeleteRequested?.Invoke(this, _currentTache);
            }
        }

        private void BtnSauvegarder_Click(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;
            ApplyChangesToTache();
            TacheSaved?.Invoke(this, EventArgs.Empty);
        }

        public void LoadTache(Tache tache)
        {
            _isLoading = true;
            _currentTache = tache;

            if (tache != null)
            {
                kryptonHeader1.Values.Heading = $"Édition : {tache.TacheNom}";
                kryptonHeader1.Values.Description = $"ID: {tache.TacheId}";
                txtTacheNom.Text = tache.TacheNom;
                numHeuresHomme.Value = tache.HeuresHommeEstimees;
                chkIsJalon.Checked = tache.EstJalon;

                PopulateComboBoxes();

                cmbBlocNom.SelectedValue = tache.BlocId ?? "";
                cmbMetier.SelectedValue = tache.MetierId ?? "";
                cmbEtat.SelectedItem = tache.Statut;

                heureDebut.Text = tache.DateDebutPlanifiee?.ToString("dd/MM/yy HH:mm") ?? "N/A";
                heureFin.Text = tache.DateFinPlanifiee?.ToString("dd/MM/yy HH:mm") ?? "N/A";

                this.Enabled = true;
            }
            else
            {
                Clear();
            }
            _isLoading = false;
        }

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

            _isLoading = false;
        }

        private void PopulateComboBoxes()
        {
            // Vérification que les services sont initialisés avant de les utiliser
            if (_projetService == null || _ressourceService == null) return;

            var lots = _projetService.ObtenirTousLesLots();
            var allBlocs = lots.SelectMany(l => l.Blocs).OrderBy(b => b.Nom).ToList();
            cmbBlocNom.DataSource = allBlocs;
            cmbBlocNom.DisplayMember = "Nom";
            cmbBlocNom.ValueMember = "BlocId";

            var allMetiers = _ressourceService.GetAllMetiers().OrderBy(m => m.Nom).ToList();
            cmbMetier.DataSource = allMetiers;
            cmbMetier.DisplayMember = "Nom";
            cmbMetier.ValueMember = "MetierId";

            cmbEtat.DataSource = Enum.GetValues(typeof(Statut));
        }

        private void ApplyChangesToTache()
        {
            if (_currentTache == null) return;

            _currentTache.TacheNom = txtTacheNom.Text;
            _currentTache.HeuresHommeEstimees = (int)numHeuresHomme.Value;
            _currentTache.Type = chkIsJalon.Checked ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;
            _currentTache.BlocId = cmbBlocNom.SelectedValue as string ?? "";
            _currentTache.MetierId = cmbMetier.SelectedValue as string ?? "";

            if (cmbEtat.SelectedItem is Statut statut)
            {
                _currentTache.Statut = statut;
            }
        }
    }
}