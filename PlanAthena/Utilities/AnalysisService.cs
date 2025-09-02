// Emplacement: /Utilities/AnalysisService.cs

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Service utilitaire stateless pour calculer un ensemble complet d'indicateurs de performance 
    /// clés (KPIs) à partir d'un planning consolidé.
    /// </summary>
    public class AnalysisService
    {
        // Type alias pour améliorer la lisibilité de la signature de la méthode.
        public delegate int JoursOuvresCalculator(DateTime start, DateTime end);

        #region Points d'Entrée Publics

        /// <summary>
        /// Génère un rapport d'analyse complet à partir d'un planning optimisé.
        /// </summary>
        /// <param name="planning">Le planning consolidé et propre.</param>
        /// <param name="allOuvriers">La liste de tous les ouvriers disponibles pour le projet.</param>
        /// <param name="allMetiers">La liste de tous les métiers disponibles, requise pour déterminer le nom du métier principal.</param>
        /// <param name="config">La configuration de planification utilisée.</param>
        /// <param name="joursOuvresCalculator">Une fonction externe pour calculer le nombre de jours ouvrés dans un intervalle.</param>
        /// <returns>Un rapport d'analyse détaillé.</returns>
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
        /// <param name="ouvrierClesIds">Liste des IDs des 4 ouvriers les plus en tension.</param>
        /// <param name="allOuvriers">La liste de tous les ouvriers pour retrouver leur métier principal.</param>
        /// <returns>Un rapport sur la tension des métiers.</returns>
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
                // Étape 1: Extraire le métier principal. Cette information est requise pour tous les ouvriers, qu'ils aient travaillé ou non.
                var competencePrincipale = ouvrier.Competences?.FirstOrDefault(c => c.EstMetierPrincipal)
                                           ?? ouvrier.Competences?.FirstOrDefault(); // Fallback

                string metierPrincipalId = competencePrincipale?.MetierId ?? string.Empty;
                string metierPrincipalNom = "Non défini";
                if (!string.IsNullOrEmpty(metierPrincipalId) && metiersById.TryGetValue(metierPrincipalId, out var metier))
                {
                    metierPrincipalNom = metier.Nom;
                }

                // Étape 2: Calculer les KPIs basés sur le planning.
                double heuresTravaillees = 0;
                int joursTravailles = 0;
                double tauxOccupation = 0;
                double tauxFragmentation = 0;

                if (planning.SegmentsParOuvrierId.TryGetValue(ouvrier.OuvrierId, out var segmentsOuvrier) && segmentsOuvrier.Any())
                {
                    // L'ouvrier a travaillé, on calcule ses KPIs.
                    heuresTravaillees = segmentsOuvrier.Sum(s => s.HeuresTravaillees);
                    joursTravailles = segmentsOuvrier.Select(s => s.Jour.Date).Distinct().Count();

                    double heuresTheoriques = joursTravailles * config.HeuresTravailEffectifParJour;
                    tauxOccupation = (heuresTheoriques > 0) ? (heuresTravaillees / heuresTheoriques) : 0;

                    DateTime datePremierSegment = segmentsOuvrier.Min(s => s.Jour.Date);
                    DateTime dateDernierSegment = segmentsOuvrier.Max(s => s.Jour.Date);
                    int joursOuvresSurPeriode = joursOuvresCalculator(datePremierSegment, dateDernierSegment);
                    tauxFragmentation = (joursOuvresSurPeriode > 0) ? (1.0 - ((double)joursTravailles / joursOuvresSurPeriode)) : 0;
                }

                // Étape 3: Construire le rapport final en combinant les informations.
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
                .SelectMany(kvp => kvp.Value) // Aplatir tous les segments
                .GroupBy(segment => segment.Jour.Date) // Grouper par jour
                .ToDictionary(
                    group => group.Key, // Clé = Date
                    group => group.Select(s => s.OuvrierId).Distinct().Count() // Valeur = Nombre d'ouvriers uniques
                );
        }

        #endregion
    }
}