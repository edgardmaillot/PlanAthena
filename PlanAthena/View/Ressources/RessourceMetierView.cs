using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.View.Ressources.MetierDiagram;
using System.ComponentModel;

namespace PlanAthena.View.Ressources
{
    public partial class RessourceMetierView : UserControl
    {
        private readonly RessourceService _ressourceService;
        private readonly ProjetService _projetService;

        // Événement pour demander la navigation vers une autre vue
        public event EventHandler<Type> NavigateToViewRequested;

        private bool _isLoading = false;
        private BindingList<MetierViewModel> _metiersBindingList;
        private BindingSource _metiersBindingSource;

        public RessourceMetierView(RessourceService ressourceService, ProjetService projetService)
        {
            InitializeComponent();
            _ressourceService = ressourceService;
            _projetService = projetService;

            this.Load += RessourceMetierView_Load;
        }

        #region ViewModel pour la grille
        public class MetierViewModel : INotifyPropertyChanged
        {
            private string _metierId;
            private string _nom;
            private ChantierPhase _phases;

            public string MetierId
            {
                get => _metierId;
                set { _metierId = value; OnPropertyChanged(); }
            }

            public string Nom
            {
                get => _nom;
                set { _nom = value; OnPropertyChanged(); }
            }

            public ChantierPhase Phases
            {
                get => _phases;
                set { _phases = value; OnPropertyChanged(); }
            }

            public string PhasesDisplay => GetPhasesDisplay(_phases);

            private string GetPhasesDisplay(ChantierPhase phases)
            {
                var phasesList = new List<string>();
                if (phases.HasFlag(ChantierPhase.GrosOeuvre)) phasesList.Add("Gros Œuvre");
                if (phases.HasFlag(ChantierPhase.SecondOeuvre)) phasesList.Add("Second Œuvre");
                if (phases.HasFlag(ChantierPhase.Finition)) phasesList.Add("Finition");
                return string.Join(", ", phasesList);
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private void RessourceMetierView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            InitializeBindingList();
            SetupGrid();
            InitializePictogramSelector();
            AttachEvents();
            RefreshAll();
        }

        private void InitializeBindingList()
        {
            _metiersBindingList = new BindingList<MetierViewModel>();
            _metiersBindingSource = new BindingSource(_metiersBindingList, null);
            gridMetiers.DataSource = _metiersBindingSource;
        }

