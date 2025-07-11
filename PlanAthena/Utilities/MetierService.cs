// Utilities/MetierService.cs
using PlanAthena.CsvModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Gère la connaissance et la logique métier liées aux métiers.
    /// Sert de point de vérité unique pour la liste des métiers.
    /// </summary>
    public class MetierService
    {
        // La liste interne de tous les métiers, incluant ceux chargés et ceux générés.
        private readonly Dictionary<string, MetierCsvRecord> _metiers = new Dictionary<string, MetierCsvRecord>();

        /// <summary>
        /// Charge la liste initiale des métiers depuis un fichier CSV.
        /// Écrase toutes les données précédentes.
        /// </summary>
        public void ChargerMetiers(IReadOnlyList<MetierCsvRecord> metiersFromCsv)
        {
            _metiers.Clear();
            foreach (var metier in metiersFromCsv)
            {
                // On s'assure de ne pas ajouter de doublons si le CSV en contient
                if (!_metiers.ContainsKey(metier.MetierId))
                {
                    _metiers.Add(metier.MetierId, metier);
                }
            }
        }

        /// <summary>
        /// Retourne la liste complète et à jour de tous les métiers.
        /// </summary>
        public IReadOnlyList<MetierCsvRecord> GetAllMetiers()
        {
            return _metiers.Values.ToList();
        }

        /// <summary>
        /// Fournit l'ID d'un métier de synchronisation/attente, en le créant s'il n'existe pas.
        /// </summary>
        /// <param name="dureeEnHeures">La durée de l'attente (ex: 0, 12, 24).</param>
        /// <returns>L'ID unique du métier de synchronisation.</returns>
        public string GetOrCreateSyncMetierId(int dureeEnHeures)
        {
            string metierId = $"SYNC_{dureeEnHeures}H";

            // Si le métier existe déjà, on retourne simplement son ID.
            if (_metiers.ContainsKey(metierId))
            {
                return metierId;
            }

            // Sinon, on le crée et on l'ajoute à notre dictionnaire interne.
            var syncMetier = new MetierCsvRecord
            {
                MetierId = metierId,
                Nom = $"Tâche de Synchronisation/Attente ({dureeEnHeures}h)",
                PrerequisMetierIds = "" // Les tâches de synchro n'ont pas de prérequis métier
            };

            _metiers.Add(metierId, syncMetier);

            return metierId;
        }

        /// <summary>
        /// Retourne les prérequis pour un métier donné.
        /// </summary>
        public IReadOnlyList<string> GetPrerequisForMetier(string metierId)
        {
            if (_metiers.TryGetValue(metierId, out var metier) && !string.IsNullOrEmpty(metier.PrerequisMetierIds))
            {
                return metier.PrerequisMetierIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
            return Array.Empty<string>();
        }
    }
}