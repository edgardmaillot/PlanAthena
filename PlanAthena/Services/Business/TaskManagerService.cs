// Emplacement: /Services/Business/TaskManagerService.cs
// Version: 0.4.9.1 (Hotfix + Refactoring Corrigé)

using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.TaskManager; // Ajout de la référence au DTO
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Agit comme la Source de Vérité unique pour tout ce qui concerne les tâches.
    /// Il gère leur structure (CRUD, hiérarchie) et leur état d'avancement (statut),
    /// y compris les informations agrégées issues du planning.
    /// Ce service est stateful et conserve l'état des tâches en mémoire.
    /// </summary>
    public class TaskManagerService
    {
        private readonly Dictionary<string, Tache> _taches = new();
        private readonly PlanningService _planningService;
        private readonly IIdGeneratorService _idGenerator;

        public TaskManagerService(PlanningService planningService, IIdGeneratorService idGenerator)
        {
            _planningService = planningService ?? throw new ArgumentNullException(nameof(planningService));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        #region Persistance & Cycle de Vie

        /// <summary>
        /// Charge une liste de tâches dans le service, en remplaçant les données existantes.
        /// </summary>
        /// <param name="taches">La liste des tâches à charger.</param>
        public virtual void ChargerTaches(List<Tache> taches)
        {
            ViderTaches();
            if (taches == null) return;
            foreach (var tache in taches)
            {
                _taches.TryAdd(tache.TacheId, tache);
            }
        }

        /// <summary>
        /// Supprime toutes les tâches gérées par le service.
        /// </summary>
        public virtual void ViderTaches()
        {
            _taches.Clear();
        }

        /// <summary>
        /// Retourne une copie de toutes les tâches actuellement gérées, typiquement pour la sauvegarde.
        /// </summary>
        /// <returns>Une liste de toutes les tâches.</returns>
        public virtual List<Tache> ObtenirToutesLesTachesPourSauvegarde()
        {
            return _taches.Values.ToList();
        }

        #endregion

        #region Intégration avec le Planificateur

        /// <summary>
        /// Point d'entrée unique pour la mise à jour complète de l'état des tâches après une planification réussie.
        /// Cette méthode est transactionnelle : elle nettoie l'état précédent (sous-tâches, jalons)
        /// uniquement pour les tâches replanifiées, intègre la nouvelle structure,
        /// et enrichit toutes les tâches avec les données calculées (dates, affectations, statut).
        /// </summary>
        /// <param name="planningService">Le service de planning contenant les résultats.</param>
        /// <param name="preparationResult">Le résultat de la préparation contenant la nouvelle structure des tâches.</param>
        public virtual void MettreAJourApresPlanification(PlanningService planningService, PreparationResult preparationResult)
        {
            var idsParentsReplanifies = preparationResult.ParentIdParSousTacheId.Values.Distinct().ToHashSet();

            NettoyerAnciennesDonneesDePlanification(idsParentsReplanifies);
            IntegrerNouvelleStructureDeTaches(preparationResult);

            // --- LOGIQUE DE MISE À JOUR RESTRUCTURÉE ---
            // 1. Mettre à jour les tâches feuilles (sous-tâches et tâches simples)
            var infosFeuilles = planningService.ObtenirInfosPlanificationPourTachesFeuilles();
            MettreAJourTachesFeuillesAvecPlanning(infosFeuilles);

            // 2. Mettre à jour les tâches mères (conteneurs) avec les données agrégées
            var infosMeres = planningService.ObtenirInfosPlanificationPourToutesLesTaches();
            MettreAJourTachesMeresAvecPlanning(infosMeres);

            // 3. Synchroniser les statuts (EnCours, EnRetard) en fonction de la date du jour
            SynchroniserStatutsTaches();
        }

        /// <summary>
        /// Met à jour uniquement le statut des tâches (Estimée, Planifiee, EnCours, EnRetard)
        /// en fonction de la date actuelle, sans modifier la structure ou les données planifiées.
        /// Cette méthode ne touche pas aux tâches déjà marquées comme 'Terminée'.
        /// </summary>
        public virtual void SynchroniserStatutsTaches()
        {
            var maintenant = DateTime.Now;

            foreach (var tache in _taches.Values)
            {
                if (tache.Statut == Statut.Terminée) continue;

                if (!tache.DateDebutPlanifiee.HasValue)
                {
                    tache.Statut = Statut.Estimée;
                }
                else
                {
                    if (maintenant > tache.DateFinPlanifiee)
                        tache.Statut = Statut.EnRetard;
                    else
                        tache.Statut = Statut.Planifiée;
                }
            }
        }

        #endregion

        #region CRUD Structurel

        /// <summary>
        /// Crée une nouvelle tâche standard.
        /// </summary>
        public virtual Tache CreerTache(string lotId, string blocId, string nom, int heures, string metierId = null)
        {
            var tacheMereExistantes = _taches.Values.Where(t => string.IsNullOrEmpty(t.ParentId)).ToList();
            var nouvelleTache = new Tache
            {
                LotId = lotId,
                BlocId = blocId,
                TacheId = _idGenerator.GenererProchainTacheId(blocId, tacheMereExistantes),
                TacheNom = nom,
                HeuresHommeEstimees = heures,
                Statut = Statut.Estimée,
                MetierId = metierId ?? string.Empty
            };
            _taches.Add(nouvelleTache.TacheId, nouvelleTache);
            return nouvelleTache;
        }

        /// <summary>
        /// Crée une nouvelle tâche de type Jalon Utilisateur.
        /// </summary>
        public virtual Tache CreerTacheJalon(string lotId, string blocId, string nom = "Jalon", int heures = 0)
        {
            var tacheMereExistantes = _taches.Values.Where(t => string.IsNullOrEmpty(t.ParentId)).ToList();
            var nouvelleTache = new Tache
            {
                LotId = lotId,
                BlocId = blocId,
                TacheId = _idGenerator.GenererProchainTacheId(blocId, tacheMereExistantes),
                TacheNom = nom,
                HeuresHommeEstimees = heures,
                Statut = Statut.Estimée,
                Type = TypeActivite.JalonUtilisateur
            };
            _taches.Add(nouvelleTache.TacheId, nouvelleTache);
            return nouvelleTache;
        }

        /// <summary>
        /// Met à jour une tâche existante avec les nouvelles données fournies.
        /// </summary>
        /// <param name="tacheModifiee">L'objet Tache contenant les modifications.</param>
        public virtual void ModifierTache(Tache tacheModifiee)
        {
            if (tacheModifiee == null) throw new ArgumentNullException(nameof(tacheModifiee));
            if (!_taches.ContainsKey(tacheModifiee.TacheId))
                throw new KeyNotFoundException($"La tâche avec l'ID '{tacheModifiee.TacheId}' n'a pas été trouvée.");

            _taches[tacheModifiee.TacheId] = tacheModifiee;
        }

        /// <summary>
        /// Supprime une tâche et toutes ses sous-tâches éventuelles.
        /// Lève une exception si la tâche est une dépendance pour une autre tâche.
        /// </summary>
        /// <param name="tacheId">L'ID de la tâche à supprimer.</param>
        public virtual void SupprimerTache(string tacheId)
        {
            if (!_taches.TryGetValue(tacheId, out var tacheASupprimer)) return;

            var tachesDependantes = _taches.Values
                .Where(t => (t.Dependencies ?? "").Split(',').Contains(tacheId))
                .ToList();

            if (tachesDependantes.Any())
            {
                var nomsTaches = string.Join(", ", tachesDependantes.Select(t => t.TacheNom));
                throw new InvalidConstraintException($"Impossible de supprimer la tâche '{tacheASupprimer.TacheNom}'. Elle est un prérequis pour : {nomsTaches}.");
            }

            var enfants = ObtenirTachesEnfants(tacheId);
            foreach (var enfant in enfants)
            {
                SupprimerTache(enfant.TacheId); // Appel récursif
            }

            _taches.Remove(tacheId);
            _planningService.InvaliderTache(tacheId);
        }

        #endregion

        #region Accesseurs

        /// <summary>
        /// Récupère une tâche par son ID.
        /// </summary>
        /// <returns>La Tache ou null si non trouvée.</returns>
        public virtual Tache ObtenirTache(string tacheId) => _taches.GetValueOrDefault(tacheId);

        /// <summary>
        /// Récupère une liste de tâches, avec des filtres optionnels par lot et/ou par bloc.
        /// </summary>
        /// <returns>Une liste de tâches correspondant aux critères.</returns>
        public virtual List<Tache> ObtenirToutesLesTaches(string? lotId = null, string? blocId = null)
        {
            IEnumerable<Tache> query = _taches.Values;

            if (!string.IsNullOrEmpty(lotId))
            {
                query = query.Where(t => t.LotId == lotId);
            }

            if (!string.IsNullOrEmpty(blocId))
            {
                query = query.Where(t => t.BlocId == blocId);
            }

            return query.ToList();
        }

        /// <summary>
        /// Retourne la liste des tâches associées à un métier spécifique.
        /// </summary>
        public virtual List<Tache> ObtenirTachesParMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId))
                return new List<Tache>();

            return _taches.Values.Where(t => t.MetierId == metierId).ToList();
        }

        /// <summary>
        /// Retourne la liste des sous-tâches directes d'une tâche mère.
        /// </summary>
        public virtual List<Tache> ObtenirTachesEnfants(string parentId)
        {
            return _taches.Values.Where(t => t.ParentId == parentId).ToList();
        }
        /// <summary>
        /// Calcule le nombre de tâches (non-conteneurs) dont la date de fin planifiée est passée.
        /// Utilisé pour le calcul du KPI "SPI Simplifié".
        /// </summary>
        /// <param name="dateReference">La date à laquelle comparer la date de fin planifiée.</param>
        /// <returns>Le nombre de tâches qui auraient dû être terminées.</returns>
        public virtual int ObtenirNombreTachesQuiDevraientEtreTerminees(DateTime dateReference)
        {
            return _taches.Values.Count(t =>
                !t.EstConteneur &&
                t.DateFinPlanifiee.HasValue &&
                t.DateFinPlanifiee.Value.Date <= dateReference.Date
            );
        }

        /// <summary>
        /// Calcule et retourne un résumé de l'état des tâches mères.
        /// Idéal pour obtenir des statistiques juste avant une sauvegarde.
        /// </summary>
        /// <returns>Un DTO contenant les comptes de tâches.</returns>
        public virtual TachesSummaryDto ObtenirResumeTaches()
        {
            var tachesMeres = _taches.Values.Where(t => string.IsNullOrEmpty(t.ParentId)).ToList();

            return new TachesSummaryDto
            {
                Total = tachesMeres.Count,
                Terminees = tachesMeres.Count(t => t.Statut == Statut.Terminée),
                EnRetard = tachesMeres.Count(t => t.Statut == Statut.EnRetard)
            };
        }
        public class TachesSummaryDto
        {
            public int Total { get; set; }
            public int Terminees { get; set; }
            public int EnRetard { get; set; }
        }
        #endregion

        #region Gestion de Statut Manuel

        /// <summary>
        /// Marque une ou plusieurs tâches comme 'Terminée'.
        /// Si une tâche est un conteneur, elle ne peut être marquée que si toutes ses sous-tâches sont déjà terminées.
        /// </summary>
        /// <param name="tacheIds">La liste des IDs des tâches à marquer comme terminées.</param>
        public virtual void MarquerTachesTerminees(List<string> tacheIds)
        {
            foreach (var id in tacheIds)
            {
                if (_taches.TryGetValue(id, out var tache))
                {
                    if (tache.EstConteneur)
                    {
                        var enfants = ObtenirTachesEnfants(id);
                        if (enfants.Any(e => e.Statut != Statut.Terminée))
                        {
                            continue; // On ne peut pas terminer un conteneur si ses enfants ne sont pas terminés.
                        }
                    }
                    tache.Statut = Statut.Terminée;

                    if (!string.IsNullOrEmpty(tache.ParentId))
                    {
                        MettreAJourStatutConteneur(tache.ParentId);
                    }
                }
            }
        }

        #endregion

        #region Méthodes Privées

        private void NettoyerAnciennesDonneesDePlanification(ISet<string> idsParentsReplanifies)
        {
            var idsASupprimer = _taches.Values
                .Where(t => (t.ParentId != null && idsParentsReplanifies.Contains(t.ParentId)) || t.Type == TypeActivite.JalonTechnique)
                .Select(t => t.TacheId)
                .ToList();

            foreach (var id in idsASupprimer)
            {
                _taches.Remove(id);
            }
        }

        private void IntegrerNouvelleStructureDeTaches(PreparationResult preparationResult)
        {
            var nouvellesSousTaches = preparationResult.TachesPreparees
                .Where(t => t.Type != TypeActivite.JalonTechnique &&
                            preparationResult.ParentIdParSousTacheId.ContainsKey(t.TacheId));

            foreach (var sousTache in nouvellesSousTaches)
            {
                sousTache.ParentId = preparationResult.ParentIdParSousTacheId[sousTache.TacheId];
                sousTache.Dependencies = string.Empty;
                // Pas besoin de nettoyer les dates/affectations ici, elles seront peuplées à l'étape suivante.
                _taches.TryAdd(sousTache.TacheId, sousTache);
            }
        }

        // NOUVELLE méthode privée pour gérer les taches feuilles
        private void MettreAJourTachesFeuillesAvecPlanning(IReadOnlyDictionary<string, PlanningInfoPourTache> infosFeuilles)
        {
            // On cible les sous-tâches nouvellement créées et les tâches simples.
            foreach (var tache in _taches.Values.Where(t => t.ParentId != null || !t.EstConteneur))
            {
                if (infosFeuilles.TryGetValue(tache.TacheId, out var info))
                {
                    tache.DateDebutPlanifiee = info.DateDebut;
                    tache.DateFinPlanifiee = info.DateFin;
                    tache.Affectations = info.Affectations;

                    // Une tâche feuille planifiée a le statut Planifiée (sera ajusté par SynchroniserStatutsTaches)
                    if (tache.Statut != Statut.Terminée)
                    {
                        tache.Statut = Statut.Planifiée;
                    }
                }
            }
        }


        private void MettreAJourTachesMeresAvecPlanning(IReadOnlyDictionary<string, PlanningInfoPourTache> infosPlanning)
        {
            foreach (var tache in _taches.Values.Where(t => t.ParentId == null)) // Uniquement les tâches racines
            {
                if (infosPlanning.TryGetValue(tache.TacheId, out var info))
                {
                    tache.EstConteneur = info.EstConteneur;

                    // On met à jour les dates/affectations agrégées sur le parent
                    tache.DateDebutPlanifiee = info.DateDebut;
                    tache.DateFinPlanifiee = info.DateFin;
                    tache.Affectations = info.Affectations;

                    if (tache.Statut != Statut.Terminée)
                    {
                        tache.Statut = Statut.Planifiée;
                    }
                }
                else if (tache.Statut != Statut.Terminée && !tache.EstConteneur)
                {
                    // Si une tâche simple n'est plus dans le planning, on la réinitialise.
                    tache.Statut = Statut.Estimée;
                    tache.DateDebutPlanifiee = null;
                    tache.DateFinPlanifiee = null;
                    tache.Affectations.Clear();
                }
            }
        }

        private void MettreAJourStatutConteneur(string conteneurId)
        {
            if (!_taches.TryGetValue(conteneurId, out var conteneur) || conteneur.Statut == Statut.Terminée) return;

            var enfants = ObtenirTachesEnfants(conteneurId);
            if (enfants.Any() && enfants.All(e => e.Statut == Statut.Terminée))
            {
                conteneur.Statut = Statut.Terminée;
            }
        }

        #endregion
    }
}