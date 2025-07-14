// PlanAthena.Core.Domain.Chantier.cs
using PlanAthena.Core.Domain.ValueObjects;

namespace PlanAthena.Core.Domain
{
    public class Chantier : Entity<ChantierId>
    {
        public string Description { get; }
        public PeriodePlanification PeriodeSouhaitee { get; } // VO
        public CalendrierOuvreChantier Calendrier { get; } // VO
        public ConfigurationChantier? ConfigCdC { get; } // VO, optionnel

        private readonly Dictionary<MetierId, Metier> _metiers = new Dictionary<MetierId, Metier>();
        public IReadOnlyDictionary<MetierId, Metier> Metiers => _metiers;

        private readonly Dictionary<OuvrierId, Ouvrier> _ouvriers = new Dictionary<OuvrierId, Ouvrier>();
        public IReadOnlyDictionary<OuvrierId, Ouvrier> Ouvriers => _ouvriers;

        private readonly Dictionary<BlocId, BlocTravail> _blocs = new Dictionary<BlocId, BlocTravail>();
        public IReadOnlyDictionary<BlocId, BlocTravail> Blocs => _blocs;

        private readonly Dictionary<LotId, LotTravaux> _lots = new Dictionary<LotId, LotTravaux>();
        public IReadOnlyDictionary<LotId, LotTravaux> Lots => _lots;

        public ConfigurationOptimisation? ConfigurationOptimisation { get; private set; }

        // Les tâches sont accessibles via les Blocs: Chantier.Blocs[id].Taches
        // Pas de collection globale de tâches ici pour maintenir l'encapsulation Bloc -> Tâches.

        public Chantier(
            ChantierId id,
            string description,
            PeriodePlanification periodeSouhaitee,
            CalendrierOuvreChantier calendrier,
            IEnumerable<Metier> metiers,
            IEnumerable<Ouvrier> ouvriers,
            IEnumerable<BlocTravail> blocs, // Blocs avec leurs tâches déjà initialisées
            IEnumerable<LotTravaux> lots,
            ConfigurationChantier? configCdC = null)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("La description du chantier ne peut pas être vide.", nameof(description));

            Description = description;
            PeriodeSouhaitee = periodeSouhaitee ?? throw new ArgumentNullException(nameof(periodeSouhaitee));
            Calendrier = calendrier ?? throw new ArgumentNullException(nameof(calendrier));
            ConfigCdC = configCdC; // Peut être null

            // Initialisation des collections et validations de cohérence
            ArgumentNullException.ThrowIfNull(metiers);
            ArgumentNullException.ThrowIfNull(ouvriers);
            ArgumentNullException.ThrowIfNull(blocs);
            ArgumentNullException.ThrowIfNull(lots);

            // Métiers
            foreach (var metier in metiers)
            {
                if (_metiers.ContainsKey(metier.Id)) throw new InvalidOperationException($"Métier dupliqué : {metier.Id}");
                _metiers.Add(metier.Id, metier);
            }

            // Ouvriers (et validation de leurs compétences par rapport aux métiers définis)
            foreach (var ouvrier in ouvriers)
            {
                if (_ouvriers.ContainsKey(ouvrier.Id)) throw new InvalidOperationException($"Ouvrier dupliqué : {ouvrier.Id}");
                foreach (var compMetierId in ouvrier.Competences.Keys)
                {
                    if (!_metiers.ContainsKey(compMetierId))
                        throw new InvalidOperationException($"L'ouvrier '{ouvrier.Nom}' a une compétence pour un métier non défini : {compMetierId}");
                }
                _ouvriers.Add(ouvrier.Id, ouvrier);
            }

