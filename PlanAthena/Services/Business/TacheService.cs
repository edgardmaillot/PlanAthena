using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly MetierService _metierService;
        private readonly LotService _lotService;
        private readonly BlocService _blocService;

        // Compteurs pour génération d'ID automatique
        private int _compteurTaches = 1;
        private int _compteurJalons = 1;

        public TacheService(CsvDataService csvDataService, ExcelReader excelReader, MetierService metierService, LotService lotService, BlocService blocService)
        {
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _excelReader = excelReader ?? throw new ArgumentNullException(nameof(excelReader));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
        }

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
            if (!string.IsNullOrEmpty(tache.LotId) && _lotService.ObtenirLotParId(tache.LotId) == null)
                throw new InvalidOperationException($"Le lot avec l'ID '{tache.LotId}' n'existe pas. Veuillez le créer avant d'ajouter la tâche.");

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
            if (!string.IsNullOrEmpty(tacheModifiee.LotId) && _lotService.ObtenirLotParId(tacheModifiee.LotId) == null)
                throw new InvalidOperationException($"Le lot avec l'ID '{tacheModifiee.LotId}' n'existe pas.");

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
        private string GenererIdUnique(Tache tache)
        {
            string prefixe;
            int compteur;

            if (tache.EstJalon)
            {
                prefixe = "J";
                compteur = _compteurJalons++;
            }
            else
            {
                prefixe = "T";
                compteur = _compteurTaches++;
            }

            string idCandidat;
            do
            {
                idCandidat = $"{prefixe}{compteur:D3}";
                if (!_taches.Any(t => t.TacheId == idCandidat))
                    break;
                compteur++;
            } while (true);

            if (tache.EstJalon)
                _compteurJalons = compteur + 1;
            else
                _compteurTaches = compteur + 1;

            return idCandidat;
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
                .Where(t => t.TacheId?.StartsWith("T") == true && int.TryParse(t.TacheId.Substring(1), out _))
                .Select(t => int.Parse(t.TacheId.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();

            var maxJalon = _taches
                .Where(t => t.TacheId?.StartsWith("J") == true && int.TryParse(t.TacheId.Substring(1), out _))
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
        /// Importe les tâches depuis un fichier CSV.
        /// ATTENTION: Cette méthode n'est plus fonctionnelle et doit être ré-implémentée.
        /// </summary>
        public int ImporterDepuisCsv(string filePath, bool remplacerExistantes = true)
        {
            // La logique de cette méthode doit être entièrement revue pour gérer la création
            // implicite des lots et blocs, comme discuté dans notre conception.
            throw new NotImplementedException("L'import CSV doit être ré-implémenté pour gérer la nouvelle structure de Lots/Blocs.");
        }

        /// <summary>
        /// Exporte les tâches vers un fichier CSV.
        /// ATTENTION: Cette méthode n'est plus fonctionnelle et doit être ré-implémentée.
        /// </summary>
        public void ExporterVersCsv(string filePath)
        {
            // La logique de cette méthode doit être entièrement revue pour enrichir les données
            // de tâches avec les informations de lots et blocs avant l'export.
            throw new NotImplementedException("L'export CSV doit être ré-implémenté pour gérer la nouvelle structure de Lots/Blocs.");
        }

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