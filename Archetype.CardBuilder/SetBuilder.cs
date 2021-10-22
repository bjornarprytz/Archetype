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

        public SetBuilder Card(Action<CardBuilder> builderProvider)
        {
            var cbc = BuilderFactory.CardBuilder(); // Input template data here

            builderProvider(cbc);

            Construction.Cards.Add(cbc.Build());

            return this;
        }

        protected override void PreBuild()
        {
            Console.WriteLine($"Created set with {Construction.Cards.Count} cards");
        }
    }
}
