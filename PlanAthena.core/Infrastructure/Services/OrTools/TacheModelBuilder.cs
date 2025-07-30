// PlanAthena.Core.Infrastructure.Services.OrTools/TacheModelBuilder.cs

using Google.OrTools.Sat;
using NodaTime;
using NodaTime.Extensions;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class TacheModelBuilder
    {
        private Dictionary<TacheId, IntVar> _jalonStartAbsolu = new();
        private Dictionary<TacheId, IntVar> _jalonEndAbsolu = new();

        // Collections pour préserver les métadonnées des tâches.
        // Ces dictionnaires conservent les informations originales des tâches
        // pendant la construction du modèle OR-Tools, pour l'interprétation future de la solution.
        private Dictionary<TacheId, double> _dureesOriginales = new();
        private Dictionary<TacheId, TypeActivite> _typesActivites = new();
        private Dictionary<TacheId, string> _nomsActivites = new();

        // Variables pour stocker les bornes temporelles des lots et des groupes de priorité.
        // Ces dictionnaires permettront de créer et de référencer les variables CP-SAT
        // représentant le début et la fin agrégée de chaque lot et de chaque niveau de priorité.
        private Dictionary<LotId, IntVar> _lotStarts = new();
        private Dictionary<LotId, IntVar> _lotEnds = new();
        private Dictionary<int, IntVar> _priorityGroupStarts = new();
        private Dictionary<int, IntVar> _priorityGroupEnds = new();


        // La méthode Construire est le point d'entrée pour la création des variables et contraintes
        // du modèle CP-SAT spécifiques aux tâches, ressources et précédences.
        // Elle retourne toutes les variables de décision clés et les métadonnées nécessaires
        // pour la suite du processus d'optimisation.
        public (
            Dictionary<TacheId, IntervalVar> TachesIntervals,
            Dictionary<(TacheId, OuvrierId), BoolVar> TachesAssignables,
            IntVar Makespan,
            Dictionary<TacheId, double> DureesOriginales,
            Dictionary<TacheId, TypeActivite> TypesActivites,
            Dictionary<TacheId, string> NomsActivites,
            IReadOnlyDictionary<LotId, IntVar> LotStarts,           // Débuts des lots
            IReadOnlyDictionary<LotId, IntVar> LotEnds,             // Fins des lots
            IReadOnlyDictionary<int, IntVar> PriorityGroupStarts,   // Débuts des groupes de priorité
            IReadOnlyDictionary<int, IntVar> PriorityGroupEnds      // Fins des groupes de priorité
        ) Construire(CpModel model, ProblemeOptimisation probleme)
        {
            var chantier = probleme.Chantier;
            var tachesAssignables = new Dictionary<(TacheId, OuvrierId), BoolVar>();
            var tachesIntervals = new Dictionary<TacheId, IntervalVar>();

            // Nettoyage des collections privées à chaque appel pour assurer un état propre.
            _jalonStartAbsolu.Clear();
            _jalonEndAbsolu.Clear();
            _dureesOriginales.Clear();
            _typesActivites.Clear();
            _nomsActivites.Clear();
            _lotStarts.Clear();
            _lotEnds.Clear();
            _priorityGroupStarts.Clear();
            _priorityGroupEnds.Clear();

            // Crée une "passerelle" entre les indices de slots temporels (utilisés par CP-SAT)
            // et les heures absolues (pour les calculs de durée réels et les jalons).
            var mapSlotToHeureAbsolue = CreerPontTemporel(probleme.EchelleTemps);

            // Création des variables de décision pour chaque tâche et ses potentielles assignations.
            CreerVariablesDeDecision(model, probleme, tachesAssignables, tachesIntervals, mapSlotToHeureAbsolue);
            // Ajout des contraintes assurant que chaque tâche est assignée à exactement un ouvrier compétent.
            AjouterContraintesAssignationUnique(model, chantier, tachesAssignables);
            // Ajout des contraintes de ressources, empêchant les chevauchements pour chaque ouvrier.
            AjouterContraintesRessources(model, chantier, tachesIntervals, tachesAssignables);
            // Ajout des contraintes de précédence entre tâches individuelles (s'il y en a).
            AjouterContraintesDePrecedence(model, chantier, tachesIntervals, mapSlotToHeureAbsolue);

            // AJOUT : Ajout des contraintes de précédence strictes basées sur les niveaux de priorité des lots.
            AjouterContraintesDePrecedenceParPriorite(model, probleme, tachesIntervals);

            // Calcul de la borne supérieure pour le makespan (durée totale du projet).
            // Le makespan est la variable représentant la fin de la dernière tâche du projet.
            var maxEndTime = CalculerBorneSuperieureMakespan(probleme);
            var makespan = model.NewIntVar(0, maxEndTime, "makespan");

            // La variable makespan est définie comme le maximum des fins de toutes les tâches.
            if (tachesIntervals.Any())
            {
                model.AddMaxEquality(makespan, tachesIntervals.Values.Select(v => v.EndExpr()));
            }
            else
            {
                // Si aucune tâche, le makespan est 0.
                model.Add(makespan == 0);
            }

            // Retourne toutes les variables et dictionnaires nécessaires pour l'interpréteur de solution et l'objectif.
            return (tachesIntervals, tachesAssignables, makespan,
                    _dureesOriginales, _typesActivites, _nomsActivites,
                    _lotStarts, _lotEnds, _priorityGroupStarts, _priorityGroupEnds);
        }

        // Crée une correspondance entre les indices de "slots temporels" (unités de temps utilisées par le solveur)
        // et les heures absolues depuis le début du calendrier ouvré.
        // Cela permet de convertir les temps du solveur en heures réelles pour les calculs de durée.
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

        // Calcule la borne supérieure maximale possible pour le makespan du projet.
        // Cette borne est généralement basée sur l'étendue totale de la période de planification.
        private long CalculerBorneSuperieureMakespan(ProblemeOptimisation probleme)
        {
            // La seule borne supérieure vraiment sûre est la fin de la fenêtre de planification.
            // Toute autre estimation (séquentielle, parallèle) peut être trop restrictive
            // à cause des contraintes de calendrier (jours non ouvrés, contrainte NoSplitOverDays).
            return probleme.EchelleTemps.NombreTotalSlots;
        }

        // Crée les variables de décision CP-SAT pour chaque tâche.
        // Cela inclut les variables d'intervalle pour les tâches classiques et des variables
        // de début/fin absolues pour les jalons, ainsi que les variables booléennes
        // pour l'assignation tâche-ouvrier.
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

            // Pré-regroupement des ouvriers par métier pour un accès plus rapide.
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
                // Enregistrement systématique des métadonnées originales de la tâche.
                // Ces informations sont conservées pour l'interprétation de la solution,
                // notamment pour distinguer les tâches des jalons et afficher les durées réelles.
                _dureesOriginales[tache.Id] = tache.HeuresHommeEstimees.Value;
                _typesActivites[tache.Id] = tache.Type;
                _nomsActivites[tache.Id] = tache.Nom;

                if (tache.Type == TypeActivite.Tache)
                {
                    // Pour une tâche classique, une IntervalVar est créée avec une durée fixe.
                    var dureeEnSlots = (long)tache.HeuresHommeEstimees.Value;
                    var maxStart = horizonEnSlots > dureeEnSlots ? horizonEnSlots - dureeEnSlots : 0;
                    var startVar = model.NewIntVar(0, maxStart, $"start_{tache.Id.Value}");
                    var endVar = model.NewIntVar(dureeEnSlots, horizonEnSlots, $"end_{tache.Id.Value}");
                    var sizeVar = model.NewConstant(dureeEnSlots); // Duree fixe pour la tache.
                    var intervalle = model.NewIntervalVar(startVar, sizeVar, endVar, $"interval_{tache.Id.Value}");
                    tachesIntervals.Add(tache.Id, intervalle);

                    // Contrainte pour s'assurer qu'une tâche ne chevauche pas les limites des jours ouvrés.
                    // Si une tâche dure moins qu'une journée de travail, elle doit tenir dans les heures ouvrées d'une journée.
                    if (dureeEnSlots > 0 && dureeEnSlots <= heuresParJour && heuresParJour > 1)
                    {
                        var startDansJour = model.NewIntVar(0, heuresParJour - 1, $"start_dans_jour_{tache.Id.Value}");
                        model.AddModuloEquality(startDansJour, startVar, heuresParJour);
                        model.Add(startDansJour + dureeEnSlots <= heuresParJour);
                    }
                }
                else
                {
                    // Pour les jalons, ils sont modélisés comme des points dans le temps ou des intervalles de très courte durée.
                    // Des variables absolues en heures sont utilisées pour les jalons, puis converties en slots.
                    var dureeEnHeures = (long)tache.HeuresHommeEstimees.Value; // Généralement 0 pour un jalon pur
                    var startAbsolu = model.NewIntVar(0, horizonEnHeures, $"start_abs_{tache.Id.Value}");
                    var endAbsolu = model.NewIntVar(0, horizonEnHeures, $"end_abs_{tache.Id.Value}");
                    model.Add(endAbsolu == startAbsolu + dureeEnHeures);

                    _jalonStartAbsolu[tache.Id] = startAbsolu;
                    _jalonEndAbsolu[tache.Id] = endAbsolu;

                    // Les jalons sont représentés par un intervalle de taille 1 slot pour les besoins du solveur,
                    // et sont liés à leur position absolue en heures.
                    var startSlot = model.NewIntVar(0, horizonEnSlots > 0 ? horizonEnSlots - 1 : 0, $"start_slot_{tache.Id.Value}");
                    model.AddElement(startSlot, mapSlotToHeureAbsolue, startAbsolu);

                    var endSlot = model.NewIntVar(1, horizonEnSlots, $"end_point_{tache.Id.Value}");
                    var intervalle = model.NewIntervalVar(startSlot, 1, endSlot, $"interval_{tache.Id.Value}");
                    tachesIntervals.Add(tache.Id, intervalle);
                }

                // Création des variables booléennes d'assignation tâche-ouvrier.
                // Chaque tâche doit être assignée à un ouvrier compétent.
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

        // Ajoute la contrainte qu'exactement un ouvrier doit être assigné à chaque tâche.
        private void AjouterContraintesAssignationUnique(CpModel model, Chantier chantier, IReadOnlyDictionary<(TacheId, OuvrierId), BoolVar> tachesAssignables)
        {
            var assignationsParTache = tachesAssignables.GroupBy(kvp => kvp.Key.Item1).ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Value).ToList());
            foreach (var tache in chantier.ObtenirToutesLesTaches())
            {
                if (assignationsParTache.TryGetValue(tache.Id, out var candidats) && candidats.Any())
                {
                    // Exactement un des BoolVar (représentant l'assignation d'un ouvrier) doit être vrai pour cette tâche.
                    model.AddExactlyOne(candidats);
                }
                else
                {
                    // Ce cas indique un problème de données d'entrée ou de logique de mapping.
                    throw new InvalidOperationException($"Aucun candidat d'assignation trouvé pour la tâche {tache.Id.Value} ({tache.Nom}).");
                }
            }
        }

        // Ajoute les contraintes de non-chevauchement pour chaque ouvrier.
        // Un ouvrier ne peut pas travailler sur deux tâches en même temps.
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
                        // Crée un IntervalVar "optionnel" : il est activé si l'ouvrier est assigné à la tâche.
                        var intervalleBase = tachesIntervals[tacheId];
                        var intervalleOptionnel = model.NewOptionalIntervalVar(intervalleBase.StartExpr(), intervalleBase.SizeExpr(), intervalleBase.EndExpr(), estAssignable, $"optionnel_{tacheId.Value}_a_{ouvrier.Id.Value}");
                        intervallesPourOuvrier.Add(intervalleOptionnel);
                    }
                }
                // Si l'ouvrier a plusieurs tâches potentielles, s'assurer qu'elles ne se chevauchent pas si elles sont assignées.
                if (intervallesPourOuvrier.Count > 1) model.AddNoOverlap(intervallesPourOuvrier);
            }
        }

        // Ajoute les contraintes de précédence entre tâches individuelles (Tache A doit finir avant Tache B commence).
        // Gère les dépendances classiques entre tâches et les dépendances impliquant des jalons.
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
                        // Contrainte classique : Tâche actuelle démarre après la fin de sa dépendance de type Tâche.
                        var intervalleDependance = tachesIntervals[depId];
                        model.Add(intervalleTacheActuelle.StartExpr() >= intervalleDependance.EndExpr());
                    }
                    else
                    {
                        // Pour les dépendances avec un jalon : la tâche actuelle démarre après la fin absolue du jalon.
                        var startSlotTacheActuelle = intervalleTacheActuelle.StartExpr();
                        var startAbsoluTacheActuelle = model.NewIntVar(0, horizonEnHeures, $"dep_start_abs_{tacheId.Value}");

                        // Lier le début en slots de la tâche au début absolu en heures.
                        model.AddElement(startSlotTacheActuelle, mapSlotToHeureAbsolue, startAbsoluTacheActuelle);

                        var endAbsoluJalon = _jalonEndAbsolu[depId];

                        model.Add(startAbsoluTacheActuelle >= endAbsoluJalon);
                    }
                }
            }
        }

        // AJOUT : Nouvelle méthode pour ajouter les contraintes de précédence basées sur les niveaux de priorité des lots.
        // Cette méthode assure que tous les lots d'un niveau de priorité N doivent se terminer
        // avant que n'importe quel lot d'un niveau de priorité N+1 (ou supérieur) ne puisse commencer.
        private void AjouterContraintesDePrecedenceParPriorite(
            CpModel model,
            ProblemeOptimisation probleme,
            IReadOnlyDictionary<TacheId, IntervalVar> tachesIntervals)
        {
            var chantier = probleme.Chantier;
            long horizonEnSlots = probleme.EchelleTemps.NombreTotalSlots;

            // PARTIE 1 : Calculer le début et la fin pour chaque lot individuel.
            // Ces variables agrégées pour chaque lot sont nécessaires pour ensuite
            // calculer les débuts et fins des groupes de priorité.
            foreach (var lot in chantier.Lots.Values)
            {
                var tasksInLotIntervals = new List<IntervalVar>();
                // Rassembler toutes les IntervalVar des tâches appartenant à ce lot.
                // Cela implique de parcourir les blocs pour identifier leurs tâches, puis vérifier l'appartenance au lot.
                foreach (var bloc in chantier.Blocs.Values.Where(b => b.LotParentId == lot.Id))
                {
                    foreach (var tacheId in bloc.Taches.Keys)
                    {
                        if (tachesIntervals.TryGetValue(tacheId, out var intervalVar))
                        {
                            tasksInLotIntervals.Add(intervalVar);
                        }
                    }
                }

                if (!tasksInLotIntervals.Any())
                {
                    // Si un lot ne contient aucune tâche (modélisée comme IntervalVar),
                    // ses variables de début/fin sont fixées à 0 pour ne pas bloquer le solveur.
                    var lotStartEmpty = model.NewConstant(0);
                    var lotEndEmpty = model.NewConstant(0);
                    _lotStarts.Add(lot.Id, lotStartEmpty);
                    _lotEnds.Add(lot.Id, lotEndEmpty);
                    Console.WriteLine($"[WARNING] Le lot '{lot.Nom}' (ID: {lot.Id.Value}) n'a aucune tâche active modelisée. Ses début/fin sont fixés à 0.");
                    continue; // Passe au lot suivant
                }

                // Le début d'un lot est le moment de début de sa première tâche.
                var lotStart = model.NewIntVar(0, horizonEnSlots, $"lot_start_{lot.Id.Value}");
                model.AddMinEquality(lotStart, tasksInLotIntervals.Select(t => t.StartExpr()));
                _lotStarts.Add(lot.Id, lotStart);

                // La fin d'un lot est le moment de fin de sa dernière tâche.
                var lotEnd = model.NewIntVar(0, horizonEnSlots + 1, $"lot_end_{lot.Id.Value}"); // +1 car la fin peut être à l'horizon max + 1 (pour la durée)
                model.AddMaxEquality(lotEnd, tasksInLotIntervals.Select(t => t.EndExpr()));
                _lotEnds.Add(lot.Id, lotEnd);
            }

            // PARTIE 2 : Regrouper les lots par niveau de priorité et calculer le début/fin de chaque groupe.
            // Trie les niveaux de priorité pour garantir un traitement séquentiel (priorité 10 avant 20, etc.).
            var distinctPriorities = chantier.Lots.Values
                                            .Select(l => l.Priorite)
                                            .Distinct()
                                            .OrderBy(p => p)
                                            .ToList();

            IntVar? previousPriorityGroupEnd = null; // Utilisé pour chaîner les contraintes de précédence entre les groupes.

            foreach (var currentPriority in distinctPriorities)
            {
                // Sélectionne tous les lots appartenant au niveau de priorité actuel.
                var lotsInCurrentPriorityGroup = chantier.Lots.Values
                                                        .Where(l => l.Priorite == currentPriority)
                                                        .ToList();

                // Collection des variables de début et fin des lots de ce groupe de priorité.
                var startsOfLotsInGroup = new List<IntVar>();
                var endsOfLotsInGroup = new List<IntVar>();

                foreach (var lot in lotsInCurrentPriorityGroup)
                {
                    // Récupère les variables de début/fin de lot précédemment créées.
                    if (_lotStarts.TryGetValue(lot.Id, out var lotStart) && _lotEnds.TryGetValue(lot.Id, out var lotEnd))
                    {
                        startsOfLotsInGroup.Add(lotStart);
                        endsOfLotsInGroup.Add(lotEnd);
                    }
                    else
                    {
                        Console.WriteLine($"[WARNING] Variables de début/fin non trouvées pour le lot '{lot.Nom}' (ID: {lot.Id.Value}) dans le groupe de priorité {currentPriority}. Ce lot pourrait ne pas contenir de tâches modélisées.");
                    }
                }

                if (!startsOfLotsInGroup.Any())
                {
                    // Si le groupe de priorité est vide de tâches modélisées, il ne contraint pas le temps.
                    var priorityGroupStartEmpty = model.NewConstant(0);
                    var priorityGroupEndEmpty = model.NewConstant(0);
                    _priorityGroupStarts.Add(currentPriority, priorityGroupStartEmpty);
                    _priorityGroupEnds.Add(currentPriority, priorityGroupEndEmpty);
                    Console.WriteLine($"[WARNING] Le groupe de priorité {currentPriority} n'a aucune tâche active modélisée. Ses début/fin sont fixés à 0.");
                    previousPriorityGroupEnd = priorityGroupEndEmpty; // Mettre à jour pour le chaînage
                    continue; // Passe au groupe de priorité suivant
                }

                // Le début du groupe de priorité est le minimum des débuts de tous les lots qu'il contient.
                var currentPriorityGroupStart = model.NewIntVar(0, horizonEnSlots, $"priority_group_start_P{currentPriority}");
                model.AddMinEquality(currentPriorityGroupStart, startsOfLotsInGroup);
                _priorityGroupStarts.Add(currentPriority, currentPriorityGroupStart);

                // La fin du groupe de priorité est le maximum des fins de tous les lots qu'il contient.
                var currentPriorityGroupEnd = model.NewIntVar(0, horizonEnSlots + 1, $"priority_group_end_P{currentPriority}");
                model.AddMaxEquality(currentPriorityGroupEnd, endsOfLotsInGroup);
                _priorityGroupEnds.Add(currentPriority, currentPriorityGroupEnd);

                // PARTIE 3 : Appliquer la contrainte de précédence entre les groupes de priorité.
                if (previousPriorityGroupEnd != null)
                {
                    // La première tâche du groupe de priorité actuel ne peut pas commencer
                    // avant que la dernière tâche du groupe de priorité précédent ne soit terminée.
                    model.Add(currentPriorityGroupStart >= previousPriorityGroupEnd);
                    Console.WriteLine($"[DEBUG] Ajout de la contrainte: Groupe de priorité {currentPriority} (début) >= Groupe de priorité précédent (fin).");
                }

                // Met à jour la variable pour le prochain groupe de priorité dans la séquence.
                previousPriorityGroupEnd = currentPriorityGroupEnd;
            }
        }
    }
}