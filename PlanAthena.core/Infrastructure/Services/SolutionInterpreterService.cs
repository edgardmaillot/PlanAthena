// Fichier : Infrastructure/Services/SolutionInterpreterService.cs

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Facade.Dto.Enums; 
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Core.Domain.ValueObjects;
using NodaTime.Extensions; // Pour la conversion DateTime <-> LocalDateTime


namespace PlanAthena.Core.Infrastructure.Services
{
    public class SolutionInterpreterService : ISolutionInterpreterService
    {
        public (IReadOnlyList<AffectationDto> Affectations, IReadOnlyList<FeuilleDeTempsOuvrierDto> FeuillesDeTemps) InterpreterLaSolution(
            CpSolver solver,
            ModeleCpSat modeleCpSat,
            ProblemeOptimisation probleme)
        {
            var affectations = new List<AffectationDto>();
            var chantier = probleme.Chantier;
            var echelleTemps = probleme.EchelleTemps;

            // NOUVEAU: Structure temporaire pour construire les masques de bits
            var planningParOuvrier = new Dictionary<OuvrierId, Dictionary<DateTime, long>>();

            foreach (var (key, assignVar) in modeleCpSat.TachesAssignables)
            {
                if (solver.BooleanValue(assignVar))
                {
                    var (tacheId, ouvrierId) = key;
                    var tache = chantier.ObtenirToutesLesTaches().First(t => t.Id == tacheId);
                    var ouvrier = chantier.Ouvriers[ouvrierId];
                    var intervalVar = modeleCpSat.TachesIntervals[tacheId];
                    var startSlotIndex = solver.Value(intervalVar.StartExpr());
                    var sizeInSlots = solver.Value(intervalVar.SizeExpr());

                    // 1. Logique pour AffectationDto (quasi-inchangée)
                    var dureeReelle = CalculerDureeReelle(tacheId, intervalVar, modeleCpSat, solver);
                    var dateDebut = echelleTemps.Slots[(int)startSlotIndex].Debut.InUtc().ToDateTimeUtc(); // Convention UTC

                    affectations.Add(new AffectationDto
                    {
                        TacheId = tacheId.Value,
                        TacheNom = tache.Nom,
                        OuvrierId = ouvrierId.Value,
                        OuvrierNom = $"{ouvrier.Prenom} {ouvrier.Nom}",
                        BlocId = tache.BlocParentId.Value,
                        DateDebut = dateDebut,
                        DureeHeures = dureeReelle,
                        TypeActivite = ObtenirTypeActivite(tacheId, modeleCpSat),
                        DureeOriginaleHeures = ObtenirDureeOriginale(tacheId, modeleCpSat)
                    });

                    // 2. NOUVEAU: Logique pour construire les FeuillesDeTemps (masques de bits)
                    // On ne construit les feuilles de temps que pour les tâches réelles (pas les jalons/ouvriers virtuels)
                    if (tache.Type == TypeActivite.Tache)
                    {
                        if (!planningParOuvrier.ContainsKey(ouvrierId))
                        {
                            planningParOuvrier[ouvrierId] = new Dictionary<DateTime, long>();
                        }

                        var heureDebutJournee = chantier.Calendrier.HeureDebutTravail.Hour;

                        for (int i = 0; i < sizeInSlots; i++)
                        {
                            var currentSlotIndex = startSlotIndex + i;
                            var slot = echelleTemps.Slots[(int)currentSlotIndex];

                            // Jour UTC normalisé à minuit
                            var jourNoda = slot.Debut.Date;
                            var jourUtc = new DateTime(jourNoda.Year, jourNoda.Month, jourNoda.Day, 0, 0, 0, DateTimeKind.Utc);

                            // Index du bit (0 pour la première heure de travail)
                            var heureIndex = slot.Debut.Hour - heureDebutJournee;

                            if (!planningParOuvrier[ouvrierId].ContainsKey(jourUtc))
                            {
                                planningParOuvrier[ouvrierId][jourUtc] = 0L;
                            }

                            // Application du masque de bits
                            planningParOuvrier[ouvrierId][jourUtc] |= (1L << heureIndex);
                        }
                    }
                }
            }

            // 3. NOUVEAU: Transformation de la structure temporaire en DTO de sortie
            var feuillesDeTemps = new List<FeuilleDeTempsOuvrierDto>();
            foreach (var (ouvrierId, planningJournalier) in planningParOuvrier)
            {
                var ouvrier = chantier.Ouvriers[ouvrierId];
                feuillesDeTemps.Add(new FeuilleDeTempsOuvrierDto
                {
                    OuvrierId = ouvrierId.Value,
                    OuvrierNom = $"{ouvrier.Prenom} {ouvrier.Nom}",
                    PlanningJournalier = planningJournalier
                });
            }
            return (affectations, feuillesDeTemps);
        }


        // *** NOUVELLE MÉTHODE: Calcul intelligent de la durée selon le type d'activité ***
        /// <summary>
        /// Détermine la durée réelle d'une activité en fonction de son type.
        /// - Tâches normales: utilise la durée calculée par OR-Tools (peut varier selon les contraintes)
        /// - Jalons: utilise la durée originale définie par l'utilisateur (ignorant la représentation 1h d'OR-Tools)
        /// </summary>
        private double CalculerDureeReelle(TacheId tacheId, IntervalVar intervalVar, ModeleCpSat modeleCpSat, CpSolver solver)
        {
            var typeActivite = ObtenirTypeActivite(tacheId, modeleCpSat);

            if (typeActivite == TypeActivite.Tache)
            {
                // Pour les tâches de travail réel: la durée peut être ajustée par le solveur
                // (par exemple, dans le cas de tâches découpées en sous-parties)
                return solver.Value(intervalVar.SizeExpr());
            }
            else
            {
                // Pour les jalons (séchage, attente, etc.): utiliser la durée originale
                // car OR-Tools représente les jalons avec une IntervalVar de 1 slot par commodité,
                // mais la vraie durée d'attente (72h, 24h, etc.) est celle définie par l'utilisateur
                return ObtenirDureeOriginale(tacheId, modeleCpSat);
            }
        }

        // *** NOUVELLE MÉTHODE: Récupération sécurisée du type d'activité ***
        private TypeActivite ObtenirTypeActivite(TacheId tacheId, ModeleCpSat modeleCpSat)
        {
            if (modeleCpSat.TypesActivites?.TryGetValue(tacheId, out var type) == true)
            {
                return type;
            }

            // Fallback: si les métadonnées ne sont pas disponibles, 
            // considérer comme une tâche normale par défaut
            return TypeActivite.Tache;
        }

        // *** NOUVELLE MÉTHODE: Récupération sécurisée de la durée originale ***
        private double ObtenirDureeOriginale(TacheId tacheId, ModeleCpSat modeleCpSat)
        {
            if (modeleCpSat.DureesOriginalesHeures?.TryGetValue(tacheId, out var duree) == true)
            {
                return duree;
            }

            // Fallback: si les métadonnées ne sont pas disponibles, retourner 0
            // (cela ne devrait jamais arriver dans le flux normal)
            return 0.0;
        }

        // *** NOUVELLE MÉTHODE: Détection d'un jalon ***
        /// <summary>
        /// Détermine si une activité est un jalon (point de repère temporel sans consommation de travail)
        /// par opposition à une tâche normale (travail réel nécessitant des ressources)
        /// </summary>
        private bool EstJalon(TacheId tacheId, ModeleCpSat modeleCpSat)
        {
            var typeActivite = ObtenirTypeActivite(tacheId, modeleCpSat);
            return typeActivite != TypeActivite.Tache;
        }

        
    }
}