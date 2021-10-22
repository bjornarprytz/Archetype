using Archetype.Core;

namespace Archetype.CardBuilder
{
    public class BuilderFactory
    {

        public static CardBuilder CardBuilder(CardData template = null)
        {
            return new CardBuilder(template);
        }

        public static TemplateBuilder TemplateBuilder(CardData template = null)
        {
            return new TemplateBuilder(template);
        }
    }
}
