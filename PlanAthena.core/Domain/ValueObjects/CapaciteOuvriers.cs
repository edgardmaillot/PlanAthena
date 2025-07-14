// PlanAthena.Core.Domain.ValueObjects.CapaciteOuvriers.cs
namespace PlanAthena.Core.Domain.ValueObjects
{
    public readonly record struct CapaciteOuvriers
    {
        public int Value { get; }

        public CapaciteOuvriers(int value)
        {
            if (value <= 0) // Une capacité doit être strictement positive
                throw new ArgumentOutOfRangeException(nameof(value), "La capacité d'ouvriers doit être strictement positive.");
            Value = value;
        }

        public static implicit operator int(CapaciteOuvriers capacite) => capacite.Value;
        public static implicit operator CapaciteOuvriers(int value) => new(value);

        public override string ToString() => Value.ToString();
    }
}