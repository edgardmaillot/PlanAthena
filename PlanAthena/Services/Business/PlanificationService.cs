// Fichier: Services/Business/PlanificationService.cs

using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Forms;
using PlanAthena.Services.Processing;

namespace PlanAthena.Services.Business
{
    public class PlanificationException : Exception
    {
        public PlanificationException(string message) : base(message) { }
        public PlanificationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PlanificationService
    {
        private readonly PlanAthenaCoreFacade _facade;
        private readonly DataTransformer _dataTransformer;
        private readonly PreparationSolveurService _preparationSolveurService;
        private readonly ResultatConsolidationService _consolidationService;

        private IReadOnlyList<Ouvrier> _ouvriers = new List<Ouvrier>();
        private IReadOnlyList<Tache> _taches = new List<Tache>();
        private IReadOnlyList<Metier> _metiers = new List<Metier>();

        public PlanificationService(
            PlanAthenaCoreFacade facade,
            DataTransformer dataTransformer,
            PreparationSolveurService preparationSolveurService,
            ResultatConsolidationService consolidationService)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _dataTransformer = dataTransformer ?? throw new ArgumentNullException(nameof(dataTransformer));
            _preparationSolveurService = preparationSolveurService ?? throw new ArgumentNullException(nameof(preparationSolveurService));
            _consolidationService = consolidationService ?? throw new ArgumentNullException(nameof(consolidationService));
        }

        public void ChargerDonnees(IReadOnlyList<Ouvrier> ouvriers, IReadOnlyList<Tache> taches, IReadOnlyList<Metier> metiers)
        {
            _ouvriers = ouvriers ?? new List<Ouvrier>();
            _taches = taches ?? new List<Tache>();
            _metiers = metiers ?? new List<Metier>();
        }

        public bool ValiderDonneesChargees()
        {
            return _ouvriers.Any() && _metiers.Any() && _taches.Any();
        }

        /// <summary>
        /// Lance la planification et retourne le résultat complet (brut + consolidé)
        /// </summary>
        public async Task<PlanificationResultDto> LancerPlanificationAsync(ConfigurationPlanification configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (!ValiderDonneesChargees())
                throw new PlanificationException("Toutes les données (ouvriers, métiers, tâches) doivent être chargées avant de lancer la planification.");

            try
            {
                // Étape 1 : Préparation des données avec récupération de la table de mappage
                var preparationResult = _preparationSolveurService.PreparerPourSolveur(_taches, configuration.HeuresTravailEffectifParJour);

                // Étape 2 : Transformation pour le solveur
                var inputDto = _dataTransformer.TransformToChantierSetupDto(
                    _ouvriers.ToList(),
                    preparationResult.TachesPreparees, 
                    _metiers.ToList(),
                    configuration
                );

                // Étape 3 : Appel du solveur
                var resultatBrut = await _facade.ProcessChantierAsync(inputDto);

                // Étape 4 : Consolidation pour l'export Gantt
                var ganttConsolide = _consolidationService.ConsoliderPourGantt(
                    resultatBrut,
                    preparationResult.ParentIdParSousTacheId,
                    _taches,
                    "Planning PlanAthena"
                );

                // Retourner le résultat complet
                return new PlanificationResultDto
                {
                    ResultatBrut = resultatBrut,
                    GanttConsolide = ganttConsolide
                };
            }
            catch (Exception ex)
            {
                throw new PlanificationException($"Erreur lors de la planification: {ex.Message}", ex);
            }
        }

        // CORRIGÉ : La méthode a été entièrement revue pour être plus robuste.
        public StatistiquesSimplifiees ObtenirStatistiquesTraitement(int heuresTravailEffectifParJour)
        {
            if (!ValiderDonneesChargees())
            {
                return new StatistiquesSimplifiees();
            }

            try
            {
                var preparationResult = _preparationSolveurService.PreparerPourSolveur(_taches, heuresTravailEffectifParJour);
                var tachesPourSolveur = preparationResult.TachesPreparees;

                // Calcul plus fiable des statistiques :
                // 1. Tâches découpées : on compte les tâches originales qui satisfont la condition.
                int heureLimiteDecoupage = heuresTravailEffectifParJour + 1;
                int tachesLonguesDecoupees = _taches.Count(t => t.Type == TypeActivite.Tache && t.HeuresHommeEstimees > heureLimiteDecoupage);

                // 2. Jalons techniques : on compte les tâches du bon type dans le résultat final.
                int jalonsTechniquesCrees = tachesPourSolveur.Count(t => t.Type == TypeActivite.JalonTechnique);

                return new StatistiquesSimplifiees
                {
                    TachesChef = _taches.Count,
                    TachesSolveur = tachesPourSolveur.Count,
                    TachesDecoupees = tachesLonguesDecoupees,
                    JalonsTechniques = jalonsTechniquesCrees,
                    Resume = $"Chef: {_taches.Count} tâches → Solveur: {tachesPourSolveur.Count} " +
                            $"({tachesLonguesDecoupees} découpées, {jalonsTechniquesCrees} jalons tech.)"
                };
            }
            catch (Exception)
            {
                return new StatistiquesSimplifiees
                {
                    TachesChef = _taches.Count,
                    TachesSolveur = _taches.Count,
                    Resume = $"Chef: {_taches.Count} tâches (erreur préparation solveur)"
                };
            }
        }

        public List<Tache> ObtenirTachesLogiques()
        {
            if (!ValiderDonneesChargees())
                return new List<Tache>();
            return _taches.ToList();
        }

        public List<Tache> ObtenirTachesPourSolveur(int heuresTravailEffectifParJour)
        {
            if (!ValiderDonneesChargees())
                return new List<Tache>();
            try
            {
                var preparationResult = _preparationSolveurService.PreparerPourSolveur(_taches, heuresTravailEffectifParJour);
                return preparationResult.TachesPreparees;
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner la liste originale pour éviter un crash.
                return _taches.ToList();
            }
        }
    }

    public class StatistiquesSimplifiees
    {
        public int TachesChef { get; set; }
        public int TachesSolveur { get; set; }
        public int TachesDecoupees { get; set; }
        public int JalonsTechniques { get; set; }
        public string Resume { get; set; } = "";
    }
}