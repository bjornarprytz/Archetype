using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class XEffect<T> : Effect<T>, IKeyword where T : GamePiece
    {
        public abstract string Keyword { get; }
        public int X { get { return _x > 0 ? _x : 0; } set { _x = value; } }

        private int _x;

        public XEffect(int x, EffectArgs args) 
            : base(args)
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

        protected abstract void _affect(Unit target, int modifier, IPromptable prompt);
    }
}
