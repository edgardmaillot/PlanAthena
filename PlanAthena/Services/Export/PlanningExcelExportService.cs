// Fichier: Services/Export/PlanningExcelExportService.cs

using OfficeOpenXml;
using OfficeOpenXml.Style;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.ImportExport;
using PlanAthena.Services.Infrastructure;

namespace PlanAthena.Services.Export
{
    /// <summary>
    /// Service principal pour l'export des plannings en Excel multi-onglets.
    /// Agit comme un présentateur pur : reçoit un DTO complet et se concentre exclusivement
    /// sur la mise en forme et la génération du fichier Excel, sans logique métier ni calcul.
    /// </summary>
    public class PlanningExcelExportService
    {
        private readonly CheminsPrefereService _cheminsPrefereService;

        public PlanningExcelExportService(CheminsPrefereService cheminsPrefereService)
        {
            _cheminsPrefereService = cheminsPrefereService ?? throw new ArgumentNullException(nameof(cheminsPrefereService));
        }

        /// <summary>
        /// Point d'entrée principal : exporte un planning complet en Excel.
        /// Ce service reçoit toutes les données pré-calculées et agrégées via un DTO unique.
        /// </summary>
        /// <param name="exportData">Le DTO complet contenant toutes les informations nécessaires pour l'export.</param>
        /// <param name="cheminFichierComplet">Le chemin complet (dossier + nom) où sauvegarder le fichier Excel.</param>
        /// <returns>Le chemin complet du fichier généré.</returns>
        public string ExporterPlanningComplet(ExportDataProjetDto exportData, string cheminFichierComplet)
        {
            ArgumentNullException.ThrowIfNull(exportData, nameof(exportData));
            ArgumentNullException.ThrowIfNull(exportData.Report, nameof(exportData.Report));
            ArgumentNullException.ThrowIfNull(exportData.Planning, nameof(exportData.Planning));
            ArgumentNullException.ThrowIfNull(exportData.Configuration, nameof(exportData.Configuration));

            if (string.IsNullOrWhiteSpace(cheminFichierComplet))
                throw new ArgumentException("Le chemin du fichier ne peut pas être vide.", nameof(cheminFichierComplet));

            var dossierSortie = Path.GetDirectoryName(cheminFichierComplet) ?? _cheminsPrefereService.ObtenirDernierDossierExport();
            if (!Directory.Exists(dossierSortie))
            {
                Directory.CreateDirectory(dossierSortie);
            }
            cheminFichierComplet = Path.Combine(dossierSortie, Path.GetFileName(cheminFichierComplet));


            using (var package = new ExcelPackage())
            {
                // CORRECTION : La méthode a maintenant besoin de l'objet complet pour accéder au planning et au rapport.
                _CreerOngletSynthese(package, exportData);

                var ouvriersTries = exportData.Report.AnalysesOuvriers
                                                .OrderBy(o => o.MetierPrincipalNom)
                                                .ThenBy(o => o.NomComplet)
                                                .ToList();

                foreach (var analyseOuvrier in ouvriersTries)
                {
                    exportData.Planning.SegmentsParOuvrierId.TryGetValue(analyseOuvrier.OuvrierId, out var segmentsOuvrier);

                    _CreerOngletOuvrier(package, analyseOuvrier, segmentsOuvrier ?? new List<SegmentDeTravail>(), exportData.Configuration, exportData.Planning);
                }

                package.SaveAs(new FileInfo(cheminFichierComplet));
            }

            _cheminsPrefereService.SauvegarderDernierDossier(TypeOperation.ExportExcel, cheminFichierComplet);

            return cheminFichierComplet;
        }

