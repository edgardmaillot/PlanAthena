// Modification dans ProjetData.cs - Classe Metier mise à jour

using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlanAthena.Data
{
    public class ProjetData
    {
        public InformationsProjet InformationsProjet { get; set; } = new InformationsProjet();
        public List<Metier> Metiers { get; set; } = new List<Metier>();
        public List<Ouvrier> Ouvriers { get; set; } = new List<Ouvrier>();
        public List<Tache> Taches { get; set; } = new List<Tache>();

        public List<Lot> Lots { get; set; } = new List<Lot>();
        public List<Bloc> Blocs { get; set; } = new List<Bloc>();

        public DateTime DateSauvegarde { get; set; }
        public string VersionApplication { get; set; } = "";
    }

    /// <summary>
    /// Définition de la classe Metier avec prérequis par phase V0.4.2
    /// 🔄 MODIFICATION MAJEURE : PrerequisMetierIds → PrerequisParPhase
    /// </summary>
    public class Metier
    {
        public string MetierId { get; set; } = "";
        public string Nom { get; set; } = "";

        // 🗑️ SUPPRIMÉ V0.4.2 : public string PrerequisMetierIds { get; set; }

        /// <summary>
        /// 🆕 NOUVEAU V0.4.2 : Prérequis métiers contextuels par phase de chantier.
        /// Remplace PrerequisMetierIds pour gérer les précédences selon les phases.
        /// 
        /// Exemple concret:
        /// Électricien.PrerequisParPhase = {
        ///   [GrosOeuvre]: [],                    // Gainage - aucun prérequis
        ///   [SecondOeuvre]: ["M002"],           // Maçon fini pour câblage
        ///   [Finition]: ["M012"]                // Peintre fini pour finitions électriques
        /// }
        /// </summary>
        public Dictionary<ChantierPhase, List<string>> PrerequisParPhase { get; set; } = new();

        public string CouleurHex { get; set; } = ""; // Couleur au format hexadécimal (#RRGGBB)
        public string Pictogram { get; set; } = "";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChantierPhase Phases { get; set; } = ChantierPhase.None;

        /// <summary>
        /// 🔄 MÉTHODE HELPER : Compatibilité temporaire pour migration depuis PrerequisMetierIds.
        /// À SUPPRIMER après migration complète des données.
        /// </summary>
        [JsonIgnore]
        public string PrerequisMetierIds
        {
            get
            {
                // Génère une représentation string des prérequis (toutes phases confondues)
                var allPrereqs = new HashSet<string>();
                foreach (var prereqs in PrerequisParPhase.Values)
                {
                    foreach (var prereq in prereqs)
                    {
                        allPrereqs.Add(prereq);
                    }
                }
                return string.Join(",", allPrereqs);
            }
            set
            {
                // Migration automatique : distribue les prérequis sur toutes les phases d'intervention
                if (string.IsNullOrWhiteSpace(value)) return;

                var prerequisIds = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                PrerequisParPhase.Clear();

                // Distribuer sur les phases définies pour ce métier
                if (Phases != ChantierPhase.None)
                {
                    var phasesActives = new List<ChantierPhase>();
                    if (Phases.HasFlag(ChantierPhase.GrosOeuvre)) phasesActives.Add(ChantierPhase.GrosOeuvre);
                    if (Phases.HasFlag(ChantierPhase.SecondOeuvre)) phasesActives.Add(ChantierPhase.SecondOeuvre);
                    if (Phases.HasFlag(ChantierPhase.Finition)) phasesActives.Add(ChantierPhase.Finition);

                    foreach (var phase in phasesActives)
                    {
                        PrerequisParPhase[phase] = prerequisIds.ToList();
                    }
                }
            }
        }
    }
}