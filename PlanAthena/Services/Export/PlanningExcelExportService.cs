// Fichier: Services/Export/PlanningExcelExportService.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Infrastructure;

namespace PlanAthena.Services.Export
{
    /// <summary>
    /// Service principal pour l'export des plannings en Excel multi-onglets
    /// Génère 1 fichier avec onglet synthèse + 1 onglet par ouvrier
    /// </summary>
    public class PlanningExcelExportService
    {
        private readonly CheminsPrefereService _cheminsPrefereService;

        public PlanningExcelExportService(CheminsPrefereService cheminsPrefereService)
        {
            _cheminsPrefereService = cheminsPrefereService ?? throw new ArgumentNullException(nameof(cheminsPrefereService));

            // EPPlus 4.x est libre de droits, pas de configuration nécessaire
        }

        /// <summary>
        /// Point d'entrée principal : exporte un planning complet en Excel
        /// </summary>
        public async Task<string> ExporterPlanningComplet(
            PlanificationResultDto resultat,
            IReadOnlyList<Ouvrier> ouvriers,
            IReadOnlyList<Metier> metiers,
            string nomProjet,
            ConfigurationPlanification configuration,
            ConfigurationExport config = null)
        {
            if (resultat?.ResultatBrut?.OptimisationResultat?.Affectations == null)
                throw new ArgumentException("Les résultats de planification sont requis");

            config ??= new ConfigurationExport();

            // 1. Consolidation des données
            var donneesExport = await ConsoliderDonnees(resultat, ouvriers, metiers, nomProjet, configuration);

            // 2. Génération du fichier Excel
            var cheminFichier = await GenererFichierExcel(donneesExport, config);

            // 3. Sauvegarder le chemin utilisé
            _cheminsPrefereService.SauvegarderDernierDossier(TypeOperation.ExportExcel, cheminFichier);

            return cheminFichier;
        }

        /// <summary>
        /// Consolide les données brutes en DTOs structurés pour l'export
        /// </summary>
        private async Task<PlanningExportDto> ConsoliderDonnees(
            PlanificationResultDto resultat,
            IReadOnlyList<Ouvrier> ouvriers,
            IReadOnlyList<Metier> metiers,
            string nomProjet,
            ConfigurationPlanification configuration)
        {
            var affectations = resultat.ResultatBrut.OptimisationResultat.Affectations;

            // Calcul des dates du projet
            var dateDebut = affectations.Min(a => a.DateDebut);
            var dateFin = affectations.Max(a => a.DateDebut.AddHours(a.DureeHeures));
            var dureeJours = (int)Math.Ceiling((dateFin - dateDebut).TotalDays);

            // Consolidation par métier pour la synthèse (gardé pour référence future)
            var statistiquesMetiers = ConsoliderStatistiquesMetiers(affectations, ouvriers, metiers);

            // Planning détaillé par ouvrier
            var planningsOuvriers = ConsoliderPlanningsOuvriers(affectations, ouvriers, metiers, dateDebut, dateFin, configuration);

            // Calcul du coût total
            var coutTotal = affectations.Sum(a =>
            {
                var ouvrier = ouvriers.FirstOrDefault(o => o.OuvrierId == a.OuvrierId);
                return ouvrier?.CoutJournalier * (decimal)(a.DureeHeures / 8.0) ?? 0;
            });

            return new PlanningExportDto
            {
                NomProjet = nomProjet,
                TypeSortie = configuration.TypeDeSortie, // NOUVEAU : Type de sortie
                DateDebut = dateDebut,
                DateFin = dateFin,
                DureeJours = dureeJours,
                CoutTotal = coutTotal,
                Metiers = statistiquesMetiers,
                Ouvriers = planningsOuvriers
            };
        }

        /// <summary>
        /// Consolide les statistiques par métier pour l'onglet synthèse
        /// </summary>
        private List<SyntheseMetierDto> ConsoliderStatistiquesMetiers(
            IReadOnlyList<PlanAthena.Core.Facade.Dto.Output.AffectationDto> affectations,
            IReadOnlyList<Ouvrier> ouvriers,
            IReadOnlyList<Metier> metiers)
        {
            // CORRECTION : Regrouper les ouvriers uniques par métier
            var ouvriersUniques = ouvriers.GroupBy(o => o.OuvrierId).Select(g => g.First()).ToList();

            return metiers.Select(metier =>
            {
                var ouvriersMetier = ouvriers.Where(o => o.MetierId == metier.MetierId).ToList();
                var ouvriersUniquesMetier = ouvriersMetier.GroupBy(o => o.OuvrierId)
                    .Select(g => g.First()).ToList();

                var affectationsMetier = affectations.Where(a =>
                    ouvriersMetier.Any(o => o.OuvrierId == a.OuvrierId)).ToList();

                var heuresTravaillees = affectationsMetier.Sum(a => a.DureeHeures);
                var heuresTheorique = ouvriersUniquesMetier.Count * 8 *
                    (affectations.Max(a => a.DateDebut) - affectations.Min(a => a.DateDebut)).Days;

                var tauxOccupation = heuresTheorique > 0 ? (heuresTravaillees / heuresTheorique) * 100 : 0;

                return new SyntheseMetierDto
                {
                    NomMetier = metier.Nom,
                    TauxOccupation = Math.Round(tauxOccupation, 1),
                    HeuresTravaillees = heuresTravaillees,
                    NomsOuvriers = ouvriersUniquesMetier.Select(o => $"{o.Prenom} {o.Nom}").Distinct().ToList(),
                    CouleurHex = metier.CouleurHex ?? "#CCCCCC"
                };
            }).Where(s => s.HeuresTravaillees > 0).ToList();
        }

