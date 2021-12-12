using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Builder.Builders
{
    public interface INodeBuilder : IBuilder<IMutableMapNode>
    {
        
    }
    
    public class NodeBuilder : INodeBuilder
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