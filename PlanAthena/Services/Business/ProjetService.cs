// Fichier: PlanAthena/Services/Business/ProjetService.cs
// Version: 0.4.4 (Refactorisation Finale)
using PlanAthena.Data;
using PlanAthena.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Services.Business
{
    public class ProjetService
    {
        private readonly IIdGeneratorService _idGenerator;
        private readonly Dictionary<string, Lot> _lots = new();
        private readonly Dictionary<string, Tache> _taches = new();

        public ProjetService(IIdGeneratorService idGenerator)
        {
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        #region Cycle de vie du projet

        public void InitialiserNouveauProjet()
        {
            ViderProjet();
            CreerLot("Lot Principal", 50, ChantierPhase.SecondOeuvre);
        }

        public void ChargerProjet(ProjetData projetData)
        {
            ViderProjet();
            if (projetData == null) return;

            projetData.Lots?.ForEach(lot => _lots.TryAdd(lot.LotId, lot));
            projetData.Taches?.ForEach(tache => _taches.TryAdd(tache.TacheId, tache));

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

        public ProjetData GetProjetDataPourSauvegarde()
        {
            return new ProjetData
            {
                Lots = this.ObtenirTousLesLots(),
                Taches = this.ObtenirToutesLesTaches(),
            };
        }

        public void ViderProjet()
        {
            _lots.Clear();
            _taches.Clear();
        }

        public void ViderLot(string lotId)
        {
            if (string.IsNullOrEmpty(lotId)) return;
            var tachesASupprimer = ObtenirTachesParLot(lotId);
            foreach (var tache in tachesASupprimer)
            {
                _taches.Remove(tache.TacheId);
            }
            var lot = ObtenirLotParId(lotId);
            if (lot != null)
            {
                lot.Blocs.Clear();
            }
        }

        private static string ExtraireLotIdDepuisBlocId(string blocId)
        {
            if (string.IsNullOrEmpty(blocId) || !blocId.StartsWith("L") || blocId.Length < 4) return null;
            return blocId.Substring(0, 4);
        }

        #endregion

        #region Gestion des Lots

        public Lot CreerLot(string nom = "Nouveau Lot", int priorite = 99, ChantierPhase phases = ChantierPhase.SecondOeuvre)
        {
            if (string.IsNullOrWhiteSpace(nom)) throw new ArgumentException("Le nom du lot ne peut pas être vide.", nameof(nom));
            var nouveauLot = new Lot
            {
                LotId = _idGenerator.GenererProchainLotId(_lots.Values.ToList()),
                Nom = nom,
                Priorite = priorite,
                Phases = phases
            };
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
            if (ObtenirTachesParLot(lotId).Any())
                throw new InvalidOperationException("Impossible de supprimer ce lot car il est utilisé par au moins une tâche.");
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
            var nouveauBloc = new Bloc
            {
                LotId = lotIdParent,
                BlocId = _idGenerator.GenererProchainBlocId(lotIdParent, tousLesBlocs),
                Nom = nom,
                CapaciteMaxOuvriers = capacite
            };
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
            if (ObtenirTachesParBloc(blocId).Any())
                throw new InvalidOperationException("Impossible de supprimer ce bloc car il est utilisé par au moins une tâche.");
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

        #region Gestion des Tâches

        public Tache CreerTache(string lotId, string blocId, string nom = "Nouvelle Tâche", int heures = 8)
        {
            var nouvelleTache = new Tache
            {
                LotId = lotId,
                BlocId = blocId,
                TacheId = _idGenerator.GenererProchainTacheId(blocId, _taches.Values.ToList()),
                TacheNom = nom,
                HeuresHommeEstimees = heures
            };
            _taches.Add(nouvelleTache.TacheId, nouvelleTache);
            return nouvelleTache;
        }

        public void ModifierTache(Tache tacheModifiee)
        {
            if (tacheModifiee == null) throw new ArgumentNullException(nameof(tacheModifiee));
            if (!_taches.ContainsKey(tacheModifiee.TacheId)) throw new InvalidOperationException($"La tâche '{tacheModifiee.TacheNom}' n'a pas été trouvée.");
            _taches[tacheModifiee.TacheId] = tacheModifiee;
        }

        public void SupprimerTache(string tacheId)
        {
            if (_taches.Values.Any(t => (t.Dependencies ?? "").Split(',').Select(d => d.Trim()).Contains(tacheId)))
                throw new InvalidOperationException("Impossible de supprimer cette tâche car d'autres tâches en dépendent.");
            _taches.Remove(tacheId);
        }

        public List<Tache> ObtenirToutesLesTaches() => _taches.Values.ToList();
        public Tache ObtenirTacheParId(string tacheId) => _taches.GetValueOrDefault(tacheId);
        public List<Tache> ObtenirTachesParLot(string lotId) => _taches.Values.Where(t => t.LotId == lotId).ToList();
        public List<Tache> ObtenirTachesParBloc(string blocId) => _taches.Values.Where(t => t.BlocId == blocId).ToList();
        public virtual List<Tache> ObtenirTachesParMetier(string metierId) => _taches.Values.Where(t => t.MetierId == metierId).ToList();

        #endregion
    }
}