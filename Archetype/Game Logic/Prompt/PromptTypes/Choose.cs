using System;
using System.Collections.Generic;

namespace Archetype
{
    public class Choose<T> : ActionPrompt where T : GamePiece
    {
        public override Type OptionsType => typeof(T);
 
        public Choose(Unit promptee, int x, IEnumerable<T> options) : base (promptee, x, x, options) { }
        public Choose(Unit promptee, int min, int max, IEnumerable<T> options) : base(promptee, min, max, options) { }
    }
}
