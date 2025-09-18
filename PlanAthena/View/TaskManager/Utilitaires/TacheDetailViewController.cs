// Fichier: PlanAthena/View/TaskManager/Utilitaires/TacheDetailViewController.cs
using PlanAthena.Data;
using PlanAthena.Services.Business;
using Krypton.Toolkit; // On a besoin de référencer les contrôles

namespace PlanAthena.View.TaskManager.Utilitaires
{
    public class TacheDetailViewController
    {
        private readonly TaskManagerService _taskManagerService;
        private bool _suppressPlanningWarning = false;

        public TacheDetailViewController(TaskManagerService taskManagerService)
        {
            _taskManagerService = taskManagerService;
        }

        // Règle 1: Déterminer si la tâche est en lecture seule
        public bool IsTacheReadOnly(Tache tache)
        {
            if (tache == null) return true; // Pas de tâche, tout est bloqué
            return tache.Statut == Statut.EnCours || tache.Statut == Statut.Terminée;
        }

        // Règle 2: Appliquer l'état ReadOnly à un ensemble de contrôles
        public void ApplyReadOnlyStateToControls(Control parentControl, bool isReadOnly, string[] exceptionsControls = null)
        {
            // Liste des contrôles qui doivent rester actifs (navigation)
            var exceptions = exceptionsControls ?? new[] { "cmbLots" };

            foreach (Control ctrl in parentControl.Controls)
            {
                // Vérifier si ce contrôle fait partie des exceptions
                bool isException = exceptions.Contains(ctrl.Name);

                if (ctrl is KryptonTextBox txt)
                    txt.ReadOnly = isReadOnly && !isException;
                else if (ctrl is KryptonNumericUpDown num)
                    num.Enabled = !isReadOnly || isException;
                else if (ctrl is KryptonComboBox cmb)
                    cmb.Enabled = !isReadOnly || isException;
                else if (ctrl is KryptonCheckBox chk)
                    chk.Enabled = !isReadOnly || isException;
                else if (ctrl is KryptonCheckedListBox chkList)
                    chkList.Enabled = !isReadOnly || isException;
                // Cas spécial pour les boutons
                else if (ctrl is KryptonButton btn && (btn.Name.Contains("Sauvegarder") || btn.Name.Contains("Supprimer")))
                {
                    btn.Enabled = !isReadOnly;
                }

                // Appel récursif si le contrôle a des enfants (ex: GroupBox, Panel)
                if (ctrl.Controls.Count > 0)
                {
                    ApplyReadOnlyStateToControls(ctrl, isReadOnly, exceptionsControls);
                }
            }
        }

        // Règle 3: Gérer la confirmation et l'invalidation du planning
        public bool ConfirmAndInvalidateIfNeeded(Tache tache)
        {
            if (tache == null) return false;

            bool needsWarning = tache.Statut == Statut.Planifiée || tache.Statut == Statut.EnRetard;

            if (!needsWarning) return true; // Pas besoin d'alerte, on peut continuer

            if (_suppressPlanningWarning)
            {
                _taskManagerService.InvaliderPlanification();
                return true;
            }

            var message = "Attention : Vous modifiez une tâche déjà planifiée.\n\n" +
                          "Cela va désynchroniser votre planning jusqu'au prochain calcul.\n\n" +
                          "Voulez-vous continuer ?";

            var result = MessageBox.Show(message, "Confirmation de modification",
                                         MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return false; // L'utilisateur a annulé
            }

            // L'utilisateur a confirmé
            var result2 = MessageBox.Show("Voulez-vous désactiver cette alerte pour le reste de la session ?",
                                          "Désactiver l'alerte", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result2 == DialogResult.Yes)
            {
                _suppressPlanningWarning = true;
            }

            _taskManagerService.InvaliderPlanification();
            return true;
        }
    }
}