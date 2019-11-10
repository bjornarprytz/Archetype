using System;

namespace Archetype
{
    public abstract class ChooseTargets : ActionPrompt
    {
        protected Faction _allowedFactions;
        public ChooseTargets(int x, Faction allowedFactions) : base(x)
        {
            _allowedFactions = allowedFactions;
        }

        public ChooseTargets(int min, int max, Faction allowedFactions) : base(min, max)
        {
            _allowedFactions = allowedFactions;
        }

        public override bool MeetsRequirements(object piece)
        {
            if (!base.MeetsRequirements(piece)) return false;
            if (!_allowedFactions.HasFlag(((GamePiece)piece).Team)) return false;

            return true;
        }
    }

    public class ChooseTargets<T> : ChooseTargets  where T : GamePiece
    {
        public override Type RequiredType => typeof(T);
        
        public ChooseTargets(int x, Faction allowedFactions)
            : base(x, allowedFactions)
        {
        }

        public ChooseTargets(int min, int max, Faction allowedFactions)
            : base(min, max, allowedFactions)
        {
        }
    }
}
