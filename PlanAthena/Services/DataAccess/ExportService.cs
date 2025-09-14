using ChoETL;
using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.ImportExport;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

namespace PlanAthena.Services.DataAccess
{
    public class ExportService
    {
        /// <summary>
        /// Exporte une liste de tâches au format CSV en utilisant le framework ChoETL et un DTO dédié.
        /// </summary>
        /// <param name="tachesAExporter">La liste des objets de domaine Tache à exporter.</param>
        /// <param name="filePath">Le chemin complet du fichier CSV de destination.</param>
        public void ExporterTachesCSV(List<Tache> tachesAExporter, string filePath)
        {
            // 1. Mapper la liste d'objets de domaine (Tache) vers la liste de DTOs (TacheExportDto)
            var tachesPourExport = tachesAExporter
                .Select(tache => new TacheExportDto(tache))
                .ToList();

            // 2. Utiliser ChoCSVWriter avec le type DTO pour la sérialisation.
            using (var writer = new ChoCSVWriter<TacheExportDto>(filePath)
                .WithFirstLineHeader()
                // MODIFIÉ : Utiliser la méthode Configure pour accéder à l'objet de configuration
                // et y définir l'encodage manuellement.
                .Configure(config => config.Encoding = Encoding.UTF8)
            )
            {
                writer.Write(tachesPourExport);
            }
        }
            /// <summary>
            /// Exporte une liste d'ouvriers et leurs compétences au format CSV.
            /// Un ouvrier avec plusieurs compétences générera plusieurs lignes.
            /// </summary>
            /// <param name="tousLesOuvriers">La liste des objets de domaine Ouvrier à exporter.</param>
            /// <param name="filePath">Le chemin complet du fichier CSV de destination.</param>
        public void ExporterOuvriersCSV(List<Ouvrier> tousLesOuvriers, string filePath)
        {
            // 1. Logique de transformation (mise à plat) des Ouvriers en DTOs.
            // C'est exactement la même logique que vous aviez avant.
            var recordsPourCsv = new List<OuvrierCsvRecord>();
            foreach (var ouvrier in tousLesOuvriers)
            {
                if (ouvrier.Competences.Any())
                {
                    foreach (var competence in ouvrier.Competences)
                    {
                        recordsPourCsv.Add(new OuvrierCsvRecord
                        {
                            OuvrierId = ouvrier.OuvrierId,
                            Nom = ouvrier.Nom,
                            Prenom = ouvrier.Prenom,
                            CoutJournalier = ouvrier.CoutJournalier,
                            MetierId = competence.MetierId
                        });
                    }
                }
                else // Gérer le cas où un ouvrier n'a aucune compétence
                {
                    recordsPourCsv.Add(new OuvrierCsvRecord
                    {
                        OuvrierId = ouvrier.OuvrierId,
                        Nom = ouvrier.Nom,
                        Prenom = ouvrier.Prenom,
                        CoutJournalier = ouvrier.CoutJournalier,
                        MetierId = "" // Ligne avec métier vide
                    });
                }
            }

            // 2. Écriture de la liste de DTOs avec ChoETL.
            using (var writer = new ChoCSVWriter<OuvrierCsvRecord>(filePath)
                .WithFirstLineHeader()
                .Configure(config => config.Encoding = Encoding.UTF8)
            )
            {
                writer.Write(recordsPourCsv);
            }
        }
    }
}