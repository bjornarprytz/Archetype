using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View.Atoms.Zones;

namespace Archetype.Game.Payloads.Atoms
{
    [Target("Hand")]
    public interface IHand  : IZone<ICard>, IHandFront { }

    public class Hand : Zone<ICard>, IHand
    {
        public Hand(IGameAtom owner) : base(owner) { }
    }
}
