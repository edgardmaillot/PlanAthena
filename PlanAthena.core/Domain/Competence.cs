// PlanAthena.Core.Domain.Competence.cs
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Core.Domain
{
    public class Competence : IEquatable<Competence>
    {
        public MetierId MetierId { get; }
        public NiveauExpertise Niveau { get; }
        public int PerformanceEffectivePct { get; }

        public Competence(MetierId metierId, NiveauExpertise niveau, int? performancePctInput = null)
        {
            MetierId = metierId;
            Niveau = niveau;

            if (performancePctInput.HasValue)
            {
                if (performancePctInput.Value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(performancePctInput), "Le pourcentage de performance doit être positif.");
                PerformanceEffectivePct = performancePctInput.Value;
            }
            else
            {
                PerformanceEffectivePct = 100;
            }
        }

        public bool Equals(Competence? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return MetierId.Equals(other.MetierId) &&
                   Niveau == other.Niveau &&
                   PerformanceEffectivePct == other.PerformanceEffectivePct;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Competence);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MetierId, Niveau, PerformanceEffectivePct);
        }
    }
}