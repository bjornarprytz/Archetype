using System;
using System.Collections.Generic;

namespace Archetype
{
    public class Choose<T> : ActionPrompt where T : GamePiece
    {
        public bool Aborted { get; set; }
        public IEnumerable<T> Options { get; private set; }
        public List<T> Choices { get; set; }

        public Choose(int x, IEnumerable<T> options)
            : base(x)
        {
            Options = options;
            Choices = new List<T>(x);
        }

        public Choose(int min, int max, IEnumerable<T> options)
            : base(min, max)
        {
            Options = options;
            Choices = new List<T>(max);
        }

        public void Abort()
        {
            Aborted = true;
        }
    }
}
