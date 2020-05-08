
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class PromptResponse
    {
        public List<GamePiece> Choices { get; private set; }
        public bool Aborted { get; protected set; }

        public PromptResponse(IEnumerable<GamePiece> choices, bool aborted = false) 
        {
            Choices = choices.ToList();
            Aborted = aborted;
        }
    }
}
