// Fichier: Services/Business/DTOs/ProcessDTOs.cs

using PlanAthena.Data;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Résultat de la préparation des données pour le solveur.
    /// Contient les tâches préparées et la table de mappage parent/enfant.
    /// </summary>
    public class PreparationResult
    {
        /// <summary>
        /// Liste des tâches préparées pour le solveur (avec découpage et jalons techniques)
        /// </summary>
        public List<Tache> TachesPreparees { get; set; } = new List<Tache>();

        /// <summary>
        /// Table de mappage inversée : ID de sous-tâche -> ID de tâche mère
        /// Exemple: "PREP_P1" -> "PREP", "PREP_P2" -> "PREP"
        /// </summary>
        public Dictionary<string, string> ParentIdParSousTacheId { get; set; } = new Dictionary<string, string>();
    }

}