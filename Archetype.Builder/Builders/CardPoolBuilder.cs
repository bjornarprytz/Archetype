using System;
using System.Collections.Generic;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Builder.Builders
{
    public class CardPoolBuilder : IBuilder<IProtoPool>
    {
        private readonly ProtoPool _protoPool;
        private readonly List<ISet> _sets = new ();
        
        internal CardPoolBuilder()
        {
            _protoPool = new ProtoPool(_sets);
        }

        public CardPoolBuilder AddSet(string name, Action<SetBuilder> builderProvider)
        {
            var setBuilder = BuilderFactory.SetBuilder(name);

            builderProvider(setBuilder);
            
            _sets.Add(setBuilder.Build());

            return this;
        }

        public IProtoPool Build()
        {
            return _protoPool;
        }
    }
}