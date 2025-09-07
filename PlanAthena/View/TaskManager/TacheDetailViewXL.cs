using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace PlanAthena.View.TaskManager
{
    public partial class TacheDetailViewXL : UserControl
    {
        private Tache _currentTache;
        private bool _isLoading;

        private ProjetService _projetService;
        private RessourceService _ressourceService;
        private TaskManagerService _taskManagerService;
        private TaskManagerView _parentView;


        public event EventHandler TacheSaved;
        public event EventHandler<Tache> TacheDeleteRequested;

        public TacheDetailViewXL()
        {
            InitializeComponent();
            AttachEvents();
            Clear();
        }

        public void InitializeServices(ProjetService projetService, RessourceService ressourceService, TaskManagerService taskManagerService, TaskManagerView parentView)
        {
            _projetService = projetService;
            _ressourceService = ressourceService;
            _taskManagerService = taskManagerService;
            _parentView = parentView; // Stocker la référence
        }

        private void AttachEvents()
        {
            btnSauvegarder.Click += BtnSauvegarder_Click;
            btnSupprimer.Click += BtnSupprimer_Click;
            cmbEtat.SelectedIndexChanged += OnDetailChanged;
            ChkOuvriersAffect.ItemCheck += OnDetailChanged;
            chkListDependances.ItemCheck += OnDetailChanged;
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (_currentTache != null)
            {
                TacheDeleteRequested?.Invoke(this, _currentTache);
            }
        }

        private void BtnSauvegarder_Click(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;
            ApplyChangesToTache();
            TacheSaved?.Invoke(this, EventArgs.Empty);
        }

        public void LoadTache(Tache tache)
        {
            _isLoading = true;
            _currentTache = tache;

            if (tache != null)
            {
                kryptonHeader1.Values.Heading = $"Édition : {tache.TacheNom}";
                kryptonHeader1.Values.Description = $"ID: {tache.TacheId}";
                txtTacheNom.Text = tache.TacheNom;
                numHeuresHomme.Value = tache.HeuresHommeEstimees;
                chkIsJalon.Checked = tache.EstJalon;

                PopulateComboBoxes();

                cmbBlocNom.SelectedValue = tache.BlocId ?? "";
                cmbMetier.SelectedValue = tache.MetierId ?? "";
                cmbEtat.SelectedItem = tache.Statut;

                heureDebut.Text = tache.DateDebutPlanifiee?.ToString("dd/MM/yy HH:mm") ?? "N/A";
                heureFin.Text = tache.DateFinPlanifiee?.ToString("dd/MM/yy HH:mm") ?? "N/A";

                UpdateStatutColor(tache.Statut);
                PopulateOuvriers(tache);
                LoadDependencies(tache);
                PopulateDebugInfo(tache);
                PopulateSousTaches(tache);

                this.Enabled = true;
            }
            else
            {
                Clear();
            }
            _isLoading = false;
        }

        /// <summary>
        /// (1) Met à jour la couleur du panneau en fonction du statut de la tâche.
        /// </summary>
        private void UpdateStatutColor(Statut statut)
        {
            // --- CORRECTION : Utilisation des bons noms de l'enum Statut ---
            switch (statut)
            {
                case Statut.Estimée:
                    StatutColor.StateCommon.Color1 = Color.LightGray;
                    break;
                case Statut.Planifiée:
                    StatutColor.StateCommon.Color1 = Color.Green;
                    break;
                case Statut.EnCours:
                    StatutColor.StateCommon.Color1 = Color.Orange;
                    break;
                case Statut.EnRetard:
                    StatutColor.StateCommon.Color1 = Color.Red;
                    break;
                case Statut.Terminée:
                    StatutColor.StateCommon.Color1 = Color.Black;
                    break;
                default:
                    StatutColor.StateCommon.Color1 = Color.Transparent;
                    break;
            }
        }

        /// <summary>
        /// (2) Remplit la liste des ouvriers et coche ceux qui sont affectés à la tâche.
        /// </summary>
        private void PopulateOuvriers(Tache tache)
        {
            ChkOuvriersAffect.Items.Clear();
            if (_ressourceService == null) return;

            // On n'a plus besoin de DisplayMember
            // ChkOuvriersAffect.DisplayMember = "Nom"; 

            var allOuvriers = _ressourceService.GetAllOuvriers();
            var ouvriersAffectesIds = new HashSet<string>(tache.Affectations.Select(a => a.OuvrierId));

            foreach (var ouvrier in allOuvriers.OrderBy(o => o.Nom))
            {
                // --- LA CORRECTION EST ICI ---

                // 1. On crée un KryptonListItem pour "envelopper" notre objet Ouvrier.
                var listItem = new KryptonListItem
                {
                    // On dit explicitement quoi afficher.
                    ShortText = ouvrier.Nom,

                    // On stocke l'objet Ouvrier complet dans le Tag pour le retrouver plus tard.
                    Tag = ouvrier
                };

                // 2. On ajoute le KryptonListItem à la liste.
                int index = ChkOuvriersAffect.Items.Add(listItem);

                // 3. On coche la case si nécessaire.
                if (ouvriersAffectesIds.Contains(ouvrier.OuvrierId))
                {
                    ChkOuvriersAffect.SetItemChecked(index, true);
                }
            }
        }

        /// <summary>
        /// (3) Remplit la liste des dépendances possibles pour la tâche en déléguant au parent.
        /// </summary>
        private void LoadDependencies(Tache tache)
        {
            chkListDependances.Items.Clear();

            // On délègue le calcul au parent
            if (_parentView != null && tache != null)
            {
                var items = _parentView.GetDependancesForTache(tache);
                foreach (var displayItem in items)
                {
                    bool isChecked = displayItem.OriginalData.Etat == EtatDependance.Stricte ||
                                     displayItem.OriginalData.Etat == EtatDependance.Suggeree;

                    int index = chkListDependances.Items.Add(displayItem);
                    chkListDependances.SetItemChecked(index, isChecked);
                }
            }
        }

        /// <summary>
        /// (4) Affiche les propriétés brutes de la tâche dans le RichTextBox pour le débogage.
        /// </summary>
        private void PopulateDebugInfo(Tache tache)
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- Propriétés Brutes de la Tâche ---");

            foreach (var prop in tache.GetType().GetProperties())
            {
                try
                {
                    object value = prop.GetValue(tache, null);
                    string valueStr;

                    // --- CORRECTION : Condition spéciale pour la liste d'affectations ---
                    if (prop.Name == "Affectations" && value is List<AffectationOuvrier> affectations)
                    {
                        if (affectations.Any())
                        {
                            // On formate la liste pour qu'elle soit lisible
                            var affectationDetails = affectations.Select(a => $"({a.OuvrierId}: {a.NomOuvrier}, {a.HeuresTravaillees}h)");
                            valueStr = string.Join("; ", affectationDetails);
                        }
                        else
                        {
                            valueStr = "(Liste vide)";
                        }
                    }
                    // --- Fin de la correction ---
                    else
                    {
                        // Comportement normal pour toutes les autres propriétés
                        valueStr = value?.ToString() ?? "null";
                        if (string.IsNullOrEmpty(valueStr)) valueStr = "\"\"";
                    }

                    sb.AppendLine($"{prop.Name} | {prop.PropertyType.Name} | {valueStr}");
                }
                catch { /* Ignorer les erreurs d'accès */ }
            }
            kryptonRichTextBox1.Text = sb.ToString();
        }

        /// <summary>
        /// (5) Si la tâche est un conteneur, affiche ses sous-tâches dans la grille.
        /// </summary>
        private void PopulateSousTaches(Tache tache)
        {
            DataGridSousTaches.Rows.Clear();
            DataGridSousTaches.Visible = tache.EstConteneur;

            if (!tache.EstConteneur || _taskManagerService == null) return;

            var sousTaches = _taskManagerService.ObtenirToutesLesTaches()
                                               .Where(t => t.ParentId == tache.TacheId)
                                               .ToList();

            // Si la tâche est un conteneur mais qu'aucune sous-tâche n'est trouvée,
            // affichons une ligne pour le signaler.
            if (!sousTaches.Any())
            {
                DataGridSousTaches.Rows.Add(null, "(Aucune sous-tâche trouvée)", "N/A");
                return;
            }

            // Récupérer tous les ouvriers une seule fois pour la performance
            var allOuvriers = _ressourceService.GetAllOuvriers().ToDictionary(o => o.OuvrierId);

            foreach (var sousTache in sousTaches)
            {
                string ouvrierDisplay = "N/A";
                if (sousTache.Affectations.Any())
                {
                    // On cherche le nom de l'ouvrier à partir de son ID
                    var nomsOuvriers = sousTache.Affectations
                        .Select(a => allOuvriers.TryGetValue(a.OuvrierId, out var ouvrier) ? ouvrier.Nom : a.OuvrierId)
                        .ToList();
                    ouvrierDisplay = string.Join(", ", nomsOuvriers);
                }

                DataGridSousTaches.Rows.Add(sousTache.TacheId, sousTache.TacheNom, ouvrierDisplay);
            }
        }


        /// Méthode originale 
        /*
        private void PopulateSousTaches(Tache tache)
        {
            DataGridSousTaches.Rows.Clear();
            DataGridSousTaches.Visible = tache.EstConteneur;

            if (!tache.EstConteneur || _taskManagerService == null) return;

            // --- CORRECTION : Utilisation d'un filtre Linq au lieu d'un paramètre 'parentId' ---
            var sousTaches = _taskManagerService.ObtenirToutesLesTaches()
                                               .Where(t => t.ParentId == tache.TacheId)
                                               .ToList();

            foreach (var sousTache in sousTaches)
            {
                // Pour l'ouvrier, il faudrait faire une jointure avec le service ressource pour avoir le nom.
                // Pour l'instant, on affiche l'ID comme demandé.
                var ouvrierDisplay = sousTache.Affectations.Any()
                                   ? string.Join(", ", sousTache.Affectations.Select(a => a.OuvrierId))
                                   : "N/A";

                DataGridSousTaches.Rows.Add(sousTache.TacheId, sousTache.TacheNom, ouvrierDisplay);
            }
        }
        */
        public void Clear()
        {
            _isLoading = true;
            _currentTache = null;

            kryptonHeader1.Values.Heading = "Détail de la tâche";
            kryptonHeader1.Values.Description = "Aucune tâche sélectionnée";
            txtTacheNom.Clear();
            numHeuresHomme.Value = 0;
            chkIsJalon.Checked = false;
            cmbBlocNom.DataSource = null;
            cmbMetier.DataSource = null;
            cmbEtat.DataSource = null;
            heureDebut.Clear();
            heureFin.Clear();
            ChkOuvriersAffect.Items.Clear();
            chkListDependances.Items.Clear();
            kryptonRichTextBox1.Clear();
            DataGridSousTaches.Rows.Clear();
            DataGridSousTaches.Visible = false;
            UpdateStatutColor(Statut.Estimée); // Utilise une valeur par défaut de l'enum

            _isLoading = false;
        }

        private void PopulateComboBoxes()
        {
            if (_projetService == null || _ressourceService == null) return;

            var lots = _projetService.ObtenirTousLesLots();
            var allBlocs = lots.SelectMany(l => l.Blocs).OrderBy(b => b.Nom).ToList();
            cmbBlocNom.DataSource = allBlocs;
            cmbBlocNom.DisplayMember = "Nom";
            cmbBlocNom.ValueMember = "BlocId";

            var allMetiers = _ressourceService.GetAllMetiers().OrderBy(m => m.Nom).ToList();
            cmbMetier.DataSource = allMetiers;
            cmbMetier.DisplayMember = "Nom";
            cmbMetier.ValueMember = "MetierId";

            cmbEtat.DataSource = Enum.GetValues(typeof(Statut));
        }

        private void OnDetailChanged(object sender, EventArgs e)
        {
            if (_isLoading || _currentTache == null) return;
            ApplyChangesToTache();

            if (sender == cmbEtat && cmbEtat.SelectedItem is Statut newStatut)
            {
                UpdateStatutColor(newStatut);
            }
        }

        private void ApplyChangesToTache()
        {
            if (_currentTache == null) return;

            _currentTache.TacheNom = txtTacheNom.Text;
            _currentTache.HeuresHommeEstimees = (int)numHeuresHomme.Value;
            _currentTache.Type = chkIsJalon.Checked ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;
            _currentTache.BlocId = cmbBlocNom.SelectedValue as string ?? "";
            _currentTache.MetierId = cmbMetier.SelectedValue as string ?? "";

            if (cmbEtat.SelectedItem is Statut statut)
            {
                _currentTache.Statut = statut;
            }

            // Mettre à jour les affectations
            _currentTache.Affectations.Clear();
            foreach (var item in ChkOuvriersAffect.CheckedItems)
            {
                // --- CORRECTION ---
                // On récupère le KryptonListItem, puis on accède à son Tag qui contient l'Ouvrier.
                if (item is KryptonListItem listItem && listItem.Tag is Ouvrier ouvrier)
                {
                    _currentTache.Affectations.Add(new AffectationOuvrier
                    {
                        OuvrierId = ouvrier.OuvrierId,
                        NomOuvrier = ouvrier.Nom
                    });
                }
            }

            // Mettre à jour les dépendances
            var dependancesStricts = new List<string>();
            var exclusions = new List<string>();

            for (int i = 0; i < chkListDependances.Items.Count; i++)
            {
                if (chkListDependances.Items[i] is DependanceDisplayItem item)
                {
                    bool estCochee = chkListDependances.GetItemChecked(i);
                    var tacheIdPredecesseur = item.OriginalData.TachePredecesseur.TacheId;

                    if (!item.OriginalData.EstHeritee && estCochee) dependancesStricts.Add(tacheIdPredecesseur);
                    else if (item.OriginalData.EstHeritee && !estCochee) exclusions.Add(tacheIdPredecesseur);
                }
            }
            _currentTache.Dependencies = string.Join(",", dependancesStricts.Distinct());
            _currentTache.ExclusionsDependances = string.Join(",", exclusions.Distinct());
        }
    }
}