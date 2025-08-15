// Fichier: PlanAthena/Services/DataAccess/ProjetRepository.cs
// Version: 0.4.4
// Description: Nouveau service dédié à la persistance des données du projet.
// Isole les opérations de lecture/écriture de fichiers de la logique métier.

using PlanAthena.Data;
using System;
using System.IO;
using System.Text.Json;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Gère la persistance (lecture/écriture) des fichiers de projet JSON.
    /// Ne contient aucune logique métier.
    /// </summary>
    public class ProjetRepository
    {
        /// <summary>
        /// Sauvegarde l'état complet d'un projet dans un fichier JSON.
        /// </summary>
        /// <param name="projetData">L'objet ProjetData à sérialiser.</param>
        /// <param name="filePath">Le chemin complet du fichier de destination.</param>
        public void Sauvegarder(ProjetData projetData, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Le chemin du fichier ne peut pas être vide.", nameof(filePath));

            if (projetData == null)
                throw new ArgumentNullException(nameof(projetData));

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            string jsonString = JsonSerializer.Serialize(projetData, options);
            File.WriteAllText(filePath, jsonString);
        }

        /// <summary>
        /// Charge un projet depuis un fichier JSON.
        /// </summary>
        /// <param name="filePath">Le chemin complet du fichier de projet à lire.</param>
        /// <returns>L'objet ProjetData désérialisé.</returns>
        public ProjetData Charger(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Le fichier de projet n'a pas été trouvé.", filePath);

            var jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var projetData = JsonSerializer.Deserialize<ProjetData>(jsonString, options);

            if (projetData == null)
                throw new InvalidOperationException("Le fichier de projet est invalide ou corrompu.");

            return projetData;
        }
    }
}