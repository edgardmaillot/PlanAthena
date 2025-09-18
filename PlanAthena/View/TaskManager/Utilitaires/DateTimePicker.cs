// Emplacement: PlanAthena/View/TaskManager/Utilitaires/DateTimePicker.cs Version 0.6.1

using PlanAthena.Services.DTOs.Projet;

namespace PlanAthena.View.TaskManager.Utilitaires
{
    public partial class DateTimePicker : UserControl
    {
        public event EventHandler<DateTime?> DateTimeSelected;
        public event EventHandler SelectionCancelled;

        public DateTimePicker()
        {
            InitializeComponent();
            AttachEvents();
        }

        private void AttachEvents()
        {
            kbValide.Click += KbValide_Click;

            // --- AJOUT ---
            // Câbler le nouveau bouton "Annuler"
            kbAnnule.Click += (sender, e) => SelectionCancelled?.Invoke(this, EventArgs.Empty);
        }

        public void InitializeData(DateTime? initialDate, InformationsProjet projetInfo)
        {
            // ... (Cette méthode reste inchangée, elle est déjà parfaite) ...
            PopulateHeuresComboBox(projetInfo);

            if (initialDate.HasValue)
            {
                kCalendrier.SelectionStart = initialDate.Value.Date;
                kCalendrier.SelectionEnd = initialDate.Value.Date;
                kCalendrier.SetDate(initialDate.Value.Date);
                string heureToSelect = initialDate.Value.ToString("HH:00");
                if (kCmbHeure.Items.Contains(heureToSelect))
                {
                    kCmbHeure.SelectedItem = heureToSelect;
                }
                else if (kCmbHeure.Items.Count > 0)
                {
                    kCmbHeure.SelectedIndex = 0;
                }
            }
            else
            {
                kCalendrier.SelectionStart = DateTime.Today;
                kCalendrier.SelectionEnd = DateTime.Today;
                if (kCmbHeure.Items.Count > 0)
                {
                    kCmbHeure.SelectedIndex = 0;
                }
            }
        }

        private void PopulateHeuresComboBox(InformationsProjet projetInfo)
        {
            // ... (Cette méthode reste inchangée) ...
            kCmbHeure.Items.Clear();
            if (projetInfo == null) return;

            int startHour = projetInfo.HeureOuverture;
            int workingHours = projetInfo.HeuresTravailEffectifParJour;

            for (int i = 0; i < workingHours; i++)
            {
                int currentHour = startHour + i;
                if (currentHour < 24)
                {
                    kCmbHeure.Items.Add($"{currentHour:00}:00");
                }
            }
        }

        private void KbValide_Click(object sender, EventArgs e)
        {
            // ... (Cette méthode reste inchangée) ...
            DateTime selectedDate = kCalendrier.SelectionStart.Date;

            if (kCmbHeure.SelectedItem is string heureStr)
            {
                if (int.TryParse(heureStr.Split(':')[0], out int hour))
                {
                    DateTime result = selectedDate.AddHours(hour);
                    DateTimeSelected?.Invoke(this, result);
                    return;
                }
            }

            // Si aucune heure n'est sélectionnée, on considère que la validation est pour une date "vide"
            // Cela permet de décocher une date réelle en validant sans heure
            DateTimeSelected?.Invoke(this, null);
        }

    }
}