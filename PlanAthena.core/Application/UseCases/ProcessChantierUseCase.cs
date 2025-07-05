using FluentValidation;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Core.Infrastructure.Services.OrTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanAthena.Core.Application.UseCases;

public class ProcessChantierUseCase : IProcessChantierUseCase
{
    // On garde toutes les dépendances des deux anciens use cases
    private readonly IValidator<ChantierSetupInputDto> _fluentValidator;
    private readonly IChantierSetupInputMapper _inputMapper;
    private readonly IChantierValidationService _chantierValidationService;
    private readonly IInitialFeasibilityAnalysisService _feasibilityService;
    private readonly IKeyResourceSuggestionService _keyResourceService;
    private readonly ICalendrierService _calendrierService;
    private readonly IConstructeurProblemeOrTools _problemeBuilder;

    public ProcessChantierUseCase(
        IValidator<ChantierSetupInputDto> fluentValidator,
        IChantierSetupInputMapper inputMapper,
        IChantierValidationService chantierValidationService,
        IInitialFeasibilityAnalysisService feasibilityService,
        IKeyResourceSuggestionService keyResourceService,
        ICalendrierService calendrierService,
        IConstructeurProblemeOrTools problemeBuilder)
    {
        _fluentValidator = fluentValidator;
        _inputMapper = inputMapper;
        _chantierValidationService = chantierValidationService;
        _feasibilityService = feasibilityService;
        _keyResourceService = keyResourceService;
        _calendrierService = calendrierService;
        _problemeBuilder = problemeBuilder;
    }

    public async Task<ProcessChantierResultDto> ExecuteAsync(ChantierSetupInputDto inputDto)
    {
        // --- Phase 1: Validation et Création du Domaine (logique de InitializeAndAnalyzeUseCase) ---
        var (chantier, validationMessages) = await ValiderEtCreerChantierAsync(inputDto);

        if (chantier == null)
        {
            return new ProcessChantierResultDto
            {
                ChantierId = inputDto.ChantierId,
                Etat = EtatTraitementInput.EchecValidation,
                Messages = validationMessages
            };
        }

        // --- Phase 2: Décider de l'action à mener ---
        // Si aucune config d'optimisation n'est fournie, on s'arrête à l'analyse.
        if (inputDto.OptimizationConfig == null)
        {
            return await AnalyserUniquementAsync(chantier, validationMessages);
        }

        // --- Phase 3: Exécuter l'optimisation (logique de OptimizePlanningUseCase) ---
        return await ExecuterOptimisationAsync(chantier, inputDto.OptimizationConfig, validationMessages);
    }

    private async Task<(Chantier? chantier, IReadOnlyList<MessageValidationDto> messages)> ValiderEtCreerChantierAsync(ChantierSetupInputDto inputDto)
    {
        var allMessages = new List<MessageValidationDto>();

        var fluentValidationResult = await _fluentValidator.ValidateAsync(inputDto);
        if (!fluentValidationResult.IsValid)
        {
            allMessages.AddRange(fluentValidationResult.Errors.Select(e => new MessageValidationDto { Type = TypeMessageValidation.Erreur, CodeMessage = e.ErrorCode ?? "ERR_VALIDATION", Message = e.ErrorMessage, ProprieteConcernee = e.PropertyName }));
            return (null, allMessages);
        }

        var (chantier, mappingMessages) = await _inputMapper.MapToDomainAsync(inputDto);
        allMessages.AddRange(mappingMessages);
        if (chantier == null) return (null, allMessages);

        var cycleMessages = await _chantierValidationService.ValiderChantierCompletAsync(inputDto, chantier);
        allMessages.AddRange(cycleMessages);

        if (allMessages.Any(m => m.Type == TypeMessageValidation.Erreur)) return (null, allMessages);

        return (chantier, allMessages);
    }

