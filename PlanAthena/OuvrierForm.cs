using PlanAthena.CsvModels;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena
{
    public partial class OuvrierForm : Form
    {
        private readonly OuvrierService _ouvrierService;
        private readonly MetierService _metierService;
        private readonly CsvDataService _csvDataService;
        private readonly ExcelReader _excelReader;

        private List<OuvrierCsvRecord> _ouvriers = new List<OuvrierCsvRecord>();
        private List<MetierCsvRecord> _metiers = new List<MetierCsvRecord>();
        private OuvrierInfo _ouvrierSelectionne = null;
        private OuvrierCsvRecord _competenceSelectionnee = null;
        private bool _enModification = false;

        public OuvrierForm(OuvrierService ouvrierService, MetierService metierService)
        {
            InitializeComponent();
            _ouvrierService = ouvrierService ?? throw new ArgumentNullException(nameof(ouvrierService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _csvDataService = new CsvDataService();
            _excelReader = new ExcelReader();
        }

        private void OuvrierForm_Load(object sender, EventArgs e)
        {
            ChargerDonnees();
            RafraichirAffichage();
            InitialiserInterface();
        }

        private void InitialiserInterface()
        {
            // Désactiver les contrôles au démarrage
            groupBoxDetailsOuvrier.Enabled = false;
            groupBoxCompetences.Enabled = false;
            btnModifierOuvrier.Enabled = false;
            btnSupprimerOuvrier.Enabled = false;
            btnAjouterCompetence.Enabled = false;
            btnModifierCompetence.Enabled = false;
            btnSupprimerCompetence.Enabled = false;
        }

        #region Gestion des données

        private void ChargerDonnees()
        {
            _ouvriers = _ouvrierService.ObtenirTousLesOuvriers();
            _metiers = _metierService.GetAllMetiers().ToList();
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

            var ouvriersUniques = _ouvriers.GroupBy(o => o.OuvrierId)
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
            var totalOuvriers = _ouvriers.GroupBy(o => o.OuvrierId).Count();
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
            txtOuvrierId.ReadOnly = _enModification;
        }

        private void RafraichirCompetences()
        {
            listViewCompetences.Items.Clear();
            lblStatutCompetences.Text = "0 compétence(s)";

            if (_ouvrierSelectionne == null) return;

            var competencesOuvrier = _ouvriers.Where(o => o.OuvrierId == _ouvrierSelectionne.OuvrierId).ToList();

            foreach (var competence in competencesOuvrier.OrderBy(c => c.MetierId))
            {
                var metier = _metiers.FirstOrDefault(m => m.MetierId == competence.MetierId);
                var nomMetier = metier?.Nom ?? "(Métier inconnu)";

                var item = new ListViewItem(new[] {
                    competence.MetierId,
                    nomMetier,
                    competence.NiveauExpertise.ToString(),
                    (competence.PerformancePct?.ToString() ?? "100") + "%"
                })
                {
                    Tag = competence
                };
                listViewCompetences.Items.Add(item);
            }

            lblStatutCompetences.Text = $"{competencesOuvrier.Count} compétence(s)";
        }

        #endregion

        #region Événements interface

        private void listViewOuvriers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewOuvriers.SelectedItems.Count > 0)
            {
                _ouvrierSelectionne = listViewOuvriers.SelectedItems[0].Tag as OuvrierInfo;
                btnModifierOuvrier.Enabled = true;
                btnSupprimerOuvrier.Enabled = true;
            }
            else
            {
                _ouvrierSelectionne = null;
                btnModifierOuvrier.Enabled = false;
                btnSupprimerOuvrier.Enabled = false;
            }

            _competenceSelectionnee = null;
            btnModifierCompetence.Enabled = false;
            btnSupprimerCompetence.Enabled = false;

            RafraichirDetailsOuvrier();
            RafraichirCompetences();
        }

        private void listViewCompetences_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCompetences.SelectedItems.Count > 0)
            {
                _competenceSelectionnee = listViewCompetences.SelectedItems[0].Tag as OuvrierCsvRecord;
                btnModifierCompetence.Enabled = true;
                btnSupprimerCompetence.Enabled = true;
            }
            else
            {
                _competenceSelectionnee = null;
                btnModifierCompetence.Enabled = false;
                btnSupprimerCompetence.Enabled = false;
            }
        }

        private void txtRechercheOuvrier_TextChanged(object sender, EventArgs e)
        {
            RafraichirListeOuvriers();
            RafraichirStatutOuvriers();
        }

        private void txtOuvrierId_TextChanged(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne != null && !_enModification)
            {
                _ouvrierSelectionne.OuvrierId = txtOuvrierId.Text;
                MettreAJourTousLesEnregistrements();
            }
        }

        private void txtNom_TextChanged(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne != null)
            {
                _ouvrierSelectionne.Nom = txtNom.Text;
                MettreAJourTousLesEnregistrements();
            }
        }

        private void txtPrenom_TextChanged(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne != null)
            {
                _ouvrierSelectionne.Prenom = txtPrenom.Text;
                MettreAJourTousLesEnregistrements();
            }
        }

        private void numCoutJournalier_ValueChanged(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne != null)
            {
                _ouvrierSelectionne.CoutJournalier = (int)numCoutJournalier.Value;
                MettreAJourTousLesEnregistrements();
            }
        }

        private void MettreAJourTousLesEnregistrements()
        {
            if (_ouvrierSelectionne == null) return;

            // Mettre à jour tous les enregistrements de cet ouvrier
            var enregistrements = _ouvriers.Where(o => o.OuvrierId == _ouvrierSelectionne.OuvrierId).ToList();
            foreach (var enreg in enregistrements)
            {
                enreg.Nom = _ouvrierSelectionne.Nom;
                enreg.Prenom = _ouvrierSelectionne.Prenom;
                enreg.CoutJournalier = _ouvrierSelectionne.CoutJournalier;
            }
        }

        #endregion

        #region Actions CRUD Ouvriers

        private void btnNouveauOuvrier_Click(object sender, EventArgs e)
        {
            var nouvelOuvrier = new OuvrierInfo
            {
                OuvrierId = "",
                Nom = "Nouveau",
                Prenom = "Ouvrier",
                CoutJournalier = 200,
                NombreCompetences = 0
            };

            _ouvrierSelectionne = nouvelOuvrier;
            _enModification = false;

            RafraichirDetailsOuvrier();
            RafraichirCompetences();

            // Focus sur l'ID pour saisie
            txtOuvrierId.Focus();
            txtOuvrierId.SelectAll();
        }

        private void btnModifierOuvrier_Click(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne == null) return;

            _enModification = true;
            RafraichirDetailsOuvrier();
            txtNom.Focus();
            txtNom.SelectAll();
        }

        private void btnSupprimerOuvrier_Click(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne == null) return;

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer l'ouvrier '{_ouvrierSelectionne.NomComplet}' ?\n\n" +
                $"Cela supprimera également toutes ses {_ouvrierSelectionne.NombreCompetences} compétences.\n" +
                "Cette action est irréversible.",
                "Confirmation de suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _ouvrierService.SupprimerOuvrier(_ouvrierSelectionne.OuvrierId);
                    ChargerDonnees(); // Recharger depuis le service
                    _ouvrierSelectionne = null;
                    _enModification = false;

                    RafraichirAffichage();
                    MessageBox.Show("Ouvrier supprimé avec succès.", "Suppression", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Actions CRUD Compétences

        private void btnAjouterCompetence_Click(object sender, EventArgs e)
        {
            if (_ouvrierSelectionne == null) return;

            // Vérifier que l'ouvrier existe dans la liste
            if (string.IsNullOrWhiteSpace(_ouvrierSelectionne.OuvrierId))
            {
                MessageBox.Show("Veuillez d'abord saisir un ID pour l'ouvrier.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtOuvrierId.Focus();
                return;
            }

            var metiersDisponibles = _metiers.Where(m =>
                !_ouvriers.Any(o => o.OuvrierId == _ouvrierSelectionne.OuvrierId && o.MetierId == m.MetierId)
            ).ToList();

            if (!metiersDisponibles.Any())
            {
                MessageBox.Show("Aucun métier disponible à ajouter. L'ouvrier maîtrise déjà tous les métiers.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new CompetenceDialog(metiersDisponibles, null);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var nouvelleCompetence = new OuvrierCsvRecord
                {
                    OuvrierId = _ouvrierSelectionne.OuvrierId,
                    Nom = _ouvrierSelectionne.Nom,
                    Prenom = _ouvrierSelectionne.Prenom,
                    CoutJournalier = _ouvrierSelectionne.CoutJournalier,
                    MetierId = dialog.MetierSelectionne.MetierId,
                    NiveauExpertise = dialog.NiveauExpertise,
                    PerformancePct = dialog.PerformancePct
                };

                try
                {
                    _ouvrierService.AjouterOuvrier(nouvelleCompetence);
                    ChargerDonnees(); // Recharger depuis le service

                    // Mettre à jour l'ouvrier sélectionné
                    _ouvrierSelectionne.NombreCompetences++;

                    RafraichirAffichage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ajout de la compétence :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnModifierCompetence_Click(object sender, EventArgs e)
        {
            if (_competenceSelectionnee == null) return;

            var metier = _metiers.FirstOrDefault(m => m.MetierId == _competenceSelectionnee.MetierId);
            if (metier == null)
            {
                MessageBox.Show("Métier non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var dialog = new CompetenceDialog(new List<MetierCsvRecord> { metier }, _competenceSelectionnee);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _competenceSelectionnee.NiveauExpertise = dialog.NiveauExpertise;
                    _competenceSelectionnee.PerformancePct = dialog.PerformancePct;

                    _ouvrierService.ModifierOuvrier(_competenceSelectionnee);
                    ChargerDonnees(); // Recharger depuis le service

                    RafraichirCompetences();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la modification de la compétence :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSupprimerCompetence_Click(object sender, EventArgs e)
        {
            if (_competenceSelectionnee == null) return;

            var metier = _metiers.FirstOrDefault(m => m.MetierId == _competenceSelectionnee.MetierId);
            var nomMetier = metier?.Nom ?? _competenceSelectionnee.MetierId;

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer la compétence '{nomMetier}' de l'ouvrier '{_ouvrierSelectionne.NomComplet}' ?\n\n" +
                "Cette action est irréversible.",
                "Confirmation de suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _ouvrierService.SupprimerCompetence(_competenceSelectionnee.OuvrierId, _competenceSelectionnee.MetierId);
                    ChargerDonnees(); // Recharger depuis le service

                    // Mettre à jour l'ouvrier sélectionné
                    _ouvrierSelectionne.NombreCompetences--;
                    _competenceSelectionnee = null;

                    RafraichirAffichage();
                    MessageBox.Show("Compétence supprimée avec succès.", "Suppression", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression de la compétence :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Import/Export

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
                    _enModification = false;
                    RafraichirAffichage();

                    MessageBox.Show(
                        $"Import terminé avec succès !\n\n" +
                        $"• {nombreImporte} lignes importées\n" +
                        $"• {_ouvriers.GroupBy(o => o.OuvrierId).Count()} ouvriers total",
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
                        _enModification = false;
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
            if (!_ouvriers.Any())
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

                    var nombreOuvriers = _ouvriers.GroupBy(o => o.OuvrierId).Count();
                    MessageBox.Show(
                        $"Export terminé avec succès !\n\n" +
                        $"• {_ouvriers.Count} lignes exportées\n" +
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

        #endregion

        #region Actions du formulaire

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        #endregion
    }
}