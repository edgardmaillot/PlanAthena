// Fichier: /Services/Business/PlanningService.cs Version 0.6.0

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.TaskManager;
using PlanAthena.Services.DTOs.UseCases;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Agit comme la Source de Vérité unique pour le planning de projet actuellement chargé.
    /// Il expose des vues et des données agrégées sur ce planning.
    /// Version 0.6.0 : Intègre la gestion de la PlanningBaseline et les calculs EVM.
    /// </summary>
    public class PlanningService
    {
        private ConsolidatedPlanning? _currentPlanning;
        private ConfigurationPlanification? _currentConfig;
        private PlanningBaseline? _currentBaseline; 

        // Le calque d'informations sur l'état réel des tâches.
        private Dictionary<string, Statut> _statutsReelsParTacheId = new();

        private readonly RessourceService _ressourceService;

        public PlanningService(RessourceService ressourceService)
        {
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
        }

        #region Gestion de l'État (Planning & Baseline)

        /// <summary>
        /// Charge un planning et une configuration, effaçant les données précédentes.
        /// </summary>
        public virtual void LoadPlanning(ConsolidatedPlanning? planning, ConfigurationPlanification config)
        {
            ClearPlanning();
            _currentPlanning = planning;
            _currentConfig = config;
        }

        /// <summary>
        /// Surcharge pour charger également la baseline lors de l'ouverture d'un projet.
        /// </summary>
        public virtual void LoadPlanning(ConsolidatedPlanning? planning, ConfigurationPlanification config, PlanningBaseline? baseline)
        {
            LoadPlanning(planning, config);
            _currentBaseline = baseline;
        }

        /// <summary>
        /// Met à jour le planning actuel en fusionnant les nouvelles données de planification.
        /// </summary>
        public virtual void UpdatePlanning(ConsolidatedPlanning newPlanning, ConfigurationPlanification newConfig)
        {
            if (newPlanning == null) throw new ArgumentNullException(nameof(newPlanning));
            if (newConfig == null) throw new ArgumentNullException(nameof(newConfig));

            if (_currentPlanning == null)
            {
                _currentPlanning = newPlanning;
                _currentConfig = newConfig;
                return;
            }

            var idsTachesMeresReplanifiees = newPlanning.SegmentsParOuvrierId.Values
                .SelectMany(segments => segments)
                .Select(s => s.ParentTacheId ?? s.TacheId)
                .Distinct()
                .ToHashSet();

            foreach (var segmentsList in _currentPlanning.SegmentsParOuvrierId.Values)
            {
                segmentsList.RemoveAll(s => idsTachesMeresReplanifiees.Contains(s.ParentTacheId ?? s.TacheId));
            }

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
            _currentConfig = newConfig;
        }

        /// <summary>
        /// Efface toutes les données de planning et de baseline en mémoire.
        /// </summary>
        public virtual void ClearPlanning()
        {
            _currentPlanning = null;
            _currentConfig = null;
            _statutsReelsParTacheId.Clear();
            ClearBaseline();
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

        #region NOUVEAU : API pour la Gestion de la Baseline

        /// <summary>
        /// Définit la baseline active pour le projet.
        /// </summary>
        public virtual void SetBaseline(PlanningBaseline baseline) => _currentBaseline = baseline;

        /// <summary>
        /// Récupère la baseline active.
        /// </summary>
        public virtual PlanningBaseline? GetBaseline() => _currentBaseline;

        /// <summary>
        /// Efface la baseline active.
        /// </summary>
        public virtual void ClearBaseline() => _currentBaseline = null;

        #endregion

        #region NOUVEAU : API pour les calculs EVM

        /// <summary>
        /// Point d'entrée principal pour obtenir un rapport complet sur la performance du projet (EVM).
        /// </summary>
        /// <param name="dateRef">La date à laquelle les calculs doivent être effectués.</param>
        /// <param name="toutesLesTaches">La liste complète des tâches provenant de TaskManagerService.</param>
        /// <returns>Un DTO contenant les indicateurs PV, EV, AC et BAC.</returns>
        public virtual EvmReportDto GetRapportEVMComplet(DateTime dateRef, IReadOnlyList<Tache> toutesLesTaches)
        {
            if (_currentBaseline == null)
            {
                return new EvmReportDto { BaselineExists = false };
            }

            var pv = _CalculerValeurPlanifieeCumulative(dateRef);
            var ev = _CalculerValeurAcquiseCumulative(dateRef, toutesLesTaches);
            var ac = _SimulerCoutReelCumulatif(dateRef);

            return new EvmReportDto
            {
                BaselineExists = true,
                BudgetAtCompletion = _currentBaseline.BudgetAtCompletion,
                PlannedValue = pv,
                EarnedValue = ev,
                ActualCost = ac
            };
        }

        #endregion

        #region Méthodes pour le Cockpit

        public virtual void ReconcilierAvecAvancementReel(Tache tache)
        {
            if (tache == null) return;

            if (tache.Statut != Statut.Estimée && tache.Statut != Statut.Planifiée)
            {
                _statutsReelsParTacheId[tache.TacheId] = tache.Statut;
            }
            else
            {
                _statutsReelsParTacheId.Remove(tache.TacheId);
            }
        }

        public virtual int ObtenirNombreTachesQuiDevraientEtreTerminees(DateTime dateReference, IReadOnlyList<Tache> toutesLesTaches)
        {
            return toutesLesTaches.Count(t =>
               !t.EstConteneur &&
               t.DateFinPlanifiee.HasValue &&
               t.DateFinPlanifiee.Value.Date <= dateReference.Date
           );
        }

        public virtual double CalculerPerformanceCoutCPI(IReadOnlyList<Tache> toutesLesTaches)
        {
            return 0.0;
        }

        public virtual MetierTensionData CalculerTensionMetierPourPeriodeFuture(DateTime dateDebut, int nombreJours)
        {
            if (_currentPlanning == null || _currentConfig == null)
                return new MetierTensionData { NomMetier = "N/A", TauxOccupation = 0 };

            var dateFin = dateDebut.AddDays(nombreJours);
            var ouvriers = _ressourceService.GetAllOuvriers();
            var metiers = _ressourceService.GetAllMetiers().ToDictionary(m => m.MetierId, m => m.Nom);

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

            int joursOuvresPeriode = GetNombreJoursOuvres(dateDebut, dateFin);
            double heuresDisponiblesParOuvrier = joursOuvresPeriode * _currentConfig.HeuresTravailEffectifParJour;

            var heuresDisponiblesParMetierId = ouvriers
                .GroupBy(o => o.MetierId)
                .ToDictionary(g => g.Key, g => g.Count() * heuresDisponiblesParOuvrier);

            var tensionParMetier = heuresPlanifieesParMetierId
                .Select(kvp =>
                {
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

        public virtual int GetNombreJoursOuvres(DateTime dateDebut, DateTime dateFin)
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

        public virtual ConfigurationPlanification? GetCurrentConfig() => _currentConfig;
        public virtual ConsolidatedPlanning? GetCurrentPlanning() => _currentPlanning;

        #endregion

        #region NOUVEAU : Méthodes privées pour le calcul EVM

        private decimal _CalculerValeurPlanifieeCumulative(DateTime dateRef)
        {
            if (_currentBaseline?.CourbePlannedValueCumulative == null || !_currentBaseline.CourbePlannedValueCumulative.Any())
            {
                return 0m;
            }

            return _currentBaseline.CourbePlannedValueCumulative
                .Where(kvp => kvp.Key.Date <= dateRef.Date)
                .OrderByDescending(kvp => kvp.Key)
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
        }

        private decimal _CalculerValeurAcquiseCumulative(DateTime dateRef, IReadOnlyList<Tache> toutesLesTaches)
        {
            if (_currentBaseline?.BudgetInitialParTacheId == null)
            {
                return 0m;
            }

            decimal totalEV = 0m;

            var tachesTerminees = toutesLesTaches
                .Where(t => t.Statut == Statut.Terminée &&
                            t.DateFinReelle.HasValue &&
                            t.DateFinReelle.Value.Date <= dateRef.Date);

            foreach (var tache in tachesTerminees)
            {
                if (_currentBaseline.BudgetInitialParTacheId.TryGetValue(tache.TacheId, out decimal budgetTache))
                {
                    totalEV += budgetTache;
                }
            }

            return totalEV;
        }

        private decimal _SimulerCoutReelCumulatif(DateTime dateRef)
        {
            if (_currentPlanning == null || _currentConfig == null) return 0m;

            decimal totalAC = 0m;
            var heuresParJour = (decimal)_currentConfig.HeuresTravailEffectifParJour;
            if (heuresParJour == 0) return 0m;

            // 1. Coûts de main-d'œuvre (RH)
            var segmentsPasses = _currentPlanning.SegmentsParOuvrierId.Values
                .SelectMany(s => s)
                .Where(s => s.Jour.Date < dateRef.Date);

            foreach (var segment in segmentsPasses)
            {
                var ouvrier = _ressourceService.GetOuvrierById(segment.OuvrierId);
                if (ouvrier != null)
                {
                    totalAC += ((decimal)segment.HeuresTravaillees / heuresParJour) * ouvrier.CoutJournalier;
                }
            }

            // 2. Coûts indirects
            var dateDebutProjet = _currentPlanning.DateDebutProjet;
            if (dateDebutProjet != default)
            {
                var dateFinCalcul = (dateRef > _currentPlanning.DateFinProjet) ? _currentPlanning.DateFinProjet : dateRef;
                int joursOuvresPasses = GetNombreJoursOuvres(dateDebutProjet, dateFinCalcul);
                totalAC += (decimal)joursOuvresPasses * _currentConfig.CoutIndirectJournalierAbsolu;
            }

            return totalAC;
        }

        #endregion
    }
}