// Fichier : TacheModelBuilder.cs (Version Finale, Compilable et Correcte)

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class TacheModelBuilder
    {
        public (
            Dictionary<TacheId, IntervalVar> TachesIntervals,
            Dictionary<(TacheId, OuvrierId), BoolVar> TachesAssignables,
            IntVar Makespan
        ) Construire(CpModel model, ProblemeOptimisation probleme)
        {
            var chantier = probleme.Chantier;
            var tachesAssignables = new Dictionary<(TacheId, OuvrierId), BoolVar>();
            var intervallesParOuvrier = chantier.Ouvriers.ToDictionary(o => o.Key, o => new List<IntervalVar>());
            var tachesIntervals = new Dictionary<TacheId, IntervalVar>();

            CreerVariablesDeDecision(model, probleme, tachesAssignables, intervallesParOuvrier, tachesIntervals);

            AjouterContraintesAssignationUnique(model, probleme.Chantier, tachesAssignables);
            AjouterContraintesRessources(model, probleme.Chantier, intervallesParOuvrier);
            AjouterContraintesDependances(model, probleme.Chantier, tachesIntervals);

            var makespan = model.NewIntVar(0, probleme.EchelleTemps.NombreTotalSlots, "makespan");
            if (tachesIntervals.Any())
            {
                model.AddMaxEquality(makespan, tachesIntervals.Values.Select(v => v.EndExpr()));
            }
            else
            {
                model.Add(makespan == 0);
            }

            return (tachesIntervals, tachesAssignables, makespan);
        }

        private void CreerVariablesDeDecision(
            CpModel model,
            ProblemeOptimisation probleme,
            Dictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables,
            Dictionary<OuvrierId, List<IntervalVar>> intervallesParOuvrier,
            Dictionary<TacheId, IntervalVar> tachesIntervals)
        {
            var chantier = probleme.Chantier;
            long horizon = probleme.EchelleTemps.NombreTotalSlots;
            int heuresParJour = (int)chantier.Calendrier.DureeTravailEffectiveParJour.TotalHours;

            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                var duree = (long)tache.HeuresHommeEstimees.Value;
                var startVar = model.NewIntVar(0, horizon - duree, $"start_{tache.Id.Value}");
                var endVar = model.NewIntVar(0, horizon, $"end_{tache.Id.Value}");
                var sizeVar = model.NewConstant(duree);

                var intervalleVirtuel = model.NewIntervalVar(startVar, sizeVar, endVar, $"virtuel_{tache.Id.Value}");
                tachesIntervals.Add(tache.Id, intervalleVirtuel);

                // === LA SOLUTION FINALE ET COMPILABLE : AddModuloEquality ===
                // On empêche une tâche de traverser la frontière d'un jour.
                if (duree > 0 && duree <= heuresParJour)
                {
                    // 1. On crée une variable qui représentera la position de départ DANS la journée (de 0 à heuresParJour-1)
                    var startDansJour = model.NewIntVar(0, heuresParJour - 1, $"start_dans_jour_{tache.Id.Value}");

                    // 2. On lie cette variable à la variable de départ globale via le modulo.
                    //    startDansJour = startVar % heuresParJour
                    model.AddModuloEquality(startDansJour, startVar, heuresParJour);

                    // 3. On contraint la position de départ dans le jour + la durée à ne pas dépasser la fin du jour.
                    model.Add(startDansJour + duree <= heuresParJour);
                }
                // ===================================================================

                var ouvriersCompetents = chantier.Ouvriers.Values
                    .Where(o => o.PossedeCompetence(tache.MetierRequisId));

                foreach (var ouvrier in ouvriersCompetents)
                {
                    var estAssignable = model.NewBoolVar($"estAssignable_{tache.Id.Value}_a_{ouvrier.Id.Value}");
                    tachesAssignables.Add((tache.Id, ouvrier.Id), estAssignable);

                    var intervalleOptionnel = model.NewOptionalIntervalVar(
                        startVar,
                        sizeVar,
                        endVar,
                        estAssignable,
                        $"optionnel_{tache.Id.Value}_a_{ouvrier.Id.Value}"
                    );
                    intervallesParOuvrier[ouvrier.Id].Add(intervalleOptionnel);
                }
            }
        }

        // --- Le reste du fichier est inchangé ---
        private void AjouterContraintesAssignationUnique(CpModel model, Chantier chantier, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                var candidatsPourTache = tachesAssignables.Where(kvp => kvp.Key.Item1 == tache.Id).Select(kvp => kvp.Value).ToList();
                if (candidatsPourTache.Any()) { model.Add(LinearExpr.Sum(candidatsPourTache) == 1); }
                else { throw new InvalidOperationException($"Aucun ouvrier compétent trouvé pour la tâche {tache.Id.Value}"); }
            }
        }
        private void AjouterContraintesRessources(CpModel model, Chantier chantier, IReadOnlyDictionary<OuvrierId, List<IntervalVar>> intervallesParOuvrier)
        {
            foreach (var ouvrierId in chantier.Ouvriers.Keys)
            {
                var intervalles = intervallesParOuvrier[ouvrierId];
                if (intervalles.Count > 1) { model.AddNoOverlap(intervalles); }
            }
        }
        private void AjouterContraintesDependances(CpModel model, Chantier chantier, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals)
        {
            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                if (tache.Dependencies == null || !tache.Dependencies.Any()) continue;
                var intervalleTacheActuelle = tachesIntervals[tache.Id];
                foreach (var dependanceId in tache.Dependencies)
                {
                    if (tachesIntervals.TryGetValue(dependanceId, out var intervalleDependance)) { model.Add(intervalleTacheActuelle.StartExpr() >= intervalleDependance.EndExpr()); }
                }
            }
        }
    }
}