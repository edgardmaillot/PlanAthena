using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using PlanAthena.Data; // Pour Metier
using PlanAthena.Services.Business; // Pour ProjetService
using PlanAthena.Utilities; // Pour DependanceBuilder
using PlanAthena.View.Ressources.MetierDiagram;
using PlanAthena.View.TaskManager.PertDiagram;
using System;
using System.Collections.Generic;
using System.Drawing.Printing; // Pour PrintDocument, Bitmap
using System.Linq; // Pour LINQ
using System.Reflection; // Pour MethodInfo (SauvegarderImage)
using System.Windows.Forms; // Pour UserControl, Point, ToolTip, MessageBox
using DrawingNode = Microsoft.Msagl.Drawing.Node; // Alias pour éviter les conflits

namespace PlanAthena.View.Ressources.MetierDiagram
{
    
    /// <summary>
    /// Arguments d'événement pour la sélection d'un métier via clic.
    /// </summary>
    public class MetierSelectedEventArgs : EventArgs
    {
        public Metier SelectedMetier { get; }

        public MetierSelectedEventArgs(Metier metier)
        {
            SelectedMetier = metier;
        }
    }

    /// <summary>
    /// Contrôle utilisateur pour afficher un diagramme PERT des métiers et de leurs prérequis.
    /// Basé sur une adaptation de PertDiagramControl pour les tâches.
    /// </summary>
    public partial class MetierDiagramControl : UserControl
    {

        /// <summary>
        /// 🆕 V0.4.2 : Phase de chantier que ce diagramme représente
        /// </summary>
        public ChantierPhase? PhaseActuelle { get; private set; }


        #region Champs et Dépendances

        private readonly GViewer _viewer;
        private Graph _graph; // Le graphe MSAGL des métiers

        private MetierDiagramSettings _settings; // Paramètres de style spécifiques aux métiers
        private MetierNodeBuilder _nodeBuilder; // Builder pour créer les nœuds de métier
        private ProjetService _projetService; // Service principal pour accéder aux données des métiers
        private RessourceService _ressourceService; // Service pour récupérer les prérequis et autres données métier
        private DependanceBuilder _dependanceBuilder; // Pour la validation des dépendances métier (non utilisé directement dans le diagramme, mais peut être utile pour des futures fonctionnalités d'interaction)

        private List<Metier> _metiers = new List<Metier>(); // La liste des métiers actuellement affichée

        private bool _isPanning = false; // Indicateur pour le déplacement du diagramme
        private Point _panStartPoint; // Point de départ du glisser-déposer pour le déplacement
        private PrintDocument _printDocument; // Pour l'impression du diagramme
        public Metier MetierSelectionne { get; private set; } // Propriété publique pour le métier actuellement sélectionné dans le diagramme

        // Timer pour surveiller le zoom et déclencher l'événement ZoomChanged
        private System.Windows.Forms.Timer _zoomMonitorTimer;
        private double _lastKnownZoom = 1.0;

        #endregion

        #region Événements Publics

        // Événement déclenché lorsqu'un métier est sélectionné (clic sur un nœud)
        public event EventHandler<MetierSelectedEventArgs> MetierSelected;
        // Événement déclenché lorsque le zoom du diagramme change
        public event EventHandler<ZoomChangedEventArgs> ZoomChanged;

        #endregion

        #region Propriétés Publiques

        /// <summary>
        /// Obtient le facteur de zoom actuel du diagramme.
        /// </summary>
        public double ZoomFacteur => _viewer?.ZoomF ?? 1.0;

        #endregion

        #region Initialisation

        /// <summary>
        /// Constructeur par défaut du contrôle MetierDiagramControl.
        /// Initialise le GViewer et l'ajoute au contrôle.
        /// </summary>
        public MetierDiagramControl()
        {
            InitializeComponent();
            _viewer = new GViewer { Dock = DockStyle.Fill };
            Controls.Add(_viewer);
            this.Resize += MetierDiagramControl_Resize;
        }
        private void MetierDiagramControl_Resize(object sender, EventArgs e)
        {
            // Appelez la méthode qui sait comment tout ré-afficher correctement.
            // D'après votre code, il semble que ce soit ZoomToutAjuster().
            ZoomToutAjuster();
        }

