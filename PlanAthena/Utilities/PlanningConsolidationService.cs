// Emplacement: PlanAthena/Utilities/PlanningConsolidationService.cs
using NodaTime;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Services.Business.DTOs;
using CoreEnums = PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Service utilitaire stateless chargé de transformer le résultat brut du solveur
    /// en un planning consolidé propre, avec des affectations découpées en segments journaliers horaires.
    /// </summary>
    public class PlanningConsolidationService
    {
        private const string JALON_TECHNIQUE_PREFIX = "JT_";
        private const string DECOUPAGE_SUFFIX_MARKER = "_P";
        private const double DURATION_TOLERANCE = 0.001;

        /// <summary>
        /// Traite le résultat brut du solveur pour le transformer en planning consolidé.
        /// </summary>
        /// <param name="rawResult">Le DTO de résultat brut provenant de la DLL du solveur.</param>
        /// <param name="config">La configuration de planification utilisée pour le calcul.</param>
        /// <returns>Un objet ConsolidatedPlanning contenant les segments de travail journaliers.</returns>
        public virtual ConsolidatedPlanning Process(ProcessChantierResultDto rawResult, ConfigurationPlanification config)
        {
            var planning = new ConsolidatedPlanning();
            if (rawResult?.OptimisationResultat?.Affectations == null || !rawResult.OptimisationResultat.Affectations.Any())
            {
                return planning;
            }

            var allSegments = new List<SegmentDeTravail>();
            var allJalons = new List<JalonPlanifie>();
            var allDates = new List<DateTime>();

            foreach (var affectation in rawResult.OptimisationResultat.Affectations)
            {
                if (IsTechnicalConvergenceMilestone(affectation.TacheId))
                {
                    continue; // On ignore les jalons techniques
                }

                // --- AIGUILLAGE CORRIGÉ ---
                if (affectation.TypeActivite == CoreEnums.TypeActivite.JalonUtilisateur)
                {
                    // C'est un jalon d'attente. On NE le découpe PAS.
                    // On le stocke comme un bloc de temps unique.
                    allJalons.Add(new JalonPlanifie
                    {
                        TacheId = affectation.TacheId,
                        TacheNom = affectation.TacheNom,
                        BlocId = affectation.BlocId,
                        DateDebut = affectation.DateDebut,
                        DateFin = affectation.DateFin,
                        DureeHeures = affectation.DureeHeures
                    });
                    allDates.Add(affectation.DateDebut);
                    allDates.Add(affectation.DateFin);
                }
                else // C'est une tâche de travail normale
                {
                    // On la découpe en segments de travail journaliers.
                    var segmentsForAffectation = DecouperEnTempsOuvre(affectation, config);
                    if (segmentsForAffectation.Any())
                    {
                        allSegments.AddRange(segmentsForAffectation);
                        allDates.Add(segmentsForAffectation.First().Jour);
                        allDates.Add(segmentsForAffectation.Last().Jour);
                    }
                }
            }

            planning.JalonsPlanifies = allJalons;
            return AggregateSegmentsIntoPlanning(planning, allSegments, allDates);
        }

        /// <summary>
        /// Découpe une affectation en segments journaliers en respectant le temps de travail effectif
        /// et en calculant les horaires précis pour chaque segment.
        /// </summary>
        private List<SegmentDeTravail> DecouperEnTempsOuvre(AffectationDto affectation, ConfigurationPlanification config)
        {
            var segments = new List<SegmentDeTravail>();
            var parentId = ObtenirParentIdDepuisId(affectation.TacheId);
            var joursOuvresSet = new HashSet<DayOfWeek>(config.JoursOuvres);

            // Utilisation de NodaTime pour une gestion robuste des fuseaux horaires et du temps
            var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var localDebut = LocalDateTime.FromDateTime(affectation.DateDebut);
            var instantCourant = localDebut.InZoneLeniently(timeZone).ToInstant();

            double heuresRestantes = affectation.DureeHeures;

            while (heuresRestantes > DURATION_TOLERANCE)
            {
                var zonedDateTime = instantCourant.InZone(timeZone);
                var dateCourante = zonedDateTime.Date;

                // 1. Si jour non-ouvré, sauter au début du prochain jour ouvré
                if (!joursOuvresSet.Contains(ToSystemDayOfWeek(dateCourante.DayOfWeek)))
                {
                    instantCourant = GetDebutProchainJourOuvre(dateCourante, joursOuvresSet, config.HeureDebutJournee, timeZone);
                    continue;
                }

                // 2. Définir les bornes de la journée de travail
                var debutJourneeTravail = dateCourante.At(new LocalTime(config.HeureDebutJournee, 0)).InZoneLeniently(timeZone).ToInstant();
                var finJourneeTravail = debutJourneeTravail.Plus(Duration.FromHours(config.HeuresTravailEffectifParJour));

                // 3. Ajuster l'instant courant s'il est avant le début de la journée
                if (instantCourant < debutJourneeTravail)
                {
                    instantCourant = debutJourneeTravail;
                }

                // 4. Si on a déjà dépassé la fin de journée, sauter au lendemain
                var dureeRestanteJournee = finJourneeTravail - instantCourant;
                if (dureeRestanteJournee <= Duration.Zero)
                {
                    instantCourant = GetDebutProchainJourOuvre(dateCourante, joursOuvresSet, config.HeureDebutJournee, timeZone);
                    continue;
                }

                // 5. Calculer les heures à affecter pour ce segment
                var heuresDisponiblesCeJour = dureeRestanteJournee.TotalHours;
                double heuresPourCeSegment = Math.Min(heuresRestantes, heuresDisponiblesCeJour);

                // 6. Créer le segment avec les nouvelles informations horaires
                var instantFinSegment = instantCourant.Plus(Duration.FromHours(heuresPourCeSegment));
                segments.Add(new SegmentDeTravail
                {
                    OuvrierId = affectation.OuvrierId,
                    TacheId = affectation.TacheId,
                    ParentTacheId = parentId,
                    TacheNom = affectation.TacheNom,
                    BlocId = affectation.BlocId,
                    Jour = dateCourante.ToDateTimeUnspecified(),
                    HeuresTravaillees = heuresPourCeSegment,
                    // CORRECTION: NodaTime.LocalTime n'a pas de méthode ToTimeSpan().
                    // La conversion correcte se fait via la propriété TickOfDay qui représente
                    // le nombre de ticks depuis minuit, ce qui est équivalent à un TimeSpan.
                    HeureDebut = new TimeSpan(instantCourant.InZone(timeZone).TimeOfDay.TickOfDay),
                    HeureFin = new TimeSpan(instantFinSegment.InZone(timeZone).TimeOfDay.TickOfDay)
                });

                // 7. Mettre à jour les compteurs pour la prochaine itération
                heuresRestantes -= heuresPourCeSegment;
                instantCourant = instantFinSegment;
            }

            return segments;
        }



        private bool IsTechnicalConvergenceMilestone(string tacheId)
        {
            return tacheId.StartsWith(JALON_TECHNIQUE_PREFIX, StringComparison.OrdinalIgnoreCase);
        }

        private string ObtenirParentIdDepuisId(string tacheId)
        {
            if (tacheId.Contains(DECOUPAGE_SUFFIX_MARKER))
            {
                var index = tacheId.LastIndexOf(DECOUPAGE_SUFFIX_MARKER, StringComparison.Ordinal);
                if (index > 0)
                {
                    return tacheId.Substring(0, index);
                }
            }
            return null;
        }

        private ConsolidatedPlanning AggregateSegmentsIntoPlanning(ConsolidatedPlanning planning, List<SegmentDeTravail> allSegments, List<DateTime> allDates)
        {
            if (!allDates.Any()) return planning;

            planning.DateDebutProjet = allDates.Min();
            planning.DateFinProjet = allDates.Max();

            if (allSegments.Any())
            {
                planning.SegmentsParOuvrierId = allSegments
                    .GroupBy(s => s.OuvrierId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Jour).ToList());
            }

            return planning;
        }

        private Instant GetDebutProchainJourOuvre(LocalDate dateActuelle, HashSet<DayOfWeek> joursOuvres, int heureDebut, DateTimeZone timeZone)
        {
            var prochainJour = dateActuelle.PlusDays(1);
            while (!joursOuvres.Contains(ToSystemDayOfWeek(prochainJour.DayOfWeek)))
            {
                prochainJour = prochainJour.PlusDays(1);
            }
            return prochainJour.At(new LocalTime(heureDebut, 0)).InZoneLeniently(timeZone).ToInstant();
        }

        /// <summary>
        /// Convertit NodaTime.IsoDayOfWeek en System.DayOfWeek.
        /// </summary>
        private DayOfWeek ToSystemDayOfWeek(IsoDayOfWeek isoDayOfWeek)
        {
            if (isoDayOfWeek == IsoDayOfWeek.None)
                throw new ArgumentException("IsoDayOfWeek.None n'est pas convertible.");

            return (DayOfWeek)(((int)isoDayOfWeek) % 7);
        }
    }
}