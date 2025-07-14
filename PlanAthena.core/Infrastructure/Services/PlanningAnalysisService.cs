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
            if (!affectations.Any())
            {
                // Retourner un rapport vide si pas d'affectations
                return Task.FromResult(new PlanningAnalysisReportDto
                {
                    KpisGlobaux = new GlobalKpiDto(),
                    KpisParOuvrier = new List<WorkerKpiDto>()
                });
            }

            var kpisParOuvrier = new List<WorkerKpiDto>();
            var affectationsParOuvrier = affectations.GroupBy(a => a.OuvrierId);

            foreach (var groupeOuvrier in affectationsParOuvrier)
            {
                var ouvrierId = groupeOuvrier.Key;
                var ouvrier = chantierDeReference.Ouvriers.Values.First(o => o.Id.Value == ouvrierId);
                var sesTaches = groupeOuvrier.ToList();

                var (joursPresence, heuresTravaillees) = CalculerPresenceEtHeures(sesTaches, chantierDeReference.Calendrier);
                var tauxOccupation = CalculerTauxOccupation(heuresTravaillees, joursPresence, chantierDeReference.Calendrier.DureeTravailEffectiveParJour.TotalHours);
                var tauxFragmentation = CalculerTauxFragmentation(tauxOccupation);

                kpisParOuvrier.Add(new WorkerKpiDto
                {
                    OuvrierId = ouvrierId,
                    OuvrierNom = $"{ouvrier.Prenom} {ouvrier.Nom}",
                    JoursDePresence = joursPresence,
                    HeuresTravaillees = heuresTravaillees,
                    TauxOccupation = tauxOccupation,
                    TauxFragmentation = tauxFragmentation
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

        private (int joursPresence, double heuresTravaillees) CalculerPresenceEtHeures(
    IReadOnlyList<AffectationDto> affectationsOuvrier,
    CalendrierOuvreChantier calendrier)
        {
            if (!affectationsOuvrier.Any()) return (0, 0);

            // Le calcul des heures travaillées est maintenant direct
            double heuresTravaillees = affectationsOuvrier.Sum(a => (double)a.DureeHeures);

            // Pour les jours de présence, nous devons calculer la date de fin de chaque tâche
            var dateMin = affectationsOuvrier.Min(a => a.DateDebut);
            var dateMax = affectationsOuvrier.Max(a => a.DateDebut.AddHours(a.DureeHeures));

            int joursDePresence = 0;
            // On s'assure de compter le nombre de jours ouvrés uniques où il y a eu une activité
            var joursActifs = new HashSet<DateTime>();
            foreach (var affectation in affectationsOuvrier)
            {
                var dateCourante = affectation.DateDebut.Date;
                var dateFinTache = affectation.DateDebut.AddHours(affectation.DureeHeures).Date;
                while (dateCourante <= dateFinTache)
                {
                    joursActifs.Add(dateCourante);
                    dateCourante = dateCourante.AddDays(1);
                }
            }

            joursDePresence = joursActifs.Count(jour => calendrier.EstJourOuvre(NodaTime.LocalDate.FromDateTime(jour)));

            return (joursDePresence, heuresTravaillees);
        }

        private double CalculerTauxOccupation(double heuresTravaillees, int joursPresence, double heuresStandardParJour)
        {
            if (joursPresence == 0) return 0.0;
            var tempsDisponible = joursPresence * heuresStandardParJour;
            if (tempsDisponible == 0) return 0.0;

            return Math.Round((heuresTravaillees / tempsDisponible) * 100, 2);
        }

        private double CalculerTauxFragmentation(double tauxOccupation)
        {
            return 100.0 - tauxOccupation;
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