// --- USING CORRIGÉS ---
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs; // Conservé pour ConsolidatedPlanning, ConfigurationPlanification, etc.
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.DTOs.ProjectPersistence; // Namespace pour notre NOUVEAU ProjetSummaryDto
using PlanAthena.Services.Infrastructure; // Ajout pour CheminsPrefereService dans GetSummaries
using System;
using System.Collections.Generic;
using System.IO; // Ajout pour Path.GetFileNameWithoutExtension
using System.Linq;
using System.Windows.Forms;


namespace PlanAthena.Services.Usecases
{
    /// <summary>
    /// Point d'entrée unique et chef d'orchestre pour toutes les opérations
    /// de cycle de vie d'un projet (Créer, Charger, Sauvegarder).
    /// </summary>
    public class ProjectPersistenceUseCase
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly PlanningService _planningService;
        private readonly TaskStatusService _taskStatusService;
        private readonly ProjetServiceDataAccess _dataAccess;
        private readonly CheminsPrefereService _cheminsService; // Dépendance ajoutée pour les projets récents

        private bool _isDirty = false;

        public ProjectPersistenceUseCase(
            ProjetService projetService,
            RessourceService ressourceService,
            PlanningService planningService,
            TaskStatusService taskStatusService,
            ProjetServiceDataAccess dataAccess,
            CheminsPrefereService cheminsService) // Injection ajoutée
        {
            _projetService = projetService;
            _ressourceService = ressourceService;
            _planningService = planningService;
            _taskStatusService = taskStatusService;
            _dataAccess = dataAccess;
            _cheminsService = cheminsService; // Injection assignée
        }

        /// <summary>
        /// Notifie le UseCase qu'un changement a eu lieu dans le projet,
        /// nécessitant une sauvegarde.
        /// </summary>
        public void SetProjectAsDirty() => _isDirty = true;
        public string GetCurrentProjectPath() => _dataAccess.GetCurrentProjectPath();
        #region API Publique

        public void SauvegarderProjet()
        {
            if (!_dataAccess.IsProjectPathKnown())
            {
                SauvegarderProjetSous();
                return;
            }

            string path = _dataAccess.GetCurrentProjectPath();
            ProjetData data = _AssemblerDonneesProjet();
            _dataAccess.Sauvegarder(data, path);
            _isDirty = false;
        }

        public void SauvegarderProjetSous()
        {
            var nomProjetActuel = _projetService.ObtenirTousLesLots().FirstOrDefault()?.Nom ?? "NouveauProjet";
            string defaultFileName = $"{nomProjetActuel}.json";

            string path = _dataAccess.ShowSaveDialog(defaultFileName);
            if (string.IsNullOrEmpty(path)) return; // Annulé

            ProjetData data = _AssemblerDonneesProjet();
            _dataAccess.Sauvegarder(data, path);
            _isDirty = false;
        }

        public void ChargerProjet()
        {
            if (_isDirty && !_ConfirmDiscardChanges()) return;

            string path = _dataAccess.ShowOpenDialog();
            if (string.IsNullOrEmpty(path)) return; // Annulé

            ChargerProjetDepuisChemin(path);
        }

        public void ChargerProjetDepuisChemin(string filePath)
        {
            if (_isDirty && !_ConfirmDiscardChanges()) return;

            try
            {
                ProjetData data = _dataAccess.Charger(filePath);
                _ViderEtatApplication();

                _projetService.ChargerProjet(data);
                _ressourceService.ChargerRessources(data.Metiers, data.Ouvriers);
                _taskStatusService.ChargerStatuts(data.TaskStatuses);

                if (data.Planning != null && data.Configuration != null)
                {
                    _planningService.UpdatePlanning(data.Planning, data.Configuration);
                }

                _isDirty = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du projet :\n{ex.Message}", "Erreur de chargement", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreerNouveauProjet();
            }
        }

        public void CreerNouveauProjet()
        {
            if (_isDirty && !_ConfirmDiscardChanges()) return;

            _ViderEtatApplication();
            _dataAccess.ResetCurrentProjectPath();

            _projetService.InitialiserNouveauProjet();
            _ressourceService.ChargerMetiersParDefaut();
            _taskStatusService.InitialiserStatutsPourNouveauProjet();

            _isDirty = false;
        }

        public List<ProjetSummaryDto> ObtenirSummariesProjetsRecents()
        {
            var summaries = new List<ProjetSummaryDto>();
            var recentFiles = _cheminsService.ObtenirFichiersRecents(TypeOperation.ProjetChargement);

            foreach (var filePath in recentFiles)
            {
                try
                {
                    // Utilise une instance temporaire pour ne pas affecter l'état de l'application
                    var tempDataAccess = new ProjetServiceDataAccess(_cheminsService);
                    ProjetData data = tempDataAccess.Charger(filePath);
                    var statuses = data.TaskStatuses ?? new Dictionary<string, Status>();

                    summaries.Add(new ProjetSummaryDto
                    {
                        FilePath = filePath,
                        NomProjet = data.InformationsProjet?.NomProjet ?? Path.GetFileNameWithoutExtension(filePath),
                        Description = data.InformationsProjet?.Description,
                        NombreTotalTaches = data.Taches?.Count ?? 0,
                        NombreTachesTerminees = statuses.Values.Count(s => s == Status.Terminee),
                        NombreTachesEnRetard = 0, // Simplifié pour le moment
                        ErreurLecture = false
                    });
                }
                catch
                {
                    summaries.Add(new ProjetSummaryDto
                    {
                        FilePath = filePath,
                        NomProjet = Path.GetFileNameWithoutExtension(filePath),
                        Description = "Impossible de lire le fichier.",
                        ErreurLecture = true
                    });
                }
            }
            return summaries;
        }

        #endregion

        #region Méthodes Privées

        private ProjetData _AssemblerDonneesProjet()
        {
            var data = _projetService.GetProjetDataPourSauvegarde();

            data.Metiers = _ressourceService.GetAllMetiers();
            data.Ouvriers = _ressourceService.GetAllOuvriers();
            data.Planning = _planningService.GetCurrentPlanning();
            data.TaskStatuses = (Dictionary<string, Status>)_taskStatusService.RetourneTousLesStatuts();

            // NOTE : La source de la configuration de planification devra être confirmée. 
            // Pour l'instant, nous la laissons vide.
            // data.Configuration = ... 

            data.DateSauvegarde = DateTime.Now;
            data.VersionApplication = "0.5.1";

            return data;
        }

        private void _ViderEtatApplication()
        {
            _projetService.ViderProjet();
            _ressourceService.ViderMetiers();
            _ressourceService.ViderOuvriers();
            _planningService.ClearPlanning();
            _taskStatusService.ChargerStatuts(null);
        }

        private bool _ConfirmDiscardChanges()
        {
            var result = MessageBox.Show(
                "Le projet en cours a des modifications non sauvegardées.\n\nVoulez-vous continuer et perdre ces modifications ?",
                "Modifications non sauvegardées",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            return result == DialogResult.Yes;
        }

        #endregion
    }
}