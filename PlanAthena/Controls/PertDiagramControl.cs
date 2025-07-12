using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using PlanAthena.CsvModels;

namespace PlanAthena.Controls
{
    public partial class PertDiagramControl : UserControl
    {
        private readonly GViewer _viewer;
        private List<TacheCsvRecord> _taches = new List<TacheCsvRecord>();
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
            Microsoft.Msagl.Drawing.Color.LightSteelBlue
        };
        private int _couleurIndex = 0;

        public TacheCsvRecord TacheSelectionnee { get; private set; }

        public PertDiagramControl()
        {
            InitializeComponent();

            // Créer un panel avec scroll qui contient le viewer
            var scrollPanel = new Panel();
            scrollPanel.Dock = DockStyle.Fill;
            scrollPanel.AutoScroll = true;
            scrollPanel.BackColor = SystemColors.Window;

            _viewer = new GViewer();
            _viewer.MouseClick += Viewer_MouseClick;
            _viewer.MouseDoubleClick += Viewer_MouseDoubleClick;

            // IMPORTANT : Ne pas docker le viewer, laisser les scrollbars gérer la taille
            _viewer.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _viewer.Location = new Point(0, 0);

            // Configuration du viewer
            ConfigurerViewer();

            // Ajouter le viewer au panel scrollable
            scrollPanel.Controls.Add(_viewer);

            // Ajouter le panel au contrôle principal
            this.Controls.Add(scrollPanel);
        }

        private void ConfigurerViewer()
        {
            try
            {
                // Configuration simple et propre
                _viewer.PanButtonPressed = false;  // Pas de pan, on utilise les scrollbars
                _viewer.NavigationVisible = true;   // Garder les contrôles de zoom
                _viewer.ZoomF = 0.8;
                _viewer.OutsideAreaBrush = Brushes.White;

                var tooltip = new ToolTip();
                tooltip.SetToolTip(_viewer, "Scrollbars = Navigation | Clic = Sélection | Double-clic = Éditer");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur configuration viewer: {ex.Message}");
            }
        }

        public void ChargerTaches(List<TacheCsvRecord> taches, string filtreRecherche = "")
        {
            _taches = taches ?? new List<TacheCsvRecord>();
            _couleursByMetier.Clear();
            _couleurIndex = 0;

            GenererDiagramme(filtreRecherche);
        }

        private void GenererDiagramme(string filtreRecherche = "")
        {
            _graph = new Graph("DiagrammePERT");

            // CORRECTION : Configuration forcée pour format paysage AVEC scrollbars
            _graph.Attr.LayerDirection = LayerDirection.LR;
            _graph.Attr.AspectRatio = 0.2; // Très large pour forcer le format paysage

            // Espacements généreux pour un diagramme large
            _graph.Attr.NodeSeparation = 100;
            _graph.Attr.LayerSeparation = 150;
            _graph.Attr.MinNodeHeight = 60;
            _graph.Attr.MinNodeWidth = 140;
            _graph.Attr.Margin = 30;

            var tachesAffichees = _taches.AsEnumerable();

            // Appliquer le filtre de recherche
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

            // IMPORTANT : Ajuster la taille du viewer après génération
            this.BeginInvoke(new Action(() => {
                try
                {
                    AjusterTailleViewer();
                }
                catch { }
            }));
        }

        private void AjusterTailleViewer()
        {
            try
            {
                if (_viewer.Graph != null)
                {
                    // Calculer la taille nécessaire du viewer basée sur le graphe
                    var bounds = _viewer.Graph.BoundingBox;

                    if (bounds.Width > 0 && bounds.Height > 0)
                    {
                        // Taille du viewer = taille du graphe + marge pour zoom confortable
                        var viewerWidth = (int)(bounds.Width * 1.2) + 100;
                        var viewerHeight = (int)(bounds.Height * 1.2) + 100;

                        // Assurer une taille minimum
                        viewerWidth = Math.Max(viewerWidth, 800);
                        viewerHeight = Math.Max(viewerHeight, 600);

                        _viewer.Size = new Size(viewerWidth, viewerHeight);

                        // Le panel parent gérera automatiquement les scrollbars
                        _viewer.Invalidate();
                    }
                }
            }
            catch (Exception)
            {
                // Taille par défaut en cas d'erreur
                _viewer.Size = new Size(1200, 800);
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

        private void CreerClusterPourBloc(string blocId, List<TacheCsvRecord> tachesDuBloc)
        {
            if (!tachesDuBloc.Any()) return;

            var premiereTache = tachesDuBloc.First();
            var cluster = new Subgraph(blocId)
            {
                LabelText = $"📦 {premiereTache.BlocNom}\n({tachesDuBloc.Count} tâches - {tachesDuBloc.Sum(t => t.HeuresHommeEstimees)}h)"
            };

            // Style de cluster optimisé
            cluster.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightYellow;
            cluster.Attr.Color = Microsoft.Msagl.Drawing.Color.DarkOrange;
            cluster.Attr.LineWidth = 2;
            cluster.Label.FontColor = Microsoft.Msagl.Drawing.Color.DarkBlue;
            cluster.Label.FontSize = 11;
            cluster.Attr.Padding = 15;

            foreach (var tache in tachesDuBloc.OrderBy(t => t.TacheId))
            {
                var node = CreerNoeudTache(tache);
                cluster.AddNode(node);
            }

            _graph.RootSubgraph.AddSubgraph(cluster);
        }

        private Node CreerNoeudTache(TacheCsvRecord tache)
        {
            var nodeId = tache.TacheId;
            var node = _graph.AddNode(nodeId);

            var metierAffiche = !string.IsNullOrEmpty(tache.MetierId) ? tache.MetierId : "❌ Non assigné";
            var label = $"🏷️ {tache.TacheId}\n{TronquerTexte(tache.TacheNom, 18)}\n" +
                       $"👨‍💼 {TronquerTexte(metierAffiche, 15)}\n⏱️ {tache.HeuresHommeEstimees}h";

            node.LabelText = label;

            var couleur = ObtenirCouleurPourMetier(tache.MetierId);
            node.Attr.FillColor = couleur;
            node.Attr.Shape = Shape.Box;
            node.Attr.LineWidth = 2;

            // Taille de nœud cohérente
            node.Attr.LabelMargin = 10;
            node.Label.FontSize = 9;

            // Style selon l'état du métier
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

        private void AjouterDependances(List<TacheCsvRecord> tachesAffichees)
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
                    edge.Attr.Color = Microsoft.Msagl.Drawing.Color.DarkGreen;
                    edge.Attr.LineWidth = 2;
                    edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                    edge.LabelText = ""; // Pas de label sur les flèches pour simplifier
                }
            }
        }

