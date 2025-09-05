// Fichier : PlanAthena/View/TaskManager/WebPertDiagram/AffichagePert.cs
using Microsoft.Web.WebView2.Core;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.View.TaskManager.PertDiagram;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlanAthena.View.TaskManager.WebPertDiagram
{
    public partial class AffichagePert : UserControl
    {
        // Garder une copie des données pour pouvoir retrouver une tâche par son ID
        private RessourceService _ressourceService; // Pour obtenir les couleurs
        private ProjetService _projetService;
        private PertDiagramSettings _settings;
        private List<Tache> _taches = new List<Tache>();
        // API Publique : Mêmes événements que l'ancien contrôle pour une intégration transparente
        public event EventHandler<TacheSelectedEventArgs> TacheClick;
        public event EventHandler<BlocSelectedEventArgs> BlocClick;

        public AffichagePert()
        {
            InitializeComponent();
            // Assurez-vous d'avoir un contrôle WebView2 nommé 'webView' dans le designer
            // avec sa propriété Dock = DockStyle.Fill
            this.Load += (s, e) => InitializeWebViewAsync();
        }

        public void Initialize(ProjetService projetService, RessourceService ressourceService, PertDiagramSettings settings)
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async void ChargerDonnees(List<Tache> taches)
        {
            _taches = taches ?? new List<Tache>();
            if (webView == null || webView.CoreWebView2 == null || !_taches.Any())
            {
                // Vider le graphe si pas de tâches
                //await webView?.CoreWebView2?.ExecuteScriptAsync("window.graphManager.clearGraph()");
                return;
            }

            // --- NOUVEAU : Préparer les blocs ---
            // On groupe les tâches par BlocId pour créer les nœuds parents
            var blocs = _taches
                .GroupBy(t => t.BlocId)
                .Select(g => _projetService.ObtenirBlocParId(g.Key)) // Récupérer l'objet Bloc complet
                .Where(b => b != null)
                .Select(b => new {
                    Id = b.BlocId,
                    Nom = b.Nom
                    // Ajoutez d'autres propriétés du bloc si nécessaire pour l'affichage
                })
                .ToList();

            // 1. Préparer les données pour le JavaScript
            var nodes = _taches.Select(t => {
                var metier = _ressourceService.GetMetierById(t.MetierId);
                var metierColor = _ressourceService.GetDisplayColorForMetier(t.MetierId);
                var depsCount = !string.IsNullOrEmpty(t.Dependencies) ? t.Dependencies.Split(',').Length : 0;

                return new
                {
                    // Données brutes pour l'affichage et l'interaction
                    Id = t.TacheId,
                    Nom = t.TacheNom,
                    EstJalon = t.EstJalon,
                    Heures = t.HeuresHommeEstimees,
                    MetierId = t.MetierId,
                    MetierNom = metier?.Nom ?? "Non assigné",
                    MetierPictogram = metier?.Pictogram ?? "❌",
                    DependenciesCount = depsCount,
                    // Donnée de style pré-calculée en C#
                    MetierColorHex = $"#{metierColor.R:X2}{metierColor.G:X2}{metierColor.B:X2}",
                    BlocId = t.BlocId
                };
            }).ToList();

            var edges = _taches
                .Where(t => !string.IsNullOrEmpty(t.Dependencies))
                .SelectMany(t => t.Dependencies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(depId => new { Source = depId.Trim(), Target = t.TacheId }))
                .ToList();

            var graphData = new { Blocs = blocs, Nodes = nodes, Edges = edges };

            // 2. Sérialiser les données en JSON (en camelCase pour être standard en JS)
            string json = JsonSerializer.Serialize(graphData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // 3. Envoyer les données au JavaScript en appelant une fonction globale
            await webView.CoreWebView2.ExecuteScriptAsync($"window.graphManager.loadGraph({json})");
        }

        public async void ZoomToutAjuster()
        {
            await webView?.CoreWebView2?.ExecuteScriptAsync("window.graphManager.fitView()");
        }

        // --- Logique interne du WebView ---

        private async void InitializeWebViewAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            string htmlPath = Path.Combine(AppContext.BaseDirectory, "WebAssets", "index.html");
            webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
            webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = JsonDocument.Parse(e.WebMessageAsJson).RootElement;
            var messageType = message.GetProperty("type").GetString();

            if (messageType == "nodeClick")
            {
                var nodeId = message.GetProperty("data").GetProperty("id").GetString();
                var tache = _taches.FirstOrDefault(t => t.TacheId == nodeId);
                if (tache != null)
                {
                    this.Invoke((MethodInvoker)delegate {
                        TacheClick?.Invoke(this, new TacheSelectedEventArgs(tache));
                    });
                }
            }
            // NOUVEAU : Gérer le clic sur un bloc
            else if (messageType == "blocClick")
            {
                var blocId = message.GetProperty("data").GetProperty("id").GetString();
                // Le `BlocClick` attend un string, donc pas besoin de chercher l'objet complet
                if (!string.IsNullOrEmpty(blocId))
                {
                    this.Invoke((MethodInvoker)delegate {
                        BlocClick?.Invoke(this, new BlocSelectedEventArgs(blocId));
                    });
                }
            }
        }
    }

    // Recopiez les classes d'événements TacheSelectedEventArgs, etc. ou rendez-les publiques
}