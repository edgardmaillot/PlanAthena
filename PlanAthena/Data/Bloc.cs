// Fichier: PlanAthena/Data/Bloc.cs
// Version: 0.4.4


namespace PlanAthena.Data
{
    /// <summary>
    /// Représente un bloc, une zone géographique ou fonctionnelle du chantier.
    /// </summary>
    public class Bloc
    {
        public string BlocId { get; set; } = "";
        public string Nom { get; set; } = "";
        public int CapaciteMaxOuvriers { get; set; }

        /// <summary>
        /// NOUVEAU: ID du Lot auquel ce bloc appartient.
        /// Permet de retrouver le parent sans avoir à parcourir toute la structure.
        /// </summary>
        public string LotId { get; set; } = "";
    }
}