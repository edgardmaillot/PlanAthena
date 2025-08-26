// Fichier: PlanAthena/Services/Business/PlanificationService.cs
// Version: 0.4.4
// Description: Mise à jour complète pour s'aligner sur la nouvelle architecture. Le service est sans état
// et reçoit les données (ProjetData, pools de ressources) via les paramètres de ses méthodes.

using PlanAthena.Core.Facade;
using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
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
        private readonly PreparationSolveurService _preparationSolveurService;
        private readonly ResultatConsolidationService _consolidationService;

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

        public async Task<PlanificationResultDto> LancerPlanificationAsync(
            ProjetData projet,
            List<Ouvrier> poolOuvriers,
            List<Metier> poolMetiers,
            ConfigurationPlanification configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (projet?.Taches == null || !projet.Taches.Any())
                throw new PlanificationException("Le projet ne contient aucune tâche à planifier.");
            if (poolOuvriers == null || !poolOuvriers.Any())
                throw new PlanificationException("Le pool de ressources ne contient aucun ouvrier.");
            if (poolMetiers == null || !poolMetiers.Any())
                throw new PlanificationException("Le pool de ressources ne contient aucun métier.");

            try
            {
                var preparationResult = _preparationSolveurService.PreparerPourSolveur(projet.Taches, configuration);

                var inputDto = _dataTransformer.TransformToChantierSetupDto(
                    projet,
                    poolOuvriers,
                    poolMetiers,
                    preparationResult.TachesPreparees,
                    configuration
                );

                var resultatBrut = await _facade.ProcessChantierAsync(inputDto);

                var ganttConsolide = _consolidationService.ConsoliderPourGantt(
                    resultatBrut,
                    preparationResult.ParentIdParSousTacheId,
                    projet.Taches,
                    projet.InformationsProjet.NomProjet ?? "Planning"
                );

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

        public StatistiquesSimplifiees ObtenirStatistiquesTraitement(IReadOnlyList<Tache> taches, int heuresTravailEffectifParJour)
        {
            //on obtien plus les statistiques de cette manière
            return new StatistiquesSimplifiees();
            
        }

    }

    
}