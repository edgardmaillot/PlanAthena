using PlanAthena.Services.Business; // Pour accéder à UserPreferencesService
using System;
using System.Collections.Generic;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Classe utilitaire pour gérer un dictionnaire de correspondances (traductions)
    /// enrichi par l'utilisateur.
    /// </summary>
    public class ValueMappingService
    {
        private readonly UserPreferencesService _preferencesService;
        private readonly Dictionary<string, string> _dictionnaire;

        /// <summary>
        /// Initialise une nouvelle instance du service de mapping.
        /// Charge le dictionnaire des correspondances au démarrage.
        /// </summary>
        /// <param name="preferencesService">L'instance du service de préférences utilisateur.</param>
        public ValueMappingService(UserPreferencesService preferencesService)
        {
            _preferencesService = preferencesService ?? throw new ArgumentNullException(nameof(preferencesService));

            // Charge la base de connaissances une seule fois à l'initialisation.
            _dictionnaire = _preferencesService.ChargerDictionnaire();
        }

        /// <summary>
        /// Recherche une correspondance pour une valeur donnée dans le dictionnaire.
        /// </summary>
        /// <param name="valeur">Le terme source à rechercher.</param>
        /// <returns>La traduction correspondante si elle est trouvée, sinon une chaîne vide.</returns>
        public string TrouveCorrespondance(string valeur)
        {
            if (string.IsNullOrWhiteSpace(valeur))
            {
                return string.Empty;
            }

            // La recherche est insensible à la casse grâce à l'initialisation du dictionnaire.
            if (_dictionnaire.TryGetValue(valeur, out var traduction))
            {
                return traduction;
            }

            return string.Empty;
        }

        /// <summary>
        /// Ajoute ou met à jour une correspondance dans le dictionnaire et sauvegarde les modifications.
        /// </summary>
        /// <param name="valeur">Le terme source (la clé).</param>
        /// <param name="traduction">Le terme cible (la valeur).</param>
        public void AjouteCorrespondance(string valeur, string traduction)
        {
            if (string.IsNullOrWhiteSpace(valeur) || string.IsNullOrWhiteSpace(traduction))
            {
                // On n'ajoute pas de correspondances invalides.
                return;
            }

            // L'opérateur [] gère à la fois l'ajout et la mise à jour.
            _dictionnaire[valeur] = traduction;
            _preferencesService.SauverDictionnaire(_dictionnaire);
        }

        /// <summary>
        /// Supprime une correspondance du dictionnaire et sauvegarde les modifications.
        /// </summary>
        /// <param name="valeur">Le terme source à supprimer.</param>
        public void SupprimeCorrespondance(string valeur)
        {
            if (string.IsNullOrWhiteSpace(valeur)) return;

            // On vérifie si la clé a été effectivement supprimée avant de sauvegarder,
            // pour éviter une écriture disque inutile.
            if (_dictionnaire.Remove(valeur))
            {
                _preferencesService.SauverDictionnaire(_dictionnaire);
            }
        }
    }
}