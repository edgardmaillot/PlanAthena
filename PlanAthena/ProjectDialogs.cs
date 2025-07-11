using PlanAthena.Services.Business;
using PlanAthena.Utilities;
using System;
using System.Windows.Forms;

namespace PlanAthena
{
    /// <summary>
    /// Dialog pour créer un nouveau projet
    /// </summary>
    public partial class NouveauProjetDialog : Form
    {
        public string NomProjet { get; set; } = "";
        public string Description { get; set; } = "";
        public string Auteur { get; set; } = Environment.UserName;

        public NouveauProjetDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var lblNom = new Label { Text = "Nom du projet:", Location = new System.Drawing.Point(12, 15), Size = new System.Drawing.Size(100, 23) };
            var txtNom = new TextBox { Location = new System.Drawing.Point(118, 12), Size = new System.Drawing.Size(200, 23) };

            var lblAuteur = new Label { Text = "Auteur:", Location = new System.Drawing.Point(12, 44), Size = new System.Drawing.Size(100, 23) };
            var txtAuteur = new TextBox { Text = Environment.UserName, Location = new System.Drawing.Point(118, 41), Size = new System.Drawing.Size(200, 23) };

            var lblDesc = new Label { Text = "Description:", Location = new System.Drawing.Point(12, 73), Size = new System.Drawing.Size(100, 23) };
            var txtDesc = new TextBox { Multiline = true, Location = new System.Drawing.Point(118, 70), Size = new System.Drawing.Size(200, 60) };

            var btnOK = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(163, 145), Size = new System.Drawing.Size(75, 23) };
            var btnCancel = new Button { Text = "Annuler", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(244, 145), Size = new System.Drawing.Size(75, 23) };

            btnOK.Click += (s, e) => {
                NomProjet = txtNom.Text;
                Description = txtDesc.Text;
                Auteur = txtAuteur.Text;
            };

            this.Controls.AddRange(new Control[] { lblNom, txtNom, lblAuteur, txtAuteur, lblDesc, txtDesc, btnOK, btnCancel });
            this.Text = "Nouveau Projet";
            this.Size = new System.Drawing.Size(350, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }

    /// <summary>
    /// Dialog pour import CSV groupé
    /// </summary>
    public partial class ImportCsvGroupeDialog : Form
    {
        public string CheminMetiers { get; set; } = "";
        public string CheminOuvriers { get; set; } = "";
        public string CheminTaches { get; set; } = "";

        public ImportCsvGroupeDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var lblInfo = new Label
            {
                Text = "Sélectionnez les fichiers CSV à importer:",
                Location = new System.Drawing.Point(12, 15),
                Size = new System.Drawing.Size(300, 23)
            };

            var lblMetiers = new Label { Text = "Métiers:", Location = new System.Drawing.Point(12, 50), Size = new System.Drawing.Size(60, 23) };
            var txtMetiers = new TextBox { Location = new System.Drawing.Point(78, 47), Size = new System.Drawing.Size(250, 23) };
            var btnMetiers = new Button { Text = "...", Location = new System.Drawing.Point(334, 47), Size = new System.Drawing.Size(30, 23) };

            var lblOuvriers = new Label { Text = "Ouvriers:", Location = new System.Drawing.Point(12, 79), Size = new System.Drawing.Size(60, 23) };
            var txtOuvriers = new TextBox { Location = new System.Drawing.Point(78, 76), Size = new System.Drawing.Size(250, 23) };
            var btnOuvriers = new Button { Text = "...", Location = new System.Drawing.Point(334, 76), Size = new System.Drawing.Size(30, 23) };

            var lblTaches = new Label { Text = "Tâches:", Location = new System.Drawing.Point(12, 108), Size = new System.Drawing.Size(60, 23) };
            var txtTaches = new TextBox { Location = new System.Drawing.Point(78, 105), Size = new System.Drawing.Size(250, 23) };
            var btnTaches = new Button { Text = "...", Location = new System.Drawing.Point(334, 105), Size = new System.Drawing.Size(30, 23) };

            var btnOK = new Button { Text = "Importer", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(207, 145), Size = new System.Drawing.Size(75, 23) };
            var btnCancel = new Button { Text = "Annuler", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(288, 145), Size = new System.Drawing.Size(75, 23) };

            btnMetiers.Click += (s, e) => {
                using var ofd = new OpenFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv" };
                if (ofd.ShowDialog() == DialogResult.OK) txtMetiers.Text = ofd.FileName;
            };

            btnOuvriers.Click += (s, e) => {
                using var ofd = new OpenFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv" };
                if (ofd.ShowDialog() == DialogResult.OK) txtOuvriers.Text = ofd.FileName;
            };

            btnTaches.Click += (s, e) => {
                using var ofd = new OpenFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv" };
                if (ofd.ShowDialog() == DialogResult.OK) txtTaches.Text = ofd.FileName;
            };

            btnOK.Click += (s, e) => {
                CheminMetiers = txtMetiers.Text;
                CheminOuvriers = txtOuvriers.Text;
                CheminTaches = txtTaches.Text;
            };

            this.Controls.AddRange(new Control[] {
                lblInfo, lblMetiers, txtMetiers, btnMetiers,
                lblOuvriers, txtOuvriers, btnOuvriers,
                lblTaches, txtTaches, btnTaches,
                btnOK, btnCancel
            });

            this.Text = "Import CSV Groupé";
            this.Size = new System.Drawing.Size(390, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}