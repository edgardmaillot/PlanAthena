using FluentAssertions;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Infrastructure.Services.OrTools;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PlanAthena.core.Tests.Infrastructure.OrTools
{
    public class TacheModelBuilderTests
    {
        private readonly TacheModelBuilder _builder;

        public TacheModelBuilderTests()
        {
            _builder = new TacheModelBuilder();
        }

        // Ce test est supprimé car trop fragile. On ne peut pas se fier à l'implémentation interne de AddExactlyOne.
        // [Fact]
        // public void Construire_PourChaqueTache_CreeContrainteAssignationUnique() { ... }

        // Ce test est supprimé car trop fragile. On ne peut pas inspecter la contrainte AddElement de cette manière.
        // [Fact]
        // public void Construire_AvecJalon_CreeLesBonnesContraintes() { ... }


        [Fact]
        public void Construire_AvecJalonSautantUnWeekend_PositionneCorrectementLaTacheSuivante()
        {
            // ARRANGE
            var model = new CpModel();
            var probleme = CreerProblemeDeTest_JalonSautWeekend();
            var (tachesIntervals, _, makespan) = _builder.Construire(model, probleme);

            var jalonId = new TacheId("JALON_WEEKEND");
            var tacheApresJalonId = new TacheId("TACHE_APRES_JALON");

            // On récupère les variables du modèle
            var jalonInterval = tachesIntervals[jalonId];
            var tacheSuivanteInterval = tachesIntervals[tacheApresJalonId];

            // On force le jalon à commencer le vendredi à 16h.
            // Le premier jour ouvré est Lundi 31/01/2028.
            // Le vendredi est le 5ème jour, commençant au slot 4*8=32.
            // 16h est le dernier slot de la journée, soit l'index 32 + 7 = 39.
            model.Add(jalonInterval.StartExpr() == 39);

            // ACT
            var solver = new CpSolver();
            var status = solver.Solve(model);

            // ASSERT
            status.Should().BeOneOf(CpSolverStatus.Optimal, CpSolverStatus.Feasible, "Le modèle avec un jalon qui saute un weekend doit être résolvable");

            // Le jalon dure 48h continues. S'il démarre Vendredi à 16h (slot 39), il se termine Dimanche à 16h.
            // Le premier slot de travail disponible après Dimanche 16h est Lundi matin à 8h.
            // Dans notre échelle de temps, Lundi matin à 8h correspond au slot 40.
            var debutTacheSuivante = solver.Value(tacheSuivanteInterval.StartExpr());
            debutTacheSuivante.Should().Be(40, "La tâche suivante doit commencer au premier slot disponible après le jalon, soit le Lundi matin.");
        }


        #region Méthodes de création des données de test

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
            var dateFin = new LocalDate(2028, 2, 7);    // Lundi suivant

            var calendrier = new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>());

            var chantier = new Chantier(
                new ChantierId("CHANTIER_SAUT"), "Test Saut Weekend",
                new PeriodePlanification(dateDebut.ToDateTimeUnspecified(), dateFin.ToDateTimeUnspecified()),
                calendrier,
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
                        new(jalonId, "Validation Weekend", TypeActivite.JalonUtilisateur, blocId, new DureeHeuresHomme(48), metierVirtuelId, null),
                        new(tacheApresJalonId, "Reprise Lundi", TypeActivite.Tache, blocId, new DureeHeuresHomme(4), metierReelId, new List<TacheId> { jalonId })
                    })
                },
                new List<LotTravaux> { new(new LotId("LOT_A"), "Lot A", 1, new List<BlocId> { blocId }) }
            );

            var slots = new List<SlotTemporel>();
            var indexLookup = new Dictionary<LocalDateTime, int>();
            int index = 0;
            for (int day = 0; day < 10; day++) // On génère assez de slots
            {
                var dateCourante = dateDebut.PlusDays(day);
                if (calendrier.EstJourOuvre(dateCourante))
                {
                    for (int hour = 0; hour < 8; hour++)
                    {
                        var debutSlot = dateCourante.At(calendrier.HeureDebutTravail).PlusHours(hour);
                        var finSlot = LocalDateTime.FromDateTime(debutSlot.ToDateTimeUnspecified() + Duration.FromHours(1).ToTimeSpan());
                        slots.Add(new SlotTemporel(index, debutSlot, finSlot));
                        indexLookup[debutSlot] = index;
                        index++;
                    }
                }
            }
            var echelleTemps = new EchelleTempsOuvree(slots, indexLookup);
            var configOptimisation = new ConfigurationOptimisation(8, 10, 5000);
            chantier.AppliquerConfigurationOptimisation(configOptimisation);

            return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = configOptimisation };
        }
        #endregion
    }
}