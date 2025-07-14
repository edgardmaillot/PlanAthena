using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using PlanAthena.Data;
using System.Drawing.Printing;
using System.Reflection;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;

namespace PlanAthena.Controls
{
    public partial class PertDiagramControl : UserControl
    {
        private readonly GViewer _viewer;
        private List<TacheRecord> _taches = new List<TacheRecord>();
        private Graph _graph;

        public event EventHandler<TacheSelectedEventArgs> TacheSelected;
        public event EventHandler<TacheSelectedEventArgs> TacheDoubleClicked;

        private readonly Dictionary<string, Microsoft.Msagl.Drawing.Color> _couleursByMetier =
            new Dictionary<string, Microsoft.Msagl.Drawing.Color>();
        private readonly Microsoft.Msagl.Drawing.Color[] _couleursDisponibles = {
            Microsoft.Msagl.Drawing.Color.LightBlue,
            Microsoft.Msagl.Drawing.Color.LightGreen,
            Microsoft.Msagl.Drawing.Color.LightYellow,
            Microsoft.Msagl.Drawing.Color.LightPink,
            Microsoft.Msagl.Drawing.Color.LightGray,
            Microsoft.Msagl.Drawing.Color.LightCyan,
            Microsoft.Msagl.Drawing.Color.LightSalmon,
            Microsoft.Msagl.Drawing.Color.LightCoral,
            Microsoft.Msagl.Drawing.Color.LightSteelBlue
        };
        private int _couleurIndex = 0;

        // Variables pour gérer le pan avec la souris
        private bool _isPanning = false;
        private Point _panStartPoint;
        private Panel _scrollPanel; // Référence au panel pour le scroll
        private PrintDocument _printDocument;
        public TacheRecord TacheSelectionnee { get; private set; }

        public PertDiagramControl()
        {
            InitializeComponent();

            _scrollPanel = new Panel();
            _scrollPanel.Dock = DockStyle.Fill;
            _scrollPanel.AutoScroll = true;
            _scrollPanel.BackColor = SystemColors.Window;

            _viewer = new GViewer();
            _viewer.MouseClick += Viewer_MouseClick;
            _viewer.MouseDoubleClick += Viewer_MouseDoubleClick;
            _viewer.MouseDown += Viewer_MouseDown;
            _viewer.MouseMove += Viewer_MouseMove;
            _viewer.MouseUp += Viewer_MouseUp;

            _viewer.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _viewer.Location = new Point(0, 0);

            ConfigurerViewer();

            _scrollPanel.Controls.Add(_viewer);
            this.Controls.Add(_scrollPanel);
        }

        private void ConfigurerViewer()
        {
            try
            {
                _viewer.PanButtonPressed = false;
                _viewer.NavigationVisible = false;
                _viewer.ToolBarIsVisible = false;
                _viewer.ZoomF = 0.8;
                _viewer.OutsideAreaBrush = Brushes.White;

                var tooltip = new ToolTip();
                tooltip.SetToolTip(_viewer, "Clic gauche = Sélection | Double-clic = Éditer | Clic droit + glisser = Déplacer");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur configuration viewer: {ex.Message}");
            }
        }

        #region Méthodes publiques pour le contrôle depuis TacheForm

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
                // On invoque la méthode privée SaveImageClick que vous avez trouvée.
                MethodInfo mi = typeof(GViewer).GetMethod("SaveImageClick", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null)
                {
                    mi.Invoke(_viewer, new object[] { this, EventArgs.Empty });
                }
                else
                {
                    MessageBox.Show("Impossible de trouver la fonctionnalité de sauvegarde.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue lors de la tentative de sauvegarde:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ImprimerDiagramme()
        {
            if (_viewer.Graph == null) return;

            if (_printDocument == null)
            {
                _printDocument = new PrintDocument();
                _printDocument.DefaultPageSettings.Landscape = true; // Mettre en paysage pour mieux voir le graphe

                _printDocument.PrintPage += (sender, e) =>
                {
                    // 1. Créer une image en mémoire (Bitmap) de la taille de la page
                    var bmp = new Bitmap(e.PageBounds.Width, e.PageBounds.Height);

                    // 2. Rendre le graphe sur cette image en mémoire
                    // La méthode Render attend une Image, on lui passe notre Bitmap.
                    var renderer = new Microsoft.Msagl.GraphViewerGdi.GraphRenderer(_viewer.Graph);
                    renderer.Render(bmp);

                    // 3. Dessiner le Bitmap final sur la page d'impression
                    e.Graphics.DrawImage(bmp, e.PageBounds);

                    // 4. Nettoyer l'image en mémoire
                    bmp.Dispose();
                };
            }

            using (var printDialog = new PrintDialog())
            {
                printDialog.Document = _printDocument;
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _printDocument.Print();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de l'impression:\n{ex.Message}", "Erreur d'impression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion

        #region Gestion du Pan avec le clic droit

        private void Viewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                _isPanning = true;
                _panStartPoint = e.Location;
                _viewer.Cursor = Cursors.SizeAll;
            }
        }

        private void Viewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                int dx = e.Location.X - _panStartPoint.X;
                int dy = e.Location.Y - _panStartPoint.Y;

                Point newScrollPosition = new Point(
                    _scrollPanel.HorizontalScroll.Value - dx,
                    _scrollPanel.VerticalScroll.Value - dy);

                newScrollPosition.X = Math.Max(0, Math.Min(newScrollPosition.X, _scrollPanel.HorizontalScroll.Maximum));
                newScrollPosition.Y = Math.Max(0, Math.Min(newScrollPosition.Y, _scrollPanel.VerticalScroll.Maximum));

                _scrollPanel.AutoScrollPosition = newScrollPosition;

                // Forcer le rafraîchissement pour éviter les glitchs
                _viewer.Invalidate();
            }
        }

