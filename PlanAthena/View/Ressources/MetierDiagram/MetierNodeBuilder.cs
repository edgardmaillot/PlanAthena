using Microsoft.Msagl.Drawing;
using PlanAthena.Data; // Pour Metier
using MsaglColor = Microsoft.Msagl.Drawing.Color;

namespace PlanAthena.View.Ressources.MetierDiagram
{
    /// <summary>
    /// Responsable de la construction et du style des nœuds MSAGL (DrawingNode)
    /// pour le diagramme des métiers.
    /// </summary>
    public class MetierNodeBuilder
    {
        private readonly MetierDiagramSettings _settings;
        // MetierNodeBuilder n'a PAS besoin de ProjetService ici car la couleur est gérée par le service principal
        // et le Metier est passé directement.

        public MetierNodeBuilder(MetierDiagramSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Construit un nœud MSAGL à partir d'un objet Metier.
        /// </summary>
        /// <param name="metier">L'objet Metier à représenter.</param>
        /// <returns>Le nœud MSAGL créé.</returns>
        public Node BuildNodeFromMetier(Metier metier)
        {
            // L'ID du nœud dans le graphe MSAGL doit être unique et correspondre au MetierId
            var node = new Node(metier.MetierId);
            node.Label.FontName = _settings.MetierFontName;
            node.LabelText = GetNodeLabel(metier);
            ApplyNodeStyle(node, metier);

            node.UserData = metier; // Stocke l'objet Metier complet pour récupération lors de la sélection
            return node;
        }

        /// <summary>
        /// Construit le label texte pour un nœud de métier.
        /// </summary>
        private string GetNodeLabel(Metier metier)
        {
            // Le format des labels pour les métiers est défini dans MetierDiagramSettings.
            // Il inclut le nom, l'ID, le pictogramme et les phases.
            string pictogrammeAffiche = string.IsNullOrEmpty(metier.Pictogram) ? "" : $"({metier.Pictogram})";
            string phasesAffichees = metier.Phases != ChantierPhase.None ? $"[{string.Join("/", metier.Phases.ToString().Split(new[] { ", " }, StringSplitOptions.None))}]" : "";

            return string.Format(_settings.MetierLabelFormat,
                TronquerTexte(metier.Nom, _settings.MetierNomMaxLength), // {0}: Nom du métier
                metier.MetierId,                                         // {1}: ID du métier
                pictogrammeAffiche.Trim(),                               // {2}: Pictogramme (si présent)
                phasesAffichees.Trim());                                 // {3}: Phases (si présentes)
        }

        /// <summary>
        /// Applique le style visuel à un nœud de métier.
        /// </summary>
        /// <param name="node">Le nœud MSAGL.</param>
        /// <param name="metier">L'objet Metier associé.</param>
        public void ApplyNodeStyle(Node node, Metier metier)
        {
            // La couleur de remplissage sera déterminée par ProjetService via GetDisplayColorForMetier
            // (ProjetService sera injecté au MetierDiagramControl qui appellera GetDisplayColorForMetier lors du chargement des données)
            // Pour l'instant, on utilise une couleur par défaut ou la couleur hex du métier si elle est valide.

            // Pour le style du nœud (forme, bordure, etc.)
            node.Attr.Shape = _settings.MetierShape;
            node.Attr.LineWidth = _settings.MetierLineWidth;
            node.Attr.Padding = _settings.MetierPadding;
            node.Label.FontSize = _settings.MetierFontSize;
            node.Attr.LabelMargin = (int)_settings.MetierLabelMargin;
            node.Attr.Color = _settings.MetierBorderColor; // Couleur de la bordure

            // La couleur de remplissage dépendra directement de la couleur hex du métier ou du fallback de ProjetService
            // Cette couleur sera appliquée dans MetierDiagramControl.GenererDiagramme()
            // ou MetierDiagramControl.HighlightNode() après avoir obtenu la couleur via ProjetService.
            if (!string.IsNullOrEmpty(metier.CouleurHex))
            {
                try
                {
                    var systemColor = ColorTranslator.FromHtml(metier.CouleurHex);
                    node.Attr.FillColor = new MsaglColor(systemColor.R, systemColor.G, systemColor.B);
                }
                catch
                {
                    node.Attr.FillColor = _settings.MetierDefaultFillColor; // Couleur de repli si hex invalide
                }
            }
            else
            {
                node.Attr.FillColor = _settings.MetierDefaultFillColor; // Couleur de repli si pas de couleur spécifiée
            }
        }

        private string TronquerTexte(string texte, int longueurMax)
        {
            if (string.IsNullOrEmpty(texte) || texte.Length <= longueurMax)
                return texte;
            return texte.Substring(0, longueurMax - 3) + "...";
        }
    }
}