        /// <summary>
        /// Initialise le contrôle avec les services et paramètres nécessaires.
        /// </summary>
        /// <param name="projetService">Le service de gestion du projet (pour les métiers).</param>
        /// <param name="dependanceBuilder">Le service de construction de dépendances (pour les validations).</param>
        /// <param name="settings">Les paramètres de configuration visuelle du diagramme.</param>
        public void Initialize(ProjetService projetService,RessourceService ressourceService, DependanceBuilder dependanceBuilder, MetierDiagramSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
            // Le MetierNodeBuilder n'a besoin que des settings pour son initialisation de style
            _nodeBuilder = new MetierNodeBuilder(settings);

            ConfigurerViewer();

            // Abonnement aux événements de souris du viewer pour la sélection et le déplacement
            _viewer.MouseClick += Viewer_MouseClick;
            _viewer.MouseDown += Viewer_MouseDown;
            _viewer.MouseMove += Viewer_MouseMove;
            _viewer.MouseUp += Viewer_MouseUp;
            // Le double-clic n'est pas utilisé pour l'édition des métiers (simple clic suffit)
        }

        /// <summary>
        /// Configure les propriétés de base du GViewer.
        /// </summary>
        private void ConfigurerViewer()
        {
            _viewer.PanButtonPressed = false; // Désactive le bouton de pan par défaut du viewer
            _viewer.NavigationVisible = false; // Cache la barre de navigation
            _viewer.ToolBarIsVisible = false; // Cache la barre d'outils
            _viewer.ZoomF = _settings.DefaultZoom; // Définit le zoom initial
            _viewer.OutsideAreaBrush = new SolidBrush(_settings.OutsideAreaColor); // Définit la couleur de fond

            InitialiserMonitoringZoom(); // Active le monitoring du zoom

            // Ajoute une infobulle pour guider l'utilisateur
            var tooltip = new ToolTip();
            tooltip.SetToolTip(_viewer, "Clic sur métier = Sélection | Clic sur fond = Déplacement vue");
        }

        /// <summary>
        /// Initialise et démarre un timer pour surveiller les changements de zoom.
        /// </summary>
        private void InitialiserMonitoringZoom()
        {
            _zoomMonitorTimer = new System.Windows.Forms.Timer();
            _zoomMonitorTimer.Interval = 100; // Vérifie le zoom toutes les 100 ms
            _zoomMonitorTimer.Tick += ZoomMonitorTimer_Tick;
            _zoomMonitorTimer.Start();

            _lastKnownZoom = _viewer.ZoomF; // Enregistre le zoom initial
        }

        /// <summary>
        /// Gère le tick du timer de monitoring du zoom.
        /// </summary>
        private void ZoomMonitorTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_viewer == null) return;

                var currentZoom = _viewer.ZoomF;

