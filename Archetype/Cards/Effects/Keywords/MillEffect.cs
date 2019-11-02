using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillEffect : XEffect, IKeyword
    {
        public override string Keyword => "Mill";
        public int CardsToMill => X;

        public MillEffect(Unit source, int x) : base(source, x)
        {
            Targets.Add(source);
        }

        public MillEffect(Unit source, Unit target, int x) : base(source, x)
        {
            Targets.Add(target);
        }

        protected override void _affect(Unit target, int modifier)
        {
            target.Mill(CardsToMill + modifier);
        }
    }
}