        private Microsoft.Msagl.Drawing.Color ObtenirCouleurPourMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId))
                return Microsoft.Msagl.Drawing.Color.MistyRose; // Couleur pour "sans métier"

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
            try
            {
                var objectUnderMouse = _viewer.ObjectUnderMouseCursor;

                if (objectUnderMouse?.DrawingObject is Node node && node.UserData is TacheCsvRecord tache)
                {
                    TacheSelectionnee = tache;
                    TacheSelected?.Invoke(this, new TacheSelectedEventArgs(tache));
                    MettreEnEvidenceTache(node);
                }
            }
            catch (Exception)
            {
                // Gestion d'erreur silencieuse
            }
        }

        private void Viewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                var objectUnderMouse = _viewer.ObjectUnderMouseCursor;

                if (objectUnderMouse?.DrawingObject is Node node && node.UserData is TacheCsvRecord tache)
                {
                    TacheDoubleClicked?.Invoke(this, new TacheSelectedEventArgs(tache));
                }
            }
            catch (Exception)
            {
                // Gestion d'erreur silencieuse
            }
        }

        private void MettreEnEvidenceTache(Node nodeSelectionne)
        {
            // Remettre tous les nœuds en style normal
            foreach (Node node in _graph.Nodes)
            {
                if (node.UserData is TacheCsvRecord tache)
                {
                    node.Attr.LineWidth = string.IsNullOrEmpty(tache.MetierId) ? 3 : 2;
                    node.Attr.Color = string.IsNullOrEmpty(tache.MetierId) ?
                        Microsoft.Msagl.Drawing.Color.Red : Microsoft.Msagl.Drawing.Color.DarkBlue;
                }
            }

            // Mettre en évidence le nœud sélectionné
            if (nodeSelectionne != null)
            {
                nodeSelectionne.Attr.LineWidth = 5;
                nodeSelectionne.Attr.Color = Microsoft.Msagl.Drawing.Color.Purple;
            }

            _viewer.Invalidate();
        }

        public void ZoomToutAjuster()
        {
            try
            {
                if (_viewer.Graph != null)
                {
                    // Zoom pour voir tout le contenu avec une marge confortable
                    var graphBounds = _viewer.Graph.BoundingBox;
                    var viewerBounds = _viewer.ClientRectangle;

                    if (graphBounds.Width > 0 && graphBounds.Height > 0 &&
                        viewerBounds.Width > 50 && viewerBounds.Height > 50)
                    {
                        var scaleX = (viewerBounds.Width - 100) / graphBounds.Width;
                        var scaleY = (viewerBounds.Height - 100) / graphBounds.Height;
                        var scale = Math.Min(scaleX, scaleY) * 0.85; // 85% pour avoir une bonne marge

                        // Zoom minimum pour garantir la lisibilité
                        scale = Math.Max(scale, 0.5);

                        _viewer.ZoomF = scale;
                    }
                    else
                    {
                        _viewer.ZoomF = 0.8; // Zoom par défaut confortable
                    }

                    _viewer.Invalidate();
                }
            }
            catch (Exception)
            {
                try
                {
                    _viewer.ZoomF = 0.8;
                    _viewer.Invalidate();
                }
                catch { }
            }
        }

        public bool RechercherTache(string tacheId)
        {
            var node = _graph?.FindNode(tacheId);
            if (node != null)
            {
                MettreEnEvidenceTache(node);
                TacheSelectionnee = node.UserData as TacheCsvRecord;
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
            this.Size = new System.Drawing.Size(800, 600);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ResumeLayout(false);
        }
    }

    public class TacheSelectedEventArgs : EventArgs
    {
        public TacheCsvRecord Tache { get; }

        public TacheSelectedEventArgs(TacheCsvRecord tache)
        {
            Tache = tache;
        }
    }
}