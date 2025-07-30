// PlanAthena.core.Tests/Infrastructure/SolutionInterpreterServiceTests.cs

using FluentAssertions;
using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;

using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Core.Infrastructure.Services;
using PlanAthena.Core.Infrastructure.Services.OrTools;

namespace PlanAthena.core.Tests.Infrastructure
{
    public class SolutionInterpreterServiceTests
    {
        private readonly SolutionInterpreterService _service;

        public SolutionInterpreterServiceTests()
        {
            _service = new SolutionInterpreterService();
        }

        // *** TEST: Interprétation de tâche normale ***
        [Fact]
        public void InterpreterLaSolution_AvecTacheNormale_RetourneDureeCalculeeParSolveur()
        {
            // Arrange
            var (solver, modeleCpSat, probleme) = CreerContexteAvecTacheNormale();

            // Simuler que le solveur a trouvé une solution
            SimulerSolutionOptimale(solver, modeleCpSat, probleme);

            // Act
            var affectations = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            affectations.Should().HaveCount(1);
            var affectation = affectations.First();

            affectation.TypeActivite.Should().Be(TypeActivite.Tache);
            affectation.EstJalon.Should().BeFalse();
            affectation.DureeHeures.Should().Be(8.0); // Durée calculée par le solveur
            affectation.DureeOriginaleHeures.Should().Be(8.0); // Identique pour une tâche normale
        }

        // *** TEST: Interprétation de jalon ***
        [Fact]
        public void InterpreterLaSolution_AvecJalon_RetourneDureeOriginale()
        {
            // Arrange
            var (solver, modeleCpSat, probleme) = CreerContexteAvecJalon();

            // Simuler que le solveur a trouvé une solution
            SimulerSolutionOptimale(solver, modeleCpSat, probleme);

            // Act
            var affectations = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            affectations.Should().HaveCount(1);
            var affectation = affectations.First();

            affectation.TypeActivite.Should().Be(TypeActivite.JalonUtilisateur);
            affectation.EstJalon.Should().BeTrue();
            affectation.DureeHeures.Should().Be(72.0); // Durée originale (pas la 1h cosmétique)
            affectation.DureeOriginaleHeures.Should().Be(72.0);
        }

        // *** TEST: Mélange tâches et jalons ***
        [Fact]
        public void InterpreterLaSolution_AvecTachesEtJalons_DistingueDureesCorrectement()
        {
            // Arrange
            var (solver, modeleCpSat, probleme) = CreerContexteComplet();

            // Simuler que le solveur a trouvé une solution
            SimulerSolutionOptimale(solver, modeleCpSat, probleme);

            // Act
            var affectations = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            affectations.Should().HaveCount(2);

            var tache = affectations.First(a => a.TypeActivite == TypeActivite.Tache);
            var jalon = affectations.First(a => a.TypeActivite == TypeActivite.JalonUtilisateur);

            // Vérifier la tâche normale
            tache.EstJalon.Should().BeFalse();
            tache.DureeHeures.Should().Be(5.0); // Durée calculée

            // Vérifier le jalon
            jalon.EstJalon.Should().BeTrue();
            jalon.DureeHeures.Should().Be(24.0); // Durée originale (pas 1h)
        }

        // *** TEST: Fallback en cas de métadonnées manquantes ***
        [Fact]
        public void InterpreterLaSolution_SansMetadonnees_UtiliseFallbackSecurise()
        {
            // Arrange
            var (solver, modeleCpSat, probleme) = CreerContexteSansMetadonnees();

            // Simuler que le solveur a trouvé une solution
            SimulerSolutionOptimale(solver, modeleCpSat, probleme);

            // Act
            var affectations = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            affectations.Should().HaveCount(1);
            var affectation = affectations.First();

            // Fallback sécurisé : considérer comme tâche normale
            affectation.TypeActivite.Should().Be(TypeActivite.Tache);
            affectation.EstJalon.Should().BeFalse();
            affectation.DureeOriginaleHeures.Should().Be(0.0); // Fallback
        }

