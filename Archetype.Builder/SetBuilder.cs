using System;
using System.Collections.Generic;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class SetBuilder : IBuilder<ICardSet>
    {

        private readonly ICardSet _setData;
        private readonly List<ICardProtoData> _cards = new();

        private SetBuilder(string name)
        {
            _setData = new CardSet(_cards)
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

            _cards.Add(cbc.Build());

            return this;
        }

        public ICardSet Build()
        {
            Console.WriteLine($"Created set with {_cards.Count} cards");

            return _setData;
        }
    }
}
