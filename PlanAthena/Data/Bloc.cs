namespace PlanAthena.Data
{
    /// <summary>
    /// Représente un bloc, une zone géographique ou fonctionnelle du chantier (ex: Bureau 101).
    /// Correspond au concept de "Room" dans Dalux.
    /// </summary>
    public class Bloc
    {
        public string BlocId { get; set; } = "";
        public string Nom { get; set; } = "";
        public int CapaciteMaxOuvriers { get; set; }
    }
}