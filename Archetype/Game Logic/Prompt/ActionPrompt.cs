using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class ActionPrompt
    {
        public abstract Type OptionsType { get; }
        public bool Aborted { get; set; }

        public int MaxChoices { get; protected set; }
        public int MinChoices { get; protected set; }

        public IEnumerable<GamePiece> Options { get; protected set; }

        protected ActionPrompt(int min, int max, IEnumerable<GamePiece> options)
        {
            if (min > max) throw new ArgumentException($"Min value {min} must be lower than max value {max}");

            MinChoices = min;
            MaxChoices = max;
            Options = options;
        }

        public virtual void Abort()
        {
            Aborted = true;
        }
    }
}
