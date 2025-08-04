using System;
using System.Collections.Generic;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Informations générales sur un projet
    /// </summary>
    public class InformationsProjet
    {
        public string NomProjet { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DateCreation { get; set; }
        public DateTime DateDerniereModification { get; set; }
        public string Auteur { get; set; } = "";
    }

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

    /// <summary>
    /// Résumé complet d'un projet
    /// NOTE: Utilise des DTOs qui doivent être accessibles depuis les services métier
    /// Les types StatistiquesXXX sont supposés être définis dans les services correspondants
    /// </summary>
    public class ResumeProjet
    {
        public int NombreMetiers { get; set; }
        // Note: Ces types sont référencés depuis d'autres services
        // Si erreur de compilation, ajouter les using appropriés ou déplacer ces DTOs
        public StatistiquesOuvriers StatistiquesOuvriers { get; set; } = new StatistiquesOuvriers();
        public StatistiquesTaches StatistiquesTaches { get; set; } = new StatistiquesTaches();
        public StatistiquesMappingMetiers StatistiquesMappingMetiers { get; set; } = new StatistiquesMappingMetiers();
    }
}