using PlanAthena.Data;
using PlanAthena.Services.DataAccess;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// SERVICE SOURCE DE VÉRITÉ - Gestion simplifiée des tâches
    /// PRINCIPE: Ce que décide le chef ici est respecté partout
    /// </summary>
    public class TacheService
    {
        private readonly List<TacheRecord> _taches = new List<TacheRecord>();
        private readonly CsvDataService _csvDataService;
        private readonly ExcelReader _excelReader;
        private readonly MetierService _metierService;

        // Compteurs pour génération d'ID automatique
        private int _compteurTaches = 1;
        private int _compteurJalons = 1;

        public TacheService(CsvDataService csvDataService, ExcelReader excelReader, MetierService metierService)
        {
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _excelReader = excelReader ?? throw new ArgumentNullException(nameof(excelReader));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
        }

        #region CRUD Tâches - SIMPLIFIÉ

        /// <summary>
        /// Ajoute une nouvelle tâche avec génération automatique d'ID si nécessaire
        /// </summary>
        public void AjouterTache(TacheRecord tache)
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

            _taches.Add(tache);
        }

        /// <summary>
        /// Met à jour une tâche existante
        /// </summary>
        public void ModifierTache(TacheRecord tacheModifiee)
        {
            if (tacheModifiee == null)
                throw new ArgumentNullException(nameof(tacheModifiee));

            var tacheExistante = _taches.FirstOrDefault(t => t.TacheId == tacheModifiee.TacheId);
            if (tacheExistante == null)
                throw new InvalidOperationException($"Tâche {tacheModifiee.TacheId} non trouvée.");

            // Mise à jour de TOUTES les propriétés
            tacheExistante.TacheNom = tacheModifiee.TacheNom;
            tacheExistante.HeuresHommeEstimees = tacheModifiee.HeuresHommeEstimees;
            tacheExistante.MetierId = tacheModifiee.MetierId;
            tacheExistante.Dependencies = tacheModifiee.Dependencies;
            tacheExistante.ExclusionsDependances = tacheModifiee.ExclusionsDependances;
            tacheExistante.LotId = tacheModifiee.LotId;
            tacheExistante.LotNom = tacheModifiee.LotNom;
            tacheExistante.LotPriorite = tacheModifiee.LotPriorite;
            tacheExistante.BlocId = tacheModifiee.BlocId;
            tacheExistante.BlocNom = tacheModifiee.BlocNom;
            tacheExistante.BlocCapaciteMaxOuvriers = tacheModifiee.BlocCapaciteMaxOuvriers;
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
        private string GenererIdUnique(TacheRecord tache)
        {
            string prefixe;
            int compteur;

            if (_metierService.EstJalon(tache))
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

            // Mettre à jour le compteur pour la prochaine génération
            if (_metierService.EstJalon(tache))
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
            var maxTache = 0;
            var maxJalon = 0;

            foreach (var tache in _taches)
            {
                if (tache.TacheId?.StartsWith("T") == true &&
                    int.TryParse(tache.TacheId.Substring(1), out var numTache))
                {
                    maxTache = Math.Max(maxTache, numTache);
                }
                else if (tache.TacheId?.StartsWith("J") == true &&
                         int.TryParse(tache.TacheId.Substring(1), out var numJalon))
                {
                    maxJalon = Math.Max(maxJalon, numJalon);
                }
            }

            _compteurTaches = maxTache + 1;
            _compteurJalons = maxJalon + 1;
        }

        #endregion

        #region Consultation Tâches

        /// <summary>
        /// SOURCE DE VÉRITÉ: Obtient toutes les tâches définies par le chef
        /// </summary>
        public List<TacheRecord> ObtenirToutesLesTaches()
        {
            return _taches.ToList();
        }

        /// <summary>
        /// Obtient une tâche par son ID
        /// </summary>
        public TacheRecord ObtenirTacheParId(string tacheId)
        {
            return _taches.FirstOrDefault(t => t.TacheId == tacheId);
        }

        /// <summary>
        /// Obtient les tâches d'un bloc spécifique
        /// </summary>
        public List<TacheRecord> ObtenirTachesParBloc(string blocId)
        {
            return _taches.Where(t => t.BlocId == blocId).ToList();
        }

        /// <summary>
        /// Obtient les tâches d'un lot spécifique
        /// </summary>
        public List<TacheRecord> ObtenirTachesParLot(string lotId)
        {
            return _taches.Where(t => t.LotId == lotId).ToList();
        }

        /// <summary>
        /// Obtient les tâches ayant des dépendances non résolues
        /// </summary>
        public List<TacheRecord> ObtenirTachesAvecDependancesManquantes()
        {
            var result = new List<TacheRecord>();

            foreach (var tache in _taches.Where(t => !string.IsNullOrEmpty(t.Dependencies)))
            {
                var dependances = tache.Dependencies.Split(',').Select(d => d.Trim());
                var dependancesManquantes = dependances.Where(dep =>
                    !_taches.Any(t => t.TacheId == dep)).ToList();

                if (dependancesManquantes.Any())
                {
                    result.Add(tache);
                }
            }

            return result;
        }

        /// <summary>
        /// Obtient tous les jalons (utilisateur)
        /// </summary>
        public List<TacheRecord> ObtenirJalons()
        {
            return _taches.Where(t => _metierService.EstJalon(t)).ToList();
        }

        /// <summary>
        /// COMPATIBILITÉ: Obtient jalons de surcharge (maintenant tous les jalons)
        /// </summary>
        public List<TacheRecord> ObtenirJalonsSurcharge()
        {
            return ObtenirJalons();
        }
        /// <summary>
        /// NOUVELLE MÉTHODE: Gère l'édition des jalons J_Sync_ (créés dynamiquement)
        /// </summary>
        /// <summary>
        /// MÉTHODE CORRIGÉE: Gère l'édition des jalons J_Sync_ sans créer de boucles
        /// </summary>
        public void ModifierOuCreerJalonSync(TacheRecord jalonModifie)
        {
            if (jalonModifie == null)
                throw new ArgumentNullException(nameof(jalonModifie));

            // Vérifier si c'est un jalon J_Sync_
            if (!jalonModifie.TacheId.StartsWith("J_Sync_"))
            {
                // Utiliser la méthode normale pour les autres tâches
                ModifierTache(jalonModifie);
                return;
            }

            // Nettoyer l'ID des caractères problématiques
            var idNettoye = NettoyerIdJalon(jalonModifie.TacheId);

            // Vérifier si une surcharge utilisateur existe déjà
            var jalonExistant = _taches.FirstOrDefault(t => t.TacheId == idNettoye);

            if (jalonExistant != null)
            {
                // MODIFICATION: Mettre à jour le jalon existant
                jalonExistant.TacheNom = jalonModifie.TacheNom;
                jalonExistant.HeuresHommeEstimees = jalonModifie.HeuresHommeEstimees;
                jalonExistant.Dependencies = jalonModifie.Dependencies;
                jalonExistant.ExclusionsDependances = jalonModifie.ExclusionsDependances;
                // Les autres propriétés (bloc, lot, métier) restent inchangées
            }
            else
            {
                // CRÉATION: Nouvelle surcharge utilisateur du jalon J_Sync_
                var nouveauJalon = new TacheRecord
                {
                    TacheId = idNettoye,
                    TacheNom = jalonModifie.TacheNom,
                    HeuresHommeEstimees = jalonModifie.HeuresHommeEstimees,
                    MetierId = _metierService.GetJalonMetierId(),
                    Dependencies = jalonModifie.Dependencies,
                    ExclusionsDependances = jalonModifie.ExclusionsDependances,
                    LotId = jalonModifie.LotId,
                    LotNom = jalonModifie.LotNom,
                    LotPriorite = jalonModifie.LotPriorite,
                    BlocId = jalonModifie.BlocId,
                    BlocNom = jalonModifie.BlocNom,
                    BlocCapaciteMaxOuvriers = jalonModifie.BlocCapaciteMaxOuvriers
                };

                _taches.Add(nouveauJalon);
            }
        }

        /// <summary>
        /// Nettoie l'ID d'un jalon pour éviter les problèmes avec les espaces et caractères spéciaux
        /// </summary>
        private string NettoyerIdJalon(string idOriginal)
        {
            // Remplacer les espaces et caractères problématiques par des underscores
            return idOriginal
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace(".", "_")
                .Replace("/", "_")
                .Replace("\\", "_");
        }

        /// <summary>
        /// MÉTHODE PUBLIQUE: Pour que TacheForm puisse distinguer les jalons J_Sync_
        /// </summary>
        public bool EstJalonSync(string tacheId)
        {
            return tacheId.StartsWith("J_Sync_");
        }

        /// <summary>
        /// MÉTHODE PUBLIQUE: Vérifie si un jalon J_Sync_ a été surchargé par l'utilisateur
        /// </summary>
        public bool EstJalonSyncSurcharge(string tacheId)
        {
            return EstJalonSync(tacheId) && _taches.Any(t => t.TacheId == tacheId);
        }

        /// <summary>
        /// Copie une tâche (méthode utilitaire)
        /// </summary>
        private TacheRecord CopierTache(TacheRecord source)
        {
            return new TacheRecord
            {
                TacheId = source.TacheId,
                TacheNom = source.TacheNom,
                HeuresHommeEstimees = source.HeuresHommeEstimees,
                MetierId = source.MetierId,
                Dependencies = source.Dependencies,
                ExclusionsDependances = source.ExclusionsDependances,
                LotId = source.LotId,
                LotNom = source.LotNom,
                LotPriorite = source.LotPriorite,
                BlocId = source.BlocId,
                BlocNom = source.BlocNom,
                BlocCapaciteMaxOuvriers = source.BlocCapaciteMaxOuvriers
            };
        }
        #endregion

        #region Aide aux Décisions Chef

        /// <summary>
        /// Suggère des dépendances métier pour une tâche (le chef peut les ignorer)
        /// </summary>
        public List<string> SuggererDependancesMetier(TacheRecord tache)
        {
            return _metierService.SuggererDependancesMetier(tache, _taches);
        }

        /// <summary>
        /// Applique automatiquement les suggestions métier (optionnel pour le chef)
        /// </summary>
        public void AppliquerSuggestionsMetier(string tacheId)
        {
            var tache = ObtenirTacheParId(tacheId);
            if (tache == null) return;

            var suggestions = SuggererDependancesMetier(tache);
            if (!suggestions.Any()) return;

            // Fusionner avec les dépendances existantes
            var dependancesExistantes = string.IsNullOrEmpty(tache.Dependencies)
                ? new List<string>()
                : tache.Dependencies.Split(',').Select(d => d.Trim()).ToList();

            var toutesLesDependances = dependancesExistantes.Union(suggestions).Distinct();
            tache.Dependencies = string.Join(",", toutesLesDependances);
        }

        #endregion

        #region Gestion Blocs

        /// <summary>
        /// Obtient tous les blocs uniques
        /// </summary>
        public List<BlocInfo> ObtenirTousLesBlocs()
        {
            return _taches
                .GroupBy(t => t.BlocId)
                .Select(g => new BlocInfo
                {
                    BlocId = g.Key,
                    BlocNom = g.First().BlocNom,
                    CapaciteMaxOuvriers = g.First().BlocCapaciteMaxOuvriers,
                    NombreTaches = g.Count(),
                    HeuresHommeTotal = g.Sum(t => t.HeuresHommeEstimees)
                })
                .OrderBy(b => b.BlocNom)
                .ToList();
        }

        /// <summary>
        /// Met à jour les informations d'un bloc
        /// </summary>
        public void ModifierBloc(string blocId, string nouveauNom, int nouvelleCapacite)
        {
            var tachesDuBloc = _taches.Where(t => t.BlocId == blocId).ToList();
            if (!tachesDuBloc.Any())
                throw new InvalidOperationException($"Bloc {blocId} non trouvé.");

            foreach (var tache in tachesDuBloc)
            {
                tache.BlocNom = nouveauNom;
                tache.BlocCapaciteMaxOuvriers = nouvelleCapacite;
            }
        }

        #endregion

        #region Gestion Lots

        /// <summary>
        /// Obtient tous les lots uniques
        /// </summary>
        public List<LotInfo> ObtenirTousLesLots()
        {
            return _taches
                .GroupBy(t => t.LotId)
                .Select(g => new LotInfo
                {
                    LotId = g.Key,
                    LotNom = g.First().LotNom,
                    Priorite = g.First().LotPriorite,
                    NombreTaches = g.Count(),
                    HeuresHommeTotal = g.Sum(t => t.HeuresHommeEstimees),
                    BlocsAssocies = g.Select(t => t.BlocId).Distinct().ToList()
                })
                .OrderBy(l => l.Priorite)
                .ThenBy(l => l.LotNom)
                .ToList();
        }

        /// <summary>
        /// Met à jour les informations d'un lot
        /// </summary>
        public void ModifierLot(string lotId, string nouveauNom, int nouvellePriorite)
        {
            var tachesDuLot = _taches.Where(t => t.LotId == lotId).ToList();
            if (!tachesDuLot.Any())
                throw new InvalidOperationException($"Lot {lotId} non trouvé.");

            foreach (var tache in tachesDuLot)
            {
                tache.LotNom = nouveauNom;
                tache.LotPriorite = nouvellePriorite;
            }
        }

        #endregion

        #region Mapping Tâches-Métiers COMPATIBILITÉ

        /// <summary>
        /// Associe une tâche à un métier
        /// </summary>
        public void AssocierTacheMetier(string tacheId, string metierId)
        {
            if (string.IsNullOrWhiteSpace(tacheId))
                throw new ArgumentException("L'ID de la tâche ne peut pas être vide.");

            if (string.IsNullOrWhiteSpace(metierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.");

            var tache = ObtenirTacheParId(tacheId);
            if (tache == null)
                throw new InvalidOperationException($"Tâche {tacheId} non trouvée.");

            tache.MetierId = metierId;
        }

        /// <summary>
        /// Obtient le métier associé à une tâche
        /// </summary>
        public string ObtenirMetierPourTache(string tacheId)
        {
            var tache = ObtenirTacheParId(tacheId);
            return tache?.MetierId;
        }

        /// <summary>
        /// Obtient toutes les tâches sans métier assigné
        /// </summary>
        public List<TacheRecord> ObtenirTachesSansMetier()
        {
            return _taches.Where(t => string.IsNullOrWhiteSpace(t.MetierId)).ToList();
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

        #region Import/Export

        /// <summary>
        /// Importe les tâches depuis un fichier CSV
        /// </summary>
        public int ImporterDepuisCsv(string filePath, bool remplacerExistantes = true)
        {
            var tachesImportees = _csvDataService.ImportCsv<TacheRecord>(filePath);

            if (remplacerExistantes)
            {
                _taches.Clear();
            }

            _taches.AddRange(tachesImportees);

            // Mettre à jour les compteurs basés sur les IDs existants
            MettreAJourCompteurs();

            return tachesImportees.Count;
        }

        /// <summary>
        /// Importe les tâches depuis un fichier Excel Fieldwire/Dalux
        /// </summary>
        public int ImporterDepuisExcelFieldwire(string filePath)
        {
            // TODO: Implémentation spécifique au format Fieldwire/Dalux
            var donneesExcel = _excelReader.ImportFieldwireTaches(filePath);
            return 0;
        }

        /// <summary>
        /// Exporte les tâches vers un fichier CSV
        /// </summary>
        public void ExporterVersCsv(string filePath)
        {
            _csvDataService.ExportCsv(_taches, filePath);
        }

        /// <summary>
        /// Charge les tâches depuis une liste (utilisé par PlanificationService)
        /// </summary>
        public void ChargerTaches(List<TacheRecord> taches)
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
                return new StatistiquesTaches
                {
                    NombreTachesTotal = 0,
                    HeuresHommeTotal = 0,
                    HeuresHommeMoyenneParTache = 0,
                    NombreBlocsUniques = 0,
                    NombreLotsUniques = 0,
                    TachesAvecDependances = 0,
                    JalonsSurcharge = 0
                };
            }

            var jalons = _taches.Count(t => _metierService.EstJalon(t));

            return new StatistiquesTaches
            {
                NombreTachesTotal = _taches.Count,
                HeuresHommeTotal = _taches.Sum(t => t.HeuresHommeEstimees),
                HeuresHommeMoyenneParTache = _taches.Where(t => !_metierService.EstJalon(t)).Any()
                    ? _taches.Where(t => !_metierService.EstJalon(t)).Average(t => t.HeuresHommeEstimees)
                    : 0,
                NombreBlocsUniques = _taches.Select(t => t.BlocId).Distinct().Count(),
                NombreLotsUniques = _taches.Select(t => t.LotId).Distinct().Count(),
                TachesAvecDependances = _taches.Count(t => !string.IsNullOrWhiteSpace(t.Dependencies)),
                JalonsSurcharge = jalons // Tous les jalons sont maintenant "jalons utilisateur"
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
    /// Informations sur un bloc
    /// </summary>
    public class BlocInfo
    {
        public string BlocId { get; set; } = "";
        public string BlocNom { get; set; } = "";
        public int CapaciteMaxOuvriers { get; set; }
        public int NombreTaches { get; set; }
        public int HeuresHommeTotal { get; set; }
    }

    /// <summary>
    /// Informations sur un lot
    /// </summary>
    public class LotInfo
    {
        public string LotId { get; set; } = "";
        public string LotNom { get; set; } = "";
        public int Priorite { get; set; }
        public int NombreTaches { get; set; }
        public int HeuresHommeTotal { get; set; }
        public List<string> BlocsAssocies { get; set; } = new List<string>();
    }

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