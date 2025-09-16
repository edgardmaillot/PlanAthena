// Fichier: /Utilities/AnalysisService.cs Version 0.6.0

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Service utilitaire stateless pour calculer un ensemble complet d'indicateurs de performance 
    /// clés (KPIs) et de données d'analyse à partir d'un planning consolidé.
    /// Version 0.6.0 : Ajout des calculateurs pour la baseline EVM.
    /// </summary>
    public class AnalysisService
    {
        // Type alias pour améliorer la lisibilité de la signature de la méthode.
        public delegate int JoursOuvresCalculator(DateTime start, DateTime end);

        #region Points d'Entrée Publics - Analyse de Planning

        /// <summary>
        /// Génère un rapport d'analyse complet à partir d'un planning optimisé.
        /// </summary>
        public virtual AnalysisReport GenerateReport(
            ConsolidatedPlanning planning,
            IReadOnlyList<Ouvrier> allOuvriers,
            IReadOnlyList<Metier> allMetiers,
            ConfigurationPlanification config,
            JoursOuvresCalculator joursOuvresCalculator)
        {
            var syntheseProjet = _CalculerSyntheseProjet(planning, allOuvriers, config, joursOuvresCalculator);
            var analysesOuvriers = _CalculerAnalysesOuvriers(planning, allOuvriers, allMetiers, config, joursOuvresCalculator);
            var chargeJournaliere = _CalculerChargeJournaliere(planning);

            return new AnalysisReport
            {
                SyntheseProjet = syntheseProjet,
                AnalysesOuvriers = analysesOuvriers,
                ChargeJournaliere = chargeJournaliere
            };
        }

        /// <summary>
        /// Analyse la liste des IDs d'ouvriers clés retournée par l'analyse rapide pour en déduire la tension par métier.
        /// </summary>
        public virtual MetierTensionReport AnalyzeMetierTension(IReadOnlyList<string> ouvrierClesIds, IReadOnlyList<Ouvrier> allOuvriers)
        {
            if (ouvrierClesIds == null || !ouvrierClesIds.Any() || allOuvriers == null || !allOuvriers.Any())
            {
                return new MetierTensionReport { Conclusion = "Données insuffisantes pour l'analyse.", Repartition = new List<MetierCount>() };
            }

            var ouvriersById = allOuvriers.ToDictionary(o => o.OuvrierId);

            var metiersDesOuvriersCles = ouvrierClesIds
                .Select(id => ouvriersById.TryGetValue(id, out var ouvrier) ? ouvrier.MetierId : null)
                .Where(metierId => !string.IsNullOrEmpty(metierId))
                .ToList();

            var repartition = metiersDesOuvriersCles
                .GroupBy(metierId => metierId)
                .Select(group => new MetierCount { MetierId = group.Key, Count = group.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            string conclusion = "Équilibrage des compétences correct";
            if (repartition.Any())
            {
                var counts = repartition.Select(r => r.Count).ToList();
                if (counts[0] >= 3) conclusion = "Un métier en tension forte";
                else if (counts.Count > 1 && counts[0] == 2 && counts[1] == 2) conclusion = "Deux métiers en tension";
                else if (counts[0] == 2) conclusion = "Un métier en tension";
            }

            return new MetierTensionReport
            {
                Conclusion = conclusion,
                Repartition = repartition
            };
        }

        #endregion

        #region NOUVEAU : Points d'Entrée Publics - Calculateurs EVM pour Baseline

        /// <summary>
        /// Calcule le coût total du projet (Budget à l'Achèvement - BAC) pour un planning donné.
        /// </summary>
        public virtual decimal CalculerBudgetTotal(
            ConsolidatedPlanning planning,
            IReadOnlyList<Ouvrier> allOuvriers,
            ConfigurationPlanification config,
            JoursOuvresCalculator joursOuvresCalculator)
        {
            var synthese = _CalculerSyntheseProjet(planning, allOuvriers, config, joursOuvresCalculator);
            return synthese.CoutTotalProjet;
        }

        /// <summary>
        /// Calcule la courbe de la Valeur Planifiée (PV) cumulative pour un planning donné.
        /// </summary>
        public virtual Dictionary<DateTime, decimal> CalculerCourbePlannedValueCumulative(
            ConsolidatedPlanning planning,
            IReadOnlyList<Ouvrier> allOuvriers,
            ConfigurationPlanification config)
        {
            var mapOuvriers = allOuvriers.ToDictionary(o => o.OuvrierId);
            var heuresParJour = (decimal)config.HeuresTravailEffectifParJour;
            if (heuresParJour == 0) return new Dictionary<DateTime, decimal>();

            // 1. Calculer le coût journalier RH
            var coutsJournaliers = planning.SegmentsParOuvrierId.Values
                .SelectMany(segments => segments)
                .GroupBy(s => s.Jour.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(s => {
                        if (mapOuvriers.TryGetValue(s.OuvrierId, out var ouvrier))
                        {
                            return ((decimal)s.HeuresTravaillees / heuresParJour) * ouvrier.CoutJournalier;
                        }
                        return 0m;
                    })
                );

            // 2. Ajouter les coûts indirects
            if (planning.DateDebutProjet != default && planning.DateFinProjet != default)
            {
                var joursOuvres = config.JoursOuvres ?? new List<DayOfWeek>();
                for (var jour = planning.DateDebutProjet.Date; jour <= planning.DateFinProjet.Date; jour = jour.AddDays(1))
                {
                    if (joursOuvres.Contains(jour.DayOfWeek))
                    {
                        if (!coutsJournaliers.ContainsKey(jour))
                        {
                            coutsJournaliers[jour] = 0;
                        }
                        coutsJournaliers[jour] += config.CoutIndirectJournalierAbsolu;
                    }
                }
            }

            // 3. Créer la courbe cumulative
            var courbeCumulative = new Dictionary<DateTime, decimal>();
            decimal totalCumulatif = 0m;
            foreach (var kvp in coutsJournaliers.OrderBy(kv => kv.Key))
            {
                totalCumulatif += kvp.Value;
                courbeCumulative[kvp.Key] = totalCumulatif;
            }

            return courbeCumulative;
        }

        /// <summary>
        /// Calcule le budget RH initial pour chaque tâche mère d'un planning donné.
        /// </summary>
        public virtual Dictionary<string, decimal> CalculerBudgetParTache(
            ConsolidatedPlanning planning,
            IReadOnlyList<Ouvrier> allOuvriers,
            ConfigurationPlanification config)
        {
            var mapOuvriers = allOuvriers.ToDictionary(o => o.OuvrierId);
            var heuresParJour = (decimal)config.HeuresTravailEffectifParJour;
            if (heuresParJour == 0) return new Dictionary<string, decimal>();

            var budgetParTache = new Dictionary<string, decimal>();

            var allSegments = planning.SegmentsParOuvrierId.Values.SelectMany(s => s);

            foreach (var segment in allSegments)
            {
                var tacheMereId = segment.ParentTacheId ?? segment.TacheId;
                if (string.IsNullOrEmpty(tacheMereId)) continue;

                if (mapOuvriers.TryGetValue(segment.OuvrierId, out var ouvrier))
                {
                    decimal coutSegment = ((decimal)segment.HeuresTravaillees / heuresParJour) * ouvrier.CoutJournalier;
                    if (!budgetParTache.ContainsKey(tacheMereId))
                    {
                        budgetParTache[tacheMereId] = 0;
                    }
                    budgetParTache[tacheMereId] += coutSegment;
                }
            }

            return budgetParTache;
        }


        #endregion

        #region Méthodes de Calcul Privées

        private SyntheseProjetReport _CalculerSyntheseProjet(
            ConsolidatedPlanning planning,
            IReadOnlyList<Ouvrier> allOuvriers,
            ConfigurationPlanification config,
            JoursOuvresCalculator joursOuvresCalculator)
        {
            double effortTotalJoursHomme = 0;
            decimal coutTotalRh = 0;

            var ouvriersDataById = allOuvriers.ToDictionary(o => o.OuvrierId);

            foreach (var kvp in planning.SegmentsParOuvrierId)
            {
                var segmentsOuvrier = kvp.Value;
                if (segmentsOuvrier == null || !segmentsOuvrier.Any()) continue;

                int joursTravaillesUniques = segmentsOuvrier.Select(s => s.Jour.Date).Distinct().Count();
                effortTotalJoursHomme += joursTravaillesUniques;

                if (ouvriersDataById.TryGetValue(kvp.Key, out var ouvrierData))
                {
                    coutTotalRh += joursTravaillesUniques * ouvrierData.CoutJournalier;
                }
            }

            int dureeJoursOuvres = 0;
            if (planning.DateDebutProjet != default && planning.DateFinProjet != default)
            {
                dureeJoursOuvres = joursOuvresCalculator(planning.DateDebutProjet, planning.DateFinProjet);
            }

            decimal coutTotalIndirect = dureeJoursOuvres * config.CoutIndirectJournalierAbsolu;

            return new SyntheseProjetReport
            {
                EffortTotalJoursHomme = effortTotalJoursHomme,
                CoutTotalRh = coutTotalRh,
                CoutTotalIndirect = coutTotalIndirect,
                CoutTotalProjet = coutTotalRh + coutTotalIndirect,
                DureeJoursOuvres = dureeJoursOuvres
            };
        }

        private List<AnalyseOuvrierReport> _CalculerAnalysesOuvriers(
            ConsolidatedPlanning planning,
            IReadOnlyList<Ouvrier> allOuvriers,
            IReadOnlyList<Metier> allMetiers,
            ConfigurationPlanification config,
            JoursOuvresCalculator joursOuvresCalculator)
        {
            var reports = new List<AnalyseOuvrierReport>();
            var metiersById = allMetiers.ToDictionary(m => m.MetierId);

            foreach (var ouvrier in allOuvriers)
            {
                var competencePrincipale = ouvrier.Competences?.FirstOrDefault(c => c.EstMetierPrincipal)
                                           ?? ouvrier.Competences?.FirstOrDefault();

                string metierPrincipalId = competencePrincipale?.MetierId ?? string.Empty;
                string metierPrincipalNom = "Non défini";
                if (!string.IsNullOrEmpty(metierPrincipalId) && metiersById.TryGetValue(metierPrincipalId, out var metier))
                {
                    metierPrincipalNom = metier.Nom;
                }

                double heuresTravaillees = 0;
                int joursTravailles = 0;
                double tauxOccupation = 0;
                double tauxFragmentation = 0;

                if (planning.SegmentsParOuvrierId.TryGetValue(ouvrier.OuvrierId, out var segmentsOuvrier) && segmentsOuvrier.Any())
                {
                    heuresTravaillees = segmentsOuvrier.Sum(s => s.HeuresTravaillees);
                    joursTravailles = segmentsOuvrier.Select(s => s.Jour.Date).Distinct().Count();

                    double heuresTheoriques = joursTravailles * config.HeuresTravailEffectifParJour;
                    tauxOccupation = (heuresTheoriques > 0) ? (heuresTravaillees / heuresTheoriques) : 0;

                    DateTime datePremierSegment = segmentsOuvrier.Min(s => s.Jour.Date);
                    DateTime dateDernierSegment = segmentsOuvrier.Max(s => s.Jour.Date);
                    int joursOuvresSurPeriode = joursOuvresCalculator(datePremierSegment, dateDernierSegment);
                    tauxFragmentation = (joursOuvresSurPeriode > 0) ? (1.0 - ((double)joursTravailles / joursOuvresSurPeriode)) : 0;
                }

                reports.Add(new AnalyseOuvrierReport
                {
                    OuvrierId = ouvrier.OuvrierId,
                    NomComplet = ouvrier.NomComplet,
                    MetierPrincipalId = metierPrincipalId,
                    MetierPrincipalNom = metierPrincipalNom,
                    HeuresTravaillees = Math.Round(heuresTravaillees, 2),
                    JoursTravailles = joursTravailles,
                    TauxOccupation = Math.Round(tauxOccupation, 2),
                    TauxFragmentation = Math.Round(tauxFragmentation, 2)
                });
            }

            return reports;
        }

        private Dictionary<DateTime, int> _CalculerChargeJournaliere(ConsolidatedPlanning planning)
        {
            if (planning.SegmentsParOuvrierId == null || !planning.SegmentsParOuvrierId.Any())
            {
                return new Dictionary<DateTime, int>();
            }

            return planning.SegmentsParOuvrierId
                .SelectMany(kvp => kvp.Value)
                .GroupBy(segment => segment.Jour.Date)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(s => s.OuvrierId).Distinct().Count()
                );
        }

        #endregion
    }
}