// Fichier: /Services/Business/PlanningService.cs Version 0.8.0
// Version corrigée : Centralisation des calculs EVM dans GetRapportEVMComplet

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.TaskManager;
using PlanAthena.Services.DTOs.UseCases;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Source de Vérité unique pour le planning de projet actuellement chargé.
    /// Expose des vues et des données agrégées sur ce planning.
    /// Version 0.8.0 : Centralisation des calculs EVM dans un seul point d'entrée.
    /// </summary>
    public class PlanningService
    {
        #region Champs privés

        private ConsolidatedPlanning? _currentPlanning;
        private ConfigurationPlanification? _currentConfig;
        private PlanningBaseline? _currentBaseline;
        private readonly Dictionary<string, Statut> _statutsReelsParTacheId = new();
        private readonly RessourceService _ressourceService;

        #endregion

        #region Constructeur

        public PlanningService(RessourceService ressourceService)
        {
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
        }

        #endregion

        #region Gestion de l'état (Planning & Baseline)

        public virtual void LoadPlanning(ConsolidatedPlanning? planning, ConfigurationPlanification config)
        {
            ClearPlanning();
            _currentPlanning = planning;
            _currentConfig = config;
        }

        public virtual void LoadPlanning(ConsolidatedPlanning? planning, ConfigurationPlanification config, PlanningBaseline? baseline)
        {
            LoadPlanning(planning, config);
            _currentBaseline = baseline;
        }

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

            var idsTachesMeresReplanifiees = GetIdsTachesMeresFromSegments(newPlanning);
            RemoveSegmentsForTasks(_currentPlanning, idsTachesMeresReplanifiees);
            MergeNewSegments(_currentPlanning, newPlanning);

            _currentConfig = newConfig;
        }

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

        #region API pour la Gestion de la Baseline

        public virtual void SetBaseline(PlanningBaseline baseline) => _currentBaseline = baseline;
        public virtual PlanningBaseline? GetBaseline() => _currentBaseline;
        public virtual void ClearBaseline() => _currentBaseline = null;

        #endregion

        #region API pour les calculs EVM - Point d'entrée centralisé

        /// <summary>
        /// Point d'entrée principal et UNIQUE pour obtenir un rapport complet EVM.
        /// CENTRALISE tous les calculs EVM pour éviter les duplications et incohérences.
        /// </summary>
        public virtual EvmReportDto GetRapportEVMComplet(DateTime dateRef, IReadOnlyList<Tache> toutesLesTaches)
        {
            if (!IsBaselineValid())
            {
                return new EvmReportDto
                {
                    BaselineExists = false,
                    DateReference = dateRef
                };
            }

            var tachesEligibles = FilterTasksForEVM(toutesLesTaches);

            // Calcul des indicateurs de base
            var pv = CalculerValeurPlanifieeCumulative(dateRef);
            var ev = CalculerValeurAcquiseCumulative(dateRef, tachesEligibles);
            var ac = CalculerCoutReelCumulatif(dateRef);
            var bac = _currentBaseline!.BudgetAtCompletion;

            // Calcul des variances
            var sv = ev - pv;
            var cv = ev - ac;

            // Calcul des indices de performance
            var spi = (pv > 0) ? (double)(ev / pv) : 1.0;
            var cpi = (ac > 0) ? (double)(ev / ac) : 1.0;

            // Calcul des estimations à l'achèvement
            var eac = (cpi > 0) ? bac / (decimal)cpi : bac;
            var ptc = Math.Max(0, bac - pv);
            var etc = Math.Max(0, (cpi > 0) ? (bac - ev) / (decimal)cpi : (bac - ev));
            var vac = bac - eac;

            // Calcul de SV en jours
            var svDays = CalculerScheduleVarianceEnJours(sv);

            // Calcul des pourcentages d'avancement
            var avancementPlanifie = (bac > 0) ? (double)(pv / bac) * 100.0 : 0.0;
            var avancementReel = (bac > 0) ? (double)(ev / bac) * 100.0 : 0.0;

            // Calcul de la déviation budgétaire en pourcentage
            var deviationBudget = (bac > 0) ? (double)((eac - bac) / bac) * 100.0 : 0.0;

            return new EvmReportDto
            {
                BaselineExists = true,
                DateReference = dateRef,

                // Indicateurs de base
                BudgetAtCompletion = bac,
                PlannedValue = pv,
                EarnedValue = ev,
                ActualCost = ac,

                // Variances
                ScheduleVariance = sv,
                ScheduleVarianceDays = svDays,
                CostVariance = cv,

                // Indices de performance
                SchedulePerformanceIndex = spi,
                CostPerformanceIndex = cpi,

                // Estimations à l'achèvement
                EstimateAtCompletion = eac,
                PlanToComplete = ptc,
                EstimateToComplete = etc,
                VarianceAtCompletion = vac,

                // Informations contextuelles
                AvancementPlanifiePourcentage = avancementPlanifie,
                AvancementReelPourcentage = avancementReel,
                DeviationBudgetPourcentage = deviationBudget
            };
        }

        #endregion

        #region API pour les graphiques EVM

        public virtual (bool BaselineExists, Dictionary<DateTime, decimal> PvCurve, Dictionary<DateTime, decimal> EvCurve, Dictionary<DateTime, decimal> AcCurve)
            GetCourbesEVMHistoriques(IReadOnlyList<Tache> toutesLesTaches)
        {
            if (!IsBaselineValid())
            {
                return (false, new(), new(), new());
            }

            var tachesEligibles = FilterTasksForEVM(toutesLesTaches);
            var dateDebut = _currentBaseline!.ConsPlanningInitial!.DateDebutProjet;
            var dateFinProjet = _currentBaseline.DateFinPlanifieeInitiale;
            var dateFinCalcul = (dateFinProjet < DateTime.Today) ? dateFinProjet : DateTime.Today;

            var pvCurve = BuildPlannedValueCurve();
            var (evCurve, acCurve) = BuildEarnedValueAndActualCostCurves(dateDebut, dateFinCalcul, tachesEligibles);
            SynchronizeCurveDates(pvCurve, evCurve, acCurve);

            return (true, pvCurve, evCurve, acCurve);
        }

        public virtual (bool BaselineExists, Dictionary<DateTime, (decimal Ptc, decimal Etc)> WeeklyData)
            GetEtcVsPtcHistorique(IReadOnlyList<Tache> toutesLesTaches)
        {
            if (!IsBaselineValid())
            {
                return (false, new());
            }

            var tachesEligibles = FilterTasksForEVM(toutesLesTaches);
            var bac = _currentBaseline!.BudgetAtCompletion;
            if (bac == 0) return (true, new());

            var dateDebut = _currentBaseline.ConsPlanningInitial!.DateDebutProjet;
            var dateFin = _currentBaseline.DateFinPlanifieeInitiale;
            var weeklyData = new Dictionary<DateTime, (decimal Ptc, decimal Etc)>();

            for (var date = dateDebut.Date; date <= dateFin.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Sunday && date.Date != dateFin.Date)
                    continue;

                var pv = CalculerValeurPlanifieeCumulative(date);
                var ev = CalculerValeurAcquiseCumulative(date, tachesEligibles);
                var ac = CalculerCoutReelCumulatif(date);

                var ptc = Math.Max(0, bac - pv);
                var cpi = (ac > 0) ? ev / ac : 1.0m;
                var etc = Math.Max(0, (cpi > 0) ? (bac - ev) / cpi : (bac - ev));

                weeklyData[date] = (ptc, etc);
            }

            return (true, weeklyData);
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
            var tachesEligibles = FilterTasksForEVM(toutesLesTaches);

            return tachesEligibles.Count(t =>
                t.DateFinPlanifiee.HasValue &&
                t.DateFinPlanifiee.Value.Date <= dateReference.Date);
        }

        public virtual MetierTensionData CalculerTensionMetierPourPeriodeFuture(DateTime dateDebut, int nombreJours)
        {
            if (_currentPlanning == null || _currentConfig == null)
                return new MetierTensionData { NomMetier = "N/A", TauxOccupation = 0 };

            var dateFin = dateDebut.AddDays(nombreJours);
            var tensionData = CalculateProfessionTension(dateDebut, dateFin);

            return tensionData ?? new MetierTensionData { NomMetier = "Aucun travail planifié", TauxOccupation = 0 };
        }

        #endregion

        #region Méthodes d'Agrégation pour TaskManagerService

        public virtual IReadOnlyDictionary<string, PlanningInfoPourTache> ObtenirInfosPlanificationPourToutesLesTaches()
        {
            if (_currentPlanning == null) return new Dictionary<string, PlanningInfoPourTache>();

            var allSegments = GetAllSegments();
            if (!allSegments.Any()) return new Dictionary<string, PlanningInfoPourTache>();

            return BuildPlanningInfoForParentTasks(allSegments);
        }

        public virtual IReadOnlyDictionary<string, PlanningInfoPourTache> ObtenirInfosPlanificationPourTachesFeuilles()
        {
            if (_currentPlanning == null) return new Dictionary<string, PlanningInfoPourTache>();

            var allSegments = GetAllSegments();
            if (!allSegments.Any()) return new Dictionary<string, PlanningInfoPourTache>();

            return BuildPlanningInfoForLeafTasks(allSegments);
        }

        #endregion

        #region Méthodes de Vue & Calcul

        public virtual IReadOnlyList<LogPlanningJournalier> RetournePlanningDetailleParJour()
        {
            if (_currentPlanning == null || !_currentPlanning.SegmentsParOuvrierId.Any())
            {
                return new List<LogPlanningJournalier>();
            }

            return BuildDailyPlanningLogs();
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

        #region Méthodes privées - Filtrage et validation

        private IReadOnlyList<Tache> FilterTasksForEVM(IReadOnlyList<Tache> toutesLesTaches)
        {
            return toutesLesTaches
                .Where(t => string.IsNullOrEmpty(t.ParentId) && !t.EstJalon)
                .ToList();
        }

        private bool IsBaselineValid()
        {
            return _currentBaseline?.ConsPlanningInitial != null;
        }

        private string GetTacheMereId(Tache tache)
        {
            return string.IsNullOrEmpty(tache.ParentId) ? tache.TacheId : tache.ParentId;
        }

        #endregion

        #region Méthodes privées - Calculs EVM

        private decimal CalculerValeurPlanifieeCumulative(DateTime dateRef)
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

        private decimal CalculerValeurAcquiseCumulative(DateTime dateRef, IReadOnlyList<Tache> tachesEligibles)
        {
            if (_currentBaseline?.BudgetInitialParTacheId == null)
            {
                return 0m;
            }

            var tachesTerminees = tachesEligibles
                .Where(t => t.Statut == Statut.Terminée &&
                           t.DateFinReelle.HasValue &&
                           t.DateFinReelle.Value.Date <= dateRef.Date);

            decimal totalEV = 0m;
            foreach (var tache in tachesTerminees)
            {
                var tacheMereId = GetTacheMereId(tache);
                if (_currentBaseline.BudgetInitialParTacheId.TryGetValue(tacheMereId, out decimal budgetTache))
                {
                    totalEV += budgetTache;
                }
            }

            return totalEV;
        }

        private decimal CalculerCoutReelCumulatif(DateTime dateRef)
        {
            if (_currentPlanning == null || _currentConfig == null) return 0m;

            var heuresParJour = (decimal)_currentConfig.HeuresTravailEffectifParJour;
            if (heuresParJour == 0) return 0m;

            decimal totalAC = 0m;
            totalAC += CalculerCoutMainOeuvre(dateRef, heuresParJour);
            totalAC += CalculerCoutIndirects(dateRef);

            return totalAC;
        }

        private decimal CalculerCoutMainOeuvre(DateTime dateRef, decimal heuresParJour)
        {
            var segmentsPasses = _currentPlanning!.SegmentsParOuvrierId.Values
                .SelectMany(s => s)
                .Where(s => s.Jour.Date < dateRef.Date);

            decimal cout = 0m;
            foreach (var segment in segmentsPasses)
            {
                var ouvrier = _ressourceService.GetOuvrierById(segment.OuvrierId);
                if (ouvrier != null)
                {
                    cout += ((decimal)segment.HeuresTravaillees / heuresParJour) * ouvrier.CoutJournalier;
                }
            }

            return cout;
        }

        private decimal CalculerCoutIndirects(DateTime dateRef)
        {
            var dateDebutProjet = _currentPlanning!.DateDebutProjet;
            if (dateDebutProjet == default) return 0m;

            var dateFinCalcul = (dateRef > _currentPlanning.DateFinProjet) ? _currentPlanning.DateFinProjet : dateRef;
            int joursOuvresPasses = GetNombreJoursOuvres(dateDebutProjet, dateFinCalcul);

            return (decimal)joursOuvresPasses * _currentConfig!.CoutIndirectJournalierAbsolu;
        }

        /// <summary>
        /// NOUVEAU : Conversion centralisée de la Schedule Variance en jours
        /// </summary>
        private double CalculerScheduleVarianceEnJours(decimal sv)
        {
            if (_currentBaseline == null) return 0.0;

            double dureePlanifieeJours = (_currentBaseline.DateFinPlanifieeInitiale - _currentBaseline.DateCreation).TotalDays;
            if (dureePlanifieeJours > 0 && _currentBaseline.BudgetAtCompletion > 0)
            {
                decimal coutMoyenParJour = _currentBaseline.BudgetAtCompletion / (decimal)dureePlanifieeJours;
                if (coutMoyenParJour > 0)
                {
                    return (double)(sv / coutMoyenParJour);
                }
            }

            return 0.0;
        }

        #endregion

        #region Méthodes privées - Construction des courbes

        private Dictionary<DateTime, decimal> BuildPlannedValueCurve()
        {
            var pvCurve = new Dictionary<DateTime, decimal>();

            if (_currentBaseline?.CourbePlannedValueCumulative != null)
            {
                foreach (var kvp in _currentBaseline.CourbePlannedValueCumulative.OrderBy(x => x.Key))
                {
                    pvCurve[kvp.Key.Date] = kvp.Value;
                }
            }

            return pvCurve;
        }

        private (Dictionary<DateTime, decimal> EvCurve, Dictionary<DateTime, decimal> AcCurve)
            BuildEarnedValueAndActualCostCurves(DateTime dateDebut, DateTime dateFinCalcul, IReadOnlyList<Tache> tachesEligibles)
        {
            var evCurve = new Dictionary<DateTime, decimal>();
            var acCurve = new Dictionary<DateTime, decimal>();

            if (dateDebut.Date > dateFinCalcul.Date)
            {
                return (evCurve, acCurve);
            }

            for (var date = dateDebut.Date; date <= dateFinCalcul.Date; date = date.AddDays(1))
            {
                evCurve[date] = CalculerValeurAcquiseCumulative(date, tachesEligibles);
                acCurve[date] = CalculerCoutReelCumulatif(date);
            }

            return (evCurve, acCurve);
        }

        private void SynchronizeCurveDates(Dictionary<DateTime, decimal> pvCurve,
                                         Dictionary<DateTime, decimal> evCurve,
                                         Dictionary<DateTime, decimal> acCurve)
        {
            foreach (var date in pvCurve.Keys.ToList())
            {
                if (!evCurve.ContainsKey(date))
                    evCurve[date] = evCurve.Any() ? evCurve.Last().Value : 0;
                if (!acCurve.ContainsKey(date))
                    acCurve[date] = acCurve.Any() ? acCurve.Last().Value : 0;
            }
        }

        #endregion

        #region Méthodes privées - Utilitaires

        private HashSet<string> GetIdsTachesMeresFromSegments(ConsolidatedPlanning planning)
        {
            return planning.SegmentsParOuvrierId.Values
                .SelectMany(segments => segments)
                .Select(s => s.ParentTacheId ?? s.TacheId)
                .Distinct()
                .ToHashSet();
        }

        private void RemoveSegmentsForTasks(ConsolidatedPlanning planning, HashSet<string> taskIds)
        {
            foreach (var segmentsList in planning.SegmentsParOuvrierId.Values)
            {
                segmentsList.RemoveAll(s => taskIds.Contains(s.ParentTacheId ?? s.TacheId));
            }
        }

        private void MergeNewSegments(ConsolidatedPlanning currentPlanning, ConsolidatedPlanning newPlanning)
        {
            foreach (var kvp in newPlanning.SegmentsParOuvrierId)
            {
                var ouvrierId = kvp.Key;
                var nouveauxSegments = kvp.Value;

                if (currentPlanning.SegmentsParOuvrierId.TryGetValue(ouvrierId, out var segmentsExistants))
                {
                    segmentsExistants.AddRange(nouveauxSegments);
                }
                else
                {
                    currentPlanning.SegmentsParOuvrierId[ouvrierId] = nouveauxSegments;
                }
            }
        }

        private List<SegmentDeTravail> GetAllSegments()
        {
            var calSegments = _currentPlanning.SegmentsParOuvrierId.Values.SelectMany(list => list).ToList();
            return calSegments;
        }

        private IReadOnlyDictionary<string, PlanningInfoPourTache> BuildPlanningInfoForParentTasks(List<SegmentDeTravail> allSegments)
        {
            var mapOuvrierIdToName = _ressourceService.GetAllOuvriers().ToDictionary(o => o.OuvrierId, o => o.NomComplet);
            var segmentsParTacheMere = allSegments.GroupBy(s => s.ParentTacheId ?? s.TacheId);
            var resultat = new Dictionary<string, PlanningInfoPourTache>();

            foreach (var groupe in segmentsParTacheMere)
            {
                var tacheMereId = groupe.Key;
                var segmentsDeLaTache = groupe.ToList();
                var affectations = BuildAffectations(segmentsDeLaTache, mapOuvrierIdToName);

                resultat[tacheMereId] = new PlanningInfoPourTache
                {
                    EstConteneur = segmentsDeLaTache.Any(s => !string.IsNullOrEmpty(s.ParentTacheId)),
                    DateDebut = segmentsDeLaTache.Min(s => s.Jour.Add(s.HeureDebut)),
                    DateFin = segmentsDeLaTache.Max(s => s.Jour.Add(s.HeureFin)),
                    Affectations = affectations
                };
            }

            return resultat;
        }

        private IReadOnlyDictionary<string, PlanningInfoPourTache> BuildPlanningInfoForLeafTasks(List<SegmentDeTravail> allSegments)
        {
            var mapOuvrierIdToName = _ressourceService.GetAllOuvriers().ToDictionary(o => o.OuvrierId, o => o.NomComplet);
            var segmentsParTacheFeuille = allSegments.GroupBy(s => s.TacheId);
            var resultat = new Dictionary<string, PlanningInfoPourTache>();

            foreach (var groupe in segmentsParTacheFeuille)
            {
                var tacheFeuilleId = groupe.Key;
                var segmentsDeLaTache = groupe.ToList();
                var affectations = BuildAffectations(segmentsDeLaTache, mapOuvrierIdToName);

                resultat[tacheFeuilleId] = new PlanningInfoPourTache
                {
                    EstConteneur = false,
                    DateDebut = segmentsDeLaTache.Min(s => s.Jour.Add(s.HeureDebut)),
                    DateFin = segmentsDeLaTache.Max(s => s.Jour.Add(s.HeureFin)),
                    Affectations = affectations
                };
            }

            return resultat;
        }

        private List<AffectationOuvrier> BuildAffectations(List<SegmentDeTravail> segments, Dictionary<string, string> mapOuvrierIdToName)
        {
            return segments.GroupBy(s => s.OuvrierId)
                .Select(g => new AffectationOuvrier
                {
                    OuvrierId = g.Key,
                    NomOuvrier = mapOuvrierIdToName.GetValueOrDefault(g.Key, g.Key),
                    HeuresTravaillees = (int)g.Sum(s => s.HeuresTravaillees)
                })
                .ToList();
        }

        private MetierTensionData? CalculateProfessionTension(DateTime dateDebut, DateTime dateFin)
        {
            var ouvriers = _ressourceService.GetAllOuvriers();
            var metiers = _ressourceService.GetAllMetiers().ToDictionary(m => m.MetierId, m => m.Nom);

            var heuresPlanifieesParMetierId = _currentPlanning!.SegmentsParOuvrierId.Values
                .SelectMany(segments => segments)
                .Where(s => s.Jour >= dateDebut.Date && s.Jour < dateFin.Date)
                .Join(ouvriers, segment => segment.OuvrierId, ouvrier => ouvrier.OuvrierId,
                      (segment, ouvrier) => new { segment.HeuresTravaillees, ouvrier.MetierId })
                .GroupBy(j => j.MetierId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.HeuresTravaillees));

            if (!heuresPlanifieesParMetierId.Any()) return null;

            int joursOuvresPeriode = GetNombreJoursOuvres(dateDebut, dateFin);
            double heuresDisponiblesParOuvrier = joursOuvresPeriode * _currentConfig!.HeuresTravailEffectifParJour;

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

            if (tensionParMetier == null) return null;

            return new MetierTensionData
            {
                NomMetier = metiers.GetValueOrDefault(tensionParMetier.MetierId, "Inconnu"),
                TauxOccupation = tensionParMetier.TauxOccupation
            };
        }

        private IReadOnlyList<LogPlanningJournalier> BuildDailyPlanningLogs()
        {
            var segmentsParJour = _currentPlanning!.SegmentsParOuvrierId
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

        #endregion
    }
}