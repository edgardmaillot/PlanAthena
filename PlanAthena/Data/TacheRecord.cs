namespace PlanAthena.Data
{
    public class TacheRecord
    {
        public string TacheId { get; set; }
        public string TacheNom { get; set; }
        public int HeuresHommeEstimees { get; set; }
        public string MetierId { get; set; }
        public string Dependencies { get; set; }
        public string LotId { get; set; }
        public string LotNom { get; set; }
        public int LotPriorite { get; set; }
        public string BlocId { get; set; }
        public string BlocNom { get; set; }
        public int BlocCapaciteMaxOuvriers { get; set; }
    }
}
