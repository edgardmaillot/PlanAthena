// NOUVEL EMPLACEMENT : PlanAthena/Services/Export/GanttExportService.cs
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.ImportExport;
using System.Globalization;
using System.Security;
using System.Text;

namespace PlanAthena.Services.Export
{
    /// <summary>
    /// Service d'export vers GanttProject (XML natif .gan).
    /// Agit comme un présentateur pur : reçoit un DTO complet et se concentre exclusivement
    /// sur la transformation des données en format XML hiérarchique, sans logique métier.
    /// </summary>
    public class GanttExportService
    {
        // Structure de données interne et temporaire pour faciliter la construction de la hiérarchie Gantt.
        private class GanttTaskItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string AssignedResourceName { get; set; } = "";
            public double DurationHours { get; set; }
            public bool IsMilestone { get; set; }
            public bool IsContainer => Children.Any();
            public List<GanttTaskItem> Children { get; set; } = new List<GanttTaskItem>();
            public List<string> Dependencies { get; set; } = new List<string>();
        }

        /// <summary>
        /// Point d'entrée principal. Exporte les données du projet vers un fichier .gan XML natif de GanttProject.
        /// </summary>
        /// <param name="exportData">Le DTO complet contenant toutes les informations nécessaires pour l'export.</param>
        /// <param name="filePath">Le chemin complet du fichier .gan à générer.</param>
        public void ExporterVersGanttProjectXml(ExportDataProjetDto exportData, string filePath)
        {
            ArgumentNullException.ThrowIfNull(exportData, nameof(exportData));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Le chemin du fichier ne peut pas être vide.", nameof(filePath));

            var tachesGantt = _CreerHierarchieGantt(exportData);
            var ganttXml = _GenererXmlGanttProject(tachesGantt, exportData.Report, exportData.NomProjet);
            File.WriteAllText(filePath, ganttXml, Encoding.UTF8);
        }

        #region Méthodes de Génération XML

        private string _GenererXmlGanttProject(List<GanttTaskItem> taches, AnalysisReport report, string nomProjet)
        {
            var cultureInvariante = CultureInfo.InvariantCulture;
            var idMap = new Dictionary<string, int>();

            _MapperIdsHierarchiques(taches, idMap);

            var sb = new StringBuilder();
            sb.Append(_GenererEnteteXml(nomProjet, taches));
            sb.Append(_GenererTachesXml(taches, idMap, cultureInvariante));
            sb.Append(_GenererRessourcesXml(report, cultureInvariante));
            sb.Append(_GenererAllocationsXml(taches, idMap, report, cultureInvariante));
            sb.Append(_GenererPiedXml());

            return sb.ToString();
        }

        private string _GenererEnteteXml(string nomProjet, List<GanttTaskItem> taches)
        {
            var premiereTache = taches.Where(t => t.StartDate != DateTime.MinValue).OrderBy(t => t.StartDate).FirstOrDefault();
            var dateVue = premiereTache?.StartDate.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<project name=""{SecurityElement.Escape(nomProjet)}"" company="""" webLink="""" view-date=""{dateVue}"" view-index=""0"" gantt-divider-location=""350"" resource-divider-location=""300"" version=""3.2.3247"" locale=""fr_FR"">
    <description><![CDATA[Planning généré par PlanAthena le {DateTime.Now:dd/MM/yyyy à HH:mm}]]></description>
    <view zooming-state=""default:3"" id=""gantt-chart""><field id=""tpd3"" name=""Nom"" width=""300"" order=""0""/><field id=""tpd4"" name=""Date de début"" width=""100"" order=""1""/><field id=""tpd5"" name=""Date de fin"" width=""100"" order=""2""/></view>
    <calendars><day-types><day-type id=""0""/><day-type id=""1""/><calendar id=""1"" name=""default""><default-week sun=""1"" mon=""0"" tue=""0"" wed=""0"" thu=""0"" fri=""0"" sat=""1""/></calendar></day-types></calendars>
    <tasks empty-milestones=""true""><taskproperties><taskproperty id=""tpd0"" name=""type"" type=""default"" valuetype=""icon""/><taskproperty id=""tpd3"" name=""name"" type=""default"" valuetype=""text""/><taskproperty id=""tpd4"" name=""begindate"" type=""default"" valuetype=""date""/><taskproperty id=""tpd5"" name=""enddate"" type=""default"" valuetype=""date""/><taskproperty id=""tpd9"" name=""predecessorsr"" type=""default"" valuetype=""text""/></taskproperties>";
        }

        private string _GenererTachesXml(List<GanttTaskItem> taches, Dictionary<string, int> idMap, IFormatProvider culture)
        {
            var sb = new StringBuilder();
            foreach (var tache in taches.OrderBy(t => t.StartDate))
            {
                _GenererTacheXmlRecursive(tache, idMap, culture, sb);
            }
            sb.AppendLine("    </tasks>");
            return sb.ToString();
        }

