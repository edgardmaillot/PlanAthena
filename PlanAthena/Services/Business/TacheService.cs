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
        private readonly List<Tache> _taches = new List<Tache>();
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
        private string GenererIdUnique(Tache tache)
        {
            string prefixe;
            int compteur;

            if (tache.EstJalon) // MODIFIÉ
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

            if (tache.EstJalon) // MODIFIÉ
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

        /// <summary>
        /// Obtient les tâches ayant des dépendances non résolues
        /// </summary>
        public List<Tache> ObtenirTachesAvecDependancesManquantes()
        {
            var result = new List<Tache>();

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
        public List<Tache> ObtenirJalons()
        {
            return _taches.Where(t => t.EstJalon).ToList();
        }

        /// <summary>
        /// Copie une tâche (méthode utilitaire)
        /// </summary>
        private Tache CopierTache(Tache source)
        {
            return new Tache
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
        public List<Tache> ObtenirTachesSansMetier()
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
            var tachesImportees = _csvDataService.ImportCsv<Tache>(filePath);

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

            // MODIFIÉ : On compte les jalons utilisateur directement.
            var jalons = _taches.Count(t => t.Type == TypeActivite.JalonUtilisateur);

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
                JalonsSurcharge = jalons
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