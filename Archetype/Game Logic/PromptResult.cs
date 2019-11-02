using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public delegate PromptResult DecisionPrompt(PromptRequirements requirements);

    public class PromptRequirements
    {
        public Type Type { get; private set; }
        public int Max { get; private set; }
        public int Min { get; private set; }
        public PromptRequirements(int min, int max, Type t)
        {
            if (!t.IsSubclassOf(typeof(GamePiece))) throw new Exception("Can only require Game Piece types in prompts");

            Type = t;
            Min = min;
            Max = max;
        }
    }

    public class PromptResult
    {
        public List<GamePiece> ChosenPieces { get; private set; }
    }
}
