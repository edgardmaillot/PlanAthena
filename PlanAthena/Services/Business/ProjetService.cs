// /Services/Business/ProjetService.cs V0.4.8

using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business.DTOs;

namespace PlanAthena.Services.Business
{


    public class ProjetService
    {
        private readonly IIdGeneratorService _idGenerator;
        private readonly Dictionary<string, Lot> _lots = new();
        private InformationsProjet _informationsProjet;
        public ConfigurationPlanification ConfigPlanificationActuelle { get; private set; }



        public ProjetService(IIdGeneratorService idGenerator)
        {
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            // Initialiser la configuration de session avec des valeurs par défaut
            InitialiserConfigurationParDefaut();
        }

        #region Cycle de vie du projet
        private void InitialiserConfigurationParDefaut()
        {
            ConfigPlanificationActuelle = new ConfigurationPlanification
            {
                JoursOuvres = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                HeureDebutJournee = 8,
                DureeJournaliereStandardHeures = 8,
                HeuresTravailEffectifParJour = 7,
                CoutIndirectJournalierAbsolu = 500
            };
        }
        public virtual void InitialiserNouveauProjet()
        {
            //ViderProjet();
            _informationsProjet = new InformationsProjet { NomProjet = "Nouveau Projet" };
            CreerLot("Lot Principal", 50, ChantierPhase.SecondOeuvre);
        }

        public virtual void ChargerProjet(ProjetData projetData)
        {
            //ViderProjet();
            if (projetData == null) return;

            _informationsProjet = projetData.InformationsProjet ?? new InformationsProjet { NomProjet = "Projet sans nom" };
            projetData.Lots?.ForEach(lot => _lots.TryAdd(lot.LotId, lot));

            if (projetData.Blocs != null && projetData.Blocs.Any())
            {
                foreach (var bloc in projetData.Blocs)
                {
                    string lotIdParent = ExtraireLotIdDepuisBlocId(bloc.BlocId);
                    if (lotIdParent != null && _lots.TryGetValue(lotIdParent, out var lotParent))
                    {
                        bloc.LotId = lotIdParent;
                        if (!lotParent.Blocs.Any(b => b.BlocId == bloc.BlocId))
                        {
                            lotParent.Blocs.Add(bloc);
                        }
                    }
                }
            }
        }

        public virtual ProjetData GetProjetDataPourSauvegarde()
        {
            return new ProjetData
            {
                Lots = this.ObtenirTousLesLots(),
                InformationsProjet = this._informationsProjet
            };
        }

        public virtual void ViderProjet()
        {
            _lots.Clear();
            _informationsProjet = null;
        }

        // ViderLot a été retiré car sa logique de suppression des tâches est maintenant gérée ailleurs.

        private static string ExtraireLotIdDepuisBlocId(string blocId)
        {
            if (string.IsNullOrEmpty(blocId) || !blocId.StartsWith("L") || blocId.Length < 4) return null;
            return blocId.Substring(0, 4);
        }

        #endregion

        #region Accesseurs
        public InformationsProjet ObtenirInformationsProjet() => _informationsProjet;
        #endregion

        #region Gestion des Lots
        public Lot CreerLot(string nom = "Nouveau Lot", int priorite = 99, ChantierPhase phases = ChantierPhase.SecondOeuvre)
        {
            if (string.IsNullOrWhiteSpace(nom)) throw new ArgumentException("Le nom du lot ne peut pas être vide.", nameof(nom));
            var nouveauLot = new Lot { LotId = _idGenerator.GenererProchainLotId(_lots.Values.ToList()), Nom = nom, Priorite = priorite, Phases = phases };
            _lots.Add(nouveauLot.LotId, nouveauLot);
            return nouveauLot;
        }

        public void ModifierLot(Lot lotModifie)
        {
            if (lotModifie == null) throw new ArgumentNullException(nameof(lotModifie));
            if (!_lots.ContainsKey(lotModifie.LotId)) throw new InvalidOperationException($"Le lot '{lotModifie.Nom}' n'a pas été trouvé.");
            _lots[lotModifie.LotId] = lotModifie;
        }

        public void SupprimerLot(string lotId)
        {
            _lots.Remove(lotId);
        }

        public List<Lot> ObtenirTousLesLots() => _lots.Values.OrderBy(l => l.Priorite).ThenBy(l => l.Nom).ToList();
        public Lot ObtenirLotParId(string lotId) => _lots.GetValueOrDefault(lotId);
        #endregion

        #region Gestion des Blocs
        public Bloc CreerBloc(string lotIdParent, string nom = "Nouveau Bloc", int capacite = 1)
        {
            var lotParent = ObtenirLotParId(lotIdParent);
            if (lotParent == null) throw new InvalidOperationException($"Lot parent '{lotIdParent}' non trouvé.");
            var tousLesBlocs = _lots.Values.SelectMany(l => l.Blocs).ToList();
            var nouveauBloc = new Bloc { LotId = lotIdParent, BlocId = _idGenerator.GenererProchainBlocId(lotIdParent, tousLesBlocs), Nom = nom, CapaciteMaxOuvriers = capacite };
            lotParent.Blocs.Add(nouveauBloc);
            return nouveauBloc;
        }

        public void ModifierBloc(Bloc blocModifie)
        {
            if (blocModifie == null) throw new ArgumentNullException(nameof(blocModifie));
            var lotParent = ObtenirLotParId(blocModifie.LotId);
            if (lotParent == null) throw new InvalidOperationException("Lot parent non trouvé pour ce bloc.");
            var blocIndex = lotParent.Blocs.FindIndex(b => b.BlocId == blocModifie.BlocId);
            if (blocIndex == -1) throw new InvalidOperationException($"Le bloc '{blocModifie.Nom}' n'a pas été trouvé.");
            lotParent.Blocs[blocIndex] = blocModifie;
        }

        public void SupprimerBloc(string blocId)
        {
            foreach (var lot in _lots.Values)
            {
                lot.Blocs.RemoveAll(b => b.BlocId == blocId);
            }
        }

        public List<Bloc> ObtenirBlocsParLot(string lotId)
        {
            var lot = ObtenirLotParId(lotId);
            return lot?.Blocs.OrderBy(b => b.Nom).ToList() ?? new List<Bloc>();
        }

        public List<Bloc> ObtenirTousLesBlocs() => _lots.Values.SelectMany(l => l.Blocs).OrderBy(b => b.Nom).ToList();
        public Bloc ObtenirBlocParId(string blocId) => _lots.Values.SelectMany(l => l.Blocs).FirstOrDefault(b => b.BlocId == blocId);
        #endregion
    }
}