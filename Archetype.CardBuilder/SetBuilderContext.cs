using Archetype.Core;
using System;

namespace Archetype.CardBuilder
{
    public class SetBuilderContext : BaseBuilder<SetData>
    {
        private CardData _cardTemplate;

        private SetBuilderContext(string name)
        {
            _cardTemplate = new();

            Construction.Name = name;
        }

        public static SetBuilderContext CreateSet(string name)
        {
            return new SetBuilderContext(name);
        }

        public SetBuilderContext Template(Action<TemplateBuilder> provideContext)
        {
            var cbc = BuilderFactory.TemplateBuilder();

            provideContext(cbc);

            _cardTemplate = cbc.Build();

            return this;
        }

        public SetBuilderContext Creature(Action<CreatureBuilder> provideContext)
        {
            var cbc = BuilderFactory.CreatureBuilder(_cardTemplate); // Input template here

            provideContext(cbc);

            Construction.Cards.Add(cbc.Build());

            return this;
        }

        public SetBuilderContext Spell(Action<SpellBuilder> provideContext)
        {
            var cbc = BuilderFactory.SpellBuilder(_cardTemplate); // Input template here

            provideContext(cbc);

            Construction.Cards.Add(cbc.Build());

            return this;
        }

        protected override void PreBuild()
        {
            Console.WriteLine($"Created set with {Construction.Cards.Count} cards");
        }
    }
}
