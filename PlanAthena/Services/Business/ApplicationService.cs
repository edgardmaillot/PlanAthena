using PlanAthena.Services.Business.DTOs;

namespace PlanAthena.Services.Business
{
    public class ApplicationService
    {

        public ConfigurationPlanification ConfigPlanificationActuelle { get; private set; }

        public ApplicationService()

        {
            // Initialiser la configuration de session avec des valeurs par défaut
            InitialiserConfigurationParDefaut();
        }

        private void InitialiserConfigurationParDefaut()
        {
            ConfigPlanificationActuelle = new ConfigurationPlanification
            {
                JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                HeureDebutJournee = 8,
                DureeJournaliereStandardHeures = 8,
                HeuresTravailEffectifParJour = 7,
                CoutIndirectJournalierAbsolu = 500
            };
        }


    }
}