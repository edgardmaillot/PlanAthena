using PlanAthena.Data;
using System.Linq;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service source de vérité pour la gestion des Blocs.
    /// </summary>
    public class BlocService
    {
        private readonly Dictionary<string, Bloc> _blocs = new Dictionary<string, Bloc>();
        private readonly Func<TacheService> _tacheServiceFactory;

        public BlocService(Func<TacheService> tacheServiceFactory)
        {
            _tacheServiceFactory = tacheServiceFactory ?? throw new ArgumentNullException(nameof(tacheServiceFactory));
        }

        /// <summary>
        /// Gère la création ou la mise à jour d'un objet Bloc.
        /// Vérifie si l'ID du bloc existe déjà pour déterminer s'il s'agit d'une création ou d'une mise à jour.
        /// </summary>
        /// <param name="bloc">Le bloc à sauvegarder</param>
        /// <exception cref="ArgumentNullException">Si le bloc est null</exception>
        /// <exception cref="ArgumentException">Si le nom du bloc est vide ou si la capacité maximale est invalide</exception>
        public void SaveBloc(Bloc bloc)
        {
            if (bloc == null) throw new ArgumentNullException(nameof(bloc));
            if (string.IsNullOrWhiteSpace(bloc.Nom)) throw new ArgumentException("Le nom du bloc ne peut pas être vide.", nameof(bloc));
            if (bloc.CapaciteMaxOuvriers <= 0) throw new ArgumentException("La capacité maximale d'ouvriers doit être supérieure à zéro.", nameof(bloc));

            if (_blocs.ContainsKey(bloc.BlocId))
            {
                _blocs[bloc.BlocId] = bloc;
            }
            else
            {
                _blocs.Add(bloc.BlocId, bloc);
            }
        }

        /// <summary>
        /// Génère un nouvel identifiant unique pour un bloc au format {LotId}_B00X.
        /// Cette méthode implémente directement la logique de génération pour éviter la dépendance circulaire.
        /// </summary>
        /// <param name="lotId">L'identifiant du lot parent</param>
        /// <returns>Le nouvel identifiant de bloc généré</returns>
        public string GenerateNewBlocId(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId))
                throw new ArgumentException("L'ID du lot ne peut pas être vide.", nameof(lotId));

            // Trouver tous les blocs existants pour ce lot
            var blocsExistants = _blocs.Keys
                .Where(id => id.StartsWith($"{lotId}_B"))
                .ToList();

            // Extraire les numéros existants
            var numerosExistants = new HashSet<int>();
            foreach (var blocId in blocsExistants)
            {
                var partie = blocId.Substring($"{lotId}_B".Length);
                if (int.TryParse(partie, out int numero))
                {
                    numerosExistants.Add(numero);
                }
            }

            // Trouver le prochain numéro disponible
            int prochainNumero = 1;
            while (numerosExistants.Contains(prochainNumero))
            {
                prochainNumero++;
            }

            // Formatter avec padding de 3 chiffres : B001, B002, etc.
            return $"{lotId}_B{prochainNumero:D3}";
        }

        public Bloc ObtenirBlocParId(string blocId)
        {
            _blocs.TryGetValue(blocId, out var bloc);
            return bloc;
        }

        public List<Bloc> ObtenirTousLesBlocs()
        {
            return _blocs.Values.OrderBy(b => b.Nom).ToList();
        }

        public List<Bloc> ObtenirBlocsParLot(string lotId)
        {
            if (string.IsNullOrEmpty(lotId))
            {
                return new List<Bloc>(); // Retourne une liste vide si aucun lotId n'est fourni
            }
            // Assurez-vous que le format de l'ID du bloc correspond à celui de la génération
            return _blocs.Values
                         .Where(b => b.BlocId.StartsWith($"{lotId}"))
                         .OrderBy(b => b.Nom)
                         .ToList();
        }

        /// <summary>
        /// Supprime un bloc après vérification qu'aucune tâche ne lui est associée.
        /// Utilise une factory pour éviter la dépendance circulaire avec TacheService.
        /// </summary>
        /// <param name="blocId">L'identifiant du bloc à supprimer</param>
        /// <exception cref="KeyNotFoundException">Si le bloc n'existe pas</exception>
        /// <exception cref="InvalidOperationException">Si des tâches sont associées au bloc</exception>
        public void SupprimerBloc(string blocId)
        {
            if (!_blocs.ContainsKey(blocId))
            {
                throw new KeyNotFoundException($"Bloc {blocId} non trouvé.");
            }

            // Utilisation de la factory pour éviter la dépendance circulaire
            var tacheService = _tacheServiceFactory();
            if (tacheService.ObtenirTachesParBloc(blocId).Any())
            {
                throw new InvalidOperationException($"Impossible de supprimer le bloc '{blocId}' car il est utilisé par une ou plusieurs tâches. Veuillez d'abord supprimer ou réassigner les tâches associées.");
            }

            _blocs.Remove(blocId);
        }

        /// <summary>
        /// Remplace l'ensemble des blocs gérés par le service.
        /// S'adapte à la structure simplifiée du Bloc.
        /// </summary>
        /// <param name="blocs">La nouvelle liste de blocs</param>
        public void RemplacerTousLesBlocs(List<Bloc> blocs)
        {
            _blocs.Clear();
            if (blocs != null)
            {
                foreach (var bloc in blocs)
                {
                    if (!string.IsNullOrWhiteSpace(bloc.BlocId) && !_blocs.ContainsKey(bloc.BlocId))
                    {
                        _blocs.Add(bloc.BlocId, bloc);
                    }
                }
            }
        }

        public void Vider()
        {
            _blocs.Clear();
        }
    }
}