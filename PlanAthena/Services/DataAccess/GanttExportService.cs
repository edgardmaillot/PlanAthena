using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Services.Processing;
using System.Text;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service d'export vers GanttProject (CSV et XML)
    /// </summary>
    public class GanttExportService
    {
        #region Export CSV (existant - conservé pour compatibilité)

        /// <summary>
        /// Exporte les résultats de planification vers un fichier CSV compatible GanttProject
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

        #endregion

        #region Export XML (nouveau - version consolidée)

        /// <summary>
        /// Exporte le Gantt consolidé vers un fichier .gan XML natif GanttProject
        /// </summary>
        public void ExporterVersGanttProjectXml(ConsolidatedGanttDto ganttConsolide, string filePath, ConfigurationExportGantt config)
        {
            if (ganttConsolide?.TachesRacines == null || !ganttConsolide.TachesRacines.Any())
            {
                throw new ArgumentException("Aucune tâche consolidée à exporter. Veuillez d'abord lancer une planification avec consolidation.");
            }

            var ganttXml = GenererXmlGanttProjectConsolide(ganttConsolide, config);
            File.WriteAllText(filePath, ganttXml, Encoding.UTF8);
        }

        /// <summary>
        /// Génère le contenu XML pour GanttProject à partir du Gantt consolidé
        /// </summary>
        public string GenererXmlGanttProjectConsolide(ConsolidatedGanttDto ganttConsolide, ConfigurationExportGantt config)
        {
            var nomProjet = config.NomProjet ?? ganttConsolide.NomProjet ?? "Planning PlanAthena";
            var cultureInvariante = System.Globalization.CultureInfo.InvariantCulture;

            var xml = GenererEnteteXmlConsolide(nomProjet, ganttConsolide, cultureInvariante);
            xml += GenererTachesXmlConsolide(ganttConsolide.TachesRacines, cultureInvariante);
            xml += GenererRessourcesXmlConsolide(ganttConsolide.TachesRacines, cultureInvariante);
            xml += GenererAllocationsXmlConsolide(ganttConsolide.TachesRacines, cultureInvariante);
            xml += GenererPiedXml();

            return xml;
        }

        #endregion

        #region Méthodes privées XML consolidé

        private string GenererEnteteXmlConsolide(string nomProjet, ConsolidatedGanttDto ganttConsolide, IFormatProvider culture)
        {
            var premiereTache = ganttConsolide.TachesRacines.FirstOrDefault();
            var dateVue = premiereTache?.StartDate.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<project name=""{System.Security.SecurityElement.Escape(nomProjet)}"" company="""" webLink="""" view-date=""{dateVue}"" view-index=""0"" gantt-divider-location=""350"" resource-divider-location=""300"" version=""3.2.3247"" locale=""fr_FR"">
    <description><![CDATA[Planning consolidé généré par PlanAthena le {DateTime.Now:dd/MM/yyyy à HH:mm}
    
Configuration:
- Export hiérarchique avec regroupement des sous-tâches
- Planning basé sur les heures précises
- Conversion automatique heures → durée GanttProject
- {ganttConsolide.TachesRacines.Count} tâche(s) principale(s)
- {ganttConsolide.TachesRacines.Sum(t => t.Children.Count)} sous-tâche(s) au total]]></description>
    <view zooming-state=""default:3"" id=""gantt-chart"">
        <field id=""tpd3"" name=""Nom"" width=""250"" order=""0""/>
        <field id=""tpd4"" name=""Date de début"" width=""100"" order=""1""/>
        <field id=""tpd5"" name=""Date de fin"" width=""100"" order=""2""/>
        <field id=""tpd6"" name=""Durée"" width=""60"" order=""3""/>
        <field id=""tpd7"" name=""Avancement"" width=""60"" order=""4""/>
        <field id=""tpd8"" name=""Assigné à"" width=""150"" order=""5""/>
        <option id=""filter.completedTasks"" value=""false""/>
        <option id=""color.recent""><![CDATA[#00cc00 #ff0000 #ffff00 #cc00cc #0000cc]]></option>
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

        private string GenererTachesXmlConsolide(List<GanttTaskItem> tachesRacines, IFormatProvider culture)
        {
            var xml = "";
            int compteurId = 1;

            foreach (var tache in tachesRacines.OrderBy(t => t.StartDate))
            {
                xml += GenererTacheXmlRecursive(tache, ref compteurId, culture);
            }

            xml += @"
    </tasks>";
            return xml;
        }

        private string GenererTacheXmlRecursive(GanttTaskItem tache, ref int compteurId, IFormatProvider culture, int niveau = 0)
        {
            var idTache = compteurId++;
            var xml = "";

            // CORRECTION TEMPORAIRE : Conversion heures → jours + allocation correcte
            var dateDebut = tache.StartDate.ToString("yyyy-MM-dd");
            var dureeGantt = "1"; // Toujours 1 jour pour GanttProject

            // Déterminer la couleur selon le type de tâche
            var couleur = tache.EstTacheMere ? "#0066cc" : "#00cc00";

            // Construire les notes avec avertissement temporaire
            var notes = ConstruireNotesDetaillees(tache, culture);

            xml += $@"
        <task id=""{idTache}"" name=""{System.Security.SecurityElement.Escape(tache.Name)}"" color=""{couleur}"" meeting=""false"" start=""{dateDebut}"" duration=""{dureeGantt}"" complete=""0"" thirdDate=""{dateDebut}"" thirdDate-constraint=""0"" expand=""true"">";

            if (!string.IsNullOrEmpty(notes))
            {
                xml += $@"
            <notes><![CDATA[{System.Security.SecurityElement.Escape(notes)}]]></notes>";
            }

            // CORRECTION : Les dépendances seront ajoutées à l'étape 2
            // PAS DE DÉPENDANCES ICI pour l'instant

            // HIÉRARCHIE : Si c'est une tâche mère avec des enfants, les générer récursivement
            if (tache.Children.Any())
            {
                foreach (var enfant in tache.Children.OrderBy(e => e.StartDate))
                {
                    xml += GenererTacheXmlRecursive(enfant, ref compteurId, culture, niveau + 1);
                }
            }

            xml += @"
        </task>";

            return xml;
        }

        private string ConstruireNotesDetaillees(GanttTaskItem tache, IFormatProvider culture)
        {
            var notes = new StringBuilder();

            // AVERTISSEMENT TEMPORAIRE sur les approximations
            notes.AppendLine("⚠️ EXPORT TEMPORAIRE - Approximations GanttProject");
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

            if (tache.EstTacheMere)
            {
                notes.AppendLine($"📋 Tâche conteneur ({tache.Children.Count} sous-tâche(s))");

                if (tache.Children.Any())
                {
                    notes.AppendLine("\nDétail des parties:");
                    foreach (var enfant in tache.Children.OrderBy(e => e.StartDate))
                    {
                        notes.AppendLine($"  • {enfant.Name}: {enfant.StartDate:dd/MM HH:mm} ({enfant.DurationHours.ToString("F1", culture)}h)");
                    }
                }
            }

            if (tache.Dependencies.Any())
            {
                notes.AppendLine($"\n🔗 Dépend de: {string.Join(", ", tache.Dependencies)}");
            }

            // TODO: Remplacer par timeline native quand priorité urgente résolue
            notes.AppendLine("\n📌 Dates précises disponibles dans PlanAthena");

            return notes.ToString().Trim();
        }

        private string GenererRessourcesXmlConsolide(List<GanttTaskItem> tachesRacines, IFormatProvider culture)
        {
            var xml = @"
    <resources>";

            // Extraire toutes les ressources de toutes les tâches (y compris les enfants)
            var toutesLesRessources = new HashSet<string>();
            CollecterRessources(tachesRacines, toutesLesRessources);

            var ressources = toutesLesRessources.Where(r => !string.IsNullOrEmpty(r)).ToList();

            for (int i = 0; i < ressources.Count; i++)
            {
                var nomRessource = ressources[i];

                // Calculer la charge totale de cette ressource
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

        private void CollecterRessources(List<GanttTaskItem> taches, HashSet<string> ressources)
        {
            foreach (var tache in taches)
            {
                if (!string.IsNullOrEmpty(tache.AssignedResourceName))
                {
                    // Séparer les ressources multiples et filtrer les ouvriers virtuels
                    var ressourcesTache = tache.AssignedResourceName.Split(',').Select(r => r.Trim());
                    foreach (var ressource in ressourcesTache)
                    {
                        // Filtrer les ouvriers virtuels/jalons
                        if (!ressource.Contains("Jalon") && !ressource.Contains("Ouvrier Virtuel") && !ressource.Contains("Convergence technique"))
                        {
                            ressources.Add(ressource);
                        }
                    }
                }

                if (tache.Children.Any())
                {
                    CollecterRessources(tache.Children, ressources);
                }
            }
        }

        private double CalculerChargeTotaleRessource(List<GanttTaskItem> tachesRacines, string nomRessource)
        {
            double chargeTotal = 0;
            CalculerChargeRecursive(tachesRacines, nomRessource, ref chargeTotal);
            return chargeTotal;
        }

        private void CalculerChargeRecursive(List<GanttTaskItem> taches, string nomRessource, ref double chargeTotal)
        {
            foreach (var tache in taches)
            {
                // Pour les tâches feuilles, ajouter la charge si la ressource correspond
                if (!tache.EstTacheMere && !string.IsNullOrEmpty(tache.AssignedResourceName))
                {
                    if (tache.AssignedResourceName.Contains(nomRessource))
                    {
                        chargeTotal += tache.DurationHours;
                    }
                }

                // Parcourir récursivement les enfants
                if (tache.Children.Any())
                {
                    CalculerChargeRecursive(tache.Children, nomRessource, ref chargeTotal);
                }
            }
        }

        private string GenererAllocationsXmlConsolide(List<GanttTaskItem> tachesRacines, IFormatProvider culture)
        {
            var xml = @"
    <allocations>";

            // Extraire les ressources uniques
            var toutesLesRessources = new HashSet<string>();
            CollecterRessources(tachesRacines, toutesLesRessources);
            var ressources = toutesLesRessources.Where(r => !string.IsNullOrEmpty(r)).ToList();

            int compteurIdTache = 1;
            GenererAllocationsRecursive(tachesRacines, ressources, ref compteurIdTache, ref xml, culture);

            xml += @"
    </allocations>";
            return xml;
        }

        private void GenererAllocationsRecursive(List<GanttTaskItem> taches, List<string> ressources, ref int compteurIdTache, ref string xml, IFormatProvider culture)
        {
            foreach (var tache in taches.OrderBy(t => t.StartDate))
            {
                var idTache = compteurIdTache++;

                // Générer les allocations pour cette tâche
                if (!string.IsNullOrEmpty(tache.AssignedResourceName))
                {
                    var ressourcesTache = tache.AssignedResourceName.Split(',').Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r));

                    foreach (var ressource in ressourcesTache)
                    {
                        var idRessource = ressources.IndexOf(ressource) + 1;
                        if (idRessource > 0)
                        {
                            // Pour les tâches mères, répartir la charge
                            var pourcentageCharge = tache.EstTacheMere ? 50.0 : 100.0;

                            xml += $@"
        <allocation task-id=""{idTache}"" resource-id=""{idRessource}"" function=""Default:0"" responsible=""true"" load=""{pourcentageCharge.ToString("F1", culture)}""/>";
                        }
                    }
                }

                // Traiter récursivement les enfants
                if (tache.Children.Any())
                {
                    GenererAllocationsRecursive(tache.Children, ressources, ref compteurIdTache, ref xml, culture);
                }
            }
        }

        private string GenererPiedXml()
        {
            return @"
    <vacations/>
    <previous/>
    <roles roleset-name=""Default""/>
</project>";
        }

        #endregion

        #region Export XML (ancienne version - conservée pour compatibilité)

        /// <summary>
        /// Exporte vers un fichier .gan XML natif GanttProject (ancienne version)
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
        /// Génère le contenu XML pour GanttProject (ancienne version)
        /// </summary>
        public string GenererXmlGanttProject(IEnumerable<AffectationDto> affectations, ConfigurationExportGantt config)
        {
            var nomProjet = config.NomProjet ?? "Planning PlanAthena";
            var heuresParJour = config.HeuresParJour;
            var cultureInvariante = System.Globalization.CultureInfo.InvariantCulture;

            // Grouper les affectations par tâche
            var tachesGroupees = affectations
                .GroupBy(a => new { a.TacheId, a.TacheNom })
                .Select((groupe, index) =>
                {
                    var affectationsGroupe = groupe.ToList();
                    var dateDebut = affectationsGroupe.Min(a => a.DateDebut.Date);
                    var heuresTotal = affectationsGroupe.Sum(a => a.DureeHeures);
                    var joursOuvres = Math.Max(1, (int)Math.Ceiling(heuresTotal / heuresParJour));
                    var dateFin = CalculerDateFinOuvree(dateDebut, joursOuvres, config.JoursOuvres);

                    return new TacheGroupee
                    {
                        Id = index + 1,
                        Nom = groupe.Key.TacheNom,
                        DateDebut = dateDebut,
                        DateFin = dateFin,
                        DureeJours = joursOuvres,
                        HeuresTotal = heuresTotal,
                        Affectations = affectationsGroupe
                    };
                })
                .OrderBy(t => t.DateDebut)
                .ToList();

            var xml = GenererEnteteXml(nomProjet, tachesGroupees, heuresParJour, cultureInvariante);
            xml += GenererTachesXml(tachesGroupees, heuresParJour, cultureInvariante);
            xml += GenererRessourcesXml(affectations, heuresParJour, cultureInvariante);
            xml += GenererAllocationsXml(tachesGroupees, affectations, heuresParJour, cultureInvariante);
            xml += GenererPiedXml();

            return xml;
        }

        /// <summary>
        /// Calcule la date de fin en respectant les jours ouvrés
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

        #endregion

        #region Méthodes privées XML (ancienne version)

        private string GenererEnteteXml(string nomProjet, IEnumerable<TacheGroupee> taches, double heuresParJour, IFormatProvider culture)
        {
            var premiereTache = taches.FirstOrDefault();
            var dateVue = premiereTache?.DateDebut.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<project name=""{System.Security.SecurityElement.Escape(nomProjet)}"" company="""" webLink="""" view-date=""{dateVue}"" view-index=""0"" gantt-divider-location=""350"" resource-divider-location=""300"" version=""3.2.3247"" locale=""fr_FR"">
    <description><![CDATA[Planning généré par PlanAthena le {DateTime.Now:dd/MM/yyyy à HH:mm}
    
Configuration:
- Heures de travail par jour: {heuresParJour.ToString(culture)}h
- Conversion automatique heures → jours ouvrés
- Calendrier: Lundi-Vendredi (jours ouvrés uniquement)]]></description>
    <view zooming-state=""default:3"" id=""gantt-chart"">
        <field id=""tpd3"" name=""Nom"" width=""200"" order=""0""/>
        <field id=""tpd4"" name=""Date de début"" width=""75"" order=""1""/>
        <field id=""tpd5"" name=""Date de fin"" width=""75"" order=""2""/>
        <field id=""tpd6"" name=""Durée"" width=""50"" order=""3""/>
        <option id=""filter.completedTasks"" value=""false""/>
        <option id=""color.recent""><![CDATA[#00cc00 #ff0000 #ffff00 #cc00cc #0000cc]]></option>
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

        private string GenererTachesXml(IEnumerable<TacheGroupee> tachesGroupees, double heuresParJour, IFormatProvider culture)
        {
            var xml = "";

            foreach (var tache in tachesGroupees)
            {
                var ouvriers = string.Join(", ", tache.Affectations.Select(a => a.OuvrierNom).Distinct());
                var noteDetails = $@"Durée totale: {tache.HeuresTotal.ToString("F1", culture)}h ({tache.DureeJours} jour{(tache.DureeJours > 1 ? "s" : "")} à {heuresParJour.ToString(culture)}h/jour)
Ouvriers assignés: {ouvriers}

Détail des affectations:";

                foreach (var affectation in tache.Affectations.OrderBy(a => a.DateDebut))
                {
                    noteDetails += $@"
- {affectation.OuvrierNom}: {affectation.DateDebut:dd/MM HH:mm} ({affectation.DureeHeures.ToString("F1", culture)}h)";
                }

                xml += $@"
        <task id=""{tache.Id}"" name=""{System.Security.SecurityElement.Escape(tache.Nom)}"" color=""#00cc00"" meeting=""false"" start=""{tache.DateDebut:yyyy-MM-dd}"" duration=""{tache.DureeJours}"" complete=""0"" thirdDate=""{tache.DateDebut:yyyy-MM-dd}"" thirdDate-constraint=""0"" expand=""true"">
            <notes><![CDATA[{System.Security.SecurityElement.Escape(noteDetails)}]]></notes>
        </task>";
            }

            xml += @"
    </tasks>";
            return xml;
        }

        private string GenererRessourcesXml(IEnumerable<AffectationDto> affectations, double heuresParJour, IFormatProvider culture)
        {
            var xml = @"
    <resources>";

            var ouvriersAvecCharge = affectations
                .GroupBy(a => a.OuvrierNom)
                .Select((groupe, index) =>
                {
                    var heuresTotal = groupe.Sum(a => a.DureeHeures);
                    var joursNecessaires = Math.Ceiling(heuresTotal / heuresParJour);
                    return new
                    {
                        Id = index + 1,
                        Nom = groupe.Key,
                        HeuresTotal = heuresTotal,
                        JoursEquivalent = joursNecessaires
                    };
                })
                .ToList();

            foreach (var ouvrier in ouvriersAvecCharge)
            {
                xml += $@"
        <resource id=""{ouvrier.Id}"" name=""{System.Security.SecurityElement.Escape(ouvrier.Nom)}"" function=""Default:0"" contacts="""" phone="""">
            <notes><![CDATA[Charge totale: {ouvrier.HeuresTotal.ToString("F1", culture)}h (≈{ouvrier.JoursEquivalent.ToString("F1", culture)} jours à {heuresParJour.ToString(culture)}h/jour)]]></notes>
        </resource>";
            }

            xml += @"
    </resources>";
            return xml;
        }

        private string GenererAllocationsXml(IEnumerable<TacheGroupee> tachesGroupees, IEnumerable<AffectationDto> affectations, double heuresParJour, IFormatProvider culture)
        {
            var xml = @"
    <allocations>";

            var ouvriersAvecCharge = affectations
                .GroupBy(a => a.OuvrierNom)
                .Select((groupe, index) => new { Id = index + 1, Nom = groupe.Key })
                .ToList();

            foreach (var tache in tachesGroupees)
            {
                var ouvriersParTache = tache.Affectations.GroupBy(a => a.OuvrierNom).ToList();

                foreach (var groupeOuvrier in ouvriersParTache)
                {
                    var ouvrier = ouvriersAvecCharge.FirstOrDefault(o => o.Nom == groupeOuvrier.Key);
                    if (ouvrier != null)
                    {
                        var heuresOuvrierSurTache = groupeOuvrier.Sum(a => a.DureeHeures);
                        var pourcentageCharge = Math.Min(100.0, (heuresOuvrierSurTache / (tache.DureeJours * heuresParJour)) * 100);

                        xml += $@"
        <allocation task-id=""{tache.Id}"" resource-id=""{ouvrier.Id}"" function=""Default:0"" responsible=""true"" load=""{pourcentageCharge.ToString("F1", culture)}""/>";
                    }
                }
            }

            xml += @"
    </allocations>";
            return xml;
        }

        #endregion

        #region Méthodes CSV existantes (conservées)

        private List<TacheGantt> ConstruireTachesGantt(List<AffectationDto> affectations)
        {
            var taches = new List<TacheGantt>();
            var tachesGroupees = affectations.GroupBy(a => a.TacheId).ToList();

            for (int i = 0; i < tachesGroupees.Count(); i++)
            {
                var groupeTache = tachesGroupees.ElementAt(i);
                var premiereAffectation = groupeTache.OrderBy(a => a.DateDebut).First();
                var derniereAffectation = groupeTache.OrderBy(a => a.DateDebut.AddHours(a.DureeHeures)).Last();

                var dateDebut = premiereAffectation.DateDebut.Date;
                var dateFin = derniereAffectation.DateDebut.AddHours(derniereAffectation.DureeHeures).Date;

                if (dateFin == dateDebut)
                    dateFin = dateFin.AddDays(1);

                var dureeJours = (int)(dateFin - dateDebut).TotalDays;
                var coutTotal = groupeTache.Sum(a => CalculerCoutAffectation(a));
                var predecesseurs = RechercherPredecesseurs(premiereAffectation, tachesGroupees, i);
                var ressourcePrincipale = groupeTache
                    .GroupBy(a => a.OuvrierNom)
                    .OrderByDescending(g => g.Sum(a => a.DureeHeures))
                    .First().Key;

                var tache = new TacheGantt
                {
                    Id = i,
                    Nom = premiereAffectation.TacheNom,
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    Duree = dureeJours,
                    Avancee = 0,
                    Cout = coutTotal,
                    Priorite = 1,
                    Responsable = ressourcePrincipale,
                    Predecesseurs = predecesseurs,
                    NumeroHierarchique = i + 1,
                    Ressources = ressourcePrincipale,
                    Assignments = $"{i}:100.00",
                    Notes = $"Bloc: {premiereAffectation.BlocId}"
                };

                taches.Add(tache);
            }

            return taches;
        }

        private List<RessourceGantt> ConstruireRessourcesGantt(List<AffectationDto> affectations)
        {
            var ressources = new List<RessourceGantt>();
            var ouvriersGroupes = affectations.GroupBy(a => a.OuvrierNom).ToList();

            for (int i = 0; i < ouvriersGroupes.Count(); i++)
            {
                var groupeOuvrier = ouvriersGroupes.ElementAt(i);
                var totalHeures = groupeOuvrier.Sum(a => a.DureeHeures);
                var coutTotal = groupeOuvrier.Sum(a => CalculerCoutAffectation(a));
                var tauxHoraire = totalHeures > 0 ? (int)(coutTotal / totalHeures) : 250;

                var ressource = new RessourceGantt
                {
                    Id = i,
                    Nom = groupeOuvrier.Key,
                    Role = "Default:0",
                    Email = "",
                    Telephone = "",
                    TauxNormal = tauxHoraire,
                    CoutTotal = (int)coutTotal,
                    ChargeTotal = totalHeures
                };

                ressources.Add(ressource);
            }

            return ressources;
        }

        private string RechercherPredecesseurs(AffectationDto tacheActuelle, IEnumerable<IGrouping<string, AffectationDto>> toutesLesTaches, int indexActuel)
        {
            var predecesseurs = new List<int>();

            for (int i = 0; i < indexActuel; i++)
            {
                var tachePrecedente = toutesLesTaches.ElementAt(i).First();

                if (tachePrecedente.BlocId == tacheActuelle.BlocId ||
                    tachePrecedente.DateDebut < tacheActuelle.DateDebut.AddDays(-1))
                {
                    predecesseurs.Add(i);
                }
            }

            return predecesseurs.Any() ? string.Join(" ", predecesseurs) : "";
        }

        private double CalculerCoutAffectation(AffectationDto affectation)
        {
            return affectation.DureeHeures * 31.25; // 250€/jour / 8h
        }

        private void EcrireFichierGanttProjectCsv(string filePath, List<TacheGantt> taches, List<RessourceGantt> ressources)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            writer.WriteLine("ID,Nom,Date de début,Date de fin,Durée,Avancée,Coût,Priorité,Responsable,Prédécesseurs,Numéro hiérarchique,Ressources,Assignments,Nouvelle tâche,Notes,Lien internet");

            foreach (var tache in taches)
            {
                writer.WriteLine($"{tache.Id}," +
                    $"{tache.Nom}," +
                    $"{tache.DateDebut:dd/MM/yyyy}," +
                    $"{tache.DateFin:dd/MM/yyyy}," +
                    $"{tache.Duree}," +
                    $"{tache.Avancee}," +
                    $"{tache.Cout:F1}," +
                    $"{tache.Priorite}," +
                    $"{tache.Responsable}," +
                    $"{tache.Predecesseurs}," +
                    $"{tache.NumeroHierarchique}," +
                    $"{tache.Ressources}," +
                    $"{tache.Assignments}," +
                    $"," +
                    $"{tache.Notes}," +
                    $"");
            }

            writer.WriteLine("ID,Nom,Rôle par défaut,Courriel,Téléphone,Taux normal,Coût total,Charge totale");

            foreach (var ressource in ressources)
            {
                writer.WriteLine($"{ressource.Id}," +
                    $"{ressource.Nom}," +
                    $"{ressource.Role}," +
                    $"{ressource.Email}," +
                    $"{ressource.Telephone}," +
                    $"{ressource.TauxNormal}," +
                    $"{ressource.CoutTotal}," +
                    $"{ressource.ChargeTotal:F1}");
            }
        }

        #endregion
    }

    #region Configuration

    /// <summary>
    /// Configuration pour l'export GanttProject
    /// </summary>
    public class ConfigurationExportGantt
    {
        public string NomProjet { get; set; } = "Planning PlanAthena";
        public double HeuresParJour { get; set; } = 8.0;
        public IEnumerable<DayOfWeek> JoursOuvres { get; set; } = new[] {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday
        };
    }

    #endregion

    #region Classes existantes (conservées)

    /// <summary>
    /// Représente une tâche groupée pour l'export XML
    /// </summary>
    public class TacheGroupee
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int DureeJours { get; set; }
        public double HeuresTotal { get; set; }
        public List<AffectationDto> Affectations { get; set; } = new List<AffectationDto>();
    }

    public class TacheGantt
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int Duree { get; set; }
        public int Avancee { get; set; }
        public double Cout { get; set; }
        public int Priorite { get; set; }
        public string Responsable { get; set; } = "";
        public string Predecesseurs { get; set; } = "";
        public int NumeroHierarchique { get; set; }
        public string Ressources { get; set; } = "";
        public string Assignments { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class RessourceGantt
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public string Role { get; set; } = "Default:0";
        public string Email { get; set; } = "";
        public string Telephone { get; set; } = "";
        public int TauxNormal { get; set; }
        public int CoutTotal { get; set; }
        public double ChargeTotal { get; set; }
    }

    #endregion
}