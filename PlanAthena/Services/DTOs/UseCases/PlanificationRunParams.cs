using System;

namespace PlanAthena.Services.DTOs.UseCases
{
    /// <summary>
    /// DTO contenant les paramètres éphémères pour une seule exécution de la planification.
    /// Ces données sont construites par la vue et ne sont pas persistées avec le projet.
    /// </summary>
    public class PlanificationRunParams
    {
        public DateTime DateDebutSouhaitee { get; set; }
        public DateTime DateFinSouhaitee { get; set; }
        public decimal PenaliteChangementOuvrierPourcentage { get; set; }
        public int DureeCalculMaxSecondes { get; set; }
        public string TypeDeSortie { get; set; } // ex: "OPTIMISATION_DELAI"
        public int SeuilJoursDecoupageTache { get; set; }
    }
}