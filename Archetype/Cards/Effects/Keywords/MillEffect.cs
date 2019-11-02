using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillEffect : XEffect, IKeyword
    {
        public string Keyword => "Mill";
        public int CardsToMill => X;

        public MillEffect(Unit source, int x) : base(source, x)
        {
            Targets.Add(source);
        }

        public MillEffect(Unit source, Unit target, int x) : base(source, x)
        {
            Targets.Add(target);
        }

        protected override Resolution _resolve => delegate { Source.DealMill(this); };
    }
}
