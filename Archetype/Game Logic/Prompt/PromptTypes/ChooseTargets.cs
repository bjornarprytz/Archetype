using System;

namespace Archetype
{
    public abstract class ChooseTargets : ActionPrompt
    {
        
        public ChooseTargets(int x)
            : base(x)
        {
        }

        public ChooseTargets(int min, int max)
            : base(min, max)
        {
        }
    }

    public class ChooseTargets<T> : ChooseTargets where T : GamePiece
    {
        public Predicate<T> MeetsRequirements { get; private set; }

        public override Type RequiredType => typeof(T);

        public ChooseTargets(int x, Predicate<T> targetRequirements)
            : base(x)
        {
            MeetsRequirements = targetRequirements;
        }

        public ChooseTargets(int min, int max, Predicate<T> targetRequirements)
            : base(min, max)
        {
            MeetsRequirements = targetRequirements;
        }
    }
}
