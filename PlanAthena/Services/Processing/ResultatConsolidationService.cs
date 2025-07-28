// Fichier: Services/Processing/ResultatConsolidationService.cs

using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service responsable de la consolidation des résultats de planification pour l'export Gantt.
    /// Regroupe les sous-tâches sous leurs tâches mères d'origine et inclut les jalons utilisateur.
    /// 
    /// FONCTIONNALITÉS :
    /// 1. Reconstitue la hiérarchie parent/enfant à partir de la table de mapping
    /// 2. Inclut les jalons utilisateur (séchage, attente) pour préserver les dépendances temporelles
    /// 3. Filtre les jalons techniques et ouvriers virtuels
    /// 4. Calcule les dépendances consolidées avec logique inversée pour GanttProject
    /// </summary>
    public class ResultatConsolidationService
    {
        /// <summary>
        /// Consolide les résultats de planification en regroupant les sous-tâches sous leurs tâches mères
        /// et en incluant les jalons utilisateur nécessaires.
        /// </summary>
        /// <param name="resultatBrut">Résultat brut du solveur avec les affectations détaillées</param>
        /// <param name="parentIdParSousTacheId">Table de mappage sous-tâche → tâche mère générée par PreparationSolveurService</param>
        /// <param name="tachesOriginales">Liste des tâches originales pour récupérer les noms et jalons</param>
        /// <param name="nomProjet">Nom du projet pour l'export</param>
        /// <returns>DTO consolidé pour l'export Gantt hiérarchique</returns>
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
                return ganttDto;
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

            if (!affectationsUtiles.Any())
            {
                return ganttDto;
            }

            // Regroupement des affectations par tâche parent
            var groupesAffectations = GrouperAffectationsParParent(affectationsUtiles, parentIdParSousTacheId);

            // Construire l'arborescence Gantt pour les tâches normales
            foreach (var groupe in groupesAffectations)
            {
                var tacheRacine = ConstruireTacheGantt(groupe, nomsTaskOriginales, parentIdParSousTacheId, tachesOriginales);
                ganttDto.TachesRacines.Add(tacheRacine);
            }

            // Ajouter les jalons utilisateur (J001, J002, J003, etc.)
            var jalonsUtilisateur = tachesOriginales
                .Where(t => t.Type == TypeActivite.JalonUtilisateur)
                .ToList();

            foreach (var jalon in jalonsUtilisateur)
            {
                var tacheJalon = ConstruireJalonGantt(jalon, resultatBrut);
                if (tacheJalon != null)
                {
                    ganttDto.TachesRacines.Add(tacheJalon);
                }
            }

            // Calculer les dépendances consolidées pour toutes les tâches
            CalculerDependancesConsolidees(ganttDto.TachesRacines, tachesOriginales, parentIdParSousTacheId);

            // Trier les tâches racines par date de début pour un affichage chronologique
            ganttDto.TachesRacines = ganttDto.TachesRacines.OrderBy(t => t.StartDate).ToList();

            return ganttDto;
        }

        /// <summary>
        /// Vérifie si une tâche est un jalon technique généré automatiquement (à filtrer du Gantt)
        /// </summary>
        /// <param name="tacheId">ID de la tâche à vérifier</param>
        /// <returns>True si c'est un jalon technique (JT_*)</returns>
        private static bool EstJalonTechnique(string tacheId)
        {
            return tacheId.StartsWith("JT_");
        }

        /// <summary>
        /// Vérifie si un ouvrier est virtuel (jalon technique)
        /// </summary>
        /// <param name="ouvrierNom">Nom de l'ouvrier à vérifier</param>
        /// <returns>True si c'est un ouvrier virtuel</returns>
        private static bool EstOuvrierVirtuel(string ouvrierNom)
        {
            return ouvrierNom.Contains("Jalon") ||
                   ouvrierNom.Contains("Ouvrier Virtuel") ||
                   ouvrierNom.Contains("Convergence technique");
        }

        /// <summary>
        /// Groupe les affectations par tâche parent en déduisant le parent depuis l'ID des sous-tâches.
        /// Utilise le pattern de nommage de PreparationSolveurService : T002_P1 → T002
        /// </summary>
        /// <param name="affectations">Liste des affectations à grouper</param>
        /// <param name="parentIdParSousTacheId">Table de mapping (non utilisée, on déduit depuis les IDs)</param>
        /// <returns>Dictionnaire groupé par ID de tâche parent</returns>
        private static Dictionary<string, List<AffectationDto>> GrouperAffectationsParParent(
            IEnumerable<AffectationDto> affectations,
            Dictionary<string, string> parentIdParSousTacheId)
        {
            var groupes = new Dictionary<string, List<AffectationDto>>();

            foreach (var affectation in affectations)
            {
                string cleGroupe = ObtenirParentIdDepuisId(affectation.TacheId);

                if (!groupes.ContainsKey(cleGroupe))
                {
                    groupes[cleGroupe] = new List<AffectationDto>();
                }

                groupes[cleGroupe].Add(affectation);
            }

            return groupes;
        }

        /// <summary>
        /// Déduit l'ID parent depuis l'ID de la sous-tâche.
        /// Utilise le pattern de nommage de PreparationSolveurService : T002_P1 → T002
        /// </summary>
        /// <param name="tacheId">ID de la tâche (sous-tâche ou tâche normale)</param>
        /// <returns>ID de la tâche parent</returns>
        private static string ObtenirParentIdDepuisId(string tacheId)
        {
            // Si c'est une sous-tâche (contient "_P"), extraire le parent
            if (tacheId.Contains("_P"))
            {
                var index = tacheId.IndexOf("_P");
                var parentId = tacheId.Substring(0, index); // T002_P1 → T002
                return parentId;
            }

            // Sinon, c'est déjà la tâche parent (ou une tâche simple)
            return tacheId;
        }

        /// <summary>
        /// Construit un GanttTaskItem à partir d'un groupe d'affectations.
        /// Détecte correctement les sous-tâches créées par PreparationSolveurService.
        /// </summary>
        /// <param name="groupe">Groupe d'affectations pour une même tâche parent</param>
        /// <param name="nomsTaskOriginales">Dictionnaire des noms de tâches originales</param>
        /// <param name="parentIdParSousTacheId">Table de mapping pour les dépendances</param>
        /// <param name="tachesOriginales">Liste des tâches originales</param>
        /// <returns>GanttTaskItem avec hiérarchie parent/enfant si applicable</returns>
        private static GanttTaskItem ConstruireTacheGantt(
            KeyValuePair<string, List<AffectationDto>> groupe,
            Dictionary<string, string> nomsTaskOriginales,
            Dictionary<string, string> parentIdParSousTacheId,
            IReadOnlyList<Tache> tachesOriginales)
        {
            var parentId = groupe.Key; // Ex: "T002"
            var affectations = groupe.Value.OrderBy(a => a.DateDebut).ToList();

            // Déterminer le nom de la tâche mère (nom original)
            string nomTache;
            if (nomsTaskOriginales.TryGetValue(parentId, out var nomOriginal))
            {
                nomTache = nomOriginal; // Ex: "Maconnerie" (pas "Maconnerie (Partie 1)")
            }
            else
            {
                // Fallback : utiliser le nom de la première affectation et le nettoyer
                nomTache = affectations.First().TacheNom;
                if (nomTache.Contains("(Partie"))
                {
                    var index = nomTache.IndexOf("(Partie");
                    nomTache = nomTache.Substring(0, index).Trim();
                }
            }

            // Calculer les dates et durées globales de la tâche mère
            var dateDebut = affectations.Min(a => a.DateDebut);
            var dateFin = affectations.Max(a => a.DateDebut.AddHours(a.DureeHeures));
            var dureeTotal = affectations.Sum(a => (double)a.DureeHeures);

            // Récupérer les informations de bloc/lot
            var premiereAffectation = affectations.First();
            var blocId = premiereAffectation.BlocId ?? "";
            var lotId = "";

            // Récupérer le LotId depuis la tâche originale
            var tacheOriginale = tachesOriginales.FirstOrDefault(t => t.TacheId == parentId);
            if (tacheOriginale != null)
            {
                lotId = tacheOriginale.LotId ?? "";
            }

            // Déterminer les ressources assignées (pour affichage dans le parent)
            var ressources = affectations.Select(a => a.OuvrierNom).Distinct().ToList();
            var ressourceString = string.Join(", ", ressources);

            var tacheGantt = new GanttTaskItem
            {
                Id = parentId, // Ex: "T002"
                Name = nomTache, // Ex: "Maconnerie" 
                StartDate = dateDebut,
                EndDate = dateFin,
                DurationHours = dureeTotal,
                AssignedResourceName = ressourceString, // Sera vidé si des enfants sont créés
                BlocId = blocId,
                LotId = lotId
            };

            // Détecter les sous-tâches par TacheId différents
            var affectationsParSousTache = affectations.GroupBy(a => a.TacheId).ToList();

            if (affectationsParSousTache.Count > 1)
            {
                // Cas : Tâche découpée par PreparationSolveurService
                // Créer les enfants pour chaque sous-tâche
                foreach (var groupeSousTache in affectationsParSousTache.OrderBy(g => g.Min(a => a.DateDebut)))
                {
                    var affectationsSousTache = groupeSousTache.ToList();
                    var sousTacheId = groupeSousTache.Key; // Ex: "T002_P1"
                    var nomSousTache = affectationsSousTache.First().TacheNom; // Ex: "Maconnerie (Partie 1)"

                    var dateDebutSousTache = affectationsSousTache.Min(a => a.DateDebut);
                    var dateFinSousTache = affectationsSousTache.Max(a => a.DateDebut.AddHours(a.DureeHeures));
                    var dureeSousTache = affectationsSousTache.Sum(a => (double)a.DureeHeures);

                    var ressourcesSousTache = affectationsSousTache.Select(a => a.OuvrierNom).Distinct();
                    var ressourceSousTacheString = string.Join(", ", ressourcesSousTache);

                    var enfant = new GanttTaskItem
                    {
                        Id = sousTacheId, // Ex: "T002_P1"
                        Name = nomSousTache, // Ex: "Maconnerie (Partie 1)"
                        StartDate = dateDebutSousTache,
                        EndDate = dateFinSousTache,
                        DurationHours = dureeSousTache,
                        AssignedResourceName = ressourceSousTacheString,
                        BlocId = blocId,
                        LotId = lotId
                    };

                    tacheGantt.Children.Add(enfant);
                }

                // La tâche mère devient un conteneur (pas d'assignation directe)
                tacheGantt.AssignedResourceName = "";
            }

            return tacheGantt;
        }

        /// <summary>
        /// Construit un GanttTaskItem pour un jalon utilisateur.
        /// Les jalons utilisateur (J001, J002, J003) représentent des temps d'attente/séchage
        /// et sont essentiels pour préserver les dépendances temporelles dans GanttProject.
        /// 
        /// CORRECTION : Utilise les dates précises des affectations pour calculer les dates des jalons.
        /// </summary>
        /// <param name="jalon">Jalon utilisateur à convertir</param>
        /// <param name="resultatBrut">Résultats du solveur pour calculer les dates</param>
        /// <returns>GanttTaskItem représentant le jalon, ou null si impossible à calculer</returns>
        private static GanttTaskItem? ConstruireJalonGantt(Tache jalon, ProcessChantierResultDto resultatBrut)
        {
            DateTime dateJalon;

            // Chercher directement les affectations du jalon dans les résultats
            var affectationJalon = resultatBrut.OptimisationResultat.Affectations
                .FirstOrDefault(a => a.TacheId == jalon.TacheId);

            if (affectationJalon != null)
            {
                // Utiliser directement la date de l'affectation du jalon
                dateJalon = affectationJalon.DateDebut;
            }
            else if (!string.IsNullOrEmpty(jalon.Dependencies))
            {
                // Fallback : calculer depuis les dépendances
                var dependances = jalon.Dependencies.Split(',').Select(d => d.Trim()).ToList();
                var affectationsDeps = resultatBrut.OptimisationResultat.Affectations
                    .Where(a => dependances.Any(dep => a.TacheId.StartsWith(dep)))
                    .ToList();

                if (affectationsDeps.Any())
                {
                    // Date de début du jalon = fin de la dernière dépendance
                    dateJalon = affectationsDeps.Max(a => a.DateDebut.AddHours(a.DureeHeures));
                }
                else
                {
                    // Fallback si aucune affectation trouvée
                    dateJalon = DateTime.Today.AddDays(1);
                }
            }
            else
            {
                // Jalon sans dépendances (rare)
                dateJalon = DateTime.Today.AddDays(1);
            }

            return new GanttTaskItem
            {
                Id = jalon.TacheId, // Ex: "J001"
                Name = jalon.TacheNom, // Ex: "Sechage platre"
                StartDate = dateJalon,
                EndDate = dateJalon.AddHours(jalon.HeuresHommeEstimees), // Durée d'attente (24h, 72h, 12h)
                DurationHours = jalon.HeuresHommeEstimees,
                AssignedResourceName = "", // Pas de ressource pour les jalons d'attente
                BlocId = jalon.BlocId ?? "",
                LotId = jalon.LotId ?? ""
            };
        }

        /// <summary>
        /// Calcule les dépendances consolidées pour toutes les tâches Gantt.
        /// LOGIQUE INVERSÉE pour GanttProject : Si T002 dépend de T001, alors T001.Dependencies contient [T002]
        /// </summary>
        /// <param name="tachesRacines">Liste des tâches racines à traiter</param>
        /// <param name="tachesOriginales">Liste des tâches originales avec leurs dépendances</param>
        /// <param name="parentIdParSousTacheId">Table de mapping pour remonter aux parents</param>
        private static void CalculerDependancesConsolidees(
            List<GanttTaskItem> tachesRacines,
            IReadOnlyList<Tache> tachesOriginales,
            Dictionary<string, string> parentIdParSousTacheId)
        {
            // Créer un dictionnaire de toutes les tâches Gantt par ID pour recherche rapide
            var toutesLesTachesGantt = new Dictionary<string, GanttTaskItem>();

            foreach (var tacheRacine in tachesRacines)
            {
                toutesLesTachesGantt[tacheRacine.Id] = tacheRacine;

                // Ajouter aussi les enfants
                foreach (var enfant in tacheRacine.Children)
                {
                    toutesLesTachesGantt[enfant.Id] = enfant;
                }
            }

            // LOGIQUE INVERSÉE : Pour chaque tâche originale, ajouter ses successeurs à ses dépendances
            foreach (var tacheOriginale in tachesOriginales)
            {
                if (string.IsNullOrEmpty(tacheOriginale.Dependencies))
                    continue;

                // Parser les dépendances de la tâche originale
                var dependances = tacheOriginale.Dependencies
                    .Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToList();

                // INVERSION : Pour chaque dépendance, ajouter la tâche courante comme successeur
                foreach (var depId in dependances)
                {
                    // Résoudre l'ID de la dépendance (peut être remontée au parent)
                    string depIdResolu = depId;
                    if (!toutesLesTachesGantt.ContainsKey(depId) &&
                        parentIdParSousTacheId.TryGetValue(depId, out var parentDep) &&
                        toutesLesTachesGantt.ContainsKey(parentDep))
                    {
                        depIdResolu = parentDep;
                    }

                    // Trouver la tâche Gantt de la dépendance
                    if (toutesLesTachesGantt.TryGetValue(depIdResolu, out var tacheDepGantt))
                    {
                        // INVERSION CRITIQUE : Ajouter la tâche courante aux dépendances de sa dépendance !
                        if (!tacheDepGantt.Dependencies.Contains(tacheOriginale.TacheId))
                        {
                            tacheDepGantt.Dependencies.Add(tacheOriginale.TacheId);
                        }
                    }
                }
            }
        }
    }
}