namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Exception spécifique aux erreurs de projet
    /// </summary>
    public class ProjetException : Exception
    {
        public ProjetException(string message) : base(message) { }
        public ProjetException(string message, Exception innerException) : base(message, innerException) { }
    }
}