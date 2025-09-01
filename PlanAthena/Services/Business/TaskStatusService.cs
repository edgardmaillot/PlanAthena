// Emplacement: /Services/Business/TaskStatusService.cs

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Source de Vérité pour l'état d'avancement des tâches.
    /// Maintient l'état et l'enrichit en agrégeant les données des autres services.
    /// </summary>
    public class TaskStatusService
    {
        // La source de vérité des statuts, persistée entre les sessions.
        private Dictionary<string, Status> _taskStatuses = new();

        private readonly ProjetService _projetService;
        private readonly PlanningService _planningService;
        private readonly RessourceService _ressourceService;

        // DTO interne pour optimiser les requêtes sur le planning.
        private record PlanningTaskDetails(
            DateTime DateDebut,
            DateTime DateFin,
            IReadOnlyList<string> OuvrierIds,
            bool EstConteneur,
            string? ParentTacheId);

        public TaskStatusService(
            ProjetService projetService,
            PlanningService planningService,
            RessourceService ressourceService)
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _planningService = planningService ?? throw new ArgumentNullException(nameof(planningService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
        }

        #region Persistance

        /// <summary>
        /// Charge un dictionnaire de statuts, typiquement au chargement d'un projet.
        /// </summary>
        public virtual void ChargerStatuts(Dictionary<string, Status> statuses)
        {
            _taskStatuses = new Dictionary<string, Status>(statuses ?? new Dictionary<string, Status>());
        }

        /// <summary>
        /// Retourne l'état actuel des statuts pour la sauvegarde.
        /// </summary>
        public IReadOnlyDictionary<string, Status> RetourneTousLesStatuts()
        {
            return _taskStatuses;
        }

        #endregion

        #region Gestion de l'État

        /// <summary>
        /// Met à jour le statut d'une ou plusieurs tâches.
        /// </summary>
        public void ModifierStatutTache(IEnumerable<string> tacheIds, Status newStatus)
        {
            foreach (var tacheId in tacheIds)
            {
                _taskStatuses[tacheId] = newStatus;
            }
        }

        /// <summary>
        /// Met à jour le statut des tâches qui viennent d'être planifiées.
        /// Appelé par l'orchestrateur après un run de la DLL.
        /// </summary>
        public virtual void ModifierTachePlanifiee(IEnumerable<string> plannedTaskIds)
        {
            foreach (var tacheId in plannedTaskIds)
            {
                // On ne met à jour que si le statut n'est pas déjà "en cours" ou "terminé"
                if (!_taskStatuses.TryGetValue(tacheId, out var currentStatus)
                    || (currentStatus != Status.EnCours && currentStatus != Status.Terminee))
                {
                    _taskStatuses[tacheId] = Status.Planifiee;
                }
            }
        }

        /// <summary>
        /// Initialise les statuts de toutes les tâches d'un projet à "NonPlanifiee".
        /// Utile lors de la création d'un nouveau projet.
        /// </summary>
        public virtual void InitialiserStatutsPourNouveauProjet()
        {
            _taskStatuses.Clear();
            var toutesLesTaches = _projetService.ObtenirToutesLesTaches();
            foreach (var tache in toutesLesTaches)
            {
                _taskStatuses[tache.TacheId] = Status.NonPlanifiee;
            }
        }

        #endregion

        #region Consultation Agrégée

        /// <summary>
        /// Retourne une liste enrichie d'informations sur les statuts des tâches, avec filtres optionnels.
        /// </summary>
        public IReadOnlyList<TaskStatusInfo> RetourneStatutTache(string? lotIdFilter = null, TaskTypeFilter? taskTypeFilter = null)
        {
            var toutesLesTaches = _projetService.ObtenirToutesLesTaches();
            var planningActuel = _planningService.GetCurrentPlanning();
            var infosParTacheDepuisPlanning = _PreCalculerInfosPlanning(planningActuel);

            // 1. Filtrer la liste des tâches à traiter
            IEnumerable<Tache> tachesFiltrees = toutesLesTaches;
            if (!string.IsNullOrEmpty(lotIdFilter))
            {
                tachesFiltrees = tachesFiltrees.Where(t => t.LotId == lotIdFilter);
            }

            if (taskTypeFilter.HasValue)
            {
                tachesFiltrees = tachesFiltrees.Where(t => {
                    var isContainer = infosParTacheDepuisPlanning.ContainsKey(t.TacheId) && infosParTacheDepuisPlanning[t.TacheId].EstConteneur;
                    return taskTypeFilter.Value == TaskTypeFilter.Container ? isContainer : !isContainer;
                });
            }

            // 2. Construire le résultat
            var resultats = new List<TaskStatusInfo>();
            foreach (var tache in tachesFiltrees)
            {
                resultats.Add(_ConstruireTaskStatusInfo(tache, infosParTacheDepuisPlanning));
            }

            return resultats;
        }

        /// <summary>
        /// Retourne les informations enrichies pour une seule tâche.
        /// </summary>
        public TaskStatusInfo? GetTaskStatusInfo(string tacheId)
        {
            var tache = _projetService.ObtenirTacheParId(tacheId);
            if (tache == null) return null;

            var planningActuel = _planningService.GetCurrentPlanning();
            var infosParTacheDepuisPlanning = _PreCalculerInfosPlanning(planningActuel);

            return _ConstruireTaskStatusInfo(tache, infosParTacheDepuisPlanning);
        }
        /// <summary>
        /// NOUVEAU: Construit une structure de données détaillée du planning, groupée par jour puis par ouvrier,
        /// optimisée pour un affichage séquentiel comme dans un fichier de log.
        /// </summary>
        /// <returns>Une liste d'objets représentant chaque journée de travail du planning.</returns>
        public IReadOnlyList<LogPlanningJournalier> RetournePlanningDetailleParJour()
        {
            var planning = _planningService.GetCurrentPlanning();
            if (planning == null || !planning.SegmentsParOuvrierId.Any())
            {
                return new List<LogPlanningJournalier>();
            }

            // Étape 1: Aplatir tous les segments et les grouper par jour.
            var segmentsParJour = planning.SegmentsParOuvrierId
                .SelectMany(kvp => kvp.Value) // Prend tous les segments de tous les ouvriers
                .GroupBy(segment => segment.Jour.Date); // Groupe par jour

            var resultatFinal = new List<LogPlanningJournalier>();

            // Étape 2: Pour chaque jour, grouper les segments par ouvrier.
            foreach (var groupeJour in segmentsParJour.OrderBy(g => g.Key))
            {
                var jour = groupeJour.Key;

                var ouvriersDuJour = groupeJour
                    .GroupBy(segment => segment.OuvrierId) // Dans un jour, groupe par ouvrier
                    .Select(groupeOuvrier =>
                    {
                        var ouvrierId = groupeOuvrier.Key;
                        var nomOuvrier = _ressourceService.GetOuvrierById(ouvrierId)?.NomComplet ?? ouvrierId;

                        // Crée la liste des affectations pour cet ouvrier, ce jour-là.
                        var affectations = groupeOuvrier
                            .Select(segment => new LogAffectationDuJour
                            {
                                TacheNom = segment.TacheNom,
                                BlocId = segment.BlocId,
                                DureeHeures = segment.HeuresTravaillees
                            })
                            .ToList();

                        return new LogOuvrierDuJour
                        {
                            NomOuvrier = nomOuvrier,
                            Affectations = affectations
                        };
                    })
                    .OrderBy(o => o.NomOuvrier)
                    .ToList();

                resultatFinal.Add(new LogPlanningJournalier
                {
                    Jour = jour,
                    Ouvriers = ouvriersDuJour
                });
            }

            return resultatFinal;
        }
        #endregion

        #region Logique Privée d'Agrégation

        /// <summary>
        /// Factorise la logique de construction d'un DTO TaskStatusInfo.
        /// </summary>
        private TaskStatusInfo _ConstruireTaskStatusInfo(Tache tache, IReadOnlyDictionary<string, PlanningTaskDetails> infosPlanning)
        {
            _taskStatuses.TryGetValue(tache.TacheId, out var statutDeBase); // Défaut est NonPlanifiee

            infosPlanning.TryGetValue(tache.TacheId, out var detailsPlanning);

            // Calculer le statut final (gestion de "EnRetard")
            var statutFinal = statutDeBase;
            if (statutDeBase != Status.Terminee && detailsPlanning?.DateFin < DateTime.Today)
            {
                statutFinal = Status.EnRetard;
            }

            // Obtenir les noms des ouvriers
            var nomsOuvriers = detailsPlanning?.OuvrierIds
                .Select(id => _ressourceService.GetOuvrierById(id)?.NomComplet)
                .Where(nom => !string.IsNullOrEmpty(nom))
                .Distinct()
                .ToList() ?? new List<string>();

            return new TaskStatusInfo
            {
                TacheId = tache.TacheId,
                TacheNom = tache.TacheNom,
                Statut = statutFinal,
                DateDebutPlanifiee = detailsPlanning?.DateDebut,
                DateFinPlanifiee = detailsPlanning?.DateFin,
                NomsOuvriersAssignes = nomsOuvriers!,
                EstTacheConteneur = detailsPlanning?.EstConteneur ?? false
            };
        }

        /// <summary>
        /// Optimisation: parcourt le planning une seule fois pour extraire toutes les informations 
        /// nécessaires sur les tâches (feuilles et conteneurs).
        /// </summary>
        private IReadOnlyDictionary<string, PlanningTaskDetails> _PreCalculerInfosPlanning(ConsolidatedPlanning? planning)
        {
            var resultat = new Dictionary<string, PlanningTaskDetails>();
            if (planning == null) return resultat;

            // 1. Agréger les infos des tâches feuilles (depuis les segments)
            var segmentsParTacheId = planning.SegmentsParOuvrierId
                .SelectMany(kvp => kvp.Value)
                .GroupBy(segment => segment.TacheId);

            foreach (var groupe in segmentsParTacheId)
            {
                var premierSegment = groupe.First();
                resultat[groupe.Key] = new PlanningTaskDetails(
                    DateDebut: groupe.Min(s => s.Jour),
                    DateFin: groupe.Max(s => s.Jour),
                    OuvrierIds: groupe.Select(s => s.OuvrierId).Distinct().ToList(),
                    EstConteneur: false,
                    ParentTacheId: premierSegment.ParentTacheId
                );
            }

            // 2. Identifier les tâches conteneurs et agréger les infos de leurs enfants
            var parents = resultat.Values
                .Where(details => !string.IsNullOrEmpty(details.ParentTacheId))
                .GroupBy(details => details.ParentTacheId!);

            foreach (var groupeParent in parents)
            {
                var parentId = groupeParent.Key;
                var enfantsDetails = groupeParent.ToList();

                resultat[parentId] = new PlanningTaskDetails(
                    DateDebut: enfantsDetails.Min(d => d.DateDebut),
                    DateFin: enfantsDetails.Max(d => d.DateFin),
                    OuvrierIds: enfantsDetails.SelectMany(d => d.OuvrierIds).Distinct().ToList(),
                    EstConteneur: true,
                    ParentTacheId: null // Un conteneur n'a pas de parent dans ce contexte
                );
            }

            return resultat;
        }

        #endregion
    }
}