using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DataAccess;
// Ajout pour éviter d'écrire les noms complets partout dans la méthode de mapping
using CoreEnums = PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service de transformation des données du projet vers les DTOs de la DLL PlanAthena.Core.
    /// Agit comme une couche anti-corruption, traduisant les modèles de données internes
    /// en contrats d'entrée stables et propres pour le cœur du système.
    /// </summary>
    public class DataTransformer
    {
        private static readonly string[] s_splitChars = { "," };
        private readonly LotService _lotService;
        private readonly BlocService _blocService;

        public DataTransformer(LotService lotService, BlocService blocService)
        {
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
        }

        public ChantierSetupInputDto TransformToChantierSetupDto(
            List<Ouvrier> ouvriers,
            List<Tache> processedTaches,
            List<Metier> allMetiers,
            ConfigurationUI configurationUI)
        {
            ArgumentNullException.ThrowIfNull(ouvriers);
            ArgumentNullException.ThrowIfNull(processedTaches);
            ArgumentNullException.ThrowIfNull(configurationUI);

            // Transformation des tâches
            var tachesDto = processedTaches.Select(t => new TacheDto
            {
                TacheId = t.TacheId,
                Nom = t.TacheNom,
                Type = MapToCoreTypeActivite(t.Type),
                BlocId = t.BlocId,
                HeuresHommeEstimees = t.HeuresHommeEstimees,
                MetierId = t.MetierId ?? string.Empty,
                Dependencies = t.Dependencies?.Split(s_splitChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()
            }).ToList();

            // Transformation des blocs depuis le BlocService
            var blocsDto = _blocService.ObtenirTousLesBlocs()
                .Select(b => new BlocTravailDto
                {
                    BlocId = b.BlocId,
                    Nom = b.Nom,
                    CapaciteMaxOuvriers = b.CapaciteMaxOuvriers
                }).ToList();

            // Transformation des lots depuis le LotService
            var blocIdsParLot = processedTaches
                .GroupBy(t => t.LotId)
                .ToDictionary(g => g.Key, g => g.Select(t => t.BlocId).Distinct().ToList());

            var lotsDto = _lotService.ObtenirTousLesLots()
                .Select(l => new LotTravauxDto
                {
                    LotId = l.LotId,
                    Nom = l.Nom,
                    Priorite = l.Priorite,
                    BlocIds = blocIdsParLot.GetValueOrDefault(l.LotId, new List<string>())
                }).ToList();

            // Transformation des métiers
            var metiersDto = allMetiers.Select(m => new MetierDto
            {
                MetierId = m.MetierId,
                Nom = m.Nom,
                PrerequisMetierIds = m.PrerequisMetierIds?.Split(s_splitChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()
            }).ToList();

            // Transformation des ouvriers
            var ouvriersDto = ouvriers
                .GroupBy(o => o.OuvrierId)
                .Select(g =>
                {
                    var premierOuvrier = g.First();
                    return new OuvrierDto
                    {
                        OuvrierId = g.Key,
                        Nom = premierOuvrier.Nom,
                        Prenom = premierOuvrier.Prenom,
                        CoutJournalier = premierOuvrier.CoutJournalier,
                        Competences = g.Select(c => new CompetenceDto
                        {
                            MetierId = c.MetierId,
                            Niveau = c.NiveauExpertise,
                            PerformancePct = c.PerformancePct
                        }).ToList()
                    };
                }).ToList();

            // Transformation du calendrier
            var calendrierDto = new CalendrierTravailDefinitionDto
            {
                JoursOuvres = configurationUI.JoursOuvres,
                HeureDebutJournee = configurationUI.HeureDebutJournee,
                HeuresTravailEffectifParJour = configurationUI.HeuresTravailEffectifParJour
            };

            // Configuration d'optimisation
            OptimizationConfigDto? optimConfig = null;
            if (configurationUI.TypeDeSortie != "Analyse et Estimation")
            {
                optimConfig = new OptimizationConfigDto
                {
                    TypeDeSortie = configurationUI.TypeDeSortie ?? string.Empty,
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

        /// <summary>
        /// Mappe l'énumération TypeActivite de la couche Data vers celle de la DLL Core.
        /// </summary>
        private static CoreEnums.TypeActivite MapToCoreTypeActivite(TypeActivite sourceType) =>
            sourceType switch
            {
                TypeActivite.Tache => CoreEnums.TypeActivite.Tache,
                TypeActivite.JalonUtilisateur => CoreEnums.TypeActivite.JalonUtilisateur,
                TypeActivite.JalonDeSynchronisation => CoreEnums.TypeActivite.JalonTechnique,
                TypeActivite.JalonTechnique => CoreEnums.TypeActivite.JalonTechnique,
                _ => throw new ArgumentOutOfRangeException(nameof(sourceType), $"Valeur TypeActivite non supportée pour le mapping : {sourceType}"),
            };
    }
}