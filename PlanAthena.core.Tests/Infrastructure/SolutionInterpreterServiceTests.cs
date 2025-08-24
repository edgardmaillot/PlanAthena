// PlanAthena.core.Tests/Infrastructure/SolutionInterpreterServiceTests.cs
// MODIFIÉ - Refactorisation complète pour tester la nouvelle signature et les FeuillesDeTemps.

using FluentAssertions;
using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Infrastructure.Services;
using PlanAthena.Core.Infrastructure.Services.OrTools;
using PlanAthena.Core.Tests.TestHelpers; // Utilisation de la factory de modèles de test

namespace PlanAthena.core.Tests.Infrastructure
{
    public class SolutionInterpreterServiceTests
    {
        private readonly SolutionInterpreterService _service;

        public SolutionInterpreterServiceTests()
        {
            _service = new SolutionInterpreterService();
        }

        // --- NOUVEAUX TESTS POUR LES FEUILLES DE TEMPS ---

        [Fact]
        public void InterpreterLaSolution_AvecProblemeSimple_RetourneFeuilleDeTempsCorrecte()
        {
            // Arrange
            var probleme = TestModelFactory.CreerProblemeSimple(); // 2 tâches séquentielles (8h + 16h)
            var modeleCpSat = ConstruireEtResoudreModele(probleme, out var solver);

            // Act
            var (_, feuillesDeTemps) = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            feuillesDeTemps.Should().HaveCount(1, "un seul ouvrier réel est impliqué");
            var feuilleOuvrier = feuillesDeTemps.First();

            feuilleOuvrier.OuvrierId.Should().Be("1");

            // L'ouvrier doit travailler 3 jours (8h + 8h + 8h)
            feuilleOuvrier.PlanningJournalier.Should().HaveCount(3);

            // Jour 1 : Tâche A (8h)
            var premierJour = feuilleOuvrier.PlanningJournalier.Keys.Min();
            var masqueJour1 = feuilleOuvrier.PlanningJournalier[premierJour];
            System.Numerics.BitOperations.PopCount((ulong)masqueJour1).Should().Be(8);
            masqueJour1.Should().Be(0b11111111); // Les 8 premiers bits à 1

            // Jour 2 : Tâche B (partie 1, 8h)
            var deuxiemeJour = premierJour.AddDays(1);
            var masqueJour2 = feuilleOuvrier.PlanningJournalier[deuxiemeJour];
            System.Numerics.BitOperations.PopCount((ulong)masqueJour2).Should().Be(8);
            masqueJour2.Should().Be(0b11111111);

            // Jour 3 : Tâche B (partie 2, 8h)
            var troisiemeJour = deuxiemeJour.AddDays(1);
            var masqueJour3 = feuilleOuvrier.PlanningJournalier[troisiemeJour];
            System.Numerics.BitOperations.PopCount((ulong)masqueJour3).Should().Be(8);
            masqueJour3.Should().Be(0b11111111);
        }

        [Fact]
        public void InterpreterLaSolution_AvecJalon_ExclutOuvrierVirtuelDesFeuillesDeTemps()
        {
            // Arrange
            var probleme = TestModelFactory.CreerProblemeAvecJalon();
            var modeleCpSat = ConstruireEtResoudreModele(probleme, out var solver);

            // Act
            var (_, feuillesDeTemps) = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            feuillesDeTemps.Should().HaveCount(1, "seul l'ouvrier réel doit avoir une feuille de temps");
            feuillesDeTemps.First().OuvrierId.Should().NotContain("VIRTUAL");
            feuillesDeTemps.First().OuvrierId.Should().Be("1"); // ID du plombier
        }

        // --- TESTS DE NON-RÉGRESSION SUR AFFECTATIONSDTO ---

        [Fact]
        public void InterpreterLaSolution_AvecJalon_RetourneAffectationsCorrectesPourTachesEtJalons()
        {
            // Arrange
            var probleme = TestModelFactory.CreerProblemeAvecJalon();
            var modeleCpSat = ConstruireEtResoudreModele(probleme, out var solver);

            // Act
            var (affectations, _) = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            affectations.Should().HaveCount(2, "il y a une tâche réelle et un jalon");

            var affectationJalon = affectations.Single(a => a.TacheId == TestModelFactory.JalonSechage_Id.Value);
            var affectationTache = affectations.Single(a => a.TacheId == TestModelFactory.TachePlomberie_Id.Value);

            // Valider le jalon
            affectationJalon.OuvrierId.Should().Contain("VIRTUAL");
            affectationJalon.TypeActivite.Should().Be(TypeActivite.JalonUtilisateur);
            affectationJalon.EstJalon.Should().BeTrue();
            affectationJalon.DureeHeures.Should().Be(72.0, "la durée réelle du jalon doit être préservée");

            // Valider la tâche réelle
            affectationTache.OuvrierId.Should().NotContain("VIRTUAL");
            affectationTache.TypeActivite.Should().Be(TypeActivite.Tache);
            affectationTache.EstJalon.Should().BeFalse();
            affectationTache.DureeHeures.Should().Be(8.0);
        }

        [Fact]
        public void InterpreterLaSolution_AvecProblemeSimple_RemplitCorrectementAffectationDto()
        {
            // Arrange
            var probleme = TestModelFactory.CreerProblemeSimple();
            var modeleCpSat = ConstruireEtResoudreModele(probleme, out var solver);

            // Act
            var (affectations, _) = _service.InterpreterLaSolution(solver, modeleCpSat, probleme);

            // Assert
            affectations.Should().HaveCount(2);
            var tacheA = affectations.Single(a => a.TacheId == TestModelFactory.TacheA_Id.Value);

            tacheA.TacheNom.Should().Be("Tache A");
            tacheA.OuvrierId.Should().Be("1");
            tacheA.OuvrierNom.Should().Be("1 Ouvrier");
            tacheA.BlocId.Should().Be(TestModelFactory.DefaultBlocId.Value);
            tacheA.DateDebut.Kind.Should().Be(DateTimeKind.Utc, "la convention UTC doit être respectée");
            tacheA.DureeHeures.Should().Be(8.0);
            tacheA.TypeActivite.Should().Be(TypeActivite.Tache);
            tacheA.EstJalon.Should().BeFalse();
        }

        // --- MÉTHODES UTILITAIRES DE TEST ---

        /// <summary>
        /// Factorise la logique de construction du modèle OR-Tools et sa résolution pour les tests.
        /// </summary>
        private ModeleCpSat ConstruireEtResoudreModele(ProblemeOptimisation probleme, out CpSolver solver)
        {
            var constructeur = new ConstructeurProblemeOrTools();
            var modeleCpSat = constructeur.ConstruireModele(probleme, "COUT");

            solver = new CpSolver();
            var status = solver.Solve(modeleCpSat.Model);

            // *** CORRECTION DÉFINITIVE APPLIQUÉE ICI ***
            // On crée une collection explicite pour forcer le compilateur à choisir la bonne
            // surcharge de BeOneOf qui accepte le paramètre 'because'.
            var validStatuses = new List<CpSolverStatus> { CpSolverStatus.Optimal, CpSolverStatus.Feasible };
            status.Should().BeOneOf(validStatuses,
                because: "le problème de test doit avoir une solution pour que l'interpréteur puisse travailler");

            return modeleCpSat;
        }
    }
}