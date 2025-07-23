using PlanAthena.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;
using QuikGraph;
using QuikGraph.Algorithms;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Gère la connaissance et la logique métier liées aux métiers.
    /// Sert de point de vérité unique pour la liste des métiers et leurs dépendances.
    /// Utilise QuikGraph pour assurer un tri topologique robuste.
    /// </summary>
    public class MetierService
    {
        private readonly Dictionary<string, Metier> _metiers = new Dictionary<string, Metier>();

        public MetierService()
        {
        }

        #region CRUD Operations

        public void AjouterMetier(Metier nouveauMetier)
        {
            if (nouveauMetier == null)
                throw new ArgumentNullException(nameof(nouveauMetier));
            if (string.IsNullOrWhiteSpace(nouveauMetier.MetierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(nouveauMetier.MetierId));
            if (_metiers.ContainsKey(nouveauMetier.MetierId))
                throw new InvalidOperationException($"Un métier avec l'ID '{nouveauMetier.MetierId}' existe déjà.");

            _metiers.Add(nouveauMetier.MetierId, nouveauMetier);
        }

        public void ModifierMetier(string metierId, string nouveauNom, string nouveauxPrerequisIds, string couleurHex = null)
        {
            if (!_metiers.TryGetValue(metierId, out var metierAModifier))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            metierAModifier.Nom = nouveauNom;
            metierAModifier.PrerequisMetierIds = nouveauxPrerequisIds;

            if (couleurHex != null)
            {
                metierAModifier.CouleurHex = couleurHex;
            }
        }

        public void SupprimerMetier(string metierId)
        {
            if (!_metiers.Remove(metierId))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            foreach (var metier in _metiers.Values)
            {
                var prerequis = GetPrerequisForMetier(metier.MetierId).ToList();
                if (prerequis.Remove(metierId))
                {
                    metier.PrerequisMetierIds = string.Join(",", prerequis);
                }
            }
        }

        #endregion

        #region Data Loading and Retrieval

        public void RemplacerTousLesMetiers(IReadOnlyList<Metier> metiers)
        {
            _metiers.Clear();
            if (metiers != null)
            {
                foreach (var metier in metiers)
                {
                    if (!string.IsNullOrEmpty(metier.MetierId) && !_metiers.ContainsKey(metier.MetierId))
                    {
                        _metiers.Add(metier.MetierId, metier);
                    }
                }
            }
        }

        public IReadOnlyList<Metier> GetAllMetiers()
        {
            return _metiers.Values.ToList();
        }

        public Metier GetMetierById(string metierId)
        {
            _metiers.TryGetValue(metierId, out var metier);
            return metier;
        }

        public IReadOnlyList<string> GetPrerequisForMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId)) return Array.Empty<string>();

            if (_metiers.TryGetValue(metierId, out var metier) && !string.IsNullOrEmpty(metier.PrerequisMetierIds))
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

        #region Tri Topologique

        /// <summary>
        /// Retourne la liste des métiers ordonnée selon leurs dépendances (tri topologique), en utilisant QuikGraph.
        /// Les métiers sans dépendances apparaissent en premier. Gère la détection de cycles.
        /// </summary>
        /// <returns>Une liste ordonnée de métiers.</returns>
        public List<Metier> ObtenirMetiersTriesParDependance()
        {
            var graph = new AdjacencyGraph<string, Edge<string>>();
            var metiersCollection = _metiers.Values;

            graph.AddVertexRange(metiersCollection.Select(m => m.MetierId));

            foreach (var metier in metiersCollection)
            {
                var prerequis = GetPrerequisForMetier(metier.MetierId);
                foreach (var prerequisId in prerequis)
                {
                    if (_metiers.ContainsKey(prerequisId))
                    {
                        graph.AddEdge(new Edge<string>(prerequisId, metier.MetierId));
                    }
                }
            }

            try
            {
                var sortedIds = graph.TopologicalSort().ToList();
                return sortedIds.Select(id => _metiers[id]).ToList();
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

        #region Couleurs

        // Liste statique des couleurs de fallback, centralisée ICI
        private static readonly Color[] FallbackColors = {
            Color.LightBlue, Color.LightGreen, Color.LightYellow,
            Color.LightPink, Color.LightGray, Color.LightCyan,
            Color.LightSalmon
        };
        private int _fallbackColorIndex = 0;
        private readonly Dictionary<string, Color> _assignedFallbackColors = new Dictionary<string, Color>();

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
    }
}