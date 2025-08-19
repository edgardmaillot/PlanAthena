using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View
{
    public partial class TacheDetailView : UserControl
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly DependanceBuilder _dependanceBuilder;

        private Tache _currentTache;
        private bool _isNewTacheMode;
        private bool _isLoading = false;
        private List<Bloc> _availableBlocs;

        public event EventHandler<Tache> TacheChanged;
        public event EventHandler<Tache> TacheDeleteRequested;

        public TacheDetailView()
        {
            // Constructeur pour le Designer de VS
            InitializeComponent();
        }

        public TacheDetailView(ProjetService projetService, RessourceService ressourceService, DependanceBuilder dependanceBuilder)
        {
            InitializeComponent();
            _projetService = projetService;
            _ressourceService = ressourceService;
            _dependanceBuilder = dependanceBuilder;

            AttachEvents();
            Clear();
        }

        private void AttachEvents()
        {
            textTacheNom.TextChanged += OnDetailChanged;
            numHeuresHomme.ValueChanged += OnDetailChanged;
            cmbMetier.SelectedIndexChanged += OnDetailChanged;
            cmbBlocNom.SelectedIndexChanged += OnDetailChanged;
            chkIsJalon.CheckedChanged += OnDetailChanged;
            chkListDependances.ItemCheck += (s, e) => BeginInvoke(new Action(() => OnDetailChanged(s, e)));
            btnSupprimer.Click += (s, e) => TacheDeleteRequested?.Invoke(this, _currentTache);
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
            _isNewTacheMode = isNew;

            if (_currentTache == null)
            {
                Clear();
                _isLoading = false;
                return;
            }

            textTacheNom.Text = _currentTache.TacheNom;
            numHeuresHomme.Value = Math.Max(numHeuresHomme.Minimum, _currentTache.HeuresHommeEstimees);
            chkIsJalon.Checked = _currentTache.EstJalon;

            cmbBlocNom.Enabled = true;
            cmbBlocNom.SelectedValue = _availableBlocs.Any(b => b.BlocId == _currentTache.BlocId) ? _currentTache.BlocId : "";
            cmbMetier.SelectedValue = !string.IsNullOrEmpty(_currentTache.MetierId) ? _currentTache.MetierId : "";

            this.Enabled = true;
            _isLoading = false;

            LoadDependencies();
        }

        public void Clear()
        {
            _isLoading = true;
            _currentTache = null;
            textTacheNom.Clear();
            numHeuresHomme.Value = numHeuresHomme.Minimum;
            chkIsJalon.Checked = false;
            cmbBlocNom.SelectedIndex = -1;
            cmbMetier.SelectedIndex = -1;
            numBlocCapacite.Value = numBlocCapacite.Minimum;
            chkListDependances.Items.Clear();
            this.Enabled = false;
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

            var lot = _projetService.ObtenirLotParId(_currentTache.LotId);
            if (lot == null)
            {
                _isLoading = false;
                return;
            }

            var tachesDuMemeBloc = _projetService.ObtenirTachesParBloc(_currentTache.BlocId);
            var etatsDependances = _dependanceBuilder.ObtenirDependancesPourTache(_currentTache, tachesDuMemeBloc, lot.Phases);

            etatsDependances = etatsDependances.OrderBy(d => d.TachePredecesseur.TacheNom).ToList();

            foreach (var etat in etatsDependances)
            {
                bool isChecked = etat.Etat == EtatDependance.Stricte || etat.Etat == EtatDependance.Suggeree;
                int index = chkListDependances.Items.Add(etat);
                chkListDependances.SetItemChecked(index, isChecked);
            }
            _isLoading = false;
        }

        private void OnDetailChanged(object sender = null, EventArgs e = null)
        {
            if (_isLoading || _currentTache == null) return;

            _currentTache.TacheNom = textTacheNom.Text;
            _currentTache.HeuresHommeEstimees = (int)numHeuresHomme.Value;
            _currentTache.Type = chkIsJalon.Checked ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;
            _currentTache.MetierId = _currentTache.EstJalon ? "" : (string)cmbMetier.SelectedValue ?? "";

            if (cmbBlocNom.SelectedValue is string blocId)
            {
                _currentTache.BlocId = blocId;
                var selectedBloc = _availableBlocs.FirstOrDefault(b => b.BlocId == blocId);
                if (selectedBloc != null) numBlocCapacite.Value = selectedBloc.CapaciteMaxOuvriers;
            }

            var dependancesStricts = new List<string>();
            var exclusions = new List<string>();
            foreach (DependanceAffichage item in chkListDependances.Items)
            {
                bool estCochee = chkListDependances.CheckedItems.Contains(item);
                var tacheIdPredecesseur = item.TachePredecesseur.TacheId;

                if (!item.EstHeritee && estCochee) dependancesStricts.Add(tacheIdPredecesseur);
                else if (item.EstHeritee && !estCochee) exclusions.Add(tacheIdPredecesseur);
                else if (item.EstHeritee && estCochee) dependancesStricts.Add(tacheIdPredecesseur);
            }
            _currentTache.Dependencies = string.Join(",", dependancesStricts.Distinct());
            _currentTache.ExclusionsDependances = string.Join(",", exclusions.Distinct());

            if (sender == cmbMetier || sender == cmbBlocNom || sender == chkIsJalon)
            {
                LoadDependencies();
            }

            TacheChanged?.Invoke(this, _currentTache);
        }
    }
}