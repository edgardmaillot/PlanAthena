// Emplacement: /View/TaskManager/Cockpit/TaskListView.cs
// Version: 1.0.1 (Correction des problèmes de mise à jour du DataGrid)

#region Using Directives
using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.Projet;
using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
#endregion

namespace PlanAthena.View.TaskManager.Cockpit
{
    public partial class TaskListView : UserControl
    {
        #region Champs Privés
        private PilotageProjetUseCase _useCase;
        private List<TaskListItem> _allItems;
        private ProjetService _projetService;
        private bool _isLoading = false;
        #endregion

        #region Constructeur et Initialisation
        public TaskListView()
        {
            InitializeComponent();
            typeof(DataGridView).InvokeMember("DoubleBuffered",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
        null, kryptonDataGridView1, new object[] { true });
        }

        public void Initialize(PilotageProjetUseCase useCase, ProjetService projetService)
        {
            _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));

            ConfigureDataGridView();
            AttachEvents();
        }

        private void ConfigureDataGridView()
        {
            // Configuration pour permettre l'édition personnalisée
            kryptonDataGridView1.ReadOnly = false;

            // S'assurer que seules les colonnes de dates sont éditables
            foreach (DataGridViewColumn column in kryptonDataGridView1.Columns)
            {
                if (column.Name == "DG_DDR" || column.Name == "DG_DFR")
                {
                    column.ReadOnly = false;
                }
                else
                {
                    column.ReadOnly = true;
                }
            }

            // Configuration supplémentaire pour améliorer l'expérience utilisateur
            kryptonDataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            kryptonDataGridView1.MultiSelect = false;
        }

        private void AttachEvents()
        {
            // Désabonnement systématique
            cmbFiltreLot.SelectedIndexChanged -= FilterChanged;
            cmbFiltreMetier.SelectedIndexChanged -= FilterChanged;
            cmbFiltreStatut.SelectedIndexChanged -= FilterChanged;
            kryptonDataGridView1.CellFormatting -= DataGridView_CellFormatting;
            btnResetFiltres.Click -= BtnResetFiltres_Click;
            btnAControler.Click -= BtnAControler_Click;
            kryptonDataGridView1.CellBeginEdit -= DataGridView_CellBeginEdit;
            // Ajout d'un gestionnaire pour les clics sur les cellules
            kryptonDataGridView1.CellClick -= DataGridView_CellClick;

            // Abonnement aux événements
            cmbFiltreLot.SelectedIndexChanged += FilterChanged;
            cmbFiltreMetier.SelectedIndexChanged += FilterChanged;
            cmbFiltreStatut.SelectedIndexChanged += FilterChanged;
            kryptonDataGridView1.CellFormatting += DataGridView_CellFormatting;
            btnResetFiltres.Click += BtnResetFiltres_Click;
            btnAControler.Click += BtnAControler_Click;
            kryptonDataGridView1.CellBeginEdit += DataGridView_CellBeginEdit;
            // Alternative avec CellClick pour plus de réactivité
            kryptonDataGridView1.CellClick += DataGridView_CellClick;
        }
        #endregion

        #region Logique de Données et de Rafraîchissement
        public void RefreshData()
        {
            if (_useCase == null) return;
            _isLoading = true;

            try
            {
                var data = _useCase.ObtenirDonneesPourTaskList();
                _allItems = data.Items;

                var selectedLot = cmbFiltreLot.SelectedItem?.ToString();
                var selectedMetier = cmbFiltreMetier.SelectedItem?.ToString();
                var selectedStatut = cmbFiltreStatut.SelectedItem?.ToString();
                var statutsDisponibles = Enum.GetNames(typeof(Statut)).ToList();

                PopulateFilterComboBox(cmbFiltreLot, data.LotsDisponibles, selectedLot);
                PopulateFilterComboBox(cmbFiltreMetier, data.MetiersDisponibles, selectedMetier);
                PopulateFilterComboBox(cmbFiltreStatut, statutsDisponibles, selectedStatut);

                ApplyFiltersAndPopulateGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du rafraîchissement des données : {ex.Message}",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void UpdateTaskDate(int rowIndex, string tacheId, string columnName, DateTime? newDate)
        {
            if (rowIndex < 0 || rowIndex >= kryptonDataGridView1.RowCount) return;
            var row = kryptonDataGridView1.Rows[rowIndex];
            if (row.Tag is not TaskListItem item) return;

            try
            {
                // Sauvegarder le contexte utilisateur
                int firstDisplayedRowIndex = kryptonDataGridView1.FirstDisplayedScrollingRowIndex;
                if (firstDisplayedRowIndex < 0) firstDisplayedRowIndex = 0;

                var debutReel = item.DateDebutReelle; // Utiliser les valeurs de l'objet métier
                var finReelle = item.DateFinReelle;

                // Mise à jour selon la colonne modifiée
                if (columnName == "DG_DDR")
                {
                    debutReel = newDate;
                }
                else if (columnName == "DG_DFR")
                {
                    finReelle = newDate;
                }

                // Mise à jour via le UseCase
                _useCase.MettreAJourAvancementTache(tacheId, debutReel, finReelle);

                // Mise à jour immédiate de la cellule pour un feedback instantané
                if (columnName == "DG_DDR")
                {
                    row.Cells["DG_DDR"].Value = newDate;
                }
                else if (columnName == "DG_DFR")
                {
                    row.Cells["DG_DFR"].Value = newDate;
                }

                // Rafraîchir la ligne pour mettre à jour le formatage
                kryptonDataGridView1.InvalidateRow(rowIndex);

                // Rafraîchissement complet en arrière-plan pour garantir la cohérence
                this.BeginInvoke((Action)(() =>
                {
                    RefreshData();

                    // Restaurer la sélection et la position de scroll
                    foreach (DataGridViewRow r in kryptonDataGridView1.Rows)
                    {
                        if ((r.Tag as TaskListItem)?.TacheId == tacheId)
                        {
                            r.Selected = true;
                            try
                            {
                                if (kryptonDataGridView1.RowCount > firstDisplayedRowIndex)
                                    kryptonDataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedRowIndex;
                            }
                            catch { /* Position de scroll invalide, ignorer */ }
                            break;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour de la tâche : {ex.Message}",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Gestionnaires d'Événements

        /// <summary>
        /// Gestionnaire alternatif utilisant CellClick - plus réactif que CellBeginEdit
        /// </summary>
        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var columnName = kryptonDataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName != "DG_DDR" && columnName != "DG_DFR") return;

            ShowDateTimePicker(e.RowIndex, e.ColumnIndex, columnName);
        }

        /// <summary>
        /// Gestionnaire principal pour l'édition des dates
        /// </summary>
        private void DataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = kryptonDataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName != "DG_DDR" && columnName != "DG_DFR")
            {
                return;
            }

            // Annuler l'édition standard
            e.Cancel = true;

            ShowDateTimePicker(e.RowIndex, e.ColumnIndex, columnName);
        }

        /// <summary>
        /// Méthode centralisée pour afficher le sélecteur de date
        /// </summary>
        private void ShowDateTimePicker(int rowIndex, int columnIndex, string columnName)
        {
            if (kryptonDataGridView1.Rows[rowIndex].Tag is not TaskListItem item) return;

            try
            {
                var projetInfo = _projetService.ObtenirInformationsProjet();
                DateTime? initialDate = null;

                // Récupérer la date actuelle selon la colonne
                if (columnName == "DG_DDR")
                {
                    initialDate = item.DateDebutReelle ?? item.DateDebutPlanifiee;
                }
                else if (columnName == "DG_DFR")
                {
                    initialDate = item.DateFinReelle ?? item.DateFinPlanifiee;
                }

                // Calculer la position du popup
                Rectangle cellBounds = kryptonDataGridView1.GetCellDisplayRectangle(columnIndex, rowIndex, true);
                Point location = kryptonDataGridView1.PointToScreen(new Point(cellBounds.Left, cellBounds.Bottom));

                // Créer et afficher le popup
                using (var popupForm = new DateTimePickerPopupForm(initialDate, projetInfo))
                {
                    popupForm.Location = location;

                    // S'assurer que le popup reste dans l'écran
                    var screen = Screen.FromPoint(location);
                    if (location.X + popupForm.Width > screen.WorkingArea.Right)
                    {
                        location.X = screen.WorkingArea.Right - popupForm.Width;
                    }
                    if (location.Y + popupForm.Height > screen.WorkingArea.Bottom)
                    {
                        location.Y = cellBounds.Top - popupForm.Height;
                        location = kryptonDataGridView1.PointToScreen(new Point(cellBounds.Left, location.Y));
                    }
                    popupForm.Location = location;

                    DialogResult result = popupForm.ShowDialog(this.FindForm());

                    if (result == DialogResult.OK)
                    {
                        UpdateTaskDate(rowIndex, item.TacheId, columnName, popupForm.SelectedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'affichage du sélecteur de date : {ex.Message}",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ApplyFiltersAndPopulateGrid();
        }

        private void BtnResetFiltres_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            _isLoading = true;
            cmbFiltreLot.SelectedItem = "Tous";
            cmbFiltreMetier.SelectedItem = "Tous";
            cmbFiltreStatut.SelectedItem = "Tous";
            _isLoading = false;
            ApplyFiltersAndPopulateGrid();
        }

        private void BtnAControler_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            _isLoading = true;
            cmbFiltreLot.SelectedItem = "Tous";
            cmbFiltreMetier.SelectedItem = "Tous";
            cmbFiltreStatut.SelectedItem = "Tous";
            _isLoading = false;
            ApplyAControlerFilter();
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (kryptonDataGridView1.Rows[e.RowIndex].Tag is not TaskListItem item) return;

            var style = e.CellStyle;
            var font = style.Font ?? kryptonDataGridView1.DefaultCellStyle.Font;

            // Réinitialisation du style
            style.ForeColor = kryptonDataGridView1.DefaultCellStyle.ForeColor;
            style.Font = font;

            if (item.Statut == Statut.EnRetard.ToString())
            {
                style.ForeColor = Color.Red;
                style.Font = new Font(font, FontStyle.Bold);
            }
            else if (item.Statut == Statut.Terminée.ToString())
            {
                style.ForeColor = Color.Gray;
                style.Font = new Font(font, FontStyle.Italic);
            }
        }
        #endregion

        #region Méthodes d'Application des Filtres
        private void ApplyFiltersAndPopulateGrid()
        {
            kryptonDataGridView1.Rows.Clear();
            if (_allItems == null) return;

            IEnumerable<TaskListItem> filteredItems = _allItems;

            var selectedLotNom = cmbFiltreLot.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedLotNom) && selectedLotNom != "Tous")
            {
                filteredItems = filteredItems.Where(item => {
                    var lotId = item.TacheOriginale.LotId;
                    if (string.IsNullOrEmpty(lotId)) return false;
                    var lot = _projetService.ObtenirLotParId(lotId);
                    return lot?.Nom == selectedLotNom;
                });
            }

            var selectedMetier = cmbFiltreMetier.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedMetier) && selectedMetier != "Tous")
            {
                filteredItems = filteredItems.Where(item => item.NomMetier == selectedMetier);
            }

            var selectedStatut = cmbFiltreStatut.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedStatut) && selectedStatut != "Tous")
            {
                filteredItems = filteredItems.Where(item => item.Statut == selectedStatut);
            }

            PopulateGrid(filteredItems);
        }

        private void ApplyAControlerFilter()
        {
            if (_allItems == null) return;

            var dateLimite = DateTime.Today.AddDays(1);
            var statutsCibles = new[] { Statut.Planifiée.ToString(), Statut.EnRetard.ToString() };

            var filteredItems = _allItems.Where(item =>
                item.DateDebutPlanifiee.HasValue &&
                item.DateDebutPlanifiee.Value.Date <= dateLimite &&
                statutsCibles.Contains(item.Statut)
            );

            PopulateGrid(filteredItems);
        }

        private void PopulateGrid(IEnumerable<TaskListItem> itemsToDisplay)
        {
            kryptonDataGridView1.Rows.Clear();

            kryptonDataGridView1.SuspendLayout();

            try
            {
                foreach (var item in itemsToDisplay.OrderBy(i => i.TacheOriginale.LotId).ThenBy(i => i.TacheId))
                {
                    var rowIndex = kryptonDataGridView1.Rows.Add();
                    var row = kryptonDataGridView1.Rows[rowIndex];
                    row.Tag = item;

                    row.Cells["DG_TacheId"].Value = item.TacheId;
                    row.Cells["DG_Tache"].Value = item.NomTache;
                    row.Cells["DG_Metier"].Value = item.NomMetier;
                    row.Cells["DG_Ouvrier"].Value = item.NomsOuvriersAffectes;
                    row.Cells["DG_Statut"].Value = item.Statut;
                    row.Cells["DG_DDP"].Value = item.DateDebutPlanifiee;
                    row.Cells["DG_DFP"].Value = item.DateFinPlanifiee;
                    row.Cells["DG_DDR"].Value = item.DateDebutReelle;
                    row.Cells["DG_DFR"].Value = item.DateFinReelle;
                }
            }
            finally
            {
                kryptonDataGridView1.ResumeLayout();
            }
        }

        private void PopulateFilterComboBox(KryptonComboBox comboBox, List<string> items, string currentValue)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add("Tous");
            if (items != null)
            {
                comboBox.Items.AddRange(items.ToArray());
            }
            if (!string.IsNullOrEmpty(currentValue) && comboBox.Items.Contains(currentValue))
            {
                comboBox.SelectedItem = currentValue;
            }
            else
            {
                comboBox.SelectedItem = "Tous";
            }
        }
        #endregion

        #region Classe Interne pour le Formulaire Popup
        private class DateTimePickerPopupForm : KryptonForm
        {
            public DateTime? SelectedValue { get; private set; }

            public DateTimePickerPopupForm(DateTime? initialDate, InformationsProjet projetInfo)
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                Padding = new Padding(1);
                BackColor = Color.FromArgb(180, 180, 180);
                TopMost = true; // S'assurer que le popup reste au premier plan

                var dateTimePickerControl = new PlanAthena.View.TaskManager.Utilitaires.DateTimePicker();
                this.Controls.Add(dateTimePickerControl);

                this.Size = dateTimePickerControl.Size;
                this.AutoSize = true;
                this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                dateTimePickerControl.InitializeData(initialDate, projetInfo);

                dateTimePickerControl.DateTimeSelected += (s, selectedDateTime) =>
                {
                    this.SelectedValue = selectedDateTime;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                dateTimePickerControl.SelectionCancelled += (s, args) =>
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                };

                // Gérer la fermeture avec Escape
                this.KeyPreview = true;
                this.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        this.DialogResult = DialogResult.Cancel;
                        this.Close();
                    }
                };
            }

            protected override void OnDeactivate(EventArgs e)
            {
                base.OnDeactivate(e);
                if (this.DialogResult == DialogResult.None)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
        }
        #endregion
    }
}