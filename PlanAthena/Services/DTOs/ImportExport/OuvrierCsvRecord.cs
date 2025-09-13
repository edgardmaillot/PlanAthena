// Emplacement: /Services/DTOs/ImportExport/OuvrierCsvRecord.cs

using ChoETL;

namespace PlanAthena.Services.Business.DTOs 
{
    /// <summary>
    /// DTO pour l'import/export CSV des ouvriers.
    // Chaque instance de cette classe représente UNE LIGNE dans le fichier CSV.
    /// </summary>
    [ChoCSVFileHeader]
    public class OuvrierCsvRecord
    {
        [ChoCSVRecordField(Order = 1)]
        public string OuvrierId { get; set; }

        [ChoCSVRecordField(Order = 2)]
        public string Nom { get; set; }

        [ChoCSVRecordField(Order = 3)]
        public string Prenom { get; set; }

        [ChoCSVRecordField(Order = 4)]
        public int CoutJournalier { get; set; }

        [ChoCSVRecordField(Order = 5)]
        public string MetierId { get; set; }


        // Un constructeur par défaut est une bonne pratique pour les DTOs.
        public OuvrierCsvRecord() { }
    }
}