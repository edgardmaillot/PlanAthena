using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Gère l'affichage et l'édition des détails d'une seule Tâche ou Jalon.
    /// Ce formulaire est conçu pour être utilisé de deux manières :
    /// 1. Embarqué dans un autre formulaire (ex: TacheForm) pour un affichage passif.
    /// 2. Affiché en tant que fenêtre de dialogue modale (popup) pour la création ou l'édition ciblée.
    /// </summary>
    public partial class TacheDetailForm : System.Windows.Forms.Form
    {
        // Services
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly LotService _lotService;
        private readonly BlocService _blocService;

        // État interne
        private Tache _tache;
        private bool _modeCreation;
        private bool _suppressEvents = false;

        // Sources de données pour les ComboBox
        private List<Bloc> _blocsDisponibles;
        private List<Tache> _toutesLesTaches;

        public event EventHandler TacheSauvegardee;

        public TacheDetailForm(TacheService tacheService, MetierService metierService, LotService lotService, BlocService blocService)
        {
            InitializeComponent();
            _tacheService = tacheService;
            _metierService = metierService;
            _lotService = lotService;
            _blocService = blocService;

            MettreAJourListesDeroulantes();
            ChargerTache(null, true);
        }

        public class DependanceItem
        {
            public string TacheId { get; set; }
            public string TacheNom { get; set; }
            public bool EstHeritee { get; set; }
            public bool EstExclue { get; set; }

            public override string ToString()
            {
                var result = $"{TacheNom}";
                if (EstHeritee) result += " (hérité)";
                if (EstExclue) result += " (exclu)";
                return result;
            }
        }
        public void MettreAJourListesDeroulantes()
        {
            _suppressEvents = true;
            try
            {
                cmbMetier.Items.Clear();
                cmbMetier.Items.Add(new { MetierId = "", Nom = "(Aucun métier assigné)" });
                var metiers = _metierService.GetAllMetiers().OrderBy(m => m.Nom);
                cmbMetier.Items.AddRange(metiers.ToArray());
                cmbMetier.DisplayMember = "Nom";
                cmbMetier.ValueMember = "MetierId";


                _blocsDisponibles = _blocService.ObtenirTousLesBlocs();
                cmbBlocNom.DataSource = null;
                cmbBlocNom.DataSource = _blocsDisponibles;
                cmbBlocNom.DisplayMember = "Nom";
                cmbBlocNom.ValueMember = "BlocId";

                _toutesLesTaches = _tacheService.ObtenirToutesLesTaches();
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        public void ChargerTache(Tache tache, bool modeCreation)
        {
            _suppressEvents = true;
            try
            {
                _tache = tache;
                _modeCreation = modeCreation;

                // Mise à jour des titres
                this.Text = _modeCreation ? "Nouvelle Activité" : $"Détails: {_tache?.TacheId}";
                this.grpTache.Text = this.Text;

                if (_tache == null)
                {
                    _tache = new Tache { HeuresHommeEstimees = 8, Type = TypeActivite.Tache };
                    // La logique de LotId est gérée par le formulaire appelant (TacheForm)
                    if (_blocsDisponibles.Any()) _tache.BlocId = _blocsDisponibles.First().BlocId;
                }

                txtTacheId.Visible = false; // On cache le champ ID en mode création
                

                txtTacheId.Text = _tache.TacheId ?? "";
                txtTacheNom.Text = _tache.TacheNom ?? "";
                numHeuresHomme.Value = _tache.HeuresHommeEstimees;
                chkEstJalon.Checked = _tache.EstJalon;

                cmbBlocNom.SelectedValue = !string.IsNullOrEmpty(_tache.BlocId) ? _tache.BlocId : cmbBlocNom.Items.Count > 0 ? cmbBlocNom.Items[0] : null;

                if (_tache.EstJalon || string.IsNullOrEmpty(_tache.MetierId))
                {
                    cmbMetier.SelectedIndex = 0;
                }
                else
                {
                    var metierItem = cmbMetier.Items.Cast<object>().FirstOrDefault(item => GetMetierId(item) == _tache.MetierId);
                    cmbMetier.SelectedItem = metierItem ?? cmbMetier.Items[0];
                }

                cmbMetier.Enabled = !_tache.EstJalon;
                btnSauvegarder.Enabled = true;
                btnSupprimer.Visible = !modeCreation;
                btnAnnuler.Visible = this.Modal;
            }
            finally
            {
                _suppressEvents = false;
                ChargerListeDependances();
            }
        }

        private void ChargerListeDependances()
        {
            chkListDependances.Items.Clear();
            if (_tache == null) return;

            var dependancesDirectes = ObtenirDependancesDirectes();
            var dependancesHeritees = ObtenirDependancesHeritees();
            var exclusions = ObtenirExclusions();

            var tachesCandidates = _toutesLesTaches
                .Where(t => t.TacheId != _tache.TacheId && t.BlocId == _tache.BlocId)
                .OrderBy(t => t.TacheId)
                .ToList();

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
        }

        private HashSet<string> ObtenirDependancesDirectes()
        {
            if (string.IsNullOrEmpty(_tache?.Dependencies)) return new HashSet<string>();
            return new HashSet<string>(_tache.Dependencies.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)));
        }

        private HashSet<string> ObtenirDependancesHeritees()
        {
            var dependancesHeritees = new HashSet<string>();
            if (_tache == null || _tache.EstJalon || string.IsNullOrEmpty(_tache.MetierId)) return dependancesHeritees;

            var prerequisMetiers = _metierService.GetPrerequisForMetier(_tache.MetierId);
            foreach (var prerequisMetier in prerequisMetiers)
            {
                var tachesPrerequisBloc = _toutesLesTaches
                    .Where(t => t.BlocId == _tache.BlocId && t.MetierId == prerequisMetier)
                    .Select(t => t.TacheId);
                dependancesHeritees.UnionWith(tachesPrerequisBloc);
            }
            return dependancesHeritees;
        }

        private HashSet<string> ObtenirExclusions()
        {
            if (string.IsNullOrEmpty(_tache?.ExclusionsDependances)) return new HashSet<string>();
            return new HashSet<string>(_tache.ExclusionsDependances.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)));
        }

        private string GetMetierId(object item)
        {
            if (item is Metier metier) return metier.MetierId;
            var prop = item?.GetType().GetProperty("MetierId");
            return prop?.GetValue(item)?.ToString() ?? "";
        }

        private void cmbBlocNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null || !(cmbBlocNom.SelectedItem is Bloc selectedBloc)) return;
            _tache.BlocId = selectedBloc.BlocId;
            numBlocCapacite.Value = selectedBloc.CapaciteMaxOuvriers;
            ChargerListeDependances();
        }


        private void cmbMetier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null || _tache.EstJalon) return;
            _tache.MetierId = GetMetierId(cmbMetier.SelectedItem);
            ChargerListeDependances();
        }

        private void chkEstJalon_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null) return;
            bool estJalon = chkEstJalon.Checked;
            cmbMetier.Enabled = !estJalon;
            if (estJalon)
            {
                _tache.Type = TypeActivite.JalonUtilisateur;
                if (string.IsNullOrEmpty(txtTacheNom.Text) || txtTacheNom.Text.StartsWith("Attente"))
                {
                    txtTacheNom.Text = $"Attente {numHeuresHomme.Value} heures";
                }
                cmbMetier.SelectedIndex = 0;
            }
            else
            {
                _tache.Type = TypeActivite.Tache;
            }
        }

        private void numHeuresHomme_ValueChanged(object sender, EventArgs e)
        {
            if (chkEstJalon.Checked && (string.IsNullOrEmpty(txtTacheNom.Text) || txtTacheNom.Text.StartsWith("Attente")))
            {
                txtTacheNom.Text = $"Attente {numHeuresHomme.Value} heures";
            }
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

                TacheSauvegardee?.Invoke(this, EventArgs.Empty);

                if (this.Modal)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Tâche sauvegardée avec succès.", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _modeCreation = false;
                    MettreAJourListesDeroulantes();
                    ChargerTache(_tache, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_modeCreation || _tache == null) return;

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la tâche '{_tache.TacheNom}' ?",
                "Confirmation de suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _tacheService.SupprimerTache(_tache.TacheId);
                    TacheSauvegardee?.Invoke(this, EventArgs.Empty);

                    if (this.Modal)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        ChargerTache(null, true); // Vide le formulaire embarqué
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValiderSaisie()
        {
            if (string.IsNullOrWhiteSpace(txtTacheNom.Text))
            {
                MessageBox.Show("Le nom de la tâche est obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!_tache.EstJalon && cmbMetier.SelectedIndex == 0)
            {
                if (MessageBox.Show("Aucun métier n'est assigné à cette tâche. Continuer ?", "Métier non assigné", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return false;
            }
            return true;
        }

        private void SauvegarderModifications()
        {
            _tache.TacheNom = txtTacheNom.Text.Trim();
            _tache.HeuresHommeEstimees = (int)numHeuresHomme.Value;
            _tache.MetierId = _tache.EstJalon ? "" : GetMetierId(cmbMetier.SelectedItem);

            var dependancesDirectes = new List<string>();
            var exclusions = new List<string>();
            var dependancesHeritees = ObtenirDependancesHeritees();

            for (int i = 0; i < chkListDependances.Items.Count; i++)
            {
                var item = (DependanceItem)chkListDependances.Items[i];
                bool estCochee = chkListDependances.GetItemChecked(i);

                if (item.EstHeritee)
                {
                    if (!estCochee) exclusions.Add(item.TacheId);
                }
                else
                {
                    if (estCochee) dependancesDirectes.Add(item.TacheId);
                }
            }
            _tache.Dependencies = string.Join(",", dependancesDirectes);
            _tache.ExclusionsDependances = string.Join(",", exclusions);
        }

        private void chkListDependances_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var item = (DependanceItem)chkListDependances.Items[e.Index];
            e.DrawBackground();
            Color textColor = item.EstExclue ? Color.Red : (item.EstHeritee ? Color.Blue : Color.Black);
            Font font = item.EstExclue ? new Font(e.Font, System.Drawing.FontStyle.Strikeout) : (item.EstHeritee ? new Font(e.Font, System.Drawing.FontStyle.Italic) : e.Font);
            using (var brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(item.ToString(), font, brush, e.Bounds);
            }
            e.DrawFocusRectangle();
        }
    }
}