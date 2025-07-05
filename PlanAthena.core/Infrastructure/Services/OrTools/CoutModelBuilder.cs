// Fichier : CoutModelBuilder.cs (LA VERSION FINALE, COMPLÈTE ET CORRECTE)

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
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var chantier = probleme.Chantier;
            var echelleTemps = probleme.EchelleTemps;

            // Étape 1 : Créer les variables de présence "Tâche T est active sur le Slot S"
            // Cette brique de base est cruciale et doit être parfaitement correcte.
            var presencesTacheSurSlot = CreerVariablesDePresence(model, echelleTemps, chantier, tachesIntervals);

            // Étape 2 : Pour chaque ouvrier et chaque jour, calculer sa durée de travail et lier au coût
            var tousLesCoutsJournaliers = new List<IntVar>();
            var slotsParJour = echelleTemps.Slots.GroupBy(s => s.Debut.Date);

            foreach (var ouvrier in chantier.Ouvriers.Values)
            {
                foreach (var jourGroup in slotsParJour)
                {
                    var dureeTravail = CalculerDureeTravailJournaliere(model, ouvrier, jourGroup, presencesTacheSurSlot, tachesAssignables);
                    var coutJournalier = LierDureeAuCout(model, ouvrier, jourGroup.Key, dureeTravail, probleme.Configuration);
                    tousLesCoutsJournaliers.Add(coutJournalier);
                }
            }

            // Étape 3 : Définir l'objectif final
            const long maxCoutTotalProjetEnCentimes = 10_000_000 * 100;
            var coutTotal = model.NewIntVar(0, maxCoutTotalProjetEnCentimes, "cout_total_chantier");
            model.Add(coutTotal == LinearExpr.Sum(tousLesCoutsJournaliers));
            return coutTotal;
        }

        private Dictionary<(TacheId, int), BoolVar> CreerVariablesDePresence(CpModel model, EchelleTempsOuvree echelleTemps, Chantier chantier, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals)
        {
            var presences = new Dictionary<(TacheId, int), BoolVar>();
            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                var intervalle = tachesIntervals[tache.Id];
                for (int s = 0; s < echelleTemps.NombreTotalSlots; s++)
                {
                    var p = model.NewBoolVar($"presence_t{tache.Id.Value}_s{s}");
                    presences.Add((tache.Id, s), p);

                    // La réification correcte et explicite qui corrige les erreurs de compilation.
                    // p <=> (start <= s) AND (s < end)
                    var startOk = model.NewBoolVar($"start_ok_t{tache.Id.Value}_s{s}");
                    model.Add(intervalle.StartExpr() <= s).OnlyEnforceIf(startOk);
                    model.Add(intervalle.StartExpr() > s).OnlyEnforceIf(startOk.Not());

                    var endOk = model.NewBoolVar($"end_ok_t{tache.Id.Value}_s{s}");
                    model.Add(intervalle.EndExpr() > s).OnlyEnforceIf(endOk);
                    model.Add(intervalle.EndExpr() <= s).OnlyEnforceIf(endOk.Not());

                    model.AddBoolAnd(new[] { startOk, endOk }).OnlyEnforceIf(p);
                    model.AddImplication(p, startOk);
                    model.AddImplication(p, endOk);
                }
            }
            return presences;
        }

        private IntVar CalculerDureeTravailJournaliere(CpModel model, Ouvrier ouvrier, IGrouping<LocalDate, SlotTemporel> jourGroup, IReadOnlyDictionary<(TacheId, int), BoolVar> presencesTacheSurSlot, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var travailEffectifJournalier = new List<ILiteral>();
            foreach (var slot in jourGroup)
            {
                var travailSurUneTacheCeSlot = tachesAssignables
                    .Where(a => a.Key.Item2 == ouvrier.Id)
                    .Select(a =>
                    {
                        var (tacheId, _) = a.Key;
                        var estAssignableVar = a.Value;
                        var presenceTacheVar = presencesTacheSurSlot[(tacheId, slot.Index)];

                        var travailSurTache = model.NewBoolVar($"travail_o{ouvrier.Id.Value}_t{tacheId.Value}_s{slot.Index}");
                        model.AddBoolAnd(new[] { estAssignableVar, presenceTacheVar }).OnlyEnforceIf(travailSurTache);
                        model.AddImplication(travailSurTache, estAssignableVar);
                        model.AddImplication(travailSurTache, presenceTacheVar);

                        return travailSurTache;
                    })
                    .ToList();

                travailEffectifJournalier.AddRange(travailSurUneTacheCeSlot);
            }

            var dureeJournaliere = model.NewIntVar(0, jourGroup.Count(), $"duree_o{ouvrier.Id.Value}_j{jourGroup.Key:yyyyMMdd}");
            model.Add(dureeJournaliere == LinearExpr.Sum(travailEffectifJournalier));
            return dureeJournaliere;
        }

        private IntVar LierDureeAuCout(CpModel model, Ouvrier ouvrier, LocalDate jour, IntVar dureeJournaliere, ConfigurationOptimisation config)
        {
            const long maxCoutJournalierEnCentimes = 100000 * 100;
            var coutJournalier = model.NewIntVar(0, maxCoutJournalierEnCentimes, $"cout_o{ouvrier.Id.Value}_j{jour:yyyyMMdd}");
            long[] coutsParHeureEnCentimes = CalculerCoutsParPalierEnCentimes(ouvrier, config, new ReglesCoutHeuresSupp());
            model.AddElement(dureeJournaliere, coutsParHeureEnCentimes, coutJournalier);
            return coutJournalier;
        }

        private long[] CalculerCoutsParPalierEnCentimes(Ouvrier ouvrier, ConfigurationOptimisation config, ReglesCoutHeuresSupp regles)
        {
            var couts = new long[25];
            long coutDeBaseEnCentimes = (long)ouvrier.Cout.Value * 100;
            int dureeStandard = config.DureeJournaliereStandardHeures;
            couts[0] = 0;
            if (dureeStandard <= 0) return couts;
            long multPalier1 = (long)(regles.MultiplicateurPalier1 * 100);
            long multPalier2 = (long)(regles.MultiplicateurPalier2 * 100);
            long coutHeureSuppPalier1EnCentimes = (coutDeBaseEnCentimes * multPalier1 / 100) / dureeStandard;
            long coutHeureSuppPalier2EnCentimes = (coutDeBaseEnCentimes * multPalier2 / 100) / dureeStandard;
            int finPalier1 = dureeStandard + regles.DureePalier1;
            for (int h = 1; h <= 24; h++)
            {
                if (h <= dureeStandard) { couts[h] = coutDeBaseEnCentimes; }
                else if (h <= finPalier1) { couts[h] = coutDeBaseEnCentimes + (h - dureeStandard) * coutHeureSuppPalier1EnCentimes; }
                else { couts[h] = coutDeBaseEnCentimes + (regles.DureePalier1 * coutHeureSuppPalier1EnCentimes) + (h - finPalier1) * coutHeureSuppPalier2EnCentimes; }
            }
            return couts;
        }
    }

    public class ReglesCoutHeuresSupp
    {
        public decimal MultiplicateurPalier1 { get; set; } = 1.25m;
        public decimal MultiplicateurPalier2 { get; set; } = 1.50m;
        public int DureePalier1 { get; set; } = 2;
    }
}