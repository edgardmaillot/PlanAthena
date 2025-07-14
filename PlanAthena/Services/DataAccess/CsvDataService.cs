using CsvHelper.Configuration;
using System.Globalization;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service générique pour l'import/export de fichiers CSV
    /// </summary>
    public class CsvDataService
    {
        /// <summary>
        /// Importe un fichier CSV et retourne une liste d'objets du type spécifié
        /// </summary>
        /// <typeparam name="T">Type des objets à créer</typeparam>
        /// <param name="filePath">Chemin vers le fichier CSV</param>
        /// <returns>Liste des objets importés</returns>
        /// <exception cref="FileNotFoundException">Fichier non trouvé</exception>
        /// <exception cref="CsvImportException">Erreur lors de l'import CSV</exception>
        public List<T> ImportCsv<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Le fichier CSV '{filePath}' n'existe pas.");
            }

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = true,
                    HeaderValidated = null,
                    MissingFieldFound = null,
                };

                using var reader = new StreamReader(filePath);
                using var csvReader = new CsvHelper.CsvReader(reader, config);
                var records = csvReader.GetRecords<T>().ToList();

                return records;
            }
            catch (Exception ex)
            {
                throw new CsvImportException($"Erreur lors de l'import du fichier CSV '{filePath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exporte une liste d'objets vers un fichier CSV
        /// </summary>
        /// <typeparam name="T">Type des objets à exporter</typeparam>
        /// <param name="data">Données à exporter</param>
        /// <param name="filePath">Chemin vers le fichier CSV de destination</param>
        /// <exception cref="CsvExportException">Erreur lors de l'export CSV</exception>
        public void ExportCsv<T>(IEnumerable<T> data, string filePath)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };

                using var writer = new StreamWriter(filePath);
                using var csvWriter = new CsvHelper.CsvWriter(writer, config);
                csvWriter.WriteRecords(data);
            }
            catch (Exception ex)
            {
                throw new CsvExportException($"Erreur lors de l'export vers le fichier CSV '{filePath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Valide qu'un fichier CSV peut être lu avec le type spécifié
        /// </summary>
        /// <typeparam name="T">Type attendu</typeparam>
        /// <param name="filePath">Chemin vers le fichier</param>
        /// <returns>True si le fichier est valide</returns>
        public bool ValidateCsvStructure<T>(string filePath)
        {
            try
            {
                var records = ImportCsv<T>(filePath);
                return records != null;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Exception levée lors d'erreurs d'import CSV
    /// </summary>
    public class CsvImportException : Exception
    {
        public CsvImportException(string message) : base(message) { }
        public CsvImportException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception levée lors d'erreurs d'export CSV
    /// </summary>
    public class CsvExportException : Exception
    {
        public CsvExportException(string message) : base(message) { }
        public CsvExportException(string message, Exception innerException) : base(message, innerException) { }
    }
}