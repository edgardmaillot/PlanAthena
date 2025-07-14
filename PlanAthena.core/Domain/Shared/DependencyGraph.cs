namespace PlanAthena.Core.Domain.Shared
{
    /// <summary>
    /// Représente un graphe de dépendances et fournit des méthodes pour l'analyser,
    /// notamment pour la détection de cycles. Cet objet est immuable.
    /// </summary>
    /// <typeparam name="TId">Le type de l'identifiant des noeuds du graphe.</typeparam>
    public sealed class DependencyGraph<TId> where TId : notnull
    {
        private readonly IReadOnlyDictionary<TId, IReadOnlyList<TId>> _adjacencyList;
        private readonly IReadOnlyList<TId> _nodes;

        private enum NodeColor { White, Gray, Black }

        // CONSTRUCTEUR CORRIGÉ (que tu as déjà)
        public DependencyGraph(IEnumerable<IHaveDependencies<TId>> entities)
        {
            var entityList = entities.ToList();
            var adjList = entityList.ToDictionary(e => e.Id, e => new List<TId>());

            foreach (var entity in entityList)
            {
                foreach (var dependencyId in entity.Dependencies)
                {
                    if (adjList.ContainsKey(dependencyId))
                    {
                        adjList[dependencyId].Add(entity.Id);
                    }
                }
            }

            _nodes = entityList.Select(e => e.Id).ToList();
            _adjacencyList = adjList.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<TId>)kvp.Value.AsReadOnly());
        }

        /// <summary>
        /// Détecte tous les cycles dans le graphe en utilisant un algorithme de parcours en profondeur (DFS).
        /// </summary>
        public IReadOnlyList<IReadOnlyList<TId>> DetectCycles()
        {
            var cycles = new List<IReadOnlyList<TId>>();
            var colors = _nodes.ToDictionary(n => n, _ => NodeColor.White);
            var parent = new Dictionary<TId, TId>();

            foreach (var node in _nodes)
            {
                if (colors[node] == NodeColor.White)
                {
                    DetectCyclesDfs(node, colors, parent, cycles);
                }
            }
            return cycles;
        }

        // MÉTHODE DE DÉTECTION CORRIGÉE (la ligne fautive est supprimée)
        private void DetectCyclesDfs(TId u, Dictionary<TId, NodeColor> colors, Dictionary<TId, TId> parent, List<IReadOnlyList<TId>> cycles)
        {
            colors[u] = NodeColor.Gray;

            if (_adjacencyList.TryGetValue(u, out var neighbors))
            {
                foreach (var v in neighbors)
                {
                    // La ligne "if (!colors.ContainsKey(v))" a été DÉFINITIVEMENT supprimée.
                    // C'était le bug qui subsistait.

                    if (colors[v] == NodeColor.Gray)
                    {
                        // Cycle détecté
                        var cycle = new List<TId> { v };
                        var current = u;
                        while (!current.Equals(v))
                        {
                            cycle.Add(current);
                            current = parent[current];
                        }
                        cycle.Add(v);
                        cycle.Reverse();
                        cycles.Add(cycle);
                    }

                    if (colors[v] == NodeColor.White)
                    {
                        parent[v] = u;
                        DetectCyclesDfs(v, colors, parent, cycles);
                    }
                }
            }
            colors[u] = NodeColor.Black;
        }
    }
}