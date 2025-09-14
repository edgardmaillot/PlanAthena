using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Utilities;
using CoreEnums = PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthenaTests.Utilities
{
    [TestClass]
    public class PlanningConsolidationServiceTests
    {
        private PlanningConsolidationService _service;
        private ConfigurationPlanification _config;

        [TestInitialize]
        public void Setup()
        {
            _service = new PlanningConsolidationService();

            // Configuration standard pour les tests :
            // Travail du lundi au vendredi, de 8h à 15h (7 heures effectives).
            _config = new ConfigurationPlanification
            {
                JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                HeureDebutJournee = 8,
                HeuresTravailEffectifParJour = 7
            };
        }

        [TestMethod]
        public void Process_TacheSimpleSurUnJour_CreeUnSeulSegment()
        {
            // Arrange
            var affectation = new AffectationDto
            {
                OuvrierId = "O1",
                TacheId = "T1",
                TacheNom = "Tache 1",
                DateDebut = new DateTime(2025, 9, 22, 8, 0, 0), // Lundi 8h
                DateFin = new DateTime(2025, 9, 22, 12, 0, 0),
                DureeHeures = 4,
                TypeActivite = CoreEnums.TypeActivite.Tache
            };
            var rawResult = CreateRawResult(affectation);

            // Act
            var result = _service.Process(rawResult, _config);

            // Assert
            Assert.AreEqual(1, result.SegmentsParOuvrierId["O1"].Count, "Il devrait y avoir un seul segment.");
            var segment = result.SegmentsParOuvrierId["O1"].First();
            Assert.AreEqual(4, segment.HeuresTravaillees, 0.001);
            Assert.AreEqual(new DateTime(2025, 9, 22), segment.Jour);
            Assert.IsNull(segment.ParentTacheId);
        }

        [TestMethod]
        public void Process_TacheSurDeuxJours_CreeDeuxSegments()
        {
            // Arrange
            // Tâche de 10h commençant un lundi à 8h. 7h le lundi, 3h le mardi.
            var affectation = new AffectationDto
            {
                OuvrierId = "O1",
                TacheId = "T2",
                TacheNom = "Tache 2",
                DateDebut = new DateTime(2025, 9, 22, 8, 0, 0), // Lundi 8h
                DateFin = new DateTime(2025, 9, 23, 11, 0, 0),
                DureeHeures = 10,
                TypeActivite = CoreEnums.TypeActivite.Tache
            };
            var rawResult = CreateRawResult(affectation);

            // Act
            var result = _service.Process(rawResult, _config);

            // Assert
            var segments = result.SegmentsParOuvrierId["O1"];
            Assert.AreEqual(2, segments.Count, "Il devrait y avoir deux segments.");

            Assert.AreEqual(7, segments[0].HeuresTravaillees, 0.001);
            Assert.AreEqual(new DateTime(2025, 9, 22), segments[0].Jour, "Premier segment le lundi.");

            Assert.AreEqual(3, segments[1].HeuresTravaillees, 0.001);
            Assert.AreEqual(new DateTime(2025, 9, 23), segments[1].Jour, "Deuxième segment le mardi.");
        }

        [TestMethod]
        public void Process_TacheChevauchantUnWeekend_IgnoreSamediDimanche()
        {
            // Arrange
            // Tâche de 10h commençant un vendredi à 10h. 5h le vendredi, 5h le lundi.
            var affectation = new AffectationDto
            {
                OuvrierId = "O1",
                TacheId = "T3",
                TacheNom = "Tache 3",
                DateDebut = new DateTime(2025, 9, 26, 10, 0, 0), // Vendredi 10h
                DateFin = new DateTime(2025, 9, 29, 13, 0, 0),
                DureeHeures = 10,
                TypeActivite = CoreEnums.TypeActivite.Tache
            };
            var rawResult = CreateRawResult(affectation);

            // Act
            var result = _service.Process(rawResult, _config);

            // Assert
            var segments = result.SegmentsParOuvrierId["O1"];
            Assert.AreEqual(2, segments.Count, "Il devrait y avoir deux segments.");

            Assert.AreEqual(5, segments[0].HeuresTravaillees, 0.001);
            Assert.AreEqual(new DateTime(2025, 9, 26), segments[0].Jour, "Premier segment le vendredi.");

            Assert.AreEqual(5, segments[1].HeuresTravaillees, 0.001);
            Assert.AreEqual(new DateTime(2025, 9, 29), segments[1].Jour, "Deuxième segment le lundi.");
        }

        [TestMethod]
        public void Process_JalonUtilisateur_EstAjouteCommeBlocUniqueNonDecoupe()
        {
            // Arrange
            // Jalon de 30h commençant à 10h le lundi et se terminant le lendemain à 16h.
            var dateDebut = new DateTime(2025, 9, 22, 10, 0, 0);
            var dateFin = new DateTime(2025, 9, 23, 16, 0, 0);

            var affectation = new AffectationDto
            {
                OuvrierId = "VIRTUAL_JALON_1", // L'ouvrier virtuel n'est plus utilisé pour le regroupement
                TacheId = "J1",
                TacheNom = "Séchage",
                DateDebut = dateDebut,
                DateFin = dateFin,
                DureeHeures = 30,
                TypeActivite = CoreEnums.TypeActivite.JalonUtilisateur
            };
            var rawResult = CreateRawResult(affectation);

            // Act
            var result = _service.Process(rawResult, _config);

            // Assert
            // 1. Vérifier qu'AUCUN segment de travail n'a été créé pour cet "ouvrier"
            Assert.IsFalse(result.SegmentsParOuvrierId.ContainsKey("VIRTUAL_JALON_1"),
                "Les jalons ne doivent pas créer de segments de travail.");

            // 2. Vérifier qu'UN SEUL jalon a été ajouté à la liste dédiée
            Assert.AreEqual(1, result.JalonsPlanifies.Count, "Un seul jalon planifié devrait exister.");

            // 3. Vérifier que les propriétés du jalon sont correctes et NON découpées
            var jalon = result.JalonsPlanifies.First();
            Assert.AreEqual("J1", jalon.TacheId);
            Assert.AreEqual("Séchage", jalon.TacheNom);
            Assert.AreEqual(dateDebut, jalon.DateDebut, "La date de début doit être conservée intacte.");
            Assert.AreEqual(dateFin, jalon.DateFin, "La date de fin doit être conservée intacte.");
            Assert.AreEqual(30, jalon.DureeHeures, 0.001, "La durée totale doit être conservée intacte.");
        }

        [TestMethod]
        public void Process_SousTache_ParentIdEstCorrectementDefini()
        {
            // Arrange
            var affectation = new AffectationDto
            {
                OuvrierId = "O1",
                TacheId = "T005_P1",
                TacheNom = "Tache 5 Partie 1",
                DateDebut = new DateTime(2025, 9, 22, 8, 0, 0),
                DateFin = new DateTime(2025, 9, 22, 12, 0, 0),
                DureeHeures = 4,
                TypeActivite = CoreEnums.TypeActivite.Tache
            };
            var rawResult = CreateRawResult(affectation);

            // Act
            var result = _service.Process(rawResult, _config);

            // Assert
            var segment = result.SegmentsParOuvrierId["O1"].First();
            Assert.AreEqual("T005_P1", segment.TacheId);
            Assert.AreEqual("T005", segment.ParentTacheId, "Le ParentId devrait être extrait.");
        }

        [TestMethod]
        public void Process_JalonTechnique_EstIgnore()
        {
            // Arrange
            var affectation = new AffectationDto
            {
                OuvrierId = "VIRTUAL_JT",
                TacheId = "JT_T005",
                TacheNom = "Convergence T005",
                DateDebut = new DateTime(2025, 9, 22, 8, 0, 0),
                DateFin = new DateTime(2025, 9, 22, 8, 0, 0),
                DureeHeures = 0,
                TypeActivite = CoreEnums.TypeActivite.JalonTechnique
            };
            var rawResult = CreateRawResult(affectation);

            // Act
            var result = _service.Process(rawResult, _config);

            // Assert
            Assert.AreEqual(0, result.SegmentsParOuvrierId.Count, "Le dictionnaire des segments devrait être vide.");
        }

        [TestMethod]
        public void Process_EntreeNulleOuVide_RetournePlanningVide()
        {
            // Arrange
            var emptyResult = new ProcessChantierResultDto
            {
                OptimisationResultat = new PlanningOptimizationResultDto
                {
                    Affectations = new List<AffectationDto>()
                }
            };

            // Act
            var result1 = _service.Process(null, _config);
            var result2 = _service.Process(emptyResult, _config);

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(0, result1.SegmentsParOuvrierId.Count);

            Assert.IsNotNull(result2);
            Assert.AreEqual(0, result2.SegmentsParOuvrierId.Count);
        }

        /// <summary>
        /// Méthode utilitaire pour créer un ProcessChantierResultDto à partir d'affectations.
        /// </summary>
        private ProcessChantierResultDto CreateRawResult(params AffectationDto[] affectations)
        {
            return new ProcessChantierResultDto
            {
                OptimisationResultat = new PlanningOptimizationResultDto
                {
                    Affectations = new List<AffectationDto>(affectations)
                }
            };
        }
    }
}