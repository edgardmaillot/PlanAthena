using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DataAccess;
using PlanAthena.View.Utils;
using PlanAthena.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static PlanAthena.View.Utils.ImportMapOuvrierP2;

namespace PlanAthena.Services.Usecases
{
    public class ImportWizardOrchestrator
    {
        private readonly ImportService _importService;
        private readonly RessourceService _ressourceService;

        public ImportWizardOrchestrator(ImportService importService, RessourceService ressourceService)
        {
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
        }

        #region Import Ouvriers

        /// <summary>
        /// Orchestre le processus complet d'importation des ouvriers en guidant
        /// l'utilisateur à travers un assistant en plusieurs étapes.
        /// </summary>
        /// <param name="filePath">Chemin du fichier CSV à importer.</param>
        /// <returns>Un objet ImportResult indiquant le succès ou l'échec de l'opération.</returns>
        public ImportResult LancerWizardImportOuvriers(string filePath)
        {
            // --- ÉTAPE 0 : VÉRIFICATION DES PRÉ-REQUIS ---
            if (!_ressourceService.GetAllMetiers().Any())
            {
                MessageBox.Show("Aucun métier n'est défini dans l'application.\n\nVeuillez en créer au moins un avant d'importer des ouvriers.",
                                "Pré-requis manquant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return ImportResult.Echec("Aucun métier n'est défini dans le système.");
            }

            // --- ÉTAPE 1 : DÉCISION DE L'UTILISATEUR (REMPLACER OU AJOUTER) ---
            var confirmResult = MessageBox.Show(
                "Comment voulez-vous importer les données de ce fichier ?\n\n" +
                "- 'Oui' : Efface TOUS les ouvriers actuels avant d'importer.\n" +
                "- 'Non' : Ajoute les nouveaux ouvriers et met à jour ceux existants.\n" +
                "- 'Annuler' : Ne fait rien et ferme cette fenêtre.",
                "Mode d'importation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Cancel)
            {
                return ImportResult.Echec("Importation annulée par l'utilisateur.");
            }
            bool remplacerExistants = (confirmResult == DialogResult.Yes);

            // --- PRÉPARATION DE L'ÉTAT PARTAGÉ POUR LE WIZARD ---
            var etatImport = new ImportOuvrierState
            {
                FilePath = filePath,
                RemplacerExistants = remplacerExistants
            };

            Form wizardHost = null;

            try
            {
                // --- ÉTAPE 2 : MAPPING DES COLONNES (UI ÉTAPE 1) ---
                wizardHost = CreerWizardHostForm("Étape 1/2 : Correspondance des colonnes");
                var step1View = new ImportMapOuvrierP1(filePath);
                step1View.Dock = DockStyle.Fill;
                wizardHost.Controls.Add(step1View);

                bool etape1Complete = false;
                step1View.SuivantClicked += (s, e) => { etape1Complete = true; wizardHost.Close(); };
                step1View.AnnulerClicked += (s, e) => wizardHost.Close();
                wizardHost.ShowDialog();

                if (!etape1Complete)
                {
                    return ImportResult.Echec("Importation annulée par l'utilisateur à l'étape 1.");
                }

                // On récupère les mappings d'index et l'état du header depuis la vue P1
                etatImport.ColumnIndexMappings = step1View.Mappings;
                etatImport.HasHeaderRecord = step1View.HasHeader;

                // Validation cruciale : l'utilisateur a-t-il bien mappé le champ métier ?
                if (!etatImport.ColumnIndexMappings.ContainsKey("Metier"))
                {
                    MessageBox.Show("Le champ 'Métier' est obligatoire et n'a pas été mappé à une colonne. L'importation ne peut pas continuer.",
                                   "Mapping incomplet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return ImportResult.Echec("Champ 'Métier' non mappé.");
                }

                // --- ÉTAPE 3 : MAPPING DES VALEURS (UI ÉTAPE 2) ---
                wizardHost.Dispose(); // On libère la première fenêtre
                wizardHost = CreerWizardHostForm("Étape 2/2 : Correspondance des métiers");

                var step2View = new ImportMapOuvrierP2(
                    etatImport.FilePath,
                    etatImport.ColumnIndexMappings["Metier"], // On passe l'INDEX de la colonne métier
                    _ressourceService,
                    etatImport.HasHeaderRecord
                );
                step2View.Dock = DockStyle.Fill;
                wizardHost.Controls.Add(step2View);

                bool etape2Complete = false;
                step2View.TerminerClicked += (s, e) => { etape2Complete = true; wizardHost.Close(); };
                step2View.RetourClicked += (s, e) => wizardHost.Close();
                wizardHost.ShowDialog();

                if (!etape2Complete)
                {
                    return ImportResult.Echec("Importation annulée par l'utilisateur à l'étape 2.");
                }

                // On récupère les mappings de valeurs (ex: "Chef de chantier" -> Métier M001)
                etatImport.ValueMappings = step2View.ValueMappings;

                // --- ÉTAPE 4 : TRANSFORMATION FINALE PAR L'ORCHESTRATEUR ---
                var (ouvriersPrets, warnings) = TransformerDonneesBrutesEnOuvriers(etatImport);

                // --- ÉTAPE FINALE : APPEL AU SERVICE DE CHARGEMENT "BÊTE" ---
                return _importService.ImporterOuvriers(ouvriersPrets, etatImport.RemplacerExistants);
            }
            catch (Exception ex)
            {
                // Catch-all pour les erreurs imprévues
                return ImportResult.Echec($"Une erreur critique est survenue dans l'orchestrateur : {ex.Message}");
            }
            finally
            {
                // S'assurer que la fenêtre est toujours fermée et libérée
                wizardHost?.Dispose();
            }
        }
        /// <summary>
        /// Coeur de l'ETL : prend les données brutes et les règles de mapping,
        /// et retourne une liste propre d'objets Ouvrier.
        /// </summary>
        /// <summary>
        /// Coeur de la logique de Transformation (le 'T' de ETL).
        /// Prend l'état complet du wizard (données brutes + règles de mapping)
        /// et le transforme en une liste d'objets de domaine propres.
        /// </summary>
        private (List<Ouvrier> Ouvriers, List<string> Warnings) TransformerDonneesBrutesEnOuvriers(ImportOuvrierState etat)
        {
            var warnings = new List<string>();
            var ouvriersEnConstruction = new Dictionary<string, Ouvrier>();

            // 1. Lire toutes les lignes du fichier. Simple et direct.
            var lines = File.ReadAllLines(etat.FilePath, Encoding.UTF8);
            string delimiter = _importService.DetectCsvDelimiter(etat.FilePath);

            // 2. Déterminer les lignes de données à traiter
            var linesToProcess = etat.HasHeaderRecord ? lines.Skip(1) : lines;

            // 3. Boucler sur chaque ligne pour construire les objets Ouvrier
            foreach (var line in linesToProcess)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] columns = line.Split(delimiter[0]);

                // Fonction utilitaire interne pour piocher une valeur de manière sécurisée en utilisant le mapping d'index
                string GetValue(string logicalKey)
                {
                    if (etat.ColumnIndexMappings.TryGetValue(logicalKey, out int index) && index < columns.Length)
                    {
                        return columns[index].Trim();
                    }
                    return null; // Retourne null si le champ n'a pas été mappé ou si la colonne n'existe pas
                }

                // 4. Extraire les données en utilisant les instructions de l'utilisateur (le mapping d'index)
                string nom = GetValue("Nom");
                string prenom = GetValue("Prenom");

                // Si les champs clés sont vides, on ignore la ligne
                if (string.IsNullOrWhiteSpace(nom) && string.IsNullOrWhiteSpace(prenom)) continue;

                // 5. Agréger les données. Un ouvrier peut apparaître sur plusieurs lignes (une par compétence).
                string cleOuvrier = $"{nom}|{prenom}".ToUpperInvariant();
                Ouvrier ouvrierCourant;

                // Si c'est la première fois qu'on voit cet ouvrier, on crée son objet de base
                if (!ouvriersEnConstruction.TryGetValue(cleOuvrier, out ouvrierCourant))
                {
                    int.TryParse(GetValue("TauxJour") ?? "0", out int cout);
                    ouvrierCourant = new Ouvrier { Nom = nom, Prenom = prenom, CoutJournalier = cout };
                    ouvriersEnConstruction[cleOuvrier] = ouvrierCourant;
                }

                // 6. Ajouter la compétence de la ligne actuelle à l'ouvrier en cours de construction
                string metierLibelleCsv = GetValue("Metier");
                if (!string.IsNullOrEmpty(metierLibelleCsv))
                {
                    // On utilise le mapping de valeurs de l'étape 2 pour traduire le libellé en ID de métier
                    if (etat.ValueMappings.TryGetValue(metierLibelleCsv, out var mappingItem)
                        && mappingItem.Action == MappingAction.MapToExisting)
                    {
                        string metierId = mappingItem.Metier.MetierId;
                        // On s'assure de ne pas ajouter de compétence en double
                        if (!ouvrierCourant.Competences.Any(c => c.MetierId == metierId))
                        {
                            ouvrierCourant.Competences.Add(new CompetenceOuvrier { MetierId = metierId });
                        }
                    }
                }
            }

            // 7. Finaliser les objets (ex: définir le métier principal) avant de les retourner
            foreach (var ouvrier in ouvriersEnConstruction.Values)
            {
                if (ouvrier.Competences.Any() && !ouvrier.Competences.Any(c => c.EstMetierPrincipal))
                {
                    ouvrier.Competences.First().EstMetierPrincipal = true;
                }
            }

            return (ouvriersEnConstruction.Values.ToList(), warnings);
        }




        /// <summary>
        /// Crée et configure un Form pour héberger un UserControl du wizard.
        /// </summary>
        private Form CreerWizardHostForm(string title)
        {
            var form = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterScreen,
                Size = new System.Drawing.Size(1000, 600), // Taille de départ raisonnable
                MinimumSize = new System.Drawing.Size(980, 500),
                FormBorderStyle = FormBorderStyle.Sizable, // Permettre le redimensionnement
                MaximizeBox = true,
                MinimizeBox = true
            };
            //form.Shown += (s, e) => form.WindowState = FormWindowState.Maximized; // Maximiser à l'affichage
            return form;
        }

