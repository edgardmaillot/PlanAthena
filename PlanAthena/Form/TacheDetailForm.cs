// Fichier : TacheDetailForm.cs

using PlanAthena.CsvModels;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class TacheDetailForm : System.Windows.Forms.Form
    {
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private TacheCsvRecord _tache;
        private bool _modeCreation;

        private List<LotInfo> _lotsDisponibles;
        private List<BlocInfo> _blocsDisponibles;

        public event EventHandler TacheSauvegardee;

        public TacheDetailForm(TacheService tacheService, MetierService metierService)
        {
            InitializeComponent();
            _tacheService = tacheService;
            _metierService = metierService;

            MettreAJourListesDeroulantes();
            ChargerTache(null, true);
        }

        private void MettreAJourListesDeroulantes()
        {
            // --- Métiers ---
            cmbMetier.Items.Clear();
            cmbMetier.Items.Add(new { MetierId = "", Nom = "(Aucun métier assigné)" });
            var metiers = _metierService.GetAllMetiers().OrderBy(m => m.Nom);
            cmbMetier.Items.AddRange(metiers.ToArray());
            cmbMetier.DisplayMember = "Nom";
            cmbMetier.ValueMember = "MetierId";

            // --- Lots ---
            _lotsDisponibles = _tacheService.ObtenirTousLesLots();
            cmbLotNom.DataSource = null;
            cmbLotNom.DataSource = _lotsDisponibles;
            cmbLotNom.DisplayMember = "LotNom";
            cmbLotNom.ValueMember = "LotId";

            // --- Blocs ---
            _blocsDisponibles = _tacheService.ObtenirTousLesBlocs();
            cmbBlocNom.DataSource = null;
            cmbBlocNom.DataSource = _blocsDisponibles;
            cmbBlocNom.DisplayMember = "BlocNom";
            cmbBlocNom.ValueMember = "BlocId";
        }

        public void ChargerTache(TacheCsvRecord tache, bool modeCreation)
        {
            _tache = tache;
            _modeCreation = modeCreation;

            this.grpTache.Text = _modeCreation ? "Nouvelle Tâche" : $"Détails Tâche: {_tache?.TacheId}";

            if (_tache == null)
            {
                _tache = new TacheCsvRecord { HeuresHommeEstimees = 8 };
                // Pour une nouvelle tâche, on sélectionne le premier lot/bloc par défaut
                if (_lotsDisponibles.Any()) _tache.LotId = _lotsDisponibles.First().LotId;
                if (_blocsDisponibles.Any()) _tache.BlocId = _blocsDisponibles.First().BlocId;
            }

            // Remplir les champs
            txtTacheId.Text = _tache.TacheId;
            txtTacheNom.Text = _tache.TacheNom;
            numHeuresHomme.Value = _tache.HeuresHommeEstimees;
            txtDependances.Text = _tache.Dependencies ?? "";

            // Sélectionner les items dans les ComboBox
            cmbLotNom.SelectedValue = _tache.LotId ?? "";
            cmbBlocNom.SelectedValue = _tache.BlocId ?? "";

            // Sélectionner le métier
            if (string.IsNullOrEmpty(_tache.MetierId))
            {
                cmbMetier.SelectedIndex = 0;
            }
            else
            {
                var metierItem = cmbMetier.Items.Cast<object>().FirstOrDefault(item => item is MetierCsvRecord m && m.MetierId == _tache.MetierId);
                cmbMetier.SelectedItem = metierItem ?? cmbMetier.Items[0];
            }

            txtTacheId.ReadOnly = !_modeCreation;
            btnSauvegarder.Enabled = tache != null || modeCreation;
        }

        private void btnSauvegarder_Click(object sender, EventArgs e)
        {
            if (!ValiderSaisie()) return;

            SauvegarderModifications();

            try
            {
                if (_modeCreation)
                {
                    _tacheService.AjouterTache(_tache);
                }
                else
                {
                    _tacheService.ModifierTache(_tache);
                }

                MettreAJourListesDeroulantes(); // Rafraîchir au cas où un nouveau lot/bloc a été créé implicitement
                TacheSauvegardee?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("Tâche sauvegardée avec succès.", "Sauvegarde réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _modeCreation = false;
                txtTacheId.ReadOnly = true;
                this.grpTache.Text = $"Détails Tâche: {_tache.TacheId}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValiderSaisie()
        {
            if (string.IsNullOrWhiteSpace(txtTacheId.Text))
            {
                MessageBox.Show("L'ID de la tâche est obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTacheId.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTacheNom.Text))
            {
                MessageBox.Show("Le nom de la tâche est obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTacheNom.Focus();
                return false;
            }
            if (_modeCreation && _tacheService.ObtenirTacheParId(txtTacheId.Text) != null)
            {
                MessageBox.Show($"La tâche {txtTacheId.Text} existe déjà.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTacheId.Focus();
                return false;
            }
            return true;
        }

        private void SauvegarderModifications()
        {
            _tache.TacheId = txtTacheId.Text.Trim();
            _tache.TacheNom = txtTacheNom.Text.Trim();
            _tache.HeuresHommeEstimees = (int)numHeuresHomme.Value;
            _tache.Dependencies = txtDependances.Text.Trim();

            // Sauvegarde depuis les ComboBox
            if (cmbLotNom.SelectedItem is LotInfo selectedLot)
            {
                _tache.LotId = selectedLot.LotId;
                _tache.LotNom = selectedLot.LotNom;
                _tache.LotPriorite = selectedLot.Priorite;
            }

            if (cmbBlocNom.SelectedItem is BlocInfo selectedBloc)
            {
                _tache.BlocId = selectedBloc.BlocId;
                _tache.BlocNom = selectedBloc.BlocNom;
                _tache.BlocCapaciteMaxOuvriers = selectedBloc.CapaciteMaxOuvriers;
            }

            if (cmbMetier.SelectedItem is MetierCsvRecord metier)
            {
                _tache.MetierId = metier.MetierId;
            }
            else
            {
                _tache.MetierId = "";
            }
        }

        private void cmbLotNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLotNom.SelectedItem is LotInfo selectedLot)
            {
                numLotPriorite.Value = selectedLot.Priorite;
            }
        }

        private void cmbBlocNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBlocNom.SelectedItem is BlocInfo selectedBloc)
            {
                numBlocCapacite.Value = selectedBloc.CapaciteMaxOuvriers;
            }
        }
    }
}