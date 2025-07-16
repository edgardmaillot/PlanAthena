namespace PlanAthena.Data
{
    /// <summary>
    /// Représente une tâche dans le système de planification
    /// </summary>
    public class TacheRecord
    {
        public string TacheId { get; set; } = "";
        public string TacheNom { get; set; } = "";
        public int HeuresHommeEstimees { get; set; }
        public string MetierId { get; set; } = "";
        public string Dependencies { get; set; } = "";

        /// <summary>
        /// Liste des IDs de tâches à exclure des dépendances héritées du métier
        /// Format: "ID1,ID2,ID3"
        /// </summary>
        public string ExclusionsDependances { get; set; } = "";

        // SUPPRIMÉ: public bool EstSurchargeJalon { get; set; } = false;

        public string LotId { get; set; } = "";
        public string LotNom { get; set; } = "";
        public int LotPriorite { get; set; }

        public string BlocId { get; set; } = "";
        public string BlocNom { get; set; } = "";
        public int BlocCapaciteMaxOuvriers { get; set; }

        public override string ToString()
        {
            return $"{TacheId} - {TacheNom} ({HeuresHommeEstimees}h)";
        }
    }
}