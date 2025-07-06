// Fichier : CoutModelBuilder.cs (Version optimisée + Coût Indirect)

using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using System; // Ajout pour Console.WriteLine
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class CoutModelBuilder
    {
        public IntVar Construire(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            // --- PARTIE 1 : Calcul du Coût des Ressources Humaines (votre code fonctionnel) ---
            var coutsJournaliersOuvriers = CalculerCoutsOuvriers(model, probleme, tachesIntervals, tachesAssignables);

            // --- PARTIE 2 : NOUVEAU - Calcul du Coût Indirect du Chantier ---
            var coutsIndirectsJournaliers = ConstruireCoutsIndirects(model, probleme, tachesIntervals);

            // --- PARTIE 3 : Objectif Final Combiné ---
            var coutTotal = model.NewIntVar(0, 1_000_000_000, "cout_total_chantier");
            var tousLesCouts = new List<LinearExpr>();
            tousLesCouts.AddRange(coutsJournaliersOuvriers);
            tousLesCouts.AddRange(coutsIndirectsJournaliers);

            if (tousLesCouts.Any())
            {
                model.Add(coutTotal == LinearExpr.Sum(tousLesCouts));
            }
            else
            {
                model.Add(coutTotal == 0);
            }

            Console.WriteLine($"[DEBUG] Modèle de coût construit avec {tousLesCouts.Count} termes de coût au total.");

            return coutTotal;
        }

        private List<LinearExpr> CalculerCoutsOuvriers(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var chantier = probleme.Chantier;
            var echelleTemps = probleme.EchelleTemps;

            // --- OPTIMISATION 1 : Pré-filtrage des données ---
            var tachesReelles = chantier.ObtenirToutesLesTaches().ToList();
            var ouvriersReels = chantier.Ouvriers.Values.ToList();
            var joursDuPlanning = echelleTemps.Slots.Select(s => s.Debut.Date).Distinct().OrderBy(d => d).ToList();

            var slotsByDay = joursDuPlanning.ToDictionary(
                jour => jour,
                jour => echelleTemps.Slots.Where(s => s.Debut.Date == jour).ToList()
            );

            Console.WriteLine($"[DEBUG] Optimisation des coûts pour {tachesReelles.Count} tâches, {ouvriersReels.Count} ouvriers, {joursDuPlanning.Count} jours");

            // --- OPTIMISATION 2 : Créer uniquement les variables nécessaires ---
            var ouvrierTravailleLeJour = new Dictionary<(OuvrierId, LocalDate), BoolVar>();
            var assignationsParOuvrier = new Dictionary<OuvrierId, List<(TacheId, BoolVar)>>();

            foreach (var ouvrier in ouvriersReels)
            {
                assignationsParOuvrier[ouvrier.Id] = new List<(TacheId, BoolVar)>();
            }

            foreach (var ((tacheId, ouvrierId), assignVar) in tachesAssignables)
            {
                if (assignationsParOuvrier.ContainsKey(ouvrierId))
                {
                    assignationsParOuvrier[ouvrierId].Add((tacheId, assignVar));
                }
            }

            foreach (var ouvrier in ouvriersReels)
            {
                if (!assignationsParOuvrier[ouvrier.Id].Any()) continue;

                foreach (var jour in joursDuPlanning)
                {
                    ouvrierTravailleLeJour.Add((ouvrier.Id, jour),
                        model.NewBoolVar($"travail_o{ouvrier.Id.Value}_j{jour:yyyyMMdd}"));
                }
            }

            Console.WriteLine($"[DEBUG] Créé {ouvrierTravailleLeJour.Count} variables de travail journalier");

            // --- OPTIMISATION 3 : Contraintes simplifiées avec réification directe ---
            foreach (var ouvrier in ouvriersReels)
            {
                if (!assignationsParOuvrier[ouvrier.Id].Any()) continue;

                foreach (var jour in joursDuPlanning)
                {
                    if (!ouvrierTravailleLeJour.ContainsKey((ouvrier.Id, jour))) continue;

                    var travailleCeJourVar = ouvrierTravailleLeJour[(ouvrier.Id, jour)];
                    var jourSlots = slotsByDay[jour];

                    if (!jourSlots.Any())
                    {
                        model.Add(travailleCeJourVar == 0);
                        continue;
                    }

                    var jourStart = jourSlots.Min(s => s.Index);
                    var jourEnd = jourSlots.Max(s => s.Index) + 1;

                    var tachesActivesCeJour = new List<BoolVar>();

                    // --- OPTIMISATION 4 : Traitement uniquement des tâches assignables ---
                    foreach (var (tacheId, assignVar) in assignationsParOuvrier[ouvrier.Id])
                    {
                        if (!tachesIntervals.ContainsKey(tacheId)) continue;

                        var interval = tachesIntervals[tacheId];
                        var tacheActiveCeJour = model.NewBoolVar($"active_{tacheId.Value}_o{ouvrier.Id.Value}_j{jour:yyyyMMdd}");

                        var chevauche = model.NewBoolVar($"overlap_{tacheId.Value}_j{jour:yyyyMMdd}");

                        var startOk = model.NewBoolVar($"start_ok_{tacheId.Value}_j{jour:yyyyMMdd}");
                        var endOk = model.NewBoolVar($"end_ok_{tacheId.Value}_j{jour:yyyyMMdd}");

                        model.Add(interval.StartExpr() < jourEnd).OnlyEnforceIf(startOk);
                        model.Add(interval.StartExpr() >= jourEnd).OnlyEnforceIf(startOk.Not());

                        model.Add(interval.EndExpr() > jourStart).OnlyEnforceIf(endOk);
                        model.Add(interval.EndExpr() <= jourStart).OnlyEnforceIf(endOk.Not());

                        model.AddBoolAnd(new[] { startOk, endOk }).OnlyEnforceIf(chevauche);
                        model.AddBoolOr(new[] { startOk.Not(), endOk.Not() }).OnlyEnforceIf(chevauche.Not());

                        model.AddBoolAnd(new[] { assignVar, chevauche }).OnlyEnforceIf(tacheActiveCeJour);
                        model.AddBoolOr(new[] { assignVar.Not(), chevauche.Not() }).OnlyEnforceIf(tacheActiveCeJour.Not());

                        tachesActivesCeJour.Add(tacheActiveCeJour);
                    }

                    // --- OPTIMISATION 6 : Contrainte de travail journalier simplifiée ---
                    if (tachesActivesCeJour.Any())
                    {
                        model.AddMaxEquality(travailleCeJourVar, tachesActivesCeJour);
                    }
                    else
                    {
                        model.Add(travailleCeJourVar == 0);
                    }
                }
            }

            // --- NOUVELLE OPTIMISATION : Contrainte Redondante ---
            foreach (var ouvrier in ouvriersReels)
            {
                if (!assignationsParOuvrier.ContainsKey(ouvrier.Id) || !assignationsParOuvrier[ouvrier.Id].Any()) continue;

                var heuresTotalesPossibles = assignationsParOuvrier[ouvrier.Id]
                    .Sum(t => tachesReelles.First(tr => tr.Id == t.Item1).HeuresHommeEstimees.Value);

                var maxJoursTravail = (int)Math.Ceiling((double)heuresTotalesPossibles / chantier.Calendrier.DureeTravailEffectiveParJour.TotalHours);

                var joursOuvrier = joursDuPlanning
                    .Where(j => ouvrierTravailleLeJour.ContainsKey((ouvrier.Id, j)))
                    .Select(j => ouvrierTravailleLeJour[(ouvrier.Id, j)])
                    .ToList<LinearExpr>();

                if (joursOuvrier.Any())
                {
                    model.Add(LinearExpr.Sum(joursOuvrier) <= maxJoursTravail);
                }
            }

            // --- OPTIMISATION 7 : Calcul de coût optimisé ---
            var coutsJournaliers = new List<LinearExpr>();
            foreach (var ((ouvrierId, _), travailleCeJourVar) in ouvrierTravailleLeJour)
            {
                var coutJournalierOuvrier = (long)chantier.Ouvriers[ouvrierId].Cout.Value * 100;
                coutsJournaliers.Add(travailleCeJourVar * coutJournalierOuvrier);
            }

            return coutsJournaliers;
        }

        private List<LinearExpr> ConstruireCoutsIndirects(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals)
        {
            var coutsIndirects = new List<LinearExpr>();
            long coutIndirectParJour = probleme.Configuration.CoutIndirectJournalierEnCentimes;

            if (coutIndirectParJour <= 0) return coutsIndirects;

            var echelleTemps = probleme.EchelleTemps;
            var joursDuPlanning = echelleTemps.Slots.Select(s => s.Debut.Date).Distinct().OrderBy(d => d).ToList();
            var slotsByDay = joursDuPlanning.ToDictionary(jour => jour, jour => echelleTemps.Slots.Where(s => s.Debut.Date == jour).ToList());

            foreach (var jour in joursDuPlanning)
            {
                var jourActif = model.NewBoolVar($"jour_actif_{jour:yyyyMMdd}");
                var jourSlots = slotsByDay[jour];
                var jourStart = jourSlots.Min(s => s.Index);
                var jourEnd = jourSlots.Max(s => s.Index) + 1;

                var tachesActivesCeJour = new List<BoolVar>();
                foreach (var interval in tachesIntervals.Values)
                {
                    var tacheActive = model.NewBoolVar($"tache_active_pour_cout_indirect_{interval.Name}_{jour:yyyyMMdd}");

                    var startOk = model.NewBoolVar($"start_ok_indirect_{interval.Name}_j{jour:yyyyMMdd}");
                    model.Add(interval.StartExpr() < jourEnd).OnlyEnforceIf(startOk);
                    model.Add(interval.StartExpr() >= jourEnd).OnlyEnforceIf(startOk.Not());

                    var endOk = model.NewBoolVar($"end_ok_indirect_{interval.Name}_j{jour:yyyyMMdd}");
                    model.Add(interval.EndExpr() > jourStart).OnlyEnforceIf(endOk);
                    model.Add(interval.EndExpr() <= jourStart).OnlyEnforceIf(endOk.Not());

                    model.AddBoolAnd(new[] { startOk, endOk }).OnlyEnforceIf(tacheActive);
                    tachesActivesCeJour.Add(tacheActive);
                }

                model.AddMaxEquality(jourActif, tachesActivesCeJour);
                coutsIndirects.Add(jourActif * coutIndirectParJour);
            }

            return coutsIndirects;
        }
    }
}