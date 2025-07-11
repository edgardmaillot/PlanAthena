using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.CsvModels;
using PlanAthena.Services.DataAccess;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Exception spécifique aux erreurs de planification
    /// </summary>
    public class PlanificationException : Exception
    {
        public PlanificationException(string message) : base(message) { }
        public PlanificationException(string message, Exception innerException) : base(message, innerException) { }
    }
    /// <summary>
    /// Service d'orchestration pour la planification des chantiers
    /// </summary>
    public class PlanificationService
    {
        private readonly PlanAthenaCoreFacade _facade;
        private readonly DataTransformer _dataTransformer;

        // Données pour la planification (injectées par les autres services)
        private List<OuvrierCsvRecord> _ouvriers = new List<OuvrierCsvRecord>();
        private List<TacheCsvRecord> _taches = new List<TacheCsvRecord>();
        private List<MetierCsvRecord> _metiers = new List<MetierCsvRecord>();

        public PlanificationService(
            PlanAthenaCoreFacade facade,
            DataTransformer dataTransformer)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _dataTransformer = dataTransformer ?? throw new ArgumentNullException(nameof(dataTransformer));
        }

        /// <summary>
        /// Charge les données depuis les autres services pour la planification
        /// </summary>
        /// <param name="ouvriers">Ouvriers depuis OuvrierService</param>
        /// <param name="taches">Tâches depuis TacheService</param>
        /// <param name="metiers">Métiers depuis MetierService</param>
        public void ChargerDonnees(
            List<OuvrierCsvRecord> ouvriers,
            List<TacheCsvRecord> taches,
            List<MetierCsvRecord> metiers)
        {
            _ouvriers = ouvriers ?? new List<OuvrierCsvRecord>();
            _taches = taches ?? new List<TacheCsvRecord>();
            _metiers = metiers ?? new List<MetierCsvRecord>();
        }

        /// <summary>
        /// Valide que toutes les données nécessaires sont chargées
        /// </summary>
        /// <returns>True si toutes les données sont présentes</returns>
        public bool ValiderDonneesChargees()
        {
            return _ouvriers.Any() && _metiers.Any() && _taches.Any();
        }

        /// <summary>
        /// Obtient le statut des données chargées
        /// </summary>
        /// <returns>Objet contenant les informations de statut</returns>
        public StatutDonnees ObtenirStatutDonnees()
        {
            return new StatutDonnees
            {
                NombreOuvriers = _ouvriers.Count,
                NombreMetiers = _metiers.Count,
                NombreTaches = _taches.Count,
                DonneesCompletes = ValiderDonneesChargees()
            };
        }

        /// <summary>
        /// Lance la planification avec PlanAthena
        /// </summary>
        /// <param name="configuration">Configuration de planification</param>
        /// <returns>Résultat de la planification</returns>
        public async Task<ProcessChantierResultDto> LancerPlanificationAsync(ConfigurationUI configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (!ValiderDonneesChargees())
                throw new PlanificationException("Toutes les données (ouvriers, métiers, tâches) doivent être chargées avant de lancer la planification.");

            try
            {
                // Créer un MetierService temporaire pour DataTransformer
                var metierServiceTemp = new MetierService();
                metierServiceTemp.ChargerMetiers(_metiers);

                // Créer un DataTransformer temporaire
                var dataTransformer = new DataTransformer(metierServiceTemp);

                // Transformation des données
                var inputDto = dataTransformer.TransformToChantierSetupDto(_ouvriers, _taches, configuration);

                // Appel à PlanAthena
                var resultat = await _facade.ProcessChantierAsync(inputDto);

                return resultat;
            }
            catch (Exception ex)
            {
                throw new PlanificationException($"Erreur lors de la planification: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Efface toutes les données chargées
        /// </summary>
        public void EffacerDonnees()
        {
            _ouvriers.Clear();
            _taches.Clear();
            _metiers.Clear();
        }

        #region Méthodes de compatibilité (pour la transition)

        /// <summary>
        /// Importe les ouvriers depuis un fichier CSV (méthode de compatibilité)
        /// </summary>
        /// <param name="filePath">Chemin vers le fichier CSV des ouvriers</param>
        /// <returns>Nombre d'ouvriers importés</returns>
        [Obsolete("Utiliser OuvrierService.ImporterDepuisCsv() à la place")]
        public int ImporterOuvriers(string filePath)
        {
            try
            {
                var csvDataService = new CsvDataService();
                _ouvriers = csvDataService.ImportCsv<OuvrierCsvRecord>(filePath);
                return _ouvriers.Count;
            }
            catch (Exception ex)
            {
                throw new PlanificationException($"Erreur lors de l'import des ouvriers: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Importe les métiers depuis un fichier CSV (méthode de compatibilité)
        /// </summary>
        /// <param name="filePath">Chemin vers le fichier CSV des métiers</param>
        /// <returns>Nombre de métiers importés</returns>
        [Obsolete("Utiliser MetierService directement à la place")]
        public int ImporterMetiers(string filePath)
        {
            try
            {
                var csvDataService = new CsvDataService();
                _metiers = csvDataService.ImportCsv<MetierCsvRecord>(filePath);
                return _metiers.Count;
            }
            catch (Exception ex)
            {
                throw new PlanificationException($"Erreur lors de l'import des métiers: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Importe les tâches depuis un fichier CSV (méthode de compatibilité)
        /// </summary>
        /// <param name="filePath">Chemin vers le fichier CSV des tâches</param>
        /// <returns>Nombre de tâches importées</returns>
        [Obsolete("Utiliser TacheService.ImporterDepuisCsv() à la place")]
        public int ImporterTaches(string filePath)
        {
            try
            {
                var csvDataService = new CsvDataService();
                _taches = csvDataService.ImportCsv<TacheCsvRecord>(filePath);
                return _taches.Count;
            }
            catch (Exception ex)
            {
                throw new PlanificationException($"Erreur lors de l'import des tâches: {ex.Message}", ex);
            }
        }

        #endregion
    }

    /// <summary>
    /// Classe représentant le statut des données chargées
    /// </summary>
    public class StatutDonnees
    {
        public int NombreOuvriers { get; set; }
        public int NombreMetiers { get; set; }
        public int NombreTaches { get; set; }
        public bool DonneesCompletes { get; set; }

        public override string ToString()
        {
            return $"Ouvriers: {NombreOuvriers}, Métiers: {NombreMetiers}, Tâches: {NombreTaches} - " +
                   (DonneesCompletes ? "Complet" : "Incomplet");
        }
    }
}