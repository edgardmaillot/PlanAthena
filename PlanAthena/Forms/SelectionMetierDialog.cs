using PlanAthena.Data;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Dialog pour sélectionner un métier dans une liste
    /// </summary>
    public partial class SelectionMetierDialog : System.Windows.Forms.Form
    {
        public MetierRecord MetierSelectionne { get; private set; }
        private ListView listViewMetiers;

        public SelectionMetierDialog(List<MetierRecord> metiersDisponibles)
        {
            InitializeComponent();
            InitialiserListe(metiersDisponibles);
            AttacherEvenements();
        }

        private void InitializeComponent()
        {
            var lblInfo = new Label
            {
                Text = "Sélectionnez un métier à ajouter comme prérequis :",
                Location = new System.Drawing.Point(12, 15),
                Size = new System.Drawing.Size(350, 20)
            };

            listViewMetiers = new ListView
            {
                Name = "listViewMetiers",
                Location = new System.Drawing.Point(12, 40),
                Size = new System.Drawing.Size(360, 200),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            listViewMetiers.Columns.Add("ID", 100);
            listViewMetiers.Columns.Add("Nom", 250);

            var btnOK = new Button
            {
                Name = "btnOK",
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(217, 255),
                Size = new System.Drawing.Size(75, 23)
            };

            var btnAnnuler = new Button
            {
                Text = "Annuler",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(298, 255),
                Size = new System.Drawing.Size(75, 23)
            };

            this.Controls.AddRange(new Control[] { lblInfo, listViewMetiers, btnOK, btnAnnuler });
            this.Text = "Sélection de Métier";
            this.Size = new System.Drawing.Size(400, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void AttacherEvenements()
        {
            listViewMetiers.DoubleClick += ListViewMetiers_DoubleClick;

            var btnOK = this.Controls["btnOK"] as Button;
            btnOK.Click += BtnOK_Click;
        }

        private void ListViewMetiers_DoubleClick(object sender, EventArgs e)
        {
            if (listViewMetiers.SelectedItems.Count > 0)
            {
                MetierSelectionne = listViewMetiers.SelectedItems[0].Tag as MetierRecord;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (listViewMetiers.SelectedItems.Count > 0)
            {
                MetierSelectionne = listViewMetiers.SelectedItems[0].Tag as MetierRecord;
            }
        }

        private void InitialiserListe(List<MetierRecord> metiers)
        {
            foreach (var metier in metiers.OrderBy(m => m.MetierId))
            {
                var item = new ListViewItem(new[] { metier.MetierId, metier.Nom })
                {
                    Tag = metier
                };
                listViewMetiers.Items.Add(item);
            }
        }
    }
}