// Fichier: Services/Business/PlanningResultatService.cs
// MODIFIÉ - Version complète de la méthode GenererRapportAnalyse.

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PlanAthena.Services.Business
{
    public class PlanningResultatService
    {
        public PlanningResultatService() { }

        public AnalysePlanificationDto GenererRapportAnalyse(
            PlanificationResultDto resultatPlanification,
            IReadOnlyList<Ouvrier> poolOuvriers,
            IReadOnlyList<Metier> poolMetiers,
            ConfigurationPlanification configuration)
        {
            var resultatBrut = resultatPlanification.ResultatBrut?.OptimisationResultat;
            var feuillesDeTemps = resultatBrut?.FeuillesDeTemps;

            // Cas 1 : Pas d'optimisation ou pas d'ouvrier réel affecté -> on se base sur l'estimation
            if (feuillesDeTemps == null || !feuillesDeTemps.Any())
            {
                return GenererRapportDepuisEstimation(resultatPlanification, configuration);
            }

            // Cas 2 : On a un planning détaillé à analyser
            var metiersMap = poolMetiers.ToDictionary(m => m.MetierId);
            var analysesOuvriers = new List<AnalyseOuvrierDto>();

            foreach (var feuille in feuillesDeTemps)
            {
                var ouvrier = poolOuvriers.First(o => o.OuvrierId == feuille.OuvrierId);

                // Calculs de KPIs à partir des masques de bits pour plus de précision et performance
                var heuresTravaillees = (double)feuille.PlanningJournalier.Values.Sum(masque => BitOperations.PopCount((ulong)masque));
                var joursTravaillesUniques = feuille.PlanningJournalier.Count;

                var dureeJourneeStandard = (double)configuration.DureeJournaliereStandardHeures;
                var capaciteSurJoursTravailles = joursTravaillesUniques * dureeJourneeStandard;
                var tauxOccupation = (capaciteSurJoursTravailles > 0)
                    ? (heuresTravaillees / capaciteSurJoursTravailles) * 100
                    : 0;

                var premiereTacheDate = feuille.PlanningJournalier.Keys.Min();
                var derniereTacheDate = feuille.PlanningJournalier.Keys.Max();
                var joursOuvresPeriode = CalculerJoursOuvres(premiereTacheDate, derniereTacheDate, configuration.JoursOuvres);
                var capaciteSurPeriodeTotale = joursOuvresPeriode * dureeJourneeStandard;

                var tauxFragmentation = 0.0;
                if (capaciteSurPeriodeTotale > 0)
                {
                    var efficience = heuresTravaillees / capaciteSurPeriodeTotale;
                    tauxFragmentation = (1 - efficience) * 100;
                    if (tauxFragmentation < 0) tauxFragmentation = 0;
                }

                var metierPrincipal = ouvrier.Competences.FirstOrDefault(c => c.EstMetierPrincipal);
                var nomMetierPrincipal = metierPrincipal != null && metiersMap.ContainsKey(metierPrincipal.MetierId) ? metiersMap[metierPrincipal.MetierId].Nom : "Non défini";
                var autresMetiers = ouvrier.Competences.Where(c => !c.EstMetierPrincipal)
                    .Select(c => metiersMap.ContainsKey(c.MetierId) ? metiersMap[c.MetierId].Nom : null)
                    .Where(nom => nom != null).ToList();

                analysesOuvriers.Add(new AnalyseOuvrierDto
                {
                    OuvrierId = ouvrier.OuvrierId,
                    NomComplet = ouvrier.NomComplet,
                    CoutJournalier = ouvrier.CoutJournalier,
                    MetierPrincipal = nomMetierPrincipal,
                    AutresMetiers = autresMetiers,
                    JoursTravaillesUniques = joursTravaillesUniques,
                    HeuresTravaillees = heuresTravaillees,
                    TauxOccupation = Math.Round(tauxOccupation, 1),
                    TauxFragmentation = Math.Round(tauxFragmentation, 1)
                });
            }

            var affectations = resultatBrut.Affectations;
            var dateDebut = affectations.Any(a => !a.OuvrierId.StartsWith("VIRTUAL")) ? affectations.Where(a => !a.OuvrierId.StartsWith("VIRTUAL")).Min(a => a.DateDebut) : DateTime.Today;
            var dateFin = affectations.Any(a => !a.OuvrierId.StartsWith("VIRTUAL")) ? affectations.Where(a => !a.OuvrierId.StartsWith("VIRTUAL")).Max(a => a.DateDebut.AddHours(a.DureeHeures)) : DateTime.Today;
            int totalJoursHomme = analysesOuvriers.Sum(o => o.JoursTravaillesUniques);

            var syntheseParMetier = poolOuvriers
                .Select(o => o.MetierId)
                .Distinct()
                .Select(metierId =>
                {
                    var ouvriersDeCeMetier = poolOuvriers.Where(o => o.MetierId == metierId).Select(o => o.OuvrierId).ToHashSet();
                    var jours = analysesOuvriers.Where(a => ouvriersDeCeMetier.Contains(a.OuvrierId)).Sum(a => a.JoursTravaillesUniques);
                    if (jours == 0) return null;
                    return new SyntheseMetierAnalyseDto
                    {
                        MetierId = metierId,
                        NomMetier = metiersMap.ContainsKey(metierId) ? metiersMap[metierId].Nom : "Inconnu",
                        TotalJoursHommeTravailles = jours
                    };
                }).Where(s => s != null).OrderBy(s => s.NomMetier).ToList();

            var syntheseProjet = new SyntheseProjetDto
            {
                NomProjet = configuration.Description,
                DateDebut = dateDebut,
                DateFin = dateFin,
                DureeJoursCalendaires = (int)Math.Ceiling((dateFin - dateDebut).TotalDays),
                CoutTotalEstime = resultatBrut.CoutTotalEstime,
                CoutTotalRhEstime = resultatBrut.CoutTotalRhEstime,
                CoutTotalIndirectEstime = resultatBrut.CoutTotalIndirectEstime,
                TotalJoursHommeTravailles = totalJoursHomme,
                SyntheseParMetier = syntheseParMetier
            };

            return new AnalysePlanificationDto
            {
                SyntheseProjet = syntheseProjet,
                AnalyseOuvriers = analysesOuvriers.OrderByDescending(o => o.TauxOccupation).ToList()
            };
        }

        private AnalysePlanificationDto GenererRapportDepuisEstimation(PlanificationResultDto resultatPlanification, ConfigurationPlanification configuration)
        {
            var estimation = resultatPlanification.ResultatBrut?.AnalyseStatiqueResultat;
            if (estimation == null) return new AnalysePlanificationDto();

            int dureeEstimeeJours = 0;
            int effortEstimeJoursHomme = 0;

            if (estimation.DureeTotaleEstimeeEnSlots.HasValue && configuration.HeuresTravailEffectifParJour > 0)
            {
                var heuresEstimees = (double)estimation.DureeTotaleEstimeeEnSlots.Value;
                var heuresParJour = (double)configuration.HeuresTravailEffectifParJour;
                dureeEstimeeJours = (int)Math.Ceiling(heuresEstimees / heuresParJour);
                effortEstimeJoursHomme = dureeEstimeeJours;
            }

            var syntheseProjet = new SyntheseProjetDto
            {
                NomProjet = configuration.Description,
                DateDebut = configuration.DateDebutSouhaitee ?? DateTime.Today,
                DateFin = (configuration.DateDebutSouhaitee ?? DateTime.Today).AddDays(dureeEstimeeJours),
                DureeJoursCalendaires = dureeEstimeeJours,
                CoutTotalEstime = estimation.CoutTotalEstime,
                TotalJoursHommeTravailles = effortEstimeJoursHomme,
                CoutTotalRhEstime = null, // Pas de détail en mode estimation
                CoutTotalIndirectEstime = null // Pas de détail en mode estimation
            };

            return new AnalysePlanificationDto
            {
                SyntheseProjet = syntheseProjet,
                AnalyseOuvriers = new List<AnalyseOuvrierDto>()
            };
        }

        private int CalculerJoursOuvres(DateTime dateDebut, DateTime dateFin, List<DayOfWeek> joursOuvres)
        {
            if (dateDebut > dateFin || joursOuvres == null || !joursOuvres.Any()) return 0;
            int compteur = 0;
            for (var date = dateDebut.Date; date <= dateFin.Date; date = date.AddDays(1))
            {
                if (joursOuvres.Contains(date.DayOfWeek)) compteur++;
            }
            return compteur > 0 ? compteur : 1;
        }
    }
}