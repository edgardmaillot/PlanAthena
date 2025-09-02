// /Services/Business/PlanningService.cs V0.4.8

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.TaskManager;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Agit comme la Source de Vérité unique pour le planning de projet actuellement chargé.
    /// Il expose des vues et des données agrégées sur ce planning.
    /// </summary>
    public class PlanningService
    {
        private ConsolidatedPlanning? _currentPlanning;
        private ConfigurationPlanification? _currentConfig;

        // NOUVELLE DÉPENDANCE : Nécessaire pour obtenir les noms des ouvriers.
        private readonly RessourceService _ressourceService;

        public PlanningService(RessourceService ressourceService)
        {
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
        }

        #region Méthodes de Gestion de l'État

        public virtual void UpdatePlanning(ConsolidatedPlanning newPlanning, ConfigurationPlanification newConfig)
        {
            _currentPlanning = newPlanning ?? throw new ArgumentNullException(nameof(newPlanning));
            _currentConfig = newConfig ?? throw new ArgumentNullException(nameof(newConfig));
        }

        public virtual ConsolidatedPlanning? GetCurrentPlanning() => _currentPlanning;

        public virtual void ClearPlanning()
        {
            _currentPlanning = null;
            _currentConfig = null;
        }

        /// <summary>
        /// NOUVEAU: Supprime tous les segments de travail liés à un TacheId (qu'il soit parent ou enfant)
        /// du planning en mémoire.
        /// </summary>
        public virtual void InvaliderTache(string tacheId)
        {
            if (_currentPlanning == null) return;

            foreach (var segmentsList in _currentPlanning.SegmentsParOuvrierId.Values)
            {
                segmentsList.RemoveAll(s => s.TacheId == tacheId || s.ParentTacheId == tacheId);
            }
        }

        #endregion

        #region Méthodes d'Agrégation pour TaskManagerService

        /// <summary>
        /// NOUVEAU: Analyse le planning actuel et retourne un dictionnaire d'informations agrégées
        /// pour chaque tâche "mère" (conceptuelle). C'est le point de contact principal pour le TaskManagerService.
        /// </summary>
        public virtual IReadOnlyDictionary<string, PlanningInfoPourTache> ObtenirInfosPlanificationPourToutesLesTaches()
        {
            if (_currentPlanning == null)
                return new Dictionary<string, PlanningInfoPourTache>();

            var allSegments = _currentPlanning.SegmentsParOuvrierId.Values.SelectMany(list => list).ToList();
            if (!allSegments.Any())
                return new Dictionary<string, PlanningInfoPourTache>();

            var mapOuvrierIdToName = _ressourceService.GetAllOuvriers().ToDictionary(o => o.OuvrierId, o => o.NomComplet);

            // Regrouper tous les segments par leur tâche "mère" (en utilisant ParentTacheId s'il existe)
            var segmentsParTacheMere = allSegments.GroupBy(s => s.ParentTacheId ?? s.TacheId);

            var resultat = new Dictionary<string, PlanningInfoPourTache>();

            foreach (var groupe in segmentsParTacheMere)
            {
                var tacheMereId = groupe.Key;
                var segmentsDeLaTache = groupe.ToList();

                var affectations = segmentsDeLaTache
                    .GroupBy(s => s.OuvrierId)
                    .Select(g => new AffectationOuvrier
                    {
                        OuvrierId = g.Key,
                        NomOuvrier = mapOuvrierIdToName.GetValueOrDefault(g.Key, g.Key),
                        HeuresTravaillees = (int)g.Sum(s => s.HeuresTravaillees)
                    }).ToList();

                var info = new PlanningInfoPourTache
                {
                    EstConteneur = segmentsDeLaTache.Any(s => !string.IsNullOrEmpty(s.ParentTacheId)),
                    DateDebut = segmentsDeLaTache.Min(s => s.Jour.Add(s.HeureDebut)),
                    DateFin = segmentsDeLaTache.Max(s => s.Jour.Add(s.HeureFin)),
                    Affectations = affectations
                };

                resultat[tacheMereId] = info;
            }

            return resultat;
        }

        #endregion

        #region Méthodes de Vue & Calcul

        /// <summary>
        /// NOUVEAU (PORTAGE): Construit une structure de données détaillée du planning, groupée par jour
        /// puis par ouvrier, optimisée pour un affichage de type log.
        /// </summary>
        public virtual IReadOnlyList<LogPlanningJournalier> RetournePlanningDetailleParJour()
        {
            if (_currentPlanning == null || !_currentPlanning.SegmentsParOuvrierId.Any())
            {
                return new List<LogPlanningJournalier>();
            }

            var segmentsParJour = _currentPlanning.SegmentsParOuvrierId
                .SelectMany(kvp => kvp.Value)
                .GroupBy(segment => segment.Jour.Date);

            var resultatFinal = new List<LogPlanningJournalier>();
            foreach (var groupeJour in segmentsParJour.OrderBy(g => g.Key))
            {
                var ouvriersDuJour = groupeJour
                    .GroupBy(segment => segment.OuvrierId)
                    .Select(groupeOuvrier =>
                    {
                        var nomOuvrier = _ressourceService.GetOuvrierById(groupeOuvrier.Key)?.NomComplet ?? groupeOuvrier.Key;
                        var affectations = groupeOuvrier
                            .Select(segment => new LogAffectationDuJour
                            {
                                TacheNom = segment.TacheNom,
                                BlocId = segment.BlocId,
                                DureeHeures = segment.HeuresTravaillees
                            })
                            .ToList();

                        return new LogOuvrierDuJour { NomOuvrier = nomOuvrier, Affectations = affectations };
                    })
                    .OrderBy(o => o.NomOuvrier)
                    .ToList();

                resultatFinal.Add(new LogPlanningJournalier { Jour = groupeJour.Key, Ouvriers = ouvriersDuJour });
            }

            return resultatFinal;
        }

        public int GetNombreJoursOuvres(DateTime dateDebut, DateTime dateFin)
        {
            if (_currentConfig?.JoursOuvres == null || !_currentConfig.JoursOuvres.Any() || dateFin < dateDebut)
                return 0;

            int nombreJoursOuvres = 0;
            for (var jourCourant = dateDebut.Date; jourCourant <= dateFin.Date; jourCourant = jourCourant.AddDays(1))
            {
                if (_currentConfig.JoursOuvres.Contains(jourCourant.DayOfWeek))
                    nombreJoursOuvres++;
            }
            return nombreJoursOuvres;
        }

        #endregion
    }
}