        /// <summary>
        /// Consolide le planning détaillé par ouvrier
        /// </summary>
        private List<PlanningOuvrierDto> ConsoliderPlanningsOuvriers(
            IReadOnlyList<PlanAthena.Core.Facade.Dto.Output.AffectationDto> affectations,
            IReadOnlyList<Ouvrier> ouvriers,
            IReadOnlyList<Metier> metiers,
            DateTime dateDebut,
            DateTime dateFin,
            ConfigurationPlanification configuration)
        {
            // CORRECTION : Regrouper par ouvrier unique (même OuvrierId = même personne)
            var ouvriersUniques = ouvriers.GroupBy(o => o.OuvrierId)
                .Select(g => g.First()) // Prendre le premier ouvrier de chaque groupe
                .ToList();

            return ouvriersUniques.Select(ouvrier =>
            {
                var affectationsOuvrier = affectations
                    .Where(a => a.OuvrierId == ouvrier.OuvrierId)
                    .ToList();

                // CORRECTION : Récupérer tous les métiers de cet ouvrier pour l'affichage
                var metiersOuvrier = ouvriers
                    .Where(o => o.OuvrierId == ouvrier.OuvrierId)
                    .Select(o => metiers.FirstOrDefault(m => m.MetierId == o.MetierId)?.Nom)
                    .Where(nom => !string.IsNullOrEmpty(nom))
                    .Distinct()
                    .ToList();

                var metierPrincipal = metiersOuvrier.FirstOrDefault() ?? "Non défini";
                var metiersAffiches = metiersOuvrier.Count > 1
                    ? $"{metierPrincipal} (+{metiersOuvrier.Count - 1} autres)"
                    : metierPrincipal;

                // CORRECTION : Nouveaux calculs KPI selon spécifications
                var heuresTravaillees = affectationsOuvrier.Sum(a => a.DureeHeures);
                var dureeJourneeStandard = configuration.DureeJournaliereStandardHeures;

                // Calcul Taux d'Occupation : Heures travaillées / (Jours travaillés × Durée standard)
                var joursTravailles = affectationsOuvrier.Select(a => a.DateDebut.Date).Distinct().Count();
                var tauxOccupation = joursTravailles > 0 && dureeJourneeStandard > 0
                    ? (heuresTravaillees / (joursTravailles * dureeJourneeStandard)) * 100
                    : 0;

                // Calcul Taux de Fragmentation : 1 - (Heures travaillées / (Jours ouvrés période × Durée standard))
                var tauxFragmentation = 0.0;
                if (affectationsOuvrier.Any())
                {
                    var premiereTache = affectationsOuvrier.Min(a => a.DateDebut.Date);
                    var derniereTache = affectationsOuvrier.Max(a => a.DateDebut.Date);
                    var joursOuvresPeriode = CalculerJoursOuvres(premiereTache, derniereTache, configuration.JoursOuvres);

                    if (joursOuvresPeriode > 0 && dureeJourneeStandard > 0)
                    {
                        var efficience = heuresTravaillees / (joursOuvresPeriode * dureeJourneeStandard);
                        tauxFragmentation = (1 - efficience) * 100;

                        // CORRECTION : Fragmentation ne peut pas être négative (si plus de 100% d'efficience)
                        if (tauxFragmentation < 0) tauxFragmentation = 0;
                    }
                }

                // Générer tous les créneaux (avec jours vides)
                var creneaux = GenererCreneauxComplets(affectationsOuvrier, dateDebut, dateFin);

                return new PlanningOuvrierDto
                {
                    Nom = ouvrier.Nom,
                    Prenom = ouvrier.Prenom,
                    Metier = metiersAffiches, // Afficher le métier principal + nombre d'autres
                    TauxOccupation = Math.Round(tauxOccupation, 1),
                    TauxFragmentation = Math.Round(tauxFragmentation, 1), // NOUVEAU KPI
                    HeuresTravaillees = heuresTravaillees,
                    Creneaux = creneaux
                };
            }).Where(p => p.Creneaux.Any()).ToList();
        }

