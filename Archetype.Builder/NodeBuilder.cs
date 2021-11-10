using Archetype.Game.Payloads.Pieces;

namespace Archetype.Builder
{
    public class NodeBuilder : IBuilder<IMapNode>
    {
        private readonly MapNode _mapNode;

        public NodeBuilder()
        {
            _mapNode = new MapNode();
        }

        public IMapNode Build()
        {
            return _mapNode;
        }
    }
}