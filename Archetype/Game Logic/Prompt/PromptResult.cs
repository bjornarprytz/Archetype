using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class PromptResult
    {
        public bool Aborted { get; set; }
        public List<GamePiece> ChosenPieces { get; private set; }

        public PromptResult()
        {
            ChosenPieces = new List<GamePiece>();
        }

        internal bool Meets(UserPrompt requirements)
        {
            return ChosenPieces.Count > requirements.MaxChoices ? false
                 : ChosenPieces.Count < requirements.MinChoices ? false
                 : ChosenPieces.Any(piece => !requirements.MeetsRequirements(piece)) ? false
                 : true;
        }
    }
}
