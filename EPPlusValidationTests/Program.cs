// Fichier: TestsJetables/EPPlusValidationTests.cs
// Tests purement jetables pour valider EPPlus 4.5.3.8

using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PlanAthena.TestsJetables
{
    /// <summary>
    /// Tests jetables pour valider EPPlus 4.5.3.8 avant implémentation
    /// À SUPPRIMER après validation technique
    /// </summary>
    public class EPPlusValidationTests
    {
        private static readonly string DOSSIER_TEST = @"C:\temp\planathena_tests";

        public static void Main(string[] args)
        {
            Console.WriteLine("🧪 TESTS JETABLES EPPLUS 4.5.3.8");
            Console.WriteLine("==================================");

            try
            {
                // Créer dossier de test
                Directory.CreateDirectory(DOSSIER_TEST);

                // Test 1: Création fichier basique
                Test1_CreationFichierBasique();

                // Test 2: Multi-onglets
                Test2_MultiOnglets();

                // Test 3: Formatage
                Test3_Formatage();

                // Test 4: Simulation données PlanAthena
                Test4_SimulationDonneesPlanAthena();

                Console.WriteLine("\n✅ TOUS LES TESTS PASSÉS !");
                Console.WriteLine($"📁 Fichiers générés dans: {DOSSIER_TEST}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERREUR: {ex.Message}");
                Console.WriteLine($"💡 Détails: {ex}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        /// <summary>
        /// Test 1: Création fichier Excel basique
        /// </summary>
        static void Test1_CreationFichierBasique()
        {
            Console.WriteLine("\n🔍 Test 1: Création fichier basique...");

            var filePath = Path.Combine(DOSSIER_TEST, "test1_basique.xlsx");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Test");

                // Données simples
                worksheet.Cells["A1"].Value = "Colonne A";
                worksheet.Cells["B1"].Value = "Colonne B";
                worksheet.Cells["A2"].Value = "Donnée 1";
                worksheet.Cells["B2"].Value = "Donnée 2";

                // Sauvegarde
                var file = new FileInfo(filePath);
                package.SaveAs(file);
            }

            Console.WriteLine($"  ✅ Fichier créé: {Path.GetFileName(filePath)}");
        }

        /// <summary>
        /// Test 2: Multi-onglets (critique pour PlanAthena)
        /// </summary>
        static void Test2_MultiOnglets()
        {
            Console.WriteLine("\n🔍 Test 2: Multi-onglets...");

            var filePath = Path.Combine(DOSSIER_TEST, "test2_multi_onglets.xlsx");

            using (var package = new ExcelPackage())
            {
                // Onglet 1: Synthèse
                var synthese = package.Workbook.Worksheets.Add("SYNTHESE");
                synthese.Cells["A1"].Value = "Vue d'ensemble";
                synthese.Cells["A2"].Value = "Projet Test";
                synthese.Cells["A3"].Value = "3 ouvriers";

                // Onglet 2-4: Ouvriers (simulation)
                var ouvriers = new[] { "Pascal Lendui", "Mohamed Ladalle", "Pierre Plaquist" };

                foreach (var ouvrier in ouvriers)
                {
                    var onglet = package.Workbook.Worksheets.Add(ouvrier);
                    onglet.Cells["A1"].Value = "Date";
                    onglet.Cells["B1"].Value = "Tâche";
                    onglet.Cells["C1"].Value = "Durée";

                    // Données simulation
                    onglet.Cells["A2"].Value = "04/08/2025";
                    onglet.Cells["B2"].Value = "Préparation chantier";
                    onglet.Cells["C2"].Value = "8h";
                }

                var file = new FileInfo(filePath);
                package.SaveAs(file);
            }

            Console.WriteLine($"  ✅ Fichier multi-onglets créé: {Path.GetFileName(filePath)}");
        }

        /// <summary>
        /// Test 3: Formatage (headers, couleurs, bordures)
        /// </summary>
        static void Test3_Formatage()
        {
            Console.WriteLine("\n🔍 Test 3: Formatage...");

            var filePath = Path.Combine(DOSSIER_TEST, "test3_formatage.xlsx");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Formatage");

                // Headers avec formatage
                worksheet.Cells["A1"].Value = "Date";
                worksheet.Cells["B1"].Value = "Tâche";
                worksheet.Cells["C1"].Value = "Durée";

                // Formatage headers
                var headerRange = worksheet.Cells["A1:C1"];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Données avec formatage alternatif
                for (int i = 2; i <= 5; i++)
                {
                    worksheet.Cells[i, 1].Value = $"0{i - 1}/08/2025";
                    worksheet.Cells[i, 2].Value = $"Tâche {i - 1}";
                    worksheet.Cells[i, 3].Value = $"{i + 4}h";

                    // Couleur alternée
                    if (i % 2 == 0)
                    {
                        var rowRange = worksheet.Cells[i, 1, i, 3];
                        rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rowRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }
                }

                // Auto-fit colonnes
                worksheet.Cells.AutoFitColumns();

                var file = new FileInfo(filePath);
                package.SaveAs(file);
            }

            Console.WriteLine($"  ✅ Fichier formaté créé: {Path.GetFileName(filePath)}");
        }

        /// <summary>
        /// Test 4: Simulation structure données PlanAthena
        /// </summary>
        static void Test4_SimulationDonneesPlanAthena()
        {
            Console.WriteLine("\n🔍 Test 4: Simulation données PlanAthena...");

            var filePath = Path.Combine(DOSSIER_TEST, "test4_simulation_planathena.xlsx");

            // Simulation données (comme ce qu'on aura vraiment)
            var affectationsSimulation = new List<AffectationSimulation>
            {
                new AffectationSimulation { OuvrierNom = "Pascal Lendui", Date = "04/08/2025", TacheNom = "Preparation chantier", BlocId = "L001_B001", DureeHeures = 8 },
                new AffectationSimulation { OuvrierNom = "Pascal Lendui", Date = "05/08/2025", TacheNom = "---------------", BlocId = "", DureeHeures = 0 },
                new AffectationSimulation { OuvrierNom = "Pascal Lendui", Date = "18/08/2025", TacheNom = "Enduis bureau", BlocId = "L002_Prive", DureeHeures = 7 },

                new AffectationSimulation { OuvrierNom = "Mohamed Ladalle", Date = "05/08/2025", TacheNom = "Séparation Réserve", BlocId = "L001_B001", DureeHeures = 8 },
                new AffectationSimulation { OuvrierNom = "Mohamed Ladalle", Date = "06/08/2025", TacheNom = "Separation boutique/privé", BlocId = "L001_B001", DureeHeures = 8 }
            };

            using (var package = new ExcelPackage())
            {
                // Onglet 1: Synthèse projet
                CreerOngletSyntheseSimulation(package);

                // Onglets par ouvrier
                var ouvrierGroups = affectationsSimulation.GroupBy(a => a.OuvrierNom);

                foreach (var groupe in ouvrierGroups)
                {
                    CreerOngletOuvrierSimulation(package, groupe.Key, groupe.ToList());
                }

                var file = new FileInfo(filePath);
                package.SaveAs(file);
            }

            Console.WriteLine($"  ✅ Simulation PlanAthena créée: {Path.GetFileName(filePath)}");
            Console.WriteLine("    📋 Structure: 1 onglet synthèse + 2 onglets ouvriers");
        }

        /// <summary>
        /// Crée onglet synthèse (simulation structure finale)
        /// </summary>
        static void CreerOngletSyntheseSimulation(ExcelPackage package)
        {
            var synthese = package.Workbook.Worksheets.Add("SYNTHESE");

            // Métadonnées projet
            synthese.Cells["A1"].Value = "PROJET";
            synthese.Cells["B1"].Value = "Rénovation Boutique (Simulation)";
            synthese.Cells["A2"].Value = "Date début";
            synthese.Cells["B2"].Value = "04/08/2025";
            synthese.Cells["A3"].Value = "Date fin";
            synthese.Cells["B3"].Value = "03/09/2025";
            synthese.Cells["A4"].Value = "Coût total";
            synthese.Cells["B4"].Value = "37 337,25€";

            // Headers métiers (simulation)
            synthese.Cells["A6"].Value = "MÉTIER";
            synthese.Cells["B6"].Value = "OCCUPATION";
            synthese.Cells["C6"].Value = "HEURES TOTAL";
            synthese.Cells["D6"].Value = "OUVRIERS";

            // Données métiers (simulation)
            synthese.Cells["A7"].Value = "Enduiseur";
            synthese.Cells["B7"].Value = "63%";
            synthese.Cells["C7"].Value = "38h";
            synthese.Cells["D7"].Value = "Pascal Lendui";

            synthese.Cells["A8"].Value = "Maçon";
            synthese.Cells["B8"].Value = "62%";
            synthese.Cells["C8"].Value = "25h";
            synthese.Cells["D8"].Value = "Mohamed Ladalle";

            // Formatage headers
            var headerRange = synthese.Cells["A6:D6"];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            synthese.Cells.AutoFitColumns();
        }

        /// <summary>
        /// Crée onglet ouvrier (simulation structure finale)
        /// </summary>
        static void CreerOngletOuvrierSimulation(ExcelPackage package, string ouvrierNom, List<AffectationSimulation> affectations)
        {
            var onglet = package.Workbook.Worksheets.Add(ouvrierNom);

            // Headers
            onglet.Cells["A1"].Value = "DATE";
            onglet.Cells["B1"].Value = "TÂCHE";
            onglet.Cells["C1"].Value = "BLOC";
            onglet.Cells["D1"].Value = "DURÉE";

            // Formatage headers
            var headerRange = onglet.Cells["A1:D1"];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

            // Données
            int row = 2;
            foreach (var affectation in affectations.OrderBy(a => a.Date))
            {
                onglet.Cells[row, 1].Value = affectation.Date;
                onglet.Cells[row, 2].Value = affectation.TacheNom;
                onglet.Cells[row, 3].Value = affectation.BlocId;
                onglet.Cells[row, 4].Value = affectation.DureeHeures > 0 ? $"{affectation.DureeHeures}h" : "";

                // Formatage ligne vide
                if (affectation.TacheNom.Contains("---"))
                {
                    var rowRange = onglet.Cells[row, 1, row, 4];
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                row++;
            }

            onglet.Cells.AutoFitColumns();
        }
    }

    /// <summary>
    /// DTO simulation pour tests (sera remplacé par les vrais DTOs)
    /// </summary>
    public class AffectationSimulation
    {
        public string OuvrierNom { get; set; } = "";
        public string Date { get; set; } = "";
        public string TacheNom { get; set; } = "";
        public string BlocId { get; set; } = "";
        public int DureeHeures { get; set; }
    }
}