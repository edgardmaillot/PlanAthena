using PlanAthena.Data;
using PlanAthena.Services.Business;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service de découpage COMPLET avec gestion des dépendances métier
    /// </summary>
    public class DecoupageTachesService
    {
        private readonly MetierService _metierService;
        private const int HEURE_LIMITE_DECOUPAGE = 8;
        private const int MAX_HEURES_PAR_SOUS_TACHE = 7;

        public DecoupageTachesService(MetierService metierService)
        {
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
        }

        /// <summary>
        /// MÉTHODE PRINCIPALE: Prépare les tâches du chef pour le solveur
        /// 1. Matérialise les dépendances métier
        /// 2. Crée les jalons de synchronisation
        /// 3. Découpe les tâches longues
        /// 4. Crée jalons techniques si nécessaire
        /// </summary>
        public List<TacheRecord> PreparerPourSolveur(IReadOnlyList<TacheRecord> tachesChef)
        {
            if (tachesChef == null || !tachesChef.Any())
                return new List<TacheRecord>();

            // Étape 1: Matérialiser les dépendances métier
            var tachesAvecDependancesMetier = MaterialiserDependancesMetier(tachesChef);

            // Étape 2: Créer les jalons de synchronisation pour les fins de métier
            var (tachesAvecJalonsSync, jalonsSync) = CreerJalonsSynchronisation(tachesAvecDependancesMetier);
            var toutesLesTaches = tachesAvecJalonsSync.Concat(jalonsSync).ToList();

            // Étape 3: Découper les tâches longues
            var (tachesDecoupees, tableDecoupage) = DecouperTachesLongues(toutesLesTaches);

            // Étape 4: Créer jalons techniques pour convergence multiple
            var tachesAvecJalonsTechniques = CreerJalonsTechniques(tachesDecoupees, tableDecoupage);

            // Étape 5: Mettre à jour les dépendances après découpage
            var tachesFinales = MettreAJourDependances(tachesAvecJalonsTechniques, tableDecoupage);

            return tachesFinales;
        }

        /// <summary>
        /// POUR L'IHM: Traite les tâches pour l'affichage (avec jalons de sync mais sans découpage)
        /// </summary>
        public List<TacheRecord> TraiterPourIHM(IReadOnlyList<TacheRecord> tachesChef)
        {
            if (tachesChef == null || !tachesChef.Any())
                return new List<TacheRecord>();

            // Étape 1: Matérialiser les dépendances métier
            var tachesAvecDependancesMetier = MaterialiserDependancesMetier(tachesChef);

            // Étape 2: Créer les jalons de synchronisation pour les fins de métier
            var (tachesAvecJalonsSync, jalonsSync) = CreerJalonsSynchronisation(tachesAvecDependancesMetier);
            var toutesLesTaches = tachesAvecJalonsSync.Concat(jalonsSync).ToList();

            return toutesLesTaches;
        }

        /// <summary>
        /// Matérialise les dépendances héritées des métiers
        /// </summary>
        private List<TacheRecord> MaterialiserDependancesMetier(IReadOnlyList<TacheRecord> taches)
        {
            var tachesModifiees = new List<TacheRecord>();
            var indexTaches = taches.ToDictionary(t => t.TacheId);

            foreach (var tache in taches)
            {
                var tacheModifiee = CopierTache(tache);
                var dependancesFinales = new HashSet<string>();

                // Ajouter les dépendances directes existantes
                if (!string.IsNullOrEmpty(tache.Dependencies))
                {
                    var dependancesDirectes = tache.Dependencies.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d));

                    foreach (var dep in dependancesDirectes)
                    {
                        dependancesFinales.Add(dep);
                    }
                }

                // Ajouter les dépendances héritées des métiers (sauf pour les jalons)
                if (!string.IsNullOrEmpty(tache.MetierId) && !_metierService.EstJalon(tache))
                {
                    var prerequisMetiers = _metierService.GetPrerequisForMetier(tache.MetierId);
                    var exclusions = ObtenirExclusions(tache);

                    foreach (var prerequisMetier in prerequisMetiers)
                    {
                        // Trouver les tâches du même bloc qui utilisent ce métier prérequis
                        var tachesPrerequisBloc = taches
                            .Where(t => t.BlocId == tache.BlocId &&
                                       t.MetierId == prerequisMetier)
                            .ToList();

                        foreach (var tachePrerequisBloc in tachesPrerequisBloc)
                        {
                            // Vérifier si cette dépendance n'est pas exclue
                            if (!exclusions.Contains(tachePrerequisBloc.TacheId))
                            {
                                dependancesFinales.Add(tachePrerequisBloc.TacheId);
                            }
                        }
                    }
                }

                tacheModifiee.Dependencies = string.Join(",", dependancesFinales.OrderBy(d => d));
                tachesModifiees.Add(tacheModifiee);
            }

            return tachesModifiees;
        }

        /// <summary>
        /// Crée des jalons de synchronisation SEULEMENT quand c'est utile
        /// </summary>
        private (List<TacheRecord> TachesMisesAJour, List<TacheRecord> JalonsCreees) CreerJalonsSynchronisation(
    IReadOnlyList<TacheRecord> taches)
        {
            var tachesParBloc = taches.GroupBy(t => t.BlocId).ToList();
            var tachesMisesAJourGlobal = new List<TacheRecord>();
            var jalonsGlobaux = new List<TacheRecord>();
            var mapTaches = taches.ToDictionary(t => t.TacheId);

            foreach (var groupeBloc in tachesParBloc)
            {
                var blocId = groupeBloc.Key;
                var tachesDuBloc = groupeBloc.ToList();
                var noeudsDeSynchro = new Dictionary<string, string>();
                var jalonsDuBloc = new List<TacheRecord>();

                // Identifier les métiers qui nécessitent des jalons
                var metiersAvecDependants = new Dictionary<string, List<string>>();

                foreach (var tache in tachesDuBloc)
                {
                    if (string.IsNullOrEmpty(tache.Dependencies)) continue;

                    var dependancesIds = tache.Dependencies.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d));

                    foreach (var depId in dependancesIds)
                    {
                        if (mapTaches.TryGetValue(depId, out var tachePrecedente) &&
                            !string.IsNullOrEmpty(tachePrecedente.MetierId) &&
                            tachePrecedente.MetierId != tache.MetierId &&
                            !_metierService.EstJalon(tachePrecedente))
                        {
                            if (!metiersAvecDependants.ContainsKey(tachePrecedente.MetierId))
                                metiersAvecDependants[tachePrecedente.MetierId] = new List<string>();

                            metiersAvecDependants[tachePrecedente.MetierId].Add(tache.TacheId);
                        }
                    }
                }

                // Créer les jalons nécessaires
                foreach (var kvp in metiersAvecDependants)
                {
                    var metierId = kvp.Key;
                    var tachesDependantes = kvp.Value;
                    var tachesDuMetier = tachesDuBloc.Where(t => t.MetierId == metierId).ToList();

                    // Créer jalon si nécessaire
                    bool aPlusieurseTaches = tachesDuMetier.Count > 1;
                    bool aPlusiersDependants = tachesDependantes.Count > 1;

                    if (aPlusieurseTaches || aPlusiersDependants)
                    {
                        string idJalon = NettoyerIdJalon($"J_Sync_{metierId}_{blocId}");
                        var tacheDeReference = tachesDuMetier.First();

                        // CORRECTION: Vérifier si une surcharge utilisateur existe
                        var surchargeUtilisateur = taches.FirstOrDefault(t => t.TacheId == idJalon && _metierService.EstJalon(t));

                        TacheRecord jalon;
                        if (surchargeUtilisateur != null)
                        {
                            // Utiliser la surcharge utilisateur
                            jalon = CopierTache(surchargeUtilisateur);
                            // IMPORTANT: Recalculer les dépendances automatiquement
                            jalon.Dependencies = string.Join(",", tachesDuMetier.Select(t => t.TacheId));
                        }
                        else
                        {
                            // Créer le jalon technique par défaut
                            jalon = new TacheRecord
                            {
                                TacheId = idJalon,
                                TacheNom = $"Fin du métier {metierId} dans le bloc {tacheDeReference.BlocNom}",
                                HeuresHommeEstimees = 0,
                                Dependencies = string.Join(",", tachesDuMetier.Select(t => t.TacheId)),
                                ExclusionsDependances = "",
                                BlocId = blocId,
                                BlocNom = tacheDeReference.BlocNom,
                                MetierId = _metierService.GetJalonMetierId(),
                                LotId = tacheDeReference.LotId,
                                LotNom = tacheDeReference.LotNom,
                                LotPriorite = tacheDeReference.LotPriorite,
                                BlocCapaciteMaxOuvriers = tacheDeReference.BlocCapaciteMaxOuvriers
                            };
                        }

                        jalonsDuBloc.Add(jalon);
                        noeudsDeSynchro[metierId] = idJalon;
                    }
                }

                // Mettre à jour les dépendances des tâches
                var tachesMisesAJourDuBloc = new List<TacheRecord>();
                foreach (var tache in tachesDuBloc)
                {
                    // CORRECTION: Exclure les jalons J_Sync_ pour éviter les boucles
                    if (tache.TacheId.StartsWith("J_Sync_"))
                    {
                        tachesMisesAJourDuBloc.Add(CopierTache(tache));
                        continue;
                    }

                    var dependancesInitiales = tache.Dependencies?.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d))
                        .ToList() ?? new List<string>();

                    var nouvellesDependances = new List<string>();
                    var metiersDejaLies = new HashSet<string>();

                    foreach (var depId in dependancesInitiales)
                    {
                        if (mapTaches.TryGetValue(depId, out var tachePrecedente) &&
                            !string.IsNullOrEmpty(tachePrecedente.MetierId) &&
                            tachePrecedente.MetierId != tache.MetierId &&
                            noeudsDeSynchro.TryGetValue(tachePrecedente.MetierId, out var idJalon))
                        {
                            if (metiersDejaLies.Add(tachePrecedente.MetierId))
                            {
                                nouvellesDependances.Add(idJalon);
                            }
                        }
                        else
                        {
                            nouvellesDependances.Add(depId);
                        }
                    }

                    var tacheMiseAJour = CopierTache(tache);
                    tacheMiseAJour.Dependencies = string.Join(",", nouvellesDependances.Distinct());
                    tachesMisesAJourDuBloc.Add(tacheMiseAJour);
                }

                tachesMisesAJourGlobal.AddRange(tachesMisesAJourDuBloc);
                jalonsGlobaux.AddRange(jalonsDuBloc);
            }

            return (tachesMisesAJourGlobal, jalonsGlobaux);
        }

        /// <summary>
        /// Nettoie l'ID d'un jalon pour éviter les problèmes avec les espaces et caractères spéciaux
        /// </summary>
        private string NettoyerIdJalon(string idOriginal)
        {
            return idOriginal
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace(".", "_")
                .Replace("/", "_")
                .Replace("\\", "_");
        }
        /// <summary>
        /// Découpe les tâches longues en sous-tâches
        /// </summary>
        private (List<TacheRecord> TachesDecoupees, Dictionary<string, List<string>> TableDecoupage)
            DecouperTachesLongues(IReadOnlyList<TacheRecord> taches)
        {
            var tachesDecoupees = new List<TacheRecord>();
            var tableDecoupage = new Dictionary<string, List<string>>();

            foreach (var tache in taches)
            {
                // Ne jamais découper les jalons
                if (_metierService.EstJalon(tache))
                {
                    tachesDecoupees.Add(CopierTache(tache));
                    continue;
                }

                // Découper si trop longue
                if (tache.HeuresHommeEstimees > HEURE_LIMITE_DECOUPAGE)
                {
                    var sousTaches = DecouperTacheUnique(tache);
                    tachesDecoupees.AddRange(sousTaches);
                    tableDecoupage[tache.TacheId] = sousTaches.Select(st => st.TacheId).ToList();
                }
                else
                {
                    tachesDecoupees.Add(CopierTache(tache));
                }
            }

            return (tachesDecoupees, tableDecoupage);
        }

        /// <summary>
        /// Découpe une tâche en sous-tâches séquentielles
        /// </summary>
        private List<TacheRecord> DecouperTacheUnique(TacheRecord tacheOriginale)
        {
            var sousTaches = new List<TacheRecord>();
            int heuresRestantes = tacheOriginale.HeuresHommeEstimees;
            int compteur = 1;

            while (heuresRestantes > 0)
            {
                int heuresPourCeBloc = Math.Min(heuresRestantes, MAX_HEURES_PAR_SOUS_TACHE);
                string nouvelId = $"{tacheOriginale.TacheId}_P{compteur}";

                var sousTache = CopierTache(tacheOriginale);
                sousTache.TacheId = nouvelId;
                sousTache.TacheNom = $"{tacheOriginale.TacheNom} (Partie {compteur})";
                sousTache.HeuresHommeEstimees = heuresPourCeBloc;

                // Séquencement: P1 garde les dépendances originales, P2 dépend de P1, etc.
                if (compteur == 1)
                {
                    sousTache.Dependencies = tacheOriginale.Dependencies;
                }
                else
                {
                    sousTache.Dependencies = $"{tacheOriginale.TacheId}_P{compteur - 1}";
                }

                sousTaches.Add(sousTache);
                heuresRestantes -= heuresPourCeBloc;
                compteur++;
            }

            return sousTaches;
        }

        /// <summary>
        /// Crée des jalons techniques quand plusieurs tâches dépendent d'une tâche découpée
        /// </summary>
        private List<TacheRecord> CreerJalonsTechniques(List<TacheRecord> taches,
            Dictionary<string, List<string>> tableDecoupage)
        {
            var tachesFinales = new List<TacheRecord>(taches);
            var jalonsCreees = new Dictionary<string, string>();

            // Analyser quelles tâches découpées ont plusieurs dépendants
            var dependants = new Dictionary<string, List<string>>();

            foreach (var tache in taches)
            {
                if (string.IsNullOrEmpty(tache.Dependencies)) continue;

                var deps = tache.Dependencies.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d));
                foreach (var dep in deps)
                {
                    if (tableDecoupage.ContainsKey(dep))
                    {
                        if (!dependants.ContainsKey(dep))
                            dependants[dep] = new List<string>();
                        dependants[dep].Add(tache.TacheId);
                    }
                }
            }

            // Créer jalons techniques pour les tâches avec multiple dépendants
            foreach (var kvp in dependants.Where(d => d.Value.Count > 1))
            {
                string tacheOriginaleId = kvp.Key;
                if (!tableDecoupage.TryGetValue(tacheOriginaleId, out var sousTaches)) continue;

                string idJalonTechnique = $"JT_{tacheOriginaleId}";
                var dernierePartie = sousTaches.Last();
                var tacheRef = taches.First(t => t.TacheId == dernierePartie);

                var jalonTechnique = new TacheRecord
                {
                    TacheId = idJalonTechnique,
                    TacheNom = $"Fin de {tacheOriginaleId}",
                    HeuresHommeEstimees = 0,
                    MetierId = _metierService.GetJalonMetierId(),
                    Dependencies = dernierePartie,
                    BlocId = tacheRef.BlocId,
                    BlocNom = tacheRef.BlocNom,
                    LotId = tacheRef.LotId,
                    LotNom = tacheRef.LotNom,
                    LotPriorite = tacheRef.LotPriorite,
                    BlocCapaciteMaxOuvriers = tacheRef.BlocCapaciteMaxOuvriers
                };

                tachesFinales.Add(jalonTechnique);
                jalonsCreees[tacheOriginaleId] = idJalonTechnique;
            }

            // Mettre à jour la table de découpage pour inclure les jalons
            foreach (var kvp in jalonsCreees)
            {
                tableDecoupage[kvp.Key] = new List<string> { kvp.Value };
            }

            return tachesFinales;
        }

        /// <summary>
        /// Met à jour les dépendances après découpage
        /// </summary>
        private List<TacheRecord> MettreAJourDependances(List<TacheRecord> taches,
            Dictionary<string, List<string>> tableDecoupage)
        {
            var tachesFinales = new List<TacheRecord>();

            foreach (var tache in taches)
            {
                if (string.IsNullOrEmpty(tache.Dependencies))
                {
                    tachesFinales.Add(tache);
                    continue;
                }

                var anciennesDeps = tache.Dependencies.Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d));

                var nouvellesDeps = new List<string>();

                foreach (var dep in anciennesDeps)
                {
                    if (tableDecoupage.TryGetValue(dep, out var nouvelleDep))
                    {
                        nouvellesDeps.Add(nouvelleDep.Last());
                    }
                    else
                    {
                        nouvellesDeps.Add(dep);
                    }
                }

                var tacheMiseAJour = CopierTache(tache);
                tacheMiseAJour.Dependencies = string.Join(",", nouvellesDeps.Distinct());
                tachesFinales.Add(tacheMiseAJour);
            }

            return tachesFinales;
        }

        /// <summary>
        /// Obtient la liste des exclusions pour une tâche
        /// </summary>
        private HashSet<string> ObtenirExclusions(TacheRecord tache)
        {
            if (string.IsNullOrEmpty(tache.ExclusionsDependances))
                return new HashSet<string>();

            return new HashSet<string>(
                tache.ExclusionsDependances.Split(',')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
            );
        }

        /// <summary>
        /// Copie une tâche
        /// </summary>
        private TacheRecord CopierTache(TacheRecord source)
        {
            return new TacheRecord
            {
                TacheId = source.TacheId,
                TacheNom = source.TacheNom,
                HeuresHommeEstimees = source.HeuresHommeEstimees,
                MetierId = source.MetierId,
                Dependencies = source.Dependencies,
                ExclusionsDependances = source.ExclusionsDependances,
                LotId = source.LotId,
                LotNom = source.LotNom,
                LotPriorite = source.LotPriorite,
                BlocId = source.BlocId,
                BlocNom = source.BlocNom,
                BlocCapaciteMaxOuvriers = source.BlocCapaciteMaxOuvriers
            };
        }

        /// <summary>
        /// Statistiques de découpage compatibles avec l'existant
        /// </summary>
        public StatistiquesDecoupage ObtenirStatistiques(IReadOnlyList<TacheRecord> tachesChef,
            IReadOnlyList<TacheRecord> tachesSolveur)
        {
            var tachesChefSansJalons = tachesChef.Count(t => !_metierService.EstJalon(t));
            var tachesSolveurSansJalons = tachesSolveur.Count(t => !_metierService.EstJalon(t));
            var jalonsChef = tachesChef.Count(t => _metierService.EstJalon(t));
            var jalonsSolveur = tachesSolveur.Count(t => _metierService.EstJalon(t));
            var tachesLongues = tachesChef.Count(t => !_metierService.EstJalon(t) && t.HeuresHommeEstimees > HEURE_LIMITE_DECOUPAGE);

            return new StatistiquesDecoupage
            {
                NombreTachesOriginales = tachesChef.Count,
                NombreTachesDecoupees = tachesSolveur.Count,
                TachesOriginalesSansJalons = tachesChefSansJalons,
                TachesDecoupeesSansJalons = tachesSolveurSansJalons,
                TachesLonguesDecoupees = tachesLongues,
                SousTachesCreees = tachesSolveurSansJalons - tachesChefSansJalons + tachesLongues,
                TauxDecoupage = tachesChefSansJalons > 0 ? (double)tachesLongues / tachesChefSansJalons * 100 : 0,
                JalonsTechniquesCreees = jalonsSolveur - jalonsChef
            };
        }

        /// <summary>
        /// ANCIENNE MÉTHODE - Gardée pour compatibilité
        /// </summary>
        public List<TacheRecord> Decouper(IReadOnlyList<TacheRecord> tachesLogiques)
        {
            return PreparerPourSolveur(tachesLogiques);
        }
    }

    /// <summary>
    /// Statistiques sur le découpage des tâches
    /// </summary>
    public class StatistiquesDecoupage
    {
        public int NombreTachesOriginales { get; set; }
        public int NombreTachesDecoupees { get; set; }
        public int TachesOriginalesSansJalons { get; set; }
        public int TachesDecoupeesSansJalons { get; set; }
        public int TachesLonguesDecoupees { get; set; }
        public int SousTachesCreees { get; set; }
        public double TauxDecoupage { get; set; }
        public int JalonsTechniquesCreees { get; set; }
    }
}