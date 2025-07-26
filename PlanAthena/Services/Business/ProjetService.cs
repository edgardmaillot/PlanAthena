using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        private readonly LotService _lotService;
        private readonly BlocService _blocService;

        public ProjetService(
            OuvrierService ouvrierService,
            TacheService tacheService,
            MetierService metierService,
            CsvDataService csvDataService,
            LotService lotService,
            BlocService blocService)
        {
            _ouvrierService = ouvrierService ?? throw new ArgumentNullException(nameof(ouvrierService));
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
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
                    Metiers = _metierService.GetAllMetiers().ToList(),
                    Ouvriers = _ouvrierService.ObtenirTousLesOuvriers(),
                    Taches = _tacheService.ObtenirToutesLesTaches(),
                    Lots = _lotService.ObtenirTousLesLots(),
                    Blocs = _blocService.ObtenirTousLesBlocs(),
                    DateSauvegarde = DateTime.Now,
                    VersionApplication = "0.2.2"
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
        /// Charge un projet complet depuis un fichier JSON.
        /// Gère de manière transparente la migration depuis l'ancien format de fichier.
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
                var jsonNode = JsonNode.Parse(jsonString);

                if (jsonNode == null)
                    throw new ProjetException("Le fichier de projet est invalide ou corrompu.");

                // Détecter si c'est un ancien format (absence de la propriété "Lots")
                if (jsonNode["Lots"] == null)
                {
                    return ChargerAncienProjet(jsonNode);
                }
                else
                {
                    return ChargerNouveauProjet(jsonNode);
                }
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

        private InformationsProjet ChargerNouveauProjet(JsonNode jsonNode)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var projetData = jsonNode.Deserialize<ProjetData>(options);
            if (projetData == null)
                throw new ProjetException("Le fichier de projet est invalide ou corrompu.");

            // Charger les données dans l'ordre de dépendance
            _metierService.RemplacerTousLesMetiers(projetData.Metiers);
            _ouvrierService.ChargerOuvriers(projetData.Ouvriers);
            _lotService.RemplacerTousLesLots(projetData.Lots);
            _blocService.RemplacerTousLesBlocs(projetData.Blocs);
            _tacheService.ChargerTaches(projetData.Taches);

            return projetData.InformationsProjet;
        }

        private InformationsProjet ChargerAncienProjet(JsonNode jsonNode)
        {
            // Classe temporaire pour désérialiser l'ancienne structure de tâche
            // uniquement utilisée pour la migration.
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var oldProjectData = jsonNode.Deserialize<OldProjetDataFormat>(options);

            if (oldProjectData == null)
                throw new ProjetException("L'ancien format de projet n'a pas pu être lu.");

            // Étape 1: Déduire et créer les lots et blocs uniques
            var lotsUniques = oldProjectData.Taches
                .Where(t => !string.IsNullOrEmpty(t.LotId))
                .GroupBy(t => t.LotId)
                .Select(g => new Lot
                {
                    LotId = g.Key,
                    Nom = g.First().LotNom,
                    Priorite = g.First().LotPriorite,
                    CheminFichierPlan = "" // Information non disponible dans l'ancien format
                }).ToList();

            var blocsUniques = oldProjectData.Taches
                .Where(t => !string.IsNullOrEmpty(t.BlocId))
                .GroupBy(t => t.BlocId)
                .Select(g => new Bloc
                {
                    BlocId = g.Key,
                    Nom = g.First().BlocNom,
                    CapaciteMaxOuvriers = g.First().BlocCapaciteMaxOuvriers
                }).ToList();

            // Étape 2: Créer les nouvelles tâches avec la structure épurée
            var nouvellesTaches = oldProjectData.Taches.Select(oldTache => new Tache
            {
                TacheId = oldTache.TacheId,
                TacheNom = oldTache.TacheNom,
                HeuresHommeEstimees = oldTache.HeuresHommeEstimees,
                MetierId = oldTache.MetierId,
                Dependencies = oldTache.Dependencies,
                ExclusionsDependances = oldTache.ExclusionsDependances,
                Type = oldTache.Type,
                LotId = oldTache.LotId,
                BlocId = oldTache.BlocId
            }).ToList();

            // Étape 3: Charger toutes les données dans les services
            _metierService.RemplacerTousLesMetiers(oldProjectData.Metiers);
            _ouvrierService.ChargerOuvriers(oldProjectData.Ouvriers);
            _lotService.RemplacerTousLesLots(lotsUniques);
            _blocService.RemplacerTousLesBlocs(blocsUniques);
            _tacheService.ChargerTaches(nouvellesTaches);

            return oldProjectData.InformationsProjet;
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

                // Pour la validation, nous ne nous soucions pas du format (ancien/nouveau)
                // tant que les informations de base sont présentes.
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

        #region Export/Import CSV groupé (Non fonctionnel - à traiter dans une phase ultérieure)

        /// <summary>
        /// Exporte toutes les données vers des fichiers CSV séparés
        /// </summary>
        public void ExporterToutVersCsv(string dossierDestination, string prefixeNom = "export")
        {
            throw new NotImplementedException("La fonctionnalité d'export CSV doit être revue pour la nouvelle structure de données.");
        }

        /// <summary>
        /// Importe toutes les données depuis des fichiers CSV
        /// </summary>
        public ResumeImport ImporterToutDepuisCsv(string cheminMetiers, string cheminOuvriers, string cheminTaches)
        {
            throw new NotImplementedException("La fonctionnalité d'import CSV doit être revue pour la nouvelle structure de données.");
        }

        #endregion

        #region Utilitaires

        /// <summary>
        /// Crée un nouveau projet vide
        /// </summary>
        public InformationsProjet CreerNouveauProjet(string nomProjet, string description = "")
        {
            // Vider toutes les données existantes
            _metierService.RemplacerTousLesMetiers(new List<Metier>());
            _ouvrierService.Vider();
            _tacheService.Vider();
            _lotService.Vider();
            _blocService.Vider();

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

    #region Classes de support pour la migration

    /// <summary>
    /// DTO temporaire utilisé uniquement pour la migration d'anciens fichiers projet.
    /// </summary>
    internal class OldTacheFormat
    {
        public string TacheId { get; set; }
        public string TacheNom { get; set; }
        public int HeuresHommeEstimees { get; set; }
        public string MetierId { get; set; }
        public string Dependencies { get; set; }
        public string ExclusionsDependances { get; set; }
        public TypeActivite Type { get; set; }
        public string LotId { get; set; }
        public string LotNom { get; set; }
        public int LotPriorite { get; set; }
        public string BlocId { get; set; }
        public string BlocNom { get; set; }
        public int BlocCapaciteMaxOuvriers { get; set; }
    }

    internal class OldProjetDataFormat
    {
        public InformationsProjet InformationsProjet { get; set; }
        public List<Metier> Metiers { get; set; }
        public List<Ouvrier> Ouvriers { get; set; }
        public List<OldTacheFormat> Taches { get; set; }
    }

    #endregion
}