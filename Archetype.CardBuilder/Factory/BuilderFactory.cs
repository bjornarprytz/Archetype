using Archetype.Core;

namespace Archetype.CardBuilder
{
    public class BuilderFactory
    {
        public static CreatureBuilder CreatureBuilder(CardData template = null)
        {
            return new CreatureBuilder(template as CreatureCardData);
        }

        public static SpellBuilder SpellBuilder(CardData template = null)
        {
            return new SpellBuilder(template as SpellCardData);
        }

        public static TemplateBuilder TemplateBuilder(CardData template = null)
        {
            return new TemplateBuilder(template);
        }
    }
}
