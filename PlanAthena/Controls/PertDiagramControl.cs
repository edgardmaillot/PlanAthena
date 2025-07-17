using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using System.Drawing.Printing;
using System.Reflection;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;

namespace PlanAthena.Controls
{
    public partial class PertDiagramControl : UserControl
    {
        private readonly GViewer _viewer;
        private List<Tache> _taches = new List<Tache>();
        private List<Metier> _metiers = new List<Metier>();
        private MetierService _metierService; // Ajouté pour identifier les jalons
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
        private PrintDocument _printDocument;
        public Tache TacheSelectionnee { get; private set; }

        // Variables pour mémoriser l'état de la vue (simplifié)
        private double _dernierZoom = 0.8;

        public PertDiagramControl()
        {
            InitializeComponent();

            // SUPPRESSION des barres de défilement - utiliser uniquement le pan MSAGL
            _viewer = new GViewer();
            _viewer.Dock = DockStyle.Fill;  // Le viewer occupe tout l'espace
            _viewer.MouseClick += Viewer_MouseClick;
            _viewer.MouseDoubleClick += Viewer_MouseDoubleClick;
            _viewer.MouseDown += Viewer_MouseDown;
            _viewer.MouseMove += Viewer_MouseMove;
            _viewer.MouseUp += Viewer_MouseUp;

            // SUPPRESSION des événements Enter/Leave qui causent des repositionnements
            // _viewer.Enter += Viewer_Enter;
            // _viewer.Leave += Viewer_Leave;
            // this.Enter += Control_Enter;
            // this.Leave += Control_Leave;

            ConfigurerViewer();

            this.Controls.Add(_viewer);  // Ajouter directement le viewer
        }

        private void ConfigurerViewer()
        {
            try
            {
                _viewer.PanButtonPressed = false;  // Désactivé par défaut
                _viewer.NavigationVisible = false;
                _viewer.ToolBarIsVisible = false;
                _viewer.ZoomF = 0.8;
                _viewer.OutsideAreaBrush = Brushes.White;

                var tooltip = new ToolTip();
                tooltip.SetToolTip(_viewer, "Clic sur objet = Sélection/Déplacement objet | Clic sur fond = Déplacement vue | Double-clic = Éditer");
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

            // Mettre à jour le tooltip selon l'état
            var tooltip = new ToolTip();
            if (panActif)
            {
                tooltip.SetToolTip(_viewer, "Mode Pan activé : Glisser pour déplacer | Double-clic = Éditer");
            }
            else
            {
                tooltip.SetToolTip(_viewer, "Clic gauche = Sélection | Double-clic = Éditer | Bouton PAN pour déplacer");
            }
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

        #region Gestion intelligente clic gauche : Objet vs Fond

        private void Viewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var objectUnderMouse = _viewer.ObjectUnderMouseCursor;

                if (objectUnderMouse?.DrawingObject is DrawingNode)
                {
                    // Clic sur un objet : Mode sélection/déplacement d'objet (comportement par défaut de MSAGL)
                    _viewer.PanButtonPressed = false;
                    System.Diagnostics.Debug.WriteLine("Left click on object - Object manipulation mode");
                }
                else
                {
                    // Clic sur le fond : Mode déplacement de la vue
                    _viewer.PanButtonPressed = true;
                    _isPanning = true;
                    _panStartPoint = e.Location;
                    System.Diagnostics.Debug.WriteLine("Left click on background - Pan mode activated");
                }
            }
        }