        /// <summary>
        /// Crée l'onglet de synthèse du projet, affichant les KPIs globaux et un résumé par ouvrier enrichi.
        /// Les données proviennent directement du rapport d'analyse, du planning consolidé et de la structure du projet.
        /// </summary>
        /// <param name="package">Le package Excel courant.</param>
        /// <param name="exportData">Le DTO complet contenant le rapport, le planning et les données du projet.</param>
        private void _CreerOngletSynthese(ExcelPackage package, ExportDataProjetDto exportData)
        {
            var worksheet = package.Workbook.Worksheets.Add("SYNTHESE");
            var row = 1;
            var report = exportData.Report;
            var planning = exportData.Planning;


            var coutParOuvrierId = exportData.PoolOuvriers
                .ToDictionary(o => o.OuvrierId, o => o.CoutJournalier);

            worksheet.Cells[row, 1].Value = "PROJET";
            _FormatAsHeader(worksheet.Cells[row, 1], Color.DarkBlue);
            row += 2;

            worksheet.Cells[row, 1].Value = "Date début projet";
            worksheet.Cells[row, 2].Value = planning.DateDebutProjet.ToString("dd/MM/yyyy");
            row++;
            worksheet.Cells[row, 1].Value = "Date fin projet";
            worksheet.Cells[row, 2].Value = planning.DateFinProjet.ToString("dd/MM/yyyy");
            row++;
            worksheet.Cells[row, 1].Value = "Durée (jours ouvrés)";
            worksheet.Cells[row, 2].Value = $"{report.SyntheseProjet.DureeJoursOuvres} jours";
            row++;
            worksheet.Cells[row, 1].Value = "Effort total (jours-homme)";
            worksheet.Cells[row, 2].Value = $"{report.SyntheseProjet.EffortTotalJoursHomme:F1} jours";
            row++;
            worksheet.Cells[row, 1].Value = "Coût RH total";
            worksheet.Cells[row, 2].Value = $"{report.SyntheseProjet.CoutTotalRh:N2}€";
            row++;
            worksheet.Cells[row, 1].Value = "Coût Indirect total";
            worksheet.Cells[row, 2].Value = $"{report.SyntheseProjet.CoutTotalIndirect:N2}€";
            row++;
            worksheet.Cells[row, 1].Value = "Coût Total du Projet";
            worksheet.Cells[row, 2].Value = $"{report.SyntheseProjet.CoutTotalProjet:N2}€";
            row += 3;


            worksheet.Cells[row, 1].Value = "OUVRIER";
            worksheet.Cells[row, 2].Value = "MÉTIER PRINCIPAL";
            worksheet.Cells[row, 3].Value = "T% OCCUPATION";
            worksheet.Cells[row, 4].Value = "T% FRAGMENTATION";
            worksheet.Cells[row, 5].Value = "HEURES TOTALES";
            worksheet.Cells[row, 6].Value = "JOURS TRAV.";
            worksheet.Cells[row, 7].Value = "COÛT/JOUR";
            worksheet.Cells[row, 8].Value = "SOUS-TOTAL";

            var headerRange = worksheet.Cells[row, 1, row, 8];
            _FormatAsHeader(headerRange, Color.LightBlue);
            row++;

            var ouvriersTries = report.AnalysesOuvriers
                                        .OrderBy(o => o.MetierPrincipalNom)
                                        .ThenBy(o => o.NomComplet)
                                        .ToList();

            foreach (var ouvrier in ouvriersTries)
            {

                var coutJournalier = coutParOuvrierId[ouvrier.OuvrierId];
                var sousTotal = ouvrier.JoursTravailles * coutJournalier;

                worksheet.Cells[row, 1].Value = ouvrier.NomComplet;
                worksheet.Cells[row, 2].Value = ouvrier.MetierPrincipalNom;
                worksheet.Cells[row, 3].Value = ouvrier.TauxOccupation;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "0.0%";
                worksheet.Cells[row, 4].Value = ouvrier.TauxFragmentation;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "0.0%";
                worksheet.Cells[row, 5].Value = $"{ouvrier.HeuresTravaillees:F0}h";
                worksheet.Cells[row, 6].Value = ouvrier.JoursTravailles;
                worksheet.Cells[row, 7].Value = coutJournalier;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0 €";
                worksheet.Cells[row, 8].Value = sousTotal;
                worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0 €";

                row++;
            }

            worksheet.Cells[row, 7].Value = "Total Vérif. RH:";
            worksheet.Cells[row, 7].Style.Font.Bold = true;
            worksheet.Cells[row, 8].Formula = $"SUM(H{headerRange.Start.Row + 1}:H{row - 1})";
            worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0 €";
            worksheet.Cells[row, 8].Style.Font.Bold = true;

            worksheet.Cells.AutoFitColumns();
        }

