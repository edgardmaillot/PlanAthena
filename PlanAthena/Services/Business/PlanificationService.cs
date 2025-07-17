using PlanAthena.Core.Facade;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        // CORRIGÉ : Le nom de la variable est maintenant cohérent avec son type.
        private readonly PreparationSolveurService _preparationSolveurService;

        private IReadOnlyList<Ouvrier> _ouvriers = new List<Ouvrier>();
        private IReadOnlyList<Tache> _taches = new List<Tache>();
        private IReadOnlyList<Metier> _metiers = new List<Metier>();

        public PlanificationService(
            PlanAthenaCoreFacade facade,
            DataTransformer dataTransformer,
            PreparationSolveurService preparationSolveurService) // CORRIGÉ
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _dataTransformer = dataTransformer ?? throw new ArgumentNullException(nameof(dataTransformer));
            _preparationSolveurService = preparationSolveurService ?? throw new ArgumentNullException(nameof(preparationSolveurService)); // CORRIGÉ
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

        public async Task<ProcessChantierResultDto> LancerPlanificationAsync(ConfigurationUI configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (!ValiderDonneesChargees())
                throw new PlanificationException("Toutes les données (ouvriers, métiers, tâches) doivent être chargées avant de lancer la planification.");

            try
            {
                // La logique ici reste la même, mais le nom de la variable est plus clair.
                var tachesPourSolveur = _preparationSolveurService.PreparerPourSolveur(_taches);

                var inputDto = _dataTransformer.TransformToChantierSetupDto(
                    _ouvriers.ToList(),
                    tachesPourSolveur,
                    _metiers.ToList(),
                    configuration
                );

                var resultat = await _facade.ProcessChantierAsync(inputDto);
                return resultat;
            }
            catch (Exception ex)
            {
                throw new PlanificationException($"Erreur lors de la planification: {ex.Message}", ex);
            }
        }

        // CORRIGÉ : La méthode a été entièrement revue pour ne plus dépendre d'ObtenirStatistiques.
        public StatistiquesSimplifiees ObtenirStatistiquesTraitement()
        {
            if (!ValiderDonneesChargees())
            {
                return new StatistiquesSimplifiees();
            }

            try
            {
                var tachesPourSolveur = _preparationSolveurService.PreparerPourSolveur(_taches);

                // On calcule les statistiques directement ici.
                int tachesLonguesDecoupees = _taches.Count(t => !t.EstJalon && t.HeuresHommeEstimees > 8);
                // Les "jalons techniques" sont maintenant des "tâches de regroupement" invisibles.
                // On les compte en comparant le nombre de tâches avant et après préparation.
                int tachesDeRegroupement = tachesPourSolveur.Count(t => t.TacheNom.StartsWith("Regroupement de"));

                return new StatistiquesSimplifiees
                {
                    TachesChef = _taches.Count,
                    TachesSolveur = tachesPourSolveur.Count,
                    TachesDecoupees = tachesLonguesDecoupees,
                    JalonsTechniques = tachesDeRegroupement, // Renommé pour la cohérence de l'IHM
                    Resume = $"Chef: {_taches.Count} tâches → Solveur: {tachesPourSolveur.Count} " +
                            $"({tachesLonguesDecoupees} découpées, {tachesDeRegroupement} regroupements)"
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

        public List<Tache> ObtenirTachesPourSolveur()
        {
            if (!ValiderDonneesChargees())
                return new List<Tache>();
            try
            {
                return _preparationSolveurService.PreparerPourSolveur(_taches);
            }
            catch (Exception)
            {
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