
using System;

namespace Archetype
{
    public class TargetRequirementData : ITargetSelectionInfoFactory
    {
        public TargetPredicateData Predicate { get; set; } = new NoTargetPredicateData();
        public TargetSelectionData Selection { get; set; }

        public ISelectionInfo<ITarget> GetSelectionInfo(Unit source, GameState gameState)
        {
            return Selection.GetSelectionInfo(Predicate.GetOptions(source, gameState));
        }
    }
}
