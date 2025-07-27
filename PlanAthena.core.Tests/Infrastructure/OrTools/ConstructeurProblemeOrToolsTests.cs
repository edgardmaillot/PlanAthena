// PlanAthena.core.Tests/Infrastructure/OrTools/ConstructeurProblemeOrToolsTests.cs

using FluentAssertions;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Infrastructure.Services.OrTools;

namespace PlanAthena.core.Tests.Infrastructure.OrTools
{
    public class ConstructeurProblemeOrToolsTests
    {
        private readonly ConstructeurProblemeOrTools _constructeur;

        public ConstructeurProblemeOrToolsTests()
        {
            _constructeur = new ConstructeurProblemeOrTools();
        }

        // *** TEST: Construction modèle complet ***
        [Fact]
        public void ConstruireModele_AvecProblemeValide_RetourneModelComplet()
        {
            // Arrange
            var probleme = CreerProblemeDeTest();

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "COUT");

            // Assert
            modele.Should().NotBeNull();
            modele.Model.Should().NotBeNull();
            modele.TachesIntervals.Should().NotBeNull();
            modele.TachesAssignables.Should().NotBeNull();
            modele.Makespan.Should().NotBeNull();
            modele.CoutTotal.Should().NotBeNull();
            modele.CoutRh.Should().NotBeNull();
            modele.CoutIndirect.Should().NotBeNull();
        }

        // *** TEST: Validation des métadonnées ***
        [Fact]
        public void ConstruireModele_InclutLesMetadonneesCompletes()
        {
            // Arrange
            var probleme = CreerProblemeDeTest();

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "COUT");

            // Assert
            modele.DureesOriginalesHeures.Should().NotBeNull();
            modele.TypesActivites.Should().NotBeNull();
            modele.NomsActivites.Should().NotBeNull();

