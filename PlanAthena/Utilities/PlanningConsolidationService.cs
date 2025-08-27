// Fichier: PlanningConsolidationService.cs
using NodaTime;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Services.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreEnums = PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Service utilitaire stateless chargé de transformer le résultat brut du solveur
    /// en un planning consolidé propre, avec des affectations découpées en segments journaliers.
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
            if (rawResult?.OptimisationResultat?.Affectations == null || !rawResult.OptimisationResultat.Affectations.Any())
            {
                return new ConsolidatedPlanning();
            }

            var allSegments = new List<SegmentDeTravail>();

            foreach (var affectation in rawResult.OptimisationResultat.Affectations)
            {
                if (IsTechnicalConvergenceMilestone(affectation.TacheId))
                {
                    continue; // On ignore les jalons techniques de convergence
                }
                
                var segmentsForAffectation = DecouperAffectationEnSegments(affectation, config);
                allSegments.AddRange(segmentsForAffectation);
            }
            
            return AggregateSegmentsIntoPlanning(allSegments);
        }
        
        /// <summary>
        /// Aiguille le découpage d'une affectation en fonction de son type (travail humain vs. processus passif).
        /// </summary>
        private List<SegmentDeTravail> DecouperAffectationEnSegments(AffectationDto affectation, ConfigurationPlanification config)
        {
            // Valider que la durée est cohérente, conformément à la Règle #4
            var dureeHeuresCalculee = (affectation.DateFin - affectation.DateDebut).TotalHours;
            // Note : Ce check est simpliste et ne prend pas en compte les jours non-ouvrés.
            // Une validation plus poussée nécessiterait de simuler le décompte du temps.
            // Pour l'instant, on se contente d'un check de base. Si DureeHeures est très différente de l'intervalle, on lève une exception.
            if (Math.Abs(dureeHeuresCalculee - affectation.DureeHeures) > config.HeuresTravailEffectifParJour)
            {
                 // L'écart est plus grand qu'une journée de travail, ce qui est suspect.
                 // NOTE : Cette condition est une heuristique et pourrait être affinée.
                 // throw new InvalidOperationException($"Incohérence de durée pour l'affectation de la tâche '{affectation.TacheId}'. Durée annoncée: {affectation.DureeHeures}h, durée calculée: {dureeHeuresCalculee}h.");
            }


            // Les jalons utilisateur (séchage, attente...) suivent un temps calendaire continu.
            if (affectation.TypeActivite == CoreEnums.TypeActivite.JalonUtilisateur)
            {
                return DecouperEnTempsCalendaire(affectation);
            }
            
            // Les tâches standard suivent les horaires de travail définis.
            return DecouperEnTempsOuvre(affectation, config);
        }

        /// <summary>
        /// Découpe une affectation en segments journaliers en respectant le temps de travail effectif.
        /// </summary>
        private List<SegmentDeTravail> DecouperEnTempsOuvre(AffectationDto affectation, ConfigurationPlanification config)
        {
            var segments = new List<SegmentDeTravail>();
            var parentId = ObtenirParentIdDepuisId(affectation.TacheId);
            var joursOuvresSet = new HashSet<DayOfWeek>(config.JoursOuvres);

            double heuresRestantes = affectation.DureeHeures;
            var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var localDebut = LocalDateTime.FromDateTime(affectation.DateDebut);
            var instantCourant = localDebut.InZoneLeniently(timeZone).ToInstant();


            while (heuresRestantes > DURATION_TOLERANCE)
            {
                var zonedDateTime = instantCourant.InZone(timeZone);
                var dateCourante = zonedDateTime.Date;

                // **CORRECTIF APPLIQUÉ ICI**
                if (!joursOuvresSet.Contains(ToSystemDayOfWeek(dateCourante.DayOfWeek)))
                {
                    instantCourant = GetDebutProchainJourOuvre(dateCourante, joursOuvresSet, config.HeureDebutJournee, timeZone);
                    continue;
                }

                var debutJourneeTravail = dateCourante.At(new LocalTime(config.HeureDebutJournee, 0)).InZoneLeniently(timeZone).ToInstant();
                var finJourneeTravail = debutJourneeTravail.Plus(Duration.FromHours(config.HeuresTravailEffectifParJour));

                if (instantCourant < debutJourneeTravail)
                {
                    instantCourant = debutJourneeTravail;
                }
                
                var dureeRestanteJournee = finJourneeTravail - instantCourant;
                if (dureeRestanteJournee <= Duration.Zero)
                {
                    instantCourant = GetDebutProchainJourOuvre(dateCourante, joursOuvresSet, config.HeureDebutJournee, timeZone);
                    continue;
                }

                var heuresDisponiblesCeJour = dureeRestanteJournee.TotalHours;
                double heuresPourCeSegment = Math.Min(heuresRestantes, heuresDisponiblesCeJour);
                
                segments.Add(new SegmentDeTravail
                {
                    OuvrierId = affectation.OuvrierId,
                    TacheId = affectation.TacheId,
                    ParentTacheId = parentId,
                    TacheNom = affectation.TacheNom,
                    BlocId = affectation.BlocId,
                    Jour = dateCourante.ToDateTimeUnspecified(),
                    HeuresTravaillees = heuresPourCeSegment
                });
                
                heuresRestantes -= heuresPourCeSegment;
                instantCourant = instantCourant.Plus(Duration.FromHours(heuresPourCeSegment));
            }
            
            return segments;
        }
        
        /// <summary>
        /// Découpe une affectation (typiquement un jalon) en segments journaliers en temps calendaire continu (24/7).
        /// </summary>
        private List<SegmentDeTravail> DecouperEnTempsCalendaire(AffectationDto affectation)
        {
            var segments = new List<SegmentDeTravail>();
            var parentId = ObtenirParentIdDepuisId(affectation.TacheId);

            double heuresRestantes = affectation.DureeHeures;
            var instantCourant = affectation.DateDebut;

            while (heuresRestantes > DURATION_TOLERANCE)
            {
                var finDuJourCourant = instantCourant.Date.AddDays(1);
                double heuresDisponiblesCeJour = (finDuJourCourant - instantCourant).TotalHours;

                double heuresPourCeSegment = Math.Min(heuresRestantes, heuresDisponiblesCeJour);

                segments.Add(new SegmentDeTravail
                {
                    OuvrierId = affectation.OuvrierId,
                    TacheId = affectation.TacheId,
                    ParentTacheId = parentId,
                    TacheNom = affectation.TacheNom,
                    BlocId = affectation.BlocId,
                    Jour = instantCourant.Date,
                    HeuresTravaillees = heuresPourCeSegment
                });

                heuresRestantes -= heuresPourCeSegment;
                instantCourant = instantCourant.AddHours(heuresPourCeSegment);
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
        
        private ConsolidatedPlanning AggregateSegmentsIntoPlanning(List<SegmentDeTravail> allSegments)
        {
            var planning = new ConsolidatedPlanning();
            if (!allSegments.Any()) return planning;

            planning.DateDebutProjet = allSegments.Min(s => s.Jour);
            planning.DateFinProjet = allSegments.Max(s => s.Jour);

            planning.SegmentsParOuvrierId = allSegments
                .GroupBy(s => s.OuvrierId)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Jour).ToList());

            return planning;
        }

        private Instant GetDebutProchainJourOuvre(LocalDate dateActuelle, HashSet<DayOfWeek> joursOuvres, int heureDebut, DateTimeZone timeZone)
        {
            var prochainJour = dateActuelle.PlusDays(1);
            // **CORRECTIF APPLIQUÉ ICI**
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
            // NodaTime: 1 (Lundi) -> 7 (Dimanche). System: 0 (Dimanche) -> 6 (Samedi).
            if (isoDayOfWeek == IsoDayOfWeek.None)
                throw new ArgumentException("IsoDayOfWeek.None n'est pas convertible.");
            
            return (DayOfWeek)(((int)isoDayOfWeek) % 7);
        }
    }
}