// Fichier : CoutModelBuilder.cs (Version ultra-optimisée)

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
        // Cache pour éviter les recalculs
        private readonly Dictionary<LocalDate, (int start, int end)> _bornesParJour = new();

        public IntVar Construire(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            // Pré-calcul des données communes
            var contextOptimisation = CreerContexteOptimisation(probleme, tachesIntervals);

            // Calcul des coûts avec le contexte partagé
            var coutsOuvriers = CalculerCoutsOuvriers(model, contextOptimisation, tachesAssignables);
            var coutsIndirects = CalculerCoutsIndirects(model, contextOptimisation);

            // Objectif final
            var coutTotal = model.NewIntVar(0, 1_000_000_000, "cout_total_chantier");
            var tousLesCouts = new List<LinearExpr>(coutsOuvriers.Count + coutsIndirects.Count);
            tousLesCouts.AddRange(coutsOuvriers);
            tousLesCouts.AddRange(coutsIndirects);

            if (tousLesCouts.Count > 0)
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

        private ContexteOptimisation CreerContexteOptimisation(
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals)
        {
            var chantier = probleme.Chantier;
            var echelleTemps = probleme.EchelleTemps;

            // Pré-calcul des jours et slots
            var joursDuPlanning = echelleTemps.Slots
                .Select(s => s.Debut.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToArray(); // Array pour meilleure performance

            var slotsByDay = joursDuPlanning.ToDictionary(
                jour => jour,
                jour => echelleTemps.Slots.Where(s => s.Debut.Date == jour).ToArray()
            );

            // Pré-calcul des bornes par jour (cache)
            _bornesParJour.Clear();
            foreach (var jour in joursDuPlanning)
            {
                var slots = slotsByDay[jour];
                if (slots.Length > 0)
                {
                    _bornesParJour[jour] = (slots.Min(s => s.Index), slots.Max(s => s.Index) + 1);
                }
            }

            // Pré-filtrage des données
            var tachesReelles = chantier.ObtenirToutesLesTaches().ToArray();
            var ouvriersReels = chantier.Ouvriers.Values.ToArray();

            return new ContexteOptimisation
            {
                Chantier = chantier,
                JoursDuPlanning = joursDuPlanning,
                SlotsByDay = slotsByDay,
                TachesReelles = tachesReelles,
                OuvriersReels = ouvriersReels,
                TachesIntervals = tachesIntervals,
                CoutIndirectParJour = probleme.Configuration.CoutIndirectJournalierEnCentimes
            };
        }

        private List<LinearExpr> CalculerCoutsOuvriers(
            CpModel model,
            ContexteOptimisation contexte,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            Console.WriteLine($"[DEBUG] Calcul coûts ouvriers pour {contexte.TachesReelles.Length} tâches, {contexte.OuvriersReels.Length} ouvriers, {contexte.JoursDuPlanning.Length} jours");

            // Optimisation: Pré-groupement des assignations par ouvrier
            var assignationsParOuvrier = GrouperAssignationsParOuvrier(contexte.OuvriersReels, tachesAssignables);

            // Création des variables de travail journalier (uniquement pour les ouvriers ayant des tâches)
            var ouvrierTravailleLeJour = CreerVariablesTravailJournalier(model, contexte, assignationsParOuvrier);

            // Contraintes optimisées
            DefinirContraintesTravailJournalier(model, contexte, assignationsParOuvrier, ouvrierTravailleLeJour);
            AjouterContraintesRedondantes(model, contexte, assignationsParOuvrier, ouvrierTravailleLeJour);

            // Calcul final des coûts
            return CalculerCoutsJournaliersOuvriers(contexte, ouvrierTravailleLeJour);
        }

        private Dictionary<OuvrierId, List<(TacheId, BoolVar)>> GrouperAssignationsParOuvrier(
            Ouvrier[] ouvriersReels,
            IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var assignationsParOuvrier = new Dictionary<OuvrierId, List<(TacheId, BoolVar)>>(ouvriersReels.Length);

            // Initialisation avec capacité estimée
            foreach (var ouvrier in ouvriersReels)
            {
                assignationsParOuvrier[ouvrier.Id] = new List<(TacheId, BoolVar)>();
            }

            // Groupement efficace
            foreach (var ((tacheId, ouvrierId), assignVar) in tachesAssignables)
            {
                if (assignationsParOuvrier.TryGetValue(ouvrierId, out var assignations))
                {
                    assignations.Add((tacheId, assignVar));
                }
            }

            return assignationsParOuvrier;
        }

        private Dictionary<(OuvrierId, LocalDate), BoolVar> CreerVariablesTravailJournalier(
            CpModel model,
            ContexteOptimisation contexte,
            Dictionary<OuvrierId, List<(TacheId, BoolVar)>> assignationsParOuvrier)
        {
            var ouvrierTravailleLeJour = new Dictionary<(OuvrierId, LocalDate), BoolVar>();

            foreach (var ouvrier in contexte.OuvriersReels)
            {
                // Optimisation: Skip les ouvriers sans assignations
                if (!assignationsParOuvrier[ouvrier.Id].Any()) continue;

                foreach (var jour in contexte.JoursDuPlanning)
                {
                    // Optimisation: Skip les jours sans slots
                    if (!_bornesParJour.ContainsKey(jour)) continue;

                    var varName = $"travail_o{ouvrier.Id.Value}_j{jour:yyyyMMdd}";
                    ouvrierTravailleLeJour.Add((ouvrier.Id, jour), model.NewBoolVar(varName));
                }
            }

            Console.WriteLine($"[DEBUG] Créé {ouvrierTravailleLeJour.Count} variables de travail journalier");
            return ouvrierTravailleLeJour;
        }

        private void DefinirContraintesTravailJournalier(
            CpModel model,
            ContexteOptimisation contexte,
            Dictionary<OuvrierId, List<(TacheId, BoolVar)>> assignationsParOuvrier,
            Dictionary<(OuvrierId, LocalDate), BoolVar> ouvrierTravailleLeJour)
        {
            foreach (var ouvrier in contexte.OuvriersReels)
            {
                if (!assignationsParOuvrier[ouvrier.Id].Any()) continue;

                foreach (var jour in contexte.JoursDuPlanning)
                {
                    if (!ouvrierTravailleLeJour.TryGetValue((ouvrier.Id, jour), out var travailleCeJourVar))
                        continue;

                    if (!_bornesParJour.TryGetValue(jour, out var bornes))
                    {
                        model.Add(travailleCeJourVar == 0);
                        continue;
                    }

                    var tachesActivesCeJour = CreerVariablesTachesActivesCeJour(
                        model, contexte, assignationsParOuvrier[ouvrier.Id],
                        ouvrier.Id, jour, bornes);

                    if (tachesActivesCeJour.Count > 0)
                    {
                        model.AddMaxEquality(travailleCeJourVar, tachesActivesCeJour);
                    }
                    else
                    {
                        model.Add(travailleCeJourVar == 0);
                    }
                }
            }
        }

        private List<BoolVar> CreerVariablesTachesActivesCeJour(
            CpModel model,
            ContexteOptimisation contexte,
            List<(TacheId, BoolVar)> assignationsOuvrier,
            OuvrierId ouvrierId,
            LocalDate jour,
            (int start, int end) bornesJour)
        {
            var tachesActivesCeJour = new List<BoolVar>();

            foreach (var (tacheId, assignVar) in assignationsOuvrier)
            {
                if (!contexte.TachesIntervals.TryGetValue(tacheId, out var interval))
                    continue;

                var tacheActiveCeJour = model.NewBoolVar($"active_{tacheId.Value}_o{ouvrierId.Value}_j{jour:yyyyMMdd}");

                // Optimisation: Contraintes de chevauchement simplifiées
                var chevauche = CreerContrainteChevauchement(model, interval, bornesJour, tacheId, jour);

                // Liaison assignation + chevauchement = tâche active
                model.AddBoolAnd(new[] { assignVar, chevauche }).OnlyEnforceIf(tacheActiveCeJour);
                model.AddBoolOr(new[] { assignVar.Not(), chevauche.Not() }).OnlyEnforceIf(tacheActiveCeJour.Not());

                tachesActivesCeJour.Add(tacheActiveCeJour);
            }

            return tachesActivesCeJour;
        }

        private BoolVar CreerContrainteChevauchement(
            CpModel model,
            IntervalVar interval,
            (int start, int end) bornesJour,
            TacheId tacheId,
            LocalDate jour)
        {
            var chevauche = model.NewBoolVar($"overlap_{tacheId.Value}_j{jour:yyyyMMdd}");

            // CORRECTION: Garder la même logique que l'original pour éviter les bugs
            var startOk = model.NewBoolVar($"start_ok_{tacheId.Value}_j{jour:yyyyMMdd}");
            var endOk = model.NewBoolVar($"end_ok_{tacheId.Value}_j{jour:yyyyMMdd}");

            model.Add(interval.StartExpr() < bornesJour.end).OnlyEnforceIf(startOk);
            model.Add(interval.StartExpr() >= bornesJour.end).OnlyEnforceIf(startOk.Not());

            model.Add(interval.EndExpr() > bornesJour.start).OnlyEnforceIf(endOk);
            model.Add(interval.EndExpr() <= bornesJour.start).OnlyEnforceIf(endOk.Not());

            model.AddBoolAnd(new[] { startOk, endOk }).OnlyEnforceIf(chevauche);
            model.AddBoolOr(new[] { startOk.Not(), endOk.Not() }).OnlyEnforceIf(chevauche.Not());

            return chevauche;
        }

        private void AjouterContraintesRedondantes(
            CpModel model,
            ContexteOptimisation contexte,
            Dictionary<OuvrierId, List<(TacheId, BoolVar)>> assignationsParOuvrier,
            Dictionary<(OuvrierId, LocalDate), BoolVar> ouvrierTravailleLeJour)
        {
            var dureeTravailParJour = contexte.Chantier.Calendrier.DureeTravailEffectiveParJour.TotalHours;

            foreach (var ouvrier in contexte.OuvriersReels)
            {
                if (!assignationsParOuvrier.TryGetValue(ouvrier.Id, out var assignations) || !assignations.Any())
                    continue;

                // CORRECTION: Garder la même logique que l'original pour les contraintes redondantes
                var heuresTotalesPossibles = assignations.Sum(t =>
                    contexte.TachesReelles.First(tr => tr.Id == t.Item1).HeuresHommeEstimees.Value);

                var maxJoursTravail = (int)Math.Ceiling((double)heuresTotalesPossibles / dureeTravailParJour);

                // Collecte des variables de jours de travail pour cet ouvrier
                var joursOuvrierVars = new List<LinearExpr>();
                foreach (var jour in contexte.JoursDuPlanning)
                {
                    if (ouvrierTravailleLeJour.TryGetValue((ouvrier.Id, jour), out var varJour))
                    {
                        joursOuvrierVars.Add(varJour);
                    }
                }

                if (joursOuvrierVars.Count > 0)
                {
                    model.Add(LinearExpr.Sum(joursOuvrierVars) <= maxJoursTravail);
                }
            }
        }

        private List<LinearExpr> CalculerCoutsJournaliersOuvriers(
            ContexteOptimisation contexte,
            Dictionary<(OuvrierId, LocalDate), BoolVar> ouvrierTravailleLeJour)
        {
            var coutsJournaliers = new List<LinearExpr>(ouvrierTravailleLeJour.Count);

            foreach (var ((ouvrierId, _), travailleCeJourVar) in ouvrierTravailleLeJour)
            {
                var coutJournalier = (long)contexte.Chantier.Ouvriers[ouvrierId].Cout.Value * 100;
                coutsJournaliers.Add(travailleCeJourVar * coutJournalier);
            }

            return coutsJournaliers;
        }

        private List<LinearExpr> CalculerCoutsIndirects(
            CpModel model,
            ContexteOptimisation contexte)
        {
            var coutsIndirects = new List<LinearExpr>();

            if (contexte.CoutIndirectParJour <= 0)
                return coutsIndirects;

            foreach (var jour in contexte.JoursDuPlanning)
            {
                var jourActif = model.NewBoolVar($"jour_actif_{jour:yyyyMMdd}");

                if (!_bornesParJour.TryGetValue(jour, out var bornes))
                {
                    model.Add(jourActif == 0);
                    continue;
                }

                var tachesActivesCeJour = CreerVariablesTachesActivesJourIndirect(
                    model, contexte, jour, bornes);

                if (tachesActivesCeJour.Count > 0)
                {
                    model.AddMaxEquality(jourActif, tachesActivesCeJour);
                }
                else
                {
                    model.Add(jourActif == 0);
                }

                coutsIndirects.Add(jourActif * contexte.CoutIndirectParJour);
            }

            return coutsIndirects;
        }

        private List<BoolVar> CreerVariablesTachesActivesJourIndirect(
            CpModel model,
            ContexteOptimisation contexte,
            LocalDate jour,
            (int start, int end) bornes)
        {
            var tachesActivesCeJour = new List<BoolVar>();

            foreach (var (tacheId, interval) in contexte.TachesIntervals)
            {
                var tacheActive = model.NewBoolVar($"tache_active_ce_jour_{tacheId.Value}_{jour:yyyyMMdd}");

                // CORRECTION: Utiliser la même logique que l'original - ne pas simplifier
                var startOk = model.NewBoolVar($"start_ok_indirect_{tacheId.Value}_j{jour:yyyyMMdd}");
                var endOk = model.NewBoolVar($"end_ok_indirect_{tacheId.Value}_j{jour:yyyyMMdd}");

                model.Add(interval.StartExpr() < bornes.end).OnlyEnforceIf(startOk);
                model.Add(interval.StartExpr() >= bornes.end).OnlyEnforceIf(startOk.Not());

                model.Add(interval.EndExpr() > bornes.start).OnlyEnforceIf(endOk);
                model.Add(interval.EndExpr() <= bornes.start).OnlyEnforceIf(endOk.Not());

                model.AddBoolAnd(new[] { startOk, endOk }).OnlyEnforceIf(tacheActive);
                tachesActivesCeJour.Add(tacheActive);
            }

            return tachesActivesCeJour;
        }

        // Classe pour encapsuler le contexte d'optimisation
        private class ContexteOptimisation
        {
            public Chantier Chantier { get; set; }
            public LocalDate[] JoursDuPlanning { get; set; }
            public Dictionary<LocalDate, SlotTemporel[]> SlotsByDay { get; set; }
            public Tache[] TachesReelles { get; set; }
            public Ouvrier[] OuvriersReels { get; set; }
            public IReadOnlyDictionary<TacheId, IntervalVar> TachesIntervals { get; set; }
            public long CoutIndirectParJour { get; set; }
        }
    }
}