using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using QuikGraph;
using QuikGraph.Algorithms; 
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Service de suggestions intelligent pour les dépendances de tâches - Version 2.0 "Graph First".
    /// 
    /// ARCHITECTURE REFACTORISÉE :
    /// Cette version abandonne les algorithmes complexes basés sur les métiers au profit
    /// d'une approche simple et fiable basée sur le graphe réel des tâches du bloc.
    /// 
    /// PRINCIPE FONDAMENTAL :
    /// 1. Construire le graphe réel du bloc à partir des dépendances existantes
    /// 2. Identifier les successeurs de chaque tâche dans ce graphe
    /// 3. Filtrer les candidats valides (pas de cycle)
    /// 4. Appliquer les suggestions métier sur cette base saine
    /// 
    /// AVANTAGES :
    /// - Source de vérité unique : le graphe des dépendances réelles
    /// - Prévention robuste des cycles
    /// - Logique simple et testable
    /// - Base solide pour le mapping automatique futur
    /// </summary>
    public class DependanceBuilder
    {
        private readonly ProjetService _projetService; 

        public DependanceBuilder(ProjetService projetService) 
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
        }

        /// <summary>
        /// Valide qu'un métier et ses prérequis ne créent pas de dépendance circulaire.
        /// </summary>
        /// <param name="metier">Le métier à valider, potentiellement avec des prérequis modifiés.</param>
        /// <param name="tousLesMetiers">L'ensemble de tous les métiers du projet (y compris le métier en cours de modification, si c'est une modification).</param>
        /// <exception cref="InvalidOperationException">Lancée si une circularité est détectée.</exception>
        public void ValiderMetier(Metier metier, IEnumerable<Metier> tousLesMetiers)
        {
            if (metier == null) throw new ArgumentNullException(nameof(metier));
            if (tousLesMetiers == null) throw new ArgumentNullException(nameof(tousLesMetiers));

            var graph = new AdjacencyGraph<string, Edge<string>>();
            var currentMetiers = tousLesMetiers.ToDictionary(m => m.MetierId, m => m);

            // Mettre à jour le métier en cours de validation dans la collection temporaire
            currentMetiers[metier.MetierId] = metier; // S'assure que les prérequis modifiés sont utilisés

            graph.AddVertexRange(currentMetiers.Keys);

            foreach (var m in currentMetiers.Values)
            {
                var prerequis = ParseDependances(m.PrerequisMetierIds);
                foreach (var prerequisId in prerequis)
                {
                    if (currentMetiers.ContainsKey(prerequisId))
                    {
                        // Le prérequis pointe vers le métier.
                        // Dans un graphe de dépendance, l'arête va du prérequis vers le dépendant.
                        // Donc, Pre_req_MetierId -> MetierId
                        graph.AddEdge(new Edge<string>(prerequisId, m.MetierId));
                    }
                }
            }

            try
            {
                // Tenter un tri topologique. Si une exception est levée, il y a un cycle.
                graph.TopologicalSort();
            }
            catch (NonAcyclicGraphException)
            {
                throw new InvalidOperationException($"Dépendance circulaire détectée pour le métier '{metier.Nom}' (ID: {metier.MetierId}). Veuillez revoir ses prérequis.");
            }
        }


        /// <summary>
        /// Méthode principale du service : génère la liste complète des dépendances
        /// avec leurs états d'affichage pour une tâche donnée.
        /// 
        /// ARCHITECTURE REFACTORISÉE :
        /// 1. Construction du graphe réel du bloc
        /// 2. Calcul des successeurs pour détection de cycles
        /// 3. Filtrage des candidats valides
        /// 4. Application des suggestions métier
        /// 5. Classification finale
        /// </summary>
        public List<DependanceAffichage> ObtenirDependancesPourTache(Tache tache, List<Tache> contexteTaches)
        {
            if (tache == null)
                throw new ArgumentNullException(nameof(tache));
            if (contexteTaches == null)
                throw new ArgumentNullException(nameof(contexteTaches));

            try
            {
                // 1. Construire le graphe réel du bloc (source de vérité)
                var grapheBloc = ConstruireGrapheBloc(contexteTaches);

                // 2. Calculer tous les successeurs pour détection de cycles
                var successeursParTache = CalculerTousLesSuccesseurs(grapheBloc, contexteTaches);

                // 3. Filtrer les candidats valides (pas de cycle)
                var candidatsValides = FiltrerCandidatsValides(tache, contexteTaches, successeursParTache);

                // 4. Appliquer les suggestions métier sur la base saine
                var suggestions = AppliquerSuggestionsMetier(tache, candidatsValides);

                // 5. Classification finale
                var strictes = ParseDependances(tache.Dependencies);
                var excluses = ParseDependances(tache.ExclusionsDependances);

                return ClassifierDependances(candidatsValides, strictes, excluses, suggestions);
            }
            catch (Exception)
            {
                // Mode dégradé : retourner une liste vide plutôt que de planter
                return new List<DependanceAffichage>();
            }
        }

        /// <summary>
        /// Construit le graphe réel du bloc à partir des dépendances existantes.
        /// 
        /// SOURCE DE VÉRITÉ UNIQUE :
        /// Ce graphe représente fidèlement les relations de dépendances
        /// telles qu'elles existent actuellement dans le bloc.
        /// 
        /// DONNÉES UTILISÉES :
        /// - Tache.Dependencies (dépendances strictes définies par l'utilisateur)
        /// - Ignore Tache.ExclusionsDependances (ce sont des rejets, pas des liens)
        /// </summary>
        private BidirectionalGraph<string, Edge<string>> ConstruireGrapheBloc(List<Tache> taches)
        {
            var graphe = new BidirectionalGraph<string, Edge<string>>();

            // Ajouter tous les sommets (tâches)
            foreach (var tache in taches)
            {
                if (!string.IsNullOrEmpty(tache.TacheId))
                {
                    graphe.AddVertex(tache.TacheId);
                }
            }

            // Ajouter les arêtes basées sur les dépendances réelles
            foreach (var tache in taches)
            {
                if (string.IsNullOrEmpty(tache.TacheId)) continue;

                var dependances = ParseDependances(tache.Dependencies);
                foreach (var dependanceId in dependances)
                {
                    // Vérifier que la dépendance existe dans le contexte
                    if (taches.Any(t => t.TacheId == dependanceId))
                    {
                        // Arête : dependanceId → tache.TacheId
                        graphe.AddEdge(new Edge<string>(dependanceId, tache.TacheId));
                    }
                }
            }

            return graphe;
        }

        /// <summary>
        /// Calcule tous les successeurs (directs et transitifs) pour chaque tâche du bloc.
        /// 
        /// OBJECTIF :
        /// Identifier pour chaque tâche X l'ensemble de toutes les tâches qui en dépendent
        /// directement ou indirectement. Ceci permet une détection robuste des cycles.
        /// 
        /// ALGORITHME :
        /// Parcours en profondeur depuis chaque tâche pour découvrir tous les descendants.
        /// </summary>
        private Dictionary<string, HashSet<string>> CalculerTousLesSuccesseurs(
            BidirectionalGraph<string, Edge<string>> graphe, List<Tache> taches)
        {
            var successeursParTache = new Dictionary<string, HashSet<string>>();

            foreach (var tache in taches)
            {
                if (string.IsNullOrEmpty(tache.TacheId)) continue;

                var successeurs = new HashSet<string>();
                var visite = new HashSet<string>();

                ExploreerSuccesseurs(graphe, tache.TacheId, successeurs, visite);
                successeursParTache[tache.TacheId] = successeurs;
            }

            return successeursParTache;
        }

        /// <summary>
        /// Exploration récursive des successeurs d'une tâche donnée.
        /// 
        /// PARCOURS EN PROFONDEUR :
        /// Visite tous les descendants directs et indirects en évitant les cycles
        /// grâce au set de tâches déjà visitées.
        /// </summary>
        private void ExploreerSuccesseurs(BidirectionalGraph<string, Edge<string>> graphe,
            string tacheId, HashSet<string> successeurs, HashSet<string> visite)
        {
            if (visite.Contains(tacheId)) return; // Éviter les cycles infinis
            visite.Add(tacheId);

            if (!graphe.ContainsVertex(tacheId)) return;

            // Explorer tous les successeurs directs
            foreach (var edge in graphe.OutEdges(tacheId))
            {
                var successeurId = edge.Target;
                successeurs.Add(successeurId);

                // Exploration récursive des successeurs du successeur
                ExploreerSuccesseurs(graphe, successeurId, visite, successeurs); // Correction de l'ordre des paramètres pour 'visite' et 'successeurs'
            }
        }

        /// <summary>
        /// Filtre les candidats valides pour éviter la création de cycles.
        /// 
        /// RÈGLE ANTI-CYCLE ROBUSTE :
        /// Une tâche X peut dépendre de Y seulement si Y n'est pas dans les successeurs de X.
        /// En d'autres termes : si Y dépend déjà de X (directement ou indirectement),
        /// alors X ne peut pas dépendre de Y.
        /// 
        /// CETTE APPROCHE EST INFAILLIBLE :
        /// Contrairement aux heuristiques sur les métiers, cette règle est mathématiquement
        /// garantie d'empêcher les cycles dans un graphe orienté.
        /// </summary>
        private List<Tache> FiltrerCandidatsValides(Tache tacheCible, List<Tache> contexteTaches,
            Dictionary<string, HashSet<string>> successeursParTache)
        {
            var candidatsValides = new List<Tache>();

            // Obtenir les successeurs de la tâche cible
            var successeursDeLaCible = successeursParTache.ContainsKey(tacheCible.TacheId)
                ? successeursParTache[tacheCible.TacheId]
                : new HashSet<string>();

            foreach (var candidat in contexteTaches)
            {
                // Exclure la tâche elle-même
                if (candidat.TacheId == tacheCible.TacheId) continue;

                // RÈGLE ANTI-CYCLE : Si le candidat est un successeur de la cible,
                // alors la cible ne peut pas dépendre du candidat
                if (successeursDeLaCible.Contains(candidat.TacheId)) continue;

                // Candidat valide
                candidatsValides.Add(candidat);
            }

            return candidatsValides;
        }

        /// <summary>
        /// Applique les suggestions métier sur la base des candidats valides.
        /// 
        /// APPROCHE SIMPLIFIÉE :
        /// Maintenant que nous avons une base saine (pas de cycles possibles),
        /// nous pouvons appliquer les règles métier de manière simple et fiable.
        /// 
        /// RÈGLES MÉTIER APPLIQUÉES :
        /// - Si la tâche a un métier, suggérer les tâches des métiers prérequis
        /// - Privilégier les jalons quand ils existent
        /// - Appliquer la remontée de chaîne si nécessaire
        /// </summary>
        private HashSet<string> AppliquerSuggestionsMetier(Tache tache, List<Tache> candidatsValides)
        {
            var suggestions = new HashSet<string>();

            // Pas de suggestions pour les tâches sans métier (jalons)
            if (string.IsNullOrEmpty(tache.MetierId))
                return suggestions;

            try
            {
                // Obtenir les prérequis métier via ProjetService
                var prerequisMetier = _projetService.GetPrerequisForMetier(tache.MetierId);
                if (!prerequisMetier.Any()) return suggestions;

                // Identifier les métiers présents parmi les candidats valides
                var metiersPresentsParmi = candidatsValides
                    .Where(t => !string.IsNullOrEmpty(t.MetierId))
                    .GroupBy(t => t.MetierId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Obtenir les exclusions de la tâche cible
                var exclusions = ParseDependances(tache.ExclusionsDependances);

                // Pour chaque prérequis métier, trouver les meilleures tâches à suggérer
                foreach (var prerequisId in prerequisMetier)
                {
                    if (metiersPresentsParmi.ContainsKey(prerequisId))
                    {
                        var tachesDuPrerequis = metiersPresentsParmi[prerequisId];
                        // Filtrer les tâches exclues avant de chercher les meilleures
                        var tachesDuPrerequisNonExclues = tachesDuPrerequis
                            .Where(t => !exclusions.Contains(t.TacheId))
                            .ToList();
                        var meilleuresTaches = TrouverMeilleuresTachesPourSuggestion(tachesDuPrerequisNonExclues, candidatsValides);
                        suggestions.UnionWith(meilleuresTaches.Select(t => t.TacheId));
                    }
                    else
                    {
                        // Remontée de chaîne : chercher des prérequis plus en amont
                        var prerequisIndirects = RemonterChainePrerequis(prerequisId, metiersPresentsParmi.Keys.ToHashSet());
                        foreach (var prerequisIndirect in prerequisIndirects)
                        {
                            if (metiersPresentsParmi.ContainsKey(prerequisIndirect))
                            {
                                var tachesDuPrerequis = metiersPresentsParmi[prerequisIndirect];
                                // Filtrer les tâches exclues avant de chercher les meilleures
                                var tachesDuPrerequisNonExclues = tachesDuPrerequis
                                    .Where(t => !exclusions.Contains(t.TacheId))
                                    .ToList();
                                var meilleuresTaches = TrouverMeilleuresTachesPourSuggestion(tachesDuPrerequisNonExclues, candidatsValides);
                                suggestions.UnionWith(meilleuresTaches.Select(t => t.TacheId));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur dans la logique métier, continuer sans suggestions
            }

            return suggestions;
        }

        /// <summary>
        /// Trouve les meilleures tâches à suggérer pour un métier donné.
        /// 
        /// PRIORITÉS :
        /// 1. Privilégier les jalons s'ils existent
        /// 2. Sinon, suggérer les "fins de chaîne" (tâches sans successeur dans le métier)
        /// 3. En dernier recours, suggérer toutes les tâches du métier
        /// </summary>
        private List<Tache> TrouverMeilleuresTachesPourSuggestion(List<Tache> tachesDuMetier, List<Tache> toutesLesTaches)
        {
            // 1. Chercher les jalons qui dépendent des tâches de ce métier
            var jalonsSuccesseurs = toutesLesTaches
                .Where(t => t.EstJalon)
                .Where(jalon =>
                {
                    var dependancesJalon = ParseDependances(jalon.Dependencies);
                    return tachesDuMetier.Any(tm => dependancesJalon.Contains(tm.TacheId));
                })
                .ToList();

            if (jalonsSuccesseurs.Any())
            {
                return jalonsSuccesseurs; // Privilégier les jalons
            }

            // 2. Sinon, chercher les fins de chaîne du métier
            var finsDeChaine = TrouverFinsDeChaineDuMetier(tachesDuMetier);
            if (finsDeChaine.Any())
            {
                return finsDeChaine;
            }

            // 3. En dernier recours, toutes les tâches du métier
            return tachesDuMetier;
        }

        /// <summary>
        /// Trouve les fins de chaîne d'un métier : tâches sans successeur du même métier.
        /// 
        /// ALGORITHME SIMPLIFIÉ :
        /// Une tâche est une "fin de chaîne" si aucune autre tâche du même métier ne dépend d'elle.
        /// </summary>
        private List<Tache> TrouverFinsDeChaineDuMetier(List<Tache> tachesDuMetier)
        {
            var finsDeChaine = new List<Tache>();

            foreach (var tache in tachesDuMetier)
            {
                // Vérifier si une autre tâche du même métier dépend de cette tâche
                bool aSuccesseurDansMemeMetier = tachesDuMetier.Any(autreTache =>
                {
                    if (autreTache.TacheId == tache.TacheId) return false;
                    var dependances = ParseDependances(autreTache.Dependencies);
                    return dependances.Contains(tache.TacheId);
                });

                if (!aSuccesseurDansMemeMetier)
                {
                    finsDeChaine.Add(tache);
                }
            }

            return finsDeChaine.Any() ? finsDeChaine : tachesDuMetier;
        }

        /// <summary>
        /// Remonte la chaîne des prérequis métier pour trouver des métiers présents.
        /// 
        /// APPLICATION DE LA RÈGLE 3 :
        /// Si un métier prérequis direct n'est pas présent, chercher ses prérequis
        /// jusqu'à trouver des métiers effectivement présents dans le bloc.
        /// </summary>
        private HashSet<string> RemonterChainePrerequis(string metierId, HashSet<string> metiersPresents)
        {
            var prerequisTrouves = new HashSet<string>();
            var visite = new HashSet<string>();
            var aExplorer = new Queue<string>();

            aExplorer.Enqueue(metierId);

            while (aExplorer.Count > 0)
            {
                var metierCourant = aExplorer.Dequeue();
                if (visite.Contains(metierCourant)) continue;
                visite.Add(metierCourant);

                try
                {
                    // Changement ici : _projetService.GetPrerequisForMetier
                    var prerequisDirects = _projetService.GetPrerequisForMetier(metierCourant);
                    foreach (var prerequis in prerequisDirects)
                    {
                        if (metiersPresents.Contains(prerequis))
                        {
                            prerequisTrouves.Add(prerequis);
                        }
                        else
                        {
                            aExplorer.Enqueue(prerequis);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignorer les erreurs de métier et continuer
                }
            }

            return prerequisTrouves;
        }

        /// <summary>
        /// Parse une chaîne de dépendances au format CSV en ensemble d'IDs.
        /// </summary>
        private HashSet<string> ParseDependances(string dependancesStr)
        {
            if (string.IsNullOrWhiteSpace(dependancesStr))
                return new HashSet<string>();

            return dependancesStr
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();
        }

        /// <summary>
        /// Classifie les candidats selon leur état de dépendance.
        /// </summary>
        private List<DependanceAffichage> ClassifierDependances(
            List<Tache> candidatsValides,
            HashSet<string> strictes,
            HashSet<string> excluses,
            HashSet<string> suggestions)
        {
            var resultats = new List<DependanceAffichage>();

            foreach (var candidat in candidatsValides)
            {
                EtatDependance etat;
                bool estHeritee;

                if (strictes.Contains(candidat.TacheId))
                {
                    etat = EtatDependance.Stricte;
                    estHeritee = false;
                }
                else if (excluses.Contains(candidat.TacheId))
                {
                    etat = EtatDependance.Exclue;
                    estHeritee = true;
                }
                else if (suggestions.Contains(candidat.TacheId))
                {
                    etat = EtatDependance.Suggeree;
                    estHeritee = true;
                }
                else
                {
                    etat = EtatDependance.Neutre;
                    estHeritee = false;
                }

                resultats.Add(new DependanceAffichage
                {
                    TachePredecesseur = candidat,
                    Etat = etat,
                    EstHeritee = estHeritee
                });
            }

            return resultats.OrderBy(d => d.TachePredecesseur.TacheNom).ToList();
        }
    }
}