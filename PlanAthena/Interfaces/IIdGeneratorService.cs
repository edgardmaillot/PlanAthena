// START OF FILE IIdGeneratorService.cs

using PlanAthena.Data;

namespace PlanAthena.Interfaces
{
    /// <summary>
    /// Définit le contrat pour un service de génération et de validation d'identifiants.
    /// L'implémentation de cette interface doit être sans état et opérer uniquement
    /// sur les données fournies en paramètres pour garantir le découplage et la testabilité.
    /// </summary>
    public interface IIdGeneratorService
    {
        /// <summary>
        /// Génère le prochain identifiant de lot disponible (ex: L001).
        /// </summary>
        /// <param name="lotsExistants">La liste de tous les lots déjà existants pour déterminer le prochain numéro disponible.</param>
        /// <returns>Le nouvel identifiant de lot unique.</returns>
        /// <exception cref="InvalidOperationException">Levée si la limite de 999 lots est atteinte.</exception>
        string GenererProchainLotId(IReadOnlyList<Lot> lotsExistants);

        /// <summary>
        /// Génère le prochain identifiant de bloc disponible pour un lot donné (ex: L001_B001).
        /// </summary>
        /// <param name="lotId">L'identifiant du lot parent.</param>
        /// <param name="blocsExistants">La liste de tous les blocs déjà existants.</param>
        /// <returns>Le nouvel identifiant de bloc unique.</returns>
        /// <exception cref="InvalidOperationException">Levée si la limite de 999 blocs est atteinte pour ce lot.</exception>
        /// <exception cref="ArgumentException">Levée si le format de lotId est invalide.</exception>
        string GenererProchainBlocId(string lotId, IReadOnlyList<Bloc> blocsExistants);

        /// <summary>
        /// Génère le prochain identifiant de tâche ou de jalon disponible pour un bloc donné (ex: L001_B001_T001 ou L001_B001_J001).
        /// </summary>
        /// <param name="blocId">L'identifiant du bloc parent.</param>
        /// <param name="tachesExistantes">La liste de toutes les tâches et jalons déjà existants.</param>
        /// <param name="type">Le type d'activité (Tache ou Jalon) pour déterminer le préfixe (T ou J).</param>
        /// <returns>Le nouvel identifiant de tâche ou jalon unique.</returns>
        /// <exception cref="InvalidOperationException">Levée si la limite de 999 éléments est atteinte pour ce type et ce bloc.</exception>
        /// <exception cref="ArgumentException">Levée si le format de blocId est invalide.</exception>
        string GenererProchainTacheId(string blocId, IReadOnlyList<Tache> tachesExistantes, TypeActivite type = TypeActivite.Tache);

        /// <summary>
        /// Génère le prochain identifiant de métier disponible (ex: M001).
        /// </summary>
        /// <param name="metiersExistants">La liste de tous les métiers déjà existants.</param>
        /// <returns>Le nouvel identifiant de métier unique.</returns>
        /// <exception cref="InvalidOperationException">Levée si la limite de 999 métiers est atteinte.</exception>
        string GenererProchainMetierId(IReadOnlyList<Metier> metiersExistants);

        /// <summary>
        /// Génère le prochain identifiant d'ouvrier disponible (ex: W001).
        /// </summary>
        /// <param name="ouvriersExistants">La liste de tous les ouvriers déjà existants.</param>
        /// <returns>Le nouvel identifiant d'ouvrier unique.</returns>
        /// <exception cref="InvalidOperationException">Levée si la limite de 999 ouvriers est atteinte.</exception>
        string GenererProchainOuvrierId(IReadOnlyList<Ouvrier> ouvriersExistants);

        /// <summary>
        /// Prend un identifiant potentiellement non standard (provenant d'un import CSV) et le normalise.
        /// Si l'ID est déjà conforme, il est retourné. Sinon, un nouvel ID standard est généré.
        /// </summary>
        /// <param name="idOriginal">L'identifiant provenant du fichier source.</param>
        /// <param name="blocIdCible">L'ID du bloc auquel cette activité sera rattachée, utilisé pour générer un nouvel ID si nécessaire.</param>
        /// <param name="tachesExistantes">La liste de toutes les tâches existantes, nécessaire pour générer un nouvel ID unique.</param>
        /// <param name="type">Le type d'activité à générer si une normalisation est nécessaire.</param>
        /// <returns>Un identifiant de tâche valide et conforme au format standard de l'application.</returns>
        string NormaliserIdDepuisCsv(string idOriginal, string blocIdCible, IReadOnlyList<Tache> tachesExistantes, TypeActivite type = TypeActivite.Tache);
    }
}