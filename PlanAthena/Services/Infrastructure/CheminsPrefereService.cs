// Fichier: Services/Infrastructure/CheminsPrefereService.cs

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace PlanAthena.Services.Infrastructure
{
    /// <summary>
    /// Service centralisé pour la gestion des chemins préférés utilisateur
    /// Améliore l'UX en mémorisant les derniers dossiers utilisés
    /// </summary>
    public class CheminsPrefereService
    {
        private const string DOSSIER_PLANATHENA = "PlanAthena";

        /// <summary>
        /// Obtient le dernier dossier utilisé pour un type d'opération
        /// Retourne un smart default si aucun chemin sauvegardé
        /// </summary>
        public string ObtenirDernierDossier(TypeOperation operation)
        {
            var cheminSauvegarde = operation switch
            {
                TypeOperation.ImportCsv => Properties.Settings.Default.DernierDossierImportCsv,
                TypeOperation.ImportExcel => Properties.Settings.Default.DernierDossierImportExcel,
                TypeOperation.ExportGantt => Properties.Settings.Default.DernierDossierExports,
                TypeOperation.ExportExcel => Properties.Settings.Default.DernierDossierExports,
                TypeOperation.ProjetSauvegarde => Properties.Settings.Default.DernierDossierProjets,
                TypeOperation.ProjetChargement => Properties.Settings.Default.DernierDossierProjets,
                _ => ""
            };

            // Vérifier que le chemin existe encore
            if (!string.IsNullOrEmpty(cheminSauvegarde) && Directory.Exists(cheminSauvegarde))
            {
                return cheminSauvegarde;
            }

            // Retourner smart default
            return ObtenirSmartDefault(operation);
        }

        /// <summary>
        /// Sauvegarde le dernier dossier utilisé après une opération réussie
        /// </summary>
        public void SauvegarderDernierDossier(TypeOperation operation, string cheminComplet)
        {
            if (string.IsNullOrEmpty(cheminComplet))
                return;

            // Extraire le dossier du chemin complet
            var dossier = Path.GetDirectoryName(cheminComplet);
            if (string.IsNullOrEmpty(dossier) || !Directory.Exists(dossier))
                return;

            // Sauvegarder selon le type d'opération
            switch (operation)
            {
                case TypeOperation.ImportCsv:
                    Properties.Settings.Default.DernierDossierImportCsv = dossier;
                    break;
                case TypeOperation.ImportExcel:
                    Properties.Settings.Default.DernierDossierImportExcel = dossier;
                    break;
                case TypeOperation.ExportGantt:
                case TypeOperation.ExportExcel:
                    Properties.Settings.Default.DernierDossierExports = dossier;
                    break;
                case TypeOperation.ProjetSauvegarde:
                case TypeOperation.ProjetChargement:
                    Properties.Settings.Default.DernierDossierProjets = dossier;
                    break;
            }

            // Sauvegarder dans les fichiers récents si c'est un projet
            if (operation == TypeOperation.ProjetChargement)
            {
                AjouterFichierRecent(cheminComplet);
            }

            // Persister les changements
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Méthodes spécialisées pour plus de clarté dans le code
        /// </summary>
        public string ObtenirDernierDossierImport() => ObtenirDernierDossier(TypeOperation.ImportCsv);
        public string ObtenirDernierDossierExport() => ObtenirDernierDossier(TypeOperation.ExportExcel);
        public string ObtenirDernierDossierProjets() => ObtenirDernierDossier(TypeOperation.ProjetChargement);

        /// <summary>
        /// Bonus UX : Obtient la liste des fichiers récents
        /// </summary>
        public List<string> ObtenirFichiersRecents(TypeOperation operation, int maxCount = 5)
        {
            if (operation != TypeOperation.ProjetChargement)
                return new List<string>();

            var fichiersRecents = Properties.Settings.Default.FichiersProjetRecents;
            if (fichiersRecents == null)
                return new List<string>();

            return fichiersRecents.Cast<string>()
                .Where(f => File.Exists(f))
                .Take(maxCount)
                .ToList();
        }

        /// <summary>
        /// Génère des chemins par défaut intelligents
        /// </summary>
        private string ObtenirSmartDefault(TypeOperation operation)
        {
            var dossierBase = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                DOSSIER_PLANATHENA
            );

            // Créer le dossier de base s'il n'existe pas
            Directory.CreateDirectory(dossierBase);

            var sousDossier = operation switch
            {
                TypeOperation.ImportCsv => "Imports",
                TypeOperation.ImportExcel => "Imports",
                TypeOperation.ExportGantt => "Exports",
                TypeOperation.ExportExcel => "Exports",
                _ => ""
            };

            if (string.IsNullOrEmpty(sousDossier))
                return dossierBase;

            var cheminComplet = Path.Combine(dossierBase, sousDossier);
            Directory.CreateDirectory(cheminComplet);
            return cheminComplet;
        }

        /// <summary>
        /// Ajoute un fichier à la liste des récents
        /// </summary>
        private void AjouterFichierRecent(string cheminFichier)
        {
            if (Properties.Settings.Default.FichiersProjetRecents == null)
            {
                Properties.Settings.Default.FichiersProjetRecents = new StringCollection();
            }

            var fichiersRecents = Properties.Settings.Default.FichiersProjetRecents;

            // Retirer si déjà présent (pour le remettre en tête)
            if (fichiersRecents.Contains(cheminFichier))
            {
                fichiersRecents.Remove(cheminFichier);
            }

            // Ajouter en tête
            fichiersRecents.Insert(0, cheminFichier);

            // Limiter à 10 fichiers récents
            while (fichiersRecents.Count > 10)
            {
                fichiersRecents.RemoveAt(fichiersRecents.Count - 1);
            }
        }
    }

    /// <summary>
    /// Types d'opérations supportées pour la gestion des chemins
    /// </summary>
    public enum TypeOperation
    {
        ImportCsv,
        ImportExcel,
        ExportGantt,
        ExportExcel,
        ProjetSauvegarde,
        ProjetChargement
    }
}