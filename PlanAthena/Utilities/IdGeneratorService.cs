// START OF FILE IdGeneratorService.cs

using PlanAthena.Data;
using PlanAthena.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service utilitaire centralisé pour la génération et la validation d'identifiants.
    /// Ce service est volontairement sans dépendances ("service feuille") pour éviter les références circulaires.
    /// Les méthodes requièrent que les données nécessaires (listes d'entités existantes) leur soient fournies en paramètre.
    /// Règles de nommage :
    /// - LotId   : L001-L999
    /// - BlocId  : {LotId}_B001-B999
    /// - TacheId : {BlocId}_T001-T999 ou {BlocId}_J001-J999
    /// - MetierId: M001-M999
    /// - OuvrierId: W001-W999
    /// </summary>
    public class IdGeneratorService : IIdGeneratorService
    {
        // Formats des IDs, définis comme constantes pour la clarté et la performance.
        private const string FormatLot = "L{0:D3}";
        private const string FormatBloc = "{0}_B{1:D3}";
        private const string FormatTache = "{0}_{1}{2:D3}"; // {0}=BlocId, {1}=Prefixe(T/J), {2}=Numero
        private const string FormatMetier = "M{0:D3}";
        private const string FormatOuvrier = "W{0:D3}";

        /// <summary>
        /// Constructeur sans dépendances pour garantir que ce service reste une "feuille" dans l'arbre des dépendances.
        /// </summary>
        public IdGeneratorService()
        {
            // Aucune injection de dépendance ici.
        }

        #region Génération IDs Lots

        public string GenererProchainLotId(IReadOnlyList<Lot> lotsExistants)
        {
            // Extrait tous les numéros de lots déjà utilisés et les stocke dans un HashSet pour une recherche rapide (complexité O(1)).
            var numerosUtilises = lotsExistants
                .Select(l => ExtraireNumeroDepuisId(l.LotId, "L"))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return string.Format(FormatLot, i);
                }
            }

            throw new InvalidOperationException("Impossible de générer un nouvel ID de lot. Limite de 999 lots atteinte.");
        }

        public static bool ValiderFormatLotId(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId)) return false;
            return Regex.IsMatch(lotId, @"^L\d{3}$");
        }

        #endregion

        #region Génération IDs Blocs
        // Prévoir une adaptation suite à la modification de la structure des blocs & lotId dans PROJET
        public string GenererProchainBlocId(string lotId, IReadOnlyList<Bloc> blocsExistants)
        {
            if (!ValiderFormatLotId(lotId))
                throw new ArgumentException("Le format de l'ID du lot fourni est invalide.", nameof(lotId));

            var prefixeRecherche = $"{lotId}_B";

            var numerosUtilises = blocsExistants
                .Where(b => b.BlocId.StartsWith(prefixeRecherche))
                .Select(b => ExtraireNumeroDepuisId(b.BlocId, prefixeRecherche))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return string.Format(FormatBloc, lotId, i);
                }
            }

            throw new InvalidOperationException($"Impossible de générer un nouvel ID de bloc pour le lot {lotId}. Limite de 999 blocs atteinte.");
        }

        public static bool ValiderFormatBlocId(string blocId)
        {
            if (string.IsNullOrWhiteSpace(blocId)) return false;
            return Regex.IsMatch(blocId, @"^L\d{3}_B\d{3}$");
        }

        #endregion

        #region Génération IDs Tâches

        public string GenererProchainTacheId(string blocId, IReadOnlyList<Tache> tachesExistantes, TypeActivite type = TypeActivite.Tache)
        {
            if (!ValiderFormatBlocId(blocId))
                throw new ArgumentException("Le format de l'ID du bloc fourni est invalide.", nameof(blocId));

            // Le préfixe est "T" pour une tâche, "J" pour tout type de jalon.
            var prefixeLettre = EstJalon(type) ? "J" : "T";
            var prefixeRecherche = $"{blocId}_{prefixeLettre}";

            var numerosUtilises = tachesExistantes
                .Where(t => t.TacheId.StartsWith(prefixeRecherche))
                .Select(t => ExtraireNumeroDepuisId(t.TacheId, prefixeRecherche))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return string.Format(FormatTache, blocId, prefixeLettre, i);
                }
            }

            string nomType = EstJalon(type) ? "jalon" : "tâche";
            throw new InvalidOperationException($"Impossible de générer un nouvel ID de {nomType} pour le bloc {blocId}. Limite de 999 éléments atteinte.");
        }

        public static bool ValiderFormatTacheId(string tacheId)
        {
            if (string.IsNullOrWhiteSpace(tacheId)) return false;
            return Regex.IsMatch(tacheId, @"^L\d{3}_B\d{3}_[TJ]\d{3}$");
        }

        #endregion

        #region Génération IDs Métiers

        public string GenererProchainMetierId(IReadOnlyList<Metier> metiersExistants)
        {
            var numerosUtilises = metiersExistants
                .Select(m => ExtraireNumeroDepuisId(m.MetierId, "M"))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return string.Format(FormatMetier, i);
                }
            }
            throw new InvalidOperationException("Impossible de générer un nouvel ID de métier. Limite de 999 métiers atteinte.");
        }

        public static bool ValiderFormatMetierId(string metierId)
        {
            if (string.IsNullOrWhiteSpace(metierId)) return false;
            return Regex.IsMatch(metierId, @"^M\d{3}$");
        }

        #endregion

        #region Génération IDs Ouvriers

        public string GenererProchainOuvrierId(IReadOnlyList<Ouvrier> ouvriersExistants)
        {
            var numerosUtilises = ouvriersExistants
                .Select(o => ExtraireNumeroDepuisId(o.OuvrierId, "W"))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return string.Format(FormatOuvrier, i);
                }
            }
            throw new InvalidOperationException("Impossible de générer un nouvel ID d'ouvrier. Limite de 999 ouvriers atteinte.");
        }

        public static bool ValiderFormatOuvrierId(string ouvrierId)
        {
            if (string.IsNullOrWhiteSpace(ouvrierId)) return false;
            return Regex.IsMatch(ouvrierId, @"^W\d{3}$");
        }

        #endregion

        #region Méthodes utilitaires

        public string NormaliserIdDepuisCsv(string idOriginal, string blocIdCible, IReadOnlyList<Tache> tachesExistantes, TypeActivite type = TypeActivite.Tache)
        {
            // Si l'ID original respecte déjà notre format standard, on le conserve.
            if (ValiderFormatTacheId(idOriginal))
            {
                return idOriginal;
            }

            // Sinon, l'ID n'est pas conforme ou est vide. On génère un nouvel ID standard et sécurisé.
            return GenererProchainTacheId(blocIdCible, tachesExistantes, type);
        }

        public static string ExtraireLotId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var match = Regex.Match(id, @"^(L\d{3})");
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string ExtraireBlocId(string tacheId)
        {
            if (string.IsNullOrWhiteSpace(tacheId)) return null;
            var match = Regex.Match(tacheId, @"^(L\d{3}_B\d{3})");
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// Méthode privée générique pour extraire la partie numérique d'un ID basé sur son préfixe.
        /// </summary>
        /// <param name="id">L'identifiant complet (ex: "L001", "L001_B002", "M042")</param>
        /// <param name="prefixe">Le préfixe à retirer avant de parser le nombre (ex: "L", "L001_B", "M")</param>
        /// <returns>Le numéro extrait, ou null si le format est incorrect.</returns>
        private static int? ExtraireNumeroDepuisId(string id, string prefixe)
        {
            if (string.IsNullOrWhiteSpace(id) || !id.StartsWith(prefixe))
                return null;

            var numeroStr = id.Substring(prefixe.Length);
            if (int.TryParse(numeroStr, out int numero))
                return numero;

            return null;
        }

        /// <summary>
        /// Détermine si le type d'activité correspond à un jalon (de quelque type que ce soit).
        /// </summary>
        private static bool EstJalon(TypeActivite type)
        {
            return type != TypeActivite.Tache;
        }

        #endregion
    }
}