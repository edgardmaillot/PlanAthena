// START OF FILE OuvrierService.cs

using PlanAthena.Data;
using PlanAthena.Services.DataAccess;
using System.Linq; // Assurez-vous d'avoir ceci

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service de gestion des ouvriers et de leurs compétences
    /// </summary>
    public class OuvrierService
    {
        // CHANGEMENT : Utilisation d'un HashSet pour gérer l'unicité des paires (OuvrierId, MetierId)
        // Un HashSet est optimal pour des vérifications d'unicité rapides.
        // Nous allons créer un type anonyme ou une classe interne pour la clé.
        private readonly HashSet<(string OuvrierId, string MetierId)> _ouvrierMetierKeys = new HashSet<(string, string)>();
        private readonly List<Ouvrier> _ouvriersList = new List<Ouvrier>(); // La liste réelle des objets Ouvrier

        private readonly CsvDataService _csvDataService;
        private readonly ExcelReader _excelReader;

        public OuvrierService(CsvDataService csvDataService, ExcelReader excelReader)
        {
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _excelReader = excelReader ?? throw new ArgumentNullException(nameof(excelReader));
        }

        #region CRUD Operations

        /// <summary>
        /// Ajoute un nouvel ouvrier (compétence spécifique pour un ouvrier)
        /// </summary>
        public void AjouterOuvrier(Ouvrier ouvrier)
        {
            if (ouvrier == null)
                throw new ArgumentNullException(nameof(ouvrier));

            if (string.IsNullOrWhiteSpace(ouvrier.OuvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.", nameof(ouvrier.OuvrierId));

            if (string.IsNullOrWhiteSpace(ouvrier.MetierId))
                throw new ArgumentException("L'ID du métier de l'ouvrier ne peut pas être vide.", nameof(ouvrier.MetierId));


            // Utilisation du HashSet pour une vérification d'unicité rapide
            var key = (ouvrier.OuvrierId, ouvrier.MetierId);
            if (_ouvrierMetierKeys.Contains(key))
            {
                // Si la combinaison existe déjà, c'est un doublon
                throw new InvalidOperationException($"L'ouvrier '{ouvrier.OuvrierId}' possède déjà la compétence '{ouvrier.MetierId}'.");
            }

            _ouvriersList.Add(ouvrier);
            _ouvrierMetierKeys.Add(key); // Ajoute la clé pour la vérification future
        }

        /// <summary>
        /// Met à jour un ouvrier existant (compétence spécifique)
        /// NOTE: Si le MetierId ou OuvrierId change, cela équivaut à une suppression/ajout.
        /// Pour l'instant, cette méthode gère uniquement la mise à jour des AUTRES propriétés.
        /// </summary>
        public void ModifierOuvrier(Ouvrier ouvrierModifie)
        {
            if (ouvrierModifie == null)
                throw new ArgumentNullException(nameof(ouvrierModifie));

            var ouvrierExistant = _ouvriersList.FirstOrDefault(o =>
                o.OuvrierId == ouvrierModifie.OuvrierId &&
                o.MetierId == ouvrierModifie.MetierId);

            if (ouvrierExistant == null)
            {
                // Si l'élément n'existe pas, on pourrait vouloir l'ajouter
                // Pour l'instant, lançons une exception comme le code original.
                throw new InvalidOperationException($"Ouvrier '{ouvrierModifie.OuvrierId}' avec compétence '{ouvrierModifie.MetierId}' non trouvé pour modification.");
            }

            // Mise à jour des propriétés (sauf OuvrierId et MetierId, car ils forment la clé)
            ouvrierExistant.Nom = ouvrierModifie.Nom;
            ouvrierExistant.Prenom = ouvrierModifie.Prenom;
            ouvrierExistant.CoutJournalier = ouvrierModifie.CoutJournalier;
            ouvrierExistant.NiveauExpertise = ouvrierModifie.NiveauExpertise;
            ouvrierExistant.PerformancePct = ouvrierModifie.PerformancePct;
        }

        /// <summary>
        /// Supprime un ouvrier (toutes ses compétences)
        /// </summary>
        public void SupprimerOuvrier(string ouvrierId)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.", nameof(ouvrierId));

            var competencesASupprimer = _ouvriersList.Where(o => o.OuvrierId == ouvrierId).ToList();

            if (!competencesASupprimer.Any())
                throw new InvalidOperationException($"Ouvrier '{ouvrierId}' non trouvé.");

            foreach (var competence in competencesASupprimer)
            {
                _ouvriersList.Remove(competence);
                _ouvrierMetierKeys.Remove((competence.OuvrierId, competence.MetierId)); // Retire la clé du HashSet
            }
        }

        /// <summary>
        /// Supprime une compétence spécifique d'un ouvrier
        /// </summary>
        public void SupprimerCompetence(string ouvrierId, string metierId)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.", nameof(ouvrierId));

            if (string.IsNullOrWhiteSpace(metierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(metierId));

            var competence = _ouvriersList.FirstOrDefault(o => o.OuvrierId == ouvrierId && o.MetierId == metierId);

            if (competence == null)
                throw new InvalidOperationException($"Compétence '{metierId}' pour l'ouvrier '{ouvrierId}' non trouvée.");

            _ouvriersList.Remove(competence);
            _ouvrierMetierKeys.Remove((ouvrierId, metierId)); // Retire la clé du HashSet
        }

        #endregion

        #region Consultation

        /// <summary>
        /// Obtient tous les ouvriers (compétences individuelles)
        /// </summary>
        public List<Ouvrier> ObtenirTousLesOuvriers()
        {
            return _ouvriersList.ToList();
        }

        /// <summary>
        /// Obtient un ouvrier par son ID (avec toutes ses compétences)
        /// </summary>
        public List<Ouvrier> ObtenirOuvrierParId(string ouvrierId)
        {
            return _ouvriersList.Where(o => o.OuvrierId == ouvrierId).ToList();
        }

        /// <summary>
        /// Obtient la liste des ouvriers uniques (sans doublons d'ID)
        /// </summary>
        public List<OuvrierInfo> ObtenirListeOuvriersUniques()
        {
            return _ouvriersList
                .GroupBy(o => o.OuvrierId)
                .Select(g => new OuvrierInfo
                {
                    OuvrierId = g.Key,
                    Nom = g.First().Nom, // Prend le nom du premier objet trouvé pour cet ouvrierId
                    Prenom = g.First().Prenom,
                    CoutJournalier = g.First().CoutJournalier,
                    NombreCompetences = g.Count()
                })
                .OrderBy(o => o.Nom)
                .ThenBy(o => o.Prenom)
                .ToList();
        }

        /// <summary>
        /// Obtient les ouvriers ayant une compétence spécifique
        /// </summary>
        public List<Ouvrier> ObtenirOuvriersParMetier(string metierId)
        {
            return _ouvriersList.Where(o => o.MetierId == metierId).ToList();
        }

        /// <summary>
        /// Vérifie si un ouvrier existe (au moins une compétence pour cet ID)
        /// </summary>
        public bool OuvrierExiste(string ouvrierId)
        {
            return _ouvriersList.Any(o => o.OuvrierId == ouvrierId);
        }

        /// <summary>
        /// Vérifie si un ouvrier a une compétence spécifique
        /// </summary>
        public bool OuvrierACompetence(string ouvrierId, string metierId)
        {
            return _ouvrierMetierKeys.Contains((ouvrierId, metierId)); // Utilisation du HashSet
        }

        #endregion

        #region Import/Export

        /// <summary>
        /// Importe les ouvriers depuis un fichier CSV
        /// </summary>
        public int ImporterDepuisCsv(string filePath, bool remplacerExistants = true)
        {
            var ouvriersImportes = _csvDataService.ImportCsv<Ouvrier>(filePath);
            int countAdded = 0;

            if (remplacerExistants)
            {
                Vider(); // Utilise la méthode Vider qui gère les deux collections
            }

            foreach (var ouvrier in ouvriersImportes)
            {
                try
                {
                    // Tente d'ajouter chaque ouvrier. Si c'est un doublon (OuvrierId + MetierId), AjouterOuvrier lancera une exception.
                    // Pour l'import, on peut choisir d'ignorer les doublons ou de les journaliser. Ici, on ignore silencieusement.
                    AjouterOuvrier(ouvrier);
                    countAdded++;
                }
                catch (InvalidOperationException ex)
                {
                    // Optionnel: Journaliser les doublons ignorés
                    System.Diagnostics.Debug.WriteLine($"Doublon d'ouvrier ignoré lors de l'import CSV: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    // Optionnel: Journaliser les données invalides
                    System.Diagnostics.Debug.WriteLine($"Données d'ouvrier invalides ignorées lors de l'import CSV: {ex.Message}");
                }
            }
            return countAdded;
        }

        /// <summary>
        /// Importe les ouvriers depuis un fichier Excel SAP
        /// </summary>
        public int ImporterDepuisExcelSap(string filePath)
        {
            // TODO: Implémentation spécifique au format SAP
            // Cette méthode devra mapper les colonnes Excel SAP vers OuvrierCsvRecord
            var donneesExcel = _excelReader.ImportSapOuvriers(filePath);

            // Placeholder - à implémenter selon le format SAP réel
            // Si vous obtenez une liste d'Ouvrier, appliquez la même logique que ImporterDepuisCsv
            // en appelant AjouterOuvrier pour chaque élément pour assurer l'unicité.
            return 0;
        }

        /// <summary>
        /// Exporte les ouvriers vers un fichier CSV
        /// </summary>
        public void ExporterVersCsv(string filePath)
        {
            _csvDataService.ExportCsv(_ouvriersList, filePath);
        }

        /// <summary>
        /// Charge les ouvriers depuis une liste (utilisé par ProjetService)
        /// </summary>
        public void ChargerOuvriers(List<Ouvrier> ouvriers)
        {
            Vider(); // Vide les deux collections avant de recharger

            if (ouvriers != null)
            {
                foreach (var ouvrier in ouvriers)
                {
                    try
                    {
                        // Utilise AjouterOuvrier pour garantir l'unicité des paires (OuvrierId, MetierId)
                        // même lors du chargement d'un projet.
                        AjouterOuvrier(ouvrier);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Devrait idéalement ne pas se produire si le fichier JSON est propre,
                        // mais c'est une sécurité. Journaliser un avertissement.
                        System.Diagnostics.Debug.WriteLine($"Avertissement: Doublon d'ouvrier détecté et ignoré lors du chargement du projet: {ex.Message}");
                    }
                    catch (ArgumentException ex)
                    {
                        // Erreur de données si OuvrierId ou MetierId est vide
                        System.Diagnostics.Debug.WriteLine($"Avertissement: Données d'ouvrier invalides ignorées lors du chargement du projet: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Statistiques

        /// <summary>
        /// Obtient des statistiques sur les ouvriers
        /// </summary>
        public StatistiquesOuvriers ObtenirStatistiques()
        {
            if (!_ouvriersList.Any()) // Utilise _ouvriersList pour les calculs
            {
                return new StatistiquesOuvriers
                {
                    NombreOuvriersTotal = 0,
                    NombreCompetencesTotal = 0,
                    CoutJournalierMoyen = 0,
                    CoutJournalierMin = 0,
                    CoutJournalierMax = 0,
                    NombreCompetencesParOuvrierMoyen = 0
                };
            }

            var ouvriersUniques = _ouvriersList.GroupBy(o => o.OuvrierId).ToList();

            return new StatistiquesOuvriers
            {
                NombreOuvriersTotal = ouvriersUniques.Count,
                NombreCompetencesTotal = _ouvriersList.Count, // Nombre total d'enregistrements (personne+compétence)
                CoutJournalierMoyen = ouvriersUniques.Average(g => g.First().CoutJournalier),
                CoutJournalierMin = ouvriersUniques.Min(g => g.First().CoutJournalier),
                CoutJournalierMax = ouvriersUniques.Max(g => g.First().CoutJournalier),
                NombreCompetencesParOuvrierMoyen = ouvriersUniques.Average(g => g.Count())
            };
        }

        #endregion

        /// <summary>
        /// Efface toutes les données
        /// </summary>
        public void Vider()
        {
            _ouvriersList.Clear();
            _ouvrierMetierKeys.Clear(); // Très important : vider aussi le HashSet des clés
        }
    }

    #region Classes de support

    /// <summary>
    /// Informations consolidées sur un ouvrier
    /// </summary>
    public class OuvrierInfo
    {
        public string OuvrierId { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public int CoutJournalier { get; set; }
        public int NombreCompetences { get; set; }

        public string NomComplet => $"{Prenom} {Nom}";
    }

    /// <summary>
    /// Statistiques sur les ouvriers
    /// </summary>
    public class StatistiquesOuvriers
    {
        public int NombreOuvriersTotal { get; set; }
        public int NombreCompetencesTotal { get; set; }
        public double CoutJournalierMoyen { get; set; }
        public int CoutJournalierMin { get; set; }
        public int CoutJournalierMax { get; set; }
        public double NombreCompetencesParOuvrierMoyen { get; set; }
    }

    #endregion
}