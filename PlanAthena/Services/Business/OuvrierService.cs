using PlanAthena.CsvModels;
using PlanAthena.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service de gestion des ouvriers et de leurs compétences
    /// </summary>
    public class OuvrierService
    {
        private readonly List<OuvrierCsvRecord> _ouvriers = new List<OuvrierCsvRecord>();
        private readonly CsvDataService _csvDataService;
        private readonly ExcelReader _excelReader;

        public OuvrierService(CsvDataService csvDataService, ExcelReader excelReader)
        {
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
            _excelReader = excelReader ?? throw new ArgumentNullException(nameof(excelReader));
        }

        #region CRUD Operations

        /// <summary>
        /// Ajoute un nouvel ouvrier
        /// </summary>
        public void AjouterOuvrier(OuvrierCsvRecord ouvrier)
        {
            if (ouvrier == null)
                throw new ArgumentNullException(nameof(ouvrier));

            if (string.IsNullOrWhiteSpace(ouvrier.OuvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.");

            if (_ouvriers.Any(o => o.OuvrierId == ouvrier.OuvrierId && o.MetierId == ouvrier.MetierId))
                throw new InvalidOperationException($"L'ouvrier {ouvrier.OuvrierId} possède déjà la compétence {ouvrier.MetierId}.");

            _ouvriers.Add(ouvrier);
        }

        /// <summary>
        /// Met à jour un ouvrier existant
        /// </summary>
        public void ModifierOuvrier(OuvrierCsvRecord ouvrierModifie)
        {
            if (ouvrierModifie == null)
                throw new ArgumentNullException(nameof(ouvrierModifie));

            var ouvrierExistant = _ouvriers.FirstOrDefault(o =>
                o.OuvrierId == ouvrierModifie.OuvrierId &&
                o.MetierId == ouvrierModifie.MetierId);

            if (ouvrierExistant == null)
                throw new InvalidOperationException($"Ouvrier {ouvrierModifie.OuvrierId} avec compétence {ouvrierModifie.MetierId} non trouvé.");

            // Mise à jour des propriétés
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
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.");

            var competencesASupprimer = _ouvriers.Where(o => o.OuvrierId == ouvrierId).ToList();

            if (!competencesASupprimer.Any())
                throw new InvalidOperationException($"Ouvrier {ouvrierId} non trouvé.");

            foreach (var competence in competencesASupprimer)
            {
                _ouvriers.Remove(competence);
            }
        }

        /// <summary>
        /// Supprime une compétence spécifique d'un ouvrier
        /// </summary>
        public void SupprimerCompetence(string ouvrierId, string metierId)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId))
                throw new ArgumentException("L'ID de l'ouvrier ne peut pas être vide.");

            if (string.IsNullOrWhiteSpace(metierId))
                throw new ArgumentException("L'ID du métier ne peut pas être vide.");

            var competence = _ouvriers.FirstOrDefault(o => o.OuvrierId == ouvrierId && o.MetierId == metierId);

            if (competence == null)
                throw new InvalidOperationException($"Compétence {metierId} pour l'ouvrier {ouvrierId} non trouvée.");

            _ouvriers.Remove(competence);
        }

        #endregion

        #region Consultation

        /// <summary>
        /// Obtient tous les ouvriers
        /// </summary>
        public List<OuvrierCsvRecord> ObtenirTousLesOuvriers()
        {
            return _ouvriers.ToList();
        }

        /// <summary>
        /// Obtient un ouvrier par son ID (avec toutes ses compétences)
        /// </summary>
        public List<OuvrierCsvRecord> ObtenirOuvrierParId(string ouvrierId)
        {
            return _ouvriers.Where(o => o.OuvrierId == ouvrierId).ToList();
        }

        /// <summary>
        /// Obtient la liste des ouvriers uniques (sans doublons d'ID)
        /// </summary>
        public List<OuvrierInfo> ObtenirListeOuvriersUniques()
        {
            return _ouvriers
                .GroupBy(o => o.OuvrierId)
                .Select(g => new OuvrierInfo
                {
                    OuvrierId = g.Key,
                    Nom = g.First().Nom,
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
        public List<OuvrierCsvRecord> ObtenirOuvriersParMetier(string metierId)
        {
            return _ouvriers.Where(o => o.MetierId == metierId).ToList();
        }

        /// <summary>
        /// Vérifie si un ouvrier existe
        /// </summary>
        public bool OuvrierExiste(string ouvrierId)
        {
            return _ouvriers.Any(o => o.OuvrierId == ouvrierId);
        }

        /// <summary>
        /// Vérifie si un ouvrier a une compétence spécifique
        /// </summary>
        public bool OuvrierACompetence(string ouvrierId, string metierId)
        {
            return _ouvriers.Any(o => o.OuvrierId == ouvrierId && o.MetierId == metierId);
        }

        #endregion

        #region Import/Export

        /// <summary>
        /// Importe les ouvriers depuis un fichier CSV
        /// </summary>
        public int ImporterDepuisCsv(string filePath, bool remplacerExistants = true)
        {
            var ouvriersImportes = _csvDataService.ImportCsv<OuvrierCsvRecord>(filePath);

            if (remplacerExistants)
            {
                _ouvriers.Clear();
            }

            _ouvriers.AddRange(ouvriersImportes);
            return ouvriersImportes.Count;
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
            return 0;
        }

        /// <summary>
        /// Exporte les ouvriers vers un fichier CSV
        /// </summary>
        public void ExporterVersCsv(string filePath)
        {
            _csvDataService.ExportCsv(_ouvriers, filePath);
        }

        /// <summary>
        /// Charge les ouvriers depuis une liste (utilisé par PlanificationService)
        /// </summary>
        public void ChargerOuvriers(List<OuvrierCsvRecord> ouvriers)
        {
            _ouvriers.Clear();
            if (ouvriers != null)
            {
                _ouvriers.AddRange(ouvriers);
            }
        }

        #endregion

        #region Statistiques

        /// <summary>
        /// Obtient des statistiques sur les ouvriers
        /// </summary>
        public StatistiquesOuvriers ObtenirStatistiques()
        {
            if (!_ouvriers.Any())
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

            var ouvriersUniques = _ouvriers.GroupBy(o => o.OuvrierId).ToList();

            return new StatistiquesOuvriers
            {
                NombreOuvriersTotal = ouvriersUniques.Count,
                NombreCompetencesTotal = _ouvriers.Count,
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
            _ouvriers.Clear();
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