            // Vérifier qu'on a les métadonnées pour toutes les tâches
            var toutesLesTaches = probleme.Chantier.ObtenirToutesLesTaches();
            modele.DureesOriginalesHeures.Should().HaveCount(toutesLesTaches.Count());
            modele.TypesActivites.Should().HaveCount(toutesLesTaches.Count());
            modele.NomsActivites.Should().HaveCount(toutesLesTaches.Count());
        }

        // *** TEST: Objectif DELAI ***
        [Fact]
        public void ConstruireModele_AvecObjectifDelai_OptimiseMakespan()
        {
            // Arrange
            var probleme = CreerProblemeDeTest();

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "DELAI");

            // Assert
            modele.Should().NotBeNull();
            modele.Makespan.Should().NotBeNull();

            // Vérifier que le modèle a été configuré pour minimiser le makespan
            // (Nous ne pouvons pas inspecter directement l'objectif, mais on vérifie que le makespan existe)
            //modele.Makespan.Proto.Name.Should().Be("makespan");
            modele.Makespan.Proto.Name.Should().Be("makespan");
        }

        // *** TEST: Objectif COUT par défaut ***
        [Fact]
        public void ConstruireModele_AvecObjectifCout_OptimiseCoutTotal()
        {
            // Arrange
            var probleme = CreerProblemeDeTest();

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "COUT");

            // Assert
            modele.Should().NotBeNull();
            modele.CoutTotal.Should().NotBeNull();
            modele.CoutTotal.Proto.Name.Should().Be("cout_total_chantier");
        }

        // *** TEST: Objectif par défaut ***
        [Fact]
        public void ConstruireModele_AvecObjectifInconnu_UtiliseCoutParDefaut()
        {
            // Arrange
            var probleme = CreerProblemeDeTest();

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "OBJECTIF_INEXISTANT");

            // Assert
            modele.Should().NotBeNull();
            modele.CoutTotal.Should().NotBeNull();
            // Par défaut, devrait optimiser le coût
        }

        // *** TEST: Cohérence des métadonnées avec les tâches ***
        [Fact]
        public void ConstruireModele_MetadonneesCorrespondentAuxTaches()
        {
            // Arrange
            var probleme = CreerProblemeAvecTachesVariees();

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "COUT");

            // Assert
            var taches = probleme.Chantier.ObtenirToutesLesTaches().ToList();

            foreach (var tache in taches)
            {
                // Vérifier les durées
                modele.DureesOriginalesHeures.Should().ContainKey(tache.Id);
                modele.DureesOriginalesHeures[tache.Id].Should().Be(tache.HeuresHommeEstimees.Value);

                // Vérifier les types
                modele.TypesActivites.Should().ContainKey(tache.Id);
                modele.TypesActivites[tache.Id].Should().Be(tache.Type);

                // Vérifier les noms
                modele.NomsActivites.Should().ContainKey(tache.Id);
                modele.NomsActivites[tache.Id].Should().Be(tache.Nom);
            }
        }

        // *** TEST: Modèle résolvable ***
        [Fact]
        public void ConstruireModele_ProduiModeleSolvable()
        {
            // Arrange
            var probleme = CreerProblemeSimple();

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "COUT");

            // Assert
            modele.Should().NotBeNull();
            modele.Model.Should().NotBeNull();

            // Vérifier que le modèle peut être résolu
            var solver = new Google.OrTools.Sat.CpSolver();
            var status = solver.Solve(modele.Model);

            status.Should().BeOneOf(
                Google.OrTools.Sat.CpSolverStatus.Optimal,
                Google.OrTools.Sat.CpSolverStatus.Feasible
            );
        }

        // --- Méthodes utilitaires ---

        private ProblemeOptimisation CreerProblemeDeTest()
        {
            return CreerProblemeAvecTachesVariees();
        }

        private ProblemeOptimisation CreerProblemeSimple()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var tacheId = new TacheId("TACHE_SIMPLE");
            var blocId = new BlocId("BLOC_SIMPLE");

            var chantier = new Chantier(
                new ChantierId("CHANTIER_SIMPLE"), "Test Simple",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 1).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Testeur") },
                new List<Ouvrier> { new(ouvrierId, "Test", "Simple", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc Simple", new CapaciteOuvriers(1), new List<Tache>
                    {
                        new(tacheId, "Tâche Simple", TypeActivite.Tache, blocId, new DureeHeuresHomme(4), metierId, null)
                    })
                },
                new List<LotTravaux>()
            );

            return CreerProblemeOptimisation(chantier, 1);
        }

        private ProblemeOptimisation CreerProblemeAvecTachesVariees()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var tacheId = new TacheId("TACHE_1");
            var jalonId = new TacheId("JALON_1");
            var jalonTechniqueId = new TacheId("JALON_TECH_1");
            var blocId = new BlocId("BLOC_VARIE");

            var chantier = new Chantier(
                new ChantierId("CHANTIER_VARIE"), "Test Varié",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 7).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Testeur") },
                new List<Ouvrier> { new(ouvrierId, "Test", "Varié", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc Varié", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(tacheId, "Tâche Normale", TypeActivite.Tache, blocId, new DureeHeuresHomme(8), metierId, null),
                        new(jalonId, "Séchage Utilisateur", TypeActivite.JalonUtilisateur, blocId, new DureeHeuresHomme(24), metierId, null),
                        new(jalonTechniqueId, "Jalon Technique", TypeActivite.JalonTechnique, blocId, new DureeHeuresHomme(12), metierId, null)
                    })
                },
                new List<LotTravaux>()
            );

            return CreerProblemeOptimisation(chantier, 5);
        }

        private ProblemeOptimisation CreerProblemeOptimisation(Chantier chantier, int nombreJours)
        {
            chantier.AppliquerConfigurationOptimisation(new ConfigurationOptimisation(7, 30.0m, 0));

            // Création d'une échelle de temps valide
            var slots = new List<SlotTemporel>();
            var indexLookup = new Dictionary<LocalDateTime, int>();
            int index = 0;

            for (int jour = 0; jour < nombreJours; jour++)
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