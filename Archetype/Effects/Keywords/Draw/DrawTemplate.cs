using System.Collections.Generic;

namespace Archetype
{
    public class DrawTemplate : EffectTemplate
    {
        public override string RulesText => $"{Requirements.TargetsText} draw(s) {_cardsToDraw} card(s)";

        public override ChooseTargets Requirements { get; protected set; }

        private int _cardsToDraw;

        public DrawTemplate(int amount, ChooseTargets requirements)
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
