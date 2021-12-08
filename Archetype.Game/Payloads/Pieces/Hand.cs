using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Hand")]
    public interface IHand  : IZone<ICard>
    {
        void Add(ICard card);
    }

    public class Hand : Zone<ICard>, IHand
    {
        public Hand(IGameAtom owner) : base(owner)
        {
            
        }

        public void Add(ICard card) => card.MoveTo(this);
    }
}
