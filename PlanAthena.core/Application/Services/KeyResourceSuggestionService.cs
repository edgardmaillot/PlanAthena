using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;

namespace PlanAthena.Core.Application.Services
{
    public class KeyResourceSuggestionService : IKeyResourceSuggestionService
    {
        private const int MAX_SUGGESTIONS = 3;

        /// <summary>
        /// Suggère des ouvriers clés pour le chantier en se basant sur la charge de travail
        /// et la rareté des compétences requises.
        /// </summary>
        /// <param name="chantier">L'agrégat Chantier du domaine, supposé valide et complet.</param>
        /// <returns>Une liste d'OuvrierId des ouvriers suggérés comme clés.</returns>
        public Task<List<OuvrierId>> SuggererOuvriersClesAsync(Chantier chantier)
        {
            if (chantier.Ouvriers.Count == 0 || !chantier.ObtenirToutesLesTaches().Any())
            {
                return Task.FromResult(new List<OuvrierId>());
            }

            // 1. Calculer la charge totale en heures-homme pour chaque métier.
            var chargeParMetier = chantier.ObtenirToutesLesTaches()
                .GroupBy(t => t.MetierRequisId)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.HeuresHommeEstimees.Value));

            // 2. Calculer la rareté de chaque métier (combien d'ouvriers le possèdent).
            var rareteParMetier = chantier.Ouvriers.Values
                .SelectMany(o => o.Competences.Keys)
                .GroupBy(metierId => metierId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 3. Calculer un score de criticité pour chaque ouvrier.
            var scoresOuvriers = new Dictionary<OuvrierId, double>();
            foreach (var ouvrier in chantier.Ouvriers.Values)
            {
                double scoreTotal = 0;
                foreach (var competenceId in ouvrier.Competences.Keys)
                {
                    chargeParMetier.TryGetValue(competenceId, out int charge);
                    rareteParMetier.TryGetValue(competenceId, out int rarete);

                    if (charge > 0 && rarete > 0)
                    {
                        // Le score pour cette compétence est la charge de travail divisée par le nombre de personnes pouvant la faire.
                        // Une charge élevée et une rareté faible (ex: 1 seule personne) donnent un score élevé.
                        double scoreCompetence = (double)charge / rarete;
                        scoreTotal += scoreCompetence;
                    }
                }
                scoresOuvriers[ouvrier.Id] = scoreTotal;
            }

            // 4. Trier les ouvriers par score et retourner les meilleurs suggestions.
            var ouvriersDejaCles = chantier.ConfigCdC?.OuvriersClefs ?? new HashSet<OuvrierId>();

            var suggestions = scoresOuvriers
                .Where(kvp => !ouvriersDejaCles.Contains(kvp.Key)) // Exclure ceux déjà marqués comme clés
                .OrderByDescending(kvp => kvp.Value) // Trier par score décroissant
                .Select(kvp => kvp.Key) // Sélectionner l'ID de l'ouvrier
                .Take(MAX_SUGGESTIONS) // Prendre les N meilleurs
                .ToList();

            return Task.FromResult(suggestions);
        }
    }
}