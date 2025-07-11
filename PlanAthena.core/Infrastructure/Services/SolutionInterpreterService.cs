// Fichier : Infrastructure/Services/SolutionInterpreterService.cs

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Facade.Dto.Output;
using System.Collections.Generic;
using System.Linq;

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

                    // 2. Lire la durée en slots (taille) décidée par le solveur
                    var dureeEnSlots = solver.Value(intervalVar.SizeExpr());

                    // 3. Traduire le slot de début en DateTime
                    var dateDebut = echelleTemps.Slots[(int)startSlotIndex].Debut.ToDateTimeUnspecified();

                    // 4. Ajouter l'affectation à la liste avec le nouveau format
                    affectations.Add(new AffectationDto
                    {
                        TacheId = tacheId.Value,
                        TacheNom = tache.Nom,
                        OuvrierId = ouvrierId.Value,
                        OuvrierNom = $"{ouvrier.Prenom} {ouvrier.Nom}",
                        BlocId = tache.BlocParentId.Value,
                        DateDebut = dateDebut,
                        DureeHeures = dureeEnSlots // La durée en slots est directement notre durée en heures
                    });
                }
            }

            return affectations;
        }
    }
}