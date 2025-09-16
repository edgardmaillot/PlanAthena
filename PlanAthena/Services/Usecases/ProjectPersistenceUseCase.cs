// Fichier: /Services/Usecases/ProjectPersistenceUseCase.cs Version 0.6.0

using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.DTOs.ProjectPersistence;
using PlanAthena.Services.DTOs.Projet;
using PlanAthena.Services.Infrastructure;

namespace PlanAthena.Services.Usecases
{
    /// <summary>
    /// Point d'entrée unique et chef d'orchestre pour toutes les opérations
    /// de cycle de vie d'un projet (Créer, Charger, Sauvegarder).
    /// Version 0.6.0 : Intègre la persistance de la PlanningBaseline.
    /// </summary>
    public class ProjectPersistenceUseCase
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly PlanningService _planningService;
        private readonly TaskManagerService _taskManagerService;
        private readonly ProjetServiceDataAccess _dataAccess;
        private readonly CheminsPrefereService _cheminsService;

        private bool _isDirty = false;

        public ProjectPersistenceUseCase(
            ProjetService projetService,
            RessourceService ressourceService,
            PlanningService planningService,
            TaskManagerService taskManagerService,
            ProjetServiceDataAccess dataAccess,
            CheminsPrefereService cheminsService)
        {
            _projetService = projetService;
            _ressourceService = ressourceService;
            _planningService = planningService;
            _taskManagerService = taskManagerService;
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

        public virtual void ChargerProjetDepuisChemin(string filePath)
        {
            if (_isDirty && !_ConfirmDiscardChanges()) return;

            try
            {
                ProjetData data = _dataAccess.Charger(filePath);
                _ViderEtatApplication();

                _projetService.ChargerProjet(data);
                _ressourceService.ChargerRessources(data.Metiers, data.Ouvriers);
                _taskManagerService.ChargerTaches(data.Taches);

                if (data.Planning != null)
                {
                    var config = data.Configuration ?? _projetService.ConfigPlanificationActuelle;
                    // MODIFIÉ : On passe la baseline chargée depuis le fichier
                    _planningService.LoadPlanning(data.Planning, config, data.Baseline);
                }

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
                    var tempDataAccess = new ProjetServiceDataAccess(_cheminsService);
                    ProjetData data = tempDataAccess.Charger(filePath);

                    if (data.Summary == null)
                    {
                        var tachesMeres = (data.Taches ?? new List<Tache>()).Where(t => string.IsNullOrEmpty(t.ParentId)).ToList();
                        data.Summary = new ProjectSummaryData
                        {
                            NombreTotalTaches = tachesMeres.Count,
                            NombreTachesTerminees = tachesMeres.Count(t => t.Statut == Statut.Terminée),
                            NombreTachesEnRetard = tachesMeres.Count(t => t.Statut == Statut.EnRetard)
                        };
                    }

                    summaries.Add(new ProjetSummaryDto
                    {
                        FilePath = filePath,
                        NomProjet = data.InformationsProjet?.NomProjet ?? Path.GetFileNameWithoutExtension(filePath),
                        Description = data.InformationsProjet?.Description,
                        NombreTotalTaches = data.Summary.NombreTotalTaches,
                        NombreTachesTerminees = data.Summary.NombreTachesTerminees,
                        NombreTachesEnRetard = data.Summary.NombreTachesEnRetard,
                        ErreurLecture = false,
                        ImagePath = data.InformationsProjet?.ImagePath,
                        IsFavorite = false
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
            var data = _projetService.GetProjetDataPourSauvegarde();

            data.Taches = _taskManagerService.ObtenirToutesLesTachesPourSauvegarde();
            data.Metiers = _ressourceService.GetAllMetiers();
            data.Ouvriers = _ressourceService.GetAllOuvriers();
            data.Planning = _planningService.GetCurrentPlanning();
            data.Configuration = _planningService.GetCurrentConfig();
            data.Baseline = _planningService.GetBaseline(); // << AJOUT

            var resumeTaches = _taskManagerService.ObtenirResumeTaches();
            data.Summary = new ProjectSummaryData
            {
                NombreTotalTaches = resumeTaches.Total,
                NombreTachesTerminees = resumeTaches.Terminees,
                NombreTachesEnRetard = resumeTaches.EnRetard
            };

            data.DateSauvegarde = DateTime.Now;
            data.VersionApplication = "0.6.0"; // Mise à jour de la version

            return data;
        }

        private void _ViderEtatApplication()
        {
            _projetService.ViderProjet();
            _ressourceService.ViderMetiers();
            _ressourceService.ViderOuvriers();
            _planningService.ClearPlanning();
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