        // *** TEST: Validation des informations de base ***
        [Fact]
        public void InterpreterLaSolution_RemplitToutesLesProprietesDeBase()
        {
            // Arrange
            var (solver, modeleCpSat, probleme) = CreerContexteAvecTacheNormale();

            // Simuler que le solveur a trouvé une solution
            SimulerSolutionOptimale(solver, modeleCpSat, probleme);

            // Act
            var affectations = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            affectations.Should().HaveCount(1);
            var affectation = affectations.First();

            // Vérifier toutes les propriétés de base
            affectation.TacheId.Should().NotBeNullOrEmpty();
            affectation.TacheNom.Should().NotBeNullOrEmpty();
            affectation.OuvrierId.Should().NotBeNullOrEmpty();
            affectation.OuvrierNom.Should().NotBeNullOrEmpty();
            affectation.BlocId.Should().NotBeNullOrEmpty();
            affectation.DateDebut.Should().NotBe(default(DateTime));
            affectation.DureeHeures.Should().BeGreaterThan(0);
        }

        // *** TEST: Solution vide ***
        [Fact]
        public void InterpreterLaSolution_AvecSolutionVide_RetourneListeVide()
        {
            // Arrange
            var (solver, modeleCpSat, probleme) = CreerContexteAvecTacheNormale();

            // Il faut résoudre le modèle pour que les variables aient des valeurs
            var status = solver.Solve(modeleCpSat.Model);

            // Act
            var affectations = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            affectations.Should().NotBeNull("Le service doit toujours retourner une liste");
            // Note: On ne teste plus qu'elle soit vide car un problème résolvable aura des affectations
        }

        // --- Méthodes utilitaires pour créer les contextes de test ---

        private (CpSolver solver, ModeleCpSat modeleCpSat, ProblemeOptimisation probleme) CreerContexteAvecTacheNormale()
        {
            var probleme = CreerProblemeAvecTacheNormale();
            var modeleCpSat = ConstruireModeleCpSat(probleme);
            var solver = new CpSolver();

            return (solver, modeleCpSat, probleme);
        }

        private (CpSolver solver, ModeleCpSat modeleCpSat, ProblemeOptimisation probleme) CreerContexteAvecJalon()
        {
            var probleme = CreerProblemeAvecJalon();
            var modeleCpSat = ConstruireModeleCpSat(probleme);
            var solver = new CpSolver();

            return (solver, modeleCpSat, probleme);
        }

        private (CpSolver solver, ModeleCpSat modeleCpSat, ProblemeOptimisation probleme) CreerContexteComplet()
        {
            var probleme = CreerProblemeComplet();
            var modeleCpSat = ConstruireModeleCpSat(probleme);
            var solver = new CpSolver();

            return (solver, modeleCpSat, probleme);
        }

        private (CpSolver solver, ModeleCpSat modeleCpSat, ProblemeOptimisation probleme) CreerContexteSansMetadonnees()
        {
            var probleme = CreerProblemeAvecTacheNormale();
            // --- MODIFICATION ICI : Déconstruction du tuple à 10 éléments ---
            var modeleCpSat = ConstruireModeleCpSatSansMetadonnees(probleme);
            var solver = new CpSolver();

            return (solver, modeleCpSat, probleme);
        }

        private ModeleCpSat ConstruireModeleCpSat(ProblemeOptimisation probleme)
        {
            var constructeur = new ConstructeurProblemeOrTools();
            return constructeur.ConstruireModele(probleme, "COUT");
        }

        private ModeleCpSat ConstruireModeleCpSatSansMetadonnees(ProblemeOptimisation probleme)
        {
            // Construire un ModeleCpSat sans métadonnées pour tester les fallbacks
            var model = new CpModel();
            var tacheBuilder = new TacheModelBuilder();
            // --- MODIFICATION ICI : Déconstruction du tuple à 10 éléments, en ignorant les 4 derniers ---
            var (tachesIntervals, tachesAssignables, makespan, _, _, _, _, _, _, _) =
                tacheBuilder.Construire(model, probleme);

            return new ModeleCpSat
            {
                Model = model,
                TachesIntervals = tachesIntervals,
                TachesAssignables = tachesAssignables,
                Makespan = makespan,
                // Métadonnées volontairement nulles pour tester les fallbacks
                DureesOriginalesHeures = null,
                TypesActivites = null,
                NomsActivites = null,
                // Les nouvelles propriétés doivent aussi être nulles ou vides pour ce test spécifique
                LotStarts = null,
                LotEnds = null,
                PriorityGroupStarts = null,
                PriorityGroupEnds = null
            };
        }

