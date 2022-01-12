using Archetype.Builder.Builders.Base;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Builder.Builders
{
    public interface INodeBuilder : IBuilder<IMutableMapNode> // TODO: This should create NodeProtoData, not instances
    {
        
    }
    
    internal class NodeBuilder : INodeBuilder
    {
        private readonly MapNode _mapNode;

        public NodeBuilder(IInstanceFactory getInstanceFactory)
        {
            _mapNode = new MapNode(default, getInstanceFactory);
        }

        public IMutableMapNode Build()
        {
            return _mapNode;
        }
    }
}