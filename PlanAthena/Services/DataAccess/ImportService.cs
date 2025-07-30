using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System.Diagnostics;
using System.Globalization;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service d'import de tâches depuis fichiers CSV - Version Simple POC
    /// </summary>
    public class ImportService
    {
        private readonly TacheService _tacheService;
        private readonly LotService _lotService;
        private readonly BlocService _blocService;
        private readonly MetierService _metierService;
        private readonly IdGeneratorService _idGenerator;

        public ImportService(
            TacheService tacheService,
            LotService lotService,
            BlocService blocService,
            MetierService metierService,
            IdGeneratorService idGenerator)
        {
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        /// <summary>
        /// Importe les tâches depuis un fichier CSV
        /// </summary>
        public ImportResult ImporterTachesCSV(string filePath, string lotIdCible, bool confirmerEcrasement = false)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 1. Validations de base
                if (!File.Exists(filePath))
                    return ImportResult.Echec($"Le fichier '{filePath}' n'existe pas.");

                if (string.IsNullOrWhiteSpace(lotIdCible))
                    return ImportResult.Echec("L'ID du lot cible ne peut pas être vide.");

                // 2. Détection automatique du séparateur et lecture CSV
                var lignes = File.ReadAllLines(filePath);
                if (lignes.Length < 2)
                    return ImportResult.Echec("Le fichier CSV est vide ou ne contient que l'en-tête.");

                // Détecter le séparateur (TAB ou point-virgule)
                var premiereLigne = lignes[0];
                char separateur = premiereLigne.Contains('\t') ? '\t' : ';';

                var headers = premiereLigne.Split(separateur).Select(h => h.Trim()).ToArray();

                // Debug : afficher les colonnes trouvées
                System.Diagnostics.Debug.WriteLine($"Séparateur détecté : '{(separateur == '\t' ? "TAB" : ";")}'");
                System.Diagnostics.Debug.WriteLine($"Colonnes trouvées : [{string.Join("], [", headers)}]");

                // Validation des colonnes requises
                var colonnesRequises = new[] {
                    "TacheId", "TacheNom", "HeuresHommeEstimees", "MetierId",
                    "Dependencies", "ExclusionsDependances", "Type", "EstJalon",
                    "LotId", "LotNom", "LotPriorite",
                    "BlocId", "BlocNom", "BlocCapaciteMaxOuvriers"
                };

                // Vérification exacte (sensible à la casse pour être précis)
                var colonnesManquantes = colonnesRequises.Except(headers).ToList();

                if (colonnesManquantes.Any())
                {
                    var messageDebug = $"Séparateur utilisé : {(separateur == '\t' ? "Tabulation" : "Point-virgule")}\n";
                    messageDebug += $"Colonnes trouvées ({headers.Length}) : {string.Join(", ", headers)}\n";
                    messageDebug += $"Colonnes manquantes ({colonnesManquantes.Count}) : {string.Join(", ", colonnesManquantes)}";
                    return ImportResult.Echec(messageDebug);
                }

                var donneesCSV = new List<Dictionary<string, string>>();

                for (int i = 1; i < lignes.Length; i++)
                {
                    var valeurs = lignes[i].Split(separateur);
                    var ligne = new Dictionary<string, string>();

                    for (int j = 0; j < headers.Length; j++)
                    {
                        var valeur = j < valeurs.Length ? valeurs[j].Trim() : "";
                        ligne[headers[j]] = valeur;
                    }
                    donneesCSV.Add(ligne);
                }

                // 3. Vérification confirmation si nécessaire
                if (!confirmerEcrasement)
                {
                    var lot = _lotService.ObtenirLotParId(lotIdCible);
                    if (lot != null)
                    {
                        var tachesExistantes = _tacheService.ObtenirTachesParLot(lotIdCible);
                        if (tachesExistantes.Count > 0)
                        {
                            var message = $"⚠️ ATTENTION : L'import dans le lot '{lot.Nom}' écrasera {tachesExistantes.Count} tâche(s) existante(s).\n\nCette action est irréversible.\n\nConfirmer l'import ?";
                            return ImportResult.DemandeConfirmation(message);
                        }
                    }
                }

                // 4. Vider le lot existant
                try
                {
                    ViderLot(lotIdCible);
                }
                catch (Exception ex)
                {
                    return ImportResult.Echec($"Impossible de vider le lot existant : {ex.Message}");
                }

                // 5. Import des données de manière atomique
                try
                {
                    var (nbTaches, nbBlocs, warnings) = ImporterDonnees(donneesCSV, lotIdCible);

                    stopwatch.Stop();
                    return ImportResult.Succes(nbTaches, 1, nbBlocs, warnings, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    // En cas d'erreur, le lot reste vide (pas de rollback partiel)
                    return ImportResult.Echec($"Erreur lors de l'import des données : {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return ImportResult.Echec($"Erreur lors de l'import: {ex.Message}");
            }
        }

        /// <summary>
        /// Vide complètement un lot en gérant les dépendances
        /// </summary>
        private void ViderLot(string lotId)
        {
            // 1. Récupérer toutes les tâches à supprimer
            var tachesASupprimer = _tacheService.ObtenirTachesParLot(lotId);
            var idsASupprimer = tachesASupprimer.Select(t => t.TacheId).ToHashSet();

            // 2. Nettoyer les dépendances dans TOUTES les tâches qui référencent celles du lot
            var toutesLesTaches = _tacheService.ObtenirToutesLesTaches();
            foreach (var tache in toutesLesTaches)
            {
                bool modifiee = false;

                // Nettoyer Dependencies
                if (!string.IsNullOrEmpty(tache.Dependencies))
                {
                    var deps = tache.Dependencies.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d) && !idsASupprimer.Contains(d))
                        .ToList();

                    var nouvelleDeps = string.Join(",", deps);
                    if (nouvelleDeps != tache.Dependencies)
                    {
                        tache.Dependencies = nouvelleDeps;
                        modifiee = true;
                    }
                }

                // Nettoyer ExclusionsDependances
                if (!string.IsNullOrEmpty(tache.ExclusionsDependances))
                {
                    var exclusions = tache.ExclusionsDependances.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d) && !idsASupprimer.Contains(d))
                        .ToList();

                    var nouvellesExclusions = string.Join(",", exclusions);
                    if (nouvellesExclusions != tache.ExclusionsDependances)
                    {
                        tache.ExclusionsDependances = nouvellesExclusions;
                        modifiee = true;
                    }
                }

                // Sauvegarder si modifiée (sauf si c'est une tâche à supprimer)
                if (modifiee && !idsASupprimer.Contains(tache.TacheId))
                {
                    _tacheService.ModifierTache(tache);
                }
            }

            // 3. Supprimer toutes les tâches du lot (maintenant sans dépendances)
            foreach (var tache in tachesASupprimer)
            {
                _tacheService.SupprimerTache(tache.TacheId);
            }

            // 4. Supprimer tous les blocs du lot
            var blocs = _blocService.ObtenirTousLesBlocs().Where(b => b.BlocId.StartsWith($"{lotId}_")).ToList();
            foreach (var bloc in blocs)
            {
                _blocService.SupprimerBloc(bloc.BlocId);
            }
        }

        /// <summary>
        /// Importe les données normalisées - VERSION CORRIGÉE pour la nouvelle API BlocService
        /// </summary>
        private (int nbTaches, int nbBlocs, List<string> warnings) ImporterDonnees(List<Dictionary<string, string>> donnees, string lotIdCible)
        {
            var warnings = new List<string>();
            var mappingBlocIds = new Dictionary<string, string>(); // Ancien ID CSV -> Nouvel ID généré

            // DEBUG : Vérifier ce qu'on reçoit
            System.Diagnostics.Debug.WriteLine($"ImporterDonnees - {donnees.Count} lignes reçues");

            // 1. Créer/récupérer le lot
            var premiereLigne = donnees.First();
            var lot = _lotService.ObtenirLotParId(lotIdCible);
            if (lot == null)
            {
                lot = new Lot
                {
                    LotId = lotIdCible,
                    Nom = premiereLigne["LotNom"],
                    Priorite = int.TryParse(premiereLigne["LotPriorite"], out int prio) ? prio : 1
                };
                _lotService.AjouterLot(lot);
                System.Diagnostics.Debug.WriteLine($"Lot créé : {lot.LotId} - {lot.Nom}");
            }

            // 2. Créer les blocs avec génération automatique d'IDs
            var blocsUniques = donnees.GroupBy(d => d["BlocId"]).Where(g => !string.IsNullOrEmpty(g.Key)).ToList();
            System.Diagnostics.Debug.WriteLine($"Blocs uniques trouvés : {blocsUniques.Count}");

            foreach (var groupe in blocsUniques)
            {
                var ligne = groupe.First();
                var ancienBlocId = ligne["BlocId"]; // ID du CSV
                var nomBloc = ligne["BlocNom"];

                System.Diagnostics.Debug.WriteLine($"Traitement bloc CSV : {ancienBlocId} -> {nomBloc}");

                // Vérifier si un bloc avec ce nom existe déjà
                var blocExistant = _blocService.ObtenirTousLesBlocs()
                    .FirstOrDefault(b => b.Nom == nomBloc && b.BlocId.StartsWith($"{lotIdCible}_"));

                if (blocExistant != null)
                {
                    // Réutiliser le bloc existant
                    mappingBlocIds[ancienBlocId] = blocExistant.BlocId;
                    System.Diagnostics.Debug.WriteLine($"Bloc réutilisé : {ancienBlocId} -> {blocExistant.BlocId}");
                }
                else
                {
                    // Créer un nouveau bloc avec ID généré automatiquement
                    var nouveauBlocId = _blocService.GenerateNewBlocId(lotIdCible);
                    var bloc = new Bloc
                    {
                        BlocId = nouveauBlocId,
                        Nom = nomBloc,
                        CapaciteMaxOuvriers = int.TryParse(ligne["BlocCapaciteMaxOuvriers"], out int cap) ? cap : 6
                    };

                    _blocService.SaveBloc(bloc); // Utiliser la nouvelle méthode SaveBloc
                    mappingBlocIds[ancienBlocId] = nouveauBlocId;
                    System.Diagnostics.Debug.WriteLine($"Bloc créé : {ancienBlocId} -> {nouveauBlocId} ({nomBloc})");
                }
            }

            // 3. Créer les tâches avec remapping des BlocIds
            var nbTachesCreees = 0;
            foreach (var ligne in donnees)
            {
                var ancienBlocId = ligne["BlocId"];
                var nouveauBlocId = mappingBlocIds.ContainsKey(ancienBlocId) ? mappingBlocIds[ancienBlocId] : ancienBlocId;

                var tache = new Tache
                {
                    // PAS D'ID - TacheService va le générer automatiquement
                    TacheNom = ligne["TacheNom"],
                    LotId = lotIdCible,
                    BlocId = nouveauBlocId, // Utiliser l'ID remappé
                    HeuresHommeEstimees = int.TryParse(ligne["HeuresHommeEstimees"], out int heures) ? heures : 8,
                    MetierId = ligne["MetierId"],
                    Type = ligne["EstJalon"] == "True" ? TypeActivite.JalonUtilisateur : TypeActivite.Tache
                    // Dependencies sera traité après par DependanceBuilder
                };

                _tacheService.AjouterTache(tache); // TacheService génère l'ID automatiquement
                nbTachesCreees++;
                System.Diagnostics.Debug.WriteLine($"Tâche créée : {tache.TacheId} - {tache.TacheNom} (Bloc: {nouveauBlocId})");
            }

            System.Diagnostics.Debug.WriteLine($"RÉSULTAT : {nbTachesCreees} tâches, {blocsUniques.Count} blocs");
            return (nbTachesCreees, blocsUniques.Count, warnings);
        }

        /// <summary>
        /// Récupère une valeur du dictionnaire de manière sécurisée
        /// </summary>
        private static string GetValueOrDefault(Dictionary<string, string> dict, string key, string defaultValue = "")
        {
            return dict.TryGetValue(key, out string value) ? value : defaultValue;
        }

        /// <summary>
        /// Applique automatiquement les dépendances suggérées par DependanceBuilder
        /// </summary>
        private int AppliquerDependancesAutomatiques(string lotId)
        {
            var dependancesAppliquees = 0;

            // 1. Récupérer toutes les tâches du lot (plus simple et direct)
            var tachesDuLot = _tacheService.ObtenirTachesParLot(lotId);
            if (!tachesDuLot.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Aucune tâche trouvée pour le lot {lotId}");
                return 0;
            }

            // 2. Grouper par bloc
            var tachesParBloc = tachesDuLot.GroupBy(t => t.BlocId).ToList();
            System.Diagnostics.Debug.WriteLine($"Trouvé {tachesParBloc.Count} blocs avec tâches dans le lot {lotId}");

            // 3. Pour chaque bloc
            foreach (var groupeBloc in tachesParBloc)
            {
                var blocId = groupeBloc.Key;
                var tachesDuBloc = groupeBloc.ToList();

                System.Diagnostics.Debug.WriteLine($"Traitement bloc {blocId} avec {tachesDuBloc.Count} tâches");

                // 4. Trier les tâches par ordre des prérequis métiers
                var metiersTries = _metierService.ObtenirMetiersTriesParDependance();
                var tachesTriees = tachesDuBloc
                    .OrderBy(t => TrouverIndexMetier(t.MetierId, metiersTries))
                    .ToList();

            }

            System.Diagnostics.Debug.WriteLine($"Total dépendances appliquées : {dependancesAppliquees}");
            return dependancesAppliquees;
        }

        /// <summary>
        /// Trouve l'index d'un métier dans la liste triée (pour l'ordre des prérequis)
        /// </summary>
        private int TrouverIndexMetier(string metierId, List<Metier> metiersTries)
        {
            if (string.IsNullOrEmpty(metierId))
                return 999; // Tâches sans métier en dernier

            var index = metiersTries.FindIndex(m => m.MetierId == metierId);
            return index >= 0 ? index : 998; // Métiers inconnus avant les tâches sans métier
        }

        /// <summary>
        /// Détermine le type d'activité avec gestion d'erreur
        /// </summary>
        private TypeActivite DeterminerType(Dictionary<string, string> ligne)
        {
            var estJalon = GetValueOrDefault(ligne, "EstJalon", "False");
            if (bool.TryParse(estJalon, out bool jalon) && jalon)
                return TypeActivite.JalonUtilisateur;

            var type = GetValueOrDefault(ligne, "Type", "");
            if (!string.IsNullOrEmpty(type) && type.Contains("Jalon"))
                return TypeActivite.JalonUtilisateur;

            return TypeActivite.Tache;
        }

        /// <summary>
        /// Remappe les dépendances
        /// </summary>
        private string RemapperDependances(string dependances, Dictionary<string, string> mapping)
        {
            if (string.IsNullOrWhiteSpace(dependances))
                return "";

            var deps = dependances.Split(',')
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrEmpty(d) && mapping.ContainsKey(d))
                .Select(d => mapping[d]);

            return string.Join(",", deps);
        }
    }
}