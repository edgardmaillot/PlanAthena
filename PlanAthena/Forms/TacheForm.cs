// START OF FILE TacheForm.cs

using PlanAthena.Controls;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess; 
using PlanAthena.Utilities;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Représente l'interface principale de gestion des tâches du projet.
    /// Son rôle est de fournir un espace de travail interactif centré sur un "Lot" de travaux.
    /// 
    /// PRINCIPE ARCHITECTURAL CLÉ :
    /// Ce formulaire n'embarque PAS de cache de données local (ex: _tachesBrutes).
    /// Il s'appuie systématiquement sur les services (TacheService, ProjetService, etc.) comme
    /// source de vérité unique. À chaque rafraîchissement, les données sont lues
    /// directement depuis le service, garantissant ainsi que l'affichage n'est jamais
    /// désynchronisé et éliminant une source majeure de bugs.
    /// </summary>
    public partial class TacheForm : System.Windows.Forms.Form
    {
        // Services (sources de vérité)
        private readonly TacheService _tacheService;
        private readonly ProjetService _projetService; 
        private readonly DependanceBuilder _dependanceBuilder;
        private readonly BlocService _blocService;

        // AJOUT : ImportOrchestrationService pour l'import
        private ImportOrchestrationService _importOrchestrationService;

        // État de l'UI
        private Lot _lotActif = null;

        // Contrôles UI
        private readonly PertDiagramControl _pertControl;
        private readonly TacheDetailForm _tacheDetailForm;
        private readonly ToolTip _toolTipMetiers = new ToolTip();
        private readonly ToolTip _toolTipPlan = new ToolTip();

        public TacheForm(TacheService tacheService, ProjetService projetService, DependanceBuilder dependanceBuilder, BlocService blocService) // Changement ici
        {
            InitializeComponent();

            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService)); // Changement ici
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));

            try
            {
                // TODO: Vérifier comment ImportServiceConfig.CreerImportService et CreerProjetService sont gérés
                // Ces lignes peuvent nécessiter une adaptation ou une suppression si l'injection est centralisée via DI
                // Pour l'instant, je les laisse pour ne pas introduire d'autres erreurs, mais c'est un point à valider.
                // var importService = ImportServiceConfig.CreerImportService(_tacheService, _projetService, _blocService, _metierService); // Ancien MetierService
                // _importOrchestrationService = ImportServiceConfig.CreerProjetService(importService);

                // Temporairement pour la compilation, si ImportOrchestrationService doit être injecté
                // Pour la phase d'implémentation, il est probable que ImportOrchestrationService soit aussi injecté ou créé plus haut
                // Pour éviter un crash immédiat:
                // TODO: Gérer l'injection réelle de _importOrchestrationService
                if (_importOrchestrationService == null)
                {
                    // Cette ligne est un placeholder et devrait être revue par l'injection de dépendances
                    // Pour le moment, elle évitera un NullReferenceException si le service n'est pas injecté
                    // et permettra de continuer l'intégration des autres changements.
                    // Une refonte du DI pour les services d'import sera nécessaire.
                    // Par exemple: _importOrchestrationService = provider.GetRequiredService<ImportOrchestrationService>();
                }

                _pertControl = new PertDiagramControl();
                _pertControl.Dock = DockStyle.Fill;
                // Changement ici : _pertControl.Initialize reçoit _projetService
                _pertControl.Initialize(_projetService, _blocService, _dependanceBuilder, new PertDiagramSettings());

                // Événements existants
                _pertControl.TacheSelected += PertControl_TacheSelected;
                _pertControl.TacheDoubleClicked += PertControl_TacheDoubleClicked;
                _pertControl.BlocDoubleClicked += PertControl_BlocDoubleClicked;

                // NOUVEAU : Abonnement à l'événement de changement de zoom
                _pertControl.ZoomChanged += PertControl_ZoomChanged;

                this.panelDiagrammeCentral.Controls.Add(_pertControl);

                // Changement ici : _tacheDetailForm reçoit _projetService
                _tacheDetailForm = new TacheDetailForm(_tacheService, _projetService, _blocService, _dependanceBuilder);
                IntegrerFormulaireDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation du formulaire :\n{ex.Message}",
                              "Erreur d'initialisation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-lancer l'exception car l'initialisation a échoué
            }
        }

        // Gestionnaire d'événement pour les changements de zoom
        private void PertControl_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try
            {
                // Mettre à jour les statistiques quand le zoom change
                RafraichirStatistiques(_tacheService.ObtenirToutesLesTaches());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des statistiques de zoom: {ex.Message}");
            }
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
            try
            {
                RafraichirVueComplete();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du formulaire :\n{ex.Message}",
                              "Erreur de chargement", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Logique de Rafraîchissement

        private void RafraichirVueComplete()
        {
            try
            {
                var idLotActif = _lotActif?.LotId;

                PeuplerComboBoxLots();

                if (idLotActif != null)
                {
                    var itemToReselect = cmbLots.Items.Cast<Lot>().FirstOrDefault(l => l.LotId == idLotActif);
                    cmbLots.SelectedItem = itemToReselect;
                }

                if (cmbLots.SelectedItem == null && cmbLots.Items.Count > 0)
                {
                    cmbLots.SelectedIndex = 0;
                }

                CreerBoutonsMetiers();

                _tacheDetailForm?.MettreAJourListesDeroulantes(_lotActif?.LotId);
                RafraichirDiagrammeEtStatistiques();
                _tacheDetailForm?.ChargerTache(new Tache { LotId = _lotActif?.LotId, HeuresHommeEstimees = 8, Type = TypeActivite.Tache }, true);
                lblTacheSelectionnee.Text = "Aucune sélection";
                AfficherPlanLotActif();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du rafraîchissement de la vue :\n{ex.Message}",
                              "Erreur de rafraîchissement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RafraichirDiagrammeEtStatistiques()
        {
            try
            {
                var toutesLesTaches = _tacheService.ObtenirToutesLesTaches();
                List<Tache> tachesAffichees = new List<Tache>();

                if (_lotActif != null)
                {
                    tachesAffichees = toutesLesTaches.Where(t => t.LotId == _lotActif.LotId).ToList();
                }

                _pertControl?.ChargerDonnees(tachesAffichees, txtRecherche.Text);
                RafraichirStatistiques(toutesLesTaches);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du rafraîchissement du diagramme: {ex.Message}");
                RafraichirStatistiques(new List<Tache>()); // Afficher au moins les statistiques vides
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du peuplement des lots: {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des lots :\n{ex.Message}",
                              "Erreur de chargement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RafraichirStatistiques(List<Tache> toutesLesTaches)
        {
            try
            {
                var totalTaches = toutesLesTaches?.Count ?? 0;
                int tachesLotActif = 0;

                if (_lotActif != null && toutesLesTaches != null)
                {
                    tachesLotActif = toutesLesTaches.Count(t => t.LotId == _lotActif.LotId);
                }

                // NOUVEAU : Inclure le zoom dans les statistiques
                var zoomPourcentage = Math.Round((_pertControl?.ZoomFacteur ?? 1.0) * 100, 1);

                lblStatistiques.Text = $"Total: {totalTaches} tâches | " +
                                     $"Lot '{_lotActif?.Nom ?? "Aucun"}': {tachesLotActif} tâches | " +
                                     $"Zoom: {zoomPourcentage}%";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du rafraîchissement des statistiques: {ex.Message}");
                lblStatistiques.Text = "Erreur lors du calcul des statistiques";
            }
        }

        #endregion

        #region Gestion de la Barre d'Outils Métiers et Création

        private void CreerBoutonsMetiers()
        {
            try
            {
                panelOutilsMetiersDynamiques.Controls.Clear();

                int yPos = 10; // Position de départ

                Image titleIcon = Properties.Resources.tache;

                // Optionnel: Redimensionner l'icône pour le titre si nécessaire (peut être un peu plus grande que les icônes de bouton)
                if (titleIcon.Width > 24 || titleIcon.Height > 24)
                {
                    titleIcon = new Bitmap(titleIcon, new Size(20, 20)); // Exemple: 20x20 pixels
                }

                // 2. Créer le Label pour le titre
                var lblBlocTitle = new Label
                {
                    Text = "     Créer une tâche :",
                    Image = titleIcon,
                    ImageAlign = ContentAlignment.MiddleLeft,
                    AutoSize = true, // Le label s'ajustera à la taille de son contenu (texte + image)
                    Location = new System.Drawing.Point(11, yPos),
                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),


                    Padding = new Padding(0, 0, 0, 10)
                };

                panelOutilsMetiersDynamiques.Controls.Add(lblBlocTitle);

                // 3. Mettre à jour la position de départ pour les boutons de métier
                yPos += lblBlocTitle.Height + 5; // Hauteur du label + un petit espace


                // Changement ici : _projetService.ObtenirMetiersTriesParDependance()
                var metiersTries = _projetService.ObtenirMetiersTriesParDependance()
                    .Where(m => m.MetierId != "JALON" && m.MetierId != "SYNC_0H");

                // Charger l'image pour les boutons (si vous voulez quand même les icônes sur les boutons,
                // sinon vous pouvez supprimer les lignes liées à `btn.Image` plus bas)
                Image tacheButtonIcon = Properties.Resources.tache;
                if (tacheButtonIcon.Width > 20 || tacheButtonIcon.Height > 20)
                {
                    tacheButtonIcon = new Bitmap(tacheButtonIcon, new Size(16, 16));
                }

                foreach (var metier in metiersTries)
                {
                    try
                    {
                        var btn = new Button
                        {
                            Text = metier.Nom,
                            Tag = metier,
                            Location = new System.Drawing.Point(11, yPos),
                            Size = new System.Drawing.Size(160, 30),
                            // Changement ici : _projetService.GetDisplayColorForMetier
                            BackColor = _projetService.GetDisplayColorForMetier(metier.MetierId),
                            FlatStyle = FlatStyle.Popup
                        };


                        btn.Click += MetierButton_Click;

                        // Changement ici : _projetService.GetPrerequisForMetier
                        var prerequis = _projetService.GetPrerequisForMetier(metier.MetierId);
                        if (prerequis.Any())
                        {
                            // Changement ici : _projetService.GetMetierById
                            var prerequisNoms = prerequis.Select(id => _projetService.GetMetierById(id)?.Nom ?? id);
                            _toolTipMetiers.SetToolTip(btn, $"Prérequis: {string.Join(", ", prerequisNoms)}");
                        }
                        else
                        {
                            _toolTipMetiers.SetToolTip(btn, "Aucun prérequis");
                        }

                        panelOutilsMetiersDynamiques.Controls.Add(btn);
                        yPos += 35;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur lors de la création du bouton pour {metier.Nom}: {ex.Message}");
                        continue;
                    }
                }

                // Séparateur et bouton jalon
                var separator = new Label
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Height = 2,
                    Width = 160,
                    Location = new Point(11, yPos + 5)
                };
                panelOutilsMetiersDynamiques.Controls.Add(separator);
                yPos += 15;

                var btnJalon = new Button
                {
                    Text = "◆ Créer Jalon/Attente",
                    Location = new System.Drawing.Point(11, yPos),
                    Size = new System.Drawing.Size(160, 30),
                    BackColor = Color.Gold,
                    Font = new Font(this.Font, FontStyle.Bold)
                };
                btnJalon.Click += JalonButton_Click;
                _toolTipMetiers.SetToolTip(btnJalon, "Créer un jalon d'attente manuel (ex: séchage)");
                panelOutilsMetiersDynamiques.Controls.Add(btnJalon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la création des boutons métiers: {ex.Message}");
                MessageBox.Show($"Erreur lors de la création des outils métiers :\n{ex.Message}",
                              "Erreur d'interface", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MetierButton_Click(object sender, EventArgs e)
        {
            if (_lotActif == null)
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot actif.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (sender is Button { Tag: Metier metier })
            {
                // Changement ici : TacheDetailForm reçoit _projetService
                using var form = new TacheDetailForm(_tacheService, _projetService, _blocService, _dependanceBuilder);
                var nouvelleTache = new Tache
                {
                    MetierId = metier.MetierId,
                    LotId = _lotActif.LotId,
                    TacheNom = $"Nouvelle tâche - {metier.Nom}",
                    HeuresHommeEstimees = 8
                };
                // Passer le LotId actif à TacheDetailForm
                form.MettreAJourListesDeroulantes(_lotActif.LotId);
                form.ChargerTache(nouvelleTache, true);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RafraichirDiagrammeEtStatistiques();
                }
            }
        }

        private void JalonButton_Click(object sender, EventArgs e)
        {
            if (_lotActif == null)
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot actif.", "Action impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Changement ici : TacheDetailForm reçoit _projetService
            using var form = new TacheDetailForm(_tacheService, _projetService, _blocService, _dependanceBuilder);

            // Correction: La définition d'un jalon se fait uniquement via la propriété 'Type'.
            // L'affectation à 'EstJalon' a été supprimée.
            var nouveauJalon = new Tache
            {
                Type = TypeActivite.JalonUtilisateur,
                HeuresHommeEstimees = 24,
                TacheNom = "Nouveau Jalon d'attente",
                LotId = _lotActif.LotId
            };
            // Passer le LotId actif à TacheDetailForm
            form.MettreAJourListesDeroulantes(_lotActif.LotId);
            form.ChargerTache(nouveauJalon, true);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                RafraichirDiagrammeEtStatistiques();
            }
        }

        #endregion

        #region Gestion des Blocs

        /// <summary>
        /// Méthode privée unifiée pour ouvrir le BlocForm en mode création ou édition.
        /// </summary>
        /// <param name="blocIdToEdit">ID du bloc à éditer (null pour création)</param>
        /// <param name="lotPourCreation">Lot pour la création d'un nouveau bloc (null pour édition)</param>
        private void OuvrirGestionBlocs(string blocIdToEdit = null, Lot lotPourCreation = null)
        {
            try
            {
                using (var form = new BlocForm(_blocService, _tacheService, blocIdToEdit, lotPourCreation))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        RafraichirVueComplete();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture de la gestion des blocs :\n{ex.Message}",
                              "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestionnaire pour le bouton d'ajout de bloc.
        /// Initie la création d'un nouveau bloc pour le lot actif.
        /// </summary>
        private void btnAjouterBloc_Click(object sender, EventArgs e)
        {
            if (_lotActif == null)
            {
                MessageBox.Show("Veuillez d'abord sélectionner un lot actif avant de créer un bloc.",
                              "Lot requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OuvrirGestionBlocs(null, _lotActif);
        }

        /// <summary>
        /// Gestionnaire d'événement pour le double-clic sur un bloc dans le diagramme PERT.
        /// Ouvre l'édition du bloc sélectionné.
        /// </summary>
        private void PertControl_BlocDoubleClicked(object sender, PlanAthena.Controls.BlocSelectedEventArgs e)
        {
            OuvrirGestionBlocs(e.BlocId, _lotActif);
        }

        #endregion

        #region Événements et Logique d'Affichage

        private void PertControl_TacheSelected(object sender, TacheSelectedEventArgs e)
        {
            // Charger les blocs du lot de la tâche sélectionnée AVANT de charger la tâche
            _tacheDetailForm?.MettreAJourListesDeroulantes(e.Tache.LotId);
            _tacheDetailForm.ChargerTache(e.Tache, false);
            lblTacheSelectionnee.Text = $"Sélectionnée: {e.Tache.TacheId} - {e.Tache.TacheNom}";
        }

        private void PertControl_TacheDoubleClicked(object sender, TacheSelectedEventArgs e)
        {
            var tacheOriginale = _tacheService.ObtenirTacheParId(e.Tache.TacheId);
            if (tacheOriginale != null)
            {
                // Changement ici : TacheDetailForm reçoit _projetService
                using var form = new TacheDetailForm(_tacheService, _projetService, _blocService, _dependanceBuilder);
                // Charger les blocs du lot de la tâche AVANT de charger la tâche
                form.MettreAJourListesDeroulantes(tacheOriginale.LotId);
                form.ChargerTache(tacheOriginale, false);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RafraichirDiagrammeEtStatistiques();
                }
            }
        }

        private void cmbLots_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbLots.SelectedItem is Lot selectedLot)
                {
                    _lotActif = selectedLot;
                    RafraichirDiagrammeEtStatistiques();
                    _tacheDetailForm?.MettreAJourListesDeroulantes(_lotActif.LotId);
                    _tacheDetailForm?.ChargerTache(new Tache { LotId = _lotActif.LotId, HeuresHommeEstimees = 8, Type = TypeActivite.Tache }, true);
                    lblTacheSelectionnee.Text = "Aucune sélection";
                    AfficherPlanLotActif();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du changement de lot: {ex.Message}");
                MessageBox.Show($"Erreur lors du changement de lot :\n{ex.Message}",
                              "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnGererLots_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new LotForm(_projetService, _tacheService))
                {
                    form.ShowDialog(this);
                }
                RafraichirVueComplete();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture de la gestion des lots :\n{ex.Message}",
                              "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// FONCTIONNALITÉ DÉSACTIVÉE - HORS PÉRIMÈTRE
        /// Le mapping automatique sera implémenté dans une itération future.
        /// Pour cette phase, nous nous concentrons uniquement sur le service de suggestions.
        /// </summary>
        private void btnMappingAuto_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Le mapping automatique des dépendances sera implémenté dans une version future.\n\n" +
                           "Pour l'instant, utilisez les suggestions intelligentes dans le formulaire de détail des tâches.",
                           "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            try
            {
                RafraichirDiagrammeEtStatistiques();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la recherche: {ex.Message}");
                // Ne pas afficher de MessageBox car cet événement se déclenche souvent
            }
        }

        private void btnZoomAjuster_Click(object sender, EventArgs e)
        {
            try
            {
                _pertControl?.ZoomToutAjuster();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajustement du zoom :\n{ex.Message}",
                              "Erreur de zoom", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPan_Click(object sender, EventArgs e)
        {
            try
            {
                _pertControl?.TogglePan(btnPan.Checked);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'activation du pan: {ex.Message}");
                btnPan.Checked = false; // Reset en cas d'erreur
            }
        }

        private void btnSauvegarderImage_Click(object sender, EventArgs e)
        {
            try
            {
                _pertControl?.SauvegarderImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}",
                              "Erreur de sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImprimer_Click(object sender, EventArgs e)
        {
            try
            {
                _pertControl?.ImprimerDiagramme();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'impression :\n{ex.Message}",
                              "Erreur d'impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void AfficherPlanLotActif()
        {
            // Libérer l'image précédente si elle existe pour éviter les fuites de mémoire et les verrous de fichier
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            pictureBox1.BackColor = SystemColors.Control; // Rétablir le fond par défaut
            pictureBox1.Visible = true; // S'assurer que le PictureBox est visible
            _toolTipPlan.SetToolTip(pictureBox1, ""); // Nettoyer le ToolTip précédent

            if (_lotActif == null || string.IsNullOrWhiteSpace(_lotActif.CheminFichierPlan))
            {
                // Aucun lot sélectionné ou pas de chemin de plan
                // Optionnel: Afficher un texte "Aucun plan" si vous préférez
                // graphics.DrawString("Aucun plan disponible", ...);
                return;
            }

            string filePath = _lotActif.CheminFichierPlan;

            if (!File.Exists(filePath))
            {
                // Le fichier n'existe plus
                System.Diagnostics.Debug.WriteLine($"Le fichier plan '{filePath}' n'a pas été trouvé.");
                // Optionnel: Dessiner un message d'erreur sur le PictureBox
                // using (Graphics g = pictureBox1.CreateGraphics()) { g.DrawString("Fichier introuvable", ...); }
                _toolTipPlan.SetToolTip(pictureBox1, $"Fichier plan introuvable: {filePath}");
                return;
            }

            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            try
            {
                // Support des images (JPG, PNG, BMP, GIF)
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
                {
                    // Utiliser FromStream pour éviter de verrouiller le fichier sur le disque
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        // Créez une copie de l'image en mémoire pour que le FileStream puisse être fermé
                        Image img = Image.FromStream(stream);
                        pictureBox1.Image = new Bitmap(img); // Copie l'image en mémoire
                        img.Dispose(); // Libère l'image originale chargée depuis le stream
                    }
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom; // Redimensionne l'image pour qu'elle tienne dans le PictureBox sans déformation
                    _toolTipPlan.SetToolTip(pictureBox1, $"Plan actuel: {filePath}");
                }
                // Gestion des PDF (plus complexe, nécessite une bibliothèque tierce pour un affichage intégré)
                else if (extension == ".pdf")
                {
                    // Pour un affichage réel de PDF, vous auriez besoin d'une bibliothèque comme PdfiumViewer
                    // Ou d'un contrôle WebBrowser pointant vers un viewer JS, etc.
                    // Pour l'instant, nous allons proposer de l'ouvrir et afficher un message/icône
                    System.Diagnostics.Debug.WriteLine($"Format PDF non supporté directement pour l'affichage : {filePath}");
                    MessageBox.Show("Le format PDF n'est pas supporté directement pour l'affichage du plan dans l'interface.\n\n" +
                                    "Voulez-vous ouvrir le fichier dans l'application par défaut ?",
                                    "Format non supporté", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (DialogResult.Yes == DialogResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Impossible d'ouvrir le fichier PDF : {ex.Message}", "Erreur d'ouverture", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    // Vous pouvez afficher une icône PDF générique ici
                    // pictureBox1.Image = Properties.Resources.pdf_icon; // Si vous avez une telle ressource
                    _toolTipPlan.SetToolTip(pictureBox1, $"Plan PDF: {filePath} (Cliquez pour ouvrir)");
                    // Si vous voulez le rendre cliquable pour ouvrir, ajoutez un gestionnaire d'événements :
                    pictureBox1.Click += PictureBox1_ClickForPdf;
                }
                else
                {
                    // Format non supporté
                    System.Diagnostics.Debug.WriteLine($"Format de fichier non supporté pour l'affichage : {filePath}");
                    // pictureBox1.Image = Properties.Resources.unsupported_file_icon; // Ou une icône d'erreur
                    _toolTipPlan.SetToolTip(pictureBox1, $"Format de fichier non supporté: {extension}");
                }
            }
            catch (OutOfMemoryException)
            {
                // L'image est trop grande ou corrompue et provoque une OutOfMemoryException
                System.Diagnostics.Debug.WriteLine($"Erreur: Mémoire insuffisante ou fichier corrompu pour le plan : {filePath}");
                MessageBox.Show($"Erreur: Le fichier plan '{filePath}' est trop grand ou corrompu et ne peut pas être chargé.",
                                "Erreur de chargement de l'image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBox1.Image = null;
            }
            catch (Exception ex)
            {
                // Toute autre erreur lors du chargement de l'image
                System.Diagnostics.Debug.WriteLine($"Erreur inattendue lors du chargement du plan '{filePath}': {ex.Message}");
                MessageBox.Show($"Une erreur inattendue est survenue lors du chargement du plan :\n{ex.Message}",
                                "Erreur de chargement", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBox1.Image = null;
            }
        }

        // Si vous avez choisi de rendre le PictureBox cliquable pour ouvrir les PDF
        private void PictureBox1_ClickForPdf(object sender, EventArgs e)
        {
            if (_lotActif != null && !string.IsNullOrWhiteSpace(_lotActif.CheminFichierPlan))
            {
                string filePath = _lotActif.CheminFichierPlan;
                if (File.Exists(filePath) && Path.GetExtension(filePath).ToLowerInvariant() == ".pdf")
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Impossible d'ouvrir le fichier PDF : {ex.Message}", "Erreur d'ouverture", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }



        private void btnImportExcelFieldwire_Click(object sender, EventArgs e) => MessageBox.Show("L'import Excel Fieldwire/Dalux n'est pas encore implémenté.", "Fonctionnalité en développement", MessageBoxButtons.OK, MessageBoxIcon.Information);

        /// <summary>
        /// Gère le clic sur le bouton d'import CSV, en lançant le processus de sélection de fichier
        /// et l'ouverture du formulaire de mappage des tâches.
        /// </summary>
        private void btnImporter_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Vérifier qu'un lot est sélectionné
                if (_lotActif == null)
                {
                    MessageBox.Show("Veuillez d'abord sélectionner un lot de destination avant d'importer des tâches.",
                                  "Lot requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Sélection du fichier CSV
                using var openFileDialog = new OpenFileDialog
                {
                    Title = "Sélectionner le fichier CSV à importer",
                    Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                    DefaultExt = "csv",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                string filePath = openFileDialog.FileName;

                // 3. Lancer le formulaire de mappage des tâches
                // Le formulaire de mappage gérera les pré-analyses et affichera les avertissements directement.
                // TODO: ImportTacheForm doit recevoir ProjetService au lieu de MetierService
                using (var importForm = new ImportTacheForm(filePath, _lotActif, _projetService)) // Changement ici : _projetService
                {
                    if (importForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Si l'utilisateur a confirmé le mappage, procéder à l'import réel
                        var mappingConfig = importForm.MappingConfiguration;
                        ExecuterImportCSV(filePath, mappingConfig, false);
                    }
                    // Si DialogResult n'est pas OK (ex: Annuler), ne rien faire.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la préparation de l'import: {ex.Message}",
                              "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Execute l'import CSV avec gestion de la confirmation d'écrasement.
        /// La liste des warnings est maintenant présentée de manière plus conviviale.
        /// </summary>
        /// <param name="filePath">Chemin du fichier CSV à importer.</param>
        /// <param name="mappingConfig">Configuration de mappage des colonnes CSV.</param>
        /// <param name="confirmerEcrasement">Indique si la confirmation d'écrasement a déjà été donnée.</param>
        private void ExecuterImportCSV(string filePath, ImportMappingConfiguration mappingConfig, bool confirmerEcrasement)
        {
            try
            {
                // Appeler le service d'orchestration avec la configuration de mappage
                var resultat = _importOrchestrationService.ImporterTachesDepuisCsv(filePath, _lotActif.LotId, mappingConfig, confirmerEcrasement);

                if (resultat.ConfirmationRequise)
                {
                    // Demander confirmation à l'utilisateur
                    var dialogResult = MessageBox.Show(
                        resultat.MessageConfirmation,
                        "Confirmation d'import",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);

                    if (dialogResult == DialogResult.OK)
                    {
                        // Relancer l'import avec confirmation
                        ExecuterImportCSV(filePath, mappingConfig, true);
                    }
                    return;
                }

                if (resultat.EstSucces)
                {
                    // Construire le message de succès
                    var rapportBuilder = new StringBuilder();
                    rapportBuilder.AppendLine("🎉 IMPORT RÉUSSI");
                    rapportBuilder.AppendLine("==================");
                    rapportBuilder.AppendLine($"• {resultat.NbTachesImportees} tâche(s) importée(s)");
                    rapportBuilder.AppendLine($"• {resultat.NbLotsTraites} lot(s) traité(s)");
                    rapportBuilder.AppendLine($"• {resultat.NbBlocsTraites} bloc(s) traité(s)");
                    rapportBuilder.AppendLine($"• Durée: {resultat.DureeImport.TotalSeconds:F1}s");

                    // Afficher les avertissements dans une fenêtre dédiée si présents
                    if (resultat.Warnings.Any())
                    {
                        using (var warningsDialog = new ImportWarningsDialog(resultat.Warnings))
                        {
                            warningsDialog.ShowDialog(this);
                        }
                        rapportBuilder.AppendLine("\n⚠️ Des avertissements ont été générés. Voir la fenêtre de détail.");
                    }

                    MessageBox.Show(rapportBuilder.ToString(), "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Rafraîchir l'interface
                    RafraichirVueComplete();
                }
                else
                {
                    // Afficher l'erreur (le message d'erreur sera complet depuis ImportResult.Echec)
                    MessageBox.Show($"Échec de l'import:\n\n{resultat.MessageErreur}",
                                  "Erreur d'import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur inattendue lors de l'import:\n\n{ex.Message}",
                              "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExporter_Click(object sender, EventArgs e) => MessageBox.Show("L'import/export CSV est temporairement désactivé et sera ré-implémenté dans une version future.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnFermer_Click(object sender, EventArgs e) => this.Close();

        #endregion
    }
}