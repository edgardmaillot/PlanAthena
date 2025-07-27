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
    public class TacheModelBuilderTests
    {
        private readonly TacheModelBuilder _builder;

        public TacheModelBuilderTests()
        {
            _builder = new TacheModelBuilder();
        }

        // *** NOUVEAU TEST: Validation de la nouvelle signature ***
        [Fact]
        public void Construire_RetourneSixElements()
        {
            // Arrange
            var model = new CpModel();
            var probleme = CreerProblemeDeTest();

            // Act
            var (tachesIntervals, tachesAssignables, makespan, dureesOriginales, typesActivites, nomsActivites) =
                _builder.Construire(model, probleme);

            // Assert
            tachesIntervals.Should().NotBeNull();
            tachesAssignables.Should().NotBeNull();
            makespan.Should().NotBeNull();
            dureesOriginales.Should().NotBeNull();
            typesActivites.Should().NotBeNull();
            nomsActivites.Should().NotBeNull();
        }

        // *** NOUVEAU TEST: Vérification des métadonnées pour tâches normales ***
        [Fact]
        public void Construire_AvecTacheNormale_PreserveMetadonnees()
        {
            // Arrange
            var model = new CpModel();
            var probleme = CreerProblemeAvecTacheNormale();

            // Act
            var (_, _, _, dureesOriginales, typesActivites, nomsActivites) =
                _builder.Construire(model, probleme);

            // Assert
            var tacheId = new TacheId("TACHE_1");

            dureesOriginales.Should().ContainKey(tacheId);
            dureesOriginales[tacheId].Should().Be(8.0); // 8 heures

            typesActivites.Should().ContainKey(tacheId);
            typesActivites[tacheId].Should().Be(TypeActivite.Tache);

            nomsActivites.Should().ContainKey(tacheId);
            nomsActivites[tacheId].Should().Be("Tâche Normale Test");
        }

        // *** NOUVEAU TEST: Vérification des métadonnées pour jalons ***
        [Fact]
        public void Construire_AvecJalon_PreserveMetadonnees()
        {
            // Arrange
            var model = new CpModel();
            var probleme = CreerProblemeAvecJalon();

            // Act
            var (_, _, _, dureesOriginales, typesActivites, nomsActivites) =
                _builder.Construire(model, probleme);

            // Assert
            var jalonId = new TacheId("JALON_1");

            dureesOriginales.Should().ContainKey(jalonId);
            dureesOriginales[jalonId].Should().Be(72.0); // 72 heures de séchage

            typesActivites.Should().ContainKey(jalonId);
            typesActivites[jalonId].Should().Be(TypeActivite.JalonUtilisateur);

            nomsActivites.Should().ContainKey(jalonId);
            nomsActivites[jalonId].Should().Be("Séchage Béton");
        }

        // *** NOUVEAU TEST: Mélange tâches et jalons ***
        [Fact]
        public void Construire_AvecTachesEtJalons_PreserveToutesLesMetadonnees()
        {
            // Arrange
            var model = new CpModel();
            var probleme = CreerProblemeComplet();

            // Act
            var (tachesIntervals, tachesAssignables, makespan, dureesOriginales, typesActivites, nomsActivites) =
                _builder.Construire(model, probleme);

            // Assert
            var tacheId = new TacheId("TACHE_1");
            var jalonId = new TacheId("JALON_1");

            // Vérifier qu'on a bien les deux activités
            tachesIntervals.Should().HaveCount(2);
            tachesAssignables.Should().NotBeEmpty();

            // Vérifier les métadonnées de la tâche
            typesActivites[tacheId].Should().Be(TypeActivite.Tache);
            dureesOriginales[tacheId].Should().Be(5.0);

            // Vérifier les métadonnées du jalon
            typesActivites[jalonId].Should().Be(TypeActivite.JalonUtilisateur);
            dureesOriginales[jalonId].Should().Be(24.0);
        }

        // *** NOUVEAU TEST: Validation des intervalles ***
        [Fact]
        public void Construire_CreeLeBonNombreDeVariablesInterval()
        {
            // Arrange
            var model = new CpModel();
            var probleme = CreerProblemeComplet();

            // Act
            var (tachesIntervals, _, _, _, _, _) = _builder.Construire(model, probleme);

            // Assert
            tachesIntervals.Should().HaveCount(2); // 1 tâche + 1 jalon

            foreach (var interval in tachesIntervals.Values)
            {
                interval.Should().NotBeNull();
                interval.ToString().Should().Contain("interval_");

            }
        }

        // *** NOUVEAU TEST: Validation des assignations ***
        [Fact]
        public void Construire_CreeLeBonNombreDeVariablesAssignation()
        {
            // Arrange
            var model = new CpModel();
            var probleme = CreerProblemeComplet();

            // Act
            var (_, tachesAssignables, _, _, _, _) = _builder.Construire(model, probleme);

            // Assert
            // 2 tâches × 1 ouvrier compétent = 2 assignations
            tachesAssignables.Should().HaveCount(2);

            foreach (var assignation in tachesAssignables.Values)
            {
                assignation.Should().NotBeNull();
                assignation.Proto.Name.Should().StartWith("estAssignable_");
            }
        }

        // *** NOUVEAU TEST: Validation du makespan ***
        [Fact]
        public void Construire_CreeMakespanAvecBornesCorrects()
        {
            // Arrange
            var model = new CpModel();
            var probleme = CreerProblemeDeTest();

            // Act
            var (_, _, makespan, _, _, _) = _builder.Construire(model, probleme);

            // Assert
            makespan.Should().NotBeNull();
            makespan.Proto.Name.Should().Be("makespan");
            makespan.Should().NotBeNull();
        }

        // --- Méthodes utilitaires pour créer différents types de problèmes ---

        private ProblemeOptimisation CreerProblemeDeTest()
        {
            return CreerProblemeAvecTacheNormale();
        }

        private ProblemeOptimisation CreerProblemeAvecTacheNormale()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var tacheId = new TacheId("TACHE_1");
            var blocId = new BlocId("BLOC_A");

            var chantier = new Chantier(
                new ChantierId("CHANTIER_TEST"), "Test Normal",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 2).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Testeur") },
                new List<Ouvrier> { new(ouvrierId, "Test", "Man", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(tacheId, "Tâche Normale Test", TypeActivite.Tache, blocId, new DureeHeuresHomme(8), metierId, null)
                    })
                },
                new List<LotTravaux>()
            );

            return CreerProblemeOptimisation(chantier);
        }

        private ProblemeOptimisation CreerProblemeAvecJalon()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var jalonId = new TacheId("JALON_1");
            var blocId = new BlocId("BLOC_A");

            var chantier = new Chantier(
                new ChantierId("CHANTIER_JALON"), "Test Jalon",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 5).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Testeur") },
                new List<Ouvrier> { new(ouvrierId, "Test", "Man", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(jalonId, "Séchage Béton", TypeActivite.JalonUtilisateur, blocId, new DureeHeuresHomme(72), metierId, null)
                    })
                },
                new List<LotTravaux>()
            );

            return CreerProblemeOptimisation(chantier);
        }

        private ProblemeOptimisation CreerProblemeComplet()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var tacheId = new TacheId("TACHE_1");
            var jalonId = new TacheId("JALON_1");
            var blocId = new BlocId("BLOC_A");

            var chantier = new Chantier(
                new ChantierId("CHANTIER_COMPLET"), "Test Complet",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 5).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Testeur") },
                new List<Ouvrier> { new(ouvrierId, "Test", "Man", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(tacheId, "Tâche Normale", TypeActivite.Tache, blocId, new DureeHeuresHomme(5), metierId, null),
                        new(jalonId, "Séchage", TypeActivite.JalonUtilisateur, blocId, new DureeHeuresHomme(24), metierId, null)
                    })
                },
                new List<LotTravaux>()
            );

            return CreerProblemeOptimisation(chantier);
        }

        private ProblemeOptimisation CreerProblemeOptimisation(Chantier chantier)
        {
            chantier.AppliquerConfigurationOptimisation(new ConfigurationOptimisation(7, 30.0m, 0));

            // Création d'une échelle de temps valide pour le test (3 jours × 8h)
            var slots = new List<SlotTemporel>();
            var indexLookup = new Dictionary<LocalDateTime, int>();
            int index = 0;

            for (int jour = 0; jour < 3; jour++)
            {
                for (int heure = 0; heure < 8; heure++)
                {
                    var debutSlot = new LocalDateTime(2028, 1, 1 + jour, 8 + heure, 0);
                    var finSlot = debutSlot.PlusHours(1);
                    var slot = new SlotTemporel(index, debutSlot, finSlot);
                    slots.Add(slot);
                    indexLookup[debutSlot] = index;
                    index++;
                }
            }

            var echelleTemps = new EchelleTempsOuvree(slots, indexLookup);

            return new ProblemeOptimisation
            {
                Chantier = chantier,
                EchelleTemps = echelleTemps,
                Configuration = chantier.ConfigurationOptimisation!
            };
        }
    }
}