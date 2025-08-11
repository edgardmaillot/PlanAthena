// Fichier : Application/Services/PlanningAnalysisService.cs

using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Core.Application.Services
{
    public class PlanningAnalysisService : IPlanningAnalysisService
    {
        public Task<PlanningAnalysisReportDto> AnalyserLePlanningAsync(
            IReadOnlyList<AffectationDto> affectations,
            Chantier chantierDeReference)
        {
            var affectationsReelles = affectations
                .Where(a => !a.OuvrierId.StartsWith("VIRTUAL"))
                .ToList();

            if (!affectationsReelles.Any())
            {
                // ... (code inchangé pour le cas vide)
            }

            var kpisParOuvrier = new List<WorkerKpiDto>();
            var affectationsParOuvrier = affectationsReelles.GroupBy(a => a.OuvrierId);

            foreach (var groupeOuvrier in affectationsParOuvrier)
            {
                var ouvrierId = groupeOuvrier.Key;
                var ouvrier = chantierDeReference.Ouvriers.Values.First(o => o.Id.Value == ouvrierId);
                var sesAffectations = groupeOuvrier.ToList();

                var heuresTravaillees = sesAffectations.Sum(a => a.DureeHeures);

                // --- DÉBUT DE LA LOGIQUE CORRIGÉE (inspirée de l'Excel) ---
                var dureeJourneeStandard = chantierDeReference.Calendrier.DureeTravailEffectiveParJour.TotalHours;

                // Calcul Taux d'Occupation
                var joursTravaillesUniques = sesAffectations.Select(a => a.DateDebut.Date).Distinct().Count();
                var capaciteSurJoursTravailles = joursTravaillesUniques * dureeJourneeStandard;
                var tauxOccupation = (capaciteSurJoursTravailles > 0)
                    ? (heuresTravaillees / capaciteSurJoursTravailles) * 100
                    : 0;

                // Calcul Taux de Fragmentation
                var premiereTacheDate = sesAffectations.Min(a => a.DateDebut.Date);
                var derniereTacheDate = sesAffectations.Max(a => a.DateDebut.Date);
                var joursOuvresPeriode = CalculerJoursOuvres(premiereTacheDate, derniereTacheDate, chantierDeReference.Calendrier);
                var capaciteSurPeriodeTotale = joursOuvresPeriode * dureeJourneeStandard;

                var tauxFragmentation = 0.0;
                if (capaciteSurPeriodeTotale > 0)
                {
                    var efficience = heuresTravaillees / capaciteSurPeriodeTotale;
                    tauxFragmentation = (1 - efficience) * 100;
                    if (tauxFragmentation < 0) tauxFragmentation = 0; // La fragmentation ne peut être négative
                }
                // --- FIN DE LA LOGIQUE CORRIGÉE ---

                kpisParOuvrier.Add(new WorkerKpiDto
                {
                    OuvrierId = ouvrierId,
                    OuvrierNom = $"{ouvrier.Prenom} {ouvrier.Nom}",
                    JoursDePresence = joursTravaillesUniques, // Utiliser la valeur correcte
                    HeuresTravaillees = heuresTravaillees,
                    TauxOccupation = Math.Round(tauxOccupation, 2),
                    TauxFragmentation = Math.Round(tauxFragmentation, 2)
                });
            }

            var kpisGlobaux = CalculerKpisGlobaux(kpisParOuvrier);

            var rapport = new PlanningAnalysisReportDto
            {
                KpisGlobaux = kpisGlobaux,
                KpisParOuvrier = kpisParOuvrier
            };

            return Task.FromResult(rapport);
        }
        private int CalculerJoursOuvres(DateTime dateDebut, DateTime dateFin, CalendrierOuvreChantier calendrier)
        {
            if (dateDebut > dateFin) return 0;

            int compteur = 0;
            var dateActuelle = dateDebut.Date;

            while (dateActuelle <= dateFin.Date)
            {
                if (calendrier.EstJourOuvre(NodaTime.LocalDate.FromDateTime(dateActuelle)))
                {
                    compteur++;
                }
                dateActuelle = dateActuelle.AddDays(1);
            }
            return compteur;
        }



        private GlobalKpiDto CalculerKpisGlobaux(IReadOnlyList<WorkerKpiDto> kpisParOuvrier)
        {
            if (!kpisParOuvrier.Any()) return new GlobalKpiDto();

            var totalHeuresTravaillees = kpisParOuvrier.Sum(k => k.HeuresTravaillees);
            if (totalHeuresTravaillees == 0) return new GlobalKpiDto();

            var tauxOccupationMoyen = kpisParOuvrier.Sum(k => k.TauxOccupation * k.HeuresTravaillees) / totalHeuresTravaillees;

            return new GlobalKpiDto
            {
                TauxOccupationMoyenPondere = Math.Round(tauxOccupationMoyen, 2)
            };
        }
    }
}