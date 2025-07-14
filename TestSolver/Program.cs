// Fichier : Program.cs (Version corrigée)
using Google.OrTools.Sat;

// --- DÉBUT : Données du Scénario (copiées ici pour l'isolation) ---
public record Tache(string Id, int Duree, string MetierId, List<string> Dependencies);
public record Ouvrier(string Id, int CoutJournalier, List<string> Competences);

public static class Scenario
{
    public static List<Tache> GetTaches() => new()
    {
        new Tache("TACHE_PLACO_1", 8, "PLAQUISTE", new List<string>()),
        new Tache("TACHE_PEINTURE_1", 7, "PEINTRE", new List<string> { "TACHE_PLACO_1" }),
        new Tache("TACHE_PEINTURE_2", 7, "PEINTRE", new List<string>()),
        new Tache("TACHE_PEINTURE_3", 7, "PEINTRE", new List<string>()),
        new Tache("TACHE_PEINTURE_4", 7, "PEINTRE", new List<string>()),
        new Tache("TACHE_PEINTURE_5", 7, "PEINTRE", new List<string>()),
        new Tache("TACHE_PEINTURE_6", 7, "PEINTRE", new List<string>()),
        new Tache("TACHE_PEINTURE_7", 7, "PEINTRE", new List<string>()),
        new Tache("TACHE_PEINTURE_8", 3, "PEINTRE", new List<string>()),
        new Tache("TACHE_PEINTURE_9", 3, "PEINTRE", new List<string>())
    };

