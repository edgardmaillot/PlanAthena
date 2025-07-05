// PlanAthena.Core.Application.Validation.InputDtoValidators.cs
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation; // Assurez-vous que le NuGet FluentValidation est installé
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Enums; // Pour les enums

namespace PlanAthena.Core.Application.Validation
{
    // --- Validateur Principal ---
    public class ChantierSetupInputDtoValidator : AbstractValidator<ChantierSetupInputDto>
    {
        public ChantierSetupInputDtoValidator()
        {
            RuleFor(x => x.ChantierId).NotEmpty().Length(1, 100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);

            RuleFor(x => x)
                .Must(HaveDateCoherence)
                .WithMessage("La date de début souhaitée doit être antérieure ou égale à la date de fin souhaitée.")
                .When(x => x.DateDebutSouhaitee.HasValue && x.DateFinSouhaitee.HasValue);

            RuleFor(x => x.FlexibiliteDebut).IsInEnum();
            RuleFor(x => x.FlexibiliteFin).IsInEnum();

            RuleFor(x => x.CalendrierTravail).NotNull().SetValidator(new CalendrierTravailDefinitionDtoValidator());

            RuleFor(x => x.Blocs).NotEmpty().WithMessage("Au moins un bloc doit être défini.");
            RuleForEach(x => x.Blocs).NotNull().SetValidator(new BlocTravailDtoValidator());
            RuleFor(x => x.Blocs).Must(HaveUniqueBlocIds).WithMessage("Les BlocId doivent être uniques.");

            RuleFor(x => x.Taches).NotEmpty().WithMessage("Au moins une tâche doit être définie.");
            RuleForEach(x => x.Taches).NotNull().SetValidator(new TacheDtoValidator());
            RuleFor(x => x.Taches).Must(HaveUniqueTacheIds).WithMessage("Les TacheId doivent être uniques.");

            RuleFor(x => x.Lots).NotEmpty().WithMessage("Au moins un lot doit être défini.");
            RuleForEach(x => x.Lots).NotNull().SetValidator(new LotTravauxDtoValidator());
            RuleFor(x => x.Lots).Must(HaveUniqueLotIds).WithMessage("Les LotId doivent être uniques.");

            RuleFor(x => x.Ouvriers).NotEmpty().WithMessage("Au moins un ouvrier doit être défini.");
            RuleForEach(x => x.Ouvriers).NotNull().SetValidator(new OuvrierDtoValidator());
            RuleFor(x => x.Ouvriers).Must(HaveUniqueOuvrierIds).WithMessage("Les OuvrierId doivent être uniques.");

            RuleFor(x => x.Metiers).NotEmpty().WithMessage("Au moins un métier doit être défini.");
            RuleForEach(x => x.Metiers).NotNull().SetValidator(new MetierDtoValidator());
            RuleFor(x => x.Metiers).Must(HaveUniqueMetierIds).WithMessage("Les MetierId doivent être uniques.");

            When(x => x.ConfigurationCdC != null, () => {
                RuleFor(x => x.ConfigurationCdC!).SetValidator(new ConfigurationChefChantierDtoValidator());
            });
        }

        private bool HaveDateCoherence(ChantierSetupInputDto dto) =>
            dto.DateDebutSouhaitee!.Value <= dto.DateFinSouhaitee!.Value;

