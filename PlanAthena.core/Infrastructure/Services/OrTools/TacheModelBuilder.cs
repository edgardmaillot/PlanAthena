// PlanAthena.Core.Infrastructure.Services.OrTools/TacheModelBuilder.cs

using Google.OrTools.Sat;
using NodaTime;
using NodaTime.Extensions;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class TacheModelBuilder
    {
        private Dictionary<TacheId, IntVar> _jalonStartAbsolu = new();
        private Dictionary<TacheId, IntVar> _jalonEndAbsolu = new();

        public (
            Dictionary<TacheId, IntervalVar> TachesIntervals,
            Dictionary<(TacheId, OuvrierId), BoolVar> TachesAssignables,
            IntVar Makespan
        ) Construire(CpModel model, ProblemeOptimisation probleme)
        {
            var chantier = probleme.Chantier;
            var tachesAssignables = new Dictionary<(TacheId, OuvrierId), BoolVar>();
            var tachesIntervals = new Dictionary<TacheId, IntervalVar>();

            _jalonStartAbsolu.Clear();
            _jalonEndAbsolu.Clear();

            var mapSlotToHeureAbsolue = CreerPontTemporel(probleme.EchelleTemps);

            CreerVariablesDeDecision(model, probleme, tachesAssignables, tachesIntervals, mapSlotToHeureAbsolue);
            AjouterContraintesAssignationUnique(model, chantier, tachesAssignables);
            AjouterContraintesRessources(model, chantier, tachesIntervals, tachesAssignables);

            AjouterContraintesDePrecedence(model, chantier, tachesIntervals, mapSlotToHeureAbsolue);

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

        private long[] CreerPontTemporel(EchelleTempsOuvree echelleTemps)
        {
            if (echelleTemps.PremierSlot == null) return Array.Empty<long>();
            var premierSlotDebut = echelleTemps.PremierSlot.Debut;
            var map = new long[echelleTemps.NombreTotalSlots];
            foreach (var slot in echelleTemps.Slots)
            {
                var dureeDepuisDebut = Period.Between(premierSlotDebut, slot.Debut, PeriodUnits.Hours).ToDuration();
                map[slot.Index] = (long)Math.Round(dureeDepuisDebut.TotalHours);
            }
            return map;
        }

        private long CalculerBorneSuperieureMakespan(ProblemeOptimisation probleme)
        {
            var chantier = probleme.Chantier;
            var totalHeuresHommeReelles = chantier.ObtenirToutesLesTaches().Where(t => t.Type == TypeActivite.Tache).Sum(t => (long)t.HeuresHommeEstimees.Value);
            var nombreOuvriers = chantier.Ouvriers.Values.Count(o => !o.Id.Value.StartsWith("VIRTUAL"));
            var estimationSequentielle = totalHeuresHommeReelles;
            var estimationParallele = nombreOuvriers > 0 ? totalHeuresHommeReelles / nombreOuvriers : totalHeuresHommeReelles;
            var horizonComplet = probleme.EchelleTemps.NombreTotalSlots;
            return Math.Min(estimationSequentielle, horizonComplet);
        }

        private void CreerVariablesDeDecision(
            CpModel model,
            ProblemeOptimisation probleme,
            Dictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables,
            Dictionary<TacheId, IntervalVar> tachesIntervals,
            long[] mapSlotToHeureAbsolue)
        {
            var chantier = probleme.Chantier;
            long horizonEnSlots = probleme.EchelleTemps.NombreTotalSlots;
            int heuresParJour = (int)chantier.Calendrier.DureeTravailEffectiveParJour.TotalHours;

            var ouvriersParMetier = chantier.Metiers.Keys.ToDictionary(
                metierId => metierId,
                metierId => chantier.Ouvriers.Values.Where(o => o.PossedeCompetence(metierId)).ToList()
            );

            long horizonEnHeures = 0;
            if (chantier.PeriodeSouhaitee.DateDebut.HasValue && chantier.PeriodeSouhaitee.DateFin.HasValue)
            {
                var debutProjet = chantier.PeriodeSouhaitee.DateDebut.Value.ToLocalDateTime();
                var finProjet = chantier.PeriodeSouhaitee.DateFin.Value.ToLocalDateTime();
                horizonEnHeures = (long)Period.Between(debutProjet, finProjet, PeriodUnits.Hours).ToDuration().TotalHours;
            }

            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                if (tache.Type == TypeActivite.Tache)
                {
                    var dureeEnSlots = (long)tache.HeuresHommeEstimees.Value;
                    var maxStart = horizonEnSlots > dureeEnSlots ? horizonEnSlots - dureeEnSlots : 0;
                    var startVar = model.NewIntVar(0, maxStart, $"start_{tache.Id.Value}");
                    var endVar = model.NewIntVar(dureeEnSlots, horizonEnSlots, $"end_{tache.Id.Value}");
                    var sizeVar = model.NewConstant(dureeEnSlots);
                    var intervalle = model.NewIntervalVar(startVar, sizeVar, endVar, $"interval_{tache.Id.Value}");
                    tachesIntervals.Add(tache.Id, intervalle);

                    if (dureeEnSlots > 0 && dureeEnSlots <= heuresParJour && heuresParJour > 1)
                    {
                        var startDansJour = model.NewIntVar(0, heuresParJour - 1, $"start_dans_jour_{tache.Id.Value}");
                        model.AddModuloEquality(startDansJour, startVar, heuresParJour);
                        model.Add(startDansJour + dureeEnSlots <= heuresParJour);
                    }
                }
                else
                {
                    var dureeEnHeures = (long)tache.HeuresHommeEstimees.Value;
                    var startAbsolu = model.NewIntVar(0, horizonEnHeures, $"start_abs_{tache.Id.Value}");
                    var endAbsolu = model.NewIntVar(0, horizonEnHeures, $"end_abs_{tache.Id.Value}");
                    model.Add(endAbsolu == startAbsolu + dureeEnHeures);

                    _jalonStartAbsolu[tache.Id] = startAbsolu;
                    _jalonEndAbsolu[tache.Id] = endAbsolu;

                    var startSlot = model.NewIntVar(0, horizonEnSlots > 0 ? horizonEnSlots - 1 : 0, $"start_slot_{tache.Id.Value}");
                    model.AddElement(startSlot, mapSlotToHeureAbsolue, startAbsolu);

                    var endSlot = model.NewIntVar(1, horizonEnSlots, $"end_point_{tache.Id.Value}");
                    var intervalle = model.NewIntervalVar(startSlot, 1, endSlot, $"interval_{tache.Id.Value}");
                    tachesIntervals.Add(tache.Id, intervalle);
                }

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

        private void AjouterContraintesAssignationUnique(CpModel model, Chantier chantier, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var assignationsParTache = tachesAssignables.GroupBy(kvp => kvp.Key.Item1).ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Value).ToList());
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

        private void AjouterContraintesRessources(CpModel model, Chantier chantier, IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var assignationsParOuvrier = tachesAssignables.GroupBy(kvp => kvp.Key.Item2).ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Key.Item1).ToList());
            foreach (var ouvrier in chantier.Ouvriers.Values)
            {
                if (!assignationsParOuvrier.TryGetValue(ouvrier.Id, out var tacheIds) || !tacheIds.Any()) continue;
                var intervallesPourOuvrier = new List<IntervalVar>();
                foreach (var tacheId in tacheIds)
                {
                    if (tachesAssignables.TryGetValue((tacheId, ouvrier.Id), out var estAssignable))
                    {
                        var intervalleBase = tachesIntervals[tacheId];
                        var intervalleOptionnel = model.NewOptionalIntervalVar(intervalleBase.StartExpr(), intervalleBase.SizeExpr(), intervalleBase.EndExpr(), estAssignable, $"optionnel_{tacheId.Value}_a_{ouvrier.Id.Value}");
                        intervallesPourOuvrier.Add(intervalleOptionnel);
                    }
                }
                if (intervallesPourOuvrier.Count > 1) model.AddNoOverlap(intervallesPourOuvrier);
            }
        }

        private void AjouterContraintesDePrecedence(
            CpModel model,
            Chantier chantier,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals,
            long[] mapSlotToHeureAbsolue)
        {
            var toutesLesTaches = chantier.ObtenirToutesLesTaches().ToDictionary(t => t.Id);

            long horizonEnHeures = 0;
            if (chantier.PeriodeSouhaitee.DateDebut.HasValue && chantier.PeriodeSouhaitee.DateFin.HasValue)
            {
                var debutProjet = chantier.PeriodeSouhaitee.DateDebut.Value.ToLocalDateTime();
                var finProjet = chantier.PeriodeSouhaitee.DateFin.Value.ToLocalDateTime();
                horizonEnHeures = (long)Period.Between(debutProjet, finProjet, PeriodUnits.Hours).ToDuration().TotalHours;
            }

            foreach (var (tacheId, tache) in toutesLesTaches)
            {
                if (tache.Dependencies?.Any() != true) continue;

                var intervalleTacheActuelle = tachesIntervals[tacheId];

                foreach (var depId in tache.Dependencies)
                {
                    if (!toutesLesTaches.TryGetValue(depId, out var tacheDependance)) continue;

                    if (tacheDependance.Type == TypeActivite.Tache)
                    {
                        var intervalleDependance = tachesIntervals[depId];
                        model.Add(intervalleTacheActuelle.StartExpr() >= intervalleDependance.EndExpr());
                    }
                    else
                    {
                        var startSlotTacheActuelle = intervalleTacheActuelle.StartExpr();
                        var startAbsoluTacheActuelle = model.NewIntVar(0, horizonEnHeures, $"dep_start_abs_{tacheId.Value}");

                        model.AddElement(startSlotTacheActuelle, mapSlotToHeureAbsolue, startAbsoluTacheActuelle);

                        var endAbsoluJalon = _jalonEndAbsolu[depId];

                        model.Add(startAbsoluTacheActuelle >= endAbsoluJalon);
                    }
                }
            }
        }
    }
}