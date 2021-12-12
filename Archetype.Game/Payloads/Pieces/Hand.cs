using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Hand")]
    public interface IHand  : IZone<ICard> { }

    public class Hand : Zone<ICard>, IHand
    {
        public Hand(IGameAtom owner) : base(owner) { }
    }
}
