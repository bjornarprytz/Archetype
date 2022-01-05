using System;
using Archetype.Builder.Builders.Base;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Builders
{
    public interface INodeBuilder : IBuilder<IMapNodeProtoData>
    {
        
        INodeBuilder Name(string name);
        INodeBuilder MaxStructures(int i);
    }
    
    internal class NodeBuilder : ProtoBuilder<IMapNodeProtoData>, INodeBuilder
    {
        private readonly MapNodeProtoData _mapNode;

        public NodeBuilder()
        {
            _mapNode = new MapNodeProtoData();
        }
        
        public INodeBuilder MaxStructures(int i)
        {
            if (i < 0)
                throw new ArgumentException("{nameof(IMapNodeProtoData.MaxStructures)} cannot be negative", nameof(i));
            
            _mapNode.MaxStructures = i;

            return this;
        }
        
        public INodeBuilder Name(string name)
        {
            _mapNode.Name = name;

            return this;
        }

        protected override IMapNodeProtoData BuildInternal()
        {
            return _mapNode;
        }
    }
}