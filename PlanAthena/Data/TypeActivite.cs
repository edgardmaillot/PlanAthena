namespace PlanAthena.Data
{
    /// <summary>
    /// Définit le rôle d'une activité dans le planning.
    /// </summary>
    public enum TypeActivite
    {
        /// <summary>
        /// Une tâche standard qui consomme des ressources et du temps.
        /// </summary>
        Tache,

        /// <summary>
        /// Un jalon créé manuellement par l'utilisateur pour marquer une étape.
        /// </summary>
        JalonUtilisateur,

        /// <summary>
        /// Un jalon généré automatiquement pour synchroniser la fin d'un métier.
        /// </summary>
        JalonDeSynchronisation,

        /// <summary>
        /// Un jalon généré automatiquement pour faire converger des tâches découpées.
        /// </summary>
        JalonTechnique
    }
}