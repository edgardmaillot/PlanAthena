using PlanAthena.Data;
using PlanAthena.Services.Infrastructure;
using System.Text.Json;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Gère la persistance technique (lecture/écriture) des fichiers de projet JSON
    /// et les interactions utilisateur pour la sélection de fichiers (Open/Save dialogs).
    /// C'est la seule classe qui interagit directement avec le système de fichiers pour le projet.
    /// </summary>
    public class ProjetServiceDataAccess
    {
        private readonly CheminsPrefereService _cheminsService;
        private string _currentProjectPath;

        public ProjetServiceDataAccess(CheminsPrefereService cheminsService)
        {
            _cheminsService = cheminsService ?? throw new ArgumentNullException(nameof(cheminsService));
        }

        public virtual string GetCurrentProjectPath() => _currentProjectPath;
        public virtual bool IsProjectPathKnown() => !string.IsNullOrEmpty(_currentProjectPath);
        public virtual void ResetCurrentProjectPath() => _currentProjectPath = null;

        /// <summary>
        /// Charge et désérialise un projet depuis un fichier.
        /// Met à jour l'état interne (chemin actuel) en cas de succès.
        /// </summary>
        public virtual ProjetData Charger(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Le fichier de projet n'a pas été trouvé.", filePath);

            var jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var projetData = JsonSerializer.Deserialize<ProjetData>(jsonString, options);

            if (projetData == null)
                throw new InvalidOperationException("Le fichier de projet est invalide ou corrompu.");

            _currentProjectPath = filePath;
            _cheminsService.SauvegarderDernierDossier(TypeOperation.ProjetChargement, filePath);

            return projetData;
        }

        /// <summary>
        /// Sérialise et écrit un objet ProjetData dans un fichier.
        /// Met à jour l'état interne (chemin actuel) en cas de succès.
        /// </summary>
        public virtual void Sauvegarder(ProjetData projetData, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Le chemin du fichier ne peut pas être vide.", nameof(filePath));

            if (projetData == null)
                throw new ArgumentNullException(nameof(projetData));

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            string jsonString = JsonSerializer.Serialize(projetData, options);
            File.WriteAllText(filePath, jsonString);
            File.SetLastWriteTime(filePath, DateTime.Now);

            _currentProjectPath = filePath;
            _cheminsService.SauvegarderDernierDossier(TypeOperation.ProjetSauvegarde, filePath);
        }

        /// <summary>
        /// Affiche une boîte de dialogue "Ouvrir un fichier" pré-configurée pour les projets.
        /// </summary>
        /// <returns>Le chemin du fichier sélectionné, ou null si l'opération est annulée.</returns>
        public virtual string ShowOpenDialog()
        {
            using var ofd = new OpenFileDialog
            {
                InitialDirectory = _cheminsService.ObtenirDernierDossierProjets(),
                Filter = "Fichiers projet (*.json)|*.json|Tous les fichiers (*.*)|*.*",
                Title = "Ouvrir un projet"
            };

            return ofd.ShowDialog() == DialogResult.OK ? ofd.FileName : null;
        }

        /// <summary>
        /// Affiche une boîte de dialogue "Sauvegarder sous" pré-configurée.
        /// </summary>
        /// <param name="defaultFileName">Le nom de fichier suggéré à l'utilisateur.</param>
        /// <returns>Le chemin du fichier choisi, ou null si l'opération est annulée.</returns>
        public virtual string ShowSaveDialog(string defaultFileName)
        {
            using var sfd = new SaveFileDialog
            {
                InitialDirectory = _cheminsService.ObtenirDernierDossierProjets(),
                Filter = "Fichiers projet (*.json)|*.json",
                Title = "Sauvegarder le projet sous...",
                FileName = defaultFileName
            };

            return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
        }
        /// <summary>
        /// Sauvegarde un contenu textuel (XML, JSON simple, etc.) dans un fichier.
        /// Crée le répertoire si nécessaire.
        /// </summary>
        public virtual void SauvegarderFichierTexte(string content, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Le chemin du fichier ne peut pas être vide.", nameof(filePath));

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Charge un contenu textuel depuis un fichier.
        /// </summary>
        /// <returns>Le contenu du fichier, ou null si le fichier n'existe pas.</returns>
        public virtual string ChargerFichierTexte(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }
    }
}