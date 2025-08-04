using Microsoft.VisualBasic.ApplicationServices;
using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using QuikGraph;
using QuikGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Documents;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service de gestion des projets (sauvegarde/chargement complet)
    /// </summary>
    public class ProjetService
    {
        private readonly OuvrierService _ouvrierService;
        private readonly Func<TacheService> _tacheServiceFactory;
        private readonly CsvDataService _csvDataService;
        private readonly LotService _lotService;
        private readonly Func<BlocService> _blocServiceFactory;

        // Collection interne de métiers, gérée directement par ProjetService
        private readonly Dictionary<string, Metier> _metiersInternes = new Dictionary<string, Metier>();

        // Champs liés aux couleurs de fallback des métiers, déplacés de MetierService
        private static readonly Color[] FallbackColors = {
            Color.LightBlue, Color.LightGreen, Color.LightYellow,
            Color.LightPink, Color.LightGray, Color.LightCyan,
            Color.LightSalmon
        };
        private int _fallbackColorIndex = 0;
        private readonly Dictionary<string, Color> _assignedFallbackColors = new Dictionary<string, Color>();


        public ProjetService(
            OuvrierService ouvrierService,
            Func<TacheService> tacheServiceFactory,
            CsvDataService csvDataService,
            LotService lotService,
            Func<BlocService> blocServiceFactory)
        {
            _ouvrierService = ouvrierService ?? throw new ArgumentNullException(nameof(ouvrierService));
            _tacheServiceFactory = tacheServiceFactory ?? throw new ArgumentNullException(nameof(tacheServiceFactory));
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocServiceFactory = blocServiceFactory ?? throw new ArgumentNullException(nameof(blocServiceFactory));
        }

        private TacheService _tacheService => _tacheServiceFactory();
        private BlocService _blocService => _blocServiceFactory();

        public bool ValiderDonneesAvantPlanification(out string message)
        {
            var resumeTaches = _tacheService.ObtenirStatistiques();
            if (resumeTaches.NombreTachesTotal == 0)
            {
                message = "Aucune tâche chargée. Veuillez charger des tâches avant de lancer la planification.";
                return false;
            }

            var resumeOuvriers = _ouvrierService.ObtenirStatistiques();
            if (resumeOuvriers.NombreOuvriersTotal == 0)
            {
                message = "Aucun ouvrier chargé. Veuillez charger des ouvriers avant de lancer la planification.";
                return false;
            }

            if (GetAllMetiers().Count == 0)
            {
                message = "Aucun métier chargé. Veuillez charger des métiers avant de lancer la planification.";
                return false;
            }

            message = "Validation réussie.";
            return true;
        }

        #region Sauvegarde/Chargement Projet

        /// <summary>
        /// Sauvegarde un projet complet au format JSON
        /// </summary>
        /// <param name="filePath">Chemin du fichier de sauvegarde</param>
        /// <param name="informationsProjet">Informations sur le projet</param>
        public void SauvegarderProjet(string filePath, InformationsProjet informationsProjet)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Le chemin du fichier ne peut pas être vide.");

            if (informationsProjet == null)
                throw new ArgumentNullException(nameof(informationsProjet));

            try
            {
                var projetData = new ProjetData
                {
                    InformationsProjet = informationsProjet,
                    Metiers = GetAllMetiers().ToList(),
                    Ouvriers = _ouvrierService.ObtenirTousLesOuvriers(),
                    Taches = _tacheService.ObtenirToutesLesTaches(),
                    Lots = _lotService.ObtenirTousLesLots(),
                    Blocs = _blocService.ObtenirTousLesBlocs(),
                    DateSauvegarde = DateTime.Now,
                    VersionApplication = "0.3.8"
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonString = JsonSerializer.Serialize(projetData, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new ProjetException($"Erreur lors de la sauvegarde du projet: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Charge un projet complet depuis un fichier JSON.
        /// Gère de manière transparente la migration depuis l'ancien format de fichier.
        /// </summary>
        /// <param name="filePath">Chemin du fichier de projet</param>
        /// <returns>Informations du projet chargé</returns>
        public InformationsProjet ChargerProjet(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Le fichier de projet '{filePath}' n'existe pas.");

            try
            {
                var jsonString = File.ReadAllText(filePath);
                var jsonNode = JsonNode.Parse(jsonString);

                if (jsonNode == null)
                    throw new ProjetException("Le fichier de projet est invalide ou corrompu.");

                return ChargerNouveauProjet(jsonNode);
            }
            catch (JsonException ex)
            {
                throw new ProjetException($"Erreur lors de la lecture du fichier JSON: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ProjetException($"Erreur lors du chargement du projet: {ex.Message}", ex);
            }
        }

        private InformationsProjet ChargerNouveauProjet(JsonNode jsonNode)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var projetData = jsonNode.Deserialize<ProjetData>(options);
            if (projetData == null)
                throw new ProjetException("Le fichier de projet est invalide ou corrompu.");

            // Charger les données dans l'ordre de dépendance
            RemplacerTousLesMetiers(projetData.Metiers);
            _ouvrierService.ChargerOuvriers(projetData.Ouvriers);
            _lotService.RemplacerTousLesLots(projetData.Lots);
            _blocService.RemplacerTousLesBlocs(projetData.Blocs);
            _tacheService.ChargerTaches(projetData.Taches);

            return projetData.InformationsProjet;
        }

        /// <summary>
        /// Valide qu'un fichier de projet est valide
        /// </summary>
        /// <param name="filePath">Chemin du fichier à valider</param>
        /// <returns>Résultat de la validation</returns>
        public ValidationProjet ValiderProjet(string filePath)
        {
            var validation = new ValidationProjet { EstValide = false };

            try
            {
                if (!File.Exists(filePath))
                {
                    validation.Erreurs.Add("Le fichier n'existe pas.");
                    return validation;
                }

                // Pour la validation, nous ne nous soucions pas du format (ancien/nouveau)
                // tant que les informations de base sont présentes.
                var jsonString = File.ReadAllText(filePath);
                var projetData = JsonSerializer.Deserialize<ProjetData>(jsonString);

                if (projetData == null)
                {
                    validation.Erreurs.Add("Le fichier JSON est invalide.");
                    return validation;
                }

                // Validation des données essentielles
                if (projetData.InformationsProjet == null)
                    validation.Erreurs.Add("Informations du projet manquantes.");

                if (projetData.Metiers == null || projetData.Metiers.Count == 0)
                    validation.Avertissements.Add("Aucun métier défini.");

                if (projetData.Ouvriers == null || projetData.Ouvriers.Count == 0)
                    validation.Avertissements.Add("Aucun ouvrier défini.");

                if (projetData.Taches == null || projetData.Taches.Count == 0)
                    validation.Avertissements.Add("Aucune tâche définie.");

                validation.EstValide = validation.Erreurs.Count == 0;
                validation.InformationsProjet = projetData.InformationsProjet;

                return validation;
            }
            catch (Exception ex)
            {
                validation.Erreurs.Add($"Erreur lors de la validation: {ex.Message}");
                return validation;
            }
        }

        #endregion

        #region CRUD Operations - Métiers (déplacé de MetierService)

        /// <summary>
        /// Ajoute un nouveau métier au projet.
        /// </summary>
        public void AjouterMetier(Metier nouveauMetier)
        {
            if (nouveauMetier == null)
                throw new ArgumentNullException(nameof(nouveauMetier));
            if (string.IsNullOrWhiteSpace(nouveauMetier.MetierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(nouveauMetier.MetierId));
            if (_metiersInternes.ContainsKey(nouveauMetier.MetierId))
                throw new InvalidOperationException($"Un métier avec l'ID '{nouveauMetier.MetierId}' existe déjà.");

            _metiersInternes.Add(nouveauMetier.MetierId, nouveauMetier);
        }

        /// <summary>
        /// Modifie un métier existant.
        /// </summary>
        public void ModifierMetier(string metierId, string nouveauNom, string nouveauxPrerequisIds, string couleurHex = null, string pictogram = null, ChantierPhase phases = ChantierPhase.None)
        {
            if (!_metiersInternes.TryGetValue(metierId, out var metierAModifier))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            metierAModifier.Nom = nouveauNom;
            metierAModifier.PrerequisMetierIds = nouveauxPrerequisIds;

            if (couleurHex != null)
            {
                metierAModifier.CouleurHex = couleurHex;
            }
            if (pictogram != null)
            {
                metierAModifier.Pictogram = pictogram;
            }
            if (phases != ChantierPhase.None || (metierAModifier.Phases != ChantierPhase.None && phases == ChantierPhase.None)) // Update only if phases are provided or explicitly set to None
            {
                metierAModifier.Phases = phases;
            }
        }

        /// <summary>
        /// Supprime un métier du projet.
        /// </summary>
        public void SupprimerMetier(string metierId)
        {
            if (!_metiersInternes.Remove(metierId))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            // Supprimer ce métier des prérequis des autres métiers
            foreach (var metier in _metiersInternes.Values)
            {
                var prerequis = GetPrerequisForMetier(metier.MetierId).ToList();
                if (prerequis.Remove(metierId))
                {
                    metier.PrerequisMetierIds = string.Join(",", prerequis);
                }
            }
        }

        #endregion

        #region Data Loading and Retrieval - Métiers (déplacé de MetierService)

        /// <summary>
        /// Remplace tous les métiers existants par une nouvelle liste.
        /// </summary>
        public void RemplacerTousLesMetiers(IReadOnlyList<Metier> metiers)
        {
            _metiersInternes.Clear();
            if (metiers != null)
            {
                foreach (var metier in metiers)
                {
                    if (!string.IsNullOrEmpty(metier.MetierId) && !_metiersInternes.ContainsKey(metier.MetierId))
                    {
                        _metiersInternes.Add(metier.MetierId, metier);
                    }
                }
            }
        }

        /// <summary>
        /// Retourne tous les métiers du projet.
        /// </summary>
        public IReadOnlyList<Metier> GetAllMetiers()
        {
            return _metiersInternes.Values.ToList();
        }

        /// <summary>
        /// Retourne un métier par son ID.
        /// </summary>
        public Metier GetMetierById(string metierId)
        {
            _metiersInternes.TryGetValue(metierId, out var metier);
            return metier;
        }

        /// <summary>
        /// Obtient la liste des IDs des métiers prérequis pour un métier donné.
        /// </summary>
        public IReadOnlyList<string> GetPrerequisForMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId)) return Array.Empty<string>();

            if (_metiersInternes.TryGetValue(metierId, out var metier) && !string.IsNullOrEmpty(metier.PrerequisMetierIds))
            {
                return metier.PrerequisMetierIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Obtient la liste complète et transitive de tous les prérequis pour un métier donné.
        /// </summary>
        public HashSet<string> GetTransitivePrerequisites(string metierId)
        {
            var allPrereqs = new HashSet<string>();
            var toExplore = new Queue<string>(GetPrerequisForMetier(metierId));

            while (toExplore.Count > 0)
            {
                var current = toExplore.Dequeue();
                if (allPrereqs.Add(current)) // Si on l'ajoute (il n'y était pas déjà)
                {
                    var parents = GetPrerequisForMetier(current);
                    foreach (var parent in parents)
                    {
                        toExplore.Enqueue(parent);
                    }
                }
            }
            return allPrereqs;
        }

        #endregion

        #region Tri Topologique - Métiers (déplacé de MetierService)

        /// <summary>
        /// Retourne la liste des métiers ordonnée selon leurs dépendances (tri topologique), en utilisant QuikGraph.
        /// Les métiers sans dépendances apparaissent en premier. Gère la détection de cycles.
        /// </summary>
        /// <returns>Une liste ordonnée de métiers.</returns>
        public List<Metier> ObtenirMetiersTriesParDependance()
        {
            var graph = new AdjacencyGraph<string, Edge<string>>();
            var metiersCollection = _metiersInternes.Values;

            graph.AddVertexRange(metiersCollection.Select(m => m.MetierId));

            foreach (var metier in metiersCollection)
            {
                var prerequis = GetPrerequisForMetier(metier.MetierId);
                foreach (var prerequisId in prerequis)
                {
                    if (_metiersInternes.ContainsKey(prerequisId))
                    {
                        graph.AddEdge(new Edge<string>(prerequisId, metier.MetierId));
                    }
                }
            }

            try
            {
                var sortedIds = graph.TopologicalSort().ToList();
                return sortedIds.Select(id => _metiersInternes[id]).ToList();
            }
            catch (NonAcyclicGraphException)
            {
                // Une dépendance circulaire a été détectée entre les métiers.
                // On retourne une liste non triée pour éviter de planter l'UI.
                // Un mécanisme de logging ou de notification à l'utilisateur serait idéal ici.
                return metiersCollection.OrderBy(m => m.Nom).ToList();
            }
        }

        #endregion

        #region Couleurs - Métiers (déplacé de MetierService)

        /// <summary>
        /// Obtient la couleur d'affichage pour un métier.
        /// Priorité 1: Utilise la couleur personnalisée si elle est valide.
        /// Priorité 2: Attribue et mémorise une couleur de fallback unique.
        /// </summary>
        public Color GetDisplayColorForMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId))
            {
                return Color.MistyRose; // Couleur pour "non assigné"
            }

            var metier = GetMetierById(metierId);

            // Priorité 1: Couleur personnalisée
            if (metier != null && !string.IsNullOrEmpty(metier.CouleurHex))
            {
                try
                {
                    return ColorTranslator.FromHtml(metier.CouleurHex);
                }
                catch
                {
                    // La couleur est malformée, on passe au fallback
                }
            }

            // Priorité 2: Couleur de fallback
            if (!_assignedFallbackColors.ContainsKey(metierId))
            {
                _assignedFallbackColors[metierId] = FallbackColors[_fallbackColorIndex % FallbackColors.Length];
                _fallbackColorIndex++;
            }
            return _assignedFallbackColors[metierId];
        }

        #endregion

        #region Export/Import CSV groupé (Non fonctionnel - à traiter dans une phase ultérieure)

        /// <summary>
        /// Exporte toutes les données vers des fichiers CSV séparés
        /// </summary>
        public void ExporterToutVersCsv(string dossierDestination, string prefixeNom = "export")
        {
            throw new NotImplementedException("La fonctionnalité d'export CSV doit être revue pour la nouvelle structure de données.");
        }

        /// <summary>
        /// Importe toutes les données depuis des fichiers CSV
        /// </summary>
        public ResumeImport ImporterToutDepuisCsv(string cheminMetiers, string cheminOuvriers, string cheminTaches)
        {
            throw new NotImplementedException("La fonctionnalité d'import CSV doit être revue pour la nouvelle structure de données.");
        }

        #endregion

        #region Utilitaires

        /// <summary>
        /// Crée un nouveau projet vide
        /// </summary>
        public InformationsProjet CreerNouveauProjet(string nomProjet, string description = "")
        {
            // Vider toutes les données existantes
            // Utilise la méthode interne RemplacerTousLesMetiers()
            RemplacerTousLesMetiers(new List<Metier>());
            _ouvrierService.Vider();
            _tacheService.Vider();
            _lotService.Vider();
            _blocService.Vider();

            // Appel à ChargerMetiersParDefaut pour initialiser les métiers
            ChargerMetiersParDefaut();

            return new InformationsProjet
            {
                NomProjet = nomProjet,
                Description = description,
                DateCreation = DateTime.Now,
                DateDerniereModification = DateTime.Now,
                Auteur = Environment.UserName
            };
        }

        /// <summary>
        /// Obtient un résumé du projet actuel
        /// </summary>
        public ResumeProjet ObtenirResumeProjet()
        {
            return new ResumeProjet
            {
                // Utilise la méthode interne GetAllMetiers()
                NombreMetiers = GetAllMetiers().Count,
                StatistiquesOuvriers = _ouvrierService.ObtenirStatistiques(),
                StatistiquesTaches = _tacheService.ObtenirStatistiques(),
                StatistiquesMappingMetiers = _tacheService.ObtenirStatistiquesMappingMetiers()
            };
        }

        /// <summary>
        /// Charge les métiers par défaut depuis le fichier de configuration.
        /// </summary>
        private void ChargerMetiersParDefaut()
        {
            // Ne charger que si aucun métier n'est déjà là (pour les nouveaux projets)
            if (_metiersInternes.Any()) return;

            try
            {
                // Chemin du fichier de configuration à côté de l'exécutable
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "DefaultMetiersConfig.json");

                if (!File.Exists(filePath))
                {
                    
                    return;
                }

                string json = File.ReadAllText(filePath);
                var defaultMetiers = JsonSerializer.Deserialize<List<Metier>>(json);

                if (defaultMetiers != null)
                {
                    RemplacerTousLesMetiers(defaultMetiers);
                }
            }
            catch (Exception ex)
            {
                // Gérer l'erreur de chargement des métiers par défaut (log, message d'erreur si critique)
                Console.WriteLine($"Erreur lors du chargement des métiers par défaut: {ex.Message}");
            }
        }

        #endregion
    }

    #region Classes de support

    /// <summary>
    /// Informations générales sur un projet
    /// </summary>
    public class InformationsProjet
    {
        public string NomProjet { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DateCreation { get; set; }
        public DateTime DateDerniereModification { get; set; }
        public string Auteur { get; set; } = "";
    }

    /// <summary>
    /// Résultat de validation d'un projet
    /// </summary>
    public class ValidationProjet
    {
        public bool EstValide { get; set; }
        public List<string> Erreurs { get; set; } = new List<string>();
        public List<string> Avertissements { get; set; } = new List<string>();
        public InformationsProjet InformationsProjet { get; set; }
    }

    /// <summary>
    /// Résumé d'un import CSV groupé
    /// </summary>
    public class ResumeImport
    {
        public bool Succes { get; set; }
        public string MessageErreur { get; set; } = "";
        public int MetiersImportes { get; set; }
        public int OuvriersImportes { get; set; }
        public int TachesImportees { get; set; }
        public int TotalImporte => MetiersImportes + OuvriersImportes + TachesImportees;
    }

    /// <summary>
    /// Résumé complet d'un projet
    /// </summary>
    public class ResumeProjet
    {
        public int NombreMetiers { get; set; }
        public StatistiquesOuvriers StatistiquesOuvriers { get; set; } = new StatistiquesOuvriers();
        public StatistiquesTaches StatistiquesTaches { get; set; } = new StatistiquesTaches();
        public StatistiquesMappingMetiers StatistiquesMappingMetiers { get; set; } = new StatistiquesMappingMetiers();
    }

    /// <summary>
    /// Exception spécifique aux erreurs de projet
    /// </summary>
    public class ProjetException : Exception
    {
        public ProjetException(string message) : base(message) { }
        public ProjetException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion
}