    private async Task<ProcessChantierResultDto> AnalyserUniquementAsync(Chantier chantier, IReadOnlyList<MessageValidationDto> validationMessages)
    {
        var allMessages = new List<MessageValidationDto>(validationMessages);

        var feasibilityMessages = await _feasibilityService.AnalyserFaisabiliteAsync(chantier);
        allMessages.AddRange(feasibilityMessages);

        var suggestedKeyResourceIds = await _keyResourceService.SuggererOuvriersClesAsync(chantier);
        var analyseRessourcesResult = new AnalyseRessourcesResultatDto { OuvriersClesSuggereIds = suggestedKeyResourceIds.Select(id => id.Value).ToList() };
        if (suggestedKeyResourceIds.Any())
        {
            allMessages.Add(new MessageValidationDto { Type = TypeMessageValidation.Suggestion, CodeMessage = "SUGGEST_KEY_RESOURCES", Message = $"Suggère les ouvriers suivants comme clés : {string.Join(", ", suggestedKeyResourceIds.Select(id => id.Value))}" });
        }

        var finalEtat = allMessages.Any(m => m.Type == TypeMessageValidation.Avertissement) ? EtatTraitementInput.SuccesAvecAvertissements : EtatTraitementInput.Succes;

        return new ProcessChantierResultDto
        {
            ChantierId = chantier.Id.Value,
            Etat = finalEtat,
            Messages = allMessages,
            AnalyseResultat = analyseRessourcesResult
        };
    }

    private async Task<ProcessChantierResultDto> ExecuterOptimisationAsync(Chantier chantier, OptimizationConfigDto config, IReadOnlyList<MessageValidationDto> validationMessages)
    {
        chantier.AppliquerConfigurationOptimisation(new(config.DureeJournaliereStandardHeures, config.PenaliteChangementOuvrierPourcentage));

        var echelleTemps = _calendrierService.CreerEchelleTempsOuvree(chantier.Calendrier, LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateDebut.Value), LocalDate.FromDateTime(chantier.PeriodeSouhaitee.DateFin.Value));
        var probleme = new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = chantier.ConfigurationOptimisation! };
        var modeleCpSat = _problemeBuilder.ConstruireModele(probleme);

        var solver = new CpSolver();
        var solverStatus = solver.Solve(modeleCpSat.Model);
        //Console.WriteLine($"Nombre de variables: {modeleCpSat.Model.Model.Variables.Count}");
        //Console.WriteLine($"Nombre de contraintes: {modeleCpSat.Model.Model.Constraints.Count}");
        //Console.WriteLine($"Horizon: {echelleTemps.NombreTotalSlots}");
        //Console.WriteLine($"Nombre de tâches: {chantier.ObtenirToutesLesTaches().Count()}");

        // Configurez le solver pour plus de logs
        //solver.StringParameters = "log_search_progress:true";
        //Console.WriteLine($"Temps de résolution: {solver.WallTime}s");
        //Console.WriteLine($"Statut: {solverStatus}");
        var resultatStatus = solverStatus switch
        {
            CpSolverStatus.Optimal => OptimizationStatus.Optimal,
            CpSolverStatus.Feasible => OptimizationStatus.Feasible,
            _ => OptimizationStatus.Infeasible
        };

        var optimResultDto = new PlanningOptimizationResultDto { ChantierId = chantier.Id.Value, Status = resultatStatus };
        if (resultatStatus == OptimizationStatus.Optimal || resultatStatus == OptimizationStatus.Feasible)
        {
            optimResultDto = optimResultDto with
            {
                CoutTotalEstime = (modeleCpSat.CoutTotal != null) ? solver.Value(modeleCpSat.CoutTotal) : null,
                DureeTotaleJoursOuvres = (modeleCpSat.Makespan != null) ? solver.Value(modeleCpSat.Makespan) : null
            };
        }

        return new ProcessChantierResultDto
        {
            ChantierId = chantier.Id.Value,
            Etat = EtatTraitementInput.Succes,
            Messages = validationMessages,
            OptimisationResultat = optimResultDto
        };
    }
}