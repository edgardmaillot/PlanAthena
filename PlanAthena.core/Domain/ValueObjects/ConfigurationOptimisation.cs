// Dans le fichier Domain/ValueObjects/ConfigurationOptimisation.cs

namespace PlanAthena.Core.Domain.ValueObjects
{
    public record ConfigurationOptimisation
    {
        public int DureeJournaliereStandardHeures { get; }
        public decimal PenaliteChangementOuvrierPourcentage { get; }
        public long CoutIndirectJournalierEnCentimes { get; }

        // --- MODIFICATION DU CONSTRUCTEUR ---
        public ConfigurationOptimisation(
            int dureeJournaliereStandardHeures,
            decimal penaliteChangementOuvrierPourcentage,
            long coutIndirectJournalierEnCentimes)
        {
            if (dureeJournaliereStandardHeures <= 0)
                throw new ArgumentOutOfRangeException(nameof(dureeJournaliereStandardHeures), "La durée journalière standard doit être strictement positive.");

            if (penaliteChangementOuvrierPourcentage < 0)
                throw new ArgumentOutOfRangeException(nameof(penaliteChangementOuvrierPourcentage), "La pénalité ne peut pas être négative.");

            if (coutIndirectJournalierEnCentimes < 0)
                throw new ArgumentOutOfRangeException(nameof(coutIndirectJournalierEnCentimes), "Le coût indirect ne peut pas être négatif.");

            DureeJournaliereStandardHeures = dureeJournaliereStandardHeures;
            PenaliteChangementOuvrierPourcentage = penaliteChangementOuvrierPourcentage;
            CoutIndirectJournalierEnCentimes = coutIndirectJournalierEnCentimes;
        }
    }
}