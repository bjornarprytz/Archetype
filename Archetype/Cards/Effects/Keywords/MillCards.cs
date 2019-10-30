using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class MillCards : XEffect, IKeyword
    {
        public string Keyword => "Mill";
        public int CardsToMill => X;

        public MillCards(Unit source, int x) : base(source, x)
        {
            Targets.Add(source);
        }

        public MillCards(Unit source, Unit target, int x) : base(source, x)
        {
            Targets.Add(target);
        }

        protected override Resolution _resolve => delegate { Source.DealMill(this); };
    }
}
