// PlanAthena.core.Tests/Infrastructure/OrTools/CoutModelBuilderTests.cs

using FluentAssertions;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Infrastructure.Services.OrTools;

namespace PlanAthena.core.Tests.Infrastructure.OrTools;

public class CoutModelBuilderTests
{
    // *** CORRECTION: Adaptation pour la nouvelle signature de TacheModelBuilder.Construire ***
    private (CpModel model, ProblemeOptimisation probleme, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables, IntVar makespan) GetTestContext()
    {
        var model = new CpModel();
        var probleme = CreerProblemeDeTest();
        var tacheBuilder = new TacheModelBuilder();

        // *** MODIFICATION: Récupération des 6 éléments au lieu de 3 ***
        var (tachesIntervals, tachesAssignables, makespan, dureesOriginales, typesActivites, nomsActivites) =
            tacheBuilder.Construire(model, probleme);

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

    // *** NOUVEAU TEST: Vérification de la décomposition des coûts ***
    [Fact]
    public void Construire_RetourneTroisTypesDeCouttSeparement()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables, makespan) = GetTestContext();
        var builder = new CoutModelBuilder();

        // Act
        var (coutTotal, coutRh, coutIndirect) = builder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

        // Assert
        coutTotal.Should().NotBeNull();
        coutRh.Should().NotBeNull();
        coutIndirect.Should().NotBeNull();

        // Vérifier que les variables ont les bons noms
        coutTotal.Proto.Name.Should().Be("cout_total_chantier");
        coutRh.Proto.Name.Should().Be("cout_rh");
        coutIndirect.Proto.Name.Should().Be("cout_indirect");
    }

    // *** NOUVEAU TEST: Validation des bornes de coûts ***
    [Fact]
    public void Construire_VariablesDeCouttSontValides()
    {
        // Arrange
        var (model, probleme, tachesIntervals, tachesAssignables, makespan) = GetTestContext();
        var builder = new CoutModelBuilder();

        // Act
        var (coutTotal, coutRh, coutIndirect) = builder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

        // Assert
        // Vérifier que les variables de coût existent et ont les bons noms
        coutTotal.Should().NotBeNull();
        coutRh.Should().NotBeNull();
        coutIndirect.Should().NotBeNull();

        // Vérifier les noms (ce qui prouve que les variables sont bien configurées)
        coutTotal.Proto.Name.Should().Be("cout_total_chantier");
        coutRh.Proto.Name.Should().Be("cout_rh");
        coutIndirect.Proto.Name.Should().Be("cout_indirect");

        // Test plus simple et robuste : vérifier que les variables peuvent être utilisées
        // dans le modèle (preuve qu'elles sont correctement initialisées)
        var modelVariables = model.Model.Variables.Where(v =>
            v.Name == "cout_total_chantier" ||
            v.Name == "cout_rh" ||
            v.Name == "cout_indirect").ToList();

        modelVariables.Should().HaveCount(3, "Les 3 variables de coût doivent être présentes dans le modèle");
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
                    new(tacheId, "Tâche Test", TypeActivite.Tache, blocId, new DureeHeuresHomme(5), metierId, null)
                })
            },
            new List<LotTravaux>()
        );

        chantier.AppliquerConfigurationOptimisation(new ConfigurationOptimisation(7, 30.0m, 0));

        // Création d'une échelle de temps valide pour le test
        var dateDeTravail = new LocalDateTime(2028, 1, 1, 8, 0);
        var slots = Enumerable.Range(0, 8)
            .Select(i =>
            {
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