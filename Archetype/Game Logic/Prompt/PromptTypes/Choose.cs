using System;
using System.Collections.Generic;

namespace Archetype
{
    public class Choose<T> : ActionPrompt where T : GamePiece
    {
        public int MaxChoices { get; protected set; }
        public int MinChoices { get; protected set; }


        public IEnumerable<T> Options { get; private set; }
        public List<T> Choices { get; set; }

        public Choose(int x, IEnumerable<T> options)
        {
            MaxChoices = MinChoices = x;
            Options = options;
            Choices = new List<T>(x);
        }

        public Choose(int min, int max, IEnumerable<T> options)
        {
            MaxChoices = max;
            MinChoices = min;
            Options = options;
            Choices = new List<T>(max);
        }
    }
}
