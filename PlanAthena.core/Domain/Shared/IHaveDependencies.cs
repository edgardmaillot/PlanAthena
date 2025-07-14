namespace PlanAthena.Core.Domain.Shared
{
    /// <summary>
    /// Définit une entité qui possède une identité et une liste de dépendances vers d'autres entités du même type.
    /// Permet une gestion générique des graphes de dépendances.
    /// </summary>
    /// <typeparam name="TId">Le type de l'identifiant de l'entité.</typeparam>
    public interface IHaveDependencies<TId> where TId : notnull
    {
        TId Id { get; }
        IReadOnlyCollection<TId> Dependencies { get; } // Utilisation de IReadOnlyCollection, plus générique que List ou Set
    }
}