        private void _GenererTacheXmlRecursive(GanttTaskItem tache, Dictionary<string, int> idMap, IFormatProvider culture, StringBuilder sb)
        {
            var idTache = idMap[tache.Id];
            var dateDebut = tache.StartDate.ToString("yyyy-MM-dd");
            var dureeGantt = Math.Max(1, (int)Math.Ceiling(tache.DurationHours / 8.0));
            var couleur = tache.IsContainer ? "#0066cc" : (tache.IsMilestone ? "#cc00cc" : "#00cc00");
            var isMeeting = tache.IsMilestone && !tache.IsContainer ? "true" : "false";

            sb.AppendLine($@"        <task id=""{idTache}"" name=""{SecurityElement.Escape(tache.Name)}"" color=""{couleur}"" meeting=""{isMeeting}"" start=""{dateDebut}"" duration=""{dureeGantt}"" complete=""0"" expand=""true"">");
            sb.AppendLine($@"            <notes><![CDATA[{_ConstruireNotesDetaillees(tache, culture)}]]></notes>");

            foreach (var depId in tache.Dependencies)
            {
                if (idMap.TryGetValue(depId, out var depNumericId))
                {
                    sb.AppendLine($@"            <depend id=""{depNumericId}"" type=""2"" difference=""0"" hardness=""Strong""/>");
                }
            }

            foreach (var enfant in tache.Children.OrderBy(e => e.StartDate))
            {
                _GenererTacheXmlRecursive(enfant, idMap, culture, sb);
            }
            sb.AppendLine("        </task>");
        }

        private string _GenererRessourcesXml(AnalysisReport report, IFormatProvider culture)
        {
            var sb = new StringBuilder();
            sb.AppendLine("    <resources>");
            int resourceIdCounter = 1;
            foreach (var analyseOuvrier in report.AnalysesOuvriers.Where(o => o.HeuresTravaillees > 0).OrderBy(o => o.NomComplet))
            {
                sb.AppendLine($@"        <resource id=""{resourceIdCounter++}"" name=""{SecurityElement.Escape(analyseOuvrier.NomComplet)}"" function=""Default:0""><notes><![CDATA[Charge totale: {analyseOuvrier.HeuresTravaillees.ToString("F1", culture)}h]]></notes></resource>");
            }
            sb.AppendLine("    </resources>");
            return sb.ToString();
        }

        private string _GenererAllocationsXml(List<GanttTaskItem> taches, Dictionary<string, int> idMap, AnalysisReport report, IFormatProvider culture)
        {
            var sb = new StringBuilder();
            sb.AppendLine("    <allocations>");
            var resourceMap = report.AnalysesOuvriers.Where(o => o.HeuresTravaillees > 0).OrderBy(o => o.NomComplet)
                .Select((o, index) => new { o.NomComplet, Id = index + 1 }).ToDictionary(r => r.NomComplet, r => r.Id);
            _GenererAllocationsRecursive(taches, idMap, resourceMap, sb);
            sb.AppendLine("    </allocations>");
            return sb.ToString();
        }

        private void _GenererAllocationsRecursive(IEnumerable<GanttTaskItem> taches, Dictionary<string, int> taskIdMap, Dictionary<string, int> resourceMap, StringBuilder sb)
        {
            foreach (var tache in taches)
            {
                if (!tache.IsContainer && !tache.IsMilestone && !string.IsNullOrEmpty(tache.AssignedResourceName))
                {
                    if (taskIdMap.TryGetValue(tache.Id, out var taskId) && resourceMap.TryGetValue(tache.AssignedResourceName, out var resourceId))
                    {
                        sb.AppendLine($@"        <allocation task-id=""{taskId}"" resource-id=""{resourceId}"" function=""Default:0"" responsible=""true"" load=""100.0""/>");
                    }
                }
                if (tache.Children.Any())
                {
                    _GenererAllocationsRecursive(tache.Children, taskIdMap, resourceMap, sb);
                }
            }
        }

        private string _GenererPiedXml() => @"    <vacations/><previous/><roles roleset-name=""Default""/></project>";

        #endregion

        #region Méthodes de Préparation des Données

