using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class TacheDetailForm : System.Windows.Forms.Form
    {
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly LotService _lotService;
        private readonly BlocService _blocService;
        private readonly DependanceBuilder _dependanceBuilder;
        private Tache _tache;
        private bool _modeCreation;
        private bool _suppressEvents = false;
        private List<Bloc> _blocsDisponibles;

        public event EventHandler TacheSauvegardee;

        public TacheDetailForm(TacheService tacheService, MetierService metierService, LotService lotService, BlocService blocService, DependanceBuilder dependanceBuilder)
        {
            InitializeComponent();
            _tacheService = tacheService;
            _metierService = metierService;
            _lotService = lotService;
            _blocService = blocService;
            _dependanceBuilder = dependanceBuilder;

            MettreAJourListesDeroulantes();
            ChargerTache(null, true);
        }

        public void MettreAJourListesDeroulantes()
        {
            _suppressEvents = true;
            try
            {
                var metiersPourListe = new List<Metier> { new Metier { MetierId = "", Nom = "(Aucun métier assigné)" } };
                metiersPourListe.AddRange(_metierService.GetAllMetiers().OrderBy(m => m.Nom));
                cmbMetier.DataSource = null;
                cmbMetier.DataSource = metiersPourListe;
                cmbMetier.DisplayMember = "Nom";
                cmbMetier.ValueMember = "MetierId";

                _blocsDisponibles = _blocService.ObtenirTousLesBlocs();
                cmbBlocNom.DataSource = null;
                cmbBlocNom.DataSource = _blocsDisponibles;
                cmbBlocNom.DisplayMember = "Nom";
                cmbBlocNom.ValueMember = "BlocId";
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

                this.Text = _modeCreation ? "Nouvelle Activité" : $"Détails: {_tache?.TacheId}";
                this.grpTache.Text = this.Text;

                if (_tache == null)
                {
                    _tache = new Tache { HeuresHommeEstimees = 8, Type = TypeActivite.Tache };
                    if (_blocsDisponibles.Any()) _tache.BlocId = _blocsDisponibles.First().BlocId;
                }

                txtTacheId.Visible = false; // On garde l'ID caché

                txtTacheId.Text = _tache.TacheId ?? "";
                txtTacheNom.Text = _tache.TacheNom ?? "";
                numHeuresHomme.Value = _tache.HeuresHommeEstimees;
                chkEstJalon.Checked = _tache.EstJalon;

                if (cmbBlocNom.Items.Count > 0)
                {
                    cmbBlocNom.SelectedValue = _blocsDisponibles.Any(b => b.BlocId == _tache.BlocId) ? _tache.BlocId : _blocsDisponibles.First().BlocId;
                }

                cmbMetier.SelectedValue = !string.IsNullOrEmpty(_tache.MetierId) ? _tache.MetierId : "";
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

        /// <summary>
        /// Orchestre l'affichage de la liste des dépendances.
        /// 
        /// ARCHITECTURE SIMPLIFIÉE : Cette méthode délègue toute la logique complexe au DependanceBuilder
        /// et se contente d'afficher les résultats avec le bon formatage visuel.
        /// 
        /// RÈGLE APPLIQUÉE : Pré-filtre les tâches du même bloc conformément à la Règle 1 (Isolation).
        /// </summary>
        private void ChargerListeDependances()
        {
            chkListDependances.Items.Clear();
            if (_tache == null || string.IsNullOrEmpty(_tache.BlocId)) return;

            // Pré-filtrage : tâches du même bloc uniquement (Règle 1)
            var tachesDuMemeBloc = _tacheService.ObtenirTachesParBloc(_tache.BlocId);

            // Délégation complète de la logique au DependanceBuilder
            var etatsDependances = _dependanceBuilder.PeuplerListeDependance(_tache, tachesDuMemeBloc);

            foreach (var etat in etatsDependances)
            {
                // Logique de cochage selon l'état
                bool estCochee = etat.Etat == EtatDependance.Stricte || etat.Etat == EtatDependance.Suggeree;
                chkListDependances.Items.Add(etat, estCochee);
            }
        }

        private void cmbBlocNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null || !(cmbBlocNom.SelectedItem is Bloc selectedBloc)) return;
            _tache.BlocId = selectedBloc.BlocId;
            numBlocCapacite.Value = selectedBloc.CapaciteMaxOuvriers;

            // Recharger la liste des dépendances car le contexte a changé.
            ChargerListeDependances();
        }

        private void chkEstJalon_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null) return;

            bool estJalon = chkEstJalon.Checked;
            cmbMetier.Enabled = !estJalon;

            // Correction: La propriété 'Type' est la source de vérité pour définir un jalon.
            // L'affectation directe à 'EstJalon' a été supprimée.
            if (estJalon)
            {
                _tache.Type = TypeActivite.JalonUtilisateur;
                if (string.IsNullOrEmpty(txtTacheNom.Text) || txtTacheNom.Text.StartsWith("Attente"))
                {
                    txtTacheNom.Text = $"Attente {numHeuresHomme.Value} heures";
                }
                cmbMetier.SelectedValue = "";
            }
            else
            {
                _tache.Type = TypeActivite.Tache;
            }
            ChargerListeDependances(); // Recharger car les suggestions peuvent disparaître/apparaître.
        }

        private void numHeuresHomme_ValueChanged(object sender, EventArgs e)
        {
            if (chkEstJalon.Checked && (string.IsNullOrEmpty(txtTacheNom.Text) || txtTacheNom.Text.StartsWith("Attente")))
            {
                txtTacheNom.Text = $"Attente {numHeuresHomme.Value} heures";
            }
        }

        private void cmbMetier_SelectedIndexChanged(object sender, EventArgs e)
        {
            // On s'assure que l'événement n'est pas supprimé, que la tâche existe, qu'elle n'est pas un jalon,
            // et que l'élément sélectionné est bien un objet Metier.
            if (_suppressEvents || _tache == null || _tache.EstJalon || !(cmbMetier.SelectedItem is Metier selectedMetier)) return;

            // On assigne directement l'ID du métier, ce qui est plus sûr.
            _tache.MetierId = selectedMetier.MetierId;

            // On recharge la liste des dépendances car les suggestions métier ont changé.
            ChargerListeDependances();
        }

        private void btnSauvegarder_Click(object sender, EventArgs e)
        {
            if (!ValiderSaisie()) return;
            SauvegarderModifications();
            try
            {
                if (_modeCreation) _tacheService.AjouterTache(_tache);
                else _tacheService.ModifierTache(_tache);

                // Étape de validation globale, cruciale après chaque modification.
                var toutesLesTaches = _tacheService.ObtenirToutesLesTaches();
                var validationResult = _dependanceBuilder.ConstruireDependancesLogiques(toutesLesTaches);

                if (!validationResult.EstValide)
                {
                    MessageBox.Show(validationResult.MessageErreur, "Validation échouée", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Idéalement, on pourrait annuler la transaction ici, mais pour l'instant on prévient l'utilisateur.
                }

                TacheSauvegardee?.Invoke(this, EventArgs.Empty);
                if (this.Modal) { this.DialogResult = DialogResult.OK; this.Close(); }
                else
                {
                    MessageBox.Show("Tâche sauvegardée avec succès.", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _modeCreation = false;
                    ChargerTache(_tache, false); // Recharger la tâche pour voir le résultat.
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
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la tâche '{_tache.TacheNom}' ?", "Confirmation de suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _tacheService.SupprimerTache(_tache.TacheId);
                    TacheSauvegardee?.Invoke(this, EventArgs.Empty);
                    if (this.Modal) { this.DialogResult = DialogResult.OK; this.Close(); }
                    else { ChargerTache(null, true); }
                }
                catch (Exception ex) { MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
                MessageBox.Show("Le nom de la tâche est obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false;
            }
            if (!_tache.EstJalon && cmbMetier.SelectedValue is string metierId && string.IsNullOrEmpty(metierId))
            {
                if (MessageBox.Show("Aucun métier n'est assigné à cette tâche. Continuer ?", "Métier non assigné", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return false;
            }
            if (cmbBlocNom.SelectedItem == null)
            {
                MessageBox.Show("Veuillez assigner la tâche à un bloc.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false;
            }
            return true;
        }

        /// <summary>
        /// Met à jour les propriétés de l'objet Tache en lisant l'état de l'UI.
        /// C'est ici que le choix de l'utilisateur est traduit en chaînes de caractères `Dependencies` et `ExclusionsDependances`.
        /// </summary>
        private void SauvegarderModifications()
        {
            _tache.TacheNom = txtTacheNom.Text.Trim();
            _tache.HeuresHommeEstimees = (int)numHeuresHomme.Value;

            // Correction: L'état du jalon est défini via la propriété 'Type', qui est la source de vérité.
            _tache.Type = chkEstJalon.Checked ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;

            // La logique suivante fonctionne maintenant correctement car la propriété 'EstJalon' (lecture seule)
            // retournera la bonne valeur calculée à partir du 'Type' qui vient d'être défini.
            if (cmbMetier.SelectedItem is Metier selectedMetier)
            {
                _tache.MetierId = _tache.EstJalon ? "" : selectedMetier.MetierId;
            }
            if (cmbBlocNom.SelectedItem is Bloc selectedBloc)
            {
                _tache.BlocId = selectedBloc.BlocId;
            }

            var dependancesStricts = new List<string>();
            var exclusions = new List<string>();

            for (int i = 0; i < chkListDependances.Items.Count; i++)
            {
                var item = (DependanceAffichage)chkListDependances.Items[i];
                bool estCochee = chkListDependances.GetItemChecked(i);
                var tacheIdPredecesseur = item.TachePredecesseur.TacheId;

                // Une dépendance est stricte si elle n'est pas héritée du métier ET qu'elle est cochée.
                if (!item.EstHeritee && estCochee)
                {
                    dependancesStricts.Add(tacheIdPredecesseur);
                }
                // Une dépendance est une exclusion si elle est héritée du métier ET qu'elle est décochée.
                else if (item.EstHeritee && !estCochee)
                {
                    exclusions.Add(tacheIdPredecesseur);
                }
                // Si la dépendance est héritée ET cochée, c'est une dépendance implicite.
                // On l'ajoute à la liste des dépendances strictes pour la matérialiser.
                else if (item.EstHeritee && estCochee)
                {
                    dependancesStricts.Add(tacheIdPredecesseur);
                }
            }
            _tache.Dependencies = string.Join(",", dependancesStricts.Distinct());
            _tache.ExclusionsDependances = string.Join(",", exclusions.Distinct());
        }

        private void chkListDependances_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var item = (DependanceAffichage)chkListDependances.Items[e.Index];
            e.DrawBackground();

            Color textColor;
            Font font = e.Font;

            switch (item.Etat)
            {
                case EtatDependance.Exclue:
                    textColor = Color.Gray;
                    font = new Font(e.Font, FontStyle.Strikeout);
                    break;
                case EtatDependance.Suggeree:
                    textColor = Color.Blue;
                    font = new Font(e.Font, FontStyle.Italic);
                    break;
                case EtatDependance.Stricte:
                    textColor = Color.Black;
                    font = new Font(e.Font, FontStyle.Bold);
                    break;
                default:
                    textColor = Color.Black;
                    break;
            }

            using (var brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(item.ToString(), font, brush, e.Bounds.Left, e.Bounds.Top);
            }

            e.DrawFocusRectangle();
        }
    }
}