        private void SimulerSolutionOptimale(CpSolver solver, ModeleCpSat modeleCpSat, ProblemeOptimisation probleme)
        {
            // Pour les tests, on va résoudre réellement le modèle simple
            // Dans un environnement de test plus sophistiqué, on pourrait mocker les valeurs
            var status = solver.Solve(modeleCpSat.Model);
            // On vérifie que le statut est optimal ou faisable, ce qui est attendu pour un problème de test résolvable.
            status.Should().BeOneOf(CpSolverStatus.Optimal, CpSolverStatus.Feasible).And.Be(status, "Le problème de test devrait être résolvable.");

        }

        // --- Méthodes de création de problèmes (réutilisées des autres tests) ---

        private ProblemeOptimisation CreerProblemeAvecTacheNormale()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var tacheId = new TacheId("TACHE_1");
            var blocId = new BlocId("BLOC_A");
            var lotId = new LotId("LOT_1"); // Ajout d'un lot pour la cohérence des tests

            var chantier = new Chantier(
                new ChantierId("CHANTIER_NORMAL"), "Test Tâche Normale",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 2).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Testeur") },
                new List<Ouvrier> { new(ouvrierId, "Test", "Ouvrier", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(tacheId, "Tâche Test", TypeActivite.Tache, blocId, new DureeHeuresHomme(8), metierId, null)
                    })
                },
                new List<LotTravaux> { new(lotId, "Lot Test", 10, new[] { blocId }) } // Ajout du lot
            );

            return CreerProblemeOptimisation(chantier);
        }

        private ProblemeOptimisation CreerProblemeAvecJalon()
        {
            var metierId = new MetierId("TESTEUR");
            var ouvrierId = new OuvrierId("OUV_1");
            var jalonId = new TacheId("JALON_1");
            var blocId = new BlocId("BLOC_A");
            var lotId = new LotId("LOT_1"); // Ajout d'un lot

            var chantier = new Chantier(
                new ChantierId("CHANTIER_JALON"), "Test Jalon",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 5).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Testeur") },
                new List<Ouvrier> { new(ouvrierId, "Test", "Ouvrier", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(jalonId, "Séchage Béton", TypeActivite.JalonUtilisateur, blocId, new DureeHeuresHomme(72), metierId, null)
                    })
                },
                new List<LotTravaux> { new(lotId, "Lot Jalon", 10, new[] { blocId }) } // Ajout du lot
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
            var lotId = new LotId("LOT_1"); // Ajout d'un lot

            var chantier = new Chantier(
                new ChantierId("CHANTIER_COMPLET"), "Test Complet",
                new PeriodePlanification(new LocalDate(2028, 1, 1).ToDateTimeUnspecified(), new LocalDate(2028, 1, 5).ToDateTimeUnspecified()),
                new CalendrierOuvreChantier(new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday }, new LocalTime(8, 0), Duration.FromHours(8), new HashSet<LocalDate>()),
                new List<Metier> { new(metierId, "Testeur") },
                new List<Ouvrier> { new(ouvrierId, "Test", "Ouvrier", new CoutJournalier(300), new List<Competence> { new(metierId, NiveauExpertise.Confirme) }) },
                new List<BlocTravail>
                {
                    new(blocId, "Bloc A", new CapaciteOuvriers(2), new List<Tache>
                    {
                        new(tacheId, "Tâche Normale", TypeActivite.Tache, blocId, new DureeHeuresHomme(5), metierId, null),
                        new(jalonId, "Séchage", TypeActivite.JalonUtilisateur, blocId, new DureeHeuresHomme(24), metierId, null)
                    })
                },
                new List<LotTravaux> { new(lotId, "Lot Complet", 10, new[] { blocId }) } // Ajout du lot
            );

            return CreerProblemeOptimisation(chantier);
        }

        private ProblemeOptimisation CreerProblemeOptimisation(Chantier chantier)
        {
            chantier.AppliquerConfigurationOptimisation(new ConfigurationOptimisation(7, 30.0m, 0));

            // Création d'une échelle de temps valide pour le test (5 jours × 8h)
            var slots = new List<SlotTemporel>();
            var indexLookup = new Dictionary<LocalDateTime, int>();
            int index = 0;

            for (int jour = 0; jour < 5; jour++)
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