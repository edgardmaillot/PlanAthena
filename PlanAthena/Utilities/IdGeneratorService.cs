using PlanAthena.Data;
using PlanAthena.Services.Business;
using System; // Ajouté pour ArgumentNullException, InvalidOperationException
using System.Collections.Generic; // Ajouté pour HashSet
using System.Linq; // Ajouté pour Select, Where, ToHashSet

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service centralisé pour la génération et validation des IDs selon les règles de nommage
    /// Règles: LotId = L001-L999, BlocId = {LotId}_B001-B999, TacheId = {BlocId}_T001-T999 ou {BlocId}_J001-J999
    /// </summary>
    public class IdGeneratorService
    {
        private readonly LotService _lotService;
        private readonly BlocService _blocService;
        private readonly TacheService _tacheService;
        private readonly ConfigurationIds _config;

        public IdGeneratorService(LotService lotService, BlocService blocService, TacheService tacheService)
        {
            _lotService = lotService ?? throw new ArgumentNullException(nameof(lotService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _config = new ConfigurationIds();
        }

        #region Génération IDs Lots

        /// <summary>
        /// Génère le prochain ID de lot disponible (L001, L002, etc.)
        /// </summary>
        public string GenererProchainLotId()
        {
            var lotsExistants = _lotService.ObtenirTousLesLots();
            var numerosUtilises = lotsExistants
                .Select(l => ExtraireNumeroLot(l.LotId))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return string.Format(_config.FormatLot, i);
                }
            }

            throw new InvalidOperationException("Impossible de générer un nouvel ID de lot. Limite de 999 lots atteinte.");
        }

        /// <summary>
        /// Valide qu'un ID de lot respecte le format L001-L999
        /// </summary>
        public bool ValiderFormatLotId(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(lotId, @"^L\d{3}$");
        }

        private int? ExtraireNumeroLot(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId) || !lotId.StartsWith("L")) return null;
            if (int.TryParse(lotId.Substring(1), out int numero)) return numero;
            return null;
        }

        #endregion

        #region Génération IDs Blocs

        /// <summary>
        /// Génère le prochain ID de bloc disponible pour un lot donné ({LotId}_B001, {LotId}_B002, etc.)
        /// </summary>
        public string GenererProchainBlocId(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId))
                throw new ArgumentException("L'ID du lot ne peut pas être vide.", nameof(lotId));

            var blocsExistants = _blocService.ObtenirTousLesBlocs();
            var numerosUtilises = blocsExistants
                .Where(b => b.BlocId.StartsWith($"{lotId}_B"))
                .Select(b => ExtraireNumeroBloc(b.BlocId, lotId))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return string.Format(_config.FormatBloc, lotId, i);
                }
            }

            throw new InvalidOperationException($"Impossible de générer un nouvel ID de bloc pour le lot {lotId}. Limite de 999 blocs atteinte.");
        }

        /// <summary>
        /// Valide qu'un ID de bloc respecte le format {LotId}_B001-B999
        /// </summary>
        public bool ValiderFormatBlocId(string blocId)
        {
            if (string.IsNullOrWhiteSpace(blocId)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(blocId, @"^L\d{3}_B\d{3}$");
        }

        private int? ExtraireNumeroBloc(string blocId, string lotId)
        {
            var prefixe = $"{lotId}_B";
            if (string.IsNullOrWhiteSpace(blocId) || !blocId.StartsWith(prefixe)) return null;
            if (int.TryParse(blocId.Substring(prefixe.Length), out int numero)) return numero;
            return null;
        }

        #endregion

        #region Génération IDs Tâches

        /// <summary>
        /// Génère le prochain ID de tâche disponible pour un bloc donné
        /// CORRECTION : Utilise la logique de TacheService pour éviter les doublons
        /// </summary>
        public string GenererProchainTacheId(string blocId, TypeActivite type = TypeActivite.Tache)
        {
            if (string.IsNullOrWhiteSpace(blocId))
                throw new ArgumentException("L'ID du bloc ne peut pas être vide.", nameof(blocId));

            // CORRECTION : Créer une tâche temporaire et utiliser la logique existante de TacheService
            var tacheTemp = new Tache
            {
                Type = type,
                BlocId = blocId,
                LotId = ExtraireLotId(blocId),
                TacheNom = "temp"
                // TacheId sera généré automatiquement par TacheService.GenererIdUnique()
            };

            // Utiliser la logique existante de génération d'ID
            // Note: Cette approche nécessite d'accéder à la méthode privée ou de l'exposer
            // Pour l'instant, on fait une génération basique en attendant

            var tachesExistantes = _tacheService.ObtenirToutesLesTaches();
            var prefixe = type == TypeActivite.Tache ? "T" : "J";
            var patternId = $"{blocId}_{prefixe}";

            // Trouver le plus grand numéro existant
            var numerosUtilises = tachesExistantes
                .Where(t => t.TacheId.StartsWith(patternId))
                .Select(t => ExtraireNumero(t.TacheId, patternId))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            // Trouver le premier numéro disponible
            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return $"{patternId}{i:D3}";
                }
            }

            throw new InvalidOperationException($"Impossible de générer un nouvel ID de {(type == TypeActivite.Tache ? "tâche" : "jalon")} pour le bloc {blocId}. Limite de 999 éléments atteinte.");
        }

        /// <summary>
        /// Extrait le numéro d'une tâche depuis son ID
        /// </summary>
        private int? ExtraireNumero(string tacheId, string prefixe)
        {
            if (string.IsNullOrWhiteSpace(tacheId) || !tacheId.StartsWith(prefixe))
                return null;

            var numeroStr = tacheId.Substring(prefixe.Length);
            if (int.TryParse(numeroStr, out int numero))
                return numero;

            return null;
        }

        /// <summary>
        /// Valide qu'un ID de tâche respecte le format {BlocId}_T001-T999 ou {BlocId}_J001-J999
        /// </summary>
        public bool ValiderFormatTacheId(string tacheId)
        {
            if (string.IsNullOrWhiteSpace(tacheId)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(tacheId, @"^L\d{3}_B\d{3}_[TJ]\d{3}$");
        }

        private int? ExtraireNumeroTache(string tacheId, string blocId, string prefixe)
        {
            var prefixeComplet = $"{blocId}_{prefixe}";
            if (string.IsNullOrWhiteSpace(tacheId) || !tacheId.StartsWith(prefixeComplet)) return null;
            if (int.TryParse(tacheId.Substring(prefixeComplet.Length), out int numero)) return numero;
            return null;
        }

        #endregion

        #region Génération IDs Métiers

        /// <summary>
        /// Génère le prochain ID de métier disponible (M001, M002, etc.)
        /// </summary>
        public string GenererProchainMetierId(IReadOnlyList<Metier> existingMetiers)
        {
            var numerosUtilises = existingMetiers
                .Select(m => ExtraireNumeroMetier(m.MetierId))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!numerosUtilises.Contains(i))
                {
                    return $"M{i:D3}"; // Format M001, M002...
                }
            }
            throw new InvalidOperationException("Impossible de générer un nouvel ID de métier. Limite de 999 métiers atteinte.");
        }

        /// <summary>
        /// Valide qu'un ID de métier respecte le format M001-M999
        /// </summary>
        public bool ValiderFormatMetierId(string metierId)
        {
            if (string.IsNullOrWhiteSpace(metierId)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(metierId, @"^M\d{3}$");
        }

        private int? ExtraireNumeroMetier(string metierId)
        {
            if (string.IsNullOrWhiteSpace(metierId) || !metierId.StartsWith("M")) return null;
            if (int.TryParse(metierId.Substring(1), out int numero)) return numero;
            return null;
        }

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Extrait l'ID du lot depuis un ID de bloc ou de tâche
        /// </summary>
        public string ExtraireLotId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var match = System.Text.RegularExpressions.Regex.Match(id, @"^(L\d{3})");
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// Extrait l'ID du bloc depuis un ID de tâche
        /// </summary>
        public string ExtraireBlocId(string tacheId)
        {
            if (string.IsNullOrWhiteSpace(tacheId)) return null;

            var match = System.Text.RegularExpressions.Regex.Match(tacheId, @"^(L\d{3}_B\d{3})");
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// Normalise un ID CSV vers le format standard en utilisant le préfixe de bloc
        /// </summary>
        public string NormaliserIdDepuisCsv(string idOriginal, string blocIdCible, TypeActivite type = TypeActivite.Tache)
        {
            if (string.IsNullOrWhiteSpace(idOriginal))
            {
                return GenererProchainTacheId(blocIdCible, type);
            }

            // Si l'ID original respecte déjà notre format, le conserver
            if (ValiderFormatTacheId(idOriginal))
            {
                return idOriginal;
            }

            // Sinon, générer un nouvel ID basé sur le nom original
            var idSanitized = SanitizeIdString(idOriginal);
            var prefixe = type == TypeActivite.Tache ? "T" : "J";
            return $"{blocIdCible}_{prefixe}_{idSanitized}";
        }

        private string SanitizeIdString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "001";

            // Remplacer espaces et caractères spéciaux par des underscores
            var sanitized = System.Text.RegularExpressions.Regex.Replace(input, @"[^\w]", "_");

            // Limiter la longueur pour éviter des IDs trop longs
            if (sanitized.Length > 20)
                sanitized = sanitized.Substring(0, 20);

            return sanitized;
        }

        #endregion
    }
}