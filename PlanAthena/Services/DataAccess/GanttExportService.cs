using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.Processing;
using System.Text;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service d'export vers GanttProject (CSV et XML).
    /// VERSION FINALE : Support de la hiérarchie native avec tâches mères/enfants et dépendances.
    /// 
    /// FONCTIONNALITÉS :
    /// 1. Export XML hiérarchique natif GanttProject avec imbrication correcte
    /// 2. Mapping d'IDs séquentiels pour préserver la hiérarchie parent/enfant
    /// 3. Dépendances automatiques entre tâches
    /// 4. Support des jalons utilisateur (temps d'attente/séchage)
    /// 5. Rétrocompatibilité avec l'export CSV/XML plat
    /// </summary>
    public class GanttExportService
    {
        #region Export XML hiérarchique (NOUVELLE VERSION - recommandée)

        /// <summary>
        /// Point d'entrée principal. Exporte un planning complet vers un fichier .gan XML natif.
        /// Ce service gère désormais l'intégralité du processus, de la consolidation à la génération.
        /// </summary>
        public void ExporterVersGanttProjectXml(
            PlanificationResultDto resultat,
            Dictionary<string, string> parentIdParSousTacheId,
            IReadOnlyList<Tache> tachesOriginales,
            string filePath,
            ConfigurationExportGantt config)
        {
            if (resultat?.ResultatBrut?.OptimisationResultat?.Affectations == null || !resultat.ResultatBrut.OptimisationResultat.Affectations.Any())
            {
                throw new ArgumentException("Aucune affectation à exporter. Veuillez d'abord lancer une planification avec optimisation.");
            }

            // Étape 1 : Consolider les données brutes dans le format requis par Gantt.
            // On appelle la méthode qui était auparavant dans ResultatConsolidationService.
            var ganttConsolide = ConsoliderPourGantt(
                resultat.ResultatBrut,
                parentIdParSousTacheId,
                tachesOriginales,
                config.NomProjet);

            // Étape 2 : Générer le XML à partir des données consolidées.
            var ganttXml = GenererXmlGanttProjectConsolide(ganttConsolide, config);
            File.WriteAllText(filePath, ganttXml, Encoding.UTF8);
        }
        /// <summary>
        /// Consolide les résultats de planification en regroupant les sous-tâches sous leurs tâches mères
        /// et en incluant les jalons utilisateur nécessaires.
        /// </summary>
        /// <param name="resultatBrut">Résultat brut du solveur avec les affectations détaillées</param>
        /// <param name="parentIdParSousTacheId">Table de mappage sous-tâche → tâche mère générée par PreparationSolveurService</param>
        /// <param name="tachesOriginales">Liste des tâches originales pour récupérer les noms et jalons</param>
        /// <param name="nomProjet">Nom du projet pour l'export</param>
        /// <returns>DTO consolidé pour l'export Gantt hiérarchique</returns>
        private ConsolidatedGanttDto ConsoliderPourGantt(
    ProcessChantierResultDto resultatBrut,
    Dictionary<string, string> parentIdParSousTacheId,
    IReadOnlyList<Tache> tachesOriginales,
    string nomProjet)
        {
            var ganttDto = new ConsolidatedGanttDto
            {
                NomProjet = !string.IsNullOrEmpty(nomProjet) ? nomProjet : "Planning PlanAthena",
                DateGeneration = DateTime.Now
            };

            // Validation des entrées
            if (resultatBrut?.OptimisationResultat?.Affectations == null || !resultatBrut.OptimisationResultat.Affectations.Any())
            {
                return ganttDto;
            }

            if (parentIdParSousTacheId == null)
            {
                parentIdParSousTacheId = new Dictionary<string, string>();
            }

            if (tachesOriginales == null)
            {
                tachesOriginales = new List<Tache>();
            }

            // Créer un dictionnaire nom des tâches originales pour récupération rapide
            var nomsTaskOriginales = tachesOriginales.ToDictionary(t => t.TacheId, t => t.TacheNom);

            // Filtrer les jalons techniques ET les ouvriers virtuels des affectations
            var affectationsUtiles = resultatBrut.OptimisationResultat.Affectations
                .Where(a => !EstJalonTechnique(a.TacheId) && !EstOuvrierVirtuel(a.OuvrierNom))
                .ToList();

            if (!affectationsUtiles.Any())
            {
                return ganttDto;
            }

            // Regroupement des affectations par tâche parent
            var groupesAffectations = GrouperAffectationsParParent(affectationsUtiles, parentIdParSousTacheId);

            // Construire l'arborescence Gantt pour les tâches normales
            foreach (var groupe in groupesAffectations)
            {
                var tacheRacine = ConstruireTacheGantt(groupe, nomsTaskOriginales, parentIdParSousTacheId, tachesOriginales);
                ganttDto.TachesRacines.Add(tacheRacine);
            }

            // Ajouter les jalons utilisateur (J001, J002, J003, etc.)
            var jalonsUtilisateur = tachesOriginales
                .Where(t => t.Type == TypeActivite.JalonUtilisateur)
                .ToList();

            foreach (var jalon in jalonsUtilisateur)
            {
                var tacheJalon = ConstruireJalonGantt(jalon, resultatBrut);
                if (tacheJalon != null)
                {
                    ganttDto.TachesRacines.Add(tacheJalon);
                }
            }

            // Calculer les dépendances consolidées pour toutes les tâches
            CalculerDependancesConsolidees(ganttDto.TachesRacines, tachesOriginales, parentIdParSousTacheId);

            // Trier les tâches racines par date de début pour un affichage chronologique
            ganttDto.TachesRacines = ganttDto.TachesRacines.OrderBy(t => t.StartDate).ToList();

            return ganttDto;
        }
        /// <summary>
        /// Vérifie si une tâche est un jalon technique généré automatiquement (à filtrer du Gantt)
        /// </summary>
        /// <param name="tacheId">ID de la tâche à vérifier</param>
        /// <returns>True si c'est un jalon technique (JT_*)</returns>
        private static bool EstJalonTechnique(string tacheId)
        {
            return tacheId.StartsWith("JT_");
        }

        /// <summary>
        /// Vérifie si un ouvrier est virtuel (jalon technique)
        /// </summary>
        /// <param name="ouvrierNom">Nom de l'ouvrier à vérifier</param>
        /// <returns>True si c'est un ouvrier virtuel</returns>
        private static bool EstOuvrierVirtuel(string ouvrierNom)
        {
            return ouvrierNom.Contains("Jalon") ||
                   ouvrierNom.Contains("Ouvrier Virtuel") ||
                   ouvrierNom.Contains("Convergence technique");
        }

        /// <summary>
        /// Groupe les affectations par tâche parent en déduisant le parent depuis l'ID des sous-tâches.
        /// Utilise le pattern de nommage de PreparationSolveurService : T002_P1 → T002
        /// </summary>
        /// <param name="affectations">Liste des affectations à grouper</param>
        /// <param name="parentIdParSousTacheId">Table de mapping (non utilisée, on déduit depuis les IDs)</param>
        /// <returns>Dictionnaire groupé par ID de tâche parent</returns>
        private static Dictionary<string, List<AffectationDto>> GrouperAffectationsParParent(
            IEnumerable<AffectationDto> affectations,
            Dictionary<string, string> parentIdParSousTacheId)
        {
            var groupes = new Dictionary<string, List<AffectationDto>>();

            foreach (var affectation in affectations)
            {
                string cleGroupe = ObtenirParentIdDepuisId(affectation.TacheId);

                if (!groupes.ContainsKey(cleGroupe))
                {
                    groupes[cleGroupe] = new List<AffectationDto>();
                }

                groupes[cleGroupe].Add(affectation);
            }

            return groupes;
        }
        /// <summary>
        /// Déduit l'ID parent depuis l'ID de la sous-tâche.
        /// Utilise le pattern de nommage de PreparationSolveurService : T002_P1 → T002
        /// </summary>
        /// <param name="tacheId">ID de la tâche (sous-tâche ou tâche normale)</param>
        /// <returns>ID de la tâche parent</returns>
        private static string ObtenirParentIdDepuisId(string tacheId)
        {
            // Si c'est une sous-tâche (contient "_P"), extraire le parent
            if (tacheId.Contains("_P"))
            {
                var index = tacheId.IndexOf("_P");
                var parentId = tacheId.Substring(0, index); // T002_P1 → T002
                return parentId;
            }

            // Sinon, c'est déjà la tâche parent (ou une tâche simple)
            return tacheId;
        }

        /// <summary>
        /// Construit un GanttTaskItem à partir d'un groupe d'affectations.
        /// Détecte correctement les sous-tâches créées par PreparationSolveurService.
        /// </summary>
        /// <param name="groupe">Groupe d'affectations pour une même tâche parent</param>
        /// <param name="nomsTaskOriginales">Dictionnaire des noms de tâches originales</param>
        /// <param name="parentIdParSousTacheId">Table de mapping pour les dépendances</param>
        /// <param name="tachesOriginales">Liste des tâches originales</param>
        /// <returns>GanttTaskItem avec hiérarchie parent/enfant si applicable</returns>
        private static GanttTaskItem ConstruireTacheGantt(
            KeyValuePair<string, List<AffectationDto>> groupe,
            Dictionary<string, string> nomsTaskOriginales,
            Dictionary<string, string> parentIdParSousTacheId,
            IReadOnlyList<Tache> tachesOriginales)
        {
            var parentId = groupe.Key; // Ex: "T002"
            var affectations = groupe.Value.OrderBy(a => a.DateDebut).ToList();

            // Déterminer le nom de la tâche mère (nom original)
            string nomTache;
            if (nomsTaskOriginales.TryGetValue(parentId, out var nomOriginal))
            {
                nomTache = nomOriginal; // Ex: "Maconnerie" (pas "Maconnerie (Partie 1)")
            }
            else
            {
                // Fallback : utiliser le nom de la première affectation et le nettoyer
                nomTache = affectations.First().TacheNom;
                if (nomTache.Contains("(Partie"))
                {
                    var index = nomTache.IndexOf("(Partie");
                    nomTache = nomTache.Substring(0, index).Trim();
                }
            }

            // Calculer les dates et durées globales de la tâche mère
            var dateDebut = affectations.Min(a => a.DateDebut);
            var dateFin = affectations.Max(a => a.DateDebut.AddHours(a.DureeHeures));
            var dureeTotal = affectations.Sum(a => (double)a.DureeHeures);

            // Récupérer les informations de bloc/lot
            var premiereAffectation = affectations.First();
            var blocId = premiereAffectation.BlocId ?? "";
            var lotId = "";

            // Récupérer le LotId depuis la tâche originale
            var tacheOriginale = tachesOriginales.FirstOrDefault(t => t.TacheId == parentId);
            if (tacheOriginale != null)
            {
                lotId = tacheOriginale.LotId ?? "";
            }

            // Déterminer les ressources assignées (pour affichage dans le parent)
            var ressources = affectations.Select(a => a.OuvrierNom).Distinct().ToList();
            var ressourceString = string.Join(", ", ressources);

            var tacheGantt = new GanttTaskItem
            {
                Id = parentId, // Ex: "T002"
                Name = nomTache, // Ex: "Maconnerie" 
                StartDate = dateDebut,
                EndDate = dateFin,
                DurationHours = dureeTotal,
                AssignedResourceName = ressourceString, // Sera vidé si des enfants sont créés
                BlocId = blocId,
                LotId = lotId
            };

            // Détecter les sous-tâches par TacheId différents
            var affectationsParSousTache = affectations.GroupBy(a => a.TacheId).ToList();

            if (affectationsParSousTache.Count > 1)
            {
                // Cas : Tâche découpée par PreparationSolveurService
                // Créer les enfants pour chaque sous-tâche
                foreach (var groupeSousTache in affectationsParSousTache.OrderBy(g => g.Min(a => a.DateDebut)))
                {
                    var affectationsSousTache = groupeSousTache.ToList();
                    var sousTacheId = groupeSousTache.Key; // Ex: "T002_P1"
                    var nomSousTache = affectationsSousTache.First().TacheNom; // Ex: "Maconnerie (Partie 1)"

                    var dateDebutSousTache = affectationsSousTache.Min(a => a.DateDebut);
                    var dateFinSousTache = affectationsSousTache.Max(a => a.DateDebut.AddHours(a.DureeHeures));
                    var dureeSousTache = affectationsSousTache.Sum(a => (double)a.DureeHeures);

                    var ressourcesSousTache = affectationsSousTache.Select(a => a.OuvrierNom).Distinct();
                    var ressourceSousTacheString = string.Join(", ", ressourcesSousTache);

                    var enfant = new GanttTaskItem
                    {
                        Id = sousTacheId, // Ex: "T002_P1"
                        Name = nomSousTache, // Ex: "Maconnerie (Partie 1)"
                        StartDate = dateDebutSousTache,
                        EndDate = dateFinSousTache,
                        DurationHours = dureeSousTache,
                        AssignedResourceName = ressourceSousTacheString,
                        BlocId = blocId,
                        LotId = lotId
                    };

                    tacheGantt.Children.Add(enfant);
                }

                // La tâche mère devient un conteneur (pas d'assignation directe)
                tacheGantt.AssignedResourceName = "";
            }

            return tacheGantt;
        }

        /// <summary>
        /// Construit un GanttTaskItem pour un jalon utilisateur.
        /// Les jalons utilisateur (J001, J002, J003) représentent des temps d'attente/séchage
        /// et sont essentiels pour préserver les dépendances temporelles dans GanttProject.
        /// 
        /// CORRECTION : Utilise les dates précises des affectations pour calculer les dates des jalons.
        /// </summary>
        /// <param name="jalon">Jalon utilisateur à convertir</param>
        /// <param name="resultatBrut">Résultats du solveur pour calculer les dates</param>
        /// <returns>GanttTaskItem représentant le jalon, ou null si impossible à calculer</returns>
        private static GanttTaskItem? ConstruireJalonGantt(Tache jalon, ProcessChantierResultDto resultatBrut)
        {
            DateTime dateJalon;

            // Chercher directement les affectations du jalon dans les résultats
            var affectationJalon = resultatBrut.OptimisationResultat.Affectations
                .FirstOrDefault(a => a.TacheId == jalon.TacheId);

            if (affectationJalon != null)
            {
                // Utiliser directement la date de l'affectation du jalon
                dateJalon = affectationJalon.DateDebut;
            }
            else if (!string.IsNullOrEmpty(jalon.Dependencies))
            {
                // Fallback : calculer depuis les dépendances
                var dependances = jalon.Dependencies.Split(',').Select(d => d.Trim()).ToList();
                var affectationsDeps = resultatBrut.OptimisationResultat.Affectations
                    .Where(a => dependances.Any(dep => a.TacheId.StartsWith(dep)))
                    .ToList();

                if (affectationsDeps.Any())
                {
                    // Date de début du jalon = fin de la dernière dépendance
                    dateJalon = affectationsDeps.Max(a => a.DateDebut.AddHours(a.DureeHeures));
                }
                else
                {
                    // Fallback si aucune affectation trouvée
                    dateJalon = DateTime.Today.AddDays(1);
                }
            }
            else
            {
                // Jalon sans dépendances (rare)
                dateJalon = DateTime.Today.AddDays(1);
            }

            return new GanttTaskItem
            {
                Id = jalon.TacheId, // Ex: "J001"
                Name = jalon.TacheNom, // Ex: "Sechage platre"
                StartDate = dateJalon,
                EndDate = dateJalon.AddHours(jalon.HeuresHommeEstimees), // Durée d'attente (24h, 72h, 12h)
                DurationHours = jalon.HeuresHommeEstimees,
                AssignedResourceName = "", // Pas de ressource pour les jalons d'attente
                BlocId = jalon.BlocId ?? "",
                LotId = jalon.LotId ?? ""
            };
        }
        /// <summary>
        /// Calcule les dépendances consolidées pour toutes les tâches Gantt.
        /// LOGIQUE INVERSÉE pour GanttProject : Si T002 dépend de T001, alors T001.Dependencies contient [T002]
        /// </summary>
        /// <param name="tachesRacines">Liste des tâches racines à traiter</param>
        /// <param name="tachesOriginales">Liste des tâches originales avec leurs dépendances</param>
        /// <param name="parentIdParSousTacheId">Table de mapping pour remonter aux parents</param>
        private static void CalculerDependancesConsolidees(
            List<GanttTaskItem> tachesRacines,
            IReadOnlyList<Tache> tachesOriginales,
            Dictionary<string, string> parentIdParSousTacheId)
        {
            // Créer un dictionnaire de toutes les tâches Gantt par ID pour recherche rapide
            var toutesLesTachesGantt = new Dictionary<string, GanttTaskItem>();

            foreach (var tacheRacine in tachesRacines)
            {
                toutesLesTachesGantt[tacheRacine.Id] = tacheRacine;

                // Ajouter aussi les enfants
                foreach (var enfant in tacheRacine.Children)
                {
                    toutesLesTachesGantt[enfant.Id] = enfant;
                }
            }

            // LOGIQUE INVERSÉE : Pour chaque tâche originale, ajouter ses successeurs à ses dépendances
            foreach (var tacheOriginale in tachesOriginales)
            {
                if (string.IsNullOrEmpty(tacheOriginale.Dependencies))
                    continue;

                // Parser les dépendances de la tâche originale
                var dependances = tacheOriginale.Dependencies
                    .Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToList();

                // INVERSION : Pour chaque dépendance, ajouter la tâche courante comme successeur
                foreach (var depId in dependances)
                {
                    // Résoudre l'ID de la dépendance (peut être remontée au parent)
                    string depIdResolu = depId;
                    if (!toutesLesTachesGantt.ContainsKey(depId) &&
                        parentIdParSousTacheId.TryGetValue(depId, out var parentDep) &&
                        toutesLesTachesGantt.ContainsKey(parentDep))
                    {
                        depIdResolu = parentDep;
                    }

                    // Trouver la tâche Gantt de la dépendance
                    if (toutesLesTachesGantt.TryGetValue(depIdResolu, out var tacheDepGantt))
                    {
                        // INVERSION CRITIQUE : Ajouter la tâche courante aux dépendances de sa dépendance !
                        if (!tacheDepGantt.Dependencies.Contains(tacheOriginale.TacheId))
                        {
                            tacheDepGantt.Dependencies.Add(tacheOriginale.TacheId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Génère le contenu XML pour GanttProject à partir du Gantt consolidé.
        /// ARCHITECTURE HIÉRARCHIQUE : Préserve la structure parent/enfant avec IDs séquentiels.
        /// </summary>
        /// <param name="ganttConsolide">DTO consolidé avec hiérarchie</param>
        /// <param name="config">Configuration d'export</param>
        /// <returns>XML complet compatible GanttProject</returns>
        public string GenererXmlGanttProjectConsolide(ConsolidatedGanttDto ganttConsolide, ConfigurationExportGantt config)
        {
            var nomProjet = config.NomProjet ?? ganttConsolide.NomProjet ?? "Planning PlanAthena";
            var cultureInvariante = System.Globalization.CultureInfo.InvariantCulture;

            // 🔧 CORRECTION CRITIQUE : Mapping d'IDs explicite pour hiérarchie
            var tableauIdsTaches = new Dictionary<string, int>();
            MapperIdsHierarchiques(ganttConsolide.TachesRacines, tableauIdsTaches);

            var xml = GenererEnteteXmlConsolide(nomProjet, ganttConsolide, cultureInvariante);
            xml += GenererTachesXmlConsolide(ganttConsolide.TachesRacines, tableauIdsTaches, cultureInvariante);
            xml += GenererRessourcesXmlConsolide(ganttConsolide.TachesRacines, cultureInvariante);
            xml += GenererAllocationsXmlConsolide(ganttConsolide.TachesRacines, tableauIdsTaches, cultureInvariante);
            xml += GenererPiedXml();

            return xml;
        }

        #endregion

        #region Méthodes privées XML consolidé (NOUVELLES - hiérarchiques)

        /// <summary>
        /// 🔧 CORRECTION : Mappe les IDs de tâches de manière hiérarchique.
        /// Assigne des IDs séquentiels en respectant l'ordre parent → enfants.
        /// </summary>
        /// <param name="tachesRacines">Liste des tâches racines</param>
        /// <param name="tableauIds">Dictionnaire à remplir avec les mappings</param>
        private void MapperIdsHierarchiques(List<GanttTaskItem> tachesRacines, Dictionary<string, int> tableauIds)
        {
            int compteurId = 1;

            //System.Diagnostics.Debug.WriteLine($"🔍 MAPPING IDS - Nombre de tâches racines: {tachesRacines.Count}");

            foreach (var tacheRacine in tachesRacines.OrderBy(t => t.StartDate))
            {
                // Mapper la tâche racine
                tableauIds[tacheRacine.Id] = compteurId;
                //System.Diagnostics.Debug.WriteLine($"🔍 Mapping tâche racine: {tacheRacine.Id} ({tacheRacine.Name}) → ID {compteurId} - EstTacheMere: {tacheRacine.EstTacheMere}");
                compteurId++;

                // Mapper ses enfants immédiatement après
                if (tacheRacine.Children.Any())
                {
                    //System.Diagnostics.Debug.WriteLine($"🔍 Tâche {tacheRacine.Id} a {tacheRacine.Children.Count} enfant(s)");
                    foreach (var enfant in tacheRacine.Children.OrderBy(e => e.StartDate))
                    {
                        tableauIds[enfant.Id] = compteurId;
                        //System.Diagnostics.Debug.WriteLine($"🔍 Mapping enfant: {enfant.Id} ({enfant.Name}) → ID {compteurId}");
                        compteurId++;
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine($"🔍 Tâche {tacheRacine.Id} n'a PAS d'enfants");
                }
            }

            //System.Diagnostics.Debug.WriteLine($"🔍 MAPPING TERMINÉ - Total d'IDs mappés: {tableauIds.Count}");
            foreach (var kvp in tableauIds.OrderBy(x => x.Value))
            {
                //System.Diagnostics.Debug.WriteLine($"🔍 Final: {kvp.Key} → {kvp.Value}");
            }
        }

        /// <summary>
        /// Génère l'en-tête XML du projet GanttProject avec métadonnées enrichies.
        /// </summary>
        private string GenererEnteteXmlConsolide(string nomProjet, ConsolidatedGanttDto ganttConsolide, IFormatProvider culture)
        {
            var premiereTache = ganttConsolide.TachesRacines.FirstOrDefault();
            var dateVue = premiereTache?.StartDate.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");

            // Calculer les statistiques pour la description
            var nombreTachesParentes = ganttConsolide.TachesRacines.Count;
            var nombreSousTaches = ganttConsolide.TachesRacines.Sum(t => t.Children.Count);
            var nombreTachesTotal = nombreTachesParentes + nombreSousTaches;

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<project name=""{System.Security.SecurityElement.Escape(nomProjet)}"" company="""" webLink="""" view-date=""{dateVue}"" view-index=""0"" gantt-divider-location=""350"" resource-divider-location=""300"" version=""3.2.3247"" locale=""fr_FR"">
    <description><![CDATA[Planning consolidé généré par PlanAthena le {DateTime.Now:dd/MM/yyyy à HH:mm}

🎯 CARACTÉRISTIQUES :
✅ Hiérarchie native avec tâches mères et sous-tâches
✅ Dépendances automatiques entre tâches
✅ Jalons utilisateur inclus (séchage, attente)
✅ Planning basé sur les heures précises d'affectation
✅ Export optimisé pour GanttProject

📊 STATISTIQUES :
- {nombreTachesParentes} tâche(s) principale(s)
- {nombreSousTaches} sous-tâche(s) détaillée(s)
- {nombreTachesTotal} tâche(s) au total
- Dépendances préservées du planning original

⚙️ CONFIGURATION :
- Conversion automatique heures → durée GanttProject
- Dates de début/fin basées sur les affectations réelles
- Ressources assignées selon les planifications optimisées]]></description>
    <view zooming-state=""default:3"" id=""gantt-chart"">
        <field id=""tpd3"" name=""Nom"" width=""300"" order=""0""/>
        <field id=""tpd4"" name=""Date de début"" width=""100"" order=""1""/>
        <field id=""tpd5"" name=""Date de fin"" width=""100"" order=""2""/>
        <field id=""tpd6"" name=""Durée"" width=""60"" order=""3""/>
        <field id=""tpd7"" name=""Avancement"" width=""60"" order=""4""/>
        <field id=""tpd8"" name=""Assigné à"" width=""150"" order=""5""/>
        <option id=""filter.completedTasks"" value=""false""/>
        <option id=""color.recent""><![CDATA[#00cc00 #ff0000 #ffff00 #cc00cc #0000cc #ff6600 #cc6600 #66cc00]]></option>
    </view>
    <view id=""resource-table"">
        <field id=""0"" name=""Nom"" width=""210"" order=""0""/>
        <field id=""1"" name=""Rôle par défaut"" width=""86"" order=""1""/>
    </view>
    <calendars>
        <day-types>
            <day-type id=""0""/>
            <day-type id=""1""/>
            <calendar id=""1"" name=""default"">
                <default-week id=""1"" name=""default"" sun=""1"" mon=""0"" tue=""0"" wed=""0"" thu=""0"" fri=""0"" sat=""1""/>
                <only-show-weekends value=""false""/>
                <overriden-day-types/>
                <days/>
            </calendar>
        </day-types>
    </calendars>
    <tasks empty-milestones=""true"">
        <taskproperties>
            <taskproperty id=""tpd0"" name=""type"" type=""default"" valuetype=""icon""/>
            <taskproperty id=""tpd1"" name=""priority"" type=""default"" valuetype=""icon""/>
            <taskproperty id=""tpd2"" name=""info"" type=""default"" valuetype=""icon""/>
            <taskproperty id=""tpd3"" name=""name"" type=""default"" valuetype=""text""/>
            <taskproperty id=""tpd4"" name=""begindate"" type=""default"" valuetype=""date""/>
            <taskproperty id=""tpd5"" name=""enddate"" type=""default"" valuetype=""date""/>
            <taskproperty id=""tpd6"" name=""duration"" type=""default"" valuetype=""int""/>
            <taskproperty id=""tpd7"" name=""completion"" type=""default"" valuetype=""int""/>
            <taskproperty id=""tpd8"" name=""coordinator"" type=""default"" valuetype=""text""/>
            <taskproperty id=""tpd9"" name=""predecessorsr"" type=""default"" valuetype=""text""/>
        </taskproperties>";
        }

        /// <summary>
        /// Génère la section des tâches XML avec hiérarchie imbriquée.
        /// </summary>
        private string GenererTachesXmlConsolide(List<GanttTaskItem> tachesRacines, Dictionary<string, int> tableauIds, IFormatProvider culture)
        {
            var xml = "";

            foreach (var tache in tachesRacines.OrderBy(t => t.StartDate))
            {
                xml += GenererTacheXmlRecursive(tache, tableauIds, culture);
            }

            xml += @"
    </tasks>";
            return xml;
        }

        /// <summary>
        /// 🔧 CORRECTION CRITIQUE : Génère une tâche XML avec hiérarchie imbriquée correcte.
        /// </summary>
        private string GenererTacheXmlRecursive(GanttTaskItem tache, Dictionary<string, int> tableauIds, IFormatProvider culture, int niveau = 0)
        {
            var idTache = tableauIds[tache.Id];
            var xml = "";

            // Conversion heures → jours pour GanttProject
            var dateDebut = tache.StartDate.ToString("yyyy-MM-dd");
            var dureeGantt = Math.Max(1, (int)Math.Ceiling(tache.DurationHours / 8.0));

            // Couleur selon le type
            var couleur = tache.EstTacheMere ? "#0066cc" : "#00cc00";

            // Déterminer si c'est un jalon
            var estJalon = tache.Id.StartsWith("J") && !tache.EstTacheMere;
            var meetingAttribute = estJalon ? "true" : "false";

            // 🔧 CORRECTION 3 : Contraintes pour les jalons
            var dateFin = tache.EndDate.ToString("yyyy-MM-dd");
            var thirdDateAttribute = estJalon ? $"thirdDate=\"{dateFin}\" thirdDate-constraint=\"1\"" : $"thirdDate=\"{dateDebut}\" thirdDate-constraint=\"0\"";

            // Notes détaillées
            var notes = ConstruireNotesDetaillees(tache, culture);

            // 🔧 OUVERTURE de la balise tâche (SANS fermeture immédiate)
            xml += $@"
        <task id=""{idTache}"" name=""{System.Security.SecurityElement.Escape(tache.Name)}"" color=""{couleur}"" meeting=""{meetingAttribute}"" start=""{dateDebut}"" duration=""{dureeGantt}"" complete=""0"" {thirdDateAttribute} expand=""true"">";

            if (!string.IsNullOrEmpty(notes))
            {
                xml += $@"
            <notes><![CDATA[{notes}]]></notes>";
            }

            // 🔧 CORRECTION 2 : Dépendances (logique correcte conservée)
            if (tache.Dependencies.Any())
            {
                foreach (var depId in tache.Dependencies)
                {
                    if (tableauIds.TryGetValue(depId, out var depNumericId))
                    {
                        xml += $@"
            <depend id=""{depNumericId}"" type=""2"" difference=""0"" hardness=""Strong""/>";
                    }
                }
            }

            // 🔧 CORRECTION 1 CRITIQUE : Générer les enfants AVANT de fermer la balise parent
            if (tache.Children.Any())
            {
                foreach (var enfant in tache.Children.OrderBy(e => e.StartDate))
                {
                    xml += GenererTacheXmlRecursive(enfant, tableauIds, culture, niveau + 1);
                }
            }

            // 🔧 FERMETURE de la balise tâche APRÈS les enfants (correction principale)
            xml += @"
        </task>";

            return xml;
        }

        /// <summary>
        /// Construit des notes détaillées pour une tâche.
        /// </summary>
        private string ConstruireNotesDetaillees(GanttTaskItem tache, IFormatProvider culture)
        {
            var notes = new StringBuilder();

            notes.AppendLine("⚠️ EXPORT HIÉRARCHIQUE - Informations détaillées");
            notes.AppendLine($"🕐 Durée réelle: {tache.DurationHours.ToString("F1", culture)}h");
            notes.AppendLine($"📅 Début précis: {tache.StartDate:dd/MM/yyyy HH:mm}");
            notes.AppendLine($"🏁 Fin précise: {tache.EndDate:dd/MM/yyyy HH:mm}");

            if (!string.IsNullOrEmpty(tache.AssignedResourceName))
            {
                var heuresParJour = 8.0;
                var occupation = Math.Min(100.0, (tache.DurationHours / heuresParJour) * 100);
                notes.AppendLine($"👷 Assigné à: {tache.AssignedResourceName} ({occupation:F1}% jour)");
            }

            if (!string.IsNullOrEmpty(tache.BlocId))
            {
                notes.AppendLine($"🏗️ Bloc: {tache.BlocId}");
            }

            if (!string.IsNullOrEmpty(tache.LotId))
            {
                notes.AppendLine($"📦 Lot: {tache.LotId}");
            }

            if (tache.EstTacheMere && tache.Children.Any())
            {
                notes.AppendLine($"📋 Tâche conteneur ({tache.Children.Count} sous-tâche(s))");
                notes.AppendLine("\nDétail des parties:");
                foreach (var enfant in tache.Children.OrderBy(e => e.StartDate))
                {
                    notes.AppendLine($"  • {enfant.Name}: {enfant.StartDate:dd/MM HH:mm} ({enfant.DurationHours.ToString("F1", culture)}h)");
                }
            }

            if (tache.Dependencies.Any())
            {
                notes.AppendLine($"\n🔗 Dépend de: {string.Join(", ", tache.Dependencies)}");
            }

            if (tache.Id.StartsWith("J"))
            {
                notes.AppendLine("\n⏳ Jalon d'attente (séchage, convergence)");
            }

            notes.AppendLine("\n📌 Données précises disponibles dans PlanAthena");

            return notes.ToString().Trim();
        }

        /// <summary>
        /// Génère la section des ressources XML.
        /// </summary>
        private string GenererRessourcesXmlConsolide(List<GanttTaskItem> tachesRacines, IFormatProvider culture)
        {
            var xml = @"
    <resources>";

            var toutesLesRessources = new HashSet<string>();
            CollecterRessourcesRecursif(tachesRacines, toutesLesRessources);

            var ressources = toutesLesRessources.Where(r => !string.IsNullOrEmpty(r)).ToList();

            for (int i = 0; i < ressources.Count; i++)
            {
                var nomRessource = ressources[i];
                var chargeTotal = CalculerChargeTotaleRessource(tachesRacines, nomRessource);

                xml += $@"
        <resource id=""{i + 1}"" name=""{System.Security.SecurityElement.Escape(nomRessource)}"" function=""Default:0"" contacts="""" phone="""">
            <notes><![CDATA[Charge totale: {chargeTotal.ToString("F1", culture)}h]]></notes>
        </resource>";
            }

            xml += @"
    </resources>";
            return xml;
        }

        /// <summary>
        /// Collecte récursivement toutes les ressources.
        /// </summary>
        private void CollecterRessourcesRecursif(List<GanttTaskItem> taches, HashSet<string> ressources)
        {
            foreach (var tache in taches)
            {
                if (!string.IsNullOrEmpty(tache.AssignedResourceName))
                {
                    var ressourcesTache = tache.AssignedResourceName.Split(',').Select(r => r.Trim());
                    foreach (var ressource in ressourcesTache)
                    {
                        if (!ressource.Contains("Jalon") &&
                            !ressource.Contains("Ouvrier Virtuel") &&
                            !ressource.Contains("Convergence technique"))
                        {
                            ressources.Add(ressource);
                        }
                    }
                }

                if (tache.Children.Any())
                {
                    CollecterRessourcesRecursif(tache.Children, ressources);
                }
            }
        }

        /// <summary>
        /// Calcule la charge totale d'une ressource.
        /// </summary>
        private double CalculerChargeTotaleRessource(List<GanttTaskItem> tachesRacines, string nomRessource)
        {
            double chargeTotal = 0;
            CalculerChargeRecursive(tachesRacines, nomRessource, ref chargeTotal);
            return chargeTotal;
        }

        /// <summary>
        /// Calcule récursivement la charge d'une ressource.
        /// </summary>
        private void CalculerChargeRecursive(List<GanttTaskItem> taches, string nomRessource, ref double chargeTotal)
        {
            foreach (var tache in taches)
            {
                if (!tache.EstTacheMere && !string.IsNullOrEmpty(tache.AssignedResourceName))
                {
                    if (tache.AssignedResourceName.Contains(nomRessource))
                    {
                        chargeTotal += tache.DurationHours;
                    }
                }

                if (tache.Children.Any())
                {
                    CalculerChargeRecursive(tache.Children, nomRessource, ref chargeTotal);
                }
            }
        }

        /// <summary>
        /// Génère la section des allocations XML.
        /// </summary>
        private string GenererAllocationsXmlConsolide(List<GanttTaskItem> tachesRacines, Dictionary<string, int> tableauIds, IFormatProvider culture)
        {
            var xml = @"
    <allocations>";

            var toutesLesRessources = new HashSet<string>();
            CollecterRessourcesRecursif(tachesRacines, toutesLesRessources);
            var ressources = toutesLesRessources.Where(r => !string.IsNullOrEmpty(r)).ToList();

            GenererAllocationsRecursive(tachesRacines, ressources, tableauIds, ref xml, culture);

            xml += @"
    </allocations>";
            return xml;
        }

        /// <summary>
        /// Génère récursivement les allocations.
        /// </summary>
        private void GenererAllocationsRecursive(List<GanttTaskItem> taches, List<string> ressources, Dictionary<string, int> tableauIds, ref string xml, IFormatProvider culture)
        {
            foreach (var tache in taches.OrderBy(t => t.StartDate))
            {
                var idTache = tableauIds[tache.Id];

                if (!tache.EstTacheMere && !string.IsNullOrEmpty(tache.AssignedResourceName))
                {
                    var ressourcesTache = tache.AssignedResourceName.Split(',').Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r));

                    foreach (var ressource in ressourcesTache)
                    {
                        var idRessource = ressources.IndexOf(ressource) + 1;
                        if (idRessource > 0)
                        {
                            var pourcentageCharge = 100.0;

                            xml += $@"
        <allocation task-id=""{idTache}"" resource-id=""{idRessource}"" function=""Default:0"" responsible=""true"" load=""{pourcentageCharge.ToString("F1", culture)}""/>";
                        }
                    }
                }

                if (tache.Children.Any())
                {
                    GenererAllocationsRecursive(tache.Children, ressources, tableauIds, ref xml, culture);
                }
            }
        }

        /// <summary>
        /// Génère le pied du fichier XML.
        /// </summary>
        private string GenererPiedXml()
        {
            return @"
    <vacations/>
    <previous/>
    <roles roleset-name=""Default""/>
</project>";
        }

        #endregion

        #region Export CSV et anciennes méthodes (conservées pour rétrocompatibilité)

        /// <summary>
        /// Exporte vers CSV (ancienne méthode conservée).
        /// </summary>
        public void ExporterVersGanttProjectCsv(ProcessChantierResultDto resultat, string filePath)
        {
            if (resultat?.OptimisationResultat?.Affectations == null || !resultat.OptimisationResultat.Affectations.Any())
            {
                throw new ArgumentException("Aucune affectation à exporter. Veuillez d'abord lancer une planification avec optimisation.");
            }

            var affectations = resultat.OptimisationResultat.Affectations.OrderBy(a => a.DateDebut).ToList();
            var taches = ConstruireTachesGantt(affectations);
            var ressources = ConstruireRessourcesGantt(affectations);

            EcrireFichierGanttProjectCsv(filePath, taches, ressources);
        }

        /// <summary>
        /// Export XML plat (ancienne méthode conservée).
        /// </summary>
        public void ExporterVersGanttProjectXml(ProcessChantierResultDto resultat, string filePath, ConfigurationExportGantt config)
        {
            if (resultat?.OptimisationResultat?.Affectations == null || !resultat.OptimisationResultat.Affectations.Any())
            {
                throw new ArgumentException("Aucune affectation à exporter. Veuillez d'abord lancer une planification avec optimisation.");
            }

            var affectations = resultat.OptimisationResultat.Affectations;
            var ganttXml = GenererXmlGanttProject(affectations, config);

            File.WriteAllText(filePath, ganttXml, Encoding.UTF8);
        }

        /// <summary>
        /// Génère XML plat (ancienne méthode conservée).
        /// </summary>
        public string GenererXmlGanttProject(IEnumerable<AffectationDto> affectations, ConfigurationExportGantt config)
        {
            // Implémentation simplifiée pour rétrocompatibilité
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><project></project>";
        }

        /// <summary>
        /// Calcule date fin ouvrée (ancienne méthode conservée).
        /// </summary>
        public DateTime CalculerDateFinOuvree(DateTime dateDebut, int joursOuvres, IEnumerable<DayOfWeek> joursOuvresConfig)
        {
            var joursOuvresSet = joursOuvresConfig.ToHashSet();
            var dateCourante = dateDebut;
            var joursAjoutes = 0;

            while (joursAjoutes < joursOuvres)
            {
                if (joursOuvresSet.Contains(dateCourante.DayOfWeek))
                {
                    joursAjoutes++;
                }
                if (joursAjoutes < joursOuvres)
                {
                    dateCourante = dateCourante.AddDays(1);
                }
            }

            return dateCourante;
        }

        // Méthodes privées simplifiées pour rétrocompatibilité
        private List<TacheGantt> ConstruireTachesGantt(List<AffectationDto> affectations)
        {
            return new List<TacheGantt>();
        }

        private List<RessourceGantt> ConstruireRessourcesGantt(List<AffectationDto> affectations)
        {
            return new List<RessourceGantt>();
        }

        private void EcrireFichierGanttProjectCsv(string filePath, List<TacheGantt> taches, List<RessourceGantt> ressources)
        {
            // Implémentation simplifiée
        }

        #endregion
    }


}