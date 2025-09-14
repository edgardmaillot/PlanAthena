// Emplacement: /Services/DTOs/ImportExport/TacheExportDto.cs

using ChoETL;
using PlanAthena.Data;
using System.Linq;

namespace PlanAthena.Services.DTOs.ImportExport
{
    /// <summary>
    /// DTO (Data Transfer Object) spécifiquement conçu pour l'export des tâches au format CSV.
    /// Il contient tous les attributs ChoETL pour définir l'ordre, les noms de colonnes et le formatage,
    /// laissant le modèle de domaine 'Tache' propre.
    /// </summary>
    [ChoCSVFileHeader]
    public class TacheExportDto
    {
        // Les attributs définissent l'ordre et le nom des colonnes dans le fichier CSV final.
        [ChoCSVRecordField(Order = 1)]
        public string TacheId { get; set; }

        [ChoCSVRecordField(Order = 2)]
        public string IdImporte { get; set; }

        [ChoCSVRecordField(Order = 3)]
        public string TacheNom { get; set; }

        [ChoCSVRecordField(Order = 4)]
        public int HeuresHommeEstimees { get; set; }

        [ChoCSVRecordField(Order = 5)]
        public string MetierId { get; set; }

        [ChoCSVRecordField(Order = 6)]
        public string Dependencies { get; set; }

        [ChoCSVRecordField(Order = 7)]
        public string ExclusionsDependances { get; set; }

        [ChoCSVRecordField(Order = 8)]
        public TypeActivite Type { get; set; }

        [ChoCSVRecordField(Order = 9)]
        public string LotId { get; set; }

        [ChoCSVRecordField(Order = 10)]
        public string BlocId { get; set; }

        [ChoCSVRecordField(Order = 11)]
        public string ParentId { get; set; }

        [ChoCSVRecordField(Order = 12)]
        public Statut Statut { get; set; }

        [ChoCSVRecordField(Order = 13)]
        public bool EstConteneur { get; set; }

        [ChoCSVRecordField(Order = 14)]
        public DateTime? DateDebutPlanifiee { get; set; }

        [ChoCSVRecordField(Order = 15)]
        public DateTime? DateFinPlanifiee { get; set; }

        [ChoCSVRecordField(Order = 16, FieldName = "Affectations")]
        public string Affectations { get; set; }

        /// <summary>
        /// Constructeur par défaut requis par certaines bibliothèques de sérialisation.
        /// </summary>
        public TacheExportDto() { }

        /// <summary>
        /// Constructeur de mapping qui convertit un objet de domaine 'Tache' en DTO pour l'export.
        /// </summary>
        /// <param name="tache">L'objet Tache source.</param>
        public TacheExportDto(Tache tache)
        {
            TacheId = tache.TacheId;
            IdImporte = tache.IdImporte;
            TacheNom = tache.TacheNom;
            HeuresHommeEstimees = tache.HeuresHommeEstimees;
            MetierId = tache.MetierId;
            Dependencies = tache.Dependencies;
            ExclusionsDependances = tache.ExclusionsDependances;
            Type = tache.Type;
            LotId = tache.LotId;
            BlocId = tache.BlocId;
            ParentId = tache.ParentId;
            Statut = tache.Statut;
            EstConteneur = tache.EstConteneur;
            DateDebutPlanifiee = tache.DateDebutPlanifiee;
            DateFinPlanifiee = tache.DateFinPlanifiee;

            // Logique de transformation pour la propriété complexe
            if (tache.Affectations != null && tache.Affectations.Any())
            {
                Affectations = string.Join("; ", tache.Affectations.Select(a => $"{a.NomOuvrier} ({a.HeuresTravaillees}h)"));
            }
            else
            {
                Affectations = string.Empty;
            }
        }
    }
}