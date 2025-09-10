// /Services/Business/PlanningService.cs V0.5 (Cockpit)

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.TaskManager;
using PlanAthena.Services.DTOs.UseCases;

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

        // NOUVEAU : Calque d'informations pour stocker l'état réel des tâches planifiées.
        private Dictionary<string, Statut> _statutsReelsParTacheId = new();

        private readonly RessourceService _ressourceService;

        public PlanningService(RessourceService ressourceService)
        {
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
        }
        public virtual void LoadPlanning(ConsolidatedPlanning planning, ConfigurationPlanification config)
        {
            ClearPlanning(); // On s'assure de partir d'un état propre
            _currentPlanning = planning;
            _currentConfig = config;
        }

        #region Méthodes de Gestion de l'État

        /// <summary>
        /// MODIFIÉ : Met à jour le planning actuel en fusionnant les nouvelles données de planification.
        /// Cette approche non-destructive préserve l'état des tâches qui n'ont pas été replanifiées.
        /// </summary>
        public virtual void UpdatePlanning(ConsolidatedPlanning newPlanning, ConfigurationPlanification newConfig)
        {
            if (newPlanning == null) throw new ArgumentNullException(nameof(newPlanning));
            if (newConfig == null) throw new ArgumentNullException(nameof(newConfig));

            // Si aucun planning n'existe, on prend le nouveau.
            if (_currentPlanning == null)
            {
                _currentPlanning = newPlanning;
                _currentConfig = newConfig;
                return;
            }

            // --- Logique de Fusion ---

            // 1. Identifier les tâches "mères" qui ont été replanifiées.
            var idsTachesMeresReplanifiees = newPlanning.SegmentsParOuvrierId.Values
                .SelectMany(segments => segments)
                .Select(s => s.ParentTacheId ?? s.TacheId)
                .Distinct()
                .ToHashSet();

            // 2. Nettoyer l'ancien planning : supprimer tous les segments liés à ces tâches mères.
            foreach (var segmentsList in _currentPlanning.SegmentsParOuvrierId.Values)
            {
                segmentsList.RemoveAll(s => idsTachesMeresReplanifiees.Contains(s.ParentTacheId ?? s.TacheId));
            }

            // 3. Fusionner le nouveau planning dans l'ancien.
            foreach (var kvp in newPlanning.SegmentsParOuvrierId)
            {
                var ouvrierId = kvp.Key;
                var nouveauxSegments = kvp.Value;

                if (_currentPlanning.SegmentsParOuvrierId.TryGetValue(ouvrierId, out var segmentsExistants))
                {
                    segmentsExistants.AddRange(nouveauxSegments);
                }
                else
                {
                    _currentPlanning.SegmentsParOuvrierId[ouvrierId] = nouveauxSegments;
                }
            }

            // Mettre à jour la configuration avec la plus récente.
            _currentConfig = newConfig;
        }

        public virtual ConsolidatedPlanning? GetCurrentPlanning() => _currentPlanning;

        public virtual void ClearPlanning()
        {
            _currentPlanning = null;
            _currentConfig = null;
            _statutsReelsParTacheId.Clear(); // Nettoyer aussi les statuts réels
        }


        public virtual void InvaliderTache(string tacheId)
        {
            if (_currentPlanning == null) return;

            foreach (var segmentsList in _currentPlanning.SegmentsParOuvrierId.Values)
            {
                segmentsList.RemoveAll(s => s.TacheId == tacheId || s.ParentTacheId == tacheId);
            }
        }

        #endregion

        #region Méthodes pour le Cockpit

        /// <summary>
        /// NOUVEAU : Met à jour le calque de statut réel pour une seule tâche de manière ciblée.
        /// </summary>
        public virtual void ReconcilierAvecAvancementReel(Tache tache)
        {
            if (tache == null) return;

            // On met à jour ou on ajoute le statut réel pour cette tâche spécifique.
            if (tache.Statut != Statut.Estimée && tache.Statut != Statut.Planifiée)
            {
                _statutsReelsParTacheId[tache.TacheId] = tache.Statut;
            }
            else
            {
                // Si le statut redevient "prévisionnel", on retire la tâche du calque.
                _statutsReelsParTacheId.Remove(tache.TacheId);
            }
        }

        /// <summary>
        /// Calcule le nombre de tâches (non-conteneurs) dont la date de fin planifiée est passée.
        /// Utilisé pour le calcul du KPI "SPI Simplifié".
        /// </summary>
        /// <param name="dateReference">La date à laquelle comparer la date de fin planifiée.</param>
        /// <param name="toutesLesTaches">La liste complète des tâches du projet.</param>
        /// <returns>Le nombre de tâches qui auraient dû être terminées.</returns>
        public virtual int ObtenirNombreTachesQuiDevraientEtreTerminees(DateTime dateReference, IReadOnlyList<Tache> toutesLesTaches)
        {
            return toutesLesTaches.Count(t =>
               !t.EstConteneur &&
               t.DateFinPlanifiee.HasValue &&
               t.DateFinPlanifiee.Value.Date <= dateReference.Date
           );
        }

        /// <summary>
        /// Calcule l'indicateur de performance des coûts (CPI).
        /// Pour la V1, cette méthode retourne une valeur par défaut.
        /// </summary>
        /// <returns>Le CPI calculé (0.0 pour la V1).</returns>
        public virtual double CalculerPerformanceCoutCPI(IReadOnlyList<Tache> toutesLesTaches)
        {
            // La logique de calcul du CPI sera implémentée dans une version future.
            return 0.0;
        }

        /// <summary>
        /// Analyse le planning futur pour identifier le métier le plus sollicité.
        /// </summary>
        /// <param name="dateDebut">La date de début de la période d'analyse.</param>
        /// <param name="nombreJours">Le nombre de jours à analyser dans le futur.</param>
        /// <returns>Un DTO contenant le nom et le taux d'occupation du métier le plus en tension.</returns>
        public virtual MetierTensionData CalculerTensionMetierPourPeriodeFuture(DateTime dateDebut, int nombreJours)
        {
            if (_currentPlanning == null || _currentConfig == null)
                return new MetierTensionData { NomMetier = "N/A", TauxOccupation = 0 };

            var dateFin = dateDebut.AddDays(nombreJours);
            var ouvriers = _ressourceService.GetAllOuvriers();
            var metiers = _ressourceService.GetAllMetiers().ToDictionary(m => m.MetierId, m => m.Nom);

            // Heures planifiées par métier sur la période
            var heuresPlanifieesParMetierId = _currentPlanning.SegmentsParOuvrierId.Values
                .SelectMany(segments => segments)
                .Where(s => s.Jour >= dateDebut.Date && s.Jour < dateFin.Date)
                .Join(ouvriers,
                      segment => segment.OuvrierId,
                      ouvrier => ouvrier.OuvrierId,
                      (segment, ouvrier) => new { segment.HeuresTravaillees, ouvrier.MetierId })
                .GroupBy(j => j.MetierId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.HeuresTravaillees));

            if (!heuresPlanifieesParMetierId.Any())
                return new MetierTensionData { NomMetier = "Aucun travail planifié", TauxOccupation = 0 };

            // Heures disponibles par métier sur la période
            int joursOuvresPeriode = GetNombreJoursOuvres(dateDebut, dateFin);
            double heuresDisponiblesParOuvrier = joursOuvresPeriode * _currentConfig.HeuresTravailEffectifParJour;

            var heuresDisponiblesParMetierId = ouvriers
                .GroupBy(o => o.MetierId)
                .ToDictionary(g => g.Key, g => g.Count() * heuresDisponiblesParOuvrier);

            // Calcul du taux d'occupation et recherche du maximum
            var tensionParMetier = heuresPlanifieesParMetierId
                .Select(kvp => {
                    var metierId = kvp.Key;
                    var heuresPlanifiees = kvp.Value;
                    heuresDisponiblesParMetierId.TryGetValue(metierId, out var heuresDisponibles);
                    double tauxOccupation = (heuresDisponibles > 0) ? (heuresPlanifiees / heuresDisponibles) : 0;
                    return new { MetierId = metierId, TauxOccupation = tauxOccupation };
                })
                .OrderByDescending(x => x.TauxOccupation)
                .FirstOrDefault();

            if (tensionParMetier == null)
                return new MetierTensionData { NomMetier = "N/A", TauxOccupation = 0 };

            return new MetierTensionData
            {
                NomMetier = metiers.GetValueOrDefault(tensionParMetier.MetierId, "Inconnu"),
                TauxOccupation = tensionParMetier.TauxOccupation
            };
        }

        #endregion



        #region Méthodes d'Agrégation pour TaskManagerService


        public virtual IReadOnlyDictionary<string, PlanningInfoPourTache> ObtenirInfosPlanificationPourToutesLesTaches()
        {
            // Logique existante inchangée
            if (_currentPlanning == null) return new Dictionary<string, PlanningInfoPourTache>();
            var allSegments = _currentPlanning.SegmentsParOuvrierId.Values.SelectMany(list => list).ToList();
            if (!allSegments.Any()) return new Dictionary<string, PlanningInfoPourTache>();
            var mapOuvrierIdToName = _ressourceService.GetAllOuvriers().ToDictionary(o => o.OuvrierId, o => o.NomComplet);
            var segmentsParTacheMere = allSegments.GroupBy(s => s.ParentTacheId ?? s.TacheId);
            var resultat = new Dictionary<string, PlanningInfoPourTache>();
            foreach (var groupe in segmentsParTacheMere)
            {
                var tacheMereId = groupe.Key;
                var segmentsDeLaTache = groupe.ToList();
                var affectations = segmentsDeLaTache.GroupBy(s => s.OuvrierId).Select(g => new AffectationOuvrier { OuvrierId = g.Key, NomOuvrier = mapOuvrierIdToName.GetValueOrDefault(g.Key, g.Key), HeuresTravaillees = (int)g.Sum(s => s.HeuresTravaillees) }).ToList();
                resultat[tacheMereId] = new PlanningInfoPourTache { EstConteneur = segmentsDeLaTache.Any(s => !string.IsNullOrEmpty(s.ParentTacheId)), DateDebut = segmentsDeLaTache.Min(s => s.Jour.Add(s.HeureDebut)), DateFin = segmentsDeLaTache.Max(s => s.Jour.Add(s.HeureFin)), Affectations = affectations };
            }
            return resultat;
        }

        public virtual IReadOnlyDictionary<string, PlanningInfoPourTache> ObtenirInfosPlanificationPourTachesFeuilles()
        {
            // Logique existante inchangée
            if (_currentPlanning == null) return new Dictionary<string, PlanningInfoPourTache>();
            var allSegments = _currentPlanning.SegmentsParOuvrierId.Values.SelectMany(list => list).ToList();
            if (!allSegments.Any()) return new Dictionary<string, PlanningInfoPourTache>();
            var mapOuvrierIdToName = _ressourceService.GetAllOuvriers().ToDictionary(o => o.OuvrierId, o => o.NomComplet);
            var segmentsParTacheFeuille = allSegments.GroupBy(s => s.TacheId);
            var resultat = new Dictionary<string, PlanningInfoPourTache>();
            foreach (var groupe in segmentsParTacheFeuille)
            {
                var tacheFeuilleId = groupe.Key;
                var segmentsDeLaTache = groupe.ToList();
                var affectations = segmentsDeLaTache.GroupBy(s => s.OuvrierId).Select(g => new AffectationOuvrier { OuvrierId = g.Key, NomOuvrier = mapOuvrierIdToName.GetValueOrDefault(g.Key, g.Key), HeuresTravaillees = (int)g.Sum(s => s.HeuresTravaillees) }).ToList();
                var info = new PlanningInfoPourTache { EstConteneur = false, DateDebut = segmentsDeLaTache.Min(s => s.Jour.Add(s.HeureDebut)), DateFin = segmentsDeLaTache.Max(s => s.Jour.Add(s.HeureFin)), Affectations = affectations };
                resultat[tacheFeuilleId] = info;
            }
            return resultat;
        }
        #endregion

        #region Méthodes de Vue & Calcul

        // NOTE: Cette méthode n'est PAS modifiée pour l'instant, comme convenu.
        // Le UseCase se chargera de la jointure avec le statut réel.
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

        // consommé par PilotageProjetUseCase
        public virtual ConfigurationPlanification? GetCurrentConfig() => _currentConfig;
    }
}