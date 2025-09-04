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
        // --- Champs d'identification du projet ---
        public string NomProjet { get; set; } = "Nouveau Projet";
        public string Description { get; set; } = "";
        public DateTime DateCreation { get; set; }
        public DateTime DateDerniereModification { get; set; }
        public string Auteur { get; set; } = "";

        // --- Champs de configuration stable fusionnés ---

        public List<DayOfWeek> JoursOuvres { get; set; } = new List<DayOfWeek>();

        public int HeureOuverture { get; set; } = 8;

        public int HeureFermeture { get; set; } = 16;

        public int DureeJournaliereStandardHeures { get; set; } = 8;

        public int DureeTravailHeures { get; set; } = 7;

        public decimal CoutJournalier { get; set; } = 500;
    }
}