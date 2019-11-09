using System;

namespace Archetype
{
    public class PlayCardPrompt : UserPrompt
    {
        
        internal string TargetsText => $"{_numberOfTargets} {_targetsFaction} {_typeTargets}";

        private Faction _allowedFactions;
        protected override Type _typeRestriction => typeof(GamePiece);

        public PlayCardPrompt(int x, Type t, Faction allowedFactions)
            : base(x, t)
        {
            _allowedFactions = allowedFactions;
        }

        public PlayCardPrompt(int min, int max, Type t, Faction allowedFactions)
            : base(min, max, t)
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
}
