using PlanAthena.Data;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Gère la connaissance et la logique métier liées aux métiers.
    /// Sert de point de vérité unique pour la liste des métiers.
    /// </summary>
    public class MetierService
    {
        private readonly Dictionary<string, MetierRecord> _metiers = new Dictionary<string, MetierRecord>();
        private const string JALON_METIER_ID = "JALON";

        public MetierService()
        {
            // S'assurer que le métier JALON existe toujours
            CreerMetierJalonSiNecessaire();
        }

        #region CRUD Operations

        /// <summary>
        /// Ajoute un nouveau métier.
        /// </summary>
        public void AjouterMetier(MetierRecord nouveauMetier)
        {
            if (nouveauMetier == null)
                throw new ArgumentNullException(nameof(nouveauMetier));
            if (string.IsNullOrWhiteSpace(nouveauMetier.MetierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(nouveauMetier.MetierId));
            if (_metiers.ContainsKey(nouveauMetier.MetierId))
                throw new InvalidOperationException($"Un métier avec l'ID '{nouveauMetier.MetierId}' existe déjà.");

            _metiers.Add(nouveauMetier.MetierId, nouveauMetier);
        }

        /// <summary>
        /// Ajoute un nouveau métier avec des paramètres individuels.
        /// </summary>
        public void AjouterMetier(string metierId, string nom, string prerequisMetierIds = "", string couleurHex = "")
        {
            if (string.IsNullOrWhiteSpace(metierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(metierId));
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom du métier ne peut pas être vide.", nameof(nom));
            if (_metiers.ContainsKey(metierId))
                throw new InvalidOperationException($"Un métier avec l'ID '{metierId}' existe déjà.");

            var nouveauMetier = new MetierRecord
            {
                MetierId = metierId,
                Nom = nom,
                PrerequisMetierIds = prerequisMetierIds ?? "",
                CouleurHex = couleurHex ?? ""
            };

            _metiers.Add(metierId, nouveauMetier);
        }

        /// <summary>
        /// Modifie un métier existant.
        /// </summary>
        public void ModifierMetier(string metierId, string nouveauNom, string nouveauxPrerequisIds, string couleurHex = null)
        {
            if (!_metiers.TryGetValue(metierId, out var metierAModifier))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            // Empêcher la modification du métier JALON
            if (metierId == JALON_METIER_ID)
                throw new InvalidOperationException("Le métier JALON ne peut pas être modifié.");

            metierAModifier.Nom = nouveauNom;
            metierAModifier.PrerequisMetierIds = nouveauxPrerequisIds;

            // Ne modifier la couleur que si elle est explicitement fournie
            if (couleurHex != null)
            {
                metierAModifier.CouleurHex = couleurHex;
            }
        }

        /// <summary>
        /// Modifie uniquement la couleur d'un métier existant.
        /// </summary>
        public void ModifierCouleurMetier(string metierId, string couleurHex)
        {
            if (!_metiers.TryGetValue(metierId, out var metierAModifier))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            // Empêcher la modification du métier JALON
            if (metierId == JALON_METIER_ID)
                throw new InvalidOperationException("La couleur du métier JALON ne peut pas être modifiée.");

            metierAModifier.CouleurHex = couleurHex ?? "";
        }

        /// <summary>
        /// Supprime un métier.
        /// </summary>
        public void SupprimerMetier(string metierId)
        {
            // Empêcher la suppression du métier JALON
            if (metierId == JALON_METIER_ID)
                throw new InvalidOperationException("Le métier JALON ne peut pas être supprimé.");

            if (!_metiers.Remove(metierId))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            // Il faut aussi supprimer ce métier des prérequis des autres métiers
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

        /// <summary>
        /// Remplace la liste complète des métiers. Principalement pour l'import CSV.
        /// </summary>
        public void RemplacerTousLesMetiers(IReadOnlyList<MetierRecord> metiers)
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

            // S'assurer que le métier JALON existe toujours après le remplacement
            CreerMetierJalonSiNecessaire();
        }

        /// <summary>
        /// Retourne une copie de la liste de tous les métiers pour éviter les modifications externes.
        /// </summary>
        public IReadOnlyList<MetierRecord> GetAllMetiers()
        {
            return _metiers.Values.ToList();
        }

        /// <summary>
        /// Obtient un métier par son ID.
        /// </summary>
        public MetierRecord GetMetierById(string metierId)
        {
            _metiers.TryGetValue(metierId, out var metier);
            return metier;
        }

        /// <summary>
        /// Retourne les prérequis pour un métier donné.
        /// </summary>
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
        /// Fournit l'ID d'un métier de synchronisation/attente, en le créant s'il n'existe pas.
        /// </summary>
        public string GetOrCreateSyncMetierId(int dureeEnHeures)
        {
            string metierId = $"SYNC_{dureeEnHeures}H";

            if (_metiers.ContainsKey(metierId))
            {
                return metierId;
            }

            var syncMetier = new MetierRecord
            {
                MetierId = metierId,
                Nom = $"Tâche de Synchronisation/Attente ({dureeEnHeures}h)",
                PrerequisMetierIds = "",
                CouleurHex = "#FFE4B5" // Couleur par défaut pour les tâches de synchronisation (beige clair)
            };

            _metiers.Add(metierId, syncMetier);
            return metierId;
        }

        #endregion

        #region Gestion du métier JALON

        /// <summary>
        /// Vérifie si une tâche est un jalon.
        /// </summary>
        public bool EstJalon(TacheRecord tache)
        {
            return tache?.MetierId == JALON_METIER_ID;
        }

        /// <summary>
        /// Vérifie si un MetierId correspond à un jalon.
        /// </summary>
        public bool EstJalon(string metierId)
        {
            return metierId == JALON_METIER_ID;
        }

        /// <summary>
        /// Retourne l'ID du métier JALON.
        /// </summary>
        public string GetJalonMetierId()
        {
            return JALON_METIER_ID;
        }

        /// <summary>
        /// Crée le métier JALON s'il n'existe pas.
        /// </summary>
        private void CreerMetierJalonSiNecessaire()
        {
            if (!_metiers.ContainsKey(JALON_METIER_ID))
            {
                var metierJalon = new MetierRecord
                {
                    MetierId = JALON_METIER_ID,
                    Nom = "Jalon/Attente",
                    PrerequisMetierIds = "",
                    CouleurHex = "#FFA500" // Orange pour les jalons
                };
                _metiers.Add(JALON_METIER_ID, metierJalon);
            }
        }

        #endregion

        #region Utilitaires pour les couleurs

        /// <summary>
        /// Valide qu'une couleur hexadécimale est au bon format.
        /// </summary>
        public bool EstCouleurValide(string couleurHex)
        {
            if (string.IsNullOrEmpty(couleurHex))
                return true; // Une couleur vide est considérée comme valide

            try
            {
                System.Drawing.ColorTranslator.FromHtml(couleurHex);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtient la couleur d'un métier ou une couleur par défaut si non définie.
        /// </summary>
        public string GetCouleurMetier(string metierId, string couleurParDefaut = "#E0E0E0")
        {
            if (_metiers.TryGetValue(metierId, out var metier) && !string.IsNullOrEmpty(metier.CouleurHex))
            {
                return metier.CouleurHex;
            }
            return couleurParDefaut;
        }

        /// <summary>
        /// Retourne les métiers qui ont une couleur personnalisée définie.
        /// </summary>
        public IReadOnlyList<MetierRecord> GetMetiersAvecCouleur()
        {
            return _metiers.Values.Where(m => !string.IsNullOrEmpty(m.CouleurHex)).ToList();
        }

        /// <summary>
        /// Retourne les métiers qui n'ont pas de couleur personnalisée définie.
        /// </summary>
        public IReadOnlyList<MetierRecord> GetMetiersSansCouleur()
        {
            return _metiers.Values.Where(m => string.IsNullOrEmpty(m.CouleurHex)).ToList();
        }

        #endregion

        #region Suggestions métier (pour le chef)

        /// <summary>
        /// Propose des dépendances métier pour une tâche (le chef peut les ignorer)
        /// </summary>
        public List<string> SuggererDependancesMetier(TacheRecord tache, IReadOnlyList<TacheRecord> toutesLesTaches)
        {
            if (tache == null || string.IsNullOrEmpty(tache.MetierId) || EstJalon(tache))
                return new List<string>();

            var suggestions = new List<string>();
            var prerequisMetiers = GetPrerequisForMetier(tache.MetierId);

            foreach (var prerequisMetier in prerequisMetiers)
            {
                // Trouver les tâches du même bloc qui utilisent ce métier prérequis
                var tachesPrerequisBloc = toutesLesTaches
                    .Where(t => t.BlocId == tache.BlocId &&
                               t.MetierId == prerequisMetier &&
                               t.TacheId != tache.TacheId)
                    .Select(t => t.TacheId)
                    .ToList();

                suggestions.AddRange(tachesPrerequisBloc);
            }

            return suggestions.Distinct().ToList();
        }

        #endregion
    }

}