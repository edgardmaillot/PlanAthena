// PlanAthena.Core.Domain.ValueObjects.DureeHeuresHomme.cs
using System;

namespace PlanAthena.Core.Domain.ValueObjects
{
    public readonly record struct DureeHeuresHomme
    {
        public int Value { get; } // En heures entières

        public DureeHeuresHomme(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "La durée en heures-homme ne peut pas être négative.");
            Value = value;
        }

        public static implicit operator int(DureeHeuresHomme duree) => duree.Value;
        public static implicit operator DureeHeuresHomme(int value) => new(value);

        public static DureeHeuresHomme operator +(DureeHeuresHomme a, DureeHeuresHomme b) =>
            new(a.Value + b.Value);

        public static DureeHeuresHomme operator -(DureeHeuresHomme a, DureeHeuresHomme b) =>
            new(a.Value - b.Value); // Peut devenir négatif, la validation est à la construction

        public override string ToString() => $"{Value} HH";
    }
}