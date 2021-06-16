using Archetype.Core;

namespace Archetype.CardBuilder
{
    public class SpellBuilder : CardBuilder<SpellCardData, SpellBuilder>
    {
        internal SpellBuilder(SpellCardData template) : base(template) { }

        public SpellBuilder Damage(int damage)
        {
            Construction.Damage = damage;

            return this;
        }
    }
}
