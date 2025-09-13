// Fichier: PlanAthena/View/TaskManager/PertDiagram/PertDiagramControl.cs
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System.Drawing.Printing;
using System.Reflection;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace PlanAthena.View.TaskManager.PertDiagram
{
    public partial class PertDiagramControl : UserControl
    {
        private readonly GViewer _viewer;
        private Graph _graph;
        private PertDiagramSettings _settings;
        private PertNodeBuilder _nodeBuilder;
        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private DependanceBuilder _dependanceBuilder;
        private List<Tache> _taches = new List<Tache>();

        // Zoom management
        public double ZoomFacteur => _viewer?.ZoomF ?? 1.0;
        private double _lastKnownZoomFactor = 1.0;

        // Tooltip management 
        private System.Windows.Forms.Timer tooltipTimer;
        private Node _hoveredNode;

        // Logique de Pan 
        private bool _isPanning = false;

        private PrintDocument _printDocument;

        #region Événements Publics

        public event EventHandler<TacheSelectedEventArgs> TacheClick;
        public event EventHandler<BlocSelectedEventArgs> BlocClick;
        public event EventHandler<TacheSelectedEventArgs> TacheDoubleClick;
        public event EventHandler<BlocSelectedEventArgs> BlocDoubleClick;
        public event EventHandler<EventArgs> EmptyAreaClick;
        public event EventHandler<ZoomChangedEventArgs> ZoomChanged;

        #endregion

        

        public PertDiagramControl()
        {
            InitializeComponent();
            _viewer = new GViewer { Dock = DockStyle.Fill };
            Controls.Add(_viewer);
            InitializeTooltipHandling();

        }

        public void Initialize(ProjetService projetService, RessourceService ressourceService, DependanceBuilder dependanceBuilder, PertDiagramSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
            _nodeBuilder = new PertNodeBuilder(settings, _ressourceService);

            ConfigurerViewer();
        }

        private void ConfigurerViewer()
        {
            _viewer.PanButtonPressed = false;
            _viewer.NavigationVisible = false;
            _viewer.ToolBarIsVisible = false;
            _viewer.ZoomF = _settings.DefaultZoom;
            _viewer.OutsideAreaBrush = new SolidBrush(_settings.OutsideAreaColor);

            // Attachement des gestionnaires d'événements souris
            _viewer.MouseClick += Viewer_MouseClick;
            _viewer.MouseDoubleClick += Viewer_MouseDoubleClick;
            _viewer.MouseDown += Viewer_MouseDown;
            _viewer.MouseUp += Viewer_MouseUp;
            _viewer.ViewChangeEvent += Viewer_ViewChangeEvent;
            _viewer.MouseWheel += Viewer_MouseWheel;
            _lastKnownZoomFactor = _viewer.ZoomF;
        }

        public void ChargerDonnees(List<Tache> taches, string filtreRecherche = "")
        {
            _taches = taches ?? new List<Tache>();
            GenererDiagramme(filtreRecherche);
        }

        #region Gestionnaires d'Événements
        private void Viewer_ViewChangeEvent(object sender, EventArgs e)
        {
            // Cet événement est déclenché par les changements programmatiques.
            NotifyIfZoomChanged();
        }

        private void Viewer_MouseWheel(object sender, MouseEventArgs e)
        {
            // Le GViewer a déjà traité le zoom de la molette en interne.
            // Notre seule responsabilité est de notifier le reste de l'application
            // que le zoom a changé.
            NotifyIfZoomChanged();
        }

        private void Viewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

            // On active le mode PAN interne de GViewer si on ne clique PAS sur un objet (noeud, cluster, etc.)
            _viewer.PanButtonPressed = (_viewer.ObjectUnderMouseCursor == null);

            // On utilise notre flag pour savoir si un déplacement a commencé
            _isPanning = _viewer.PanButtonPressed;
        }

        private void Viewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

            // On désactive le mode PAN à la fin du clic
            _viewer.PanButtonPressed = false;

            // Si on était en train de "panner", on remet le flag à false.
            // On utilise un petit délai pour éviter que l'événement Click ne se déclenche par erreur juste après un déplacement.
            if (_isPanning)
            {
                var timer = new System.Windows.Forms.Timer { Interval = 50 };
                timer.Tick += (s, args) =>
                {
                    _isPanning = false;
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        private void Viewer_MouseClick(object sender, MouseEventArgs e)
        {
            // On ne traite le clic que si ce n'est pas la fin d'un déplacement
            if (e.Button != System.Windows.Forms.MouseButtons.Left || _isPanning) return;
            var objectUnderMouse = _viewer.ObjectUnderMouseCursor;

            if (objectUnderMouse == null)
            {
                ClearAllHighlights();
                _viewer.Invalidate();
                EmptyAreaClick?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (objectUnderMouse.DrawingObject is DrawingNode node && node.UserData is Tache tache)
            {
                TacheClick?.Invoke(this, new TacheSelectedEventArgs(tache, TacheInteractionType.SingleClick));
                MettreEnEvidenceTache(node);
            }
            else if (objectUnderMouse.DrawingObject is Subgraph subgraph)
            {
                BlocClick?.Invoke(this, new BlocSelectedEventArgs(subgraph.Id));
                MettreEnEvidenceBloc(subgraph);
            }
        }

        private void Viewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var objectUnderMouse = _viewer.ObjectUnderMouseCursor;
            if (objectUnderMouse == null) return;

            if (objectUnderMouse.DrawingObject is DrawingNode node && node.UserData is Tache tache)
            {
                TacheClick?.Invoke(this, new TacheSelectedEventArgs(tache, TacheInteractionType.DoubleClick));
            }
            else if (objectUnderMouse.DrawingObject is Subgraph subgraph)
            {
                BlocDoubleClick?.Invoke(this, new BlocSelectedEventArgs(subgraph.Id));
            }
        }

        #endregion

        #region Logique de Mise en Évidence (Highlighting)

        private void ClearAllHighlights()
        {
            if (_graph == null) return;

            foreach (var node in _graph.Nodes)
            {
                if (node.UserData is Tache t) { _nodeBuilder.ApplyNodeStyle(node, t); }
            }
            foreach (var subgraph in _graph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf())
            {
                subgraph.Attr.Color = _settings.ClusterBorderColor;
                subgraph.Attr.LineWidth = _settings.ClusterLineWidth;
            }
            foreach (var edge in _graph.Edges)
            {
                edge.Attr.Color = _settings.EdgeDefaultColor;
                edge.Attr.LineWidth = _settings.EdgeDefaultWidth;
            }
        }

        private void MettreEnEvidenceTache(DrawingNode nodeSelectionne)
        {
            if (_graph == null) return;
            ClearAllHighlights();

            if (nodeSelectionne != null)
            {
                nodeSelectionne.Attr.LineWidth = _settings.HighlightLineWidth;
                nodeSelectionne.Attr.Color = _settings.HighlightBorderColor;
                foreach (var edge in _graph.Edges.Where(edge => edge.Source == nodeSelectionne.Id || edge.Target == nodeSelectionne.Id))
                {
                    edge.Attr.LineWidth = _settings.HighlightEdgeWidth;
                    edge.Attr.Color = _settings.HighlightEdgeColor;
                }
            }
            _viewer.Invalidate();
        }

        private void MettreEnEvidenceBloc(Subgraph subgraphSelectionne)
        {
            if (_graph == null) return;
            ClearAllHighlights();

            if (subgraphSelectionne != null)
            {
                subgraphSelectionne.Attr.LineWidth = _settings.HighlightLineWidth;
                subgraphSelectionne.Attr.Color = _settings.HighlightBorderColor;
            }
            _viewer.Invalidate();
        }

        #endregion

        #region Méthodes de l'API Publique

        public void TogglePan(bool panActif)
        {
            _viewer.PanButtonPressed = panActif;
            Cursor = panActif ? Cursors.Hand : Cursors.Default;
        }

        public void SauvegarderImage()
        {
            if (_viewer?.Graph == null) { MessageBox.Show("Aucun diagramme à sauvegarder.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            try
            {
                MethodInfo mi = typeof(GViewer).GetMethod("SaveImageClick", BindingFlags.NonPublic | BindingFlags.Instance);
                mi?.Invoke(_viewer, new object[] { this, EventArgs.Empty });
            }
            catch (Exception ex) { MessageBox.Show($"Erreur sauvegarde:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void ImprimerDiagramme()
        {
            if (_viewer?.Graph == null) { MessageBox.Show("Aucun diagramme à imprimer.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            try
            {
                _printDocument ??= new PrintDocument();
                _printDocument.DefaultPageSettings.Landscape = true;
                _printDocument.PrintPage += (sender, e) =>
                {
                    var bmp = new Bitmap(e.PageBounds.Width, e.PageBounds.Height);
                    var renderer = new GraphRenderer(_viewer.Graph);
                    renderer.Render(bmp);
                    e.Graphics.DrawImage(bmp, e.PageBounds);
                    bmp.Dispose();
                };
                using var printDialog = new PrintDialog { Document = _printDocument };
                if (printDialog.ShowDialog() == DialogResult.OK) _printDocument.Print();
            }
            catch (Exception ex) { MessageBox.Show($"Erreur impression:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void ZoomModifier(int zoomPercentage)
        {
            if (_viewer == null) return;

            // On s'assure que la valeur est dans des limites raisonnables pour éviter les erreurs.
            // Le TrackBar le fait déjà, mais c'est une bonne pratique.
            double newZoomFactor = Math.Max(0.1, zoomPercentage / 100.0);

            _viewer.ZoomF = newZoomFactor;
            _viewer.Invalidate(); // Force le redessin du diagramme avec le nouveau zoom

            // On notifie les abonnés que le zoom a changé
            ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(newZoomFactor));
        }

        public void ZoomToutAjuster()
        {
            if (_viewer?.Graph == null) return;
            _viewer.ZoomF = 1.0;
            _viewer.Invalidate();

            // On notifie aussi ici
            ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(1.0));
        }

        #endregion

        #region Génération du Diagramme

        private void GenererDiagramme(string filtreRecherche)
        {
            try
            {
                if (_nodeBuilder == null) return;
                _graph = new Graph("DiagrammePERT");
                ConfigurerLayoutOptimal(_graph);
                var tachesAffichees = FiltrerTaches(_taches, filtreRecherche);
                if (!tachesAffichees.Any())
                {
                    CreerNoeudInformation("Aucune tâche à afficher pour ce lot");
                }
                else
                {
                    var tachesParBloc = tachesAffichees.GroupBy(t => t.BlocId);
                    foreach (var blocGroup in tachesParBloc)
                    {
                        CreerClusterPourBloc(blocGroup.Key, blocGroup.ToList());
                    }
                    AjouterDependances(tachesAffichees);
                }
                _viewer.Graph = _graph;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur génération diagramme: {ex.Message}"); }
        }

        private List<Tache> FiltrerTaches(List<Tache> taches, string filtre)
        {
            if (string.IsNullOrWhiteSpace(filtre)) return taches;
            var recherche = filtre.ToLower();
            return taches.Where(t =>
                t.TacheId.ToLower().Contains(recherche) ||
                t.TacheNom.ToLower().Contains(recherche) ||
                (!string.IsNullOrEmpty(t.MetierId) && t.MetierId.ToLower().Contains(recherche))
            ).ToList();
        }

        private void ConfigurerLayoutOptimal(Graph graph)
        {
            graph.Attr.LayerDirection = _settings.LayoutDirection;
            graph.LayoutAlgorithmSettings = new SugiyamaLayoutSettings
            {
                NodeSeparation = _settings.LayoutNodeSeparation,
                LayerSeparation = _settings.LayoutLayerSeparation,
                ClusterMargin = _settings.ClusterPadding
            };
        }

        private void CreerClusterPourBloc(string blocId, List<Tache> tachesDuBloc)
        {
            var bloc = _projetService.ObtenirBlocParId(blocId);
            if (bloc == null) return;

            int capaciteAffichee = Math.Min(bloc.CapaciteMaxOuvriers, 10);
            string capaciteIcones = string.Concat(Enumerable.Repeat(_settings.ClusterLabelIcon, capaciteAffichee));
            var cluster = new Subgraph(blocId) { LabelText = string.Format(_settings.ClusterLabelFormat, capaciteIcones, bloc.Nom, tachesDuBloc.Count, tachesDuBloc.Sum(t => t.HeuresHommeEstimees)) };
            cluster.Attr.FillColor = _settings.ClusterFillColor;
            cluster.Attr.Color = _settings.ClusterBorderColor;
            cluster.Attr.LineWidth = _settings.ClusterLineWidth;
            cluster.Label.FontColor = _settings.ClusterFontColor;
            cluster.Label.FontSize = (int)_settings.ClusterFontSize;
            cluster.Label.FontName = "Segoe UI Symbol";

            foreach (var tache in tachesDuBloc.OrderBy(t => t.TacheId))
            {
                cluster.AddNode(_nodeBuilder.BuildNodeFromTache(tache, _graph));
            }
            _graph.RootSubgraph.AddSubgraph(cluster);
        }

        private void AjouterDependances(List<Tache> tachesAffichees)
        {
            var idsAffiches = new HashSet<string>(tachesAffichees.Select(t => t.TacheId));
            foreach (var tacheCourante in tachesAffichees)
            {
                var dependancesReelles = (tacheCourante.Dependencies ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var idPredecesseur in dependancesReelles)
                {
                    if (idsAffiches.Contains(idPredecesseur))
                    {
                        var edge = _graph.AddEdge(idPredecesseur, tacheCourante.TacheId);
                        edge.Attr.Color = _settings.EdgeDefaultColor;
                        edge.Attr.LineWidth = _settings.EdgeDefaultWidth;
                        edge.Attr.ArrowheadAtTarget = _settings.EdgeArrowStyle;
                    }
                }
            }
        }

        private void CreerNoeudInformation(string message)
        {
            var node = _graph.AddNode("info");
            node.LabelText = message;
            node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightGray;
            node.Attr.Shape = Shape.Plaintext;
            node.Label.FontSize = 14;
        }

        #endregion

        #region Tooltips
        private void InitializeTooltipHandling()
        {
            // Le GViewer est le contrôle qui reçoit les clics et survols
            _viewer.ObjectUnderMouseCursorChanged += GViewer_ObjectUnderMouseCursorChanged;

            // Le timer évite que le tooltip n'apparaisse et ne disparaisse instantanément
            tooltipTimer = new System.Windows.Forms.Timer();
            tooltipTimer.Interval = 500; // Délai en ms avant d'afficher le tooltip
            tooltipTimer.Tick += TooltipTimer_Tick;
        }

        private void GViewer_ObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            // Arrêter le timer précédent pour éviter les affichages intempestifs
            tooltipTimer.Stop();
            nodeTooltip.Hide(_viewer); // Cacher l'ancien tooltip immédiatement

            // Récupérer le nœud sous le curseur
            _hoveredNode = e.NewObject?.DrawingObject as Node;

            // Si on survole bien un nœud (et pas une arête ou le vide)
            if (_hoveredNode != null)
            {
                // Redémarrer le timer. Le tooltip s'affichera si la souris reste assez longtemps.
                tooltipTimer.Start();
            }
        }

        private void TooltipTimer_Tick(object sender, EventArgs e)
        {
            // Le timer s'est écoulé, on peut afficher le tooltip
            tooltipTimer.Stop();

            if (_hoveredNode != null && _hoveredNode.UserData is Tache tache)
            {
                // Construire le texte du tooltip à partir des données de la tâche
                string tooltipText = FormatTooltipText(tache);

                // Afficher le tooltip. On l'associe au gViewer.
                nodeTooltip.Show(tooltipText, _viewer, _viewer.PointToClient(Cursor.Position), nodeTooltip.AutoPopDelay);
            }
        }

        private string FormatTooltipText(Tache tache)
        {
            // Ici, vous avez accès à l'objet Tache complet !
            // Construisez la chaîne de caractères que vous voulez afficher.

            // Exemple simple :
            // return $"ID: {tache.TacheId}\nNom: {tache.TacheNom}\nStatut: {tache.Statut}";

            // Exemple plus riche :
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Tâche : {tache.TacheNom}");
            sb.AppendLine($"ID complet : {tache.TacheId}");
            sb.AppendLine($"────────────────────");
            sb.AppendLine($"Statut : {tache.Statut}");
            sb.AppendLine($"Durée estimée : {tache.HeuresHommeEstimees}h");

            var metier = _ressourceService.GetMetierById(tache.MetierId); // Assurez-vous d'avoir accès à votre service
            if (metier != null)
            {
                sb.AppendLine($"Métier : {metier.Nom}");
            }

            if (!string.IsNullOrEmpty(tache.Dependencies))
            {
                sb.AppendLine($"Dépendances : {tache.Dependencies.Split(',').Length}");
            }

            return sb.ToString();
        }

        #endregion


        //HelpER: This method checks if the zoom level has changed and notifies subscribers if it has.
        private void NotifyIfZoomChanged()
        {
            double currentZoom = _viewer.ZoomF;

            // Utiliser une petite tolérance pour les comparaisons de double
            if (Math.Abs(currentZoom - _lastKnownZoomFactor) > 0.001)
            {
                _lastKnownZoomFactor = currentZoom;
                // Déclencher notre événement personnalisé avec la nouvelle valeur
                ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(currentZoom));
            }
        }
    }

    #region Classes d'Arguments d'Événements
    
    public enum TacheInteractionType
    {
        SingleClick,
        DoubleClick
    }

    public class TacheSelectedEventArgs : EventArgs
    {
        public Tache Tache { get; }
        public TacheInteractionType InteractionType { get; } 

        public TacheSelectedEventArgs(Tache tache, TacheInteractionType interactionType = TacheInteractionType.SingleClick)
        {
            Tache = tache;
            InteractionType = interactionType;
        }
    }
    public class BlocSelectedEventArgs : EventArgs
    {
        public string BlocId { get; }
        public BlocSelectedEventArgs(string blocId) { BlocId = blocId; }
    }
    public class ZoomChangedEventArgs : EventArgs
    {
        public double ZoomFactor { get; }
        public ZoomChangedEventArgs(double zoomFactor) { ZoomFactor = zoomFactor; }
    }
    #endregion
}