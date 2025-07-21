using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using PlanAthena.Controls.Config;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace PlanAthena.Controls
{
    public partial class PertDiagramControl : UserControl
    {
        #region Champs et Dépendances

        private readonly GViewer _viewer;
        private Graph _graph;

        private PertDiagramSettings _settings;
        private PertNodeBuilder _nodeBuilder;
        private MetierService _metierService;
        private LotService _lotService;
        private BlocService _blocService;

        private List<Tache> _taches = new List<Tache>();

        private bool _isPanning = false;
        private Point _panStartPoint;
        private PrintDocument _printDocument;
        public Tache TacheSelectionnee { get; private set; }

        #endregion

        #region Événements Publics

        public event EventHandler<TacheSelectedEventArgs> TacheSelected;
        public event EventHandler<TacheSelectedEventArgs> TacheDoubleClicked;

        #endregion

        #region Initialisation

        public PertDiagramControl()
        {
            InitializeComponent();
            _viewer = new GViewer { Dock = DockStyle.Fill };
            this.Controls.Add(_viewer);
        }

        public void Initialize(MetierService metierService, LotService lotService, BlocService blocService, PertDiagramSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
            _nodeBuilder = new PertNodeBuilder(settings, _metierService);

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
            var tooltip = new ToolTip();
            tooltip.SetToolTip(_viewer, "Clic sur objet = Sélection | Clic sur fond = Déplacement vue | Double-clic = Éditer");
        }

        #endregion

        #region Méthodes Publiques

        public void ChargerDonnees(List<Tache> taches, string filtreRecherche = "")
        {
            _taches = taches ?? new List<Tache>();
            GenererDiagramme(filtreRecherche);
        }

        public void TogglePan(bool panActif)
        {
            _viewer.PanButtonPressed = panActif;
            this.Cursor = panActif ? Cursors.Hand : Cursors.Default;
        }

        public void SauvegarderImage()
        {
            if (_viewer.Graph == null) return;
            try
            {
                MethodInfo mi = typeof(GViewer).GetMethod("SaveImageClick", BindingFlags.NonPublic | BindingFlags.Instance);
                mi?.Invoke(_viewer, new object[] { this, EventArgs.Empty });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ImprimerDiagramme()
        {
            if (_viewer.Graph == null) return;
            if (_printDocument == null)
            {
                _printDocument = new PrintDocument();
                _printDocument.DefaultPageSettings.Landscape = true;
                _printDocument.PrintPage += (sender, e) =>
                {
                    var bmp = new Bitmap(e.PageBounds.Width, e.PageBounds.Height);
                    var renderer = new GraphRenderer(_viewer.Graph);
                    renderer.Render(bmp);
                    e.Graphics.DrawImage(bmp, e.PageBounds);
                    bmp.Dispose();
                };
            }
            using (var printDialog = new PrintDialog { Document = _printDocument })
            {
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    try { _printDocument.Print(); }
                    catch (Exception ex) { MessageBox.Show($"Erreur d'impression:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        public void ZoomToutAjuster()
        {
            try
            {
                if (_viewer.Graph == null) return;
                var bounds = _viewer.Graph.BoundingBox;
                var clientSize = _viewer.ClientSize;
                if (bounds.Width > 0 && bounds.Height > 0 && clientSize.Width > 0 && clientSize.Height > 0)
                {
                    double margin = 80;
                    var scaleX = (clientSize.Width - margin) / bounds.Width;
                    var scaleY = (clientSize.Height - margin) / bounds.Height;
                    var scale = Math.Min(scaleX, scaleY);
                    _viewer.ZoomF = Math.Max(0.3, Math.Min(scale, 2.0));
                }
                else { _viewer.ZoomF = _settings.DefaultZoom; }
                _viewer.Invalidate();
            }
            catch { /* Gérer l'exception si nécessaire */ }
        }

        #endregion

        #region Génération du Diagramme

        private void GenererDiagramme(string filtreRecherche)
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
            this.BeginInvoke(new Action(() => { try { ZoomToutAjuster(); } catch { } }));
        }

        private List<Tache> FiltrerTaches(List<Tache> taches, string filtre)
        {
            if (string.IsNullOrWhiteSpace(filtre)) return taches;
            var recherche = filtre.ToLower();
            var matchingLotIds = _lotService.ObtenirTousLesLots().Where(l => l.Nom.ToLower().Contains(recherche)).Select(l => l.LotId).ToHashSet();
            var matchingBlocIds = _blocService.ObtenirTousLesBlocs().Where(b => b.Nom.ToLower().Contains(recherche)).Select(b => b.BlocId).ToHashSet();
            return taches.Where(t =>
                t.TacheId.ToLower().Contains(recherche) ||
                t.TacheNom.ToLower().Contains(recherche) ||
                (!string.IsNullOrEmpty(t.MetierId) && t.MetierId.ToLower().Contains(recherche)) ||
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
            if (!tachesDuBloc.Any()) return;
            var bloc = _blocService.ObtenirBlocParId(blocId);
            if (bloc == null) return;
            var cluster = new Subgraph(blocId)
            {
                LabelText = string.Format(_settings.ClusterLabelFormat, bloc.Nom, tachesDuBloc.Count, tachesDuBloc.Sum(t => t.HeuresHommeEstimees))
            };
            cluster.Attr.FillColor = _settings.ClusterFillColor;
            cluster.Attr.Color = _settings.ClusterBorderColor;
            cluster.Attr.LineWidth = _settings.ClusterLineWidth;
            cluster.Label.FontColor = _settings.ClusterFontColor;
            cluster.Label.FontSize = (int)_settings.ClusterFontSize;
            foreach (var tache in tachesDuBloc.OrderBy(t => t.TacheId))
            {
                var node = _nodeBuilder.BuildNodeFromTache(tache, _graph);
                cluster.AddNode(node);
            }
            _graph.RootSubgraph.AddSubgraph(cluster);
        }

        private void AjouterDependances(List<Tache> tachesAffichees)
        {
            var idsAffiches = new HashSet<string>(tachesAffichees.Select(t => t.TacheId));
            foreach (var tache in tachesAffichees.Where(t => !string.IsNullOrEmpty(t.Dependencies)))
            {
                var dependances = tache.Dependencies.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d) && idsAffiches.Contains(d));
                foreach (var dependanceId in dependances)
                {
                    var edge = _graph.AddEdge(dependanceId, tache.TacheId);
                    edge.Attr.Color = _settings.EdgeDefaultColor;
                    edge.Attr.LineWidth = _settings.EdgeDefaultWidth;
                    edge.Attr.ArrowheadAtTarget = _settings.EdgeArrowStyle;
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

        #endregion

        #region Gestion des Événements Souris & Highlighting

        private void Viewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            _viewer.PanButtonPressed = _viewer.ObjectUnderMouseCursor?.DrawingObject is not DrawingNode;
            if (_viewer.PanButtonPressed)
            {
                _isPanning = true;
                _panStartPoint = e.Location;
            }
        }

        private void Viewer_MouseMove(object sender, MouseEventArgs e) { /* Géré par MSAGL */ }

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

        #endregion
    }

    public class TacheSelectedEventArgs : EventArgs
    {
        public Tache Tache { get; }
        public TacheSelectedEventArgs(Tache tache) { Tache = tache; }
    }
}