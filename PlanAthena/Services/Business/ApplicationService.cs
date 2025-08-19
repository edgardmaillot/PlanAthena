using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.Services.Business
{
    public class ApplicationService
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly ProjetRepository _projetRepository;
        private readonly CheminsPrefereService _cheminsPrefereService;

        public ProjetData ProjetActif { get; private set; }
        public string CheminFichierProjetActif { get; private set; }
        public ConfigurationPlanification ConfigPlanificationActuelle { get; private set; }

        public ApplicationService(
            ProjetService projetService,
            RessourceService ressourceService,
            ProjetRepository projetRepository,
            CheminsPrefereService cheminsPrefereService)
        {
            _projetService = projetService;
            _ressourceService = ressourceService;
            _projetRepository = projetRepository;
            _cheminsPrefereService = cheminsPrefereService;

            // Initialiser la configuration de session avec des valeurs par défaut
            InitialiserConfigurationParDefaut();
        }

        private void InitialiserConfigurationParDefaut()
        {
            ConfigPlanificationActuelle = new ConfigurationPlanification
            {
                JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                HeureDebutJournee = 8,
                DureeJournaliereStandardHeures = 8,
                HeuresTravailEffectifParJour = 7,
                CoutIndirectJournalierAbsolu = 500
            };
        }

        public void ChargerProjetDepuisFichier(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("Le fichier de projet spécifié est introuvable.", filePath);

            var projetData = _projetRepository.Charger(filePath);
            _projetService.ChargerProjet(projetData);
            _ressourceService.ChargerRessources(projetData.Metiers, projetData.Ouvriers);

            ProjetActif = projetData;
            CheminFichierProjetActif = filePath;

            // Quand on charge un projet, on met à jour la configuration de session
            // avec les valeurs du projet, qui font autorité.
            ConfigPlanificationActuelle.HeureDebutJournee = projetData.InformationsProjet.HeureOuverture;
            ConfigPlanificationActuelle.DureeJournaliereStandardHeures = projetData.InformationsProjet.HeureFermeture - projetData.InformationsProjet.HeureOuverture;
            ConfigPlanificationActuelle.HeuresTravailEffectifParJour = projetData.InformationsProjet.DureeTravailHeures;
            ConfigPlanificationActuelle.CoutIndirectJournalierAbsolu = (long)projetData.InformationsProjet.CoutJournalier;
            // NOTE: Les jours ouvrés ne sont pas persistés, donc on les garde tels quels dans la session.

            _cheminsPrefereService.SauvegarderDernierDossier(TypeOperation.ProjetChargement, filePath);
        }

        public void CreerNouveauProjet()
        {
            _projetService.InitialiserNouveauProjet();
            _ressourceService.ChargerMetiersParDefaut();

            ProjetActif = new ProjetData
            {
                InformationsProjet = new InformationsProjet { NomProjet = "Nouveau Projet" }
            };
            CheminFichierProjetActif = null;

            // On réinitialise aussi la configuration de planification à ses valeurs par défaut.
            InitialiserConfigurationParDefaut();
        }

        public void SauvegarderProjetActuel()
        {
            if (ProjetActif == null)
            {
                throw new InvalidOperationException("Aucun projet actif à sauvegarder.");
            }

            string finalPath = CheminFichierProjetActif;

            // Si aucun chemin n'est connu, demander à l'utilisateur
            if (string.IsNullOrEmpty(finalPath))
            {
                using var sfd = new SaveFileDialog
                {
                    InitialDirectory = _cheminsPrefereService.ObtenirDernierDossierProjets(),
                    Filter = "Fichiers projet (*.json)|*.json",
                    Title = "Sauvegarder le projet sous...",
                    FileName = $"{ProjetActif.InformationsProjet.NomProjet}.json"
                };
                if (sfd.ShowDialog() != DialogResult.OK) return; // L'utilisateur a annulé
                finalPath = sfd.FileName;
            }

            // 1. Récupérer les données de la structure depuis ProjetService
            var projetDataPourSauvegarde = _projetService.GetProjetDataPourSauvegarde();

            // --- CORRECTION ---
            // 2. Récupérer les données des ressources depuis RessourceService
            projetDataPourSauvegarde.Metiers = _ressourceService.GetAllMetiers();
            projetDataPourSauvegarde.Ouvriers = _ressourceService.GetAllOuvriers();
            // --- FIN DE LA CORRECTION ---

            // 3. Ajouter les méta-données du projet
            projetDataPourSauvegarde.InformationsProjet = ProjetActif.InformationsProjet;
            projetDataPourSauvegarde.DateSauvegarde = DateTime.Now;
            // TODO: Remplacer par la version réelle de l'assembly si nécessaire
            projetDataPourSauvegarde.VersionApplication = "0.5.0";

            // 4. Sauvegarder sur le disque via le Repository
            _projetRepository.Sauvegarder(projetDataPourSauvegarde, finalPath);

            // 5. Mémoriser le chemin
            _cheminsPrefereService.SauvegarderDernierDossier(TypeOperation.ProjetSauvegarde, finalPath);

            // Mettre à jour l'état du projet actif pour refléter son nouveau chemin
            CheminFichierProjetActif = finalPath;
        }

        public ProjetSummaryDto GetProjetSummary(string filePath)
        {
            try
            {
                var projetData = _projetRepository.Charger(filePath);
                return new ProjetSummaryDto
                {
                    FilePath = filePath,
                    NomProjet = projetData.InformationsProjet?.NomProjet ?? Path.GetFileNameWithoutExtension(filePath),
                    Description = projetData.InformationsProjet?.Description ?? string.Empty
                };
            }
            catch
            {
                return new ProjetSummaryDto
                {
                    FilePath = filePath,
                    NomProjet = Path.GetFileNameWithoutExtension(filePath),
                    Description = "Erreur: Impossible de lire le fichier."
                };
            }
        }
    }
}