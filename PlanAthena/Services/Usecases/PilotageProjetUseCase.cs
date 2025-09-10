// Emplacement: /Services/Usecases/PilotageProjetUseCase.cs Version 0.5.0
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.UseCases; 
using System.Collections.Generic;
using System.Linq;
using System;

namespace PlanAthena.Services.Usecases
{

    public class PilotageProjetUseCase
    {
        private readonly TaskManagerService _taskManagerService;
        private readonly PlanningService _planningService;
        private readonly ProjetService _projetService; // AJOUT
        private readonly RessourceService _ressourceService;
        // private readonly AnalysisService _analysisService; // N'est plus utilisé ici
        // private readonly PlanningExcelExportService _exportService; // Pas utilisé en V1

        public PilotageProjetUseCase(
            TaskManagerService taskManagerService,
            PlanningService planningService,
            ProjetService projetService, // AJOUT
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

            // 1. Calcul de la Dérive Planning
            var tachesEnRetard = toutesLesTaches.Where(t => t.Statut == Statut.EnRetard && t.DateFinPlanifiee.HasValue).ToList();
            double deriveMaxJours = 0;
            if (tachesEnRetard.Any())
            {
                deriveMaxJours = tachesEnRetard.Max(t => (DateTime.Today - t.DateFinPlanifiee.Value).TotalDays);
            }

            // 2. Calcul de la Disponibilité des Ressources
            // On réutilise la méthode existante qui calcule la tension
            var tensionMetier = _planningService.CalculerTensionMetierPourPeriodeFuture(DateTime.Today, 14);
            double disponibilitePourcentage = 1.0 - tensionMetier.TauxOccupation; // C'est une approximation globale

            // 3. Déviation Budget (V1)
            double deviationBudget = 0.0;

            // 4. Détermination du Statut Météo
            ProjectWeatherStatus statut = ProjectWeatherStatus.Sunny;

            // Critères pour passer à "Nuageux" (Orages en vue)
            //if (deriveMaxJours > 5 || disponibilitePourcentage < 0.30) // Plus de 5 jours de retard OU moins de 30% de dispo
            if (deriveMaxJours > 1 || disponibilitePourcentage < 1.00)
            {
                statut = ProjectWeatherStatus.Cloudy;
            }

            // Critères pour passer à "Orageux" (Situation critique)
            //if (deriveMaxJours > 10 || disponibilitePourcentage < 0.15) // Plus de 10 jours de retard OU moins de 15% de dispo
            if (deriveMaxJours > 5 && disponibilitePourcentage < 0.50)
            {
                statut = ProjectWeatherStatus.Stormy;
            }

            return new ProjectWeatherData
            {
                DerivPlanningJours = Math.Round(deriveMaxJours, 1),
                DisponibiliteRessourcesPourcentage = disponibilitePourcentage,
                DeviationBudgetPourcentage = deviationBudget,
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

            // 1. Progression Globale
            int nombreTerminees = tachesNonConteneurs.Count(t => t.Statut == Statut.Terminée);
            double progressionGlobale = (tachesNonConteneurs.Count > 0) ? (nombreTerminees * 100.0 / tachesNonConteneurs.Count) : 0;

            // 2. SPI (Schedule Performance Index)
            var dateReference = DateTime.Today;
            int nombreDevraientEtreTerminees = _planningService.ObtenirNombreTachesQuiDevraientEtreTerminees(dateReference, toutesLesTaches);
            double spi = (nombreDevraientEtreTerminees > 0) ? (nombreTerminees / (double)nombreDevraientEtreTerminees) : 1.0;

            // 3. CPI (Cost Performance Index)
            double cpi = _planningService.CalculerPerformanceCoutCPI(toutesLesTaches);

            // 4. Lot le plus à risque
            string nomLotRisque = "N/A";
            double deriveJoursRisque = 0;

            var lotsAvecDerive = tachesNonConteneurs
                .Where(t => (t.Statut == Statut.EnCours || t.Statut == Statut.EnRetard) && t.DateFinPlanifiee.HasValue && t.DateFinPlanifiee.Value < dateReference)
                .GroupBy(t => t.LotId)
                .Select(g => new {
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

            // 5. Métier le plus en tension
            var tensionMetier = _planningService.CalculerTensionMetierPourPeriodeFuture(DateTime.Today, 14);

            return new CockpitKpiData
            {
                ProgressionGlobalePourcentage = progressionGlobale,
                PerformanceCalendrierSPI = spi,
                PerformanceCoutCPI = cpi,
                LotLePlusARisqueNom = nomLotRisque,
                LotLePlusARisqueDeriveJours = deriveJoursRisque,
                MetierLePlusEnTensionNom = tensionMetier.NomMetier,
                MetierLePlusEnTensionTauxOccupation = tensionMetier.TauxOccupation
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

            // --- NOUVELLE LOGIQUE DE STATUT ---
            if (debutReel.HasValue && finReelle.HasValue)
            {
                // Un état final : la tâche est terminée, peu importe si elle était en retard.
                tache.Statut = Statut.Terminée;
            }
            else if (debutReel.HasValue) // La tâche a démarré mais n'est pas finie
            {
                // On vérifie si elle est déjà en retard par rapport à sa fin planifiée.
                if (tache.DateFinPlanifiee.HasValue && maintenant > tache.DateFinPlanifiee.Value)
                {
                    tache.Statut = Statut.EnRetard;
                }
                else
                {
                    tache.Statut = Statut.EnCours;
                }
            }
            else // Aucune date réelle saisie, on revient à un statut prévisionnel
            {
                if (tache.DateFinPlanifiee.HasValue && maintenant > tache.DateFinPlanifiee.Value)
                {
                    // Même si on a effacé les dates réelles, elle est toujours en retard.
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

        // ExporterVueTaskList est hors périmètre pour la V1
        public virtual void ExporterVueTaskList(List<Tache> tachesAExporter)
        {
            throw new NotImplementedException("Export non implémenté en V1.");
        }

        #endregion

        #region Planning

        public virtual PlanningViewData ObtenirDonneesPourPlanningView(DateTime dateDebut, int nombreDeJours)
        {
            var planning = _planningService.GetCurrentPlanning();
            var config = _planningService.GetCurrentConfig(); // Vient d'être ajouté à PlanningService
            if (planning == null || config == null || !planning.SegmentsParOuvrierId.Any())
            {
                return new PlanningViewData(); // Retourne des données vides
            }

            var toutesLesTaches = _taskManagerService.ObtenirToutesLesTaches();
            var mapStatuts = toutesLesTaches.ToDictionary(t => t.TacheId, t => t.Statut);

            var dateFin = dateDebut.Date.AddDays(nombreDeJours);

            // 1. Filtrer les segments pertinents et les ouvriers concernés
            var segmentsFiltres = planning.SegmentsParOuvrierId
                .SelectMany(kvp => kvp.Value)
                .Where(s => s.Jour >= dateDebut.Date && s.Jour < dateFin)
                .ToList();

            var ouvrierIdsConcernes = segmentsFiltres.Select(s => s.OuvrierId).Distinct().ToList();
            var ouvriersConcernes = _ressourceService.GetAllOuvriers().Where(o => ouvrierIdsConcernes.Contains(o.OuvrierId)).OrderBy(o => o.NomComplet).ToList();

            // 2. Préparer la structure de données finale
            var resultat = new PlanningViewData
            {
                Ouvriers = ouvriersConcernes,
                Jours = Enumerable.Range(0, nombreDeJours).Select(i => dateDebut.Date.AddDays(i)).ToList()
            };

            foreach (var ouvrier in ouvriersConcernes)
            {
                resultat.BlocksParOuvrier[ouvrier.OuvrierId] = new List<PlanningBlock>[nombreDeJours];
                for (int i = 0; i < nombreDeJours; i++)
                {
                    resultat.BlocksParOuvrier[ouvrier.OuvrierId][i] = new List<PlanningBlock>();
                }
            }

            // 3. Transformer les segments en PlanningBlocks
            var heuresDebutJournee = config.HeureDebutJournee;
            var heuresTravailJournee = config.DureeJournaliereStandardHeures;

            foreach (var segment in segmentsFiltres)
            {
                double startOffset = (segment.HeureDebut.TotalHours - heuresDebutJournee) / heuresTravailJournee;
                double width = segment.HeuresTravaillees / heuresTravailJournee;

                var block = new PlanningBlock
                {
                    TacheId = segment.TacheId,
                    TacheNom = segment.TacheNom,
                    Statut = mapStatuts.GetValueOrDefault(segment.ParentTacheId ?? segment.TacheId, Statut.Planifiée),
                    StartOffsetPercent = Math.Max(0, Math.Min(1, startOffset)), // Clamp entre 0 et 1
                    WidthPercent = Math.Max(0, Math.Min(1, width))
                };

                int dayIndex = (segment.Jour.Date - dateDebut.Date).Days;
                if (dayIndex >= 0 && dayIndex < nombreDeJours)
                {
                    resultat.BlocksParOuvrier[segment.OuvrierId][dayIndex].Add(block);
                }
            }

            return resultat;
        }

        #endregion
    }
}