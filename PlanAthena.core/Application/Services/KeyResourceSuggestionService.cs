// PlanAthena.Core.Application.Services.KeyResourceSuggestionService.cs
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Core.Application.Services
{
    public class KeyResourceSuggestionService : IKeyResourceSuggestionService
    {
        private const int MAX_SUGGESTIONS = 3;

        /// <summary>
        /// Suggère des ouvriers clés pour le chantier en se basant sur la charge de travail
        /// et la rareté des compétences requises par les tâches réelles.
        /// </summary>
        /// <param name="chantier">L'agrégat Chantier du domaine, supposé valide et complet.</param>
        /// <returns>Une liste d'OuvrierId des ouvriers suggérés comme clés.</returns>
        public Task<List<OuvrierId>> SuggererOuvriersClesAsync(Chantier chantier)
        {
            // Nous filtrons les ouvriers et les tâches en amont.
            // Les suggestions ne doivent concerner que les ressources humaines réelles et le travail effectif.
            // Les ouvriers virtuels sont identifiés par leur ID qui respecte une convention de nommage.
            var ouvriersReels = chantier.Ouvriers.Values
                .Where(o => !o.Id.Value.StartsWith("VIRTUAL"))
                .ToList();

            var tachesReelles = chantier.ObtenirToutesLesTaches()
                .Where(t => t.Type == TypeActivite.Tache)
                .ToList();

            if (!ouvriersReels.Any() || !tachesReelles.Any())
            {
                return Task.FromResult(new List<OuvrierId>());
            }

            // 1. Calculer la charge totale en heures-homme pour chaque métier, basé uniquement sur les tâches réelles.
            // La propriété HeuresHommeEstimees d'un jalon représente une durée, pas une charge de travail.
            // L'exclure est essentiel pour un calcul de charge correct.
            var chargeParMetier = tachesReelles
                .GroupBy(t => t.MetierRequisId)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.HeuresHommeEstimees.Value));

            // 2. Calculer la rareté de chaque métier en se basant uniquement sur les ouvriers réels.
            var rareteParMetier = ouvriersReels
                .SelectMany(o => o.Competences.Keys)
                .GroupBy(metierId => metierId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 3. Calculer un score de criticité pour chaque ouvrier réel.
            var scoresOuvriers = new Dictionary<OuvrierId, double>();
            foreach (var ouvrier in ouvriersReels)
            {
                double scoreTotal = 0;
                foreach (var competenceId in ouvrier.Competences.Keys)
                {
                    chargeParMetier.TryGetValue(competenceId, out int charge);
                    rareteParMetier.TryGetValue(competenceId, out int rarete);

                    if (charge > 0 && rarete > 0)
                    {
                        double scoreCompetence = (double)charge / rarete;
                        scoreTotal += scoreCompetence;
                    }
                }
                scoresOuvriers[ouvrier.Id] = scoreTotal;
            }

            // 4. Trier les ouvriers par score et retourner les meilleurs suggestions.
            var ouvriersDejaCles = chantier.ConfigCdC?.OuvriersClefs ?? new HashSet<OuvrierId>();

            var suggestions = scoresOuvriers
                .Where(kvp => !ouvriersDejaCles.Contains(kvp.Key))
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .Take(MAX_SUGGESTIONS)
                .ToList();

            return Task.FromResult(suggestions);
        }
    }
}