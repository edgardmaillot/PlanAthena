// PlanAthena.Core.Domain.ValueObjects.Identifiants.cs (ou fichiers séparés)
using System; // Nécessaire pour ArgumentException dans les constructeurs si validation

namespace PlanAthena.Core.Domain.ValueObjects
{
    // Note Edgard: J'ajoute une validation simple (non vide) dans les constructeurs
    // pour illustrer la protection des invariants dès la création du VO.

    public readonly record struct ChantierId
    {
        public string Value { get; }

        public ChantierId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ChantierId ne peut pas être vide ou nul.", nameof(value));
            Value = value;
        }

        public static implicit operator string(ChantierId id) => id.Value;
        public static implicit operator ChantierId(string value) => new(value);
        public override string ToString() => Value;
    }

    public readonly record struct BlocId
    {
        public string Value { get; }

        public BlocId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("BlocId ne peut pas être vide ou nul.", nameof(value));
            Value = value;
        }

        public static implicit operator string(BlocId id) => id.Value;
        public static implicit operator BlocId(string value) => new(value);
        public override string ToString() => Value;
    }

    public readonly record struct TacheId
    {
        public string Value { get; }

        public TacheId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("TacheId ne peut pas être vide ou nul.", nameof(value));
            Value = value;
        }

        public static implicit operator string(TacheId id) => id.Value;
        public static implicit operator TacheId(string value) => new(value);
        public override string ToString() => Value;
    }

    public readonly record struct LotId
    {
        public string Value { get; }

        public LotId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("LotId ne peut pas être vide ou nul.", nameof(value));
            Value = value;
        }

        public static implicit operator string(LotId id) => id.Value;
        public static implicit operator LotId(string value) => new(value);
        public override string ToString() => Value;
    }

    public readonly record struct OuvrierId
    {
        public string Value { get; }

        public OuvrierId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("OuvrierId ne peut pas être vide ou nul.", nameof(value));
            Value = value;
        }

        public static implicit operator string(OuvrierId id) => id.Value;
        public static implicit operator OuvrierId(string value) => new(value);
        public override string ToString() => Value;
    }

    public readonly record struct MetierId
    {
        public string Value { get; }

        public MetierId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("MetierId ne peut pas être vide ou nul.", nameof(value));
            Value = value;
        }

        public static implicit operator string(MetierId id) => id.Value;
        public static implicit operator MetierId(string value) => new(value);
        public override string ToString() => Value;
    }
}