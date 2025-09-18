// Fichier: /Services/Usecases/PilotageProjetUseCase.cs Version 0.6.5

using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.UseCases;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PlanAthena.Services.Usecases
{
    /// <summary>
    /// Cas d'utilisation pour tout ce qui concerne le pilotage et le suivi du projet.
    /// Il agrège les données de plusieurs services pour les présenter aux vues du Cockpit.
    /// Version 0.7.0 : Refactorisation pour utiliser uniquement PlanningService.GetRapportEVMComplet()
    /// </summary>
    public class PilotageProjetUseCase
    {
        private readonly TaskManagerService _taskManagerService;
        private readonly PlanningService _planningService;
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;

        public PilotageProjetUseCase(
            TaskManagerService taskManagerService,
            PlanningService planningService,
            ProjetService projetService,
            RessourceService ressourceService)
        {
            _taskManagerService = taskManagerService ?? throw new ArgumentNullException(nameof(taskManagerService));
            _planningService = planningService ?? throw new ArgumentNullException(nameof(planningService));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
        }

        #region Cockpit (Alimentation des KPIs)

        public virtual ProjectWeatherData ObtenirMeteoProjet()
        {
            var toutesLesTaches = _taskManagerService.ObtenirToutesLesTaches();
            var dateReference = DateTime.Today;

            // --- CENTRALISATION : Un seul appel au rapport EVM complet ---
            var evmReport = _planningService.GetRapportEVMComplet(dateReference, toutesLesTaches);

            // --- Calcul des nouveaux indicateurs métier ---

            // 1. Calcul de la dérive globale (pire des deux indicateurs)
            double derivePerformancePourcentage = 0.0;
            if (evmReport.BaselineExists)
            {
                double spiDeriv = (1.0 - evmReport.SchedulePerformanceIndex) * 100.0;
                double cpiDeriv = (1.0 - evmReport.CostPerformanceIndex) * 100.0;
                derivePerformancePourcentage = Math.Max(spiDeriv, cpiDeriv);
            }

            // 2. Calcul de la disponibilité des ressources (sur les 14 prochains jours)
            var tensionMetier = _planningService.CalculerTensionMetierPourPeriodeFuture(dateReference, 14);
            double disponibilitePourcentage = (1.0 - tensionMetier.TauxOccupation) * 100.0;

            // 3. Comptage des tâches planifiées non démarrées (en retard de démarrage)
            var tachesRetardDemarrage = toutesLesTaches
                .Where(t => t.Statut == Statut.Planifiée &&
                           t.DateDebutPlanifiee.HasValue &&
                           t.DateDebutPlanifiee.Value.Date < dateReference.Date)
                .Count();

            // 4. Calcul du pourcentage de tâches terminées en retard
            var tachesTerminees = toutesLesTaches.Where(t => t.Statut == Statut.Terminée).ToList();
            double pourcentageTachesTermineesEnRetard = 0.0;
            if (tachesTerminees.Any())
            {
                int tachesTermineesEnRetard = tachesTerminees
                    .Count(t => t.DateFinReelle.HasValue &&
                               t.DateFinPlanifiee.HasValue &&
                               t.DateFinReelle.Value.Date > t.DateFinPlanifiee.Value.Date);

                pourcentageTachesTermineesEnRetard = (tachesTermineesEnRetard * 100.0) / tachesTerminees.Count;
            }

            // 5. Fallback : calcul de la dérive temporelle classique (pour le cas sans baseline)
            double deriveJoursClassique = toutesLesTaches
                .Where(t => t.Statut != Statut.Terminée &&
                            t.DateFinPlanifiee.HasValue &&
                            t.DateFinPlanifiee.Value.Date < dateReference.Date)
                .Sum(t => (dateReference.Date - t.DateFinPlanifiee.Value.Date).TotalDays);

            // --- LOGIQUE DE MÉTÉO (conditions selon vos spécifications) ---

            ProjectWeatherStatus statut = ProjectWeatherStatus.Sunny; // Par défaut

            // Test STORMY : conditions critiques (OU entre les groupes)
            if ((derivePerformancePourcentage > 25.0 && disponibilitePourcentage < 30.0) ||  // ET dans ce groupe
                (tachesRetardDemarrage >= 3) ||
                (pourcentageTachesTermineesEnRetard > 30.0))
            {
                statut = ProjectWeatherStatus.Stormy;
            }
            // Test RAINY : problèmes avérés (OU entre les groupes) 
            else if ((derivePerformancePourcentage > 15.0 && disponibilitePourcentage < 50.0) ||  // ET dans ce groupe
                     (tachesRetardDemarrage >= 3))  // Note: même condition que Stormy, mais testé après
            {
                statut = ProjectWeatherStatus.Rainy;
            }
            // Test CLOUDY : début de dérive (OU entre les conditions)
            else if ((derivePerformancePourcentage > 5.0) ||
                     (tachesRetardDemarrage >= 1))
            {
                statut = ProjectWeatherStatus.Cloudy;
            }

            // Fallback si pas de baseline EVM : utiliser l'ancienne logique basée sur les jours
            if (!evmReport.BaselineExists && statut == ProjectWeatherStatus.Sunny)
            {
                int deriveJoursEntiers = (int)Math.Ceiling(deriveJoursClassique);
                if (deriveJoursEntiers > 1) statut = ProjectWeatherStatus.Cloudy;
                if (deriveJoursEntiers > 5) statut = ProjectWeatherStatus.Rainy;
                if (deriveJoursEntiers > 10) statut = ProjectWeatherStatus.Stormy;
            }

            return new ProjectWeatherData
            {
                DerivPlanningJours = (int)Math.Ceiling(deriveJoursClassique),
                DisponibiliteRessourcesPourcentage = disponibilitePourcentage / 100.0, // Gardé en ratio pour compatibilité
                DeviationBudgetPourcentage = evmReport.DeviationBudgetPourcentage,
                Statut = statut
            };
        }

        public virtual CockpitKpiData ObtenirIndicateursCockpit(List<Tache> tachesFiltrees = null)
        {
            var toutesLesTaches = tachesFiltrees ?? _taskManagerService.ObtenirToutesLesTaches();
            var tachesNonConteneurs = toutesLesTaches.Where(t => !t.EstConteneur && t.Statut != Statut.Estimée).ToList();

            if (!tachesNonConteneurs.Any())
            {
                return new CockpitKpiData { LotLePlusARisqueNom = "N/A", MetierLePlusEnTensionNom = "N/A" };
            }

            // 1. Progression Globale (logique métier inchangée)
            int nombreTerminees = tachesNonConteneurs.Count(t => t.Statut == Statut.Terminée);
            double progressionGlobale = (tachesNonConteneurs.Count > 0) ? (nombreTerminees * 100.0 / tachesNonConteneurs.Count) : 0;

            // 2. Lot le plus à risque (logique métier inchangée)
            string nomLotRisque = "N/A";
            double deriveJoursRisque = 0;
            var dateReference = DateTime.Today;

            var lotsAvecDerive = tachesNonConteneurs
                .Where(t => (t.Statut == Statut.EnCours || t.Statut == Statut.EnRetard) && t.DateFinPlanifiee.HasValue && t.DateFinPlanifiee.Value < dateReference)
                .GroupBy(t => t.LotId)
                .Select(g => new
                {
                    LotId = g.Key,
                    DeriveMoyenne = g.Average(t => (dateReference - t.DateFinPlanifiee.Value).TotalDays)
                })
                .OrderByDescending(x => x.DeriveMoyenne)
                .FirstOrDefault();

            if (lotsAvecDerive != null && !string.IsNullOrEmpty(lotsAvecDerive.LotId))
            {
                nomLotRisque = _projetService.ObtenirLotParId(lotsAvecDerive.LotId)?.Nom ?? lotsAvecDerive.LotId;
                deriveJoursRisque = Math.Round(lotsAvecDerive.DeriveMoyenne, 1);
            }

            // 3. Métier le plus en tension (inchangé)
            var tensionMetier = _planningService.CalculerTensionMetierPourPeriodeFuture(dateReference, 14);

            // --- CENTRALISATION : Un seul appel au rapport EVM complet ---
            var evmReport = _planningService.GetRapportEVMComplet(dateReference, toutesLesTaches);

            return new CockpitKpiData
            {
                ProgressionGlobalePourcentage = progressionGlobale,
                LotLePlusARisqueNom = nomLotRisque,
                LotLePlusARisqueDeriveJours = deriveJoursRisque,
                MetierLePlusEnTensionNom = tensionMetier.NomMetier,
                MetierLePlusEnTensionTauxOccupation = tensionMetier.TauxOccupation,

                // --- KPIs EVM : Utilisation directe des valeurs calculées par PlanningService ---
                BudgetAtCompletion = evmReport.BudgetAtCompletion,
                EstimateAtCompletion = evmReport.EstimateAtCompletion,
                ScheduleVarianceDays = evmReport.ScheduleVarianceDays,
                CostVariance = evmReport.CostVariance,
                SchedulePerformanceIndex = evmReport.SchedulePerformanceIndex,
                CostPerformanceIndex = evmReport.CostPerformanceIndex
            };
        }

        #endregion

        #region EVM Graph

        public virtual EvmGraphData ObtenirDonneesGraphiqueEVM()
        {
            var toutesLesTaches = _taskManagerService.ObtenirToutesLesTaches();
            var (baselineExists, pvCurve, evCurve, acCurve) = _planningService.GetCourbesEVMHistoriques(toutesLesTaches);

            if (!baselineExists)
            {
                return new EvmGraphData { BaselineExists = false };
            }

            var datesTriees = pvCurve.Keys.OrderBy(d => d).ToList();

            // On récupère le BAC depuis la baseline
            var bac = _planningService.GetBaseline()?.BudgetAtCompletion ?? 0m;

            return new EvmGraphData
            {
                BaselineExists = true,
                Dates = datesTriees,
                PlannedValues = datesTriees.Select(d => (double)pvCurve.GetValueOrDefault(d, 0)).ToList(),
                EarnedValues = datesTriees.Select(d => (double)evCurve.GetValueOrDefault(d, 0)).ToList(),
                ActualCosts = datesTriees.Select(d => (double)acCurve.GetValueOrDefault(d, 0)).ToList(),
                BudgetAtCompletion = bac
            };
        }

        public virtual EtcVsPtcGraphData ObtenirDonneesGraphiqueEtcVsPtc()
        {
            var toutesLesTaches = _taskManagerService.ObtenirToutesLesTaches();
            var (baselineExists, weeklyData) = _planningService.GetEtcVsPtcHistorique(toutesLesTaches);

            if (!baselineExists || !weeklyData.Any())
            {
                return new EtcVsPtcGraphData { BaselineExists = false };
            }

            var orderedData = weeklyData.OrderBy(x => x.Key).ToList();

            return new EtcVsPtcGraphData
            {
                BaselineExists = true,
                Dates = orderedData.Select(kvp => kvp.Key).ToList(),
                PlanToCompleteValues = orderedData.Select(kvp => (double)kvp.Value.Ptc).ToList(),
                EstimateToCompleteValues = orderedData.Select(kvp => (double)kvp.Value.Etc).ToList()
            };
        }

        #endregion

        #region TaskList (Gestion de la Vue "Liste de Tâches")

        public virtual TaskListData ObtenirDonneesPourTaskList()
        {
            var toutesLesTaches = _taskManagerService.ObtenirToutesLesTaches()
                .Where(t => t.Type == TypeActivite.Tache && string.IsNullOrEmpty(t.ParentId))
                .ToList();
            var mapMetiers = _ressourceService.GetAllMetiers().ToDictionary(m => m.MetierId, m => m.Nom);

            var items = toutesLesTaches.Select(tache =>
            {
                string nomMetier = mapMetiers.GetValueOrDefault(tache.MetierId, "-");
                string nomsOuvriers = tache.Affectations.Any()
                               ? string.Join(", ", tache.Affectations.Select(a => a.NomOuvrier))
                               : "-";

                return new TaskListItem
                {
                    TacheId = tache.TacheId,
                    ParentId = tache.ParentId,
                    NomTache = tache.TacheNom,
                    NomMetier = nomMetier,
                    NomsOuvriersAffectes = nomsOuvriers,
                    Statut = tache.Statut.ToString(),
                    EstConteneur = tache.EstConteneur,
                    DateDebutPlanifiee = tache.DateDebutPlanifiee,
                    DateFinPlanifiee = tache.DateFinPlanifiee,
                    DateDebutReelle = tache.DateDebutReelle,
                    DateFinReelle = tache.DateFinReelle,
                    TacheOriginale = tache
                };
            }).ToList();

            var lotsDisponibles = toutesLesTaches
                .Select(t => t.LotId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .Select(id => _projetService.ObtenirLotParId(id)?.Nom ?? id)
                .OrderBy(nom => nom)
                .ToList();

            var metiersDisponibles = mapMetiers.Values.Distinct().OrderBy(nom => nom).ToList();

            return new TaskListData
            {
                Items = items,
                LotsDisponibles = lotsDisponibles,
                MetiersDisponibles = metiersDisponibles
            };
        }

        public virtual void MettreAJourAvancementTache(string tacheId, DateTime? debutReel, DateTime? finReelle)
        {
            var tache = _taskManagerService.ObtenirTache(tacheId);
            if (tache == null) return;

            tache.DateDebutReelle = debutReel;
            tache.DateFinReelle = finReelle;

            var maintenant = DateTime.Now;

            if (debutReel.HasValue && finReelle.HasValue)
            {
                tache.Statut = Statut.Terminée;
            }
            else if (debutReel.HasValue)
            {
                if (tache.DateFinPlanifiee.HasValue && maintenant > tache.DateFinPlanifiee.Value)
                {
                    tache.Statut = Statut.EnRetard;
                }
                else
                {
                    tache.Statut = Statut.EnCours;
                }
            }
            else
            {
                if (tache.DateFinPlanifiee.HasValue && maintenant > tache.DateFinPlanifiee.Value)
                {
                    tache.Statut = Statut.EnRetard;
                }
                else if (tache.DateDebutPlanifiee.HasValue)
                {
                    tache.Statut = Statut.Planifiée;
                }
                else
                {
                    tache.Statut = Statut.Estimée;
                }
            }

            _taskManagerService.ModifierTache(tache);
            _planningService.ReconcilierAvecAvancementReel(tache);
        }

        public virtual void ExporterVueTaskList(List<Tache> tachesAExporter)
        {
            throw new NotImplementedException("Export non implémenté en V1.");
        }

        #endregion

        #region Planning

        public virtual (DateTime? Start, DateTime? End) ObtenirPlageDeDatesDuPlanning()
        {
            var planning = _planningService.GetCurrentPlanning();
            if (planning == null || !planning.SegmentsParOuvrierId.Any())
            {
                return (null, null);
            }

            var tousLesSegments = planning.SegmentsParOuvrierId.SelectMany(kvp => kvp.Value).ToList();

            if (!tousLesSegments.Any())
            {
                return (null, null);
            }

            DateTime dateMin = tousLesSegments.Min(s => s.Jour);
            DateTime dateMax = tousLesSegments.Max(s => s.Jour);

            return (dateMin, dateMax);
        }

        public virtual PlanningViewData ObtenirDonneesPourPlanningView(DateTime dateDebut, int nombreDeJours)
        {
            var planning = _planningService.GetCurrentPlanning();
            var config = _planningService.GetCurrentConfig();
            if (planning == null || config == null || !planning.SegmentsParOuvrierId.Any())
            {
                return new PlanningViewData();
            }

            var toutesLesTaches = _taskManagerService.ObtenirToutesLesTaches();
            var mapStatuts = toutesLesTaches.ToDictionary(t => t.TacheId, t => t.Statut);
            var dateFin = dateDebut.Date.AddDays(nombreDeJours);

            var tousLesSegmentsDuProjet = planning.SegmentsParOuvrierId.SelectMany(kvp => kvp.Value);
            var tousLesOuvrierIds = tousLesSegmentsDuProjet.Select(s => s.OuvrierId).Distinct().ToList();
            var tousLesOuvriersConcernes = _ressourceService.GetAllOuvriers()
                .Where(o => tousLesOuvrierIds.Contains(o.OuvrierId))
                .OrderBy(o => o.MetierId)
                .ThenBy(o => o.NomComplet)
                .ToList();

            if (!tousLesOuvriersConcernes.Any())
            {
                return new PlanningViewData();
            }

            var resultat = new PlanningViewData
            {
                Ouvriers = tousLesOuvriersConcernes,
                Jours = Enumerable.Range(0, nombreDeJours).Select(i => dateDebut.Date.AddDays(i)).ToList()
            };
            var tousLesMetiers = _ressourceService.GetAllMetiers();
            resultat.Metiers = tousLesMetiers.ToDictionary(m => m.MetierId, m => m);
            resultat.MetierColors = tousLesMetiers.ToDictionary(
                m => m.MetierId,
                m => {
                    try { return ColorTranslator.FromHtml(m.CouleurHex); }
                    catch { return Color.LightGray; }
                });

            foreach (var ouvrier in tousLesOuvriersConcernes)
            {
                resultat.BlocksParOuvrier[ouvrier.OuvrierId] = new List<PlanningBlock>[nombreDeJours];
                for (int i = 0; i < nombreDeJours; i++)
                {
                    resultat.BlocksParOuvrier[ouvrier.OuvrierId][i] = new List<PlanningBlock>();
                }
            }

            var segmentsFiltresPourPeriode = tousLesSegmentsDuProjet
                .Where(s => s.Jour >= dateDebut.Date && s.Jour < dateFin)
                .ToList();

            var heuresDebutJournee = config.HeureDebutJournee;
            var heuresTravailJournee = config.DureeJournaliereStandardHeures;

            foreach (var segment in segmentsFiltresPourPeriode)
            {
                double startOffset = (segment.HeureDebut.TotalHours - heuresDebutJournee) / heuresTravailJournee;
                double width = segment.HeuresTravaillees / heuresTravailJournee;

                var block = new PlanningBlock
                {
                    TacheId = segment.TacheId,
                    TacheNom = segment.TacheNom,
                    Statut = mapStatuts.GetValueOrDefault(segment.ParentTacheId ?? segment.TacheId, Statut.Planifiée),
                    StartOffsetPercent = Math.Max(0, Math.Min(1, startOffset)),
                    WidthPercent = Math.Max(0, Math.Min(1, width))
                };

                int dayIndex = (segment.Jour.Date - dateDebut.Date).Days;
                if (dayIndex >= 0 && dayIndex < nombreDeJours)
                {
                    if (resultat.BlocksParOuvrier.ContainsKey(segment.OuvrierId))
                    {
                        resultat.BlocksParOuvrier[segment.OuvrierId][dayIndex].Add(block);
                    }
                }
            }

            return resultat;
        }

        #endregion
    }
}