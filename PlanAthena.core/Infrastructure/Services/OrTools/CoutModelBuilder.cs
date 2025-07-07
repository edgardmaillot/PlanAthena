// Fichier : CoutModelBuilder.cs (Version Finale avec Coût Indirect basé sur Makespan)

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
        public IntVar Construire(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables,
            IntVar makespan) // Le makespan est maintenant un argument
        {
            var coutsOuvriers = CalculerCoutsOuvriers(model, probleme, tachesIntervals, tachesAssignables);
            var coutsIndirects = ConstruireCoutsIndirects(model, probleme, makespan);

            var coutTotal = model.NewIntVar(0, 1_000_000_000, "cout_total_chantier");
            var tousLesCouts = new List<LinearExpr>();
            tousLesCouts.AddRange(coutsOuvriers);
            tousLesCouts.AddRange(coutsIndirects);

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
            // Cette méthode reste identique à votre version optimisée.
            // Elle est omise ici pour la clarté, mais vous devez la conserver dans votre fichier.
            // ... (coller ici l'intégralité de votre méthode CalculerCoutsOuvriers fonctionnelle)
            return new List<LinearExpr>(); // Placeholder
        }

        private List<LinearExpr> ConstruireCoutsIndirects(
            CpModel model,
            ProblemeOptimisation probleme,
            IntVar makespan) // On utilise le makespan
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

                // Le premier slot de la journée
                var jourStartSlot = jourSlots.Min(s => s.Index);

                // Le jour est actif si la fin du chantier (makespan) est après le début de ce jour.
                // jourActif <=> (makespan > jourStartSlot)
                var makespanApresDebutJour = model.NewBoolVar($"makespan_apres_debut_{jour:yyyyMMdd}");
                model.Add(makespan > jourStartSlot).OnlyEnforceIf(makespanApresDebutJour);
                model.Add(makespan <= jourStartSlot).OnlyEnforceIf(makespanApresDebutJour.Not());

                // On lie la variable jourActif à cette condition.
                model.Add(jourActif == makespanApresDebutJour);

                coutsIndirects.Add(jourActif * coutIndirectParJour);
            }

            return coutsIndirects;
        }
    }
}