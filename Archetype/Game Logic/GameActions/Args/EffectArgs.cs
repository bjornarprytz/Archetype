using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class EffectArgs : ChoiceArgs
    {
        internal EffectTemplate Effect { get; private set; }
        public Unit Source { get; set; }
        public List<GamePiece> Targets { get; set; }

        public int MaxTargets { get; private set; }
        public int MinTargets { get; private set; }

        public IEnumerable<GamePiece> Options { get; private set; }

        public EffectArgs(int minTargets, int maxTargets, IEnumerable<GamePiece> options)
        {
            Targets = new List<GamePiece>();

            MinTargets = minTargets;
            MaxTargets = maxTargets;
            Options = options;
        }

        public EffectArgs(int nTargets, IEnumerable<Unit> options)
        {
            Targets = new List<GamePiece>();

            MinTargets = MaxTargets = nTargets;
            Options = options;
        }

        public override bool Valid => Targets.Count <= MaxTargets && Targets.Count >= MinTargets;
    }
}