        private void SetupGrid()
        {
            gridMetiers.AutoGenerateColumns = false;
            gridMetiers.Columns.Clear();

            gridMetiers.Columns.Add(new KryptonDataGridViewTextBoxColumn
            {
                Name = "MetierId",
                HeaderText = "ID",
                DataPropertyName = "MetierId",
                FillWeight = 20,
                ReadOnly = true
            });

            gridMetiers.Columns.Add(new KryptonDataGridViewTextBoxColumn
            {
                Name = "Nom",
                HeaderText = "Nom",
                DataPropertyName = "Nom",
                FillWeight = 50,
                ReadOnly = true
            });

            gridMetiers.Columns.Add(new KryptonDataGridViewTextBoxColumn
            {
                Name = "Phases",
                HeaderText = "Phases",
                DataPropertyName = "PhasesDisplay",
                FillWeight = 30,
                ReadOnly = true
            });

            // Configuration pour que la grille prenne toute la largeur
            gridMetiers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void AttachEvents()
        {
            gridMetiers.SelectionChanged += gridMetiers_SelectionChanged;
            btnNewMetier.Click += btnNewMetier_Click;
            btnDeleteMetier.Click += btnDeleteMetier_Click;
            btnChooseColor.Click += btnChooseColor_Click;

            textName.TextChanged += Detail_Changed;
            // ENLEVER : textPictogram.TextChanged += Detail_Changed;
            chkGrosOeuvre.CheckedChanged += Detail_Changed;
            chkSecondOeuvre.CheckedChanged += Detail_Changed;
            chkFinition.CheckedChanged += Detail_Changed;

            btnConfigurePrerequis.Click += BtnConfigurePrerequis_Click;
        }

        #region Gestion du sélecteur de pictogrammes

        private Dictionary<string, List<string>> _pictogramCategories = new Dictionary<string, List<string>>
        {
            ["Par défaut"] = new List<string> {"⛏", "🧱", "⚒", "⛺", "▤", "💧", "🚪", "⛲", "⚡", "❄", "▥", "🎨", "▦", "▧", "♲"},
            ["Outils et équipements"] = new List<string> { "🔨", "🔧", "⚙", "🪓", "🪚", "📏", "📐", "⚖", "🔩", "⛓", "🪣", "🧰", "🪜", "🔗", "⚰", "🗜", "⛴", "⚓", "🎯", "📡" },
            ["Techniques et industriels"] = new List<string> { "⨌", "⧥", "✇", "⚞", "⚟", "⚡", "🔌", "💻", "📊", "📈", "⚆", "⚇", "⚈", "⚉", "⊕", "⊗", "⊙", "⊚", "⊛", "⊜", "⊝", "⊞", "⊟", "⊠", "⊡", "⌘", "⌬", "⌭", "⌮", "⌯" },
            ["Bâtiment"] = new List<string> { "🏠", "🏢", "🏗", "🏘", "🏚", "🏛", "⚒", "🔨", "🔧", "⚙", "🧱", "🚧", "⛏", "🪓", "📐", "📏" },
            ["Engins de chantier"] = new List<string> { "⛟", "⛜", "🚚", "🚛", "🚜", "🏗", "🚧", "⛽", "🚐", "🚌", "🚂", "🚆", "⛴", "⚓" },
            ["Transport"] = new List<string> { "🚗", "🚙", "🚐", "🚛", "🚜", "🏍", "🚲", "✈", "🚁", "⛵", "🚤", "🚢", "🚂", "🚆", "🚇", "🚌" },
            ["Sécurité"] = new List<string> { "☢", "☣", "⚠", "🛡", "🦺", "⛑", "🚨", "🔒", "🔓", "🗝", "⚞", "⚟" },
            ["Laboratoire/Mesure"] = new List<string> { "🧪", "⚗", "🧬", "🌡", "💉", "🔬", "🧲", "⚛", "⚖", "📊", "📈", "💻" },
            ["Électricité/Énergie"] = new List<string> { "⚡", "🔌", "🔋", "💡", "⨌", "⧥", "✇", "⚆", "⚇", "⚈", "⚉" },
            ["Géométrie et mesure"] = new List<string> { "△", "▲", "▴", "▵", "▶", "▷", "▸", "▹", "►", "▻", "◀", "◁", "◂", "◃", "◄", "◅", "◆", "◇", "◈", "◉", "◊", "○", "◌", "◍", "◎", "●", "◐", "◑", "◒", "◓", "◔", "◕", "◖", "◗", "◘", "◙", "◚", "◛", "◜", "◝"},
            ["Symboles"] = new List<string> { "⚒", "⚓", "⚔", "⚕", "⚖", "⚗", "⚘", "⚙", "⚚", "⚛", "⚜", "⚝", "⚞", "⚟", "⚠", "⚡", "⚢", "⚣", "⚤", "⚥", "⚦", "⚧", "⚨", "⚩", "⚪", "⚫", "⚬", "⚭", "⚮", "⚯" }
        };

        private void InitializePictogramSelector()
        {
            // Remplir la combo des catégories
            comboPictogramCategory.Items.Clear();
            comboPictogramCategory.Items.Add("(Sélectionner une catégorie)");
            foreach (var category in _pictogramCategories.Keys)
            {
                comboPictogramCategory.Items.Add(category);
            }
            comboPictogramCategory.SelectedIndex = 0;

            // Vider la grille au début
            flowPictogramGrid.Controls.Clear();
        }

        private void ComboPictogramCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            flowPictogramGrid.Controls.Clear();

            if (comboPictogramCategory.SelectedIndex <= 0) return;

            var selectedCategory = comboPictogramCategory.SelectedItem.ToString();
            if (!_pictogramCategories.ContainsKey(selectedCategory)) return;

            var pictograms = _pictogramCategories[selectedCategory];

            foreach (var pictogram in pictograms)
            {
                var btn = new Button
                {
                    Text = pictogram,
                    Font = new Font("Segoe UI Symbol", 16F),
                    Size = new Size(35, 35),
                    Margin = new Padding(2),
                    FlatStyle = FlatStyle.Standard,
                    BackColor = Color.White,
                    UseVisualStyleBackColor = true
                };

                btn.Click += (s, args) =>
                {
                    SetSelectedPictogram(pictogram);
                };

                flowPictogramGrid.Controls.Add(btn);
            }
        }

