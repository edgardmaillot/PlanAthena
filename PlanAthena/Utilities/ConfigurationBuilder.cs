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