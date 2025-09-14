// Fichier: PlanAthena/Services/Processing/DataTransformer.cs
// Version: 0.4.4
// Description: Mise à jour complète pour s'aligner sur la nouvelle architecture.
// Le service opère désormais sur l'objet ProjetData et les pools de ressources globaux.

using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using CoreEnums = PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Services.Processing
{
    public class DataTransformer
    {
        // Le service n'a plus besoin de dépendances pour fonctionner.
        public DataTransformer()
        {
        }

        public ChantierSetupInputDto TransformToChantierSetupDto(
            ProjetData projet,
            List<Ouvrier> poolOuvriers,
            List<Metier> poolMetiers,
            List<Tache> processedTaches,
            ConfigurationPlanification configurationPlanification)
        {
            ArgumentNullException.ThrowIfNull(projet);
            ArgumentNullException.ThrowIfNull(poolOuvriers);
            ArgumentNullException.ThrowIfNull(poolMetiers);
            ArgumentNullException.ThrowIfNull(processedTaches);
            ArgumentNullException.ThrowIfNull(configurationPlanification);

            // Transformation des tâches
            var tachesDto = processedTaches.Select(t => new TacheDto
            {
                TacheId = t.TacheId,
                Nom = t.TacheNom,
                Type = MapToCoreTypeActivite(t.Type),
                BlocId = t.BlocId,
                HeuresHommeEstimees = t.HeuresHommeEstimees,
                MetierId = t.MetierId ?? string.Empty,
                Dependencies = t.Dependencies?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()
            }).ToList();

            // Transformation des blocs (extraits de la hiérarchie des lots)
            var blocsDto = projet.Lots.SelectMany(l => l.Blocs)
                .Select(b => new BlocTravailDto
                {
                    BlocId = b.BlocId,
                    Nom = b.Nom,
                    CapaciteMaxOuvriers = b.CapaciteMaxOuvriers
                }).ToList();

            // Transformation des lots
            var blocIdsParLot = processedTaches
                .GroupBy(t => t.LotId)
                .ToDictionary(g => g.Key, g => g.Select(t => t.BlocId).Distinct().ToList());

            var lotsDto = projet.Lots.Select(l => new LotTravauxDto
            {
                LotId = l.LotId,
                Nom = l.Nom,
                Priorite = l.Priorite,
                BlocIds = blocIdsParLot.GetValueOrDefault(l.LotId, new List<string>())
            }).ToList();

            // Transformation des métiers (depuis le pool de ressources)
            var metiersDto = poolMetiers.Select(m => new MetierDto
            {
                MetierId = m.MetierId,
                Nom = m.Nom,
                // Aplatit les prérequis de toutes les phases pour compatibilité avec le Core
                PrerequisMetierIds = m.PrerequisParPhase.Values.SelectMany(prereqs => prereqs).Distinct().ToArray()
            }).ToList();

            // Transformation des ouvriers (depuis le pool de ressources)
            var ouvriersDto = poolOuvriers.Select(ouvrier => new OuvrierDto
            {
                OuvrierId = ouvrier.OuvrierId,
                Nom = ouvrier.Nom,
                Prenom = ouvrier.Prenom,
                CoutJournalier = ouvrier.CoutJournalier,
                Competences = ouvrier.Competences.Select(comp => new CompetenceDto
                {
                    MetierId = comp.MetierId,
                    // Valeurs par défaut pour les champs supprimés mais requis par le Core
                    Niveau = CoreEnums.NiveauExpertise.Confirme,
                    PerformancePct = 100
                }).ToList()
            }).ToList();

            // Transformation du calendrier
            var calendrierDto = new CalendrierTravailDefinitionDto
            {
                JoursOuvres = configurationPlanification.JoursOuvres,
                HeureDebutJournee = configurationPlanification.HeureDebutJournee,
                HeuresTravailEffectifParJour = configurationPlanification.HeuresTravailEffectifParJour
            };

            // Configuration d'optimisation
            OptimizationConfigDto? optimConfig = null;
            if (configurationPlanification.TypeDeSortie != "Analyse et Estimation")
            {
                optimConfig = new OptimizationConfigDto
                {
                    TypeDeSortie = configurationPlanification.TypeDeSortie ?? string.Empty,
                    DureeJournaliereStandardHeures = configurationPlanification.DureeJournaliereStandardHeures,
                    PenaliteChangementOuvrierPourcentage = configurationPlanification.PenaliteChangementOuvrierPourcentage,
                    CoutIndirectJournalier = configurationPlanification.CoutIndirectJournalierAbsolu,
                    DureeCalculMaxSecondes = configurationPlanification.DureeCalculMaxMinutes * 60
                };
            }

            // Construction du DTO final
            return new ChantierSetupInputDto
            {
                ChantierId = $"CHANTIER_{DateTime.Now:yyyyMMdd_HHmmss}",
                Description = configurationPlanification.Description,
                DateDebutSouhaitee = configurationPlanification.DateDebutSouhaitee,
                DateFinSouhaitee = configurationPlanification.DateFinSouhaitee,
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