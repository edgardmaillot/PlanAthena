// Fichier : Facade/Dto/Output/PlanningAnalysisReportDto.cs

namespace PlanAthena.Core.Facade.Dto.Output
{
    public record PlanningAnalysisReportDto
    {
        public required GlobalKpiDto KpisGlobaux { get; init; }
        public required IReadOnlyList<WorkerKpiDto> KpisParOuvrier { get; init; }
    }

    public record GlobalKpiDto
    {
        // Note : Les coûts et durées sont dans PlanningOptimizationResultDto.
        // On ne les duplique pas ici, sauf si nécessaire.
        public double TauxOccupationMoyenPondere { get; init; }
    }

    public record WorkerKpiDto
    {
        public required string OuvrierId { get; init; }
        public required string OuvrierNom { get; init; }
        public int JoursDePresence { get; init; }
        public double HeuresTravaillees { get; init; }
        public double TauxOccupation { get; init; }
        public double TauxFragmentation { get; init; }
    }
}