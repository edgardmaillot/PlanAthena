// Fichier : TacheModelBuilder.cs (Version Optimisée)

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.Shared;
using PlanAthena.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class TacheModelBuilder
    {
        // Cache pour éviter les recalculs
        private Dictionary<MetierId, HashSet<MetierId>> _prerequisitesCache;
        private Dictionary<BlocId, Dictionary<MetierId, List<Tache>>> _tachesParMetierParBlocCache;

        public (
            Dictionary<TacheId, IntervalVar> TachesIntervals,
            Dictionary<(TacheId, OuvrierId), BoolVar> TachesAssignables,
            IntVar Makespan
        ) Construire(CpModel model, ProblemeOptimisation probleme)
        {
            var chantier = probleme.Chantier;
            var tachesAssignables = new Dictionary<(TacheId, OuvrierId), BoolVar>();
            var tachesIntervals = new Dictionary<TacheId, IntervalVar>();

            // Précalcul des caches
            PreparerCaches(chantier);

            CreerVariablesDeDecision(model, probleme, tachesAssignables, tachesIntervals);
            AjouterContraintesAssignationUnique(model, chantier, tachesAssignables);
            AjouterContraintesRessources(model, chantier, tachesIntervals, tachesAssignables);
            AjouterContraintesDePrecedence(model, chantier, tachesIntervals);

            // Optimisation: Calcul de makespan avec borne supérieure précise
            var maxEndTime = CalculerBorneSuperieureMakespan(probleme);
            var makespan = model.NewIntVar(0, maxEndTime, "makespan");

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

        private void PreparerCaches(Chantier chantier)
        {
            // Cache des prérequis métier avec fermeture transitive
            _prerequisitesCache = new Dictionary<MetierId, HashSet<MetierId>>();
            foreach (var metier in chantier.Metiers.Values)
            {
                _prerequisitesCache[metier.Id] = CalculerFermetureTransitive(metier, chantier.Metiers);
            }

            // Cache des tâches par métier par bloc
            _tachesParMetierParBlocCache = new Dictionary<BlocId, Dictionary<MetierId, List<Tache>>>();
            foreach (var bloc in chantier.Blocs.Values)
            {
                _tachesParMetierParBlocCache[bloc.Id] = bloc.Taches.Values
                    .GroupBy(t => t.MetierRequisId)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
        }

        private HashSet<MetierId> CalculerFermetureTransitive(Metier metier, IReadOnlyDictionary<MetierId, Metier> metiers)
        {
            var result = new HashSet<MetierId>();
            var toProcess = new Queue<MetierId>(metier.PrerequisMetierIds);
            var processed = new HashSet<MetierId>();

            while (toProcess.Any())
            {
                var currentId = toProcess.Dequeue();
                if (processed.Contains(currentId)) continue;

                result.Add(currentId);
                processed.Add(currentId);

                if (metiers.TryGetValue(currentId, out var prerequisMetier))
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

            return result;
        }

        private long CalculerBorneSuperieureMakespan(ProblemeOptimisation probleme)
        {
            var chantier = probleme.Chantier;
            var totalHeuresHomme = chantier.ObtenirToutesLesTaches().Sum(t => (long)t.HeuresHommeEstimees.Value);
            var nombreOuvriers = chantier.Ouvriers.Count;

            // Estimation pessimiste: toutes les tâches en séquence
            var estimationSequentielle = totalHeuresHomme;

            // Estimation avec parallélisation parfaite
            var estimationParallele = nombreOuvriers > 0 ? totalHeuresHomme / nombreOuvriers : totalHeuresHomme;

            // Prendre le minimum entre l'estimation séquentielle et l'horizon complet
            var horizonComplet = probleme.EchelleTemps.NombreTotalSlots;

            return Math.Min(estimationSequentielle, horizonComplet);
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

            // Optimisation: Pré-calcul des ouvriers compétents par métier
            var ouvriersParMetier = chantier.Metiers.Keys.ToDictionary(
                metierId => metierId,
                metierId => chantier.Ouvriers.Values.Where(o => o.PossedeCompetence(metierId)).ToList()
            );

            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                var duree = (long)tache.HeuresHommeEstimees.Value;

                // Optimisation: Borne supérieure plus précise pour le start
                var maxStart = horizon > duree ? horizon - duree : 0;
                var startVar = model.NewIntVar(0, maxStart, $"start_{tache.Id.Value}");
                var endVar = model.NewIntVar(duree, horizon, $"end_{tache.Id.Value}");
                var sizeVar = model.NewConstant(duree);

                var intervalleVirtuel = model.NewIntervalVar(startVar, sizeVar, endVar, $"interval_{tache.Id.Value}");
                tachesIntervals.Add(tache.Id, intervalleVirtuel);

                // Optimisation: Contrainte de journée uniquement si nécessaire
                if (duree > 0 && duree <= heuresParJour && heuresParJour > 1)
                {
                    var startDansJour = model.NewIntVar(0, heuresParJour - 1, $"start_dans_jour_{tache.Id.Value}");
                    model.AddModuloEquality(startDansJour, startVar, heuresParJour);
                    model.Add(startDansJour + duree <= heuresParJour);
                }

                // Utilisation du cache des ouvriers compétents
                var ouvriersCompetents = ouvriersParMetier[tache.MetierRequisId];

                if (!ouvriersCompetents.Any())
                {
                    throw new InvalidOperationException($"Aucun ouvrier compétent trouvé pour la tâche {tache.Id.Value} ({tache.Nom}) - Métier: {tache.MetierRequisId.Value}");
                }

                foreach (var ouvrier in ouvriersCompetents)
                {
                    var estAssignable = model.NewBoolVar($"estAssignable_{tache.Id.Value}_a_{ouvrier.Id.Value}");
                    tachesAssignables.Add((tache.Id, ouvrier.Id), estAssignable);
                }
            }
        }

        private void AjouterContraintesAssignationUnique(
            CpModel model,
            Chantier chantier,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            // Optimisation: Pré-groupement des assignations par tâche
            var assignationsParTache = tachesAssignables
                .GroupBy(kvp => kvp.Key.Item1)
                .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Value).ToList());

            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                if (assignationsParTache.TryGetValue(tache.Id, out var candidats) && candidats.Any())
                {
                    model.AddExactlyOne(candidats);
                }
                else
                {
                    throw new InvalidOperationException($"Aucun candidat d'assignation trouvé pour la tâche {tache.Id.Value} ({tache.Nom}).");
                }
            }
        }

        private void AjouterContraintesRessources(
            CpModel model,
            Chantier chantier,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            // Optimisation: Pré-groupement des assignations par ouvrier
            var assignationsParOuvrier = tachesAssignables
                .GroupBy(kvp => kvp.Key.Item2)
                .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Key.Item1).ToList());

            foreach (var ouvrier in chantier.Ouvriers.Values)
            {
                if (!assignationsParOuvrier.TryGetValue(ouvrier.Id, out var tacheIds) || !tacheIds.Any())
                {
                    continue; // Ignorer les ouvriers sans assignations possibles
                }

                var intervallesPourOuvrier = new List<IntervalVar>();
                foreach (var tacheId in tacheIds)
                {
                    if (tachesAssignables.TryGetValue((tacheId, ouvrier.Id), out var estAssignable))
                    {
                        var intervalleBase = tachesIntervals[tacheId];
                        var intervalleOptionnel = model.NewOptionalIntervalVar(
                            intervalleBase.StartExpr(),
                            intervalleBase.SizeExpr(),
                            intervalleBase.EndExpr(),
                            estAssignable,
                            $"optionnel_{tacheId.Value}_a_{ouvrier.Id.Value}"
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

        private void AjouterContraintesDePrecedence(
            CpModel model,
            Chantier chantier,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals)
        {
            // ÉTAPE 1: Dépendances explicites (optimisé avec validation préalable)
            var tachesAvecDependances = chantier.ObtenirToutesLesTaches()
                .Where(t => t.Dependencies?.Any() == true)
                .ToList();

            foreach (var tache in tachesAvecDependances)
            {
                var intervalleTacheActuelle = tachesIntervals[tache.Id];
                var intervallesDependances = tache.Dependencies
                    .Where(depId => tachesIntervals.ContainsKey(depId))
                    .Select(depId => tachesIntervals[depId])
                    .ToList();

                if (intervallesDependances.Any())
                {
                    // Optimisation: Utilisation d'une contrainte max pour les dépendances multiples
                    if (intervallesDependances.Count == 1)
                    {
                        model.Add(intervalleTacheActuelle.StartExpr() >= intervallesDependances[0].EndExpr());
                    }
                    else
                    {
                        var maxFinDependances = model.NewIntVar(0, int.MaxValue,
                            $"max_fin_deps_{tache.Id.Value}");
                        model.AddMaxEquality(maxFinDependances, intervallesDependances.Select(i => i.EndExpr()));
                        model.Add(intervalleTacheActuelle.StartExpr() >= maxFinDependances);
                    }
                }
            }

            // ÉTAPE 2: Dépendances implicites (optimisé avec cache)
            foreach (var bloc in chantier.Blocs.Values)
            {
                var tachesParMetier = _tachesParMetierParBlocCache[bloc.Id];

                foreach (var (metierId, tachesDuMetier) in tachesParMetier)
                {
                    var prerequisPourCeMetier = _prerequisitesCache[metierId];
                    if (!prerequisPourCeMetier.Any()) continue;

                    // Optimisation: Collecte efficace des tâches prérequises
                    var tachesPrerequisesDansLeBloc = prerequisPourCeMetier
                        .Where(prereqId => tachesParMetier.ContainsKey(prereqId))
                        .SelectMany(prereqId => tachesParMetier[prereqId])
                        .ToList();

                    if (!tachesPrerequisesDansLeBloc.Any()) continue;

                    // Optimisation: Variable unique pour le maximum des fins de prérequis
                    var finMaxPrerequis = model.NewIntVar(0, int.MaxValue,
                        $"fin_prerequis_{metierId.Value}_bloc_{bloc.Id.Value}");
                    model.AddMaxEquality(finMaxPrerequis,
                        tachesPrerequisesDansLeBloc.Select(t => tachesIntervals[t.Id].EndExpr()));

                    // Application de la contrainte à toutes les tâches du métier
                    foreach (var tache in tachesDuMetier)
                    {
                        model.Add(tachesIntervals[tache.Id].StartExpr() >= finMaxPrerequis);
                    }
                }
            }
        }
    }
}