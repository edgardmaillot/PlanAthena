using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Data;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Dialog pour ajouter/modifier une compétence d'ouvrier
    /// </summary>
    public partial class CompetenceDialog : System.Windows.Forms.Form
    {
        public MetierRecord MetierSelectionne { get; private set; }
        public NiveauExpertise NiveauExpertise { get; private set; }
        public int? PerformancePct { get; private set; }

        private ComboBox cmbMetier;
        private ComboBox cmbNiveau;
        private NumericUpDown numPerformance;
        private readonly bool _modificationMode;

        public CompetenceDialog(List<MetierRecord> metiersDisponibles, OuvrierRecord competenceExistante = null)
        {
            _modificationMode = competenceExistante != null;
            InitializeComponent();
            InitialiserDonnees(metiersDisponibles, competenceExistante);
        }

        private void InitializeComponent()
        {
            var lblMetier = new Label
            {
                Text = "Métier:",
                Location = new Point(12, 15),
                Size = new Size(60, 23)
            };

            cmbMetier = new ComboBox
            {
                Location = new Point(80, 12),
                Size = new Size(250, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Nom",
                ValueMember = "MetierId"
            };

            var lblNiveau = new Label
            {
                Text = "Niveau:",
                Location = new Point(12, 50),
                Size = new Size(60, 23)
            };

            cmbNiveau = new ComboBox
            {
                Location = new Point(80, 47),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Remplir les niveaux d'expertise
            foreach (NiveauExpertise niveau in Enum.GetValues(typeof(NiveauExpertise)))
            {
                cmbNiveau.Items.Add(new { Text = GetNiveauText(niveau), Value = niveau });
            }
            cmbNiveau.DisplayMember = "Text";
            cmbNiveau.ValueMember = "Value";

            var lblPerformance = new Label
            {
                Text = "Performance (%):",
                Location = new Point(12, 85),
                Size = new Size(100, 23)
            };

            numPerformance = new NumericUpDown
            {
                Location = new Point(120, 82),
                Size = new Size(80, 23),
                Minimum = 1,
                Maximum = 200,
                Value = 100
            };

            var btnOK = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(175, 125),
                Size = new Size(75, 23)
            };

            var btnAnnuler = new Button
            {
                Text = "Annuler",
                DialogResult = DialogResult.Cancel,
                Location = new Point(256, 125),
                Size = new Size(75, 23)
            };

            btnOK.Click += BtnOK_Click;

            this.Controls.AddRange(new Control[] {
                lblMetier, cmbMetier, lblNiveau, cmbNiveau,
                lblPerformance, numPerformance, btnOK, btnAnnuler
            });

            this.Text = _modificationMode ? "Modifier une compétence" : "Ajouter une compétence";
            this.Size = new Size(360, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void InitialiserDonnees(List<MetierRecord> metiersDisponibles, OuvrierRecord competenceExistante)
        {
            // Remplir la liste des métiers
            cmbMetier.DataSource = metiersDisponibles.OrderBy(m => m.Nom).ToList();

            if (_modificationMode && competenceExistante != null)
            {
                // Mode modification
                cmbMetier.Enabled = false;

                // Sélectionner le métier existant
                var metierExistant = metiersDisponibles.FirstOrDefault(m => m.MetierId == competenceExistante.MetierId);
                if (metierExistant != null)
                {
                    cmbMetier.SelectedItem = metierExistant;
                }

                // Sélectionner le niveau existant
                var niveauItem = cmbNiveau.Items.Cast<dynamic>()
                    .FirstOrDefault(item => item.Value.Equals(competenceExistante.NiveauExpertise));
                if (niveauItem != null)
                {
                    cmbNiveau.SelectedItem = niveauItem;
                }

                // Performance existante
                numPerformance.Value = competenceExistante.PerformancePct ?? 100;
            }
            else
            {
                // Mode ajout - valeurs par défaut
                if (cmbMetier.Items.Count > 0)
                    cmbMetier.SelectedIndex = 0;

                if (cmbNiveau.Items.Count > 0)
                    cmbNiveau.SelectedIndex = 1; // Débutant par défaut

                numPerformance.Value = 100;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (cmbMetier.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un métier.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbNiveau.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un niveau d'expertise.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MetierSelectionne = cmbMetier.SelectedItem as MetierRecord;
            NiveauExpertise = ((dynamic)cmbNiveau.SelectedItem).Value;
            PerformancePct = (int)numPerformance.Value;
        }

        private string GetNiveauText(NiveauExpertise niveau)
        {
            return niveau switch
            {
                NiveauExpertise.Debutant => "Débutant",
                NiveauExpertise.Confirme => "Confirmé",
                NiveauExpertise.Expert => "Expert",
                _ => niveau.ToString()
            };
        }
    }
}