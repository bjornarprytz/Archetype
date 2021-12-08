using System;
using System.Collections.Generic;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class SetBuilder : IBuilder<ICardSet>
    {

        private readonly ICardSet _setData;
        private readonly Dictionary<Guid, ICardProtoData> _cards = new();

        private CardMetaData _template = new();

        internal SetBuilder(string name)
        {
            _template = _template with { SetName = name };
            
            _setData = new CardSet(_cards)
            {
                Name = name
            };
        }

        public SetBuilder SetTemplate(CardMetaData newTemplate)
        {
            _template = newTemplate with { SetName = _setData.Name };

            return this;
        }

        public SetBuilder ChangeTemplate(Func<CardMetaData, CardMetaData> changeFunc)
        {
            _template = changeFunc(_template) with { SetName = _setData.Name };

            return this;
        }

        public SetBuilder ClearTemplate()
        {
            _template = new CardMetaData { SetName = _setData.Name };

            return this;
        }

        public SetBuilder Card(Action<CardBuilder> builderProvider)
        {
            var cbc = BuilderFactory.CardBuilder(_template);

            builderProvider(cbc);

            var card = cbc.Build();
            
            _cards.Add(card.Guid, card);

            return this;
        }

        public ICardSet Build()
        {
            if (_setData.Name.IsMissing())
                throw new MissingSetNameException();
            
            Console.WriteLine($"Created set {_setData.Name} with {_cards.Count} cards");

            return _setData;
        }
    }
}