        /// <summary>
        /// Génère tous les créneaux (travaillés + vides) pour un ouvrier
        /// CORRECTION : Supporte plusieurs tâches par jour
        /// </summary>
        private List<CreneauTravailDto> GenererCreneauxComplets(
            IReadOnlyList<PlanAthena.Core.Facade.Dto.Output.AffectationDto> affectationsOuvrier,
            DateTime dateDebut,
            DateTime dateFin)
        {
            var creneaux = new List<CreneauTravailDto>();
            var dateActuelle = dateDebut.Date;

            while (dateActuelle <= dateFin.Date)
            {
                // CORRECTION : Récupérer TOUTES les affectations du jour
                var affectationsJour = affectationsOuvrier
                    .Where(a => a.DateDebut.Date == dateActuelle)
                    .OrderBy(a => a.DateDebut) // Trier par heure de début
                    .ToList();

                if (affectationsJour.Any())
                {
                    // CORRECTION : Ajouter UNE LIGNE par tâche du jour
                    foreach (var affectation in affectationsJour)
                    {
                        creneaux.Add(new CreneauTravailDto
                        {
                            Date = dateActuelle,
                            TacheNom = affectation.TacheNom,
                            BlocId = affectation.BlocId ?? "",
                            DureeHeures = (int)affectation.DureeHeures,
                            EstJourVide = false
                        });
                    }
                }
                else
                {
                    // Jour sans affectation
                    creneaux.Add(new CreneauTravailDto
                    {
                        Date = dateActuelle,
                        TacheNom = "---------------",
                        BlocId = "",
                        DureeHeures = 0,
                        EstJourVide = true
                    });
                }

                dateActuelle = dateActuelle.AddDays(1);
            }

            return creneaux;
        }

        /// <summary>
        /// Génère le fichier Excel multi-onglets
        /// </summary>
        private async Task<string> GenererFichierExcel(PlanningExportDto donnees, ConfigurationExport config)
        {
            var dossierSortie = _cheminsPrefereService.ObtenirDernierDossierExport();
            var nomFichier = string.IsNullOrEmpty(config.NomFichier)
                ? ConfigurationExport.GenererNomFichierDefaut(donnees.NomProjet)
                : config.NomFichier;

            var cheminComplet = Path.Combine(dossierSortie, nomFichier);

            using var package = new ExcelPackage();

            // Créer l'onglet synthèse
            await CreerOngletSynthese(package, donnees);

            // Créer un onglet par ouvrier
            foreach (var ouvrier in donnees.Ouvriers)
            {
                await CreerOngletOuvrier(package, ouvrier, donnees.NomProjet);
            }

            // Sauvegarder le fichier (EPPlus 4.x - version synchrone)
            package.SaveAs(new FileInfo(cheminComplet));

            return cheminComplet;
        }

        /// <summary>
        /// Crée l'onglet synthèse du projet
        /// </summary>
        private async Task CreerOngletSynthese(ExcelPackage package, PlanningExportDto donnees)
        {
            var worksheet = package.Workbook.Worksheets.Add("SYNTHESE");
            var row = 1;

            // Section projet
            worksheet.Cells[row, 1].Value = "PROJET";
            FormatAsHeader(worksheet.Cells[row, 1], Color.DarkBlue);
            row += 2;

            worksheet.Cells[row, 1].Value = "Nom";
            worksheet.Cells[row, 2].Value = donnees.NomProjet;
            row++;

            worksheet.Cells[row, 1].Value = "Type optimisation";
            worksheet.Cells[row, 2].Value = donnees.TypeSortie;
            row++;

            worksheet.Cells[row, 1].Value = "Date début";
            worksheet.Cells[row, 2].Value = donnees.DateDebut.ToString("dd/MM/yyyy");
            row++;

            worksheet.Cells[row, 1].Value = "Date fin";
            worksheet.Cells[row, 2].Value = donnees.DateFin.ToString("dd/MM/yyyy");
            row++;

            worksheet.Cells[row, 1].Value = "Durée";
            worksheet.Cells[row, 2].Value = $"{donnees.DureeJours} jours";
            row++;

            worksheet.Cells[row, 1].Value = "Coût total";
            worksheet.Cells[row, 2].Value = $"{donnees.CoutTotal:N2}€";
            row += 3;

            // NOUVEAU : Section ouvriers au lieu des métiers
            worksheet.Cells[row, 1].Value = "OUVRIER";
            worksheet.Cells[row, 2].Value = "T% OCCUPATION";
            worksheet.Cells[row, 3].Value = "T% FRAGMENTATION";
            worksheet.Cells[row, 4].Value = "HEURES TOTALES";

            var headerRange = worksheet.Cells[row, 1, row, 4];
            FormatAsHeader(headerRange, Color.LightBlue);
            row++;

            // NOUVEAU : Trier par taux d'occupation décroissant pour repérer les surchargés
            var ouvriersTriés = donnees.Ouvriers.OrderByDescending(o => o.TauxOccupation).ToList();

            foreach (var ouvrier in ouvriersTriés)
            {
                worksheet.Cells[row, 1].Value = ouvrier.NomComplet;
                worksheet.Cells[row, 2].Value = $"{ouvrier.TauxOccupation:F1}%";
                worksheet.Cells[row, 3].Value = $"{ouvrier.TauxFragmentation:F1}%";
                worksheet.Cells[row, 4].Value = $"{ouvrier.HeuresTravaillees:F0}h";
                row++;
            }

            // Auto-ajustement des colonnes
            worksheet.Cells.AutoFitColumns();
        }

