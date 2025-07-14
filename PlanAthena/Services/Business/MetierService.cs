// Fichier : Services/Business/MetierService.cs

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
        /// Modifie un métier existant.
        /// </summary>
        public void ModifierMetier(string metierId, string nouveauNom, string nouveauxPrerequisIds)
        {
            if (!_metiers.TryGetValue(metierId, out var metierAModifier))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            metierAModifier.Nom = nouveauNom;
            metierAModifier.PrerequisMetierIds = nouveauxPrerequisIds;
        }

        /// <summary>
        /// Supprime un métier.
        /// </summary>
        public void SupprimerMetier(string metierId)
        {
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
            if (metiers == null) return;

            foreach (var metier in metiers)
            {
                if (!_metiers.ContainsKey(metier.MetierId))
                {
                    _metiers.Add(metier.MetierId, metier);
                }
            }
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
                PrerequisMetierIds = ""
            };

            _metiers.Add(metierId, syncMetier);
            return metierId;
        }

        #endregion
    }
}