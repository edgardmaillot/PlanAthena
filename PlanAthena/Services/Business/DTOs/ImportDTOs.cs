namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Résumé d'un import CSV groupé
    /// </summary>
    public class ResumeImport
    {
        public bool Succes { get; set; }
        public string MessageErreur { get; set; } = "";
        public int MetiersImportes { get; set; }
        public int OuvriersImportes { get; set; }
        public int TachesImportees { get; set; }
        public int TotalImporte => MetiersImportes + OuvriersImportes + TachesImportees;
    }
}