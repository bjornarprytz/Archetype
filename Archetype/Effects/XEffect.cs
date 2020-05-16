using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class XEffect : Effect<Unit>
    {
        public int X { get { return _x > 0 ? _x : 0; } set { _x = value; } }

        private int _x;

        public XEffect(int x, EffectArgs args) 
            : base(args)
        {
            X = x;
        }

        protected override Resolution _resolve => (prompt) =>
        {
            Source.ModifyOutgoingEffect(this); // TODO: Verify that this is not of type XEffect, but the concrete subtype inheriting from XEffect

            foreach (Unit target in Targets)
            {
                _affect(target, prompt);
            }
        };
    }
}
