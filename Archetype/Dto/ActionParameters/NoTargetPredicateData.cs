
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class NoTargetPredicateData : TargetPredicateData
    {
        public override IEnumerable<ITarget> GetOptions(Unit source, GameState gameState)
        {
            return Enumerable.Empty<ITarget>();
        }
    }
}
