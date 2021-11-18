using System;
using System.Collections.Generic;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Builder
{
    public class CardPoolBuilder : IBuilder<ICardPool>
    {
        private readonly CardPool _cardPool;
        private readonly List<ICardSet> _sets = new ();
        
        internal CardPoolBuilder()
        {
            _cardPool = new CardPool(_sets);
        }

        public CardPoolBuilder AddSet(string name, Action<SetBuilder> builderProvider)
        {
            var setBuilder = BuilderFactory.SetBuilder(name);

            builderProvider(setBuilder);
            
            _sets.Add(setBuilder.Build());

            return this;
        }

        public ICardPool Build()
        {
            return _cardPool;
        }
    }
}