using PlanAthena.Data;
using PlanAthena.Services.Business;


namespace PlanAthena.Services.Processing
{
    public class DependanceService
    {
        private readonly MetierService _metierService;

        public DependanceService(MetierService metierService)
        {
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
        }

        /// <summary>
        /// Calcule les dépendances métier et les ajoute aux dépendances existantes des tâches.
        /// Ne modifie pas la liste originale, retourne une nouvelle liste enrichie.
        /// </summary>
        public List<TacheRecord> CalculerDependancesMetier(IReadOnlyList<TacheRecord> tachesBrutes)
        {
            if (tachesBrutes == null || !tachesBrutes.Any())
                return new List<TacheRecord>();

            var tachesResultat = tachesBrutes.Select(CopierTache).ToList();
            var tachesParBloc = tachesResultat.GroupBy(t => t.BlocId);

            foreach (var groupeBloc in tachesParBloc)
            {
                var tachesDuBloc = groupeBloc.ToList();
                foreach (var tacheCourante in tachesDuBloc)
                {
                    var prerequisMetier = _metierService.GetPrerequisForMetier(tacheCourante.MetierId);
                    if (!prerequisMetier.Any()) continue;

                    // Trouver toutes les tâches du bloc qui correspondent aux métiers prérequis
                    var tachesPrecedentesIds = tachesDuBloc
                        .Where(t => prerequisMetier.Contains(t.MetierId))
                        .Select(t => t.TacheId)
                        .ToList();

                    if (tachesPrecedentesIds.Any())
                    {
                        var dependancesExistantes = tacheCourante.Dependencies?
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .ToList() ?? new List<string>();

                        dependancesExistantes.AddRange(tachesPrecedentesIds);
                        tacheCourante.Dependencies = string.Join(",", dependancesExistantes.Distinct());
                    }
                }
            }
            return tachesResultat;
        }

        private TacheRecord CopierTache(TacheRecord source)
        {
            return new TacheRecord
            {
                TacheId = source.TacheId,
                TacheNom = source.TacheNom,
                HeuresHommeEstimees = source.HeuresHommeEstimees,
                MetierId = source.MetierId,
                Dependencies = source.Dependencies,
                LotId = source.LotId,
                LotNom = source.LotNom,
                LotPriorite = source.LotPriorite,
                BlocId = source.BlocId,
                BlocNom = source.BlocNom,
                BlocCapaciteMaxOuvriers = source.BlocCapaciteMaxOuvriers
            };
        }
    }
}