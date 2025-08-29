// 0.4.8 : Fichier: ConsolidatedPlanning.cs
using System;
using System.Collections.Generic;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Représente un planning consolidé avec des segments de travail journaliers et des jalons.
    /// C'est une structure de données propre, prête à être utilisée par les services d'analyse,
    /// de reporting ou de mise à jour de statut.
    /// </summary>
    public class ConsolidatedPlanning
    {
        /// <summary>
        /// La date de début calculée du premier événement (tâche ou jalon) du projet.
        /// </summary>
        public DateTime DateDebutProjet { get; set; }

        /// <summary>
        /// La date de fin calculée du dernier événement (tâche ou jalon) du projet.
        /// </summary>
        public DateTime DateFinProjet { get; set; }

        /// <summary>
        /// Dictionnaire des segments de travail effectif, regroupés par ID d'ouvrier.
        /// Ne contient que les tâches qui représentent un travail réel.
        /// </summary>
        public Dictionary<string, List<SegmentDeTravail>> SegmentsParOuvrierId { get; set; }

        /// <summary>
        /// Liste des jalons utilisateur (ex: séchage, attente) qui représentent des blocs de temps
        /// continus et non-ouvrés. Ces éléments ne sont pas découpés.
        /// </summary>
        public List<JalonPlanifie> JalonsPlanifies { get; set; }

        public ConsolidatedPlanning()
        {
            SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>();
            JalonsPlanifies = new List<JalonPlanifie>(); // Initialisation du nouveau champ
        }
    }
}