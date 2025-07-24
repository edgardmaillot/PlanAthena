using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using MsaglColor = Microsoft.Msagl.Drawing.Color; // Alias pour éviter les conflits
using SystemColor = System.Drawing.Color;        // Alias pour la clarté

namespace PlanAthena.Controls.Config
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
        public double LayoutMinNodeWidth { get; set; } = 50;
        public double LayoutMargin { get; set; } = 20;

        // Paramètres spécifiques à l'algorithme Sugiyama
        public int SugiyamaRepetitionCoefficient { get; set; } = 3;
        public int SugiyamaBrandesThreshold { get; set; } = 1;
        public PackingMethod SugiyamaPackingMethod { get; set; } = PackingMethod.Columns;

        #endregion

        #region Clusters (Blocs)

        public MsaglColor ClusterFillColor { get; set; } = MsaglColor.LightYellow;
        public MsaglColor ClusterBorderColor { get; set; } = MsaglColor.DarkOrange;
        public double ClusterLineWidth { get; set; } = 2;
        public double ClusterFontSize { get; set; } = 12;
        public MsaglColor ClusterFontColor { get; set; } = MsaglColor.DarkBlue;
        public double ClusterPadding { get; set; } = 20;
        public string ClusterLabelFormat { get; set; } = "{0} - ({1} tâches - {2}h)";

        #endregion

        #region Noeuds - Tâches

        public Shape TacheShape { get; set; } = Shape.Box;
        public double TacheLineWidth { get; set; } = 2;
        public double TachePadding { get; set; } = 18;
        public double TacheFontSize { get; set; } = 9;
        public double TacheLabelMargin { get; set; } = 12;
        public MsaglColor TacheDefaultBorderColor { get; set; } = MsaglColor.DarkBlue;
        public MsaglColor TacheSansMetierBorderColor { get; set; } = MsaglColor.Red;
        public MsaglColor TacheSansMetierFillColor { get; set; } = MsaglColor.MistyRose;

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

        #region Labels & Texte

        public string JalonAvecDureeIcon { get; set; } = "⏳";
        public string JalonSansDureeIcon { get; set; } = "◆";
        public string UnassignedMetierLabel { get; set; } = "❌ Non assigné";
        public int TacheNomMaxLength { get; set; } = 20;
        public int MetierNomMaxLength { get; set; } = 18;
        public string TacheLabelFormat { get; set; } = "🏷️ {0}\n{1}\n👨‍💼 {2}\n⏱️ {3}h";
        public string TacheLabelFormatAvecDeps { get; set; } = "🏷️ {0}\n{1}\n👨‍💼 {2}\n⏱️ {3}h\n🔗 {4} dép.";

        #endregion

        #region Arêtes (Dépendances)

        public MsaglColor EdgeDefaultColor { get; set; } = MsaglColor.DarkGreen;
        public double EdgeDefaultWidth { get; set; } = 2;
        public ArrowStyle EdgeArrowStyle { get; set; } = ArrowStyle.Normal;

        #endregion

        #region Highlighting (Mise en évidence)

        public double HighlightLineWidth { get; set; } = 5;
        public MsaglColor HighlightBorderColor { get; set; } = MsaglColor.Purple;
        public double HighlightEdgeWidth { get; set; } = 3;
        public MsaglColor HighlightEdgeColor { get; set; } = MsaglColor.Purple;

        #endregion

        #region Couleurs

        /// <summary>
        /// Liste de couleurs de repli pour les métiers qui n'ont pas de couleur personnalisée.
        /// </summary>
        public MsaglColor[] MetierFallbackColors { get; set; } = {
            MsaglColor.LightBlue,
            MsaglColor.LightGreen,
            MsaglColor.LightYellow,
            MsaglColor.LightPink,
            MsaglColor.LightGray,
            MsaglColor.LightCyan,
            MsaglColor.LightSalmon,
            MsaglColor.LightCoral,
            MsaglColor.LightSteelBlue
        };

        #endregion
    }
}