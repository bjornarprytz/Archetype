using Archetype.Core;
using System;

namespace Archetype.CardBuilder
{
    public class SetBuilder : BaseBuilder<SetData>
    {
        private CardData _cardTemplate;

        private SetBuilder(string name)
        {
            _cardTemplate = new CardData();

            Construction.Name = name;
        }

        public static SetBuilder CreateSet(string name)
        {
            return new SetBuilder(name);
        }

        public SetBuilder Template(Action<TemplateBuilder> builderProvider)
        {
            var cbc = BuilderFactory.TemplateBuilder();

            builderProvider(cbc);

            _cardTemplate = cbc.Build();

            return this;
        }

        public SetBuilder Creature(Action<CreatureBuilder> builderProvider)
        {
            var cbc = BuilderFactory.CreatureBuilder(_cardTemplate); // Input template here

            builderProvider(cbc);

            Construction.Cards.Add(cbc.Build());

            return this;
        }

        public SetBuilder Spell(Action<SpellBuilder> provideContext)
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
