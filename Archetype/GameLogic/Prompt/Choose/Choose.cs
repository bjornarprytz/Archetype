using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Archetype
{
    public class Choose<T> : Choose where T : GamePiece
    {
        public override Type OptionsType => typeof(T);

        public Choose(int x, IEnumerable<T> options) : base(x, x, options) { }
        public Choose(int min, int max, IEnumerable<T> options) : base(min, max, options) { }
    }

    public abstract class Choose : EventArgs
    {
        public abstract Type OptionsType { get; }

        public int MaxChoices { get; protected set; }
        public int MinChoices { get; protected set; }

        public List<GamePiece> Options { get; protected set; }
        public List<GamePiece> Choices { get; protected set; }

        public bool IsCancelled { get; set; }

        public Choose(int min, int max, IEnumerable<GamePiece> options)
        {
            if (min > max) throw new ArgumentException($"Min value {min} must be lower than max value {max}");

            MinChoices = min;
            MaxChoices = max;
            Options = options.ToList();
        }

        public bool TryChoose(ICollection<GamePiece> choices)
        {
            if (choices == null ||
                choices.Count > MaxChoices ||
                choices.Count < MinChoices)
            {
                return false;
            }

            if (choices.Any(c => c.GetType() != OptionsType || !Options.Contains(c)))
            {
                return false;
            }


            Choices = choices.ToList();
            return true;
        }
    }
}
