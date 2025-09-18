// Fichier : SplashScreen.cs (Version corrigée et finale)
using System.Drawing;
using System.Windows.Forms;

namespace PlanAthena.View.Utils
{
    public partial class SplashScreen : UserControl
    {
        public SplashScreen()
        {
            InitializeComponent();
            // SUPPRIMEZ les appels à DisplayDimensions d'ici.
        }

        // Cette méthode est appelée de l'extérieur (par CockpitView)
        public void DisplayDimensions(Size size)
        {
            // Si lblDimensions existe, on l'utilise.
            if (lblDimensions != null)
            {
                lblDimensions.Text = $"Width = {size.Width}\nHeight = {size.Height}";
            }
        }
    }
}