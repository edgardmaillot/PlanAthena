// Emplacement: /Data/AffectationOuvrier.cs

namespace PlanAthena.Data
{
    /// <summary>
    /// Représente l'affectation d'un ouvrier à une tâche avec le nombre d'heures travaillées.
    /// Utilisé dans la propriété enrichie 'Affectations' de la classe Tache.
    /// Ce modèle est persistant et sauvegardé avec le projet.
    /// </summary>
    public record AffectationOuvrier
    {
        public string OuvrierId { get; init; }
        public string NomOuvrier { get; init; } // Dénormalisé pour un accès facile par l'IHM
        public int HeuresTravaillees { get; init; }
    }
}