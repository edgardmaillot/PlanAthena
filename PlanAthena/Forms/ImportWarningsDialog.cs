using System.Text;
using System.Windows.Forms;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Formulaire simple pour afficher une liste détaillée d'avertissements (warnings)
    /// suite à une opération d'import ou autre.
    /// </summary>
    public partial class ImportWarningsDialog : Form
    {
        public ImportWarningsDialog(List<string> warnings)
        {
            InitializeComponent();
            this.Text = "Avertissements d'Import";
            PopulateWarnings(warnings);
        }

        private void PopulateWarnings(List<string> warnings)
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