using PlanAthena.Services.DTOs.Projet;
namespace PlanAthena.Services.DTOs.ProjectPersistence
{
    /// <summary>
    /// Résultat de validation d'un projet
    /// </summary>
    public class ValidationProjet
    {
        public bool EstValide { get; set; }
        public List<string> Erreurs { get; set; } = new List<string>();
        public List<string> Avertissements { get; set; } = new List<string>();
        public InformationsProjet InformationsProjet { get; set; }
    }
}
