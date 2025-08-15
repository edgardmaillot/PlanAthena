// Fichier: PlanAthena/Data/Metier.cs
// Version: 0.4.4 (Corrigé)
// Description: Correction suite à retour. Suppression de la propriété helper 'PrerequisMetierIds'
// pour respecter la séparation des couches. L'entité est maintenant un pur objet de données.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlanAthena.Data
{
    /// <summary>
    /// Représente un métier avec ses prérequis contextuels par phase de chantier.
    /// </summary>
    public class Metier
    {
        public string MetierId { get; set; } = "";
        public string Nom { get; set; } = "";

        /// <summary>
        /// Dictionnaire des prérequis métiers, où la clé est la phase de chantier
        /// et la valeur est la liste des IDs des métiers prérequis pour cette phase.
        /// </summary>
        public Dictionary<ChantierPhase, List<string>> PrerequisParPhase { get; set; } = new();

        public string CouleurHex { get; set; } = ""; // Couleur au format hexadécimal (#RRGGBB)
        public string Pictogram { get; set; } = "";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChantierPhase Phases { get; set; } = ChantierPhase.None;
    }
}