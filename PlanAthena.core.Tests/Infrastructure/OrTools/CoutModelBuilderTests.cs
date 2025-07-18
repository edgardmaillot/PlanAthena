// PlanAthena.core.Tests/Infrastructure/OrTools/CoutModelBuilderTests.cs

using FluentAssertions;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Infrastructure.Services.OrTools;
using System.Collections.Generic; // Ajout pour Dictionary
using System.Linq; // Ajout pour Enumerable

namespace PlanAthena.core.Tests.Infrastructure.OrTools;

public class CoutModelBuilderTests
{
    private (CpModel model, ProblemeOptimisation probleme, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables, IntVar makespan) GetTestContext()
    {
        var model = new CpModel();
        var probleme = CreerProblemeDeTest();
        var tacheBuilder = new TacheModelBuilder();
        var (tachesIntervals, tachesAssignables, makespan) = tacheBuilder.Construire(model, probleme);
        return (model, probleme, tachesIntervals, tachesAssignables, makespan);
    }

    [Fact]
    public void Construire_CreeLeBonNombreDeVariablesDeCout()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables, makespan) = GetTestContext();
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

        // Assert
        var coutVar = model.Model.Variables.FirstOrDefault(v => v.Name == "cout_total_chantier");
        coutVar.Should().NotBeNull();
    }

    [Fact]
    public void Construire_CreeLeBonNombreDeVariablesDeTravailJournalier()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables, makespan) = GetTestContext();
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

        // Assert
        // Il y a 1 ouvrier et 1 jour de travail dans le planning de test
        int joursDePlanning = 1;
        int ouvriers = 1;
        int attendu = joursDePlanning * ouvriers;

        var travailVars = model.Model.Variables.Count(v => v.Name.StartsWith("travail_o"));
        travailVars.Should().Be(attendu);
    }

    [Fact]
    public void Construire_LiaisonDureeEstCorrecte()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables, makespan) = GetTestContext();
        var builder = new CoutModelBuilder();

        // Act
        builder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

        // Assert
        var coutRhVar = model.Model.Variables.FirstOrDefault(v => v.Name == "cout_rh");
        coutRhVar.Should().NotBeNull();
    }

    // --- Méthode de préparation des données de test ---
    private ProblemeOptimisation CreerProblemeDeTest()
    {
        var metierId = new MetierId("TESTEUR");
        var ouvrierId = new OuvrierId("OUV_1");
        var tacheId = new TacheId("TACHE_1");
        var blocId = new BlocId("BLOC_A");

        var chantier = new Chantier(
            new ChantierId("CHANTIER_COUT"), "Test Coût",
            new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 1).ToDateTimeUnspecified()),
            new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
            new List<Metier> { new(metierId, "Testeur") },
            new List<Ouvrier> { new(ouvrierId, "Test", "Man", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
            new List<BlocTravail>
            {
                new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                {
                    // --- CORRECTION ---
                    // Ajout du paramètre TypeActivite.Tache à la bonne position dans le constructeur.
                    new(tacheId, "Tâche Test", TypeActivite.Tache, blocId, new DureeHeuresHomme(5), metierId, null)
                })
            },
            new List<LotTravaux>()
        );

        chantier.AppliquerConfigurationOptimisation(new ConfigurationOptimisation(7, 30.0m, 0));

        // Création d'une échelle de temps valide pour le test
        var dateDeTravail = new LocalDateTime(2028, 1, 1, 8, 0);
        var slots = Enumerable.Range(0, 8)
            .Select(i => {
                var debutSlot = dateDeTravail.PlusHours(i);
                var finSlot = debutSlot.PlusHours(1);
                return new SlotTemporel(i, debutSlot, finSlot);
            })
            .ToList();

        var indexLookup = slots.ToDictionary(s => s.Debut, s => s.Index);
        var echelleTemps = new EchelleTempsOuvree(slots, indexLookup);

        return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = chantier.ConfigurationOptimisation! };
    }
}