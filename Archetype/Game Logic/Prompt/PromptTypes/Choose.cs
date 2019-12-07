using System;
using System.Collections.Generic;

namespace Archetype
{
    public class Choose<T> : ActionPrompt where T : GamePiece
    {
        public override Type OptionsType => typeof(T);

        public Choose(int x, IEnumerable<T> options) : base (x, x, options) { }
        public Choose(int min, int max, IEnumerable<T> options) : base(min, max, options) { }
    }
}
