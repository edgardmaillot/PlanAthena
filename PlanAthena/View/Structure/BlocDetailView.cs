using Krypton.Toolkit;
using PlanAthena.Data;
using System;
using System.Windows.Forms;

namespace PlanAthena.View.Structure
{
    public partial class BlocDetailView : UserControl
    {
        private Bloc _currentBloc;
        private bool _isLoading;

        // Événement pour notifier le parent qu'une modification a eu lieu
        public event EventHandler BlocChanged;

        public BlocDetailView()
        {
            InitializeComponent();
            this.Load += BlocDetailView_Load;
        }

        private void BlocDetailView_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            AttachEvents();
            Clear();
        }

        private void AttachEvents()
        {
            textName.TextChanged += OnDetailChanged;
            numCapacity.ValueChanged += OnDetailChanged;
        }

        /// <summary>
        /// Charge les informations d'un bloc dans le formulaire.
        /// </summary>
        public void LoadBloc(Bloc bloc)
        {
            _isLoading = true;
            _currentBloc = bloc;

            if (bloc != null)
            {
                textId.Text = bloc.BlocId;
                textName.Text = bloc.Nom;
                numCapacity.Value = bloc.CapaciteMaxOuvriers;
                // Les champs X et Y restent vides et désactivés
                this.Enabled = true;
            }
            else
            {
                Clear();
            }
            _isLoading = false;
        }

        /// <summary>
        /// Vide et désactive le formulaire.
        /// </summary>
        public void Clear()
        {
            _isLoading = true;
            _currentBloc = null;
            textId.Clear();
            textName.Clear();
            numCapacity.Value = 1;
            textLocationX.Clear();
            textLocationY.Clear();
            this.Enabled = false;
            _isLoading = false;
        }

        private void OnDetailChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentBloc == null) return;

            // Mettre à jour l'objet Bloc en mémoire
            _currentBloc.Nom = textName.Text;
            _currentBloc.CapaciteMaxOuvriers = (int)numCapacity.Value;

            // Lever l'événement pour notifier le parent (sauvegarde automatique)
            BlocChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}