using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IHandFront : IZoneFront
    {
        
    }
    
    [Target("Hand")]
    internal interface IHand  : IZone<ICard>, IHandFront { }

    internal class Hand : Zone<ICard>, IHand
    {
        public Hand(IGameAtom owner) : base(owner) { }
    }
}
