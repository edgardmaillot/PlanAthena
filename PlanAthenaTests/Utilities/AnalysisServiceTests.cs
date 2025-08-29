// Emplacement: /PlanAthena.Tests/Utilities/AnalysisServiceTests.cs

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Tests.Utilities
{
    [TestClass]
    public class AnalysisServiceTests
    {
        private AnalysisService _analysisService;
        private List<Ouvrier> _sampleOuvriers;
        private List<Metier> _sampleMetiers;
        private ConfigurationPlanification _sampleConfig;

        // "Calculateur" simple qui compte tous les jours sauf les week-ends.
        private readonly AnalysisService.JoursOuvresCalculator _joursOuvresCalculatorSimple = (start, end) =>
        {
            int count = 0;
            for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
            {
                if (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
                {
                    count++;
                }
            }
            return count;
        };

        [TestInitialize]
        public void TestInitialize()
        {
            _analysisService = new AnalysisService();

            _sampleOuvriers = new List<Ouvrier>
            {
                new Ouvrier { OuvrierId = "O1", Nom = "Durand", Prenom = "Paul", CoutJournalier = 100, Competences = new List<CompetenceOuvrier> { new CompetenceOuvrier { MetierId = "ELEC", EstMetierPrincipal = true } } },
                new Ouvrier { OuvrierId = "O2", Nom = "Martin", Prenom = "Alice", CoutJournalier = 120, Competences = new List<CompetenceOuvrier> { new CompetenceOuvrier { MetierId = "PLOM", EstMetierPrincipal = true } } },
                new Ouvrier { OuvrierId = "O3", Nom = "Petit", Prenom = "Thomas", CoutJournalier = 100, Competences = new List<CompetenceOuvrier> { new CompetenceOuvrier { MetierId = "ELEC", EstMetierPrincipal = true } } }
            };

            _sampleMetiers = new List<Metier>
            {
                new Metier { MetierId = "ELEC", Nom = "Électricien" },
                new Metier { MetierId = "PLOM", Nom = "Plombier" }
            };

            _sampleConfig = new ConfigurationPlanification
            {
                HeuresTravailEffectifParJour = 7,
                CoutIndirectJournalierAbsolu = 500
            };
        }

        #region Tests pour GenerateReport

        [TestMethod]
        public void GenerateReport_WithNominalData_ShouldCalculateCorrectSynthese()
        {
            // Arrange
            var planning = new ConsolidatedPlanning
            {
                DateDebutProjet = new DateTime(2023, 10, 2), // Lundi
                DateFinProjet = new DateTime(2023, 10, 4),   // Mercredi
                SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>
                {
                    ["O1"] = new List<SegmentDeTravail>
                    {
                        new SegmentDeTravail { Jour = new DateTime(2023, 10, 2), HeuresTravaillees = 7 }, // Lundi
                        new SegmentDeTravail { Jour = new DateTime(2023, 10, 3), HeuresTravaillees = 7 }  // Mardi
                    },
                    ["O2"] = new List<SegmentDeTravail>
                    {
                        new SegmentDeTravail { Jour = new DateTime(2023, 10, 4), HeuresTravaillees = 7 }  // Mercredi
                    }
                }
            };

            // Act
            var report = _analysisService.GenerateReport(planning, _sampleOuvriers, _sampleMetiers, _sampleConfig, _joursOuvresCalculatorSimple);
            var synthese = report.SyntheseProjet;

            // Assert
            Assert.AreEqual(3, synthese.EffortTotalJoursHomme, "L'effort total (jours-homme) doit être correct."); // O1 (2j) + O2 (1j) = 3
            Assert.AreEqual(2 * 100 + 1 * 120, synthese.CoutTotalRh, "Le coût RH total doit être correct.");
            Assert.AreEqual(3, synthese.DureeJoursOuvres, "La durée en jours ouvrés du projet doit être correcte."); // Lun, Mar, Mer
            Assert.AreEqual(3 * 500, synthese.CoutTotalIndirect, "Le coût indirect total doit être correct.");
            Assert.AreEqual(320 + 1500, synthese.CoutTotalProjet, "Le coût total du projet doit être correct.");
        }

        [TestMethod]
        public void GenerateReport_WithNominalData_ShouldCalculateCorrectAnalyseOuvrier()
        {
            // Arrange
            var planning = new ConsolidatedPlanning
            {
                SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>
                {
                    ["O1"] = new List<SegmentDeTravail> //Fragmentation: Lundi, Mercredi (Mardi off)
                    {
                        new SegmentDeTravail { Jour = new DateTime(2023, 10, 2), HeuresTravaillees = 7 }, // Lundi
                        new SegmentDeTravail { Jour = new DateTime(2023, 10, 4), HeuresTravaillees = 3.5 } // Mercredi
                    }
                }
            };

            // Act
            var report = _analysisService.GenerateReport(planning, _sampleOuvriers, _sampleMetiers, _sampleConfig, _joursOuvresCalculatorSimple);
            var analyseO1 = report.AnalysesOuvriers.First(r => r.OuvrierId == "O1");
            var analyseO2 = report.AnalysesOuvriers.First(r => r.OuvrierId == "O2");

            // Assert - Ouvrier O1
            Assert.AreEqual(10.5, analyseO1.HeuresTravaillees);
            Assert.AreEqual(2, analyseO1.JoursTravailles);
            Assert.AreEqual(Math.Round(10.5 / (2.0 * 7.0), 2), analyseO1.TauxOccupation); // 10.5 / 14 = 0.75
            Assert.AreEqual(Math.Round(1.0 - (2.0 / 3.0), 2), analyseO1.TauxFragmentation); // Période Lu-Me = 3j ouvrés. 1 - (2/3) = 0.33

            // Assert - Ouvrier O2 (non affecté)
            Assert.AreEqual(0, analyseO2.HeuresTravaillees);
            Assert.AreEqual(0, analyseO2.JoursTravailles);
            Assert.AreEqual(0, analyseO2.TauxOccupation);
            Assert.AreEqual(0, analyseO2.TauxFragmentation);
        }

        [TestMethod]
        public void GenerateReport_WithNominalData_ShouldCalculateCorrectChargeJournaliere()
        {
            // Arrange
            var planning = new ConsolidatedPlanning
            {
                SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>
                {
                    ["O1"] = new List<SegmentDeTravail> { new SegmentDeTravail { Jour = new DateTime(2023, 10, 2), OuvrierId = "O1" } },
                    ["O2"] = new List<SegmentDeTravail> { new SegmentDeTravail { Jour = new DateTime(2023, 10, 2), OuvrierId = "O2" } }, // O2 travaille aussi le 2
                    ["O3"] = new List<SegmentDeTravail> { new SegmentDeTravail { Jour = new DateTime(2023, 10, 3), OuvrierId = "O3" } }
                }
            };

            // Act
            var report = _analysisService.GenerateReport(planning, _sampleOuvriers, _sampleMetiers, _sampleConfig, _joursOuvresCalculatorSimple);
            var charge = report.ChargeJournaliere;

            // Assert
            Assert.AreEqual(2, charge.Count, "Il doit y avoir 2 jours de charge.");
            Assert.AreEqual(2, charge[new DateTime(2023, 10, 2)], "La charge pour le 2/10 doit être de 2 ouvriers.");
            Assert.AreEqual(1, charge[new DateTime(2023, 10, 3)], "La charge pour le 3/10 doit être de 1 ouvrier.");
        }

        [TestMethod]
        public void GenerateReport_WithEmptyPlanning_ShouldReturnEmptyReport()
        {
            // Arrange
            var planning = new ConsolidatedPlanning(); // Planning vide

            // Act
            var report = _analysisService.GenerateReport(planning, _sampleOuvriers, _sampleMetiers, _sampleConfig, _joursOuvresCalculatorSimple);

            // Assert
            Assert.AreEqual(0, report.SyntheseProjet.CoutTotalProjet);
            Assert.AreEqual(0, report.SyntheseProjet.EffortTotalJoursHomme);
            Assert.AreEqual(_sampleOuvriers.Count, report.AnalysesOuvriers.Count, "Doit retourner un rapport pour chaque ouvrier.");
            Assert.IsTrue(report.AnalysesOuvriers.All(r => r.JoursTravailles == 0));
            Assert.AreEqual(0, report.ChargeJournaliere.Count);
        }

        #endregion

        #region NOUVEAU: Tests pour l'enrichissement du Métier Principal

        [TestMethod]
        public void GenerateReport_WithValidData_ShouldEnrichReportWithMetierPrincipal()
        {
            // Arrange
            var planning = new ConsolidatedPlanning(); // Pas besoin de planning pour ce test

            // Act
            var report = _analysisService.GenerateReport(planning, _sampleOuvriers, _sampleMetiers, _sampleConfig, _joursOuvresCalculatorSimple);
            var analyseO1 = report.AnalysesOuvriers.First(r => r.OuvrierId == "O1");
            var analyseO2 = report.AnalysesOuvriers.First(r => r.OuvrierId == "O2");

            // Assert
            Assert.AreEqual("ELEC", analyseO1.MetierPrincipalId, "L'ID du métier principal pour O1 doit être correct.");
            Assert.AreEqual("Électricien", analyseO1.MetierPrincipalNom, "Le nom du métier principal pour O1 doit être correct.");
            Assert.AreEqual("PLOM", analyseO2.MetierPrincipalId, "L'ID du métier principal pour O2 doit être correct.");
            Assert.AreEqual("Plombier", analyseO2.MetierPrincipalNom, "Le nom du métier principal pour O2 doit être correct.");
        }

        [TestMethod]
        public void GenerateReport_WithoutExplicitPrincipalMetier_ShouldUseFirstCompetenceAsFallback()
        {
            // Arrange
            var planning = new ConsolidatedPlanning();
            var ouvrierMultiCompetence = new List<Ouvrier>
            {
                new Ouvrier
                {
                    OuvrierId = "O4",
                    Competences = new List<CompetenceOuvrier>
                    {
                        new CompetenceOuvrier { MetierId = "PLOM", EstMetierPrincipal = false }, // Premier dans la liste
                        new CompetenceOuvrier { MetierId = "ELEC", EstMetierPrincipal = false }
                    }
                }
            };

            // Act
            var report = _analysisService.GenerateReport(planning, ouvrierMultiCompetence, _sampleMetiers, _sampleConfig, _joursOuvresCalculatorSimple);
            var analyseO4 = report.AnalysesOuvriers.First(r => r.OuvrierId == "O4");

            // Assert
            Assert.AreEqual("PLOM", analyseO4.MetierPrincipalId, "Doit utiliser le premier métier de la liste comme fallback.");
            Assert.AreEqual("Plombier", analyseO4.MetierPrincipalNom, "Le nom du métier de fallback doit être correct.");
        }

        #endregion

        #region Tests pour AnalyzeMetierTension

        [TestMethod]
        [DataRow(new[] { "O1", "O3", "O2", "O1" }, "Un métier en tension forte")] // 3 ELEC, 1 PLOM
        [DataRow(new[] { "O1", "O3", "O2", "O2" }, "Deux métiers en tension")]     // 2 ELEC, 2 PLOM
        [DataRow(new[] { "O1", "O1", "O2", "O3" }, "Un métier en tension forte")] // Répétition O1, 3 ELEC, 1 PLOM
        [DataRow(new[] { "O1", "O2", "O3", "O3" }, "Un métier en tension forte")]
        [DataRow(new[] { "O1", "O2", "O3", "O2" }, "Deux métiers en tension")]
        public void AnalyzeMetierTension_VariousCases_ShouldReturnCorrectConclusion(string[] ouvrierIds, string expectedConclusion)
        {
            // Arrange
            var sampleOuvriers = new List<Ouvrier>
            {
                new Ouvrier { OuvrierId = "O1", Competences = new List<CompetenceOuvrier>{ new CompetenceOuvrier { MetierId = "ELEC"} } },
                new Ouvrier { OuvrierId = "O2", Competences = new List<CompetenceOuvrier>{ new CompetenceOuvrier { MetierId = "PLOM"} } },
                new Ouvrier { OuvrierId = "O3", Competences = new List<CompetenceOuvrier>{ new CompetenceOuvrier { MetierId = "ELEC"} } },
                new Ouvrier { OuvrierId = "O4", Competences = new List<CompetenceOuvrier>{ new CompetenceOuvrier { MetierId = "MACO"} } }
            };

            // Act
            var report = _analysisService.AnalyzeMetierTension(ouvrierIds.ToList(), sampleOuvriers);

            // Assert
            Assert.AreEqual(expectedConclusion, report.Conclusion);
        }

        [TestMethod]
        public void AnalyzeMetierTension_WithMissingOuvrierId_ShouldIgnoreIt()
        {
            // Arrange
            var ouvrierIds = new List<string> { "O1", "O99", "O3" }; // O99 n'existe pas

            // Act
            var report = _analysisService.AnalyzeMetierTension(ouvrierIds, _sampleOuvriers);
            var repartitionElec = report.Repartition.FirstOrDefault(r => r.MetierId == "ELEC");

            // Assert
            Assert.AreEqual("Un métier en tension", report.Conclusion); // 2 ELEC = "Un métier en tension"
            Assert.IsNotNull(repartitionElec);
            Assert.AreEqual(2, repartitionElec.Count);
            Assert.AreEqual(1, report.Repartition.Count, "Seul le métier ELEC doit être dans la répartition.");
        }

        [TestMethod]
        public void AnalyzeMetierTension_WithEmptyInput_ShouldReturnDefaultReport()
        {
            // Arrange
            var emptyIds = new List<string>();

            // Act
            var report = _analysisService.AnalyzeMetierTension(emptyIds, _sampleOuvriers);

            // Assert
            Assert.AreEqual("Données insuffisantes pour l'analyse.", report.Conclusion);
        }

        #endregion
    }
}