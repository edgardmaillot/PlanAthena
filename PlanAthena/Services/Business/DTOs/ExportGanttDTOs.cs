// Fichier: Services/Business/DTOs/ExportGanttDTOs.cs

using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Configuration pour l'export GanttProject
    /// </summary>
    public class ConfigurationExportGantt
    {
        public string NomProjet { get; set; } = "Planning PlanAthena";
        public double HeuresParJour { get; set; } = 8.0;
        public IEnumerable<DayOfWeek> JoursOuvres { get; set; } = new[] {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday
        };
    }

    /// <summary>
    /// Représente un groupe de tâches pour l'export Gantt
    /// </summary>
    public class TacheGroupee
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int DureeJours { get; set; }
        public double HeuresTotal { get; set; }
        public List<AffectationDto> Affectations { get; set; } = new List<AffectationDto>();
    }

    /// <summary>
    /// Représente une tâche formatée pour GanttProject
    /// </summary>
    public class TacheGantt
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int Duree { get; set; }
        public int Avancee { get; set; }
        public double Cout { get; set; }
        public int Priorite { get; set; }
        public string Responsable { get; set; } = "";
        public string Predecesseurs { get; set; } = "";
        public int NumeroHierarchique { get; set; }
        public string Ressources { get; set; } = "";
        public string Assignments { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    /// <summary>
    /// Représente une ressource formatée pour GanttProject
    /// </summary>
    public class RessourceGantt
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public string Role { get; set; } = "Default:0";
        public string Email { get; set; } = "";
        public string Telephone { get; set; } = "";
        public int TauxNormal { get; set; }
        public int CoutTotal { get; set; }
        public double ChargeTotal { get; set; }
    }
}