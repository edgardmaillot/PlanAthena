// PertNodeBuilder.cs

using ChoETL;
using Microsoft.Msagl.Drawing;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using MsaglColor = Microsoft.Msagl.Drawing.Color;

namespace PlanAthena.View.TaskManager.PertDiagram
{
    public class PertNodeBuilder
    {
        private readonly PertDiagramSettings _settings;
        private readonly RessourceService _ressourceService;

        public PertNodeBuilder(PertDiagramSettings settings, RessourceService ressourceService)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
        }

        public Node BuildNodeFromTache(Tache tache, Graph graph)
        {
            var node = graph.AddNode(tache.TacheId);
            node.LabelText = GetNodeLabel(tache);
            ApplyNodeStyle(node, tache);
            node.UserData = tache;
            return node;
        }

        private string GetNodeLabel(Tache tache)
        {
            if (tache.EstJalon)
            {
                return tache.HeuresHommeEstimees > 0 ? $"{_settings.JalonAvecDureeIcon}{tache.HeuresHommeEstimees}" : _settings.JalonSansDureeIcon;
            }

            var metier = _ressourceService.GetMetierById(tache.MetierId);

            // Formatage des différentes parties du libellé
            string tacheIcon = GetTacheIcon(tache);
            string shortTacheId = GetShortTacheId(tache.TacheId);
            string metierIcon = metier?.Pictogram ?? "";
            string statutIcon = GetStatutIcon(tache);
            string formattedTacheNom = FormatTacheNom(tache.TacheNom, _settings.TacheNomLength);

            return string.Format(_settings.TacheLabelFormat,
                tacheIcon,               // {0}: IconeTache
                shortTacheId,            // {1}: TacheId Court
                metierIcon,              // {2}: IconeMetier
                tache.HeuresHommeEstimees, // {3}: Duree
                statutIcon,              // {4}: IconeStatut
                formattedTacheNom);      // {5}: Nom Tache formaté
        }

        #region Méthodes d'assistance pour les icônes et le formatage

        private string GetTacheIcon(Tache tache)
        {
            if (tache.Statut == Statut.Estimée)
            {
                return _settings.TacheIconEstimee;
            }
            return tache.EstConteneur ? _settings.TacheIconContainer : _settings.TacheIconFeuille;
        }

        private string GetStatutIcon(Tache tache)
        {
            // Le switch gère tous les cas, y compris EnRetard.
            switch (tache.Statut)
            {
                case Statut.Estimée: return _settings.StatutIconEstimee;
                case Statut.Planifiée: return _settings.StatutIconPlanifiee;
                case Statut.EnCours: return _settings.StatutIconEnCours;
                case Statut.Terminée: return _settings.StatutIconTerminee;
                case Statut.EnRetard: return _settings.StatutIconEnRetard;
                default: return _settings.StatutIconDefault;
            }
        }

        /// <summary>
        /// Extrait la dernière partie d'un ID (ex: "L001_B001_T010" -> "T010").
        /// </summary>
        private string GetShortTacheId(string fullId)
        {
            if (string.IsNullOrEmpty(fullId)) return "";

            int lastUnderscore = fullId.LastIndexOf('_');
            return lastUnderscore > -1 ? fullId.Substring(lastUnderscore + 1) : fullId;
        }

        /// <summary>
        /// Tronque ou complète une chaîne avec des espaces pour atteindre une longueur fixe.
        /// </summary>
        private string FormatTacheNom(string text, int fixedLength)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', fixedLength);
            }

            if (text.Length > fixedLength)
            {
                return text.Substring(0, fixedLength);
            }

            return text.PadRight(fixedLength, ' ');
        }

        #endregion

        public void ApplyNodeStyle(Node node, Tache tache)
        {
            node.Label.FontName = "Segoe UI Symbol";
            if (tache.EstJalon)
            {
                // Style des jalons (inchangé)
                node.Attr.Shape = _settings.JalonShape;
                node.Attr.LineWidth = _settings.JalonLineWidth;
                node.Attr.Padding = _settings.JalonPadding;
                node.Label.FontSize = _settings.JalonFontSize;
                node.Attr.LabelMargin = (int)_settings.JalonLabelMargin;
                node.Attr.Color = _settings.JalonBorderColor;
                node.Attr.FillColor = GetFillColor(tache); // Les jalons peuvent aussi avoir une couleur de fond
            }
            else // C'est une tâche standard
            {
                // --- 1. Appliquer les styles de base communs à toutes les tâches ---
                node.Attr.Shape = _settings.TacheShape;
                node.Attr.Padding = _settings.TachePadding;
                node.Label.FontSize = _settings.TacheFontSize;
                node.Attr.LabelMargin = (int)_settings.TacheLabelMargin;

                // --- 2. Appliquer les styles spécifiques basés sur le statut ---
                if (tache.Statut == Statut.Terminée)
                {
                    // Style "Terminé" : fond sombre, police claire, bordure par défaut
                    node.Attr.FillColor = _settings.TacheTermineeFillColor;
                    node.Label.FontColor = _settings.TacheTermineeFontColor;
                    node.Attr.Color = _settings.TacheDefaultBorderColor;
                    node.Attr.LineWidth = _settings.TacheLineWidth;
                }
                else if (tache.Statut == Statut.EnRetard)
                {
                    // Style "En Retard" : fond du métier, bordure rouge et épaisse
                    node.Attr.FillColor = GetFillColor(tache);
                    // La couleur de la police reste par défaut (noir)
                    node.Attr.Color = _settings.TacheEnRetardBorderColor;
                    node.Attr.AddStyle(Style.Dotted);
                    
                    node.Attr.LineWidth = _settings.TacheEnRetardLineWidth;
                }
                else // Pour tous les autres statuts (Estimée, Planifiée, EnCours)
                {
                    // Style standard : fond du métier, bordure par défaut
                    node.Attr.FillColor = GetFillColor(tache);
                    node.Attr.Color = string.IsNullOrEmpty(tache.MetierId) ? _settings.TacheSansMetierBorderColor : _settings.TacheDefaultBorderColor;
                    node.Attr.LineWidth = _settings.TacheLineWidth;
                }
            }
        }

        private MsaglColor GetFillColor(Tache tache)
        {
            // Cette méthode est maintenant simplifiée. Elle ne retourne que la couleur de fond
            // "normale", qui sera surchargée par ApplyNodeStyle si besoin.

            if (tache.EstJalon && (tache.Type == TypeActivite.JalonDeSynchronisation || tache.Type == TypeActivite.JalonTechnique))
            {
                return _settings.JalonTechniqueFillColor;
            }

            if (string.IsNullOrEmpty(tache.MetierId))
            {
                return _settings.TacheSansMetierFillColor;
            }

            var systemColor = _ressourceService.GetDisplayColorForMetier(tache.MetierId);
            return new MsaglColor(systemColor.R, systemColor.G, systemColor.B);
        }


    }
}