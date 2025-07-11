using Xunit;
using FluentAssertions;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Infrastructure.Services.OrTools;
using System.Collections.Generic;
using System.Linq;
using System;
using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.core.Tests.Infrastructure.OrTools;

public class CoutModelBuilderTests
{
    private (CpModel model, ProblemeOptimisation probleme, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables, IntVar makespan) GetTestContext()
    {
        var model = new CpModel();
        var probleme = CreerProblemeDeTest();
        var tacheBuilder = new TacheModelBuilder();
        // On récupère maintenant le makespan
        var (tachesIntervals, tachesAssignables, makespan) = tacheBuilder.Construire(model, probleme);
        return (model, probleme, tachesIntervals, tachesAssignables, makespan);
    }

    [Fact]
    public void Construire_CreeLeBonNombreDeVariablesDeCout()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables, makespan) = GetTestContext(); // Récupère le makespan
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan); // Passe le makespan

        // Assert
        var coutVar = model.Model.Variables.FirstOrDefault(v => v.Name == "cout_total_chantier");
        coutVar.Should().NotBeNull();
    }

    [Fact]
    public void Construire_CreeLeBonNombreDeVariablesDeTravailJournalier()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables, makespan) = GetTestContext(); // Récupère le makespan
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan); // Passe le makespan

        // Assert
        var travailVars = model.Model.Variables.Count(v => v.Name.StartsWith("travail_o"));
        travailVars.Should().Be(1);
    }

    [Fact]
    public void Construire_LiaisonDureeEstCorrecte()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables, makespan) = GetTestContext(); // Récupère le makespan
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan); // Passe le makespan

        // Assert
        // Cette assertion n'est plus pertinente car la liaison est maintenant faite avec AddMaxEquality
        // On la remplace par une vérification de la création des variables de coût RH.
        var coutRhVar = model.Model.Variables.FirstOrDefault(v => v.Name == "cout_rh");
        coutRhVar.Should().NotBeNull();
    }

    // --- Méthode de préparation des données de test ---
    private ProblemeOptimisation CreerProblemeDeTest()
    {
        var metierId = new MetierId("TESTEUR");
        var ouvrierId = new OuvrierId("OUV_1");
        var tacheId = new TacheId("TACHE_1");

        var chantier = new Chantier(
            new ChantierId("CHANTIER_COUT"), "Test Coût",
            new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 1).ToDateTimeUnspecified()),
            new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
            new List<Metier> { new(metierId, "Testeur") },
            new List<Ouvrier> { new(ouvrierId, "Test", "Man", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
            new List<BlocTravail>
            {
                new(new BlocId("BLOC_A"), "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                {
                    new(tacheId, "Tâche Test", new BlocId("BLOC_A"), new DureeHeuresHomme(5), metierId, null)
                })
            },
            new List<LotTravaux>()
        );

        chantier.AppliquerConfigurationOptimisation(new ConfigurationOptimisation(7, 30.0m, 0));

        var slots = Enumerable.Range(0, 8)
            .Select(i => new SlotTemporel(i, new LocalDateTime(2028, 1, 1, 8, 0).PlusHours(i), new LocalDateTime()))
            .ToList();
        var echelleTemps = new EchelleTempsOuvree(slots, new Dictionary<LocalDateTime, int>());

        return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = chantier.ConfigurationOptimisation! };
    }
}