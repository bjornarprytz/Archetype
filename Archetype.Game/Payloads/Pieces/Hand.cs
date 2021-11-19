using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IHand  : IZone<ICard>
    {
        void Add(ICard card);
        void Remove(ICard card);
    }

    public class Hand : Zone<ICard>, IHand
    {
        public Hand(IGameAtom owner) : base(owner)
        {
            
        }

        public void Add(ICard card) => AddPiece(card);
        public void Remove(ICard card) => RemovePiece(card);
    }
}
