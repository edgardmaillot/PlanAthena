// Fichier : Infrastructure/Services/OrTools/CoutModelBuilder.cs

using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class CoutModelBuilder
    {
        public (IntVar CoutTotal, IntVar CoutRh, IntVar CoutIndirect) Construire(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables,
            IntVar makespan)
        {
            var (coutsOuvriersExpr, _) = CalculerCoutsOuvriers(model, probleme, tachesIntervals, tachesAssignables);
            var coutsIndirectsExpr = ConstruireCoutsIndirects(model, probleme, makespan);

            var coutRh = model.NewIntVar(0, 1_000_000_000, "cout_rh");
            var coutIndirect = model.NewIntVar(0, 1_000_000_000, "cout_indirect");
            var coutTotal = model.NewIntVar(0, 1_000_000_000, "cout_total_chantier");

            model.Add(coutRh == LinearExpr.Sum(coutsOuvriersExpr));
            model.Add(coutIndirect == LinearExpr.Sum(coutsIndirectsExpr));
            model.Add(coutTotal == coutRh + coutIndirect);

            Console.WriteLine($"[DEBUG] Modèle de coût construit.");

            return (coutTotal, coutRh, coutIndirect);
        }


        private (List<LinearExpr> couts, Dictionary<(OuvrierId, LocalDate), BoolVar> travailJournalier) CalculerCoutsOuvriers(
    CpModel model,
    ProblemeOptimisation probleme,
    IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
    IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var chantier = probleme.Chantier;
            var echelleTemps = probleme.EchelleTemps;
            var joursDuPlanning = echelleTemps.Slots.Select(s => s.Debut.Date).Distinct().OrderBy(d => d).ToList();
            var slotsByDay = joursDuPlanning.ToDictionary(jour => jour, jour => echelleTemps.Slots.Where(s => s.Debut.Date == jour).ToList());

            // --- CORRECTION DE LA REQUÊTE LINQ ---
            var assignationsParOuvrier = tachesAssignables
                .GroupBy(kv => kv.Key.Item2) // Grouper par OuvrierId
                .ToDictionary(
                    g => g.Key, // La clé du dictionnaire est l'OuvrierId
                    g => g.Select(kv => (kv.Key.Item1, kv.Value)).ToList() // La valeur est une List de tuples (TacheId, BoolVar)
                );
            // --- FIN DE LA CORRECTION ---

            Console.WriteLine($"[DEBUG] Calcul coûts ouvriers pour {chantier.ObtenirToutesLesTaches().Count()} tâches, {chantier.Ouvriers.Count} ouvriers, {joursDuPlanning.Count} jours");

            var ouvrierTravailleLeJour = new Dictionary<(OuvrierId, LocalDate), BoolVar>();
            foreach (var ouvrier in chantier.Ouvriers.Values)
            {
                if (!assignationsParOuvrier.ContainsKey(ouvrier.Id) || !assignationsParOuvrier[ouvrier.Id].Any()) continue;
                foreach (var jour in joursDuPlanning)
                {
                    ouvrierTravailleLeJour.Add((ouvrier.Id, jour), model.NewBoolVar($"travail_o{ouvrier.Id.Value}_j{jour:yyyyMMdd}"));
                }
            }

            Console.WriteLine($"[DEBUG] Créé {ouvrierTravailleLeJour.Count} variables de travail journalier");

            foreach (var (key, travailleCeJourVar) in ouvrierTravailleLeJour)
            {
                var (ouvrierId, jour) = key;
                var jourSlots = slotsByDay[jour];
                var jourStart = jourSlots.Min(s => s.Index);
                var jourEnd = jourSlots.Max(s => s.Index) + 1;
                var tachesActivesCeJour = new List<BoolVar>();

                // Cette boucle fonctionne maintenant car assignationsParOuvrier[ouvrierId] est bien une List<(TacheId, BoolVar)>
                foreach (var (tacheId, assignVar) in assignationsParOuvrier[ouvrierId])
                {
                    var interval = tachesIntervals[tacheId];
                    var tacheActiveCeJour = model.NewBoolVar($"active_{tacheId.Value}_o{ouvrierId.Value}_j{jour:yyyyMMdd}");
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
                model.AddMaxEquality(travailleCeJourVar, tachesActivesCeJour);
            }

            var coutsJournaliers = new List<LinearExpr>();
            foreach (var (key, travailleCeJourVar) in ouvrierTravailleLeJour)
            {
                var coutJournalierOuvrier = (long)chantier.Ouvriers[key.Item1].Cout.Value * 100;
                coutsJournaliers.Add(travailleCeJourVar * coutJournalierOuvrier);
            }

            return (coutsJournaliers, ouvrierTravailleLeJour);
        }

        private List<LinearExpr> ConstruireCoutsIndirects(
            CpModel model,
            ProblemeOptimisation probleme,
            IntVar makespan)
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
                if (!jourSlots.Any())
                {
                    model.Add(jourActif == 0);
                    continue;
                }

                var jourStartSlot = jourSlots.Min(s => s.Index);

                var makespanApresDebutJour = model.NewBoolVar($"makespan_apres_debut_{jour:yyyyMMdd}");
                model.Add(makespan > jourStartSlot).OnlyEnforceIf(makespanApresDebutJour);
                model.Add(makespan <= jourStartSlot).OnlyEnforceIf(makespanApresDebutJour.Not());

                model.Add(jourActif == makespanApresDebutJour);

                coutsIndirects.Add(jourActif * coutIndirectParJour);
            }

            return coutsIndirects;
        }
    }
}