        private void SetSelectedPictogram(string pictogram)
        {
            lblPictogramPreview.Values.Text = pictogram;
            lblPictogramPreview.Text = pictogram;

            // Sauvegarder si un métier est sélectionné
            var metier = GetSelectedMetier();
            if (metier != null && !_isLoading)
            {
                metier.Pictogram = pictogram;
                try
                {
                    _ressourceService.ModifierMetier(metier);
                    RefreshSingleMetierInGrid(metier.MetierId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur de modification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    RefreshDetails();
                }
            }
        }

        private void BtnClearPictogram_Click(object sender, EventArgs e)
        {
            SetSelectedPictogram("");
        }

        #endregion

        #region Logique de rafraîchissement optimisée

        private void RefreshAll()
        {
            var selectedId = GetSelectedMetierId();
            RebuildMetiersBindingList();

            if (selectedId != null)
            {
                SelectMetierInGrid(selectedId);
            }

            RefreshUIFromSelection();
        }

        private void RebuildMetiersBindingList()
        {
            _isLoading = true;

            var metiers = _ressourceService.GetAllMetiers();

            // Vider et reconstruire la liste
            _metiersBindingList.Clear();

            foreach (var metier in metiers)
            {
                _metiersBindingList.Add(CreateViewModelFromMetier(metier));
            }

            _isLoading = false;
        }

        private MetierViewModel CreateViewModelFromMetier(Metier metier)
        {
            return new MetierViewModel
            {
                MetierId = metier.MetierId,
                Nom = metier.Nom,
                Phases = metier.Phases
            };
        }

        private void RefreshSingleMetierInGrid(string metierId)
        {
            var metier = _ressourceService.GetMetierById(metierId);
            if (metier == null) return;

            var existingViewModel = _metiersBindingList.FirstOrDefault(vm => vm.MetierId == metierId);
            if (existingViewModel != null)
            {
                // Mettre à jour le ViewModel existant
                existingViewModel.Nom = metier.Nom;
                existingViewModel.Phases = metier.Phases;
            }
        }

        private void RefreshUIFromSelection()
        {
            RefreshDetails();
            UpdateButtonStates();
        }

        private void RefreshDetails()
        {
            _isLoading = true;
            var metier = GetSelectedMetier();
            if (metier != null)
            {
                textId.Text = metier.MetierId;
                textName.Text = metier.Nom;
                lblPictogramPreview.Values.Text = metier.Pictogram ?? ""; // MODIFIER cette ligne
                panelColor.StateCommon.Color1 = _ressourceService.GetDisplayColorForMetier(metier.MetierId);

                chkGrosOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.GrosOeuvre);
                chkSecondOeuvre.Checked = metier.Phases.HasFlag(ChantierPhase.SecondOeuvre);
                chkFinition.Checked = metier.Phases.HasFlag(ChantierPhase.Finition);
            }
            else
            {
                ClearDetails();
            }
            _isLoading = false;
        }

        private void ClearDetails()
        {
            textId.Clear();
            textName.Clear();
            lblPictogramPreview.Values.Text = ""; // MODIFIER cette ligne
            panelColor.StateCommon.Color1 = SystemColors.Control;
            chkGrosOeuvre.Checked = false;
            chkSecondOeuvre.Checked = false;
            chkFinition.Checked = false;

            // Réinitialiser le sélecteur
            comboPictogramCategory.SelectedIndex = 0;
            flowPictogramGrid.Controls.Clear();
        }

