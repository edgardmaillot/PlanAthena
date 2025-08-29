// 0.4.8 : Fichier: SegmentDeTravail.cs
using System;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Représente une portion de travail indivisible effectuée par un ouvrier sur une tâche
    /// au cours d'une seule journée.
    /// </summary>
    public class SegmentDeTravail
    {
        /// <summary>
        /// Identifiant de l'ouvrier (réel ou virtuel pour les jalons) qui effectue le travail.
        /// </summary>
        public string OuvrierId { get; set; }

        /// <summary>
        /// Identifiant unique de la tâche ou sous-tâche en cours (ex: "T002_P1").
        /// </summary>
        public string TacheId { get; set; }

        /// <summary>
        /// Identifiant de la tâche mère si TacheId est une sous-tâche (ex: "T002").
        /// Peut être null ou vide si la tâche n'est pas une sous-tâche.
        /// </summary>
        public string ParentTacheId { get; set; }

        /// <summary>
        /// Nom de la tâche ou sous-tâche.
        /// </summary>
        public string TacheNom { get; set; }

        /// <summary>
        /// Identifiant du bloc de travail associé.
        /// </summary>
        public string BlocId { get; set; }

        /// <summary>
        /// Le jour calendaire concerné par ce segment (la partie heure est ignorée, toujours 00:00:00).
        /// </summary>
        public DateTime Jour { get; set; }

        /// <summary>
        /// Nombre d'heures travaillées sur cette tâche, par cet ouvrier, pour ce jour spécifique.
        /// </summary>
        public double HeuresTravaillees { get; set; }
        /// <summary>
        /// L'heure de début du segment de travail dans la journée.
        /// </summary>
        public TimeSpan HeureDebut { get; set; }

        /// <summary>
        /// L'heure de fin du segment de travail dans la journée.
        /// </summary>
        public TimeSpan HeureFin { get; set; }
    }
}