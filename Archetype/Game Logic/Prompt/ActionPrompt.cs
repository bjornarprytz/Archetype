using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class ActionPrompt
    {
        public Predicate<object> MatchesType => o => o.GetType() == RequiredType;
        public int MaxChoices { get; protected set; }
        public int MinChoices { get; protected set; }

        public abstract Type RequiredType { get; }

        public ActionPrompt(int x)
        {
            MaxChoices = MinChoices = x;
        }
        public ActionPrompt(int min, int max)
        {
            MaxChoices = max;
            MinChoices = min;
        }
    }
}
