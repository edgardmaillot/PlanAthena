// 0.4.8 : Fichier: ConsolidatedPlanning.cs
using System;
using System.Collections.Generic;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Représente un planning consolidé avec des segments de travail journaliers.
    /// C'est une structure de données propre, prête à être utilisée par les services d'analyse,
    /// de reporting ou de mise à jour de statut.
    /// </summary>
    public class ConsolidatedPlanning
    {
        /// <summary>
        /// La date de début calculée du premier segment de travail du projet.
        /// </summary>
        public DateTime DateDebutProjet { get; set; }

        /// <summary>
        /// La date de fin calculée du dernier segment de travail du projet.
        /// </summary>
        public DateTime DateFinProjet { get; set; }

        /// <summary>
        /// Dictionnaire des segments de travail regroupés par ID d'ouvrier (réel ou virtuel).
        /// Clé: OuvrierId, Valeur: Liste des segments de travail pour cet ouvrier.
        /// </summary>
        public Dictionary<string, List<SegmentDeTravail>> SegmentsParOuvrierId { get; set; }

        public ConsolidatedPlanning()
        {
            SegmentsParOuvrierId = new Dictionary<string, List<SegmentDeTravail>>();
        }
    }
}