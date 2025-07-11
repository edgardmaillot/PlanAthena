// Utilities/CsvGenerator.cs
using PlanAthena.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PlanAthena.Utilities
{
    public static class CsvGenerator
    {
        /// <summary>
        /// Génère le fichier taches.csv à partir des données du projet.
        /// </summary>
        /// <param name="outputFilePath">Le chemin complet où enregistrer le fichier CSV.</param>
        public static void GenerateTachesCsv(string outputFilePath)
        {
            // 1. Charger toutes les données du projet en mémoire
            var projectData = ProjectDataLoader.GetImmeubleCentreVilleRequest();

            // 2. Pour des recherches rapides, créer un dictionnaire des zones (Lots)
            var zoneLookup = projectData.Zones.ToDictionary(z => z.ZoneId);

            // 3. Utiliser un StringBuilder pour construire le contenu du CSV, c'est efficace
            var csvBuilder = new StringBuilder();

            // 4. Ajouter l'en-tête du fichier CSV
            csvBuilder.AppendLine("TacheId;TacheNom;HeuresHommeEstimees;MetierId;Dependencies;LotId;LotNom;LotPriorite;BlocId;BlocNom;BlocCapaciteMaxOuvriers");

            // 5. Parcourir chaque bloc, puis chaque tâche (opération) dans ce bloc
            foreach (var bloc in projectData.Blocs)
            {
                // Retrouver les infos du Lot (Zone) associé à ce Bloc
                if (!zoneLookup.TryGetValue(bloc.AssociatedZoneId, out var lot))
                {
                    // Sécurité : si une zone n'est pas trouvée, on passe pour éviter une erreur
                    continue;
                }

                foreach (var operation in bloc.Operations)
                {
                    // Préparer les données pour une ligne
                    var tacheId = EscapeCsvField(operation.OperationId);
                    var tacheNom = EscapeCsvField(operation.OperationId); // On utilise l'ID comme nom
                    var heures = operation.Hours;
                    var metierId = EscapeCsvField(operation.TradeCode);

                    // Concaténer les dépendances avec des virgules
                    var dependencies = string.Join(",", operation.DependsOnOperationIds);
                    var escapedDependencies = EscapeCsvField(dependencies);

                    var lotId = EscapeCsvField(lot.ZoneId);
                    var lotNom = EscapeCsvField(lot.ZoneId); // On utilise l'ID comme nom
                    var lotPriorite = lot.PriorityLevel;

                    var blocId = EscapeCsvField(bloc.BlocId);
                    var blocNom = EscapeCsvField(bloc.BlocId); // On utilise l'ID comme nom
                    var blocCapacite = bloc.MaxConcurrentWorkersInBloc;

                    // Construire la ligne et l'ajouter au builder
                    csvBuilder.AppendLine($"{tacheId};{tacheNom};{heures};{metierId};{escapedDependencies};{lotId};{lotNom};{lotPriorite};{blocId};{blocNom};{blocCapacite}");
                }
            }

            // 6. Écrire le contenu final dans le fichier
            File.WriteAllText(outputFilePath, csvBuilder.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// "Échappe" un champ pour le format CSV. Si le champ contient un point-virgule,
        /// une virgule ou des guillemets, il est entouré de guillemets.
        /// Les guillemets internes sont doublés.
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // Le champ des dépendances est le seul qui contiendra des virgules
            if (field.Contains(',') || field.Contains(';') || field.Contains('"'))
            {
                // Remplace les guillemets par des doubles guillemets et entoure le tout de guillemets
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}