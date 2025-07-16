using PlanAthena.Data;
using PlanAthena.Services.Business;

namespace PlanAthena.Forms
{
    public partial class TacheDetailForm : System.Windows.Forms.Form
    {
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private TacheRecord _tache;
        private bool _modeCreation;
        private bool _suppressEvents = false; // Pour éviter les boucles infinies

        private List<LotInfo> _lotsDisponibles;
        private List<BlocInfo> _blocsDisponibles;
        private List<TacheRecord> _toutesLesTaches;

        public bool ModeJalonSync { get; set; } = false;
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
            _suppressEvents = true; // Supprimer les événements pendant la mise à jour

            try
            {
                // --- Métiers ---
                cmbMetier.Items.Clear();
                cmbMetier.Items.Add(new { MetierId = "", Nom = "(Aucun métier assigné)" });
                var metiers = _metierService.GetAllMetiers()
                    .Where(m => m.MetierId != _metierService.GetJalonMetierId())
                    .OrderBy(m => m.Nom);
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

                // --- Tâches pour dépendances ---
                _toutesLesTaches = _tacheService.ObtenirToutesLesTaches();
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        public void ChargerTache(TacheRecord tache, bool modeCreation)
        {
            _suppressEvents = true;

            try
            {
                _tache = tache;
                _modeCreation = modeCreation;

                this.grpTache.Text = _modeCreation ? "Nouvelle Tâche" : $"Détails Tâche: {_tache?.TacheId}";

                if (_tache == null)
                {
                    _tache = new TacheRecord { HeuresHommeEstimees = 8 };
                    // Assigner des valeurs par défaut
                    if (_lotsDisponibles.Any())
                    {
                        var premierLot = _lotsDisponibles.First();
                        _tache.LotId = premierLot.LotId;
                        _tache.LotNom = premierLot.LotNom;
                        _tache.LotPriorite = premierLot.Priorite;
                    }
                    if (_blocsDisponibles.Any())
                    {
                        var premierBloc = _blocsDisponibles.First();
                        _tache.BlocId = premierBloc.BlocId;
                        _tache.BlocNom = premierBloc.BlocNom;
                        _tache.BlocCapaciteMaxOuvriers = premierBloc.CapaciteMaxOuvriers;
                    }
                }

                // Remplir les champs
                txtTacheId.Text = _tache.TacheId ?? "";
                txtTacheNom.Text = _tache.TacheNom ?? "";
                numHeuresHomme.Value = _tache.HeuresHommeEstimees;

                bool estJalon = _metierService.EstJalon(_tache);
                chkEstJalon.Checked = estJalon;

                // Sélectionner les ComboBox
                if (!string.IsNullOrEmpty(_tache.LotId))
                {
                    cmbLotNom.SelectedValue = _tache.LotId;
                }
                else if (cmbLotNom.Items.Count > 0)
                {
                    cmbLotNom.SelectedIndex = 0;
                }

                if (!string.IsNullOrEmpty(_tache.BlocId))
                {
                    cmbBlocNom.SelectedValue = _tache.BlocId;
                }
                else if (cmbBlocNom.Items.Count > 0)
                {
                    cmbBlocNom.SelectedIndex = 0;
                }

                // Sélectionner le métier
                if (estJalon)
                {
                    cmbMetier.SelectedIndex = 0;
                }
                else if (string.IsNullOrEmpty(_tache.MetierId))
                {
                    cmbMetier.SelectedIndex = 0;
                }
                else
                {
                    var metierItem = cmbMetier.Items.Cast<object>().FirstOrDefault(item => GetMetierId(item) == _tache.MetierId);
                    cmbMetier.SelectedItem = metierItem ?? cmbMetier.Items[0];
                }

                // Interface
                txtTacheId.ReadOnly = !_modeCreation;
                cmbMetier.Enabled = !estJalon;
                btnSauvegarder.Enabled = true;
            }
            finally
            {
                _suppressEvents = false;
                // CORRECTION: Charger les dépendances APRÈS avoir configuré tous les champs
                ChargerListeDependances();
            }
        }

        private void ChargerListeDependances()
        {
            chkListDependances.Items.Clear();

            if (_tache == null) return;

            System.Diagnostics.Debug.WriteLine($"[FIX] ChargerListeDependances - BlocId: '{_tache.BlocId}'");

            // Obtenir les dépendances directes et héritées
            var dependancesDirectes = ObtenirDependancesDirectes();
            var dependancesHeritees = ObtenirDependancesHeritees();
            var exclusions = ObtenirExclusions();

            // CORRECTION: Filtrage simplifié mais correct
            var tachesCandidates = _toutesLesTaches
                .Where(t => t.TacheId != _tache.TacheId && // Pas la tâche elle-même
                           (t.BlocId == _tache.BlocId ||    // Tâches du même bloc
                            _metierService.EstJalon(t)))    // OU jalons (globaux)
                .OrderBy(t => t.TacheId)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[FIX] {tachesCandidates.Count} tâches candidates trouvées");

            foreach (var tacheCandidate in tachesCandidates)
            {
                bool estDependanceDirecte = dependancesDirectes.Contains(tacheCandidate.TacheId);
                bool estDependanceHeritee = dependancesHeritees.Contains(tacheCandidate.TacheId);
                bool estExclue = exclusions.Contains(tacheCandidate.TacheId);
                bool estCochee = (estDependanceDirecte || estDependanceHeritee) && !estExclue;

                var item = new DependanceItem
                {
                    TacheId = tacheCandidate.TacheId,
                    TacheNom = tacheCandidate.TacheNom,
                    EstHeritee = estDependanceHeritee && !estDependanceDirecte,
                    EstExclue = estExclue
                };

                int index = chkListDependances.Items.Add(item);
                chkListDependances.SetItemChecked(index, estCochee);
            }

            System.Diagnostics.Debug.WriteLine($"[FIX] {chkListDependances.Items.Count} items ajoutés à la liste");
        }

        private HashSet<string> ObtenirDependancesDirectes()
        {
            if (string.IsNullOrEmpty(_tache?.Dependencies))
                return new HashSet<string>();

            return new HashSet<string>(
                _tache.Dependencies.Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
            );
        }

        private HashSet<string> ObtenirDependancesHeritees()
        {
            var dependancesHeritees = new HashSet<string>();

            if (string.IsNullOrEmpty(_tache?.MetierId) || _metierService.EstJalon(_tache))
                return dependancesHeritees;

            var prerequisMetiers = _metierService.GetPrerequisForMetier(_tache.MetierId);

            foreach (var prerequisMetier in prerequisMetiers)
            {
                var tachesPrerequisBloc = _toutesLesTaches
                    .Where(t => t.BlocId == _tache.BlocId && t.MetierId == prerequisMetier)
                    .Select(t => t.TacheId);

                foreach (var tacheId in tachesPrerequisBloc)
                {
                    dependancesHeritees.Add(tacheId);
                }
            }

            return dependancesHeritees;
        }

        private HashSet<string> ObtenirExclusions()
        {
            if (string.IsNullOrEmpty(_tache?.ExclusionsDependances))
                return new HashSet<string>();

            return new HashSet<string>(
                _tache.ExclusionsDependances.Split(',')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
            );
        }

        private string GetMetierId(object item)
        {
            if (item is MetierRecord metier) return metier.MetierId;
            var prop = item.GetType().GetProperty("MetierId");
            return prop?.GetValue(item)?.ToString() ?? "";
        }

        #region Événements

        private void cmbBlocNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null) return;

