// Fichier : Infrastructure/Services/OrTools/CoutModelBuilder.cs (Version Optimisée)

using Google.OrTools.Sat;
using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain.ValueObjects;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class CoutModelBuilder
    {
        // Cache pour éviter les recalculs répétitifs
        private Dictionary<LocalDate, List<SlotTemporel>> _slotsByDayCache;
        private List<LocalDate> _joursDuPlanningCache;

        public (IntVar CoutTotal, IntVar CoutRh, IntVar CoutIndirect) Construire(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables,
            IntVar makespan)
        {
            // Précalcul des données communes
            PreparerCachesTemporels(probleme.EchelleTemps);

            var (coutsOuvriersExpr, _) = CalculerCoutsOuvriers(model, probleme, tachesIntervals, tachesAssignables);
            var coutsIndirectsExpr = ConstruireCoutsIndirects(model, probleme, makespan);

            // Calcul des bornes supérieures plus précises
            var borneCoutRh = CalculerBorneSuperieureCoutRh(probleme);
            var borneCoutIndirect = CalculerBorneSuperieureCoutIndirect(probleme);
            var borneCoutTotal = borneCoutRh + borneCoutIndirect;

            var coutRh = model.NewIntVar(0, borneCoutRh, "cout_rh");
            var coutIndirect = model.NewIntVar(0, borneCoutIndirect, "cout_indirect");
            var coutTotal = model.NewIntVar(0, borneCoutTotal, "cout_total_chantier");

            model.Add(coutRh == LinearExpr.Sum(coutsOuvriersExpr));
            model.Add(coutIndirect == LinearExpr.Sum(coutsIndirectsExpr));
            model.Add(coutTotal == coutRh + coutIndirect);

            Console.WriteLine($"[DEBUG] Modèle de coût optimisé construit. Bornes: RH={borneCoutRh}, Indirect={borneCoutIndirect}");

            return (coutTotal, coutRh, coutIndirect);
        }

        private void PreparerCachesTemporels(EchelleTempsOuvree echelleTemps)
        {
            _joursDuPlanningCache = echelleTemps.Slots.Select(s => s.Debut.Date).Distinct().OrderBy(d => d).ToList();
            _slotsByDayCache = _joursDuPlanningCache.ToDictionary(
                jour => jour,
                jour => echelleTemps.Slots.Where(s => s.Debut.Date == jour).ToList()
            );
        }

        private long CalculerBorneSuperieureCoutRh(ProblemeOptimisation probleme)
        {
            var chantier = probleme.Chantier;
            var totalHeuresHomme = chantier.ObtenirToutesLesTaches().Sum(t => t.HeuresHommeEstimees.Value);
            var coutMaxParHeure = chantier.Ouvriers.Values.Max(o => o.Cout.Value);
            return (long)(totalHeuresHomme * coutMaxParHeure * 100); // en centimes
        }

        private long CalculerBorneSuperieureCoutIndirect(ProblemeOptimisation probleme)
        {
            var coutParJour = probleme.Configuration.CoutIndirectJournalierEnCentimes;
            var nombreJours = _joursDuPlanningCache.Count;
            return coutParJour * nombreJours;
        }

        private (List<LinearExpr> couts, Dictionary<(OuvrierId, LocalDate), BoolVar> travailJournalier) CalculerCoutsOuvriers(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var chantier = probleme.Chantier;

            // Optimisation 1: Pré-groupement des assignations par ouvrier
            var assignationsParOuvrier = tachesAssignables
                .GroupBy(kv => kv.Key.Item2)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(kv => chantier.ObtenirToutesLesTaches().Any(t => t.Id == kv.Key.Item1))
                          .Select(kv => (kv.Key.Item1, kv.Value))
                          .ToList()
                );

            // Optimisation 2: Filtrage précoce des ouvriers sans assignations
            var ouvriersActifs = chantier.Ouvriers.Values
                .Where(o => assignationsParOuvrier.ContainsKey(o.Id) && assignationsParOuvrier[o.Id].Any())
                .ToList();

            Console.WriteLine($"[DEBUG] Calcul optimisé pour {ouvriersActifs.Count} ouvriers actifs sur {chantier.Ouvriers.Count} total");

            var ouvrierTravailleLeJour = new Dictionary<(OuvrierId, LocalDate), BoolVar>();

            // Optimisation 3: Pré-calcul des plages journalières
            var plagesJournalieres = _joursDuPlanningCache.ToDictionary(
                jour => jour,
                jour =>
                {
                    var slots = _slotsByDayCache[jour];
                    return (Start: slots.Min(s => s.Index), End: slots.Max(s => s.Index) + 1);
                }
            );

            // Création des variables de travail journalier uniquement pour les ouvriers actifs
            foreach (var ouvrier in ouvriersActifs)
            {
                foreach (var jour in _joursDuPlanningCache)
                {
                    ouvrierTravailleLeJour.Add((ouvrier.Id, jour),
                        model.NewBoolVar($"travail_o{ouvrier.Id.Value}_j{jour:yyyyMMdd}"));
                }
            }

            // Optimisation 4: Contraintes de chevauchement simplifiées
            foreach (var (key, travailleCeJourVar) in ouvrierTravailleLeJour)
            {
                var (ouvrierId, jour) = key;
                var (jourStart, jourEnd) = plagesJournalieres[jour];
                var tachesActivesCeJour = new List<BoolVar>();

                foreach (var (tacheId, assignVar) in assignationsParOuvrier[ouvrierId])
                {
                    var interval = tachesIntervals[tacheId];
                    var tacheActiveCeJour = model.NewBoolVar($"active_{tacheId.Value}_o{ouvrierId.Value}_j{jour:yyyyMMdd}");

                    // Contraintes de chevauchement (version corrigée - retour à l'original)
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

                if (tachesActivesCeJour.Any())
                {
                    model.AddMaxEquality(travailleCeJourVar, tachesActivesCeJour);
                }
                else
                {
                    model.Add(travailleCeJourVar == 0);
                }
            }

            // Calcul des coûts journaliers (version corrigée)
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

            // Optimisation 7: Calcul groupé des jours actifs
            var joursActifs = new List<BoolVar>();
            var plagesJournalieres = _joursDuPlanningCache.ToDictionary(
                jour => jour,
                jour =>
                {
                    var slots = _slotsByDayCache[jour];
                    return slots.Any() ? slots.Min(s => s.Index) : -1;
                }
            );

            foreach (var jour in _joursDuPlanningCache)
            {
                var jourStartSlot = plagesJournalieres[jour];
                if (jourStartSlot == -1)
                {
                    continue; // Ignorer les jours sans slots
                }

                var jourActif = model.NewBoolVar($"jour_actif_{jour:yyyyMMdd}");
                model.Add(makespan > jourStartSlot).OnlyEnforceIf(jourActif);
                model.Add(makespan <= jourStartSlot).OnlyEnforceIf(jourActif.Not());

                joursActifs.Add(jourActif);
            }

            // Optimisation 8: Expression unique pour tous les coûts indirects
            if (joursActifs.Any())
            {
                var totalJoursActifs = model.NewIntVar(0, joursActifs.Count, "total_jours_actifs");
                model.Add(totalJoursActifs == LinearExpr.Sum(joursActifs));
                coutsIndirects.Add(totalJoursActifs * coutIndirectParJour);
            }

            return coutsIndirects;
        }
    }
}