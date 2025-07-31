using PlanAthena.Data;

namespace PlanAthena.Forms
{
    /// <summary>
    /// Dialog pour sélectionner un métier dans une liste
    /// </summary>
    public partial class SelectionMetierDialog : System.Windows.Forms.Form
    {
        public Metier MetierSelectionne { get; private set; }
        private ListView listViewMetiers;

        public SelectionMetierDialog(List<Metier> metiersDisponibles)
        {
            InitializeComponent();
            InitialiserListe(metiersDisponibles);
            AttacherEvenements();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectionMetierDialog));
            lblInfo = new Label();
            listViewMetiers = new ListView();
            btnOK = new Button();
            btnAnnuler = new Button();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // lblInfo
            // 
            lblInfo.Location = new Point(21, 0);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(100, 23);
            lblInfo.TabIndex = 0;
            // 
            // listViewMetiers
            // 
            listViewMetiers.Location = new Point(0, 0);
            listViewMetiers.Name = "listViewMetiers";
            listViewMetiers.Size = new Size(121, 97);
            listViewMetiers.TabIndex = 1;
            listViewMetiers.UseCompatibleStateImageBehavior = false;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(0, 0);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 2;
            // 
            // btnAnnuler
            // 
            btnAnnuler.Location = new Point(0, 0);
            btnAnnuler.Name = "btnAnnuler";
            btnAnnuler.Size = new Size(75, 23);
            btnAnnuler.TabIndex = 3;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 29);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(58, 50);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // SelectionMetierDialog
            // 
            ClientSize = new Size(384, 281);
            Controls.Add(pictureBox1);
            Controls.Add(lblInfo);
            Controls.Add(listViewMetiers);
            Controls.Add(btnOK);
            Controls.Add(btnAnnuler);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectionMetierDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Sélection de Métier";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
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
                MetierSelectionne = listViewMetiers.SelectedItems[0].Tag as Metier;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (listViewMetiers.SelectedItems.Count > 0)
            {
                MetierSelectionne = listViewMetiers.SelectedItems[0].Tag as Metier;
            }
        }

        private void InitialiserListe(List<Metier> metiers)
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
        private Label lblInfo;
        private Button btnOK;
        private Button btnAnnuler;
        private PictureBox pictureBox1;
    }
}