using System;

namespace Archetype
{
    public abstract class ChooseTargets : ActionPrompt
    {
        internal string TargetsText => $"{_numberOfTargets} {_targetsFaction} {_typeTargets}";

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

        private string _numberOfTargets => MaxChoices == MinChoices ? $"{MaxChoices}" : $"{MinChoices}-{MaxChoices}";
        private string _targetsFaction
        {
            get
            {
                switch (_allowedFactions)
                {
                    case Faction.Enemy:
                        return "enemy";
                    case Faction.Player:
                        return "friendly";
                    case Faction.Neutral:
                        return "neutral";
                    case Faction.Any:
                    default:
                        return string.Empty;
                }
            }
        }
        private string _typeTargets => $"{RequiredType}";
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
