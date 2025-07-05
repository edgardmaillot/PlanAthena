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
    private (CpModel model, ProblemeOptimisation probleme, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables) GetTestContext()
    {
        var model = new CpModel();
        var probleme = CreerProblemeDeTest();
        var tacheBuilder = new TacheModelBuilder();
        var (tachesIntervals, tachesAssignables, _) = tacheBuilder.Construire(model, probleme);
        return (model, probleme, tachesIntervals, tachesAssignables);
    }

    [Fact]
    public void Construire_CreeLeBonNombreDeVariablesDeCout()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables) = GetTestContext();
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables);

        // Assert
        // CORRECTION : Utilisation de model.Model pour accéder au proto
        var coutVars = model.Model.Variables.Count(v => v.Name.StartsWith("cout_") && v.Name.Contains("2028"));
        coutVars.Should().Be(1);
    }

    [Fact]
    public void Construire_CreeLeBonNombreDeVariablesDeDuree()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables) = GetTestContext();
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables);

        // Assert
        // CORRECTION : Le filtre est plus précis pour correspondre au nommage de votre code.
        var dureeVars = model.Model.Variables.Count(v => v.Name.StartsWith("duree_o"));
        dureeVars.Should().Be(1);
    }

    [Fact]
    public void Construire_LiaisonDureeEstCorrecte()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables) = GetTestContext();
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables);

        // Assert
        // CORRECTION : On vérifie la présence de la contrainte qui lie la durée à la somme des contributions.
        var contrainteDeLiaison = model.Model.Constraints
            .FirstOrDefault(c => c.Linear != null && c.Linear.Vars.Any(i => model.Model.Variables[i].Name.StartsWith("travail_o")));

        contrainteDeLiaison.Should().NotBeNull();
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

        chantier.AppliquerConfigurationOptimisation(new ConfigurationOptimisation(7, 30.0m));

        var slots = Enumerable.Range(0, 8)
            .Select(i => new SlotTemporel(i, new LocalDateTime(2028, 1, 1, 8, 0).PlusHours(i), new LocalDateTime()))
            .ToList();
        var echelleTemps = new EchelleTempsOuvree(slots, new Dictionary<LocalDateTime, int>());

        return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = chantier.ConfigurationOptimisation! };
    }
}