// Fichier : Infrastructure/Services/SolutionInterpreterService.cs

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Facade.Dto.Enums; 
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Core.Domain.ValueObjects;

namespace PlanAthena.Core.Infrastructure.Services
{
    public class SolutionInterpreterService : ISolutionInterpreterService
    {
        public IReadOnlyList<AffectationDto> InterpreterLaSolution(
            CpSolver solver,
            ModeleCpSat modeleCpSat,
            ProblemeOptimisation probleme)
        {
            var affectations = new List<AffectationDto>();
            var chantier = probleme.Chantier;
            var echelleTemps = probleme.EchelleTemps;

            // Parcourir toutes les décisions d'assignation possibles
            foreach (var (key, assignVar) in modeleCpSat.TachesAssignables)
            {
                // Si le solveur a décidé que cette assignation est VRAIE
                if (solver.BooleanValue(assignVar))
                {
                    var (tacheId, ouvrierId) = key;

                    // On doit retrouver l'intervalle "optionnel" spécifique à cette assignation.
                    // Cette information n'est pas directement dans modeleCpSat, il faut donc adapter
                    // légèrement la façon dont on récupère l'intervalle.
                    // Si la structure ne le permet pas, on se base sur l'intervalle principal et on assume que
                    // le "size" est celui défini dans le modèle.
                    var intervalVar = modeleCpSat.TachesIntervals[tacheId];

                    // Récupérer les informations du domaine
                    var tache = chantier.ObtenirToutesLesTaches().First(t => t.Id == tacheId);
                    var ouvrier = chantier.Ouvriers[ouvrierId];

                    // 1. Lire le slot de début décidé par le solveur
                    var startSlotIndex = solver.Value(intervalVar.StartExpr());

                    // 2. *** MODIFICATION CRITIQUE: Calcul intelligent de la durée ***
                    // Pour les tâches normales: utiliser la durée calculée par le solveur (SizeExpr)
                    // Pour les jalons: utiliser la durée originale (72h, 24h, etc.) car SizeExpr = 1 (cosmétique)
                    var dureeReelle = CalculerDureeReelle(tacheId, intervalVar, modeleCpSat, solver);

                    // 3. Traduire le slot de début en DateTime
                    var dateDebut = echelleTemps.Slots[(int)startSlotIndex].Debut.ToDateTimeUnspecified();

                    // 4. Ajouter l'affectation à la liste avec la durée corrigée
                    affectations.Add(new AffectationDto
                    {
                        TacheId = tacheId.Value,
                        TacheNom = tache.Nom,
                        OuvrierId = ouvrierId.Value,
                        OuvrierNom = $"{ouvrier.Prenom} {ouvrier.Nom}",
                        BlocId = tache.BlocParentId.Value,
                        DateDebut = dateDebut,
                        DureeHeures = dureeReelle, // *** CORRECTION: Durée intelligente au lieu de dureeEnSlots ***

                        // *** AJOUT: Métadonnées pour export Gantt et affichage ***
                        TypeActivite = ObtenirTypeActivite(tacheId, modeleCpSat),
                        //EstJalon = EstJalon(tacheId, modeleCpSat),
                        DureeOriginaleHeures = ObtenirDureeOriginale(tacheId, modeleCpSat)
                    });
                }
            }

            return affectations;
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