using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Data;
// Ajout pour éviter d'écrire les noms complets partout dans la méthode de mapping
using CoreEnums = PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service de transformation des données CSV vers les DTOs de la DLL PlanAthena.Core.
    /// Agit comme une couche anti-corruption, traduisant les modèles de données externes
    /// en contrats d'entrée stables et propres pour le cœur du système.
    /// </summary>
    public class DataTransformer
    {
        public DataTransformer()
        {
        }

        public ChantierSetupInputDto TransformToChantierSetupDto(
            List<Ouvrier> ouvriers,
            List<Tache> processedTaches,
            List<Metier> allMetiers,
            ConfigurationUI configurationUI)
        {
            if (ouvriers == null) throw new ArgumentNullException(nameof(ouvriers));
            if (processedTaches == null) throw new ArgumentNullException(nameof(processedTaches));
            if (configurationUI == null) throw new ArgumentNullException(nameof(configurationUI));

            // Transformation des tâches
            var tachesDto = processedTaches.Select(t => new TacheDto
            {
                TacheId = t.TacheId,
                Nom = t.TacheNom,
                // CORRECTION : Mappe l'énumération du projet de données vers l'énumération de la DLL Core.
                Type = MapToCoreTypeActivite(t.Type),
                BlocId = t.BlocId,
                HeuresHommeEstimees = t.HeuresHommeEstimees,
                MetierId = t.MetierId ?? string.Empty,
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
            var metiersDto = allMetiers.Select(m => new MetierDto
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
                        // RESTAURATION : Le cast direct est conservé tel que dans le code original.
                        // Cela suppose que c.NiveauExpertise est un type (probablement int) compatible avec l'énumération de la DLL.
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

        /// <summary>
        /// Mappe l'énumération TypeActivite de la couche Data vers celle de la DLL Core.
        /// </summary>
        private CoreEnums.TypeActivite MapToCoreTypeActivite(PlanAthena.Data.TypeActivite sourceType)
        {
            switch (sourceType)
            {
                case PlanAthena.Data.TypeActivite.Tache:
                    return CoreEnums.TypeActivite.Tache;
                case PlanAthena.Data.TypeActivite.JalonUtilisateur:
                    return CoreEnums.TypeActivite.JalonUtilisateur;
                case PlanAthena.Data.TypeActivite.JalonDeSynchronisation:
                case PlanAthena.Data.TypeActivite.JalonTechnique:
                    return CoreEnums.TypeActivite.JalonTechnique;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"Valeur TypeActivite non supportée pour le mapping : {sourceType}");
            }
        }
    }
}