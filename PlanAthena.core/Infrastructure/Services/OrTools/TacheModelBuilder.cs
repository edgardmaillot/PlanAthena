// Fichier : TacheModelBuilder.cs (Version Finale Complète)

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.Shared; // Pour DependencyGraph
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
            var tachesIntervals = new Dictionary<TacheId, IntervalVar>();

            // Note: La création des intervalles par ouvrier est maintenant gérée dans AjouterContraintesRessources
            // pour plus de clarté, car c'est son unique usage.
            CreerVariablesDeDecision(model, probleme, tachesAssignables, tachesIntervals);

            AjouterContraintesAssignationUnique(model, chantier, tachesAssignables);
            AjouterContraintesRessources(model, chantier, tachesIntervals, tachesAssignables);
            AjouterContraintesDePrecedence(model, chantier, tachesIntervals);

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
            Dictionary<TacheId, IntervalVar> tachesIntervals)
        {
            var chantier = probleme.Chantier;
            long horizon = probleme.EchelleTemps.NombreTotalSlots;
            int heuresParJour = (int)chantier.Calendrier.DureeTravailEffectiveParJour.TotalHours;

            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                var duree = (long)tache.HeuresHommeEstimees.Value;
                var startVar = model.NewIntVar(0, horizon > duree ? horizon - duree : 0, $"start_{tache.Id.Value}");
                var endVar = model.NewIntVar(0, horizon, $"end_{tache.Id.Value}");
                var sizeVar = model.NewConstant(duree);

                var intervalleVirtuel = model.NewIntervalVar(startVar, sizeVar, endVar, $"interval_{tache.Id.Value}");
                tachesIntervals.Add(tache.Id, intervalleVirtuel);

                if (duree > 0 && duree <= heuresParJour)
                {
                    var startDansJour = model.NewIntVar(0, heuresParJour - 1, $"start_dans_jour_{tache.Id.Value}");
                    model.AddModuloEquality(startDansJour, startVar, heuresParJour);
                    model.Add(startDansJour + duree <= heuresParJour);
                }

                var ouvriersCompetents = chantier.Ouvriers.Values
                    .Where(o => o.PossedeCompetence(tache.MetierRequisId));

                foreach (var ouvrier in ouvriersCompetents)
                {
                    var estAssignable = model.NewBoolVar($"estAssignable_{tache.Id.Value}_a_{ouvrier.Id.Value}");
                    tachesAssignables.Add((tache.Id, ouvrier.Id), estAssignable);
                }
            }
        }

        private void AjouterContraintesAssignationUnique(CpModel model, Chantier chantier, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                var candidatsPourTache = tachesAssignables.Where(kvp => kvp.Key.Item1 == tache.Id).Select(kvp => kvp.Value).ToList();
                if (candidatsPourTache.Any())
                {
                    model.AddExactlyOne(candidatsPourTache);
                }
                else
                {
                    throw new InvalidOperationException($"Aucun ouvrier compétent trouvé pour la tâche {tache.Id.Value} ({tache.Nom}).");
                }
            }
        }

        private void AjouterContraintesRessources(CpModel model, Chantier chantier, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            foreach (var ouvrier in chantier.Ouvriers.Values)
            {
                var intervallesPourOuvrier = new List<IntervalVar>();
                foreach (var tache in chantier.ObtenirToutesLesTaches())
                {
                    if (tachesAssignables.TryGetValue((tache.Id, ouvrier.Id), out var estAssignable))
                    {
                        var intervalleBase = tachesIntervals[tache.Id];
                        var intervalleOptionnel = model.NewOptionalIntervalVar(
                            intervalleBase.StartExpr(),
                            intervalleBase.SizeExpr(),
                            intervalleBase.EndExpr(),
                            estAssignable,
                            $"optionnel_{tache.Id.Value}_a_{ouvrier.Id.Value}"
                        );
                        intervallesPourOuvrier.Add(intervalleOptionnel);
                    }
                }

                if (intervallesPourOuvrier.Count > 1)
                {
                    model.AddNoOverlap(intervallesPourOuvrier);
                }
            }
        }

        private void AjouterContraintesDePrecedence(CpModel model, Chantier chantier, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals)
        {
            // --- ÉTAPE 1 : Gérer les dépendances explicites (Tâche -> Tâche) ---
            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                if (tache.Dependencies == null || !tache.Dependencies.Any()) continue;

                var intervalleTacheActuelle = tachesIntervals[tache.Id];
                foreach (var dependanceId in tache.Dependencies)
                {
                    if (tachesIntervals.TryGetValue(dependanceId, out var intervalleDependance))
                    {
                        model.Add(intervalleTacheActuelle.StartExpr() >= intervalleDependance.EndExpr());
                    }
                }
            }

            // --- ÉTAPE 2 : Gérer les dépendances implicites (Métier -> Métier) ---

            // 2a. Calculer manuellement la fermeture transitive des dépendances métier
            var allPrerequisites = new Dictionary<MetierId, HashSet<MetierId>>();
            foreach (var metier in chantier.Metiers.Values)
            {
                allPrerequisites[metier.Id] = new HashSet<MetierId>();
                var toProcess = new Queue<MetierId>(metier.PrerequisMetierIds);
                var processed = new HashSet<MetierId>();

                while (toProcess.Any())
                {
                    var currentId = toProcess.Dequeue();
                    if (processed.Contains(currentId)) continue;

                    allPrerequisites[metier.Id].Add(currentId);
                    processed.Add(currentId);

                    if (chantier.Metiers.TryGetValue(currentId, out var prerequisMetier))
                    {
                        foreach (var nextId in prerequisMetier.PrerequisMetierIds)
                        {
                            if (!processed.Contains(nextId))
                            {
                                toProcess.Enqueue(nextId);
                            }
                        }
                    }
                }
            }

            // 2b. Appliquer les contraintes par bloc
            foreach (var bloc in chantier.Blocs.Values)
            {
                var tachesParMetier = bloc.Taches.Values
                    .GroupBy(t => t.MetierRequisId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var (metierId, tachesDuMetier) in tachesParMetier)
                {
                    var prerequisPourCeMetier = allPrerequisites[metierId];
                    if (!prerequisPourCeMetier.Any()) continue;

                    var tachesPrerequisesDansLeBloc = new List<Tache>();
                    foreach (var prerequisId in prerequisPourCeMetier)
                    {
                        if (tachesParMetier.TryGetValue(prerequisId, out var taches))
                        {
                            tachesPrerequisesDansLeBloc.AddRange(taches);
                        }
                    }

                    if (!tachesPrerequisesDansLeBloc.Any()) continue;

                    var finMaxPrerequis = model.NewIntVar(0, int.MaxValue, $"fin_prerequis_{metierId.Value}_bloc_{bloc.Id.Value}");
                    model.AddMaxEquality(finMaxPrerequis, tachesPrerequisesDansLeBloc.Select(t => tachesIntervals[t.Id].EndExpr()));

                    foreach (var tache in tachesDuMetier)
                    {
                        model.Add(tachesIntervals[tache.Id].StartExpr() >= finMaxPrerequis);
                    }
                }
            }
        }
    }
}