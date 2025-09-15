// Version 0.5.1
using System.ComponentModel;

namespace PlanAthena.Services.DTOs.ImportExport
{
    // --- DTO d'Entrée ---

    /// <summary>
    /// DTO générique fourni par l'Orchestrateur pour configurer l'écran ImportP3.
    /// </summary>
    /// <typeparam name="T">Le type d'objet métier à afficher (Tache, Ouvrier, etc.)</typeparam>
    public class ImportP3Config
    {
        public string EntityDisplayName { get; set; }
        public Image EntityImage { get; set; }

        // La source de données est préparée par l'orchestrateur
        public BindingSource DataSource { get; set; }

        public List<RejectedRowInfo> RejectedRows { get; set; } = new List<RejectedRowInfo>();
    }

    /// <summary>
    /// Contient les informations sur une ligne rejetée du fichier source.
    /// </summary>
    public class RejectedRowInfo
    {
        public int OriginalLineNumber { get; set; }
        public string RawRowContent { get; set; }
        public string Reason { get; set; }
    }

    // --- DTO de Sortie ---

    /// <summary>
    /// DTO générique exposé par ImportP3 contenant l'état final des données.
    /// </summary>
    /// <typeparam name="T">Le type d'objet métier (Tache, Ouvrier, etc.)</typeparam>
    public class ImportP3Result
    {
        // On retourne la liste de travail, l'orchestrateur la convertira en List<T> finale.
        public IBindingList FinalData { get; set; }
    }
}