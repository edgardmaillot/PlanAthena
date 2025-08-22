using Krypton.Toolkit;
using Krypton.Navigator;
using Krypton.Docking;
using Krypton.Workspace;
using PlanAthena.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager
{
    public partial class CreationToolboxView : UserControl
    {
        // Événements pour notifier le parent
        public event EventHandler AddBlocRequested;
        public event EventHandler<Metier> AddTacheRequested;

        public CreationToolboxView()
        {
            InitializeComponent();

            // On s'assure que les boutons dynamiques s'étirent sur toute la largeur
            panelDynamicButtons.Resize += PanelDynamicButtons_Resize;
        }

        /// <summary>
        /// Remplit la boîte à outils avec les boutons des métiers pertinents.
        /// </summary>
        public void PopulateMetiers(IEnumerable<Metier> metiers, Func<string, Color> colorProvider)
        {
            panelDynamicButtons.SuspendLayout();

            // Vider les anciens boutons
            foreach (Control ctrl in panelDynamicButtons.Controls)
            {
                ctrl.Dispose();
            }
            panelDynamicButtons.Controls.Clear();

            // Créer les nouveaux boutons
            if (metiers != null)
            {
                foreach (var metier in metiers)
                {
                    var btn = new KryptonColorButton
                    {
                        Text = metier.Nom,
                        Tag = metier,
                        AutoSize = true,
                        MinimumSize = new Size(150, 35),
                        MaximumSize = new Size(150, 35),
                        Margin = new Padding(0, 3, 0, 3)
                    };

                    // Appliquer la couleur fournie par le service
                    if (colorProvider != null)
                    {
                        //btn.StateCommon.Back.Color1 = colorProvider(metier.MetierId);
                        btn.SelectedColor = colorProvider(metier.MetierId);
                    }

                    // VÉRIFICATION CRUCIALE : L'événement est bien attaché ici
                    btn.Click += MetierButton_Click;

                    panelDynamicButtons.Controls.Add(btn);
                }
            }

            panelDynamicButtons.ResumeLayout();

            // Forcer le redimensionnement initial
            PanelDynamicButtons_Resize(panelDynamicButtons, EventArgs.Empty);
        }

        private void MetierButton_Click(object sender, EventArgs e)
        {
            if (sender is KryptonButton { Tag: Metier metier })
            {
                // Lever l'événement en passant le métier concerné
                AddTacheRequested?.Invoke(this, metier);
            }
        }

        private void btnAddBloc_Click(object sender, EventArgs e)
        {
            // Lever l'événement simple
            AddBlocRequested?.Invoke(this, EventArgs.Empty);
        }

        private void PanelDynamicButtons_Resize(object sender, EventArgs e)
        {
            foreach (Control control in panelDynamicButtons.Controls)
            {
                if (control is KryptonButton)
                {
                    control.Width = panelDynamicButtons.ClientSize.Width - control.Margin.Left - control.Margin.Right;
                }
            }
        }
    }
}