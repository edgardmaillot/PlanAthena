// Fichier: PlanAthena/Services/Business/DTOs/OuvrierDTOs.cs
// Version: 0.4.4
// Description: Centralisation des DTOs relatifs aux Ouvriers, incluant les statistiques.

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Informations consolidées sur un ouvrier pour affichage dans les listes.
    /// </summary>
    public class OuvrierInfo
    {
        public string OuvrierId { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public int CoutJournalier { get; set; }
        public int NombreCompetences { get; set; }
        public string NomComplet => $"{Prenom} {Nom}";
    }

    /// <summary>
    /// Statistiques consolidées sur l'ensemble des ouvriers.
    /// </summary>
    public class StatistiquesOuvriers
    {
        public int NombreOuvriersTotal { get; set; }
        public int NombreCompetencesTotal { get; set; }
        public double CoutJournalierMoyen { get; set; }
        public int CoutJournalierMin { get; set; }
        public int CoutJournalierMax { get; set; }
        public double NombreCompetencesParOuvrierMoyen { get; set; }
    }
}