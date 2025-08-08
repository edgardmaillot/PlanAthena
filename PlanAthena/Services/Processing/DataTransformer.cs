using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreEnums = PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service de transformation des données V0.4.2.1
    /// 🔧 VERSION FINALE CORRIGÉE : Gère la nouvelle structure Ouvrier/Compétences,
    /// Metier/PrerequisParPhase et fournit des valeurs par défaut pour les champs supprimés
    /// (NiveauExpertise, PerformancePct) afin de respecter le contrat de la DLL Core.
    /// </summary>
    public class DataTransformer
    {
        private readonly ProjetService _projetService;
        private readonly BlocService _blocService;

        public DataTransformer(ProjetService projetService, BlocService blocService)
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
        }

        public ChantierSetupInputDto TransformToChantierSetupDto(
            List<Ouvrier> ouvriers,
            List<Tache> processedTaches,
            List<Metier> allMetiers,
            ConfigurationPlanification configurationPlanification)
        {
            ArgumentNullException.ThrowIfNull(ouvriers);
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

            // Transformation des blocs depuis le BlocService
            var blocsDto = _blocService.ObtenirTousLesBlocs()
                .Select(b => new BlocTravailDto
                {
                    BlocId = b.BlocId,
                    Nom = b.Nom,
                    CapaciteMaxOuvriers = b.CapaciteMaxOuvriers
                }).ToList();

            // Transformation des lots depuis le ProjetService
            var blocIdsParLot = processedTaches
                .GroupBy(t => t.LotId)
                .ToDictionary(g => g.Key, g => g.Select(t => t.BlocId).Distinct().ToList());

            var lotsDto = _projetService.ObtenirTousLesLots()
                .Select(l => new LotTravauxDto
                {
                    LotId = l.LotId,
                    Nom = l.Nom,
                    Priorite = l.Priorite,
                    BlocIds = blocIdsParLot.GetValueOrDefault(l.LotId, new List<string>())
                }).ToList();

            // 🔧 NOUVELLE LOGIQUE V0.4.2.1 - Transformation des métiers
            // On envoie au Core la liste de tous les prérequis, toutes phases confondues,
            // car le contrat de la DLL Core ne gère pas encore les phases.
            var metiersDto = allMetiers.Select(m => new MetierDto
            {
                MetierId = m.MetierId,
                Nom = m.Nom,
                PrerequisMetierIds = _projetService.GetTousPrerequisConfondus(m.MetierId).ToArray()
            }).ToList();

            // 🔧 NOUVELLE LOGIQUE V0.4.2.1 - Transformation des ouvriers
            // On parcourt directement la liste des ouvriers uniques et leurs compétences.
            var ouvriersDto = ouvriers.Select(ouvrier => new OuvrierDto
            {
                OuvrierId = ouvrier.OuvrierId,
                Nom = ouvrier.Nom,
                Prenom = ouvrier.Prenom,
                CoutJournalier = ouvrier.CoutJournalier,
                Competences = ouvrier.Competences.Select(comp => new CompetenceDto
                {
                    MetierId = comp.MetierId,

                    // 🔧 CORRECTION FINALE : On envoie des valeurs par défaut VALIDES pour les champs
                    // qui ont été supprimés de notre modèle mais sont TOUJOURS REQUIS par le Core.

                    // Pour NiveauExpertise, on envoie "Confirmé" pour passer la validation de l'enum.
                    Niveau = CoreEnums.NiveauExpertise.Confirme,

                    // Pour PerformancePct, on envoie 100% par défaut.
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
                    CoutIndirectJournalierPourcentage = configurationPlanification.CoutIndirectJournalierPourcentage
                };
            }

            // Construction du DTO final
            return new ChantierSetupInputDto
            {
                ChantierId = $"CHANTIER_TEST_{DateTime.Now:yyyyMMdd_HHmmss}",
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