namespace PlanAthena.Services.DTOs.ImportExport
{
    /// <summary>
    /// DTO "plat" spécifiquement conçu pour l'export des ouvriers au format CSV.
    /// Chaque instance représente une seule ligne dans le fichier final,
    /// avec le nom du métier déjà résolu.
    /// </summary>
    public class OuvrierExportDto
    {
        public string OuvrierId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public int CoutJournalier { get; set; }

        // a corriger, car en réalité c'est un ID qui est exporté.
        //voir RessourceOuvrierView.btnExporterOuvriers_Click pour l'implémentation de ce DTO
        public string MetierNom { get; set; } // La propriété qui contient le nom traduit
    }
}