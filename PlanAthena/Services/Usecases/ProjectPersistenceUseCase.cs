// /Services/Usecases/ProjectPersistenceUseCase.cs V0.4.8

using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.DTOs.ProjectPersistence;
using PlanAthena.Services.Infrastructure;


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
        // REMPLACÉ: TaskStatusService est maintenant TaskManagerService
        private readonly TaskManagerService _taskManagerService;
        private readonly ProjetServiceDataAccess _dataAccess;
        private readonly CheminsPrefereService _cheminsService;

        private bool _isDirty = false;

        public ProjectPersistenceUseCase(
            ProjetService projetService,
            RessourceService ressourceService,
            PlanningService planningService,
            TaskManagerService taskManagerService, // MODIFIÉ
            ProjetServiceDataAccess dataAccess,
            CheminsPrefereService cheminsService)
        {
            _projetService = projetService;
            _ressourceService = ressourceService;
            _planningService = planningService;
            _taskManagerService = taskManagerService; // MODIFIÉ
            _dataAccess = dataAccess;
            _cheminsService = cheminsService;
        }

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
            var nomProjetActuel = _projetService.ObtenirInformationsProjet()?.NomProjet ?? "NouveauProjet";
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

                // --- NOUVELLE LOGIQUE DE CHARGEMENT ---
                // 1. Charger les données structurelles
                _projetService.ChargerProjet(data);
                _ressourceService.ChargerRessources(data.Metiers, data.Ouvriers);
                _taskManagerService.ChargerTaches(data.Taches); // Charger les tâches avec leur statut persistant

                // 2. Charger les données du planning s'il existe
                if (data.Planning != null && data.Configuration != null)
                {
                    _planningService.UpdatePlanning(data.Planning, data.Configuration);
                }

                // 3. Synchroniser les statuts pour refléter l'état actuel (EnCours, EnRetard, etc.)
                _taskManagerService.SynchroniserStatutsTaches();

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
            // Pas besoin d'initialiser les tâches, le service est déjà vide.

            _isDirty = false;
        }

        public List<ProjetSummaryDto> ObtenirSummariesProjetsRecents()
        {
            var summaries = new List<ProjetSummaryDto>();
            // ... (logique inchangée, mais le calcul du statut devrait être ajusté si nécessaire)
            // Pour le POC, nous laissons cette partie telle quelle.
            // Elle pourrait être améliorée en calculant le statut EnRetard dynamiquement.
            var recentFiles = _cheminsService.ObtenirFichiersRecents(TypeOperation.ProjetChargement);

            foreach (var filePath in recentFiles)
            {
                try
                {
                    var tempDataAccess = new ProjetServiceDataAccess(_cheminsService);
                    ProjetData data = tempDataAccess.Charger(filePath);
                    var taches = data.Taches ?? new List<Tache>();

                    summaries.Add(new ProjetSummaryDto
                    {
                        FilePath = filePath,
                        NomProjet = data.InformationsProjet?.NomProjet ?? Path.GetFileNameWithoutExtension(filePath),
                        Description = data.InformationsProjet?.Description,
                        NombreTotalTaches = taches.Count(t => string.IsNullOrEmpty(t.ParentId)), // Ne compter que les tâches mères
                        NombreTachesTerminees = taches.Count(t => string.IsNullOrEmpty(t.ParentId) && t.Statut == Statut.Terminée),
                        NombreTachesEnRetard = 0, // Simplifié pour le moment
                        ErreurLecture = false
                    });
                }
                catch
                {
                    summaries.Add(new ProjetSummaryDto { FilePath = filePath, NomProjet = Path.GetFileNameWithoutExtension(filePath), Description = "Impossible de lire le fichier.", ErreurLecture = true });
                }
            }
            return summaries;
        }

        #endregion

        #region Méthodes Privées

        private ProjetData _AssemblerDonneesProjet()
        {
            // Collecter les données de la structure du projet (Lots, etc.)
            var data = _projetService.GetProjetDataPourSauvegarde();

            // --- NOUVELLE LOGIQUE D'ASSEMBLAGE ---
            // Collecter les autres "sources de vérité"
            data.Taches = _taskManagerService.ObtenirToutesLesTachesPourSauvegarde();
            data.Metiers = _ressourceService.GetAllMetiers();
            data.Ouvriers = _ressourceService.GetAllOuvriers();
            data.Planning = _planningService.GetCurrentPlanning();

            // SUPPRIMÉ: La propriété TaskStatuses est obsolète
            // data.TaskStatuses = (Dictionary<string, Status>)_taskStatusService.RetourneTousLesStatuts();

            data.DateSauvegarde = DateTime.Now;
            // Version mise à jour pour refléter cette modification majeure
            data.VersionApplication = "0.5.0";

            return data;
        }

        private void _ViderEtatApplication()
        {
            _projetService.ViderProjet();
            _ressourceService.ViderMetiers();
            _ressourceService.ViderOuvriers();
            _planningService.ClearPlanning();
            // MODIFIÉ: Appeler la méthode de vidage du nouveau service
            _taskManagerService.ViderTaches();
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