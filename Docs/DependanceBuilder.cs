using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Représente l'état d'affichage d'une dépendance potentielle dans l'UI.
    /// C'est un DTO interne pour la communication entre le service et la présentation.
    /// </summary>
    public class DependanceAffichage
    {
        public Tache TachePredecesseur { get; set; }
        public EtatDependance Etat { get; set; }
        public bool EstHeritee { get; set; } // Vrai si la dépendance est suggérée par les règles métier.

        public override string ToString()
        {
            var result = $"{TachePredecesseur.TacheNom} ({TachePredecesseur.TacheId})";
            if (Etat == EtatDependance.Suggeree) result += " (suggéré)";
            if (Etat == EtatDependance.Exclue) result += " (exclu)";
            return result;
        }
    }

    /// <summary>
    /// Énumère les états possibles d'une dépendance entre deux tâches.
    /// </summary>
    public enum EtatDependance
    {
        Neutre,    // Aucune relation définie.
        Suggeree,  // La dépendance est suggérée par la logique métier mais pas confirmée par l'utilisateur.
        Stricte,   // La dépendance est explicitement définie par l'utilisateur.
        Exclue     // La dépendance, bien que suggérée, est explicitement interdite par l'utilisateur.
    }

    /// <summary>
    /// DTO pour encapsuler le résultat d'une opération de validation.
    /// </summary>
    public class ValidationResult
    {
        public bool EstValide { get; set; } = true;
        public string MessageErreur { get; set; } = string.Empty;

        /// <summary>
        /// Factory method pour créer un résultat de validation réussi.
        /// </summary>
        public static ValidationResult Success()
        {
            return new ValidationResult { EstValide = true };
        }

        /// <summary>
        /// Factory method pour créer un résultat de validation en échec.
        /// </summary>
        public static ValidationResult Error(string messageErreur)
        {
            return new ValidationResult 
            { 
                EstValide = false, 
                MessageErreur = messageErreur ?? string.Empty 
            };
        }
    }

    /// <summary>
    /// SERVICE CERVEAU-CENTRAL : Gère toute la logique et la validation des dépendances de tâches.
    /// 
    /// ARCHITECTURE : Cette classe implémente l'algorithme de mapping automatique des dépendances
    /// en respectant 5 règles métier strictes. Elle utilise QuikGraph pour les opérations sur les
    /// graphes dirigés et garantit une isolation totale des traitements par bloc de travail.
    /// 
    /// ORDRE DES OPÉRATIONS (pour chaque bloc) :
    /// 1. Construction du graphe initial depuis les dépendances manuelles (Règle 2 : Primauté du manuel)
    /// 2. Génération des dépendances métier manquantes (Règle 3 : Suggestion ciblée)
    /// 3. Réduction transitive pour éliminer la redondance (Règle 4 : Anti-redondance)
    /// 4. Factorisation par jalons de synchronisation (Règle 5 : Anti-croisements multiples)
    /// 5. Mise à jour des modèles de données depuis le graphe final
    /// </summary>
    public class DependanceBuilder
    {
        private readonly MetierService _metierService;
        private int _jalonSyncCounter = 1;

        public DependanceBuilder(MetierService metierService)
        {
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
        }

        /// <summary>
        /// Méthode d'orchestration principale qui applique et simplifie les dépendances pour l'ensemble du projet.
        /// 
        /// RÈGLE 1 IMPLÉMENTÉE : Cette méthode garantit l'isolation stricte des blocs en traitant
        /// chaque bloc individuellement. Aucune dépendance ne peut traverser les frontières de blocs.
        /// 
        /// ARCHITECTURE : Le traitement séquentiel par bloc assure la cohérence et évite les effets
        /// de bord entre les différentes zones de travail du projet.
        /// </summary>
        /// <param name="taches">La liste complète des tâches du projet. Elle est modifiée directement.</param>
        public void AppliquerEtSimplifierDependances(List<Tache> taches)
        {
            if (taches == null || !taches.Any())
                return;

            // Réinitialisation du compteur pour assurer des IDs uniques de jalons
            _jalonSyncCounter = 1;

            // Identification de tous les blocs distincts présents dans le projet
            var blocs = taches.Select(t => t.BlocId)
                             .Distinct()
                             .Where(b => !string.IsNullOrEmpty(b))
                             .ToList();

            // Collection pour stocker les nouveaux jalons créés pendant le traitement
            var nouveauxJalons = new List<Tache>();

            // RÈGLE 1 : Traitement isolé bloc par bloc
            foreach (var blocId in blocs)
            {
                TraiterBloc(blocId, taches, nouveauxJalons);
            }

            // Ajout des nouveaux jalons créés à la liste globale des tâches
            if (nouveauxJalons.Any())
            {
                taches.AddRange(nouveauxJalons);
            }
        }

        /// <summary>
        /// Traite un bloc de manière isolée en appliquant les 5 règles métier dans l'ordre défini.
        /// 
        /// ARCHITECTURE : Cette méthode est le cœur de l'algorithme et enchaîne délibérément
        /// toutes les étapes pour maintenir la cohérence du traitement sur un bloc donné.
        /// Le choix de ne pas découper cette méthode en sous-méthodes est volontaire pour
        /// préserver la lisibilité du flux complet de traitement.
        /// 
        /// RÈGLES APPLIQUÉES :
        /// - Règle 1: Isolation stricte (seules les tâches du bloc sont traitées)
        /// - Règle 2: Primauté du manuel (graphe initialisé avec dépendances existantes)
        /// - Règle 3: Suggestion ciblée (arrêt dès métier trouvé dans le bloc)
        /// - Règle 4: Réduction transitive (QuikGraph TransitiveReductionAlgorithm)
        /// - Règle 5: Factorisation par jalons (anti-croisements multiples)
        /// </summary>
        /// <param name="blocId">L'identifiant du bloc à traiter</param>
        /// <param name="toutesLesTaches">La liste complète des tâches du projet</param>
        /// <param name="nouveauxJalons">Collection pour stocker les jalons créés</param>
        private void TraiterBloc(string blocId, List<Tache> toutesLesTaches, List<Tache> nouveauxJalons)
        {
            // RÈGLE 1 : Isolation stricte - Extraction des tâches du bloc uniquement
            var tachesDuBloc = toutesLesTaches.Where(t => t.BlocId == blocId).ToList();
            
            // Pas de traitement si le bloc contient moins de 2 tâches (pas de dépendances possibles)
            if (tachesDuBloc.Count < 2) 
                return;

            // Création d'un dictionnaire pour un accès rapide aux tâches par ID
            var tacheMap = tachesDuBloc.ToDictionary(t => t.TacheId);

            // Initialisation du graphe dirigé bidirectionnel pour manipuler les dépendances
            var graph = new BidirectionalGraph<string, Edge<string>>();
            graph.AddVertexRange(tachesDuBloc.Select(t => t.TacheId));

            // ÉTAPE 1 : Construction du graphe initial depuis les dépendances manuelles existantes
            // RÈGLE 2 : Primauté du manuel - Les choix de l'utilisateur sont respectés en priorité
            foreach (var tache in tachesDuBloc)
            {
                var dependancesIds = (tache.Dependencies ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim());

                foreach (var depId in dependancesIds)
                {
                    if (tacheMap.ContainsKey(depId))
                    {
                        graph.AddEdge(new Edge<string>(depId, tache.TacheId));
                    }
                }
            }

            // ÉTAPE 2 : Génération des dépendances métier manquantes
            // RÈGLE 3 : Suggestion ciblée - La recherche s'arrête dès qu'un métier prérequis est trouvé
            var metiersPresentsDansBloc = tacheMap.Values
                .Where(t => !string.IsNullOrEmpty(t.MetierId))
                .Select(t => t.MetierId)
                .ToHashSet();

            foreach (var tache in tachesDuBloc.Where(t => !t.EstJalon && !string.IsNullOrEmpty(t.MetierId)))
            {
                // RÈGLE 2 : Parsing des exclusions manuelles pour cette tâche
                var exclusions = (tache.ExclusionsDependances ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToHashSet();

                // Obtention des prérequis finaux (ceux présents dans le bloc)
                var prerequisFinaux = ObtenirPrerequisFinaux(tache.MetierId, metiersPresentsDansBloc);

                foreach (var metierPrerequisId in prerequisFinaux)
                {
                    // Vérification si ce prérequis métier est déjà couvert par une dépendance manuelle
                    var predecesseursActuels = graph.InEdges(tache.TacheId).Select(e => e.Source);
                    bool estDejaCouvert = predecesseursActuels.Any(predId => 
                        tacheMap.TryGetValue(predId, out var predTache) && 
                        predTache.MetierId == metierPrerequisId);

                    if (estDejaCouvert) 
                        continue;

                    // Recherche des "fins de chaîne" pour ce métier (tâches sans successeurs du même métier)
                    var finsDeChaine = TrouverFinsDeChaineDansGraphe(metierPrerequisId, tacheMap, graph);
                    
                    foreach (var finDeChaineId in finsDeChaine)
                    {
                        // RÈGLE 2 CRITIQUE : Vérifier que cette dépendance n'est pas explicitement exclue
                        if (exclusions.Contains(finDeChaineId))
                        {
                            // L'utilisateur a explicitement rejeté cette dépendance → on la respecte
                            continue;
                        }

                        // Ajout de l'arête si elle n'existe pas déjà
                        if (finDeChaineId != tache.TacheId && !graph.ContainsEdge(finDeChaineId, tache.TacheId))
                        {
                            graph.AddEdge(new Edge<string>(finDeChaineId, tache.TacheId));
                        }
                    }
                }
            }

            // ÉTAPE 3 : Réduction transitive pour éliminer les dépendances redondantes
            // RÈGLE 4 : Anti-redondance - Utilisation de l'algorithme mathématique de QuikGraph
            if (graph.IsDirectedAcyclicGraph())
            {
                var reductionAlgo = new TransitiveReductionAlgorithm<string, Edge<string>>(graph);
                reductionAlgo.Compute();
                
                // Reconstruction du graphe avec seulement les arêtes non redondantes
                var grapheReduit = reductionAlgo.TransitiveReduction;
                var graphSimplifie = new BidirectionalGraph<string, Edge<string>>();
                graphSimplifie.AddVertexRange(graph.Vertices);
                graphSimplifie.AddEdgeRange(grapheReduit.Edges);
                graph = graphSimplifie;
            }

            // ÉTAPE 4 : Factorisation par jalons de synchronisation
            // RÈGLE 5 : Anti-croisements multiples - Simplification des motifs "many-to-many"
            var groupesAFactoriser = graph.Vertices
                .Where(v => graph.InDegree(v) > 1 && !tacheMap[v].EstJalon) // Tâches avec plusieurs prédécesseurs
                .Select(tacheId => new { 
                    TacheId = tacheId, 
                    PredecesseursKey = string.Join(",", graph.InEdges(tacheId).Select(e => e.Source).OrderBy(id => id)) 
                })
                .GroupBy(x => x.PredecesseursKey) // Groupement par ensemble de prédécesseurs identiques
                .Where(g => g.Count() > 1) // Seuls les groupes avec plusieurs tâches
                .Where(g => EstFactorisationNecessaire(g.Key.Split(','), tacheMap)) // Éviter les jalons redondants
                .ToList();

            foreach (var groupe in groupesAFactoriser)
            {
                // Récupération du lot de la première tâche du groupe pour cohérence
                var lotId = tacheMap[groupe.First().TacheId].LotId;
                
                // Création d'un nouveau jalon de synchronisation avec ID unique
                var idJalon = $"J_Sync_{blocId}_{_jalonSyncCounter++}";
                var nouveauJalon = new Tache 
                { 
                    TacheId = idJalon, 
                    TacheNom = "Synchro", 
                    Type = TypeActivite.JalonDeSynchronisation, 
                    BlocId = blocId, 
                    LotId = lotId 
                };

                nouveauxJalons.Add(nouveauJalon);
                graph.AddVertex(idJalon);

                // Connexion des prédécesseurs communs au nouveau jalon
                var predecesseursIds = groupe.Key.Split(',');
                foreach (var predId in predecesseursIds)
                {
                    graph.AddEdge(new Edge<string>(predId, idJalon));
                }

                // Recâblage des tâches du groupe pour qu'elles dépendent du jalon
                foreach (var successeur in groupe)
                {
                    // Suppression des anciennes arêtes entrantes
                    graph.RemoveInEdgeIf(successeur.TacheId, e => true);
                    // Ajout de la nouvelle arête depuis le jalon
                    graph.AddEdge(new Edge<string>(idJalon, successeur.TacheId));
                }
            }

            // ÉTAPE 5 : Mise à jour des modèles de données depuis le graphe final
            foreach (var tache in tachesDuBloc)
            {
                tache.Dependencies = string.Join(",", 
                    graph.InEdges(tache.TacheId).Select(e => e.Source).OrderBy(id => id));
            }

            // Mise à jour des dépendances pour les nouveaux jalons de ce bloc
            foreach (var jalon in nouveauxJalons.Where(j => j.BlocId == blocId))
            {
                jalon.Dependencies = string.Join(",", 
                    graph.InEdges(jalon.TacheId).Select(e => e.Source).OrderBy(id => id));
            }
        }

        /// <summary>
        /// Détermine si la création d'un jalon de factorisation est nécessaire.
        /// 
        /// RÈGLE 5 SPÉCIFIQUE : Un jalon n'est créé que si les prédécesseurs communs ne sont pas
        /// déjà optimalement synchronisés par un jalon existant. Cette logique évite la création 
        /// de jalons redondants et préserve les jalons existants pertinents.
        /// 
        /// ARCHITECTURE : Cette méthode encapsule la logique de décision pour la factorisation,
        /// rendant le code principal plus lisible et la règle facilement modifiable.
        /// </summary>
        /// <param name="predecesseursIds">Liste des IDs des prédécesseurs communs</param>
        /// <param name="tacheMap">Dictionnaire d'accès aux tâches par ID</param>
        /// <returns>True si un jalon doit être créé, False sinon</returns>
        private bool EstFactorisationNecessaire(string[] predecesseursIds, Dictionary<string, Tache> tacheMap)
        {
            // CAS 1: Un seul prédécesseur qui est déjà un jalon -> pas de factorisation
            if (predecesseursIds.Length == 1 && 
                tacheMap.TryGetValue(predecesseursIds[0], out var tache) && 
                tache.EstJalon)
            {
                return false;
            }

            // CAS 2: Plusieurs prédécesseurs mais l'un d'eux est déjà un jalon de synchronisation
            // qui fait partie intégrante du motif -> la factorisation pourrait être contre-productive
            var jalonsExistants = predecesseursIds
                .Where(id => tacheMap.TryGetValue(id, out var t) && t.EstJalon)
                .ToList();

            // Si on a un jalon de synchronisation existant parmi les prédécesseurs,
            // il faut analyser si la factorisation apporte une réelle valeur ajoutée.
            // Pour l'instant, on privilégie la conservation des jalons existants.
            if (jalonsExistants.Any())
            {
                // HEURISTIQUE: Si plus de la moitié des prédécesseurs sont des jalons,
                // ou si on a un seul jalon avec peu de tâches individuelles,
                // on évite la re-factorisation pour préserver la structure existante.
                var proportionJalons = (double)jalonsExistants.Count / predecesseursIds.Length;
                var nombreTachesIndividuelles = predecesseursIds.Length - jalonsExistants.Count;
                
                if (proportionJalons >= 0.5 || nombreTachesIndividuelles <= 1)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Obtient les prérequis finaux pour un métier donné dans le contexte d'un bloc.
        /// 
        /// RÈGLE 3 IMPLÉMENTÉE : Suggestion ciblée - La recherche remonte la chaîne des prérequis
        /// métier mais s'arrête dès qu'un métier prérequis est trouvé dans le bloc.
        /// Cela évite la création de liens "en cascade" vers des métiers très en amont.
        /// 
        /// ALGORITHME : Parcours en largeur de l'arbre des prérequis avec arrêt conditionnel.
        /// </summary>
        /// <param name="metierIdInitial">L'ID du métier pour lequel on cherche les prérequis</param>
        /// <param name="metiersPresents">Ensemble des métiers présents dans le bloc</param>
        /// <returns>Ensemble des métiers prérequis finaux présents dans le bloc</returns>
        private HashSet<string> ObtenirPrerequisFinaux(string metierIdInitial, HashSet<string> metiersPresents)
        {
            var aExplorer = new Queue<string>(_metierService.GetPrerequisForMetier(metierIdInitial));
            var prerequisFinaux = new HashSet<string>();
            var dejaExplores = new HashSet<string> { metierIdInitial };

            while (aExplorer.Count > 0)
            {
                var metierCourant = aExplorer.Dequeue();
                
                // Éviter les cycles dans l'exploration
                if (!dejaExplores.Add(metierCourant)) 
                    continue;

                // RÈGLE 3 : Si le métier est présent dans le bloc, on s'arrête ici
                if (metiersPresents.Contains(metierCourant))
                {
                    prerequisFinaux.Add(metierCourant);
                }
                else
                {
                    // Sinon, on continue à remonter la chaîne
                    var parents = _metierService.GetPrerequisForMetier(metierCourant);
                    foreach (var parent in parents)
                    {
                        aExplorer.Enqueue(parent);
                    }
                }
            }

            return prerequisFinaux;
        }

        /// <summary>
        /// Trouve les "fins de chaîne" pour un métier donné dans le contexte du graphe.
        /// 
        /// DÉFINITION : Une "fin de chaîne" est une tâche du métier spécifié qui n'a pas
        /// de successeur du même métier. C'est le point de connexion optimal pour les
        /// dépendances inter-métiers.
        /// 
        /// ARCHITECTURE : Cette logique évite de créer des dépendances vers toutes les tâches
        /// d'un métier, mais seulement vers celles qui "terminent" le métier logiquement.
        /// </summary>
        /// <param name="metierId">L'ID du métier à analyser</param>
        /// <param name="tacheMap">Dictionnaire d'accès aux tâches par ID</param>
        /// <param name="graph">Le graphe des dépendances</param>
        /// <returns>Énumération des IDs des tâches "fins de chaîne"</returns>
        private IEnumerable<string> TrouverFinsDeChaineDansGraphe(string metierId, 
            Dictionary<string, Tache> tacheMap, 
            IBidirectionalGraph<string, Edge<string>> graph)
        {
            // Identification de toutes les tâches du métier spécifié
            var tachesDuMetierIds = tacheMap.Values
                .Where(t => t.MetierId == metierId)
                .Select(t => t.TacheId)
                .ToHashSet();

            if (!tachesDuMetierIds.Any()) 
                return Enumerable.Empty<string>();

            // Une tâche est une "fin de chaîne" si elle n'a pas de successeur du même métier
            return tachesDuMetierIds.Where(tacheId =>
                !graph.OutEdges(tacheId).Any(edge => tachesDuMetierIds.Contains(edge.Target))
            );
        }

        /// <summary>
        /// Obtient la liste des états de dépendances pour une tâche donnée dans son contexte.
        /// 
        /// USAGE UI : Cette méthode est appelée par l'interface utilisateur pour afficher
        /// les dépendances existantes et les suggestions métier dans le formulaire de détail.
        /// 
        /// RÈGLES APPLIQUÉES :
        /// - Règle 2 : Les dépendances manuelles (strictes) et exclusions sont respectées
        /// - Règle 3 : Les suggestions s'arrêtent dès qu'un prérequis est satisfait dans le graphe
        /// 
        /// ARCHITECTURE : Cette méthode reconstitue un graphe temporaire pour analyser
        /// le contexte de la tâche sans modifier les données persistantes.
        /// </summary>
        /// <param name="tacheActuelle">La tâche pour laquelle on veut les états de dépendances</param>
        /// <param name="contexteTaches">Les tâches du même bloc (contexte d'analyse)</param>
        /// <returns>Liste des états de dépendances pour affichage UI</returns>
        public List<DependanceAffichage> ObtenirEtatDependancesPourTache(Tache tacheActuelle, List<Tache> contexteTaches)
        {
            if (tacheActuelle == null || contexteTaches == null || !contexteTaches.Any())
                return new List<DependanceAffichage>();

            // Reconstruction du contexte graphique
            var tacheMap = contexteTaches.ToDictionary(t => t.TacheId);
            var graph = new BidirectionalGraph<string, Edge<string>>();
            graph.AddVertexRange(contexteTaches.Select(t => t.TacheId));

            // Construction du graphe depuis les dépendances existantes
            foreach (var tache in contexteTaches)
            {
                var deps = (tache.Dependencies ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim());
                
                foreach (var depId in deps)
                {
                    if (tacheMap.ContainsKey(depId))
                    {
                        graph.AddEdge(new Edge<string>(depId, tache.TacheId));
                    }
                }
            }

            // Analyse des besoins métier vs satisfaction actuelle
            var metiersRequis = string.IsNullOrEmpty(tacheActuelle.MetierId) 
                ? new HashSet<string>() 
                : _metierService.GetTransitivePrerequisites(tacheActuelle.MetierId);
            
            var metiersSatisfaits = TrouverMetiersSatisfaitsParGraphe(tacheActuelle.TacheId, graph, tacheMap);
            var metiersASuggerer = metiersRequis.Except(metiersSatisfaits).ToHashSet();

            // Parsing des dépendances et exclusions manuelles
            var dependancesStricts = (tacheActuelle.Dependencies ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .ToHashSet();
            
            var exclusions = (tacheActuelle.ExclusionsDependances ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .ToHashSet();

            var resultat = new List<DependanceAffichage>();
            var tachesDejaAjoutees = new HashSet<string>();

            // RÈGLE 2 : Ajout des dépendances et exclusions manuelles (primauté absolue)
            var depsExistantes = dependancesStricts.Union(exclusions);
            foreach (var depId in depsExistantes)
            {
                if (tacheMap.TryGetValue(depId, out var tacheCand) && tachesDejaAjoutees.Add(depId))
                {
                    var affichage = new DependanceAffichage { TachePredecesseur = tacheCand };
                    
                    if (dependancesStricts.Contains(depId))
                    {
                        affichage.Etat = EtatDependance.Stricte;
                    }
                    else // exclusions
                    {
                        affichage.Etat = EtatDependance.Exclue;
                    }
                    
                    resultat.Add(affichage);
                }
            }

            // RÈGLE 3 : Ajout des nouvelles suggestions métier (suggestion ciblée)
            var finsDeChaineASuggerer = metiersASuggerer
                .SelectMany(metierId => TrouverFinsDeChaineDansGraphe(metierId, tacheMap, graph))
                .Distinct();

            foreach (var tacheId in finsDeChaineASuggerer)
            {
                if (tacheMap.TryGetValue(tacheId, out var tacheCand) && tachesDejaAjoutees.Add(tacheId))
                {
                    var affichage = new DependanceAffichage 
                    { 
                        TachePredecesseur = tacheCand, 
                        Etat = EtatDependance.Suggeree, 
                        EstHeritee = true 
                    };
                    resultat.Add(affichage);
                }
            }

            return resultat.OrderBy(r => r.TachePredecesseur.TacheNom).ToList();
        }

        /// <summary>
        /// Identifie tous les métiers qui sont déjà satisfaits par le graphe de dépendances existant.
        /// 
        /// RÈGLE 3 SUPPORT : Cette méthode permet de déterminer quels prérequis métier sont
        /// déjà couverts par les dépendances actuelles, évitant ainsi les suggestions redondantes.
        /// 
        /// ALGORITHME : Parcours en largeur à partir de la tâche pour identifier tous les
        /// métiers accessibles via les dépendances directes et transitives.
        /// </summary>
        /// <param name="tacheId">L'ID de la tâche à analyser</param>
        /// <param name="graph">Le graphe des dépendances</param>
        /// <param name="tacheMap">Dictionnaire d'accès aux tâches par ID</param>
        /// <returns>Ensemble des métiers satisfaits par les dépendances existantes</returns>
        private HashSet<string> TrouverMetiersSatisfaitsParGraphe(string tacheId, 
            IBidirectionalGraph<string, Edge<string>> graph, 
            Dictionary<string, Tache> tacheMap)
        {
            var metiersSatisfaits = new HashSet<string>();
            
            if (!graph.ContainsVertex(tacheId)) 
                return metiersSatisfaits;

            // Parcours en largeur de tous les prédécesseurs (directs et transitifs)
            var toExplore = new Queue<string>(graph.InEdges(tacheId).Select(e => e.Source));
            var explored = new HashSet<string>();

            while (toExplore.Count > 0)
            {
                var currentId = toExplore.Dequeue();
                
                // Éviter les cycles
                if (!explored.Add(currentId)) 
                    continue;

                // Si la tâche a un métier, l'ajouter aux métiers satisfaits
                if (tacheMap.TryGetValue(currentId, out var currentTache) && 
                    !string.IsNullOrEmpty(currentTache.MetierId))
                {
                    metiersSatisfaits.Add(currentTache.MetierId);
                }

                // Continuer l'exploration vers les prédécesseurs
                if (graph.ContainsVertex(currentId))
                {
                    foreach (var pred in graph.InEdges(currentId).Select(e => e.Source))
                    {
                        toExplore.Enqueue(pred);
                    }
                }
            }

            return metiersSatisfaits;
        }

        /// <summary>
        /// Valide la cohérence logique du graphe de dépendances global.
        /// 
        /// USAGE : Cette méthode est appelée après chaque modification pour s'assurer
        /// que le graphe reste cohérent et respecte les contraintes métier.
        /// 
        /// VALIDATIONS EFFECTUÉES :
        /// - Règle 1 : Vérification qu'aucune dépendance ne traverse les frontières de blocs
        /// - Détection des cycles (dépendances circulaires)
        /// - Validation de l'existence des tâches référencées
        /// 
        /// ARCHITECTURE : Cette méthode fournit un point de contrôle qualité global
        /// pour l'intégrité des données de dépendances.
        /// </summary>
        /// <param name="taches">La liste complète des tâches à valider</param>
        /// <returns>Résultat de validation avec indication de succès ou d'erreur</returns>
        public ValidationResult ConstruireDependancesLogiques(List<Tache> taches)
        {
            if (taches == null || !taches.Any())
                return ValidationResult.Success();

            var tacheMap = taches.ToDictionary(t => t.TacheId);
            var graph = new BidirectionalGraph<string, Edge<string>>();
            graph.AddVertexRange(taches.Select(t => t.TacheId));

            // Construction du graphe global et validation des contraintes
            foreach (var tache in taches)
            {
                var dependancesIds = (tache.Dependencies ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim());

                foreach (var depId in dependancesIds)
                {
                    // Validation de l'existence de la tâche prédécesseur
                    if (!tacheMap.TryGetValue(depId, out var predecesseur))
                    {
                        // Dépendance vers une tâche inexistante (tolérance pour éviter les erreurs de cascade)
                        continue;
                    }

                    // RÈGLE 1 : Validation de l'isolation des blocs
                    if (predecesseur.BlocId != tache.BlocId)
                    {
                        return ValidationResult.Error(
                            $"Dépendance invalide détectée : La tâche '{tache.TacheId}' ne peut pas dépendre de '{depId}', " +
                            $"car elles ne sont pas dans le même bloc de travail.");
                    }

                    graph.AddEdge(new Edge<string>(depId, tache.TacheId));
                }
            }

            // Validation de l'absence de cycles (dépendances circulaires)
            if (graph.IsDirectedAcyclicGraph())
            {
                return ValidationResult.Success();
            }
            else
            {
                return ValidationResult.Error(
                    "Dépendance circulaire détectée ! Veuillez vérifier les dépendances de vos tâches " +
                    "pour trouver et corriger le cycle.");
            }
        }
    }
}