// Services de validation supplémentaires pour détecter les cas d'infaisabilité

using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Core.Application.Services
{
    /// <summary>
    /// Service de validation des contraintes de capacité et de ressources
    /// </summary>
    public class CapacityValidationService : ICapacityValidationService
    {
        private const string CAPACITY_OVERLOAD_CODE = "WARN_CAPACITY_OVERLOAD";
        private const string INSUFFICIENT_WORKERS_CODE = "WARN_INSUFFICIENT_WORKERS";
        private const string BLOC_OVERCAPACITY_CODE = "WARN_BLOC_OVERCAPACITY";

        public Task<List<MessageValidationDto>> ValidateCapacityConstraintsAsync(ChantierSetupInputDto inputDto, Chantier chantier)
        {
            var messages = new List<MessageValidationDto>();

            // 1. Vérifier la charge de travail globale vs ressources disponibles
            messages.AddRange(ValidateGlobalWorkload(chantier));

            // 2. Vérifier les capacités des blocs vs nombre d'ouvriers
            messages.AddRange(ValidateBlocCapacities(chantier));

            // 3. Vérifier les contraintes de compétences
            messages.AddRange(ValidateSkillConstraints(chantier));

            // 4. Vérifier les dates impératives vs période projet
            messages.AddRange(ValidateDateConstraints(inputDto, chantier));

            return Task.FromResult(messages);
        }

        private IEnumerable<MessageValidationDto> ValidateGlobalWorkload(Chantier chantier)
        {
            // CORRECTION: Filtrer les ouvriers virtuels pour avoir le vrai nombre d'ouvriers
            var ouvriersReels = chantier.Ouvriers.Values
                .Where(o => !o.Nom.Contains("Ouvrier Virtuel") && !o.Nom.Contains("Virtuel"))
                .ToList();

            // Calculer la charge totale de travail (exclure les jalons)
            var totalHeuresRequises = chantier.ObtenirToutesLesTaches()
                .Where(t => t.Type == TypeActivite.Tache) // Exclure les jalons
                .Sum(t => t.HeuresHommeEstimees.Value);

            if (totalHeuresRequises == 0) yield break;

            // Calculer la capacité totale disponible avec les vrais ouvriers
            var nombreJoursProjet = CalculateProjectDays(chantier);
            var heuresJournalieresStandard = chantier.ConfigurationOptimisation?.DureeJournaliereStandardHeures ?? 8;
            var capaciteTotaleDisponible = ouvriersReels.Count * nombreJoursProjet * heuresJournalieresStandard;

            // Vérifier si la charge dépasse largement la capacité (seuil de 90%)
            if (totalHeuresRequises > capaciteTotaleDisponible * 0.9)
            {
                yield return new MessageValidationDto
                {
                    Type = TypeMessageValidation.Avertissement,
                    CodeMessage = CAPACITY_OVERLOAD_CODE,
                    Message = $"Charge de travail très élevée : {totalHeuresRequises}h requises vs ~{capaciteTotaleDisponible}h disponibles " +
                             $"({ouvriersReels.Count} ouvriers réels). Le planning pourrait être très contraint ou infaisable.",
                    ProprieteConcernee = "Taches"
                };
            }
        }

        private IEnumerable<MessageValidationDto> ValidateBlocCapacities(Chantier chantier)
        {
            foreach (var bloc in chantier.Blocs.Values)
            {
                var tachesDansBloc = bloc.Taches.Values.ToList();
                if (!tachesDansBloc.Any()) continue;

                // CORRECTION: Filtrer les ouvriers virtuels créés pour les jalons
                var ouvriersReels = chantier.Ouvriers.Values
                    .Where(o => !o.Nom.Contains("Ouvrier Virtuel") && !o.Nom.Contains("Virtuel"))
                    .ToList();

                // Vérifier si le nombre d'ouvriers compétents dépasse la capacité du bloc
                var metiersRequiDansBloc = tachesDansBloc
                    .Where(t => t.Type == TypeActivite.Tache) // Exclure les jalons
                    .Select(t => t.MetierRequisId)
                    .Distinct()
                    .ToList();

                if (!metiersRequiDansBloc.Any()) continue; // Bloc sans tâches réelles

                var ouvriersCompetentsPourBloc = ouvriersReels
                    .Where(o => metiersRequiDansBloc.Any(metier => o.Competences.ContainsKey(metier)))
                    .Count();

                if (ouvriersCompetentsPourBloc > bloc.CapaciteMaxOuvriers)
                {
                    yield return new MessageValidationDto
                    {
                        Type = TypeMessageValidation.Avertissement,
                        CodeMessage = BLOC_OVERCAPACITY_CODE,
                        Message = $"Bloc '{bloc.Nom}' : {ouvriersCompetentsPourBloc} ouvriers compétents disponibles " +
                                 $"mais capacité limitée à {bloc.CapaciteMaxOuvriers}. Risque de goulot d'étranglement.",
                        ProprieteConcernee = $"Blocs[{bloc.Id.Value}].CapaciteMaxOuvriers"
                    };
                }
            }
        }

        private IEnumerable<MessageValidationDto> ValidateSkillConstraints(Chantier chantier)
        {
            var messages = new List<MessageValidationDto>();

            // CORRECTION: Filtrer les ouvriers virtuels
            var ouvriersReels = chantier.Ouvriers.Values
                .Where(o => !o.Nom.Contains("Ouvrier Virtuel") && !o.Nom.Contains("Virtuel"))
                .ToList();

            // Pour chaque métier requis, vérifier s'il y a assez d'ouvriers compétents
            var metiersRequis = chantier.ObtenirToutesLesTaches()
                .Where(t => t.Type == TypeActivite.Tache) // Exclure les jalons
                .GroupBy(t => t.MetierRequisId)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.HeuresHommeEstimees.Value));

            foreach (var (metierId, heuresRequises) in metiersRequis)
            {
                var ouvriersCompetents = ouvriersReels
                    .Where(o => o.Competences.ContainsKey(metierId))
                    .ToList();

                if (ouvriersCompetents.Count == 0)
                {
                    // Ce cas est déjà couvert par InitialFeasibilityAnalysisService
                    continue;
                }

                // Vérifier si un seul ouvrier porte toute la charge d'un métier critique
                if (ouvriersCompetents.Count == 1 && heuresRequises > 40) // Plus de 40h de travail
                {
                    var metierNom = chantier.Metiers.TryGetValue(metierId, out var metier) ? metier.Nom : "Inconnu";
                    messages.Add(new MessageValidationDto
                    {
                        Type = TypeMessageValidation.Avertissement,
                        CodeMessage = INSUFFICIENT_WORKERS_CODE,
                        Message = $"Métier '{metierNom}' : {heuresRequises}h de travail pour un seul ouvrier compétent. " +
                                 "Risque de goulot d'étranglement ou d'infaisabilité si cet ouvrier a d'autres contraintes.",
                        ProprieteConcernee = "Ouvriers"
                    });
                }
            }

            return messages;
        }

        private IEnumerable<MessageValidationDto> ValidateDateConstraints(ChantierSetupInputDto inputDto, Chantier chantier)
        {
            if (!inputDto.DateDebutSouhaitee.HasValue || !inputDto.DateFinSouhaitee.HasValue)
                yield break;

            var debutProjet = inputDto.DateDebutSouhaitee.Value;
            var finProjet = inputDto.DateFinSouhaitee.Value;

            // CORRECTION: D'après votre structure DTO, les dates et flexibilités sont au niveau du chantier,
            // pas au niveau des tâches individuelles. 
            // Cette validation vérifie que les dates du chantier sont cohérentes (déjà fait par FluentValidation)
            // et que la période n'est pas trop courte par rapport à la charge de travail

            // Calculer si la période est réaliste par rapport à la charge
            var totalHeuresTaches = inputDto.Taches.Sum(t => t.HeuresHommeEstimees);
            var nombreJoursProjet = (finProjet - debutProjet).Days + 1;
            var heuresJournalieres = inputDto.CalendrierTravail.HeuresTravailEffectifParJour;
            var capaciteTheorique = nombreJoursProjet * inputDto.Ouvriers.Count * heuresJournalieres;

            if (totalHeuresTaches > capaciteTheorique)
            {
                yield return new MessageValidationDto
                {
                    Type = TypeMessageValidation.Avertissement,
                    CodeMessage = "WARN_PERIODE_TROP_COURTE",
                    Message = $"Période projet ({nombreJoursProjet} jours) possiblement trop courte : " +
                             $"{totalHeuresTaches}h de travail vs {capaciteTheorique}h de capacité théorique maximale.",
                    ProprieteConcernee = "DateFinSouhaitee"
                };
            }

            // Si FlexibiliteDebut/Fin sont impératives, vérifier qu'il y a une marge de manœuvre
            if (inputDto.FlexibiliteDebut == FlexibiliteDate.Imperative && inputDto.FlexibiliteFin == FlexibiliteDate.Imperative)
            {
                // Dates impératives + charge élevée = risque d'infaisabilité
                if (totalHeuresTaches > capaciteTheorique * 0.8)
                {
                    yield return new MessageValidationDto
                    {
                        Type = TypeMessageValidation.Avertissement,
                        CodeMessage = "WARN_DATES_IMPERATIVES_RISQUE",
                        Message = "Dates de début et fin impératives avec une charge de travail élevée. " +
                                 "Risque important d'infaisabilité si des imprévus surviennent.",
                        ProprieteConcernee = "FlexibiliteDebut"
                    };
                }
            }
        }

        private int CalculateProjectDays(Chantier chantier)
        {
            if (!chantier.PeriodeSouhaitee.DateDebut.HasValue || !chantier.PeriodeSouhaitee.DateFin.HasValue)
                return 30; // Valeur par défaut

            var duree = chantier.PeriodeSouhaitee.DateFin.Value - chantier.PeriodeSouhaitee.DateDebut.Value;
            return Math.Max(1, duree.Days);
        }
    }

    /// <summary>
    /// Service de validation des références croisées
    /// </summary>
    public class CrossReferenceValidationService : ICrossReferenceValidationService
    {
        public Task<List<MessageValidationDto>> ValidateCrossReferencesAsync(ChantierSetupInputDto inputDto, Chantier? chantier)
        {
            var messages = new List<MessageValidationDto>();

            // 1. Vérifier que toutes les références de BlocId dans les tâches existent
            messages.AddRange(ValidateTacheBlocReferences(inputDto));

            // 2. Vérifier que toutes les références MetierId dans les tâches existent
            messages.AddRange(ValidateTacheMetierReferences(inputDto));

            // 3. Vérifier que toutes les dépendances de tâches existent
            messages.AddRange(ValidateTaskDependencies(inputDto));

            // 4. Vérifier les références dans les lots
            messages.AddRange(ValidateLotBlocReferences(inputDto));

            // 5. Vérifier les références dans les compétences des ouvriers
            messages.AddRange(ValidateOuvrierCompetenceReferences(inputDto));

            return Task.FromResult(messages);
        }

        private IEnumerable<MessageValidationDto> ValidateTacheBlocReferences(ChantierSetupInputDto inputDto)
        {
            var blocIds = inputDto.Blocs.Select(b => b.BlocId).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return inputDto.Taches
                .Where(t => !blocIds.Contains(t.BlocId))
                .Select(t => new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_TACHE_BLOC_NOT_FOUND",
                    Message = $"Tâche '{t.Nom}' : le BlocId '{t.BlocId}' n'existe pas.",
                    ProprieteConcernee = $"Taches[{t.TacheId}].BlocId"
                });
        }

        private IEnumerable<MessageValidationDto> ValidateTacheMetierReferences(ChantierSetupInputDto inputDto)
        {
            var metierIds = inputDto.Metiers.Select(m => m.MetierId).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return inputDto.Taches
                .Where(t => t.Type == TypeActivite.Tache && !string.IsNullOrEmpty(t.MetierId) && !metierIds.Contains(t.MetierId))
                .Select(t => new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_TACHE_METIER_NOT_FOUND",
                    Message = $"Tâche '{t.Nom}' : le MetierId '{t.MetierId}' n'existe pas.",
                    ProprieteConcernee = $"Taches[{t.TacheId}].MetierId"
                });
        }

        private IEnumerable<MessageValidationDto> ValidateTaskDependencies(ChantierSetupInputDto inputDto)
        {
            var tacheIds = inputDto.Taches.Select(t => t.TacheId).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return inputDto.Taches
                .SelectMany(t => t.Dependencies.Select(dep => new { Tache = t, Dependency = dep }))
                .Where(x => !tacheIds.Contains(x.Dependency))
                .Select(x => new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_TACHE_DEPENDENCY_NOT_FOUND",
                    Message = $"Tâche '{x.Tache.Nom}' : la dépendance '{x.Dependency}' n'existe pas.",
                    ProprieteConcernee = $"Taches[{x.Tache.TacheId}].Dependencies"
                });
        }

        private IEnumerable<MessageValidationDto> ValidateLotBlocReferences(ChantierSetupInputDto inputDto)
        {
            var blocIds = inputDto.Blocs.Select(b => b.BlocId).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return inputDto.Lots
                .SelectMany(lot => lot.BlocIds.Select(blocId => new { Lot = lot, BlocId = blocId }))
                .Where(x => !blocIds.Contains(x.BlocId))
                .Select(x => new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_LOT_BLOC_NOT_FOUND",
                    Message = $"Lot '{x.Lot.Nom}' : le BlocId '{x.BlocId}' n'existe pas.",
                    ProprieteConcernee = $"Lots[{x.Lot.LotId}].BlocIds"
                });
        }

        private IEnumerable<MessageValidationDto> ValidateOuvrierCompetenceReferences(ChantierSetupInputDto inputDto)
        {
            var metierIds = inputDto.Metiers.Select(m => m.MetierId).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return inputDto.Ouvriers
                .SelectMany(ouvrier => ouvrier.Competences.Select(comp => new { Ouvrier = ouvrier, Competence = comp }))
                .Where(x => !metierIds.Contains(x.Competence.MetierId))
                .Select(x => new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_OUVRIER_COMPETENCE_NOT_FOUND",
                    Message = $"Ouvrier '{x.Ouvrier.Nom} {x.Ouvrier.Prenom}' : compétence '{x.Competence.MetierId}' référence un métier inexistant.",
                    ProprieteConcernee = $"Ouvriers[{x.Ouvrier.OuvrierId}].Competences"
                });
        }
    }
}

