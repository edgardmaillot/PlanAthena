using System.Text;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Formulaire simple pour afficher une liste détaillée d'avertissements (warnings)
    /// suite à une opération d'import ou autre.
    /// </summary>
    public partial class ImportWarningsView : Form
    {
        public ImportWarningsView(List<string> warnings)
        {
            InitializeComponent();
            this.Text = "Avertissements d'Import";
            Populate_Warnings(warnings);
        }

        private void Populate_Warnings(List<string> warnings)
        {
            if (warnings == null || !warnings.Any())
            {
                txtWarnings.Text = "Aucun avertissement.";
                return;
            }

            var sb = new StringBuilder();
            foreach (var warning in warnings)
            {
                sb.AppendLine(warning);
            }
            txtWarnings.Text = sb.ToString();
            txtWarnings.Select(0, 0); // Positionne le curseur au début
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}