                // Si le zoom a changé significativement, déclenche l'événement
                if (Math.Abs(currentZoom - _lastKnownZoom) > 0.01)
                {
                    _lastKnownZoom = currentZoom;
                    ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(currentZoom));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du monitoring du zoom: {ex.Message}");
            }
        }

        /// <summary>
        /// Libère les ressources utilisées par le contrôle.
        /// </summary>
        /// <param name="disposing">True si les ressources managées doivent être libérées.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _zoomMonitorTimer?.Stop();
                _zoomMonitorTimer?.Dispose();
                _zoomMonitorTimer = null;
                _printDocument?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Méthodes Publiques

        /// <summary>
        /// Charge les données des métiers à afficher dans le diagramme et regénère celui-ci.
        /// </summary>
        /// <param name="metiers">La liste des métiers à afficher.</param>
        public void ChargerDonnees(List<Metier> metiers, ChantierPhase? phase = null)
        {
            _metiers = metiers ?? new List<Metier>();
            PhaseActuelle = phase; // 🆕 Stocker la phase
            GenererDiagramme();
        }

        /// <summary>
        /// Active ou désactive le mode de déplacement du diagramme (panoramique).
        /// </summary>
        /// <param name="panActif">True pour activer le pan, False pour le désactiver.</param>
        public void TogglePan(bool panActif)
        {
            _viewer.PanButtonPressed = panActif;
            Cursor = panActif ? Cursors.Hand : Cursors.Default;
        }

        /// <summary>
        /// Sauvegarde une image du diagramme actuel.
        /// </summary>
        public void SauvegarderImage()
        {
            if (_viewer?.Graph == null)
            {
                MessageBox.Show("Aucun diagramme à sauvegarder.", "Information",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Utilise la méthode interne de GViewer via réflexion pour la sauvegarde
                MethodInfo mi = typeof(GViewer).GetMethod("SaveImageClick", BindingFlags.NonPublic | BindingFlags.Instance);
                mi?.Invoke(_viewer, new object[] { this, EventArgs.Empty });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Imprime le diagramme actuel.
        /// </summary>
        public void ImprimerDiagramme()
        {
            if (_viewer?.Graph == null)
            {
                MessageBox.Show("Aucun diagramme à imprimer.", "Information",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_printDocument == null)
                {
                    _printDocument = new PrintDocument();
                    _printDocument.DefaultPageSettings.Landscape = true; // Imprimer en mode paysage
                    _printDocument.PrintPage += (sender, e) =>
                    {
                        try
                        {
                            // Rend le graphe sur un bitmap et le dessine sur la page
                            var bmp = new Bitmap(e.PageBounds.Width, e.PageBounds.Height);
                            var renderer = new GraphRenderer(_viewer.Graph);
                            renderer.Render(bmp);
                            e.Graphics.DrawImage(bmp, e.PageBounds);
                            bmp.Dispose();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erreur lors du rendu de l'impression: {ex.Message}");
                        }
                    };
                }

                using (var printDialog = new PrintDialog { Document = _printDocument })
                {
                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        _printDocument.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'impression:\n{ex.Message}", "Erreur d'impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ajuste le zoom du diagramme pour qu'il s'adapte au contenu du viewer.
        /// </summary>
        public void ZoomToutAjuster()
        {
            try
            {
                if (_viewer?.Graph == null) return;

                // Demande au viewer de zoomer pour adapter tout le graphe
                _viewer.ZoomF = 1.0; // Réinitialise le zoom pour un recalcul complet
                _viewer.Graph = _graph; // Force le recalcul du layout et du zoom optimal
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajustement du zoom: {ex.Message}");
            }
        }

        /// <summary>
        /// Force la sélection visuelle d'un nœud de métier dans le diagramme.
        /// Utile pour re-sélectionner un nœud après une action externe (sauvegarde, annulation).
        /// </summary>
        /// <param name="metierId">L'ID du métier (et du nœud) à sélectionner. Si null ou vide, désélectionne tout.</param>
        public void ForceSelection(string metierId)
        {
            if (_graph == null) return;

            // Réinitialise la mise en évidence de tous les nœuds et arêtes avant de potentiellement en sélectionner un nouveau
            DeselectAllNodes();

            if (!string.IsNullOrEmpty(metierId))
            {
                var nodeToSelect = _graph.FindNode(metierId);
                if (nodeToSelect != null)
                {
                    HighlightNode(nodeToSelect); // Applique la mise en évidence
                    MetierSelectionne = nodeToSelect.UserData as Metier; // Met à jour la propriété interne
                }
                else
                {
                    MetierSelectionne = null; // Le nœud n'a pas été trouvé
                }
            }
            else
            {
                MetierSelectionne = null; // Aucun ID fourni, pas de sélection
            }
            _viewer.Invalidate(); // Demande un rafraîchissement visuel pour la nouvelle mise en évidence
        }

        #endregion

        #region Génération du Diagramme

        /// <summary>
        /// Génère le graphe MSAGL des métiers et de leurs prérequis.
        /// </summary>
        private void GenererDiagramme()
        {
            try
            {
                if (_nodeBuilder == null) return;

                _graph = new Graph("DiagrammeMetiers");
                ConfigurerLayoutOptimal(_graph);

                if (!_metiers.Any())
                {
                    CreerNoeudInformation("Aucun métier à afficher.");
                }
                else
                {
                    // Ajouter les nœuds pour chaque métier
                    foreach (var metier in _metiers.OrderBy(m => m.Nom))
                    {
                        var node = _nodeBuilder.BuildNodeFromMetier(metier); // Utilise le MetierNodeBuilder
                        // Note : La couleur de remplissage est déjà définie dans MetierNodeBuilder.ApplyNodeStyle
                        _graph.AddNode(node);
                    }
                    // Ajouter les arêtes (dépendances entre métiers)
                    AjouterDependancesMetier();
                }

                _viewer.Graph = _graph; // Assigner le graphe au viewer pour affichage
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la génération du diagramme des métiers: {ex.Message}");
            }
        }

        /// <summary>
        /// Configure les paramètres de l'algorithme de layout pour le graphe.
        /// </summary>
        /// <param name="graph">Le graphe MSAGL.</param>
        private void ConfigurerLayoutOptimal(Graph graph)
        {
            graph.Attr.LayerDirection = _settings.LayoutDirection;
            graph.Attr.AspectRatio = _settings.LayoutAspectRatio;
            graph.Attr.NodeSeparation = _settings.LayoutNodeSeparation;
            graph.Attr.LayerSeparation = _settings.LayoutLayerSeparation;
            graph.Attr.MinNodeHeight = _settings.LayoutMinNodeHeight;
            graph.Attr.MinNodeWidth = _settings.LayoutMinNodeWidth;
            graph.Attr.Margin = _settings.LayoutMargin;

            // Paramètres spécifiques à l'algorithme Sugiyama
            graph.LayoutAlgorithmSettings = new SugiyamaLayoutSettings
            {
                RepetitionCoefficientForOrdering = _settings.SugiyamaRepetitionCoefficient,
                BrandesThreshold = _settings.SugiyamaBrandesThreshold,
                PackingMethod = _settings.SugiyamaPackingMethod,
                ClusterMargin = _settings.MetierPadding // Sera ignoré si pas de clusters utilisés
            };
        }

        /// <summary>
        /// Ajoute les arêtes (dépendances) entre les métiers dans le graphe.
        /// 🔧 CORRIGÉ : Évite le fallback défaillant de GetPrerequisMetier()
        /// </summary>
        private void AjouterDependancesMetier()
        {
            foreach (var metierCourant in _metiers)
            {
                // 🔧 CORRECTION : Accès direct sans fallback défaillant
                List<string> prerequisIds = new List<string>();

                if (PhaseActuelle.HasValue)
                {
                    // Cas normal : on demande les prérequis pour la phase spécifique du diagramme
                    prerequisIds = _ressourceService.GetPrerequisPourPhase(metierCourant.MetierId, PhaseActuelle.Value);
                }
                else
                {
                    // Cas de fallback (si un diagramme est affiché sans phase) : on affiche toutes les dépendances
                    prerequisIds = _ressourceService.GetTousPrerequisConfondus(metierCourant.MetierId);
                }

                //System.Diagnostics.Debug.WriteLine($"🔍 DIAGRAMME - Métier: {metierCourant.Nom}, Phase: {PhaseActuelle}");
                //System.Diagnostics.Debug.WriteLine($"  ✅ Prérequis: [{string.Join(", ", prerequisIds)}]");

                foreach (var prerequisId in prerequisIds)
                {
                    // Vérifier que le prérequis existe et est affiché dans le graphe
                    if (_metiers.Any(m => m.MetierId == prerequisId))
                    {
                        // Crée une arête du prérequis vers le métier dépendant
                        var edge = _graph.AddEdge(prerequisId, metierCourant.MetierId);
                        edge.Attr.Color = _settings.EdgeDefaultColor;
                        edge.Attr.LineWidth = _settings.EdgeDefaultWidth;
                        edge.Attr.ArrowheadAtTarget = _settings.EdgeArrowStyle;

                        //System.Diagnostics.Debug.WriteLine($"    ➡️ Flèche: {prerequisId} → {metierCourant.MetierId}");
                    }
                }
            }
        }

        /// <summary>
        /// Crée un nœud d'information dans le graphe (utilisé quand aucun métier n'est à afficher).
        /// </summary>
        /// <param name="message">Le message à afficher dans le nœud.</param>
        private void CreerNoeudInformation(string message)
        {
            var node = _graph.AddNode("info");
            node.LabelText = message;
            node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightGray;
            node.Attr.Shape = Shape.Box;
            node.Label.FontSize = 14;
        }

        #endregion

        #region Gestion des Événements Souris & Highlighting

        /// <summary>
        /// Gère l'événement MouseDown pour activer le mode de déplacement (panoramique).
        /// </summary>
        private void Viewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            // Active le pan si l'objet sous la souris n'est pas un nœud (clic sur le fond)
            _viewer.PanButtonPressed = _viewer.ObjectUnderMouseCursor?.DrawingObject is not DrawingNode;
            if (_viewer.PanButtonPressed)
            {
                _isPanning = true;
                _panStartPoint = e.Location;
            }
        }

        /// <summary>
        /// Gère l'événement MouseMove (laisse vide car le pan est géré par GViewer).
        /// </summary>
        private void Viewer_MouseMove(object sender, MouseEventArgs e) { }

        /// <summary>
        /// Gère l'événement MouseUp pour désactiver le mode de déplacement.
        /// </summary>
        private void Viewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left || !_isPanning) return;
            _isPanning = false;
            _viewer.PanButtonPressed = false;
        }

        /// <summary>
        /// Gère l'événement MouseClick pour la sélection des nœuds de métier.
        /// </summary>
        private void Viewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left || _isPanning) return;
            var objectUnderMouse = _viewer.ObjectUnderMouseCursor;

            // Si l'objet cliqué est un nœud et que son UserData est un Metier
            if (objectUnderMouse?.DrawingObject is DrawingNode selectedNode && selectedNode.UserData is Metier metier)
            {
                MetierSelectionne = metier; // Met à jour la propriété interne du contrôle
                MetierSelected?.Invoke(this, new MetierSelectedEventArgs(metier)); // Déclenche l'événement public
                HighlightNode(selectedNode); // Applique la mise en évidence visuelle
            }
            else
            {
                // Clic sur le fond ou un autre objet non-métier (info node, etc.), désélectionne tout
                MetierSelectionne = null;
                HighlightNode(null); // Désélectionne visuellement tous les nœuds
            }
        }

        // Le double-clic n'est pas utilisé pour les métiers (l'édition se fait au simple clic sur ce formulaire)
        private void Viewer_MouseDoubleClick(object sender, MouseEventArgs e) { }

        /// <summary>
        /// Applique la mise en évidence visuelle à un nœud spécifique et réinitialise les autres.
        /// </summary>
        /// <param name="nodeToHighlight">Le nœud MSAGL à mettre en évidence, ou null pour désélectionner tout.</param>
        private void HighlightNode(DrawingNode nodeToHighlight)
        {
            if (_graph == null || !_graph.Nodes.Any()) return;

            // Réinitialiser le style de tous les nœuds et arêtes avant d'appliquer une nouvelle mise en évidence
            DeselectAllNodes();

            // Appliquer la mise en évidence sur le nœud cible et ses arêtes adjacentes
            if (nodeToHighlight != null)
            {
                nodeToHighlight.Attr.LineWidth = _settings.HighlightLineWidth;
                nodeToHighlight.Attr.Color = _settings.HighlightBorderColor;

                foreach (var edge in _graph.Edges)
                {
                    if (edge.Source == nodeToHighlight.Id || edge.Target == nodeToHighlight.Id)
                    {
                        edge.Attr.LineWidth = _settings.HighlightEdgeWidth;
                        edge.Attr.Color = _settings.HighlightEdgeColor;
                    }
                }
            }
            _viewer.Invalidate(); // Demande un rafraîchissement de l'affichage du viewer
        }

        /// <summary>
        /// Réinitialise le style visuel de tous les nœuds et arêtes à leur état par défaut (désélectionné).
        /// </summary>
        private void DeselectAllNodes()
        {
            if (_graph == null || !_graph.Nodes.Any()) return;

            foreach (var node in _graph.Nodes)
            {
                // Applique le style par défaut via le node builder, en passant l'objet métier d'origine
                _nodeBuilder.ApplyNodeStyle(node, node.UserData as Metier);
            }
            foreach (var edge in _graph.Edges)
            {
                edge.Attr.Color = _settings.EdgeDefaultColor;
                edge.Attr.LineWidth = _settings.EdgeDefaultWidth;
            }
        }

        #endregion
    }
}