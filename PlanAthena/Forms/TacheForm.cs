// Fichier: PlanAthena/Forms/TacheForm.cs
using PlanAthena.View;
using PlanAthena.View.TaskManager;
using PlanAthena.View.TaskManager.PertDiagram;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    public partial class TacheForm : Form
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly ImportService _importService;
        private String _lotActifId = null;
        private readonly PertDiagramControl _pertControl;
        private readonly TacheDetailForm _tacheDetailForm;
        private readonly ToolTip _toolTipMetiers = new ToolTip();
        private readonly ToolTip _toolTipPlan = new ToolTip();

        public TacheForm(ProjetService projetService, RessourceService ressourceService, DependanceBuilder dependanceBuilder, ImportService importService)
        {
            InitializeComponent();
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));

            try
            {
                _pertControl = new PertDiagramControl { Dock = DockStyle.Fill };
                _pertControl.Initialize(_projetService, _ressourceService, _dependanceBuilder, new PertDiagramSettings());
                //_pertControl.TacheSelected += PertControl_TacheSelected;
                //_pertControl.TacheDoubleClicked += PertControl_TacheDoubleClicked;
                //_pertControl.BlocDoubleClicked += PertControl_BlocDoubleClicked;
                _pertControl.ZoomChanged += PertControl_ZoomChanged;
                this.panelDiagrammeCentral.Controls.Add(_pertControl);
                _tacheDetailForm = new TacheDetailForm(_projetService, _ressourceService, _dependanceBuilder);
                IntegrerFormulaireDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur initialisation:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void PertControl_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try { RafraichirStatistiques(_projetService.ObtenirToutesLesTaches()); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur stats zoom: {ex.Message}"); }
        }

        private void IntegrerFormulaireDetails()
        {
            _tacheDetailForm.TopLevel = false;
            _tacheDetailForm.FormBorderStyle = FormBorderStyle.None;
            _tacheDetailForm.Dock = DockStyle.Fill;
            panelDetailsTache.Controls.Add(_tacheDetailForm);
            _tacheDetailForm.Show();
            _tacheDetailForm.TacheSauvegardee += (s, e) => RafraichirDiagrammeEtStatistiques();
        }

        private void TacheForm_Load(object sender, EventArgs e)
        {
            try { RafraichirVueComplete(); }
            catch (Exception ex) { MessageBox.Show($"Erreur chargement:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void RafraichirVueComplete()
        {
            try
            {
                PeuplerComboBoxLots();
                if (_lotActifId != null)
                {
                    var itemToReselect = cmbLots.Items.Cast<Lot>().FirstOrDefault(l => l.LotId == _lotActifId);
                    cmbLots.SelectedItem = itemToReselect;
                }
                if (cmbLots.SelectedItem == null && cmbLots.Items.Count > 0)
                {
                    cmbLots.SelectedIndex = 0;
                }
                CreerBoutonsMetiers();
                _tacheDetailForm?.MettreAJourListesDeroulantes(_lotActifId);
                RafraichirDiagrammeEtStatistiques();
                _tacheDetailForm?.ChargerTache(new Tache { LotId = _lotActifId, HeuresHommeEstimees = 8, Type = TypeActivite.Tache }, true);
                lblTacheSelectionnee.Text = "Aucune sélection";
                AfficherPlanLotActif();
            }
            catch (Exception ex) { MessageBox.Show($"Erreur rafraîchissement:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void RafraichirDiagrammeEtStatistiques()
        {
            try
            {
                var toutesLesTaches = _projetService.ObtenirToutesLesTaches();
                List<Tache> tachesAffichees = (_lotActifId != null) ? toutesLesTaches.Where(t => t.LotId == _lotActifId).ToList() : new List<Tache>();
                _pertControl?.ChargerDonnees(tachesAffichees, txtRecherche.Text);
                RafraichirStatistiques(toutesLesTaches);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur rafraîchissement diagramme: {ex.Message}");
                RafraichirStatistiques(new List<Tache>());
            }
        }

        private void PeuplerComboBoxLots()
        {
            try
            {
                var lots = _projetService.ObtenirTousLesLots();
                cmbLots.DataSource = null;
                cmbLots.DataSource = lots;
                cmbLots.DisplayMember = "Nom";
                cmbLots.ValueMember = "LotId";
            }
            catch (Exception ex) { MessageBox.Show($"Erreur chargement lots:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void RafraichirStatistiques(List<Tache> toutesLesTaches)
        {
            try
            {
                var totalTaches = toutesLesTaches?.Count ?? 0;
                var lotActif = _projetService.ObtenirLotParId(_lotActifId);
                int tachesLotActif = (_lotActifId != null && toutesLesTaches != null) ? toutesLesTaches.Count(t => t.LotId == _lotActifId) : 0;
                var zoomPourcentage = Math.Round((_pertControl?.ZoomFacteur ?? 1.0) * 100, 1);
                lblStatistiques.Text = $"Total: {totalTaches} tâches | Lot '{lotActif?.Nom ?? "Aucun"}': {tachesLotActif} tâches | Zoom: {zoomPourcentage}%";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur stats: {ex.Message}");
                lblStatistiques.Text = "Erreur calcul stats";
            }
        }

        private void CreerBoutonsMetiers()
        {
            try
            {
                panelOutilsMetiersDynamiques.Controls.Clear();
                int yPos = 10;
                Image titleIcon = new Bitmap(Properties.Resources.tache, new Size(20, 20));
                var lblBlocTitle = new Label { Text = "     Créer une tâche :", Image = titleIcon, ImageAlign = ContentAlignment.MiddleLeft, AutoSize = true, Location = new Point(11, yPos), Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold), Padding = new Padding(0, 0, 0, 10) };
                panelOutilsMetiersDynamiques.Controls.Add(lblBlocTitle);
                yPos += lblBlocTitle.Height + 5;

                if (_lotActifId == null) return;
                var lotActif = _projetService.ObtenirLotParId(_lotActifId);
                var metiersTries = _dependanceBuilder.ObtenirMetiersTriesParDependance()
                    .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H" && m.Phases.HasFlag(lotActif.Phases));

                foreach (var metier in metiersTries)
                {
                    try
                    {
                        var btn = new Button { Text = metier.Nom, Tag = metier, Location = new Point(11, yPos), Size = new Size(160, 30), BackColor = _ressourceService.GetDisplayColorForMetier(metier.MetierId), FlatStyle = FlatStyle.Popup };
                        btn.Click += MetierButton_Click;
                        var prerequis = _ressourceService.GetPrerequisPourPhase(metier.MetierId, lotActif.Phases);
                        if (prerequis.Any())
                        {
                            var prerequisNoms = prerequis.Select(id => _ressourceService.GetMetierById(id)?.Nom ?? id);
                            _toolTipMetiers.SetToolTip(btn, $"Prérequis: {string.Join(", ", prerequisNoms)}");
                        }
                        else { _toolTipMetiers.SetToolTip(btn, "Aucun prérequis"); }
                        panelOutilsMetiersDynamiques.Controls.Add(btn);
                        yPos += 35;
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur création bouton {metier.Nom}: {ex.Message}"); }
                }

                var separator = new Label { BorderStyle = BorderStyle.Fixed3D, Height = 2, Width = 160, Location = new Point(11, yPos + 5) };
                panelOutilsMetiersDynamiques.Controls.Add(separator);
                yPos += 15;
                var btnJalon = new Button { Text = "◆ Créer Jalon/Attente", Location = new Point(11, yPos), Size = new Size(160, 30), BackColor = Color.Gold, Font = new Font(this.Font, FontStyle.Bold) };
                btnJalon.Click += JalonButton_Click;
                _toolTipMetiers.SetToolTip(btnJalon, "Créer un jalon d'attente manuel");
                panelOutilsMetiersDynamiques.Controls.Add(btnJalon);
            }
            catch (Exception ex) { MessageBox.Show($"Erreur création outils métiers:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void MetierButton_Click(object sender, EventArgs e)
        {
            if (_lotActifId == null) { MessageBox.Show("Sélectionnez un lot.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (sender is Button { Tag: Metier metier })
            {
                using var form = new TacheDetailForm(_projetService, _ressourceService, _dependanceBuilder);
                var nouvelleTache = new Tache { MetierId = metier.MetierId, LotId = _lotActifId, TacheNom = $"Nouvelle tâche - {metier.Nom}", HeuresHommeEstimees = 8 };
                form.MettreAJourListesDeroulantes(_lotActifId);
                form.ChargerTache(nouvelleTache, true);
                if (form.ShowDialog(this) == DialogResult.OK) RafraichirDiagrammeEtStatistiques();
            }
        }

        private void JalonButton_Click(object sender, EventArgs e)
        {
            if (_lotActifId == null) { MessageBox.Show("Sélectionnez un lot.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            using var form = new TacheDetailForm(_projetService, _ressourceService, _dependanceBuilder);
            var nouveauJalon = new Tache { Type = TypeActivite.JalonUtilisateur, HeuresHommeEstimees = 24, TacheNom = "Nouveau Jalon d'attente", LotId = _lotActifId };
            form.MettreAJourListesDeroulantes(_lotActifId);
            form.ChargerTache(nouveauJalon, true);
            if (form.ShowDialog(this) == DialogResult.OK) RafraichirDiagrammeEtStatistiques();
        }

        private void OuvrirGestionBlocs(string blocIdToEdit = null, Lot lotPourCreation = null)
        {
            try
            {
                using (var form = new BlocForm(_projetService, blocIdToEdit, lotPourCreation))
                {
                    if (form.ShowDialog(this) == DialogResult.OK) RafraichirVueComplete();
                }
            }
            catch (Exception ex) { MessageBox.Show($"Erreur ouverture gestion blocs:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnAjouterBloc_Click(object sender, EventArgs e)
        {
            if (_lotActifId == null) { MessageBox.Show("Sélectionnez un lot.", "Lot requis", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var lotActif = _projetService.ObtenirLotParId(_lotActifId);
            OuvrirGestionBlocs(null, lotActif);
        }

        private void PertControl_BlocDoubleClicked(object sender, BlocSelectedEventArgs e)
        {
            if (_lotActifId == null) { MessageBox.Show("Sélectionnez un lot.", "Lot requis", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var lotActif = _projetService.ObtenirLotParId(_lotActifId);
            OuvrirGestionBlocs(e.BlocId, lotActif);
        }

        private void PertControl_TacheSelected(object sender, TacheSelectedEventArgs e)
        {
            _tacheDetailForm?.MettreAJourListesDeroulantes(e.Tache.LotId);
            _tacheDetailForm.ChargerTache(e.Tache, false);
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom}";
        }

        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            var tacheOriginale = _projetService.ObtenirTacheParId(e.Tache.TacheId);
            if (tacheOriginale != null)
            {
                using var form = new TacheDetailForm(_projetService, _ressourceService, _dependanceBuilder);
                form.MettreAJourListesDeroulantes(tacheOriginale.LotId);
                form.ChargerTache(tacheOriginale, false);
                if (form.ShowDialog(this) == DialogResult.OK) RafraichirDiagrammeEtStatistiques();
            }
        }

        private void cmbLots_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbLots.SelectedItem is Lot selectedLot)
                {
                    _lotActifId = selectedLot.LotId;
                    CreerBoutonsMetiers();
                    RafraichirDiagrammeEtStatistiques();
                    _tacheDetailForm?.MettreAJourListesDeroulantes(_lotActifId);
                    _tacheDetailForm?.ChargerTache(new Tache { LotId = _lotActifId, HeuresHommeEstimees = 8, Type = TypeActivite.Tache }, true);
                    lblTacheSelectionnee.Text = "Aucune sélection";
                    AfficherPlanLotActif();
                }
            }
            catch (Exception ex) { MessageBox.Show($"Erreur changement de lot:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void btnGererLots_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new LotForm(_projetService)) { form.ShowDialog(this); }
                RafraichirVueComplete();
            }
            catch (Exception ex) { MessageBox.Show($"Erreur ouverture gestion lots:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnMappingAuto_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fonctionnalité en développement.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            try { RafraichirDiagrammeEtStatistiques(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur recherche: {ex.Message}"); }
        }

        private void btnZoomAjuster_Click(object sender, EventArgs e)
        {
            try { _pertControl?.ZoomToutAjuster(); }
            catch (Exception ex) { MessageBox.Show($"Erreur zoom:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void btnPan_Click(object sender, EventArgs e)
        {
            try { _pertControl?.TogglePan(btnPan.Checked); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur pan: {ex.Message}"); btnPan.Checked = false; }
        }

        private void btnSauvegarderImage_Click(object sender, EventArgs e)
        {
            try { _pertControl?.SauvegarderImage(); }
            catch (Exception ex) { MessageBox.Show($"Erreur sauvegarde image:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnImprimer_Click(object sender, EventArgs e)
        {
            try { _pertControl?.ImprimerDiagramme(); }
            catch (Exception ex) { MessageBox.Show($"Erreur impression:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void AfficherPlanLotActif()
        {
            if (pictureBox1.Image != null) { pictureBox1.Image.Dispose(); pictureBox1.Image = null; }
            pictureBox1.BackColor = SystemColors.Control;
            pictureBox1.Visible = true;
            _toolTipPlan.SetToolTip(pictureBox1, "");
            var lotActif = _projetService.ObtenirLotParId(_lotActifId);
            if (lotActif == null || string.IsNullOrWhiteSpace(lotActif.CheminFichierPlan)) return;
            string filePath = lotActif.CheminFichierPlan;
            if (!File.Exists(filePath)) { _toolTipPlan.SetToolTip(pictureBox1, $"Fichier introuvable: {filePath}"); return; }
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            try
            {
                if (new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" }.Contains(extension))
                {
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        Image img = Image.FromStream(stream);
                        pictureBox1.Image = new Bitmap(img);
                        img.Dispose();
                    }
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    _toolTipPlan.SetToolTip(pictureBox1, $"Plan: {filePath}");
                }
                else if (extension == ".pdf")
                {
                    if (MessageBox.Show("Ouvrir le PDF dans l'application par défaut ?", "Format non supporté", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true }); }
                        catch (Exception ex) { MessageBox.Show($"Impossible d'ouvrir le PDF: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                    _toolTipPlan.SetToolTip(pictureBox1, $"Plan PDF: {filePath}");
                    pictureBox1.Click += PictureBox1_ClickForPdf;
                }
                else { _toolTipPlan.SetToolTip(pictureBox1, $"Format non supporté: {extension}"); }
            }
            catch (Exception ex) { MessageBox.Show($"Erreur chargement plan:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); pictureBox1.Image = null; }
        }

        private void PictureBox1_ClickForPdf(object sender, EventArgs e)
        {
            var lotActif = _projetService.ObtenirLotParId(_lotActifId);
            if (lotActif != null && !string.IsNullOrWhiteSpace(lotActif.CheminFichierPlan))
            {
                string filePath = lotActif.CheminFichierPlan;
                if (File.Exists(filePath) && Path.GetExtension(filePath).ToLowerInvariant() == ".pdf")
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true }); }
                    catch (Exception ex) { MessageBox.Show($"Impossible d'ouvrir le PDF: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void btnImportExcelFieldwire_Click(object sender, EventArgs e) => MessageBox.Show("Fonctionnalité en développement.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void btnImporter_Click(object sender, EventArgs e)
        {
            try
            {
                if (_lotActifId == null) { MessageBox.Show("Sélectionnez un lot.", "Lot requis", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                using var openFileDialog = new OpenFileDialog { Title = "Sélectionner le fichier CSV", Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*", DefaultExt = "csv", Multiselect = false };
                if (openFileDialog.ShowDialog() != DialogResult.OK) return;
                string filePath = openFileDialog.FileName;
                using (var importForm = new ImportTacheForm(filePath, _projetService.ObtenirLotParId(_lotActifId), _projetService, _ressourceService))
                {
                    if (importForm.ShowDialog(this) == DialogResult.OK)
                    {
                        var mappingConfig = importForm.MappingConfiguration;
                        ExecuterImportCSV(filePath, mappingConfig, false);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Erreur préparation import: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ExecuterImportCSV(string filePath, ImportMappingConfiguration mappingConfig, bool confirmerEcrasement)
        {
            try
            {
                // L'APPEL EST MAINTENANT DIRECT ET SIMPLE.
                // On passe les informations de l'IHM, et le service fait tout le travail en interne.
                var resultat = _importService.ImporterTachesCSV(filePath, _lotActifId, mappingConfig, confirmerEcrasement);

                // La logique de gestion du résultat reste la même, elle était déjà correcte.
                if (resultat.ConfirmationRequise)
                {
                    if (MessageBox.Show(resultat.MessageConfirmation, "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        ExecuterImportCSV(filePath, mappingConfig, true);
                    }
                    return;
                }

                if (resultat.EstSucces)
                {
                    var rapportBuilder = new StringBuilder();
                    rapportBuilder.AppendLine("🎉 IMPORT RÉUSSI");
                    rapportBuilder.AppendLine($"• {resultat.NbTachesImportees} tâche(s) importée(s)");
                    rapportBuilder.AppendLine($"• {resultat.NbBlocsTraites} bloc(s) créé(s)");
                    rapportBuilder.AppendLine($"• Durée: {resultat.DureeImport.TotalSeconds:F1}s");
                    if (resultat.Warnings.Any())
                    {
                        using (var warningsDialog = new ImportWarningsDialog(resultat.Warnings)) { warningsDialog.ShowDialog(this); }
                        rapportBuilder.AppendLine("\n⚠️ Des avertissements ont été générés.");
                    }
                    MessageBox.Show(rapportBuilder.ToString(), "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // On rafraîchit simplement la vue, les services sont déjà à jour.
                    RafraichirVueComplete();
                }
                else
                {
                    MessageBox.Show($"Échec de l'import:\n\n{resultat.MessageErreur}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur inattendue import:\n\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExporter_Click(object sender, EventArgs e) => MessageBox.Show("Fonctionnalité en développement.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void btnFermer_Click(object sender, EventArgs e) => this.Close();
    }
}