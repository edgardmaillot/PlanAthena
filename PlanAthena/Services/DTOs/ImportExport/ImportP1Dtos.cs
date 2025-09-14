using System.Collections.Generic;
using System.Drawing;

namespace PlanAthena.Services.DTOs.ImportExport
{
    /// <summary>
    /// DTO fourni par l'Orchestrateur pour configurer l'écran ImportP1.
    /// </summary>
    public class ImportP1Config
    {
        /// <summary>
        /// Le nom de l'entité à afficher dans le titre (ex: "Ouvriers", "Tâches").
        /// </summary>
        public string EntityDisplayName { get; set; }

        /// <summary>
        /// L'image à afficher pour représenter l'entité.
        /// </summary>
        public Image EntityImage { get; set; }

        /// <summary>
        /// La liste des champs que l'IHM doit générer dynamiquement.
        /// </summary>
        public List<MappingFieldDefinition> FieldsToMap { get; set; } = new List<MappingFieldDefinition>();

        /// <summary>
        /// Les en-têtes lus depuis la première ligne du fichier CSV.
        /// </summary>
        public List<string> CsvHeaders { get; set; } = new List<string>();

        /// <summary>
        /// Un aperçu des 10 premières lignes de données pour le DataGridView.
        /// </summary>
        public List<string[]> DataPreview { get; set; } = new List<string[]>();

        /// <summary>
        /// L'état initial de la checkbox "La première ligne contient des en-têtes".
        /// </summary>
        public bool InitialHasHeaderState { get; set; } = true;

        /// <summary>
        /// Les mappings suggérés par ValueMappingService.
        /// Clé = InternalName (ex: "Nom"), Valeur = CsvHeader (ex: "lastname").
        /// </summary>
        public Dictionary<string, string> SuggestedMappings { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Décrit un champ cible à mapper et comment il doit se comporter dans l'IHM.
    /// </summary>
    public class MappingFieldDefinition
    {
        /// <summary>
        /// L'identifiant technique du champ (ex: "TauxJour", "TaskName").
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Le nom à afficher dans le label pour l'utilisateur (ex: "Coût journalier").
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Indique si ce champ doit être obligatoirement mappé ou avoir une valeur par défaut.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Indique si l'utilisateur peut fournir une valeur par défaut pour ce champ.
        /// </summary>
        public bool AllowDefaultValue { get; set; }
    }

    /// <summary>
    /// DTO exposé par ImportP1 contenant les choix de mapping de l'utilisateur.
    /// </summary>
    public class ImportP1Result
    {
        /// <summary>
        /// L'état final de la checkbox "La première ligne contient des en-têtes".
        /// </summary>
        public bool HasHeader { get; set; }

        /// <summary>
        /// L'état final de la checkbox "Mémoriser mes choix".
        /// </summary>
        public bool ShouldMemorizeMappings { get; set; }

        /// <summary>
        /// La liste des décisions de mapping de l'utilisateur.
        /// </summary>
        public List<FieldMappingResult> FieldMappings { get; set; } = new List<FieldMappingResult>();
    }

    /// <summary>
    /// Décrit comment un champ cible a été mappé par l'utilisateur.
    /// </summary>
    public class FieldMappingResult
    {
        /// <summary>
        /// L'identifiant technique du champ (ex: "TauxJour").
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// L'en-tête CSV auquel le champ a été mappé (peut être null).
        /// </summary>
        public string MappedCsvHeader { get; set; }

        /// <summary>
        /// La valeur par défaut fournie par l'utilisateur (peut être null).
        /// </summary>
        public string DefaultValue { get; set; }
    }
}