// Fichier: PlanAthena/Controls/PertDiagramControl.cs
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Services.Business; // Ajouté pour ProjetService
using PlanAthena.Utilities;
using System.Drawing.Printing;
using System.Reflection;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace PlanAthena.Controls
{
    public class BlocSelectedEventArgs : EventArgs
    {
        public string BlocId { get; }
        public BlocSelectedEventArgs(string blocId) { BlocId = blocId; }
    }

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
        private bool _isPanning = false;
        private Point _panStartPoint;
        private PrintDocument _printDocument;
        public Tache TacheSelectionnee { get; private set; }
        private System.Windows.Forms.Timer _zoomMonitorTimer;
        private double _lastKnownZoom = 1.0;

        public event EventHandler<TacheSelectedEventArgs> TacheSelected;
        public event EventHandler<TacheSelectedEventArgs> TacheDoubleClicked;
        public event EventHandler<BlocSelectedEventArgs> BlocDoubleClicked;
        public event EventHandler<ZoomChangedEventArgs> ZoomChanged;

        public double ZoomFacteur => _viewer?.ZoomF ?? 1.0;

        public PertDiagramControl()
        {
            InitializeComponent();
            _viewer = new GViewer { Dock = DockStyle.Fill };
            Controls.Add(_viewer);
        }

        public void Initialize(ProjetService projetService, RessourceService ressourceService, DependanceBuilder dependanceBuilder, PertDiagramSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _dependanceBuilder = dependanceBuilder ?? throw new ArgumentNullException(nameof(dependanceBuilder));
            _nodeBuilder = new PertNodeBuilder(settings, _ressourceService);

            ConfigurerViewer();
            _viewer.MouseClick += Viewer_MouseClick;
            _viewer.MouseDoubleClick += Viewer_MouseDoubleClick;
            _viewer.MouseDown += Viewer_MouseDown;
            _viewer.MouseMove += Viewer_MouseMove;
            _viewer.MouseUp += Viewer_MouseUp;
        }

        private void ConfigurerViewer()
        {
            _viewer.PanButtonPressed = false;
            _viewer.NavigationVisible = false;
            _viewer.ToolBarIsVisible = false;
            _viewer.ZoomF = _settings.DefaultZoom;
            _viewer.OutsideAreaBrush = new SolidBrush(_settings.OutsideAreaColor);
            InitialiserMonitoringZoom();
            var tooltip = new ToolTip();
            tooltip.SetToolTip(_viewer, "Clic sur objet = Sélection | Clic sur fond = Déplacement vue | Double-clic = Éditer");
        }

        private void InitialiserMonitoringZoom()
        {
            _zoomMonitorTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _zoomMonitorTimer.Tick += ZoomMonitorTimer_Tick;
            _zoomMonitorTimer.Start();
            _lastKnownZoom = _viewer.ZoomF;
        }

        private void ZoomMonitorTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_viewer == null) return;
                var currentZoom = _viewer.ZoomF;
                if (Math.Abs(currentZoom - _lastKnownZoom) > 0.01)
                {
                    _lastKnownZoom = currentZoom;
                    ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(currentZoom));
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur monitoring zoom: {ex.Message}"); }
        }

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

        public void ChargerDonnees(List<Tache> taches, string filtreRecherche = "")
        {
            _taches = taches ?? new List<Tache>();
            GenererDiagramme(filtreRecherche);
        }

        public void TogglePan(bool panActif)
        {
            _viewer.PanButtonPressed = panActif;
            Cursor = panActif ? Cursors.Hand : Cursors.Default;
        }

        public void SauvegarderImage()
        {
            if (_viewer?.Graph == null)
            {
                MessageBox.Show("Aucun diagramme à sauvegarder.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                MethodInfo mi = typeof(GViewer).GetMethod("SaveImageClick", BindingFlags.NonPublic | BindingFlags.Instance);
                mi?.Invoke(_viewer, new object[] { this, EventArgs.Empty });
            }
            catch (Exception ex) { MessageBox.Show($"Erreur sauvegarde:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void ImprimerDiagramme()
        {
            if (_viewer?.Graph == null)
            {
                MessageBox.Show("Aucun diagramme à imprimer.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                if (_printDocument == null)
                {
                    _printDocument = new PrintDocument();
                    _printDocument.DefaultPageSettings.Landscape = true;
                    _printDocument.PrintPage += (sender, e) =>
                    {
                        try
                        {
                            var bmp = new Bitmap(e.PageBounds.Width, e.PageBounds.Height);
                            var renderer = new GraphRenderer(_viewer.Graph);
                            renderer.Render(bmp);
                            e.Graphics.DrawImage(bmp, e.PageBounds);
                            bmp.Dispose();
                        }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur rendu: {ex.Message}"); }
                    };
                }
                using (var printDialog = new PrintDialog { Document = _printDocument })
                {
                    if (printDialog.ShowDialog() == DialogResult.OK) _printDocument.Print();
                }
            }
            catch (Exception ex) { MessageBox.Show($"Erreur impression:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void ZoomToutAjuster()
        {
            try
            {
                if (_viewer?.Graph == null) return;
                var bounds = _viewer.Graph.BoundingBox;
                var clientSize = _viewer.ClientSize;
                if (bounds.Width <= 0 || bounds.Height <= 0 || clientSize.Width <= 0 || clientSize.Height <= 0) return;
                var newZoom = 1.0;
                _viewer.ZoomF = newZoom;
                _viewer.Invalidate();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur zoom: {ex.Message}"); }
        }

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
            var matchingLotIds = _projetService.ObtenirTousLesLots().Where(l => l.Nom.ToLower().Contains(recherche)).Select(l => l.LotId).ToHashSet();
            var matchingBlocIds = _projetService.ObtenirTousLesBlocs().Where(b => b.Nom.ToLower().Contains(recherche)).Select(b => b.BlocId).ToHashSet();
            return taches.Where(t =>
                t.TacheId.ToLower().Contains(recherche) ||
                t.TacheNom.ToLower().Contains(recherche) ||
                !string.IsNullOrEmpty(t.MetierId) && t.MetierId.ToLower().Contains(recherche) ||
                matchingLotIds.Contains(t.LotId) ||
                matchingBlocIds.Contains(t.BlocId)
            ).ToList();
        }

        private void ConfigurerLayoutOptimal(Graph graph)
        {
            graph.Attr.LayerDirection = _settings.LayoutDirection;
            graph.Attr.AspectRatio = _settings.LayoutAspectRatio;
            graph.Attr.NodeSeparation = _settings.LayoutNodeSeparation;
            graph.Attr.LayerSeparation = _settings.LayoutLayerSeparation;
            graph.Attr.MinNodeHeight = _settings.LayoutMinNodeHeight;
            graph.Attr.MinNodeWidth = _settings.LayoutMinNodeWidth;
            graph.Attr.Margin = _settings.LayoutMargin;
            graph.LayoutAlgorithmSettings = new SugiyamaLayoutSettings
            {
                RepetitionCoefficientForOrdering = _settings.SugiyamaRepetitionCoefficient,
                BrandesThreshold = _settings.SugiyamaBrandesThreshold,
                PackingMethod = _settings.SugiyamaPackingMethod,
                ClusterMargin = _settings.ClusterPadding
            };
        }

        private void CreerClusterPourBloc(string blocId, List<Tache> tachesDuBloc)
        {
            var touslesblocs = _projetService.ObtenirTousLesBlocs();
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
            cluster.Label.FontName = "Segoe UI Emoji";
            if (tachesDuBloc.Any())
            {
                foreach (var tache in tachesDuBloc.OrderBy(t => t.TacheId))
                {
                    cluster.AddNode(_nodeBuilder.BuildNodeFromTache(tache, _graph));
                }
            }
            else
            {
                var emptyNode = _graph.AddNode($"{blocId}_empty");
                emptyNode.LabelText = "Aucune tâche";
                emptyNode.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightGray;
                emptyNode.Attr.Shape = Shape.Box;
                emptyNode.Label.FontSize = 10;
                emptyNode.Label.FontColor = Microsoft.Msagl.Drawing.Color.DarkGray;
                cluster.AddNode(emptyNode);
            }
            _graph.RootSubgraph.AddSubgraph(cluster);
        }

        private void AjouterDependances(List<Tache> tachesAffichees)
        {
            var idsAffiches = new HashSet<string>(tachesAffichees.Select(t => t.TacheId));
            foreach (var tacheCourante in tachesAffichees)
            {
                var dependancesReelles = (tacheCourante.Dependencies ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim());
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
            node.Attr.Shape = Shape.Box;
            node.Label.FontSize = 14;
        }

        private void Viewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            _viewer.PanButtonPressed = _viewer.ObjectUnderMouseCursor?.DrawingObject is not DrawingNode;
            if (_viewer.PanButtonPressed) { _isPanning = true; _panStartPoint = e.Location; }
        }

        private void Viewer_MouseMove(object sender, MouseEventArgs e) { }

        private void Viewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left || !_isPanning) return;
            _isPanning = false;
            _viewer.PanButtonPressed = false;
        }

        private void Viewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left || _isPanning) return;
            var objectUnderMouse = _viewer.ObjectUnderMouseCursor;
            if (objectUnderMouse?.DrawingObject is DrawingNode selectedNode && selectedNode.UserData is Tache tache)
            {
                TacheSelectionnee = tache;
                TacheSelected?.Invoke(this, new TacheSelectedEventArgs(tache));
                MettreEnEvidenceTache(selectedNode);
            }
        }

        private void Viewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var objectUnderMouse = _viewer.ObjectUnderMouseCursor;
            if (objectUnderMouse?.DrawingObject is DrawingNode node && node.UserData is Tache tache)
            {
                TacheDoubleClicked?.Invoke(this, new TacheSelectedEventArgs(tache));
                return;
            }
            if (objectUnderMouse?.DrawingObject is Subgraph subgraph)
            {
                BlocDoubleClicked?.Invoke(this, new BlocSelectedEventArgs(subgraph.Id));
                return;
            }
        }

        private void MettreEnEvidenceTache(DrawingNode nodeSelectionne)
        {
            if (_graph == null || !_graph.Nodes.Any()) return;
            foreach (var node in _graph.Nodes) { _nodeBuilder.ApplyNodeStyle(node, node.UserData as Tache); }
            foreach (var edge in _graph.Edges)
            {
                edge.Attr.Color = _settings.EdgeDefaultColor;
                edge.Attr.LineWidth = _settings.EdgeDefaultWidth;
            }
            if (nodeSelectionne != null)
            {
                nodeSelectionne.Attr.LineWidth = _settings.HighlightLineWidth;
                nodeSelectionne.Attr.Color = _settings.HighlightBorderColor;
                foreach (var edge in _graph.Edges)
                {
                    if (edge.Source == nodeSelectionne.Id || edge.Target == nodeSelectionne.Id)
                    {
                        edge.Attr.LineWidth = _settings.HighlightEdgeWidth;
                        edge.Attr.Color = _settings.HighlightEdgeColor;
                    }
                }
            }
            _viewer.Invalidate();
        }
    }

    public class TacheSelectedEventArgs : EventArgs
    {
        public Tache Tache { get; }
        public TacheSelectedEventArgs(Tache tache) { Tache = tache; }
    }

    public class ZoomChangedEventArgs : EventArgs
    {
        public double ZoomFactor { get; }
        public ZoomChangedEventArgs(double zoomFactor) { ZoomFactor = zoomFactor; }
    }
}