    public static List<Ouvrier> GetOuvriers() => new()
    {
        new Ouvrier("OUV_PLAQUISTE_1", 300, new List<string> { "PLAQUISTE" }),
        new Ouvrier("OUV_PEINTRE_1", 250, new List<string> { "PEINTRE" }),
        new Ouvrier("OUV_PEINTRE_2", 300, new List<string> { "PEINTRE" })
    };
}
// --- FIN : Données du Scénario ---

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- DÉBUT TEST BASÉ SUR LE NOUVEAU PRINCIPE (VERSION CORRIGÉE) ---");
        var model = new CpModel();
        var taches = Scenario.GetTaches();
        var ouvriers = Scenario.GetOuvriers();
        int heuresParJour = 8;
        long horizonSlots = 20 * heuresParJour;
        int nbJours = 20;

        Console.WriteLine($"Horizon: {horizonSlots} heures, {nbJours} jours");
        Console.WriteLine($"Nombre de tâches: {taches.Count}");
        Console.WriteLine($"Nombre d'ouvriers: {ouvriers.Count}");

        // --- 1. Variables de Décision du Planning ---
        var tachesIntervals = new Dictionary<string, IntervalVar>();
        var tachesAssignables = new Dictionary<(string, string), BoolVar>();

        foreach (var tache in taches)
        {
            var startVar = model.NewIntVar(0, horizonSlots - tache.Duree, $"start_{tache.Id}");
            var endVar = model.NewIntVar(tache.Duree, horizonSlots, $"end_{tache.Id}");
            var intervalle = model.NewIntervalVar(startVar, model.NewConstant(tache.Duree), endVar, $"interval_{tache.Id}");
            tachesIntervals.Add(tache.Id, intervalle);

            var assignationsPourTache = new List<BoolVar>();
            foreach (var ouvrier in ouvriers)
            {
                var estAssignable = model.NewBoolVar($"assign_{tache.Id}_to_{ouvrier.Id}");
                tachesAssignables.Add((tache.Id, ouvrier.Id), estAssignable);

                // CONTRAINTE DE COMPÉTENCE : L'ouvrier doit avoir la compétence requise
                if (!ouvrier.Competences.Contains(tache.MetierId))
                {
                    model.Add(estAssignable == 0); // Force à false si pas de compétence
                }

                assignationsPourTache.Add(estAssignable);
            }
            model.AddExactlyOne(assignationsPourTache);
        }

        // --- 2. Contraintes de dépendances ---
        foreach (var tache in taches)
        {
            foreach (var dependencyId in tache.Dependencies)
            {
                if (tachesIntervals.ContainsKey(dependencyId))
                {
                    // La tâche dépendante ne peut commencer qu'après la fin de la dépendance
                    model.Add(tachesIntervals[tache.Id].StartExpr() >= tachesIntervals[dependencyId].EndExpr());
                }
            }
        }

        // --- 3. Contraintes de non-chevauchement pour chaque ouvrier ---
        for (int o = 0; o < ouvriers.Count; o++)
        {
            var ouvrier = ouvriers[o];
            var intervalsOuvrier = new List<IntervalVar>();

            foreach (var tache in taches)
            {
                var assignVar = tachesAssignables[(tache.Id, ouvrier.Id)];
                var interval = tachesIntervals[tache.Id];

                // Créer un intervalle optionnel pour cet ouvrier
                var intervalOptional = model.NewOptionalIntervalVar(
                    interval.StartExpr(),
                    interval.SizeExpr(),
                    interval.EndExpr(),
                    assignVar,
                    $"optional_{tache.Id}_{ouvrier.Id}"
                );
                intervalsOuvrier.Add(intervalOptional);
            }

            // Un ouvrier ne peut pas faire deux tâches en même temps
            model.AddNoOverlap(intervalsOuvrier);
        }

        // --- 4. Variable de Coût : L'Ouvrier travaille-t-il ce jour ? ---
        var ouvrierTravailleLeJour = new BoolVar[ouvriers.Count, nbJours];
        for (int o = 0; o < ouvriers.Count; o++)
        {
            for (int j = 0; j < nbJours; j++)
            {
                ouvrierTravailleLeJour[o, j] = model.NewBoolVar($"travail_o{o}_j{j}");
            }
        }

        // --- 5. Lier le Planning au Coût (VERSION CORRIGÉE) ---
        for (int o = 0; o < ouvriers.Count; o++)
        {
            var ouvrier = ouvriers[o];
            for (int j = 0; j < nbJours; j++)
            {
                var jourStart = j * heuresParJour;
                var jourEnd = (j + 1) * heuresParJour;

                var tachesActivesCeJour = new List<BoolVar>();

                foreach (var tache in taches)
                {
                    var assignVar = tachesAssignables[(tache.Id, ouvrier.Id)];
                    var interval = tachesIntervals[tache.Id];

                    // Une tâche est active ce jour si :
                    // - Elle est assignée à cet ouvrier
                    // - Elle chevauche avec ce jour (start < jourEnd ET end > jourStart)
                    var tacheActiveCeJour = model.NewBoolVar($"active_{tache.Id}_o{o}_j{j}");

                    // Contraintes pour déterminer si la tâche chevauche avec le jour
                    var startAvantFinJour = model.NewBoolVar($"start_avant_fin_j_{tache.Id}_o{o}_j{j}");
                    var finApresDebutJour = model.NewBoolVar($"fin_apres_debut_j_{tache.Id}_o{o}_j{j}");

                    model.Add(interval.StartExpr() < jourEnd).OnlyEnforceIf(startAvantFinJour);
                    model.Add(interval.StartExpr() >= jourEnd).OnlyEnforceIf(startAvantFinJour.Not());

                    model.Add(interval.EndExpr() > jourStart).OnlyEnforceIf(finApresDebutJour);
                    model.Add(interval.EndExpr() <= jourStart).OnlyEnforceIf(finApresDebutJour.Not());

                    // La tâche est active ce jour si elle est assignée ET chevauche
                    model.AddBoolAnd(new[] { assignVar, startAvantFinJour, finApresDebutJour })
                          .OnlyEnforceIf(tacheActiveCeJour);
                    model.AddBoolOr(new[] { assignVar.Not(), startAvantFinJour.Not(), finApresDebutJour.Not() })
                          .OnlyEnforceIf(tacheActiveCeJour.Not());

                    tachesActivesCeJour.Add(tacheActiveCeJour);
                }

                // L'ouvrier travaille ce jour si au moins une tâche est active
                if (tachesActivesCeJour.Count > 0)
                {
                    // Si une tâche est active, l'ouvrier travaille
                    foreach (var tacheActive in tachesActivesCeJour)
                    {
                        model.AddImplication(tacheActive, ouvrierTravailleLeJour[o, j]);
                    }

                    // Si l'ouvrier travaille, au moins une tâche doit être active
                    var implicationLiterals = new List<ILiteral>(tachesActivesCeJour.Cast<ILiteral>());
                    implicationLiterals.Add(ouvrierTravailleLeJour[o, j].Not());
                    model.AddBoolOr(implicationLiterals);
                }
                else
                {
                    // Aucune tâche possible pour cet ouvrier
                    model.Add(ouvrierTravailleLeJour[o, j] == 0);
                }
            }
        }

        // --- 6. Objectif ---
        var coutTotal = model.NewIntVar(0, 100000 * 100, "cout_total");
        var coutsJournaliers = new List<LinearExpr>();
        for (int o = 0; o < ouvriers.Count; o++)
        {
            for (int j = 0; j < nbJours; j++)
            {
                coutsJournaliers.Add(ouvrierTravailleLeJour[o, j] * (ouvriers[o].CoutJournalier * 100));
            }
        }
        model.Add(coutTotal == LinearExpr.Sum(coutsJournaliers));
        model.Minimize(coutTotal);

        // --- 7. Résolution ---
        var solver = new CpSolver();
        solver.StringParameters = "max_time_in_seconds:30.0";
        var status = solver.Solve(model);

        // --- 8. Affichage des résultats ---
        Console.WriteLine($"Statut de la résolution: {status}");
        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            Console.WriteLine($"\nCoût Total Calculé: {solver.Value(coutTotal) / 100.0m:C}");

            // Affichage détaillé des assignations
            Console.WriteLine("\nDétail des assignations :");
            foreach (var tache in taches)
            {
                foreach (var ouvrier in ouvriers)
                {
                    if (tachesAssignables.TryGetValue((tache.Id, ouvrier.Id), out var assignVar))
                    {
                        if (solver.BooleanValue(assignVar))
                        {
                            var interval = tachesIntervals[tache.Id];
                            var startHeure = solver.Value(interval.StartExpr());
                            var endHeure = solver.Value(interval.EndExpr());
                            var startJour = startHeure / heuresParJour;
                            var endJour = (endHeure - 1) / heuresParJour;

                            Console.WriteLine(
                                $"  - Tâche '{tache.Id}' (Durée: {tache.Duree}h, Métier: {tache.MetierId}) " +
                                $"assignée à '{ouvrier.Id}' (Coût/jour: {ouvrier.CoutJournalier}€) " +
                                $"| Début: {startHeure}h (J{startJour}), Fin: {endHeure}h (J{endJour})"
                            );
                        }
                    }
                }
            }

            // Affichage des jours de travail
            Console.WriteLine("\nJours de travail par ouvrier :");
            for (int o = 0; o < ouvriers.Count; o++)
            {
                var ouvrier = ouvriers[o];
                var joursTravailes = new List<int>();
                for (int j = 0; j < nbJours; j++)
                {
                    if (solver.BooleanValue(ouvrierTravailleLeJour[o, j]))
                    {
                        joursTravailes.Add(j);
                    }
                }
                Console.WriteLine($"  - {ouvrier.Id}: {joursTravailes.Count} jours (J{string.Join(", J", joursTravailes)}) = {joursTravailes.Count * ouvrier.CoutJournalier}€");
            }
        }
        else
        {
            Console.WriteLine("Aucune solution trouvée.");
        }

        Console.WriteLine("--- FIN DU TEST ---");
    }
}