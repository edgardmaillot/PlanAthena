using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess; // Assurez-vous que ce using est présent
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class OuvrierForm : Form
    {
        private readonly OuvrierService _ouvrierService;
        private readonly ProjetService _projetService;
        private readonly IIdGeneratorService _idGeneratorService;

        // 🔧 CHANGEMENT MAJEUR : La source de vérité est la liste des DTOs uniques pour l'affichage.
        private List<OuvrierInfo> _ouvriersInfo = new List<OuvrierInfo>();
        private List<Metier> _metiers = new List<Metier>();

        // 🔧 L'ouvrier sélectionné est un DTO. On ira chercher l'objet complet au besoin.
        private OuvrierInfo _ouvrierSelectionne = null;
        private CompetenceOuvrier _competenceSelectionnee = null;

        private bool _isLoading = false;

        public OuvrierForm(OuvrierService ouvrierService, ProjetService projetService, IIdGeneratorService idGeneratorService)
        {
            InitializeComponent();
            _ouvrierService = ouvrierService ?? throw new ArgumentNullException(nameof(ouvrierService));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _idGeneratorService = idGeneratorService ?? throw new ArgumentNullException(nameof(idGeneratorService));
        }

        private void OuvrierForm_Load(object sender, EventArgs e)
        {
            ChargerDonnees();
            MettreAJourUI();
        }

        #region Gestion des Données et de l'Affichage

        private void ChargerDonnees()
        {
            // 🔧 SIMPLIFIÉ : On récupère directement la liste des DTOs uniques.
            _ouvriersInfo = _ouvrierService.ObtenirListeOuvriersUniques();
            _metiers = _projetService.GetAllMetiers().ToList();
        }

        private void MettreAJourUI()
        {
            string idSelectionActuelle = _ouvrierSelectionne?.OuvrierId;

            RemplirListeOuvriers();

            // Tenter de resélectionner l'ouvrier
            if (idSelectionActuelle != null)
            {
                var itemToSelect = listViewOuvriers.Items.Cast<ListViewItem>().FirstOrDefault(i => (i.Tag as OuvrierInfo)?.OuvrierId == idSelectionActuelle);
                if (itemToSelect != null)
                {
                    itemToSelect.Selected = true;
                    itemToSelect.EnsureVisible();
                }
            }

            // Si la sélection a été perdue, on la vide
            if (listViewOuvriers.SelectedItems.Count == 0)
            {
                _ouvrierSelectionne = null;
            }

            MettreAJourDetailsOuvrier();
            MettreAJourListeCompetences();
            MettreAJourEtatBoutons();
        }

        private void RemplirListeOuvriers()
        {
            _isLoading = true;
            listViewOuvriers.Items.Clear();

            var recherche = txtRechercheOuvrier.Text.ToLower();
            var ouvriersFiltres = _ouvriersInfo.Where(o =>
                string.IsNullOrWhiteSpace(recherche) ||
                o.OuvrierId.ToLower().Contains(recherche) ||
                o.NomComplet.ToLower().Contains(recherche)
            ).OrderBy(o => o.Nom).ThenBy(o => o.Prenom);

            foreach (var ouvrierInfo in ouvriersFiltres)
            {
                var item = new ListViewItem(new[] {
                    ouvrierInfo.OuvrierId,
                    ouvrierInfo.NomComplet,
                    ouvrierInfo.CoutJournalier + "€",
                    ouvrierInfo.NombreCompetences.ToString()
                })
                {
                    Tag = ouvrierInfo // 🔧 Le Tag est maintenant un OuvrierInfo
                };
                listViewOuvriers.Items.Add(item);
            }

            lblStatutOuvriers.Text = $"{listViewOuvriers.Items.Count}/{_ouvriersInfo.Count} ouvrier(s)";
            _isLoading = false;
        }

        private void MettreAJourDetailsOuvrier()
        {
            _isLoading = true;
            if (_ouvrierSelectionne != null)
            {
                txtOuvrierId.Text = _ouvrierSelectionne.OuvrierId;
                txtNom.Text = _ouvrierSelectionne.Nom;
                txtPrenom.Text = _ouvrierSelectionne.Prenom;
                numCoutJournalier.Value = _ouvrierSelectionne.CoutJournalier;
                txtOuvrierId.ReadOnly = true; // L'ID n'est plus modifiable après création.
            }
            else
            {
                txtOuvrierId.Clear();
                txtNom.Clear();
                txtPrenom.Clear();
                numCoutJournalier.Value = 0;
            }
            _isLoading = false;
        }

        private void MettreAJourListeCompetences()
        {
            listViewCompetences.Items.Clear();
            lblStatutCompetences.Text = "0 compétence(s)";

            if (_ouvrierSelectionne != null)
            {
                var ouvrierComplet = _ouvrierService.ObtenirOuvrierParId(_ouvrierSelectionne.OuvrierId);
                if (ouvrierComplet != null)
                {
                    foreach (var competence in ouvrierComplet.Competences.OrderBy(c => c.MetierId))
                    {
                        var metier = _projetService.GetMetierById(competence.MetierId);
                        var nomMetier = metier?.Nom ?? "(Métier inconnu)";
                        var estPrincipal = competence.EstMetierPrincipal ? " (Principal)" : "";
                        var item = new ListViewItem(new[] { competence.MetierId, nomMetier + estPrincipal }) { Tag = competence };
                        if (competence.EstMetierPrincipal) { item.Font = new System.Drawing.Font(listViewCompetences.Font, System.Drawing.FontStyle.Bold); }
                        listViewCompetences.Items.Add(item);
                    }
                    lblStatutCompetences.Text = $"{ouvrierComplet.Competences.Count} compétence(s)";
                }
            }
        }

        private void MettreAJourEtatBoutons()
        {
            bool ouvrierEstSelectionne = _ouvrierSelectionne != null;
            bool competenceEstSelectionnee = _competenceSelectionnee != null;

            groupBoxDetailsOuvrier.Enabled = ouvrierEstSelectionne;
            groupBoxCompetences.Enabled = ouvrierEstSelectionne;
            btnModifierOuvrier.Enabled = ouvrierEstSelectionne;
            btnSupprimerOuvrier.Enabled = ouvrierEstSelectionne;
            btnAjouterCompetence.Enabled = ouvrierEstSelectionne;
            btnModifierCompetence.Enabled = competenceEstSelectionnee;
            btnSupprimerCompetence.Enabled = competenceEstSelectionnee;
        }

        #endregion

        #region Événements

        private void listViewOuvriers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            if (listViewOuvriers.SelectedItems.Count > 0)
            {
                _ouvrierSelectionne = listViewOuvriers.SelectedItems[0].Tag as OuvrierInfo;
            }
            else
            {
                _ouvrierSelectionne = null;
            }
            _competenceSelectionnee = null;
            MettreAJourDetailsOuvrier();
            MettreAJourListeCompetences();
            MettreAJourEtatBoutons();
        }

        private void listViewCompetences_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCompetences.SelectedItems.Count > 0)
            {
                _competenceSelectionnee = listViewCompetences.SelectedItems[0].Tag as CompetenceOuvrier;
            }
            else
            {
                _competenceSelectionnee = null;
            }
            MettreAJourEtatBoutons();
        }

        private void txtRechercheOuvrier_TextChanged(object sender, EventArgs e)
        {
            RemplirListeOuvriers();
        }

        private void DetailOuvrier_Changed(object sender, EventArgs e)
        {
            if (_isLoading || _ouvrierSelectionne == null) return;

            // Mettre à jour le DTO en mémoire
            _ouvrierSelectionne.Nom = txtNom.Text.Trim();
            _ouvrierSelectionne.Prenom = txtPrenom.Text.Trim();
            _ouvrierSelectionne.CoutJournalier = (int)numCoutJournalier.Value;

            // Créer un objet Ouvrier temporaire pour la mise à jour
            var ouvrierAModifier = new Ouvrier
            {
                OuvrierId = _ouvrierSelectionne.OuvrierId,
                Nom = _ouvrierSelectionne.Nom,
                Prenom = _ouvrierSelectionne.Prenom,
                CoutJournalier = _ouvrierSelectionne.CoutJournalier
            };

            _ouvrierService.ModifierOuvrier(ouvrierAModifier);

            // Mettre à jour l'affichage dans la liste de gauche
            var itemToUpdate = listViewOuvriers.Items.Cast<ListViewItem>().FirstOrDefault(i => (i.Tag as OuvrierInfo)?.OuvrierId == _ouvrierSelectionne.OuvrierId);
            if (itemToUpdate != null)
            {
                itemToUpdate.SubItems[1].Text = _ouvrierSelectionne.NomComplet;
                itemToUpdate.SubItems[2].Text = _ouvrierSelectionne.CoutJournalier + "€";
            }
        }

        #endregion

        #region Actions CRUD

        private void btnNouveauOuvrier_Click(object sender, EventArgs e)
        {
            var nouvelOuvrier = new Ouvrier
            {
                OuvrierId = _idGeneratorService.GenererProchainOuvrierId(_ouvrierService.ObtenirTousLesOuvriers()),
                Nom = "Ouvrier",
                Prenom = "Nouveau",
                CoutJournalier = 200,
            };

            var premierMetier = _metiers.FirstOrDefault();
            if (premierMetier == null)
            {
                MessageBox.Show("Impossible de créer un ouvrier : aucun métier n'est défini.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            nouvelOuvrier.Competences.Add(new CompetenceOuvrier { MetierId = premierMetier.MetierId, EstMetierPrincipal = true });

            try
            {
                _ouvrierService.AjouterOuvrier(nouvelOuvrier);
                ChargerDonnees();
                MettreAJourUI(); // Ceci va resélectionner le nouvel ouvrier
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAjouterCompetence_Click(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne == null) return;

            var ouvrierComplet = _ouvrierService.ObtenirOuvrierParId(_ouvrierSelectionne.OuvrierId);
            if (ouvrierComplet == null) return;

            var competencesActuelles = ouvrierComplet.Competences.Select(c => c.MetierId).ToList();
            var metiersDisponibles = _metiers.Where(m => !competencesActuelles.Contains(m.MetierId)).ToList();
            if (!metiersDisponibles.Any())
            {
                MessageBox.Show("L'ouvrier maîtrise déjà tous les métiers.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new CompetenceDialog(metiersDisponibles, null);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _ouvrierService.AjouterCompetence(_ouvrierSelectionne.OuvrierId, dialog.MetierSelectionne.MetierId);
                    // Mettre à jour le nombre de compétences dans notre DTO local
                    _ouvrierSelectionne.NombreCompetences++;
                    MettreAJourUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSupprimerOuvrier_Click(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne == null) return;

            var result = MessageBox.Show($"Supprimer l'ouvrier '{_ouvrierSelectionne.NomComplet}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _ouvrierService.SupprimerOuvrier(_ouvrierSelectionne.OuvrierId);
                    ChargerDonnees();
                    MettreAJourUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // La logique des autres boutons (modifier, supprimer compétence, etc.) est similaire et peut être adaptée sur ce modèle.
        // Je laisse les versions simplifiées pour le moment.
        private void btnModifierCompetence_Click(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne == null || _competenceSelectionnee == null) return;

            var result = MessageBox.Show(
                $"Définir '{_projetService.GetMetierById(_competenceSelectionnee.MetierId)?.Nom}' comme métier principal ?",
                "Définir comme principal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _ouvrierService.DefinirMetierPrincipal(_ouvrierSelectionne.OuvrierId, _competenceSelectionnee.MetierId);
                MettreAJourListeCompetences();
            }
        }

        private void btnSupprimerCompetence_Click(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne == null || _competenceSelectionnee == null) return;

            var metier = _projetService.GetMetierById(_competenceSelectionnee.MetierId);
            var result = MessageBox.Show($"Supprimer la compétence '{metier?.Nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _ouvrierService.SupprimerCompetence(_ouvrierSelectionne.OuvrierId, _competenceSelectionnee.MetierId);
                    _ouvrierSelectionne.NombreCompetences--;
                    _competenceSelectionnee = null;
                    MettreAJourUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnModifierOuvrier_Click(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne != null)
            {
                txtNom.Focus();
            }
        }

        #endregion

        #region Import/Export/Fermer
        private void btnImporter_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                Title = "Importer les ouvriers depuis un fichier CSV"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Voulez-vous remplacer tous les ouvriers existants ?\n\n" +
                        "• Oui : Remplace tous les ouvriers actuels\n" +
                        "• Non : Ajoute aux ouvriers existants\n" +
                        "• Annuler : Annule l'import",
                        "Mode d'import",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel) return;

                    var nombreImporte = _ouvrierService.ImporterDepuisCsv(ofd.FileName, result == DialogResult.Yes);
                    ChargerDonnees(); // Recharger depuis le service

                    _ouvrierSelectionne = null;
                    _competenceSelectionnee = null;
                    //_enModification = false;
                    RafraichirAffichage();

                    MessageBox.Show(
                        $"Import terminé avec succès !\n\n" +
                        $"• {nombreImporte} lignes importées\n" +
                        $"• {_ouvriersInfo.GroupBy(o => o.OuvrierId).Count()} ouvriers total",
                        "Import réussi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'import :\n{ex.Message}",
                        "Erreur d'import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportExcelSAP_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Tous les fichiers (*.*)|*.*",
                Title = "Importer les ouvriers depuis un fichier Excel SAP"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // TODO: Implémenter l'import Excel SAP spécifique
                    var nombreImporte = _ouvrierService.ImporterDepuisExcelSap(ofd.FileName);

                    if (nombreImporte > 0)
                    {
                        ChargerDonnees();
                        _ouvrierSelectionne = null;
                        _competenceSelectionnee = null;
                        //_enModification = false;
                        RafraichirAffichage();

                        MessageBox.Show(
                            $"Import Excel SAP terminé avec succès !\n\n" +
                            $"• {nombreImporte} ouvriers importés",
                            "Import réussi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "L'import Excel SAP n'est pas encore implémenté.\n" +
                            "Utilisez l'import CSV pour le moment.",
                            "Fonctionnalité en développement",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'import Excel SAP :\n{ex.Message}",
                        "Erreur d'import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!_ouvriersInfo.Any())
            {
                MessageBox.Show("Aucun ouvrier à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                Title = "Exporter les ouvriers vers un fichier CSV",
                FileName = $"ouvriers_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _ouvrierService.ExporterVersCsv(sfd.FileName);

                    var nombreOuvriers = _ouvriersInfo.GroupBy(o => o.OuvrierId).Count();
                    MessageBox.Show(
                        $"Export terminé avec succès !\n\n" +
                        $"• {_ouvriersInfo.Count} lignes exportées\n" +
                        $"• {nombreOuvriers} ouvriers\n" +
                        $"• Fichier : {Path.GetFileName(sfd.FileName)}",
                        "Export réussi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'export :\n{ex.Message}",
                        "Erreur d'export",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void RafraichirAffichage()
        {
            RafraichirListeOuvriers();
            RafraichirStatutOuvriers();
            RafraichirDetailsOuvrier();
            RafraichirCompetences();
        }

        private void RafraichirListeOuvriers()
        {
            listViewOuvriers.Items.Clear();

            var ouvriersUniques = _ouvriersInfo.GroupBy(o => o.OuvrierId)
                .Select(g => new OuvrierInfo
                {
                    OuvrierId = g.Key,
                    Nom = g.First().Nom,
                    Prenom = g.First().Prenom,
                    CoutJournalier = g.First().CoutJournalier,
                    NombreCompetences = g.Count()
                }).AsEnumerable();

            // Filtrage par recherche
            if (!string.IsNullOrWhiteSpace(txtRechercheOuvrier.Text))
            {
                var recherche = txtRechercheOuvrier.Text.ToLower();
                ouvriersUniques = ouvriersUniques.Where(o =>
                    o.OuvrierId.ToLower().Contains(recherche) ||
                    o.NomComplet.ToLower().Contains(recherche));
            }

            foreach (var ouvrier in ouvriersUniques.OrderBy(o => o.Nom).ThenBy(o => o.Prenom))
            {
                var item = new ListViewItem(new[] {
                    ouvrier.OuvrierId,
                    ouvrier.NomComplet,
                    ouvrier.CoutJournalier.ToString() + "€",
                    ouvrier.NombreCompetences.ToString()
                })
                {
                    Tag = ouvrier
                };
                listViewOuvriers.Items.Add(item);
            }
        }

        private void RafraichirStatutOuvriers()
        {
            var totalOuvriers = _ouvriersInfo.GroupBy(o => o.OuvrierId).Count();
            var afficheOuvriers = listViewOuvriers.Items.Count;

            if (afficheOuvriers == totalOuvriers)
                lblStatutOuvriers.Text = $"{totalOuvriers} ouvrier(s)";
            else
                lblStatutOuvriers.Text = $"{afficheOuvriers}/{totalOuvriers} ouvrier(s) affiché(s)";
        }

        private void RafraichirDetailsOuvrier()
        {
            if (_ouvrierSelectionne == null)
            {
                groupBoxDetailsOuvrier.Enabled = false;
                groupBoxCompetences.Enabled = false;
                txtOuvrierId.Clear();
                txtNom.Clear();
                txtPrenom.Clear();
                numCoutJournalier.Value = 0;
                return;
            }

            groupBoxDetailsOuvrier.Enabled = true;
            groupBoxCompetences.Enabled = true;
            btnAjouterCompetence.Enabled = true;

            txtOuvrierId.Text = _ouvrierSelectionne.OuvrierId;
            txtNom.Text = _ouvrierSelectionne.Nom;
            txtPrenom.Text = _ouvrierSelectionne.Prenom;
            numCoutJournalier.Value = _ouvrierSelectionne.CoutJournalier;

            // Gestion du mode modification
            //txtOuvrierId.ReadOnly = _enModification;
        }

        private void RafraichirCompetences()
        {
            listViewCompetences.Items.Clear();
            lblStatutCompetences.Text = "0 compétence(s)";

            if (_ouvrierSelectionne == null) return;

            // 🔧 CORRECTION : On demande au service l'objet Ouvrier COMPLET pour l'ID sélectionné.
            var ouvrierComplet = _ouvrierService.ObtenirOuvrierParId(_ouvrierSelectionne.OuvrierId);

            // On vérifie que l'ouvrier a bien été trouvé.
            if (ouvrierComplet == null) return;

            // On itère sur la VRAIE liste de compétences de cet ouvrier.
            foreach (var competence in ouvrierComplet.Competences.OrderBy(c => c.MetierId))
            {
                var metier = _projetService.GetMetierById(competence.MetierId);
                var nomMetier = metier?.Nom ?? "(Métier inconnu)";
                var estPrincipal = competence.EstMetierPrincipal ? " (Principal)" : "";

                var item = new ListViewItem(new[] {
            competence.MetierId,
            nomMetier + estPrincipal
            // Les colonnes NiveauExpertise et PerformancePct n'existent plus dans le modèle V0.4.2
            // Il faudra adapter le Designer pour supprimer ces colonnes de listViewCompetences.
        })
                {
                    Tag = competence // Le tag est maintenant l'objet CompetenceOuvrier
                };

                if (competence.EstMetierPrincipal)
                {
                    item.Font = new System.Drawing.Font(listViewCompetences.Font, System.Drawing.FontStyle.Bold);
                }

                listViewCompetences.Items.Add(item);
            }

            lblStatutCompetences.Text = $"{ouvrierComplet.Competences.Count} compétence(s)";
        }
        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        #endregion
    }
}