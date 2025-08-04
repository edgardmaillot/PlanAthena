using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using System; // Ajouté pour ArgumentNullException, InvalidOperationException, Func
using System.Collections.Generic; // Ajouté pour List
using System.Linq; // Ajouté pour Any, FirstOrDefault, Select, DefaultIfEmpty, Max

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// SERVICE SOURCE DE VÉRITÉ - Gestion simplifiée des tâches
    /// PRINCIPE: Ce que décide le chef ici est respecté partout
    /// </summary>
    public class TacheService
    {
        private readonly List<Tache> _taches = new List<Tache>();
        private readonly CsvDataService _csvDataService;
        private readonly ExcelReader _excelReader;
        private readonly Func<ProjetService> _projetServiceFactory; // Changement ici: ProjetService devient une factory
        private readonly Func<BlocService> _blocServiceFactory; // Changement ici: BlocService devient une factory

        // Compteurs pour génération d'ID automatique
        private int _compteurTaches = 1;
        private int _compteurJalons = 1;

        public TacheService(
            CsvDataService csvDataService,
            ExcelReader excelReader,
            Func<ProjetService> projetServiceFactory, // Prend une factory
            Func<BlocService> blocServiceFactory) // Prend une factory
        {
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _excelReader = excelReader ?? throw new ArgumentNullException(nameof(excelReader));
            _projetServiceFactory = projetServiceFactory ?? throw new ArgumentNullException(nameof(projetServiceFactory)); // Assigner la factory
            _blocServiceFactory = blocServiceFactory ?? throw new ArgumentNullException(nameof(blocServiceFactory)); // Assigner la factory
        }

        // Propriété pour accéder à l'instance de ProjetService de manière paresseuse
        private ProjetService _projetService => _projetServiceFactory();
        // Propriété pour accéder à l'instance de BlocService de manière paresseuse
        private BlocService _blocService => _blocServiceFactory();

        #region CRUD Tâches - SIMPLIFIÉ

        /// <summary>
        /// Ajoute une nouvelle tâche avec génération automatique d'ID si nécessaire
        /// </summary>
        public void AjouterTache(Tache tache)
        {
            if (tache == null)
                throw new ArgumentNullException(nameof(tache));

            // Génération automatique d'ID si vide
            if (string.IsNullOrWhiteSpace(tache.TacheId))
            {
                tache.TacheId = GenererIdUnique(tache);
            }

            // Vérifier unicité
            if (_taches.Any(t => t.TacheId == tache.TacheId))
            {
                throw new InvalidOperationException($"La tâche {tache.TacheId} existe déjà.");
            }

            // Validation de l'existence des IDs de référence
            if (!string.IsNullOrEmpty(tache.LotId) && _projetService.ObtenirLotParId(tache.LotId) == null)
                throw new InvalidOperationException($"Le lot avec l'ID '{tache.LotId}' n'existe pas. Veuillez le créer avant d'ajouter la tâche.");

            // Utilise la propriété _blocService
            if (!string.IsNullOrEmpty(tache.BlocId) && _blocService.ObtenirBlocParId(tache.BlocId) == null)
                throw new InvalidOperationException($"Le bloc avec l'ID '{tache.BlocId}' n'existe pas. Veuillez le créer avant d'ajouter la tâche.");

            _taches.Add(tache);
        }

        /// <summary>
        /// Met à jour une tâche existante
        /// </summary>
        public void ModifierTache(Tache tacheModifiee)
        {
            if (tacheModifiee == null)
                throw new ArgumentNullException(nameof(tacheModifiee));

            var tacheExistante = _taches.FirstOrDefault(t => t.TacheId == tacheModifiee.TacheId);
            if (tacheExistante == null)
                throw new InvalidOperationException($"Tâche {tacheModifiee.TacheId} non trouvée.");

            // Validation de l'existence des IDs de référence
            if (!string.IsNullOrEmpty(tacheModifiee.LotId) && _projetService.ObtenirLotParId(tacheModifiee.LotId) == null)
                throw new InvalidOperationException($"Le lot avec l'ID '{tacheModifiee.LotId}' n'existe pas.");

            // Utilise la propriété _blocService
            if (!string.IsNullOrEmpty(tacheModifiee.BlocId) && _blocService.ObtenirBlocParId(tacheModifiee.BlocId) == null)
                throw new InvalidOperationException($"Le bloc avec l'ID '{tacheModifiee.BlocId}' n'existe pas.");

            // Mise à jour des propriétés de la tâche
            tacheExistante.TacheNom = tacheModifiee.TacheNom;
            tacheExistante.HeuresHommeEstimees = tacheModifiee.HeuresHommeEstimees;
            tacheExistante.MetierId = tacheModifiee.MetierId;
            tacheExistante.Dependencies = tacheModifiee.Dependencies;
            tacheExistante.ExclusionsDependances = tacheModifiee.ExclusionsDependances;
            tacheExistante.LotId = tacheModifiee.LotId;
            tacheExistante.BlocId = tacheModifiee.BlocId;
        }

        /// <summary>
        /// Supprime une tâche
        /// </summary>
        public void SupprimerTache(string tacheId)
        {
            if (string.IsNullOrWhiteSpace(tacheId))
                throw new ArgumentException("L'ID de la tâche ne peut pas être vide.");

            var tache = _taches.FirstOrDefault(t => t.TacheId == tacheId);
            if (tache == null)
                throw new InvalidOperationException($"Tâche {tacheId} non trouvée.");

            // Vérifier les dépendances avant suppression
            var tachesDependantes = _taches.Where(t =>
                !string.IsNullOrEmpty(t.Dependencies) &&
                t.Dependencies.Split(',').Any(dep => dep.Trim() == tacheId)).ToList();

            if (tachesDependantes.Any())
            {
                var nomsTaches = string.Join(", ", tachesDependantes.Select(t => t.TacheId));
                throw new InvalidOperationException($"Impossible de supprimer la tâche {tacheId}. Elle est référencée par: {nomsTaches}");
            }

            _taches.Remove(tache);
        }

        #endregion

        #region Génération d'ID automatique

        /// <summary>
        /// Génère un ID unique pour une nouvelle tâche ou jalon
        /// </summary>
        public string GenererIdUnique(string blocId, TypeActivite type)
        {
            // La logique de génération d'ID est maintenant dans IdGeneratorService
            // TacheService doit utiliser IdGeneratorService.
            // Pour l'instant, comme IdGeneratorService n'est pas une dépendance directe de TacheService (pour éviter le cycle),
            // nous allons faire une version simplifiée, ou passer par ProjetService si c'est la source de génération d'IDs.

            // Si TacheService doit générer l'ID, il doit le faire lui-même ou via une factory.
            // Dans votre plan, IdGeneratorService a été modifié pour avoir GenererProchainTacheId.
            // La façon la plus simple est que ProjetService (ou MainForm) appelle IdGeneratorService et passe l'ID à TacheService.
            // Pour cette méthode interne, nous devrions avoir une logique simple ou une dépendance à IdGeneratorService.

            // Pour l'instant, je vais réimplémenter la logique de TacheService avec les compteurs internes,
            // car IdGeneratorService n'est pas injecté ici directement pour éviter la complexité des cycles sur plusieurs niveaux.
            // Cela duplique un peu la logique, mais est plus simple pour briser le cycle ici.

            string prefixe = (type == TypeActivite.Tache) ? "T" : "J";
            int compteur = (type == TypeActivite.Tache) ? _compteurTaches : _compteurJalons;

            string idCandidat;
            do
            {
                idCandidat = $"{blocId}_{prefixe}{compteur:D3}";
                if (!_taches.Any(t => t.TacheId == idCandidat))
                    break;
                compteur++;
            } while (true);

            if (type == TypeActivite.Tache)
                _compteurTaches = compteur + 1;
            else
                _compteurJalons = compteur + 1;

            return idCandidat;
        }

        /// <summary>
        /// Génère un ID unique pour une nouvelle tâche ou jalon (anciennement GenererIdUnique(Tache tache))
        /// Cette version est une surcharge pour compatibilité.
        /// </summary>
        private string GenererIdUnique(Tache tache)
        {
            return GenererIdUnique(tache.BlocId, tache.Type);
        }

        /// <summary>
        /// Met à jour les compteurs basés sur les IDs existants
        /// </summary>
        private void MettreAJourCompteurs()
        {
            if (!_taches.Any())
            {
                _compteurTaches = 1;
                _compteurJalons = 1;
                return;
            }

            var maxTache = _taches
                .Where(t => t.TacheId?.StartsWith("T") == true && int.TryParse(t.TacheId.Substring(1), out _)) // Vérifier le préfixe
                .Select(t => int.Parse(t.TacheId.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();

            var maxJalon = _taches
                .Where(t => t.TacheId?.StartsWith("J") == true && int.TryParse(t.TacheId.Substring(1), out _)) // Vérifier le préfixe
                .Select(t => int.Parse(t.TacheId.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();

            _compteurTaches = maxTache + 1;
            _compteurJalons = maxJalon + 1;
        }

        #endregion

        #region Consultation Tâches

        /// <summary>
        /// SOURCE DE VÉRITÉ: Obtient toutes les tâches définies par le chef
        /// </summary>
        public List<Tache> ObtenirToutesLesTaches()
        {
            return _taches.ToList();
        }

        /// <summary>
        /// Obtient une tâche par son ID
        /// </summary>
        public Tache ObtenirTacheParId(string tacheId)
        {
            return _taches.FirstOrDefault(t => t.TacheId == tacheId);
        }

        /// <summary>
        /// Obtient les tâches d'un bloc spécifique
        /// </summary>
        public List<Tache> ObtenirTachesParBloc(string blocId)
        {
            return _taches.Where(t => t.BlocId == blocId).ToList();
        }

        /// <summary>
        /// Obtient les tâches d'un lot spécifique
        /// </summary>
        public List<Tache> ObtenirTachesParLot(string lotId)
        {
            return _taches.Where(t => t.LotId == lotId).ToList();
        }

        #endregion

        #region Import/Export (Non fonctionnel après refonte - à traiter dans une phase ultérieure)

        /// <summary>
        /// Charge les tâches depuis une liste (utilisé par ProjetService)
        /// </summary>
        public void ChargerTaches(List<Tache> taches)
        {
            _taches.Clear();
            if (taches != null)
            {
                _taches.AddRange(taches);
                MettreAJourCompteurs();
            }
        }

        #endregion

        #region Statistiques

        /// <summary>
        /// Obtient des statistiques sur les tâches du chef
        /// </summary>
        public StatistiquesTaches ObtenirStatistiques()
        {
            if (!_taches.Any())
            {
                return new StatistiquesTaches();
            }

            return new StatistiquesTaches
            {
                NombreTachesTotal = _taches.Count,
                HeuresHommeTotal = _taches.Sum(t => t.HeuresHommeEstimees),

                HeuresHommeMoyenneParTache = _taches.Any(t => !t.EstJalon)
                    ? _taches.Where(t => !t.EstJalon).Average(t => t.HeuresHommeEstimees)
                    : 0,
                NombreBlocsUniques = _taches.Select(t => t.BlocId).Distinct().Count(),
                NombreLotsUniques = _taches.Select(t => t.LotId).Distinct().Count(),
                TachesAvecDependances = _taches.Count(t => !string.IsNullOrWhiteSpace(t.Dependencies)),
                JalonsSurcharge = _taches.Count(t => t.Type == TypeActivite.JalonUtilisateur)
            };
        }

        /// <summary>
        /// Obtient les statistiques de mapping
        /// </summary>
        public StatistiquesMappingMetiers ObtenirStatistiquesMappingMetiers()
        {
            var tachesAvecMetier = _taches.Count(t => !string.IsNullOrWhiteSpace(t.MetierId));
            var tachesSansMetier = _taches.Count - tachesAvecMetier;

            return new StatistiquesMappingMetiers
            {
                TotalTaches = _taches.Count,
                TachesAvecMetier = tachesAvecMetier,
                TachesSansMetier = tachesSansMetier,
                PourcentageMapping = _taches.Count > 0 ? (double)tachesAvecMetier / _taches.Count * 100 : 0
            };
        }

        #endregion

        /// <summary>
        /// Efface toutes les données
        /// </summary>
        public void Vider()
        {
            _taches.Clear();
            _compteurTaches = 1;
            _compteurJalons = 1;
        }
    }

    #region Classes de support

    /// <summary>
    /// Statistiques sur les tâches
    /// </summary>
    public class StatistiquesTaches
    {
        public int NombreTachesTotal { get; set; }
        public int HeuresHommeTotal { get; set; }
        public double HeuresHommeMoyenneParTache { get; set; }
        public int NombreBlocsUniques { get; set; }
        public int NombreLotsUniques { get; set; }
        public int TachesAvecDependances { get; set; }
        public int JalonsSurcharge { get; set; }
    }

    /// <summary>
    /// Statistiques sur le mapping tâches-métiers
    /// </summary>
    public class StatistiquesMappingMetiers
    {
        public int TotalTaches { get; set; }
        public int TachesAvecMetier { get; set; }
        public int TachesSansMetier { get; set; }
        public double PourcentageMapping { get; set; }
    }

    #endregion
}