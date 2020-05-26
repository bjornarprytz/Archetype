using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class ActionPrompt
    {
        public abstract Type OptionsType { get; }

        public Unit Promptee { get; set; }

        public int MaxChoices { get; protected set; }
        public int MinChoices { get; protected set; }

        public IEnumerable<GamePiece> Options { get; protected set; }

        protected ActionPrompt(Unit promptee, int min, int max, IEnumerable<GamePiece> options)
        {
            if (min > max) throw new ArgumentException($"Min value {min} must be lower than max value {max}");

            Promptee = promptee;
            MinChoices = min;
            MaxChoices = max;
            Options = options;
        }

        public PromptResponse Respond(IEnumerable<GamePiece> choices)
        {
            return new PromptResponse(choices);
        }

        public virtual PromptResponse Abort()
        {
            return new PromptResponse(null, aborted: true);
        }
    }
}