        private List<GanttTaskItem> _CreerHierarchieGantt(ExportDataProjetDto exportData)
        {
            var mapTachesGantt = new Dictionary<string, GanttTaskItem>();
            var mapOuvrierIdToName = exportData.Report.AnalysesOuvriers.ToDictionary(o => o.OuvrierId, o => o.NomComplet);

            // Étape 1: Créer une instance de GanttTaskItem pour chaque tâche définie dans le projet.
            foreach (var tacheOriginale in exportData.ProjetStructure.Taches)
            {
                mapTachesGantt[tacheOriginale.TacheId] = new GanttTaskItem
                {
                    Id = tacheOriginale.TacheId,
                    Name = tacheOriginale.TacheNom,
                    Dependencies = tacheOriginale.Dependencies?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>(),
                    IsMilestone = tacheOriginale.EstJalon
                };
            }

            // Étape 2: Traiter les jalons planifiés. Ils ne sont pas découpés.
            foreach (var jalon in exportData.Planning.JalonsPlanifies)
            {
                if (mapTachesGantt.TryGetValue(jalon.TacheId, out var item))
                {
                    item.StartDate = jalon.DateDebut;
                    item.EndDate = jalon.DateFin;
                    item.DurationHours = jalon.DureeHeures;
                }
            }

            // Étape 3: Traiter les segments de travail pour créer les sous-tâches et enrichir les tâches mères.
            var allSegments = exportData.Planning.SegmentsParOuvrierId.Values.SelectMany(s => s);
            var segmentsParTacheMere = allSegments.GroupBy(s => s.ParentTacheId ?? s.TacheId);

            foreach (var groupe in segmentsParTacheMere)
            {
                if (mapTachesGantt.TryGetValue(groupe.Key, out var tacheMere))
                {
                    foreach (var segment in groupe)
                    {
                        tacheMere.Children.Add(new GanttTaskItem
                        {
                            Id = segment.TacheId,
                            Name = segment.TacheNom,
                            StartDate = segment.Jour.Add(segment.HeureDebut),
                            EndDate = segment.Jour.Add(segment.HeureFin),
                            DurationHours = segment.HeuresTravaillees,
                            AssignedResourceName = mapOuvrierIdToName.GetValueOrDefault(segment.OuvrierId, segment.OuvrierId)
                        });
                    }
                    tacheMere.StartDate = tacheMere.Children.Min(c => c.StartDate);
                    tacheMere.EndDate = tacheMere.Children.Max(c => c.EndDate);
                    tacheMere.DurationHours = tacheMere.Children.Sum(c => c.DurationHours);
                }
            }

            // Étape 4: Résoudre les dépendances pour qu'elles pointent vers les bonnes entités (tâche finale ou jalon).
            foreach (var item in mapTachesGantt.Values)
            {
                var nouvellesDeps = new List<string>();
                foreach (var depId in item.Dependencies)
                {
                    if (mapTachesGantt.TryGetValue(depId, out var dependance))
                    {
                        if (dependance.IsContainer && dependance.Children.Any())
                        {
                            var derniereSousTache = dependance.Children.OrderBy(c => c.EndDate).LastOrDefault();
                            if (derniereSousTache != null) nouvellesDeps.Add(derniereSousTache.Id);
                        }
                        else
                        {
                            nouvellesDeps.Add(dependance.Id);
                        }
                    }
                }
                item.Dependencies = nouvellesDeps;
            }

            // Étape 5: Gérer les tâches sans date (typiquement les jalons de synchro vides).
            foreach (var item in mapTachesGantt.Values)
            {
                if (item.StartDate == DateTime.MinValue && item.Dependencies.Any())
                {
                    var maxEndDate = item.Dependencies
                        .Select(depId => mapTachesGantt.TryGetValue(depId, out var depTask) ? depTask.EndDate : DateTime.MinValue)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max();
                    item.StartDate = maxEndDate;
                    item.EndDate = maxEndDate;
                }
            }

            return mapTachesGantt.Values.ToList();
        }

        private string _ConstruireNotesDetaillees(GanttTaskItem tache, IFormatProvider culture)
        {
            var notes = new StringBuilder();
            notes.AppendLine("Informations détaillées PlanAthena");
            notes.AppendLine($"🕐 Durée réelle: {tache.DurationHours.ToString("F1", culture)}h");
            if (tache.StartDate != DateTime.MinValue) notes.AppendLine($"📅 Début précis: {tache.StartDate:dd/MM/yyyy HH:mm}");
            if (tache.EndDate != DateTime.MinValue) notes.AppendLine($"🏁 Fin précise: {tache.EndDate:dd/MM/yyyy HH:mm}");
            if (!string.IsNullOrEmpty(tache.AssignedResourceName)) notes.AppendLine($"👷 Assigné à: {tache.AssignedResourceName}");
            if (tache.IsContainer) notes.AppendLine($"\n📋 Tâche conteneur ({tache.Children.Count} sous-tâche(s))");
            if (tache.Dependencies.Any()) notes.AppendLine($"\n🔗 Dépend de: {string.Join(", ", tache.Dependencies)}");
            return notes.ToString().Trim();
        }

        private void _MapperIdsHierarchiques(List<GanttTaskItem> taches, Dictionary<string, int> idMap)
        {
            int compteurId = 1;
            foreach (var tache in taches.OrderBy(t => t.StartDate))
            {
                if (!idMap.ContainsKey(tache.Id)) idMap[tache.Id] = compteurId++;
                foreach (var enfant in tache.Children.OrderBy(e => e.StartDate))
                {
                    if (!idMap.ContainsKey(enfant.Id)) idMap[enfant.Id] = compteurId++;
                }
            }
        }

        #endregion
    }
}