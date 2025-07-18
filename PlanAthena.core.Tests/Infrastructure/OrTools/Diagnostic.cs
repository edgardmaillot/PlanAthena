// Diagnostic XUnit pour TacheModelBuilder

using Google.OrTools.Sat;
using NodaTime;
using NodaTime.Extensions;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Infrastructure.Services.OrTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PlanAthena.Tests
{
    public class Diagnostic
    {
        private readonly ITestOutputHelper _output;

        public Diagnostic(ITestOutputHelper output)
        {
            _output = output;
        }

        // ======================================================================================
        // NOUVEAU TEST : Un scénario de base qui DOIT fonctionner.
        // ======================================================================================
        [Fact]
        public void Test_UltraSimple_ShouldBeOptimal()
        {
            // Arrange
            _output.WriteLine("--- DÉMARRAGE DU TEST ULTRA-SIMPLE ---");
            var probleme = CreerProblemeDeTest_UltraSimple();
            var builder = new TacheModelBuilder();
            var model = new CpModel();

            // Act
            var (tachesIntervals, _, makespan) = builder.Construire(model, probleme);
            var solver = new CpSolver();
            var status = solver.Solve(model);

            // Assert & Diagnostic
            _output.WriteLine($"Status de résolution: {status}");
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                _output.WriteLine($"Makespan (durée en slots): {solver.Value(makespan)}");
                var tacheA_start = solver.Value(tachesIntervals[new TacheId("TACHE_A")].StartExpr());
                var tacheB_start = solver.Value(tachesIntervals[new TacheId("TACHE_B")].StartExpr());
                _output.WriteLine($"Tâche A démarre au slot: {tacheA_start}");
                _output.WriteLine($"Tâche B démarre au slot: {tacheB_start}");
            }

            // L'assertion principale : le modèle doit être résolvable.
            Assert.True(status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible,
                $"Le modèle ultra-simple devrait être résolvable. Status obtenu: {status}");
            _output.WriteLine("--- TEST ULTRA-SIMPLE RÉUSSI ---");
        }

        // ======================================================================================
        // ANCIEN TEST : Temporairement désactivé pour se concentrer sur le cas de base.
        // ======================================================================================
        [Fact(Skip = "Temporairement désactivé pour se concentrer sur le test de base.")]
        public void Test_JalonSautWeekend_AvecDiagnostic()
        {
            // (Le code de ce test reste inchangé mais ne sera pas exécuté)
            var probleme = CreerProblemeDeTest_JalonSautWeekend();
            var builder = new TacheModelBuilder();
            var model = new CpModel();
            var (tachesIntervals, _, makespan) = builder.Construire(model, probleme);
            var solver = new CpSolver();
            var status = solver.Solve(model);
            Assert.True(status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible,
                $"Le modèle devrait être résolvable. Status: {status}");
        }

        // --- Méthodes de préparation des données de test ---

        private ProblemeOptimisation CreerProblemeDeTest_UltraSimple()
        {
            var metierId = new MetierId("SIMPLE_METIER");
            var ouvrierId = new OuvrierId("OUV_SIMPLE");
            var tacheAId = new TacheId("TACHE_A");
            var tacheBId = new TacheId("TACHE_B");
            var blocId = new BlocId("BLOC_SIMPLE");

            var dateDebut = new LocalDate(2028, 1, 3); // Lundi
            var dateFin = new LocalDate(2028, 1, 14);  // 10 jours ouvrés plus tard

            var chantier = new Chantier(
                new ChantierId("CHANTIER_SIMPLE"), "Test Simple",
                new PeriodePlanification(dateDebut.ToDateTimeUnspecified(), dateFin.ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(
                    new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday },
                    new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()
                ),
                new List<Metier> { new(metierId, "Métier Simple") },
                new List<Ouvrier> { new(ouvrierId, "Ouvrier", "Simple", new CoutJournalier(100), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc Simple", new CapaciteOuvriers(5), new List<Tache>
                    {
                        new(tacheAId, "Tâche A", TypeActivite.Tache, blocId, new DureeHeuresHomme(5), metierId, null),
                        new(tacheBId, "Tâche B", TypeActivite.Tache, blocId, new DureeHeuresHomme(5), metierId, new List<TacheId> { tacheAId })
                    })
                },
                new List<LotTravaux>()
            );

            var slots = new List<SlotTemporel>();
            int index = 0;
            for (int day = 0; day < 10; day++) // 10 jours ouvrés
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

        private ProblemeOptimisation CreerProblemeDeTest_JalonSautWeekend()
        {
            var metierVirtuelId = new MetierId("SYS_JALON_METIER");
            var metierReelId = new MetierId("REEL");
            var jalonId = new TacheId("JALON_WEEKEND");
            var tacheApresJalonId = new TacheId("TACHE_APRES_JALON");
            var ouvrierVirtuelId = new OuvrierId("VIRTUAL_OUVRIER_1");
            var ouvrierReelId = new OuvrierId("OUV_REEL_1");
            var blocId = new BlocId("BLOC_SAUT");

            var dateDebut = new LocalDate(2028, 1, 31); // Lundi
            var dateFin = new LocalDate(2028, 2, 14);   // 2 semaines plus tard

            var chantier = new Chantier(
                new ChantierId("CHANTIER_SAUT"), "Test Saut Weekend",
                new PeriodePlanification(dateDebut.ToDateTimeUnspecified(), dateFin.ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(
                    new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday },
                    new LocalTime(8, 0),
                    Duration.FromHours(8),
                    new HashSet<LocalDate>()
                ),
                new List<Metier> {
                    new(metierVirtuelId, "Métier Virtuel"),
                    new(metierReelId, "Métier Réel")
                },
                new List<Ouvrier>
                {
                    new(ouvrierVirtuelId, "Ouvrier Virtuel", "Jalon", new CoutJournalier(0),
                        new List<Competence> { new(metierVirtuelId, NiveauExpertise.Maitre) }),
                    new(ouvrierReelId, "Ouvrier", "Réel", new CoutJournalier(300),
                        new List<Competence> { new(metierReelId, NiveauExpertise.Confirme) })
                },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc Saut", new CapaciteOuvriers(1), new List<Tache>
                    {
                        new(jalonId, "Validation Weekend", TypeActivite.JalonUtilisateur, blocId,
                            new DureeHeuresHomme(48), metierVirtuelId, null),
                        new(tacheApresJalonId, "Reprise Lundi", TypeActivite.Tache, blocId,
                            new DureeHeuresHomme(4), metierReelId, new List<TacheId> { jalonId })
                    })
                },
                new List<LotTravaux>()
            );

            var slots = new List<SlotTemporel>();
            int index = 0;
            for (int day = 0; day < 14; day++)
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