        private void Viewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Diagnostics.Debug.WriteLine("Background drag in progress");
                // MSAGL gère automatiquement le pan
            }
        }

        private void Viewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && _isPanning)
            {
                _isPanning = false;
                _viewer.PanButtonPressed = false;
                System.Diagnostics.Debug.WriteLine("Background drag ended - Pan deactivated");
            }
        }

        #endregion

        #region Gestion du focus et de l'état de la vue - SUPPRIMÉ

        // Ces méthodes ont été supprimées car elles causaient des repositionnements intempestifs
        // lors de l'affichage de MessageBox ou changements de focus

        #endregion

        public void ChargerTaches(List<Tache> taches, string filtreRecherche = "")
        {
            _taches = taches ?? new List<Tache>();
            _couleursByMetier.Clear();
            _couleurIndex = 0;
            GenererDiagramme(filtreRecherche);
        }

        /// <summary>
        /// Charge les métiers pour pouvoir utiliser leurs couleurs personnalisées
        /// </summary>
        /// <param name="metiers">Liste des métiers avec leurs couleurs</param>
        public void ChargerMetiers(List<Metier> metiers)
        {
            _metiers = metiers ?? new List<Metier>();
            _couleursByMetier.Clear();
            _couleurIndex = 0;
        }

        /// <summary>
        /// Charge à la fois les tâches et les métiers avec un MetierService pour identifier les jalons
        /// </summary>
        /// <param name="taches">Liste des tâches</param>
        /// <param name="metiers">Liste des métiers avec leurs couleurs</param>
        /// <param name="filtreRecherche">Filtre de recherche optionnel</param>
        /// <param name="metierService">Service pour identifier les jalons (optionnel)</param>
        public void ChargerDonnees(List<Tache> taches, List<Metier> metiers, string filtreRecherche = "", MetierService metierService = null)
        {
            _taches = taches ?? new List<Tache>();
            _metiers = metiers ?? new List<Metier>();
            _metierService = metierService; // Stocker la référence pour identifier les jalons
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
            graph.Attr.NodeSeparation = 30;
            graph.Attr.LayerSeparation = 90;
            graph.Attr.MinNodeHeight = 50;
            graph.Attr.MinNodeWidth = 50;
            graph.Attr.Margin = 20;

            // Configuration compatible avec MSAGL 1.1.6
            var settings = new SugiyamaLayoutSettings();

            // Paramètres disponibles dans la version 1.1.6
            settings.NodeSeparation = 30;
            settings.LayerSeparation = 90;
            settings.MinNodeHeight = 50;
            settings.MinNodeWidth = 50;

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

        private void CreerClusterPourBloc(string blocId, List<Tache> tachesDuBloc)
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

        private DrawingNode CreerNoeudTache(Tache tache)
        {
            var nodeId = tache.TacheId;
            var node = _graph.AddNode(nodeId);

            // Vérifier si c'est un jalon
            bool estJalon = tache.EstJalon;

            var metierAffiche = !string.IsNullOrEmpty(tache.MetierId) ? tache.MetierId : "❌ Non assigné";

            // Affichage enrichi avec informations sur les dépendances
            var dependances = !string.IsNullOrEmpty(tache.Dependencies)
                ? tache.Dependencies.Split(',').Length
                : 0;

            string label;
            if (estJalon)
            {
                // LABEL ULTRA-COMPACT : juste une icône
                if (tache.HeuresHommeEstimees > 0)
                {
                    label = $"⏳";  // Jalon avec attente (sablier)
                }
                else
                {
                    label = $"🏁";  // Jalon sans attente (drapeau)
                }
            }
            else
            {
                // Label normal pour les tâches (inchangé)
                label = $"🏷️ {tache.TacheId}\n{TronquerTexte(tache.TacheNom, 20)}\n" +
                       $"👨‍💼 {TronquerTexte(metierAffiche, 18)}\n" +
                       $"⏱️ {tache.HeuresHommeEstimees}h";

                if (dependances > 0)
                {
                    label += $"\n🔗 {dependances} dép.";
                }
            }

            node.LabelText = label;

            var couleur = ObtenirCouleurPourMetier(tache.MetierId);
            node.Attr.FillColor = couleur;

            // MODIFICATION PRINCIPALE : Forme différente pour les jalons
            if (estJalon)
            {
                if (tache.Type == TypeActivite.JalonDeSynchronisation || tache.Type == TypeActivite.JalonTechnique)
                {
                    node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightGoldenrodYellow; // Une couleur distincte pour les jalons auto
                }
                node.Attr.Shape = Shape.Diamond;
                node.Attr.LineWidth = 3;
                node.Attr.Padding = 3;          // Très compact
                node.Label.FontSize = 14;
                node.Attr.LabelMargin = 2;
            }
            else
            {
                node.Attr.Shape = Shape.Box;
                node.Attr.LineWidth = 2;
                node.Attr.Padding = 18;         // Plus spacieux
                node.Label.FontSize = 9;
                node.Attr.LabelMargin = 12;     // Plus de marge

                // Optionnel : forcer une largeur minimale par le texte
                if (label.Length < 30)
                {
                    label += new string(' ', 30 - label.Length); // Espaces de remplissage
                }
            }

            // Style selon l'état de la tâche
            if (!estJalon && string.IsNullOrEmpty(tache.MetierId))
            {
                node.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                node.Attr.LineWidth = 3;
            }
            else if (estJalon)
            {
                // Couleur spéciale pour les jalons
                node.Attr.Color = Microsoft.Msagl.Drawing.Color.Orange;
            }
            else
            {
                node.Attr.Color = Microsoft.Msagl.Drawing.Color.DarkBlue;
            }

            node.UserData = tache;
            return node;
        }

        private void AjouterDependances(List<Tache> tachesAffichees)
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

            // Priorité 1: Utiliser la couleur personnalisée du métier si elle existe
            if (!string.IsNullOrEmpty(metierId) && _metiers != null)
            {
                var metier = _metiers.FirstOrDefault(m => m.MetierId == metierId);
                if (metier != null && !string.IsNullOrEmpty(metier.CouleurHex))
                {
                    try
                    {
                        // Convertir la couleur hexadécimale en couleur System.Drawing
                        var systemColor = ColorTranslator.FromHtml(metier.CouleurHex);

                        // Convertir en couleur MSAGL
                        var msaglColor = new Microsoft.Msagl.Drawing.Color(
                            systemColor.R,
                            systemColor.G,
                            systemColor.B);

                        return msaglColor;
                    }
                    catch (Exception)
                    {
                        // En cas d'erreur de conversion, continuer avec le système de fallback
                    }
                }
            }

            // Priorité 2 (fallback): Utiliser le système de couleurs auto-générées existant
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
            // Seulement pour la sélection, pas pour le pan
            if (e.Button != System.Windows.Forms.MouseButtons.Left || _isPanning)
                return;

            try
            {
                var objectUnderMouse = _viewer.ObjectUnderMouseCursor;
                if (objectUnderMouse?.DrawingObject is DrawingNode node && node.UserData is Tache tache)
                {
                    TacheSelectionnee = tache;
                    TacheSelected?.Invoke(this, new TacheSelectedEventArgs(tache));
                    MettreEnEvidenceTache(node);
                    System.Diagnostics.Debug.WriteLine($"Object selected: {tache.TacheId}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Click on background - no selection");
                }
            }
            catch (Exception) { }
        }

        private void Viewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                var objectUnderMouse = _viewer.ObjectUnderMouseCursor;
                if (objectUnderMouse?.DrawingObject is DrawingNode node && node.UserData is Tache tache)
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
                if (node.UserData is Tache tache)
                {
                    bool estJalon = tache.EstJalon;

                    if (estJalon)
                    {
                        node.Attr.LineWidth = 3;
                        node.Attr.Color = Microsoft.Msagl.Drawing.Color.Orange;
                    }
                    else
                    {
                        node.Attr.LineWidth = string.IsNullOrEmpty(tache.MetierId) ? 3 : 2;
                        node.Attr.Color = string.IsNullOrEmpty(tache.MetierId) ?
                            Microsoft.Msagl.Drawing.Color.Red : Microsoft.Msagl.Drawing.Color.DarkBlue;
                    }
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
                if (_viewer.Graph == null) return;

                // Calcul manuel du zoom optimal (confirmé fonctionnel par les logs)
                var bounds = _viewer.Graph.BoundingBox;
                var clientSize = _viewer.ClientSize;

                if (bounds.Width > 0 && bounds.Height > 0 && clientSize.Width > 0 && clientSize.Height > 0)
                {
                    double margin = 80; // Marge autour du graphique
                    var scaleX = (clientSize.Width - margin) / bounds.Width;
                    var scaleY = (clientSize.Height - margin) / bounds.Height;
                    var scale = Math.Min(scaleX, scaleY);

                    // Limiter le zoom entre 0.3 et 2.0 pour éviter les extrêmes
                    scale = Math.Max(0.3, Math.Min(scale, 2.0));

                    _viewer.ZoomF = scale;
                }
                else
                {
                    _viewer.ZoomF = 0.8; // Zoom par défaut si calcul impossible
                }

                _dernierZoom = _viewer.ZoomF;
                _viewer.Invalidate();
            }
            catch (Exception)
            {
                // Fallback simple en cas d'erreur
                _viewer.ZoomF = 0.8;
                _dernierZoom = 0.8;
                _viewer.Invalidate();
            }
        }

        public bool RechercherTache(string tacheId)
        {
            var node = _graph?.FindNode(tacheId);
            if (node != null)
            {
                MettreEnEvidenceTache(node);
                TacheSelectionnee = node.UserData as Tache;
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
        public Tache Tache { get; }
        public TacheSelectedEventArgs(Tache tache)
        {
            Tache = tache;
        }
    }
}