        private void _CreerOngletOuvrier(
            ExcelPackage package,
            AnalyseOuvrierReport analyseOuvrier,
            IReadOnlyList<SegmentDeTravail> segmentsOuvrier,
            ConfigurationPlanification config,
            ConsolidatedPlanning planningGlobal)
        {
            var nomOnglet = _NettoyerNomOnglet(analyseOuvrier.NomComplet);
            var worksheet = package.Workbook.Worksheets.Add(nomOnglet);
            var row = 1;

            worksheet.Cells[row, 1].Value = $"Planning {analyseOuvrier.NomComplet}";
            _FormatAsHeader(worksheet.Cells[row, 1, row, 5], Color.DarkGreen);
            row++;

            worksheet.Cells[row, 1].Value = $"Métier : {analyseOuvrier.MetierPrincipalNom}";
            worksheet.Cells[row, 1].Style.Font.Italic = true;
            worksheet.Cells[row, 1].Style.Font.Color.SetColor(Color.DarkGray);
            row += 2;

            worksheet.Cells[row, 1].Value = "DATE/HEURE";
            worksheet.Cells[row, 2].Value = "JOUR";
            worksheet.Cells[row, 3].Value = "TÂCHE";
            worksheet.Cells[row, 4].Value = "BLOC";
            worksheet.Cells[row, 5].Value = "DURÉE";

            var planningHeaderRange = worksheet.Cells[row, 1, row, 5];
            _FormatAsHeader(planningHeaderRange, Color.LightBlue);
            row++;

            var dateDebutProjet = planningGlobal.DateDebutProjet.Date;
            var dateFinProjet = planningGlobal.DateFinProjet.Date;

            var segmentsParJour = segmentsOuvrier.GroupBy(s => s.Jour.Date)
                                                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.HeureDebut).ToList());

            for (var jourCourant = dateDebutProjet; jourCourant <= dateFinProjet; jourCourant = jourCourant.AddDays(1))
            {
                var estJourOuvert = config.JoursOuvres.Contains(jourCourant.DayOfWeek);

                if (segmentsParJour.TryGetValue(jourCourant, out var segmentsDuJour) && segmentsDuJour.Any())
                {
                    foreach (var segment in segmentsDuJour)
                    {
                        worksheet.Cells[row, 1].Value = $"{segment.Jour:dd/MM/yyyy} ({segment.HeureDebut:hh\\:mm} - {segment.HeureFin:hh\\:mm})";
                        worksheet.Cells[row, 2].Value = segment.Jour.ToString("dddd", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
                        worksheet.Cells[row, 3].Value = segment.TacheNom;
                        worksheet.Cells[row, 4].Value = segment.BlocId;
                        worksheet.Cells[row, 5].Value = $"{segment.HeuresTravaillees:F1}h";
                        row++;
                    }
                }
                else
                {
                    worksheet.Cells[row, 1].Value = jourCourant.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 2].Value = jourCourant.ToString("dddd", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
                    worksheet.Cells[row, 3].Value = "---";
                    worksheet.Cells[row, 4].Value = "";
                    worksheet.Cells[row, 5].Value = "---";

                    var rowRange = worksheet.Cells[row, 1, row, 5];
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    if (estJourOuvert)
                    {
                        rowRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }
                    else
                    {
                        rowRange.Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
                        rowRange.Style.Font.Color.SetColor(Color.White);
                    }
                    row++;
                }
            }

            worksheet.Cells.AutoFitColumns();
        }

        private void _FormatAsHeader(ExcelRange range, Color couleurFond)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(couleurFond);
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        private string _NettoyerNomOnglet(string nom)
        {
            if (string.IsNullOrEmpty(nom))
                return "Ouvrier";

            var caracteresInterdits = new char[] { '/', '\\', ':', '*', '?', '[', ']' };
            var nomNettoye = nom;

            foreach (var c in caracteresInterdits)
            {
                nomNettoye = nomNettoye.Replace(c, '_');
            }

            return nomNettoye.Length > 31 ? nomNettoye.Substring(0, 31) : nomNettoye;
        }
    }
}