using Archetype.Core;
using System;
using System.Linq;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.CardBuilder
{
    public class SetBuilder : IBuilder<ICardSet>
    {

        private CardSet _setData;

        private SetBuilder(string name)
        {
            _setData = new CardSet
            {
                Name = name
            };
        }

        public static SetBuilder CreateSet(string name)
        {
            return new SetBuilder(name);
        }

        public SetBuilder Card(Action<CardBuilder> builderProvider)
        {
            var cbc = BuilderFactory.CardBuilder(); // Input template data here

            builderProvider(cbc);

            _setData.AddCard(cbc.Build());

            return this;
        }

        public ICardSet Build()
        {
            Console.WriteLine($"Created set with {_setData.Cards.Count()} cards");

            return _setData;
        }
    }
}
