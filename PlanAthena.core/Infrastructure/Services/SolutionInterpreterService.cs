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
                    var intervalVar = modeleCpSat.TachesIntervals[tacheId];

                    // Récupérer les informations du domaine pour enrichir le DTO
                    var tache = chantier.ObtenirToutesLesTaches().First(t => t.Id == tacheId);
                    var ouvrier = chantier.Ouvriers[ouvrierId];

                    // Traduire les slots en DateTime
                    var startSlotIndex = solver.Value(intervalVar.StartExpr());
                    var endSlotIndex = solver.Value(intervalVar.EndExpr());

                    var dateDebut = echelleTemps.Slots[(int)startSlotIndex].Debut.ToDateTimeUnspecified();
                    // La fin est la fin du dernier slot. Le dernier slot est endSlotIndex - 1.
                    var dateFin = echelleTemps.Slots[(int)endSlotIndex - 1].Fin.ToDateTimeUnspecified();

                    affectations.Add(new AffectationDto
                    {
                        TacheId = tacheId.Value,
                        TacheNom = tache.Nom,
                        OuvrierId = ouvrierId.Value,
                        OuvrierNom = $"{ouvrier.Prenom} {ouvrier.Nom}",
                        BlocId = tache.BlocParentId.Value,
                        DateDebut = dateDebut,
                        DateFin = dateFin
                    });
                }
            }

            return affectations;
        }
    }
}