// Emplacement: /Services/Business/PlanningService.cs

using PlanAthena.Services.Business.DTOs;
using System;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Agit comme la Source de Vérité unique pour le planning de projet actuellement chargé.
    /// Ce service est destiné à être un singleton (ou scoped) pour maintenir en mémoire 
    /// l'état du planning consolidé et le rendre accessible aux autres services de l'application.
    /// </summary>
    public class PlanningService
    {
        // L'instance du planning consolidé, résultat du dernier run de planification.
        private ConsolidatedPlanning? _currentPlanning;

        // La configuration qui a été utilisée pour générer le _currentPlanning.
        // Essentielle pour les calculs calendaires cohérents.
        private ConfigurationPlanification? _currentConfig;

        #region Méthodes de Gestion de l'État

        /// <summary>
        /// Met à jour l'état du service avec un nouveau planning et sa configuration associée.
        /// C'est la méthode principale pour charger des données dans le service.
        /// </summary>
        /// <param name="newPlanning">Le nouveau planning consolidé à stocker.</param>
        /// <param name="newConfig">La configuration de planification utilisée pour générer ce planning.</param>
        /// <exception cref="ArgumentNullException">Levée si newPlanning ou newConfig est null.</exception>
        public virtual void UpdatePlanning(ConsolidatedPlanning newPlanning, ConfigurationPlanification newConfig)
        {
            // Valider que les entrées ne sont pas nulles pour éviter un état incohérent.
            _currentPlanning = newPlanning ?? throw new ArgumentNullException(nameof(newPlanning));
            _currentConfig = newConfig ?? throw new ArgumentNullException(nameof(newConfig));
        }

        /// <summary>
        /// Retourne une référence vers le planning actuellement stocké.
        /// </summary>
        /// <returns>L'instance de ConsolidatedPlanning ou null si aucun planning n'est chargé.</returns>
        public virtual ConsolidatedPlanning? GetCurrentPlanning()
        {
            return _currentPlanning;
        }

        /// <summary>
        /// Réinitialise l'état du service, effaçant le planning et la configuration actuels.
        /// À appeler lors du déchargement d'un projet, par exemple.
        /// </summary>
        public void ClearPlanning()
        {
            _currentPlanning = null;
            _currentConfig = null;
        }

        #endregion

        #region Méthodes de Calcul

        /// <summary>
        /// Calcule le nombre de jours ouvrés entre deux dates (inclusives),
        /// en se basant sur les jours ouvrés définis dans la configuration du planning actuel.
        /// </summary>
        /// <param name="dateDebut">La date de début de l'intervalle.</param>
        /// <param name="dateFin">La date de fin de l'intervalle.</param>
        /// <returns>
        /// Le nombre total de jours ouvrés. Retourne 0 si aucun planning/configuration n'est chargé,
        /// si la liste des jours ouvrés est vide, ou si l'intervalle est invalide.
        /// </returns>
        public int GetNombreJoursOuvres(DateTime dateDebut, DateTime dateFin)
        {
            // Gérer les cas où aucun planning/config n'est chargé.
            // Comportement sécurisé : si pas de config, aucun jour n'est "ouvré".
            if (_currentConfig?.JoursOuvres == null || !_currentConfig.JoursOuvres.Any())
            {
                return 0;
            }

            // Gérer les entrées invalides.
            if (dateFin < dateDebut)
            {
                return 0;
            }

            int nombreJoursOuvres = 0;
            // Itérer du début du premier jour à la fin du dernier jour.
            for (var jourCourant = dateDebut.Date; jourCourant <= dateFin.Date; jourCourant = jourCourant.AddDays(1))
            {
                // Vérifier si le jour de la semaine est dans la liste des jours ouvrés de la config.
                if (_currentConfig.JoursOuvres.Contains(jourCourant.DayOfWeek))
                {
                    nombreJoursOuvres++;
                }
            }

            return nombreJoursOuvres;
        }

        #endregion
    }
}