// Fichier : Services/Business/PlanificationService.cs

using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Processing; // Ajout du using
using PlanAthena.Utilities;         // Ajout du using


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
        private readonly DependanceService _dependanceService;
        private readonly ChantierDataProcessor _chantierDataProcessor;

        private IReadOnlyList<OuvrierRecord> _ouvriers = new List<OuvrierRecord>();
        private IReadOnlyList<TacheRecord> _taches = new List<TacheRecord>();
        private IReadOnlyList<MetierRecord> _metiers = new List<MetierRecord>();

        public PlanificationService(
            PlanAthenaCoreFacade facade,
            DataTransformer dataTransformer,
            DependanceService dependanceService,
            ChantierDataProcessor chantierDataProcessor)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _dataTransformer = dataTransformer ?? throw new ArgumentNullException(nameof(dataTransformer));
            _dependanceService = dependanceService ?? throw new ArgumentNullException(nameof(dependanceService));
            _chantierDataProcessor = chantierDataProcessor ?? throw new ArgumentNullException(nameof(chantierDataProcessor));
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

        public async Task<ProcessChantierResultDto> LancerPlanificationAsync(ConfigurationUI configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (!ValiderDonneesChargees())
                throw new PlanificationException("Toutes les données (ouvriers, métiers, tâches) doivent être chargées avant de lancer la planification.");

            try
            {
                // Étape 1: Enrichir les tâches avec les dépendances métier
                var tachesAvecDependances = _dependanceService.CalculerDependancesMetier(_taches.ToList());

                // Étape 2: Préparer ces tâches pour le solveur (synchro, découpage)
                var tachesPourSolveur = _chantierDataProcessor.ProcessTachesPourSolveur(tachesAvecDependances);

                // Étape 3: Transformer les données en DTO pour la DLL Core
                var inputDto = _dataTransformer.TransformToChantierSetupDto(
                    _ouvriers.ToList(),
                    tachesPourSolveur,
                    _metiers.ToList(),
                    configuration
                );

                // Étape 4: Appel à PlanAthena Core
                var resultat = await _facade.ProcessChantierAsync(inputDto);

                return resultat;
            }
            catch (Exception ex)
            {
                throw new PlanificationException($"Erreur lors de la planification: {ex.Message}", ex);
            }
        }
    }
}