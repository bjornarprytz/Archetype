using Archetype.Core;
using System;

namespace Archetype.CardBuilder
{
    public class CreatureBuilder : CardBuilder<CreatureCardData, CreatureBuilder>
    {
        internal CreatureBuilder(CreatureCardData template) : base(template) { } // Factory this

        public CreatureBuilder PT(int power, int toughness)
        {
            Construction.Power = power;
            Construction.Toughness = toughness;

            return this;
        }
    }
}
