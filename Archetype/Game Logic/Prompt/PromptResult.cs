using System;
using System.Collections.Generic;

namespace Archetype
{
    public class PromptResult
    {
        public bool Aborted { get; set; }
        public List<GamePiece> ChosenPieces { get; private set; }
    }
}
