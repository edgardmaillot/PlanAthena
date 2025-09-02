namespace PlanAthena.Data
{
    [Flags]
    public enum ChantierPhase
    {
        None = 0,
        GrosOeuvre = 1,
        SecondOeuvre = 2,
        Finition = 4,
        GarantieDecennale = 8 // Anticipe la future phase
    }
}