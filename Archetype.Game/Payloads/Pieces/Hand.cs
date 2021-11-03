using System.Collections;
using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IHand  : IZone<ICard>
    {
    }

    public class Hand : GamePiece, IHand
    {
        private readonly List<ICard> _cards = new();
        
        public Hand(IGamePiece owner) : base(owner)
        {
            
        }

        public IEnumerable<ICard> Contents => _cards;
    }
}
