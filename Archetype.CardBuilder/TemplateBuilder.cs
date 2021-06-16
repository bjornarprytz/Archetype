using Archetype.Core;

namespace Archetype.CardBuilder
{
    public class TemplateBuilder : CardBuilder<CardData, TemplateBuilder>
    {
        internal TemplateBuilder(CardData template) : base(template)
        {
        }
    }
}
