// Fichier: PlanAthena/Services/Business/DTOs/ProjetDTOs.cs
// Version: 0.4.4
// Description: Fichier nettoyé, ajout des 'using' nécessaires pour référencer
// les DTOs de statistiques qui ont été déplacés dans leurs propres fichiers.

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
        public int HeureOuverture { get; set; } = 8;
        public int HeureFermeture { get; set; } = 16;
        public int DureeTravailHeures { get; set; } = 7;
        public decimal CoutJournalier { get; set; } = 500;
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
    /// Résumé complet d'un projet pour affichage.
    /// </summary>
    public class ResumeProjet
    {
        public int NombreMetiers { get; set; }
        public StatistiquesOuvriers StatistiquesOuvriers { get; set; } = new StatistiquesOuvriers();
        public StatistiquesTaches StatistiquesTaches { get; set; } = new StatistiquesTaches();
        public StatistiquesMappingMetiers StatistiquesMappingMetiers { get; set; } = new StatistiquesMappingMetiers();
    }
}