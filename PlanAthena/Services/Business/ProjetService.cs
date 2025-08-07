using Microsoft.VisualBasic.ApplicationServices;
using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using QuikGraph;
using QuikGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Documents;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service de gestion des projets (sauvegarde/chargement complet)
    /// 🔄 VERSION V0.4.2 - Système métiers avec prérequis par phase
    /// </summary>
    public class ProjetService
    {
        private readonly OuvrierService _ouvrierService;
        private readonly Func<TacheService> _tacheServiceFactory;
        private readonly CsvDataService _csvDataService;
        private readonly Func<BlocService> _blocServiceFactory;

        // Collection interne de métiers, gérée directement par ProjetService
        private readonly Dictionary<string, Metier> _metiersInternes = new Dictionary<string, Metier>();

        // AJOUT: Collection interne de lots, fusionnée de ProjetService
        private readonly Dictionary<string, Lot> _lotsInternes = new Dictionary<string, Lot>();

        // Champs liés aux couleurs de fallback des métiers, déplacés de MetierService
        private static readonly Color[] FallbackColors = {
            Color.LightBlue, Color.LightGreen, Color.LightYellow,
            Color.LightPink, Color.LightGray, Color.LightCyan,
            Color.LightSalmon
        };
        private int _fallbackColorIndex = 0;
        private readonly Dictionary<string, Color> _assignedFallbackColors = new Dictionary<string, Color>();

        public ProjetService(
            OuvrierService ouvrierService,
            Func<TacheService> tacheServiceFactory,
            CsvDataService csvDataService,
            Func<BlocService> blocServiceFactory)
        {
            _ouvrierService = ouvrierService ?? throw new ArgumentNullException(nameof(ouvrierService));
            _tacheServiceFactory = tacheServiceFactory ?? throw new ArgumentNullException(nameof(tacheServiceFactory));
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _blocServiceFactory = blocServiceFactory ?? throw new ArgumentNullException(nameof(blocServiceFactory));
        }

        private TacheService _tacheService => _tacheServiceFactory();
        private BlocService _blocService => _blocServiceFactory();

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

            if (GetAllMetiers().Count == 0)
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
                    Metiers = GetAllMetiers().ToList(),
                    Ouvriers = _ouvrierService.ObtenirTousLesOuvriers(),
                    Taches = _tacheService.ObtenirToutesLesTaches(),
                    Lots = ObtenirTousLesLots(),
                    Blocs = _blocService.ObtenirTousLesBlocs(),
                    DateSauvegarde = DateTime.Now,
                    VersionApplication = "0.4.2"
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

                return ChargerNouveauProjet(jsonNode);
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
            RemplacerTousLesMetiers(projetData.Metiers);
            _ouvrierService.ChargerOuvriers(projetData.Ouvriers);
            RemplacerTousLesLots(projetData.Lots);
            _blocService.RemplacerTousLesBlocs(projetData.Blocs);
            _tacheService.ChargerTaches(projetData.Taches);

            return projetData.InformationsProjet;
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

        #region CRUD Operations - Lots (fusionné de ProjetService)

        /// <summary>
        /// Ajoute un nouveau lot au projet.
        /// </summary>
        public void AjouterLot(Lot lot)
        {
            if (lot == null) throw new ArgumentNullException(nameof(lot));
            if (string.IsNullOrWhiteSpace(lot.LotId)) throw new ArgumentException("L'ID du lot ne peut pas être vide.");
            if (_lotsInternes.ContainsKey(lot.LotId)) throw new InvalidOperationException($"Un lot avec l'ID '{lot.LotId}' existe déjà.");
            _lotsInternes.Add(lot.LotId, lot);
        }

        /// <summary>
        /// Modifie un lot existant.
        /// </summary>
        public void ModifierLot(Lot lotModifie)
        {
            if (lotModifie == null) throw new ArgumentNullException(nameof(lotModifie));
            if (!_lotsInternes.ContainsKey(lotModifie.LotId)) throw new KeyNotFoundException($"Lot {lotModifie.LotId} non trouvé.");
            _lotsInternes[lotModifie.LotId] = lotModifie;
        }

        /// <summary>
        /// Obtient un lot par son ID.
        /// </summary>
        public Lot ObtenirLotParId(string lotId)
        {
            _lotsInternes.TryGetValue(lotId, out var lot);
            return lot;
        }

        /// <summary>
        /// Retourne tous les lots triés par priorité puis nom.
        /// </summary>
        public List<Lot> ObtenirTousLesLots()
        {
            return _lotsInternes.Values.OrderBy(l => l.Priorite).ThenBy(l => l.Nom).ToList();
        }

        /// <summary>
        /// Supprime un lot du projet.
        /// TODO: Valider qu'aucune tâche n'utilise ce lot.
        /// </summary>
        public void SupprimerLot(string lotId)
        {
            if (!_lotsInternes.Remove(lotId))
            {
                throw new KeyNotFoundException($"Lot {lotId} non trouvé.");
            }
        }

        /// <summary>
        /// Remplace tous les lots existants par une nouvelle liste.
        /// </summary>
        public void RemplacerTousLesLots(List<Lot> lots)
        {
            _lotsInternes.Clear();
            if (lots != null)
            {
                foreach (var lot in lots)
                {
                    if (!_lotsInternes.ContainsKey(lot.LotId))
                    {
                        _lotsInternes.Add(lot.LotId, lot);
                    }
                }
            }
        }

        /// <summary>
        /// Vide tous les lots du projet.
        /// </summary>
        public void ViderLots()
        {
            _lotsInternes.Clear();
        }

        #endregion

        #region CRUD Operations - Métiers V0.4.2 (🔄 MIS À JOUR)

        /// <summary>
        /// Ajoute un nouveau métier au projet.
        /// </summary>
        public void AjouterMetier(Metier nouveauMetier)
        {
            if (nouveauMetier == null)
                throw new ArgumentNullException(nameof(nouveauMetier));
            if (string.IsNullOrWhiteSpace(nouveauMetier.MetierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(nouveauMetier.MetierId));
            if (_metiersInternes.ContainsKey(nouveauMetier.MetierId))
                throw new InvalidOperationException($"Un métier avec l'ID '{nouveauMetier.MetierId}' existe déjà.");

            _metiersInternes.Add(nouveauMetier.MetierId, nouveauMetier);
        }

        /// <summary>
        /// 🔄 MODIFIÉ V0.4.2 : Support Dictionary PrerequisParPhase
        /// Utilisé par: MetierForm pour sauvegarder précédences par phase
        /// </summary>
        public void ModifierMetier(string metierId, string nom,
            Dictionary<ChantierPhase, List<string>> prerequisParPhase,
            string couleurHex = null, string pictogram = null, ChantierPhase? phases = null)
        {
            if (!_metiersInternes.TryGetValue(metierId, out var metierAModifier))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            metierAModifier.Nom = nom;

            // 🆕 NOUVEAU : Gestion Dictionary PrerequisParPhase
            metierAModifier.PrerequisParPhase = prerequisParPhase ?? new Dictionary<ChantierPhase, List<string>>();

            if (couleurHex != null)
                metierAModifier.CouleurHex = couleurHex;

            if (pictogram != null)
                metierAModifier.Pictogram = pictogram;

            if (phases.HasValue)
                metierAModifier.Phases = phases.Value;
        }

        /// <summary>
        /// 🆕 NOUVEAU V0.4.2 : Support création métiers spécifiques (amiante, nucléaire, QSE)
        /// Utilisé par: MetierForm Concept 1 pour métiers rares
        /// 🔄 TODO V0.5 : Déléguer génération ID à IdMetierGeneratorService
        /// </summary>
        /// <param name="nom">Nom du métier spécifique</param>
        /// <param name="phases">Phases d'intervention</param>
        /// <param name="couleurHex">Couleur personnalisée (optionnel)</param>
        /// <returns>ID généré pour le nouveau métier</returns>
        public string AjouterMetierSpecifique(string nom, ChantierPhase phases, string couleurHex = null)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom du métier ne peut pas être vide.", nameof(nom));

            if (phases == ChantierPhase.None)
                throw new ArgumentException("Au moins une phase d'intervention doit être spécifiée.", nameof(phases));

            // 🔄 TEMPORAIRE V0.4.2 : Génération ID locale (à migrer vers IdMetierGeneratorService V0.5)
            var nomNormalise = nom.ToUpperInvariant()
                .Replace(" ", "_")
                .Replace("É", "E")
                .Replace("È", "E")
                .Replace("Ê", "E");

            var prefixe = $"M_{nomNormalise}_";
            var compteur = 1;
            string idCandidat;

            do
            {
                idCandidat = $"{prefixe}{compteur:D3}";
                compteur++;
            } while (_metiersInternes.ContainsKey(idCandidat));

            var nouveauMetier = new Metier
            {
                MetierId = idCandidat,
                Nom = nom,
                Phases = phases,
                CouleurHex = couleurHex ?? "",
                PrerequisParPhase = new Dictionary<ChantierPhase, List<string>>()
            };

            AjouterMetier(nouveauMetier);
            return idCandidat;
        }

        /// <summary>
        /// Supprime un métier du projet.
        /// 🔄 MODIFIÉ V0.4.2 : Mise à jour nettoyage prérequis avec nouvelle structure
        /// </summary>
        public void SupprimerMetier(string metierId)
        {
            if (!_metiersInternes.Remove(metierId))
                throw new KeyNotFoundException($"Le métier avec l'ID '{metierId}' n'a pas été trouvé.");

            // Supprimer ce métier des prérequis des autres métiers
            foreach (var metier in _metiersInternes.Values)
            {
                var prerequisParPhaseModifies = new Dictionary<ChantierPhase, List<string>>();

                foreach (var (phase, prerequisPhase) in metier.PrerequisParPhase)
                {
                    var prerequisNettoyes = prerequisPhase.Where(id => id != metierId).ToList();
                    if (prerequisNettoyes.Count > 0)
                    {
                        prerequisParPhaseModifies[phase] = prerequisNettoyes;
                    }
                }

                metier.PrerequisParPhase = prerequisParPhaseModifies;
            }
        }

        #endregion

        #region Data Loading and Retrieval - Métiers V0.4.2 (🔄 MIS À JOUR)

        /// <summary>
        /// Remplace tous les métiers existants par une nouvelle liste.
        /// </summary>
        public void RemplacerTousLesMetiers(IReadOnlyList<Metier> metiers)
        {
            _metiersInternes.Clear();
            if (metiers != null)
            {
                foreach (var metier in metiers)
                {
                    if (!string.IsNullOrEmpty(metier.MetierId) && !_metiersInternes.ContainsKey(metier.MetierId))
                    {
                        _metiersInternes.Add(metier.MetierId, metier);
                    }
                }
            }
        }

        /// <summary>
        /// Retourne tous les métiers du projet.
        /// </summary>
        public IReadOnlyList<Metier> GetAllMetiers()
        {
            return _metiersInternes.Values.ToList();
        }

        /// <summary>
        /// Retourne un métier par son ID.
        /// </summary>
        public Metier GetMetierById(string metierId)
        {
            if (string.IsNullOrEmpty(metierId))
            {
                return null;
            }

            _metiersInternes.TryGetValue(metierId, out var metier);
            return metier;
        }

        /// <summary>
        /// 🆕 V0.4.2.1 - Obtient les prérequis pour une phase SPÉCIFIQUE.
        /// C'est la méthode à utiliser par défaut pour respecter la logique par phase.
        /// </summary>
        /// <param name="metierId">L'ID du métier</param>
        /// <param name="phase">La phase de chantier concernée. Ne peut pas être null.</param>
        /// <returns>Une liste d'IDs de métiers prérequis pour cette phase précise.</returns>
        public List<string> GetPrerequisPourPhase(string metierId, ChantierPhase phase)
        {
            var metier = GetMetierById(metierId);
            if (metier?.PrerequisParPhase != null && metier.PrerequisParPhase.TryGetValue(phase, out var prerequis))
            {
                return prerequis.ToList(); // Retourne la liste des prérequis pour la phase
            }
            return new List<string>(); // Retourne une liste vide si pas de prérequis pour cette phase
        }

        /// <summary>
        /// 🆕 V0.4.2.1 - Obtient TOUS les prérequis d'un métier, toutes phases confondues.
        /// À utiliser avec précaution, principalement pour la validation de cycles globaux.
        /// </summary>
        /// <param name="metierId">L'ID du métier</param>
        /// <returns>Une liste unique de tous les IDs de prérequis, peu importe la phase.</returns>
        public List<string> GetTousPrerequisConfondus(string metierId)
        {
            var metier = GetMetierById(metierId);
            if (metier?.PrerequisParPhase == null)
            {
                return new List<string>();
            }

            // Aplatit les listes de toutes les phases et supprime les doublons
            return metier.PrerequisParPhase.Values
                .SelectMany(prereqs => prereqs)
                .Distinct()
                .ToList();
        }


        #endregion


        #region Couleurs - Métiers (déplacé de MetierService)

        /// <summary>
        /// Obtient la couleur d'affichage pour un métier.
        /// Priorité 1: Utilise la couleur personnalisée si elle est valide.
        /// Priorité 2: Attribue et mémorise une couleur de fallback unique.
        /// </summary>
        public Color GetDisplayColorForMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId))
            {
                return Color.MistyRose; // Couleur pour "non assigné"
            }

            var metier = GetMetierById(metierId);

            // Priorité 1: Couleur personnalisée
            if (metier != null && !string.IsNullOrEmpty(metier.CouleurHex))
            {
                try
                {
                    return ColorTranslator.FromHtml(metier.CouleurHex);
                }
                catch
                {
                    // La couleur est malformée, on passe au fallback
                }
            }

            // Priorité 2: Couleur de fallback
            if (!_assignedFallbackColors.ContainsKey(metierId))
            {
                _assignedFallbackColors[metierId] = FallbackColors[_fallbackColorIndex % FallbackColors.Length];
                _fallbackColorIndex++;
            }
            return _assignedFallbackColors[metierId];
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
            RemplacerTousLesMetiers(new List<Metier>());
            _ouvrierService.Vider();
            _tacheService.Vider();
            ViderLots();
            _blocService.Vider();

            // Appel à ChargerMetiersParDefaut pour initialiser les métiers
            ChargerMetiersParDefaut();

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
                NombreMetiers = GetAllMetiers().Count,
                StatistiquesOuvriers = _ouvrierService.ObtenirStatistiques(),
                StatistiquesTaches = _tacheService.ObtenirStatistiques(),
                StatistiquesMappingMetiers = _tacheService.ObtenirStatistiquesMappingMetiers()
            };
        }

        /// <summary>
        /// Charge les métiers par défaut depuis le fichier de configuration.
        /// 🔄 MODIFIÉ V0.4.2 : Support migration automatique PrerequisMetierIds → PrerequisParPhase
        /// 🔄 TODO V0.5 : Déléguer à IdMetierGeneratorService + config loader dédié
        /// </summary>
        private void ChargerMetiersParDefaut()
        {
            // Ne charger que si aucun métier n'est déjà là (pour les nouveaux projets)
            if (_metiersInternes.Any()) return;

            try
            {
                // Chemin du fichier de configuration à côté de l'exécutable
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "DefaultMetiersConfig.json");

                if (!File.Exists(filePath))
                {
                    return;
                }

                string json = File.ReadAllText(filePath);
                var defaultMetiers = JsonSerializer.Deserialize<List<Metier>>(json);

                if (defaultMetiers != null)
                {
                    // 🆕 V0.4.2 : Migration automatique pour les métiers de DefaultMetiersConfig.json
                    // Les métiers avec ancienne propriété PrerequisMetierIds sont automatiquement migrés
                    // grâce au setter de la propriété helper Metier.PrerequisMetierIds
                    RemplacerTousLesMetiers(defaultMetiers);
                }
            }
            catch (Exception ex)
            {
                // Gérer l'erreur de chargement des métiers par défaut (log, message d'erreur si critique)
                Console.WriteLine($"Erreur lors du chargement des métiers par défaut: {ex.Message}");
            }
        }

        #endregion
    }
}