        /// <summary>
        /// DTO interne pour passer les informations entre les étapes du wizard.
        /// </summary>
        public class ImportOuvrierState
        {
            public string FilePath { get; set; }
            public bool RemplacerExistants { get; set; }
            public bool HasHeaderRecord { get; set; }
            public Dictionary<string, int> ColumnIndexMappings { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, ImportMapOuvrierP2.MetierMappingItem> ValueMappings { get; set; } = new Dictionary<string, ImportMapOuvrierP2.MetierMappingItem>();
        }

        #endregion
        #region Logique de la Voie Rapide

        /// <summary>
        /// Tente de reconnaître et d'importer directement un fichier CSV au format natif de PlanAthena.
        /// </summary>
        /// <returns>Un objet ImportResult si l'import direct réussit, sinon null.</returns>
        private ImportResult TenterImportDirect(string filePath, bool remplacerExistants)
        {
            try
            {
                // --- Étape 1 : Définir la "signature" d'un fichier natif ---
                // On utilise un HashSet pour une comparaison rapide et insensible à la casse.
                var requiredHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OuvrierId", "Nom", "Prenom", "CoutJournalier", "MetierId"
        };

                // --- Étape 2 : Lire uniquement la première ligne (l'en-tête) ---
                string headerLine = File.ReadLines(filePath, Encoding.UTF8).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    return null; // Fichier vide, on laisse le wizard gérer.
                }

                // --- Étape 3 : Comparer les en-têtes ---
                var delimiter = headerLine.Contains(';') ? ';' : ','; // Simple détection
                var csvHeaders = headerLine.Split(delimiter).Select(h => h.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                // IsSubsetOf vérifie si tous les en-têtes requis sont présents dans le fichier.
                // C'est flexible : le fichier peut avoir des colonnes en plus sans que ça échoue.
                if (requiredHeaders.IsSubsetOf(csvHeaders))
                {
                    // C'est un fichier natif ! On lance la voie rapide.
                    return _importService.ImporterOuvriersFormatNatif(filePath, remplacerExistants);
                }

                // --- Étape 4 : Les en-têtes ne correspondent pas ---
                return null; // On signale à l'orchestrateur de continuer avec le wizard.
            }
            catch (Exception)
            {
                // Si une erreur se produit (fichier verrouillé, etc.), on ne bloque pas l'utilisateur.
                // On se rabat sur le wizard qui affichera une erreur plus claire si nécessaire.
                return null;
            }
        }

        #endregion
    }
}