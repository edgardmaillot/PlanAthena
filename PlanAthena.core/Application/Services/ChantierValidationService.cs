using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.Shared;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Input; // Ajout pour le DTO d'input
using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Core.Application.Services
{
    // NOTE: Le nom du service dans le repo est probablement 'ChantierValidationService', même si l'interface est asynchrone.
    // Je garde le nom de la classe que nous avions.
    public class ChantierValidationService : IChantierValidationService
    {
        private const string CYCLE_ERROR_CODE = "ERR_VALID_CYCLE_TACHE";
        private const int MINIMUM_TASKS_FOR_CYCLE = 2;

        // CORRECTION 1: Implémentation de la bonne méthode d'interface.
        // La méthode est asynchrone pour respecter l'interface, même si notre logique ici est synchrone.
        public Task<List<MessageValidationDto>> ValiderChantierCompletAsync(ChantierSetupInputDto inputDto, Chantier? chantier)
        {
            var messages = new List<MessageValidationDto>();

            // Si le chantier est null, le mapping a déjà échoué.
            // On ne peut pas faire de validation de logique métier.
            if (chantier == null)
            {
                // On pourrait ajouter un message ici, mais en général, les erreurs de mapping sont déjà retournées.
                // Donc on retourne une liste vide.
                return Task.FromResult(messages);
            }

            messages.AddRange(ValidateTaskCycles(chantier));
            // D'autres validations peuvent être ajoutées ici à l'avenir

            return Task.FromResult(messages);
        }

        private IEnumerable<MessageValidationDto> ValidateTaskCycles(Chantier chantier)
        {
            return chantier.Blocs.Values
                .Where(HasPotentialForCycles)
                .SelectMany(DetectAndFormatCyclesInBloc);
        }

        private static bool HasPotentialForCycles(BlocTravail bloc)
        {
            return bloc.Taches.Count >= MINIMUM_TASKS_FOR_CYCLE;
        }

        private IEnumerable<MessageValidationDto> DetectAndFormatCyclesInBloc(BlocTravail bloc)
        {
            // CORRECTION 2: Ajout du .Cast<T>() pour la conversion de type explicite.
            var graph = new DependencyGraph<TacheId>(bloc.Taches.Values.Cast<IHaveDependencies<TacheId>>());
            var cycles = graph.DetectCycles();

            return cycles.Select(cycle => CreateCycleValidationMessage(bloc, cycle));
        }

        private static MessageValidationDto CreateCycleValidationMessage(BlocTravail bloc, IReadOnlyList<TacheId> cycle)
        {
            var cyclePath = FormatCyclePath(cycle);
            var message = $"Un cycle de dépendances a été détecté dans le bloc '{bloc.Nom}' ({bloc.Id.Value}): {cyclePath}.";

            return new MessageValidationDto
            {
                Type = TypeMessageValidation.Erreur,
                CodeMessage = CYCLE_ERROR_CODE,
                Message = message,
                ProprieteConcernee = $"Blocs[{bloc.Id.Value}].Taches.Dependencies"
            };
        }

        private static string FormatCyclePath(IEnumerable<TacheId> cycle)
        {
            return string.Join(" -> ", cycle.Select(id => id.Value));
        }
    }
}