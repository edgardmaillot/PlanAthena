// PlanAthena.Core.Domain.Entity.cs
namespace PlanAthena.Core.Domain
{
    /// <summary>
    /// Classe de base pour les entités du domaine.
    /// Fournit une gestion de l'identité et de l'égalité basée sur l'ID.
    /// </summary>
    /// <typeparam name="TId">Le type de l'identifiant de l'entité.</typeparam>
    public abstract class Entity<TId> : IEquatable<Entity<TId>>
        where TId : notnull // L'ID ne peut pas être null
    {
        public TId Id { get; protected set; } // protected set pour permettre l'initialisation par les classes dérivées

        protected Entity(TId id)
        {
            // if (id == null || id.Equals(default(TId))) // Pour les structs, default(TId) peut être valide (ex: Guid.Empty)
            // {
            //     throw new ArgumentException("L'identifiant de l'entité ne peut pas être la valeur par défaut.", nameof(id));
            // }
            // Edgard: La validation de l'ID lui-même (ex: non vide pour un string) est dans le VO de l'ID.
            // Ici, on s'assure juste qu'on a un ID.
            Id = id;
        }

        // Constructeur sans paramètre pour certains ORM ou sérialiseurs, mais protégé pour forcer l'usage via constructeur avec ID.
        // Pour DDD, on préfère souvent ne pas avoir de constructeur sans paramètre public.
        protected Entity() { }


        public override bool Equals(object? obj)
        {
            return obj is Entity<TId> other && Equals(other);
        }

        public virtual bool Equals(Entity<TId>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            // Si l'un des IDs est la valeur par défaut (pour les structs comme Guid.Empty par ex,
            // ou si on utilisait 0 pour des int IDs), ils ne sont pas persistés et ne peuvent pas être égaux.
            // Pour nos VOs d'ID basés sur string, cette vérification de default est moins pertinente
            // car la validation est dans le VO lui-même.
            // if (Id.Equals(default(TId)) || other.Id.Equals(default(TId)))
            //    return false;

            return Id.Equals(other.Id);
        }

        public static bool operator ==(Entity<TId> left, Entity<TId> right)
        {
            if (left is null && right is null)
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(Entity<TId> left, Entity<TId> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            // L'ID est la seule chose qui définit l'identité de l'entité.
            return Id.GetHashCode();
        }
    }
}