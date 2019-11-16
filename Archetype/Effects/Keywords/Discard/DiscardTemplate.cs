using System.Collections.Generic;

namespace Archetype
{
    public class DiscardTemplate : EffectTemplate
    {
        private int _cardsToDiscard;

        public DiscardTemplate(int amount, TargetParams<Unit> requirements)
            : base(requirements)
        {
            _cardsToDiscard = amount;
        }

        public override Effect CreateEffect(Unit source, List<Unit> targets)
        {
            return new DiscardEffect(_cardsToDiscard, source, targets);
        }
    }
}
