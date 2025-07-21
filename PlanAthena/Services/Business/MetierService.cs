using PlanAthena.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Gère la connaissance et la logique métier liées aux métiers.
    /// Sert de point de vérité unique pour la liste des métiers.
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

        #endregion

        #region Tri Topologique

        /// <summary>
        /// Retourne la liste des métiers ordonnée selon leurs dépendances (tri topologique).
        /// Les métiers sans dépendances apparaissent en premier.
        /// </summary>
        /// <returns>Une liste ordonnée de métiers.</returns>
        public List<Metier> ObtenirMetiersTriesParDependance()
        {
            var sortedList = new List<Metier>();
            var inDegree = new Dictionary<string, int>();
            var graph = new Dictionary<string, List<string>>();
            var allMetierIds = _metiers.Keys;

            foreach (var metierId in allMetierIds)
            {
                inDegree[metierId] = 0;
                graph[metierId] = new List<string>();
            }

            foreach (var metier in _metiers.Values)
            {
                var prerequis = GetPrerequisForMetier(metier.MetierId);
                foreach (var prerequisId in prerequis)
                {
                    if (allMetierIds.Contains(prerequisId))
                    {
                        graph[prerequisId].Add(metier.MetierId);
                        inDegree[metier.MetierId]++;
                    }
                }
            }

            var queue = new Queue<string>(allMetierIds.Where(m => inDegree[m] == 0));

            while (queue.Count > 0)
            {
                var metierId = queue.Dequeue();
                sortedList.Add(_metiers[metierId]);

                foreach (var neighbor in graph[metierId])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Si la liste triée ne contient pas tous les métiers, il y a une dépendance circulaire.
            // On ajoute les métiers restants à la fin pour éviter de planter.
            if (sortedList.Count < _metiers.Count)
            {
                var metiersManquants = _metiers.Values.Where(m => !sortedList.Contains(m));
                sortedList.AddRange(metiersManquants);
            }

            return sortedList;
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