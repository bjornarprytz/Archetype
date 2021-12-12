using System;
using System.Collections.Generic;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Builder.Builders
{
    public interface IPoolBuilder : IBuilder<IProtoPool>
    {
        IPoolBuilder AddSet(string name, Action<ISetBuilder> builderProvider);
    }
    
    public class PoolBuilder : IPoolBuilder
    {
        private readonly IBuilderFactory _builderFactory;
        private readonly ProtoPool _protoPool;
        private readonly List<ISet> _sets = new ();
        
        public PoolBuilder(IBuilderFactory builderFactory)
        {
            _builderFactory = builderFactory;
            _protoPool = new ProtoPool(_sets);
        }

        public IPoolBuilder AddSet(string name, Action<ISetBuilder> builderProvider)
        {
            var setBuilder = 
                _builderFactory
                    .Create<ISetBuilder>()
                    .Name(name);

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