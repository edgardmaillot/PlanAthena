// PertDiagramSettings.cs

using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using MsaglColor = Microsoft.Msagl.Drawing.Color; // Alias pour éviter les conflits
using SystemColor = System.Drawing.Color;        // Alias pour la clarté

namespace PlanAthena.View.TaskManager.PertDiagram
{
    /// <summary>
    /// Centralise tous les paramètres de configuration visuelle et de layout
    /// pour le PertDiagramControl.
    /// </summary>
    public class PertDiagramSettings
    {
        #region Layout & Viewer

        public double DefaultZoom { get; set; } = 0.8;
        public SystemColor OutsideAreaColor { get; set; } = SystemColor.White;
        public LayerDirection LayoutDirection { get; set; } = LayerDirection.LR;
        public double LayoutAspectRatio { get; set; } = 0.3;
        public double LayoutNodeSeparation { get; set; } = 30;
        public double LayoutLayerSeparation { get; set; } = 90;
        public double LayoutMinNodeHeight { get; set; } = 50;
        public double LayoutMinNodeWidth { get; set; } = 220; // Augmenté pour le nouveau format
        public double? TacheFixedWidth { get; set; } = 220;
        public double LayoutMargin { get; set; } = 20;

        // Paramètres spécifiques à l'algorithme Sugiyama
        public int SugiyamaRepetitionCoefficient { get; set; } = 3;
        public int SugiyamaBrandesThreshold { get; set; } = 1;
        public PackingMethod SugiyamaPackingMethod { get; set; } = PackingMethod.Columns;

        #endregion

        #region Clusters (Blocs)
        public MsaglColor ClusterFillColor { get; set; } = MsaglColor.LightGray;
        public MsaglColor ClusterBorderColor { get; set; } = MsaglColor.DarkGray;
        public double ClusterLineWidth { get; set; } = 2;
        public double ClusterFontSize { get; set; } = 12;
        public MsaglColor ClusterFontColor { get; set; } = MsaglColor.DarkBlue;
        public double ClusterPadding { get; set; } = 20;
        public string ClusterLabelIcon { get; set; } = "👷";
        public string ClusterLabelFormat { get; set; } = "{0} | {1}- ({2} tâches - {3}h)";
        #endregion

        #region Noeuds - Tâches

        public Shape TacheShape { get; set; } = Shape.Box;
        public double TacheLineWidth { get; set; } = 2;
        public double TachePadding { get; set; } = 10;
        public double TacheFontSize { get; set; } = 12;
        public double TacheLabelMargin { get; set; } = 12;
        public MsaglColor TacheDefaultBorderColor { get; set; } = MsaglColor.DarkBlue;
        public MsaglColor TacheSansMetierBorderColor { get; set; } = MsaglColor.Red;
        public MsaglColor TacheSansMetierFillColor { get; set; } = MsaglColor.MistyRose;

        #endregion

        #region Styles des statuts 

        // --- Style pour les tâches terminées
        public MsaglColor TacheTermineeFillColor { get; set; } = new MsaglColor(44, 44, 44);
        public MsaglColor TacheTermineeFontColor { get; set; } = new MsaglColor(200, 200, 200);

        // --- Style pour les tâches en retard
        public MsaglColor TacheEnRetardBorderColor { get; set; } = MsaglColor.Red;
        public double TacheEnRetardLineWidth { get; set; } = 4;

        #endregion

        #region Noeuds - Jalons
        public Shape JalonShape { get; set; } = Shape.Diamond;
        public double JalonLineWidth { get; set; } = 3;
        public double JalonPadding { get; set; } = 3;
        public double JalonFontSize { get; set; } = 14;
        public double JalonLabelMargin { get; set; } = 2;
        public MsaglColor JalonBorderColor { get; set; } = MsaglColor.Orange;
        public MsaglColor JalonTechniqueFillColor { get; set; } = MsaglColor.LightGoldenrodYellow;
        #endregion

        #region Labels, Texte & Icônes (SECTION MISE À JOUR)

        // --- Icônes pour les Jalons
        public string JalonAvecDureeIcon { get; set; } = "⏳";
        public string JalonSansDureeIcon { get; set; } = "⚙️";

        // --- Icônes pour les Tâches (selon le type/statut)
        public string TacheIconEstimee { get; set; } = "";       // U+E088
        public string TacheIconContainer { get; set; } = "";   // U+E0CC
        public string TacheIconFeuille { get; set; } = "";       // U+E0CF

        // --- Icônes pour les Statuts de tâche
        public string StatutIconEstimee { get; set; } = "";       // U+E1ED
        public string StatutIconPlanifiee { get; set; } = "";     // U+E163
        public string StatutIconEnCours { get; set; } = "";       // U+E117
        public string StatutIconTerminee { get; set; } = "";      // U+E1EF
        public string StatutIconEnRetard { get; set; } = "";       // U+E290
        public string StatutIconDefault { get; set; } = "?";       // Au cas où

        // --- Libellés et formats
        public string UnassignedMetierLabel { get; set; } = "Non assigné";
        public int TacheNomLength { get; set; } = 20; // Nouvelle longueur fixe pour le nom de la tâche

        // --- FORMAT DE LIBELLÉ 
        // {0}: IconeTache, {1}: TacheId Court, {2}: IconeMetier, {3}: Duree, {4}: IconeStatut, {5}: Nom Tache formaté
        public string TacheLabelFormat { get; set; } = "{0} {1} {2}  -    {3}h {4}\n────────────────────\n{5}";

        #endregion

        #region Arêtes, Highlighting & Couleurs

        public MsaglColor EdgeDefaultColor { get; set; } = MsaglColor.DarkGreen;
        public double EdgeDefaultWidth { get; set; } = 2;
        public ArrowStyle EdgeArrowStyle { get; set; } = ArrowStyle.Normal;
        public double HighlightLineWidth { get; set; } = 5;
        public MsaglColor HighlightBorderColor { get; set; } = MsaglColor.Purple;
        public double HighlightEdgeWidth { get; set; } = 3;
        public MsaglColor HighlightEdgeColor { get; set; } = MsaglColor.Purple;
        public MsaglColor[] MetierFallbackColors { get; set; } = {
            MsaglColor.LightBlue, MsaglColor.LightGreen, MsaglColor.LightYellow, MsaglColor.LightPink,
            MsaglColor.LightGray, MsaglColor.LightCyan, MsaglColor.LightSalmon, MsaglColor.LightCoral,
            MsaglColor.LightSteelBlue
        };
        #endregion
    }
}