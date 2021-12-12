using Archetype.Game.Payloads.Pieces;

namespace Archetype.Builder.Builders
{
    public class NodeBuilder : IBuilder<IMutableMapNode>
    {
        private readonly MapNode _mapNode;

        internal NodeBuilder()
        {
            _mapNode = new MapNode(default);
        }

        public IMutableMapNode Build()
        {
            return _mapNode;
        }
    }
}