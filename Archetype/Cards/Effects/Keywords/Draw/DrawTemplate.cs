using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class DrawTemplate : EffectTemplate
    {
        public override string RulesText => $"{Requirements.TargetsText} draw(s) {_cardsToDraw} card(s)";

        public override PromptRequirements Requirements { get; protected set; }

        private int _cardsToDraw;

        public DrawTemplate(int amount, PromptRequirements requirements)
        {
            _cardsToDraw = amount;
            Requirements = requirements;
        }

        public override Effect CreateEffect(Unit source, PromptResult userInput)
        {
            if (!userInput.Meets(Requirements)) throw new Exception("User input insufficient to create effect");

            List<Unit> targets = new List<Unit>(userInput.ChosenPieces.Select(piece => (Unit)piece));

            return new DrawEffect(_cardsToDraw, source, targets);
        }
    }
}
