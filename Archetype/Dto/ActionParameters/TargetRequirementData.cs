
using System;

namespace Archetype
{
    public class TargetRequirementData : ITargetSelectInfoFactory
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = int.MaxValue;
        public TargetPredicateData Predicate { get; set; } = new NoTargetPredicateData();

        public SelectionMethod SelectionMethod { get; set; }

        public ITargetSelectInfo GetTargetInfo(Unit source, GameState gameState)
        {
            switch (SelectionMethod)
            {
                case SelectionMethod.All:
                    return new AllSelectionInfo(Predicate.GetOptions(source, gameState));
                case SelectionMethod.Any:
                    return new AnySelectionInfo(Min, Max, Predicate.GetOptions(source, gameState));
                case SelectionMethod.Random:
                    return new RandomSelectionInfo(Min, Predicate.GetOptions(source, gameState));
                case SelectionMethod.Self:
                    return new SelfSelectionInfo(source);
                case SelectionMethod.None:
                    return new NoneSelectionInfo();
                default:
                    throw new Exception($"Unrecognized SelectionMethod: {SelectionMethod}");
            }
        }
    }
}
