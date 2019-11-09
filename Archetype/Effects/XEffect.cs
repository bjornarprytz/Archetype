using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class XEffect : Effect, IKeyword
    {
        public abstract string Keyword { get; }
        public int X { get { return _x > 0 ? _x : 0; } set { _x = value; } }

        private int _x;

        public XEffect(int x, Unit source, List<Unit> targets=null) 
            : base(source, targets)
        {
            X = x;
        }

        protected override Resolution _resolve => (prompt) =>
        {
            X += Source.ModifierAsSource(Keyword);

            foreach(Unit target in Targets)
            {
                _affect(target, target.ModifierAsTarget(Keyword), prompt);
            }
        };

        protected abstract void _affect(Unit target, int modifier, RequiredAction prompt);
    }
}
