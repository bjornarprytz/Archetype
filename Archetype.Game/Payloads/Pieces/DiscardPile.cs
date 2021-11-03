using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IDiscardPile : IZone<ICard>
    {
        void Bury(ICard card);
        void Exhume(ICard card);
    }
    
    public class DiscardPile : GamePiece, IDiscardPile
    {
        private readonly List<ICard> _cards = new();
        
        public DiscardPile(IGamePiece owner) : base(owner)
        {
        }

        public IEnumerable<ICard> Contents => _cards;
        
        public void Bury(ICard card)
        {
            _cards.Add(card);

            card.CurrentZone = this;
        }

        public void Exhume(ICard card)
        {
            _cards.Remove(card);
        }
    }
}
