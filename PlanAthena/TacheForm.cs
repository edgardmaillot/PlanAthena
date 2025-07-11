using PlanAthena.Core.Domain;
using PlanAthena.CsvModels;
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
    public partial class TacheForm : Form
    {
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly CsvDataService _csvDataService;
        private readonly ExcelReader _excelReader;

        private List<TacheCsvRecord> _taches = new List<TacheCsvRecord>();
        private List<MetierCsvRecord> _metiers = new List<MetierCsvRecord>();
        private TacheCsvRecord _elementSelectionne = null;
        private bool _enModification = false;

        // Enumération pour le type d'affichage
        private enum ModeAffichage
        {
            Lots,
            Blocs,
            Taches
        }

        private ModeAffichage _modeAffichageActuel = ModeAffichage.Taches;

        public TacheForm(TacheService tacheService, MetierService metierService)
        {
            InitializeComponent();
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _csvDataService = new CsvDataService();
            _excelReader = new ExcelReader();
        }

        private void TacheForm_Load(object sender, EventArgs e)
        {
            ChargerDonnees();
            InitialiserInterface();
            RafraichirAffichage();
        }

        private void InitialiserInterface()
        {
            // Désactiver les contrôles au démarrage
            groupBoxDetails.Enabled = false;
            btnModifierElement.Enabled = false;
            btnSupprimerElement.Enabled = false;

            // Initialiser le combo des métiers
            InitialiserComboMetiers();
        }

        private void InitialiserComboMetiers()
        {
            cmbMetier.Items.Clear();
            cmbMetier.Items.Add(new { MetierId = "", Nom = "(Aucun métier assigné)" });

            foreach (var metier in _metiers.OrderBy(m => m.Nom))
            {
                cmbMetier.Items.Add(metier);
            }

            cmbMetier.DisplayMember = "Nom";
            cmbMetier.ValueMember = "MetierId";
        }

        #region Gestion des données

        private void ChargerDonnees()
        {
            _taches = _tacheService.ObtenirToutesLesTaches();
            _metiers = _metierService.GetAllMetiers().ToList();
        }

        private void RafraichirAffichage()
        {
            RafraichirArbre();
            RafraichirStatutArbre();
            RafraichirDetails();
            RafraichirMapping();
        }

        private void RafraichirArbre()
        {
            treeViewTaches.Nodes.Clear();

            var tachesAfichees = _taches.AsEnumerable();

            // Filtrage par recherche
            if (!string.IsNullOrWhiteSpace(txtRechercheArbre.Text))
            {
                var recherche = txtRechercheArbre.Text.ToLower();
                tachesAfichees = tachesAfichees.Where(t =>
                    t.TacheId.ToLower().Contains(recherche) ||
                    t.TacheNom.ToLower().Contains(recherche) ||
                    t.BlocId.ToLower().Contains(recherche) ||
                    t.LotId.ToLower().Contains(recherche));
            }

            switch (_modeAffichageActuel)
            {
                case ModeAffichage.Lots:
                    ConstruireArbreLots(tachesAfichees);
                    break;
                case ModeAffichage.Blocs:
                    ConstruireArbreBlocs(tachesAfichees);
                    break;
                case ModeAffichage.Taches:
                    ConstruireArbreTaches(tachesAfichees);
                    break;
            }

            treeViewTaches.ExpandAll();
        }

        private void ConstruireArbreLots(IEnumerable<TacheCsvRecord> taches)
        {
            var lots = taches.GroupBy(t => t.LotId).OrderBy(g => g.First().LotPriorite);

            foreach (var lot in lots)
            {
                var lotNode = new TreeNode($"📁 {lot.Key} - {lot.First().LotNom}")
                {
                    Tag = new { Type = "Lot", Data = lot.First() }
                };

                var blocs = lot.GroupBy(t => t.BlocId);
                foreach (var bloc in blocs)
                {
                    var blocNode = new TreeNode($"📂 {bloc.Key} - {bloc.First().BlocNom}")
                    {
                        Tag = new { Type = "Bloc", Data = bloc.First() }
                    };

                    foreach (var tache in bloc.OrderBy(t => t.TacheId))
                    {
                        var icone = string.IsNullOrEmpty(tache.MetierId) ? "❌" : "✅";
                        var tacheNode = new TreeNode($"{icone} {tache.TacheId} - {tache.TacheNom}")
                        {
                            Tag = new { Type = "Tache", Data = tache }
                        };
                        blocNode.Nodes.Add(tacheNode);
                    }

                    lotNode.Nodes.Add(blocNode);
                }

                treeViewTaches.Nodes.Add(lotNode);
            }
        }

        private void ConstruireArbreBlocs(IEnumerable<TacheCsvRecord> taches)
        {
            var blocs = taches.GroupBy(t => t.BlocId).OrderBy(g => g.Key);

            foreach (var bloc in blocs)
            {
                var blocNode = new TreeNode($"📂 {bloc.Key} - {bloc.First().BlocNom}")
                {
                    Tag = new { Type = "Bloc", Data = bloc.First() }
                };

                foreach (var tache in bloc.OrderBy(t => t.TacheId))
                {
                    var icone = string.IsNullOrEmpty(tache.MetierId) ? "❌" : "✅";
                    var tacheNode = new TreeNode($"{icone} {tache.TacheId} - {tache.TacheNom}")
                    {
                        Tag = new { Type = "Tache", Data = tache }
                    };
                    blocNode.Nodes.Add(tacheNode);
                }

                treeViewTaches.Nodes.Add(blocNode);
            }
        }

        private void ConstruireArbreTaches(IEnumerable<TacheCsvRecord> taches)
        {
            foreach (var tache in taches.OrderBy(t => t.TacheId))
            {
                var icone = string.IsNullOrEmpty(tache.MetierId) ? "❌" : "✅";
                var metierInfo = string.IsNullOrEmpty(tache.MetierId) ? "" : $" ({tache.MetierId})";
                var tacheNode = new TreeNode($"{icone} {tache.TacheId} - {tache.TacheNom}{metierInfo}")
                {
                    Tag = new { Type = "Tache", Data = tache }
                };
                treeViewTaches.Nodes.Add(tacheNode);
            }
        }

        private void RafraichirStatutArbre()
        {
            var total = _taches.Count;
            var affiches = CompterElementsAffiches();

            if (affiches == total)
                lblStatutArbre.Text = $"{total} élément(s)";
            else
                lblStatutArbre.Text = $"{affiches}/{total} élément(s) affiché(s)";
        }

        private int CompterElementsAffiches()
        {
            int count = 0;
            foreach (TreeNode node in treeViewTaches.Nodes)
            {
                count += CompterNoeuds(node);
            }
            return count;
        }

        private int CompterNoeuds(TreeNode node)
        {
            int count = 1;
            foreach (TreeNode child in node.Nodes)
            {
                count += CompterNoeuds(child);
            }
            return count;
        }

        private void RafraichirDetails()
        {
            if (_elementSelectionne == null)
            {
                groupBoxDetails.Enabled = false;
                ViderDetails();
                return;
            }

            groupBoxDetails.Enabled = true;
            RemplirDetails(_elementSelectionne);
        }

        private void ViderDetails()
        {
            txtIdElement.Clear();
            txtNomElement.Clear();
            numHeuresHomme.Value = 0;
            txtDependances.Clear();
            cmbMetier.SelectedIndex = 0; // "(Aucun métier assigné)"
        }

        private void RemplirDetails(TacheCsvRecord tache)
        {
            txtIdElement.Text = tache.TacheId;
            txtNomElement.Text = tache.TacheNom;
            numHeuresHomme.Value = tache.HeuresHommeEstimees;
            txtDependances.Text = tache.Dependencies ?? "";

            // Sélectionner le métier
            if (string.IsNullOrEmpty(tache.MetierId))
            {
                cmbMetier.SelectedIndex = 0;
            }
            else
            {
                var metierItem = cmbMetier.Items.Cast<object>()
                    .FirstOrDefault(item =>
                    {
                        if (item is MetierCsvRecord metier)
                            return metier.MetierId == tache.MetierId;
                        return false;
                    });

                if (metierItem != null)
                    cmbMetier.SelectedItem = metierItem;
                else
                    cmbMetier.SelectedIndex = 0;
            }
        }

        private void RafraichirMapping()
        {
            // Rafraîchir la liste des tâches sans métier
            listViewTachesSansMetier.Items.Clear();

            var tachesSansMetier = _taches.Where(t => string.IsNullOrEmpty(t.MetierId)).ToList();

            foreach (var tache in tachesSansMetier.OrderBy(t => t.TacheId))
            {
                var item = new ListViewItem(new[] {
                    tache.TacheId,
                    tache.TacheNom,
                    tache.BlocId
                })
                {
                    Tag = tache
                };
                listViewTachesSansMetier.Items.Add(item);
            }

            // Rafraîchir les statistiques de mapping
            var totalTaches = _taches.Count;
            var tachesAvecMetier = totalTaches - tachesSansMetier.Count;
            var pourcentage = totalTaches > 0 ? (double)tachesAvecMetier / totalTaches * 100 : 0;

            lblStatutMapping.Text = $"Mapping: {pourcentage:F0}% ({tachesAvecMetier}/{totalTaches} tâches)";
            progressMapping.Maximum = totalTaches;
            progressMapping.Value = tachesAvecMetier;
        }

        #endregion

        #region Import/Export

        private void btnImporter_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                Title = "Importer les tâches depuis un fichier CSV"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Voulez-vous remplacer toutes les tâches existantes ?\n\n" +
                        "• Oui : Remplace toutes les tâches actuelles\n" +
                        "• Non : Ajoute aux tâches existantes\n" +
                        "• Annuler : Annule l'import",
                        "Mode d'import",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel) return;

                    var nombreImporte = _tacheService.ImporterDepuisCsv(ofd.FileName, result == DialogResult.Yes);
                    ChargerDonnees();

                    _elementSelectionne = null;
                    _enModification = false;
                    InitialiserComboMetiers();
                    RafraichirAffichage();

                    MessageBox.Show(
                        $"Import terminé avec succès !\n\n" +
                        $"• {nombreImporte} tâches importées\n" +
                        $"• {_taches.Count} tâches total\n" +
                        $"• {_taches.Count(t => !string.IsNullOrEmpty(t.MetierId))} tâches avec métier",
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

        private void btnImportExcelFieldwire_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Fichiers Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Tous les fichiers (*.*)|*.*",
                Title = "Importer les tâches depuis un fichier Excel Fieldwire/Dalux"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // TODO: Implémenter l'import Excel Fieldwire/Dalux spécifique
                    var nombreImporte = _tacheService.ImporterDepuisExcelFieldwire(ofd.FileName);

                    if (nombreImporte > 0)
                    {
                        ChargerDonnees();
                        _elementSelectionne = null;
                        _enModification = false;
                        InitialiserComboMetiers();
                        RafraichirAffichage();

                        MessageBox.Show(
                            $"Import Excel Fieldwire terminé avec succès !\n\n" +
                            $"• {nombreImporte} tâches importées",
                            "Import réussi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "L'import Excel Fieldwire/Dalux n'est pas encore implémenté.\n" +
                            "Utilisez l'import CSV pour le moment.",
                            "Fonctionnalité en développement",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'import Excel Fieldwire :\n{ex.Message}",
                        "Erreur d'import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (!_taches.Any())
            {
                MessageBox.Show("Aucune tâche à exporter.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                Title = "Exporter les tâches vers un fichier CSV",
                FileName = $"taches_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _tacheService.ExporterVersCsv(sfd.FileName);

                    var statistiques = _tacheService.ObtenirStatistiques();
                    var mappingStats = _tacheService.ObtenirStatistiquesMappingMetiers();

                    MessageBox.Show(
                        $"Export terminé avec succès !\n\n" +
                        $"• {statistiques.NombreTachesTotal} tâches exportées\n" +
                        $"• {statistiques.NombreBlocsUniques} blocs\n" +
                        $"• {statistiques.NombreLotsUniques} lots\n" +
                        $"• Mapping: {mappingStats.PourcentageMapping:F0}%\n" +
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

        #region Événements interface

        private void radioAffichage_CheckedChanged(object sender, EventArgs e)
        {
            if (radioLots.Checked)
                _modeAffichageActuel = ModeAffichage.Lots;
            else if (radioBlocs.Checked)
                _modeAffichageActuel = ModeAffichage.Blocs;
            else if (radioTaches.Checked)
                _modeAffichageActuel = ModeAffichage.Taches;

            RafraichirArbre();
        }

        private void txtRechercheArbre_TextChanged(object sender, EventArgs e)
        {
            RafraichirArbre();
            RafraichirStatutArbre();
        }

        private void treeViewTaches_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag != null)
            {
                dynamic nodeData = e.Node.Tag;
                if (nodeData.Type == "Tache")
                {
                    _elementSelectionne = nodeData.Data as TacheCsvRecord;
                    btnModifierElement.Enabled = true;
                    btnSupprimerElement.Enabled = true;
                }
                else
                {
                    _elementSelectionne = null;
                    btnModifierElement.Enabled = false;
                    btnSupprimerElement.Enabled = false;
                }
            }
            else
            {
                _elementSelectionne = null;
                btnModifierElement.Enabled = false;
                btnSupprimerElement.Enabled = false;
            }

            RafraichirDetails();
        }

        private void txtIdElement_TextChanged(object sender, EventArgs e)
        {
            if (_elementSelectionne != null && !_enModification)
            {
                _elementSelectionne.TacheId = txtIdElement.Text;
            }
        }

        private void txtNomElement_TextChanged(object sender, EventArgs e)
        {
            if (_elementSelectionne != null)
            {
                _elementSelectionne.TacheNom = txtNomElement.Text;
            }
        }

        private void numHeuresHomme_ValueChanged(object sender, EventArgs e)
        {
            if (_elementSelectionne != null)
            {
                _elementSelectionne.HeuresHommeEstimees = (int)numHeuresHomme.Value;
            }
        }

        private void txtDependances_TextChanged(object sender, EventArgs e)
        {
            if (_elementSelectionne != null)
            {
                _elementSelectionne.Dependencies = txtDependances.Text;
            }
        }

        private void cmbMetier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_elementSelectionne != null && cmbMetier.SelectedItem != null)
            {
                if (cmbMetier.SelectedItem is MetierCsvRecord metier)
                {
                    _elementSelectionne.MetierId = metier.MetierId;
                }
                else
                {
                    _elementSelectionne.MetierId = "";
                }

                // Sauvegarder et rafraîchir
                try
                {
                    _tacheService.ModifierTache(_elementSelectionne);
                    ChargerDonnees();
                    RafraichirArbre();
                    RafraichirMapping();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la sauvegarde du mapping :\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void listViewTachesSansMetier_DoubleClick(object sender, EventArgs e)
        {
            if (listViewTachesSansMetier.SelectedItems.Count > 0)
            {
                var tache = listViewTachesSansMetier.SelectedItems[0].Tag as TacheCsvRecord;
                if (tache != null)
                {
                    // Sélectionner cette tâche dans l'arbre
                    SelectionnerTacheDansArbre(tache);
                }
            }
        }

        private void SelectionnerTacheDansArbre(TacheCsvRecord tache)
        {
            foreach (TreeNode node in treeViewTaches.Nodes)
            {
                if (TrouverEtSelectionnerTache(node, tache))
                    break;
            }
        }

        private bool TrouverEtSelectionnerTache(TreeNode node, TacheCsvRecord tache)
        {
            if (node.Tag != null)
            {
                dynamic nodeData = node.Tag;
                if (nodeData.Type == "Tache" && nodeData.Data == tache)
                {
                    treeViewTaches.SelectedNode = node;
                    node.EnsureVisible();
                    return true;
                }
            }

            foreach (TreeNode child in node.Nodes)
            {
                if (TrouverEtSelectionnerTache(child, tache))
                    return true;
            }

            return false;
        }

        #endregion

        #region Actions CRUD

        private void btnNouvelleElement_Click(object sender, EventArgs e)
        {
            var nouvelleTache = new TacheCsvRecord
            {
                TacheId = "",
                TacheNom = "Nouvelle tâche",
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

            try
            {
                _tacheService.AjouterTache(nouvelleTache);
                ChargerDonnees();
                _elementSelectionne = nouvelleTache;
                _enModification = false;

                RafraichirAffichage();
                SelectionnerTacheDansArbre(nouvelleTache);

                // Focus sur l'ID pour saisie
                txtIdElement.Focus();
                txtIdElement.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création de la tâche :\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModifierElement_Click(object sender, EventArgs e)
        {
            if (_elementSelectionne == null) return;

            _enModification = true;
            txtNomElement.Focus();
            txtNomElement.SelectAll();
        }

        private void btnSupprimerElement_Click(object sender, EventArgs e)
        {
            if (_elementSelectionne == null) return;

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer la tâche '{_elementSelectionne.TacheId}' ?\n\n" +
                $"Nom: {_elementSelectionne.TacheNom}\n" +
                "Cette action est irréversible.",
                "Confirmation de suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _tacheService.SupprimerTache(_elementSelectionne.TacheId);
                    ChargerDonnees();
                    _elementSelectionne = null;
                    _enModification = false;

                    RafraichirAffichage();
                    MessageBox.Show("Tâche supprimée avec succès.", "Suppression",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnMappingAuto_Click(object sender, EventArgs e)
        {
            var tachesSansMetier = _taches.Where(t => string.IsNullOrEmpty(t.MetierId)).ToList();

            if (!tachesSansMetier.Any())
            {
                MessageBox.Show("Toutes les tâches ont déjà un métier assigné.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Le mapping automatique va essayer d'assigner des métiers aux {tachesSansMetier.Count} tâches sans métier.\n\n" +
                "Cette fonction n'est pas encore implémentée complètement.\n" +
                "Voulez-vous continuer ?",
                "Mapping automatique",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // TODO: Implémenter la logique de mapping automatique
                MessageBox.Show(
                    "Mapping automatique en cours de développement.\n" +
                    "Utilisez le mapping manuel pour le moment.",
                    "Fonctionnalité en développement",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}
#endregion