        private void Viewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                _isPanning = false;
                _viewer.Cursor = Cursors.Default;
            }
        }

        #endregion

        public void ChargerTaches(List<TacheRecord> taches, string filtreRecherche = "")
        {
            _taches = taches ?? new List<TacheRecord>();
            _couleursByMetier.Clear();
            _couleurIndex = 0;
            GenererDiagramme(filtreRecherche);
        }

        private void GenererDiagramme(string filtreRecherche = "")
        {
            _graph = new Graph("DiagrammePERT");

            // Configuration optimisée pour la réduction transitive
            ConfigurerLayoutOptimal(_graph);

            var tachesAffichees = _taches.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtreRecherche))
            {
                var recherche = filtreRecherche.ToLower();
                tachesAffichees = tachesAffichees.Where(t =>
                    t.TacheId.ToLower().Contains(recherche) ||
                    t.TacheNom.ToLower().Contains(recherche) ||
                    t.BlocId.ToLower().Contains(recherche) ||
                    t.LotId.ToLower().Contains(recherche) ||
                    (!string.IsNullOrEmpty(t.MetierId) && t.MetierId.ToLower().Contains(recherche)));
            }

            if (!tachesAffichees.Any())
            {
                CreerNoeudInformation("Aucune tâche à afficher");
                _viewer.Graph = _graph;
                AjusterTailleViewer();
                return;
            }

            var tachesParBloc = tachesAffichees.GroupBy(t => t.BlocId).ToList();
            foreach (var blocGroup in tachesParBloc)
            {
                CreerClusterPourBloc(blocGroup.Key, blocGroup.ToList());
            }

            AjouterDependances(tachesAffichees.ToList());

            _viewer.Graph = _graph;

            this.BeginInvoke(new Action(() =>
            {
                try { AjusterTailleViewer(); } catch { }
            }));
        }

        /// <summary>
        /// Configure le layout pour une visualisation optimale avec les paramètres disponibles dans MSAGL 1.1.6
        /// </summary>
        private void ConfigurerLayoutOptimal(Graph graph)
        {
            // Configuration générale du graphe
            graph.Attr.LayerDirection = LayerDirection.LR;
            graph.Attr.AspectRatio = 0.3;
            graph.Attr.NodeSeparation = 20;
            graph.Attr.LayerSeparation = 90;
            graph.Attr.MinNodeHeight = 70;
            graph.Attr.MinNodeWidth = 160;
            graph.Attr.Margin = 20;

            // Configuration compatible avec MSAGL 1.1.6
            var settings = new SugiyamaLayoutSettings();

            // Paramètres disponibles dans la version 1.1.6
            settings.NodeSeparation = 20;
            settings.LayerSeparation = 90;
            settings.MinNodeHeight = 70;
            settings.MinNodeWidth = 160;

            // Optimiser l'arrangement des couches pour réduire les croisements
            settings.RepetitionCoefficientForOrdering = 3;
            settings.BrandesThreshold = 1;

            // Paramètres pour améliorer la lisibilité
            settings.ClusterMargin = 15;
            settings.PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Columns;

            // Appliquer les paramètres
            graph.LayoutAlgorithmSettings = settings;
        }

        private void AjusterTailleViewer()
        {
            try
            {
                if (_viewer.Graph != null)
                {
                    var bounds = _viewer.Graph.BoundingBox;
                    if (bounds.Width > 0 && bounds.Height > 0)
                    {
                        var viewerWidth = (int)(bounds.Width * 1.3) + 150;
                        var viewerHeight = (int)(bounds.Height * 1.3) + 150;
                        viewerWidth = Math.Max(viewerWidth, 1000);
                        viewerHeight = Math.Max(viewerHeight, 700);
                        _viewer.Size = new Size(viewerWidth, viewerHeight);
                        _viewer.Invalidate();
                    }
                }
            }
            catch (Exception)
            {
                _viewer.Size = new Size(1400, 900);
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

        private void CreerClusterPourBloc(string blocId, List<TacheRecord> tachesDuBloc)
        {
            if (!tachesDuBloc.Any()) return;

            var premiereTache = tachesDuBloc.First();
            var cluster = new Subgraph(blocId)
            {
                LabelText = $"📦 {premiereTache.BlocNom}\n({tachesDuBloc.Count} tâches - {tachesDuBloc.Sum(t => t.HeuresHommeEstimees)}h)"
            };

            // Style amélioré pour les clusters
            cluster.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightYellow;
            cluster.Attr.Color = Microsoft.Msagl.Drawing.Color.DarkOrange;
            cluster.Attr.LineWidth = 2;
            cluster.Label.FontColor = Microsoft.Msagl.Drawing.Color.DarkBlue;
            cluster.Label.FontSize = 12;
            cluster.Attr.Padding = 20;

            foreach (var tache in tachesDuBloc.OrderBy(t => t.TacheId))
            {
                var node = CreerNoeudTache(tache);
                cluster.AddNode(node);
            }

            _graph.RootSubgraph.AddSubgraph(cluster);
        }

        private DrawingNode CreerNoeudTache(TacheRecord tache)
        {
            var nodeId = tache.TacheId;
            var node = _graph.AddNode(nodeId);

            var metierAffiche = !string.IsNullOrEmpty(tache.MetierId) ? tache.MetierId : "❌ Non assigné";

            // Affichage enrichi avec informations sur les dépendances
            var dependances = !string.IsNullOrEmpty(tache.Dependencies)
                ? tache.Dependencies.Split(',').Length
                : 0;

            var label = $"🏷️ {tache.TacheId}\n{TronquerTexte(tache.TacheNom, 20)}\n" +
                       $"👨‍💼 {TronquerTexte(metierAffiche, 18)}\n" +
                       $"⏱️ {tache.HeuresHommeEstimees}h";

            if (dependances > 0)
            {
                label += $"\n🔗 {dependances} dép.";
            }

            node.LabelText = label;

            var couleur = ObtenirCouleurPourMetier(tache.MetierId);
            node.Attr.FillColor = couleur;
            node.Attr.Shape = Shape.Box;
            node.Attr.LineWidth = 2;
            node.Attr.LabelMargin = 12;
            node.Label.FontSize = 9;

            // Style selon l'état de la tâche
            if (string.IsNullOrEmpty(tache.MetierId))
            {
                node.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                node.Attr.LineWidth = 3;
            }
            else
            {
                node.Attr.Color = Microsoft.Msagl.Drawing.Color.DarkBlue;
            }

            node.UserData = tache;
            return node;
        }

        private void AjouterDependances(List<TacheRecord> tachesAffichees)
        {
            var idsAffiches = new HashSet<string>(tachesAffichees.Select(t => t.TacheId));

            foreach (var tache in tachesAffichees.Where(t => !string.IsNullOrEmpty(t.Dependencies)))
            {
                var dependances = tache.Dependencies.Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d) && idsAffiches.Contains(d));

                foreach (var dependanceId in dependances)
                {
                    var edge = _graph.AddEdge(dependanceId, tache.TacheId);

                    // Style amélioré pour les arêtes
                    edge.Attr.Color = Microsoft.Msagl.Drawing.Color.DarkGreen;
                    edge.Attr.LineWidth = 2;
                    edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                    edge.LabelText = "";

                    // Réduire la courbure pour une meilleure lisibilité
                    edge.Attr.Weight = 1;
                }
            }
        }

        private Microsoft.Msagl.Drawing.Color ObtenirCouleurPourMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId))
                return Microsoft.Msagl.Drawing.Color.MistyRose;

            if (!_couleursByMetier.ContainsKey(metierId))
            {
                _couleursByMetier[metierId] = _couleursDisponibles[_couleurIndex % _couleursDisponibles.Length];
                _couleurIndex++;
            }
            return _couleursByMetier[metierId];
        }

        private string TronquerTexte(string texte, int longueurMax)
        {
            if (string.IsNullOrEmpty(texte) || texte.Length <= longueurMax)
                return texte;
            return texte.Substring(0, longueurMax - 2) + "..";
        }

        private void Viewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isPanning) return;
            try
            {
                var objectUnderMouse = _viewer.ObjectUnderMouseCursor;
                if (objectUnderMouse?.DrawingObject is DrawingNode node && node.UserData is TacheRecord tache)
                {
                    TacheSelectionnee = tache;
                    TacheSelected?.Invoke(this, new TacheSelectedEventArgs(tache));
                    MettreEnEvidenceTache(node);
                }
            }
            catch (Exception) { }
        }

        private void Viewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                var objectUnderMouse = _viewer.ObjectUnderMouseCursor;
                if (objectUnderMouse?.DrawingObject is DrawingNode node && node.UserData is TacheRecord tache)
                {
                    TacheDoubleClicked?.Invoke(this, new TacheSelectedEventArgs(tache));
                }
            }
            catch (Exception) { }
        }

        private void MettreEnEvidenceTache(DrawingNode nodeSelectionne)
        {
            if (_graph == null || !_graph.Nodes.Any()) return;

            // Réinitialiser tous les nœuds
            foreach (DrawingNode node in _graph.Nodes)
            {
                if (node.UserData is TacheRecord tache)
                {
                    node.Attr.LineWidth = string.IsNullOrEmpty(tache.MetierId) ? 3 : 2;
                    node.Attr.Color = string.IsNullOrEmpty(tache.MetierId) ?
                        Microsoft.Msagl.Drawing.Color.Red : Microsoft.Msagl.Drawing.Color.DarkBlue;
                }
            }

            // Mettre en évidence le nœud sélectionné et ses connexions
            if (nodeSelectionne != null)
            {
                nodeSelectionne.Attr.LineWidth = 5;
                nodeSelectionne.Attr.Color = Microsoft.Msagl.Drawing.Color.Purple;

                // Mettre en évidence les arêtes connectées
                foreach (DrawingEdge edge in _graph.Edges)
                {
                    if (edge.Source == nodeSelectionne.Id || edge.Target == nodeSelectionne.Id)
                    {
                        edge.Attr.LineWidth = 3;
                        edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Purple;
                    }
                    else
                    {
                        edge.Attr.LineWidth = 2;
                        edge.Attr.Color = Microsoft.Msagl.Drawing.Color.DarkGreen;
                    }
                }
            }

            _viewer.Invalidate();
        }

        public void ZoomToutAjuster()
        {
            try
            {
                if (_viewer.Graph == null || _viewer.Graph.Width == 0) return;

                var graphBounds = _viewer.Graph.BoundingBox;
                var panelBounds = _scrollPanel.ClientRectangle;

                if (graphBounds.Width > 0 && graphBounds.Height > 0 && panelBounds.Width > 50)
                {
                    double margin = 60;
                    var scaleX = (panelBounds.Width - margin) / graphBounds.Width;
                    var scaleY = (panelBounds.Height - margin) / graphBounds.Height;

                    var scale = Math.Min(scaleX, scaleY);
                    scale = Math.Min(scale, 2.0); // Limiter le zoom maximum
                    scale = Math.Max(scale, 0.1); // Limiter le zoom minimum

                    _viewer.ZoomF = scale;

                    // NOUVEAU: Centrer la vue sur le graphe
                    // Calculer le centre du graphe dans les coordonnées du viewer (après zoom)
                    int centerX = (int)(graphBounds.Center.X * scale);
                    int centerY = (int)(graphBounds.Center.Y * scale);

                    // Calculer la position de scroll pour que ce centre soit au milieu du panel
                    int scrollX = centerX - (panelBounds.Width / 2);
                    int scrollY = centerY - (panelBounds.Height / 2);

                    // Appliquer la position de scroll en s'assurant qu'elle est dans les limites
                    scrollX = Math.Max(0, Math.Min(scrollX, _scrollPanel.HorizontalScroll.Maximum));
                    scrollY = Math.Max(0, Math.Min(scrollY, _scrollPanel.VerticalScroll.Maximum));

                    _scrollPanel.AutoScrollPosition = new Point(scrollX, scrollY);
                }
                else
                {
                    _viewer.ZoomF = 1.0;
                }

                _viewer.Invalidate();
            }
            catch (Exception)
            {
                // Ignorer les erreurs potentielles
            }
        }

        public bool RechercherTache(string tacheId)
        {
            var node = _graph?.FindNode(tacheId);
            if (node != null)
            {
                MettreEnEvidenceTache(node);
                TacheSelectionnee = node.UserData as TacheRecord;
                return true;
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "PertDiagramControl";
            this.Size = new System.Drawing.Size(1000, 700);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ResumeLayout(false);
        }
    }

    public class TacheSelectedEventArgs : EventArgs
    {
        public TacheRecord Tache { get; }
        public TacheSelectedEventArgs(TacheRecord tache)
        {
            Tache = tache;
        }
    }
}