            if (cmbBlocNom.SelectedItem is BlocInfo selectedBloc)
            {
                _tache.BlocId = selectedBloc.BlocId;
                _tache.BlocNom = selectedBloc.BlocNom;
                _tache.BlocCapaciteMaxOuvriers = selectedBloc.CapaciteMaxOuvriers;
                numBlocCapacite.Value = selectedBloc.CapaciteMaxOuvriers;

                System.Diagnostics.Debug.WriteLine($"[FIX] Bloc changé vers: {selectedBloc.BlocId}");

                // CORRECTION: Recharger les dépendances avec le nouveau bloc
                ChargerListeDependances();
            }
        }

        private void cmbLotNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null) return;

            if (cmbLotNom.SelectedItem is LotInfo selectedLot)
            {
                _tache.LotId = selectedLot.LotId;
                _tache.LotNom = selectedLot.LotNom;
                _tache.LotPriorite = selectedLot.Priorite;
                numLotPriorite.Value = selectedLot.Priorite;

                System.Diagnostics.Debug.WriteLine($"[FIX] Lot changé vers: {selectedLot.LotId}");
            }
        }

        private void cmbMetier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null || chkEstJalon.Checked) return;

            _tache.MetierId = GetMetierId(cmbMetier.SelectedItem);
            System.Diagnostics.Debug.WriteLine($"[FIX] Métier changé vers: {_tache.MetierId}");

            // Recharger les dépendances car elles peuvent changer selon le métier
            ChargerListeDependances();
        }

        private void chkEstJalon_CheckedChanged(object sender, EventArgs e)
        {
            bool estJalon = chkEstJalon.Checked;
            cmbMetier.Enabled = !estJalon;

            if (estJalon)
            {
                if (string.IsNullOrEmpty(txtTacheNom.Text) || txtTacheNom.Text.StartsWith("Attente"))
                {
                    txtTacheNom.Text = $"Attente {numHeuresHomme.Value} heures";
                }
                cmbMetier.SelectedIndex = 0;
            }
        }

        private void numHeuresHomme_ValueChanged(object sender, EventArgs e)
        {
            if (chkEstJalon.Checked && (string.IsNullOrEmpty(txtTacheNom.Text) || txtTacheNom.Text.StartsWith("Attente")))
            {
                txtTacheNom.Text = $"Attente {numHeuresHomme.Value} heures";
            }
        }

        #endregion

        #region Sauvegarde

        private void btnSauvegarder_Click(object sender, EventArgs e)
        {
            if (!ValiderSaisie()) return;

            SauvegarderModifications();

            try
            {
                if (ModeJalonSync)
                {
                    _tacheService.ModifierOuCreerJalonSync(_tache);
                }
                else
                {
                    if (_modeCreation)
                    {
                        _tacheService.AjouterTache(_tache);
                    }
                    else
                    {
                        _tacheService.ModifierTache(_tache);
                    }
                }

                MettreAJourListesDeroulantes();
                TacheSauvegardee?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("Tâche sauvegardée avec succès.", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _modeCreation = false;
                this.grpTache.Text = $"Détails Tâche: {_tache.TacheId}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValiderSaisie()
        {
            if (string.IsNullOrWhiteSpace(txtTacheNom.Text))
            {
                MessageBox.Show("Le nom de la tâche est obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTacheNom.Focus();
                return false;
            }

            if (!chkEstJalon.Checked && cmbMetier.SelectedIndex == 0)
            {
                var result = MessageBox.Show("Aucun métier n'est assigné à cette tâche. Continuer ?",
                    "Métier non assigné", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    cmbMetier.Focus();
                    return false;
                }
            }

            return true;
        }

        private void SauvegarderModifications()
        {
            _tache.TacheNom = txtTacheNom.Text.Trim();
            _tache.HeuresHommeEstimees = (int)numHeuresHomme.Value;

            if (chkEstJalon.Checked)
            {
                _tache.MetierId = _metierService.GetJalonMetierId();
            }
            else
            {
                _tache.MetierId = GetMetierId(cmbMetier.SelectedItem);
            }

            // Sauvegarder les dépendances et exclusions
            SauvegarderDependances();
        }

        private void SauvegarderDependances()
        {
            var dependancesDirectes = new List<string>();
            var exclusions = new List<string>();
            var dependancesHeritees = ObtenirDependancesHeritees();

            for (int i = 0; i < chkListDependances.Items.Count; i++)
            {
                var item = (DependanceItem)chkListDependances.Items[i];
                bool estCochee = chkListDependances.GetItemChecked(i);

                if (item.EstHeritee)
                {
                    // Pour les dépendances héritées, si elles ne sont pas cochées, c'est une exclusion
                    if (!estCochee)
                    {
                        exclusions.Add(item.TacheId);
                    }
                }
                else
                {
                    // Pour les dépendances non héritées, si elles sont cochées, c'est une dépendance directe
                    if (estCochee)
                    {
                        dependancesDirectes.Add(item.TacheId);
                    }
                }
            }

            _tache.Dependencies = string.Join(",", dependancesDirectes);
            _tache.ExclusionsDependances = string.Join(",", exclusions);
        }

        #endregion

        #region Affichage

        private void chkListDependances_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var item = (DependanceItem)chkListDependances.Items[e.Index];

            e.DrawBackground();

            Color textColor = Color.Black;
            Font font = e.Font;

            if (item.EstHeritee)
            {
                textColor = Color.Blue;
                font = new Font(e.Font, FontStyle.Italic);
            }

            if (item.EstExclue)
            {
                textColor = Color.Red;
                font = new Font(e.Font, FontStyle.Strikeout);
            }

            using (var brush = new SolidBrush(textColor))
            {
                var text = $"{item.TacheId} - {item.TacheNom}";
                if (item.EstHeritee) text += " (hérité)";
                if (item.EstExclue) text += " (exclu)";

                e.Graphics.DrawString(text, font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        #endregion
    }

    /// <summary>
    /// Représente un item de dépendance dans la CheckedListBox
    /// </summary>
    public class DependanceItem
    {
        public string TacheId { get; set; }
        public string TacheNom { get; set; }
        public bool EstHeritee { get; set; }
        public bool EstExclue { get; set; }

        public override string ToString()
        {
            var result = $"{TacheId} - {TacheNom}";
            if (EstHeritee) result += " (hérité)";
            if (EstExclue) result += " (exclu)";
            return result;
        }
    }
}