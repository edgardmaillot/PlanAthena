using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.CsvModels;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service de transformation des données CSV vers les DTOs PlanAthena
    /// </summary>
    public class DataTransformer
    {
        private readonly MetierService _metierService;

        public DataTransformer(MetierService metierService)
        {
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
        }

        /// <summary>
        /// Transforme les données CSV en ChantierSetupInputDto pour PlanAthena
        /// </summary>
        /// <param name="ouvriers">Liste des ouvriers CSV</param>
        /// <param name="taches">Liste des tâches CSV</param>
        /// <param name="configurationUI">Configuration depuis l'interface utilisateur</param>
        /// <returns>DTO configuré pour PlanAthena</returns>
        public ChantierSetupInputDto TransformToChantierSetupDto(
            List<OuvrierCsvRecord> ouvriers,
            List<TacheCsvRecord> taches,
            ConfigurationUI configurationUI)
        {
            if (ouvriers == null) throw new ArgumentNullException(nameof(ouvriers));
            if (taches == null) throw new ArgumentNullException(nameof(taches));
            if (configurationUI == null) throw new ArgumentNullException(nameof(configurationUI));

            // Pré-traitement des tâches (logique existante)
            var dataProcessor = new ChantierDataProcessor();
            var processedTaches = dataProcessor.ProcessTaches(taches, _metierService);
            var allMetiersRecords = _metierService.GetAllMetiers();

            // Transformation des tâches
            var tachesDto = processedTaches.Select(t => new TacheDto
            {
                TacheId = t.TacheId,
                Nom = t.TacheNom,
                BlocId = t.BlocId,
                HeuresHommeEstimees = t.HeuresHommeEstimees,
                MetierId = t.MetierId,
                Dependencies = t.Dependencies?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()
            }).ToList();

            // Transformation des blocs
            var blocsDto = processedTaches
                .GroupBy(t => t.BlocId)
                .Select(g => g.First())
                .Select(t => new BlocTravailDto
                {
                    BlocId = t.BlocId,
                    Nom = t.BlocNom,
                    CapaciteMaxOuvriers = t.BlocCapaciteMaxOuvriers
                }).ToList();

            // Transformation des lots
            var lotsDto = processedTaches
                .GroupBy(t => t.LotId)
                .Select(g => new LotTravauxDto
                {
                    LotId = g.Key,
                    Nom = g.First().LotNom,
                    Priorite = g.First().LotPriorite,
                    BlocIds = g.Select(b => b.BlocId).Distinct().ToList()
                }).ToList();

            // Transformation des métiers
            var metiersDto = allMetiersRecords.Select(m => new MetierDto
            {
                MetierId = m.MetierId,
                Nom = m.Nom,
                PrerequisMetierIds = m.PrerequisMetierIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()
            }).ToList();

            // Transformation des ouvriers
            var ouvriersDto = ouvriers
                .GroupBy(o => o.OuvrierId)
                .Select(g => new OuvrierDto
                {
                    OuvrierId = g.Key,
                    Nom = g.First().Nom,
                    Prenom = g.First().Prenom,
                    CoutJournalier = g.First().CoutJournalier,
                    Competences = g.Select(c => new CompetenceDto
                    {
                        MetierId = c.MetierId,
                        Niveau = (PlanAthena.Core.Facade.Dto.Enums.NiveauExpertise)c.NiveauExpertise,
                        PerformancePct = c.PerformancePct
                    }).ToList()
                }).ToList();

            // Transformation du calendrier
            var calendrierDto = new CalendrierTravailDefinitionDto
            {
                JoursOuvres = configurationUI.JoursOuvres,
                HeureDebutJournee = configurationUI.HeureDebutJournee,
                HeuresTravailEffectifParJour = configurationUI.HeuresTravailEffectifParJour
            };

            // Configuration d'optimisation
            OptimizationConfigDto optimConfig = null;
            if (configurationUI.TypeDeSortie != "Analyse et Estimation")
            {
                optimConfig = new OptimizationConfigDto
                {
                    TypeDeSortie = configurationUI.TypeDeSortie,
                    DureeJournaliereStandardHeures = configurationUI.DureeJournaliereStandardHeures,
                    PenaliteChangementOuvrierPourcentage = configurationUI.PenaliteChangementOuvrierPourcentage,
                    CoutIndirectJournalierPourcentage = configurationUI.CoutIndirectJournalierPourcentage
                };
            }

            // Construction du DTO final
            return new ChantierSetupInputDto
            {
                ChantierId = $"CHANTIER_TEST_{DateTime.Now:yyyyMMdd_HHmmss}",
                Description = configurationUI.Description,
                DateDebutSouhaitee = configurationUI.DateDebutSouhaitee,
                DateFinSouhaitee = configurationUI.DateFinSouhaitee,
                CalendrierTravail = calendrierDto,
                OptimizationConfig = optimConfig,
                Taches = tachesDto,
                Blocs = blocsDto,
                Lots = lotsDto,
                Metiers = metiersDto,
                Ouvriers = ouvriersDto
            };
        }
    }

    /// <summary>
    /// Classe pour encapsuler la configuration de l'interface utilisateur
    /// </summary>
    public class ConfigurationUI
    {
        public List<DayOfWeek> JoursOuvres { get; set; } = new List<DayOfWeek>();
        public int HeureDebutJournee { get; set; }
        public int HeuresTravailEffectifParJour { get; set; }
        public string TypeDeSortie { get; set; } = "Analyse et Estimation";
        public string Description { get; set; } = "";
        public DateTime? DateDebutSouhaitee { get; set; }
        public DateTime? DateFinSouhaitee { get; set; }
        public int DureeJournaliereStandardHeures { get; set; }
        public decimal PenaliteChangementOuvrierPourcentage { get; set; }
        public decimal CoutIndirectJournalierPourcentage { get; set; }
    }
}