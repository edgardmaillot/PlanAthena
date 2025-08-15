// Fichier: PlanAthena/Forms/TacheDetailForm.cs
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class TacheDetailForm : Form
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly DependanceBuilder _dependanceBuilder;
        private Tache _tache;
        private bool _modeCreation;
        private bool _suppressEvents = false;
        private List<Bloc> _blocsDisponibles;
        private ToolTip _tooltip;

        public event EventHandler TacheSauvegardee;

        public TacheDetailForm(ProjetService projetService, RessourceService ressourceService, DependanceBuilder dependanceBuilder)
        {
            InitializeComponent();
            chkListDependances.DrawMode = DrawMode.OwnerDrawFixed;
            chkListDependances.ItemHeight = 20;
            chkListDependances.DrawItem += chkListDependances_DrawItem;
            _tooltip = new ToolTip { InitialDelay = 500, ReshowDelay = 100 };
            chkListDependances.MouseMove += chkListDependances_MouseMove;
            _projetService = projetService;
            _ressourceService = ressourceService;
            _dependanceBuilder = dependanceBuilder;
        }

        public void MettreAJourListesDeroulantes(string lotId)
        {
            _suppressEvents = true;
            try
            {
                var metiersPourListe = new List<Metier> { new Metier { MetierId = "", Nom = "(Aucun métier assigné)" } };
                metiersPourListe.AddRange(_ressourceService.GetAllMetiers().OrderBy(m => m.Nom));
                cmbMetier.DataSource = null;
                cmbMetier.DataSource = metiersPourListe;
                cmbMetier.DisplayMember = "Nom";
                cmbMetier.ValueMember = "MetierId";
                _blocsDisponibles = _projetService.ObtenirBlocsParLot(lotId);
                cmbBlocNom.DataSource = null;
                cmbBlocNom.DataSource = _blocsDisponibles;
                cmbBlocNom.DisplayMember = "Nom";
                cmbBlocNom.ValueMember = "BlocId";
            }
            finally { _suppressEvents = false; }
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
                    else _tache.BlocId = null;
                }
                txtTacheId.Visible = false;
                txtTacheId.Text = _tache.TacheId ?? "";
                txtTacheNom.Text = _tache.TacheNom ?? "";
                numHeuresHomme.Value = _tache.HeuresHommeEstimees;
                chkEstJalon.Checked = _tache.EstJalon;
                if (cmbBlocNom.Items.Count > 0)
                {
                    cmbBlocNom.SelectedValue = _blocsDisponibles.Any(b => b.BlocId == _tache.BlocId) ? _tache.BlocId : _blocsDisponibles.First().BlocId;
                }
                else { cmbBlocNom.SelectedIndex = -1; }
                cmbMetier.SelectedValue = !string.IsNullOrEmpty(_tache.MetierId) ? _tache.MetierId : "";
                cmbMetier.Enabled = !_tache.EstJalon;
                btnSauvegarder.Enabled = true;
                btnSupprimer.Visible = !modeCreation;
                btnAnnuler.Visible = this.Modal;
            }
            finally
            {
                _suppressEvents = false;
                if (_modeCreation && cmbBlocNom.SelectedItem != null)
                {
                    if (_tache.BlocId == null && _blocsDisponibles.Any())
                    {
                        _tache.BlocId = _blocsDisponibles.First().BlocId;
                    }
                    cmbBlocNom_SelectedIndexChanged(cmbBlocNom, EventArgs.Empty);
                }
                else if (!_modeCreation) { ChargerListeDependances(); }
            }
        }

        private void ChargerListeDependances()
        {
            try
            {
                chkListDependances.DrawItem -= chkListDependances_DrawItem;
                chkListDependances.DrawItem += chkListDependances_DrawItem;
                chkListDependances.Items.Clear();
                if (_tache == null) return;
                if (string.IsNullOrEmpty(_tache.LotId)) throw new InvalidOperationException($"Tâche '{_tache.TacheNom}' (ID: {_tache.TacheId}) non assignée à un lot.");
                var lotDeLaTache = _projetService.ObtenirLotParId(_tache.LotId);
                if (lotDeLaTache == null) throw new InvalidOperationException($"Lot '{_tache.LotId}' associé à la tâche '{_tache.TacheNom}' non trouvé.");
                if (lotDeLaTache.Phases == ChantierPhase.None) throw new InvalidOperationException($"Lot '{lotDeLaTache.Nom}' n'a pas de phase définie.");
                ChantierPhase phaseContexte = lotDeLaTache.Phases;
                if (string.IsNullOrEmpty(_tache.BlocId)) return;
                var tachesDuMemeBloc = _projetService.ObtenirTachesParBloc(_tache.BlocId);
                var etatsDependances = _dependanceBuilder.ObtenirDependancesPourTache(_tache, tachesDuMemeBloc, phaseContexte);
                foreach (var etat in etatsDependances)
                {
                    bool estCochee = etat.Etat == EtatDependance.Stricte || etat.Etat == EtatDependance.Suggeree;
                    chkListDependances.Items.Add(etat, estCochee);
                }
                chkListDependances.Invalidate();
            }
            catch (Exception ex) { MessageBox.Show($"Erreur chargement dépendances : {ex.Message}", "Erreur Critique", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void cmbBlocNom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _tache == null || !(cmbBlocNom.SelectedItem is Bloc selectedBloc)) return;
            _tache.BlocId = selectedBloc.BlocId;
            numBlocCapacite.Value = selectedBloc.CapaciteMaxOuvriers;
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
                cmbMetier.SelectedValue = "";
            }
            else { _tache.Type = TypeActivite.Tache; }
            ChargerListeDependances();
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
            if (_suppressEvents || _tache == null || _tache.EstJalon || !(cmbMetier.SelectedItem is Metier selectedMetier)) return;
            _tache.MetierId = selectedMetier.MetierId;
            ChargerListeDependances();
        }

        private void btnSauvegarder_Click(object sender, EventArgs e)
        {
            if (!ValiderSaisie()) return;
            SauvegarderModifications();
            try
            {
                var tacheExistante = _projetService.ObtenirTacheParId(_tache.TacheId);
                if (tacheExistante != null)
                {
                    _projetService.ModifierTache(_tache);
                }
                else
                {
                    var lotId = ((Bloc)cmbBlocNom.SelectedItem).LotId;
                    var blocId = ((Bloc)cmbBlocNom.SelectedItem).BlocId;
                    var nouvelleTacheCreeeParService = _projetService.CreerTache(lotId, blocId);
                    nouvelleTacheCreeeParService.TacheNom = _tache.TacheNom;
                    nouvelleTacheCreeeParService.HeuresHommeEstimees = _tache.HeuresHommeEstimees;
                    nouvelleTacheCreeeParService.MetierId = _tache.MetierId;
                    nouvelleTacheCreeeParService.Dependencies = _tache.Dependencies;
                    nouvelleTacheCreeeParService.ExclusionsDependances = _tache.ExclusionsDependances;
                    nouvelleTacheCreeeParService.Type = _tache.Type;
                    _tache = nouvelleTacheCreeeParService;
                }
                TacheSauvegardee?.Invoke(this, EventArgs.Empty);
                if (this.Modal)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    //MessageBox.Show("Tâche sauvegardée.", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _modeCreation = false;
                    ChargerTache(_tache, false);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Erreur sauvegarde:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_modeCreation || _tache == null) return;
            var result = MessageBox.Show($"Supprimer la tâche '{_tache.TacheNom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _projetService.SupprimerTache(_tache.TacheId);
                    TacheSauvegardee?.Invoke(this, EventArgs.Empty);
                    if (this.Modal) { this.DialogResult = DialogResult.OK; this.Close(); }
                    else { ChargerTache(null, true); }
                }
                catch (Exception ex) { MessageBox.Show($"Erreur suppression:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
            if (cmbBlocNom.SelectedItem == null || string.IsNullOrEmpty(((Bloc)cmbBlocNom.SelectedItem).BlocId))
            {
                MessageBox.Show("Veuillez assigner la tâche à un bloc.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false;
            }
            if (!_tache.EstJalon && cmbMetier.SelectedValue is string metierId && string.IsNullOrEmpty(metierId))
            {
                if (MessageBox.Show("Aucun métier n'est assigné. Continuer ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return false;
            }
            return true;
        }

        private void SauvegarderModifications()
        {
            _tache.TacheNom = txtTacheNom.Text.Trim();
            _tache.HeuresHommeEstimees = (int)numHeuresHomme.Value;
            _tache.Type = chkEstJalon.Checked ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;
            if (cmbMetier.SelectedItem is Metier selectedMetier) _tache.MetierId = _tache.EstJalon ? "" : selectedMetier.MetierId;
            if (cmbBlocNom.SelectedItem is Bloc selectedBloc) _tache.BlocId = selectedBloc.BlocId;

            var dependancesStricts = new List<string>();
            var exclusions = new List<string>();
            for (int i = 0; i < chkListDependances.Items.Count; i++)
            {
                var item = (DependanceAffichage)chkListDependances.Items[i];
                bool estCochee = chkListDependances.GetItemChecked(i);
                var tacheIdPredecesseur = item.TachePredecesseur.TacheId;
                if (!item.EstHeritee && estCochee) dependancesStricts.Add(tacheIdPredecesseur);
                else if (item.EstHeritee && !estCochee) exclusions.Add(tacheIdPredecesseur);
                else if (item.EstHeritee && estCochee) dependancesStricts.Add(tacheIdPredecesseur);
            }
            _tache.Dependencies = string.Join(",", dependancesStricts.Distinct());
            _tache.ExclusionsDependances = string.Join(",", exclusions.Distinct());
        }

        private void chkListDependances_MouseMove(object sender, MouseEventArgs e)
        {
            var index = chkListDependances.IndexFromPoint(e.Location);
            if (index >= 0 && index < chkListDependances.Items.Count)
            {
                var item = (DependanceAffichage)chkListDependances.Items[index];
                var tache = item.TachePredecesseur;
                string tooltipText = $"ID: {tache.TacheId}\nNom: {tache.TacheNom}\nMétier: {(string.IsNullOrEmpty(tache.MetierId) ? "Aucun" : tache.MetierId)}\nDurée: {tache.HeuresHommeEstimees}h\nÉtat: {GetEtatDescription(item.Etat)}";
                _tooltip.SetToolTip(chkListDependances, tooltipText);
            }
            else { _tooltip.SetToolTip(chkListDependances, ""); }
        }

        private string GetEtatDescription(EtatDependance etat)
        {
            return etat switch
            {
                EtatDependance.Suggeree => "Suggérée par les règles métier",
                EtatDependance.Exclue => "Exclue par l'utilisateur",
                EtatDependance.Stricte => "Définie manuellement",
                _ => "Aucune relation particulière",
            };
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
                case EtatDependance.Exclue: textColor = Color.Red; font = new Font(e.Font, FontStyle.Strikeout); break;
                case EtatDependance.Suggeree: textColor = Color.Blue; font = new Font(e.Font, FontStyle.Italic); break;
                case EtatDependance.Stricte: textColor = Color.Black; font = new Font(e.Font, FontStyle.Bold); break;
                default: textColor = Color.Black; break;
            }
            using (var brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(item.ToString(), font, brush, e.Bounds.Left, e.Bounds.Top);
            }
            e.DrawFocusRectangle();
        }
    }
}