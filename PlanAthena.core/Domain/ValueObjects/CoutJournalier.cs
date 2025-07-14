// PlanAthena.Core.Domain.ValueObjects.CoutJournalier.cs
namespace PlanAthena.Core.Domain.ValueObjects
{
    public readonly record struct CoutJournalier
    {
        public int Value { get; } // En euros entiers
        public string Devise { get; }

        public CoutJournalier(int value, string devise = "EUR")
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Le coût journalier ne peut pas être négatif.");
            if (string.IsNullOrWhiteSpace(devise))
                throw new ArgumentException("La devise ne peut pas être vide ou nulle.", nameof(devise));

            Value = value;
            Devise = devise;
        }

        // Pas d'opérateur de conversion implicite vers int pour éviter la perte d'information de la devise,
        // sauf si c'est un besoin très spécifique et conscient.

        public override string ToString() => $"{Value} {Devise}";
    }
}