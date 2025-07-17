using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using System.Text.Json;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service de gestion des projets (sauvegarde/chargement complet)
    /// </summary>
    public class ProjetService
    {
        private readonly OuvrierService _ouvrierService;
        private readonly TacheService _tacheService;
        private readonly MetierService _metierService;
        private readonly CsvDataService _csvDataService;

        public ProjetService(
            OuvrierService ouvrierService,
            TacheService tacheService,
            MetierService metierService,
            CsvDataService csvDataService)
        {
            _ouvrierService = ouvrierService ?? throw new ArgumentNullException(nameof(ouvrierService));
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
        }
        public bool ValiderDonneesAvantPlanification(out string message)
        {
            var resumeTaches = _tacheService.ObtenirStatistiques();
            if (resumeTaches.NombreTachesTotal == 0)
            {
                message = "Aucune tâche chargée. Veuillez charger des tâches avant de lancer la planification.";
                return false;
            }

            var resumeOuvriers = _ouvrierService.ObtenirStatistiques();
            if (resumeOuvriers.NombreOuvriersTotal == 0)
            {
                message = "Aucun ouvrier chargé. Veuillez charger des ouvriers avant de lancer la planification.";
                return false;
            }

            if (_metierService.GetAllMetiers().Count == 0)
            {
                message = "Aucun métier chargé. Veuillez charger des métiers avant de lancer la planification.";
                return false;
            }

            message = "Validation réussie.";
            return true;
        }
        #region Sauvegarde/Chargement Projet

        /// <summary>
        /// Sauvegarde un projet complet au format JSON
        /// </summary>
        /// <param name="filePath">Chemin du fichier de sauvegarde</param>
        /// <param name="informationsProjet">Informations sur le projet</param>
        public void SauvegarderProjet(string filePath, InformationsProjet informationsProjet)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Le chemin du fichier ne peut pas être vide.");

            if (informationsProjet == null)
                throw new ArgumentNullException(nameof(informationsProjet));

            try
            {
                var projetData = new ProjetData
                {
                    InformationsProjet = informationsProjet,
                    Ouvriers = _ouvrierService.ObtenirTousLesOuvriers(),
                    Taches = _tacheService.ObtenirToutesLesTaches(),
                    Metiers = _metierService.GetAllMetiers().ToList(),
                    DateSauvegarde = DateTime.Now,
                    VersionApplication = "1.0.0" // TODO: Récupérer depuis AssemblyInfo
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonString = JsonSerializer.Serialize(projetData, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new ProjetException($"Erreur lors de la sauvegarde du projet: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Charge un projet complet depuis un fichier JSON
        /// </summary>
        /// <param name="filePath">Chemin du fichier de projet</param>
        /// <returns>Informations du projet chargé</returns>
        public InformationsProjet ChargerProjet(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Le fichier de projet '{filePath}' n'existe pas.");

            try
            {
                var jsonString = File.ReadAllText(filePath);
                var projetData = JsonSerializer.Deserialize<ProjetData>(jsonString);

                if (projetData == null)
                    throw new ProjetException("Le fichier de projet est invalide ou corrompu.");

                // Charger les données dans les services
                _metierService.RemplacerTousLesMetiers(projetData.Metiers);
                _ouvrierService.ChargerOuvriers(projetData.Ouvriers);
                _tacheService.ChargerTaches(projetData.Taches);

                return projetData.InformationsProjet;
            }
            catch (JsonException ex)
            {
                throw new ProjetException($"Erreur lors de la lecture du fichier JSON: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ProjetException($"Erreur lors du chargement du projet: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Valide qu'un fichier de projet est valide
        /// </summary>
        /// <param name="filePath">Chemin du fichier à valider</param>
        /// <returns>Résultat de la validation</returns>
        public ValidationProjet ValiderProjet(string filePath)
        {
            var validation = new ValidationProjet { EstValide = false };

            try
            {
                if (!File.Exists(filePath))
                {
                    validation.Erreurs.Add("Le fichier n'existe pas.");
                    return validation;
                }

                var jsonString = File.ReadAllText(filePath);
                var projetData = JsonSerializer.Deserialize<ProjetData>(jsonString);

                if (projetData == null)
                {
                    validation.Erreurs.Add("Le fichier JSON est invalide.");
                    return validation;
                }

                // Validation des données essentielles
                if (projetData.InformationsProjet == null)
                    validation.Erreurs.Add("Informations du projet manquantes.");

                if (projetData.Metiers == null || projetData.Metiers.Count == 0)
                    validation.Avertissements.Add("Aucun métier défini.");

                if (projetData.Ouvriers == null || projetData.Ouvriers.Count == 0)
                    validation.Avertissements.Add("Aucun ouvrier défini.");

                if (projetData.Taches == null || projetData.Taches.Count == 0)
                    validation.Avertissements.Add("Aucune tâche définie.");

                validation.EstValide = validation.Erreurs.Count == 0;
                validation.InformationsProjet = projetData.InformationsProjet;

                return validation;
            }
            catch (Exception ex)
            {
                validation.Erreurs.Add($"Erreur lors de la validation: {ex.Message}");
                return validation;
            }
        }

        #endregion

        #region Export/Import CSV groupé

        /// <summary>
        /// Exporte toutes les données vers des fichiers CSV séparés
        /// </summary>
        /// <param name="dossierDestination">Dossier de destination</param>
        /// <param name="prefixeNom">Préfixe pour les noms de fichiers</param>
        public void ExporterToutVersCsv(string dossierDestination, string prefixeNom = "export")
        {
            if (!Directory.Exists(dossierDestination))
                Directory.CreateDirectory(dossierDestination);

            try
            {
                // Export des métiers
                var cheminMetiers = Path.Combine(dossierDestination, $"{prefixeNom}_metiers.csv");
                _csvDataService.ExportCsv(_metierService.GetAllMetiers(), cheminMetiers);

                // Export des ouvriers
                var cheminOuvriers = Path.Combine(dossierDestination, $"{prefixeNom}_ouvriers.csv");
                _ouvrierService.ExporterVersCsv(cheminOuvriers);

                // Export des tâches
                var cheminTaches = Path.Combine(dossierDestination, $"{prefixeNom}_taches.csv");
                _tacheService.ExporterVersCsv(cheminTaches);
            }
            catch (Exception ex)
            {
                throw new ProjetException($"Erreur lors de l'export CSV: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Importe toutes les données depuis des fichiers CSV
        /// </summary>
        /// <param name="cheminMetiers">Chemin du fichier métiers</param>
        /// <param name="cheminOuvriers">Chemin du fichier ouvriers</param>
        /// <param name="cheminTaches">Chemin du fichier tâches</param>
        /// <returns>Résumé de l'import</returns>
        public ResumeImport ImporterToutDepuisCsv(string cheminMetiers, string cheminOuvriers, string cheminTaches)
        {
            var resume = new ResumeImport();

            try
            {
                // Import des métiers
                if (!string.IsNullOrEmpty(cheminMetiers) && File.Exists(cheminMetiers))
                {
                    var metiers = _csvDataService.ImportCsv<Metier>(cheminMetiers);
                    _metierService.RemplacerTousLesMetiers(metiers);
                    resume.MetiersImportes = metiers.Count;
                }

                // Import des ouvriers
                if (!string.IsNullOrEmpty(cheminOuvriers) && File.Exists(cheminOuvriers))
                {
                    resume.OuvriersImportes = _ouvrierService.ImporterDepuisCsv(cheminOuvriers);
                }

                // Import des tâches
                if (!string.IsNullOrEmpty(cheminTaches) && File.Exists(cheminTaches))
                {
                    resume.TachesImportees = _tacheService.ImporterDepuisCsv(cheminTaches);
                }

                resume.Succes = true;
                return resume;
            }
            catch (Exception ex)
            {
                resume.Succes = false;
                resume.MessageErreur = ex.Message;
                return resume;
            }
        }

        #endregion

        #region Utilitaires

        /// <summary>
        /// Crée un nouveau projet vide
        /// </summary>
        /// <param name="nomProjet">Nom du projet</param>
        /// <param name="description">Description du projet</param>
        /// <returns>Informations du nouveau projet</returns>
        public InformationsProjet CreerNouveauProjet(string nomProjet, string description = "")
        {
            // Vider toutes les données existantes
            _metierService.RemplacerTousLesMetiers(new List<Metier>());
            _ouvrierService.Vider();
            _tacheService.Vider();

            return new InformationsProjet
            {
                NomProjet = nomProjet,
                Description = description,
                DateCreation = DateTime.Now,
                DateDerniereModification = DateTime.Now,
                Auteur = Environment.UserName
            };
        }

        /// <summary>
        /// Obtient un résumé du projet actuel
        /// </summary>
        /// <returns>Résumé des données du projet</returns>
        public ResumeProjet ObtenirResumeProjet()
        {
            return new ResumeProjet
            {
                NombreMetiers = _metierService.GetAllMetiers().Count,
                StatistiquesOuvriers = _ouvrierService.ObtenirStatistiques(),
                StatistiquesTaches = _tacheService.ObtenirStatistiques(),
                StatistiquesMappingMetiers = _tacheService.ObtenirStatistiquesMappingMetiers()
            };
        }

        #endregion
    }

    #region Classes de support

    /// <summary>
    /// Informations générales sur un projet
    /// </summary>
    public class InformationsProjet
    {
        public string NomProjet { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DateCreation { get; set; }
        public DateTime DateDerniereModification { get; set; }
        public string Auteur { get; set; } = "";
    }

    /// <summary>
    /// Résultat de validation d'un projet
    /// </summary>
    public class ValidationProjet
    {
        public bool EstValide { get; set; }
        public List<string> Erreurs { get; set; } = new List<string>();
        public List<string> Avertissements { get; set; } = new List<string>();
        public InformationsProjet InformationsProjet { get; set; }
    }

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

    /// <summary>
    /// Résumé complet d'un projet
    /// </summary>
    public class ResumeProjet
    {
        public int NombreMetiers { get; set; }
        public StatistiquesOuvriers StatistiquesOuvriers { get; set; } = new StatistiquesOuvriers();
        public StatistiquesTaches StatistiquesTaches { get; set; } = new StatistiquesTaches();
        public StatistiquesMappingMetiers StatistiquesMappingMetiers { get; set; } = new StatistiquesMappingMetiers();
    }

    /// <summary>
    /// Exception spécifique aux erreurs de projet
    /// </summary>
    public class ProjetException : Exception
    {
        public ProjetException(string message) : base(message) { }
        public ProjetException(string message, Exception innerException) : base(message, innerException) { }
    }


    #endregion
}
