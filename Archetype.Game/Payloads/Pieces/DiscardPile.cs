using System;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IDiscardPile : IZone<ICard>
    {
        void Bury(ICard card);
        void Exhume(ICard card);
    }
    
    public class DiscardPile : Zone<ICard>, IDiscardPile
    {
        public DiscardPile(IGameAtom owner) : base(owner)
        {
            if (owner == null)
                throw new ArgumentException("DiscardPile needs an owner", nameof(owner));
        }

        public void Bury(ICard card) => AddPiece(card);
        public void Exhume(ICard card) => RemovePiece(card);
    }
}
