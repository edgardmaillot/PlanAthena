using PlanAthena.Services.Business;
using System.Globalization; // Pour la comparaison de chaînes
using System.Text;         // Pour la normalisation

namespace PlanAthena.Utilities
{
    public class ValueMappingService
    {
        private readonly UserPreferencesService _preferencesService;
        private readonly Dictionary<string, string> _dictionnaire;

        public ValueMappingService(UserPreferencesService preferencesService)
        {
            _preferencesService = preferencesService ?? throw new ArgumentNullException(nameof(preferencesService));
            _dictionnaire = _preferencesService.ChargerDictionnaire();
            // NOTE : Le dictionnaire est maintenant insensible à la casse ET aux accents grâce à la normalisation des clés.
        }

        /// <summary>
        /// Normalise une chaîne pour la comparaison : la met en minuscule et retire les accents.
        /// </summary>
        private string NormaliserCle(string cle)
        {
            if (string.IsNullOrWhiteSpace(cle)) return cle;

            var normalizedString = cle.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        public string TrouveCorrespondance(string valeur)
        {
            if (string.IsNullOrWhiteSpace(valeur)) return string.Empty;

            string cleNormalisee = NormaliserCle(valeur);

            // On doit maintenant itérer car les clés du dictionnaire ne sont pas normalisées
            foreach (var kvp in _dictionnaire)
            {
                if (NormaliserCle(kvp.Key) == cleNormalisee)
                {
                    return kvp.Value;
                }
            }

            return string.Empty;
        }

        public virtual void AjouteCorrespondance(string valeur, string traduction)
        {
            if (string.IsNullOrWhiteSpace(valeur) || string.IsNullOrWhiteSpace(traduction)) return;

            // On pourrait vouloir éviter d'ajouter des clés normalisées identiques
            string cleNormalisee = NormaliserCle(valeur);
            string cleExistante = null;
            foreach (var key in _dictionnaire.Keys)
            {
                if (NormaliserCle(key) == cleNormalisee)
                {
                    cleExistante = key;
                    break;
                }
            }

            // Si une clé équivalente existe (ex: "Manoeuvre" existe et on ajoute "Manœuvre"), on la remplace.
            if (cleExistante != null && cleExistante != valeur)
            {
                _dictionnaire.Remove(cleExistante);
            }

            _dictionnaire[valeur] = traduction;
            _preferencesService.SauverDictionnaire(_dictionnaire);
        }

        public virtual void SupprimeCorrespondance(string valeur)
        {
            if (string.IsNullOrWhiteSpace(valeur)) return;

            // Logique similaire à AjouteCorrespondance pour trouver la bonne clé à supprimer
            string cleNormalisee = NormaliserCle(valeur);
            string cleASupprimer = null;
            foreach (var key in _dictionnaire.Keys)
            {
                if (NormaliserCle(key) == cleNormalisee)
                {
                    cleASupprimer = key;
                    break;
                }
            }

            if (cleASupprimer != null && _dictionnaire.Remove(cleASupprimer))
            {
                _preferencesService.SauverDictionnaire(_dictionnaire);
            }
        }
    }
}