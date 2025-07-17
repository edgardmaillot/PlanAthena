namespace PlanAthena.Data
{
    /// <summary>
    /// Représente une activité dans le système de planification, qui peut être une tâche ou un jalon.
    /// </summary>
    public class Tache
    {
        public string TacheId { get; set; } = "";
        public string TacheNom { get; set; } = "";
        public int HeuresHommeEstimees { get; set; }
        public string MetierId { get; set; } = "";
        public string Dependencies { get; set; } = "";
        public string ExclusionsDependances { get; set; } = "";

        // NOUVEAU: Propriétés pour définir la nature de l'activité
        public TypeActivite Type { get; set; } = TypeActivite.Tache;
        public bool EstJalon => Type != TypeActivite.Tache;


        // Propriétés de regroupement
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