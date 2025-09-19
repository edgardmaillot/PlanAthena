using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using MsaglColor = Microsoft.Msagl.Drawing.Color; // Alias pour éviter les conflits
using SystemColor = System.Drawing.Color;        // Alias pour la clarté

namespace PlanAthena.View.Ressources.MetierDiagram
{
    /// <summary>
    /// Centralise tous les paramètres de configuration visuelle et de layout
    /// pour le MetierDiagramControl.
    /// </summary>
    public class MetierDiagramSettings
    {
        #region Layout & Viewer

        public double DefaultZoom { get; set; } = 0.9; // Zoom un peu plus grand pour les métiers
        public SystemColor OutsideAreaColor { get; set; } = SystemColor.LightYellow; // Couleur de fond du diagramme
        public LayerDirection LayoutDirection { get; set; } = LayerDirection.TB;
        public double LayoutAspectRatio { get; set; } = 0.5; // Plus large que haut
        public double LayoutNodeSeparation { get; set; } = 40;
        public double LayoutLayerSeparation { get; set; } = 100;
        public double LayoutMinNodeHeight { get; set; } = 60;
        public double LayoutMinNodeWidth { get; set; } = 80;
        public double LayoutMargin { get; set; } = 30;

        // Paramètres spécifiques à l'algorithme Sugiyama (mêmes que pour les tâches)
        public int SugiyamaRepetitionCoefficient { get; set; } = 3;
        public int SugiyamaBrandesThreshold { get; set; } = 1;
        public PackingMethod SugiyamaPackingMethod { get; set; } = PackingMethod.Columns;

        #endregion

        #region Noeuds - Métiers

        public Shape MetierShape { get; set; } = Shape.Box; // Forme de boîte par défaut
        public double MetierLineWidth { get; set; } = 2; // Épaisseur de bordure
        public double MetierPadding { get; set; } = 15; // Marge interne du nœud
        public double MetierFontSize { get; set; } = 10; // Taille de police
        public double MetierLabelMargin { get; set; } = 5; // Marge du label
        public MsaglColor MetierBorderColor { get; set; } = MsaglColor.DarkGray; // Couleur de bordure par défaut
        public MsaglColor MetierDefaultFillColor { get; set; } = MsaglColor.LightGray; // Couleur de remplissage par défaut (si aucune couleur hex ou fallback ProjetService)

        #endregion

        #region Labels & Texte

        public int MetierNomMaxLength { get; set; } = 25; // Longueur max du nom du métier
        // Nouveau format pour les labels des nœuds de métier
        // {0}: Nom du métier, {1}: ID du métier, {2}: Pictogramme, {3}: Phases
        public string MetierLabelFormat { get; set; } = "{0}\n {2}\nID: {1}\n {3}";
        //public string MetierFontName { get; set; } = "Material Symbols Outlined Medium";
        public string MetierFontName { get; set; } = "Segoe UI Symbol";

        #endregion

        #region Arêtes (Prérequis)

        public MsaglColor EdgeDefaultColor { get; set; } = MsaglColor.DarkSlateGray; // Couleur des flèches de prérequis
        public double EdgeDefaultWidth { get; set; } = 1.5; // Épaisseur des flèches
        public ArrowStyle EdgeArrowStyle { get; set; } = ArrowStyle.Normal; // Style de la pointe de flèche

        #endregion

        #region Highlighting (Mise en évidence)

        public double HighlightLineWidth { get; set; } = 4; // Épaisseur de la bordure du nœud sélectionné
        public MsaglColor HighlightBorderColor { get; set; } = MsaglColor.RoyalBlue; // Couleur de bordure du nœud sélectionné
        public double HighlightEdgeWidth { get; set; } = 2.5; // Épaisseur des arêtes adjacentes au nœud sélectionné
        public MsaglColor HighlightEdgeColor { get; set; } = MsaglColor.RoyalBlue; // Couleur des arêtes adjacentes

        #endregion
    }
}