using System;
using System.Linq;
using System.Windows.Forms;
using PlanAthena.CsvModels;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;

namespace PlanAthena.Forms
{
    public partial class TacheDetailForm : System.Windows.Forms.Form
    {
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private TacheCsvRecord _tache;
        private bool _modeCreation;

        public TacheCsvRecord Tache => _tache;
        public bool Modifiee { get; private set; }

        public TacheDetailForm(TacheService tacheService, MetierService metierService,
                              TacheCsvRecord tache = null, bool modeCreation = false)
        {
            InitializeComponent();
            _tacheService = tacheService;
            _metierService = metierService;
            _tache = tache;
            _modeCreation = modeCreation;

            InitialiserInterface();
            ChargerDonnees();
        }

        private void InitialiserInterface()
        {
            this.Text = _modeCreation ? "Nouvelle Tâche" : "Détails de la Tâche";

            // Initialiser le combo des métiers
            cmbMetier.Items.Clear();
            cmbMetier.Items.Add(new { MetierId = "", Nom = "(Aucun métier assigné)" });

            var metiers = _metierService.GetAllMetiers().OrderBy(m => m.Nom);
            foreach (var metier in metiers)
            {
                cmbMetier.Items.Add(metier);
            }

            cmbMetier.DisplayMember = "Nom";
            cmbMetier.ValueMember = "MetierId";
        }

        private void ChargerDonnees()
        {
            if (_tache == null)
            {
                // Mode création - valeurs par défaut
                _tache = new TacheCsvRecord
                {
                    TacheId = "",
                    TacheNom = "",
                    HeuresHommeEstimees = 8,
                    MetierId = "",
                    Dependencies = "",
                    LotId = "LOT_01",
                    LotNom = "Lot par défaut",
                    LotPriorite = 1,
                    BlocId = "BLOC_01",
                    BlocNom = "Bloc par défaut",
                    BlocCapaciteMaxOuvriers = 3
                };
            }

            // Remplir les champs
            txtTacheId.Text = _tache.TacheId;
            txtTacheNom.Text = _tache.TacheNom;
            numHeuresHomme.Value = _tache.HeuresHommeEstimees;
            txtDependances.Text = _tache.Dependencies ?? "";
            txtLotId.Text = _tache.LotId;
            txtLotNom.Text = _tache.LotNom;
            numLotPriorite.Value = _tache.LotPriorite;
            txtBlocId.Text = _tache.BlocId;
            txtBlocNom.Text = _tache.BlocNom;
            numBlocCapacite.Value = _tache.BlocCapaciteMaxOuvriers;

            // Sélectionner le métier
            if (string.IsNullOrEmpty(_tache.MetierId))
            {
                cmbMetier.SelectedIndex = 0;
            }
            else
            {
                var metierItem = cmbMetier.Items.Cast<object>()
                    .FirstOrDefault(item => item is MetierCsvRecord metier && metier.MetierId == _tache.MetierId);
                cmbMetier.SelectedItem = metierItem ?? cmbMetier.Items[0];
            }

            txtTacheId.ReadOnly = !_modeCreation;
        }

        private void btnOK_Click(object sender, EventArgs e)
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

                Modifiee = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValiderSaisie()
        {
            if (string.IsNullOrWhiteSpace(txtTacheId.Text))
            {
                MessageBox.Show("L'ID de la tâche est obligatoire.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTacheId.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTacheNom.Text))
            {
                MessageBox.Show("Le nom de la tâche est obligatoire.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTacheNom.Focus();
                return false;
            }

            if (_modeCreation && _tacheService.ObtenirTacheParId(txtTacheId.Text) != null)
            {
                MessageBox.Show($"La tâche {txtTacheId.Text} existe déjà.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            _tache.LotId = txtLotId.Text.Trim();
            _tache.LotNom = txtLotNom.Text.Trim();
            _tache.LotPriorite = (int)numLotPriorite.Value;
            _tache.BlocId = txtBlocId.Text.Trim();
            _tache.BlocNom = txtBlocNom.Text.Trim();
            _tache.BlocCapaciteMaxOuvriers = (int)numBlocCapacite.Value;

            if (cmbMetier.SelectedItem is MetierCsvRecord metier)
            {
                _tache.MetierId = metier.MetierId;
            }
            else
            {
                _tache.MetierId = "";
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}