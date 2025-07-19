using PlanAthena.Data;

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
                    if (!_metiers.ContainsKey(metier.MetierId))
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


        // Liste statique des couleurs de fallback, centralisée ICI
        // Utilisé dans PertDiagramSettings.cs pour les tâches métier 
        private static readonly Color[] FallbackColors = {
        Color.LightBlue, Color.LightGreen, Color.LightYellow,
        Color.LightPink, Color.LightGray, Color.LightCyan,
        Color.LightSalmon
    };
        private int _fallbackColorIndex = 0;
        private readonly Dictionary<string, Color> _assignedFallbackColors = new Dictionary<string, Color>();

        /// <summary>
        /// NOUVELLE MÉTHODE : Obtient la couleur d'affichage pour un métier.
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

    }
}