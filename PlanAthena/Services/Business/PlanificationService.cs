using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Processing;
using PlanAthena.Utilities;

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
        private readonly DecoupageTachesService _decoupageTachesService;

        private IReadOnlyList<OuvrierRecord> _ouvriers = new List<OuvrierRecord>();
        private IReadOnlyList<TacheRecord> _taches = new List<TacheRecord>();
        private IReadOnlyList<MetierRecord> _metiers = new List<MetierRecord>();

        public PlanificationService(
            PlanAthenaCoreFacade facade,
            DataTransformer dataTransformer,
            DecoupageTachesService decoupageTachesService)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _dataTransformer = dataTransformer ?? throw new ArgumentNullException(nameof(dataTransformer));
            _decoupageTachesService = decoupageTachesService ?? throw new ArgumentNullException(nameof(decoupageTachesService));
        }

        public void ChargerDonnees(IReadOnlyList<OuvrierRecord> ouvriers, IReadOnlyList<TacheRecord> taches, IReadOnlyList<MetierRecord> metiers)
        {
            _ouvriers = ouvriers ?? new List<OuvrierRecord>();
            _taches = taches ?? new List<TacheRecord>();
            _metiers = metiers ?? new List<MetierRecord>();
        }

        public bool ValiderDonneesChargees()
        {
            return _ouvriers.Any() && _metiers.Any() && _taches.Any();
        }

        /// <summary>
        /// FLUX SIMPLIFIÉ : Données chef → Découpage → Solveur
        /// </summary>
        public async Task<ProcessChantierResultDto> LancerPlanificationAsync(ConfigurationUI configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (!ValiderDonneesChargees())
                throw new PlanificationException("Toutes les données (ouvriers, métiers, tâches) doivent être chargées avant de lancer la planification.");

            try
            {
                // ÉTAPE UNIQUE: Découpage pour le solveur (respect total des décisions du chef)
                var tachesPourSolveur = _decoupageTachesService.PreparerPourSolveur(_taches);

                // Transformation vers DLL Core
                var inputDto = _dataTransformer.TransformToChantierSetupDto(
                    _ouvriers.ToList(),
                    tachesPourSolveur,
                    _metiers.ToList(),
                    configuration
                );

                // Appel solveur
                var resultat = await _facade.ProcessChantierAsync(inputDto);

                return resultat;
            }
            catch (Exception ex)
            {
                throw new PlanificationException($"Erreur lors de la planification: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// NOUVELLES STATISTIQUES SIMPLIFIÉES
        /// </summary>
        public StatistiquesSimplifiees ObtenirStatistiquesTraitement()
        {
            if (!ValiderDonneesChargees())
            {
                return new StatistiquesSimplifiees();
            }

            try
            {
                var tachesSolveur = _decoupageTachesService.PreparerPourSolveur(_taches);
                var statsDecoupage = _decoupageTachesService.ObtenirStatistiques(_taches, tachesSolveur);

                return new StatistiquesSimplifiees
                {
                    TachesChef = _taches.Count,
                    TachesSolveur = tachesSolveur.Count,
                    TachesDecoupees = statsDecoupage.TachesLonguesDecoupees,
                    JalonsTechniques = statsDecoupage.JalonsTechniquesCreees,
                    Resume = $"Chef: {_taches.Count} tâches → Solveur: {tachesSolveur.Count} " +
                            $"({statsDecoupage.TachesLonguesDecoupees} découpées, {statsDecoupage.JalonsTechniquesCreees} jalons techniques)"
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

        /// <summary>
        /// Obtient les tâches telles que définies par le chef (pour l'IHM)
        /// </summary>
        public List<TacheRecord> ObtenirTachesLogiques()
        {
            if (!ValiderDonneesChargees())
                return new List<TacheRecord>();

            // SIMPLE: Retourner directement les tâches du chef
            return _taches.ToList();
        }

        /// <summary>
        /// Obtient les tâches prêtes pour le solveur (avec découpage)
        /// </summary>
        public List<TacheRecord> ObtenirTachesPourSolveur()
        {
            if (!ValiderDonneesChargees())
                return new List<TacheRecord>();

            try
            {
                return _decoupageTachesService.PreparerPourSolveur(_taches);
            }
            catch (Exception)
            {
                return _taches.ToList(); // Fallback sur les tâches chef
            }
        }
    }

    /// <summary>
    /// NOUVELLES STATISTIQUES SIMPLIFIÉES
    /// Remplace StatistiquesCompletesTraitement
    /// </summary>
    public class StatistiquesSimplifiees
    {
        public int TachesChef { get; set; }
        public int TachesSolveur { get; set; }
        public int TachesDecoupees { get; set; }
        public int JalonsTechniques { get; set; }
        public string Resume { get; set; } = "";
    }
}