        private bool HaveUniqueBlocIds(IReadOnlyList<BlocTravailDto> blocs) =>
            blocs.Select(b => b.BlocId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == blocs.Count;

        private bool HaveUniqueTacheIds(IReadOnlyList<TacheDto> taches) =>
            taches.Select(t => t.TacheId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == taches.Count;

        private bool HaveUniqueLotIds(IReadOnlyList<LotTravauxDto> lots) =>
            lots.Select(l => l.LotId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == lots.Count;

        private bool HaveUniqueOuvrierIds(IReadOnlyList<OuvrierDto> ouvriers) =>
            ouvriers.Select(o => o.OuvrierId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == ouvriers.Count;

        private bool HaveUniqueMetierIds(IReadOnlyList<MetierDto> metiers) =>
            metiers.Select(m => m.MetierId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == metiers.Count;
    }

    // --- Validateurs pour DTOs Imbriqués ---
    public class CalendrierTravailDefinitionDtoValidator : AbstractValidator<CalendrierTravailDefinitionDto>
    {
        public CalendrierTravailDefinitionDtoValidator()
        {
            RuleFor(x => x.JoursOuvres).NotEmpty().WithMessage("Au moins un jour ouvré doit être spécifié.");
            RuleForEach(x => x.JoursOuvres).IsInEnum();
            RuleFor(x => x.JoursOuvres)
                .Must(jours => jours.Distinct().Count() == jours.Count)
                .WithMessage("Les jours ouvrés ne doivent pas contenir de doublons.");

            RuleFor(x => x.HeureDebutJournee).InclusiveBetween(0, 23);
            RuleFor(x => x.HeuresTravailEffectifParJour).GreaterThan(0).LessThanOrEqualTo(24);

            // JoursChomes : chaque date doit être valide, pas de validation de doublons pour l'instant (HashSet dans VO s'en charge)
        }
    }

    public class BlocTravailDtoValidator : AbstractValidator<BlocTravailDto>
    {
        public BlocTravailDtoValidator()
        {
            RuleFor(x => x.BlocId).NotEmpty().Length(1, 100);
            RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CapaciteMaxOuvriers).GreaterThan(0);
        }
    }

    public class TacheDtoValidator : AbstractValidator<TacheDto>
    {
        public TacheDtoValidator()
        {
            RuleFor(x => x.TacheId).NotEmpty().Length(1, 100);
            RuleFor(x => x.Nom).NotEmpty().MaximumLength(250);
            RuleFor(x => x.BlocId).NotEmpty();
            RuleFor(x => x.HeuresHommeEstimees).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MetierId).NotEmpty();

            RuleFor(x => x.Dependencies)
                .Must((tache, deps) => NotContainSelfReference(tache.TacheId, deps))
                .WithMessage("Une tâche ne peut pas dépendre d'elle-même.")
                .Must(HaveUniqueValues)
                .WithMessage("Les dépendances de tâches doivent être uniques pour une même tâche.");
        }
        private bool NotContainSelfReference(string id, IReadOnlyList<string> dependancyIds) =>
            !dependancyIds.Contains(id, StringComparer.OrdinalIgnoreCase);
        private bool HaveUniqueValues(IReadOnlyList<string> values) =>
            values.Distinct(StringComparer.OrdinalIgnoreCase).Count() == values.Count;
    }

    public class LotTravauxDtoValidator : AbstractValidator<LotTravauxDto>
    {
        public LotTravauxDtoValidator()
        {
            RuleFor(x => x.LotId).NotEmpty().Length(1, 100);
            RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BlocIds).NotEmpty().WithMessage("Un lot doit contenir au moins un BlocId.");
            RuleFor(x => x.BlocIds).Must(HaveUniqueValues).WithMessage("Les BlocId au sein d'un lot doivent être uniques.");
            RuleFor(x => x.Priorite).GreaterThan(0);

            When(x => x.DateDebutAuPlusTotSouhaitee.HasValue && x.DateFinAuPlusTardSouhaitee.HasValue, () => {
                RuleFor(x => x)
                    .Must(lot => lot.DateDebutAuPlusTotSouhaitee!.Value <= lot.DateFinAuPlusTardSouhaitee!.Value)
                    .WithMessage("La date de début souhaitée du lot doit être antérieure ou égale à sa date de fin souhaitée.");
            });
        }
        private bool HaveUniqueValues(IReadOnlyList<string> values) =>
            values.Distinct(StringComparer.OrdinalIgnoreCase).Count() == values.Count;
    }

    public class CompetenceDtoValidator : AbstractValidator<CompetenceDto>
    {
        public CompetenceDtoValidator()
        {
            RuleFor(x => x.MetierId).NotEmpty();
            RuleFor(x => x.Niveau).IsInEnum();
            When(x => x.PerformancePct.HasValue, () => {
                RuleFor(x => x.PerformancePct!.Value).GreaterThan(0); // Ex: 10, 50, 100, 120, 200
            });
        }
    }

    public class OuvrierDtoValidator : AbstractValidator<OuvrierDto>
    {
        public OuvrierDtoValidator()
        {
            RuleFor(x => x.OuvrierId).NotEmpty().Length(1, 100);
            RuleFor(x => x.Nom).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Prenom).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CoutJournalier).GreaterThanOrEqualTo(0); // Un coût de 0 est possible (bénévole?) mais typiquement >0
            RuleFor(x => x.Competences).NotEmpty().WithMessage("Un ouvrier doit avoir au moins une compétence.");
            RuleForEach(x => x.Competences).NotNull().SetValidator(new CompetenceDtoValidator());
            RuleFor(x => x.Competences)
                .Must(HaveUniqueMetiersInCompetences)
                .WithMessage("Un ouvrier ne peut pas avoir plusieurs définitions de compétence pour le même métier.");
        }
        private bool HaveUniqueMetiersInCompetences(IReadOnlyList<CompetenceDto> competences) =>
            competences.Select(c => c.MetierId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == competences.Count;
    }

    public class MetierDtoValidator : AbstractValidator<MetierDto>
    {
        public MetierDtoValidator()
        {
            RuleFor(x => x.MetierId).NotEmpty().Length(1, 100);
            RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
            RuleFor(x => x.PrerequisMetierIds)
                .Must((metier, prereqs) => NotContainSelfReference(metier.MetierId, prereqs))
                .WithMessage("Un métier ne peut pas être son propre prérequis.")
                .Must(HaveUniqueValues)
                .WithMessage("Les prérequis métier doivent être uniques pour un même métier.");
        }
        private bool NotContainSelfReference(string id, IReadOnlyList<string> dependancyIds) =>
            !dependancyIds.Contains(id, StringComparer.OrdinalIgnoreCase);
        private bool HaveUniqueValues(IReadOnlyList<string> values) =>
            values.Distinct(StringComparer.OrdinalIgnoreCase).Count() == values.Count;
    }

    public class ConfigurationChefChantierDtoValidator : AbstractValidator<ConfigurationChefChantierDto>
    {
        public ConfigurationChefChantierDtoValidator()
        {
            RuleFor(x => x.OuvriersClefsIds)
                .Must(HaveUniqueValues)
                .WithMessage("La liste des OuvriersClefsIds ne doit pas contenir de doublons.");
            RuleFor(x => x.MetiersClefsIds)
                .Must(HaveUniqueValues)
                .WithMessage("La liste des MetiersClefsIds ne doit pas contenir de doublons.");
        }
        private bool HaveUniqueValues(IReadOnlyList<string> values) =>
           values.Distinct(StringComparer.OrdinalIgnoreCase).Count() == values.Count;
    }
}