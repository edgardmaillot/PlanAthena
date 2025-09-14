// Emplacement: /View/TaskManager/Cockpit/TaskListView.cs V0.5.0.1
using Krypton.Toolkit;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager
{
    public partial class TaskListView : UserControl
    {
        private const string FD = "dd/MM HH:mm";
        private PilotageProjetUseCase _useCase;
        private List<TaskListItem> _allItems;
        private ProjetService _projetService;
        private bool _isLoading = false;


        public TaskListView()
        {
            InitializeComponent();
        }

        public void Initialize(PilotageProjetUseCase useCase, ProjetService projetService)
        {
            _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            AttachEvents();
        }

        private void AttachEvents()
        {
            cmbFiltreLot.SelectedIndexChanged -= FilterChanged;
            cmbFiltreMetier.SelectedIndexChanged -= FilterChanged;
            cmbFiltreStatut.SelectedIndexChanged -= FilterChanged;
            kryptonDataGridView1.CellFormatting -= DataGridView_CellFormatting;
            btnResetFiltres.Click -= BtnResetFiltres_Click;
            kryptonDataGridView1.CellEndEdit -= DataGridView_CellEndEdit;
            btnAControler.Click -= BtnAControler_Click;

            cmbFiltreLot.SelectedIndexChanged += FilterChanged;
            cmbFiltreMetier.SelectedIndexChanged += FilterChanged;
            cmbFiltreStatut.SelectedIndexChanged += FilterChanged;
            kryptonDataGridView1.CellFormatting += DataGridView_CellFormatting;
            btnResetFiltres.Click += BtnResetFiltres_Click;
            kryptonDataGridView1.CellEndEdit += DataGridView_CellEndEdit;
            btnAControler.Click += BtnAControler_Click;
        }

        public void RefreshData()
        {
            if (_useCase == null) return;
            _isLoading = true;

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

            _isLoading = false;
        }

        private void PopulateFilterComboBox(KryptonComboBox comboBox, List<string> items, string currentValue)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add("Tous");
            if (items != null)
            {
                comboBox.Items.AddRange(items.ToArray());
            }
            if (!string.IsNullOrEmpty(currentValue) && items.Contains(currentValue))
            {
                comboBox.SelectedItem = currentValue;
            }
            else
            {
                comboBox.SelectedItem = "Tous";
            }
        }
        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = kryptonDataGridView1.Rows[e.RowIndex];
            if (row.Tag is not TaskListItem item) return;

            // Récupérer le style actuel pour ne modifier que ce qui est nécessaire
            var style = e.CellStyle;
            var font = style.Font;

            // Appliquer le style conditionnel
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
            // IMPORTANT : Pas de 'else' pour réinitialiser. Le DataGridView gère cela lui-même 
            // en ré-appliquant le style par défaut de la ligne/colonne avant cet événement.
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

            // Réinitialiser les filtres standards pour ne pas interférer
            _isLoading = true;
            cmbFiltreLot.SelectedItem = "Tous";
            cmbFiltreMetier.SelectedItem = "Tous";
            cmbFiltreStatut.SelectedItem = "Tous";
            _isLoading = false;

            ApplyAControlerFilter();
        }

        private void ApplyAControlerFilter()
        {
            kryptonDataGridView1.Rows.Clear();
            if (_allItems == null) return;

            var dateLimite = DateTime.Today.AddDays(1); // Aujourd'hui et demain
            var statutsCibles = new[] { Statut.Planifiée.ToString(), Statut.EnRetard.ToString() };

            var filteredItems = _allItems.Where(item =>
            {
                // Filtre a) sur la date de début planifiée
                bool dateValide = item.DateDebutPlanifiee.HasValue && item.DateDebutPlanifiee.Value.Date <= dateLimite;
                // Filtre b) sur le statut
                bool statutValide = statutsCibles.Contains(item.Statut);

                return dateValide && statutValide;
            });

            // CODE EN DOUBLON - À REFACTORISER
            foreach (var item in filteredItems.OrderBy(i => i.TacheOriginale.LotId).ThenBy(i => i.TacheId))
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

            foreach (var item in filteredItems.OrderBy(i => i.TacheOriginale.LotId).ThenBy(i => i.TacheId))
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

        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_isLoading) return;
            if (e.RowIndex < 0) return;

            var row = kryptonDataGridView1.Rows[e.RowIndex];
            if (row.Tag is not TaskListItem item) return;

            // --- Étape 1 : Sauvegarder le contexte de l'utilisateur ---
            string selectedTaskId = item.TacheId;
            int firstDisplayedRowIndex = kryptonDataGridView1.FirstDisplayedScrollingRowIndex;
            if (firstDisplayedRowIndex < 0) firstDisplayedRowIndex = 0;

            // --- Étape 2 : Mettre à jour les données (comme avant) ---
            var debutReel = row.Cells["DG_DDR"].Value as DateTime?;
            var finReelle = row.Cells["DG_DFR"].Value as DateTime?;

            _useCase.MettreAJourAvancementTache(item.TacheId, debutReel, finReelle);

            // --- Étape 3 : Déclencher un rafraîchissement complet de manière asynchrone ---
            this.BeginInvoke((Action)(() =>
            {
                RefreshData(); // Recharge _allItems depuis la source de vérité et redessine la grille

                // --- Étape 4 : Restaurer le contexte de l'utilisateur après le rafraîchissement ---
                foreach (DataGridViewRow r in kryptonDataGridView1.Rows)
                {
                    if ((r.Tag as TaskListItem)?.TacheId == selectedTaskId)
                    {
                        r.Selected = true;
                        // S'assurer que la ligne est visible
                        if (kryptonDataGridView1.RowCount > firstDisplayedRowIndex)
                        {
                            kryptonDataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedRowIndex;
                        }
                        break;
                    }
                }
            }));
        }

        #region Utilitaires pour DataGridView DateTimePicker (Version 0.5.0.1 - CORRIGÉE)

        /// <summary>
        /// Classe de colonne pour héberger des cellules de type DateTimePicker.
        /// </summary>
        public class DataGridViewDateTimePickerColumn : DataGridViewColumn
        {
            public DataGridViewDateTimePickerColumn() : base(new DataGridViewDateTimePickerCell()) { }

            public override DataGridViewCell CellTemplate
            {
                get { return base.CellTemplate; }
                set
                {
                    if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewDateTimePickerCell)))
                    {
                        throw new InvalidCastException("Must be a DataGridViewDateTimePickerCell");
                    }
                    base.CellTemplate = value;
                }
            }
        }

        /// <summary>
        /// Cellule personnalisée qui gère l'affichage et l'édition avec un DateTimePicker.
        /// </summary>
        public class DataGridViewDateTimePickerCell : DataGridViewTextBoxCell
        {
            public DataGridViewDateTimePickerCell()
            {
                // CORRECTION : Utiliser le nouveau format personnalisé sans l'année.
                this.Style.Format = FD;
            }

            public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
            {
                base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
                var ctl = (DataGridViewDateTimePickerEditingControl)this.DataGridView.EditingControl;

                // CORRECTION : Logique d'initialisation améliorée.
                ctl.ShowCheckBox = true;
                if (this.Value == null || this.Value == DBNull.Value)
                {
                    // La cellule est vide, on cherche la date planifiée comme valeur par défaut.
                    var item = this.DataGridView.Rows[rowIndex].Tag as TaskListItem;
                    DateTime? plannedDate = null;

                    // On détermine si on édite la date de début ou de fin pour prendre la bonne date planifiée.
                    if (this.OwningColumn.Name == "DG_DDR")
                    {
                        plannedDate = item?.DateDebutPlanifiee;
                    }
                    else if (this.OwningColumn.Name == "DG_DFR")
                    {
                        plannedDate = item?.DateFinPlanifiee;
                    }

                    // Si une date planifiée existe, on l'utilise, sinon on prend maintenant.
                    ctl.Value = plannedDate ?? DateTime.Now;
                    ctl.Checked = false; // La case est décochée car la valeur n'est pas encore "réelle".
                }
                else
                {
                    // La cellule a déjà une valeur, on l'utilise.
                    ctl.Value = (DateTime)this.Value;
                    ctl.Checked = true;
                }
            }

            // Le reste de la classe (DetachEditingControl, EditType, etc.) reste inchangé.
            public override void DetachEditingControl()
            {
                var ctl = this.DataGridView.EditingControl as DataGridViewDateTimePickerEditingControl;
                if (ctl != null && ctl.Visible)
                {
                    if (ctl.Checked == false)
                    {
                        this.Value = null;
                    }
                    else
                    {
                        this.Value = ctl.Value;
                    }
                }
                base.DetachEditingControl();
            }
            public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, System.ComponentModel.TypeConverter formattedValueTypeConverter, System.ComponentModel.TypeConverter valueTypeConverter)
            {
                // On retourne directement la valeur du contrôle d'édition si il existe.
                if (this.DataGridView.EditingControl is DataGridViewDateTimePickerEditingControl ctl)
                {
                    return ctl.Checked ? ctl.Value : (DateTime?)null;
                }

                // Sinon, on laisse la classe de base tenter sa chance (utile pour le collage de données).
                return base.ParseFormattedValue(formattedValue, cellStyle, formattedValueTypeConverter, valueTypeConverter);
            }
            public override Type EditType => typeof(DataGridViewDateTimePickerEditingControl);
            public override Type ValueType => typeof(DateTime?);
            public override object DefaultNewRowValue => null;
        }

        /// <summary>
        /// Le contrôle DateTimePicker qui est réellement affiché dans la cellule en mode édition.
        /// </summary>
        public class DataGridViewDateTimePickerEditingControl : DateTimePicker, IDataGridViewEditingControl
        {
            // CORRECTION : Implémentation complète et correcte de l'interface.
            public DataGridView EditingControlDataGridView { get; set; }
            public int EditingControlRowIndex { get; set; }
            public bool EditingControlValueChanged { get; set; }

            public object EditingControlFormattedValue
            {
                // CORRECTION : Appliquer le nouveau format.
                get { return this.Value.ToString(FD); }
                set
                {
                    if (value is string s && DateTime.TryParse(s, out var dt))
                    {
                        this.Value = dt;
                    }
                }
            }

            public DataGridViewDateTimePickerEditingControl()
            {
                this.Format = DateTimePickerFormat.Custom;
                // CORRECTION : Appliquer le nouveau format.
                this.CustomFormat = FD;
            }

            public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) => EditingControlFormattedValue;

            public void ApplyCellStyleToEditingControl(DataGridViewCellStyle style)
            {
                this.Font = style.Font;
                this.CalendarForeColor = style.ForeColor;
                this.CalendarMonthBackground = style.BackColor;
            }

            public void PrepareEditingControlForEdit(bool selectAll) { /* Pas d'action */ }

            public bool RepositionEditingControlOnValueChange => false;

            public bool EditingControlWantsInputKey(Keys key, bool wantsInputKey)
            {
                switch (key & Keys.KeyCode)
                {
                    case Keys.Left:
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Right:
                    case Keys.Home:
                    case Keys.End:
                    case Keys.PageDown:
                    case Keys.PageUp:
                        return true;
                    default:
                        return !wantsInputKey;
                }
            }

            public Cursor EditingPanelCursor => base.Cursor;

            protected override void OnValueChanged(EventArgs eventargs)
            {
                this.EditingControlValueChanged = true;
                this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
                base.OnValueChanged(eventargs);
            }
        }

        #endregion
    }
}