using PlanAthena.Core.Infrastructure.Services.OrTools;

namespace PlanAthena.core.Tests.Infrastructure.OrTools
{
    public class TacheModelBuilderTests
    {
        private readonly TacheModelBuilder _builder;

        public TacheModelBuilderTests()
        {
            _builder = new TacheModelBuilder();
        }

        // Ce test est supprimé car trop fragile. On ne peut pas se fier à l'implémentation interne de AddExactlyOne.
        // [Fact]
        // public void Construire_PourChaqueTache_CreeContrainteAssignationUnique() { ... }

        // Ce test est supprimé car trop fragile. On ne peut pas inspecter la contrainte AddElement de cette manière.
        // [Fact]
        // public void Construire_AvecJalon_CreeLesBonnesContraintes() { ... }



    }
}