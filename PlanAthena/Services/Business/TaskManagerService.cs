// Services/Business/TaskManagerService.cs V0.4.8

using PlanAthena.Data;
using PlanAthena.Interfaces;
using System.Data;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Agit comme la Source de Vérité unique pour tout ce qui concerne les tâches.
    /// Il gère leur structure (CRUD, hiérarchie) et leur état d'avancement (statut),
    /// y compris les informations agrégées issues du planning.
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

        public virtual void ChargerTaches(List<Tache> taches)
        {
            //ViderTaches();
            if (taches == null) return;
            foreach (var tache in taches)
            {
                _taches.TryAdd(tache.TacheId, tache);
            }
        }

        public virtual void ViderTaches()
        {
            _taches.Clear();
        }

        public virtual List<Tache> ObtenirToutesLesTachesPourSauvegarde()
        {
            return _taches.Values.ToList();
        }

        #endregion

        #region Intégration avec le Planificateur

        /// <summary>
        /// Met à jour la structure hiérarchique des tâches suite à une nouvelle planification.
        /// Cette méthode remplace toutes les anciennes sous-tâches par les nouvelles.
        /// Elle met également à jour les données planifiées (dates, affectations) sur toutes les tâches.
        /// </summary>
        public virtual void MettreAJourDecompositionTaches(List<Tache> tachesCalculees, Dictionary<string, string> parentIdParSousTacheId)
        {
            // --- 1. Nettoyage ---
            var anciennesSousTachesIds = _taches.Values.Where(t => !string.IsNullOrEmpty(t.ParentId)).Select(t => t.TacheId).ToList();
            foreach (var id in anciennesSousTachesIds)
            {
                _taches.Remove(id);
            }
            foreach (var tache in _taches.Values)
            {
                tache.EstConteneur = false; // Réinitialisation
            }

            // --- 2. Intégration des nouvelles sous-tâches ---
            foreach (var tacheCalc in tachesCalculees)
            {
                if (parentIdParSousTacheId.TryGetValue(tacheCalc.TacheId, out var parentId))
                {
                    tacheCalc.ParentId = parentId;
                    _taches.TryAdd(tacheCalc.TacheId, tacheCalc);
                }
            }

            // --- 3. Mise à jour des flags conteneurs ---
            var tousLesParentIds = parentIdParSousTacheId.Values.Distinct();
            foreach (var parentId in tousLesParentIds)
            {
                if (_taches.TryGetValue(parentId, out var tacheMere))
                {
                    tacheMere.EstConteneur = true;
                }
            }

            // --- 4. Mise à jour des données enrichies (Dates, Affectations) ---
            MettreAJourDonneesPlanifiees();
        }


        /// <summary>
        /// Met à jour uniquement le statut des tâches (Estimée, Planifiee, EnCours, EnRetard)
        /// en fonction de la date actuelle, sans modifier la structure ou les données planifiées.
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
                    else if (maintenant >= tache.DateDebutPlanifiee)
                        tache.Statut = Statut.EnCours;
                    else
                        tache.Statut = Statut.Planifiée;
                }
            }
        }

        #endregion

        #region CRUD Structurel

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
                Statut = Statut.Estimée, // Statut par défaut à la création
                MetierId = metierId ?? string.Empty
            };
            _taches.Add(nouvelleTache.TacheId, nouvelleTache);
            return nouvelleTache;
        }
        public virtual Tache CreerTacheJalon(string lotId, string blocId, string nom="Jalon", int heures=0)
        {
            var tacheMereExistantes = _taches.Values.Where(t => string.IsNullOrEmpty(t.ParentId)).ToList();
            var nouvelleTache = new Tache
            {
                LotId = lotId,
                BlocId = blocId,
                TacheId = _idGenerator.GenererProchainTacheId(blocId, tacheMereExistantes),
                TacheNom = nom,
                HeuresHommeEstimees = heures,
                Statut = Statut.Estimée, // Statut par défaut à la création
                Type = TypeActivite.JalonUtilisateur
            };
            _taches.Add(nouvelleTache.TacheId, nouvelleTache);
            return nouvelleTache;
        }

        public virtual void ModifierTache(Tache tacheModifiee)
        {
            if (tacheModifiee == null) throw new ArgumentNullException(nameof(tacheModifiee));
            if (!_taches.ContainsKey(tacheModifiee.TacheId))
                throw new KeyNotFoundException($"La tâche avec l'ID '{tacheModifiee.TacheId}' n'a pas été trouvée.");

            _taches[tacheModifiee.TacheId] = tacheModifiee;
        }

        public virtual void SupprimerTache(string tacheId)
        {
            if (!_taches.TryGetValue(tacheId, out var tacheASupprimer)) return;

            // --- NOUVELLE RÈGLE MÉTIER ---
            // Vérifier si cette tâche est une dépendance pour une autre.
            var tachesDependantes = _taches.Values
                .Where(t => (t.Dependencies ?? "").Split(',').Contains(tacheId))
                .ToList();

            if (tachesDependantes.Any())
            {
                var nomsTaches = string.Join(", ", tachesDependantes.Select(t => t.TacheNom));
                throw new InvalidConstraintException($"Impossible de supprimer la tâche '{tacheASupprimer.TacheNom}'. Elle est un prérequis pour : {nomsTaches}.");
            }
            // --- FIN DE LA RÈGLE MÉTIER ---

            // Logique de suppression récursive (pour les sous-tâches)
            var enfants = ObtenirTachesEnfants(tacheId);
            foreach (var enfant in enfants)
            {
                SupprimerTache(enfant.TacheId);
            }

            _taches.Remove(tacheId);
            _planningService.InvaliderTache(tacheId);

            // Mettre à jour le parent si c'était le dernier enfant
            if (!string.IsNullOrEmpty(tacheASupprimer.ParentId))
            {
                if (_taches.TryGetValue(tacheASupprimer.ParentId, out var parent))
                {
                    if (!ObtenirTachesEnfants(parent.TacheId).Any())
                    {
                        parent.EstConteneur = false;
                    }
                }
            }
        }

        #endregion

        #region Accesseurs

        public virtual Tache ObtenirTache(string tacheId) => _taches.GetValueOrDefault(tacheId);

        /// <summary>
        /// Récupère une liste de tâches, avec des filtres optionnels par lot et/ou par bloc.
        /// </summary>
        /// <param name="lotId">Si fourni, ne retourne que les tâches de ce lot.</param>
        /// <param name="blocId">Si fourni, ne retourne que les tâches de ce bloc.</param>
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
        /// NOUVEAU: Retourne la liste des tâches associées à un métier spécifique.
        /// Utilisé principalement pour les validations avant suppression.
        /// </summary>
        public virtual List<Tache> ObtenirTachesParMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId))
                return new List<Tache>();

            return _taches.Values.Where(t => t.MetierId == metierId).ToList();
        }

        public virtual List<Tache> ObtenirTachesEnfants(string parentId)
        {
            return _taches.Values.Where(t => t.ParentId == parentId).ToList();
        }

        #endregion

        #region Gestion de Statut Manuel

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
                            // Optionnel: lever une exception ou ignorer silencieusement.
                            // Pour l'instant, on ignore pour ne pas bloquer les opérations en masse.
                            continue;
                        }
                    }
                    tache.Statut = Statut.Terminée;
                    // Mettre à jour le parent si tous ses enfants sont maintenant terminés
                    if (!string.IsNullOrEmpty(tache.ParentId))
                    {
                        MettreAJourStatutConteneur(tache.ParentId);
                    }
                }
            }
        }

        #endregion

        #region Méthodes Privées

        private void MettreAJourDonneesPlanifiees()
        {
            var planningInfo = _planningService.ObtenirInfosPlanificationPourToutesLesTaches();

            foreach (var tache in _taches.Values.Where(t => !t.EstConteneur && string.IsNullOrEmpty(t.ParentId)))
            {
                // Tâches "feuilles" qui n'ont pas été découpées
                if (planningInfo.TryGetValue(tache.TacheId, out var info))
                {
                    tache.DateDebutPlanifiee = info.DateDebut;
                    tache.DateFinPlanifiee = info.DateFin;
                    tache.Affectations = info.Affectations;
                }
            }

            foreach (var conteneur in _taches.Values.Where(t => t.EstConteneur))
            {
                // Tâches "conteneurs"
                MettreAJourDonneesConteneur(conteneur.TacheId);
            }
        }

        private void MettreAJourDonneesConteneur(string conteneurId)
        {
            if (!_taches.TryGetValue(conteneurId, out var conteneur)) return;

            var enfants = ObtenirTachesEnfants(conteneurId);
            if (!enfants.Any()) return;

            // Recupérer les infos du planning pour chaque enfant
            var planningInfoEnfants = _planningService.ObtenirInfosPlanificationPourToutesLesTaches();

            var affectationsAgregees = new List<AffectationOuvrier>();
            DateTime? dateDebutMin = null;
            DateTime? dateFinMax = null;

            foreach (var enfant in enfants)
            {
                if (planningInfoEnfants.TryGetValue(enfant.TacheId, out var infoEnfant))
                {
                    if (!dateDebutMin.HasValue || infoEnfant.DateDebut < dateDebutMin)
                        dateDebutMin = infoEnfant.DateDebut;

                    if (!dateFinMax.HasValue || infoEnfant.DateFin > dateFinMax)
                        dateFinMax = infoEnfant.DateFin;

                    affectationsAgregees.AddRange(infoEnfant.Affectations);
                }
            }

            conteneur.DateDebutPlanifiee = dateDebutMin;
            conteneur.DateFinPlanifiee = dateFinMax;
            conteneur.Affectations = affectationsAgregees
                .GroupBy(a => a.OuvrierId)
                .Select(g => new AffectationOuvrier
                {
                    OuvrierId = g.Key,
                    NomOuvrier = g.First().NomOuvrier,
                    HeuresTravaillees = g.Sum(a => a.HeuresTravaillees)
                }).ToList();
        }

        private void MettreAJourStatutConteneur(string conteneurId)
        {
            if (!_taches.TryGetValue(conteneurId, out var conteneur) || conteneur.Statut == Statut.Terminée) return;

            var enfants = ObtenirTachesEnfants(conteneurId);
            if (!enfants.Any()) return;

            if (enfants.All(e => e.Statut == Statut.Terminée))
            {
                conteneur.Statut = Statut.Terminée;
            }
            // La logique d'agrégation des autres statuts (EnCours, EnRetard) est déjà gérée par SynchroniserStatutsTaches
            // qui se base sur les dates agrégées du conteneur. Cette méthode ne sert donc qu'à remonter le statut "Terminee".
        }

        #endregion
    }
}