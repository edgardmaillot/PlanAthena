// PlanAthena.core.Tests/Infrastructure/OrTools/TacheModelBuilderTests.cs

using FluentAssertions;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Infrastructure.Services.OrTools;

namespace PlanAthena.core.Tests.Infrastructure.OrTools
{
    public class TacheModelBuilderTests_old
    {
        [Fact]
        public void Construire_AvecChantierSimple_CreeLeBonNombreDeVariables()
        {
            var (model, probleme) = CreerProblemeDeTestSimple();
            var builder = new TacheModelBuilder();
            var (tachesIntervals, tachesAssignables, _) = builder.Construire(model, probleme);
            tachesIntervals.Should().HaveCount(2);
            tachesAssignables.Should().HaveCount(2);
        }

        [Fact]
        public void Construire_AvecChantierSimple_AjouteLesBonnesContraintes()
        {
            var (model, probleme) = CreerProblemeDeTestSimple();
            var builder = new TacheModelBuilder();
            int contraintesInitiales = model.Model.Constraints.Count;
            builder.Construire(model, probleme);
            int contraintesAttendues = 12;
            model.Model.Constraints.Count.Should().Be(contraintesInitiales + contraintesAttendues);
        }

        [Fact]
        public void Construire_QuandTacheSansOuvrier_LeveUneException()
        {
            var (model, probleme) = CreerProblemeDeTest_TacheOrpheline();
            var builder = new TacheModelBuilder();
            var action = () => builder.Construire(model, probleme);
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("Aucun ouvrier compétent trouvé pour la tâche TACHE_ORPHELINE (Câblage) - Métier: ELECTRICIEN");
        }

        [Fact]
        public void Construire_AvecJalon_CreeLesVariablesDeCanalisation()
        {
            var (model, probleme) = CreerProblemeDeTest_AvecJalon();
            var builder = new TacheModelBuilder();
            builder.Construire(model, probleme);
            model.Model.Variables.Should().Contain(v => v.Name.StartsWith("start_abs_JALON_1"));
            model.Model.Variables.Should().Contain(v => v.Name.StartsWith("end_abs_JALON_1"));
            model.Model.Variables.Should().Contain(v => v.Name.StartsWith("start_slot_JALON_1"));
        }

        [Fact]
        public void Construire_AvecJalonSautantUnWeekend_PositionneCorrectementLaTacheSuivante()
        {
            // Arrange
            var model = new CpModel();
            var probleme = CreerProblemeDeTest_JalonSautWeekend();
            var builder = new TacheModelBuilder();

            var (tachesIntervals, _, _) = builder.Construire(model, probleme);

            var jalonId = new TacheId("JALON_WEEKEND");
            var tacheApresJalonId = new TacheId("TACHE_APRES_JALON");
            var jalonInterval = tachesIntervals[jalonId];
            var tacheSuivanteInterval = tachesIntervals[tacheApresJalonId];

            // On force le jalon à commencer le vendredi à 16h (dernier slot de la semaine, index 39)
            model.Add(jalonInterval.StartExpr() == 39);

            // Act
            var solver = new CpSolver();
            var status = solver.Solve(model);

            // Assert
            status.Should().BeOneOf(CpSolverStatus.Optimal, CpSolverStatus.Feasible);

            // Le jalon dure 48h (vendredi 16h -> dimanche 16h).
            // La tâche suivante doit donc commencer au plus tôt au premier slot disponible après,
            // soit le lundi matin à 8h, qui est le slot 40.
            solver.Value(tacheSuivanteInterval.StartExpr()).Should().BeGreaterThanOrEqualTo(40);
        }


