using System.Collections.Generic;

namespace Archetype
{
    public class DrawTemplate : EffectTemplate
    {
        private int _cardsToDraw;

        public DrawTemplate(int amount, TargetParams<Unit> requirements, TargetOptions targetPool)
            : base (requirements, targetPool)
        {
            _cardsToDraw = amount;
        }

        public override Effect CreateEffect(EffectArgs args) => new DrawEffect(_cardsToDraw, args);
    }
}
