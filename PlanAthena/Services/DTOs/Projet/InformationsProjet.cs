using System;
using System.Collections.Generic;

namespace PlanAthena.Services.DTOs.Projet
{
    /// <summary>
    /// Contient les informations générales et la configuration stable (prospective) d'un projet.
    /// C'est la source de vérité pour les données persistantes du projet.
    /// </summary>
    public class InformationsProjet
    {
        // --- Identification ---
        public string NomProjet { get; set; } = "Nouveau Projet";
        public string Description { get; set; } = "";
        public string ImagePath { get; set; } = ""; // Pour l'illustration
        public string Auteur { get; set; } = "";
        public DateTime DateCreation { get; set; }
        public DateTime DateDerniereModification { get; set; }

        // --- Configuration de la Planification ---
        public List<DayOfWeek> JoursOuvres { get; set; } = new List<DayOfWeek>
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday
        };

        public int HeureOuverture { get; set; } = 8;
        public int HeureFermeture { get; set; } = 17; // Plus logique que DureeJournaliere
        public int HeuresTravailEffectifParJour { get; set; } = 7;
        public decimal CoutIndirectJournalierAbsolu { get; set; } = 500;
    }
    public class ProjectSummaryData
    {
        public int NombreTotalTaches { get; set; }
        public int NombreTachesTerminees { get; set; }
        public int NombreTachesEnRetard { get; set; }
    }
}