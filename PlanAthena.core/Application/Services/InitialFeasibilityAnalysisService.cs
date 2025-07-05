using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Core.Application.Services
{
    public class InitialFeasibilityAnalysisService : IInitialFeasibilityAnalysisService
    {
        private const string METIER_NON_COUVERT_CODE = "WARN_METIER_NON_COUVERT";

        /// <summary>
        /// Analyse la faisabilité préliminaire du chantier, en se concentrant sur la couverture des métiers.
        /// </summary>
        /// <param name="chantier">L'agrégat Chantier du domaine, supposé valide et complet.</param>
        /// <returns>Une liste de messages (Avertissements) si des métiers requis ne sont pas couverts.</returns>
        public Task<List<MessageValidationDto>> AnalyserFaisabiliteAsync(Chantier chantier)
        {
            var messages = new List<MessageValidationDto>();

            // 1. Lister tous les MetierId distincts requis par les tâches.
            // L'utilisation d'un HashSet est efficace pour garantir l'unicité et pour des recherches rapides.
            var metiersRequis = chantier.ObtenirToutesLesTaches()
                                        .Select(tache => tache.MetierRequisId)
                                        .ToHashSet();

            // Si aucune tâche ne requiert de métier, il n'y a rien à analyser.
            if (metiersRequis.Count == 0)
            {
                return Task.FromResult(messages);
            }

            // 2. Lister tous les MetierId distincts possédés par les ouvriers.
            var metiersDisponibles = chantier.Ouvriers.Values
                                             .SelectMany(ouvrier => ouvrier.Competences.Keys)
                                             .ToHashSet();

            // 3. Identifier les métiers requis qui ne sont pas disponibles.
            // La méthode Except() est parfaite et très lisible pour cette opération d'ensemble.
            var metiersNonCouverts = metiersRequis.Except(metiersDisponibles);

            // 4. Créer un message d'avertissement pour chaque métier non couvert.
            foreach (var metierId in metiersNonCouverts)
            {
                // On récupère le nom du métier pour un message plus clair pour l'utilisateur.
                string nomMetier = chantier.Metiers.TryGetValue(metierId, out var metier) ? metier.Nom : "Inconnu";

                messages.Add(new MessageValidationDto
                {
                    Type = TypeMessageValidation.Avertissement,
                    CodeMessage = METIER_NON_COUVERT_CODE,
                    Message = $"Le métier '{nomMetier}' ({metierId.Value}) est requis par une ou plusieurs tâches, mais aucun ouvrier ne possède cette compétence.",
                    ProprieteConcernee = "Ouvriers" // La propriété impactée est la liste des ouvriers.
                });
            }

            return Task.FromResult(messages);
        }
    }
}