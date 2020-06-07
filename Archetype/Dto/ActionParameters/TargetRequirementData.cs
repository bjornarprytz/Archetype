
namespace Archetype
{
    public class TargetRequirementData
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = int.MaxValue;
        public TargetPredicateData Predicate { get; set; }

        public SelectionMethod SelectionMethod { get; set; }


        internal TargetInfo GetTargetInfo(Unit source, GameState gameState)
        {
            return new TargetInfo(Min, Max, Predicate.GetOptions(source, gameState), SelectionMethod);
        }

    }
}
