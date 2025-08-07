// Data/Ouvrier.cs - Structure mise à jour V0.4.2

using PlanAthena.Services.Business.DTOs; // Pour CompetenceOuvrier
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PlanAthena.Data
{
    /// <summary>
    /// Représente un ouvrier avec ses compétences multiples.
    /// 🔄 ARCHITECTURE V0.4.2 : Un ouvrier = un ID unique avec compétences multiples
    /// </summary>
    public class Ouvrier
    {
        /// <summary>
        /// Identifiant unique de l'ouvrier.
        /// ✅ ID unique - plus de duplication pour multi-métiers
        /// </summary>
        public string OuvrierId { get; set; } = "";

        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public int CoutJournalier { get; set; }

        // 🗑️ SUPPRIMÉ V0.4.2 : public string MetierId { get; set; }
        // 🗑️ SUPPRIMÉ V0.4.2 : public NiveauExpertise NiveauExpertise { get; set; }
        // 🗑️ SUPPRIMÉ V0.4.2 : public int? PerformancePct { get; set; }

        /// <summary>
        /// 🆕 NOUVEAU V0.4.2 : Liste des compétences métiers de l'ouvrier.
        /// Remplace le workaround multi-métiers par duplication OuvrierId.
        /// </summary>
        public List<CompetenceOuvrier> Competences { get; set; } = new();

        /// <summary>
        /// 🔄 PROPRIÉTÉ HELPER : Compatibilité temporaire pour migration.
        /// Retourne le MetierId principal pour l'affichage/export.
        /// À SUPPRIMER après migration complète.
        /// </summary>
        [JsonIgnore]
        public string MetierId
        {
            get
            {
                var metierPrincipal = Competences?.FirstOrDefault(c => c.EstMetierPrincipal);
                return metierPrincipal?.MetierId ?? Competences?.FirstOrDefault()?.MetierId ?? "";
            }
            set
            {
                // Migration automatique : définit comme métier principal unique
                if (string.IsNullOrWhiteSpace(value)) return;

                Competences ??= new List<CompetenceOuvrier>();

                // Supprime l'ancien métier principal
                var ancienPrincipal = Competences.FirstOrDefault(c => c.EstMetierPrincipal);
                if (ancienPrincipal != null)
                    ancienPrincipal.EstMetierPrincipal = false;

                // Ajoute ou met à jour le nouveau métier principal
                var competenceExistante = Competences.FirstOrDefault(c => c.MetierId == value);
                if (competenceExistante != null)
                {
                    competenceExistante.EstMetierPrincipal = true;
                }
                else
                {
                    Competences.Add(new CompetenceOuvrier
                    {
                        MetierId = value,
                        EstMetierPrincipal = true
                    });
                }
            }
        }

        /// <summary>
        /// Nom complet formaté pour l'affichage
        /// </summary>
        [JsonIgnore]
        public string NomComplet => $"{Prenom} {Nom}";
    }
}