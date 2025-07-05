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

public class TacheModelBuilderTests
{
    private (CpModel model, ProblemeOptimisation probleme) GetTestContext()
    {
        var model = new CpModel();
        var probleme = CreerProblemeDeTestSimple();
        return (model, probleme);
    }

    [Fact]
    public void Construire_AvecChantierSimple_CreeLeBonNombreDeVariables()
    {
        // Arrange
        var (model, probleme) = GetTestContext();
        var builder = new TacheModelBuilder();

        // Act
        var (tachesIntervals, tachesAssignables, _) = builder.Construire(model, probleme);

        // Assert
        tachesIntervals.Should().HaveCount(2);
        tachesAssignables.Should().HaveCount(2);
    }

    [Fact]
    public void Construire_AvecChantierSimple_AjouteLesBonnesContraintes()
    {
        // Arrange
        var (model, probleme) = GetTestContext();
        var builder = new TacheModelBuilder();
        int contraintesInitiales = model.Model.Constraints.Count;

        // Act
        builder.Construire(model, probleme);

        // Assert
        // POURQUOI : Le comptage est mis à jour pour refléter la réalité du code.
        // - 4 contraintes pour les 2 NewOptionalIntervalVar
        // - 2 contraintes AddExactlyOne
        // - 1 contrainte de dépendance
        // - 1 contrainte AddMaxEquality (makespan)
        // - 4 contraintes pour les 2 tâches via AddModuloEquality (2 par tâche)
        // Total = 12
        int contraintesAttendues = 12;
        model.Model.Constraints.Count.Should().Be(contraintesInitiales + contraintesAttendues);
    }

    [Fact]
    public void Construire_QuandTacheSansOuvrier_LeveUneException()
    {
        // Arrange
        var (model, probleme) = (new CpModel(), CreerProblemeDeTest_TacheOrpheline());
        var builder = new TacheModelBuilder();

        // Act
        var action = () => builder.Construire(model, probleme);

        // Assert
        action.Should().Throw<InvalidOperationException>()
              .WithMessage("Aucun ouvrier compétent trouvé pour la tâche TACHE_ORPHELINE");
    }

    // --- Méthodes de préparation des données de test ---
    private ProblemeOptimisation CreerProblemeDeTestSimple()
    {
        var maconId = new MetierId("MACON");
        var peintreId = new MetierId("PEINTRE");
        var tache1Id = new TacheId("TACHE_1");
        var tache2Id = new TacheId("TACHE_2");
        var ouvrier1Id = new OuvrierId("OUV_1");
        var ouvrier2Id = new OuvrierId("OUV_2");
        var dateDebut = new LocalDate(2028, 1, 1);
        var dateFin = new LocalDate(2028, 1, 31);

        var chantier = new Chantier(
            new ChantierId("CHANTIER_TEST"), "Test",
            new PeriodePlanification(dateDebut.ToDateTimeUnspecified(), dateFin.ToDateTimeUnspecified()),
            new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
            new List<Metier> { new(maconId, "Maçon"), new(peintreId, "Peintre") },
            new List<Ouvrier>
            {
                new(ouvrier1Id, "Durand", "Jean", new CoutJournalier(300), new List<Competence> { new(maconId, NiveauExpertise.Confirme) }),
                new(ouvrier2Id, "Dupont", "Alice", new CoutJournalier(350), new List<Competence> { new(peintreId, NiveauExpertise.Expert) })
            },
            new List<BlocTravail>
            {
                new(new BlocId("BLOC_A"), "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                {
                    new(tache1Id, "Mur", new BlocId("BLOC_A"), new DureeHeuresHomme(8), maconId, null),
                    new(tache2Id, "Peinture", new BlocId("BLOC_A"), new DureeHeuresHomme(8), peintreId, new List<TacheId> { tache1Id })
                })
            },
            new List<LotTravaux>()
        );
        var echelleTemps = new EchelleTempsOuvree(Enumerable.Range(0, 100).Select(i => new SlotTemporel(i, new LocalDateTime(), new LocalDateTime())).ToList(), new Dictionary<LocalDateTime, int>());
        return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps };
    }

    private ProblemeOptimisation CreerProblemeDeTest_TacheOrpheline()
    {
        var maconId = new MetierId("MACON");
        var electricienId = new MetierId("ELECTRICIEN");
        var dateDebut = new LocalDate(2028, 1, 1);
        var dateFin = new LocalDate(2028, 1, 31);
        var chantier = new Chantier(
            new ChantierId("CHANTIER_ORPHELIN"), "Test Orphelin",
            new PeriodePlanification(dateDebut.ToDateTimeUnspecified(), dateFin.ToDateTimeUnspecified()),
            new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
            new List<Metier> { new(maconId, "Maçon"), new(electricienId, "Electricien") },
            new List<Ouvrier> { new(new OuvrierId("OUV_1"), "Durand", "Jean", new CoutJournalier(300), new List<Competence> { new(maconId, NiveauExpertise.Confirme) }) },
            new List<BlocTravail>
            {
                new(new BlocId("BLOC_A"), "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                {
                    new(new TacheId("TACHE_ORPHELINE"), "Câblage", new BlocId("BLOC_A"), new DureeHeuresHomme(8), electricienId, null)
                })
            },
            new List<LotTravaux>()
        );
        var echelleTemps = new EchelleTempsOuvree(Enumerable.Range(0, 100).Select(i => new SlotTemporel(i, new LocalDateTime(), new LocalDateTime())).ToList(), new Dictionary<LocalDateTime, int>());
        return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps };
    }
}