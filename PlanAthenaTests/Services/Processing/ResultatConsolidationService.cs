// Fichier: Services/Processing/ResultatConsolidationService.cs

using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service responsable de la consolidation des résultats de planification pour l'export Gantt.
    /// Regroupe les sous-tâches sous leurs tâches mères d'origine.
    /// </summary>
    public class ResultatConsolidationService
    {
        /// <summary>
        /// Consolide les résultats de planification en regroupant les sous-tâches sous leurs tâches mères
        /// </summary>
        /// <param name="resultatBrut">Résultat brut du solveur</param>
        /// <param name="parentIdParSousTacheId">Table de mappage sous-tâche -> tâche mère</param>
        /// <param name="tachesOriginales">Liste des tâches originales pour récupérer les noms</param>
        /// <param name="nomProjet">Nom du projet</param>
        /// <returns>DTO consolidé pour l'export Gantt</returns>
        public ConsolidatedGanttDto ConsoliderPourGantt(
            ProcessChantierResultDto resultatBrut,
            Dictionary<string, string> parentIdParSousTacheId,
            IReadOnlyList<Tache> tachesOriginales,
            string nomProjet)
        {
            var ganttDto = new ConsolidatedGanttDto
            {
                NomProjet = !string.IsNullOrEmpty(nomProjet) ? nomProjet : "Planning PlanAthena",
                DateGeneration = DateTime.Now
            };

            // Validation des entrées
            if (resultatBrut?.OptimisationResultat?.Affectations == null || !resultatBrut.OptimisationResultat.Affectations.Any())
            {
                return ganttDto; // Retourne un DTO vide
            }

            if (parentIdParSousTacheId == null)
            {
                parentIdParSousTacheId = new Dictionary<string, string>();
            }

            if (tachesOriginales == null)
            {
                tachesOriginales = new List<Tache>();
            }

            // Créer un dictionnaire nom des tâches originales pour récupération rapide
            var nomsTaskOriginales = tachesOriginales.ToDictionary(t => t.TacheId, t => t.TacheNom);

            // Filtrer les jalons techniques ET les ouvriers virtuels des affectations
            var affectationsUtiles = resultatBrut.OptimisationResultat.Affectations
                .Where(a => !EstJalonTechnique(a.TacheId) && !EstOuvrierVirtuel(a.OuvrierNom))
                .ToList();

            // Grouper les affectations par tâche parent ou tâche originale
            var groupesAffectations = GrouperAffectationsParParent(affectationsUtiles, parentIdParSousTacheId);

            // Construire l'arborescence Gantt
            foreach (var groupe in groupesAffectations)
            {
                var tacheRacine = ConstruireTacheGantt(groupe, nomsTaskOriginales, parentIdParSousTacheId, tachesOriginales);
                ganttDto.TachesRacines.Add(tacheRacine);
            }

            // Trier les tâches racines par date de début
            ganttDto.TachesRacines = ganttDto.TachesRacines.OrderBy(t => t.StartDate).ToList();

            return ganttDto;
        }

        /// <summary>
        /// Vérifie si une tâche est un jalon technique (à filtrer du Gantt)
        /// </summary>
        private static bool EstJalonTechnique(string tacheId)
        {
            return tacheId.StartsWith("JT_");
        }

        /// <summary>
        /// Vérifie si un ouvrier est virtuel (jalon technique)
        /// </summary>
        private static bool EstOuvrierVirtuel(string ouvrierNom)
        {
            return ouvrierNom.Contains("Jalon") || ouvrierNom.Contains("Ouvrier Virtuel") || ouvrierNom.Contains("Convergence technique");
        }

        /// <summary>
        /// Groupe les affectations par tâche parent (ou par tâche elle-même si pas de parent)
        /// </summary>
        private static Dictionary<string, List<AffectationDto>> GrouperAffectationsParParent(
            IEnumerable<AffectationDto> affectations,
            Dictionary<string, string> parentIdParSousTacheId)
        {
            var groupes = new Dictionary<string, List<AffectationDto>>();

            foreach (var affectation in affectations)
            {
                string cleGroupe;

                // Si la tâche a un parent, on groupe par le parent
                if (parentIdParSousTacheId.TryGetValue(affectation.TacheId, out var parentId))
                {
                    cleGroupe = parentId;
                }
                else
                {
                    // Sinon, on groupe par la tâche elle-même
                    cleGroupe = affectation.TacheId;
                }

                if (!groupes.ContainsKey(cleGroupe))
                {
                    groupes[cleGroupe] = new List<AffectationDto>();
                }

                groupes[cleGroupe].Add(affectation);
            }

            return groupes;
        }

        /// <summary>
        /// Construit un GanttTaskItem à partir d'un groupe d'affectations
        /// </summary>
        private static GanttTaskItem ConstruireTacheGantt(
            KeyValuePair<string, List<AffectationDto>> groupe,
            Dictionary<string, string> nomsTaskOriginales,
            Dictionary<string, string> parentIdParSousTacheId,
            IReadOnlyList<Tache> tachesOriginales)
        {
            var parentId = groupe.Key;
            var affectations = groupe.Value.OrderBy(a => a.DateDebut).ToList();

            // Déterminer le nom de la tâche
            string nomTache;
            if (nomsTaskOriginales.TryGetValue(parentId, out var nomOriginal))
            {
                nomTache = nomOriginal;
            }
            else
            {
                // Fallback : utiliser le nom de la première affectation
                nomTache = affectations.First().TacheNom;
                // Si c'est une sous-tâche, nettoyer le nom
                if (nomTache.Contains("(Partie"))
                {
                    var index = nomTache.IndexOf("(Partie");
                    nomTache = nomTache.Substring(0, index).Trim();
                }
            }

            // Calculer les dates et durées
            var dateDebut = affectations.Min(a => a.DateDebut);
            var dateFin = affectations.Max(a => a.DateDebut.AddHours(a.DureeHeures));
            var dureeTotal = affectations.Sum(a => (double)a.DureeHeures); // Cast vers double

            // Récupérer les informations de bloc/lot
            var premiereAffectation = affectations.First();
            var blocId = premiereAffectation.BlocId ?? "";
            var lotId = ""; // Non disponible dans AffectationDto, on pourrait l'ajouter si besoin

            // Si on trouve la tâche originale, récupérer son LotId
            var tacheOriginale = tachesOriginales.FirstOrDefault(t => t.TacheId == parentId);
            if (tacheOriginale != null)
            {
                lotId = tacheOriginale.LotId ?? "";
            }

            // Déterminer les ressources assignées
            var ressources = affectations.Select(a => a.OuvrierNom).Distinct().ToList();
            var ressourceString = string.Join(", ", ressources);

            var tacheGantt = new GanttTaskItem
            {
                Id = parentId,
                Name = nomTache,
                StartDate = dateDebut,
                EndDate = dateFin,
                DurationHours = dureeTotal,
                AssignedResourceName = ressourceString,
                BlocId = blocId,
                LotId = lotId
            };

            // Si c'est une tâche découpée (plusieurs affectations pour différentes sous-tâches)
            var affectationsParSousTache = affectations.GroupBy(a => a.TacheId).ToList();
            if (affectationsParSousTache.Count > 1)
            {
                // Créer les enfants
                foreach (var groupeSousTache in affectationsParSousTache.OrderBy(g => g.Min(a => a.DateDebut)))
                {
                    var affectationsSousTache = groupeSousTache.ToList();
                    var sousTacheId = groupeSousTache.Key;
                    var nomSousTache = affectationsSousTache.First().TacheNom;

                    var dateDebutSousTache = affectationsSousTache.Min(a => a.DateDebut);
                    var dateFinSousTache = affectationsSousTache.Max(a => a.DateDebut.AddHours(a.DureeHeures));
                    var dureeSousTache = affectationsSousTache.Sum(a => (double)a.DureeHeures); // Cast vers double

                    var ressourcesSousTache = affectationsSousTache.Select(a => a.OuvrierNom).Distinct();
                    var ressourceSousTacheString = string.Join(", ", ressourcesSousTache);

                    var enfant = new GanttTaskItem
                    {
                        Id = sousTacheId,
                        Name = nomSousTache,
                        StartDate = dateDebutSousTache,
                        EndDate = dateFinSousTache,
                        DurationHours = dureeSousTache,
                        AssignedResourceName = ressourceSousTacheString,
                        BlocId = blocId,
                        LotId = lotId
                    };

                    tacheGantt.Children.Add(enfant);
                }
            }

            // TODO: Calculer les dépendances à partir des tâches originales
            // Ce sera fait lors de l'amélioration de l'export XML
            CalculerDependances(tacheGantt, tachesOriginales, parentIdParSousTacheId);

            return tacheGantt;
        }

        /// <summary>
        /// Calcule les dépendances d'une tâche Gantt à partir des tâches originales
        /// </summary>
        private static void CalculerDependances(
            GanttTaskItem tacheGantt,
            IReadOnlyList<Tache> tachesOriginales,
            Dictionary<string, string> parentIdParSousTacheId)
        {
            var tacheOriginale = tachesOriginales.FirstOrDefault(t => t.TacheId == tacheGantt.Id);
            if (tacheOriginale == null || string.IsNullOrEmpty(tacheOriginale.Dependencies))
            {
                return;
            }

            var dependances = tacheOriginale.Dependencies
                .Split(',')
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrEmpty(d))
                .ToList();

            // Mapper les dépendances vers les tâches parentes si nécessaire
            var dependancesFinales = new HashSet<string>();
            foreach (var dep in dependances)
            {
                // Si la dépendance est une sous-tâche, remonter au parent
                if (parentIdParSousTacheId.TryGetValue(dep, out var parentDep))
                {
                    dependancesFinales.Add(parentDep);
                }
                else
                {
                    // Vérifier que la dépendance existe dans les tâches originales
                    var depExiste = tachesOriginales.Any(t => t.TacheId == dep);
                    if (depExiste)
                    {
                        dependancesFinales.Add(dep);
                    }
                }
            }

            tacheGantt.Dependencies = dependancesFinales.ToList();
        }
    }
}