        // --- Méthodes de préparation des données de test ---
        private (CpModel, ProblemeOptimisation) CreerProblemeDeTestSimple()
        {
            var model = new CpModel();
            var maconId = new MetierId("MACON");
            var peintreId = new MetierId("PEINTRE");
            var tache1Id = new TacheId("TACHE_1");
            var tache2Id = new TacheId("TACHE_2");
            var ouvrier1Id = new OuvrierId("OUV_1");
            var ouvrier2Id = new OuvrierId("OUV_2");
            var blocId = new BlocId("BLOC_A");
            var chantier = new Chantier(
                new ChantierId("CHANTIER_TEST"), "Test",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 31).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(maconId, "Maçon"), new(peintreId, "Peintre") },
                new List<Ouvrier>
                {
                    new(ouvrier1Id, "Durand", "Jean", new CoutJournalier(300), new List<Competence> { new(maconId, NiveauExpertise.Confirme) }),
                    new(ouvrier2Id, "Dupont", "Alice", new CoutJournalier(350), new List<Competence> { new(peintreId, NiveauExpertise.Expert) })
                },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(tache1Id, "Mur", TypeActivite.Tache, blocId, new DureeHeuresHomme(8), maconId, null),
                        new(tache2Id, "Peinture", TypeActivite.Tache, blocId, new DureeHeuresHomme(8), peintreId, new List<TacheId> { tache1Id })
                    })
                },
                new List<LotTravaux>()
            );
            var echelleTemps = new EchelleTempsOuvree(
                Enumerable.Range(0, 100).Select(i => new SlotTemporel(i, new LocalDateTime(2028, 1, 1, 8, 0).PlusHours(i), new LocalDateTime(2028, 1, 1, 8, 0).PlusHours(i + 1))).ToList(),
                new Dictionary<LocalDateTime, int>()
            );
            return (model, new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps });
        }

        private (CpModel, ProblemeOptimisation) CreerProblemeDeTest_TacheOrpheline()
        {
            var model = new CpModel();
            var maconId = new MetierId("MACON");
            var electricienId = new MetierId("ELECTRICIEN");
            var blocId = new BlocId("BLOC_A");
            var tacheId = new TacheId("TACHE_ORPHELINE");
            var chantier = new Chantier(
                new ChantierId("CHANTIER_ORPHELIN"), "Test Orphelin",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 31).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(maconId, "Maçon"), new(electricienId, "Electricien") },
                new List<Ouvrier> { new(new OuvrierId("OUV_1"), "Durand", "Jean", new CoutJournalier(300), new List<Competence> { new(maconId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(tacheId, "Câblage", TypeActivite.Tache, blocId, new DureeHeuresHomme(8), electricienId, null)
                    })
                },
                new List<LotTravaux>()
            );
            var echelleTemps = new EchelleTempsOuvree(
                Enumerable.Range(0, 100).Select(i => new SlotTemporel(i, new LocalDateTime(2028, 1, 1, 8, 0).PlusHours(i), new LocalDateTime(2028, 1, 1, 8, 0).PlusHours(i + 1))).ToList(),
                new Dictionary<LocalDateTime, int>()
            );
            return (model, new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps });
        }

        private (CpModel, ProblemeOptimisation) CreerProblemeDeTest_AvecJalon()
        {
            var model = new CpModel();
            var metierVirtuelId = new MetierId("SYS_JALON_METIER");
            var jalonId = new TacheId("JALON_1");
            var ouvrierVirtuelId = new OuvrierId("VIRTUAL_OUVRIER_1");
            var blocId = new BlocId("BLOC_JALON");
            var chantier = new Chantier(
                new ChantierId("CHANTIER_JALON"), "Test Jalon",
                new PeriodePlanification(new LocalDate(2028, 1, 3).ToDateTimeUnspecified(), new LocalDate(2028, 1, 4).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierVirtuelId, "Métier Virtuel pour Jalons") },
                new List<Ouvrier> { new(ouvrierVirtuelId, "Ouvrier Virtuel", "Jalon", new CoutJournalier(0), new List<Competence> { new(metierVirtuelId, NiveauExpertise.Maitre) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc Jalon", new CapaciteOuvriers(1), new List<Tache>
                    {
                        new(jalonId, "Attente livraison", TypeActivite.JalonUtilisateur, blocId, new DureeHeuresHomme(24), metierVirtuelId, null)
                    })
                },
                new List<LotTravaux>()
            );
            var echelleTemps = new EchelleTempsOuvree(
                Enumerable.Range(0, 16).Select(i => new SlotTemporel(i, new LocalDateTime(2028, 1, 3, 8, 0).PlusHours(i % 8).PlusDays(i / 8), new LocalDateTime(2028, 1, 3, 8, 0).PlusHours((i % 8) + 1).PlusDays(i / 8))).ToList(),
                new Dictionary<LocalDateTime, int>()
            );
            return (model, new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps });
        }

        private ProblemeOptimisation CreerProblemeDeTest_JalonSautWeekend()
        {
            var metierVirtuelId = new MetierId("SYS_JALON_METIER");
            var metierReelId = new MetierId("REEL");
            var jalonId = new TacheId("JALON_WEEKEND");
            var tacheApresJalonId = new TacheId("TACHE_APRES_JALON");
            var ouvrierVirtuelId = new OuvrierId("VIRTUAL_OUVRIER_1");
            var ouvrierReelId = new OuvrierId("OUV_REEL_1");
            var blocId = new BlocId("BLOC_SAUT");

            // CORRECTION : On étend la période de planification pour inclure le résultat attendu.
            var dateDebut = new LocalDate(2028, 1, 31); // Lundi
            var dateFin = new LocalDate(2028, 2, 7);  // Lundi suivant

            var chantier = new Chantier(
                new ChantierId("CHANTIER_SAUT"), "Test Saut Weekend",
                new PeriodePlanification(dateDebut.ToDateTimeUnspecified(), dateFin.ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierVirtuelId, "Métier Virtuel"), new(metierReelId, "Métier Réel") },
                new List<Ouvrier>
                {
                    new(ouvrierVirtuelId, "Ouvrier Virtuel", "Jalon", new CoutJournalier(0), new List<Competence> { new(metierVirtuelId, NiveauExpertise.Maitre) }),
                    new(ouvrierReelId, "Ouvrier", "Réel", new CoutJournalier(300), new List<Competence> { new(metierReelId, NiveauExpertise.Confirme) })
                },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc Saut", new CapaciteOuvriers(1), new List<Tache>
                    {
                        // Jalon de 48h (weekend complet)
                        new(jalonId, "Validation Weekend", TypeActivite.JalonUtilisateur, blocId, new DureeHeuresHomme(48), metierVirtuelId, null),
                        new(tacheApresJalonId, "Reprise Lundi", TypeActivite.Tache, blocId, new DureeHeuresHomme(4), metierReelId, new List<TacheId> { jalonId })
                    })
                },
                new List<LotTravaux>()
            );

            var slots = new List<SlotTemporel>();
            int index = 0;
            for (int day = 0; day < 10; day++)
            {
                var dateCourante = dateDebut.PlusDays(day);
                if (chantier.Calendrier.EstJourOuvre(dateCourante))
                {
                    for (int hour = 0; hour < 8; hour++)
                    {
                        var debutSlot = dateCourante.At(chantier.Calendrier.HeureDebutTravail).PlusHours(hour);
                        slots.Add(new SlotTemporel(index++, debutSlot, debutSlot.PlusHours(1)));
                    }
                }
            }
            var echelleTemps = new EchelleTempsOuvree(slots, slots.ToDictionary(s => s.Debut, s => s.Index));

            return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps };
        }
    }
}