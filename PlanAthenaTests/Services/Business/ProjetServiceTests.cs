// Fichier: PlanAthenaTests/Services/TestDoubles/FakeIdGeneratorService.cs
// Description: Implémentation prédictible de IIdGeneratorService pour les tests unitaires.
// Version CORRIGÉE pour correspondre à IIdGeneratorService.

using PlanAthena.Data;
using PlanAthena.Interfaces;

namespace PlanAthenaTests.Services.TestDoubles
{
    public class FakeIdGeneratorService : IIdGeneratorService
    {
        private int _lotCounter = 1;
        private int _blocCounter = 1;
        private int _tacheCounter = 1;
        private int _jalonCounter = 1;
        private int _metierCounter = 1;
        private int _ouvrierCounter = 1;

        public string GenererProchainLotId(IReadOnlyList<Lot> lotsExistants) => $"L{_lotCounter++:D3}";

        public string GenererProchainBlocId(string lotIdParent, IReadOnlyList<Bloc> blocsExistants) => $"{lotIdParent}_B{_blocCounter++:D3}";

        public string GenererProchainTacheId(string blocIdParent, IReadOnlyList<Tache> tachesExistantes, TypeActivite type = TypeActivite.Tache)
        {
            if (type == TypeActivite.Tache)
            {
                return $"{blocIdParent}_T{_tacheCounter++:D3}";
            }
            // Pour tous les autres types (jalons)
            return $"{blocIdParent}_J{_jalonCounter++:D3}";
        }

        public string GenererProchainMetierId(IReadOnlyList<Metier> metiersExistants) => $"M{_metierCounter++:D3}";

        public string GenererProchainOuvrierId(IReadOnlyList<Ouvrier> ouvriersExistants) => $"W{_ouvrierCounter++:D3}"; // Note: Le service réel utilise 'W', j'adapte.

        public string NormaliserIdDepuisCsv(string idOriginal, string blocIdCible, IReadOnlyList<Tache> tachesExistantes, TypeActivite type = TypeActivite.Tache)
        {
            // Pour les tests, on génère simplement un nouvel ID prédictible.
            return GenererProchainTacheId(blocIdCible, tachesExistantes, type);
        }
    }
}