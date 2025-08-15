// Fichier: PlanAthena/Data/ProjetData.cs
// Version: 0.4.4
// Description: Simplification du DTO de persistance. Suppression de la liste redondante de Blocs
// et de la définition de la classe Metier (maintenant dans son propre fichier).

using PlanAthena.Services.Business.DTOs;
using System;
using System.Collections.Generic;

namespace PlanAthena.Data
{
    /// <summary>
    /// DTO racine pour la sérialisation/désérialisation d'un fichier de projet.
    /// </summary>
    public class ProjetData
    {
        public InformationsProjet InformationsProjet { get; set; } = new InformationsProjet();
        public List<Metier> Metiers { get; set; } = new List<Metier>();
        public List<Ouvrier> Ouvriers { get; set; } = new List<Ouvrier>();
        public List<Tache> Taches { get; set; } = new List<Tache>();
        public List<Lot> Lots { get; set; } = new List<Lot>();
        public List<Bloc> Blocs { get; set; } = new List<Bloc>();

        // La liste des blocs est maintenant persistée via la hiérarchie dans chaque Lot.


        public DateTime DateSauvegarde { get; set; }
        public string VersionApplication { get; set; } = "";
    }

}