            // Blocs (et validation de leurs tâches par rapport aux métiers définis)
            var tousLesBlocIds = new HashSet<BlocId>();
            foreach (var bloc in blocs)
            {
                if (_blocs.ContainsKey(bloc.Id)) throw new InvalidOperationException($"Bloc dupliqué : {bloc.Id}");
                foreach (var tache in bloc.Taches.Values)
                {
                    if (!_metiers.ContainsKey(tache.MetierRequisId))
                        throw new InvalidOperationException($"La tâche '{tache.Nom}' du bloc '{bloc.Nom}' référence un métier non défini : {tache.MetierRequisId}");
                    // Valider que les dépendances de tâches existent au sein du même bloc
                    foreach (var depId in tache.Dependencies)
                    {
                        if (!bloc.Taches.ContainsKey(depId))
                            throw new InvalidOperationException($"La tâche '{tache.Nom}' a une dépendance '{depId}' non trouvée dans le même bloc '{bloc.Nom}'.");
                    }
                }
                _blocs.Add(bloc.Id, bloc);
                tousLesBlocIds.Add(bloc.Id);
            }

            // Lots (et validation de leurs BlocIds et assignation LotParentId aux Blocs)
            var blocIdsDansLesLots = new HashSet<BlocId>();
            foreach (var lot in lots)
            {
                if (_lots.ContainsKey(lot.Id)) throw new InvalidOperationException($"Lot dupliqué : {lot.Id}");
                foreach (var blocIdInLot in lot.BlocIds)
                {
                    if (!tousLesBlocIds.Contains(blocIdInLot))
                        throw new InvalidOperationException($"Le lot '{lot.Nom}' référence un BlocId non défini : {blocIdInLot}");
                    if (blocIdsDansLesLots.Contains(blocIdInLot))
                        throw new InvalidOperationException($"Le BlocId '{blocIdInLot}' est assigné à plusieurs lots. Un bloc ne peut appartenir qu'à un seul lot.");

                    blocIdsDansLesLots.Add(blocIdInLot);
                    _blocs[blocIdInLot].LotParentId = lot.Id; // Assigner le LotParentId au Bloc
                }
                _lots.Add(lot.Id, lot);
            }
            // Optionnel: Valider que tous les blocs sont dans un lot, si c'est une règle métier.
            // if (tousLesBlocIds.Count != blocIdsDansLesLots.Count)
            //    throw new InvalidOperationException("Certains blocs ne sont assignés à aucun lot.");


            // Valider les références dans ConfigCdC si présent
            if (ConfigCdC != null)
            {
                foreach (var ouvrierClefId in ConfigCdC.OuvriersClefs)
                    if (!_ouvriers.ContainsKey(ouvrierClefId)) throw new InvalidOperationException($"Ouvrier clef non trouvé : {ouvrierClefId}");
                foreach (var metierClefId in ConfigCdC.MetiersClefs)
                    if (!_metiers.ContainsKey(metierClefId)) throw new InvalidOperationException($"Métier clef non trouvé : {metierClefId}");
            }

            // Valider les dépendances de lots (que les LotId référencés existent)
            // (Notre LotTravauxDto n'a plus DependancesLotIds pour le MVP, donc cette validation est retirée)
        }

        // Méthodes de lecture / interrogation du domaine
        public IEnumerable<Tache> ObtenirToutesLesTaches() => _blocs.Values.SelectMany(b => b.Taches.Values);

        public DureeHeuresHomme CalculerChargeGlobaleHeuresHomme()
        {
            int totalHeures = 0;
            foreach (var bloc in _blocs.Values)
            {
                totalHeures += bloc.CalculerChargeTotalHeuresHomme().Value;
            }
            return new DureeHeuresHomme(totalHeures);
        }

        /// <summary>
        /// Applique une nouvelle configuration d'optimisation au chantier.
        /// POURQUOI : Respecte l'encapsulation de l'agrégat. La modification de l'état
        /// passe par une méthode métier explicite.
        /// </summary>
        /// <param name="config">La configuration à appliquer.</param>
        public void AppliquerConfigurationOptimisation(ConfigurationOptimisation config)
        {
            ArgumentNullException.ThrowIfNull(config);
            ConfigurationOptimisation = config;
        }
    }
}