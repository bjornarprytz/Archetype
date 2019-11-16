using System;
using System.Collections.Generic;

namespace Archetype
{
    public class Choose<T> : ActionPrompt where T : GamePiece
    {
        public List<T> Choices { get; set; }

        public Choose(int x, IEnumerable<T> availableChoices)
            : base(x)
        {
            Choices = new List<T>(x);
        }

        public Choose(int min, int max, IEnumerable<T> availableChoices)
            : base(min, max)
        {
            Choices = new List<T>(max);
        }
    }
}
