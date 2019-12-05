using System.Collections.Generic;

namespace Archetype
{
    public class DiscardTemplate : EffectTemplate
    {
        private int _cardsToDiscard;

        public DiscardTemplate(int amount, TargetParams<Unit> requirements, TargetOptions targetPool)
            : base(requirements, targetPool)
        {
            _cardsToDiscard = amount;
        }

        public override Effect CreateEffect(EffectArgs args) => new DiscardEffect(_cardsToDiscard, args);
    }
}