        private void UpdateButtonStates()
        {
            bool metierSelected = GetSelectedMetier() != null;
            groupDetails.Enabled = metierSelected;
            btnDeleteMetier.Enabled = metierSelected;
        }

        #endregion

        #region Helpers

        private string GetSelectedMetierId()
        {
            if (gridMetiers.SelectedRows.Count > 0)
            {
                var selectedViewModel = gridMetiers.SelectedRows[0].DataBoundItem as MetierViewModel;
                return selectedViewModel?.MetierId;
            }
            return null;
        }

        private Metier GetSelectedMetier()
        {
            var id = GetSelectedMetierId();
            return id != null ? _ressourceService.GetMetierById(id) : null;
        }

        private void SelectMetierInGrid(string metierId)
        {
            if (metierId == null) return;

            _isLoading = true;
            for (int i = 0; i < _metiersBindingList.Count; i++)
            {
                if (_metiersBindingList[i].MetierId == metierId)
                {
                    gridMetiers.Rows[i].Selected = true;
                    gridMetiers.FirstDisplayedScrollingRowIndex = i;
                    break;
                }
            }
            _isLoading = false;
        }

        #endregion

        #region Événements des contrôles - Version optimisée

        private void gridMetiers_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            RefreshUIFromSelection();
        }

        private void Detail_Changed(object sender, EventArgs e)
        {
            if (_isLoading) return;
            var metier = GetSelectedMetier();
            if (metier == null) return;

            // Sauvegarder les modifications
            metier.Nom = textName.Text;
            // ENLEVER : metier.Pictogram = textPictogram.Text;

            ChantierPhase phases = ChantierPhase.None;
            if (chkGrosOeuvre.Checked) phases |= ChantierPhase.GrosOeuvre;
            if (chkSecondOeuvre.Checked) phases |= ChantierPhase.SecondOeuvre;
            if (chkFinition.Checked) phases |= ChantierPhase.Finition;
            metier.Phases = phases;

            try
            {
                _ressourceService.ModifierMetier(metier);
                RefreshSingleMetierInGrid(metier.MetierId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur de modification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshDetails();
            }
        }

        private void btnNewMetier_Click(object sender, EventArgs e)
        {
            try
            {
                var nouveauMetier = _ressourceService.CreerMetier();

                // Reconstruire la liste pour inclure le nouveau métier
                RebuildMetiersBindingList();

                // Sélectionner le nouveau métier
                SelectMetierInGrid(nouveauMetier.MetierId);

                // Forcer le rafraîchissement de l'interface utilisateur
                RefreshUIFromSelection();

                // Donner le focus au champ nom pour saisie immédiate
                textName.Focus();
                textName.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur de création", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteMetier_Click(object sender, EventArgs e)
        {
            var metier = GetSelectedMetier();
            if (metier == null) return;

            if (MessageBox.Show($"Supprimer le métier '{metier.Nom}' ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _ressourceService.SupprimerMetier(metier.MetierId);

                    // Retirer de la BindingList
                    var viewModel = _metiersBindingList.FirstOrDefault(vm => vm.MetierId == metier.MetierId);
                    if (viewModel != null)
                    {
                        _metiersBindingList.Remove(viewModel);
                    }

                    RefreshUIFromSelection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Impossible de supprimer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnChooseColor_Click(object sender, EventArgs e)
        {
            var metier = GetSelectedMetier();
            if (metier == null) return;

            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = _ressourceService.GetDisplayColorForMetier(metier.MetierId);
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        metier.CouleurHex = ColorTranslator.ToHtml(colorDialog.Color);
                        _ressourceService.ModifierMetier(metier);
                        panelColor.StateCommon.Color1 = colorDialog.Color;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Erreur de modification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnConfigurePrerequis_Click(object sender, EventArgs e)
        {
            NavigateToViewRequested?.Invoke(this, typeof(PrerequisMetierView));
        }

        #endregion
    }
}