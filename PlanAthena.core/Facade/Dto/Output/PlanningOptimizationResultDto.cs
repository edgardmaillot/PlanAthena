// Fichier : Facade/Dto/Output/PlanningOptimizationResultDto.cs

using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Core.Facade.Dto.Output
{
    public record PlanningOptimizationResultDto
    {
        public string ChantierId { get; init; }
        public OptimizationStatus Status { get; init; }

        // --- MODIFICATIONS ---
        // Décomposition des coûts (en centimes)
        public long? CoutTotalEstime { get; init; }
        public long? CoutTotalRhEstime { get; init; }
        public long? CoutTotalIndirectEstime { get; init; }

        // Renommé pour plus de clarté
        public long? DureeTotaleEnSlots { get; init; }

        // Ajout de la liste détaillée des affectations
        public IReadOnlyList<AffectationDto> Affectations { get; init; } = Array.Empty<AffectationDto>();
    }

    // Ce sous-DTO est ajouté au même fichier pour la simplicité
    public class AffectationDto
    {
        public string TacheId { get; set; }
        public string TacheNom { get; set; }
        public string OuvrierId { get; set; }
        public string OuvrierNom { get; set; }
        public string BlocId { get; set; }
        public DateTime DateDebut { get; set; }
        public double DureeHeures { get; set; }
        public DateTime DateFin { get; set; }


        /// <summary>
        /// Type d'activité (Tache, JalonUtilisateur, JalonTechnique).
        /// Permet de distinguer le travail réel des points de repère temporels.
        /// </summary>
        public TypeActivite TypeActivite { get; set; }

        /// <summary>
        /// Indique si cette affectation représente un jalon (true) ou une tâche de travail (false).
        /// Propriété calculée pour faciliter l'affichage et les exports.
        /// </summary>
        public bool EstJalon => TypeActivite != TypeActivite.Tache;

        /// <summary>
        /// Durée originale en heures telle que définie par l'utilisateur.
        /// Pour les jalons: conserve la vraie durée d'attente (72h, 24h, etc.)
        /// Pour les tâches: généralement identique à DureeHeures
        /// Utilisé principalement pour l'export Gantt précis.
        /// </summary>
        public double? DureeOriginaleHeures { get; set; }
    }
}