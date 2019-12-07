
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class PromptResponse
    {
        public List<GamePiece> Choices { get; private set; }
        public bool Aborted { get; protected set; }

        private PromptResponse() { }

        public static PromptResponse Choose(IEnumerable<GamePiece> choices)
        {
            return new PromptResponse() { Choices = choices.ToList(), Aborted = false };
        }
    }
}
