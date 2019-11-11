using System.Collections.Generic;

namespace Archetype
{
    public class DrawTemplate : EffectTemplate
    {
        private int _cardsToDraw;

        public DrawTemplate(int amount, TargetParams<Unit> requirements)
            : base (requirements)
        {
            _cardsToDraw = amount;
        }

        public override Effect CreateEffect(Unit source, Decision userInput)
        {
            List<Unit> targets = HandleUserInput(userInput);

            return new DrawEffect(_cardsToDraw, source, targets);
        }
    }
}
