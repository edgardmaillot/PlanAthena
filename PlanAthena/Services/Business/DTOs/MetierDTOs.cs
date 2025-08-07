// Services/Business/DTOs/MetierDTOs.cs
// 🆕 NOUVEAU V0.4.2 - DTOs Métiers pour refonte système compétences

using PlanAthena.Data;
using System.Collections.Generic;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// DTO représentant une compétence d'un ouvrier dans un métier spécifique.
    /// 🔄 REMPLACE : CompetenceDto (DataTransformer) qui utilisait NiveauExpertise supprimé
    /// </summary>
    public class CompetenceOuvrier
    {
        /// <summary>
        /// Identifiant du métier pour lequel l'ouvrier a une compétence
        /// </summary>
        public string MetierId { get; set; } = "";

        /// <summary>
        /// Indique si ce métier est le métier principal de l'ouvrier.
        /// Utilisé pour l'affichage et l'export (un seul métier principal par ouvrier).
        /// </summary>
        public bool EstMetierPrincipal { get; set; } = false;

        // 🗑️ SUPPRIMÉ : NiveauExpertise, PerformancePct (problèmes RH/légaux identifiés)
    }

    /// <summary>
    /// DTO pour la création de métiers spécifiques (amiante, nucléaire, QSE).
    /// Utilisé par MetierForm Concept 1.
    /// </summary>
    public class MetierSpecifiqueDto
    {
        public string Nom { get; set; } = "";
        public ChantierPhase Phases { get; set; } = ChantierPhase.None;
        public string CouleurHex { get; set; } = "";
        public string Pictogram { get; set; } = "";
    }

    /// <summary>
    /// DTO pour la configuration des prérequis par phase.
    /// Utilisé par MetierForm Concept 2.
    /// </summary>
    public class PrerequisParPhaseDto
    {
        public string MetierId { get; set; } = "";
        public string NomMetier { get; set; } = "";
        public Dictionary<ChantierPhase, List<string>> PrerequisParPhase { get; set; } = new();
    }
}