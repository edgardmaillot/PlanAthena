// Fichier: Services/Export/PlanningExcelExportService.cs
// VERSION FINALE CORRIGÉE - Fusion de la structure robuste de l'ancienne version
// avec la logique de données correcte de la nouvelle architecture.

using OfficeOpenXml;
using OfficeOpenXml.Style;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlanAthena.Services.Export
{
    public class PlanningExcelExportService
    {
        private readonly CheminsPrefereService _cheminsPrefereService;

        public PlanningExcelExportService(CheminsPrefereService cheminsPrefereService)
        {
            _cheminsPrefereService = cheminsPrefereService ?? throw new ArgumentNullException(nameof(cheminsPrefereService));
        }

        public async Task ExporterPlanningComplet(
            AnalysePlanificationDto rapport,
            PlanificationResultDto resultatPlanification,
            ConfigurationPlanification configuration,
            string fullFilePath)
        {
            if (rapport == null) throw new ArgumentNullException(nameof(rapport));
            if (resultatPlanification?.ResultatBrut?.OptimisationResultat == null) throw new ArgumentNullException(nameof(resultatPlanification));
            if (string.IsNullOrEmpty(fullFilePath)) throw new ArgumentNullException(nameof(fullFilePath));

            await GenererFichierExcel(rapport, resultatPlanification.ResultatBrut.OptimisationResultat, configuration, fullFilePath);

            _cheminsPrefereService.SauvegarderDernierDossier(TypeOperation.ExportExcel, Path.GetDirectoryName(fullFilePath));
        }

        private async Task GenererFichierExcel(
            AnalysePlanificationDto rapport,
            PlanningOptimizationResultDto resultatOptimisation,
            ConfigurationPlanification configuration,
            string filePath)
        {
            using var package = new ExcelPackage();

            CreerOngletSynthese(package, rapport, configuration);

            foreach (var ouvrierAnalyse in rapport.AnalyseOuvriers)
            {
                var feuilleOuvrier = resultatOptimisation.FeuillesDeTemps.FirstOrDefault(f => f.OuvrierId == ouvrierAnalyse.OuvrierId);
                var affectationsOuvrier = resultatOptimisation.Affectations
                    .Where(a => a.OuvrierId == ouvrierAnalyse.OuvrierId)
                    .ToList();

                CreerOngletOuvrier(
                    package,
                    ouvrierAnalyse,
                    feuilleOuvrier,
                    affectationsOuvrier,
                    rapport.SyntheseProjet.DateDebut,
                    rapport.SyntheseProjet.DateFin,
                    configuration);
            }

            await Task.Run(() => package.SaveAs(new FileInfo(filePath)));
        }

        private void CreerOngletSynthese(ExcelPackage package, AnalysePlanificationDto rapport, ConfigurationPlanification configuration)
        {
            var worksheet = package.Workbook.Worksheets.Add("SYNTHESE");
            var row = 1;
            var synthese = rapport.SyntheseProjet;

            worksheet.Cells[row, 1, row, 6].Value = "PROJET";
            FormatAsHeader(worksheet.Cells[row, 1, row, 6], Color.DarkBlue);
            row++;

            worksheet.Cells[row, 1].Value = "Nom";
            worksheet.Cells[row, 2].Value = synthese.NomProjet; row++;
            worksheet.Cells[row, 1].Value = "Type optimisation";
            worksheet.Cells[row, 2].Value = configuration.TypeDeSortie; row++;
            worksheet.Cells[row, 1].Value = "Date début";
            worksheet.Cells[row, 2].Value = synthese.DateDebut.ToString("dd/MM/yyyy"); row++;
            worksheet.Cells[row, 1].Value = "Date fin";
            worksheet.Cells[row, 2].Value = synthese.DateFin.ToString("dd/MM/yyyy"); row++;
            worksheet.Cells[row, 1].Value = "Durée";
            worksheet.Cells[row, 2].Value = $"{synthese.DureeJoursCalendaires} jours"; row++;

            worksheet.Cells[row, 1].Value = "Coût total";
            worksheet.Cells[row, 2].Value = (synthese.CoutTotalEstime / 100.0m) ?? 0;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00 €"; row++;
            worksheet.Cells[row, 1].Value = "Coût Main d'Oeuvre (RH)";
            worksheet.Cells[row, 2].Value = (synthese.CoutTotalRhEstime / 100.0m) ?? 0;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00 €"; row++;
            worksheet.Cells[row, 1].Value = "Coût Indirect";
            worksheet.Cells[row, 2].Value = (synthese.CoutTotalIndirectEstime / 100.0m) ?? 0;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00 €"; row++;

            worksheet.Cells[row, 1].Value = "Total Jours-Homme";
            worksheet.Cells[row, 2].Value = synthese.TotalJoursHommeTravailles; row += 2;

            worksheet.Cells[row, 1, row, 6].Value = "SYNTHESE OUVRIERS";
            FormatAsHeader(worksheet.Cells[row, 1, row, 6], Color.FromArgb(0, 112, 192));
            row++;

            worksheet.Cells[row, 1].Value = "Nom Complet";
            worksheet.Cells[row, 2].Value = "T% Occupation";
            worksheet.Cells[row, 3].Value = "T% Fragmentation";
            worksheet.Cells[row, 4].Value = "Heures Totales";
            worksheet.Cells[row, 5].Value = "Jours Travaillés";
            worksheet.Cells[row, 6].Value = "Taux Journalier";
            FormatAsSubHeader(worksheet.Cells[row, 1, row, 6]);
            row++;

            foreach (var ouvrier in rapport.AnalyseOuvriers.OrderBy(o => o.NomComplet))
            {
                worksheet.Cells[row, 1].Value = ouvrier.NomComplet;
                worksheet.Cells[row, 2].Value = ouvrier.TauxOccupation / 100;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "0.0%";
                worksheet.Cells[row, 3].Value = ouvrier.TauxFragmentation / 100;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "0.0%";
                worksheet.Cells[row, 4].Value = $"{ouvrier.HeuresTravaillees:F0}h";
                worksheet.Cells[row, 5].Value = ouvrier.JoursTravaillesUniques;
                worksheet.Cells[row, 6].Value = ouvrier.CoutJournalier;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00 €";
                row++;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreerOngletOuvrier(
            ExcelPackage package,
            AnalyseOuvrierDto ouvrier,
            FeuilleDeTempsOuvrierDto feuilleOuvrier,
            List<AffectationDto> affectationsOuvrier,
            DateTime dateDebutProjet,
            DateTime dateFinProjet,
            ConfigurationPlanification configuration)
        {
            var nomOnglet = NettoyerNomOnglet(ouvrier.NomComplet);
            var worksheet = package.Workbook.Worksheets.Add(nomOnglet);
            var row = 1;

            worksheet.Cells[row, 1, row, 5].Value = $"Planning - {ouvrier.NomComplet}";
            FormatAsHeader(worksheet.Cells[row, 1, row, 5], Color.DarkGreen);
            row += 2;
            worksheet.Cells[row, 1].Value = "Métier Principal";
            worksheet.Cells[row, 2].Value = ouvrier.MetierPrincipal; row++;
            worksheet.Cells[row, 1].Value = "Occupation";
            worksheet.Cells[row, 2].Value = $"{ouvrier.TauxOccupation:F1}%"; row++;
            worksheet.Cells[row, 1].Value = "Fragmentation";
            worksheet.Cells[row, 2].Value = $"{ouvrier.TauxFragmentation:F1}%"; row++;
            worksheet.Cells[row, 1].Value = "Heures Totales";
            worksheet.Cells[row, 2].Value = $"{ouvrier.HeuresTravaillees:F0}h"; row++;
            worksheet.Cells[row, 1].Value = "Taux Journalier";
            worksheet.Cells[row, 2].Value = ouvrier.CoutJournalier;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00 €"; row++;
            row++;
            worksheet.Cells[row, 1, row, 5].Value = "PLANNING DETAILLE";
            FormatAsHeader(worksheet.Cells[row, 1, row, 5], Color.FromArgb(0, 112, 192));
            row++;
            worksheet.Cells[row, 1].Value = "DATE";
            worksheet.Cells[row, 2].Value = "JOUR";
            worksheet.Cells[row, 3].Value = "TÂCHE";
            worksheet.Cells[row, 4].Value = "BLOC";
            worksheet.Cells[row, 5].Value = "DURÉE";
            FormatAsSubHeader(worksheet.Cells[row, 1, row, 5]);
            row++;

            var creneaux = GenererCreneauxDepuisFeuilleDeTemps(feuilleOuvrier, affectationsOuvrier, dateDebutProjet, dateFinProjet, configuration);

            foreach (var creneau in creneaux)
            {
                worksheet.Cells[row, 1].Value = creneau.Date;
                worksheet.Cells[row, 1].Style.Numberformat.Format = "dd/mm/yyyy";
                worksheet.Cells[row, 2].Value = creneau.Date.ToString("dddd", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
                worksheet.Cells[row, 3].Value = creneau.TacheNom;
                worksheet.Cells[row, 4].Value = creneau.BlocId;
                worksheet.Cells[row, 5].Value = creneau.DureeHeures > 0 ? $"{creneau.DureeHeures}h" : "";

                if (creneau.EstJourVide)
                {
                    var rowRange = worksheet.Cells[row, 1, row, 5];
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    rowRange.Style.Font.Color.SetColor(Color.DarkGray);
                }
                row++;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private List<CreneauTravailDto> GenererCreneauxDepuisFeuilleDeTemps(
            FeuilleDeTempsOuvrierDto feuilleOuvrier,
            List<AffectationDto> affectationsOuvrier,
            DateTime dateDebutProjet,
            DateTime dateFinProjet,
            ConfigurationPlanification configuration)
        {
            var creneaux = new List<CreneauTravailDto>();
            if (feuilleOuvrier == null) return creneaux;
            // 1. Initialiser la variable de suivi pour le dernier travail connu
            AffectationDto derniereAffectationConnue = null;
            var tacheParHeureUtc = new Dictionary<DateTime, AffectationDto>();
            foreach (var affectation in affectationsOuvrier)
            {
                // On utilise DureeOriginaleHeures pour les jalons, et DureeHeures pour les tâches
                var dureeReelle = affectation.EstJalon ? (affectation.DureeOriginaleHeures ?? affectation.DureeHeures) : affectation.DureeHeures;
                for (int i = 0; i < dureeReelle; i++)
                {
                    tacheParHeureUtc[affectation.DateDebut.AddHours(i)] = affectation;
                }
            }

            for (var dateActuelle = dateDebutProjet.Date; dateActuelle <= dateFinProjet.Date; dateActuelle = dateActuelle.AddDays(1))
            {
                var jourUtc = new DateTime(dateActuelle.Year, dateActuelle.Month, dateActuelle.Day, 0, 0, 0, DateTimeKind.Utc);

                if (feuilleOuvrier.PlanningJournalier.TryGetValue(jourUtc, out var masque))
                {
                    var tachesDuJour = new List<(AffectationDto affectation, int duree)>();

                    // CORRECTION: La boucle doit couvrir toutes les heures potentielles de travail, y compris les heures supplémentaires
                    for (int heureIndex = 0; heureIndex < 24; heureIndex++)
                    {
                        if ((masque & (1L << heureIndex)) != 0)
                        {
                            var heureDebutSlotUtc = jourUtc.AddHours(configuration.HeureDebutJournee + heureIndex);

                            if (tacheParHeureUtc.TryGetValue(heureDebutSlotUtc, out var affectation))
                            {
                                if (tachesDuJour.Any() && tachesDuJour.Last().affectation.TacheId == affectation.TacheId)
                                {
                                    // Cas où un masque existe mais aucune tâche n'est trouvée
                                    string nomTacheFallback = "Travail non identifié";
                                    string blocIdFallback = "";

                                    // 1. Utiliser la dernière tâche connue si elle existe
                                    if (derniereAffectationConnue != null)
                                    {
                                        nomTacheFallback = derniereAffectationConnue.TacheNom + " (suite)";
                                        blocIdFallback = derniereAffectationConnue.BlocId;
                                    }

                                    creneaux.Add(new CreneauTravailDto
                                    {
                                        Date = dateActuelle,
                                        TacheNom = nomTacheFallback,
                                        BlocId = blocIdFallback,
                                        EstJourVide = false,
                                        DureeHeures = (int)System.Numerics.BitOperations.PopCount((ulong)masque)
                                    });
                                    // 2. Mettre à jour la dernière tâche connue
                                    derniereAffectationConnue = affectation;
                                }
                                else
                                {
                                    tachesDuJour.Add((affectation, 1));
                                }
                            }
                        }
                    }

                    if (!tachesDuJour.Any() && masque != 0) // Cas où un masque existe mais aucune tâche n'est trouvée, c'est le reliquat de la tâche de la veille
                    {
                        creneaux.Add(new CreneauTravailDto { Date = dateActuelle, TacheNom = "Travail non identifié", EstJourVide = false, DureeHeures = (int)System.Numerics.BitOperations.PopCount((ulong)masque) });
                    }
                    else if (tachesDuJour.Any())
                    {
                        foreach (var (affectation, duree) in tachesDuJour)
                        {
                            creneaux.Add(new CreneauTravailDto
                            {
                                Date = dateActuelle,
                                TacheNom = affectation.TacheNom,
                                BlocId = affectation.BlocId,
                                DureeHeures = duree,
                                EstJourVide = false
                            });
                        }
                    }
                    else // Masque est 0
                    {
                        creneaux.Add(new CreneauTravailDto { Date = dateActuelle, TacheNom = "---", EstJourVide = true });
                    }
                }
                else
                {
                    creneaux.Add(new CreneauTravailDto { Date = dateActuelle, TacheNom = "---", EstJourVide = true });
                }
            }
            return creneaux;
        }


private void FormatAsHeader(ExcelRange range, Color couleurFond)
        {
            range.Merge = true;
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(couleurFond);
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private void FormatAsSubHeader(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        private string NettoyerNomOnglet(string nom)
        {
            if (string.IsNullOrEmpty(nom)) return "Ouvrier";
            var caracteresInterdits = new char[] { '/', '\\', ':', '*', '?', '[', ']' };
            foreach (var c in caracteresInterdits) nom = nom.Replace(c, '_');
            return nom.Length > 31 ? nom.Substring(0, 31) : nom;
        }
    }
}