        /// <summary>
        /// Crée l'onglet détaillé d'un ouvrier
        /// </summary>
        private async Task CreerOngletOuvrier(ExcelPackage package, PlanningOuvrierDto ouvrier, string nomProjet)
        {
            var nomOnglet = NettoyerNomOnglet(ouvrier.NomComplet);
            var worksheet = package.Workbook.Worksheets.Add(nomOnglet);
            var row = 1;

            // En-tête ouvrier
            worksheet.Cells[row, 1].Value = $"Planning - {ouvrier.NomComplet}";
            FormatAsHeader(worksheet.Cells[row, 1, row, 5], Color.DarkGreen);
            row += 2;

            // Info ouvrier
            worksheet.Cells[row, 1].Value = "Métier";
            worksheet.Cells[row, 2].Value = ouvrier.Metier;
            row++;

            worksheet.Cells[row, 1].Value = "Occupation";
            worksheet.Cells[row, 2].Value = $"{ouvrier.TauxOccupation:F1}%";
            row++;

            worksheet.Cells[row, 1].Value = "Fragmentation";
            worksheet.Cells[row, 2].Value = $"{ouvrier.TauxFragmentation:F1}%";
            row++;

            worksheet.Cells[row, 1].Value = "Heures";
            worksheet.Cells[row, 2].Value = $"{ouvrier.HeuresTravaillees:F0}h";
            row += 2;

            // Headers planning
            worksheet.Cells[row, 1].Value = "DATE";
            worksheet.Cells[row, 2].Value = "JOUR";
            worksheet.Cells[row, 3].Value = "TÂCHE";
            worksheet.Cells[row, 4].Value = "BLOC";
            worksheet.Cells[row, 5].Value = "DURÉE";

            var planningHeaderRange = worksheet.Cells[row, 1, row, 5];
            FormatAsHeader(planningHeaderRange, Color.LightBlue);
            row++;

            // Créneaux de travail
            foreach (var creneau in ouvrier.Creneaux)
            {
                worksheet.Cells[row, 1].Value = creneau.Date.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = creneau.NomJour;
                worksheet.Cells[row, 3].Value = creneau.TacheFormatee;
                worksheet.Cells[row, 4].Value = creneau.BlocId;
                worksheet.Cells[row, 5].Value = creneau.DureeFormatee;

                // Formatage spécial pour les jours vides
                if (creneau.EstJourVide)
                {
                    var rowRange = worksheet.Cells[row, 1, row, 5];
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    rowRange.Style.Font.Color.SetColor(Color.DarkGray);
                }

                row++;
            }

            // Auto-ajustement des colonnes
            worksheet.Cells.AutoFitColumns();
        }

        /// <summary>
        /// Applique le formatage header (gras + couleur de fond)
        /// </summary>
        private void FormatAsHeader(ExcelRange range, Color couleurFond)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(couleurFond);
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Calcule le nombre de jours ouvrés entre deux dates (incluses)
        /// TODO v0.5 : Intégrer le vrai CalendrierTravail avec jours fériés/roulements/3x8
        /// Actuellement : Basé sur chkListJoursOuvres de MainForm (principe 5j/7)
        /// </summary>
        private int CalculerJoursOuvres(DateTime dateDebut, DateTime dateFin, List<DayOfWeek> joursOuvres)
        {
            if (dateDebut > dateFin) return 0;
            if (!joursOuvres.Any()) return 0; // Éviter division par zéro

            int compteur = 0;
            var dateActuelle = dateDebut.Date;

            while (dateActuelle <= dateFin.Date)
            {
                if (joursOuvres.Contains(dateActuelle.DayOfWeek))
                {
                    compteur++;
                }
                dateActuelle = dateActuelle.AddDays(1);
            }

            return compteur;
        }

        /// <summary>
        /// Nettoie un nom d'onglet pour Excel (31 chars max, pas de caractères spéciaux)
        /// </summary>
        private string NettoyerNomOnglet(string nom)
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