namespace PlanAthena.Data
{
    /// <summary>
    /// Représente un lot de travaux, un regroupement logique de tâches (ex: CVC RDC).
    /// Correspond au concept de "Location" dans Dalux.
    /// </summary>
    public class Lot
    {
        public string LotId { get; set; } = "";
        public string Nom { get; set; } = "";
        public int Priorite { get; set; }
        public string CheminFichierPlan { get; set; } = "";
    }
}