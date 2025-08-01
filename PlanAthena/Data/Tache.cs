namespace PlanAthena.Data
{
    /// <summary>
    /// Représente une activité dans le système de planification, qui peut être une tâche ou un jalon.
    /// </summary>
    public class Tache
    {
        public string TacheId { get; set; } = "";
        /// IdImporte = ID d'origine de la tâche tel qu'il figurait dans le fichier source importé.
        /// Cet ID n'est pas utilisé pour la logique interne de PlanAthena mais sert de référence.
        public string IdImporte { get; set; } = ""; 
        public string TacheNom { get; set; } = "";
        public int HeuresHommeEstimees { get; set; }
        public string MetierId { get; set; } = "";
        public string Dependencies { get; set; } = "";
        public string ExclusionsDependances { get; set; } = "";
        public TypeActivite Type { get; set; } = TypeActivite.Tache;
        public bool EstJalon => Type != TypeActivite.Tache;

        public string LotId { get; set; } = "";
        public string BlocId { get; set; } = "";

        public override string ToString()
        {
            return $"{TacheId} - {TacheNom} ({HeuresHommeEstimees}h)";
        }
    }
}