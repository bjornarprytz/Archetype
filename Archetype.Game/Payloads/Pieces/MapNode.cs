using System.Collections.Generic;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IMapNode : IZone // Add generic type here?
    {
        IEnumerable<IMapNode> Neighbours { get; }
    }
    
    public class MapNode : GamePiece, IMapNode
    {
        public MapNode(IGamePiece owner) : base(owner) // Owner may not make much sense here, depending on the game design
        {
        }

        public IEnumerable<IMapNode> Neighbours { get; }
    }
}