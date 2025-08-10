// Fichier : Utilities/ConfigurationBuilder.cs

using PlanAthena.Services.DataAccess;
using PlanAthena.Services.Business.DTOs;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Construit les objets de configuration à partir de l'interface utilisateur.
    /// </summary>
    public class ConfigurationBuilder
    {
        public ConfigurationPlanification ConstruireDepuisUI(
        List<DayOfWeek> joursOuvres,
        int heureDebut,
        int heuresTravail,
        string typeSortie,
        string description,
        DateTime? dateDebut,
        DateTime? dateFin,
        int dureeStandard,
        decimal penaliteChangement,
        long coutIndirectAbsolu,
        int dureeCalculMaxMinutes)
        {
            return new ConfigurationPlanification
            {
                // Paramètres existants
                JoursOuvres = joursOuvres,
                HeureDebutJournee = heureDebut,
                HeuresTravailEffectifParJour = heuresTravail,
                TypeDeSortie = ConvertirTypeDeSortie(typeSortie),
                Description = description,
                DateDebutSouhaitee = dateDebut,
                DateFinSouhaitee = dateFin,
                DureeJournaliereStandardHeures = dureeStandard,
                PenaliteChangementOuvrierPourcentage = penaliteChangement,

                // Nouveaux paramètres
                CoutIndirectJournalierAbsolu = coutIndirectAbsolu,
                DureeCalculMaxMinutes = dureeCalculMaxMinutes,

                // --- MOCKING POUR COMPATIBILITÉ ---
                // La DLL actuelle s'attend à un pourcentage. On met une valeur par défaut (0).
                // La logique de la DLL ne devrait pas utiliser ce champ si on passe bien
                // le coût absolu dans les nouvelles versions.
                CoutIndirectJournalierPourcentage = 15m,

                /*
                // --- PRÉPARATION POUR LA FUTURE DLL ---
                // Lorsque la DLL sera mise à jour pour utiliser directement les nouvelles valeurs,
                // il faudra :
                // 1. Supprimer le champ "CoutIndirectJournalierPourcentage" de la DTO ConfigurationPlanification.
                // 2. Transmettre les champs "CoutIndirectJournalierAbsolu" et "DureeCalculMaxMinutes"
                //    directement à la façade du solveur.
                //
                // Exemple d'appel futur à la façade (à adapter) :
                //
                // var inputDto = _dataTransformer.TransformToChantierSetupDto(...);
                // inputDto.ParametresSolveur.DureeMaxCalculSecondes = configuration.DureeCalculMaxMinutes * 60;
                // inputDto.ParametresEconomiques.CoutJournalierIndirect = configuration.CoutIndirectJournalierAbsolu;
                */
            };
        }

        public ConfigurationExportGantt ConstruireConfigExportGantt(string nomProjet, double heuresParJour, IEnumerable<DayOfWeek> joursOuvres)
        {
            return new ConfigurationExportGantt
            {
                NomProjet = nomProjet,
                HeuresParJour = heuresParJour,
                JoursOuvres = joursOuvres
            };
        }

        private string ConvertirTypeDeSortie(string selectionUI)
        {
            return selectionUI switch
            {
                "Optimisation Coût" => "OPTIMISATION_COUT",
                "Optimisation Délai" => "OPTIMISATION_DELAI",
                _ => "Analyse et Estimation"
            };
        }
    }
}