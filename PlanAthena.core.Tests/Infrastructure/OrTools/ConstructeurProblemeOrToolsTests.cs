// PlanAthena.core.Tests/Infrastructure/OrTools/ConstructeurProblemeOrToolsTests.cs

using FluentAssertions;
using FluentAssertions.Numeric; // Nécessaire pour BeGreaterThanOrEqualTo
using Google.OrTools.Sat;
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

        // TEST: Construction modèle complet avec toutes les variables attendues.
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

            // Vérification des nouvelles propriétés pour les lots et groupes de priorité
            modele.LotStarts.Should().NotBeNull();
            modele.LotEnds.Should().NotBeNull();
            modele.PriorityGroupStarts.Should().NotBeNull();
            modele.PriorityGroupEnds.Should().NotBeNull();
        }

        // TEST: Validation des métadonnées des tâches (durées, types, noms).
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

            // Vérifier qu'on a les métadonnées pour toutes les tâches du problème de test.
            var toutesLesTaches = probleme.Chantier.ObtenirToutesLesTaches();
            modele.DureesOriginalesHeures.Should().HaveCount(toutesLesTaches.Count());
            modele.TypesActivites.Should().HaveCount(toutesLesTaches.Count());
            modele.NomsActivites.Should().HaveCount(toutesLesTaches.Count());
        }

        // TEST: S'assurer que le modèle est configuré pour minimiser le makespan quand l'objectif est "DELAI".
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

            // Vérifier que la variable de Makespan existe et a le nom attendu.
            modele.Makespan.Proto.Name.Should().Be("makespan");
        }

        // TEST: S'assurer que le modèle est configuré pour minimiser le coût total quand l'objectif est "COUT".
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

        // TEST: S'assurer que l'objectif par défaut (COUT) est utilisé si l'objectif fourni est inconnu.
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
            modele.CoutTotal.Proto.Name.Should().Be("cout_total_chantier");
        }

        // TEST: Vérifier que les métadonnées des tâches correspondent bien aux données originales.
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
                modele.DureesOriginalesHeures.Should().ContainKey(tache.Id);
                modele.DureesOriginalesHeures[tache.Id].Should().Be(tache.HeuresHommeEstimees.Value);

                modele.TypesActivites.Should().ContainKey(tache.Id);
                modele.TypesActivites[tache.Id].Should().Be(tache.Type);

                modele.NomsActivites.Should().ContainKey(tache.Id);
                modele.NomsActivites[tache.Id].Should().Be(tache.Nom);
            }
        }

        // TEST: S'assurer que le modèle construit est solvable par le solveur CP-SAT.
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

            // Tente de résoudre le modèle pour vérifier sa faisabilité de base.
            var solver = new Google.OrTools.Sat.CpSolver();
            var status = solver.Solve(modele.Model);

            status.Should().BeOneOf(
                Google.OrTools.Sat.CpSolverStatus.Optimal,
                Google.OrTools.Sat.CpSolverStatus.Feasible
            );
        }

        // NOUVEAU : Teste que le ModeleCpSat expose correctement les variables de début/fin pour chaque lot.
        [Fact]
        public void ConstruireModele_ExposeLotStartsAndEndsCorrectly()
        {
            // Arrange
            var probleme = CreerProblemeAvecPlusieursLotsEtTaches(); // Utilise un problème avec des lots définis

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "COUT");

            // Assert
            modele.LotStarts.Should().NotBeNull().And.HaveCount(probleme.Chantier.Lots.Count);
            modele.LotEnds.Should().NotBeNull().And.HaveCount(probleme.Chantier.Lots.Count);

            foreach (var lot in probleme.Chantier.Lots.Values)
            {
                modele.LotStarts.Should().ContainKey(lot.Id);
                modele.LotEnds.Should().ContainKey(lot.Id);
                modele.LotStarts[lot.Id].Should().NotBeNull();
                modele.LotEnds[lot.Id].Should().NotBeNull();
            }
        }

        // NOUVEAU : Teste que le ModeleCpSat expose correctement les variables de début/fin pour chaque groupe de priorité.
        [Fact]
        public void ConstruireModele_ExposePriorityGroupStartsAndEndsCorrectly()
        {
            // Arrange
            var probleme = CreerProblemeAvecPlusieursLotsEtTaches(); // Utilise un problème avec différentes priorités
            var expectedDistinctPriorities = probleme.Chantier.Lots.Values.Select(l => l.Priorite).Distinct().Count();

            // Act
            var modele = _constructeur.ConstruireModele(probleme, "COUT");

            // Assert
            modele.PriorityGroupStarts.Should().NotBeNull().And.HaveCount(expectedDistinctPriorities);
            modele.PriorityGroupEnds.Should().NotBeNull().And.HaveCount(expectedDistinctPriorities);

            foreach (var priority in probleme.Chantier.Lots.Values.Select(l => l.Priorite).Distinct())
            {
                modele.PriorityGroupStarts.Should().ContainKey(priority);
                modele.PriorityGroupEnds.Should().ContainKey(priority);
                modele.PriorityGroupStarts[priority].Should().NotBeNull();
                modele.PriorityGroupEnds[priority].Should().NotBeNull();
            }
        }

        // --- Méthodes utilitaires pour créer des problèmes de test ---

        // Crée un problème de test générique, utilisant une tâche normale simple.
        private ProblemeOptimisation CreerProblemeDeTest()
        {
            return CreerProblemeAvecTacheNormale();
        }

        // Crée un problème simple avec une seule tâche et un lot.
        private ProblemeOptimisation CreerProblemeSimple()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var tacheId = new TacheId("TACHE_SIMPLE");
            var blocId = new BlocId("BLOC_SIMPLE");
            var lotId = new LotId("LOT_SIMPLE"); // Ajout d'un lot pour la cohérence

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
                new List<LotTravaux> { new(lotId, "Lot Simple", 10, new[] { blocId }) } // Ajout du lot
            );

            return CreerProblemeOptimisation(chantier, 1);
        }

        // Crée un problème avec des tâches de types variés (normale, jalon) pour tester la gestion des métadonnées.
        private ProblemeOptimisation CreerProblemeAvecTachesVariees()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var tacheId = new TacheId("TACHE_1");
            var jalonId = new TacheId("JALON_1");
            var jalonTechniqueId = new TacheId("JALON_TECH_1");
            var blocId = new BlocId("BLOC_VARIE");
            var lotId = new LotId("LOT_VARIE"); // Ajout d'un lot

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
                new List<LotTravaux> { new(lotId, "Lot Varié", 10, new[] { blocId }) } // Ajout du lot
            );

            return CreerProblemeOptimisation(chantier, 5);
        }

        // Crée un problème avec une seule tâche normale et un lot, pour les tests basiques.
        private ProblemeOptimisation CreerProblemeAvecTacheNormale()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var tacheId = new TacheId("TACHE_1");
            var blocId = new BlocId("BLOC_A");
            var lotId = new LotId("LOT_1"); // Ajouter un lot pour le test

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
                new List<LotTravaux> { new(lotId, "Lot Test", 10, new[] { blocId }) } // Lot avec priorité
            );

            return CreerProblemeOptimisation(chantier, 1);
        }

        // Crée un problème avec plusieurs lots et tâches, pour tester la gestion des lots et groupes de priorité.
        private ProblemeOptimisation CreerProblemeAvecPlusieursLotsEtTaches()
        {
            var metierId = new MetierId("GENERIC_METIER");
            var ouvrierId = new OuvrierId("GENERIC_OUV");

            var lot10Id = new LotId("LOT_10");
            var lot20AId = new LotId("LOT_20A");
            var lot20BId = new LotId("LOT_20B");
            var lot30Id = new LotId("LOT_30");

            var bloc10Id = new BlocId("BLOC_10");
            var bloc20AId = new BlocId("BLOC_20A");
            var bloc20BId = new BlocId("BLOC_20B");
            var bloc30Id = new BlocId("BLOC_30");

            var tache10 = new Tache(new TacheId("TACHE_LOT_10"), "Tache P10", TypeActivite.Tache, bloc10Id, new DureeHeuresHomme(8), metierId);
            var tache20A = new Tache(new TacheId("TACHE_LOT_20A"), "Tache P20A", TypeActivite.Tache, bloc20AId, new DureeHeuresHomme(4), metierId);
            var tache20B = new Tache(new TacheId("TACHE_LOT_20B"), "Tache P20B", TypeActivite.Tache, bloc20BId, new DureeHeuresHomme(6), metierId);
            var tache30 = new Tache(new TacheId("TACHE_LOT_30"), "Tache P30", TypeActivite.Tache, bloc30Id, new DureeHeuresHomme(10), metierId);

            var chantier = new Chantier(
                new ChantierId("CHANTIER_MULTI_LOTS"), "Test Multi Lots",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 10).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Métier Générique") },
                new List<Ouvrier> { new(ouvrierId, "Ouvrier", "Générique", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(bloc10Id, "Bloc Lot 10", new CapaciteOuvriers(1), new List<Tache> { tache10 }),
                    new(bloc20AId, "Bloc Lot 20A", new CapaciteOuvriers(1), new List<Tache> { tache20A }),
                    new(bloc20BId, "Bloc Lot 20B", new CapaciteOuvriers(1), new List<Tache> { tache20B }),
                    new(bloc30Id, "Bloc Lot 30", new CapaciteOuvriers(1), new List<Tache> { tache30 })
                },
                new List<LotTravaux>
                {
                    new(lot10Id, "Lot P10", 10, new[] { bloc10Id }),
                    new(lot20AId, "Lot P20A", 20, new[] { bloc20AId }),
                    new(lot20BId, "Lot P20B", 20, new[] { bloc20BId }),
                    new(lot30Id, "Lot P30", 30, new[] { bloc30Id })
                }
            );

            return CreerProblemeOptimisation(chantier, 10);
        }

        // Crée un objet ProblemeOptimisation à partir d'un Chantier et d'un nombre de jours.
        private ProblemeOptimisation CreerProblemeOptimisation(Chantier chantier, int nombreJours)
        {
            chantier.AppliquerConfigurationOptimisation(new ConfigurationOptimisation(7, 30.0m, 0));

            // Crée une échelle de temps simple basée sur le nombre de jours spécifié.
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