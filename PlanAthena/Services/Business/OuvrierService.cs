// Services/Business/OuvrierService.cs
// 🔄 REFONTE COMPLÈTE V0.4.2 - Architecture Dictionary + Compétences multiples

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs; // 🆕 Import DTOs déplacés
using PlanAthena.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service de gestion des ouvriers et de leurs compétences
    /// 🔄 VERSION V0.4.2 - Architecture Dictionary + Suppression workaround multi-métiers
    /// </summary>
    public class OuvrierService
    {
        // 🔄 ARCHITECTURE NOUVELLE V0.4.2 : Dictionary<string, Ouvrier> au lieu List<Ouvrier>
        private readonly Dictionary<string, Ouvrier> _ouvriersUniques = new();

        // 🗑️ SUPPRIMÉ V0.4.2 : HashSet<(string, string)> _ouvrierMetierKeys (workaround)
        // 🗑️ SUPPRIMÉ V0.4.2 : List<Ouvrier> _ouvriersList

        private readonly CsvDataService _csvDataService;
        private readonly ExcelReader _excelReader;

        public OuvrierService(CsvDataService csvDataService, ExcelReader excelReader)
        {
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _excelReader = excelReader ?? throw new ArgumentNullException(nameof(excelReader));
        }

        #region CRUD Operations V0.4.2 (🔄 MIS À JOUR)

        /// <summary>
        /// 🔄 MODIFIÉ V0.4.2 : Ajoute un ouvrier avec architecture Dictionary
        /// Un ouvrier = un ID unique avec compétences multiples
        /// </summary>
        public void AjouterOuvrier(Ouvrier ouvrier)
        {
            if (ouvrier == null)
                throw new ArgumentNullException(nameof(ouvrier));

            if (string.IsNullOrWhiteSpace(ouvrier.OuvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.", nameof(ouvrier.OuvrierId));

            if (_ouvriersUniques.ContainsKey(ouvrier.OuvrierId))
                throw new InvalidOperationException($"L'ouvrier '{ouvrier.OuvrierId}' existe déjà.");

            // 🆕 V0.4.2 : Validation compétences
            if (ouvrier.Competences == null || !ouvrier.Competences.Any())
                throw new ArgumentException("L'ouvrier doit avoir au moins une compétence.", nameof(ouvrier.Competences));

            _ouvriersUniques.Add(ouvrier.OuvrierId, ouvrier);
        }

        /// <summary>
        /// 🔄 MODIFIÉ V0.4.2 : Met à jour un ouvrier existant (données générales uniquement)
        /// Pour modifier les compétences, utiliser AjouterCompetence()/SupprimerCompetence()
        /// </summary>
        public void ModifierOuvrier(Ouvrier ouvrierModifie)
        {
            if (ouvrierModifie == null)
                throw new ArgumentNullException(nameof(ouvrierModifie));

            if (!_ouvriersUniques.TryGetValue(ouvrierModifie.OuvrierId, out var ouvrierExistant))
                throw new InvalidOperationException($"Ouvrier '{ouvrierModifie.OuvrierId}' non trouvé pour modification.");

            // Mise à jour des propriétés générales (pas les compétences)
            ouvrierExistant.Nom = ouvrierModifie.Nom;
            ouvrierExistant.Prenom = ouvrierModifie.Prenom;
            ouvrierExistant.CoutJournalier = ouvrierModifie.CoutJournalier;
        }

        /// <summary>
        /// 🔄 MODIFIÉ V0.4.2 : Supprime un ouvrier (et toutes ses compétences)
        /// </summary>
        public void SupprimerOuvrier(string ouvrierId)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.", nameof(ouvrierId));

            if (!_ouvriersUniques.Remove(ouvrierId))
                throw new InvalidOperationException($"Ouvrier '{ouvrierId}' non trouvé.");
        }

        #endregion

        #region 🆕 NOUVELLES MÉTHODES V0.4.2 - Gestion Compétences

        /// <summary>
        /// 🆕 NOUVEAU V0.4.2 : Ajoute une compétence à un ouvrier existant
        /// Utilisé par: OuvrierForm, Import, Migration données
        /// </summary>
        public void AjouterCompetence(string ouvrierId, string metierId, bool estPrincipal = false)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.", nameof(ouvrierId));

            if (string.IsNullOrWhiteSpace(metierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(metierId));

            if (!_ouvriersUniques.TryGetValue(ouvrierId, out var ouvrier))
                throw new InvalidOperationException($"Ouvrier '{ouvrierId}' non trouvé.");

            // Vérifier si la compétence existe déjà
            if (ouvrier.Competences.Any(c => c.MetierId == metierId))
                throw new InvalidOperationException($"L'ouvrier '{ouvrierId}' possède déjà la compétence '{metierId}'.");

            // Si estPrincipal = true, retirer le flag des autres compétences
            if (estPrincipal)
            {
                foreach (var competence in ouvrier.Competences)
                {
                    competence.EstMetierPrincipal = false;
                }
            }

            ouvrier.Competences.Add(new CompetenceOuvrier
            {
                MetierId = metierId,
                EstMetierPrincipal = estPrincipal
            });
        }

        /// <summary>
        /// 🆕 NOUVEAU V0.4.2 : Supprime une compétence d'un ouvrier
        /// Utilisé par: OuvrierForm, Migration données
        /// </summary>
        public void SupprimerCompetence(string ouvrierId, string metierId)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.", nameof(ouvrierId));

            if (string.IsNullOrWhiteSpace(metierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(metierId));

            if (!_ouvriersUniques.TryGetValue(ouvrierId, out var ouvrier))
                throw new InvalidOperationException($"Ouvrier '{ouvrierId}' non trouvé.");

            var competence = ouvrier.Competences.FirstOrDefault(c => c.MetierId == metierId);
            if (competence == null)
                throw new InvalidOperationException($"Compétence '{metierId}' pour l'ouvrier '{ouvrierId}' non trouvée.");

            // Ne pas permettre la suppression de la dernière compétence
            if (ouvrier.Competences.Count == 1)
                throw new InvalidOperationException($"Impossible de supprimer la dernière compétence de l'ouvrier '{ouvrierId}'.");

            ouvrier.Competences.Remove(competence);
        }

        /// <summary>
        /// 🆕 NOUVEAU V0.4.2 : Définit le métier principal d'un ouvrier
        /// Utilisé par: OuvrierForm pour sélectionner métier d'affichage
        /// </summary>
        public void DefinirMetierPrincipal(string ouvrierId, string metierId)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.", nameof(ouvrierId));

            if (string.IsNullOrWhiteSpace(metierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.", nameof(metierId));

            if (!_ouvriersUniques.TryGetValue(ouvrierId, out var ouvrier))
                throw new InvalidOperationException($"Ouvrier '{ouvrierId}' non trouvé.");

            var competence = ouvrier.Competences.FirstOrDefault(c => c.MetierId == metierId);
            if (competence == null)
                throw new InvalidOperationException($"Compétence '{metierId}' non trouvée pour l'ouvrier '{ouvrierId}'.");

            // Retirer le flag principal des autres compétences
            foreach (var comp in ouvrier.Competences)
            {
                comp.EstMetierPrincipal = false;
            }

            // Définir le nouveau métier principal
            competence.EstMetierPrincipal = true;
        }

        /// <summary>
        /// 🆕 NOUVEAU V0.4.2 : Obtient la liste des compétences d'un ouvrier
        /// Utilisé par: OuvrierForm, Export, affichage
        /// </summary>
        public List<string> GetCompetencesOuvrier(string ouvrierId)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId))
                return new List<string>();

            if (!_ouvriersUniques.TryGetValue(ouvrierId, out var ouvrier))
                return new List<string>();

            return ouvrier.Competences.Select(c => c.MetierId).ToList();
        }

        #endregion

        #region Consultation V0.4.2 (🔄 ADAPTÉES)

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Obtient tous les ouvriers (structure unique)
        /// ⚠️ BREAKING CHANGE : Retourne maintenant les ouvriers uniques, pas les compétences individuelles
        /// </summary>
        public List<Ouvrier> ObtenirTousLesOuvriers()
        {
            return _ouvriersUniques.Values.ToList();
        }

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Obtient un ouvrier par son ID
        /// ⚠️ BREAKING CHANGE : Retourne un seul Ouvrier au lieu d'une List<Ouvrier>
        /// </summary>
        public Ouvrier ObtenirOuvrierParId(string ouvrierId)
        {
            _ouvriersUniques.TryGetValue(ouvrierId, out var ouvrier);
            return ouvrier;
        }

        /// <summary>
        /// 🔄 SIMPLIFIÉ V0.4.2 : Plus de GroupBy, direct depuis Dictionary
        /// </summary>
        public List<OuvrierInfo> ObtenirListeOuvriersUniques()
        {
            return _ouvriersUniques.Values
                .Select(o => new OuvrierInfo
                {
                    OuvrierId = o.OuvrierId,
                    Nom = o.Nom,
                    Prenom = o.Prenom,
                    CoutJournalier = o.CoutJournalier,
                    NombreCompetences = o.Competences?.Count ?? 0
                })
                .OrderBy(o => o.Nom)
                .ThenBy(o => o.Prenom)
                .ToList();
        }

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Obtient les ouvriers ayant une compétence spécifique
        /// </summary>
        public List<Ouvrier> ObtenirOuvriersParMetier(string metierId)
        {
            return _ouvriersUniques.Values
                .Where(o => o.Competences?.Any(c => c.MetierId == metierId) == true)
                .ToList();
        }

        /// <summary>
        /// 🔄 SIMPLIFIÉ V0.4.2 : Vérification directe dans Dictionary
        /// </summary>
        public bool OuvrierExiste(string ouvrierId)
        {
            return _ouvriersUniques.ContainsKey(ouvrierId);
        }

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Vérifie si un ouvrier a une compétence spécifique
        /// </summary>
        public bool OuvrierACompetence(string ouvrierId, string metierId)
        {
            if (!_ouvriersUniques.TryGetValue(ouvrierId, out var ouvrier))
                return false;

            return ouvrier.Competences?.Any(c => c.MetierId == metierId) == true;
        }

        #endregion

        #region Import/Export V0.4.2 (🔄 MIS À JOUR)

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Importe les ouvriers depuis un fichier CSV
        /// ⚠️ MIGRATION : Convertit automatiquement l'ancien format (MetierId) vers le nouveau (Competences)
        /// </summary>
        public int ImporterDepuisCsv(string filePath, bool remplacerExistants = true)
        {
            // On importe les "lignes" du CSV, qui peuvent contenir des doublons d'OuvrierId
            var lignesOuvrierImportees = _csvDataService.ImportCsv<Ouvrier>(filePath);
            int countAddedOrUpdated = 0;

            if (remplacerExistants)
            {
                Vider();
            }

            foreach (var ligneOuvrier in lignesOuvrierImportees)
            {
                try
                {
                    // La migration automatique via la propriété `MetierId` est correcte, on la garde.
                    if (ligneOuvrier.Competences == null || !ligneOuvrier.Competences.Any())
                    {
                        if (!string.IsNullOrWhiteSpace(ligneOuvrier.MetierId))
                        {
                            ligneOuvrier.Competences.Add(new CompetenceOuvrier { MetierId = ligneOuvrier.MetierId, EstMetierPrincipal = true });
                        }
                        else
                        {
                            continue; // Ligne sans métier, on ignore
                        }
                    }

                    // La compétence à ajouter (la seule de cette ligne)
                    var competenceDeLaLigne = ligneOuvrier.Competences.First();

                    // Vérifier si l'ouvrier existe déjà dans notre dictionnaire
                    if (OuvrierExiste(ligneOuvrier.OuvrierId))
                    {
                        // L'ouvrier existe : on lui AJOUTE la nouvelle compétence
                        // On met estPrincipal à false car seule la première compétence importée sera la principale par défaut.
                        AjouterCompetence(ligneOuvrier.OuvrierId, competenceDeLaLigne.MetierId, false);
                    }
                    else
                    {
                        // Nouvel ouvrier : on l'ajoute. La première compétence est principale par défaut.
                        competenceDeLaLigne.EstMetierPrincipal = true;
                        ligneOuvrier.Competences = new List<CompetenceOuvrier> { competenceDeLaLigne };
                        AjouterOuvrier(ligneOuvrier);
                    }
                    countAddedOrUpdated++;
                }
                catch (InvalidOperationException ex)
                {
                    // Doublon de compétence (Ouvrier X a déjà métier Y), on ignore
                    System.Diagnostics.Debug.WriteLine($"Doublon de compétence ignoré lors de l'import CSV: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    // Données invalides ignorées
                    System.Diagnostics.Debug.WriteLine($"Données d'ouvrier invalides ignorées lors de l'import CSV: {ex.Message}");
                }
            }
            return countAddedOrUpdated;
        }

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Importe les ouvriers depuis un fichier Excel SAP
        /// </summary>
        public int ImporterDepuisExcelSap(string filePath)
        {
            // TODO: Implémentation spécifique au format SAP avec migration automatique
            var donneesExcel = _excelReader.ImportSapOuvriers(filePath);

            // Placeholder - à implémenter selon le format SAP réel
            return 0;
        }

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Exporte les ouvriers vers un fichier CSV
        /// </summary>
        public void ExporterVersCsv(string filePath)
        {
            _csvDataService.ExportCsv(_ouvriersUniques.Values.ToList(), filePath);
        }

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Charge les ouvriers depuis une liste (utilisé par ProjetService)
        /// ⚠️ MIGRATION : Convertit automatiquement l'ancien format vers le nouveau
        /// </summary>
        public void ChargerOuvriers(List<Ouvrier> ouvriers)
        {
            Vider();

            if (ouvriers != null)
            {
                foreach (var ouvrier in ouvriers)
                {
                    try
                    {
                        // 🆕 V0.4.2 : Migration automatique MetierId → Competences
                        if (ouvrier.Competences == null || !ouvrier.Competences.Any())
                        {
                            if (!string.IsNullOrWhiteSpace(ouvrier.MetierId))
                            {
                                ouvrier.Competences = new List<CompetenceOuvrier>
                                {
                                    new CompetenceOuvrier
                                    {
                                        MetierId = ouvrier.MetierId,
                                        EstMetierPrincipal = true
                                    }
                                };
                            }
                            else
                            {
                                // Ouvrier sans métier, ignorer
                                System.Diagnostics.Debug.WriteLine($"Ouvrier '{ouvrier.OuvrierId}' ignoré lors du chargement : aucun métier défini.");
                                continue;
                            }
                        }

                        AjouterOuvrier(ouvrier);
                    }
                    catch (InvalidOperationException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Avertissement: Doublon d'ouvrier détecté et ignoré lors du chargement du projet: {ex.Message}");
                    }
                    catch (ArgumentException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Avertissement: Données d'ouvrier invalides ignorées lors du chargement du projet: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Statistiques V0.4.2 (🔄 ADAPTÉES)

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Obtient des statistiques sur les ouvriers uniques
        /// </summary>
        public StatistiquesOuvriers ObtenirStatistiques()
        {
            if (!_ouvriersUniques.Any())
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

            var ouvriers = _ouvriersUniques.Values;

            return new StatistiquesOuvriers
            {
                NombreOuvriersTotal = ouvriers.Count(),
                NombreCompetencesTotal = ouvriers.Sum(o => o.Competences?.Count ?? 0),
                CoutJournalierMoyen = ouvriers.Average(o => o.CoutJournalier),
                CoutJournalierMin = ouvriers.Min(o => o.CoutJournalier),
                CoutJournalierMax = ouvriers.Max(o => o.CoutJournalier),
                NombreCompetencesParOuvrierMoyen = ouvriers.Average(o => o.Competences?.Count ?? 0)
            };
        }

        #endregion

        /// <summary>
        /// 🔄 ADAPTÉ V0.4.2 : Efface toutes les données
        /// </summary>
        public void Vider()
        {
            _ouvriersUniques.Clear();
        }
    }
}